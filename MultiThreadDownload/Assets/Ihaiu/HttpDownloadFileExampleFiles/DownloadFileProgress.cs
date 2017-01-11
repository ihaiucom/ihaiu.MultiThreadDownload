using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Ihaius;
using System.Collections.Generic;

namespace Ihaiu.Examples
{
    public class DownloadFileProgress : MonoBehaviour 
    {
        public DownloadFile     downloadFile;

        public RectTransform    totalBar;
        public Text             totalRateText;
        public Text             infoText;
        public RectTransform    blockPrefabBg;
        public RectTransform    blockPrefabColor;
        public RectTransform    blockContainer;



        private float               blockTotalWidth = 0;
        private List<RectTransform> blockItemsBg      = new List<RectTransform>();
        private List<RectTransform> blockItemsColor      = new List<RectTransform>();

    	void Start () 
        {
            blockTotalWidth = blockContainer.rect.width;
            blockItemsBg.Add(blockPrefabBg);
            blockItemsColor.Add(blockPrefabColor);
    	}


    	
        private float _t = 0;
    	void Update () 
        {
            _t += Time.deltaTime;

            if (_t > 1)
            {
                _t = _t - 1;
                OnUpdate();
            }
    	}

        private long preLoadedSize = 0;
        private float loadedTime = 0;
        private float leftTime = 0;
        void OnUpdate()
        {
            if (downloadFile == null)
                return;

            long speed = 0;
            if (downloadFile.state == DownloadFile.StateType.Loading)
            {
                speed = downloadFile.loadedSize - preLoadedSize;
                preLoadedSize = downloadFile.loadedSize;
            }

            totalBar.localScale = new Vector3(downloadFile.progress, 1, 1);
            totalRateText.text = Mathf.Ceil(downloadFile.progress * 100) + "%";


            if(downloadFile.state == DownloadFile.StateType.End)
            {

                if (loadedTime > 0)
                {
                    infoText.text = string.Format("{0}       已下载: {1} / {2}       花费时间:{3}", "已完成", ToSizeStr(downloadFile.loadedSize),  ToSizeStr(downloadFile.size), ToTimeStr(loadedTime));
                }
                else
                {
                    infoText.text = "已完成";
                }
            }
            else if(downloadFile.state == DownloadFile.StateType.Loading)
            {
                loadedTime += 1;
                if(speed > 0) leftTime = (downloadFile.size - downloadFile.loadedSize) / speed;
                infoText.text = string.Format("下载速度: {0}/S       已下载: {1} / {2}       已下载时间:{3}  剩余时间:{4}", ToSizeStr(speed), ToSizeStr(downloadFile.loadedSize),  ToSizeStr(downloadFile.size), ToTimeStr(loadedTime), speed > 0 ? ToTimeStr(leftTime) : "--:--:--" );
            }
            else
            {
                if (downloadFile.size <= 0)
                {
                    infoText.text = "等待中。。。";
                }
                else
                {
                    infoText.text = string.Format("{0}       已下载: {1} / {2}", downloadFile.state == DownloadFile.StateType.Pause ? "暂停" : "排队中", ToSizeStr(downloadFile.loadedSize),  ToSizeStr(downloadFile.size) );
                }
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

                float b = downloadFile.blockList[i].begin * 1f / downloadFile.size;
                float w = (downloadFile.blockList[i].end - downloadFile.blockList[i].begin) * 1f / downloadFile.size;
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