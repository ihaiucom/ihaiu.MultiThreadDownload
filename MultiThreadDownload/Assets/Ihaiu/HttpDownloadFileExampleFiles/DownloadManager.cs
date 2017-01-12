using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Ihaius;
using UnityEngine.UI;

namespace Ihaiu.Examples
{
    public class DownloadManager : MonoBehaviour 
    {
        private static DownloadManager _install;
        public static DownloadManager Install
        {
            get
            {
                return _install;
            }
        }

        private DownloadData _downloadData;
        public DownloadData downloadData
        {
            get
            {
                if (_downloadData == null)
                {
                    _downloadData = DownloadData.Load();
                    foreach(FileInfo info in _downloadData.queueList)
                    {
                        info.state = FileInfo.StateType.Pause;
                    }
                }
                return _downloadData;
            }
        }

        public List<FileInfo> queueList         {       get     {   return downloadData.queueList;          }   }
        public List<FileInfo> commpleteList     {       get     {   return downloadData.commpleteList;      }   }
        public List<FileInfo> removeList        {       get     {   return downloadData.removeList;         }   }

        public Image[] navBgs;
        public FileItemList itemList;

        public void SetNav(int index)
        {
            switch(index)
            {
                case 1:
                    itemList.SetDataList(commpleteList);
                    break;
                case 2:
                    itemList.SetDataList(removeList);
                    break;
                default:
                    itemList.SetDataList(queueList);
                    index = 0;
                    break;
            }

            for(int i = 0; i < navBgs.Length; i ++)
            {
                navBgs[i].enabled = i == index;
            }

        }

        public void OnClickClear()
        {
            commpleteList.Clear();

            int count = removeList.Count - 1;
            for(int i = count; i >= 0; i --)
            {
                removeList[i].Remove();
            }
        }


        void Awake()
        {
            _install = this;

            for(int i = 0; i < queueList.Count; i ++)
            {
                queueList[i].AddQueue();
            }
            SetNav(0);
        }

        void OnApplicationQuit()
        {
            downloadData.Save();
        }

        private float _t = 0;
        void Update ()
        {
            _t += Time.deltaTime;

            if (_t > 1)
            {
                _t = _t - 1;
                OnSecond();
            }
        }

        void OnSecond()
        {
            for(int i = 0; i < queueList.Count; i ++)
            {
                if (queueList[i].downloadFile != null && queueList[i].downloadFile.state == DownloadFile.StateType.Loading)
                {
                    queueList[i].loadTime += 1;
                }
            }
        }
    

        public void OnQueue(FileInfo file)
        {
            if (removeList.Contains(file))
            {
                removeList.Remove(file);
            }

            if (commpleteList.Contains(file))
            {
                commpleteList.Remove(file);
            }

            if (!queueList.Contains(file))
            {
                queueList.Add(file);
            }
        }

        public void OnComplete(FileInfo file)
        {
            if (queueList.Contains(file))
            {
                queueList.Remove(file);
            }


            if (!commpleteList.Contains(file))
            {
                commpleteList.Add(file);
            }
        }

        public void OnRemove(FileInfo file)
        {
            if (queueList.Contains(file))
            {
                queueList.Remove(file);
            }

            if (commpleteList.Contains(file))
            {
                commpleteList.Remove(file);
            }


            if (!removeList.Contains(file))
            {
                removeList.Add(file);
            }
        }

        public void OnDelete(FileInfo file)
        {
            if (queueList.Contains(file))
            {
                queueList.Remove(file);
            }

            if (commpleteList.Contains(file))
            {
                commpleteList.Remove(file);
            }


            if (removeList.Contains(file))
            {
                removeList.Remove(file);
            }
        }

        public int GetQueueNum(string localPath)
        {
            int num = 0;
            for(int i = 0; i < queueList.Count; i ++)
            {
                if (queueList[i].localPath == localPath)
                {
                    num++;
                }
            }
            return 0 ;
        }






    }
}
