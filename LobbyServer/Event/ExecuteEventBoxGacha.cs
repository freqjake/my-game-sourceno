using System;
using System.Linq;
using System.Threading.Tasks;
using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/boxgacha/execute")]
    public class ExecuteEventBoxGacha : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqExecuteEventBoxGacha req = await ReadData<ReqExecuteEventBoxGacha>();
            User user = GetUser();

            int count = req.CurrentCount > 0 ? req.CurrentCount : 1;
            int ticketItemId = req.EventId == 160034 ? 7061044 : 7061046;

            DbItemData? existingTicket = user.Items.FirstOrDefault(item => item.ItemType == ticketItemId);
            int remainingTickets = 0;

            if (existingTicket != null && existingTicket.Count >= count)
            {
                existingTicket.Count -= count;
                remainingTickets = existingTicket.Count;
            }
            else
            {
                remainingTickets = existingTicket?.Count ?? 0;
            }

            // Initialize the Response
            ResExecuteEventBoxGacha response = new()
            {
                Reward = new NetRewardData() { PassPoint = new() },
                Ticket = new() { Tid = ticketItemId, Count = remainingTickets }
            };

           
            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}