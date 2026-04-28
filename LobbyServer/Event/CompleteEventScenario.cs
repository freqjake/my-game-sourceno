using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Event
{
    [PacketPath("/event/scenario/complete")]
    public class CompleteEventScenario : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqSetEventScenarioComplete req = await ReadData<ReqSetEventScenarioComplete>();
            User user = GetUser();
            ResSetEventScenarioComplete response = new();

            // Locate the event in the user's active event info
            if (user.EventInfo.TryGetValue(req.EventId, out EventData? evtData))
            {
                if (!evtData.CompletedScenarios.Contains(req.ScenarioId))
                {
                    evtData.CompletedScenarios.Add(req.ScenarioId);

                    // Grant Event Scenario Rewards
                    if (GameData.Instance.ScenarioRewards.TryGetValue(req.ScenarioId, out ScenarioRewardsRecord? record))
                    {
                        response.Reward = RewardUtils.RegisterRewardsForUser(user, record.RewardId);

                        // CRITICAL FIX: Sync the FinalValue so the item icon appears in the popup!
                        if (response.Reward != null && response.Reward.Currency != null)
                        {
                            foreach (var c in response.Reward.Currency)
                            {
                                c.FinalValue = user.Currency.ContainsKey((CurrencyType)c.Type) ? user.Currency[(CurrencyType)c.Type] : 0;
                            }
                        }
                    }

                    // Force save to the database so the checkmark stays on relogin!
                    JsonDb.Save();
                }
            }

            await WriteDataAsync(response);
        }
    }
}