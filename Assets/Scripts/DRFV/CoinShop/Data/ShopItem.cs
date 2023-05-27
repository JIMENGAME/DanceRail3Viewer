using System;
using System.Collections.Generic;
using DRFV.Global;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace DRFV.CoinShop.Data
{
    public class ShopItem
    {
        [JsonIgnore] public int id;

        [JsonProperty("type")]
        public ShopItemType type;
        [JsonProperty("name")]
        public Dictionary<string, string> name;
        [JsonProperty("info")]
        public Dictionary<string, string> info;
        [JsonProperty("price")]
        public int price;
        [JsonProperty("sprite_path")]
        public string spritePath;

        [JsonProperty("keyword")]
        public string keyword;

        [JsonProperty("condition")]
        public ShopItemConditions condition = 0;

        [JsonProperty("item_value")]
        public int? itemValue;
        
        #region CustomVars

        [JsonProperty("event_key")]
        [CanBeNull] public string eventKey;
        [JsonProperty("level")]
        public int? level;
        [JsonProperty("skill_tier")]
        public int? skillTier;

        #endregion

        [JsonIgnore]
        public bool enabled
        {
            get
            {
                bool result = true;
                if (type == ShopItemType.SONG)
                {
                    result &= (PlayerPrefs.GetInt("unlock_" + keyword, 0) == 0);
                }

                if (condition.HasFlag(ShopItemConditions.PRIME_REQUIRED))
                {
                    result &= PlayerData.Instance.IsPrime;
                }

                if (condition.HasFlag(ShopItemConditions.LEVEL_REQUIRED) && level != null)
                {
                    result &= level <= PlayerData.Instance.level;
                }

                if (condition.HasFlag(ShopItemConditions.SKILL_TIER_REQUIRED) && skillTier != null)
                {
                    result &= skillTier <= PlayerData.Instance.skillTier;
                }

                if (condition.HasFlag(ShopItemConditions.EVENT_REQUIRED) &&
                    !string.IsNullOrEmpty(eventKey))
                {
                    result &= true;
                    // TODO: 活动系统
                }

                return result;
            }
        }
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
            LEVEL_REQUIRED = 1 << 1,
            SKILL_TIER_REQUIRED = 1 << 2,
            EVENT_REQUIRED = 1 << 3
        }
    }
}