using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Episode
{
    [PacketPath("/episode/mission/scenario/get")]
    public class ListEpMissionCompletedScenario : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqListEpMissionCompletedScenario req = await ReadData<ReqListEpMissionCompletedScenario>();
            User user = GetUser();

            ResListEpMissionCompletedScenario response = new();

            // Send back all the scenarios the user has completed so the map adds checkmarks
            if (user.CompletedScenarios != null)
            {
                foreach (string scenarioId in user.CompletedScenarios)
                {
                    response.ScenarioList.Add(scenarioId);
                }
            }

            await WriteDataAsync(response);
        }
    }
}