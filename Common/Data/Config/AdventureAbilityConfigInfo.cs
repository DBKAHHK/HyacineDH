using EggLink.DanhengServer.Data.Config.Task;
using Newtonsoft.Json.Linq;

namespace EggLink.DanhengServer.Data.Config;

public class AdventureAbilityConfigInfo
{
    public List<TaskConfigInfo> OnAbort { get; set; } = [];
    public string Name { get; set; } = "";
    public List<TaskConfigInfo> OnAdd { get; set; } = [];
    public List<TaskConfigInfo> OnRemove { get; set; } = [];
    public List<TaskConfigInfo> OnStart { get; set; } = [];

    public static AdventureAbilityConfigInfo LoadFromJsonObject(JObject obj)
    {
        AdventureAbilityConfigInfo info = new();

        if (obj.ContainsKey(nameof(OnAbort)))
            info.OnAbort = obj[nameof(OnAbort)]
                ?.Select(x => TaskConfigInfo.LoadFromJsonObject((x as JObject)!)).ToList() ?? [];

        if (obj.ContainsKey(nameof(Name)))
            info.Name = obj[nameof(Name)]?.ToObject<string>() ?? "";

        if (obj.ContainsKey(nameof(OnAdd)))
            info.OnAdd = obj[nameof(OnAdd)]
                ?.Select(x => TaskConfigInfo.LoadFromJsonObject((x as JObject)!)).ToList() ?? [];

        if (obj.ContainsKey(nameof(OnRemove)))
            info.OnRemove = obj[nameof(OnRemove)]
                ?.Select(x => TaskConfigInfo.LoadFromJsonObject((x as JObject)!)).ToList() ?? [];

        if (obj.ContainsKey(nameof(OnStart)))
            info.OnStart = obj[nameof(OnStart)]
                ?.Select(x => TaskConfigInfo.LoadFromJsonObject((x as JObject)!)).ToList() ?? [];

        return info;
    }
}