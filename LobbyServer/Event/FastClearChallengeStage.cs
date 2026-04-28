using EpinelPS.Database;
using EpinelPS.Data;
using EpinelPS.Models;
using EpinelPS.Utils;
using EpinelPS.LobbyServer.Event.StoryEvent;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/challengestage/fastclear")]
    public class FastClearChallengeStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqFastClearChallengeEventStage req = await ReadData<ReqFastClearChallengeEventStage>();
            User user = GetUser();

            if (!user.EventInfo.TryGetValue(req.EventId, out EventData? eventData))
            {
                eventData = new EventData() { LastStage = 0 };
                user.EventInfo.Add(req.EventId, eventData);
            }

            if (eventData.ChallengeTicket >= req.ClearCount)
            {
                eventData.ChallengeTicket -= req.ClearCount;
            }
            else
            {
                eventData.ChallengeTicket = 0;
            }

            ResFastClearChallengeEventStage response = new();

            // 1. Create LOCAL variables to satisfy the "ref" requirements
            NetRewardData localReward = new();
            NetRewardData localBonusReward = new();

            // 2. Pass local variables, multiplying rewards by req.ClearCount
            ClearEventStageHelper.ClearStage(user, req.StageId, ref localReward, ref localBonusReward, 1, req.ClearCount);

            // 3. Assign only the main reward to the response
            response.Reward = localReward;

            user.AddTrigger(Trigger.EventDungeonStageClear, req.ClearCount, req.EventId);

            response.RemainTicket = eventData.ChallengeTicket;

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}