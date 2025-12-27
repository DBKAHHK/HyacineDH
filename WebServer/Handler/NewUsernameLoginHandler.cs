using EggLink.DanhengServer.Database.Account;
using EggLink.DanhengServer.Util;
using EggLink.DanhengServer.WebServer.Objects;
using Microsoft.AspNetCore.Mvc;
using static EggLink.DanhengServer.WebServer.Objects.NewLoginResJson;

namespace EggLink.DanhengServer.WebServer.Handler;

public class NewUsernameLoginHandler
{
    public JsonResult Handle(string account, string password)
    {
        NewLoginResJson res = new();
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
                return new JsonResult(new NewLoginResJson { message = "Account not found", retcode = -201 });
            }
        }

        if (accountData != null)
        {
            res.message = "OK";
            res.data = new VerifyData(accountData.Uid.ToString(), accountData.Username + "@egglink.me",
                accountData.GenerateDispatchToken());
            res.data.user_info.account_name = accountData.Uid.ToString();
        }

        return new JsonResult(res);
    }
}
