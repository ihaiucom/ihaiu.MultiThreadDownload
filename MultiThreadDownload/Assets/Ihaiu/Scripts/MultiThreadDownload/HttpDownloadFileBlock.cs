using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System;


namespace Ihaius
{
    public class HttpDownloadFileBlock 
    {
        public DownloadFile downloadFile;
        public string tmpPath;
        public long begin;
        public long end;
        public long loadedSize = 0;
        public HttpDownload.LoadThread loadThread;

        private bool _isComplete;
        public bool IsComplete
        {
            get
            {
                if (_isComplete)
                    return _isComplete;
                
                return end > 0 && begin + loadedSize > end;
            }

            set
            {
                _isComplete = value;
            }
        }

        public float progress
        {
            get
            {
                if (end - begin <= 0)
                {
                    return 0;
                }

                return loadedSize * 1f / (end - begin);
            }
        }

        public HttpDownloadFileBlock(DownloadFile downloadFile, long begin, long end, long loadedSize, string tmpPath)
        {
            this.downloadFile   = downloadFile;
            this.begin          = begin;
            this.end            = end;
            this.loadedSize     = loadedSize;
            this.tmpPath        = tmpPath;
            downloadFile.loadedSize += loadedSize;
        }


        HttpWebRequest  httpRequest;
        HttpWebResponse httpResponse;
        Stream          httpStream;
        FileStream      outStream;

        public void Load(HttpDownload.LoadThread loadThread)
        {
            _isComplete = false;
            if (IsComplete)
            {
                downloadFile.OnLoadBlock(this);
                return;
            }

            downloadFile.state = DownloadFile.StateType.Loading;

			MLog.LogFormat("", "HttpDownloadFileBlock Load downloadFile.state={0}", downloadFile.state);

            this.loadThread = loadThread;



            Byte[]  buffer      = new Byte[1024];
            int     readSize    = 0;

            httpRequest = WebRequest.Create(downloadFile.url) as HttpWebRequest;
            if(end > 0) httpRequest.AddRange((int)(begin + loadedSize), (int)end);

            httpResponse    = httpRequest.GetResponse() as HttpWebResponse;
            httpStream      = httpResponse.GetResponseStream();

            outStream = new FileStream(tmpPath, loadedSize == 0 ? FileMode.Create : FileMode.Append);

            while (true)
            {
                readSize = httpStream.Read(buffer, 0, buffer.Length);
                if (readSize <= 0)
                {
                    break;
                }
                outStream.Write(buffer, 0, readSize);
                loadedSize += readSize;

                lock(downloadFile)
                {
                    downloadFile.loadedSize += readSize;
                }
            }

            Abort();

            if (end <= 0)
            {
                lock(downloadFile)
                {
                    downloadFile.size = downloadFile.loadedSize;
                }

            }
            IsComplete = true;
            this.loadThread = null;
            downloadFile.OnLoadBlock(this);
        }

        public void Abort()
        {

            if(outStream    != null) outStream.Close();
            if(httpStream   != null) httpStream.Close();
            if(httpResponse != null) httpResponse.Close();
            if(httpRequest  != null) httpRequest.Abort();

            outStream       = null;
            httpStream      = null;
            httpResponse    = null;
            httpRequest     = null;

        }

        public override string ToString()
        {
            return string.Format("[HttpDownloadFileBlock: IsComplete={0}, progress={1}, tmpPath={2}]", IsComplete, progress, tmpPath);
        }
    }
}
