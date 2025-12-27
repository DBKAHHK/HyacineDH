namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("DecideAvatarOrder.json")]
public class DecideAvatarOrderExcel : ExcelResource
{
    public int ItemID { get; set; } = 0;
    public int Order { get; set; } = 0;

    public override int GetId()
    {
        return ItemID;
    }

    public override void Loaded()
    {
        GameData.DecideAvatarOrderData.Add(ItemID, this);
    }
}