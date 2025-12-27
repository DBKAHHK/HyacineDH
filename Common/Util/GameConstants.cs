namespace EggLink.DanhengServer.Util;

public static class GameConstants
{
    public const string GAME_VERSION = "3.8.0";
    public const string AvatarDbVersion = "20250430";
    public const int GameVersionInt = 3200;
    public const int MAX_STAMINA = 300;
    public const int MAX_STAMINA_RESERVE = 2400;
    public const int STAMINA_RECOVERY_TIME = 360; // 6 minutes
    public const int STAMINA_RESERVE_RECOVERY_TIME = 1080; // 18 minutes
    public const int INVENTORY_MAX_EQUIPMENT = 1500;
    public const int INVENTORY_MAX_RELIC = 1500;
    public const int INVENTORY_MAX_MATERIAL = 2000;
    public const int MAX_LINEUP_COUNT = 9;
    public const int LAST_TRAIN_WORLD_ID = 501;
    public const int AMBUSH_BUFF_ID = 1000102;
    public const int CHALLENGE_ENTRANCE = 100000103;
    public const int CHALLENGE_STORY_ENTRANCE = 102020107;
    public const int CHALLENGE_BOSS_ENTRANCE = 1030402;
    public const int CURRENT_ROGUE_TOURN_SEASON = 2;

    public static readonly List<int> UpgradeWorldLevel = [20, 30, 40, 50, 60, 65];
    public static readonly List<int> AllowedChessRogueEntranceId = [8020701, 8020901, 8020401, 8020201];
}