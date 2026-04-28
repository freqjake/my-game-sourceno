using EpinelPS.Utils;
using EpinelPS.Database;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.MiniGame
{
    [PacketPath("/MiniGame/TTS/Profile/Get")]
    public class GetTTSProfile : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetMiniGameTtsProfile>();
            User user = GetUser();

            ResGetMiniGameTtsProfile response = new();

            // READ FROM THE DATABASE!
            response.MyServerRankData = new NetMyMiniGameTtsTotalRankData()
            {
                Score = user.TotalTtsScore, // Pulls the total accumulated score from memory
                Position = 1
            };

            await WriteDataAsync(response);
        }
    }
}