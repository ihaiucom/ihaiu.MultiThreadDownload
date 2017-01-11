using UnityEngine;
using System.Collections;
using System.IO;

namespace Ihaius
{
    public static class DownloadUtil 
    {

        public static string GetTmpPath(this string localPath, int i)
        {
            return localPath + "_" + i;
        }


        public static void CheckDir(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }

}