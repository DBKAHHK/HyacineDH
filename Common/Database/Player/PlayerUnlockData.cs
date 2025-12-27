using SqlSugar;

namespace EggLink.DanhengServer.Database.Player;

[SugarTable("UnlockData")]
public class PlayerUnlockData : BaseDatabaseDataHelper
{
    [SugarColumn(IsJson = true)] public List<int> HeadIcons { get; set; } = [];
    [SugarColumn(IsJson = true)] public List<int> ChatBubbles { get; set; } = [];
    [SugarColumn(IsJson = true)] public List<int> PhoneThemes { get; set; } = [];
    [SugarColumn(IsJson = true)] public List<int> PersonalCards { get; set; } = [];
    [SugarColumn(IsJson = true)] public List<int> PhoneCases { get; set; } = [];
    [SugarColumn(IsJson = true)] public Dictionary<int, List<int>> Skins { get; set; } = [];
}