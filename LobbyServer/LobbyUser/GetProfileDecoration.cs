using EpinelPS.Utils;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/ProfileCard/DecorationLayout/Get")]
    public class GetProfileDecoration : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqProfileCardDecorationLayout req = await ReadData<ReqProfileCardDecorationLayout>();
            var user = GetUser();

            ResProfileCardDecorationLayout r = new()
            {
                Layout = new ProfileCardDecorationLayout
                {
                    BackgroundId = user.ProfileCardLayout.BackgroundId,
                    ShowCharacterSpine = user.ProfileCardLayout.ShowCharacterSpine
                }
            };

            // Rebuild the sticker network list from the database
            foreach (var sticker in user.ProfileCardLayout.StickerPlacements)
            {
                r.Layout.StickerPlacements.Add(new StickerPlacement
                {
                    StickerId = sticker.StickerId,
                    Layer = sticker.Layer,
                    Normalizedx = sticker.Normalizedx,
                    Normalizedy = sticker.Normalizedy,
                    RotationRadian = sticker.RotationRadian,
                    Scale = sticker.Scale
                });
            }

            await WriteDataAsync(r);
        }
    }
}