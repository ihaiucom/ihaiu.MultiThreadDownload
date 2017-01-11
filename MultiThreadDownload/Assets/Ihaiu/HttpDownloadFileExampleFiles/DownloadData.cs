using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace Ihaiu.Examples
{
    [System.Serializable]
    public class DownloadData 
    {
        [SerializeField]
        public string   dir = "";

        [SerializeField]
        public List<FileInfo> queueList             = new List<FileInfo>();

        [SerializeField]
        public List<FileInfo> commpleteList         = new List<FileInfo>();

        [SerializeField]
        public List<FileInfo> removeList            = new List<FileInfo>();

        public static string path
        {
            get
            {
                #if UNITY_STANDALONE
                return Path.GetFullPath(Application.dataPath + "/../res/" + "data_list.json");
                #else
                return Application.persistentDataPath + "/" + "data_list.json";
                #endif
            }
        }

        public void Save()
        {

            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string json = JsonUtility.ToJson(this, true);
            File.WriteAllText(path, json);
        }

        public static DownloadData Load()
        {
            if (!File.Exists(path))
            {
                return new DownloadData();
            }

            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<DownloadData>(json);
        }
    }
}
