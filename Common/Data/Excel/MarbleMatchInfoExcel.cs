namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("MarbleMatchInfo.json")]
public class MarbleMatchInfoExcel : ExcelResource
{
    public int ID { get; set; }

    public override int GetId()
    {
        return ID;
    }

    public override void Loaded()
    {
        GameData.MarbleMatchInfoData.Add(ID, this);
    }
}