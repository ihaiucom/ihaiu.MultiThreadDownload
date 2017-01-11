using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Ihaiu.Examples
{
    public class FileItemList : MonoBehaviour 
    {
        public FileItem         prefab;
        public RectTransform    container;
        public List<FileItem>   itemList = new List<FileItem>();
        public List<FileInfo>   dataList = new List<FileInfo>();
        private float containerMinHeight = 300;
        private float itemHeight = 100;
        public int gap = 0;

        void Start()
        {
            itemHeight = (prefab.transform as RectTransform).rect.height;
            containerMinHeight = (container.parent as RectTransform).rect.height;
            itemList.Add(prefab);
        }

        private float _t = 0;
        void Update ()
        {
            _t += Time.deltaTime;

            if (_t > 0.5f)
            {
                _t = 0;
                OnSecond();
            }
        }

        private int _dataCount = 0;
        void OnSecond()
        {
            if (_dataCount != dataList.Count)
            {
                _dataCount = dataList.Count;
                SetItemList();
            }
        }

        public void SetDataList(List<FileInfo>   dataList)
        {
            this.dataList = dataList;
            SetItemList();
        }

        void SetItemList()
        {
            FileItem fileItem;
            for(int i = 0 ; i < dataList.Count; i ++)
            {
                if (i < itemList.Count)
                {
                    fileItem = itemList[i];
                }
                else
                {
                    GameObject go = GameObject.Instantiate(prefab.gameObject);
                    go.transform.SetParent(container, false);
                    fileItem = go.GetComponent<FileItem>();
                    itemList.Add(fileItem);
                }

                fileItem.SetData(dataList[i]);
                fileItem.rectTransform.anchoredPosition = new Vector2(0, (itemHeight + gap) * i * -1);
                fileItem.gameObject.SetActive(true);
            }

            for(int i = dataList.Count; i < itemList.Count; i ++)
            {
                itemList[i].gameObject.SetActive(false);
            }

            container.sizeDelta = new Vector2(container.sizeDelta.x, Mathf.Max( (itemHeight + gap) * dataList.Count - gap,  containerMinHeight));
        }
    }
}
