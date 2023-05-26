using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DRFV.CoinShop.Data;
using DRFV.inokana;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Application = UnityEngine.Application;

namespace DRFV.Global
{
    public class StaticResources : MonoSingleton<StaticResources>
    {
        public string dataPath;
        public string cachePath;
        private static string localizationId = "zh-cn";
        public ShopItem[] shopItems;

        // Start is called before the first frame update
        protected override void OnAwake()
        {
            DontDestroyOnLoad(gameObject);
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

            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_WIN
            File.SetAttributes(cachePath, FileAttributes.Hidden);
#endif
                JArray shopItemsJArray= JArray.Parse(Resources.Load<TextAsset>("SHOP/shop_items_list").text);
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
                    _data.enabled = true;
                    if (_data.type == ShopItem.ShopItemType.SONG)
                    {
                        _data.enabled &= (PlayerPrefs.GetInt("unlock_" + _data.keyword, 0) == 0);
                    }

                    if (_data.condition.HasFlag(ShopItem.ShopItemConditions.PRIME_REQUIRED))
                    {
                        _data.enabled &= true;
                        // TODO: 会员系统
                    }

                    if (_data.condition.HasFlag(ShopItem.ShopItemConditions.TIER_REQUIRED) && _data.tier != null)
                    {
                        _data.enabled &= _data.tier <= int.MaxValue;
                        // TODO: 等级系统
                    }

                    if (_data.condition.HasFlag(ShopItem.ShopItemConditions.EVENT_REQUIRED) &&
                        !string.IsNullOrEmpty(_data.eventKey))
                    {
                        _data.enabled &= true;
                        // TODO: 活动系统
                    }
                    shopItemList.Add(_data);
                }

                shopItems = shopItemList.ToArray();
            }
        }

        private static string GetDataPath()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
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

        private static string GetCachePath()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                    return Application.dataPath + "/../cache/";
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return Application.temporaryCachePath + "/";
                case RuntimePlatform.Android:
                    return "/storage/emulated/0/DR3Viewer/.cache/";
                default:
                    Application.Quit();
                    throw new ArgumentException("Unsupported System");
            }
        }
    }
}