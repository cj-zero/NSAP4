using NetOffice.WordApi;
using NetOffice.WordApi.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Range = NetOffice.WordApi.Range;

namespace Infrastructure.Wrod
{
    public class WordHandler
    {

        public static bool DOCTemplateConvert(string templatePath, string filePath, List<WordModel> wordModels)
        {
            bool result = false;
            string extension = Path.GetExtension(filePath).ToLower();
            int endIndex = filePath.LastIndexOf(extension);
            string targetPath = string.Format("{0}.docx", filePath.Substring(0, endIndex));

            Application wordApplication = null;
            Document wordDocument = null;

            object paramMissing = Type.Missing;
            object paramFalse = false;
            object paramTemplatePath = templatePath;
            object paramTrue = true;
            object paramDocType = WdDocumentType.wdTypeDocument;
            object noThing = Missing.Value;
            object paramTargetPath = targetPath;
            object doNotSaveChanges = WdSaveOptions.wdDoNotSaveChanges;

            try
            {
                wordApplication = new Application();
                wordApplication.Visible = false;
                wordDocument = wordApplication.Documents.Add(paramTemplatePath, paramFalse, paramDocType, paramTrue);

                foreach (var item in wordModels)
                {
                    if (item.MarkPosition == 1)
                        wordApplication.ActiveWindow.View.SeekView = WdSeekView.wdSeekCurrentPageHeader;
                    if (item.MarkPosition == 2)
                        wordApplication.ActiveWindow.View.SeekView = WdSeekView.wdSeekCurrentPageFooter;

                    Table _headTable = null;
                    if (item.MarkPosition == 1 || item.MarkPosition == 2)
                    {
                        _headTable = wordApplication.Selection.HeaderFooter.Range.Tables[item.TableMark];
                    }
                    else
                    {
                        _headTable = wordDocument.Tables[item.TableMark];
                    }
                    if (_headTable.Rows.Count < item.XCellMark)
                        _headTable.Rows.Add();
                    Range _headRange = _headTable.Cell(item.XCellMark, item.YCellMark).Range;
                    switch (item.ValueType)
                    {
                        case 0:
                            _headRange.Text = item.ValueData.ToString();
                            break;
                        case 1:
                            _headRange.Text = string.Empty;
                            Thread.Sleep(100);
                            _headRange.InlineShapes.AddPicture(item.ValueData.ToString(), paramTrue, paramTrue, paramMissing);
                            break;
                    }
                    if (item.MarkPosition == 1 || item.MarkPosition == 2)
                        wordApplication.ActiveWindow.View.SeekView = WdSeekView.wdSeekMainDocument;
                }
                wordDocument.SaveAs(paramTargetPath, noThing, noThing, noThing, noThing, noThing, noThing, noThing, noThing, noThing, noThing, noThing, noThing, noThing, noThing, noThing);

                result = true;
                // result = DOCConvertToPDF(targetPath, filePath);
                try
                {
                    //File.Delete(targetPath);
                }
                catch { }
            }
            catch (Exception ex)
            {
                throw ex;
                result = false;
            }
            finally
            {
                if (wordDocument != null)
                {
                    wordDocument.Close(doNotSaveChanges, paramMissing, paramMissing);
                    wordDocument.Dispose();
                }
                if (wordApplication != null)
                {
                    wordApplication.Quit(paramMissing, paramMissing, paramMissing);
                    wordApplication.Dispose();
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return result;
        }

        public static string DocConvertToPdf(string docPath)
        {

            string result = "false";
            string extension = Path.GetExtension(docPath).ToLower();
            int endIndex = docPath.LastIndexOf(extension);
            string targetPath = string.Format("{0}.pdf", docPath.Substring(0, endIndex));
            Application wordApplication = null;
            Document wordDocument = null;

            object doNotSaveChanges = WdSaveOptions.wdDoNotSaveChanges;
            object paramMissing = Type.Missing;

            try
            {
                wordApplication = new Application(); wordApplication.Visible = false;

                object paramSourceDocPath = docPath;
                string paramExportFilePath = targetPath;
                bool paramOpenAfterExport = false, paramIncludeDocProps = true, paramKeepIRM = true;
                bool paramDocStructureTags = true, paramBitmapMissingFonts = true, paramUseISO19005_1 = false;
                int paramStartPage = 0, paramEndPage = 0;

                WdExportFormat paramExportFormat = WdExportFormat.wdExportFormatPDF;
                WdExportOptimizeFor paramExportOptimizeFor = WdExportOptimizeFor.wdExportOptimizeForPrint;
                WdExportRange paramExportRange = WdExportRange.wdExportAllDocument;
                WdExportItem paramExportItem = WdExportItem.wdExportDocumentContent;
                WdExportCreateBookmarks paramCreateBookmarks = WdExportCreateBookmarks.wdExportCreateWordBookmarks;

                wordDocument = wordApplication.Documents.Open(
                    paramSourceDocPath,
                    paramMissing, paramMissing, paramMissing,
                    paramMissing, paramMissing, paramMissing,
                    paramMissing, paramMissing, paramMissing,
                    paramMissing, paramMissing, paramMissing,
                    paramMissing, paramMissing, paramMissing
                );

                if (wordDocument != null)
                {
                    wordDocument.ExportAsFixedFormat(
                        paramExportFilePath, paramExportFormat, paramOpenAfterExport,
                        paramExportOptimizeFor, paramExportRange, paramStartPage,
                        paramEndPage, paramExportItem, paramIncludeDocProps,
                        paramKeepIRM, paramCreateBookmarks, paramDocStructureTags,
                        paramBitmapMissingFonts, paramUseISO19005_1, paramMissing
                    );
                }
                result = targetPath;
            }
            catch
            {
                result = "false";
            }
            finally
            {
                if (wordDocument != null)
                {
                    wordDocument.Close(doNotSaveChanges, paramMissing, paramMissing);
                    wordDocument = null;
                }
                if (wordApplication != null)
                {
                    wordApplication.Quit(paramMissing, paramMissing, paramMissing);
                    wordApplication = null;
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return result;
        }

        /// <summary>
        /// 文档模板导出
        /// </summary>
        /// <param name="templatePath"></param>
        /// <param name="filePath"></param>
        /// <param name="wordModels"></param>
        /// <param name="oBookMark"></param>
        /// <returns></returns>
        public static bool DOCTemplateConvert(object templatePath, string filePath, List<WordMarkModel> wordModels, object[] oBookMark)
        {
            bool result = false;
            string extension = Path.GetExtension(filePath).ToLower();
            int endIndex = filePath.LastIndexOf(extension);
            string targetPath = string.Format("{0}.docx", filePath.Substring(0, endIndex));
            object oMissing = System.Reflection.Missing.Value;
            try
            {
                Microsoft.Office.Interop.Word._Application wordApplication = new Microsoft.Office.Interop.Word.Application();
                Microsoft.Office.Interop.Word._Document wordDocument = null;
                //设置为不可见
                wordApplication.Visible = false;
                //以模板为基础生成文档
                wordDocument = wordApplication.Documents.Add(ref templatePath, ref oMissing, ref oMissing, ref oMissing);
                for (int i = 0; i < oBookMark.Length; i++)
                {
                    var mark = wordModels.FirstOrDefault(zw => zw.MarkName == oBookMark[i].ToString());
                    wordDocument.Bookmarks.get_Item(ref oBookMark[i]).Range.Text = mark.MarkValue.ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
                result = false;
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return result;
        }
    }
}
