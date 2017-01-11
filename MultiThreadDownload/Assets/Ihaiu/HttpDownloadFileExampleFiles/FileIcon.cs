using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ihaiu.Examples
{
    public class FileIcon : MonoBehaviour 
    {
    	private static FileIcon _install;
    	public static FileIcon Install
    	{
    		get
    		{
    			return _install;
    		}
    	}


    	public Sprite[] icons;
    	public Dictionary<string, int> dict = new Dictionary<string, int>();

        public Sprite icon_none;
        public Sprite icon_ios;
        public Sprite icon_android;
        public Sprite icon_unity;

    	void Awake()
    	{
    		_install = this;

            dict.Add(".psd", 11);
            dict.Add(".jpg", 12);
            dict.Add(".gif", 13);
            dict.Add(".png", 14);
            dict.Add(".bmp", 15);
            dict.Add(".tga", 16);
            dict.Add(".ico", 17);
            dict.Add(".hdr", 18);
            dict.Add(".raw", 19);
            dict.Add(".pdf", 20);
            dict.Add(".txt", 21);
            dict.Add(".doc", 22);
            dict.Add(".rtf", 23);
            dict.Add(".odt", 24);
            dict.Add(".sub", 25);
            dict.Add(".unx", 26);
            dict.Add(".ort", 27);
            dict.Add(".chm", 28);
            dict.Add(".wpd", 29);
            dict.Add(".mp3", 30);
            dict.Add(".wav", 31);
            dict.Add(".aac", 32);
            dict.Add(".flac", 33);
            dict.Add(".cda", 34);
            dict.Add(".midi", 35);
            dict.Add(".rmf", 36);
            dict.Add(".ogg", 37);
            dict.Add(".voc", 38);
            dict.Add(".wma", 39);
            dict.Add(".qt", 40);
            dict.Add(".flv", 41);
            dict.Add(".mp4", 42);
            dict.Add(".avi", 43);
            dict.Add(".3gp", 44);
            dict.Add(".mpg", 45);
            dict.Add(".mkv", 46);
            dict.Add(".mov", 47);
            dict.Add(".asf", 48);
            dict.Add(".vob", 49);
            dict.Add(".fb2", 50);
            dict.Add(".djvu", 51);
            dict.Add(".mobi", 52);
            dict.Add(".epub", 53);
            dict.Add(".iw4", 54);
            dict.Add(".prc", 55);
            dict.Add(".chm2", 56);
            dict.Add(".tcr", 57);
            dict.Add(".ebk", 58);
            dict.Add(".azw", 59);
            dict.Add(".zip", 60);
            dict.Add(".rar", 61);
            dict.Add(".iso", 62);
            dict.Add(".jar", 63);
            dict.Add(".tar", 64);
            dict.Add(".ace", 65);
            dict.Add(".lzh", 66);
            dict.Add(".arj", 67);
            dict.Add(".cab", 68);
            dict.Add(".zoo", 69);
            dict.Add(".js", 70);
            dict.Add(".htm", 71);
            dict.Add(".css", 72);
            dict.Add(".xml", 73);
            dict.Add(".mht", 74);
            dict.Add(".psp", 75);
            dict.Add(".eml", 76);
            dict.Add(".php", 77);
            dict.Add(".java", 78);
            dict.Add(".py", 79);
            dict.Add(".ini", 80);
            dict.Add(".sys", 81);
            dict.Add(".key", 82);
            dict.Add(".ppt", 83);
            dict.Add(".nfo", 84);
            dict.Add(".xls", 85);
            dict.Add(".csv", 86);
            dict.Add(".cab2", 87);
            dict.Add(".mdb", 88);
            dict.Add(".com", 89);
            dict.Add(".swf", 90);
            dict.Add(".exe", 91);
            dict.Add(".dat", 92);
            dict.Add(".hlp", 93);
            dict.Add(".dll", 94);
            dict.Add(".faq", 95);
            dict.Add(".rss", 96);
            dict.Add(".fon", 97);
            dict.Add(".ttf", 98);
            dict.Add(".otf", 99);

    	}


    	public Sprite getIcon(string extend)
    	{
    		extend = extend.ToLower();
    		if(dict.ContainsKey(extend))
    		{
    			return icons[dict[extend]];
    		}

            switch(extend)
            {
                case ".unitypackage":
                    return icon_unity;
                case ".apk":
                    return icon_android;
                case ".app":
                    return icon_ios;
            }

            return icon_none;
    	}

    	public static Sprite GetIcon(string extend)
    	{
    		return Install.getIcon(extend);
    	}



    	#if UNITY_EDITOR
    	public string[] extends = new string[]{
    		"ai", "eps", "cdr", "svg", "wmf", "art", "cgm", "emf", "vsd", "ps",
    		"tif", "psd", "jpg", "gif", "png", "bmp", "tga", "ico", "hdr", "raw",
    		"pdf", "txt", "doc", "rtf", "odt", "sub", "unx", "ort", "chm", "wpd",
    		"mp3", "wav", "aac", "flac", "cda", "midi", "rmf", "ogg", "voc", "wma",
    		"qt", "flv", "mp4", "avi", "3gp", "mpg", "mkv", "mov", "asf", "vob",
    		"fb2", "djvu", "mobi", "epub", "iw4", "prc", "chm2", "tcr", "ebk", "azw",
    		"zip", "rar", "iso", "jar", "tar", "ace", "lzh", "arj", "cab", "zoo",
    		"js", "htm", "css", "xml", "mht", "psp", "eml", "php", "java", "py",
    		"ini", "sys", "key", "ppt", "nfo", "xls", "csv", "cab2", "mdb", "com",
    		"swf", "exe", "dat", "hlp", "dll", "faq", "rss", "fon", "ttf", "otf"
    	};

    	[ContextMenu("Generate")]
    	public void Generate()
    	{
    		StringBuilder sb = new StringBuilder();
            Dictionary<string, int> dict = new Dictionary<string, int>();
    		for(int i = 0; i < extends.Length; i ++)
    		{
                if (dict.ContainsKey(extends[i]))
                {
                    Debug.Log(extends[i] + "  " + i);
                    continue;
                }
                dict.Add(extends[i], i);
    			sb.AppendLine(string.Format("dict.Add(\"{0}\", {1});", "." + extends[i], i));
    		}

    		Debug.Log(sb.ToString());
    	}


    	[ContextMenu("GenerateSprite")]
    	public void GenerateSprite()
    	{
    		icons = new Sprite[extends.Length];
    		Object[] objs = AssetDatabase.LoadAllAssetsAtPath("Assets/Ihaiu/HttpDownloadFileExampleFiles/arts/icon_file.png");

    		for(int i = 0; i < extends.Length; i ++)
    		{
    			icons[i] = (Sprite) objs[i + 1];
    		}

    	}

    	#endif
    }
}