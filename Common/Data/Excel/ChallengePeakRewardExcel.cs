namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("ChallengePeakReward.json")]
public class ChallengePeakRewardExcel : ExcelResource
{
    public int ID { get; set; }

    public int RewardGroupID { get; set; }

    public string? RewardType { get; set; }

    public int TypeValue { get; set; }

    public int RewardID { get; set; }

    public override int GetId()
    {
        return ID;
    }

    public override void Loaded()
    {
        if (!GameData.ChallengePeakRewardData.TryGetValue(RewardGroupID, out var list))
        {
            list = [];
            GameData.ChallengePeakRewardData[RewardGroupID] = list;
        }

        list.Add(this);
    }
}

