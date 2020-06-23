using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Infrastructure
{
    public class DirUtil
    {
        public static void CheckOrCreateDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
