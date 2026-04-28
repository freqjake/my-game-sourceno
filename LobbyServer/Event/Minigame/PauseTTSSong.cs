using EpinelPS.Utils;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.MiniGame
{
    [PacketPath("/MiniGame/TTS/Pause")]
    public class PauseTTSSong : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // The client says "Song paused/unpaused!"
            var req = await ReadData<ReqPauseMiniGameTtsPlay>();

            ReqPauseMiniGameTtsPlay response = new();

            await WriteDataAsync(response);
        }
    }
}