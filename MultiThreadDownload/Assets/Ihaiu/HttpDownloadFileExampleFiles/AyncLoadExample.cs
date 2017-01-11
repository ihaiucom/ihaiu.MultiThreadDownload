using UnityEngine;
using System.Collections;
using Ihaius;

namespace Ihaiu.Examples
{
    public class AyncLoadExample : MonoBehaviour
    {
        public string url           = "https://gdl.25pp.com/s/0/8/20161221170050b1d50b_byzh_1.0.1.apk?x-oss-process=udf/uc-apk,HzTDjEgZKhUVb41438b741950e99&cc=1979899524&vh=91d455eb0c2c93690347e2e0321c4c89&sf=153215407";
        public string localPath     = "./TestDownload/bych.apk";
        public DownloadFileProgress progress;

        private DownloadFile _download;
        public DownloadFile download
        {
            get
            {
                if (_download == null)
                {
                    _download = new DownloadFile(url, localPath);
                }
                return _download;
            }
        }

        void Start()
        {
            if (progress != null)
            {
                progress.downloadFile = download;
            }
        }

        public void GetSize()
        {
            download.GetSize();
            Debug.Log("size:" + download.size);
        }

        public void Load()
        {
            download.AsyncLoad();
        }

        public void Pause()
        {
            download.AsyncPause();
        }
    }
}