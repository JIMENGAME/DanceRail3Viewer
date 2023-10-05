using DRFV.Global;
using DRFV.Global.Utilities;
using DRFV.Select;
using DRFV.Setting;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Dankai
{
    public class Stage : MonoBehaviour
    {
        [SerializeField] private Text tId, tTier, tTitle, tScore, tDetail;
        [SerializeField] private Image iCover, iTier;

        public void Init(int id, SongDataContainer songDataContainer, DankaiResultData dankaiResultData)
        {
            tId.text = (id + 1).ToString();
            tTier.text = songDataContainer.selectedDiff + "";
            iCover.sprite = songDataContainer.songData.cover;
            int wakuId = songDataContainer.selectedDiff switch
            {
                < 6 => 1,
                < 11 => 2,
                < 16 => 3,
                < 21 => 4,
                < 26 => 5,
                _ => 1
            };
            iTier.sprite = Resources.Load<Sprite>($"GENERAL/tier_waku0{wakuId}");
            tTitle.text = songDataContainer.songData.songName;
            tScore.text = GameUtil.ParseScore(Mathf.RoundToInt(dankaiResultData.score), SCORE_TYPE.ORIGINAL);
            tDetail.text =
                $"<color=#FF7>{dankaiResultData.pj}</color>/<color=#F97>{dankaiResultData.p}</color>/<color=#7F7>{dankaiResultData.g}</color>/<color=#F77>{dankaiResultData.m}</color>";
            if (tTitle.preferredWidth > 400f)
            {
                tTitle.transform.localScale = new Vector2(400f / tTitle.preferredWidth, 1f);
            }
        }
    }
}