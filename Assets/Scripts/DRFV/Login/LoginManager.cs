using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DRFV.Global;
using DRFV.Main;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DRFV.Login
{
    public class LoginManager : MonoBehaviour
    {
        public MainManager MainManager;
        public GameObject LoginObj;
        public InputField UrlInput, QQInput, PasswordInput;
        public Toggle RememberMeToggle;
        private bool lastRMIsOn;
        private string qq, password;
        private string urlPrefix;

        private string pfkRememberMe = "RememberMe",
            pfkUrl = "Server_Url",
            pfkQQ = "Server_QQ",
            pfkToken = "LoginToken";

        public Text messageText;


        public void Start()
        {
            if (AccountInfo.Instance.acountStatus != AccountInfo.AcountStatus.UNINITIALIZED)
            {
                EndLogin();
            }
            else
            {
                LoginObj.SetActive(true);
                lastRMIsOn = RememberMeToggle.isOn = PlayerPrefs.GetInt(pfkRememberMe, 0) == 1;
                if (lastRMIsOn)
                {
                    urlPrefix = UrlInput.text = PlayerPrefs.GetString(pfkUrl);
                    qq = QQInput.text = PlayerPrefs.GetString(pfkQQ);
                    PasswordInput.text = "00000000";
                }
            }
            // Debug.Log(ConvertToByteArray("dancerail"));
            // Debug.Log(ConvertToByteArray("lucario"));
            // Debug.Log($"{(byte) 0x1f} {(byte) 0x1e} {(byte) 0x33}");
        }

        private void EndLogin()
        {
            MainManager.Init();
            LoginObj.SetActive(false);
        }

        public void Awake()
        {
            AddInputFieldClickEvent(UrlInput, OnInputFieldClicked);
            AddInputFieldClickEvent(QQInput, OnQQFieldChanged);
            AddInputFieldClickEvent(PasswordInput, OnInputFieldClicked);
        }
#if UNITY_EDITOR
        private string ConvertToByteArray(string content)
        {
            if (content.Equals("")) return "";
            byte[] arr = Encoding.UTF8.GetBytes(content);
            StringBuilder stringBuilder = new StringBuilder("{");
            for (int i = 0; i < arr.Length - 1; i++)
            {
                stringBuilder.Append(arr[i]).Append(", ");
            }

            stringBuilder.Append(arr[^1]).Append("}");
            return stringBuilder.ToString();
        }
#endif

        private void OnVerifyTokenSuccess(byte[] data)
        {
            JObject jObject = HttpRequest.Data2JObject(data);
            try
            {
                string status = jObject["status"].ToString();
                if (status.Equals("success"))
                {
                    AccountInfo.Instance.acountStatus = AccountInfo.AcountStatus.LOGINED;
                    AccountInfo.Instance.username = jObject["username"].ToString();
                    HttpRequest.Instance.Get(
                        urlPrefix +
                        $"users/{jObject["qq"].ToObject<ulong>()}/avatar.{jObject["avatar_ext"]}?token={AccountInfo.Instance.loginToken}",
                        GetAvatarCallback, GetAvatarFailedCallback);
                }
                else
                {
                    string error = jObject["error"].ToString();
                    ShowMessage(error switch
                    {
                        "NullToken" => "本地账户不存在，请重新登录",
                        "TokenExpired" => "本地账户信息已过期，请重新登录",
                        "TokenInvalid" => "本地账户信息无效，请重新登录",
                        "QQNotExists" => "本地账户不存在，请重新登录",
                        _ => "使用本地账户登录时发生未知错误，请重新登录"
                    });
                    lastRMIsOn = false;
                    QQInput.text = "";
                    PasswordInput.text = "";
                }
            }
            catch (Exception)
            {
                ShowMessage("使用本地账户登录时发生未知错误，请重新登录");
                lastRMIsOn = false;
                QQInput.text = "";
                PasswordInput.text = "";
            }
        }

        private void DemoMode()
        {
            AccountInfo.Instance.acountStatus = AccountInfo.AcountStatus.DEMO;
            AccountInfo.Instance.username = "Demo";
            AccountInfo.Instance.loginToken = "";
            AccountInfo.Instance.urlPrefix = urlPrefix;
            AccountInfo.Instance.avatar = Resources.Load<Sprite>("DemoAvatar");
            EndLogin();
        }

        public void Login()
        {
            ShowMessage("正在登录...");
#if UNITY_IOS || UNITY_EDITOR
            if (QQInput.text.Trim() == "221991527" && PasswordInput.text == "demo")
            {
                DemoMode();
                return;
            }
#endif
            if (lastRMIsOn && PlayerPrefs.HasKey(pfkToken) && PlayerPrefs.HasKey(pfkUrl))
            {
                AccountInfo.Instance.urlPrefix = PlayerPrefs.GetString(pfkUrl);
                AccountInfo.Instance.loginToken = PlayerPrefs.GetString(pfkToken);
                HttpRequest.Instance.Get(
                    AccountInfo.Instance.urlPrefix + "?type=verify_token&token=" + AccountInfo.Instance.loginToken,
                    OnVerifyTokenSuccess, LoginFailedCallback);
                return;
            }

            if (!lastRMIsOn)
            {
                string url = UrlInput.text.Trim();
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "http://" + url;
                }

                try
                {
                    url = new Uri(url).AbsoluteUri;
                }
                catch (Exception)
                {
                    // ignored
                }

                urlPrefix = url;

                if (string.IsNullOrEmpty(QQInput.text) || string.IsNullOrEmpty(PasswordInput.text))
                {
                    ShowMessage("错误：QQ或密码为空");
                    return;
                }

                qq = QQInput.text.Trim();
                password = SaltedMD5(PasswordInput.text);
            }

            HttpRequest.Instance.Get(
                urlPrefix +
                $"?type=login&qq={qq}&password={password}",
                LoginCallback,
                LoginFailedCallback,
                null,
                10);
        }

        private void LoginCallback(byte[] data)
        {
            JObject jObject = HttpRequest.Data2JObject(data);
            try
            {
                switch (jObject["status"].ToString())
                {
                    case "success":
                        AccountInfo.Instance.acountStatus = AccountInfo.AcountStatus.LOGINED;
                        AccountInfo.Instance.username = jObject["username"].ToString();
                        AccountInfo.Instance.loginToken = jObject["token"].ToString();
                        AccountInfo.Instance.urlPrefix = urlPrefix;
                        PlayerPrefs.SetInt(pfkRememberMe, RememberMeToggle.isOn ? 1 : 0);
                        if (RememberMeToggle.isOn)
                        {
                            PlayerPrefs.SetString(pfkUrl, urlPrefix);
                            PlayerPrefs.SetString(pfkQQ, qq);
                            PlayerPrefs.SetString(pfkToken, AccountInfo.Instance.loginToken);
                        }

                        PlayerPrefs.Save();

                        HttpRequest.Instance.Get(
                            urlPrefix +
                            $"users/{qq}/avatar.{jObject["avatar_ext"]}?token={AccountInfo.Instance.loginToken}",
                            GetAvatarCallback, GetAvatarFailedCallback);
                        break;
                    case "failed":
                        string error = jObject["error"].ToString();
                        switch (error)
                        {
                            case "QQNotExists":
                                ShowMessage("错误：用户不存在");
                                break;
                            case "WrongPassword":
                                ShowMessage("错误：密码不正确");
                                break;
                            default:
                                ShowMessage("未知错误：" + error);
                                break;
                        }

                        break;
                    default:
                        ShowMessage("错误：非法数据");
                        break;
                }
            }
            catch (Exception)
            {
                ShowMessage("错误：非法数据");
            }
        }

        private void AddInputFieldClickEvent(InputField inputField, UnityAction<BaseEventData> selectEvent) //可以在Awake中调用
        {
            var eventTrigger = inputField.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = inputField.gameObject.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry onClick = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Select
            };

            onClick.callback.AddListener(selectEvent);
            eventTrigger.triggers.Add(onClick);
        }

        private void OnInputFieldClicked(BaseEventData data)
        {
            if (lastRMIsOn)
            {
                PasswordInput.text = "";
                lastRMIsOn = false;
            }
        }

        private void OnQQFieldChanged(BaseEventData data)
        {
            PasswordInput.text = "";
            lastRMIsOn = false;
        }

        private void GetAvatarCallback(byte[] data)
        {
            AccountInfo.Instance.avatar = GetSprite(data);
            EndLogin();
        }

        private void GetAvatarFailedCallback()
        {
            ShowMessage("错误：无法获取头像，请重试");
        }

        private void LoginFailedCallback()
        {
            ShowMessage("错误：无法连接至服务器");
        }

        public void Offline()
        {
            AccountInfo.Instance.acountStatus = AccountInfo.AcountStatus.OFFLINE;
            EndLogin();
        }

        private void ShowMessage(string message)
        {
            messageText.text = message;
        }

        public Sprite GetSprite(byte[] bytes)
        {
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(bytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            return sprite;
        }

        private string SaltedMD5(string password)
        {
            byte[] data = Encoding.UTF8.GetBytes(password);
            byte[] salt1 = { 100, 97, 110, 99, 101, 114, 97, 105, 108 }; // dancerail的UTF8
            byte[] salt2 = { 31, 30, 51 }; // 0x1f 0x1e 0x33
            byte[] salt3 = { 108, 117, 99, 97, 114, 105, 111 }; // lucario的UTF8
            byte[] output = new byte[data.Length + salt1.Length + salt2.Length + salt3.Length];
            int i = 0;
            for (; i < salt1.Length; i++)
            {
                output[i] = salt1[i];
            }

            output[++i] = data[0];
            i++;
            for (int j = 0; j < salt2.Length; j++, i++)
            {
                output[i] = salt2[j];
            }

            i++;
            for (int j = 1; j < data.Length; j++, i++)
            {
                output[i] = data[j];
            }

            output = output.Concat(salt3).ToArray();
            byte[] md5 = MD5.Create().ComputeHash(output);
            StringBuilder sBuilder = new StringBuilder();
            foreach (var t in md5)
            {
                sBuilder.Append(t.ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}