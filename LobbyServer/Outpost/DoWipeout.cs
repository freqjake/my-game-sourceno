using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/obtainfastbattlereward")]
    public class DoWipeout : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqObtainFastBattleReward req = await ReadData<ReqObtainFastBattleReward>();
            ResObtainFastBattleReward response = new();
            User user = GetUser();

            if (user.ResetableData.WipeoutCount >= 12)
            {
                throw new InvalidOperationException("wipeout count cannot exceed 12.");
            }

            // Fix: Check and subtract currency based on the Fast Battle cost table
            int nextWipeoutId = user.ResetableData.WipeoutCount + 1;
            if (GameData.Instance.OutpostFastBattleTable.TryGetValue(nextWipeoutId, out var fastBattleRecord))
            {
                if (fastBattleRecord.PriceValue > 0)
                {
                    long currentCurrency = user.Currency.GetValueOrDefault(fastBattleRecord.PriceType, 0);
                    if (currentCurrency < fastBattleRecord.PriceValue)
                    {
                        throw new InvalidOperationException($"Not enough currency. Needed: {fastBattleRecord.PriceValue} of type {fastBattleRecord.PriceType}");
                    }

                    // Deduct the currency
                    user.Currency[fastBattleRecord.PriceType] -= fastBattleRecord.PriceValue;
                }
            }

            user.ResetableData.WipeoutCount++;
            response.FastBattleCount = user.ResetableData.WipeoutCount;

            response.Reward = NetUtils.GetOutpostReward(user, TimeSpan.FromHours(2));
            NetUtils.RegisterRewardsForUser(user, response.Reward);

            // Update FinalValue for the client so the balance doesn't become 0
            foreach (var c in response.Reward.Currency)
            {
                c.FinalValue = user.Currency[(CurrencyType)c.Type];
            }

            // Send updated currencies to the client to reflect the deduction in the top bar
            foreach (KeyValuePair<CurrencyType, long> item in user.Currency)
            {
                response.Currencies.Add(new NetUserCurrencyData() { Type = (int)item.Key, Value = item.Value });
            }

            user.AddTrigger(Trigger.OutpostFastBattleReward, 1);

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}