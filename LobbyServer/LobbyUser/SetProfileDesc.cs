using EpinelPS.Database;
using EpinelPS.Utils;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/user/setprofiledesc")]
    public class SetProfileDesc : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // Read the incoming request containing the new description text
            ReqSetProfileDesc req = await ReadData<ReqSetProfileDesc>();
            var user = GetUser();

            // Save the new description to the user's profile
            user.ProfileDesc = req.Desc;

            // Save the database
            JsonDb.Save();

            // Send the success response back to the client
            ResSetProfileDesc response = new();
            await WriteDataAsync(response);
        }
    }
}