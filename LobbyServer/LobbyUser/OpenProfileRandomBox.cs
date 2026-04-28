using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/ProfileCard/ProfileRandomBox/Open")]
    public class OpenProfileRandomBox : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqOpenProfileRandomBox req = await ReadData<ReqOpenProfileRandomBox>();
            User user = GetUser();

            // Find the box fragments/pieces in inventory
            DbItemData boxItem = user.Items.FirstOrDefault(x => x.Isn == req.Isn)
                                 ?? throw new InvalidDataException($"Cannot find profile random box with ISN {req.Isn}");

            int piecesRequiredPerOpen = 50;
            int totalPiecesCost = req.NumOpens * piecesRequiredPerOpen;

            if (totalPiecesCost > boxItem.Count)
            {
                throw new Exception($"Not enough pieces! Cost is {totalPiecesCost}, but user only has {boxItem.Count}.");
            }

            // Deduct the pieces
            boxItem.Count -= totalPiecesCost;
            if (boxItem.Count <= 0)
            {
                user.Items.Remove(boxItem);
            }

            NetRewardData boxRewards = NetUtils.UseLootBox(user, boxItem.ItemType, req.NumOpens);
            ResOpenProfileRandomBox response = new();

            int profileCardTicketItemId = 9430001;
            int totalTicketsGained = 0;

            foreach (var item in boxRewards.Item)
            {
                bool isDuplicate = false; // Change to true if you want to test duplicates

                if (isDuplicate)
                {
                    totalTicketsGained += 50;
                    response.OpeningResult.Add(new ProfileRandomBoxSingleOpeningResult
                    {
                        ObjectTid = item.Tid,
                        ExchangedForTicketMaterial = true
                    });
                }
                else
                {
                    response.OpeningResult.Add(new ProfileRandomBoxSingleOpeningResult
                    {
                        ObjectTid = item.Tid,
                        ExchangedForTicketMaterial = false
                    });
                }
            }

            if (totalTicketsGained > 0)
            {
                DbItemData ticketItem = user.Items.FirstOrDefault(x => x.ItemType == profileCardTicketItemId);
                if (ticketItem != null)
                {
                    ticketItem.Count += totalTicketsGained;
                }
                else
                {
                    ticketItem = new DbItemData
                    {
                        ItemType = profileCardTicketItemId,
                        Count = totalTicketsGained,
                        Isn = user.GenerateUniqueItemId()
                    };
                    user.Items.Add(ticketItem);
                }

                // If duplicates need to be synced too, you can handle them here based on what the client expects.
            }

            // THE FIX: Sync the consumed box fragments back to the client so the UI piece count drops instantly!
            // (If this gives a red line, change '=' to use .Add() just like before)
            response.ProfileCardTicketMaterialSync.Add(NetUtils.UserItemDataToNet(boxItem));

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}