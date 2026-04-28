using EpinelPS.Utils;
using EpinelPS.Database;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.MiniGame
{
    [PacketPath("/MiniGame/TTS/PlayTime/Save")]
    public class SaveTTSPlayTime : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqSaveMiniGameTtsPlayTime>();

            // We just send a success response so the game doesn't crash!
            // (We removed the playtime tracking because the client doesn't send the time)
            ResSaveMiniGameTtsPlayTime response = new();
            await WriteDataAsync(response);
        }
    }
}