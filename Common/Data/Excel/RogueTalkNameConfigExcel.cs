namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("RogueTalkNameConfig.json")]
public class RogueTalkNameConfigExcel : ExcelResource
{
    public int TalkNameID { get; set; }
    public HashName Name { get; set; } = new();

    public override int GetId()
    {
        return TalkNameID;
    }

    public override void Loaded()
    {
        GameData.RogueTalkNameConfigData.Add(TalkNameID, this);
    }
}