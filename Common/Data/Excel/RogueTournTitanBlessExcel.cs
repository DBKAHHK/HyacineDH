using EggLink.DanhengServer.Enums.TournRogue;
using EggLink.DanhengServer.Proto;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("RogueTournTitanBless.json")]
public class RogueTournTitanBlessExcel : ExcelResource
{
    public int TitanBlessID { get; set; }
    public int MazeBuffID { get; set; }
    public int TitanBlessLevel { get; set; }
    public int BlessRatio { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public RogueTitanTypeEnum TitanType { get; set; }

    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public List<RogueTitanCategoryEnum> BlessBattleDisplayCategoryList { get; set; } = [];

    public override int GetId()
    {
        return TitanBlessID;
    }

    public override void Loaded()
    {
        GameData.RogueTournTitanBlessData.Add(TitanBlessID, this);
    }

    public RogueCommonActionResult ToResultProto(RogueCommonActionResultSourceType select)
    {
        return new RogueCommonActionResult
        {
            Source = select,
            RogueAction = new RogueCommonActionResultData
            {
                TitanBlessEvent = new RogueTitanBlessEvent
                {
                    EventUniqueId = (uint)TitanBlessID
                }
            }
        };
    }
}