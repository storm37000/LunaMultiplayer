using ByteSizeLib;
using LmpCommon;
using LmpCommon.Message.Data.Screenshot;
using LmpCommon.Message.Server;
using LmpCommon.Time;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.Settings.Structures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Graphics = System.Drawing.Graphics;

namespace Server.System
{
    public class ScreenshotSystem
    {
        public static readonly string ScreenshotPath = Path.Combine(ServerContext.UniverseDirectory, "Screenshots");

        private static readonly ConcurrentDictionary<string, DateTime> LastUploadRequest = new ConcurrentDictionary<string, DateTime>();

        #region Public Methods

        /// <summary>
        /// Saves a received screenshot and creates the miniature
        /// </summary>
        public static void SaveScreenshot(ClientStructure client, ScreenshotDataMsgData data)
        {
            Task.Run(() =>
            {
                var playerFolder = Path.Combine(ScreenshotPath, client.PlayerName);
                if (!FileHandler.FolderExists(playerFolder))
                {
                    FileHandler.FolderCreate(playerFolder);
                }

                var lastTime = LastUploadRequest.GetOrAdd(client.PlayerName, DateTime.MinValue);
                if (DateTime.Now - lastTime > TimeSpan.FromMilliseconds(ScreenshotSettings.SettingsStore.MinScreenshotIntervalMs))
                {
                    LastUploadRequest.AddOrUpdate(client.PlayerName, DateTime.Now, (key, existingVal) => DateTime.Now);
                    if (data.Screenshot.DateTaken == 0) data.Screenshot.DateTaken = LunaNetworkTime.UtcNow.ToBinary();
                    var fileName = $"{data.Screenshot.DateTaken}.png";
                    var fullPath = Path.Combine(playerFolder, fileName);
                    if (!FileHandler.FileExists(fullPath))
                    {
                        LunaLog.Normal($"Saving screenshot {fileName} ({ByteSize.FromBytes(data.Screenshot.NumBytes).KiloBytes}{ByteSize.KiloByteSymbol}) from: {client.PlayerName}.");
                        FileHandler.CreateFile(fullPath, data.Screenshot.Data, data.Screenshot.NumBytes);
                        if (Common.PlatformIsWindows())
                        {
                            CreateMiniature(fullPath);
                        }
                        SendNotification(client.PlayerName);
                    }
                    else
                    {
                        LunaLog.Warning($"{client.PlayerName} tried to overwrite a screnshot!");
                        return;
                    }
                }
                else
                {
                    LunaLog.Warning($"{client.PlayerName} is sending screenshots too fast!");
                    return;
                }

                //Remove oldest screenshots if the player has too many
                RemovePlayerOldestScreenshots(playerFolder);

                //Checks if we are above the max folders limit
                CheckMaxFolders();
            });
        }

        /// <summary>
        /// Send the screenshot folders that exist on the server
        /// </summary>
        public static void SendScreenshotFolders(ClientStructure client)
        {
            Task.Run(() =>
            {
                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ScreenshotFoldersReplyMsgData>();
                msgData.Folders = FileHandler.GetDirectoriesInPath(ScreenshotPath).Select(d => new DirectoryInfo(d).Name).ToArray();
                msgData.NumFolders = msgData.Folders.Length;

                MessageQueuer.SendToClient<ScreenshotSrvMsg>(client, msgData);
                if (msgData.NumFolders > 0)
                    LunaLog.Debug($"Sending {msgData.NumFolders} screenshot folders to: {client.PlayerName}");
            });
        }

        /// <summary>
        /// Sends the screenshots in a folder
        /// </summary>
        public static void SendScreenshotList(ClientStructure client, ScreenshotListRequestMsgData data)
        {
            Task.Run(() =>
            {
                var screenshots = new List<ScreenshotInfo>();

                foreach (var file in FileHandler.GetFilesInPath(Path.Combine(ScreenshotPath, data.FolderName)).Where(f => !f.StartsWith("small_")))
                {
                    if (long.TryParse(Path.GetFileNameWithoutExtension(file), out var dateTaken))
                    {
                        if (data.AlreadyOwnedPhotoIds.Contains(dateTaken))
                            continue;

                        var contents = FileHandler.ReadFile(file);
                        LunaLog.Debug("IMG: " + LunaMath.UShortFromBytes(contents[18], contents[19]) + " X " + LunaMath.UShortFromBytes(contents[22], contents[23]));
                        screenshots.Add(new ScreenshotInfo
                        {
                            Data = contents,
                            DateTaken = dateTaken,
                            NumBytes = contents.Length,
                            Height = LunaMath.UShortFromBytes(contents[18], contents[19]),
                            Width = LunaMath.UShortFromBytes(contents[22], contents[23]),
                            FolderName = data.FolderName,
                        });
                    }
                    else
                    {
                        LunaLog.Error("Failed to parse data on screenshot: " + file);
                    }
                }

                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ScreenshotListReplyMsgData>();
                msgData.FolderName = data.FolderName;
                msgData.Screenshots = screenshots.ToArray();
                msgData.NumScreenshots = screenshots.Count;

                LunaLog.Debug($"Sending {msgData.NumScreenshots} ({data.FolderName}) screenshots to: {client.PlayerName}");
                MessageQueuer.SendToClient<ScreenshotSrvMsg>(client, msgData);
            });
        }

        /// <summary>
        /// Sends the requested screenshot
        /// </summary>
        public static void SendScreenshot(ClientStructure client, ScreenshotDownloadRequestMsgData data)
        {
            Task.Run(() =>
            {
                var file = Path.Combine(ScreenshotPath, data.FolderName, $"{data.DateTaken}.png");
                if (FileHandler.FileExists(file))
                {
                    var contents = FileHandler.ReadFile(file);
                    var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ScreenshotDataMsgData>();
                    msgData.Screenshot.DateTaken = data.DateTaken;
                    msgData.Screenshot.Data = contents;
                    msgData.Screenshot.NumBytes = msgData.Screenshot.Data.Length;
                    msgData.Screenshot.Height = LunaMath.UShortFromBytes(contents[18], contents[19]);
                    msgData.Screenshot.Width = LunaMath.UShortFromBytes(contents[22], contents[23]);
                    msgData.Screenshot.FolderName = data.FolderName;

                    LunaLog.Debug($"Sending screenshot ({ByteSize.FromBytes(msgData.Screenshot.NumBytes).KiloBytes}{ByteSize.KiloByteSymbol}): {data.DateTaken} to: {client.PlayerName}.");
                    MessageQueuer.SendToClient<ScreenshotSrvMsg>(client, msgData);
                }
            });
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Sends a notification of new screenshot to all players
        /// </summary>
        private static void SendNotification(string folderName)
        {
            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ScreenshotNotificationMsgData>();
            msgData.FolderName = folderName;

            MessageQueuer.SendToAllClients<ScreenshotSrvMsg>(msgData);
        }

        /// <summary>
        /// Checks if we have too many player folders and if so, it deletes the oldest one
        /// </summary>
        private static void CheckMaxFolders()
        {
            while (FileHandler.GetDirectoriesInPath(ScreenshotPath).Length > ScreenshotSettings.SettingsStore.MaxScreenshotsFolders)
            {
                var oldestFolder = FileHandler.GetDirectoriesInPath(ScreenshotPath).Select(d => new DirectoryInfo(d)).OrderBy(d => d.LastWriteTime).FirstOrDefault();
                if (oldestFolder != null)
                {
                    LunaLog.Debug($"Removing oldest screenshot folder {oldestFolder.Name}");
                    FileHandler.FolderDelete(oldestFolder.FullName);
                }
            }
        }

        /// <summary>
        /// If the player has too many screenshots this method will remove the oldest ones
        /// </summary>
        private static void RemovePlayerOldestScreenshots(string playerFolder)
        {
            while (new DirectoryInfo(playerFolder).GetFiles().Where(f => !f.Name.StartsWith("small_")).Count() > ScreenshotSettings.SettingsStore.MaxScreenshotsPerUser)
            {
                var oldestScreenshot = new DirectoryInfo(playerFolder).GetFiles().Where(f => !f.Name.StartsWith("small_")).OrderBy(f => f.LastWriteTime).FirstOrDefault();
                if (oldestScreenshot != null)
                {
                    LunaLog.Debug($"Deleting old screenshot {oldestScreenshot.FullName}");
                    FileHandler.FileDelete(oldestScreenshot.FullName);
                    if (Common.PlatformIsWindows())
                    {
                        FileHandler.FileDelete(Path.Combine(ScreenshotPath, playerFolder, "small_" + oldestScreenshot.Name));
                    }
                }
            }
        }

        /// <summary>
        /// Creates a miniature of 120x120 pixels of the given picture
        /// </summary>
        private static void CreateMiniature(string path)
        {
            var fileName = Path.GetFileName(path);
            using (var image = Image.FromFile(path))
            using (var newImage = ScaleImage(image, 213, 120))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                newImage.Save(Path.Combine(Path.GetDirectoryName(path), $"small_{fileName}"), ImageFormat.Png);
            }
        }

        /// <summary>
        /// Scales a image in byte array
        /// </summary>
        private static byte[] ScaleImage(byte[] data, int numBytes, int maxWidth, int maxHeight)
        {
            using (var stream = new MemoryStream(data, 0, numBytes))
            using (var image = Image.FromStream(stream))
            {
                if (image.Width <= maxWidth && image.Height <= maxHeight)
                {
                    Array.Resize(ref data, numBytes);
                    return data;
                }

                using (var newImage = ScaleImage(image, maxWidth, maxHeight))
                using (var outputStream = new MemoryStream())
                {
                    newImage.Save(outputStream, image.RawFormat);
                    return outputStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Scales a given image
        /// </summary>
        private static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            if (image.Width <= maxWidth && image.Height <= maxHeight)
                return image;

            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }
        #endregion
    }
}
