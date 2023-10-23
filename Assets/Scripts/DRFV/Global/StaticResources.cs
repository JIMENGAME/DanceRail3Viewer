using System;
using System.Collections.Generic;
using System.IO;
using DRFV.CoinShop.Data;
using DRFV.Global.Managers;
using DRFV.Global.Utilities;
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
            InitPaths();
            Util.Init();
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
            
            if (!Directory.Exists(dataPath + "resources"))
            {
                Directory.CreateDirectory(dataPath + "resources");
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

        private void InitPaths()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    string result;
                    using (StreamReader sr = new StreamReader(Application.dataPath + "/../Debug_DataPath.txt"))
                    {
                        result = sr.ReadLine().Replace("\\", "/");
                    }

                    if (!result.EndsWith("/")) result += "/";
                    dataPath = result;
                    cachePath = dataPath + "cache/";
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                    dataPath = Application.dataPath + "/../";
                    cachePath = dataPath + "cache/";
                    break;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.IPhonePlayer:
                    dataPath = Application.persistentDataPath + "/";
                    cachePath = Application.temporaryCachePath + "/";
                    break;
                case RuntimePlatform.Android:
                    dataPath =  Path.Combine(Path.GetFullPath(Application.persistentDataPath + "/../../../.."), "DR3Viewer") + "/";
                    cachePath = dataPath + ".cache/";
                    break;
                default:
                    Application.Quit();
                    throw new ArgumentException("Unsupported System");
            }
        }
    }
}