using EpinelPS.Database;
using EpinelPS.Utils;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/ProfileCard/DecorationLayout/Save")]
    public class SaveProfileDecoration : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSaveProfileCardDecorationLayout req = await ReadData<ReqSaveProfileCardDecorationLayout>();
            var user = GetUser();

            if (req.Layout != null)
            {
                user.ProfileCardLayout.BackgroundId = req.Layout.BackgroundId;
                user.ProfileCardLayout.ShowCharacterSpine = req.Layout.ShowCharacterSpine;

                // Clear old stickers and save the new ones
                user.ProfileCardLayout.StickerPlacements.Clear();
                foreach (var sticker in req.Layout.StickerPlacements)
                {
                    user.ProfileCardLayout.StickerPlacements.Add(new Models.DbStickerPlacement
                    {
                        StickerId = sticker.StickerId,
                        Layer = sticker.Layer,
                        Normalizedx = sticker.Normalizedx,
                        Normalizedy = sticker.Normalizedy,
                        RotationRadian = sticker.RotationRadian,
                        Scale = sticker.Scale
                    });
                }

                JsonDb.Save();
            }

            ResSaveProfileCardDecorationLayout response = new();
            await WriteDataAsync(response);
        }
    }
}