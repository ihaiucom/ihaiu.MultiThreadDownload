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

        public static string DownloadsPath
        {
            get
            {

                #if UNITY_STANDALONE_WIN
                return Path.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/../Downloads") ;
                #else
                return System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/Downloads" ;
                #endif
            }
        }
    }

}