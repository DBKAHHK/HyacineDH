using Newtonsoft.Json;

namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("ChallengePeakCommonConst.json")]
public class ChallengePeakCommonConstExcel : ExcelResource
{
    public string ConstValueName { get; set; } = "";

    public ChallengePeakConstValue? Value { get; set; }

    public override int GetId()
    {
        return ConstValueName.GetHashCode();
    }

    public override void Loaded()
    {
        if (string.IsNullOrWhiteSpace(ConstValueName) || Value == null) return;
        GameData.ChallengePeakCommonConstData[ConstValueName] = Value;
    }
}

public class ChallengePeakConstValue
{
    public int? IntValue { get; set; }
    public string? StringValue { get; set; }

    public List<ChallengePeakConstValue>? ArrayValue { get; set; }

    [JsonExtensionData] public Dictionary<string, object>? Extra { get; set; }
}

