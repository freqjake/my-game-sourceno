using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Models;
using EpinelPS.Utils;
using EpinelPS.LobbyServer.Event.StoryEvent;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/challengestage/clear")]
    public class ClearChallengeStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqClearChallengeEventStage req = await ReadData<ReqClearChallengeEventStage>();
            User user = GetUser();

            if (!user.EventInfo.TryGetValue(req.EventId, out EventData? eventData))
            {
                eventData = new EventData() { LastStage = 0 };
                user.EventInfo.Add(req.EventId, eventData);
            }

            ResClearChallengeEventStage response = new();

            // 1. Create LOCAL variables to satisfy the "ref" requirements of the helper
            NetRewardData localReward = new();
            NetRewardData localBonusReward = new();

            // 2. Pass the local variables to the helper
            ClearEventStageHelper.ClearStage(user, req.StageId, ref localReward, ref localBonusReward, req.BattleResult, 1);

            // 3. Assign the local reward to the response property
            response.Reward = localReward;
            // Note: We ignore localBonusReward because Challenge Stages don't use it!

            // If the battle was a victory (Result == 1)
            if (req.BattleResult == 1)
            {
                if (eventData.ChallengeTicket > 0)
                {
                    eventData.ChallengeTicket -= 1;
                }

                if (!eventData.ClearedStages.Contains(req.StageId))
                {
                    eventData.ClearedStages.Add(req.StageId);
                }
                if (eventData.LastStage < req.StageId)
                {
                    eventData.LastStage = req.StageId;
                }

                user.AddTrigger(Trigger.EventStageClear, 1, req.StageId);
                user.AddTrigger(Trigger.EventDungeonStageClear, 1, req.EventId);
            }

            response.RemainTicket = eventData.ChallengeTicket;

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}