namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("ChallengePeakGroupConfig.json")]
public class ChallengePeakGroupConfigExcel : ExcelResource
{
    public int ID { get; set; }
    public TextMapHash? Title { get; set; }

    public int RecommendID { get; set; }
    public int ActivityModule { get; set; }

    public List<int> PreLevelIDList { get; set; } = [];
    public int BossLevelID { get; set; }

    public int RewardGroupID { get; set; }

    public string? BossUI3DPrefabPath { get; set; }
    public string? BossUI3DAnimatorPath { get; set; }
    public string? ThemePosterTabPicPath { get; set; }
    public string? ThemeIconPicPath { get; set; }
    public string? HandBookPanelBannerPath { get; set; }

    public List<string> RankIconPathList { get; set; } = [];

    public override int GetId()
    {
        return ID;
    }

    public override void Loaded()
    {
        GameData.ChallengePeakGroupConfigData[ID] = this;
    }
}

