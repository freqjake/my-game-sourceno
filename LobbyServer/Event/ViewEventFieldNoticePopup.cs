using EpinelPS.Utils;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.EventField
{
    // Matches the exact path from your console error
    [PacketPath("/event/field/noticepopup/view")]
    public class ViewEventFieldNoticePopup : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // 1. Read the incoming request
            var req = await ReadData<ReqViewEventFieldNoticePopup>();

            // 2. Initialize the response
            ReqViewEventFieldNoticePopup response = new();

            // 3. Send it back! 
            // A simple empty response is usually all the client needs to safely close the popup.
            await WriteDataAsync(response);
        }
    }
}