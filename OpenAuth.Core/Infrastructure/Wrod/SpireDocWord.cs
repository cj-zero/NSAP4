using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Spire.Doc;

namespace Infrastructure.Wrod
{

    public static class SpireDocWord
    {
        static Document document = new Document();
        /// <summary>
        /// 获取模板
        /// </summary>
        public static void GetDocument(string templatesPath)
        {
            try
            {
                string wordTemplatePath = templatesPath;
                document.LoadFromFile(wordTemplatePath);
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message;
                throw;
            }
        }
        public static void CreateNewWord(string docxName)
        {
            document.SaveToFile(docxName, FileFormat.Docx);
        }
        /// <summary>
        /// 替换模板
        /// </summary>
        /// <param name="purchaseContract"></param>
        public static void ReplaseTemplateWord(ExportBase purchaseContract)
        {
            try
            {
                foreach (System.Reflection.PropertyInfo p in purchaseContract.GetType().GetProperties())
                {
                    Console.WriteLine("Name:{0} Value:{1}", p.Name, p.GetValue(purchaseContract));
                    document.Replace(p.Name, p.GetValue(purchaseContract).ToString(), false, true);
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                throw;
            }
        }

    }

    /// <summary>
    /// 导出基类
    /// </summary>
    public class ExportBase
    {
        /// <summary>
        /// 根据属性获取值
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string GetValue(string propertyName)
        {
            string value = "";
            try
            {
                if (!string.IsNullOrEmpty(propertyName))
                {
                    var objectValue = this.GetType().GetProperty(propertyName).GetValue(this, null);
                    if (objectValue != null)
                    {
                        value = objectValue.ToString();
                    }
                }
            }
            catch (Exception)
            {
            }
            return value;
        }
        /// <summary>
        /// 根据属性获取描述值
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string GetDescription(string propertyName)
        {
            try
            {
                PropertyInfo item = this.GetType().GetProperty(propertyName);
                string des = ((DescriptionAttribute)Attribute.GetCustomAttribute(item, typeof(DescriptionAttribute))).Description;// 属性值
                return des;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
