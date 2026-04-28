using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using log4net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using static EpinelPS.ResGetEventMissionClearList.Types;

namespace EpinelPS.LobbyServer.Event.Mission
{
    public static class EventMissionHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EventMissionHelper));

        // =======================================================
        // METHOD 1: GetCleared (This is the method you accidentally deleted!)
        // =======================================================
        public static RepeatedField<NetEventMissionClearData> GetCleared(User user, int eventId)
        {
            var clearData = new RepeatedField<NetEventMissionClearData>();
            if (!user.EventMissionInfo.TryGetValue(eventId, out var userEvent)) return clearData;
            
            int dateDay = user.GetDateDay();

            // Check if it's a new day, reset daily missions
            if (userEvent.LastDay != dateDay)
            {
                ResetUserDailyMission(user, eventId, dateDay);
            }

            foreach (var id in userEvent.DailyMissionIdList)
            {
                clearData.Add(new NetEventMissionClearData() { EventId = eventId, EventMissionId = id, CreatedAt = userEvent.LastDate });
            }

            foreach (var id in userEvent.MissionIdList)
            {
                clearData.Add(new NetEventMissionClearData() { EventId = eventId, EventMissionId = id, CreatedAt = userEvent.LastDate });
            }
            return clearData;
        }

        // =======================================================
        // METHOD 2: GetClearedList (With the Relogin Fix!)
        // =======================================================
        public static RepeatedField<NestEventMissionClear> GetClearedList(User user, RepeatedField<int> eventIds)
        {
            var clearDatas = new RepeatedField<NestEventMissionClear>();
            if (eventIds.Count == 0) return clearDatas;
            foreach (var eventId in eventIds)
            {
                var clearData = new NestEventMissionClear { EventId = eventId };
                
                // This calls Method 1, which is why it threw a red error when Method 1 was deleted!
                clearData.EventMissionClearList.AddRange(GetCleared(user, eventId));
                
                // RELOGIN BUG FIX: Actually add the data to the response package!
                clearDatas.Add(clearData); 
            }
            return clearDatas;
        }

        // =======================================================
        // METHOD 3: ObtainReward (With the 0/5 Progress Bar Fix!)
        // =======================================================
        public static void ObtainReward(User user, ref NetRewardData reward, int eventId, RepeatedField<int> missionIds, Timestamp timeStamp)
        {
            EventMissionData userEvent = GetUserEventMissionData(user, eventId);
            int dateDay = user.GetDateDay();

            if (userEvent.LastDay != dateDay)
            {
                ResetUserDailyMission(user, eventId, dateDay);
            }

            var userMissionIds = userEvent.MissionIdList ?? new List<int>();
            var userDailyMissionIds = userEvent.DailyMissionIdList ?? new List<int>();

            var eventMissionRecords = GameData.Instance.EventMissionListTable.Values.Where(em =>
                missionIds.Contains(em.Id)
                && !userMissionIds.Contains(em.Id)
                && !userDailyMissionIds.Contains(em.Id)).ToList();

            if (eventMissionRecords.Count == 0) return;

            List<Reward_Data> rewards = new List<Reward_Data>();
            foreach (var mission in eventMissionRecords)
            {
                if (mission.RewardId == 0)
                {
                    if (mission.RewardPointValue > 0)
                    {
                        user.AddTrigger(Trigger.PointRewardEvent, mission.RewardPointValue, mission.Group);
                    }
                    continue;
                }

                // Keep the original group trigger just in case
                user.AddTrigger(Trigger.MissionClearEvent, 1, mission.Group);

                var rewardRecord = GameData.Instance.GetRewardTableEntry(mission.RewardId);
                if (rewardRecord is null || rewardRecord.Rewards.Count == 0) continue;
                foreach (var item in rewardRecord.Rewards)
                {
                    var itemIndex = rewards.FindIndex(x => x.RewardId == item.RewardId);
                    if (itemIndex >= 0)
                        rewards[itemIndex].RewardValue += item.RewardValue;
                    else
                        rewards.Add(item);
                }
            }

            foreach (var r in rewards)
            {
                RewardUtils.AddSingleObject(user, ref reward, r.RewardId, r.RewardType, r.RewardValue);
            }

            var groupIds = eventMissionRecords.Select(x => x.Group).Distinct();
            var categoryRecords = GameData.Instance.EventMissionCategoryTable.Values.Where(ec => groupIds.Contains(ec.MissionListGroup)).ToList();

            foreach (var mission in eventMissionRecords)
            {
                var categoryRecord = categoryRecords.FirstOrDefault(ec => ec.MissionListGroup == mission.Group);

                if (categoryRecord != null && categoryRecord.InitType == EventMissionInitType.Daily)
                {
                    userEvent.DailyMissionIdList.Add(mission.Id);
                }
                else
                {
                    userEvent.MissionIdList.Add(mission.Id);
                }

                // ==========================================================
                // THE 0/5 PROGRESS BAR FIX!
                // Broadcast the clear to all possible channels so the overarching mission hears it.
                // ==========================================================
                user.AddTrigger(Trigger.MissionClearEvent, 1, mission.Id); // Broadast to specific mission
                user.AddTrigger(Trigger.MissionClearEvent, 1, eventId);    // Broadcast to the whole Event
                user.AddTrigger(Trigger.MissionClearEvent, 1, 0);          // Broadcast globally
            }

            foreach (var item in reward.Item)
            {
                user.AddTrigger(Trigger.ObtainEventCurrencyMaterial, item.Count, item.Tid);
            }

            userEvent.LastDate = timeStamp.ToDateTime().Ticks;
            user.EventMissionInfo[eventId] = userEvent;

            JsonDb.Save();
        }

        // =======================================================
        // METHOD 4: ResetUserDailyMission
        // =======================================================
        private static void ResetUserDailyMission(User user, int eventId, int dateDay)
        {
            if (!user.EventMissionInfo.TryGetValue(eventId, out var userEvent)) return;
            if (userEvent.LastDay == dateDay) return;
            user.EventMissionInfo[eventId].DailyMissionIdList = new List<int>();
            user.EventMissionInfo[eventId].LastDay = dateDay;
            JsonDb.Save();
        }

        // =======================================================
        // METHOD 5: GetUserEventMissionData
        // =======================================================
        private static EventMissionData GetUserEventMissionData(User user, int eventId)
        {
            if (!user.EventMissionInfo.TryGetValue(eventId, out var userEvent))
            {
                userEvent = new EventMissionData();
                userEvent.LastDay = user.GetDateDay();
                user.EventMissionInfo.Add(eventId, userEvent);
            }
            return userEvent;
        }
    }
}