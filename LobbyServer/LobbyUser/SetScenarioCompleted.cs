using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.LobbyUser
{
    [PacketPath("/User/SetScenarioComplete")]
    public class SetScenarioCompleted : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetScenarioComplete req = await ReadData<ReqSetScenarioComplete>();
            User user = GetUser();

            ResSetScenarioComplete response = new()
            {
                Reward = new NetRewardData()
            };

            if (!user.CompletedScenarios.Contains(req.ScenarioId))
            {
                user.CompletedScenarios.Add(req.ScenarioId);

                // Grant rewards if the scenario has any
                if (GameData.Instance.ScenarioRewards.TryGetValue(req.ScenarioId, out ScenarioRewardsRecord? record))
                {
                    response.Reward = RewardUtils.RegisterRewardsForUser(user, record.RewardId);

                    // CRITICAL FIX: Sync the FinalValue so the Gem icon appears in the popup!
                    if (response.Reward != null && response.Reward.Currency != null)
                    {
                        foreach (var c in response.Reward.Currency)
                        {
                            c.FinalValue = user.Currency.ContainsKey((CurrencyType)c.Type) ? user.Currency[(CurrencyType)c.Type] : 0;
                        }
                    }
                }

                // Force save to the database so it survives a relogin
                JsonDb.Save();
            }

            await WriteDataAsync(response);
        }
    }
}