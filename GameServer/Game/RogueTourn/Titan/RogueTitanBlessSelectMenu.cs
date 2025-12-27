using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Data.Excel;
using EggLink.DanhengServer.Enums.TournRogue;
using EggLink.DanhengServer.GameServer.Game.Rogue;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;

namespace EggLink.DanhengServer.GameServer.Game.RogueTourn.Titan;

public class RogueTitanBlessSelectMenu(RogueTournInstance rogue)
{
    public List<RogueTournTitanBlessExcel> Blesses { get; set; } = [];
    public int QueueAppend { get; set; } = 3;
    public bool TypeSelect { get; set; }

    public void RollTitanBless(int count = 3, bool typeSelect = false)
    {
        var list = GameData.RogueTournTitanBlessData.Values.Where(x =>
                x.TitanType == rogue.TitanType && x.TitanBlessLevel ==
                (rogue.RogueTitanBlessInstance.EnhanceBlessList.Count(j => j.TitanType == rogue.TitanType) < 2 ? 2 : 3))
            .ToList();

        if (typeSelect)
        {
            list = [];
            TypeSelect = true;
            // random 2 types
            var night = GameData.RogueTournTitanBlessData.Values.Where(x =>
                GameData.RogueTournTitanTypeData.GetValueOrDefault(x.TitanType)?.RogueTitanCategory ==
                RogueTitanCategoryEnum.Night && x.TitanBlessLevel == 1 &&
                !rogue.RogueTitanBlessInstance.BlessTypeExcel.Contains(x)).ToList().RandomElement();

            var day = GameData.RogueTournTitanBlessData.Values.Where(x =>
                GameData.RogueTournTitanTypeData.GetValueOrDefault(x.TitanType)?.RogueTitanCategory ==
                RogueTitanCategoryEnum.Day && x.TitanBlessLevel == 1 &&
                !rogue.RogueTitanBlessInstance.BlessTypeExcel.Contains(x)).ToList().RandomElement();

            list.Add(day);
            list.Add(night);
        }

        if (list.Count == 0) return;

        var result = new List<RogueTournTitanBlessExcel>();

        for (var i = 0; i < count; i++)
        {
            var blessExcel = list.RandomElement();
            result.Add(blessExcel);
            list.Remove(blessExcel);

            if (list.Count == 0) break; // No more formulas to roll
        }

        Blesses = result;
    }

    public RogueActionInstance GetActionInstance()
    {
        rogue.CurActionQueuePosition += QueueAppend;
        return new RogueActionInstance
        {
            QueuePosition = rogue.CurActionQueuePosition,
            RogueTitanBlessSelectMenu = this
        };
    }


    public RogueTitanBlessSelectInfo ToProto()
    {
        return new RogueTitanBlessSelectInfo
        {
            BlessSelectType = TypeSelect
                ? TitanBlessSelectType.KSelectTitanBlessType
                : TitanBlessSelectType.KSelectTitanBlessEnhance,
            TitanBlessIdList = { Blesses.Select(x => (uint)x.TitanBlessID) },
            SelectHintId = (uint)(TypeSelect ? 310001 : 310002)
        };
    }
}