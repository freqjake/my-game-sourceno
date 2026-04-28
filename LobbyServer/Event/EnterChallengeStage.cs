using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/challengestage/enter")]
    public class EnterChallengeStage : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // Corrected the request type to ReqEnterChallengeEventStage
            ReqEnterChallengeEventStage req = await ReadData<ReqEnterChallengeEventStage>();
            User user = GetUser();

            // Corrected the response type to ResEnterChallengeEventStage
            ResEnterChallengeEventStage response = new()
            {
                // Assign properties if needed
            };

            await WriteDataAsync(response);
        }
    }
}