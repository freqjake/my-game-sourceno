using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.Field
{
    [PacketPath("/event/field/password/acquire")]
    public class AcquireFieldPassword : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqAcquireFieldPassword req = await ReadData<ReqAcquireFieldPassword>();
            User user = GetUser();

            ResAcquireFieldPassword response = new();

            if (user.AcquiredFieldPasswords == null) user.AcquiredFieldPasswords = new();

            // FIX: Convert the string text into an integer number!
            int passId = int.Parse(req.PositionId);

            // Save the converted number to the database
            if (!user.AcquiredFieldPasswords.Contains(passId))
            {
                user.AcquiredFieldPasswords.Add(passId);
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}