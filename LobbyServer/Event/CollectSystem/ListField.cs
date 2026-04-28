using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.Field
{
    [PacketPath("/event/collect-system/list-field")]
    public class ListFieldEventCollectData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqListFieldEventCollectData req = await ReadData<ReqListFieldEventCollectData>();
            User user = GetUser();

            ResListFieldEventCollectData response = new();

            if (user.AcquiredFieldCollectibles != null && user.AcquiredFieldCollectibles.Count > 0)
            {
                NetEventCollectData collectData = new NetEventCollectData();

                // This line will no longer have a red error!
                collectData.EventCollectIdList.AddRange(user.AcquiredFieldCollectibles);

                response.FieldEventCollectList.Add(collectData);
            }

            await WriteDataAsync(response);
        }
    }
}