using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Episode
{
    [PacketPath("/episode/mission/enter")]
    public class ListMission : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqListValidEpMission req = await ReadData<ReqListValidEpMission>();
            ResListValidEpMission response = new();

            // We leave this empty because this packet does NOT handle the map checkmarks!

            await WriteDataAsync(response);
        }
    }
}