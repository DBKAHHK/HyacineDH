using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Enums.TournRogue;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Game.Rogue;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Lineup;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.RogueTourn;

public class RogueTournManager(PlayerInstance player) : BasePlayerManager(player)
{
    public RogueTournInstance? RogueTournInstance { get; set; }

    public async ValueTask<(Retcode, RogueTournInstance?)> StartRogueTourn(List<int> avatars, int area)
    {
        RogueTournInstance = null;
        var areaExcel = GameData.RogueTournAreaData.GetValueOrDefault(area);

        if (areaExcel == null)
            return (Retcode.RetRogueAreaInvalid, null);

        var baseAvatarIds = new List<int>();
        foreach (var avatar in avatars.Select(id => Player.AvatarManager!.GetFormalAvatar(id)))
        {
            if (avatar == null)
                return (Retcode.RetAvatarNotExist, null);

            avatar.SetCurHp(10000, true);
            avatar.SetCurSp(5000, true);
            baseAvatarIds.Add(avatar.BaseAvatarId);
        }

        Player.LineupManager!.SetExtraLineup(ExtraLineupType.LineupTournRogue, baseAvatarIds);
        await Player.LineupManager!.GainMp(5, false);
        await Player.SendPacket(new PacketSyncLineupNotify(Player.LineupManager!.GetCurLineup()!));

        var instance = new RogueTournInstance(Player, area);
        RogueTournInstance = instance;
        await instance.EnterRoom(1, RogueTournRoomTypeEnum.Battle);
        return (Retcode.RetSucc, instance);
    }

    #region Serialization

    public RogueTournInfo ToProto()
    {
        var proto = new RogueTournInfo
        {
            ExtraScoreInfo = ToExtraScoreProto(),
            PermanentInfo = ToPermanentTalentProto(),
            RogueSeasonInfo = ToSeasonProto(),
            RogueTournAreaInfo = { ToAreaProtoList() },
            RogueTournDifficultyInfo = { ToDifficultyProtoList() },
            RogueTournExpInfo = ToExpProto(),
            RogueTournHandbook = ToHandbookProto(),
            RogueTournSaveList =
            {
                Capacity = 0
            },
            SeasonTalentInfo = ToSeasonTalentProto(),
            LKCEFCLJCBM = new KCLCHJMNPGL()
        };

        return proto;
    }

    public RogueTournSeasonTalent ToSeasonTalentProto()
    {
        return new RogueTournSeasonTalent
        {
            TalentInfoList = new RogueTalentInfoList
            {
                TalentInfo =
                {
                    GameData.RogueTournTitanTalentData.Values.Select(x => new RogueTalentInfo
                    {
                        TalentId = (uint)x.ID,
                        Status = RogueTalentStatus.Enable
                    })
                }
            }
        };
    }

    public ExtraScoreInfo ToExtraScoreProto()
    {
        return new ExtraScoreInfo
        {
            EndTime = RogueManager.GetCurrentRogueTime().Item2,
            Week = 1
        };
    }

    public RogueTournPermanentTalentInfo ToPermanentTalentProto()
    {
        return new RogueTournPermanentTalentInfo
        {
            TalentInfoList = new RogueTalentInfoList
            {
                TalentInfo =
                {
                    GameData.RogueTournPermanentTalentData.Values.Select(x => new RogueTalentInfo
                    {
                        TalentId = (uint)x.TalentID,
                        Status = RogueTalentStatus.Enable
                    })
                }
            }
        };
    }

    public RogueTournSeasonInfo ToSeasonProto()
    {
        return new RogueTournSeasonInfo
        {
            SubTournId = GameConstants.CURRENT_ROGUE_TOURN_SEASON,
            MainTournId = 1
        };
    }

    public List<RogueTournAreaInfo> ToAreaProtoList()
    {
        return (from areaExcel in GameData.RogueTournAreaData
                where areaExcel.Value.AreaGroupID != RogueTournAreaGroupIDEnum.WeekChallenge &&
                      areaExcel.Value.TournMode != RogueTournModeEnum.Tourn1
                select new RogueTournAreaInfo
                {
                    AreaId = (uint)areaExcel.Value.AreaID, Completed = true, IsTakenReward = true, IsUnlocked = true
                })
            .ToList();
    }

    public List<RogueTournDifficultyInfo> ToDifficultyProtoList()
    {
        return [];
    }

    public RogueTournExpInfo ToExpProto()
    {
        return new RogueTournExpInfo
        {
            Exp = 0,
            TakenLevelRewards =
            {
                Capacity = 0
            }
        };
    }

    public RogueTournHandbookInfo ToHandbookProto()
    {
        var proto = new RogueTournHandbookInfo
        {
            RogueTournHandbookSeasonId = GameConstants.CURRENT_ROGUE_TOURN_SEASON
        };

        foreach (var hexAvatar in GameData.RogueTournHexAvatarBaseTypeData.Keys)
            proto.HandbookHexAvatarList.Add((uint)hexAvatar);

        foreach (var buff in GameData.RogueBuffData.Values)
            if (buff is RogueTournBuffExcel { IsInHandbook: true })
                proto.HandbookBuffList.Add((uint)buff.MazeBuffID);

        foreach (var formulaId in GameData.RogueTournFormulaData.Keys) proto.HandbookFormulaList.Add((uint)formulaId);

        foreach (var miracleId in GameData.RogueTournHandbookMiracleData.Keys)
            proto.HandbookMiracleList.Add((uint)miracleId);

        foreach (var blessId in GameData.RogueTournTitanBlessData.Keys) proto.HandbookTitanBlessList.Add((uint)blessId);

        //foreach (var eventId in GameData.RogueTournHandBookEventData.Keys) proto.HandbookEventList.Add((uint)eventId);

        return proto;
    }

    #endregion
}