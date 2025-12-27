namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("AvatarSkillTreeConfig.json,AvatarSkillTreeConfigLD.json", true)]
public class AvatarSkillTreeConfigExcel : ExcelResource
{
    public int PointID { get; set; }
    public int Level { get; set; }
    public int AvatarID { get; set; }
    public int EnhancedID { get; set; }
    public bool DefaultUnlock { get; set; }
    public int MaxLevel { get; set; }

    public override int GetId()
    {
        return PointID * 100 + Level;
    }

    public override void AfterAllDone()
    {
        GameData.AvatarConfigData.TryGetValue(AvatarID, out var excel);
        GameData.AvatarSkillTreeConfigData.TryAdd(GetId(), this);
        if (excel == null) return;

        excel.DefaultSkillTree.TryAdd(EnhancedID, []);
        excel.SkillTree.TryAdd(EnhancedID, []);
        if (DefaultUnlock && excel.DefaultSkillTree[EnhancedID].All(x => x.PointID != PointID))
            excel.DefaultSkillTree[EnhancedID].Add(this);
        if (excel.SkillTree[EnhancedID].All(x => x.PointID != PointID)) excel.SkillTree[EnhancedID].Add(this);
    }
}