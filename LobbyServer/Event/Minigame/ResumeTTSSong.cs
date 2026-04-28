using EpinelPS.Utils;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.MiniGame
{
    [PacketPath("/MiniGame/TTS/Resume")]
    public class ResumeTTSSong : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // The client says "Unpausing the song!"
            var req = await ReadData<ReqResumeMiniGameTtsPlay>();

            // We give the thumbs up to continue
            ReqResumeMiniGameTtsPlay response = new();

            await WriteDataAsync(response);
        }
    }
}