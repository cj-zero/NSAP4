using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using OpenAuth.App.ProductModel;
using Spire.Doc;
using Spire.Doc.Documents;
using Spire.Doc.Fields;

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
        public static void AddImage(string imgfile)
        {
            //添加section和段落
            Section section = document.AddSection();
            Paragraph para = section.AddParagraph();

            //加载图片到System.Drawing.Image对象, 使用AppendPicture方法将图片插入到段落
            Image image = Image.FromFile(imgfile);
            ImageFormat format = image.RawFormat;
            using (MemoryStream ms = new MemoryStream())
            {
                if (format.Equals(ImageFormat.Jpeg))
                {
                    image.Save(ms, ImageFormat.Jpeg);
                }
                else if (format.Equals(ImageFormat.Png))
                {
                    image.Save(ms, ImageFormat.Png);
                }
                else if (format.Equals(ImageFormat.Bmp))
                {
                    image.Save(ms, ImageFormat.Bmp);
                }
                else if (format.Equals(ImageFormat.Gif))
                {
                    image.Save(ms, ImageFormat.Gif);
                }
                else if (format.Equals(ImageFormat.Icon))
                {
                    image.Save(ms, ImageFormat.Icon);
                }
                byte[] buffer = new byte[ms.Length];
                //Image.Save()会改变MemoryStream的Position，需要重新Seek到Begin
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);
                DocPicture picture = document.Sections[0].Tables[0].Rows[80].Cells[1].Paragraphs[0].AppendPicture(buffer);
                //设置文字环绕方式
                picture.TextWrappingStyle = TextWrappingStyle.Square;

                //指定图片位置
                picture.HorizontalPosition = 135.0f;
                picture.VerticalPosition = 50.0f;

                //设置图片大小
                picture.Width = 100;
                picture.Height = 200;
            }



        }
        public static void AddImageTechnical(string imgfile)
        {
            //添加section和段落
            Section section = document.AddSection();
            Paragraph para = section.AddParagraph();

            //加载图片到System.Drawing.Image对象, 使用AppendPicture方法将图片插入到段落
            Image image = Image.FromFile(imgfile);
            ImageFormat format = image.RawFormat;
            using (MemoryStream ms = new MemoryStream())
            {
                if (format.Equals(ImageFormat.Jpeg))
                {
                    image.Save(ms, ImageFormat.Jpeg);
                }
                else if (format.Equals(ImageFormat.Png))
                {
                    image.Save(ms, ImageFormat.Png);
                }
                else if (format.Equals(ImageFormat.Bmp))
                {
                    image.Save(ms, ImageFormat.Bmp);
                }
                else if (format.Equals(ImageFormat.Gif))
                {
                    image.Save(ms, ImageFormat.Gif);
                }
                else if (format.Equals(ImageFormat.Icon))
                {
                    image.Save(ms, ImageFormat.Icon);
                }
                byte[] buffer = new byte[ms.Length];
                //Image.Save()会改变MemoryStream的Position，需要重新Seek到Begin
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer, 0, buffer.Length);
                //DocPicture picture = document.Sections[0].Tables[0].Rows[0].Cells[0].Paragraphs[0].AppendPicture(buffer);
                DocPicture picture = document.Sections[0].Paragraphs[0].AppendPicture(buffer);
                //设置文字环绕方式
                picture.TextWrappingStyle = TextWrappingStyle.Square;

                //指定图片位置
                picture.HorizontalPosition = 135.0f;
                picture.VerticalPosition = 50.0f;

                //设置图片大小
                picture.Width = 100;
                picture.Height = 200;
                DocPicture picture1 = document.Sections[3].Tables[3].Rows[80].Cells[1].Paragraphs[0].AppendPicture(buffer);

                //设置文字环绕方式
                picture1.TextWrappingStyle = TextWrappingStyle.Square;

                //指定图片位置
                picture1.HorizontalPosition = 135.0f;
                picture1.VerticalPosition = 50.0f;

                //设置图片大小
                picture1.Width = 100;
                picture1.Height = 200;
            }



        }
    }
}
