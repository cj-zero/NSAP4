using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Material
{
    public class ContentJson
    {
        public string materialCode { get; set; }
        public string selectList { get; set; }
        public List<ChangeContent> changeContent { get; set; }
        public string preMaterial { get; set; }
        public string postMaterial { get; set; }
        public string preNums { get; set; }
        public string postNums { get; set; }
        public string unit { get; set; }
        public List<FileList> fileList { get; set; }
        public string modification { get; set; }
    }

    public class ChangeContent
    {
        public string title { get; set; }
        public string type { get; set; }
        public bool isNumber { get; set; }
        public string value { get; set; }
    }
    public class FileList
    {
        public string url { get; set; }
    }
}
