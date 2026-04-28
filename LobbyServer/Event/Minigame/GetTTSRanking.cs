using EpinelPS.Utils;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.MiniGame
{
    [PacketPath("/MiniGame/TTS/Ranking/Total/Get")]
    public class GetTTSRanking : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // Read the request
            var req = await ReadData<ReqGetMiniGameTtsTotalRanking>();

            // Send an empty response so the game doesn't crash!
            ResGetMiniGameTtsTotalRanking response = new();

            await WriteDataAsync(response);
        }
    }
}