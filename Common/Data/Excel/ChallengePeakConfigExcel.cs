using Newtonsoft.Json;

namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("ChallengePeakConfig.json")]
public class ChallengePeakConfigExcel : ExcelResource
{
    public int ID { get; set; }

    public TextMapHash? Title { get; set; }

    public List<int> NormalTargetList { get; set; } = [];

    public List<string> DamageType { get; set; } = [];

    public List<int> EventIDList { get; set; } = [];

    public List<int> TagList { get; set; } = [];

    public List<int> ProgressValueList { get; set; } = [];

    public List<int> HPProgressValueList { get; set; } = [];

    public override int GetId()
    {
        return ID;
    }

    public override void Loaded()
    {
        GameData.ChallengePeakConfigData[ID] = this;
    }
}

public class TextMapHash
{
    [JsonProperty("Hash")] public long Hash { get; set; }
}

