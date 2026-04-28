using EpinelPS.Utils;
using EpinelPS.Database;
using EpinelPS.Data;
using System.Linq;

namespace EpinelPS.LobbyServer.Event.StoryEvent
{
    [PacketPath("/event/storydungeon/clearstage")]
    public class ClearEventStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqClearEventStage req = await ReadData<ReqClearEventStage>();
            User user = GetUser();

            // SAFELY get or create event data (Never overwrite!)
            if (!user.EventInfo.TryGetValue(req.EventId, out EventData? eventData))
            {
                eventData = new EventData() { LastStage = 0, LastDay = 0, FreeTicket = 0 };
                user.EventInfo.Add(req.EventId, eventData);
            }

            ResClearEventStage response = new();

            int difficultId = 0;
            NetRewardData reward = new();
            NetRewardData bonusReward = new();
            ClearEventStageHelper.ClearStage(user, req.StageId, ref reward, ref bonusReward, req.BattleResult, 1);

            // ONLY update progression if the battle was a victory
            if (req.BattleResult == 1)
            {
                if (!eventData.ClearedStages.Contains(req.StageId))
                {
                    eventData.ClearedStages.Add(req.StageId);
                }
                if (eventData.LastStage < req.StageId)
                {
                    eventData.LastStage = req.StageId;
                }
                eventData.Diff = difficultId;

                user.AddTrigger(Trigger.EventStageClear, 1, req.StageId);
                user.AddTrigger(Trigger.EventDungeonStageClear, 1, req.EventId);
                // --- MANUAL MISSION COMPLETION FIX ---
                // Because the trigger system is unfinished, we manually unlock the mission here!
                if (user.ClearedEventMissions == null) user.ClearedEventMissions = new();

                if (!user.ClearedEventMissions.ContainsKey(req.EventId))
                {
                    user.ClearedEventMissions.Add(req.EventId, new List<int>());
                }

                // In most Nikke event tables, the Mission ID to clear a stage matches the Stage ID.
                // (e.g. Beating Stage 10101 unlocks Mission 10101).
                if (!user.ClearedEventMissions[req.EventId].Contains(req.StageId))
                {
                    user.ClearedEventMissions[req.EventId].Add(req.StageId);
                }
                // -------------------------------------
            }

            if (bonusReward.Item.Count > 0)
            {
                bonusReward.Item.ToList().ForEach(item =>
                {
                    user.AddTrigger(Trigger.ObtainEventCurrencyMaterial, item.Count, item.Tid);
                });
            }

            // Subtract ticket and save state to DB
            response.RemainTicket = EventStoryHelper.SubtractTicket(user, req.EventId, 1);

            response.Reward = reward;
            response.BonusReward = bonusReward;

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}