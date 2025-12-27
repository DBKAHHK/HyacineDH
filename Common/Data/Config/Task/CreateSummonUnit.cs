namespace EggLink.DanhengServer.Data.Config.Task;

public class CreateSummonUnit : TaskConfigInfo
{
    public int SummonUnitID { get; set; }
    public DynamicFloat Duration { get; set; } = new();
}