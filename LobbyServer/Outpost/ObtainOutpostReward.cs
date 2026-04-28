using EpinelPS.Database;
using EpinelPS.Utils;
using EpinelPS.Data;

namespace EpinelPS.LobbyServer.Outpost
{
    [PacketPath("/outpost/obtainoutpostbattlereward")]
    public class ObtainOutpostReward : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqObtainOutpostBattleReward req = await ReadData<ReqObtainOutpostBattleReward>();
            User user = GetUser();

            ResObtainOutpostBattleReward response = new();


            TimeSpan battleTime = DateTime.UtcNow - user.BattleTime;
            long battleTimeMs = (long)(battleTime.TotalNanoseconds / 100);
            long overBattleTime = battleTimeMs > 864000000000 ? battleTimeMs - 864000000000 : 0;

            response.OutpostBattleTime = new NetOutpostBattleTime() { MaxBattleTime = 864000000000, MaxOverBattleTime = 12096000000000, BattleTime = 0, OverBattleTime = 0 };
            response.BattleTime = 0;
            response.MaxBattleTime = 864000000000;

            response.Reward = NetUtils.GetOutpostReward(user, battleTime);
            NetUtils.RegisterRewardsForUser(user, response.Reward);

            // Fix: Update FinalValue for the client so the balance doesn't become 0
            foreach (var c in response.Reward.Currency)
            {
                c.FinalValue = user.Currency[(CurrencyType)c.Type];
            }

            user.BattleTime = DateTime.UtcNow;

            user.AddTrigger(Trigger.OutpostBattleReward, 1);

            JsonDb.Save();

            await WriteDataAsync(response);
        }
    }
}