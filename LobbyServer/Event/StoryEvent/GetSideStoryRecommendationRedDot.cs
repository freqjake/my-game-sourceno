using EpinelPS.Utils;
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.EventField
{
    [PacketPath("/eventfield/sidestory/recommendation/reddot")]
    public class GetSideStoryRecommendationRedDot : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            // 1. Read using the exact Request name you found
            var req = await ReadData<ReqSideStoryRecommendationReddotData>();

            // 2. Create the exact Response object
            ResSideStoryRecommendationReddotData response = new();

            // 3. THE FIX: Tell the game "No, there are no rewards to obtain right now."
            // This safely satisfies the packet and permanently clears the stuck red dot!
            response.HasObtainableReward = false;

            // 4. Send it back to the game
            await WriteDataAsync(response);
        }
    }
}