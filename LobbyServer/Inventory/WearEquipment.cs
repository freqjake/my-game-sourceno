using EpinelPS.Database;
using EpinelPS.Utils;
using System.Linq;

namespace EpinelPS.LobbyServer.Inventory
{
    [PacketPath("/inventory/wearequipment")]
    public class WearEquipment : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqWearEquipment req = await ReadData<ReqWearEquipment>();
            User user = GetUser();

            ResWearEquipment response = new();

            // Get the slot position (e.g. Head, Body, Legs) for the target item
            int pos = NetUtils.GetItemPos(user, req.Isn);

            // STEP 1: Find the old item that is currently equipped in this slot
            DbItemData? oldItem = user.Items.FirstOrDefault(i => i.Csn == req.Csn && i.Position == pos);

            if (oldItem != null)
            {
                // Unequip the old item
                oldItem.Csn = 0;

                // BUG FIX: We MUST add the unequipped item to the response so the client knows it went back to the bag
                response.Items.Add(NetUtils.ToNet(oldItem));
            }

            // STEP 2: Find the new item the user wants to equip
            DbItemData? newItem = user.Items.FirstOrDefault(i => i.Isn == req.Isn);

            if (newItem != null)
            {
                // Equip the new item to the character
                newItem.Csn = req.Csn;
                newItem.Position = pos;

                // Add the newly equipped item to the response
                response.Items.Add(NetUtils.ToNet(newItem));
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}