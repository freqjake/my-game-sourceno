using EpinelPS.Utils;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.MiniGame
{
    [PacketPath("/MiniGame/TTS/Ranking/Song/Get")]
    public class GetTTSSongRanking : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // The client asks: "Who are the top 10 players for this song?"
            var req = await ReadData<ReqGetMiniGameTtsSongRanking>();

            // We send back an empty response: "Nobody yet!"
            ResGetMiniGameTtsSongRanking response = new();

            await WriteDataAsync(response);
        }
    }
}