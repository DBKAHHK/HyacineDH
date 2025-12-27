using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.Database.Avatar;
using EggLink.DanhengServer.Database.Lineup;
using EggLink.DanhengServer.Enums.Avatar;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Lineup;
using EggLink.DanhengServer.GameServer.Server.Packet.Send.Scene;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;
using LineupInfo = EggLink.DanhengServer.Database.Lineup.LineupInfo;
using static EggLink.DanhengServer.GameServer.Plugin.Event.PluginEvent;

namespace EggLink.DanhengServer.GameServer.Game.Lineup;

public class LineupManager : BasePlayerManager
{
    public LineupManager(PlayerInstance player) : base(player)
    {
        LineupData = DatabaseHelper.Instance!.GetInstanceOrCreateNew<LineupData>(player.Uid);
        foreach (var lineupInfo in LineupData.Lineups.Values)
        {
            lineupInfo.LineupData = LineupData;
            lineupInfo.AvatarData = player.AvatarManager!.AvatarData;
        }
    }

    public LineupData LineupData { get; }

    #region Detail

    public LineupInfo? GetLineup(int lineupIndex)
    {
        LineupData.Lineups.TryGetValue(lineupIndex, out var lineup);
        return lineup;
    }

    public LineupInfo? GetExtraLineup(ExtraLineupType type)
    {
        var index = (int)type + 10;
        LineupData.Lineups.TryGetValue(index, out var lineup);
        return lineup;
    }

    public LineupInfo? GetCurLineup()
    {
        var lineup = GetLineup(LineupData.GetCurLineupIndex());
        return lineup;
    }

    public int GetMaxMp()
    {
        return 5 + LineupData.ExtraMpCount;
    }

    public List<AvatarLineupData> GetAvatarsFromTeam(int index)
    {
        var lineup = GetLineup(index);
        if (lineup == null) return [];

        var avatarList = new List<AvatarLineupData>();
        foreach (var avatar in lineup.BaseAvatars!)
        {
            var avatarType = AvatarType.AvatarFormalType;
            BaseAvatarInfo? avatarInfo = null;
            if (avatar.SpecialAvatarId > 0)
            {
                avatarInfo = Player.AvatarManager!.GetTrialAvatar(avatar.SpecialAvatarId);
                avatarType = AvatarType.AvatarTrialType;
            }
            else if (avatar.AssistUid > 0)
            {
                var avatarStorage = DatabaseHelper.Instance?.GetInstance<AvatarData>(avatar.AssistUid);
                avatarType = AvatarType.AvatarAssistType;
                if (avatarStorage == null) continue;
                foreach (var avatarData in avatarStorage.FormalAvatars.Where(avatarData =>
                             avatarData.AvatarId == avatar.BaseAvatarId))
                {
                    avatarInfo = avatarData;
                    break;
                }
            }
            else
            {
                avatarInfo = Player.AvatarManager!.GetFormalAvatar(avatar.BaseAvatarId);
            }

            if (avatarInfo == null) continue;
            avatarList.Add(new AvatarLineupData(avatarInfo, avatarType));
        }

        return avatarList;
    }

    public List<AvatarLineupData> GetAvatarsFromCurTeam()
    {
        return GetAvatarsFromTeam(LineupData.GetCurLineupIndex());
    }

    public List<LineupInfo> GetAllLineup()
    {
        var lineupList = new List<LineupInfo>();
        foreach (var lineupInfo in LineupData.Lineups.Values) lineupList.Add(lineupInfo);
        if (lineupList.Count < GameConstants.MAX_LINEUP_COUNT)
            for (var i = lineupList.Count; i < GameConstants.MAX_LINEUP_COUNT; i++)
            {
                var lineup = new LineupInfo
                {
                    Name = "",
                    LineupType = 0,
                    BaseAvatars = [],
                    LineupData = LineupData,
                    AvatarData = Player.AvatarManager!.AvatarData
                };
                lineupList.Add(lineup);
                LineupData.Lineups.Add(i, lineup);
            }

        return lineupList;
    }

    #endregion

    #region Management

    public async ValueTask<bool> SetCurLineup(int lineupIndex)
    {
        if (lineupIndex < 0 || !LineupData.Lineups.ContainsKey(lineupIndex)) return false;
        if (GetLineup(lineupIndex)!.BaseAvatars!.Count == 0) return false;
        LineupData.CurLineup = lineupIndex;
        LineupData.CurExtraLineup = -1;

        Player.SceneInstance?.SyncLineup();
        await Player.SendPacket(new PacketSyncLineupNotify(GetCurLineup()!));

        return true;
    }

    public void SetExtraLineup(ExtraLineupType type, List<int> baseAvatarIds, bool refresh = false)
    {
        if (type == ExtraLineupType.LineupNone)
        {
            // reset lineup
            LineupData.CurExtraLineup = -1;
            return;
        }

        var index = (int)type + 10;

        // destroy old lineup
        LineupData.Lineups.Remove(index);

        // create new lineup
        var lineup = new LineupInfo
        {
            Name = "",
            LineupType = (int)type,
            BaseAvatars = [],
            LineupData = LineupData,
            AvatarData = Player.AvatarManager!.AvatarData
        };

        var worldLevel = type == ExtraLineupType.LineupStageTrial ? 0 : Player.Data.WorldLevel;

        foreach (var avatarId in baseAvatarIds)
        {
            var trial = Player.AvatarManager!.GetTrialAvatar(avatarId, refresh);
            if (trial != null)
            {
                if (GameData.MultiplePathAvatarConfigData.TryGetValue(trial.AvatarId, out var pathExcel) && pathExcel.Gender != GenderTypeEnum.GENDER_NONE)
                {
                    if (pathExcel.Gender != (GenderTypeEnum)Player.Data.CurrentGender)
                    {
                        continue;
                    }
                }

                trial.CheckLevel(worldLevel);
                lineup.BaseAvatars!.Add(new LineupAvatarInfo
                    { BaseAvatarId = trial.BaseAvatarId, SpecialAvatarId = trial.SpecialAvatarId });
            }
            else
            {
                lineup.BaseAvatars!.Add(new LineupAvatarInfo { BaseAvatarId = avatarId });
            }
        }

        LineupData.Lineups.Add(index, lineup);
        LineupData.CurExtraLineup = index;
    }

    public async ValueTask AddAvatar(int lineupIndex, int avatarId, bool sendPacket = true)
    {
        if (lineupIndex < 0) return;
        LineupData.Lineups.TryGetValue(lineupIndex, out var lineup);

        if (lineup == null)
        {
            var baseAvatarId = avatarId;
            var specialAvatarId = avatarId * 10 + 0;
            GameData.SpecialAvatarData.TryGetValue(specialAvatarId, out var specialAvatar);
            if (specialAvatar != null)
            {
                Player.AvatarManager!.GetTrialAvatar(avatarId)?.CheckLevel(Player.Data.WorldLevel);
                baseAvatarId = specialAvatar.AvatarID;
            }
            else
            {
                if (baseAvatarId > 8000) baseAvatarId = 8001;
            }

            lineup = new LineupInfo
            {
                Name = "",
                LineupType = 0,
                BaseAvatars =
                [
                    new LineupAvatarInfo
                        { BaseAvatarId = baseAvatarId, SpecialAvatarId = specialAvatar?.SpecialAvatarID ?? 0 }
                ],
                LineupData = LineupData,
                AvatarData = Player.AvatarManager!.AvatarData
            };
            LineupData.Lineups.Add(lineupIndex, lineup);
        }
        else
        {
            if (lineup.BaseAvatars!.Count >= 4) return;

            var baseAvatarId = avatarId;
            var specialAvatarId = avatarId * 10 + 0;
            GameData.SpecialAvatarData.TryGetValue(specialAvatarId, out var specialAvatar);
            if (specialAvatar != null)
            {
                Player.AvatarManager!.GetTrialAvatar(avatarId)?.CheckLevel(Player.Data.WorldLevel);
                baseAvatarId = specialAvatar.AvatarID;
            }
            else
            {
                if (baseAvatarId > 8000) baseAvatarId = 8001;
            }

            lineup.BaseAvatars?.Add(new LineupAvatarInfo
                { BaseAvatarId = baseAvatarId, SpecialAvatarId = specialAvatar?.SpecialAvatarID ?? 0 });
            LineupData.Lineups[lineupIndex] = lineup;
        }

        if (sendPacket)
        {
            if (lineupIndex == LineupData.GetCurLineupIndex()) Player.SceneInstance?.SyncLineup();
            InvokeOnPlayerSyncLineup(Player, lineup);
            await Player.SendPacket(new PacketSyncLineupNotify(lineup));
        }
    }

    public async ValueTask AddAvatarToCurTeam(int avatarId, bool sendPacket = true)
    {
        await AddAvatar(LineupData.GetCurLineupIndex(), avatarId, sendPacket);
    }

    public async ValueTask AddSpecialAvatarToCurTeam(int specialAvatarId, bool sendPacket = true)
    {
        LineupData.Lineups.TryGetValue(LineupData.GetCurLineupIndex(), out var lineup);
        GameData.SpecialAvatarData.TryGetValue(specialAvatarId, out var specialAvatar);
        if (specialAvatar == null) return;
        Player.AvatarManager!.GetTrialAvatar(specialAvatar.SpecialAvatarID)?.CheckLevel(Player.Data.WorldLevel);
        if (lineup == null)
        {
            lineup = new LineupInfo
            {
                Name = "",
                LineupType = 0,
                BaseAvatars =
                [
                    new LineupAvatarInfo
                        { BaseAvatarId = specialAvatar.AvatarID, SpecialAvatarId = specialAvatar.SpecialAvatarID }
                ],
                LineupData = LineupData,
                AvatarData = Player.AvatarManager!.AvatarData
            };
            LineupData.Lineups.Add(LineupData.GetCurLineupIndex(), lineup);
        }
        else
        {
            if (lineup.BaseAvatars!.Count >= 4) lineup.BaseAvatars!.RemoveAt(3); // remove last avatar
            lineup.BaseAvatars?.Add(new LineupAvatarInfo
                { BaseAvatarId = specialAvatar.AvatarID, SpecialAvatarId = specialAvatar.SpecialAvatarID });
            LineupData.Lineups[LineupData.GetCurLineupIndex()] = lineup;
        }

        if (sendPacket)
        {
            Player.SceneInstance?.SyncLineup();
            InvokeOnPlayerSyncLineup(Player, lineup);
            await Player.SendPacket(new PacketSyncLineupNotify(lineup));
        }
    }

    public async ValueTask RemoveAvatar(int lineupIndex, int avatarId, bool sendPacket = true)
    {
        if (lineupIndex < 0) return;
        LineupData.Lineups.TryGetValue(lineupIndex, out var lineup);
        if (lineup == null) return;
        GameData.SpecialAvatarData.TryGetValue(avatarId * 10 + Player.Data.WorldLevel, out var specialAvatar);
        if (specialAvatar != null)
            lineup.BaseAvatars?.RemoveAll(avatar => avatar.BaseAvatarId == specialAvatar.AvatarID);
        else
            lineup.BaseAvatars?.RemoveAll(avatar => avatar.BaseAvatarId == avatarId);
        LineupData.Lineups[lineupIndex] = lineup;

        if (sendPacket)
        {
            if (lineupIndex == LineupData.GetCurLineupIndex()) Player.SceneInstance?.SyncLineup();
            InvokeOnPlayerSyncLineup(Player, lineup);
            await Player.SendPacket(new PacketSyncLineupNotify(lineup));
        }
    }

    public async ValueTask RemoveAvatarFromCurTeam(int avatarId, bool sendPacket = true)
    {
        await RemoveAvatar(LineupData.GetCurLineupIndex(), avatarId, sendPacket);
    }

    public async ValueTask ReplaceLineup(int lineupIndex, List<int> lineupSlotList,
        ExtraLineupType extraLineupType = ExtraLineupType.LineupNone)
    {
        if (extraLineupType != ExtraLineupType.LineupNone)
        {
            LineupData.CurExtraLineup = (int)extraLineupType + 10;
            if (!LineupData.Lineups.ContainsKey(LineupData.CurExtraLineup)) SetExtraLineup(extraLineupType, []);
        }

        LineupInfo lineup;
        if (LineupData.CurExtraLineup != -1)
            lineup = LineupData.Lineups[LineupData.CurExtraLineup]; // Extra lineup
        else if (lineupIndex < 0 || !LineupData.Lineups.TryGetValue(lineupIndex, out var dataLineup))
            return;
        else
            lineup = dataLineup;
        lineup.BaseAvatars = [];
        var index = lineup.LineupType == 0 ? lineupIndex : LineupData.GetCurLineupIndex();
        foreach (var avatar in lineupSlotList) await AddAvatar(index, avatar, false);

        if (index == LineupData.GetCurLineupIndex()) Player.SceneInstance?.SyncLineup();
        InvokeOnPlayerSyncLineup(Player, lineup);
        await Player.SendPacket(new PacketSyncLineupNotify(lineup));
    }

    public async ValueTask ReplaceLineup(ReplaceLineupCsReq req)
    {
        if (req.ExtraLineupType != ExtraLineupType.LineupNone)
        {
            LineupData.CurExtraLineup = (int)req.ExtraLineupType + 10;
            if (!LineupData.Lineups.ContainsKey(LineupData.CurExtraLineup)) SetExtraLineup(req.ExtraLineupType, []);
        }

        LineupInfo lineup;
        if (LineupData.CurExtraLineup != -1)
            lineup = LineupData.Lineups[LineupData.CurExtraLineup]; // Extra lineup
        else if (!LineupData.Lineups.ContainsKey((int)req.Index))
            return;
        else
            lineup = LineupData.Lineups[(int)req.Index];
        lineup.BaseAvatars = [];
        var index = lineup.LineupType == 0 ? (int)req.Index : LineupData.GetCurLineupIndex();
        foreach (var avatar in req.LineupSlotList) await AddAvatar(index, (int)avatar.Id, false);

        if (index == LineupData.GetCurLineupIndex()) Player.SceneInstance?.SyncLineup();
        InvokeOnPlayerSyncLineup(Player, lineup);
        await Player.SendPacket(new PacketSyncLineupNotify(lineup));
    }

    public async ValueTask DestroyExtraLineup(ExtraLineupType type)
    {
        var index = (int)type + 10;
        LineupData.Lineups.Remove(index);
        await Player.SendPacket(new PacketExtraLineupDestroyNotify(type));
    }

    public async ValueTask CostMp(int count, uint castEntityId = 1)
    {
        var curLineup = GetCurLineup()!;
        curLineup.Mp -= count;
        curLineup.Mp = Math.Min(Math.Max(0, curLineup.Mp), GetMaxMp());

        await Player.SendPacket(new PacketSceneCastSkillMpUpdateScNotify(castEntityId, curLineup.Mp));
    }

    public async ValueTask GainMp(int count, bool sendPacket = true,
        SyncLineupReason reason = SyncLineupReason.SyncReasonNone)
    {
        var curLineup = GetCurLineup()!;
        curLineup.Mp += count;
        curLineup.Mp = Math.Min(Math.Max(0, curLineup.Mp), GetMaxMp());
        if (sendPacket)
            await Player.SendPacket(
                new PacketSyncLineupNotify(GetCurLineup()!, reason));
    }

    #endregion
}

public record AvatarLineupData(BaseAvatarInfo AvatarInfo, AvatarType AvatarType);