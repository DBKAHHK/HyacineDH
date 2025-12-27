using EggLink.DanhengServer.Proto;
using Newtonsoft.Json;

namespace EggLink.DanhengServer.Data.Excel;

[ResourceEntity("ShopGoodsConfig.json")]
public class ShopGoodsConfigExcel : ExcelResource
{
    public int GoodsID { get; set; }
    public int ShopID { get; set; }
    public int ItemID { get; set; }
    public int ItemCount { get; set; }
    public List<int> CurrencyList { get; set; } = [];
    public List<int> CurrencyCostList { get; set; } = [];

    [JsonIgnore] public Dictionary<int, int> CostList { get; set; } = [];

    public override int GetId()
    {
        return GoodsID;
    }

    public override void Loaded()
    {
        // Align with lunarcore: skip invalid goods entries
        if (ItemID == 0 || ShopID == 0) return;

        var len = Math.Min(CurrencyList.Count, CurrencyCostList.Count);
        for (var i = 0; i < len; i++) CostList[CurrencyList[i]] = CurrencyCostList[i];
    }

    public override void AfterAllDone()
    {
        if (ItemID == 0 || ShopID == 0) return;
        if (!GameData.ShopConfigData.TryGetValue(ShopID, out var shopConfig) || shopConfig == null) return;
        shopConfig.Goods.Add(this);
    }

    public Goods ToProto()
    {
        return new Goods
        {
            EndTime = uint.MaxValue,
            GoodsId = (uint)GoodsID,
            ItemId = (uint)ItemID
        };
    }
}
