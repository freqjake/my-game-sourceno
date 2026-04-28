using EpinelPS.Utils;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.MiniGame
{
    [PacketPath("/MiniGame/TTS/Start")]
    public class StartTTSSong : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // The client says "Starting the song!"
            var req = await ReadData<ReqResumeMiniGameTtsPlay>();

            // We give the thumbs up
            ReqResumeMiniGameTtsPlay response = new();

            await WriteDataAsync(response);
        }
    }
}