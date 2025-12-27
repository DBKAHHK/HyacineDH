using EggLink.DanhengServer.Configuration;
using EggLink.DanhengServer.Internationalization;
using Newtonsoft.Json;

namespace EggLink.DanhengServer.Util;

public static class ConfigManager
{
    public static readonly Logger Logger = new("ConfigManager");
    private static readonly string ConfigFilePath = "Config.json";
    private static string HotfixFilePath => Config.Path.ConfigPath + "/Hotfix.json";
    public static ConfigContainer Config { get; private set; } = new();
    public static HotfixContainer Hotfix { get; private set; } = new();

    public static void LoadConfig()
    {
        LoadConfigData();
        LoadHotfixData();
    }

    private static void LoadConfigData()
    {
        var file = new FileInfo(ConfigFilePath);
        if (!file.Exists)
        {
            Config = new ConfigContainer
            {
                MuipServer =
                {
                    AdminKey = Guid.NewGuid().ToString()
                },
                ServerOption =
                {
                    Language = UtilTools.GetCurrentLanguage()
                }
            };

            Logger.Info("Current Language is " + Config.ServerOption.Language);
            Logger.Info("Muipserver Admin key: " + Config.MuipServer.AdminKey);
            SaveData(Config, ConfigFilePath);
        }

        using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var reader = new StreamReader(stream))
        {
            var json = reader.ReadToEnd();
            Config = JsonConvert.DeserializeObject<ConfigContainer>(json, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            })!;
        }

        SaveData(Config, ConfigFilePath);
    }

    private static void LoadHotfixData()
    {
        var file = new FileInfo(HotfixFilePath);

        // Generate all necessary versions
        var verList = new List<string>();
        var prefix = new List<string> { "CN", "OS" };
        foreach (var pre in prefix)
            if (GameConstants.GAME_VERSION[^1] == '5')
                for (var i = 1; i < 6; i++)
                    verList.Add(pre + GameConstants.GAME_VERSION + i);
            else
                verList.Add(pre + GameConstants.GAME_VERSION);

        if (!file.Exists)
        {
            Hotfix = new HotfixContainer();
            SaveData(Hotfix, HotfixFilePath);
            file.Refresh();
        }

        using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var reader = new StreamReader(stream))
        {
            var json = reader.ReadToEnd();
            Hotfix = JsonConvert.DeserializeObject<HotfixContainer>(json)!;
        }

        foreach (var version in verList)
            if (!Hotfix.HotfixData.TryGetValue(version, out _))
                Hotfix.HotfixData[version] = new DownloadUrlConfig();

        Logger.Info(I18NManager.Translate("Server.ServerInfo.CurrentVersion", GameConstants.GAME_VERSION));

        SaveData(Hotfix, HotfixFilePath);
    }

    private static void SaveData(object data, string path)
    {
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        using var writer = new StreamWriter(stream);
        writer.Write(json);
    }

    public static void InitDirectories()
    {
        foreach (var property in Config.Path.GetType().GetProperties())
        {
            var dir = property.GetValue(Config.Path)?.ToString();

            if (!string.IsNullOrEmpty(dir))
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
        }
    }
}
