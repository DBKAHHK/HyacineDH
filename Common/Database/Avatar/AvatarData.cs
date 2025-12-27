using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Database.Player;
using EggLink.DanhengServer.Enums.Avatar;
using EggLink.DanhengServer.Proto;
using SqlSugar;
using LineupInfo = EggLink.DanhengServer.Database.Lineup.LineupInfo;

namespace EggLink.DanhengServer.Database.Avatar;

[SugarTable("Avatar")]
public class AvatarData : BaseDatabaseDataHelper
{
    [SugarColumn(IsJson = true)] public List<OldAvatarInfo> Avatars { get; set; } = [];
    [SugarColumn(IsJson = true)] public List<FormalAvatarInfo> FormalAvatars { get; set; } = [];
    [SugarColumn(IsJson = true)] public List<SpecialAvatarInfo> TrialAvatars { get; set; } = [];

    [SugarColumn(IsJson = true)] public List<int> AssistAvatars { get; set; } = [];

    [SugarColumn(IsJson = true)] public List<int> DisplayAvatars { get; set; } = [];

    public string DatabaseVersion { get; set; } = "0";
}

public abstract class BaseAvatarInfo
{
    public int BaseAvatarId { get; set; }
    public int AvatarId { get; set; } // special avatar id / base avatar id
    public int Promotion { get; set; }
    public int Level { get; set; }
    public int CurrentHp { get; set; } = 10000;
    public int CurrentSp { get; set; }
    public int ExtraLineupHp { get; set; } = 10000;
    public int ExtraLineupSp { get; set; }
    public Dictionary<int, PathInfo> PathInfos { get; set; } = [];

    public void SetCurHp(int value, bool isExtraLineup)
    {
        if (isExtraLineup)
            ExtraLineupHp = value;
        else
            CurrentHp = value;
    }

    public void SetCurSp(int value, bool isExtraLineup)
    {
        if (isExtraLineup)
            ExtraLineupSp = value;
        else
            CurrentSp = value;
    }

    public int GetCurHp(bool isExtraLineup)
    {
        return isExtraLineup ? ExtraLineupHp : CurrentHp;
    }

    public int GetCurSp(bool isExtraLineup)
    {
        return isExtraLineup ? ExtraLineupSp : CurrentSp;
    }

    public PathInfo GetCurPathInfo()
    {
        if (PathInfos.TryGetValue(AvatarId, out var info)) return info;

        PathInfos.Add(AvatarId, new PathInfo(AvatarId));
        return PathInfos[AvatarId];
    }

    public PathInfo? GetPathInfo(int pathId)
    {
        if (PathInfos.TryGetValue(pathId, out var value)) return value;
        return null;
    }

    public abstract BattleAvatar ToBattleProto(PlayerDataCollection collection,
        AvatarType avatarType = AvatarType.AvatarFormalType);

    public abstract LineupAvatar ToLineupInfo(int slot, LineupInfo info,
        AvatarType avatarType = AvatarType.AvatarFormalType);

    public abstract Proto.Avatar ToProto();
}

public class FormalAvatarInfo : BaseAvatarInfo
{
    public FormalAvatarInfo()
    {
        // only for db
    }

    public FormalAvatarInfo(int baseAvatarId, int avatarId, bool addSkills)
    {
        // TODO add skills
        BaseAvatarId = baseAvatarId;
        AvatarId = avatarId;
        if (addSkills) CheckPathSkillTree();
    }

    public int Exp { get; set; }
    public int Rewards { get; set; }
    public long Timestamp { get; set; }
    public bool IsMarked { get; set; } = false;

    public bool HasTakenReward(int promotion)
    {
        return (Rewards & (1 << promotion)) != 0;
    }

    public void ValidateHero(Gender gender)
    {
        foreach (var pathInfo in PathInfos.ToArray())
        {
            if (!GameData.MultiplePathAvatarConfigData.TryGetValue(pathInfo.Key, out var path)) continue;
            if (path.Gender == GenderTypeEnum.GENDER_NONE) continue;
            if (path.Gender == (GenderTypeEnum)gender) continue;
            PathInfos.Remove(pathInfo.Key);
        }
    }

    public void CheckPathSkillTree()
    {
        if (!GameData.AvatarConfigData.TryGetValue(AvatarId, out var excel)) return;
        if (PathInfos.ContainsKey(AvatarId)) return;
        if (excel.DefaultSkillTree[0].Count == 0) return;

        // create path info
        var path = new PathInfo(AvatarId);
        path.GetSkillTree();

        PathInfos.Add(AvatarId, path);
    }

    public void TakeReward(int promotion)
    {
        Rewards |= 1 << promotion;
    }

    public override Proto.Avatar ToProto()
    {
        var proto = new Proto.Avatar
        {
            BaseAvatarId = (uint)BaseAvatarId,
            Level = (uint)Level,
            Exp = (uint)Exp,
            Promotion = (uint)Promotion,
            Rank = (uint)GetCurPathInfo().Rank,
            FirstMetTimeStamp = (ulong)Timestamp,
            IsMarked = IsMarked,
            DressedSkinId = (uint)GetCurPathInfo().Skin,
            UnkEnhancedId = (uint)GetCurPathInfo().EnhanceId
        };

        foreach (var item in GetCurPathInfo().Relic)
            proto.EquipRelicList.Add(new EquipRelic
            {
                RelicUniqueId = (uint)item.Value,
                Type = (uint)item.Key
            });

        if (GetCurPathInfo().EquipId != 0) proto.EquipmentUniqueId = (uint)GetCurPathInfo().EquipId;

        foreach (var skill in GetCurPathInfo().GetSkillTree())
            proto.SkilltreeList.Add(new AvatarSkillTree
            {
                PointId = (uint)skill.Key,
                Level = (uint)skill.Value
            });

        for (var i = 0; i < Promotion; i++)
            if (HasTakenReward(i))
                proto.HasTakenPromotionRewardList.Add((uint)i);

        return proto;
    }

    public override LineupAvatar ToLineupInfo(int slot, LineupInfo info,
        AvatarType avatarType = AvatarType.AvatarFormalType)
    {
        return new LineupAvatar
        {
            Id = (uint)BaseAvatarId,
            Slot = (uint)slot,
            AvatarType = avatarType,
            Hp = info.IsExtraLineup() ? (uint)ExtraLineupHp : (uint)CurrentHp,
            SpBar = new SpBarInfo
            {
                CurSp = info.IsExtraLineup() ? (uint)ExtraLineupSp : (uint)CurrentSp,
                MaxSp = 10000
            }
        };
    }

    public override BattleAvatar ToBattleProto(PlayerDataCollection collection,
        AvatarType avatarType = AvatarType.AvatarFormalType)
    {
        var proto = new BattleAvatar
        {
            Id = (uint)AvatarId,
            AvatarType = avatarType,
            Level = (uint)Level,
            Promotion = (uint)Promotion,
            Rank = (uint)GetCurPathInfo().Rank,
            Index = (uint)collection.LineupInfo.GetSlot(BaseAvatarId),
            Hp = (uint)GetCurHp(collection.LineupInfo.LineupType != 0),
            SpBar = new SpBarInfo
            {
                CurSp = (uint)GetCurSp(collection.LineupInfo.LineupType != 0),
                MaxSp = 10000
            },
            WorldLevel = (uint)collection.PlayerData.WorldLevel,
            EnhancedId = (uint)GetCurPathInfo().EnhanceId
        };

        foreach (var skill in GetCurPathInfo().GetSkillTree())
            proto.SkilltreeList.Add(new AvatarSkillTree
            {
                PointId = (uint)skill.Key,
                Level = (uint)skill.Value
            });

        foreach (var relic in GetCurPathInfo().Relic)
        {
            var item = collection.InventoryData.RelicItems?.Find(item => item.UniqueId == relic.Value);
            if (item != null)
            {
                var protoRelic = new BattleRelic
                {
                    Id = (uint)item.ItemId,
                    UniqueId = (uint)item.UniqueId,
                    Level = (uint)item.Level,
                    MainAffixId = (uint)item.MainAffix
                };

                if (item.SubAffixes.Count >= 1)
                    foreach (var subAffix in item.SubAffixes)
                        protoRelic.SubAffixList.Add(subAffix.ToProto());

                proto.RelicList.Add(protoRelic);
            }
        }

        if (GetCurPathInfo().EquipId != 0)
        {
            var item = collection.InventoryData.EquipmentItems.Find(item => item.UniqueId == GetCurPathInfo().EquipId);
            if (item != null)
                proto.EquipmentList.Add(new BattleEquipment
                {
                    Id = (uint)item.ItemId,
                    Level = (uint)item.Level,
                    Promotion = (uint)item.Promotion,
                    Rank = (uint)item.Rank
                });
        }
        else if (GetCurPathInfo().EquipData != null)
        {
            proto.EquipmentList.Add(new BattleEquipment
            {
                Id = (uint)GetCurPathInfo().EquipData!.ItemId,
                Level = (uint)GetCurPathInfo().EquipData!.Level,
                Promotion = (uint)GetCurPathInfo().EquipData!.Promotion,
                Rank = (uint)GetCurPathInfo().EquipData!.Rank
            });
        }

        return proto;
    }

    public List<MultiPathAvatarInfo> ToAvatarPathProto()
    {
        var res = new List<MultiPathAvatarInfo>();

        foreach (var pathInfo in PathInfos.Values)
        {
            var proto = new MultiPathAvatarInfo
            {
                AvatarId = (MultiPathAvatarType)pathInfo.PathId,
                Rank = (uint)pathInfo.Rank,
                PathEquipmentId = (uint)pathInfo.EquipId,
                DressedSkinId = (uint)pathInfo.Skin,
                UnkEnhancedId = (uint)GetCurPathInfo().EnhanceId
            };

            foreach (var skill in pathInfo.GetSkillTree())
                proto.MultiPathSkillTree.Add(new AvatarSkillTree
                {
                    PointId = (uint)skill.Key,
                    Level = (uint)skill.Value
                });

            foreach (var relic in pathInfo.Relic)
                proto.EquipRelicList.Add(new EquipRelic
                {
                    Type = (uint)relic.Key,
                    RelicUniqueId = (uint)relic.Value
                });

            res.Add(proto);
        }

        return res;
    }

    public DisplayAvatarDetailInfo ToDetailProto(int pos, PlayerDataCollection collection)
    {
        var proto = new DisplayAvatarDetailInfo
        {
            AvatarId = (uint)AvatarId,
            Level = (uint)Level,
            Exp = (uint)Exp,
            Promotion = (uint)Promotion,
            Rank = (uint)GetCurPathInfo().Rank,
            Pos = (uint)pos,
            DressedSkinId = (uint)GetCurPathInfo().Skin
        };

        var inventory = collection.InventoryData;
        foreach (var item in GetCurPathInfo().Relic)
        {
            var relic = inventory.RelicItems.Find(x => x.UniqueId == item.Value)!;
            proto.RelicList.Add(relic.ToDisplayRelicProto());
        }

        if (GetCurPathInfo().EquipId != 0)
        {
            var equip = inventory.EquipmentItems.Find(x => x.UniqueId == GetCurPathInfo().EquipId)!;
            proto.Equipment = equip.ToDisplayEquipmentProto();
        }

        foreach (var skill in GetCurPathInfo().GetSkillTree())
            proto.SkilltreeList.Add(new AvatarSkillTree
            {
                PointId = (uint)skill.Key,
                Level = (uint)skill.Value
            });

        return proto;
    }
}

public class SpecialAvatarInfo : BaseAvatarInfo
{
    public int SpecialAvatarId { get; set; }


    public void CheckLevel(int worldLevel)
    {
        if (!GameData.SpecialAvatarData.TryGetValue(AvatarId * 10 + worldLevel, out var specialAvatar))
            if (!GameData.SpecialAvatarData.TryGetValue(AvatarId * 10 + 1, out specialAvatar))
                return;

        Level = specialAvatar.Level;
        Promotion = specialAvatar.Promotion;
        GetCurPathInfo().Rank = specialAvatar.Rank;
        GetCurPathInfo().EquipData = new ItemData
        {
            ItemId = specialAvatar.EquipmentID,
            Level = specialAvatar.EquipmentLevel,
            Promotion = specialAvatar.EquipmentPromotion,
            Rank = specialAvatar.EquipmentRank
        };
    }

    public override Proto.Avatar ToProto()
    {
        var proto = new Proto.Avatar
        {
            BaseAvatarId = (uint)BaseAvatarId,
            Level = (uint)Level,
            Promotion = (uint)Promotion,
            Rank = (uint)GetCurPathInfo().Rank,
            DressedSkinId = (uint)GetCurPathInfo().Skin
        };

        foreach (var item in GetCurPathInfo().Relic)
            proto.EquipRelicList.Add(new EquipRelic
            {
                RelicUniqueId = (uint)item.Value,
                Type = (uint)item.Key
            });

        if (GetCurPathInfo().EquipId != 0) proto.EquipmentUniqueId = (uint)GetCurPathInfo().EquipId;

        foreach (var skill in GetCurPathInfo().GetSkillTree())
            proto.SkilltreeList.Add(new AvatarSkillTree
            {
                PointId = (uint)skill.Key,
                Level = (uint)skill.Value
            });

        return proto;
    }

    public override LineupAvatar ToLineupInfo(int slot, LineupInfo info,
        AvatarType avatarType = AvatarType.AvatarFormalType)
    {
        return new LineupAvatar
        {
            Id = (uint)SpecialAvatarId,
            Slot = (uint)slot,
            AvatarType = avatarType,
            Hp = info.IsExtraLineup() ? (uint)ExtraLineupHp : (uint)CurrentHp,
            SpBar = new SpBarInfo
            {
                CurSp = info.IsExtraLineup() ? (uint)ExtraLineupSp : (uint)CurrentSp,
                MaxSp = 10000
            }
        };
    }

    public override BattleAvatar ToBattleProto(PlayerDataCollection collection,
        AvatarType avatarType = AvatarType.AvatarFormalType)
    {
        var proto = new BattleAvatar
        {
            Id = (uint)SpecialAvatarId,
            AvatarType = avatarType,
            Level = (uint)Level,
            Promotion = (uint)Promotion,
            Rank = (uint)GetCurPathInfo().Rank,
            Index = (uint)collection.LineupInfo.GetSlot(BaseAvatarId),
            Hp = (uint)GetCurHp(collection.LineupInfo.LineupType != 0),
            SpBar = new SpBarInfo
            {
                CurSp = (uint)GetCurSp(collection.LineupInfo.LineupType != 0),
                MaxSp = 10000
            },
            WorldLevel = (uint)collection.PlayerData.WorldLevel
        };

        foreach (var skill in GetCurPathInfo().GetSkillTree())
            proto.SkilltreeList.Add(new AvatarSkillTree
            {
                PointId = (uint)skill.Key,
                Level = (uint)skill.Value
            });

        foreach (var relic in GetCurPathInfo().Relic)
        {
            var item = collection.InventoryData.RelicItems?.Find(item => item.UniqueId == relic.Value);
            if (item != null)
            {
                var protoRelic = new BattleRelic
                {
                    Id = (uint)item.ItemId,
                    UniqueId = (uint)item.UniqueId,
                    Level = (uint)item.Level,
                    MainAffixId = (uint)item.MainAffix
                };

                if (item.SubAffixes.Count >= 1)
                    foreach (var subAffix in item.SubAffixes)
                        protoRelic.SubAffixList.Add(subAffix.ToProto());

                proto.RelicList.Add(protoRelic);
            }
        }

        if (GetCurPathInfo().EquipId != 0)
        {
            var item = collection.InventoryData.EquipmentItems.Find(item => item.UniqueId == GetCurPathInfo().EquipId);
            if (item != null)
                proto.EquipmentList.Add(new BattleEquipment
                {
                    Id = (uint)item.ItemId,
                    Level = (uint)item.Level,
                    Promotion = (uint)item.Promotion,
                    Rank = (uint)item.Rank
                });
        }
        else if (GetCurPathInfo().EquipData != null)
        {
            proto.EquipmentList.Add(new BattleEquipment
            {
                Id = (uint)GetCurPathInfo().EquipData!.ItemId,
                Level = (uint)GetCurPathInfo().EquipData!.Level,
                Promotion = (uint)GetCurPathInfo().EquipData!.Promotion,
                Rank = (uint)GetCurPathInfo().EquipData!.Rank
            });
        }

        return proto;
    }
}

public class OldAvatarInfo
{
    public int AvatarId { get; set; }

    public int PathId { get; set; }
    public int Level { get; set; }
    public int Exp { get; set; }
    public int Promotion { get; set; }
    public int Rewards { get; set; }
    public long Timestamp { get; set; }
    public int CurrentHp { get; set; } = 10000;
    public int CurrentSp { get; set; }
    public int ExtraLineupHp { get; set; } = 10000;
    public int ExtraLineupSp { get; set; }
    public bool IsMarked { get; set; } = false;
    public Dictionary<int, int> SkillTree { get; set; } = [];

    public Dictionary<int, Dictionary<int, int>> SkillTreeExtra { get; set; } =
        []; // for hero  heroId -> skillId -> level

    public Dictionary<int, PathInfo> PathInfoes { get; set; } = [];
}

public class PathInfo(int pathId)
{
    public int PathId { get; set; } = pathId;
    public int Skin { get; set; }
    public int Rank { get; set; }
    public int EquipId { get; set; } = 0;
    public Dictionary<int, int> Relic { get; set; } = [];
    public ItemData? EquipData { get; set; } // for special avatar
    public int EnhanceId { get; set; }
    public Dictionary<int, EnhanceInfo> EnhanceInfos { get; set; } = [];

    public Dictionary<int, int> GetSkillTree()
    {
        if (EnhanceInfos.TryGetValue(EnhanceId, out var enhance))
        {
            return enhance.SkillTree;
        }

        EnhanceInfos[EnhanceId] = new EnhanceInfo(EnhanceId);
        
        // create default skill tree
        var avatarExcel = GameData.AvatarConfigData.GetValueOrDefault(PathId);
        if (avatarExcel == null) return [];

        var skills = avatarExcel.DefaultSkillTree.GetValueOrDefault(EnhanceId);
        if (skills == null) return [];

        foreach (var skill in skills)
        {
            EnhanceInfos[EnhanceId].SkillTree.Add(skill.PointID, skill.Level);
        }

        return EnhanceInfos[EnhanceId].SkillTree;
    }
}

public class EnhanceInfo(int enhanceId)
{
    public int EnhanceId { get; set; } = enhanceId;
    public Dictionary<int, int> SkillTree { get; set; } = [];
}

public record PlayerDataCollection(PlayerData PlayerData, InventoryData InventoryData, LineupInfo LineupInfo);