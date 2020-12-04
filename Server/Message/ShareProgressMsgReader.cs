﻿using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using Server.Client;
using Server.System;

namespace Server.Message
{
    public class ShareProgressMsgReader
    {
        public static void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var data = (ShareProgressBaseMsgData)message.Data;
            switch (data.ShareProgressMessageType)
            {
                case ShareProgressMessageType.FundsUpdate:
                    ShareFundsSystem.FundsReceived(client, (ShareProgressFundsMsgData)data);
                    break;
                case ShareProgressMessageType.ScienceUpdate:
                    ShareScienceSystem.ScienceReceived(client, (ShareProgressScienceMsgData)data);
                    break;
                case ShareProgressMessageType.ScienceSubjectUpdate:
                    ShareScienceSubjectSystem.ScienceSubjectReceived(client, (ShareProgressScienceSubjectMsgData)data);
                    break;
                case ShareProgressMessageType.ReputationUpdate:
                    ShareReputationSystem.ReputationReceived(client, (ShareProgressReputationMsgData)data);
                    break;
                case ShareProgressMessageType.TechnologyUpdate:
                    ShareTechnologySystem.TechnologyReceived(client, (ShareProgressTechnologyMsgData)data);
                    break;
                case ShareProgressMessageType.ContractsUpdate:
                    ShareContractsSystem.ContractsReceived(client, (ShareProgressContractsMsgData)data);
                    break;
                case ShareProgressMessageType.AchievementsUpdate:
                    ShareAchievementsSystem.AchievementsReceived(client, (ShareProgressAchievementsMsgData)data);
                    break;
                case ShareProgressMessageType.StrategyUpdate:
                    ShareStrategySystem.StrategyReceived(client, (ShareProgressStrategyMsgData)data);
                    break;
                case ShareProgressMessageType.FacilityUpgrade:
                    ShareUpgradeableFacilitiesSystem.UpgradeReceived(client, (ShareProgressFacilityUpgradeMsgData)data);
                    break;
                case ShareProgressMessageType.PartPurchase:
                    SharePartPurchaseSystem.PurchaseReceived(client, (ShareProgressPartPurchaseMsgData)data);
                    break;
                case ShareProgressMessageType.ExperimentalPart:
                    ShareExperimentalPartSystem.ExperimentalPartReceived(client, (ShareProgressExperimentalPartMsgData)data);
                    break;
            }
        }
    }
}
