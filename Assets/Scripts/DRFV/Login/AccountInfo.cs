using System;
using DRFV.inokana;
using DRFV.Shop;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Login
{
    public class AccountInfo : MonoSingleton<AccountInfo>
    {
        public AcountStatus acountStatus;
        public string username;
        public Sprite avatar;
        public string loginToken;
        public string urlPrefix;

        protected override void OnAwake()
        {
            Init();
        }

        public void Init()
        {
            acountStatus = AcountStatus.UNINITIALIZED;
            username = "Offline...";
            avatar = Resources.Load<Sprite>("placeholder");
            loginToken = "";
            urlPrefix = "";
            songlistGot = false;
            SongInfos = Array.Empty<SongInfo>();
        }

        public void UpdateAccountPanel()
        {
            GameObject accountPanel = GameObject.FindWithTag("AccountPanel");
            if (accountPanel == null) return;
            accountPanel.transform.Find("AvatarFrame/Avatar").GetComponent<Image>().sprite = avatar;
            accountPanel.transform.Find("UserName").GetComponent<Text>().text = username;
        }

        public enum AcountStatus
        {
            UNINITIALIZED = 0,
            OFFLINE = 1,
            LOGINED = 2,
            DEMO = 3
        }

        public bool songlistGot;
        public SongInfo[] SongInfos;
    }
}
