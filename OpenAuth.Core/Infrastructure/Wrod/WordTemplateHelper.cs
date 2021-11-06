
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace Infrastructure.Wrod
{


    public class WordTemplateHelper
    {
        /// <summary>
        /// NPOI操作word
        /// </summary>
        /// <param name="TemplatePath">模板路径</param>
        /// <param name="SavePath">保存路径</param>
        /// <param name="keywords">关键字集合</param>
        public static void WriteToPublicationOfResult(string TemplatePath, string SavePath, Dictionary<string, string> keywords)
        {




            //File.Copy(TemplatePath, SavePath, true);

            //FileStream fs = new FileStream(SavePath, FileMode.Open, FileAccess.Read);

            //XWPFDocument document = new XWPFDocument();

            //document.Write(fs);

            //fs.Close();

            //XWPFParagraph para = document.CreateParagraph();
            //ReplaceKeyWords((IList<XWPFParagraph>)para, keywords);
            //para.ReplaceText("zaosheng", "123");
            //InputStream iss= new FileInputStream(TemplatePath);
            try
            {
                FileStream fileStream = new FileStream(TemplatePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                //FileStream fs = File.OpenRead(TemplatePath);
                //FileStream fs = new FileStream(TemplatePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                XWPFDocument document = new XWPFDocument(fileStream);
                foreach (var table in document.Tables)
                {
                    foreach (var row in table.Rows)
                    {
                        foreach (var cell in row.GetTableCells())
                        {
                            ReplaceKeyWords(cell.Paragraphs, keywords);//替换表格中的关键字
                        }
                    }
                }
                ReplaceKeyWords(document.Paragraphs, keywords);//替换模板中非表格的关键字
                FileStream output = new FileStream(SavePath, FileMode.Create);
                document.Write(output);
                fileStream.Close();
                fileStream.Dispose();
                output.Close();
                output.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 遍历段落，替换关键字
        /// </summary>
        /// <param name="Paragraphs">段落</param>
        /// <param name="keywords">关键字集合</param>
        public static void ReplaceKeyWords(IList<XWPFParagraph> Paragraphs, Dictionary<string, string> keywords)
        {
            foreach (var item in keywords)
            {
                foreach (var para in Paragraphs)
                {
                    string oldtext = para.ParagraphText;
                    if (oldtext == "") continue;
                    string temptext = para.ParagraphText;
                    if (temptext.Contains("{$" + item.Key + "}")) temptext = temptext.Replace("{$" + item.Key + "}", item.Value);
                    para.ReplaceText(oldtext, temptext);
                }
            }

        }
        /// <summary>
        /// 格式化关键字集合
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="t">关键字集对象</param>
        /// <returns></returns>
        public static Dictionary<string, string> getProperties<T>(T t)
        {
            Dictionary<string, string> keywords = new Dictionary<string, string>();
            if (t == null)
            {
                return keywords;
            }
            System.Reflection.PropertyInfo[] properties = t.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            if (properties.Length <= 0)
            {
                return keywords;
            }
            foreach (System.Reflection.PropertyInfo item in properties)
            {
                string name = item.Name;
                object value = item.GetValue(t, null);
                if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
                {
                    keywords.Add(name, value.ToString());
                }
                else
                {
                    getProperties(value);
                }
            }
            return keywords;
        }
        /// <summary>
        /// Word 模板 替换
        /// <para>当前适用的字段模板形如：[=Name]，其中 Name 就是字段名</para>
        /// <para>返回 true 表示成功</para>
        /// </summary>
        /// <param name="tempPath">Word 文件 模板路径</param>
        /// <param name="newWordPath">生成的新 Word 文件的路径</param>
        /// <param name="textDic">文字字典集合</param>
        /// <returns></returns>
        //public static bool WordTemplateReplace(string tempPath, string newWordPath,
        //    Dictionary<string, string> textDic)
        //{
        //    try
        //    {
        //        var a = @"D:\Dev\OpenDemo\Word模板文件的替换并生成新的Word文件\WordTestDemo\WordFileDemo\bin\Debug\WordFiles\Templates\测试模板-0.docx";
        //        var doc = DocX.Load(tempPath);  // 加载 Word 模板文件

        //        #region 字段替换文字

        //        if (textDic != null && textDic.Count > 0)
        //        {
        //            foreach (var paragraph in doc.Paragraphs)   // 遍历当前 Word 文件中的所有（段落）段
        //            {
        //                foreach (var texts in textDic)
        //                {
        //                    try
        //                    {
        //                        paragraph.ReplaceText($"{texts.Key}", texts.Value);  // 替换段落中的文字
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        // 不处理
        //                        continue;
        //                    }
        //                }
        //            }

        //            foreach (var table in doc.Tables)   // 遍历当前 Word 文件中的所有表格
        //            {
        //                foreach (var row in table.Rows) // 遍历表格中的每一行
        //                {
        //                    foreach (var cell in row.Cells)     //遍历每一行中的每一列
        //                    {
        //                        foreach (var paragraph in cell.Paragraphs)  // 遍历当前表格里的所有（段落）段
        //                        {
        //                            foreach (var texts in textDic)
        //                            {
        //                                try
        //                                {
        //                                    paragraph.ReplaceText($"[={texts.Key}]", texts.Value);  // 替换段落中的文字
        //                                }
        //                                catch (Exception ex)
        //                                {
        //                                    // 不处理
        //                                    continue;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        #endregion



        //        doc.SaveAs(newWordPath);

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        // 不处理
        //        return false;
        //    }
        //}

    }

}

