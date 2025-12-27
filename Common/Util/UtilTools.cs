using EggLink.DanhengServer.Enums;
using EggLink.DanhengServer.Enums.Task;
using System.Globalization;

namespace EggLink.DanhengServer.Util;

public static class UtilTools
{
    public static string GetCurrentLanguage()
    {
        var uiCulture = CultureInfo.CurrentUICulture;
        return uiCulture.Name switch
        {
            "zh-CN" => "CHS",
            "zh-TW" => "CHT",
            "ja-JP" => "JP",
            _ => "EN"
        };
    }

    public static bool CheckAnd(List<Func<bool>> actions, bool defaultValue = true)
    {
        if (actions.Count == 0) return defaultValue;

        foreach (var action in actions)
        {
            try
            {
                var returnValue = action.Invoke();
                if (!returnValue)
                {
                    return false;
                }
            }
            catch
            {
                // ignored
            }
        }

        return true;
    }

    public static bool CheckOr(List<Func<bool>> actions, bool defaultValue = false)
    {
        if (actions.Count == 0) return defaultValue;
        foreach (var action in actions)
        {
            try
            {
                var returnValue = action.Invoke();
                if (returnValue)
                {
                    return true;
                }
            }
            catch
            {
                // ignored
            }
        }
        return false;
    }

    public static bool CompareNumberByOperationEnum(int left, int right, CompareTypeEnum operation)
    {
        return operation switch
        {
            CompareTypeEnum.Greater => left > right,
            CompareTypeEnum.GreaterEqual => left >= right,
            CompareTypeEnum.NotEqual => left != right,
            CompareTypeEnum.Equal => left == right,
            CompareTypeEnum.LessEqual => left <= right,
            CompareTypeEnum.Less => left < right,
            _ => false
        };
    }
}