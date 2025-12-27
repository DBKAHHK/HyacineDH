using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Server.Packet.Send.RechargeGift;

public class PacketGetRechargeGiftInfoScRsp : BasePacket
{
    public PacketGetRechargeGiftInfoScRsp() : base(CmdIds.GetRechargeGiftInfoScRsp)
    {
        var proto = new GetRechargeGiftInfoScRsp
        {
            RechargeBenefitList =
            {
                GameData.RechargeGiftConfigData.Values.Select(x => new RechargeGiftInfo
                {
                    GiftType = (uint)x.GiftType,
                    EndTime = uint.MaxValue,
                    GiftDataList =
                    {
                        x.GiftIDList.Select(h => new RechargeGiftData
                        {
                            Status = RechargeGiftStatus.Received,
                            Index = (uint)x.GiftIDList.IndexOf(h)
                        })
                    }
                })
            }
        };

        SetData(proto);
    }
}