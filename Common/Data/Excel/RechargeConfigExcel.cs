namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("RechargeConfig.json")]
public class RechargeConfigExcel : ExcelResource
{
    public string TierID { get; set; } = "";
    public string ProductID { get; set; } = "";
    public int GiftType { get; set; }
    public int ListOrder { get; set; }

    public override int GetId()
    {
        return TierID.GetHashCode();
    }

    public override void Loaded()
    {
        GameData.RechargeConfigData[ProductID] = this;
    }
}