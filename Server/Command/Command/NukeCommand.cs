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
    public class NukeCommand : SimpleCommand
    {
        public static async void CheckTimer()
        {
            //0 or less is disabled.
            if (GeneralSettings.SettingsStore.AutoNuke > 0)
            {
                while (ServerContext.ServerRunning)
                {
                    await Task.Delay((int)TimeSpan.FromMinutes(GeneralSettings.SettingsStore.AutoNuke).TotalMilliseconds);
                    RunNuke();
                }
            }
        }

        public override bool Execute(string commandArgs)
        {
            RunNuke();
            return true;
        }

        private static void RunNuke()
        {
            uint removalCount = 0;

            var vesselList = VesselStoreSystem.CurrentVessels.ToArray();
            foreach (var vesselKeyVal in vesselList)
            {
                if (vesselKeyVal.Value.Fields.GetSingle("landed").Value.ToLower() == "true" &&
                    vesselKeyVal.Value.Fields.GetSingle("landedAt").Value.ToLower().Contains("ksc") ||
                    vesselKeyVal.Value.Fields.GetSingle("landedAt").Value.ToLower().Contains("runway") ||
                    vesselKeyVal.Value.Fields.GetSingle("landedAt").Value.ToLower().Contains("launchpad"))
                {
                    LunaLog.Normal($"Removing vessel: {vesselKeyVal.Key} from KSC");

                    VesselStoreSystem.RemoveVessel(vesselKeyVal.Key);

                    //Send a vessel remove message
                    var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselRemoveMsgData>();
                    msgData.VesselId = vesselKeyVal.Key;

                    MessageQueuer.SendToAllClients<VesselSrvMsg>(msgData);

                    removalCount++;
                }
            }

            if (removalCount > 0)
                LunaLog.Normal($"Nuked {removalCount} vessels around the KSC");
        }
    }
}
