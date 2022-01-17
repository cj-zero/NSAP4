using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OBS;
using OBS.Model;

namespace Infrastructure.HuaweiOBS
{
    public class HuaweiOBSHelper
    {

        //详细参考资料:https://support.huaweicloud.com/sdk-dotnet-devg-obs/obs_25_0101.html

        private readonly string _ak = "CKNDX1ETRGTJBZXBRMYH";
        private readonly string _sk = "gOSb1j7zxyDIoyaRYBmDtO3t4UMxmnYmfrPWSSUX";
        private readonly string _endPoint = "obs.cn-south-1.myhuaweicloud.com";
        private readonly string _bucketName = "erp4";
        private readonly string _location = "cn-south-1";

        private readonly ObsClient _client;

        public HuaweiOBSHelper()
        {
            _client = new ObsClient(_ak, _sk, new ObsConfig { Endpoint = _endPoint });
        }

        /// <summary>
        /// 创建客户端
        /// </summary>
        /// <returns></returns>
        private ObsClient GetObsClient()
        {
            var client = new ObsClient(_ak, _sk, new ObsConfig { Endpoint = _endPoint });
            return client;
        }

        /// <summary>
        /// 创建桶
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        private CreateBucketResponse CreateBucketResponse(ObsClient client, string bucketName, string location)
        {
            var bucketRequest = new CreateBucketRequest
            {
                BucketName = bucketName,
                Location = location,
                StorageClass = StorageClassEnum.Standard,
                CannedAcl = CannedAclEnum.Private
            };

            CreateBucketResponse response = client.CreateBucket(bucketRequest);
            return response;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        public PutObjectResponse PutObjectResponse(string fileName, Stream fileStream)
        {
            //创建客户端
            var client = _client;
            //判断桶容器是否存在
            if(!client.HeadBucket(new HeadBucketRequest { BucketName = _bucketName }))
            {
                //创建桶容器
                var bucketResponse = CreateBucketResponse(client, _bucketName, _location);
            }
            //创建上传文件对象(采用流式上传)
            var putObject = new PutObjectRequest
            {
                BucketName = _bucketName,
                ObjectKey = fileName,
                InputStream = fileStream,
                CannedAcl = CannedAclEnum.PublicRead
            };
            var response = client.PutObject(putObject);
            return response;
        }
    }
}
