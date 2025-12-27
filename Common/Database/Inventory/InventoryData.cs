using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Enums.Item;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;
using SqlSugar;

namespace EggLink.DanhengServer.Database.Inventory;

[SugarTable("InventoryData")]
public class InventoryData : BaseDatabaseDataHelper
{
    [SugarColumn(IsJson = true)] public List<ItemData> MaterialItems { get; set; } = [];

    [SugarColumn(IsJson = true)] public List<ItemData> EquipmentItems { get; set; } = [];

    [SugarColumn(IsJson = true)] public List<ItemData> RelicItems { get; set; } = [];

    [SugarColumn(IsJson = true)] public Dictionary<int, RelicPlanData> RelicPlans { get; set; } = [];

    public int NextUniqueId { get; set; } = 100;
}

public class ItemData
{
    public int UniqueId { get; set; }
    public int ItemId { get; set; }
    public int Count { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public int TotalExp { get; set; }
    public int Promotion { get; set; }
    public int Rank { get; set; } // Superimpose
    public bool Locked { get; set; }
    public bool Discarded { get; set; }

    public int MainAffix { get; set; }
    public List<ItemSubAffix> SubAffixes { get; set; } = [];
    public List<ItemSubAffix> ReforgeSubAffixes { get; set; } = [];

    public int EquipAvatar { get; set; }

    public int CalcTotalExpGained()
    {
        if (Level <= 0) return Exp;
        GameData.RelicConfigData.TryGetValue(ItemId, out var costExcel);
        if (costExcel == null) return 0;
        var exp = 0;
        for (var i = 0; i < Level; i++)
        {
            GameData.RelicExpTypeData.TryGetValue(costExcel.ExpType * 100 + i, out var typeExcel);
            if (typeExcel != null)
                exp += typeExcel.Exp;
        }

        return exp + Exp;
    }

    #region Action

    public void AddRandomRelicMainAffix()
    {
        GameData.RelicConfigData.TryGetValue(ItemId, out var config);
        if (config == null) return;
        GameData.RelicMainAffixData.TryGetValue(config.MainAffixGroup, out var affixes);
        if (affixes == null) return;
        List<int> affixList = [];
        affixList.AddRange(from affix in affixes.Values select affix.AffixID);
        MainAffix = affixList.RandomElement();
    }

    public void AddRelicSubAffix(List<(int, int)> subAffixes)
    {
        GameData.RelicConfigData.TryGetValue(ItemId, out var config);
        if (config == null) return;

        var subAffixConfig = GameData.RelicSubAffixData[config.SubAffixGroup];

        foreach (var (subId, subCnt) in subAffixes)
        {
            if (!subAffixConfig.TryGetValue(subId, out var excel)) continue;
            SubAffixes.Add(new ItemSubAffix(excel, subCnt));
        }
    }

    public void AddRandomRelicSubAffix(int count = 1)
    {
        if (count <= 0 || MainAffix == 0) return;
        GameData.RelicConfigData.TryGetValue(ItemId, out var config);
        if (config == null) return;

        var mainAffixConfig = GameData.RelicMainAffixData[config.MainAffixGroup];
        var mainProperty = mainAffixConfig[MainAffix].Property;
        var subAffixConfig = GameData.RelicSubAffixData[config.SubAffixGroup];
        var subAffixKeys = subAffixConfig.Keys.ToList();

        while (count > 0)
        {
            var subId = subAffixKeys.RandomElement();
            if (SubAffixes.Any(x => x.Id == subId)) continue;
            if (subAffixConfig[subId] != null &&
                subAffixConfig[subId].Property == mainProperty) continue;

            SubAffixes.Add(new ItemSubAffix(subAffixConfig[subId], 1));
            count--;
        }
    }

    public void IncreaseRandomRelicSubAffix(int times = 1)
    {
        if (times <= 0) return;
        GameData.RelicConfigData.TryGetValue(ItemId, out var config);
        if (config == null) return;
        GameData.RelicSubAffixData.TryGetValue(config.SubAffixGroup, out var affixes);
        if (affixes == null) return;

        for (var i = 0; i < times; i++)
        {
            var element = SubAffixes.RandomElement();
            var affix = affixes.Values.ToList().Find(x => x.AffixID == element.Id);
            if (affix == null) return;
            element.IncreaseStep(affix.StepNum);
        }
    }

    /**
     * Init relic sub affixes based on rarity
     * 20% chance to get one more affix
     * r3 1-2
     * r4 2-3
     * r5 3-4
     */
    public void InitRandomRelicSubAffixesByRarity(ItemRarityEnum rarity = ItemRarityEnum.Unknown)
    {
        if (rarity == ItemRarityEnum.Unknown)
        {
            GameData.ItemConfigData.TryGetValue(ItemId, out var config);
            if (config == null) return;
            rarity = config.Rarity;
        }

        int initSubAffixesCount;
        switch (rarity)
        {
            case ItemRarityEnum.Rare:
                initSubAffixesCount = 1 + LuckyRelicSubAffixCount();
                break;
            case ItemRarityEnum.VeryRare:
                initSubAffixesCount = 2 + LuckyRelicSubAffixCount();
                break;
            case ItemRarityEnum.SuperRare:
                initSubAffixesCount = 3 + LuckyRelicSubAffixCount();
                break;
            default:
                return;
        }

        AddRandomRelicSubAffix(initSubAffixesCount);
    }

    public int LuckyRelicSubAffixCount()
    {
        return Random.Shared.Next(100) < 20 ? 1 : 0;
    }

    #endregion

    #region Serialization

    public Material ToMaterialProto()
    {
        return new Material
        {
            Tid = (uint)ItemId,
            Num = (uint)Count
        };
    }

    public Relic ToRelicProto()
    {
        Relic relic = new()
        {
            Tid = (uint)ItemId,
            UniqueId = (uint)UniqueId,
            Level = (uint)Level,
            IsProtected = Locked,
            Exp = (uint)Exp,
            IsDiscarded = Discarded,
            DressAvatarId = (uint)EquipAvatar,
            MainAffixId = (uint)MainAffix
        };
        if (SubAffixes.Count > 0)
            foreach (var subAffix in SubAffixes)
                relic.SubAffixList.Add(subAffix.ToProto());
        if (ReforgeSubAffixes.Count > 0)
            foreach (var subAffix in ReforgeSubAffixes)
                relic.ReforgeSubAffixList.Add(subAffix.ToProto());
        return relic;
    }

    public Equipment ToEquipmentProto()
    {
        return new Equipment
        {
            Tid = (uint)ItemId,
            UniqueId = (uint)UniqueId,
            Level = (uint)Level,
            Exp = (uint)Exp,
            IsProtected = Locked,
            Promotion = (uint)Promotion,
            Rank = (uint)Rank,
            DressAvatarId = (uint)EquipAvatar
        };
    }

    public ChallengeBossEquipmentInfo ToChallengeEquipmentProto()
    {
        return new ChallengeBossEquipmentInfo
        {
            Tid = (uint)ItemId,
            UniqueId = (uint)UniqueId,
            Level = (uint)Level,
            Promotion = (uint)Promotion,
            Rank = (uint)Rank
        };
    }

    public ChallengeBossRelicInfo ToChallengeRelicProto()
    {
        var proto = new ChallengeBossRelicInfo
        {
            Tid = (uint)ItemId,
            UniqueId = (uint)UniqueId,
            Level = (uint)Level,
            MainAffixId = (uint)MainAffix
        };

        if (SubAffixes.Count < 1) return proto;
        foreach (var subAffix in SubAffixes)
            proto.SubAffixList.Add(subAffix.ToProto());

        return proto;
    }

    public Item ToProto()
    {
        return new Item
        {
            ItemId = (uint)ItemId,
            Num = (uint)Count,
            Level = (uint)Level,
            MainAffixId = (uint)MainAffix,
            Rank = (uint)Rank,
            Promotion = (uint)Promotion,
            UniqueId = (uint)UniqueId
        };
    }

    public PileItem ToPileProto()
    {
        return new PileItem
        {
            ItemId = (uint)ItemId,
            ItemNum = (uint)Count
        };
    }

    public DisplayEquipmentInfo ToDisplayEquipmentProto()
    {
        return new DisplayEquipmentInfo
        {
            Tid = (uint)ItemId,
            Level = (uint)Level,
            Exp = (uint)Exp,
            Promotion = (uint)Promotion,
            Rank = (uint)Rank
        };
    }

    public DisplayRelicInfo ToDisplayRelicProto()
    {
        DisplayRelicInfo relic = new()
        {
            Tid = (uint)ItemId,
            Level = (uint)Level,
            Type = (uint)GameData.RelicConfigData[ItemId].Type,
            MainAffixId = (uint)MainAffix
        };

        if (SubAffixes.Count >= 1)
            foreach (var subAffix in SubAffixes)
                relic.SubAffixList.Add(subAffix.ToProto());

        return relic;
    }

    public ItemData Clone()
    {
        return new ItemData
        {
            UniqueId = UniqueId,
            ItemId = ItemId,
            Count = Count,
            Level = Level,
            Exp = Exp,
            TotalExp = TotalExp,
            Promotion = Promotion,
            Rank = Rank,
            Locked = Locked,
            Discarded = Discarded,
            MainAffix = MainAffix,
            SubAffixes = [.. SubAffixes.Select(x => x.Clone())],
            ReforgeSubAffixes = [.. ReforgeSubAffixes.Select(x => x.Clone())],
            EquipAvatar = EquipAvatar
        };
    }

    #endregion
}

public class ItemSubAffix
{
    public ItemSubAffix()
    {
    }

    public ItemSubAffix(RelicSubAffixConfigExcel excel, int count)
    {
        Id = excel.AffixID;
        Count = count;
        Step = Extensions.RandomInt(0, excel.StepNum * count + 1);
    }

    public int Id { get; set; }
    public int Count { get; set; }
    public int Step { get; set; }

    public void IncreaseStep(int stepNum)
    {
        Count++;
        Step += Extensions.RandomInt(0, stepNum + 1);
    }

    public RelicAffix ToProto()
    {
        return new RelicAffix
        {
            AffixId = (uint)Id,
            Cnt = (uint)Count,
            Step = (uint)Step
        };
    }

    public ItemSubAffix Clone()
    {
        return new ItemSubAffix
        {
            Id = Id,
            Count = Count,
            Step = Step
        };
    }
}

public class RelicPlanData
{
    public int EquipAvatar { get; set; }
    public List<int> InsideRelic { get; set; } = [];
    public List<int> OutsideRelic { get; set; } = [];
}