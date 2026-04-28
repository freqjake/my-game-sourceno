using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.MiniGame
{
    [PacketPath("/event/minigame/lycoris/clearmission")]
    public class ClearLycorisMission : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqClearMiniGameLycorisMission req = await ReadData<ReqClearMiniGameLycorisMission>();
            User user = GetUser();

            ResClearMiniGameLycorisMission response = new();

            // Echo the score back to the client
            response.Mission = new NetMiniGameLycorisMission()
            {
                MissionId = req.MissionId,
                Score = req.Score,
                StarMask = req.StarMask
            };

            // SAVE THE PROGRESS: If you won, save the mission ID to your database!
            // (Note: your logs showed "isSuccess": true, so we check that!)
            if (req.IsSuccess)
            {
                if (user.ClearedLycorisMissions == null) user.ClearedLycorisMissions = new();

                if (!user.ClearedLycorisMissions.Contains(req.MissionId))
                {
                    user.ClearedLycorisMissions.Add(req.MissionId);
                    JsonDb.Save(); // Save to db.json!
                }
            }

            // FIX: Use the specific Lycoris class shown in your error message!
            response.Reward = new NetMiniGameLycorisData()
            {
                Gold = 1,
                Core = 10
            };

            await WriteDataAsync(response);
        }
    }
}