namespace EggLink.DanhengServer.GameServer.Game.Scene.Component;

public interface IGameModifier
{
    public List<string> Modifiers { get; set; }
    public ValueTask AddModifier(string modifierName);
    public ValueTask RemoveModifier(string modifierName);
}