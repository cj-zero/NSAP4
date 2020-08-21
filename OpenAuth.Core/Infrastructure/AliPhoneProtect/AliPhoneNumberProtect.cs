using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Http;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Dyplsapi.Model.V20170525;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
namespace Infrastructure
{
    /// <summary>
    ///阿里云隐私号码Api接口对接
    /// </summary>
    public class AliPhoneNumberProtect
    {
        private ILogger<AliPhoneNumberProtect> _logger;
        //public AliPhoneNumberProtect(ILogger<AliPhoneNumberProtect> logger)
        //{
        //    _logger = logger;
        //}
        //产品名称:云通信隐私保护产品,开发者无需替换
        const String product = "Dyplsapi";
        //产品域名,开发者无需替换
        const String domain = "dyplsapi.aliyuncs.com";

        // TODO 此处需要替换成开发者自己的AK(在阿里云访问控制台寻找)
        const String accessKeyId = "LTAIVJksYtIgE0Oc";
        const String accessKeySecret = "e5CoXXQ1uZGIlKrKk0JeOXdee7eSKW";

        // 号码池Key
        const String PoolKey = "FC100000103720732";

        public class PhoneInfo
        {
            /// <summary>
            /// 隐私号码
            /// </summary>
            public string SecretNo { get; set; }
            /// <summary>
            /// 绑定关系Id
            /// </summary>
            public string SubsId { get; set; }
        }

        /// <summary>
        /// 添加AXB号码的绑定关系
        /// </summary>
        /// <returns></returns>
        public static string bindAxb(string PhoneNoA, string PhoneNoB)
        {
            string SecretNo = string.Empty;
            //先从redis判断是否存在
            string key = PhoneNoA + "_" + PhoneNoB;
            string keyback = PhoneNoB + "_" + PhoneNoA;
            PhoneInfo value = RedisHelper.Get<PhoneInfo>(key) != null ? RedisHelper.Get<PhoneInfo>(key) : RedisHelper.Get<PhoneInfo>(keyback);
            if (value == null)
            {
                IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", accessKeyId, accessKeySecret);
                DefaultAcsClient client = new DefaultAcsClient(profile);
                CommonRequest request = new CommonRequest();
                request.Method = MethodType.POST;
                request.Domain = "dyplsapi.aliyuncs.com";
                request.Version = "2017-05-25";
                request.Action = "BindAxn";
                // request.Protocol = ProtocolType.HTTP;
                request.AddQueryParameters("PhoneNoA", PhoneNoA);
                request.AddQueryParameters("Expiration", DateTime.Now.AddMinutes(10).ToString("yyyy-MM-dd HH:mm:ss"));
                //request.AddQueryParameters("Expiration", DateTime.Now.AddDays(7).ToString("yyyy-MM-dd HH:mm:ss"));
                request.AddQueryParameters("PoolKey", PoolKey);
                request.AddQueryParameters("PhoneNoB", PhoneNoB);
                ////可选:是否需要录制音频-默认是false
                //request.AddQueryParameters("IsRecordingEnabled", false);
                try
                {
                    CommonResponse response = client.GetCommonResponse(request);
                    BindAxbResponse AxbData = JsonConvert.DeserializeObject<BindAxbResponse>(response.Data);
                    if ("OK".Equals(AxbData.Code))
                    {
                        SecretNo = AxbData.SecretBindDTO.SecretNo;
                        PhoneInfo phoneInfo = new PhoneInfo
                        {
                            SecretNo = AxbData.SecretBindDTO.SecretNo,
                            SubsId = AxbData.SecretBindDTO.SubsId
                        };
                        //设置7天缓存
                        RedisHelper.Set(key, phoneInfo, new TimeSpan(0, 0, 10, 0));
                    }
                    else
                    {
                        //_logger.LogError(response.ToString());
                    }
                }
                catch (ServerException e)
                {
                    //_logger.LogError(e, e.ToString());
                }
                catch (ClientException e)
                {
                    //_logger.LogError(e, e.ToString());
                }
            }
            else
            {
                SecretNo = value.SecretNo;
            }
            return SecretNo;
        }

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

        /// <summary>
        /// 解除绑定
        /// </summary>
        /// <param name="PhoneNoA"></param>
        /// <param name="PhoneNoB"></param>
        /// <returns></returns>
        public static bool Unbind(String PhoneNoA, String PhoneNoB)
        {
            //先从redis判断是否存在
            string key = PhoneNoA + "_" + PhoneNoB;
            string keyback = PhoneNoB + "_" + PhoneNoA;
            PhoneInfo phoneInfo = RedisHelper.Get<PhoneInfo>(key) != null ? RedisHelper.Get<PhoneInfo>(key) : RedisHelper.Get<PhoneInfo>(keyback);
            if (phoneInfo != null)
            {
                IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", accessKeyId, accessKeySecret);
                DefaultAcsClient client = new DefaultAcsClient(profile);
                CommonRequest request = new CommonRequest();
                request.Method = MethodType.POST;
                request.Domain = "dyplsapi.aliyuncs.com";
                request.Version = "2017-05-25";
                request.Action = "UnbindSubscription";
                // request.Protocol = ProtocolType.HTTP;
                request.AddQueryParameters("SubsId", phoneInfo.SubsId);
                request.AddQueryParameters("SecretNo", phoneInfo.SecretNo);
                request.AddQueryParameters("PoolKey", PoolKey);
                try
                {
                    CommonResponse response = client.GetCommonResponse(request);
                    BindAxbResponse AxbData = JsonConvert.DeserializeObject<BindAxbResponse>(response.Data);
                    if (!"OK".Equals(AxbData.Code))
                    {
                        //_logger.LogError(response.ToString());
                        return false;
                    }
                    //删除redis缓存
                    RedisHelper.Del(key);
                    RedisHelper.Del(keyback);
                    Console.WriteLine(System.Text.Encoding.Default.GetString(response.HttpResponse.Content));
                }
                catch (ServerException e)
                {
                    //_logger.LogError(e, e.ToString());
                    return false;
                }
                catch (ClientException e)
                {
                    //_logger.LogError(e, e.ToString());
                    return false;
                }
            }
            return true;
        }

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
