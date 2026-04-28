using EpinelPS.Database;
using EpinelPS.Utils;
using Google.Protobuf;

namespace EpinelPS.LobbyServer.Event.Field
{
    [PacketPath("/event/field/password-door/list")]
    public class ListPasswordDoor : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqListFieldPasswordDoorData req = await ReadData<ReqListFieldPasswordDoorData>();
            User user = GetUser();

            ResListFieldPasswordDoorData response = new();

            // 1. Send the password clues the user has picked up
            if (user.AcquiredFieldPasswords != null)
            {
                response.AcquiredFieldPasswordIdList.AddRange(user.AcquiredFieldPasswords);
            }

            // 2. Send the doors the user has permanently unlocked
            if (user.UnlockedFieldPasswordDoors != null)
            {
                response.UnlockedFieldPasswordDoorIdList.AddRange(user.UnlockedFieldPasswordDoors);
            }

            await WriteDataAsync(response);
        }
    }
}