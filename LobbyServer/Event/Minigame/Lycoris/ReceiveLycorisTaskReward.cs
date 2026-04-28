using EpinelPS.Utils;
using EpinelPS.Database;

namespace EpinelPS.LobbyServer.Event.MiniGame
{
    [PacketPath("/event/minigame/lycoris/receivetaskreward")]
    public class ReceiveLycorisTaskReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            var req = await ReadData<ReqMiniGameLycorisReceiveTaskReward>();
            User user = GetUser();

            ResMiniGameLycorisReceiveTaskReward response = new();
            response.Reward = new NetRewardData();

            if (req.TaskIds != null)
            {
                if (user.ClaimedLycorisTasks == null) user.ClaimedLycorisTasks = new();

                foreach (int id in req.TaskIds)
                {
                    // 1. Logic to decide WHICH reward to give based on Task ID
                    switch (id)
                    {
                        case 1: // If Task ID is 1
                            response.Reward.Item.Add(new NetItemData { Tid = 7091001, Count = 10 }); // Skill Manual 1
                            break;
                        case 2: // If Task ID is 2
                            response.Reward.Item.Add(new NetItemData { Tid = 7091002, Count = 10 }); // Skill Manual 2
                            break;
                        case 3: // If Task ID is 3
                            response.Reward.Item.Add(new NetItemData { Tid = 7091003, Count = 10 }); // Skill Manual 3 or Burst Manual
                            break;
                        default: // For all other tasks
                            response.Reward.Item.Add(new NetItemData { Tid = 2, Count = 1000 }); // Default Credits
                            break;
                    }

                    // 2. Save progress to database
                    if (!user.ClaimedLycorisTasks.Contains(id))
                    {
                        user.ClaimedLycorisTasks.Add(id);
                    }

                    // 3. Tell the UI this task is now "Received"
                    response.Tasks.Add(new NetMiniGameLycorisTask()
                    {
                        TaskId = id,
                        IsReceived = true
                    });
                }

                JsonDb.Save();
            }

            await WriteDataAsync(response);
        }
    }
}