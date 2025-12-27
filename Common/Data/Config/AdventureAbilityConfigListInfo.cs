using EggLink.DanhengServer.Data.Config.AdventureAbility;
using Newtonsoft.Json.Linq;

namespace EggLink.DanhengServer.Data.Config;

public class AdventureAbilityConfigListInfo
{
    public List<AdventureAbilityConfigInfo> AbilityList { get; set; } = [];
    public Dictionary<string, AdventureModifierConfig>? GlobalModifiers { get; set; } = [];

    public static AdventureAbilityConfigListInfo LoadFromJsonObject(JObject obj)
    {
        AdventureAbilityConfigListInfo info = new();

        if (obj.ContainsKey(nameof(AbilityList)))
            info.AbilityList = obj[nameof(AbilityList)]
                ?.Select(x => AdventureAbilityConfigInfo.LoadFromJsonObject((x as JObject)!)).ToList() ?? [];

        if (!obj.ContainsKey(nameof(GlobalModifiers))) return info;
        info.GlobalModifiers = [];
        foreach (var jObject in obj[nameof(GlobalModifiers)]!.ToObject<Dictionary<string, JObject>>()!)
            info.GlobalModifiers.Add(jObject.Key, AdventureModifierConfig.LoadFromJObject(jObject.Value));

        return info;
    }
}