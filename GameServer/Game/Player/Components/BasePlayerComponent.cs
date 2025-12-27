namespace EggLink.DanhengServer.GameServer.Game.Player.Components;

public abstract class BasePlayerComponent(PlayerInstance player)
{
    protected PlayerInstance Player { get; } = player;
}