using System.Text;
using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Custom;
using EggLink.DanhengServer.Enums.TournRogue;
using EggLink.DanhengServer.Util;
using Newtonsoft.Json;

namespace EggLink.DanhengServer.Program.Generator;

public static class TournRoomGenerator
{
    public static List<int> AllowedFloorIdList { get; set; } = [80601001, 80602001, 80603001, 80604001];
    public static List<RogueTournRoomConfig> SavedRoomInstanceList { get; set; } = [];

    public static void GenerateFile(string path)
    {
        // get floor info
        foreach (var floorId in AllowedFloorIdList)
        {
            var areaGroupId = 0;
            var baseModuleId = 0;
            var roomType = RogueTournRoomTypeEnum.Unknown;
            var isCommon = false;
            Dictionary<RogueTournRoomTypeEnum, List<int>> contentGroupId = [];

            var info = GameData.FloorInfoData.Values.First(x => x.FloorID == floorId);
            foreach (var groupInfo in info.GroupInstanceList.Where(x =>
                         !x.IsDelete && x.Name.Contains("RogueModule_Tournament") && !x.Name.Contains("Tpl_")))
            {
                if (groupInfo.Name.Contains("_Area"))
                {
                    if (areaGroupId > 0 && baseModuleId > 0 && contentGroupId.Count > 0)
                        foreach (var group in contentGroupId)
                            FlushRoom(GameData.MapEntranceData.First(x => x.Value.FloorID == floorId).Key, areaGroupId,
                                baseModuleId, group.Value, group.Key);

                    contentGroupId.Clear();

                    areaGroupId = groupInfo.ID;
                    continue;
                }

                if (groupInfo.Name.Contains("_Base"))
                {
                    if (areaGroupId > 0 && baseModuleId > 0 && contentGroupId.Count > 0)
                        foreach (var group in contentGroupId)
                            FlushRoom(GameData.MapEntranceData.First(x => x.Value.FloorID == floorId).Key, areaGroupId,
                                baseModuleId, group.Value, group.Key);

                    contentGroupId.Clear();

                    baseModuleId = groupInfo.ID;
                    isCommon = false;
                    if (groupInfo.Name.Contains("_Common"))
                        isCommon = true;
                    else if (groupInfo.Name.Contains("_Boss"))
                        roomType = RogueTournRoomTypeEnum.Boss;
                    else if (groupInfo.Name.Contains("_Elite"))
                        roomType = RogueTournRoomTypeEnum.Elite;
                    else if (groupInfo.Name.Contains("_Shop"))
                        roomType = RogueTournRoomTypeEnum.Shop;
                    else if (groupInfo.Name.Contains("_Rest"))
                        roomType = RogueTournRoomTypeEnum.Respite;
                    else if (groupInfo.Name.Contains("_Adventure"))
                        roomType = RogueTournRoomTypeEnum.Adventure;
                    else if (groupInfo.Name.Contains("_Secret"))
                        roomType = RogueTournRoomTypeEnum.Hidden;

                    continue;
                }

                if (areaGroupId == 0 || baseModuleId == 0) continue;

                if (isCommon)
                {
                    // contain Battle Event Coin
                    if (groupInfo.Name.Contains("Monster"))
                    {
                        contentGroupId.TryAdd(RogueTournRoomTypeEnum.Battle, []);
                        contentGroupId[RogueTournRoomTypeEnum.Battle].Add(groupInfo.ID);
                    }
                    else if (groupInfo.Name.Contains("Event"))
                    {
                        contentGroupId.TryAdd(RogueTournRoomTypeEnum.Event, []);
                        contentGroupId[RogueTournRoomTypeEnum.Event].Add(groupInfo.ID);
                    }
                    else if (groupInfo.Name.Contains("Coin"))
                    {
                        contentGroupId.TryAdd(RogueTournRoomTypeEnum.Coin, []);
                        contentGroupId[RogueTournRoomTypeEnum.Coin].Add(groupInfo.ID);
                    }
                }
                else
                {
                    contentGroupId.TryAdd(roomType, []);
                    contentGroupId[roomType].Add(groupInfo.ID);
                }
            }
        }

        // save
        File.AppendAllText(path, JsonConvert.SerializeObject(SavedRoomInstanceList, Formatting.Indented),
            Encoding.UTF8);

        // log
        Logger.GetByClassName().Info($"Generated in {path} Successfully!");
    }

    public static void FlushRoom(int entranceId, int areaGroupId, int baseGroupId, List<int> contentGroupIds,
        RogueTournRoomTypeEnum type)
    {
        SavedRoomInstanceList.Add(new RogueTournRoomConfig
        {
            AnchorGroup = baseGroupId,
            AnchorId = 1,
            DefaultLoadBasicGroup = { areaGroupId, baseGroupId },
            DefaultLoadGroup = contentGroupIds,
            EntranceId = entranceId,
            RoomType = type
        });
    }
}