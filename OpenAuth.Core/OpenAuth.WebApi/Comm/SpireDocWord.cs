using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using OpenAuth.App.ProductModel;
using Spire.Doc;

namespace OpenAuth.WebApi.Comm
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
}
