using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Models;
using System;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Event.Mission
{
    [PacketPath("/event/dailymission/obtainreward")]
    public class ObtainDailyEventReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqObtainDailyEventReward req = await ReadData<ReqObtainDailyEventReward>();
            User user = GetUser();

            ResObtainDailyEventReward response = new();
            response.Reward = new NetRewardData();

            try
            {
                if (!user.EventMissionInfo.TryGetValue(req.EventId, out EventMissionData? missionData))
                {
                    missionData = new EventMissionData { LastDay = user.GetDateDay() };
                    user.EventMissionInfo.Add(req.EventId, missionData);
                }

                foreach (int missionId in req.DailyEventId)
                {
                    // This checks if you ALREADY claimed it. If yes, it skips the triggers!
                    if (!missionData.MissionIdList.Contains(missionId))
                    {
                        missionData.MissionIdList.Add(missionId);
                        missionData.LastDate = DateTime.UtcNow.Ticks;

                        if (GameData.Instance.DailyEventTable.TryGetValue(missionId, out var missionRecord))
                        {
                            RewardRecord? rewardData = GameData.Instance.GetRewardTableEntry(missionRecord.RewardId);
                            if (rewardData != null)
                            {
                                NetRewardData grantedReward = RewardUtils.RegisterRewardsForUser(user, rewardData);
                                response.Reward.MergeFrom(grantedReward);
                            }
                        }

                        // ==========================================================
                        // THE ULTIMATE 0/5 & 0/45 BAR FIX (THE SHOTGUN BLAST)
                        // ==========================================================

                        // Extract all possible Day IDs (e.g., if mission is 200010102)
                        int dayGroupId = missionId / 100; // Results in 2000101
                        int dayNumber = dayGroupId % 100; // Results in 1
                        int overarchingId = dayGroupId * 100; // Results in 200010100

                        // 1. Trigger the specific sub-mission
                        user.AddTrigger(Trigger.MissionClearEvent, 1, missionId);

                        // 2. Trigger the 0/45 Master Bar (This worked on your Radmin account!)
                        user.AddTrigger(Trigger.EventPoint, 1, req.EventId);
                        user.AddTrigger(Trigger.MissionClearEvent, 1, req.EventId);

                        // 3. Trigger the 0/5 Day Bar
                        // We blast EventPoint to every possible Day ID channel!
                        user.AddTrigger(Trigger.EventPoint, 1, dayGroupId);
                        user.AddTrigger(Trigger.MissionClearEvent, 1, dayGroupId);

                        user.AddTrigger(Trigger.EventPoint, 1, dayNumber);
                        user.AddTrigger(Trigger.MissionClearEvent, 1, dayNumber);

                        user.AddTrigger(Trigger.EventPoint, 1, overarchingId);
                        user.AddTrigger(Trigger.MissionClearEvent, 1, overarchingId);
                    }
                }

                JsonDb.Save();
            }
            catch (Exception ex)
            {
                Logging.Warn($"ObtainDailyEventReward failed: {ex.Message}");
            }

            await WriteDataAsync(response);
        }
    }
}