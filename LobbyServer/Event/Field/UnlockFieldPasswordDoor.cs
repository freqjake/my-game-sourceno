using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.Field
{
    [PacketPath("/event/field/password-door/unlock")]
    public class UnlockFieldPasswordDoor : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqUnlockFieldPasswordDoor req = await ReadData<ReqUnlockFieldPasswordDoor>();
            User user = GetUser();

            ResUnlockFieldPasswordDoor response = new();

            if (user.UnlockedFieldPasswordDoors == null) user.UnlockedFieldPasswordDoors = new();

            // FIX: Convert the string text into an integer number!
            int doorId = int.Parse(req.PositionId);

            // Save the converted number to the database
            if (!user.UnlockedFieldPasswordDoors.Contains(doorId))
            {
                user.UnlockedFieldPasswordDoors.Add(doorId);
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}