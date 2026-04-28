using EpinelPS.Data;
using EpinelPS.Database;
using EpinelPS.Utils;

namespace EpinelPS.LobbyServer.Character.Counsel
{
    [PacketPath("/character/attractive/present")]
    public class Present : LobbyMsgHandler
    {
        protected override async Task HandleAsync()
        {
            ReqCharacterPresent req = await ReadData<ReqCharacterPresent>();
            User user = GetUser();

            ResCharacterPresent response = new ResCharacterPresent();

            NetUserAttractiveData? bondInfo = user.BondInfo.FirstOrDefault(x => x.NameCode == req.NameCode);
            if (bondInfo == null)
            {
                return;
            }

            int totalExpGained = 0;
            CharacterRecord? characterRecord = GameData.Instance.CharacterTable.Values.FirstOrDefault(x => x.NameCode == req.NameCode);
            // 1. Create a list to track items that changed to notify the client
            List<NetUserItemData> changedItems = new();
            foreach (NetItemData item in req.Items)
            {
                ItemMaterialRecord? materialInfo = GameData.Instance.itemMaterialTable.GetValueOrDefault(item.Tid);
                if (materialInfo != null && materialInfo.ItemSubType == ItemSubType.AttractiveMaterial)
                {
                    int expGained = materialInfo.ItemValue * (int)item.Count;

                    if (characterRecord != null)
                    {
                        if (materialInfo.MaterialType == MaterialType.Corporation)
                        {
                            string corporation = materialInfo.NameLocalkey.Split('_')[2];
                            if (corporation.Equals(characterRecord.Corporation.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                expGained *= 5;
                            }
                        }
                        else if (materialInfo.MaterialType == MaterialType.Squad)
                        {
                            string squad = materialInfo.NameLocalkey.Split('_')[2];
                            if (squad.Equals(characterRecord.Squad.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                expGained *= 3;
                            }
                        }
                    }

                    totalExpGained += expGained;

                    DbItemData? userItem = user.Items.FirstOrDefault(x => x.ItemType == item.Tid);
                    if (userItem != null)
                    {
                        // 2. Subtract the amount used
                        userItem.Count -= (int)item.Count;

                        // 3. Add to the notification list BEFORE removing it from the database list
                        changedItems.Add(NetUtils.UserItemDataToNet(userItem));

                        // 4. Now safely remove from the database list if depleted
                        if (userItem.Count <= 0)
                        {
                            user.Items.Remove(userItem);
                        }
                    }
                }
            }

            int beforeLv = bondInfo.Lv;
            int beforeExp = bondInfo.Exp;

            bondInfo.Exp += totalExpGained;
            UpdateAttractiveLevel(bondInfo);

            response.Attractive = bondInfo;
            response.Exp = new NetIncreaseExpData
            {
                NameCode = bondInfo.NameCode,
                BeforeLv = beforeLv,
                BeforeExp = beforeExp,
                CurrentLv = bondInfo.Lv,
                CurrentExp = bondInfo.Exp,
                GainExp = totalExpGained
            };

            // 5. Send ONLY the changed items to the client for immediate UI update
            response.Items.Clear(); // Ensure it's clean
            response.Items.AddRange(changedItems);

            JsonDb.Save();
            await WriteDataAsync(response);
        }

        private void UpdateAttractiveLevel(NetUserAttractiveData attractiveData)
        {
            while (attractiveData.Lv < 40)
            {
                AttractiveLevelRecord? levelInfo = GameData.Instance.AttractiveLevelTable.Values.FirstOrDefault(x => x.AttractiveLevel == attractiveData.Lv);

                if (levelInfo == null)
                {
                    break;
                }

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