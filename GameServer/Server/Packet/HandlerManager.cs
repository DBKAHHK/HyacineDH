using System.Reflection;

namespace EggLink.DanhengServer.GameServer.Server.Packet;

public static class HandlerManager
{
    private static readonly EggLink.DanhengServer.Util.Logger Logger = new("HandlerMgr");
    public static Dictionary<int, Handler> handlers = [];

    public static void Init()
    {
        handlers.Clear();

        var classes = Assembly.GetExecutingAssembly().GetTypes(); // Get all classes in the assembly
        Array.Sort(classes, (a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));

        foreach (var cls in classes)
        {
            var attribute = (Opcode?)Attribute.GetCustomAttribute(cls, typeof(Opcode));

            if (attribute == null) continue;
            if (!typeof(Handler).IsAssignableFrom(cls) || cls.IsAbstract) continue;
            if (attribute.CmdId == 0) continue;

            if (handlers.TryGetValue(attribute.CmdId, out var existing))
            {
                Logger.Warn($"重复的 CmdId 处理器注册: {attribute.CmdId}，已存在 {existing.GetType().FullName}，跳过 {cls.FullName}");
                continue;
            }

            var instance = Activator.CreateInstance(cls) as Handler;
            if (instance == null)
            {
                Logger.Warn($"无法创建 Handler 实例: {cls.FullName} (CmdId={attribute.CmdId})");
                continue;
            }

            handlers.Add(attribute.CmdId, instance);
        }
    }

    public static Handler? GetHandler(int cmdId)
    {
        return handlers.TryGetValue(cmdId, out var handler) ? handler : null;
    }
}
