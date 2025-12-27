using System.Text;
using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Internationalization;
using EggLink.DanhengServer.Program.Program;
using EggLink.DanhengServer.Util;
using Newtonsoft.Json;

namespace EggLink.DanhengServer.Program.Handbook;

public static class HandbookGenerator
{
    public static void GenerateAll()
    {
        var config = ConfigManager.Config;
        var directory = new DirectoryInfo(config.Path.ResourcePath + "/TextMap");
        var handbook = new DirectoryInfo("GM Handbook");
        if (!handbook.Exists)
            handbook.Create();
        if (!directory.Exists)
            return;

        foreach (var langFile in directory.GetFiles())
        {
            if (langFile.Extension != ".json") return;
            var lang = langFile.Name.Replace("TextMap", "").Replace(".json", "");

            // Check if handbook needs to regenerate
            var handbookPath = $"GM Handbook/GM Handbook {lang}.txt";
            if (File.Exists(handbookPath))
            {
                var handbookInfo = new FileInfo(handbookPath);
                if (handbookInfo.LastWriteTime >= langFile.LastWriteTime)
                    continue; // Skip if handbook is newer than language file
            }

            Generate(lang);
        }

        Logger.GetByClassName()
            .Info(I18NManager.Translate("Server.ServerInfo.GeneratedItem", I18NManager.Translate("Word.Handbook")));
    }

    public static void Generate(string lang)
    {
        var config = ConfigManager.Config;
        var textMapPath = config.Path.ResourcePath + "/TextMap/TextMap" + lang + ".json";
        var fallbackTextMapPath = config.Path.ResourcePath + "/TextMap/TextMap" + config.ServerOption.FallbackLanguage +
                                  ".json";
        if (!File.Exists(textMapPath))
        {
            Logger.GetByClassName().Error(I18NManager.Translate("Server.ServerInfo.FailedToReadItem", textMapPath,
                I18NManager.Translate("Word.NotFound")));
            return;
        }

        if (!File.Exists(fallbackTextMapPath))
        {
            Logger.GetByClassName().Error(I18NManager.Translate("Server.ServerInfo.FailedToReadItem", textMapPath,
                I18NManager.Translate("Word.NotFound")));
            return;
        }

        var textMap = JsonConvert.DeserializeObject<Dictionary<long, string>>(File.ReadAllText(textMapPath));
        var fallbackTextMap =
            JsonConvert.DeserializeObject<Dictionary<long, string>>(File.ReadAllText(fallbackTextMapPath));

        if (textMap == null || fallbackTextMap == null)
        {
            Logger.GetByClassName().Error(I18NManager.Translate("Server.ServerInfo.FailedToReadItem", textMapPath,
                I18NManager.Translate("Word.Error")));
            return;
        }

        var builder = new StringBuilder();
        builder.AppendLine("#Handbook generated in " + DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
        builder.AppendLine();
        builder.AppendLine("#Command");
        builder.AppendLine();
        GenerateCmd(builder, lang);

        builder.AppendLine();
        builder.AppendLine("#Avatar");
        builder.AppendLine();
        GenerateAvatar(builder, textMap, fallbackTextMap, lang == config.ServerOption.Language);

        builder.AppendLine();
        builder.AppendLine("#Item");
        builder.AppendLine();
        GenerateItem(builder, textMap, fallbackTextMap, lang == config.ServerOption.Language);

        builder.AppendLine();
        builder.AppendLine("#StageId");
        builder.AppendLine();
        GenerateStageId(builder, textMap, fallbackTextMap);

        builder.AppendLine();
        builder.AppendLine("#MainMission");
        builder.AppendLine();
        GenerateMainMissionId(builder, textMap, fallbackTextMap);

        builder.AppendLine();
        builder.AppendLine("#SubMission");
        builder.AppendLine();
        GenerateSubMissionId(builder, textMap, fallbackTextMap);

        builder.AppendLine();
        builder.AppendLine("#RogueBuff");
        builder.AppendLine();
        GenerateRogueBuff(builder, textMap, fallbackTextMap, lang == config.ServerOption.Language);

        builder.AppendLine();
        builder.AppendLine("#RogueMiracle");
        builder.AppendLine();
        GenerateRogueMiracleDisplay(builder, textMap, fallbackTextMap, lang == config.ServerOption.Language);

#if DEBUG
        builder.AppendLine();
        builder.AppendLine("#RogueDiceSurface");
        builder.AppendLine();
        GenerateRogueDiceSurfaceDisplay(builder, textMap, fallbackTextMap);

        builder.AppendLine();
        builder.AppendLine("#RogueDialogue");
        builder.AppendLine();
        GenerateRogueDialogueDisplay(builder, textMap, fallbackTextMap);
#endif

        builder.AppendLine();
        WriteToFile(lang, builder.ToString());
    }

    public static void GenerateCmd(StringBuilder builder, string lang)
    {
        foreach (var cmd in EntryPoint.CommandManager.CommandInfo)
        {
            builder.Append("\t" + cmd.Key);
            var desc = I18NManager.TranslateAsCertainLang(lang, cmd.Value.Description).Replace("\n", "\n\t\t");
            builder.AppendLine(": " + desc);
        }
    }

    public static void GenerateItem(StringBuilder builder, Dictionary<long, string> map,
        Dictionary<long, string> fallback, bool setName)
    {
        foreach (var item in GameData.ItemConfigData.Values)
        {
            var name = map.TryGetValue(item.ItemName.Hash, out var value) ? value :
                fallback.TryGetValue(item.ItemName.Hash, out value) ? value : $"[{item.ItemName.Hash}]";
            builder.AppendLine(item.ID + ": " + name);

            if (setName && name != $"[{item.ItemName.Hash}]") item.Name = name;
        }
    }

    public static void GenerateAvatar(StringBuilder builder, Dictionary<long, string> map,
        Dictionary<long, string> fallback, bool setName)
    {
        foreach (var avatar in GameData.AvatarConfigData.Values)
        {
            var name = map.TryGetValue(avatar.AvatarName.Hash, out var value) ? value :
                fallback.TryGetValue(avatar.AvatarName.Hash, out value) ? value : $"[{avatar.AvatarName.Hash}]";
            builder.AppendLine(avatar.AvatarID + ": " + name);

            if (setName && name != $"[{avatar.AvatarName.Hash}]") avatar.Name = name;
        }
    }

    public static void GenerateMainMissionId(StringBuilder builder, Dictionary<long, string> map,
        Dictionary<long, string> fallback)
    {
        foreach (var mission in GameData.MainMissionData.Values)
        {
            var name = map.TryGetValue(mission.Name.Hash, out var value) ? value :
                fallback.TryGetValue(mission.Name.Hash, out value) ? value : $"[{mission.Name.Hash}]";
            builder.AppendLine(mission.MainMissionID + ": " + name);
        }
    }

    public static void GenerateSubMissionId(StringBuilder builder, Dictionary<long, string> map,
        Dictionary<long, string> fallback)
    {
        foreach (var mission in GameData.SubMissionData.Values)
        {
            var name = map.TryGetValue(mission.TargetText.Hash, out var value) ? value :
                fallback.TryGetValue(mission.TargetText.Hash, out value) ? value : $"[{mission.TargetText.Hash}]";
            builder.AppendLine(mission.SubMissionID + ": " + name);
        }
    }

    public static void GenerateStageId(StringBuilder builder, Dictionary<long, string> map,
        Dictionary<long, string> fallback)
    {
        foreach (var stage in GameData.StageConfigData.Values)
        {
            var name = map.TryGetValue(stage.StageName.Hash, out var value) ? value :
                fallback.TryGetValue(stage.StageName.Hash, out value) ? value : $"[{stage.StageName.Hash}]";
            builder.AppendLine(stage.StageID + ": " + name);
        }
    }

    public static void GenerateRogueBuff(StringBuilder builder, Dictionary<long, string> map,
        Dictionary<long, string> fallback, bool setName)
    {
        foreach (var buff in GameData.RogueMazeBuffData)
        {
            var name = map.TryGetValue(buff.Value.BuffName.Hash, out var value)
                ? value
                : fallback.TryGetValue(buff.Value.BuffName.Hash, out value)
                    ? value
                    : $"[{buff.Value.BuffName.Hash}]";
            builder.AppendLine(buff.Key + ": " + name + " --- Level:" + buff.Value.Lv);

            if (setName && name != $"[{buff.Value.BuffName.Hash}]") buff.Value.Name = name;
        }
    }

    public static void GenerateRogueMiracleDisplay(StringBuilder builder, Dictionary<long, string> map,
        Dictionary<long, string> fallback, bool setName)
    {
        foreach (var display in GameData.RogueMiracleData.Values)
        {
            var name = map.TryGetValue(display.MiracleName.Hash, out var value)
                ? value
                : fallback.TryGetValue(display.MiracleName.Hash, out value)
                    ? value
                    : $"[{display.MiracleName.Hash}]";
            builder.AppendLine(display.MiracleID + ": " + name);

            if (setName && name != $"[{display.MiracleName.Hash}]") display.Name = name;
        }
    }

    public static string GetNameFromTextMap(long key, Dictionary<long, string> map, Dictionary<long, string> fallback)
    {
        if (map.TryGetValue(key, out var value)) return value;
        if (fallback.TryGetValue(key, out value)) return value;
        return $"[{key}]";
    }

    public static void WriteToFile(string lang, string content)
    {
        File.WriteAllText($"GM Handbook/GM Handbook {lang}.txt", content);
    }

#if DEBUG
    public static void GenerateRogueDiceSurfaceDisplay(StringBuilder builder, Dictionary<long, string> map,
        Dictionary<long, string> fallback)
    {
        foreach (var display in GameData.RogueNousDiceSurfaceData.Values)
        {
            var name = map.TryGetValue(display.SurfaceName.Hash, out var value)
                ? value
                : fallback.TryGetValue(display.SurfaceName.Hash, out value)
                    ? value
                    : $"[{display.SurfaceName.Hash}]";
            var desc = map.TryGetValue(display.SurfaceDesc.Hash, out var c) ? c : $"[{display.SurfaceDesc.Hash}]";
            builder.AppendLine(display.SurfaceID + ": " + name + "\n" + "Desc: " + desc);
        }
    }

    public static void GenerateRogueDialogueDisplay(StringBuilder builder, Dictionary<long, string> map,
        Dictionary<long, string> fallback)
    {
        foreach (var npc in GameData.RogueNPCData.Values.Where(x => x.RogueNpcConfig != null))
        {
            builder.AppendLine("NpcId: " + npc.RogueNPCID);
            foreach (var dialogue in npc.RogueNpcConfig?.DialogueList ?? [])
            {
                var eventNameHash =
                    GameData.RogueTalkNameConfigData.GetValueOrDefault(dialogue.TalkNameID)?.Name.Hash ??
                    -1;
                var eventName = GetNameFromTextMap(eventNameHash, map, fallback);
                builder.AppendLine($"  Progress: {dialogue.DialogueProgress} | {eventName}");
                builder.AppendLine($"  Type: {npc.RogueNpcConfig!.DialogueType}");
                builder.AppendLine("  Options: ");

                foreach (var option in dialogue.OptionInfo?.OptionList ?? [])
                {
                    var display = GameData.RogueDialogueOptionDisplayData.GetValueOrDefault(option.DisplayID);
                    if (display == null) continue;

                    var optionName = GetNameFromTextMap(display.OptionTitle.Hash, map, fallback);
                    var optionDesc = GetNameFromTextMap(display.OptionDesc.Hash, map, fallback);
                    builder.AppendLine($"    Option: {option.OptionID} - {optionName}");
                    builder.AppendLine($"      {optionDesc}".Replace("#2", option.DescValue.ToString())
                        .Replace("#5", option.DescValue2.ToString()).Replace("#6", option.DescValue3.ToString())
                        .Replace("#7", option.DescValue4.ToString()));
                    if (option.DynamicMap.Count == 0) continue;

                    builder.AppendLine("      Dynamic Value:");
                    foreach (var value in option.DynamicMap)
                    {
                        var dynamic = GameData.RogueDialogueDynamicDisplayData.GetValueOrDefault(value.Value.DisplayID);
                        if (dynamic == null) continue;
                        var dynamicName = GetNameFromTextMap(dynamic.ContentText.Hash, map, fallback);
                        builder.AppendLine($"        Dynamic Id: {value.Key} | {dynamicName}");
                    }
                }
            }
        }
    }
#endif
}