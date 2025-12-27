using EggLink.DanhengServer.Internationalization;

namespace EggLink.DanhengServer.Command.Command.Cmd;

[CommandInfo("relic", "Game.Command.Relic.Desc", "Game.Command.Relic.Usage")]
public class CommandRelic : ICommand
{
    [CommandDefault]
    public async ValueTask GiveRelic(CommandArg arg)
    {
        var player = arg.Target?.Player;
        if (player == null)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.PlayerNotFound"));
            return;
        }

        if (arg.BasicArgs.Count < 2)
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        // Parse character
        arg.CharacterArgs.TryGetValue("x", out var str);
        arg.CharacterArgs.TryGetValue("l", out var levelStr);
        str ??= "1";
        levelStr ??= "0";
        if (!int.TryParse(str, out var amount) || !int.TryParse(levelStr, out var level))
        {
            await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
            return;
        }

        // Parse main affix
        var startIndex = 1;
        var mainAffixId = 0;
        if (!arg.BasicArgs[1].Contains(':'))
        {
            mainAffixId = int.Parse(arg.BasicArgs[1]);
            startIndex++;
        }

        // Parse sub affixes
        var subAffixes = new List<(int, int)>();
        for (var ii = startIndex; ii < arg.BasicArgs.Count; ii++)
        {
            var subAffix = arg.BasicArgs[ii].Split(':');
            if (subAffix.Length != 2 || !int.TryParse(subAffix[0], out var subId) ||
                !int.TryParse(subAffix[1], out var subLevel))
            {
                await arg.SendMsg(I18NManager.Translate("Game.Command.Notice.InvalidArguments"));
                return;
            }

            subAffixes.Add((subId, subLevel));
        }

        for (var i = 0; i < amount; i++)
        {
            var (ret, _) = await player.InventoryManager!.HandleRelic(
                int.Parse(arg.BasicArgs[0]), ++player.InventoryManager!.Data.NextUniqueId,
                level, mainAffixId, subAffixes);

            switch (ret)
            {
                case 1:
                    await arg.SendMsg(I18NManager.Translate("Game.Command.Relic.RelicNotFound"));
                    return;
                case 2:
                    await arg.SendMsg(I18NManager.Translate("Game.Command.Relic.InvalidMainAffixId"));
                    return;
                case 3:
                    await arg.SendMsg(I18NManager.Translate("Game.Command.Relic.InvalidSubAffixId"));
                    return;
            }
        }

        await arg.SendMsg(I18NManager.Translate("Game.Command.Relic.RelicGiven", player.Uid.ToString(),
            amount.ToString(), arg.BasicArgs[0], mainAffixId.ToString()));
    }
}