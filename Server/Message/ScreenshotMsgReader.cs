﻿using System;
using LmpCommon.Message.Data.Screenshot;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using Server.Client;
using Server.System;

namespace Server.Message
{
    public class ScreenshotMsgReader
    {
        public static void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var data = (ScreenshotBaseMsgData)message.Data;
            switch (data.ScreenshotMessageType)
            {
                case ScreenshotMessageType.FoldersRequest:
                    ScreenshotSystem.SendScreenshotFolders(client);
                    break;
                case ScreenshotMessageType.ListRequest:
                    ScreenshotSystem.SendScreenshotList(client, (ScreenshotListRequestMsgData)data);
                    break;
                case ScreenshotMessageType.ScreenshotData:
                    ScreenshotSystem.SaveScreenshot(client, (ScreenshotDataMsgData)data);
                    break;
                case ScreenshotMessageType.DownloadRequest:
                    ScreenshotSystem.SendScreenshot(client, (ScreenshotDownloadRequestMsgData)data);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}