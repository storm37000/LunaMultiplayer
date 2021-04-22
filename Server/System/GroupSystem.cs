﻿using LmpCommon;
using LmpCommon.Message.Data.Groups;
using LmpCommon.Message.Server;
using LmpCommon.Xml;
using Server.Context;
using Server.Server;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server.System
{
    public static class GroupSystem
    {
        public static readonly string GroupsPath = Path.Combine(ServerContext.UniverseDirectory, "Groups");
        public static readonly string GroupsFilePath = Path.Combine(GroupsPath, "Groups.xml");

        public static ConcurrentDictionary<string, Group> Groups { get; } = new ConcurrentDictionary<string, Group>();

        public static void CreateGroup(string clientPlayerName, string groupName)
        {
            if (!Groups.ContainsKey(groupName))
            {
                var newGroup = new Group
                {
                    Members = new[] { clientPlayerName },
                    MembersCount = 1,
                    Owner = clientPlayerName,
                    Name = groupName
                };
                if (Groups.TryAdd(groupName, newGroup))
                {
                    var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<GroupUpdateMsgData>();
                    msgData.Group = newGroup;

                    MessageQueuer.SendToAllClients<GroupSrvMsg>(msgData);
                    SaveGroups();
                }
            }
        }

        public static void RemoveGroup(string clientPlayerName, string groupName)
        {
            if (Groups.TryGetValue(groupName, out var existingGroup) && existingGroup.Owner == clientPlayerName
                && Groups.TryRemove(groupName, out _))
            {
                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<GroupRemoveMsgData>();
                msgData.GroupName = groupName;

                MessageQueuer.SendToAllClients<GroupSrvMsg>(msgData);
                SaveGroups();
            }
        }

        public static void UpdateGroup(string clientPlayerName, Group group)
        {
            if (Groups.TryGetValue(group.Name, out var existingGroup))
            {
                if (existingGroup.Owner == clientPlayerName)
                {
                    //We are the owner of the group so we can do whatever we want
                    if (Groups.TryUpdate(group.Name, group, existingGroup))
                    {
                        var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<GroupUpdateMsgData>();
                        msgData.Group = group;

                        MessageQueuer.SendToAllClients<GroupSrvMsg>(msgData);
                        SaveGroups();
                    }
                }
                else
                {
                    //We are not the owner of the group so the only thing we can do is to add ourself to the "invited" hashset
                    if (group.Owner == existingGroup.Owner && Common.ScrambledEquals(group.Members, existingGroup.Members))
                    {
                        var invited = group.Invited.Except(existingGroup.Invited).ToArray();
                        if (invited.Length == 1 && invited[0] == clientPlayerName && Groups.TryUpdate(group.Name, group, existingGroup))
                        {
                            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<GroupUpdateMsgData>();
                            msgData.Group = group;

                            MessageQueuer.SendToAllClients<GroupSrvMsg>(msgData);
                            SaveGroups();
                        }
                    }
                }
            }
        }

        public static void SaveGroups()
        {
            if (FileHandler.FolderExists(GroupsPath))
            {
                FileHandler.WriteToFile(GroupsFilePath, LunaXmlSerializer.SerializeToXml(Groups.Values.ToList()));
            }
        }

        public static void LoadGroups()
        {
            if (FileHandler.FolderExists(GroupsFilePath))
            {
                var values = LunaXmlSerializer.ReadXmlFromPath<List<Group>>(GroupsFilePath);
                foreach (var value in values)
                {
                    Groups.TryAdd(value.Name, value);
                }
            }
        }
    }
}
