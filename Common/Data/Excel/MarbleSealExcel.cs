namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("MarbleSeal.json")]
public class MarbleSealExcel : ExcelResource
{
    public int ID { get; set; }
    public int Attack { get; set; }
    public float MaxSpeed { get; set; }
    public float Mass { get; set; }
    public int Hp { get; set; }
    public int ActionPriority { get; set; }
    public float Size { get; set; }

    public override int GetId()
    {
        return ID;
    }

    public override void Loaded()
    {
        GameData.MarbleSealData.Add(ID, this);
    }
}