using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Models;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/challengestage/get")]
    public class GetChallengeStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqChallengeEventStageData req = await ReadData<ReqChallengeEventStageData>();
            User user = GetUser();

            // 1. Get or create the EventData
            if (!user.EventInfo.TryGetValue(req.EventId, out EventData? eventData))
            {
                eventData = new() { LastStage = 0 };
                user.EventInfo.Add(req.EventId, eventData);
            }

            // 2. Handle Daily Ticket Reset (Challenge stages give 3 per day)
            int dateDay = user.GetDateDay();
            if (dateDay > eventData.ChallengeLastDay)
            {
                eventData.ChallengeTicket = 1;
                eventData.ChallengeLastDay = dateDay;
            }

            // 3. Build Response using the DB value
            ResChallengeEventStageData response = new()
            {
                RemainTicket = eventData.ChallengeTicket, // Use DB value instead of hardcoded 3
                TeamData = new NetUserTeamData
                {
                    Type = (int)TeamType.StoryEvent
                },
            };

            // check if user has a team for this type
            if (user.UserTeams.TryGetValue((int)TeamType.StoryEvent, out NetUserTeamData? teamData))
            {
                response.TeamData = teamData;
            }

            // placeholder response data for last cleared stage
            response.LastClearedEventStageList.Add(new NetLastClearedEventStageData()
            {
                DifficultyId = eventData.Diff,
                StageId = eventData.LastStage
            });

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}