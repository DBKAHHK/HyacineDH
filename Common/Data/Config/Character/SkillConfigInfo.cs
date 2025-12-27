using EggLink.DanhengServer.Enums.Avatar;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EggLink.DanhengServer.Data.Config.Character;

public class SkillConfigInfo
{
    public string EntryAbility { get; set; } = "";

    [JsonConverter(typeof(StringEnumConverter))]
    public SkillUseTypeEnum UseType { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public AdventureSkillTypeEnum AdventureSkillType { get; set; }
}