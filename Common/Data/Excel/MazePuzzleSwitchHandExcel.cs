namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("MazePuzzleSwitchHand.json")]
public class MazePuzzleSwitchHandExcel : ExcelResource
{
    public int SwitchID { get; set; }
    public int PlaneID { get; set; }
    public int FloorID { get; set; }
    public List<int> SwitchHandID { get; set; } = [];

    public override int GetId()
    {
        return SwitchID;
    }

    public override void Loaded()
    {
        GameData.MazePuzzleSwitchHandData.Add(SwitchID, this);
    }
}