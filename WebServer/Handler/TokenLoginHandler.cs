using EggLink.DanhengServer.Database.Account;
using EggLink.DanhengServer.WebServer.Objects;
using Microsoft.AspNetCore.Mvc;
using static EggLink.DanhengServer.WebServer.Objects.LoginResJson;

namespace EggLink.DanhengServer.WebServer.Handler;

public class TokenLoginHandler
{
    private static readonly EggLink.DanhengServer.Util.Logger Logger = new("TokenLogin");

    public JsonResult Handle(string uid, string token)
    {
        var account = AccountData.GetAccountByUid(int.Parse(uid));
        var res = new LoginResJson();
        if (account == null || !account?.DispatchToken?.Equals(token) == true)
        {
            if (account == null)
            {
                Logger.Warn($"Verify failed: uid={uid}, reason=account_not_found");
            }
            else
            {
                var got = token[..Math.Min(token.Length, 8)];
                var expectToken = account.DispatchToken ?? "";
                var expect = expectToken[..Math.Min(expectToken.Length, 8)];
                Logger.Warn($"Verify failed: uid={uid}, reason=token_mismatch, got={got}, expect={expect}");
            }
            res.retcode = -201;
            res.message = "Game account cache information error";
        }
        else
        {
            res.message = "HyacineLover@StarRail.mail";
            res.data = new VerifyData(account!.Uid.ToString(), res.message, token);
            res.data.account.name = account.Uid.ToString();
        }

        return new JsonResult(res);
    }
}
