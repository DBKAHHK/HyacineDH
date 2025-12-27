using EggLink.DanhengServer.GameServer.Server.Packet.Send.EraFlipper;
using EggLink.DanhengServer.Proto;

namespace EggLink.DanhengServer.GameServer.Game.Scene.Component;

public class EraFlipperSceneComponent(SceneInstance scene) : BaseSceneComponent(scene)
{
    public int CurRegionId { get; set; }
    public int RegionState { get; set; }

    public override async ValueTask Initialize()
    {
        CurRegionId = SceneInst.Player.SceneData!.EraFlipperData.CurRegionId;
        if (CurRegionId != 0)
        {
            RegionState = SceneInst.Player.SceneData!.EraFlipperData.RegionState.GetValueOrDefault(CurRegionId, 0);

            await SceneInst.Player.SendPacket(
                new PacketEraFlipperDataChangeScNotify(SceneInst.FloorId, CurRegionId, RegionState));
        }
    }

    public void EnterEraFlipperRegion(int regionId, int state)
    {
        CurRegionId = regionId;
        RegionState = state;

        SceneInst.Player.SceneData!.EraFlipperData.CurRegionId = regionId;
        SceneInst.Player.SceneData!.EraFlipperData.RegionState[CurRegionId] = state;
    }

    public void LeaveFlipperRegion()
    {
        CurRegionId = 0;
        RegionState = 0;

        SceneInst.Player.SceneData!.EraFlipperData.CurRegionId = 0;
    }

    public void ChangeEraFlipperState(int state)
    {
        if (CurRegionId == 0) return;
        RegionState = state;

        // save
        SceneInst.Player.SceneData!.EraFlipperData.RegionState[CurRegionId] = state;
    }

    public void ChangeEraFlipperStates(List<EraFlipperData> dataList)
    {
        foreach (var data in dataList)
        {
            SceneInst.Player.SceneData!.EraFlipperData.RegionState[(int)data.EraFlipperRegionId] = (int)data.State;
            if (data.EraFlipperRegionId == CurRegionId)
            {
                RegionState = (int)data.State;
            }
        }
    }
}