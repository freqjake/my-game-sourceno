using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character.Counsel
{
    [PacketPath("/character/counsel/quick")]
    public class QuickCounsel : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqCharacterQuickCounsel req = await ReadData<ReqCharacterQuickCounsel>();
            User user = GetUser();

            ResCharacterQuickCounsel response = new ResCharacterQuickCounsel();

            foreach (KeyValuePair<CurrencyType, long> currency in user.Currency)
            {
                response.Currencies.Add(new NetUserCurrencyData() { Type = (int)currency.Key, Value = currency.Value });
            }

            NetUserAttractiveData? bondInfo = user.BondInfo.FirstOrDefault(x => x.NameCode == req.NameCode);

            if (bondInfo != null)
            {
                // Prevent bypass
                if (!bondInfo.CanCounselToday)
                {
                    await WriteDataAsync(response);
                    return;
                }

                int beforeLv = bondInfo.Lv;
                int beforeExp = bondInfo.Exp;

                bondInfo.Exp += 100;
                bondInfo.CounseledCount++;

                // FIX: Lock character
                bondInfo.CanCounselToday = false;
                UpdateAttractiveLevel(bondInfo);

                response.Attractive = bondInfo;
                response.Exp = new NetIncreaseExpData
                {
                    NameCode = bondInfo.NameCode,
                    BeforeLv = beforeLv,
                    BeforeExp = beforeExp,
                    CurrentLv = bondInfo.Lv,
                    CurrentExp = bondInfo.Exp,
                    GainExp = 100
                };
            }
            else
            {
                NetUserAttractiveData data = new NetUserAttractiveData()
                {
                    NameCode = req.NameCode,
                    Exp = 100,
                    CounseledCount = 1,
                    IsFavorites = false,

                    // FIX: Lock new character
                    CanCounselToday = false,
                    Lv = 1
                };
                UpdateAttractiveLevel(data);
                user.BondInfo.Add(data);
                response.Attractive = data;
                response.Exp = new NetIncreaseExpData
                {
                    NameCode = data.NameCode,
                    BeforeLv = 1,
                    BeforeExp = 0,
                    CurrentLv = 1,
                    CurrentExp = 100,
                    GainExp = 100
                };
            }
            user.AddTrigger(Trigger.CharacterCounsel, 1);
            JsonDb.Save();
            await WriteDataAsync(response);
        }

        private void UpdateAttractiveLevel(NetUserAttractiveData attractiveData)
        {
            while (attractiveData.Lv < 40)
            {
                AttractiveLevelRecord? levelInfo = GameData.Instance.AttractiveLevelTable.Values.FirstOrDefault(x => x.AttractiveLevel == attractiveData.Lv);

                if (levelInfo == null) break;

                if (attractiveData.Exp >= levelInfo.AttractivePoint)
                {
                    attractiveData.Exp -= levelInfo.AttractivePoint;
                    attractiveData.Lv++;
                }
                else
                {
                    break;
                }
            }
        }
    }
}