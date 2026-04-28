using EpinelPS.Utils;
using System.Linq;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/scenario/exist")]
    public class GetUserScenarioExist : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqExistScenario req = await ReadData<ReqExistScenario>();

            ResExistScenario response = new();

            foreach (string? item in req.ScenarioGroupIds)
            {
                if (FindScenarioInMainStages(item) || FindScenarioInArchiveStages(item))
                {
                    response.ExistGroupIds.Add(item);
                }
            }

            await WriteDataAsync(response);
        }

        private bool FindScenarioInMainStages(string scenarioGroupId)
        {
            if (string.IsNullOrEmpty(scenarioGroupId)) return false;
            User user = GetUser();

            // Fix: Use StartsWith to match group prefixes (e.g., "d_main_01_01" matches "d_main_01_01_01")
            return user.CompletedScenarios.Any(s => s != null && s.StartsWith(scenarioGroupId));
        }

        private bool FindScenarioInArchiveStages(string scenarioGroupId)
        {
            if (string.IsNullOrEmpty(scenarioGroupId)) return false;
            User user = GetUser();

            foreach (EventData evtData in user.EventInfo.Values)
            {
                // Fix: Apply the same StartsWith logic for Event Archives
                if (evtData.CompletedScenarios.Any(s => s != null && s.StartsWith(scenarioGroupId)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}