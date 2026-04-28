using EpinelPS.Utils;
using EpinelPS.Database; // Make sure this is here to access the User!
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.MiniGame
{
    [PacketPath("/MiniGame/TTS/Enter")]
    public class EnterTTSMiniGame : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqEnterMiniGameTtsTitle>();
            ResEnterMiniGameTtsTitle response = new();

            // 1. THE EXPERT UNLOCK FIX
            // The loading screen checks this object. If it is null, it crashes!
            response.ExpertUnlockResult = new();

            // 2. Grab your database entry
            User user = GetUser();

            // 3. Build your profile
            response.UserData = new NetWholeUserData()
            {
                Usn = (long)user.ID,
                Server = 10001,
                Nickname = user.Nickname,
                Lv = user.userPointData?.UserLevel ?? 99,
                Icon = user.ProfileIconId,
                IconPrism = user.ProfileIconIsPrism,
                Frame = user.ProfileFrame,
                LastActionAt = user.LastLogin.Ticks,
                UserTitleId = user.TitleId,
                GuildName = user.Nickname,
            };

            // 4. Core State & Timers
            response.HasFinishedTutorial = true;
            response.PlayCount = 1;
            // 1. Fix for the Play Time (just use a normal 0, no 'L')
            // Tells Protobuf exactly "Zero Seconds"
            response.TotalPlayTime = new Google.Protobuf.WellKnownTypes.Duration { Seconds = 0 };

            // 2. Fix for the Timestamp (Creates a proper Protobuf Timestamp for "right now")
            response.ServerTimeStamp = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(System.DateTime.UtcNow);

            // 5. THE MASTER KEY FIX: Unlock EVERYTHING!
            // We loop through hundreds of IDs to guarantee we hit every song in the game files.

            // First, unlock standard IDs (1 to 50)
            for (int i = 1; i <= 50; i++)
            {
                response.AvailableEventTtsSongManagerTableIds.Add(i);
                response.NewUnlockedEventTtsSongManagerTableIds.Add(i);
            }

            // Second, unlock the Event-specific IDs (830000 to 830050)
            for (int i = 830000; i <= 830050; i++)
            {
                response.AvailableEventTtsSongManagerTableIds.Add(i);
                response.NewUnlockedEventTtsSongManagerTableIds.Add(i);
            }

            // Set the last played to the first event song to be safe
            response.LastPlayedDifficulty = (EpinelPS.MiniGameTtsDifficulty)1;

            // 6. Ranks (Positions set to 1 to prevent math crashes)
            response.MyUnionTotalRankData = new NetMyMiniGameTtsTotalRankData() { Score = 0, Position = 1 };
            response.MyFriendTotalRankData = new NetMyMiniGameTtsTotalRankData() { Score = 0, Position = 1 };

            // 7. Song History
            response.SongPlayDataList.Add(new NetMiniGameTtsSongPlayData()
            {
                // We just type the number directly here now!
                EventTtsSongManagerTableId = 830001,
                Difficulty = (EpinelPS.MiniGameTtsDifficulty)1,
                IsCleared = true,
                Score = 0
            });

            await WriteDataAsync(response);
        }
    }
}