using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.Field
{
    [PacketPath("/event/field/notice-popup/view")] // Check your server logs for the exact POST path!
    public class ViewEventFieldNoticePopup : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqViewEventFieldNoticePopup req = await ReadData<ReqViewEventFieldNoticePopup>();
            User user = GetUser();

            ResViewEventFieldNoticePopup response = new();

            if (user.ViewedEventNotices == null) user.ViewedEventNotices = new();

            // Save the IDs of the popups you just read
            foreach (int noticeId in req.EventFieldNoticePopupTableIds)
            {
                if (!user.ViewedEventNotices.Contains(noticeId))
                {
                    user.ViewedEventNotices.Add(noticeId);
                }
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}