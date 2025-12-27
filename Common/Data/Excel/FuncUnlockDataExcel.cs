using EggLink.DanhengServer.Enums.Quest;
using EggLink.DanhengServer.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("FuncUnlockData.json")]
public class FuncUnlockDataExcel : ExcelResource
{
    public int UnlockID { get; set; }
    public List<FuncUnlockCondition> Conditions { get; set; } = [];

    public override int GetId()
    {
        return UnlockID;
    }

    public override void Loaded()
    {
        GameData.FuncUnlockDataData[UnlockID] = this;
    }
}

public class FuncUnlockCondition
{
    [JsonConverter(typeof(LenientStringEnumConverter<ConditionTypeEnum>))]
    public ConditionTypeEnum Type { get; set; }

    public string Param { get; set; } = string.Empty;
}
