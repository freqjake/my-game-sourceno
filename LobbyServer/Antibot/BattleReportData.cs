using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Antibot
{
    [PacketPath("/antibot/battlereportdata")]
    public class BattleReportData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqBattleReportData req = await ReadData<ReqBattleReportData>();

            // Create a clean empty response to satisfy the client
            ResBattleReportData response = new();

            // Send the response back immediately so the client can transition back to the lobby
            await WriteDataAsync(response);
        }
    }
}