using EpinelPS.Utils;
using System.Linq;

namespace EpinelPS.LobbyServer.Character.Counsel;

[PacketPath("/character/attractive/check")]
public class CheckCharacterCounsel : LobbyMsgHandler
{
    protected override async Task HandleAsync()
    {
        ReqCounseledBefore req = await ReadData<ReqCounseledBefore>();
        User user = GetUser();

        ResCounseledBefore response = new();

        // 1. Look for the character in the user's saved BondInfo list.
        // NOTE: If your compiler gives a red line under 'req.Tid', change it 
        // to 'req.CounselTid' or 'req.NameCode' based on your allmsgs.cs file.
        var bondInfo = user.BondInfo.FirstOrDefault(x => x.NameCode == req.CounselTid);

        // 2. If the record exists (is not null), they HAVE been counseled before.
        // The game client uses this to decide whether to show the "Already Counseled" UI 
        // or skip the first-time intro dialogue.
        response.IsCounseledBefore = bondInfo != null;

        await WriteDataAsync(response);
    }
}