using EggLink.DanhengServer.Enums.Avatar;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EggLink.DanhengServer.Data.Config.Task;

public class AdventureByPlayerCurrentSkillType : PredicateConfigInfo
{
    [JsonConverter(typeof(StringEnumConverter))]
    public AdventureSkillTypeEnum SkillType { get; set; }
}