using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/infracore/check")]
    public class CheckInfracore : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqCheckReceiveInfraCoreReward req = await ReadData<ReqCheckReceiveInfraCoreReward>();
            ResCheckReceiveInfraCoreReward response = new();

            User user = GetUser();
            int currentLevel = user.InfraCoreLvl;
            Dictionary<int, InfraCoreGradeRecord> gradeTable = GameData.Instance.InfracoreTable;

            // FIX: Assume everything is claimed (true). 
            // We will loop through every level to verify.
            bool isReceived = true;

            for (int i = 1; i <= currentLevel; i++)
            {
                if (gradeTable.TryGetValue(i, out var gradeData) && gradeData.RewardId > 0)
                {
                    bool thisLevelClaimed = user.InfraCoreRewardReceived.ContainsKey(i) && user.InfraCoreRewardReceived[i];

                    // If we find even ONE unclaimed level, change it to false and stop checking!
                    if (!thisLevelClaimed)
                    {
                        isReceived = false;
                        break;
                    }
                }
            }

            response.IsReceived = isReceived;

            await WriteDataAsync(response);
        }
    }
}