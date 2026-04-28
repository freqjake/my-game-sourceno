using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/User/GetScenarioList")]
    public class GetScenarioList : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetScenarioList req = await ReadData<ReqGetScenarioList>();
            User user = GetUser();

            ResGetScenarioList response = new();

            // FIX: Ensure 'item' matches the type in your User model (likely int)
            // If the protobuf 'ScenarioList' expects strings, use .ToString()
            foreach (var item in user.CompletedScenarios)
            {
                response.ScenarioList.Add(item.ToString());
            }

            await WriteDataAsync(response);
        }
    }
}