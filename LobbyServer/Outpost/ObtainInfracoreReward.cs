using EpinelPS.Utils;
using EpinelPS.Data;
using EpinelPS.Database;
using System.Collections.Generic;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/infracore/reward")]
    public class ObtainInfracoreReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqObtainInfraCoreReward req = await ReadData<ReqObtainInfraCoreReward>();
            ResObtainInfraCoreReward response = new();
            User user = GetUser();

            int currentLevel = user.InfraCoreLvl;
            Dictionary<int, InfraCoreGradeRecord> gradeTable = GameData.Instance.InfracoreTable;

            List<NetRewardData> allRewards = new();
            bool changed = false;

            // 1. Loop through ALL levels up to the current level to ensure no rewards are skipped
            for (int i = 1; i <= currentLevel; i++)
            {
                if (gradeTable.TryGetValue(i, out var gradeData) && gradeData.RewardId > 0)
                {
                    bool isReceived = user.InfraCoreRewardReceived.ContainsKey(i) && user.InfraCoreRewardReceived[i];

                    if (!isReceived)
                    {
                        user.InfraCoreRewardReceived[i] = true;

                        var reward = RewardUtils.RegisterRewardsForUser(user, gradeData.RewardId);
                        if (reward != null)
                        {
                            allRewards.Add(reward);
                        }

                        changed = true;
                    }
                }
            }

            if (changed)
            {
                // 2. Merge all the rewards safely
                response.Reward = NetUtils.MergeRewards(allRewards, user);

                // 3. CRITICAL FIX: Sync the FinalValue! 
                // This forces the game client to display the Gem icon in the animation 
                // and correctly update your wallet on the top right of the screen.
                if (response.Reward != null && response.Reward.Currency != null)
                {
                    foreach (var c in response.Reward.Currency)
                    {
                        c.FinalValue = user.Currency.ContainsKey((CurrencyType)c.Type) ? user.Currency[(CurrencyType)c.Type] : 0;
                    }
                }

                // 4. Save the user so the claim status persists!
                JsonDb.Save();
            }
            else
            {
                response.Reward = new NetRewardData();
            }

            await WriteDataAsync(response);
        }
    }
}