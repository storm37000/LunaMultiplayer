using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Server;
using Server.Command.Command.Base;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.Settings.Structures;
using Server.System;
using System;
using System.Threading.Tasks;

namespace Server.Command.Command
{
    public class DekesslerCommand : SimpleCommand
    {
        public static async void CheckTimer()
        {
            //0 or less is disabled.
            if (GeneralSettings.SettingsStore.AutoDekessler > 0)
            {
                while (ServerContext.ServerRunning)
                {
                    await Task.Delay((int)TimeSpan.FromMinutes(GeneralSettings.SettingsStore.AutoDekessler).TotalMilliseconds);
                    RunDekessler();
                }
            }
        }

        public override bool Execute(string commandArgs)
        {
            RunDekessler();
            return true;
        }

        private static void RunDekessler()
        {
            uint removalCount = 0;

            var vesselList = VesselStoreSystem.CurrentVessels.ToArray();
            foreach (var vesselKeyVal in vesselList)
            {
                if (vesselKeyVal.Value.Fields.GetSingle("type").Value.ToLower() == "debris")
                {
                    LunaLog.Normal($"Removing debris vessel: {vesselKeyVal.Key}");

                    VesselStoreSystem.RemoveVessel(vesselKeyVal.Key);

                    //Send a vessel remove message
                    var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselRemoveMsgData>();
                    msgData.VesselId = vesselKeyVal.Key;

                    MessageQueuer.SendToAllClients<VesselSrvMsg>(msgData);

                    removalCount++;
                }
            }

            if (removalCount > 0)
                LunaLog.Normal($"Removed {removalCount} debris");
        }
    }
}
