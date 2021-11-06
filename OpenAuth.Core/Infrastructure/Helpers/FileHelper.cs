using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Infrastructure.Extensions;
using Word = Microsoft.Office.Interop.Word;
using System.Reflection;
using System.Data;

namespace Infrastructure.Helpers {
	public class FileHelper {
		private static object _filePathObj = new object();

		/// <summary>
		/// 通过迭代器读取平面文件行内容(必须是带有\r\n换行的文件,百万行以上的内容读取效率存在问题,适用于100M左右文件，行100W内，超出的会有卡顿)
		/// </summary>
		/// <param name="fullPath">文件全路径</param>
		/// <param name="page">分页页数</param>
		/// <param name="pageSize">分页大小</param>
		/// <param name="seekEnd"> 是否最后一行向前读取,默认从前向后读取</param>
		/// <returns></returns>
		public static IEnumerable<string> ReadPageLine(string fullPath, int page, int pageSize, bool seekEnd = false) {
			if (page <= 0) {
				page = 1;
			}
			fullPath = StringExtension.ReplacePath(fullPath);
			var lines = File.ReadLines(fullPath, Encoding.UTF8);
			if (seekEnd) {
				int lineCount = lines.Count();
				int linPageCount = (int)Math.Ceiling(lineCount / (pageSize * 1.00));
				//超过总页数，不处理
				if (page > linPageCount) {
					page = 0;
					pageSize = 0;
				} else if (page == linPageCount)//最后一页，取最后一页剩下所有的行
				  {
					pageSize = lineCount - (page - 1) * pageSize;
					if (page == 1) {
						page = 0;
					} else {
						page = lines.Count() - page * pageSize;
					}
				} else {
					page = lines.Count() - page * pageSize;
				}
			} else {
				page = (page - 1) * pageSize;
			}
			lines = lines.Skip(page).Take(pageSize);

			var enumerator = lines.GetEnumerator();
			int count = 1;
			while (enumerator.MoveNext() || count <= pageSize) {
				yield return enumerator.Current;
				count++;
			}
			enumerator.Dispose();
		}
		public static bool FileExists(string path) {
			return File.Exists(StringExtension.ReplacePath(path));
		}

		public static string GetCurrentDownLoadPath() {
			return ("Download\\").MapPath();
		}

		public static bool DirectoryExists(string path) {
			return Directory.Exists(StringExtension.ReplacePath(path));
		}


		public static string Read_File(string fullpath, string filename, string suffix) {
			return ReadFile((fullpath + "\\" + filename + suffix).MapPath());
		}
		public static string ReadFile(string fullName) {
			//  Encoding code = Encoding.GetEncoding(); //Encoding.GetEncoding("gb2312");
			string temp = fullName.MapPath().ReplacePath();
			string str = "";
			if (!File.Exists(temp)) {
				return str;
			}
			StreamReader sr = null;
			try {
				sr = new StreamReader(temp);
				str = sr.ReadToEnd(); // 读取文件
			} catch { }
			sr?.Close();
			sr?.Dispose();
			return str;
		}



		/// <summary>
		/// 取后缀名
		/// </summary>
		/// <param name="filename">文件名</param>
		/// <returns>.gif|.html格式</returns>
		public static string GetPostfixStr(string filename) {
			int start = filename.LastIndexOf(".");
			int length = filename.Length;
			string postfix = filename.Substring(start, length - start);
			return postfix;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">路径 </param>
		/// <param name="fileName">文件名</param>
		/// <param name="content">写入的内容</param>
		/// <param name="appendToLast">是否将内容添加到未尾,默认不添加</param>
		public static void WriteFile(string path, string fileName, string content, bool appendToLast = false) {
			path = StringExtension.ReplacePath(path);
			fileName = StringExtension.ReplacePath(fileName);
			if (!Directory.Exists(path))//如果不存在就创建file文件夹
				Directory.CreateDirectory(path);

			using (FileStream stream = File.Open(Path.Combine(path, fileName), FileMode.OpenOrCreate, FileAccess.Write)) {
				byte[] by = Encoding.Default.GetBytes(content);
				if (appendToLast) {
					stream.Position = stream.Length;
				} else {
					stream.SetLength(0);
				}
				stream.Write(by, 0, by.Length);
			}
		}



		/// <summary>
		/// 追加文件
		/// </summary>
		/// <param name="Path">文件路径</param>
		/// <param name="strings">内容</param>
		public static void FileAdd(string Path, string strings) {
			StreamWriter sw = File.AppendText(StringExtension.ReplacePath(Path));
			sw.Write(strings);
			sw.Flush();
			sw.Close();
			sw.Dispose();
		}


		/// <summary>
		/// 拷贝文件
		/// </summary>
		/// <param name="OrignFile">原始文件</param>
		/// <param name="NewFile">新文件路径</param>
		public static void FileCoppy(string OrignFile, string NewFile) {
			File.Copy(StringExtension.ReplacePath(OrignFile), StringExtension.ReplacePath(NewFile), true);
		}


		/// <summary>
		/// 删除文件
		/// </summary>
		/// <param name="Path">路径</param>
		public static void FileDel(string Path) {
			File.Delete(StringExtension.ReplacePath(Path));
		}

		/// <summary>
		/// 移动文件
		/// </summary>
		/// <param name="OrignFile">原始路径</param>
		/// <param name="NewFile">新路径</param>
		public static void FileMove(string OrignFile, string NewFile) {
			File.Move(StringExtension.ReplacePath(OrignFile), StringExtension.ReplacePath(NewFile));
		}

		/// <summary>
		/// 在当前目录下创建目录
		/// </summary>
		/// <param name="OrignFolder">当前目录</param>
		/// <param name="NewFloder">新目录</param>
		public static void FolderCreate(string OrignFolder, string NewFloder) {
			Directory.SetCurrentDirectory(StringExtension.ReplacePath(OrignFolder));
			Directory.CreateDirectory(StringExtension.ReplacePath(NewFloder));
		}

		/// <summary>
		/// 创建文件夹
		/// </summary>
		/// <param name="Path"></param>
		public static void FolderCreate(string Path) {
			// 判断目标目录是否存在如果不存在则新建之
			if (!Directory.Exists(StringExtension.ReplacePath(Path)))
				Directory.CreateDirectory(StringExtension.ReplacePath(Path));
		}


		public static void FileCreate(string Path) {
			FileInfo CreateFile = new FileInfo(StringExtension.ReplacePath(Path)); //创建文件 
			if (!CreateFile.Exists) {
				FileStream FS = CreateFile.Create();
				FS.Close();
			}
		}
		/// <summary>
		/// 递归删除文件夹目录及文件
		/// </summary>
		/// <param name="dir"></param>  
		/// <returns></returns>
		public static void DeleteFolder(string dir) {
			dir = StringExtension.ReplacePath(dir);
			if (Directory.Exists(dir)) //如果存在这个文件夹删除之 
			{
				foreach (string d in Directory.GetFileSystemEntries(dir)) {
					if (File.Exists(d))
						File.Delete(d); //直接删除其中的文件                        
					else
						DeleteFolder(d); //递归删除子文件夹 
				}
				Directory.Delete(dir, true); //删除已空文件夹                 
			}
		}


		/// <summary>
		/// 指定文件夹下面的所有内容copy到目标文件夹下面
		/// </summary>
		/// <param name="srcPath">原始路径</param>
		/// <param name="aimPath">目标文件夹</param>
		public static void CopyDir(string srcPath, string aimPath) {
			try {
				aimPath = StringExtension.ReplacePath(aimPath);
				// 检查目标目录是否以目录分割字符结束如果不是则添加之
				if (aimPath[aimPath.Length - 1] != Path.DirectorySeparatorChar)
					aimPath += Path.DirectorySeparatorChar;
				// 判断目标目录是否存在如果不存在则新建之
				if (!Directory.Exists(aimPath))
					Directory.CreateDirectory(aimPath);
				// 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组
				//如果你指向copy目标文件下面的文件而不包含目录请使用下面的方法
				//string[] fileList = Directory.GetFiles(srcPath);
				string[] fileList = Directory.GetFileSystemEntries(StringExtension.ReplacePath(srcPath));
				//遍历所有的文件和目录
				foreach (string file in fileList) {
					//先当作目录处理如果存在这个目录就递归Copy该目录下面的文件

					if (Directory.Exists(file))
						CopyDir(file, aimPath + Path.GetFileName(file));
					//否则直接Copy文件
					else
						File.Copy(file, aimPath + Path.GetFileName(file), true);
				}
			} catch (Exception ee) {
				throw new Exception(ee.ToString());
			}
		}

		/// <summary>
		/// 获取文件夹大小
		/// </summary>
		/// <param name="dirPath">文件夹路径</param>
		/// <returns></returns>
		public static long GetDirectoryLength(string dirPath) {
			dirPath = StringExtension.ReplacePath(dirPath);
			if (!Directory.Exists(dirPath))
				return 0;
			long len = 0;
			DirectoryInfo di = new DirectoryInfo(dirPath);
			foreach (FileInfo fi in di.GetFiles()) {
				len += fi.Length;
			}
			DirectoryInfo[] dis = di.GetDirectories();
			if (dis.Length > 0) {
				for (int i = 0; i < dis.Length; i++) {
					len += GetDirectoryLength(dis[i].FullName);
				}
			}
			return len;
		}

		/// <summary>
		/// 获取指定文件详细属性
		/// </summary>
		/// <param name="filePath">文件详细路径</param>
		/// <returns></returns>
		public static string GetFileAttibe(string filePath) {
			string str = "";
			filePath = StringExtension.ReplacePath(filePath);
			System.IO.FileInfo objFI = new System.IO.FileInfo(filePath);
			str += "详细路径:" + objFI.FullName + "<br>文件名称:" + objFI.Name + "<br>文件长度:" + objFI.Length.ToString() + "字节<br>创建时间" + objFI.CreationTime.ToString() + "<br>最后访问时间:" + objFI.LastAccessTime.ToString() + "<br>修改时间:" + objFI.LastWriteTime.ToString() + "<br>所在目录:" + objFI.DirectoryName + "<br>扩展名:" + objFI.Extension;
			return str;
		}
		public class WordTemplate {
			/// <summary>
			/// 0 正文；1页眉；2页脚
			/// </summary>
			public int MarkPosition { get; set; }

			/// <summary>
			/// 下标从 1 开始
			/// </summary>
			public int TableMark { get; set; }

			/// <summary>
			/// 下标从 1 开始
			/// </summary>
			public int XCellMark { get; set; }

			/// <summary>
			/// 下标从 1 开始
			/// </summary>
			public int YCellMark { get; set; }

			/// <summary>
			/// 0文本(string)；1图片(string)；2列表(datatable)；3html字符串（string）
			/// </summary>
			public int ValueType { get; set; }

			public object ValueData { get; set; }
			//public FileData Filedata { get; set; }

			public int FileXCellMark { get; set; }
			public int FileYCellMark { get; set; }
		}
		public static FilePocket OrdersFilePath {
			get {
				FilePocket filePocket = new FilePocket();
				string uploadRelPath = "/Templates/files/orders/", uploadAbsPath = string.Empty;
				uploadRelPath = uploadRelPath + DateTime.Now.ToString("yyyyMM") + "/";
				//uploadAbsPath = Path.Combine(Directory.GetCurrentDirectory(), "templets", "files", path);
				uploadAbsPath = Path.Combine(Directory.GetCurrentDirectory() + uploadRelPath).Replace("/", "\\");

				if (!Directory.Exists(uploadAbsPath)) {
					Directory.CreateDirectory(uploadAbsPath);
				}
				filePocket.PhysicalPath = uploadAbsPath;
				filePocket.VirtualPath = uploadRelPath;

				return filePocket;
			}
		}
		public static FilePocket TempletFilePath {
			get {
				FilePocket filePocket = new FilePocket();
				string uploadRelPath = "\\Templates\\", uploadAbsPath = string.Empty;
				uploadAbsPath = Path.Combine(Directory.GetCurrentDirectory() + uploadRelPath).Replace("/", "\\");
				if (!Directory.Exists(uploadAbsPath)) {
					Directory.CreateDirectory(uploadAbsPath);
				}
				filePocket.PhysicalPath = uploadAbsPath;
				filePocket.VirtualPath = uploadRelPath;

				return filePocket;
			}
		}
		/// <summary>
		/// 上传保存本地路径
		/// </summary>
		public static FilePocket FilePath
		{
			get
			{
				FilePocket filePocket = new FilePocket();
				string uploadRelPath = "/Templates/files/", uploadAbsPath = string.Empty;
				uploadRelPath = uploadRelPath + DateTime.Now.ToString("yyyyMM") + "/";
				uploadAbsPath = Path.Combine(Directory.GetCurrentDirectory() + uploadRelPath).Replace("/", "\\");

				if (!Directory.Exists(uploadAbsPath))
				{
					Directory.CreateDirectory(uploadAbsPath);
				}
				filePocket.PhysicalPath = uploadAbsPath;
				filePocket.VirtualPath = uploadRelPath;

				return filePocket;
			}
		}
		#region PDF导出
		public static bool DOCTemplateToPDF(string templatePath, string filePath, List<WordTemplate> wordMarks) {

			bool result = false;
			string extension = Path.GetExtension(filePath).ToLower();
			int endIndex = filePath.LastIndexOf(extension);
			string targetPath = string.Format("{0}.doc", filePath.Substring(0, endIndex));

			Word._Application wordApplication = null;
			Word._Document wordDocument = null;

			object paramMissing = Type.Missing; object paramFalse = false; object paramTemplatePath = templatePath;
			object paramTrue = true; object paramDocType = Word.WdDocumentType.wdTypeDocument; object noThing = Missing.Value;
			object paramTargetPath = targetPath; object doNotSaveChanges = Word.WdSaveOptions.wdDoNotSaveChanges;

			try {
				wordApplication = new Word.Application(); wordApplication.Visible = false;
				wordDocument = wordApplication.Documents.Add(ref paramTemplatePath, ref paramFalse, ref paramDocType, ref paramTrue);

				foreach (WordTemplate item in wordMarks) {
					if (item.MarkPosition == 1)
						wordApplication.ActiveWindow.View.SeekView = Word.WdSeekView.wdSeekCurrentPageHeader;
					if (item.MarkPosition == 2)
						wordApplication.ActiveWindow.View.SeekView = Word.WdSeekView.wdSeekCurrentPageFooter;

					Word.Table _headTable = null;
					if (item.MarkPosition == 1 || item.MarkPosition == 2) {
						_headTable = wordApplication.Selection.HeaderFooter.Range.Tables[item.TableMark];
					} else {
						_headTable = wordDocument.Tables[item.TableMark];
					}
					Word.Range _headRange = _headTable.Cell(item.XCellMark, item.YCellMark).Range;
					switch (item.ValueType) {
						case 0:
							_headRange.Text = item.ValueData.ToString();
							break;
						case 1:
							_headRange.Text = string.Empty;
							_headRange.InlineShapes.AddPicture(item.ValueData.ToString(), ref paramTrue, ref paramTrue, ref paramMissing);
							break;
						case 2:
							_headTable.Rows.Add(paramMissing);
							int _rowsCount = _headTable.Rows.Count;
							if (_rowsCount > 1) {
								for (int i = 1; i < _rowsCount; i++) {
									_headTable.Rows[1].Delete();
								}
							}
							DataTable dTable = (DataTable)item.ValueData;
							for (int i = 0; i < dTable.Rows.Count; i++) {
								if (i > 0) _headTable.Rows.Add(paramMissing);
								for (int z = 0; z < dTable.Columns.Count; z++) {
									_headTable.Cell(i + 1, z + 1).Range.Text = dTable.Rows[i][z].ToString();
								}
							}
							break;
						case 3:
							FileData fd = new FileData();
							fd.Content = item.ValueData.ToString();
							fd.DataType = "HTML";
							fd.FilePath = filePath.Substring(0, endIndex);
							if (fd.GenerateFileData()) {
								_headRange.InsertFile(fd.FilePath);
							}
							File.Delete(fd.FilePath);
							break;
					}
					if (item.MarkPosition == 1 || item.MarkPosition == 2)
						wordApplication.ActiveWindow.View.SeekView = Word.WdSeekView.wdSeekMainDocument;
				}
				wordDocument.SaveAs(ref paramTargetPath, ref noThing, ref noThing, ref noThing, ref noThing, ref noThing, ref noThing, ref noThing, ref noThing, ref noThing, ref noThing, ref noThing, ref noThing, ref noThing, ref noThing, ref noThing);

				if (wordDocument != null) {
					wordDocument.Close(ref doNotSaveChanges, ref paramMissing, ref paramMissing);
					wordDocument = null;
				}
				if (wordApplication != null) {
					wordApplication.Quit(ref paramMissing, ref paramMissing, ref paramMissing);
					wordApplication = null;
				}
				result = DOCConvertToPDFNos(targetPath, filePath);
				try {
					File.Delete(targetPath);
				} catch (Exception ex) {
					StringBuilder sBuilder = new StringBuilder();
					sBuilder.Append("Error Caught Event ▼\r\n");
					sBuilder.AppendFormat("┏Error in:{0}\r\n", templatePath.ToString());
					sBuilder.AppendFormat("┣Error Message:{0}\r\n", ex.Message.ToString().Replace("\r", "").Replace("\n", ""));
					sBuilder.AppendFormat("┗Stack Trace:{0}\r\n", ex.StackTrace == null ? "" : ex.StackTrace.ToString());

					string errorDir = @"\ErrLog";
					if (!Directory.Exists(errorDir)) {
						Directory.CreateDirectory(errorDir);
					}
					string errfilePath = errorDir + @"\errorLog.txt";
					if (!System.IO.File.Exists(errfilePath)) {
						System.IO.File.Create(errfilePath).Close();//创建完文件后必须关闭掉流
					}
					System.IO.File.SetAttributes(errfilePath, System.IO.FileAttributes.Normal);
					System.IO.StreamWriter sr = new System.IO.StreamWriter(errfilePath, true);
					sr.WriteLine("===============" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "=============");
					sr.WriteLine("错误的详细信息：");
					sr.WriteLine(sBuilder.ToString());
					sr.Close();

					//throw ex;
					result = false;
				}
			} catch (Exception ex) {
				StringBuilder sBuilder = new StringBuilder();
				sBuilder.Append("Error Caught Event ▼\r\n");
				sBuilder.AppendFormat("┏Error in:{0}\r\n", templatePath.ToString());
				sBuilder.AppendFormat("┣Error Message:{0}\r\n", ex.Message.ToString().Replace("\r", "").Replace("\n", ""));
				sBuilder.AppendFormat("┗Stack Trace:{0}\r\n", ex.StackTrace == null ? "" : ex.StackTrace.ToString());

				string errorDir = @"\ErrLog";
				if (!Directory.Exists(errorDir)) {
					Directory.CreateDirectory(errorDir);
				}
				string errfilePath = errorDir + @"\errorLog.txt";
				if (!System.IO.File.Exists(errfilePath)) {
					System.IO.File.Create(errfilePath).Close();//创建完文件后必须关闭掉流
				}
				System.IO.File.SetAttributes(errfilePath, System.IO.FileAttributes.Normal);
				System.IO.StreamWriter sr = new System.IO.StreamWriter(errfilePath, true);
				sr.WriteLine("===============" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "=============");
				sr.WriteLine("错误的详细信息：");
				sr.WriteLine(sBuilder.ToString());
				sr.Close();

				//throw ex;
				result = false;
			} finally {
				if (wordDocument != null) {
					wordDocument.Close(ref doNotSaveChanges, ref paramMissing, ref paramMissing);
					wordDocument = null;
				}
				if (wordApplication != null) {
					wordApplication.Quit(ref paramMissing, ref paramMissing, ref paramMissing);
					wordApplication = null;
				}
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
			return result;
		}
		private static bool DOCConvertToPDFNos(string sourcePath, string targetPath) {
			bool result = false;
			Word._Application wordApplication = null;
			Word._Document wordDocument = null;

			object doNotSaveChanges = Word.WdSaveOptions.wdDoNotSaveChanges;
			object paramMissing = Type.Missing;

			try {
				wordApplication = new Word.Application(); wordApplication.Visible = false;

				object paramSourceDocPath = sourcePath; string paramExportFilePath = targetPath;
				bool paramOpenAfterExport = false, paramIncludeDocProps = true, paramKeepIRM = true;
				bool paramDocStructureTags = true, paramBitmapMissingFonts = true, paramUseISO19005_1 = false;
				int paramStartPage = 0, paramEndPage = 0;

				Word.WdExportFormat paramExportFormat = Word.WdExportFormat.wdExportFormatPDF;
				Word.WdExportOptimizeFor paramExportOptimizeFor = Word.WdExportOptimizeFor.wdExportOptimizeForPrint;
				Word.WdExportRange paramExportRange = Word.WdExportRange.wdExportAllDocument;
				Word.WdExportItem paramExportItem = Word.WdExportItem.wdExportDocumentContent;
				Word.WdExportCreateBookmarks paramCreateBookmarks = Word.WdExportCreateBookmarks.wdExportCreateWordBookmarks;

				wordDocument = wordApplication.Documents.Open(
					ref paramSourceDocPath,
					ref paramMissing, ref paramMissing, ref paramMissing,
					ref paramMissing, ref paramMissing, ref paramMissing,
					ref paramMissing, ref paramMissing, ref paramMissing,
					ref paramMissing, ref paramMissing, ref paramMissing,
					ref paramMissing, ref paramMissing, ref paramMissing
				);

				if (wordDocument != null) {
					wordDocument.ExportAsFixedFormat(
						paramExportFilePath, paramExportFormat, paramOpenAfterExport,
						paramExportOptimizeFor, paramExportRange, paramStartPage,
						paramEndPage, paramExportItem, paramIncludeDocProps,
						paramKeepIRM, paramCreateBookmarks, paramDocStructureTags,
						paramBitmapMissingFonts, paramUseISO19005_1, ref paramMissing
					);
				}
				result = true;
			} catch {
				result = false;
			} finally {
				if (wordDocument != null) {
					wordDocument.Close(ref doNotSaveChanges, ref paramMissing, ref paramMissing);
					wordDocument = null;
				}
				if (wordApplication != null) {
					wordApplication.Quit(ref paramMissing, ref paramMissing, ref paramMissing);
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
		public static bool DOCTemplateConvert(object templatePath, string filePath, List<Wrod.WordMarkModel> wordModels, object[] oBookMark)
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
				object objfilePath = (object)filePath;
				wordDocument.SaveAs(ref objfilePath, ref oMissing, ref oMissing, ref oMissing,
				ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
				ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
				ref oMissing, ref oMissing);
				wordDocument.Close(ref oMissing, ref oMissing, ref oMissing);
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
		public class FilePocket {
			public string PhysicalPath { get; set; }

			public string VirtualPath { get; set; }
		}
		[Serializable]
		public class FileData {

			public bool IsFile = false;
			public string FilePath;
			public string DataType;
			public string Content;
			public bool GenerateFileData() {
				bool ret = false;
				try {

					string htmlName = string.Format("{0}.html", "HtmlContent");
					FilePath = FilePath + htmlName;
					if (DataType == "HTML") {
						StreamWriter sw = new StreamWriter(FilePath, false, Encoding.Default);
						sw.Write("<html><head></head><body>");//temp.html中没有完整的html文件标记不行，没有的话会在word中显示html tag而不是样式，预先写入模版也行
						sw.Write(Content);
						sw.Write("</body></html>");
						sw.Close();
						ret = true;
					}
				} catch {
					//throw new Exception();
					ret = false;
					return ret;

				}
				return ret;
			}
		}
	}
	#endregion
}
