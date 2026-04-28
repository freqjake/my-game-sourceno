using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.Field
{
    [PacketPath("/event/field/object/save")] // Check your server logs for the exact POST path!
    public class SaveEventFieldObject : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSaveEventFieldNonResettableFieldObject req = await ReadData<ReqSaveEventFieldNonResettableFieldObject>();
            User user = GetUser();

            ResSaveEventFieldNonResettableFieldObject response = new();

            if (user.SavedFieldObjects == null) user.SavedFieldObjects = new();

            // Save the unique ID of the switch/door/barrier
            foreach (var fieldObject in req.FieldObjects)
            {
                if (!user.SavedFieldObjects.Contains(fieldObject.PositionId))
                {
                    user.SavedFieldObjects.Add(fieldObject.PositionId);
                }
            }

            JsonDb.Save();
            await WriteDataAsync(response);
        }
    }
}