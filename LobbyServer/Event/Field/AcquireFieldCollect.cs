using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.Field
{
    [PacketPath("/event/field/collect/acquire")]
    public class AcquireEventCollect : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqAcquireEventCollect req = await ReadData<ReqAcquireEventCollect>();
            User user = GetUser();

            ResAcquireEventCollect response = new();

            if (user.AcquiredFieldCollectibles == null) user.AcquiredFieldCollectibles = new();

            // Try to parse the text into a number
            // Note: If this line gives a red error, it means PositionId is ALREADY a number! 
            // If so, just change it to: int collectId = req.PositionId;
            int collectId = int.Parse(req.PositionId);

            if (!user.AcquiredFieldCollectibles.Contains(collectId))
            {
                user.AcquiredFieldCollectibles.Add(collectId);
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}