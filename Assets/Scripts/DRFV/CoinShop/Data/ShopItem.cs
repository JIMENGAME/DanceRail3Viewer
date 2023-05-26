using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace DRFV.CoinShop.Data
{
    public class ShopItem
    {
        [JsonIgnore] public int id;

        public ShopItemType type;
        public string name;
        public string info;
        public int price;
        public string spritePath;

        public string keyword;

        public ShopItemConditions condition = 0;
        
        #region CustomVars

        [CanBeNull] public string eventKey;
        public int? tier;

        #endregion

        [JsonIgnore] public bool enabled;
        [JsonIgnore] public Sprite icon;

        public enum ShopItemType
        {
            ITEM = 0,
            SONG = 1
        }

        [Flags]
        public enum ShopItemConditions
        {
            PRIME_REQUIRED = 1 << 0,
            TIER_REQUIRED = 1 << 1,
            EVENT_REQUIRED = 1 << 2
        }
    }
}