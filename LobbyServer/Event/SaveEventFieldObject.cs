using System.Linq;
using System.Threading.Tasks;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.EventField
{
    // Make sure this path matches the exact error in your console!
    // If your console said "/event/field/saveobject", keep it like that.
    [PacketPath("/event/field/saveobject")]
    public class SaveEventFieldObject : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // 1. Read using the exact Archive Request name you found!
            var req = await ReadData<ReqSaveArchiveFieldNonResettableFieldObject>();
            User user = GetUser();

            // 2. Initialize the exact Archive Response
            ResSaveArchiveFieldNonResettableFieldObject response = new();

            // 3. Safely initialize the database memory
            if (user.AcquiredFieldCollectibles == null)
            {
                user.AcquiredFieldCollectibles = new();
            }

            // 4. Because "FieldObjects" is a list, we loop through it to save every item
            // 4. Because "FieldObjects" is a list of objects, we loop through them using 'var'
            // 4. Because "FieldObjects" is a list of objects, we loop through them
            // 4. Loop through the objects
            if (req.FieldObjects != null)
            {
                foreach (var fieldObj in req.FieldObjects)
                {
                    // 1. USE THE SECRET WORD YOU FOUND IN STEP 1 HERE (Replace 'MapObjectSn')
                    // 2. We use (int) to force it into a number format for your database
                    int objectId = (int)fieldObj.Type;

                    if (!user.AcquiredFieldCollectibles.Contains(objectId))
                    {
                        user.AcquiredFieldCollectibles.Add(objectId);
                    }
                }

                JsonDb.Save();
            }

            // 5. Send the success response back
            await WriteDataAsync(response);
        }
    }
}