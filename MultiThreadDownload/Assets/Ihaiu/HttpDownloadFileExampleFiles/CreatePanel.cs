using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Net;
using System.Text;
using System;
using System.Collections.Generic;
using System.Threading;
using Ihaius;


namespace Ihaiu.Examples
{
    public class CreatePanel : MonoBehaviour 
    {
        public InputField inputLocalDir;
        public InputField inputUrl;
        public LoadNameThread loadNameThread = new LoadNameThread();
        private Coroutine checkCoroutine;
    	void Start () 
        {
            checkCoroutine = DownloadManager.Install.StartCoroutine(CheckList());

            inputLocalDir.text = DownloadManager.Install.downloadData.dir;
			if(string.IsNullOrEmpty(inputLocalDir.text))
			{
                
                inputLocalDir.text = DownloadUtil.DownloadsPath;
			}
    	}

        void OnDestroy()
        {
            if (checkCoroutine != null)
            {
                DownloadManager.Install.StopCoroutine(checkCoroutine);
            }

            loadNameThread.Exit();
        }
    	
        IEnumerator CheckList()
        {
            while(true)
            {
                yield return new WaitForSeconds(0.1f);

                while (loadNameThread.endList.Count > 0)
                {
                    loadNameThread.endList[0].AddQueue();
                    loadNameThread.endList.RemoveAt(0);
                }
            }
        }


        public void OnClickDown()
        {
            string localDir = inputLocalDir.text;
            if (string.IsNullOrEmpty(localDir))
            {
                localDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/Downloads" ;
            }

            if (DownloadManager.Install.downloadData.dir != localDir)
            {
                DownloadManager.Install.downloadData.dir = localDir;
            }

            string str      = inputUrl.text;
            string[] arr    = str.Split('\n');
            for(int i = 0; i < arr.Length; i ++)
            {
                string url = arr[i].Trim();
                if (string.IsNullOrEmpty(url))
                    continue;

                loadNameThread.Add(      new FileInfo().SetUrlAndDir(url, localDir)      );
            }

            Hide();
        }


        public void Show()
        {
            if (string.IsNullOrEmpty(inputLocalDir.text))
            {
                inputLocalDir.text = DownloadUtil.DownloadsPath;
            }
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            inputUrl.text = "";
        }


        public class LoadNameThread
        {
            public enum StateType
            {
                WaitStart,
                Runing,
                End
            }

            public StateType    state   = StateType.WaitStart;

            private Thread       thread;
            private bool         isExit = false;


            public List<FileInfo>   waitList = new List<FileInfo>();
            public List<FileInfo>   endList = new List<FileInfo>();
            public FileInfo         file;

            HttpWebRequest  httpWebQuest;
            HttpWebResponse httpWebResponse;

            public void Add(FileInfo file)
            {
                waitList.Add(file);
                Start();
            }


            public void Start()
            {

                if (state == StateType.Runing)
                    return;

                Exit();

                thread = new Thread(new ThreadStart(Run));
                state = StateType.Runing;
                isExit = false;
                thread.Start();
            }


            public void Exit()
            {
                if (thread != null)
                {
                    thread.Abort();
                    thread = null;
                }


                if (httpWebResponse != null)    { httpWebResponse.Close();      httpWebResponse     = null; }
                if (httpWebQuest    != null)    { httpWebQuest.Abort();         httpWebQuest        = null; }

                file = null;
                state = StateType.End;
                isExit = true;
            }

            private void Run()
            {
                while(!isExit)
                {
                    lock(waitList)
                    {
                        if (waitList.Count <= 0)
                        {
                            break;
                        }

                        file = waitList[0];
                        waitList.RemoveAt(0);
                    }


                    file.localPath = Path.Combine(file.dir, GetNamefromUrl(file.url, out file.size));

                    lock (endList)
                    {
                        endList.Add(file);
                    }


                   
                    file = null;
                }

                state = StateType.End;
                thread = null;
            }

            private string GetNamefromUrl(string url, out int size)
            {
                httpWebQuest = (HttpWebRequest)WebRequest.Create(url);
                httpWebQuest.Method = "GET";
                httpWebQuest.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:19.0) Gecko/20100101 Firefox/19.0";
                httpWebQuest.KeepAlive = false;
                System.Net.ServicePointManager.DefaultConnectionLimit = 200;
                httpWebResponse = (HttpWebResponse)httpWebQuest.GetResponse();
                string locationInfo = httpWebResponse.ResponseUri.ToString();
                string sizeinfo = httpWebResponse.Headers["Content-Length"];
                string fileinfo = httpWebResponse.Headers["Content-Disposition"];
                string mathkey = "filename=";

                if (httpWebResponse != null)    { httpWebResponse.Close();      httpWebResponse     = null; }
                if (httpWebQuest    != null)    { httpWebQuest.Abort();         httpWebQuest        = null; }

                size = string.IsNullOrEmpty(sizeinfo) ? 0 : Convert.ToInt32(sizeinfo);

                if (fileinfo == null)
                {
                    if (!string.IsNullOrEmpty(locationInfo) && !locationInfo.Equals(url))
                    {
                        return GetNamefromUrl(locationInfo, out size);
                    }
                    return Path.GetFileName(url).Split('?')[0];
                }
                return fileinfo.Substring(fileinfo.LastIndexOf(mathkey)).Replace(mathkey, "").Replace("\"", "").Split('?')[0];
            }


        }
    }
}