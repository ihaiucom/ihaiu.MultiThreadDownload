using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System;

namespace Ihaius
{
    public class DownloadFile 
    {
        public static int ProcessorCount
        {
            get
            {
                return HttpDownload.ProcessorCount;
            }
        }

        public enum StateType
        {
            None,
            Pause,
            Wait,
            Loading,
            End
        }

        /** 单个临时文件最小值 */
        public int singleTmpFileSize = 1024 * 1024 * 10;

        public string   url;
        public string   localPath;
        public long     size        = 0;
        public long     loadedSize  = 0;
        public StateType state = StateType.None;
        public Action<DownloadFile> completeCallback;

        public List<HttpDownloadFileBlock> blockList = new List<HttpDownloadFileBlock>();
       
        public float progress
        {
            get
            {
                if (size <= 0)
                {
                    return 0;
                }

                return loadedSize * 1f / size;
            }
        }

        public DownloadFile(string url, string localPath)
        {
            this.url        = url;
            this.localPath  = localPath;
        }



        #region 同步下载文件
        public bool Load()
        {
            if (url.ToLower().StartsWith("http"))
            {
                return LoadHttp();
            }
            else
            {
                return LoadFile();
            }
        }


        public bool LoadFile()
        {
            try
            {
                url = url.Replace("file:////", "file:///");
                var httpRequest = WebRequest.CreateDefault(new Uri(url));
                var httpResponse = httpRequest.GetResponse();

                var httpStream = httpResponse.GetResponseStream();
                CheckDir(localPath);
                var outStream = new FileStream(localPath, FileMode.Create);
                var buffer = new Byte[1024];
                var readBytes = 0;
                while (true)
                {
                    readBytes = httpStream.Read(buffer, 0, buffer.Length);
                    if (readBytes <= 0)
                    {
                        break;
                    }
                    outStream.Write(buffer, 0, readBytes);
                }
                outStream.Close();
                httpStream.Close();
                httpResponse.Close();
                return true;
            }
            catch (WebException e)
            {
                Debug.LogError(e.Message);
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }

        public bool LoadHttp()
        {

            try
            {
                var httpRequest = WebRequest.Create(url) as WebRequest;
                var httpResponse = httpRequest.GetResponse() as HttpWebResponse;


                var httpStream = httpResponse.GetResponseStream();
                CheckDir(localPath);
                var outStream = new FileStream(localPath, FileMode.Create);
                var buffer = new Byte[1024];
                var readBytes = 0;
                while (true)
                {
                    readBytes = httpStream.Read(buffer, 0, buffer.Length);
                    if (readBytes <= 0)
                    {
                        break;
                    }
                    outStream.Write(buffer, 0, readBytes);
                }
                outStream.Close();
                httpStream.Close();
                httpResponse.Close();
                httpRequest.Abort();
                return true;
            }
            catch (WebException e)
            {
                Debug.LogError(e.Message);
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return false;
            }
        }
        #endregion;



        #region 异步下载文件
        public void AsyncLoad()
        {
            state = StateType.Wait;
            HttpDownload.Add(this);
        }

        public void AsyncPause()
        {
            state = StateType.Pause;
            HttpDownload.Remove(this);
        }

        public void AsyncContinue()
        {
            state = StateType.Wait;
            HttpDownload.Add(this);
        }

        public void AsyncRemove()
        {
            state = StateType.None;
            HttpDownload.Remove(this);
        }


        private HttpWebRequest  httpRequest;
        private HttpWebResponse httpResponse;
        public void AbortSizeRequest()
        {
            
            if(httpResponse != null) httpResponse.Close();
            if(httpRequest  != null) httpRequest.Abort();

            httpResponse    = null;
            httpRequest     = null;
        }

        public void GetSize()
        {
            try
            {
                httpRequest = WebRequest.Create(url) as HttpWebRequest;
                httpResponse = httpRequest.GetResponse() as HttpWebResponse;
                this.size = httpResponse.ContentLength;
            }
            catch (WebException e)
            {
                UnityEngine.Debug.LogError(e.Message);
            }

            AbortSizeRequest();
        }





        public void Cut()
        {
            blockList.Clear();
            loadedSize = 0;


            CheckDir(localPath);

            string localTmpPath = null;
            //检测是否有未下载完毕的临时文件
            for (int i = 0; i < ProcessorCount; i ++)
            {
                if (File.Exists(  GetTmpPath(i)  ))
                {
                    localTmpPath = GetTmpPath(i);
                    break;
                }
            }

            if (size <= 0)
            {

                if (!string.IsNullOrEmpty(localTmpPath))
                {
                    File.Delete(localTmpPath);
                }
                
                localTmpPath = GetTmpPath(0);
                blockList.Add(  new HttpDownloadFileBlock(this, 0, 0, 0, localTmpPath)    );
                return;
            }


            //文件块大小
            long blockSize   = size / ProcessorCount;  
            //余数
            long modSize     = size % ProcessorCount;


            FileStream inStream;
            HttpDownloadFileBlock fileBlock;


            if (!string.IsNullOrEmpty(localTmpPath))
            {
                if (size < singleTmpFileSize)
                {
                    inStream = new FileStream(localTmpPath, FileMode.Open);
                    blockList.Add(  new HttpDownloadFileBlock(this, 0, size - 1, inStream.Length, localTmpPath)    );
                    inStream.Close();
                }
                else
                {
                    for(int i = 0; i < ProcessorCount; i ++)
                    {
                        localTmpPath = GetTmpPath(i);
                        if (File.Exists(localTmpPath))
                        {

                            inStream = new FileStream(localTmpPath, FileMode.Open);

                            fileBlock = new HttpDownloadFileBlock(this, i * blockSize, (i + 1) * blockSize - 1, inStream.Length, localTmpPath);
                            if (i == ProcessorCount - 1)
                            {
                                fileBlock.end += modSize;
                            }
                            blockList.Add(fileBlock);
                            inStream.Close();

                        }
                        else
                        {
                            fileBlock = new HttpDownloadFileBlock(this, i * blockSize, (i + 1) * blockSize - 1, 0, localTmpPath);
                            if (i == ProcessorCount - 1)
                            {
                                fileBlock.end += modSize;
                            }
                            blockList.Add(fileBlock);
                        }
                    }
                }
            }
            else
            {
                if (size < singleTmpFileSize)
                {
                    localTmpPath = GetTmpPath(0);
                    blockList.Add(new HttpDownloadFileBlock(this, 0, size - 1, 0, localTmpPath));
                }
                else
                {
                    for(int i = 0; i < ProcessorCount; i ++)
                    {
                        localTmpPath = GetTmpPath(i);

                        fileBlock = new HttpDownloadFileBlock(this, i * blockSize, (i + 1) * blockSize - 1, 0, localTmpPath);
                        if (i == ProcessorCount - 1)
                        {
                            fileBlock.end += modSize;
                        }
                        blockList.Add(fileBlock);
                    }
                }

            }
        }


        public void OnLoadBlock(HttpDownloadFileBlock block)
        {
            bool isAllComplete = true;
            for(int i = 0 ; i < blockList.Count; i ++)
            {
                if (!blockList[i].IsComplete)
                {
                    isAllComplete = false;
                    break;
                }
            }

            if (isAllComplete)
            {
                MergeFile();
            }
        }


        private void MergeFile()
        {
            if (size < singleTmpFileSize)
            {
                if (File.Exists(localPath))
                {
                    File.Delete(localPath);
                }

                File.Move(GetTmpPath(0), localPath);
				Complete();
                return;
            }

            FileStream  outStream   = new FileStream(localPath, FileMode.Create);
            FileStream  inStream;
            int         readSize    = 0;
            Byte[]      buffer      = new Byte[1024];

            for (int i = 0; i < ProcessorCount; i ++)
            {
                inStream = new FileStream(GetTmpPath(i), FileMode.Open);
                while (true)
                {
                    readSize = inStream.Read(buffer, 0, buffer.Length);
                    if (readSize <= 0)
                    {
                        break;
                    }
                    outStream.Write(buffer, 0, readSize);
                }
                inStream.Close();
            }
            outStream.Close();
            ClearTmp();
			Complete();
        }

		private void Complete()
		{
			state = StateType.End;
			MLog.LogFormat("", "DownloadFile Complete {0}", this);

			if (completeCallback != null)
				HttpDownload.AddLoaded(this);
		}

        public void ClearTmp()
        {
            string tmpPath = "";
            for (int i = 0; i < ProcessorCount; i ++)
            {
                tmpPath = GetTmpPath(i);
                if (File.Exists(tmpPath))
                {
                    File.Delete(tmpPath);
                }
            }
        }


        private string GetTmpPath(int i)
        {
            return DownloadUtil.GetTmpPath(localPath,i);
        }


        private void CheckDir(string filePath)
        {
            DownloadUtil.CheckDir(filePath);
        }
        #endregion

        public override string ToString()
        {
            return string.Format("[DownloadFile: url={0}, localPath={1}, size={2}, loadSize={3}, blockCount={4}]", url, localPath, size, loadedSize, blockList.Count);
        }
    }
}