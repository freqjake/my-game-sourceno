using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.MiniGame
{
    [PacketPath("/event/minigame/lycoris/getdata")]
    public class GetLycorisData : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqMiniGameLycorisDataInfo req = await ReadData<ReqMiniGameLycorisDataInfo>();
            User user = GetUser();

            ResMiniGameLycorisDataInfo response = new();
            response.BaseData = new NetMiniGameLycorisData() { Gold = 0, Core = 0 };
            response.Skills = new NetMiniGameLycorisSkill();
            response.DailyTask = new NetMiniGameLycorisDailyTask() { IsFinished = true, IsReceived = true };

            // LOAD THE PROGRESS: Tell the client which stages you already beat!
            if (user.ClearedLycorisMissions != null)
            {
                response.HasEverEnteredMissionIdList.AddRange(user.ClearedLycorisMissions);

                // Add a fake scoreboard entry so the UI shows you have 3-stars on it
                foreach (int missionId in user.ClearedLycorisMissions)
                {
                    response.Missions.Add(new NetMiniGameLycorisMission()
                    {
                        MissionId = missionId,
                        Score = 5000, // Default display score
                        StarMask = 7
                    });
                }
            }
            // Inside GetLycorisData HandleAsync:
            if (user.ClaimedLycorisTasks != null)
            {
                foreach (int taskId in user.ClaimedLycorisTasks)
                {
                    response.Tasks.Add(new NetMiniGameLycorisTask()
                    {
                        TaskId = taskId,
                        IsReceived = true
                    });
                }
            }
            await WriteDataAsync(response);
        }
    }
}