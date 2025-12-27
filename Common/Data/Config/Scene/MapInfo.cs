namespace EggLink.DanhengServer.Data.Config.Scene;

public class MapInfo
{
    public List<AreaInfo> AreaList { get; set; } = [];
    public List<int> MapLayerList { get; set; } = [];
}

public class AreaInfo
{
    public int ID { get; set; }
    public MinimapVolumeInfo MinimapVolume { get; set; } = new();
    public List<int> RegionIDList { get; set; } = [];
    public List<int> MapLayerList { get; set; } = [];
}

public class MinimapVolumeInfo
{
    public List<SectionsInfo> Sections { get; set; } = new();
}

public class SectionsInfo
{
    public int ID { get; set; }
    public int MapLayerID { get; set; }
    public bool IsRect { get; set; }
    public List<int> Indices { get; set; } = [];
}