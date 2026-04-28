using EpinelPS.Utils;
using EpinelPS.Data;
using EpinelPS.Database;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.Event
{
    /*[PacketPath("/event/boxgacha/get")]
    public class GetEventBoxGacha : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqGetEventBoxGacha req = await ReadData<ReqGetEventBoxGacha>();
            User user = GetUser();

            int ticketItemId = 0;
            try
            {
                if (GameData.Instance.EventBoxGachaTable != null &&
                    GameData.Instance.EventBoxGachaTable.TryGetValue(req.EventId, out var gachaRecord))
                {
                    ticketItemId = gachaRecord.EventItemId;
                }
            }
            catch (Exception) { }

            if (ticketItemId == 0)
            {
                ticketItemId = req.EventId == 160034 ? 7061044 : 7061046;
            }

            // ==========================================
            // PREVENT UI GLITCH ("FREE" BUTTON)
            // By ensuring the user always has at least 100 tickets, 
            // the client will never glitch and display the broken "Free" text!
            // ==========================================
            var existingItem = user.Items.FirstOrDefault(item => item.ItemType == ticketItemId);
            if (existingItem != null)
            {
                if (existingItem.Count < 10)
                {
                    existingItem.Count = 100;
                }
            }
            else
            {
                user.Items.Add(new DbItemData { ItemType = ticketItemId, Count = 100, Isn = user.GenerateUniqueItemId() });
            }
            JsonDb.Save();

            ResGetEventBoxGacha response = new()
            {
                GachaCount = 0
            };

            await WriteDataAsync(response);
        }
    }*/
}