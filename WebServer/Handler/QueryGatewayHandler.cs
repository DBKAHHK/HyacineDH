using System.Text.RegularExpressions;
using EggLink.DanhengServer.Configuration;
using EggLink.DanhengServer.Data;
using EggLink.DanhengServer.Enums;
using EggLink.DanhengServer.Internationalization;
using EggLink.DanhengServer.Proto;
using EggLink.DanhengServer.Util;
using Google.Protobuf;

namespace EggLink.DanhengServer.WebServer.Handler;

internal partial class QueryGatewayHandler
{
    public static Logger Logger = new("GatewayServer");
    public string Data;

    public QueryGatewayHandler(string version)
    {
        var config = ConfigManager.Config;

        // build gateway proto
        var gateServer = new GateServer
        {
            RegionName = config.GameServer.GameServerId,
            Ip = config.GameServer.PublicAddress,
            Port = config.GameServer.Port,
            Msg = I18NManager.Translate("Server.Web.Maintain"),
            Unk1 = true,
            Unk2 = true,
            Unk3 = true,
            Unk4 = true,
            FDLIDHBPDJP = true,
            Unk5 = true,
            Unk6 = true,
            Unk7 = true,
            Unk8 = true
        };
        if (ConfigManager.Config.GameServer.UsePacketEncryption)
            gateServer.ClientSecretKey = Convert.ToBase64String(Crypto.ClientSecretKey!.GetBytes());

        // Auto separate CN/OS prefix
        var region = ConfigManager.Hotfix.Region;
        if (region == BaseRegionEnum.None) _ = Enum.TryParse(version[..2], out region);
        var baseUrl = region switch
        {
            BaseRegionEnum.CN => BaseUrl.CN,
            BaseRegionEnum.OS => BaseUrl.OS,
            _ => BaseUrl.OS
        };

        // Separate CN/OS hotfix by client
        var ver = VersionRegex().Replace(version, "");
        ConfigManager.Hotfix.HotfixData.TryGetValue(ver, out var urls);
        if (urls != null)
        {
            if (urls.AssetBundleUrl != "")
                gateServer.AssetBundleUrl = baseUrl + urls.AssetBundleUrl;
            if (urls.ExAssetBundleUrl != "")
                gateServer.AssetBundleUrl = baseUrl + urls.ExAssetBundleUrl;
            if (urls.ExResourceUrl != "")
                gateServer.ExResourceUrl = baseUrl + urls.ExResourceUrl;
            if (urls.LuaUrl != "")
                gateServer.LuaUrl = baseUrl + urls.LuaUrl;
            if (urls.IfixUrl != "")
                gateServer.IfixUrl = baseUrl + urls.IfixUrl;
        }

        if (!ResourceManager.IsLoaded) gateServer.Unk9 = true;
        Logger.Info("Client request: query_gateway");

        Data = Convert.ToBase64String(gateServer.ToByteArray());
    }

    [GeneratedRegex(@"BETA|PROD|CECREATION|Android|Win|iOS")]
    private static partial Regex VersionRegex();
}