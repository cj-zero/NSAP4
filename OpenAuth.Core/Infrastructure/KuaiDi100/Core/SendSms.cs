
using Utils;
using KuaiDi100.Common;
using KuaiDi100.Common.Request;
using KuaiDi100.Common.Request.Sms;

public class SendSms{


    public static string query(SendSmsReq query){
        
        var request = ObjectToDictionaryUtils.ObjectToMap(query);
        if(request == null){
            return null;
        }
        var result = HttpUtils.doPostForm(ApiInfoConstant.SEND_SMS_URL,request);
        return result;
    }
}   