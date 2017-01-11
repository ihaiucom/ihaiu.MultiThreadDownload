using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;


namespace Ihaius
{
    public class HttpDownload : MonoBehaviour
    {
        #region static
        public static int ProcessorCount
        {
            get
            {
//                return Mathf.Max(Environment.ProcessorCount - 1, 1);
                return 4;
            }
        }

        private static HttpDownload _install;
        private static HttpDownload Install
        {
            get
            {
                if (_install == null)
                {
                    GameObject go = GameObject.Find("GameManager");
                    if(go == null)
                    {
                        go = new GameObject("GameManager");
                    }

                    _install = go.GetComponent<HttpDownload>();
                    if (_install == null)
                    {
                        _install = go.AddComponent<HttpDownload>();
                    }
                }
                return _install;
            }
        }

        public static DownloadFile Add(string url, string localPath)
        {
            return Add(url, localPath, 0, null);
        }

        public static DownloadFile Add(string url, string localPath, int size)
        {
            return Add(url, localPath, size, null);
        }


        public static DownloadFile Add(string url, string localPath,  Action<DownloadFile> callback)
        {
            return Add(url, localPath, 0, callback);
        }

        public static DownloadFile Add(string url, string localPath, int size, Action<DownloadFile> callback)
        {
            DownloadFile file = new DownloadFile(url, localPath);
            file.size = size;
            file.completeCallback = callback;

            Add(file);
            return file;
        }

        public static void Add(DownloadFile downloadFile)
        {
            Install.AddFile(downloadFile);
        }


        public static void Remove(DownloadFile downloadFile)
        {
            Install.RemoveFile(downloadFile);
        }

        public static void AddLoaded(DownloadFile downloadFile)
        {
            Install.AddFileLoaded(downloadFile);
        }

        public static void Exit()
        {
            Install.ExitAllThread();
        }

        #endregion

        #region wait

        public List<DownloadFile>           waitFileList        = new List<DownloadFile>();
        public List<DownloadFile>           loadSizeFileList    = new List<DownloadFile>();
        public List<DownloadFile>           loadedFileList = new List<DownloadFile>();
        public List<HttpDownloadFileBlock>  waitBlockList       = new List<HttpDownloadFileBlock>();

        public void AddFileLoaded(DownloadFile file)
        {
            loadedFileList.Add(file);
        }

        public void AddFile(DownloadFile file)
        {
            MLog.LogFormat("[Main]", "HttpDownload.AddFile {0}", file);
            waitFileList.Add(file);
            CheckLoadSizeThreadStart();
        }

        public void RemoveFile(DownloadFile file)
        {
            MLog.LogFormat("[Main]", "HttpDownload.RemoveFile Before {0}, waitFileList.Count={1}, loadSizeFileList.Count={2}", file, waitFileList.Count, loadSizeFileList.Count);
            if(loadSizeThread != null && loadSizeThread.file == file)
            {
                loadSizeThread.Exit();
            }

            if (waitFileList.Contains(file))
            {
                waitFileList.Remove(file);
            }


            if (loadSizeFileList.Contains(file))
            {
                loadSizeFileList.Remove(file);
            }


            for(int i = 0; i < file.blockList.Count; i ++)
            {
                if (file.blockList[i].loadThread != null)
                {
                    file.blockList[i].loadThread.Exit();
                }

                if (waitBlockList.Contains(file.blockList[i]))
                {
                    waitBlockList.Remove(file.blockList[i]);
                }
            }

            MLog.LogFormat("[Main]", "HttpDownload.RemoveFile RemoveComplete {0}, waitFileList.Count={1}, loadSizeFileList.Count={2}", file, waitFileList.Count, loadSizeFileList.Count);


            CheckLoadSizeThreadStart();
            CheckLoadThreadStart();

            MLog.LogFormat("[Main]", "HttpDownload.RemoveFile End {0}, waitFileList.Count={1}, loadSizeFileList.Count={2}", file, waitFileList.Count, loadSizeFileList.Count);

        }

        public void AddFileToBlock(DownloadFile file)
        {
            for(int i = 0; i < file.blockList.Count; i ++)
            {
                if(!waitBlockList.Contains(file.blockList[i]))
                    waitBlockList.Add(file.blockList[i]);
            }
            CheckLoadThreadStart();
        }


        #endregion






        private LoadSizeThread      loadSizeThread;
        private List<LoadThread>    loadThreadList = new List<LoadThread>();
        private bool                isExit = false;

        public HttpDownload()
        {
            loadSizeThread = new LoadSizeThread(this);

            for(int i = 0; i < ProcessorCount; i ++)
            {
                LoadThread thread = new LoadThread(this, i);
                loadThreadList.Add(thread);
            }
        }

        void Update()
        {
            while (loadSizeFileList.Count > 0)
            {
                AddFileToBlock(loadSizeFileList[0]);
                loadSizeFileList.RemoveAt(0);
            }

            while(loadedFileList.Count > 0)
            {
                if (loadedFileList[0].completeCallback != null)
                {
                    loadedFileList[0].completeCallback(loadedFileList[0]);
                }
                loadedFileList.RemoveAt(0);
            }
        }


        void OnApplicationQuit()
        {
            ExitAllThread();
        }


        private void CheckLoadSizeThreadStart()
        {

            MLog.LogFormat("[Main]", "HttpDownload.CheckLoadSizeThreadStart waitFileList.Count={0}, loadSizeThread.state={1}", waitFileList.Count, loadSizeThread.state);

            if (waitFileList.Count == 0)
                return;

            if (loadSizeThread.state != LoadSizeThread.StateType.Runing)
            {
                loadSizeThread.Start();
            }
        }


        private void CheckLoadThreadStart()
        {
            MLog.LogFormat("[Main]", "HttpDownload.CheckLoadThreadStart waitBlockList.Count={0}", waitBlockList.Count);


            if (waitBlockList.Count == 0)
                return;
            
            for(int i = 0; i < loadThreadList.Count; i ++)
            {
                if (loadThreadList[i].state != LoadThread.StateType.Runing)
                {
                    loadThreadList[i].Start();
                }
            }
        }

        public void ExitAllThread()
        {
            loadSizeThread.Exit();
            for(int i = 0; i < loadThreadList.Count; i ++)
            {
                loadThreadList[i].Exit();
            }
        }

        public class LoadThread
        {
            public enum StateType
            {
                WaitStart,
                Runing,
                End
            }

            public StateType    state = StateType.WaitStart;

            private HttpDownload httpDownload;
            public  int                     id;
            private Thread                  thread;
            private bool                    isExit = false;
            private HttpDownloadFileBlock   block;


            public LoadThread(HttpDownload httpDownload, int id)
            {
                this.id             = id;
                this.httpDownload   = httpDownload;
            }

            public void Start()
            {

                MLog.LogFormat("[LoadThread]", "Start Before {0}", this);

                if (state == StateType.Runing)
                    return;
                
                Exit();

                thread = new Thread(new ThreadStart(Run));
                state = StateType.Runing;
                isExit = false;
                thread.Start();

                MLog.LogFormat("[LoadThread]", "Start After {0}", this);
            }


            public void Exit()
            {
                MLog.LogFormat("[LoadThread]", "Exit Before {0}", this);

                if (block != null)
                {
                    block.Abort();
                    block = null;
                }

                if (thread != null)
                {
                    thread.Abort();
                    thread = null;
                }


                state = StateType.End;
                isExit = true;

                MLog.LogFormat("[LoadThread]", "Exit After {0}", this);
            }

            private void Run()
            {
                while(!isExit)
                {

                    MLog.LogFormat("[LoadThread]", "Run Doing Before {0}, httpDownload.waitBlockList.Count={1}", this, httpDownload.waitBlockList.Count);

                    lock(httpDownload.waitBlockList)
                    {
                        if (httpDownload.waitBlockList.Count <= 0)
                        {
                            break;
                        }

                        block = httpDownload.waitBlockList[0];
                        httpDownload.waitBlockList.RemoveAt(0);
                    }

                    block.Load(this);
                    block = null;

                    MLog.LogFormat("[LoadThread]", "Run Doing After {0}, httpDownload.waitBlockList.Count={1}, block={2}", this, httpDownload.waitBlockList.Count, block);
                }

                state = StateType.End;
                thread = null;

                MLog.LogFormat("[LoadThread]", "Run End {0}", this);
            }

            public override string ToString()
            {
                return string.Format("[LoadThread: id={0}, state={1}, isExit={2}, thread={3}]", id, state, isExit, thread);
            }
        }


        public class LoadSizeThread
        {
            public enum StateType
            {
                WaitStart,
                Runing,
                End
            }

            public StateType    state   = StateType.WaitStart;
            public DownloadFile file    = null; 

            private HttpDownload httpDownload;
            private Thread       thread;
            private bool         isExit = false;


            public LoadSizeThread(HttpDownload httpDownload)
            {
                this.httpDownload = httpDownload;
            }

            public void Start()
            {

                MLog.LogFormat("[LoadSizeThread]", "Start Before {0}", this);

                if (state == StateType.Runing)
                    return;

                Exit();

                thread = new Thread(new ThreadStart(Run));
                state = StateType.Runing;
                isExit = false;
                thread.Start();

                MLog.LogFormat("[LoadSizeThread]", "Start After {0}", this);
            }


            public void Exit()
            {
                MLog.LogFormat("[LoadSizeThread]", "Exit Before {0}", this);

                if (thread != null)
                {
                    thread.Abort();
                    thread = null;
                }

                if (file != null)
                {
                    file.AbortSizeRequest();
                }

                file = null;
                state = StateType.End;
                isExit = true;

                MLog.LogFormat("[LoadSizeThread]", "Exit After {0}", this);
            }

            private void Run()
            {
                while(!isExit)
                {

                    MLog.LogFormat("[LoadSizeThread]", "Run Doing Before {0}, httpDownload.waitFileList.Count={1}, httpDownload.loadSizeFileList.Count={2}", this, httpDownload.waitFileList.Count, httpDownload.loadSizeFileList.Count);

                    lock(httpDownload.waitFileList)
                    {
                        if (httpDownload.waitFileList.Count <= 0)
                        {
                            break;
                        }

                        file = httpDownload.waitFileList[0];
                        httpDownload.waitFileList.RemoveAt(0);
                    }

                    if (file.size <= 0)
                    {
                        file.GetSize();
                    }

                    file.Cut();

                    lock(httpDownload.loadSizeFileList)
                    {
                        httpDownload.loadSizeFileList.Add(file);
                    }
                    file = null;

                    MLog.LogFormat("[LoadSizeThread]", "Run Doing After {0}, httpDownload.waitFileList.Count={1}, httpDownload.loadSizeFileList.Count={2}", this, httpDownload.waitFileList.Count, httpDownload.loadSizeFileList.Count);
                }

                state = StateType.End;
                thread = null;

                MLog.LogFormat("[LoadSizeThread]", "Run End {0}", this);
            }


            public override string ToString()
            {
                return string.Format("[LoadSizeThread:  state={0}, isExit={1}, thread={2}, file={3}]", state, isExit, thread, file);
            }
        }
    }
}