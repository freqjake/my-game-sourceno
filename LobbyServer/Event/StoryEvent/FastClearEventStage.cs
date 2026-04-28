using EpinelPS.Utils;
using EpinelPS.Database;
using EpinelPS.Data;
using System.Linq;

namespace EpinelPS.LobbyServer.Event.StoryEvent
{
    [PacketPath("/event/storydungeon/fastclear")]
    public class FastClearEventStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqFastClearEventStage req = await ReadData<ReqFastClearEventStage>();
            User user = GetUser();

            // SAFELY get or create event data
            if (!user.EventInfo.TryGetValue(req.EventId, out EventData? eventData))
            {
                eventData = new EventData() { LastStage = 0, LastDay = 0, FreeTicket = 0 };
                user.EventInfo.Add(req.EventId, eventData);
            }

            ResFastClearEventStage response = new();

            NetRewardData reward = new();
            NetRewardData bonusReward = new();

            // Fast clear is always a victory (1), multiply rewards by req.ClearCount
            ClearEventStageHelper.ClearStage(user, req.StageId, ref reward, ref bonusReward, 1, req.ClearCount);

            user.AddTrigger(Trigger.EventDungeonStageClear, req.ClearCount, req.EventId);

            if (bonusReward.Item.Count > 0)
            {
                bonusReward.Item.ToList().ForEach(item =>
                {
                    user.AddTrigger(Trigger.ObtainEventCurrencyMaterial, item.Count, item.Tid);
                });
            }

            // Subtract tickets based on clear count
            response.RemainTicket = EventStoryHelper.SubtractTicket(user, req.EventId, req.ClearCount);

            response.Reward = reward;
            response.BonusReward = bonusReward;

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}