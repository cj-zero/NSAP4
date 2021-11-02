using Infrastructure;
using Infrastructure.Wrod;
using Newtonsoft.Json;
using OpenAuth.App.Interface;
using OpenAuth.App.ProductModel;
using OpenAuth.App.ProductModel.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.ProductModel;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App
{
    /// <summary>
    /// 商品选项服务
    /// </summary>
    public class ProductModelApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;
        ServiceBaseApp _serviceBaseApp;
        public ProductModelApp(ServiceBaseApp serviceBaseApp, IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
            _serviceBaseApp = serviceBaseApp;

        }
        /// <summary>
        /// 获取设备编码
        /// </summary>
        /// <returns></returns>
        public List<string> GetDeviceCodingList()
        {
            var productModelSelections = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete);
            return productModelSelections.Select(zw => zw.DeviceCoding).OrderBy(zw => zw).Distinct().ToList();
        }
        /// <summary>
        /// 获取产品系列
        /// </summary>
        /// <returns></returns>
        public List<string> GetProductTypeList()
        {
            var productModelSelections = UnitWork.Find<ProductModelType>(u => !u.IsDelete);
            return productModelSelections.Select(zw => zw.Name).OrderBy(zw => zw).Distinct().ToList();
        }
        /// <summary>
        /// 获取电流等级
        /// </summary>
        /// <returns></returns>
        public List<string> GetVoltageList()
        {
            var productModelSelections = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete);
            return productModelSelections.Select(zw => zw.Voltage).Distinct().OrderBy(zw => zw).ToList();
        }
        /// <summary>
        /// 获取电压等级
        /// </summary>
        /// <returns></returns>
        public List<string> GetCurrentList()
        {
            var productModelSelections = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete);
            return productModelSelections.Select(zw => zw.Current).Distinct().OrderBy(zw => zw).ToList();
        }
        /// <summary>
        /// 获取通道数量
        /// </summary>
        /// <returns></returns>
        public List<int> GetChannelList()
        {
            var productModelSelections = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete);
            return productModelSelections.Select(zw => zw.ChannelNumber).Distinct().OrderBy(zw => zw).ToList();
        }
        /// <summary>
        /// 获取功率
        /// </summary>
        /// <returns></returns>
        public List<string> GetTotalPowerList()
        {
            var productModelSelections = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete);
            return productModelSelections.Select(zw => zw.TotalPower).Distinct().OrderBy(zw => zw).ToList();
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public List<ProductModelInfo> GetProductModelGrid(ProductModelReq queryModel, out int rowcount)
        {
            Expression<Func<ProductModelSelection, bool>> exps = t => true;
            exps = exps.And(t => !t.IsDelete);
            if (!string.IsNullOrWhiteSpace(queryModel.ProductType))
            {
                exps = exps.And(t => t.ProductType == queryModel.ProductType);
            }
            if (!string.IsNullOrWhiteSpace(queryModel.DeviceCoding))
            {
                exps = exps.And(t => t.ProductType == queryModel.DeviceCoding);
            }
            if (!string.IsNullOrWhiteSpace(queryModel.Voltage))
            {
                exps = exps.And(t => t.Voltage == queryModel.Voltage);
            }
            if (!string.IsNullOrWhiteSpace(queryModel.Current))
            {
                exps = exps.And(t => t.Current == queryModel.Current);
            }
            if (!string.IsNullOrWhiteSpace(queryModel.TotalPower))
            {
                exps = exps.And(t => t.TotalPower == queryModel.TotalPower);
            }
            if (queryModel.ChannelNumber > 0)
            {
                exps = exps.And(t => t.ChannelNumber == queryModel.ChannelNumber);
            }
            var productModelSelectionList = UnitWork.Find(queryModel.page, queryModel.limit, "", exps);
            var list = productModelSelectionList.MapToList<ProductModelSelection>();
            rowcount = UnitWork.GetCount(exps);
            var result = new List<ProductModelInfo>();
            foreach (var item in list)
            {
                var scon = new ProductModelInfo();
                scon.ChannelNumber = item.ChannelNumber;
                scon.Current = item.Current;
                scon.CurrentAccurack = item.CurrentAccurack;
                scon.DeviceCoding = item.DeviceCoding;
                scon.Id = item.Id;
                scon.Voltage = item.Voltage;
                scon.TotalPower = item.TotalPower;
                scon.Size = item.Size;
                scon.Weight = item.Weight;
                scon.UnitPrice = item.UnitPrice;
            }
            return result;
        }
        /// <summary>
        /// 获取产品手册
        /// </summary>
        /// <param name="ProductModelCategoryId"></param>
        /// <returns></returns>
        public List<string> GetProductImg(int ProductModelCategoryId)
        {
            List<string> imgs = new List<string>();
            var productModelCategory = UnitWork.Find<ProductModelCategory>(u => !u.IsDelete && u.Id == ProductModelCategoryId)?.FirstOrDefault();
            if (productModelCategory != null && !string.IsNullOrWhiteSpace(productModelCategory.Image))
            {
                imgs = JsonConvert.DeserializeObject<List<string>>(productModelCategory.Image);
            }
            return imgs;
        }
        /// <summary>
        /// 获取产品手册
        /// </summary>
        /// <param name="ProductModelCategoryId"></param>
        /// <returns></returns>
        public List<string> GetCaseImage(int ProductModelCategoryId)
        {
            List<string> imgs = new List<string>();
            var productModelCategory = UnitWork.Find<ProductModelCategory>(u => !u.IsDelete && u.Id == ProductModelCategoryId)?.FirstOrDefault();
            if (productModelCategory != null && !string.IsNullOrWhiteSpace(productModelCategory.CaseImage))
            {
                imgs = JsonConvert.DeserializeObject<List<string>>(productModelCategory.CaseImage);
            }
            return imgs;
        }
        /// <summary>
        /// 导出规格说明书
        /// </summary>
        public void ExportProductSpecsDoc(int id)
        {
            var productModelSelection = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete && u.Id == id).FirstOrDefault();
            if (productModelSelection != null)
            {
                var productModelCategory = UnitWork.Find<ProductModelCategory>(u => !u.IsDelete && u.Id == productModelSelection.ProductModelCategoryId).FirstOrDefault();
                var productModelSelectionInfo = UnitWork.Find<ProductModelSelectionInfo>(u => !u.IsDelete && u.ProductModelSelectionId == productModelSelection.Id).FirstOrDefault();
                object templatePath = "";
                List<WordMarkModel> wordModels = new List<WordMarkModel>();
                string filePath = "";
                object[] oBookMark = new object[20];
                WordHandler.DOCTemplateConvert(templatePath, filePath, wordModels, oBookMark);
            }
        }
    }
}
