using EpinelPS.Utils;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.MiniGame
{
    [PacketPath("/MiniGame/TTS/Song/Get")]
    public class GetTTSSong : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqGetMiniGameTtsSongData>();
            ResGetMiniGameTtsSongData response = new();

            // THE CRASH FIX: Give Unity the Friend and Union rank folders it's looking for!
            response.MyFriendSongRankData = new NetMyMiniGameTtsSongRankData()
            {
                Score = 0,
                Position = 1
            };

            response.MyUnionSongRankData = new NetMyMiniGameTtsSongRankData()
            {
                Score = 0,
                Position = 1
            };

            await WriteDataAsync(response);
        }
    }
}