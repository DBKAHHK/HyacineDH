namespace EggLink.DanhengServer.Data.Config.Task;

public class DestroySummonUnit : TaskConfigInfo
{
    public SummonUnitSelector SummonUnit { get; set; } = new();
}

public class SummonUnitSelector
{
    public int SummonUnitID { get; set; }
}