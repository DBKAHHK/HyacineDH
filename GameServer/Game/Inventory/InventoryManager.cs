using System.Collections.Frozen;
using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Enums.Item;
using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Game.Scene;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Avatar;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Lineup;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Player;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.PlayerSync;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;
using Google.Protobuf.Collections;

namespace EggLink.DanhengServer.GameServer.Game.Inventory;

public class InventoryManager(PlayerInstance player) : BasePlayerManager(player)
{
    public InventoryData Data = DatabaseHelper.Instance!.GetInstanceOrCreateNew<InventoryData>(player.Uid);

    public async ValueTask AddItem(ItemData itemData, bool notify = true)
    {
        await PutItem(itemData.ItemId, itemData.Count,
            itemData.Rank, itemData.Promotion,
            itemData.Level, itemData.Exp, itemData.TotalExp,
            itemData.MainAffix, itemData.SubAffixes,
            itemData.ReforgeSubAffixes, itemData.UniqueId);

        await Player.SendPacket(new PacketPlayerSyncScNotify(itemData));
        if (notify) await Player.SendPacket(new PacketScenePlaneEventScNotify(itemData));
    }

    public async ValueTask AddItems(List<ItemData> items, bool notify = true)
    {
        var syncItems = new List<ItemData>();
        foreach (var item in items)
        {
            var i = await AddItem(item.ItemId, item.Count, false, sync: false, returnRaw: true);
            if (i != null) syncItems.Add(i);
        }

        await Player.SendPacket(new PacketPlayerSyncScNotify(syncItems));
        if (notify) await Player.SendPacket(new PacketScenePlaneEventScNotify(items));
    }

    public async ValueTask<ItemData?> AddItem(int itemId, int count, bool notify = true, int rank = 1, int level = 1,
        bool sync = true, bool returnRaw = false)
    {
        GameData.ItemConfigData.TryGetValue(itemId, out var itemConfig);
        if (itemConfig == null) return null;

        ItemData? itemData = null;

        switch (itemConfig.ItemMainType)
        {
            case ItemMainTypeEnum.Equipment:
                if (Data.EquipmentItems.Count + 1 > GameConstants.INVENTORY_MAX_EQUIPMENT) // get the max equipment
                {
                    await Player.SendPacket(new PacketRetcodeNotify(Retcode.RetEquipmentExceedLimit));
                    break;
                }

                itemData = await PutItem(itemId, 1, rank, level: level, uniqueId: ++Data.NextUniqueId);
                break;
            case ItemMainTypeEnum.Usable:
                switch (itemConfig.ItemSubType)
                {
                    case ItemSubTypeEnum.HeadIcon:
                        Player.PlayerUnlockData!.HeadIcons.Add(itemId);
                        break;
                    case ItemSubTypeEnum.ChatBubble:
                        Player.PlayerUnlockData!.ChatBubbles.Add(itemId);
                        break;
                    case ItemSubTypeEnum.PhoneTheme:
                        Player.PlayerUnlockData!.PhoneThemes.Add(itemId);
                        break;
                    case ItemSubTypeEnum.PersonalCard:
                        Player.PlayerUnlockData!.PersonalCards.Add(itemId);
                        break;
                    case ItemSubTypeEnum.PhoneCase:
                        Player.PlayerUnlockData!.PhoneCases.Add(itemId);
                        break;
                    case ItemSubTypeEnum.AvatarSkin:
                        var avatarId = GameData.AvatarSkinData[itemId].AvatarID;
                        if (!Player.PlayerUnlockData!.Skins.TryGetValue(avatarId, out var value))
                        {
                            value = [];
                            Player.PlayerUnlockData.Skins[avatarId] = value;
                        }

                        value.Add(itemId);
                        await Player.SendPacket(new PacketUnlockAvatarSkinScNotify(itemId));
                        break;
                    case ItemSubTypeEnum.Food:
                    case ItemSubTypeEnum.Book:
                    case ItemSubTypeEnum.FindChest:
                        itemData = await PutItem(itemId, count);
                        break;
                }

                itemData ??= new ItemData
                {
                    ItemId = itemId,
                    Count = count
                };
                break;
            case ItemMainTypeEnum.Relic:
                //I dont think one player can have more than max count of relic until i see a player get 50000 relic and the client crashed :(
                if (Data.RelicItems.Count + 1 > GameConstants.INVENTORY_MAX_RELIC) // get the max relic
                {
                    await Player.SendPacket(new PacketRetcodeNotify(Retcode.RetRelicExceedLimit));
                    break;
                }

                (_, itemData) = await HandleRelic(itemId, ++Data.NextUniqueId, 0);
                break;
            case ItemMainTypeEnum.Virtual:
                var actualCount = 0;
                switch (itemConfig.ID)
                {
                    case 1:
                        Player.Data.Hcoin += count;
                        actualCount = Player.Data.Hcoin;
                        break;
                    case 2:
                        Player.Data.Scoin += count;
                        actualCount = Player.Data.Scoin;
                        break;
                    case 3:
                        Player.Data.Mcoin += count;
                        actualCount = Player.Data.Mcoin;
                        break;
                    case 11:
                        Player.Data.Stamina += count;
                        actualCount = Player.Data.Stamina;
                        break;
                    case 22:
                        Player.Data.Exp += count;
                        Player.OnAddExp();
                        actualCount = Player.Data.Exp;
                        break;
                    case 32:
                        Player.Data.TalentPoints += count;
                        // TODO : send VirtualItemPacket instead of PlayerSyncPacket
                        break;
                }

                if (count != 0)
                {
                    await Player.SendPacket(new PacketPlayerSyncScNotify(Player.ToProto()));
                    itemData = new ItemData
                    {
                        ItemId = itemId,
                        Count = actualCount
                    };
                }

                break;
            case ItemMainTypeEnum.AvatarCard:
                // add avatar
                var avatar = Player.AvatarManager?.GetFormalAvatar(itemId);
                if (avatar != null)
                {
                    var rankUpItem = Player.InventoryManager!.GetItem(itemId + 10000);
                    if ((avatar.PathInfos[itemId].Rank + rankUpItem?.Count ?? 0) <= 5)
                        itemData = await PutItem(itemId + 10000, 1);
                }
                else
                {
                    await Player.AddAvatar(itemId, sync, notify);
                    await AddItem(itemId + 200000, 1, false);
                }

                break;
            case ItemMainTypeEnum.Mission:
                itemData = await PutItem(itemId, count);
                break;
            default:
                itemData = await PutItem(itemId, Math.Min(count, itemConfig.PileLimit));
                break;
        }

        ItemData? clone = null;
        if (itemData == null) return returnRaw ? itemData : clone ?? itemData;

        clone = itemData.Clone();
        if (sync)
            await Player.SendPacket(new PacketPlayerSyncScNotify(itemData));
        clone.Count = count;
        if (notify) await Player.SendPacket(new PacketScenePlaneEventScNotify(clone));

        Player.MissionManager?.HandleFinishType(MissionFinishTypeEnum.GetItem, itemData.ToProto());

        return returnRaw ? itemData : clone;
    }

    public async ValueTask<ItemData> PutItem(int itemId, int count, int rank = 0, int promotion = 0, int level = 0,
        int exp = 0, int totalExp = 0, int mainAffix = 0, List<ItemSubAffix>? subAffixes = null,
        List<ItemSubAffix>? regorgeSubAffixes = null, int uniqueId = 0)
    {
        if (promotion == 0 && level > 10) promotion = GameData.GetMinPromotionForLevel(level);
        var item = new ItemData
        {
            ItemId = itemId,
            Count = count,
            Rank = rank,
            Promotion = promotion,
            Level = level,
            Exp = exp,
            TotalExp = totalExp,
            MainAffix = mainAffix,
            SubAffixes = subAffixes ?? [],
            ReforgeSubAffixes = regorgeSubAffixes ?? []
        };

        if (uniqueId > 0) item.UniqueId = uniqueId;

        switch (GameData.ItemConfigData[itemId].ItemMainType)
        {
            case ItemMainTypeEnum.Material:
            case ItemMainTypeEnum.Pet:
            case ItemMainTypeEnum.Virtual:
            case ItemMainTypeEnum.Usable:
            case ItemMainTypeEnum.Mission:
                var oldItem = Data.MaterialItems.Find(x => x.ItemId == itemId);
                if (oldItem != null)
                {
                    oldItem.Count += count;
                    item = oldItem;
                    break;
                }

                Data.MaterialItems.Add(item);
                break;
            case ItemMainTypeEnum.Equipment:
                if (Data.EquipmentItems.Count + 1 > GameConstants.INVENTORY_MAX_EQUIPMENT)
                {
                    await Player.SendPacket(new PacketRetcodeNotify(Retcode.RetEquipmentExceedLimit));
                    return item;
                }

                Data.EquipmentItems.Add(item);
                break;
            case ItemMainTypeEnum.Relic:
                if (Data.RelicItems.Count + 1 > GameConstants.INVENTORY_MAX_RELIC)
                {
                    await Player.SendPacket(new PacketRetcodeNotify(Retcode.RetRelicExceedLimit));
                    return item;
                }

                Data.RelicItems.Add(item);
                break;
        }

        return item;
    }

    public async ValueTask<List<ItemData>> RemoveItems(List<(int itemId, int count, int uniqueId)> items,
        bool sync = true)
    {
        List<ItemData> removedItems = [];
        foreach (var item in items)
        {
            var removedItem = await RemoveItem(item.itemId, item.count, item.uniqueId, false);
            if (removedItem != null) removedItems.Add(removedItem);
        }

        if (sync && removedItems.Count > 0) await Player.SendPacket(new PacketPlayerSyncScNotify(removedItems));
        return removedItems;
    }

    public async ValueTask<ItemData?> RemoveItem(int itemId, int count, int uniqueId = 0, bool sync = true)
    {
        GameData.ItemConfigData.TryGetValue(itemId, out var itemConfig);
        if (itemConfig == null) return null;

        ItemData? itemData = null;

        switch (itemConfig.ItemMainType)
        {
            case ItemMainTypeEnum.Material:
            case ItemMainTypeEnum.Pet:
            case ItemMainTypeEnum.Mission:
            case ItemMainTypeEnum.Usable:
                var item = Data.MaterialItems.Find(x => x.ItemId == itemId);
                if (item == null) return null;
                item.Count -= count;
                if (item.Count <= 0)
                {
                    Data.MaterialItems.Remove(item);
                    item.Count = 0;
                }

                itemData = item;
                break;
            case ItemMainTypeEnum.Virtual:
                switch (itemConfig.ID)
                {
                    case 1:
                        Player.Data.Hcoin -= count;
                        itemData = new ItemData { ItemId = itemId, Count = count };
                        break;
                    case 2:
                        Player.Data.Scoin -= count;
                        itemData = new ItemData { ItemId = itemId, Count = count };
                        break;
                    case 3:
                        Player.Data.Mcoin -= count;
                        itemData = new ItemData { ItemId = itemId, Count = count };
                        break;
                    case 32:
                        Player.Data.TalentPoints -= count;
                        itemData = new ItemData { ItemId = itemId, Count = count };
                        break;
                }

                if (sync && itemData != null) await Player.SendPacket(new PacketPlayerSyncScNotify(Player.ToProto()));
                break;
            case ItemMainTypeEnum.Equipment:
                var equipment = Data.EquipmentItems.Find(x => x.UniqueId == uniqueId);
                if (equipment == null) return null;
                Data.EquipmentItems.Remove(equipment);
                equipment.Count = 0;
                itemData = equipment;
                break;
            case ItemMainTypeEnum.Relic:
                var relic = Data.RelicItems.Find(x => x.UniqueId == uniqueId);
                if (relic == null) return null;
                Data.RelicItems.Remove(relic);
                relic.Count = 0;
                itemData = relic;
                break;
        }

        if (itemData != null && sync) await Player.SendPacket(new PacketPlayerSyncScNotify(itemData));

        Player.MissionManager?.HandleFinishType(MissionFinishTypeEnum.UseItem, new ItemData
        {
            ItemId = itemId,
            Count = count
        });

        return itemData;
    }

    /// <summary>
    ///     Get item by itemId and uniqueId, if uniqueId provided, itemId will be ignored
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="uniqueId"></param>
    /// <returns></returns>
    public ItemData? GetItem(int itemId, int uniqueId = 0, ItemMainTypeEnum mainType = ItemMainTypeEnum.Unknown)
    {
        GameData.ItemConfigData.TryGetValue(itemId, out var itemConfig);
        if (itemConfig == null && mainType == ItemMainTypeEnum.Unknown) return null;
        if (itemConfig != null)
            mainType = itemConfig.ItemMainType;
        switch (mainType)
        {
            case ItemMainTypeEnum.Material:
            case ItemMainTypeEnum.Pet:
                return Data.MaterialItems.Find(x => x.ItemId == itemId);
            case ItemMainTypeEnum.Equipment:
                return uniqueId > 0
                    ? Data.EquipmentItems.Find(x => x.UniqueId == uniqueId)
                    : Data.EquipmentItems.Find(x => x.ItemId == itemId);
            case ItemMainTypeEnum.Relic:
                return uniqueId > 0
                    ? Data.RelicItems.Find(x => x.UniqueId == uniqueId)
                    : Data.RelicItems.Find(x => x.ItemId == itemId);
            case ItemMainTypeEnum.Virtual:
                switch (itemConfig?.ID ?? 0)
                {
                    case 1:
                        return new ItemData
                        {
                            ItemId = itemId,
                            Count = Player.Data.Hcoin
                        };
                    case 2:
                        return new ItemData
                        {
                            ItemId = itemId,
                            Count = Player.Data.Scoin
                        };
                    case 3:
                        return new ItemData
                        {
                            ItemId = itemId,
                            Count = Player.Data.Mcoin
                        };
                    case 11:
                        return new ItemData
                        {
                            ItemId = itemId,
                            Count = Player.Data.Stamina
                        };
                    case 22:
                        return new ItemData
                        {
                            ItemId = itemId,
                            Count = Player.Data.Exp
                        };
                    case 32:
                        return new ItemData
                        {
                            ItemId = itemId,
                            Count = Player.Data.TalentPoints
                        };
                }

                break;
        }

        return null;
    }

    public void HandlePlaneEvent(int eventId)
    {
        GameData.PlaneEventData.TryGetValue(eventId * 10 + Player.Data.WorldLevel, out var planeEvent);
        if (planeEvent == null) return;
        GameData.RewardDataData.TryGetValue(planeEvent.Reward, out var rewardData);
        rewardData?.GetItems().ForEach(async x => await AddItem(x.Item1, x.Item2));

        foreach (var id in planeEvent.DropList)
        {
            GameData.RewardDataData.TryGetValue(id, out var reward);
            reward?.GetItems().ForEach(async x => await AddItem(x.Item1, x.Item2));
        }
    }

    public async ValueTask<List<ItemData>> HandleReward(int rewardId, bool notify = false, bool sync = true)
    {
        GameData.RewardDataData.TryGetValue(rewardId, out var rewardData);
        if (rewardData == null) return [];
        List<ItemData> items = [];

        foreach (var item in rewardData.GetItems())
        {
            var i = await AddItem(item.Item1, item.Item2, notify, sync: false);
            if (i != null) items.Add(i);
        }

        if (sync)
            await Player.SendPacket(new PacketPlayerSyncScNotify(items));

        var hCoin = await AddItem(1, rewardData.Hcoin, notify, sync: false);
        if (hCoin != null)
            items.Add(hCoin);

        return items;
    }

    public async ValueTask<List<ItemData>> HandleMappingInfo(int mappingId, int worldLevel)
    {
        // calculate drops
        List<ItemData> items = [];
        List<ItemData> resItems = [];
        GameData.MappingInfoData.TryGetValue(mappingId * 10 + worldLevel, out var mapping);
        if (mapping != null)
        {
            foreach (var item in mapping.DropItemList)
            {
                var random = Random.Shared.Next(0, 101);

                if (random <= item.Chance)
                {
                    var amount = item.ItemNum > 0 ? item.ItemNum : Random.Shared.Next(item.MinCount, item.MaxCount + 1);

                    GameData.ItemConfigData.TryGetValue(item.ItemID, out var itemData);
                    if (itemData == null) continue;

                    items.Add(new ItemData
                    {
                        ItemId = item.ItemID,
                        Count = amount * (item.ItemID == 22
                            ? 1
                            : ConfigManager.Config.ServerOption.ValidFarmingDropRate())
                    });
                }
            }

            // Generate relics
            var relicDrops = mapping.GenerateRelicDrops();

            // Let AddItem notify relics count exceeding limit 
            items.AddRange(Data.RelicItems.Count + relicDrops.Count - 1 > GameConstants.INVENTORY_MAX_RELIC
                ? relicDrops[..(GameConstants.INVENTORY_MAX_RELIC - Data.RelicItems.Count + 1)]
                : relicDrops);

            foreach (var item in items)
            {
                var i = (await Player.InventoryManager!.AddItem(item.ItemId, item.Count, false))!;
                i.Count = item.Count; // return the all thing

                resItems.Add(i);
            }
        }

        return resItems;
    }

    public async ValueTask<(int, ItemData?)> HandleRelic(
        int relicId, int uniqueId, int level, int mainAffixId = 0, List<(int, int)>? subAffixes = null)
    {
        // Excel
        GameData.RelicConfigData.TryGetValue(relicId, out var itemConfig);
        GameData.ItemConfigData.TryGetValue(relicId, out var itemConfigExcel);
        if (itemConfig == null || itemConfigExcel == null)
            return (1, null);

        GameData.RelicSubAffixData.TryGetValue(itemConfig.SubAffixGroup, out var subAffixConfig);
        GameData.RelicMainAffixData.TryGetValue(itemConfig.MainAffixGroup, out var mainAffixConfig);
        if (subAffixConfig == null || mainAffixConfig == null)
            return (1, null);

        var relic = new ItemData
        {
            ItemId = relicId,
            Level = Math.Max(Math.Min(level, 9999), 0),
            UniqueId = uniqueId,
            Count = 1
        };

        // MainAffixId
        if (mainAffixId == 0 || !mainAffixConfig.TryGetValue(mainAffixId, out _))
            relic.AddRandomRelicMainAffix();
        else
            relic.MainAffix = mainAffixId;

        // SubAffixes
        subAffixes ??= [];
        if (subAffixes.Count > 4) return (3, null);
        relic.AddRelicSubAffix(subAffixes); // Add from input

        var initSubCnt = new Random().Next(3, 5);
        relic.AddRandomRelicSubAffix(initSubCnt - subAffixes.Count);
        if (initSubCnt == 3 && level / 3 > 0) relic.AddRandomRelicSubAffix(); // Random add init subAffixes

        var remainUpCnt = level / 3 - (4 - initSubCnt) - subAffixes.Sum(x => x.Item2);
        relic.IncreaseRandomRelicSubAffix(remainUpCnt); // Level up

        await Player.InventoryManager!.AddItem(relic, false);
        return (0, relic);
    }

    public async ValueTask<ItemData?> ComposeItem(int composeId, int count, List<ItemCost> costData)
    {
        // Cost items in req
        foreach (var cost in costData)
            await RemoveItem((int)cost.PileItem.ItemId, (int)cost.PileItem.ItemNum);

        // Cost items in excel
        GameData.ItemComposeConfigData.TryGetValue(composeId, out var composeConfig);
        if (composeConfig == null) return null;
        foreach (var cost in composeConfig.MaterialCost)
            await RemoveItem(cost.ItemID, cost.ItemNum * count);

        await RemoveItem(2, composeConfig.CoinCost * count);

        return await AddItem(composeConfig.ItemID, count, false);
    }

    public async ValueTask<ItemData?> ComposeRelic(ComposeSelectedRelicCsReq req)
    {
        // Cost items in req
        if (req.ComposeItemList != null)
            foreach (var cost in req.ComposeItemList.ItemList)
                await RemoveItem((int)cost.PileItem.ItemId, (int)cost.PileItem.ItemNum);
        if (req.WrItemList != null)
            foreach (var subCost in req.WrItemList.ItemList)
                await RemoveItem((int)subCost.PileItem.ItemId, (int)subCost.PileItem.ItemNum);

        // Cost items in excel
        GameData.ItemComposeConfigData.TryGetValue((int)req.ComposeId, out var composeConfig);
        if (composeConfig == null) return null;
        foreach (var cost in composeConfig.MaterialCost)
            await RemoveItem(cost.ItemID, (int)(cost.ItemNum * req.Count));

        await RemoveItem(2, (int)(composeConfig.CoinCost * req.Count));

        var relicId = (int)req.ComposeRelicId;
        GameData.RelicConfigData.TryGetValue(relicId, out var itemConfig);
        GameData.RelicSubAffixData.TryGetValue(itemConfig!.SubAffixGroup, out var subAffixConfig);

        // Add relic
        var mainAffix = (int)req.MainAffixId;
        var itemData = new ItemData
        {
            ItemId = relicId,
            Level = 0,
            UniqueId = ++Data.NextUniqueId,
            MainAffix = mainAffix,
            SubAffixes = req.SubAffixIdList.Select(subId => new ItemSubAffix(subAffixConfig![(int)subId], 1)).ToList(),
            Count = 1
        };
        if (mainAffix == 0) itemData.AddRandomRelicMainAffix();
        itemData.AddRandomRelicSubAffix(3 - itemData.SubAffixes.Count + itemData.LuckyRelicSubAffixCount());
        await AddItem(itemData, false);

        return itemData;
    }

    public async ValueTask<List<ItemData>> SellItem(ItemCostData costData, bool toMaterial)
    {
        List<ItemData> items = [];
        Dictionary<int, int> itemMap = [];
        List<(int itemId, int count, int uniqueId)> removeItems = [];

        foreach (var cost in costData.ItemList)
            if (cost.EquipmentUniqueId != 0) // equipment
            {
                var itemData = Data.EquipmentItems.Find(x => x.UniqueId == cost.EquipmentUniqueId);
                if (itemData == null) continue;
                removeItems.Add((itemData.ItemId, 1, (int)cost.EquipmentUniqueId));
                GameData.ItemConfigData.TryGetValue(itemData.ItemId, out var itemConfig);
                if (itemConfig == null) continue;
                foreach (var returnItem in itemConfig.ReturnItemIDList) // return items
                {
                    if (!itemMap.ContainsKey(returnItem.ItemID)) itemMap[returnItem.ItemID] = 0;
                    itemMap[returnItem.ItemID] += returnItem.ItemNum;
                }
            }
            else if (cost.RelicUniqueId != 0) // relic
            {
                var itemData = Data.RelicItems.Find(x => x.UniqueId == cost.RelicUniqueId);
                if (itemData == null) continue;
                removeItems.Add((itemData.ItemId, 1, (int)cost.RelicUniqueId));
                GameData.ItemConfigData.TryGetValue(itemData.ItemId, out var itemConfig);
                if (itemConfig == null) continue;
                if (itemConfig.Rarity != ItemRarityEnum.SuperRare || toMaterial)
                {
                    foreach (var returnItem in itemConfig.ReturnItemIDList) // basic return items
                    {
                        itemMap.TryAdd(returnItem.ItemID, 0);
                        itemMap[returnItem.ItemID] += returnItem.ItemNum;
                    }

                    var expReturned = (int)(itemData.CalcTotalExpGained() * 0.8);

                    var credit = (int)(expReturned * 1.5);
                    if (credit > 0)
                    {
                        itemMap.TryAdd(2, 0);
                        itemMap[2] += (int)(expReturned * 1.5);
                    }

                    var lostGoldFragCnt = expReturned / 500;
                    if (lostGoldFragCnt > 0)
                    {
                        itemMap.TryAdd(232, 0);
                        itemMap[232] += lostGoldFragCnt;
                    }

                    var lostGoldLightdust = expReturned % 500 / 100;
                    if (lostGoldLightdust > 0)
                    {
                        itemMap.TryAdd(231, 0);
                        itemMap[231] += lostGoldLightdust;
                    }
                }
                else
                {
                    var expGained = itemData.CalcTotalExpGained();
                    var remainsCnt = (int)(10 + expGained * 0.005144);
                    if (remainsCnt > 0)
                    {
                        itemMap.TryAdd(235, 0);
                        itemMap[235] += remainsCnt;
                    }
                }
            }
            else
            {
                removeItems.Add(((int)cost.PileItem.ItemId, (int)cost.PileItem.ItemNum, 0));
            }

        var removedItems = RemoveItems(removeItems);

        foreach (var itemInfo in itemMap)
        {
            var item = await AddItem(itemInfo.Key, itemInfo.Value, false);
            if (item != null) items.Add(item);
        }

        return items;
    }

    public async ValueTask<(Retcode, List<ItemData>? returnItems)> UseItem(int itemId, int count = 1,
        int baseAvatarId = 0)
    {
        GameData.ItemConfigData.TryGetValue(itemId, out var itemConfig);
        if (itemConfig == null) return (Retcode.RetItemNotExist, null);
        var dataId = itemConfig.ID;
        GameData.ItemUseBuffDataData.TryGetValue(dataId, out var useConfig);
        if (useConfig == null) return (Retcode.RetItemUseConfigNotExist, null);

        for (var i = 0; i < count; i++) // do count times
        {
            if (useConfig.PreviewSkillPoint != 0) await Player.LineupManager!.GainMp((int)useConfig.PreviewSkillPoint);

            if (baseAvatarId > 0)
            {
                // single use
                var avatar = Player.AvatarManager!.GetFormalAvatar(baseAvatarId);
                if (avatar == null) return (Retcode.RetAvatarNotExist, null);

                var extraLineup = Player.LineupManager!.GetCurLineup()?.IsExtraLineup() == true;

                if (useConfig.PreviewHPRecoveryPercent != 0)
                {
                    avatar.SetCurHp(
                        Math.Min(Math.Max(avatar.CurrentHp + (int)(useConfig.PreviewHPRecoveryPercent * 10000), 0),
                            10000), extraLineup);

                    await Player.SendPacket(new PacketSyncLineupNotify(Player.LineupManager.GetCurLineup()!));
                }

                if (useConfig.PreviewHPRecoveryValue != 0)
                {
                    avatar.SetCurHp(
                        Math.Min(Math.Max(avatar.CurrentHp + (int)useConfig.PreviewHPRecoveryValue, 0), 10000),
                        extraLineup);

                    await Player.SendPacket(new PacketSyncLineupNotify(Player.LineupManager.GetCurLineup()!));
                }

                if (useConfig.PreviewPowerPercent != 0)
                {
                    avatar.SetCurSp(
                        Math.Min(Math.Max(avatar.CurrentHp + (int)(useConfig.PreviewPowerPercent * 10000), 0), 10000),
                        extraLineup);

                    await Player.SendPacket(new PacketSyncLineupNotify(Player.LineupManager.GetCurLineup()!));
                }
            }
            else
            {
                // team use
                if (useConfig.PreviewHPRecoveryPercent != 0)
                {
                    Player.LineupManager!.GetCurLineup()!.Heal((int)(useConfig.PreviewHPRecoveryPercent * 10000), true);

                    await Player.SendPacket(new PacketSyncLineupNotify(Player.LineupManager.GetCurLineup()!));
                }

                if (useConfig.PreviewHPRecoveryValue != 0)
                {
                    Player.LineupManager!.GetCurLineup()!.Heal((int)useConfig.PreviewHPRecoveryValue, true);

                    await Player.SendPacket(new PacketSyncLineupNotify(Player.LineupManager.GetCurLineup()!));
                }

                if (useConfig.PreviewPowerPercent != 0)
                {
                    Player.LineupManager!.GetCurLineup()!.AddPercentSp((int)(useConfig.PreviewPowerPercent * 10000));

                    await Player.SendPacket(new PacketSyncLineupNotify(Player.LineupManager.GetCurLineup()!));
                }
            }
        }

        //maze buff
        if (useConfig.MazeBuffID > 0)
            foreach (var info in Player.SceneInstance?.AvatarInfo.Values.ToList() ?? [])
                if (baseAvatarId == 0 || info.AvatarInfo.BaseAvatarId == baseAvatarId)
                    await info.AddBuff(new SceneBuff(useConfig.MazeBuffID, 1, info.AvatarInfo.AvatarId));

        if (useConfig.MazeBuffID2 > 0)
            foreach (var info in Player.SceneInstance?.AvatarInfo.Values.ToList() ?? [])
                if (baseAvatarId == 0 || info.AvatarInfo.BaseAvatarId == baseAvatarId)
                    await info.AddBuff(new SceneBuff(useConfig.MazeBuffID2, 1, info.AvatarInfo.AvatarId));

        // remove item
        await RemoveItem(itemId, count);

        return (Retcode.RetSucc, null);
    }

    #region Equip

    public async ValueTask EquipAvatar(int avatarId, int equipmentUniqueId)
    {
        var itemData = Data.EquipmentItems.Find(x => x.UniqueId == equipmentUniqueId);
        var avatarData = Player.AvatarManager!.GetFormalAvatar(avatarId);
        if (itemData == null || avatarData == null) return;
        var oldItem = Data.EquipmentItems.Find(x => x.UniqueId == avatarData.PathInfos[avatarId].EquipId);
        if (itemData.EquipAvatar > 0) // already be dressed
        {
            var equipAvatarId = itemData.EquipAvatar;
            var equipAvatar = Player.AvatarManager.GetFormalAvatar(equipAvatarId);
            if (equipAvatar != null && oldItem != null)
            {
                // switch
                equipAvatar.PathInfos[equipAvatarId].EquipId = oldItem.UniqueId;
                oldItem.EquipAvatar = equipAvatar.AvatarId;
                await Player.SendPacket(new PacketPlayerSyncScNotify(equipAvatar, oldItem));
            }
            else if (equipAvatar != null && oldItem == null)
            {
                equipAvatar.PathInfos[equipAvatarId].EquipId = 0;
                await Player.SendPacket(new PacketPlayerSyncScNotify(equipAvatar));
            }
        }
        else
        {
            if (oldItem != null)
            {
                oldItem.EquipAvatar = 0;
                await Player.SendPacket(new PacketPlayerSyncScNotify(oldItem));
            }
        }

        itemData.EquipAvatar = avatarData.AvatarId;
        avatarData.PathInfos[avatarId].EquipId = itemData.UniqueId;
        await Player.SendPacket(new PacketPlayerSyncScNotify(avatarData, itemData));
    }

    public async ValueTask EquipRelic(int avatarId, int relicUniqueId, int slot)
    {
        var itemData = Data.RelicItems.Find(x => x.UniqueId == relicUniqueId);
        var avatarData = Player.AvatarManager!.GetFormalAvatar(avatarId);
        if (itemData == null || avatarData == null) return;
        avatarData.PathInfos[avatarId].Relic.TryGetValue(slot, out var id);
        var oldItem = Data.RelicItems.Find(x => x.UniqueId == id);

        if (itemData.EquipAvatar > 0) // already be dressed
        {
            var equipAvatarId = itemData.EquipAvatar;
            var equipAvatar = Player.AvatarManager!.GetFormalAvatar(equipAvatarId);
            if (equipAvatar != null && oldItem != null)
            {
                // switch
                equipAvatar.PathInfos[equipAvatarId].Relic[slot] = oldItem.UniqueId;
                oldItem.EquipAvatar = equipAvatar.AvatarId;
                await Player.SendPacket(new PacketPlayerSyncScNotify(equipAvatar, oldItem));
            }
            else if (equipAvatar != null && oldItem == null)
            {
                equipAvatar.PathInfos[equipAvatarId].Relic[slot] = 0;
                await Player.SendPacket(new PacketPlayerSyncScNotify(equipAvatar));
            }
        }
        else
        {
            if (oldItem != null)
            {
                oldItem.EquipAvatar = 0;
                await Player.SendPacket(new PacketPlayerSyncScNotify(oldItem));
            }
        }

        itemData.EquipAvatar = avatarData.AvatarId;
        avatarData.PathInfos[avatarId].Relic[slot] = itemData.UniqueId;
        // save
        await Player.SendPacket(new PacketPlayerSyncScNotify(avatarData, itemData));
    }

    public async ValueTask UnequipRelic(int avatarId, int slot)
    {
        var avatarData = Player.AvatarManager!.GetFormalAvatar(avatarId);
        if (avatarData == null) return;
        var pathInfo = avatarData.PathInfos[avatarId];
        pathInfo.Relic.TryGetValue(slot, out var uniqueId);
        var itemData = Data.RelicItems.Find(x => x.UniqueId == uniqueId);
        if (itemData == null) return;
        pathInfo.Relic.Remove(slot);
        itemData.EquipAvatar = 0;
        await Player.SendPacket(new PacketPlayerSyncScNotify(avatarData, itemData));
    }

    public async ValueTask UnequipEquipment(int avatarId)
    {
        var avatarData = Player.AvatarManager!.GetFormalAvatar(avatarId);
        if (avatarData == null) return;
        var pathInfo = avatarData.PathInfos[avatarId];
        var itemData = Data.EquipmentItems.Find(x => x.UniqueId == pathInfo.EquipId);
        if (itemData == null) return;
        itemData.EquipAvatar = 0;
        pathInfo.EquipId = 0;
        await Player.SendPacket(new PacketPlayerSyncScNotify(avatarData, itemData));
    }

    public async ValueTask<List<ItemData>> LevelUpAvatar(int baseAvatarId, ItemCostData item)
    {
        var avatarData = Player.AvatarManager!.GetFormalAvatar(baseAvatarId);
        if (avatarData == null) return [];
        GameData.AvatarConfigData.TryGetValue(avatarData.AvatarId, out var avatarConfig);
        if (avatarConfig == null) return [];

        GameData.AvatarPromotionConfigData.TryGetValue(avatarData.AvatarId * 10 + avatarData.Promotion,
            out var promotionConfig);
        if (promotionConfig == null) return [];
        var exp = 0;

        foreach (var cost in item.ItemList)
        {
            GameData.ItemConfigData.TryGetValue((int)cost.PileItem.ItemId, out var itemConfig);
            if (itemConfig == null) continue;
            exp += itemConfig.Exp * (int)cost.PileItem.ItemNum;
        }

        // payment
        var costScoin = exp / 10;
        if (Player.Data.Scoin < costScoin) return [];
        foreach (var cost in item.ItemList) await RemoveItem((int)cost.PileItem.ItemId, (int)cost.PileItem.ItemNum);
        await RemoveItem(2, costScoin);

        var maxLevel = promotionConfig.MaxLevel;
        var curExp = avatarData.Exp;
        var curLevel = avatarData.Level;
        var nextLevelExp = GameData.GetAvatarExpRequired(avatarConfig.ExpGroup, avatarData.Level);
        do
        {
            int toGain;
            if (curExp + exp >= nextLevelExp)
                toGain = nextLevelExp - curExp;
            else
                toGain = exp;
            curExp += toGain;
            exp -= toGain;
            // level up
            if (curExp >= nextLevelExp)
            {
                curExp = 0;
                curLevel++;
                nextLevelExp = GameData.GetAvatarExpRequired(avatarConfig.ExpGroup, curLevel);
            }
        } while (exp > 0 && nextLevelExp > 0 && curLevel < maxLevel);

        avatarData.Level = curLevel;
        avatarData.Exp = curExp;
        // leftover
        Dictionary<int, ItemData> list = [];
        var leftover = exp;
        while (leftover > 0)
        {
            var gain = false;
            foreach (var expItem in GameData.EquipmentExpItemConfigData.Values.Reverse())
                if (leftover >= expItem.ExpProvide)
                {
                    // add
                    await PutItem(expItem.ItemID, 1);
                    if (list.TryGetValue(expItem.ItemID, out var i))
                    {
                        i.Count++;
                    }
                    else
                    {
                        i = new ItemData
                        {
                            ItemId = expItem.ItemID,
                            Count = 1
                        };
                        list[expItem.ItemID] = i;
                    }

                    leftover -= expItem.ExpProvide;
                    gain = true;
                    break;
                }

            if (!gain) break; // no more item
        }

        if (list.Count > 0) await Player.SendPacket(new PacketPlayerSyncScNotify(list.Values.ToList()));
        await Player.SendPacket(new PacketPlayerSyncScNotify(avatarData));
        return [.. list.Values];
    }

    #endregion

    #region Levelup

    public async ValueTask<List<ItemData>> LevelUpEquipment(int equipmentUniqueId, ItemCostData item)
    {
        var itemData = Data.EquipmentItems.Find(x => x.UniqueId == equipmentUniqueId);
        if (itemData == null) return [];
        GameData.EquipmentPromotionConfigData.TryGetValue(itemData.ItemId * 10 + itemData.Promotion,
            out var equipmentPromotionConfig);
        GameData.EquipmentConfigData.TryGetValue(itemData.ItemId, out var equipmentConfig);
        if (equipmentConfig == null || equipmentPromotionConfig == null) return [];
        var exp = 0;

        foreach (var cost in item.ItemList)
            if (cost.PileItem == null)
            {
                // TODO : add equipment
                exp += 100;
            }
            else
            {
                GameData.ItemConfigData.TryGetValue((int)cost.PileItem.ItemId, out var itemConfig);
                if (itemConfig == null) continue;
                exp += itemConfig.Exp * (int)cost.PileItem.ItemNum;
            }

        // payment
        var costScoin = exp / 2;
        if (Player.Data.Scoin < costScoin) return [];
        foreach (var cost in item.ItemList)
            if (cost.PileItem == null)
            {
                // TODO : add equipment
                var costItem = Data.EquipmentItems.Find(x => x.UniqueId == cost.EquipmentUniqueId);
                if (costItem == null) continue;
                await RemoveItem(costItem.ItemId, 1, (int)cost.EquipmentUniqueId);
            }
            else
            {
                await RemoveItem((int)cost.PileItem.ItemId, (int)cost.PileItem.ItemNum);
            }

        await RemoveItem(2, costScoin);

        var maxLevel = equipmentPromotionConfig.MaxLevel;
        var curExp = itemData.Exp;
        var curLevel = itemData.Level;
        var nextLevelExp = GameData.GetEquipmentExpRequired(equipmentConfig.ExpType, itemData.Level);
        do
        {
            int toGain;
            if (curExp + exp >= nextLevelExp)
                toGain = nextLevelExp - curExp;
            else
                toGain = exp;
            curExp += toGain;
            exp -= toGain;
            // level up
            if (curExp >= nextLevelExp)
            {
                curExp = 0;
                curLevel++;
                nextLevelExp = GameData.GetEquipmentExpRequired(equipmentConfig.ExpType, curLevel);
            }
        } while (exp > 0 && nextLevelExp > 0 && curLevel < maxLevel);

        itemData.Level = curLevel;
        itemData.Exp = curExp;
        // leftover
        Dictionary<int, ItemData> list = [];
        var leftover = exp;
        while (leftover > 0)
        {
            var gain = false;
            foreach (var expItem in GameData.EquipmentExpItemConfigData.Values.Reverse())
                if (leftover >= expItem.ExpProvide)
                {
                    // add
                    await PutItem(expItem.ItemID, 1);
                    if (list.TryGetValue(expItem.ItemID, out var i))
                    {
                        i.Count++;
                    }
                    else
                    {
                        i = new ItemData
                        {
                            ItemId = expItem.ItemID,
                            Count = 1
                        };
                        list[expItem.ItemID] = i;
                    }

                    leftover -= expItem.ExpProvide;
                    gain = true;
                    break;
                }

            if (!gain) break; // no more item
        }

        if (list.Count > 0) await Player.SendPacket(new PacketPlayerSyncScNotify(list.Values.ToList()));
        await Player.SendPacket(new PacketPlayerSyncScNotify(itemData));
        return [.. list.Values];
    }

    public async ValueTask<bool> PromoteAvatar(int avatarId)
    {
        // Get avatar
        var avatarData = Player.AvatarManager!.GetFormalAvatar(avatarId);
        if (avatarData == null) return false;

        GameData.AvatarConfigData.TryGetValue(avatarId, out var avatarConfig);
        if (avatarConfig == null ||
            avatarData.Promotion >= avatarConfig.MaxPromotion) return false;

        // Get promotion data
        var promotion =
            GameData.AvatarPromotionConfigData.Values.FirstOrDefault(x =>
                x.AvatarID == avatarId && x.Promotion == avatarData.Promotion)!;

        // Sanity check
        if (avatarData.Level < promotion.MaxLevel ||
            Player.Data.Level < promotion.PlayerLevelRequire ||
            Player.Data.WorldLevel < promotion.WorldLevelRequire) return false;

        // Pay items
        foreach (var cost in promotion.PromotionCostList)
            await Player.InventoryManager!.RemoveItem(cost.ItemID, cost.ItemNum);

        // Promote
        avatarData.Promotion += 1;

        // Send packets
        await Player.SendPacket(new PacketPlayerSyncScNotify(avatarData));
        return true;
    }

    public async ValueTask<bool> PromoteEquipment(int equipmentUniqueId)
    {
        var equipmentData =
            Player.InventoryManager!.Data.EquipmentItems.FirstOrDefault(x => x.UniqueId == equipmentUniqueId);
        if (equipmentData == null ||
            equipmentData.Promotion >= GameData.EquipmentConfigData[equipmentData.ItemId].MaxPromotion) return false;

        var promotionConfig = GameData.EquipmentPromotionConfigData.Values
            .FirstOrDefault(x => x.EquipmentID == equipmentData.ItemId && x.Promotion == equipmentData.Promotion);

        if (promotionConfig == null || equipmentData.Level < promotionConfig.MaxLevel ||
            Player.Data.WorldLevel < promotionConfig.WorldLevelRequire) return false;

        foreach (var cost in promotionConfig.PromotionCostList)
            await Player.InventoryManager!.RemoveItem(cost.ItemID, cost.ItemNum);

        equipmentData.Promotion++;
        await Player.SendPacket(new PacketPlayerSyncScNotify(equipmentData));

        return true;
    }

    public async ValueTask<List<ItemData>> LevelUpRelic(int uniqueId, ItemCostData costData)
    {
        var relicItem = Data.RelicItems.Find(x => x.UniqueId == uniqueId);
        if (relicItem == null) return [];

        var exp = 0;
        var money = 0;
        foreach (var cost in costData.ItemList)
            if (cost.PileItem != null)
            {
                GameData.RelicExpItemData.TryGetValue((int)cost.PileItem.ItemId, out var excel);
                if (excel != null)
                {
                    exp += excel.ExpProvide * (int)cost.PileItem.ItemNum;
                    money += excel.CoinCost * (int)cost.PileItem.ItemNum;
                }

                await RemoveItem((int)cost.PileItem.ItemId, (int)cost.PileItem.ItemNum);
            }
            else if (cost.RelicUniqueId != 0)
            {
                var costItem = Data.RelicItems.Find(x => x.UniqueId == cost.RelicUniqueId);
                if (costItem != null)
                {
                    GameData.RelicConfigData.TryGetValue(costItem.ItemId, out var costExcel);
                    if (costExcel == null) continue;

                    if (costItem.Level > 0)
                        foreach (var level in Enumerable.Range(0, costItem.Level))
                        {
                            GameData.RelicExpTypeData.TryGetValue(costExcel.ExpType * 100 + level, out var typeExcel);
                            if (typeExcel != null)
                                exp += typeExcel.Exp;
                        }
                    else
                        exp += costExcel.ExpProvide;

                    exp += costItem.Exp;
                    money += costExcel.CoinCost;

                    await RemoveItem(costItem.ItemId, 1, (int)cost.RelicUniqueId);
                }
            }

        // credit
        await RemoveItem(2, money);

        // level up
        GameData.RelicConfigData.TryGetValue(relicItem.ItemId, out var relicExcel);
        if (relicExcel == null) return [];

        GameData.RelicExpTypeData.TryGetValue(relicExcel.ExpType * 100 + relicItem.Level, out var relicType);
        do
        {
            if (relicType == null) break;
            int toGain;
            if (relicItem.Exp + exp >= relicType.Exp)
                toGain = relicType.Exp - relicItem.Exp;
            else
                toGain = exp;
            relicItem.Exp += toGain;
            exp -= toGain;

            // level up
            if (relicItem.Exp >= relicType.Exp)
            {
                relicItem.Exp = 0;
                relicItem.Level++;
                GameData.RelicExpTypeData.TryGetValue(relicExcel.ExpType * 100 + relicItem.Level, out relicType);
                // relic attribute
                if (relicItem.Level % 3 == 0)
                {
                    if (relicItem.SubAffixes.Count >= 4)
                        relicItem.IncreaseRandomRelicSubAffix();
                    else
                        relicItem.AddRandomRelicSubAffix();
                }
            }
        } while (exp > 0 && relicType?.Exp > 0 && relicItem.Level < relicExcel.MaxLevel);

        // leftover
        Dictionary<int, ItemData> list = [];
        var leftover = exp;
        while (leftover > 0)
        {
            var gain = false;
            foreach (var expItem in GameData.RelicExpItemData.Values.Reverse())
                if (leftover >= expItem.ExpProvide)
                {
                    // add
                    await PutItem(expItem.ItemID, 1);
                    if (list.TryGetValue(expItem.ItemID, out var i))
                    {
                        i.Count++;
                    }
                    else
                    {
                        i = new ItemData
                        {
                            ItemId = expItem.ItemID,
                            Count = 1
                        };
                        list[expItem.ItemID] = i;
                    }

                    leftover -= expItem.ExpProvide;
                    gain = true;
                    break;
                }

            if (!gain) break; // no more item
        }

        if (list.Count > 0) await Player.SendPacket(new PacketPlayerSyncScNotify(list.Values.ToList()));

        // sync
        await Player.SendPacket(new PacketPlayerSyncScNotify(relicItem));

        return [.. list.Values];
    }

    public async ValueTask RankUpAvatar(int avatarId, ItemCostData costData)
    {
        foreach (var cost in costData.ItemList) await RemoveItem((int)cost.PileItem.ItemId, (int)cost.PileItem.ItemNum);
        var baseAvatarId = avatarId;
        GameData.MultiplePathAvatarConfigData.TryGetValue(baseAvatarId, out var avatar);
        if (avatar != null) baseAvatarId = avatar.BaseAvatarID;
        var avatarData = Player.AvatarManager!.GetFormalAvatar(baseAvatarId);
        if (avatarData == null) return;
        avatarData.GetCurPathInfo().Rank++;
        await Player.SendPacket(new PacketPlayerSyncScNotify(avatarData));
    }

    public async ValueTask RankUpEquipment(int equipmentUniqueId, ItemCostData costData)
    {
        var rank = 0;
        foreach (var cost in costData.ItemList)
        {
            var costItem = Data.EquipmentItems.Find(x => x.UniqueId == cost.EquipmentUniqueId);
            if (costItem == null) continue;
            await RemoveItem(costItem.ItemId, 0, (int)cost.EquipmentUniqueId);
            rank++;
        }

        var itemData = Data.EquipmentItems.Find(x => x.UniqueId == equipmentUniqueId);
        if (itemData == null) return;
        itemData.Rank += rank;
        await Player.SendPacket(new PacketPlayerSyncScNotify(itemData));
    }

    #endregion

    #region Mark

    public async ValueTask<bool> LockItems(RepeatedField<uint> ids, bool isLocked,
        ItemMainTypeEnum itemType = ItemMainTypeEnum.Unknown)
    {
        List<ItemData> targetItems;
        switch (itemType)
        {
            case ItemMainTypeEnum.Equipment:
                targetItems = Data.EquipmentItems;
                break;
            case ItemMainTypeEnum.Relic:
                targetItems = Data.RelicItems;
                break;
            case ItemMainTypeEnum.Unknown:
            case ItemMainTypeEnum.Virtual:
            case ItemMainTypeEnum.AvatarCard:
            case ItemMainTypeEnum.Usable:
            case ItemMainTypeEnum.Material:
            case ItemMainTypeEnum.Mission:
            case ItemMainTypeEnum.Display:
            case ItemMainTypeEnum.Pet:
            default:
                return false;
        }

        if (targetItems.Count == 0) return false;
        var idPool = ids.ToList().ConvertAll(x => (int)x).ToFrozenSet();
        var items = new List<ItemData>();
        foreach (var x in targetItems)
        {
            if (x.Discarded || !idPool.Contains(x.UniqueId)) continue;
            x.Locked = isLocked;
            items.Add(x);
        }

        if (items.Count <= 0) return false;
        await Player.SendPacket(new PacketPlayerSyncScNotify(items));
        return true;
    }

    public async ValueTask<bool> DiscardItems(RepeatedField<uint> ids, bool discarded,
        ItemMainTypeEnum itemType = ItemMainTypeEnum.Unknown)
    {
        List<ItemData> targetItems;
        switch (itemType)
        {
            case ItemMainTypeEnum.Equipment:
                targetItems = Data.EquipmentItems;
                break;
            case ItemMainTypeEnum.Relic:
                targetItems = Data.RelicItems;
                break;
            case ItemMainTypeEnum.Unknown:
            case ItemMainTypeEnum.Virtual:
            case ItemMainTypeEnum.AvatarCard:
            case ItemMainTypeEnum.Usable:
            case ItemMainTypeEnum.Material:
            case ItemMainTypeEnum.Mission:
            case ItemMainTypeEnum.Display:
            case ItemMainTypeEnum.Pet:
            default:
                return false;
        }

        if (targetItems.Count == 0) return false;
        var idPool = ids.ToList().ConvertAll(x => (int)x).ToFrozenSet();
        var items = new List<ItemData>();
        foreach (var x in targetItems)
        {
            if (x.Locked || !idPool.Contains(x.UniqueId)) continue;
            x.Discarded = discarded;
            items.Add(x);
        }

        if (items.Count <= 0) return false;
        await Player.SendPacket(new PacketPlayerSyncScNotify(items));
        return true;
    }

    #endregion
}