using UnityEngine;
using System.Collections;
using Ihaius;
using System.IO;

namespace Ihaiu.Examples
{
    [System.Serializable]
    public class FileInfo 
    {
        public enum StateType
        {
            None,
            Queue,
            Pause,
            Complete,
            Fail,
        }

        public DownloadFile downloadFile;

        [SerializeField]
        public string       url;

        public string       dir;

        [SerializeField]
        public string       localPath;

        [SerializeField]
        public int          size;

        [SerializeField]
        public StateType    state;

        [SerializeField]
        public bool         isRemove;

        [SerializeField]
        public int          loadTime = 0;

        public FileInfo()
        {
        }



        public FileInfo(string url, string localPath, int size)
        {
            this.url        = url;
            this.localPath  = localPath;
            this.size       = size;
        }


        public FileInfo SetUrlAndDir(string url, string dir)
        {
            this.url = url;
            this.dir = dir;
            return this;
        }

        public void OnComplete(DownloadFile file)
        {
            state = StateType.Complete;
            DownloadManager.Install.OnComplete(this);
        }

        public void Pause()
        {
            if (downloadFile != null)
            {
                downloadFile.AsyncRemove();
            }

            state = StateType.Pause;
        }

        public void AddQueue()
        {
            if (downloadFile == null)
            {
                downloadFile = new DownloadFile(url, localPath);
                downloadFile.size = size;
                downloadFile.completeCallback = OnComplete;
            }
            isRemove = false;
            state = StateType.Queue;
            DownloadManager.Install.OnQueue(this);
            downloadFile.AsyncLoad();
        }

        public void Remove()
        {
            if (downloadFile != null && state == StateType.Queue)
            {
                downloadFile.AsyncRemove();
            }

            if (!isRemove)
            {
                DownloadManager.Install.OnRemove(this);
                isRemove = true;
            }
            else
            {
                if (DownloadManager.Install.GetQueueNum(localPath) <= 0)
                {
                    for (int i = 0; i < DownloadFile.ProcessorCount; i++)
                    {
                        string tmp = DownloadUtil.GetTmpPath(localPath, i);
                        if (File.Exists(tmp))
                        {
                            File.Delete(tmp);
                        }
                    }
                }
                DownloadManager.Install.OnDelete(this);
            }

            state = StateType.None;
        }




    }
}