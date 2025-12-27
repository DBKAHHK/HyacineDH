using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.Database.TrainParty;
using EggLink.DanhengServer.GameServer.Game.Player;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.TrainParty;

public class TrainPartyManager(PlayerInstance player) : BasePlayerManager(player)
{
    public TrainData Data { get; } =
        DatabaseHelper.Instance!.GetInstanceOrCreateNew<TrainData>(player.Uid);

    public TrainAreaInfo? SetDynamicId(int areaId, int slotId, int dynamicId)
    {
        if (!Data.Areas.TryGetValue(areaId, out var area)) return null;

        area.DynamicInfo[slotId] = dynamicId;

        return area;
    }

    public TrainPartyData ToProto()
    {
        var proto = new TrainPartyData
        {
            TrainPartyInfo = ToPartyInfo(),
            PassengerInfo = ToPassenger(),
            UnlockAreaNum = 6
        };

        return proto;
    }

    public TrainPartyInfo ToPartyInfo()
    {
        var proto = new TrainPartyInfo
        {
            LABABMEDHGI = 30,
            CurFund = (uint)Data.Fund,
            AreaList = { Data.Areas.Values.Select(x => x.ToProto()) },
            DynamicIdList = { GameData.TrainPartyDynamicConfigData.Select(x => (uint)x.Key) }
        };

        return proto;
    }

    public static TrainPartyPassengerInfo ToPassenger()
    {
        var info = new TrainPartyPassengerInfo();
        info.PassengerInfoList.AddRange(
            GameData.TrainPartyPassengerConfigData.Select(x => new TrainPartyPassenger
            {
                PassengerId = (uint)x.Key
            })
        );
        return info;
    }
}