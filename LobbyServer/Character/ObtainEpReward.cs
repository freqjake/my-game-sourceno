using EpinelPS.Utils;
using EpinelPS.Data;
using EpinelPS.Database;
using System.Linq;

namespace EpinelPS.LobbyServer.Character
{
    [PacketPath("/character/attractive/obtainreward")]
    public class ObtainEpReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqObtainAttractiveReward req = await ReadData<ReqObtainAttractiveReward>();
            ResObtainAttractiveReward response = new();
            User user = GetUser();

            // 1. Search for the character's specific reward first
            var specificRecords = GameData.Instance.AttractiveLevelReward.Values
                .Where(x => x.AttractiveLevel == req.Lv && x.NameCode == req.NameCode).ToList();

            // 2. If it doesn't exist (like Ep 2-5), find the generic fallback reward
            var fallbackRecords = GameData.Instance.AttractiveLevelReward.Values
                .Where(x => x.AttractiveLevel == req.Lv && (x.NameCode == 0 || x.NameCode == 9999)).ToList();

            int targetRewardId = 0;
            int tableIdToSave = 0;

            if (specificRecords.Any())
            {
                targetRewardId = specificRecords[0].RewardId;
                tableIdToSave = specificRecords[0].Id;
            }
            else if (fallbackRecords.Any())
            {
                targetRewardId = fallbackRecords[0].RewardId;
                tableIdToSave = fallbackRecords[0].Id;
            }
            else
            {
                throw new Exception($"Reward missing for Lv {req.Lv}, NameCode {req.NameCode}");
            }

            // 3. Grant the reward and save the state
            foreach (NetUserAttractiveData item in user.BondInfo)
            {
                if (item.NameCode == req.NameCode)
                {
                    bool changed = false;

                    // FIX: Save the raw Level number (e.g. 1, 2, 3, 4, 5)
                    if (!item.ObtainedRewardLevels.Contains(req.Lv))
                    {
                        item.ObtainedRewardLevels.Add(req.Lv);
                        changed = true;
                    }

                    // FIX: Also save the specific Table ID. 
                    // This guarantees the client's "Contains" check passes on relogin regardless of version.
                    if (tableIdToSave != 0 && !item.ObtainedRewardLevels.Contains(tableIdToSave))
                    {
                        item.ObtainedRewardLevels.Add(tableIdToSave);
                        changed = true;
                    }

                    if (changed)
                    {
                        RewardRecord reward = GameData.Instance.GetRewardTableEntry(targetRewardId)
                            ?? throw new Exception("failed to get reward");

                        response.Reward = RewardUtils.RegisterRewardsForUser(user, reward);

                        // Force the save to the database so it survives relogin
                        JsonDb.Save();
                    }
                    break;
                }
            }

            await WriteDataAsync(response);
        }
    }
}