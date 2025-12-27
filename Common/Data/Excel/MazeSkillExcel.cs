namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("MazeSkill.json,MazeSkillLD.json", true)]
public class MazeSkillExcel : ExcelResource
{
    public int MazeSkillId { get; set; }
    public int MPCost { get; set; }
    public int MazeSkilltype { get; set; }

    public override int GetId()
    {
        return MazeSkillId;
    }

    public override void Loaded()
    {
        GameData.MazeSkillData.TryAdd(MazeSkillId, this);
    }
}