using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database.Inventory;
using EggLink.DanhengServer.Enums.Item;
using EggLink.DanhengServer.Enums.Mission;
using EggLink.DanhengServer.GameServer.Game.Player;

namespace EggLink.DanhengServer.GameServer.Game.Shop;

public class ShopService(PlayerInstance player) : BasePlayerManager(player)
{
    public async ValueTask<List<ItemData>> BuyItem(int shopId, int goodsId, int count)
    {
        GameData.ShopConfigData.TryGetValue(shopId, out var shopConfig);
        if (shopConfig == null) return [];
        var goods = shopConfig.Goods.Find(g => g.GoodsID == goodsId);
        if (goods == null) return [];
        GameData.ItemConfigData.TryGetValue(goods.ItemID, out var itemConfig);
        if (itemConfig == null) return [];

        // Verify cost before removing, otherwise negative inventory may occur.
        foreach (var cost in goods.CostList)
        {
            var need = cost.Value * count;
            var have = Player.InventoryManager!.GetItem(cost.Key)?.Count ?? 0;
            if (have < need) return [];
        }

        foreach (var cost in goods.CostList) await Player.InventoryManager!.RemoveItem(cost.Key, cost.Value * count);
        var items = new List<ItemData>();
        if (itemConfig.ItemMainType == ItemMainTypeEnum.Equipment || itemConfig.ItemMainType == ItemMainTypeEnum.Relic)
        {
            var num = Math.Max(1, goods.ItemCount) * count;
            for (var i = 0; i < num; i++)
            {
                var item = await Player.InventoryManager!.AddItem(itemConfig.ID, 1, false);
                if (item != null) items.Add(item);
            }
        }
        else
        {
            var num = Math.Max(1, goods.ItemCount) * count;
            var item = await Player.InventoryManager!.AddItem(itemConfig.ID, num, false);
            if (item != null) items.Add(item);
        }

        await Player.MissionManager!.HandleFinishType(MissionFinishTypeEnum.BuyShopGoods, goods);

        return items;
    }
}
