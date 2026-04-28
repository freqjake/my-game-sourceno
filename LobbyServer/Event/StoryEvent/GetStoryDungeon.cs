using EpinelPS.Utils;
using EpinelPS.Database;
using EpinelPS.Data;
using System.Linq;

namespace EpinelPS.LobbyServer.Event.StoryEvent
{
    [PacketPath("/event/storydungeon/get")]
    public class GetStoryDungeon : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqStoryDungeonEventData req = await ReadData<ReqStoryDungeonEventData>();
            User user = GetUser();

            // SAFELY get or create event data without wiping existing progress
            if (!user.EventInfo.TryGetValue(req.EventId, out EventData? eventData))
            {
                eventData = new EventData() { LastDay = 0, FreeTicket = 0 };
                user.EventInfo.Add(req.EventId, eventData);
            }

            ResStoryDungeonEventData response = new()
            {
                // This safely handles the daily reset
                RemainTicket = EventStoryHelper.GetTicket(user, req.EventId),
                TeamData = new NetUserTeamData
                {
                    Type = (int)TeamType.StoryEvent
                },
            };

            if (user.UserTeams.TryGetValue((int)TeamType.StoryEvent, out NetUserTeamData? teamData))
            {
                response.TeamData = teamData;
            }

            // FIX: Add uniquely cleared stages ONLY. The old code added the LastStage twice, confusing the UI.
            foreach (var stageId in eventData.ClearedStages.Distinct())
            {
                response.LastClearedEventStageList.Add(new NetLastClearedEventStageData()
                {
                    StageId = stageId
                });
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}