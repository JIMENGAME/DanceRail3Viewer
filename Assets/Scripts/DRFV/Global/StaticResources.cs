using System;
using System.Collections.Generic;
using System.IO;
using DRFV.CoinShop.Data;
using DRFV.inokana;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Audio;
using Application = UnityEngine.Application;

namespace DRFV.Global
{
    public class StaticResources : MonoSingleton<StaticResources>
    {
        public string dataPath;
        public string cachePath;
        public ShopItem[] shopItems;
        public AudioMixer audioMixer;

        // Start is called before the first frame update
        protected override void OnAwake()
        {
            DontDestroyOnLoad(gameObject);
            Util.Init();
            dataPath = GetDataPath();
            cachePath = GetCachePath();
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            if (!Directory.Exists(dataPath + "songs"))
            {
                Directory.CreateDirectory(dataPath + "songs");
            }

            if (!Directory.Exists(dataPath + "settings"))
            {
                Directory.CreateDirectory(dataPath + "settings");
            }

            if (!Directory.Exists(dataPath + "settings/note_sfx"))
            {
                Directory.CreateDirectory(dataPath + "settings/note_sfx");
            }

            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                File.SetAttributes(cachePath, FileAttributes.Hidden);
#endif
            }

            JArray shopItemsJArray = JArray.Parse(Resources.Load<TextAsset>("SHOP/shop_items_list").text);
            List<ShopItem> shopItemList = new List<ShopItem>();
            for (var i = 0; i < shopItemsJArray.Count; i++)
            {
                ShopItem _data = shopItemsJArray[i].ToObject<ShopItem>();
                _data.id = i;
                var a = _data.type switch
                {
                    ShopItem.ShopItemType.ITEM => "ITEMS",
                    ShopItem.ShopItemType.SONG => "SONGS",
                    _ => throw new ArgumentOutOfRangeException()
                };
                _data.icon = Resources.Load<Sprite>($"SHOP/{a}/{_data.spritePath}");
                shopItemList.Add(_data);
            }

            shopItems = shopItemList.ToArray();

        }

        private string GetDataPath()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    string result;
                    using (StreamReader sr = new StreamReader(Application.dataPath + "/../Debug_DataPath.txt"))
                    {
                        result = sr.ReadLine();
                    }

                    result = result.Replace("\\", "/");
                    if (!result.EndsWith("/")) result += "/";
                    return result;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                    return Application.dataPath + "/../";
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.IPhonePlayer:
                    return Application.persistentDataPath + "/";
                case RuntimePlatform.Android:
                    return "/storage/emulated/0/DR3Viewer/";
                default:
                    Application.Quit();
                    throw new ArgumentException("Unsupported System");
            }
        }

        private string GetCachePath()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                    return dataPath + "cache/";
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return Application.temporaryCachePath + "/";
                case RuntimePlatform.Android:
                    return dataPath + ".cache/";
                default:
                    Application.Quit();
                    throw new ArgumentException("Unsupported System");
            }
        }
    }
}