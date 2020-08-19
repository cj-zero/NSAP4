using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Dyplsapi.Model.V20170525;
using System;

namespace Infrastructure
{
    class AliPhoneNumberProtect
    {
        //产品名称:云通信隐私保护产品,开发者无需替换
        const String product = "Dyplsapi";
        //产品域名,开发者无需替换
        const String domain = "dyplsapi.aliyuncs.com";

        // TODO 此处需要替换成开发者自己的AK(在阿里云访问控制台寻找)
        const String accessKeyId = "LTAIVJksYtIgE0Oc";
        const String accessKeySecret = "LTAIVJksYtIgE0Oc";

        /// <summary>
        /// 添加AXB号码的绑定关系
        /// </summary>
        /// <returns></returns>
        //public static BindAxbResponse bindAxb()
        //{
        //    IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", accessKeyId, accessKeySecret);
        //    DefaultProfile.AddEndpoint("cn-hangzhou", "cn-hangzhou", product, domain);
        //    IAcsClient client = new DefaultAcsClient(profile);
        //    //必填:号池Key
        //    BindAxbRequest request = new BindAxbRequest();
        //    //request.setPoolKey("FC123456");
        //    request.PhoneNoA = "15010101010";
        //    request.PhoneNoB = "15020202020";
        //    request.Expiration = "2017-09-18 17:00:00";
        //    //可选:是否需要录制音频-默认是false
        //    request.IsRecordingEnabled = false;
        //    //外部业务自定义ID属性
        //    request.OutId = "yourOutId";
        //    //hint 此处可能会抛出异常，注意catch
        //    BindAxbResponse response = null;
        //    try
        //    {
        //        response = client.GetAcsResponse(request);
        //    }
        //    catch (ServerException e)
        //    {

        //    }
        //    catch (ClientException e)
        //    {

        //    }
        //    return response;
        //}

        //public static BindAxnResponse bindAxn()
        //{
        //    IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", accessKeyId, accessKeySecret);
        //    DefaultProfile.AddEndpoint("cn-hangzhou", "cn-hangzhou", product, domain);
        //    IAcsClient client = new DefaultAcsClient(profile);
        //    BindAxnRequest request = new BindAxnRequest();
        //    request.PhoneNoA = "15010101010";
        //    request.PhoneNoB = "15020202020";
        //    request.Expiration = "2017-09-18 17:00:00";
        //    //可选:是否需要录制音频-默认是false
        //    request.IsRecordingEnabled = false;
        //    //外部业务自定义ID属性
        //    request.OutId = "yourOutId";
        //    //AXN的隐私号码X一共支持2种号码类型 NO_95 NO_170
        //    request.NoType = "NO_95";

        //    BindAxnResponse response = null;
        //    try
        //    {
        //        response = client.GetAcsResponse(request);
        //    }
        //    catch (ServerException e)
        //    {
        //    }
        //    catch (ClientException e)
        //    {
        //    }
        //    return response;
        //}

        //public static UnbindSubscriptionResponse unbind(String subId, String secretNo)
        //{
        //    IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", accessKeyId, accessKeySecret);
        //    DefaultProfile.AddEndpoint("cn-hangzhou", "cn-hangzhou", product, domain);
        //    IAcsClient client = new DefaultAcsClient(profile);
        //    UnbindSubscriptionRequest request = new UnbindSubscriptionRequest();
        //    request.SubsId = subId;
        //    request.SecretNo = secretNo;
        //    UnbindSubscriptionResponse response = null;
        //    try
        //    {
        //        response = client.GetAcsResponse(request);
        //    }
        //    catch (ServerException e)
        //    {
        //    }
        //    catch (ClientException e)
        //    {
        //    }
        //    return response;
        //}
        //public static UpdateSubscriptionResponse updateSubscription()
        //{
        //    IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", accessKeyId, accessKeySecret);
        //    DefaultProfile.AddEndpoint("cn-hangzhou", "cn-hangzhou", product, domain);
        //    IAcsClient client = new DefaultAcsClient(profile);
        //    UpdateSubscriptionRequest request = new UpdateSubscriptionRequest();

        //    //对应的产品类型
        //    request.ProductType = "AXB_170";
        //    //必填:绑定关系ID
        //    request.SubsId = "123456";
        //    //必填:绑定关系对应的X号码
        //    request.PhoneNoX = "170000000";
        //    //必填:操作类型指令
        //    request.OperateType = "updateNoA";
        //    //可选:需要修改的A号码
        //    request.PhoneNoA = "150000000";
        //    UpdateSubscriptionResponse response = null;
        //    try
        //    {
        //        response = client.GetAcsResponse(request);
        //        if (response.Code != null && response.Code == "OK")
        //        {
        //            //请求成功
        //        }
        //    }
        //    catch (ServerException e)
        //    {
        //    }
        //    catch (ClientException e)
        //    {
        //    }
        //    return response;
        //}

        //public static QuerySubscriptionDetailResponse querySubscriptionDetail()
        //{
        //    IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", accessKeyId, accessKeySecret);
        //    DefaultProfile.AddEndpoint("cn-hangzhou", "cn-hangzhou", product, domain);
        //    IAcsClient client = new DefaultAcsClient(profile);
        //    QuerySubscriptionDetailRequest request = new QuerySubscriptionDetailRequest();

        //    //对应的产品类型
        //    request.ProductType = "AXB_170";
        //    //必填:绑定关系ID
        //    request.SubsId = "123456";
        //    //必填:绑定关系对应的X号码
        //    request.PhoneNoX = "170000000";
        //    QuerySubscriptionDetailResponse response = null;
        //    try
        //    {
        //        response = client.GetAcsResponse(request);
        //        if (response.Code != null && response.Code == "OK")
        //        {
        //            //请求成功
        //        }
        //    }
        //    catch (ServerException e)
        //    {
        //    }
        //    catch (ClientException e)
        //    {
        //    }
        //    return response;
        //}

        //public static QueryRecordFileDownloadUrlResponse queryRecordFileDownloadUrl(String callId, String CallTime)
        //{
        //}
    }
}
