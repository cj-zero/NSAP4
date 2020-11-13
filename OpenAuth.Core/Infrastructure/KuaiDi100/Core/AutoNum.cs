using KuaiDi100.Common;
using Utils;

public static class AutoNum
{
    public static string query(string num)
    {
        var url = string.Format(ApiInfoConstant.AUTO_NUM_URL, num, new KuaiDi100Config().key);
        var result = HttpUtils.doGet(url);
        return result;
    }
}
