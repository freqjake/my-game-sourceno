using EpinelPS.Utils;
namespace EpinelPS.LobbyServer.Event.MiniGame
{
    [PacketPath("/event/minigame/lycoris/getranking")]
    public class GetLycorisRanking : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // FIX: Read the client's data
            await ReadData<ReqGetMiniGameLycorisRanking>();

            await WriteDataAsync(new ResGetMiniGameLycorisRanking());
        }
    }
}