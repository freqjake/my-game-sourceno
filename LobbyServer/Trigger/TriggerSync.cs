using EpinelPS.Utils;
using EpinelPS.Data; // Added to access Currency types
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.TriggerController
{
    [PacketPath("/trigger/sync")]
    public class TriggerSync : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSyncTrigger req = await ReadData<ReqSyncTrigger>();
            User user = GetUser();
            ResSyncTrigger response = new();

            // --- FIX: Initialize SyncData if it's null ---
            
            // 2. Find new triggers
            TriggerModel[] newTriggers = [.. user.Triggers.Where(x => x.Id > req.Seq)];
            //Console.WriteLine($"[SYNC] Seq: {req.Seq} | New Triggers found: {newTriggers.Length}");

            int triggerCount = 0;
            foreach (TriggerModel item in newTriggers)
            {
                triggerCount++;
                response.Triggers.Add(item.ToNet());

             

                if (triggerCount >= 2000)
                {
                    response.HasRemainData = true;
                    break;
                }
            }



            await WriteDataAsync(response);
        }
    }
}