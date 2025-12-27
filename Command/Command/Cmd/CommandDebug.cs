using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Internationalization;

namespace EggLink.DanhengServer.Command.Command.Cmd;

[CommandInfo("debug", "Game.Command.Debug.Desc", "Game.Command.Debug.Usage")]
public class CommandDebug : ICommand
{
    [CommandMethod("0 specific")]
    public async ValueTask SpecificNextStage(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.BasicArgs.Count == 0)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        if (!int.TryParse(arg.BasicArgs[0], out var stageId))
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        if (!GameData.StageConfigData.TryGetValue(stageId, out var stage))
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Debug.InvalidStageId"));
            return;
        }

        player.BattleManager!.NextBattleStageConfig = stage;
        await arg.SendMsg(I18NManager.Translate("Game.Command.Debug.SetStageId"));
    }

    [CommandMethod("0 monster")]
    public async ValueTask AddMonster(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.BasicArgs.Count == 0)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        if (!int.TryParse(arg.BasicArgs[0], out var monsterId))
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        if (!GameData.MonsterConfigData.TryGetValue(monsterId, out _))
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Debug.InvalidStageId"));
            return;
        }

        player.BattleManager!.NextBattleMonsterIds.Add(monsterId);
        await arg.SendMsg(I18NManager.Translate("Game.Command.Debug.SetStageId"));
    }
}