using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Stage
{
    [PacketPath("/stageclearinfo/get")]
    public class GetStageClearInfo : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetStageClearInfo req = await ReadData<ReqGetStageClearInfo>();
            ResGetStageClearInfo response = new();
            User user = GetUser();

            // This is correct! It sends the history to unlock chapters.
            if (user.StageClearHistorys != null)
            {
                response.Historys.AddRange(user.StageClearHistorys);
            }

            await WriteDataAsync(response);
        }
    }
}