using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Episode
{
    [PacketPath("/episode/mission/scenario/complete")]
    public class SetEpMissionScenarioComplete : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetEpMissionScenarioComplete req = await ReadData<ReqSetEpMissionScenarioComplete>();
            User user = GetUser();

            ResSetEpMissionScenarioComplete response = new()
            {
                PeriodValidationResult = EpMissionPeriodValidationResult.Ok
            };

            // Save the scenario ID so it persists!
            if (!string.IsNullOrEmpty(req.ScenarioId) && !user.CompletedScenarios.Contains(req.ScenarioId))
            {
                user.CompletedScenarios.Add(req.ScenarioId);
                JsonDb.Save();
            }

            await WriteDataAsync(response);
        }
    }
}