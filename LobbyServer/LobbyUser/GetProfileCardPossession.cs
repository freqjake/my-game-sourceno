using EpinelPS.Utils;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/ProfileCard/Possession/Get")]
    public class GetProfileCardPossession : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqProfileCardObjectList req = await ReadData<ReqProfileCardObjectList>();
            var user = GetUser();
            ResProfileCardObjectList response = new();

            foreach (var item in user.Items)
            {
                string idString = item.ItemType.ToString();

                // Backgrounds (10xxxx)
                if (idString.Length == 6 && idString.StartsWith("10"))
                {
                    response.BackgroundIds.Add(item.ItemType);
                }
                // Stickers (20xxxx or 30xxxx)
                else if (idString.Length == 6 && (idString.StartsWith("20") || idString.StartsWith("30")))
                {
                    response.StickerIds.Add(item.ItemType);
                }
            }

            // Always give default background
            if (!response.BackgroundIds.Contains(101001))
            {
                response.BackgroundIds.Add(101001);
            }

            await WriteDataAsync(response);
        }
    }
}