using UnityEngine;
using System.Collections;
using System.IO;
using Ihaius;
using System.Threading;
using System;

namespace Ihaiu.Examples
{
    public class SyncLoadExample : MonoBehaviour 
    {

    	void Start () {
    	
    	}
    	
    	void Update () {
    	
    	}

        public string picUrl = "http://blog.ihaiu.com/assets/docpic/unity_guidref_1.png";
        public string picPath = "./TestDownload/unity_guidref_1.png";
        public void Load()
        {
            Log("Main", "开始加载");
            new DownloadFile(picUrl, picPath).Load();
            Log("Main", "加载完成");
        }


        public void CoroutineLoad()
        {
            Log("Main", "Begin");
            StartCoroutine(OnLoad());
            Log("Main", "End");
        }

        IEnumerator OnLoad()
        {
            Log("Coroutine", "开始加载");
            yield return null;
            new DownloadFile(picUrl, picPath).Load();
            Log("Coroutine", "加载完成");
        }

        public void ThreadLoad()
        {

            Log("Main", "Begin");
            ThreadStart threadStart = new ThreadStart(delegate()
                {

                    Log("Thread", "开始加载");
                    new DownloadFile(picUrl, picPath).Load();
                    Log("Thread", "加载完成");
                });



            Thread thread = new Thread(threadStart);
            thread.Start();

            Log("Main", "End");
        }


        private void Log(string threadName, string msg)
        {
            Debug.LogFormat("{0} [{1}]  {2}", time, threadName, msg);
        }



        public string time
        {
            get
            {
                return DateTime.Now.ToString("mm：ss：ffff");
            }
        }





    }
}