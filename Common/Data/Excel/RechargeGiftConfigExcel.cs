namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("RechargeGiftConfig.json")]
public class RechargeGiftConfigExcel : ExcelResource
{
    public int GiftType { get; set; }
    public List<int> GiftIDList { get; set; } = [];

    public override int GetId()
    {
        return GiftType;
    }

    public override void Loaded()
    {
        GameData.RechargeGiftConfigData.Add(GiftType, this);
    }
}