using System;
using DRFV.Global.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Global.Components
{
    public class PlayerDataComponent : MonoBehaviour
    {
        public Text exp, coin, purpleCard, skillTier, prime, changeNicknameCard;

        public void Refresh()
        {
            exp.text = $"当前经验：{PlayerData.Instance.level}级，距下一级还有{PlayerData.Instance.NextLevel()}点经验";
            coin.text = $"薯片：{PlayerData.Instance.coin}片";
            purpleCard.text = $"やんで片：{PlayerData.Instance.purpleCard}片";
            changeNicknameCard.text = $"改名卡：{PlayerData.Instance.changeNickNameCard}片";
            skillTier.text = $"技能鉴定等级：{PlayerData.Instance.skillTier}级";
            if (PlayerData.Instance.PrimeEndTime < Util.DateTimeToTimeStamp(DateTime.Now))
            {
                prime.gameObject.SetActive(false);
            }
            else
            {
                prime.text = $"会员到期时间：{Util.TimeStampToDateTime(PlayerData.Instance.PrimeEndTime):yyyy-MM-dd HH:mm:ss}";
                prime.gameObject.SetActive(true);
            }
        }
    }
}