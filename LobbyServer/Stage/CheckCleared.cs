using EpinelPS.Utils;
using EpinelPS.Data; // Added to access GameData and ChapterMod

namespace EpinelPS.LobbyServer.Stage
{
    [PacketPath("/stage/checkclear")]
    public class CheckCleared : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqCheckStageClear req = await ReadData<ReqCheckStageClear>();
            ResCheckStageClear response = new();
            User user = GetUser();

            foreach (int requestedStage in req.StageIds)
            {
                bool isCleared = false;

                // 1. Standard Check: Is it physically saved in the map database?
                foreach (KeyValuePair<string, FieldInfoNew> fields in user.FieldInfoNew)
                {
                    if (fields.Value.CompletedStages.Contains(requestedStage))
                    {
                        isCleared = true;
                        break;
                    }
                }

                // 2. AUTO-RECOVERY FIX: If the old buggy code forgot to save the map,
                // mathematically prove the player beat it using their LastStageCleared tracker!
                if (!isCleared)
                {
                    var stageData = GameData.Instance.GetStageData(requestedStage);
                    if (stageData != null)
                    {
                        // If the stage they are asking about is an older level, force it to 'True'
                        if (stageData.ChapterMod == ChapterMod.Normal && requestedStage <= user.LastNormalStageCleared) isCleared = true;
                        if (stageData.ChapterMod == ChapterMod.Hard && requestedStage <= user.LastHardStageCleared) isCleared = true;
                        if (stageData.ChapterMod == ChapterMod.Story && requestedStage <= user.LastStoryStageCleared) isCleared = true;
                    }
                }

                // If either check passed, tell the game client to unlock the buttons!
                if (isCleared)
                {
                    response.ClearedStageIds.Add(requestedStage);
                }
            }

            await WriteDataAsync(response);
        }
    }
}