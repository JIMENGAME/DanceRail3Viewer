using System;
using DRFV.Global;
using DRFV.Global.Utilities;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Select.Components
{
    public class DiffButton : MonoBehaviour
    {
        private int _idx;
        public Text tierText;
        public Text highScore;
        public Image backGroundImage;
        public int tier;
        public GameObject highlightAnimator;

        public void Init(int tier, int idx)
        {
            this.tier = tier;
            _idx = idx;
            tierText.text = tier == 0 ? "?" : tier + "";
            backGroundImage.color = tierText.color = Util.GetTierColor(tier);
            string md5 = Util.GetMd5OfChart(TheSelectManager.Instance.songDataNow.keyword, tier);
            if (PlayerPrefs.HasKey("SongScore_" + md5))
            {
                JObject jObject = JObject.Parse(PlayerPrefs.GetString("SongScore_" + md5));
                highScore.gameObject.SetActive(true);
                highScore.text =
                    "Hi:" + jObject["score"].ToObject<int>().ToString("N0");
                highScore.color = jObject["type"].ToObject<string>() switch
                {
                    "fd" => new Color(1f, 1f, 1f),
                    "cp" => new Color(1f, 1f, 1f),
                    "fc" => Color.green,
                    "ap" => new Color(1f, 0.5f, 0.5f),
                    "apj" => Color.yellow,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public void OnClicked()
        {
            TheSelectManager.Instance.UpdateDiff(_idx);
        }
    }
}