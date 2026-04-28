using EpinelPS.Utils;
using EpinelPS.Database; // Ensures we can use JsonDb
using System.Threading.Tasks;

namespace EpinelPS.LobbyServer.MiniGame
{
    [PacketPath("/MiniGame/TTS/Finish")]
    public class FinishTTSSong : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqFinishMiniGameTtsPlay>();
            User user = GetUser();

            int songId = req.EventTtsSongManagerTableId;
            long finalScore = req.Score;

            // Check if they beat their old high score (or if it's their first time playing)
            if (!user.TtsSongHighScores.ContainsKey(songId) || finalScore > user.TtsSongHighScores[songId])
            {
                long oldScore = user.TtsSongHighScores.ContainsKey(songId) ? user.TtsSongHighScores[songId] : 0;

                // Add the difference to their total server score
                user.TotalTtsScore += (finalScore - oldScore);

                // Overwrite the old high score with the new one
                user.TtsSongHighScores[songId] = finalScore;

                // SAVE TO DB.JSON!
                JsonDb.Save();
            }

            ReqFinishMiniGameTtsPlay response = new();
            await WriteDataAsync(response);
        }
    }
}