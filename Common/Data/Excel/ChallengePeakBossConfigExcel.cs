namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("ChallengePeakBossConfig.json")]
public class ChallengePeakBossConfigExcel : ExcelResource
{
    public int ID { get; set; }
    public TextMapHash? HardTitle { get; set; }

    public List<int> BuffList { get; set; } = [];
    public int ColorMedalTarget { get; set; }
    public int HardTarget { get; set; }

    public List<int> HardEventIDList { get; set; } = [];
    public List<int> HardTagList { get; set; } = [];

    public override int GetId()
    {
        return ID;
    }

    public override void Loaded()
    {
        GameData.ChallengePeakBossConfigData[ID] = this;
    }
}

