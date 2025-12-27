using System.Globalization;
using System.IO.Compression;
using EggLink.DanhengServer.Command.Command;
using EggLink.DanhengServer.Configuration;
using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Database;
using EggLink.DanhengServer.Database.Avatar;
using EggLink.DanhengServer.Enums;
using EggLink.DanhengServer.Enums.Rogue;
using EggLink.DanhengServer.GameServer.Command;
using EggLink.DanhengServer.GameServer.Plugin;
using EggLink.DanhengServer.GameServer.Server;
using EggLink.DanhengServer.GameServer.Server.Packet;
using EggLink.DanhengServer.Internationalization;
using EggLink.DanhengServer.Kcp;
using EggLink.DanhengServer.Program.Generator;
using EggLink.DanhengServer.Program.Handbook;
using EggLink.DanhengServer.Util;
using EggLink.DanhengServer.WebServer;
using EggLink.DanhengServer.WebServer.Server;

namespace EggLink.DanhengServer.Program.Program;

public class EntryPoint
{
    private static readonly Logger Logger = new("HyacineDH"); // Changed from "Program" to "HyacineDH"
    public static readonly DatabaseHelper DatabaseHelper = new();
    public static readonly Listener Listener = new();
    public static readonly CommandManager CommandManager = new();

    public static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            Logger.Info(I18NManager.Translate("Server.ServerInfo.Shutdown"));
            PerformCleanup();
        };
        AppDomain.CurrentDomain.UnhandledException += (obj, arg) =>
        {
            Logger.Error(I18NManager.Translate("Server.ServerInfo.UnhandledException", obj.GetType().Name),
                (Exception)arg.ExceptionObject);
            Logger.Info(I18NManager.Translate("Server.ServerInfo.Shutdown"));
            PerformCleanup();
            Environment.Exit(1);
        };

        Console.CancelKeyPress += (_, eventArgs) =>
        {
            Logger.Info(I18NManager.Translate("Server.ServerInfo.CancelKeyPressed"));
            eventArgs.Cancel = true;
            Environment.Exit(0);
        };
        var time = DateTime.Now;

        // pack the old log
        var logDirectory = new DirectoryInfo(GetConfig().Path.LogPath);
        if (logDirectory.Exists)
        {
            List<string> packed = [];
            foreach (var oldFile in logDirectory.GetFiles().ToArray())
            {
                if (!oldFile.Name.EndsWith(".log")) continue;
                if (oldFile.Name.EndsWith("-debug.log")) continue;
                if (packed.Contains(oldFile.Name)) continue;

                var fileName = oldFile.Name.Replace(".log", "");
                var debugFileName = fileName + "-debug";
                var oldDebugFile = logDirectory.GetFiles(debugFileName + ".log").FirstOrDefault();

                if (oldFile.Exists)
                {
                    var zipFileName = fileName + ".zip";
                    var zipFile = new FileInfo(GetConfig().Path.LogPath + $"/{zipFileName}");
                    if (zipFile.Exists) zipFile.Delete();
                    using (var zip = ZipFile.Open(zipFile.FullName, ZipArchiveMode.Create))
                    {
                        zip.CreateEntryFromFile(oldFile.FullName, oldFile.Name);
                        if (oldDebugFile is { Exists: true })
                            zip.CreateEntryFromFile(oldDebugFile.FullName, oldDebugFile.Name);
                    }

                    oldFile.Delete();
                    oldDebugFile?.Delete();
                    packed.Add(oldFile.Name);
                    packed.Add(oldDebugFile?.Name ?? "");
                }
            }
        }

        // Initialize the logfile
        var counter = 0;
        FileInfo file;
        FileInfo zi;
        while (true)
        {
            file = new FileInfo(GetConfig().Path.LogPath + $"/{DateTime.Now:yyyy-MM-dd}-{++counter}.log");
            zi = new FileInfo(GetConfig().Path.LogPath + $"/{DateTime.Now:yyyy-MM-dd}-{counter}.zip");
            if (file is not { Exists: false, Directory: not null }) continue;
            if (zi is not { Exists: false, Directory: not null }) continue;
            file.Directory.Create();
            break;
        }

        var debugFile = new FileInfo(GetConfig().Path.LogPath + $"/{DateTime.Now:yyyy-MM-dd}-{counter}-debug.log");

        Logger.SetLogFile(file);
        Logger.SetDebugLogFile(debugFile);
        // Starting the server
        Logger.Info(I18NManager.Translate("Server.ServerInfo.StartingServer"));

        // Load the config
        Logger.Info(I18NManager.Translate("Server.ServerInfo.LoadingItem", I18NManager.Translate("Word.Config")));
        try
        {
            ConfigManager.LoadConfig();
        }
        catch (Exception e)
        {
            Logger.Error(
                I18NManager.Translate("Server.ServerInfo.FailedToLoadItem", I18NManager.Translate("Word.Config")), e);
            Console.ReadLine();
            return;
        }

        // Load the language
        Logger.Info(I18NManager.Translate("Server.ServerInfo.LoadingItem", I18NManager.Translate("Word.Language")));
        try
        {
            I18NManager.LoadLanguage();
        }
        catch (Exception e)
        {
            Logger.Error(
                I18NManager.Translate("Server.ServerInfo.FailedToLoadItem", I18NManager.Translate("Word.Language")), e);
            Console.ReadLine();
            return;
        }

        // Initialize the database
        try
        {
            _ = Task.Run(DatabaseHelper.Initialize); // do not wait

            while (!DatabaseHelper.LoadAccount) Thread.Sleep(100);

            Logger.Info(I18NManager.Translate("Server.ServerInfo.LoadedItem",
                I18NManager.Translate("Word.DatabaseAccount")));
        }
        catch (Exception e)
        {
            Logger.Error(
                I18NManager.Translate("Server.ServerInfo.FailedToLoadItem", I18NManager.Translate("Word.Database")), e);
            Console.ReadLine();
            return;
        }

        HandlerManager.Init();
        if (ConfigManager.Config.GameServer.UsePacketEncryption)
        {
            Crypto.ClientSecretKey = Crypto.InitEc2b();
            if (Crypto.ClientSecretKey == null) ConfigManager.Config.GameServer.UsePacketEncryption = false;
        }

        Logger.Warn(I18NManager.Translate("Server.ServerInfo.WaitForAllDone"));
        WebProgram.Main([], GetConfig().HttpServer.Port, GetConfig().HttpServer.GetBindDisplayAddress());
        Logger.Info(I18NManager.Translate("Server.ServerInfo.ServerRunning", I18NManager.Translate("Word.Dispatch"),
            GetConfig().HttpServer.GetDisplayAddress()));

        DanhengListener.BaseConnection = typeof(Connection);
        DanhengListener.StartListener();

        GenerateLogMap();

        // Load the game data
        try
        {
            var isCache = false;
            if (File.Exists(ResourceCache.CachePath))
                if (ConfigManager.Config.ServerOption.UseCache)
                {
                    Logger.Info(I18NManager.Translate("Server.ServerInfo.LoadingItem",
                        I18NManager.Translate("Word.Cache")));
                    isCache = ResourceCache.LoadCache();

                    // Clear all game data if cache loading fails
                    if (!isCache)
                    {
                        ResourceCache.ClearGameData();
                        Logger.Warn(I18NManager.Translate("Server.ServerInfo.CacheLoadFailed"));
                    }
                }
                else
                {
                    File.Delete(ResourceCache.CachePath);
                    Logger.Warn(I18NManager.Translate("Server.ServerInfo.CacheLoadSkip"));
                }

            if (!isCache)
            {
                Logger.Info(I18NManager.Translate("Server.ServerInfo.LoadingItem",
                    I18NManager.Translate("Word.GameData")));
                ResourceManager.LoadGameData();

                // Async process cache saving
                if (ConfigManager.Config.ServerOption.UseCache && ResourceCache.IsComplete)
                {
                    Logger.Warn(I18NManager.Translate("Server.ServerInfo.WaitingItem",
                        I18NManager.Translate("Word.Cache")));
                    _ = ResourceCache.SaveCache();
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error(
                I18NManager.Translate("Server.ServerInfo.FailedToLoadItem", I18NManager.Translate("Word.GameData")), e);
            Console.ReadLine();
            return;
        }

        // check option
        if (args.Contains("-generate-tourn"))
        {
            TournRoomGenerator.GenerateFile("RogueTournRoom.json");
            return;
        }

        // Register the command handlers
        try
        {
            CommandManager.RegisterCommands();
        }
        catch (Exception e)
        {
            Logger.Error(
                I18NManager.Translate("Server.ServerInfo.FailedToInitializeItem",
                    I18NManager.Translate("Word.Command")), e);
            Console.ReadLine();
            return;
        }

        // Load the plugins
        Logger.Info(I18NManager.Translate("Server.ServerInfo.LoadingItem", I18NManager.Translate("Word.Plugin")));
        try
        {
            PluginManager.LoadPlugins();
        }
        catch (Exception e)
        {
            Logger.Error(
                I18NManager.Translate("Server.ServerInfo.FailedToLoadItem", I18NManager.Translate("Word.Plugin")), e);
            Console.ReadLine();
            return;
        }

        CommandExecutor.OnRunCommand += (sender, e) => { CommandManager.HandleCommand(e, sender); };

        MuipManager.OnExecuteCommand += CommandManager.HandleCommand;
        MuipManager.OnGetServerInformation += x =>
        {
            foreach (var con in DanhengListener.Connections.Values)
                if ((con as Connection)?.Player != null)
                    x.Add((con as Connection)!.Player!.Uid, (con as Connection)!.Player!.Data);
        };
        MuipManager.OnGetPlayerStatus += (int uid, out PlayerStatusEnum status, out PlayerSubStatusEnum subStatus) =>
        {
            subStatus = PlayerSubStatusEnum.None;
            foreach (var con in DanhengListener.Connections.Values)
                if ((con as Connection)!.Player != null && (con as Connection)!.Player!.Uid == uid)
                {
                    if ((con as Connection)!.Player!.RogueManager?.GetRogueInstance() != null)
                    {
                        status =
                            (con as Connection)!.Player!.ChessRogueManager?.RogueInstance?.AreaExcel
                                .RogueVersionId switch
                                {
                                    RogueSubModeEnum.ChessRogue => PlayerStatusEnum.ChessRogueNous,
                                    RogueSubModeEnum.ChessRogueNous => PlayerStatusEnum.ChessRogue,
                                    _ => PlayerStatusEnum.Rogue
                                };
                    }
                    else if ((con as Connection)!.Player!.ChallengeManager?.ChallengeInstance != null)
                    {
                        status = PlayerStatusEnum.Challenge;
                        if ((con as Connection)!.Player?.ChallengeManager?.ChallengeInstance?.Excel.StoryExcel != null)
                            status = PlayerStatusEnum.ChallengeStory;
                        else if ((con as Connection)!.Player?.ChallengeManager?.ChallengeInstance?.Excel.BossExcel !=
                                 null)
                            status = PlayerStatusEnum.ChallengeBoss;
                    }
                    else if ((con as Connection)!.Player!.RaidManager?.RaidData.CurRaidId != 0)
                    {
                        status = PlayerStatusEnum.Raid;
                    }
                    else if ((con as Connection)!.Player!.StoryLineManager?.StoryLineData.CurStoryLineId != 0)
                    {
                        status = PlayerStatusEnum.StoryLine;
                    }
                    else
                    {
                        status = PlayerStatusEnum.Explore;
                    }

                    if ((con as Connection)!.Player!.BattleInstance != null) subStatus = PlayerSubStatusEnum.Battle;

                    return;
                }

            status = PlayerStatusEnum.Offline;
        };

        // generate the handbook
        new Task(HandbookGenerator.GenerateAll).Start();

        if (!DatabaseHelper.LoadAllData)
        {
            Logger.Warn(I18NManager.Translate("Server.ServerInfo.WaitForAllDone"));
            var t = Task.Run(() =>
            {
                while (!DatabaseHelper.LoadAllData) // wait for all data to be loaded
                    Thread.Sleep(100);
            });

            await t.WaitAsync(new CancellationToken());

            Logger.Info(I18NManager.Translate("Server.ServerInfo.LoadedItem", I18NManager.Translate("Word.Database")));
        }

        // check if the database is up to date
        var updated = false;
        foreach (var avatarData in DatabaseHelper.GetAllInstanceFromMap<AvatarData>()!)
        {
            if (avatarData.DatabaseVersion == GameConstants.AvatarDbVersion) continue;

            foreach (var avatar in avatarData.Avatars)
            {
                var formalAvatar = new FormalAvatarInfo
                {
                    BaseAvatarId = avatar.AvatarId,
                    AvatarId = avatar.PathId == 0 ? avatar.AvatarId : avatar.PathId,
                    CurrentHp = avatar.CurrentHp,
                    CurrentSp = avatar.CurrentSp,
                    Exp = avatar.Exp,
                    ExtraLineupHp = avatar.ExtraLineupHp,
                    ExtraLineupSp = avatar.ExtraLineupSp,
                    IsMarked = avatar.IsMarked,
                    Level = avatar.Level,
                    Promotion = avatar.Promotion,
                    PathInfos = []
                };

                foreach (var info in avatar.PathInfoes)
                {
                    if (info.Value.PathId == 0)
                        info.Value.PathId = avatar.AvatarId;
                    formalAvatar.PathInfos.Add(info.Value.PathId, new PathInfo(info.Value.PathId)
                    {
                        PathId = info.Value.PathId,
                        EquipId = info.Value.EquipId,
                        Rank = info.Value.Rank,
                        Relic = info.Value.Relic,
                        Skin = info.Value.Skin,
                        EnhanceInfos =
                        {
                            {
                                0, new EnhanceInfo(0)
                                {
                                    SkillTree = avatar.SkillTreeExtra.GetValueOrDefault(info.Value.PathId) ?? []
                                }
                            }
                        }
                    });
                }

                avatarData.FormalAvatars.Add(formalAvatar);
            }

            avatarData.DatabaseVersion = "20250430";
            updated = true;
            DatabaseHelper.ToSaveUidList.Add(avatarData.Uid);
        }

        if (updated)
        {
            Logger.Info(I18NManager.Translate("Server.ServerInfo.UpdatedItem",
                I18NManager.Translate("Word.Database")));

            // Persist database changes and exit; avoid blocking on Console.ReadLine() during normal server startup.
            DatabaseHelper.SaveDatabase();
            if (args.Contains("--pause-on-update")) Console.ReadLine();
            Environment.Exit(0);
        }

        if (args.Contains("--upgrade-database")) DatabaseHelper.UpgradeDatabase();

        if (args.Contains("--move")) DatabaseHelper.MoveFromSqlite();

        var elapsed = DateTime.Now - time;
        Logger.Info(I18NManager.Translate("Server.ServerInfo.ServerStarted",
            Math.Round(elapsed.TotalSeconds, 2).ToString(CultureInfo.InvariantCulture)));

        if (GetConfig().ServerOption.EnableMission)
            Logger.Warn(I18NManager.Translate("Server.ServerInfo.MissionEnabled"));

        ResourceManager.IsLoaded = true;

        CommandManager.Start();
    }

    public static ConfigContainer GetConfig()
    {
        return ConfigManager.Config;
    }

    private static void PerformCleanup()
    {
        PluginManager.UnloadPlugins();
        DanhengListener.Connections.Values.ToList().ForEach(x => x.Stop());

        DatabaseHelper.SaveThread?.Interrupt();
        DatabaseHelper.SaveDatabase();
    }

    private static void GenerateLogMap()
    {
        // get opcode from CmdIds
        var opcodes = typeof(CmdIds).GetFields().Where(x => x.FieldType == typeof(int)).ToList();
        foreach (var opcode in opcodes)
        {
            var name = opcode.Name;
            var value = (int)opcode.GetValue(null)!;
            DanhengConnection.LogMap.TryAdd(value, name);
        }
    }
}
