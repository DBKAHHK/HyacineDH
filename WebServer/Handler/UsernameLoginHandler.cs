using EggLink.DanhengServer.Database.Account;
using EggLink.DanhengServer.Util;
using EggLink.DanhengServer.WebServer.Objects;
using Microsoft.AspNetCore.Mvc;
using static EggLink.DanhengServer.WebServer.Objects.LoginResJson;

namespace EggLink.DanhengServer.WebServer.Handler;

public class UsernameLoginHandler
{
    public JsonResult Handle(string account, string password, bool isCrypto)
    {
        LoginResJson res = new();
        account = account.Trim();
        var accountData = AccountData.GetAccountByUserName(account);

        if (accountData == null)
        {
            if (ConfigManager.Config.ServerOption.AutoCreateUser)
            {
                AccountHelper.CreateAccount(account, 0);
                accountData = AccountData.GetAccountByUserName(account);
            }
            else
            {
                return new JsonResult(new LoginResJson { message = "Account not found", retcode = -201 });
            }
        }

        if (accountData != null)
        {
            res.message = "HyacineLover@StarRail.mail";
            res.data = new VerifyData(accountData.Uid.ToString(), res.message, accountData.GenerateDispatchToken());
            res.data.account.name = accountData.Uid.ToString();
        }

        return new JsonResult(res);
    }
}
