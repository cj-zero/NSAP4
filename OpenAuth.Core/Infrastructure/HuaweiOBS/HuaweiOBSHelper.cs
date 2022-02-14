using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private readonly ObsClient _obsClient;

        public HuaweiOBSHelper()
        {
            _obsClient = new ObsClient(_ak, _sk, _endPoint);
        }

        #region 管理桶

        /// <summary>
        /// 获取桶列表
        /// </summary>
        /// <returns></returns>
        public IList<ObsBucket> GetBucketLists()
        {
            var response = _obsClient.ListBuckets(new ListBucketsRequest { IsQueryLocation = true });
            return response.Buckets;
        }

        /// <summary>
        /// 判断桶是否存在
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        public bool IsExistsBucket(string bucketName)
        {
            var exists = _obsClient.HeadBucket(new HeadBucketRequest { BucketName = bucketName });
            return exists;
        }

        /// <summary>
        /// 创建桶
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        public CreateBucketResponse CreateBucket(string bucketName)
        {
            try
            {
                if (IsExistsBucket(bucketName)) { throw new Exception("bucket is exists."); }

                var createBucketRequest = new CreateBucketRequest
                {
                    BucketName = bucketName,
                    Location = _location,
                    CannedAcl = CannedAclEnum.Private,
                    StorageClass = StorageClassEnum.Standard,
                };
                var response = _obsClient.CreateBucket(createBucketRequest);

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message ?? "", ex);
            }
        }

        /// <summary>
        /// 删除桶
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        public DeleteBucketResponse DeleteBucket(string bucketName)
        {
            try
            {
                //若桶中仍然有对象则不能删除
                var response = _obsClient.DeleteBucket(new DeleteBucketRequest { BucketName = bucketName });
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message ?? "", ex);
            }
        }

        #endregion

        #region 管理对象

        /// <summary>
        /// 判断对象是否存在
        /// </summary>
        /// <param name="bucketName">桶容器名称</param>
        /// <param name="objectKey">文件对象关键字</param>
        /// <returns></returns>
        public bool IsExistsObject(string objectKey, string? bucketName)
        {
            var response = _obsClient.HeadObject(new HeadObjectRequest { BucketName = bucketName ?? _bucketName, ObjectKey = objectKey, });
            return response;
        }

        /// <summary>
        /// 上传对象
        /// </summary>
        /// <param name="fileName">文件名(重复的文件名会覆盖原文件)</param>
        /// <param name="bucketName">桶容器名称</param>
        /// <param name="stream">文件流</param>
        /// <param name="objectKey">上传成功后返回的文件名</param>
        /// <returns></returns>
        public PutObjectResponse PutObject(string fileName, string? bucketName, Stream stream, out string objectKey)
        {
            try
            {
                if (!IsExistsBucket(bucketName ?? _bucketName)) { CreateBucket(bucketName ?? _bucketName); }
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = bucketName ?? _bucketName,
                    ObjectKey = fileName,
                    InputStream = stream,
                    CannedAcl = CannedAclEnum.PublicRead,
                };
                var response = _obsClient.PutObject(putObjectRequest);
                objectKey = putObjectRequest.ObjectKey;

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message ?? "", ex);
            }
        }

        /// <summary>
        /// 下载对象
        /// </summary>
        /// <param name="bucketName">桶容器名称</param>
        /// <param name="objectKey">文件对象关键字</param>
        /// <param name="filePath"></param>
        public void GetObject(string objectKey, string? bucketName, string filePath)
        {
            try
            {
                using (var response = _obsClient.GetObject(new GetObjectRequest { BucketName = bucketName ?? _bucketName, ObjectKey = objectKey }))
                {
                    response.WriteResponseStreamToFile(filePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message ?? "");
            }
        }

        /// <summary>
        /// 获取对象列表
        /// </summary>
        /// <param name="bucketName">桶容器名称</param>
        /// <returns></returns>
        public IList<ObsObject> GetObjectLists(string? bucketName)
        {
            var response = _obsClient.ListObjects(new ListObjectsRequest { BucketName = bucketName ?? _bucketName });
            return response.ObsObjects;
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="bucketName">桶容器名称</param>
        /// <param name="objectKey">文件对象关键字</param>
        /// <returns></returns>
        public bool DeleteObject(string objectKey, string? bucketName)
        {
            try
            {
                _obsClient.DeleteObject(new DeleteObjectRequest { BucketName = bucketName ?? _bucketName, ObjectKey = objectKey });
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message ?? "");
            }
        }

        /// <summary>
        /// 批量删除对象
        /// </summary>
        /// <param name="bucketName">桶容器名称</param>
        /// <param name="objectKeys">文件对象关键字</param>
        /// <returns></returns>
        public bool DeleteObjects(IEnumerable<string> objectKeys, string? bucketName)
        {
            try
            {
                var keyVersions = objectKeys.Select(x => new KeyVersion { Key = x }).ToList();
                _obsClient.DeleteObjects(new DeleteObjectsRequest { BucketName = bucketName ?? _bucketName, Objects = keyVersions });

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message ?? ex.Message ?? "");
            }
        }

        #endregion
    }
}
