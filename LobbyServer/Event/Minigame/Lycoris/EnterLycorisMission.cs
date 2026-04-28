using EpinelPS.Utils;
namespace EpinelPS.LobbyServer.Event.MiniGame
{
    [PacketPath("/event/minigame/lycoris/entermission")]
    public class EnterLycorisMission : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // FIX: Read the client's data so the server doesn't cut the connection!
            await ReadData<ReqEnterMiniGameLycorisMission>();

            await WriteDataAsync(new ResEnterMiniGameLycorisMission());
        }
    }
}