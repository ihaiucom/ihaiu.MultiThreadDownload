using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Ihaius;
using System.IO;

namespace Ihaiu.Examples
{
    public class FileItem : MonoBehaviour 
    {
        public FileInfo fileInfo;

        public void SetData(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
            SetOpState();
            OnUpdate();
        }

        public Image        iconImage;
        public Text         urlText;
        public Text         pathText;
        public Text         rateText;
        public Text         sizeText;
        public Text         stateText;
        public Text         timeText;

        public RectTransform    totalBar;

        public RectTransform    blockPrefabBg;
        public RectTransform    blockPrefabColor;
        public RectTransform    blockContainer;

        private float               blockTotalWidth = 0;
        private List<RectTransform> blockItemsBg            = new List<RectTransform>();
        private List<RectTransform> blockItemsColor         = new List<RectTransform>();

        public GameObject button_pause;
        public GameObject button_play;
        public GameObject button_replay;


        private RectTransform _rectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = (RectTransform) transform;
                }
                return _rectTransform;
            }
        }



        public void OnClickPause()
        {
            if (fileInfo != null)
            {
                fileInfo.Pause();
            }
        }

        public void OnClickPlay()
        {
            if (fileInfo != null)
            {
                fileInfo.AddQueue();
            }
        }

        public void OnClickReplay()
        {
            if (fileInfo != null)
            {
                fileInfo.AddQueue();
            }
        }

        public void OnClickRemove()
        {
            if (fileInfo != null)
            {
                fileInfo.Remove();
            }
        }

        public void OnClickOpen()
        {
            if (fileInfo != null)
            {
                if (File.Exists(fileInfo.localPath))
                {
                    RevealInFinder(fileInfo.localPath);
                }
                else if (File.Exists(DownloadUtil.GetTmpPath(fileInfo.localPath, 0)))
                {
                    RevealInFinder(DownloadUtil.GetTmpPath(fileInfo.localPath, 0));
                }
                else
                {
                    RevealInFinder(Path.GetDirectoryName(fileInfo.localPath));
                }
            }
        }

        public void RevealInFinder(string path)
        {
            #if UNITY_STANDALONE_OSX
            System.Diagnostics.Process.Start( "/usr/bin/open", "-R " + path);

            #elif UNITY_STANDALONE_WIN
            path = path.Replace("/", "\\");
            System.Diagnostics.Process.Start("Explorer.exe", "/select, \"" +path+ "\"");
            #endif
        }




        void Start () 
        {
            Rect rect = blockContainer.rect;
            blockTotalWidth = blockContainer.rect.size.x;
            
            blockItemsBg.Add(blockPrefabBg);
            blockItemsColor.Add(blockPrefabColor);
        }

        private FileInfo.StateType _preState;
        private float _t = 0;
        void Update () 
        {

            if (blockTotalWidth < 0)
                blockTotalWidth = blockContainer.rect.size.x;
            
            _t += Time.deltaTime;

            if (_t > 1)
            {
                _t = _t - 1;
                OnUpdate();
            }

            if (fileInfo != null)
            {
                if (_preState != fileInfo.state)
                {
                    _preState = fileInfo.state;

                    SetOpState();
                }
            }
        }

        void SetOpState()
        {
            if (fileInfo.isRemove)
            {
                button_play.SetActive(false);
                button_pause.SetActive(false);
                button_replay.SetActive(true);
            }
            else
            {
                switch (fileInfo.state)
                {

                    case FileInfo.StateType.Queue:
                        button_play.SetActive(false);
                        button_pause.SetActive(true);
                        button_replay.SetActive(false);
                        break;

                    case FileInfo.StateType.Pause:
                        button_play.SetActive(true);
                        button_pause.SetActive(false);
                        button_replay.SetActive(false);
                        break;

                    default:
                        button_play.SetActive(false);
                        button_pause.SetActive(false);
                        button_replay.SetActive(true);
                        break;
                }
            }
        }


        private long preLoadedSize = 0;
        private float loadedTime = 0;
        private float leftTime = 0;
        private FileInfo _preFileInfo;
        void OnUpdate()
        {
            if (fileInfo == null)
                return;

            if (_preFileInfo != fileInfo)
            {
                _preFileInfo = fileInfo;


                iconImage.sprite = FileIcon.GetIcon(Path.GetExtension(fileInfo.localPath));
                urlText.text    = Path.GetFileName(fileInfo.url);
                pathText.text    = fileInfo.localPath;
            }


            DownloadFile downloadFile = fileInfo.downloadFile;
            if (downloadFile == null)
            {
                if (fileInfo.state == FileInfo.StateType.Complete)
                {
                    totalBar.localScale = new Vector3(1, 1, 1);
                    rateText.text = Mathf.Ceil(1 * 100) + "%";
                }
                else
                {

                    totalBar.localScale = new Vector3(0, 1, 1);
                    rateText.text = "";
                }

                for(int i = 0; i < blockItemsColor.Count; i ++)
                {
                    blockItemsColor[i].gameObject.SetActive(false);
                    blockItemsBg[i].gameObject.SetActive(false);
                }

                sizeText.text = fileInfo.size > 0 ?   ToSizeStr(fileInfo.size) : "";
                stateText.text = "";
                timeText.text = fileInfo.loadTime > 0 ?   ToTimeStr(fileInfo.loadTime) : "";
                return;
            }


            long speed = 0;
            if (downloadFile.state == DownloadFile.StateType.Loading)
            {
                speed = downloadFile.loadedSize - preLoadedSize;
                preLoadedSize = downloadFile.loadedSize;
            }

            totalBar.localScale = new Vector3(downloadFile.progress, 1, 1);
            rateText.text = Mathf.Ceil(downloadFile.progress * 100) + "%";
            sizeText.text = downloadFile.size <= 0 ? "" : string.Format("{0} / {1}", ToSizeStr(downloadFile.loadedSize), ToSizeStr(downloadFile.size));

            if (fileInfo.state == FileInfo.StateType.Complete)
            {
                stateText.text = "已完成";
            }
            else if (fileInfo.state == FileInfo.StateType.Complete)
            {
                stateText.text = "已暂停";
            }
            else if (downloadFile.state == DownloadFile.StateType.Loading)
            {
                stateText.text = ToSizeStr(speed) + "/S";
            }
            else if(fileInfo.state == FileInfo.StateType.Queue)
            {
                stateText.text = "队列中";
            }
            else
            {
                stateText.text = "";
            }

            if (downloadFile.state == DownloadFile.StateType.Loading)
            {
                if (speed > 0)
                    leftTime = (downloadFile.size - downloadFile.loadedSize) / speed;
                timeText.text = string.Format("{0} / {1}", ToTimeStr(fileInfo.loadTime), speed > 0 ? ToTimeStr(leftTime) : "--:--:--");
            }
            else if (fileInfo.loadTime > 0)
            {
                timeText.text = string.Format("{0}", ToTimeStr(fileInfo.loadTime));
            }
            else
            {
                timeText.text = "";
            }


            RectTransform blockItemBg;
            RectTransform blockItemColor;
            for(int i = 0; i < downloadFile.blockList.Count; i ++)
            {
                if (i < blockItemsColor.Count)
                {
                    blockItemColor = blockItemsColor[i];
                    blockItemBg = blockItemsBg[i];
                }
                else
                {

                    blockItemBg = (RectTransform) GameObject.Instantiate(blockPrefabBg.gameObject).transform;
                    blockItemBg.SetParent(blockContainer, false);
                    blockItemsBg.Add(blockItemBg);
                    blockItemBg.SetAsFirstSibling();


                    blockItemColor = (RectTransform) GameObject.Instantiate(blockPrefabColor.gameObject).transform;
                    blockItemColor.SetParent(blockContainer, false);
                    blockItemsColor.Add(blockItemColor);
                    blockItemColor.SetAsLastSibling();
                }

                float b = downloadFile.size <= 0 ? 0 :  downloadFile.blockList[i].begin * 1f / downloadFile.size;
                float w = downloadFile.size <= 0 ? 0 : (downloadFile.blockList[i].end - downloadFile.blockList[i].begin) * 1f / downloadFile.size;
                b *= blockTotalWidth;
                w *= blockTotalWidth;

                blockItemBg.anchoredPosition        = blockItemColor.anchoredPosition  = new Vector2(b, blockItemColor.anchoredPosition.y);
                blockItemBg.sizeDelta               = blockItemColor.sizeDelta         = new Vector2(w, blockItemColor.sizeDelta.y);
                blockItemColor.localScale           = new Vector3(downloadFile.blockList[i].progress, 1, 1);
                if(!blockItemColor.gameObject.activeSelf) blockItemColor.gameObject.SetActive(true);
                if(!blockItemBg.gameObject.activeSelf) blockItemBg.gameObject.SetActive(true);
            }

            for(int i = downloadFile.blockList.Count; i < blockItemsColor.Count; i ++)
            {
                blockItemsColor[i].gameObject.SetActive(false);
                blockItemsBg[i].gameObject.SetActive(false);
            }
        }


        private string ToSizeStr(long size)
        {
            if (size == 0)
                return "--";
            
            if (size < 1024)
            {
                return size + "B";
            }
            else if(size < 1024 * 1024)
            {
                return (Mathf.FloorToInt(    (  size / 1024f                ) * 100      )     / 100f) + "KB";
            }
            else if(size < 1024 * 1024 * 1024)
            {
                return (Mathf.FloorToInt(    (   size / (1024f * 1024)      ) * 100      )     / 100f) + "MB";
            }
            else
            {
                return (Mathf.FloorToInt(    (   size / (1024f * 1024)      ) * 100      )     / 100f) + "GB";
            }
        }

        private string ToTimeStr(float t)
        {
            int h = 0;
            int m = 0;
            int s = 0;
            int time = Mathf.RoundToInt(t);
            s = time % 60;
            m = (time - s) / 60 % 60;
            h = ((time - s) / 60 - m) / 60 % 24;
            return h.ToString("D2") + ":" + m.ToString("D2") + ":" + s.ToString("D2");
        }

       
    }
}
