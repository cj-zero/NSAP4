
using Utils;
using KuaiDi100.Common;
using KuaiDi100.Common.Request.Electronic.Image;

public class PrintImg{


    public static string query(PrintImgReq param){
        
        var request = ObjectToDictionaryUtils.ObjectToMap(param);
        
        if(request == null){
            return null;
        }
        var result = HttpUtils.doPostForm(ApiInfoConstant.ELECTRONIC_ORDER_PRINT_URL,request);
        return result;
    }
}   