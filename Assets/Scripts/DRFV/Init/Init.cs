using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using DRFV.Global;
using DRFV.Global.Managers;
using DRFV.Global.Utilities;
using DRFV.Language;
using DRFV.Pool;
using DRFV.Setting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DRFV.Init
{
    public class Init : MonoBehaviour
    {
        public GameObject loading;

        public Image entryMask;

        public AudioSource music;

        private bool inited = false, loadingStarted = false;

        private static readonly Color CameraColor = new(78 / 255f, 40 / 255f, 78 / 255f, 1f),
            MaskColor = new(31 / 255f, 30 / 255f, 51 / 255f, 1f);

        public bool aprilFoolTest;

        public AudioClip aprilFoolClip;

        // Start is called before the first frame update
        void Start()
        {
            DateTime now = DateTime.Now;
            RuntimeSettingsManager.Instance.isAprilFool = now.Month == 4 && now.Day == 1;
#if UNITY_EDITOR
            RuntimeSettingsManager.Instance.isAprilFool = aprilFoolTest || RuntimeSettingsManager.Instance.isAprilFool;
#endif
            AudioConfiguration audioConfiguration = AudioSettings.GetConfiguration();
            audioConfiguration.dspBufferSize = Application.platform switch
            {
                RuntimePlatform.OSXEditor => 1024,
                RuntimePlatform.OSXPlayer => 1024,
                RuntimePlatform.WindowsPlayer => 1024,
                RuntimePlatform.WindowsEditor => 1024,
                RuntimePlatform.IPhonePlayer => 256,
                RuntimePlatform.Android => 256,
                RuntimePlatform.LinuxPlayer => 1024,
                RuntimePlatform.LinuxEditor => 1024,
                _ => throw new ArgumentOutOfRangeException()
            };
            AudioSettings.Reset(audioConfiguration);
            entryMask.color = RuntimeSettingsManager.Instance.isAprilFool ? CameraColor : MaskColor;
            Camera.main.backgroundColor = RuntimeSettingsManager.Instance.isAprilFool ? MaskColor : CameraColor;
            if (RuntimeSettingsManager.Instance.isAprilFool) music.clip = aprilFoolClip;
            FadeManager.Instance.JumpScene(SceneManager.GetActiveScene().name);
            OBSManager.Instance.Init();
            PoolManager.Instance.RefreshSetting();
            if (Util.Skybox)
            {
                FadeManager.Instance.OnSceneChanged += () =>
                {
                    RenderSettings.skybox = Util.Skybox;
                    DynamicGI.UpdateEnvironment();
                };
            }
            StartCoroutine(LoadEntry());
        }

        IEnumerator LoadEntry()
        {
            MoveOldSettingsStructToNew();
            GlobalSettings currentSettings = GlobalSettings.CurrentSettings;
#if !UNITY_EDITOR
            Application.targetFrameRate = currentSettings.MaxFPS;
#else
            Application.targetFrameRate = -1;
#endif
            yield return new WaitUntil(() => LanguageManager.Instance != null);
            yield return new WaitUntil(() => LanguageManager.Instance.isDone);
            loading.SetActive(false);
            entryMask.DOFade(0f, 1f);
            yield return new WaitForSeconds(1f);
            music.Play();
            inited = true;
        }

        private void MoveOldSettingsStructToNew()
        {
            GlobalSettings currentSettings = GlobalSettings.CurrentSettings;
            if (currentSettings.SaveVersion < 0)
            {
                currentSettings.SaveVersion = 0;
                currentSettings.TapSize = PlayerPrefs.GetInt("TapSize", 2);
                currentSettings.FreeFlickSize = PlayerPrefs.GetInt("FreeFlickSize", 3);
                currentSettings.FlickSize = PlayerPrefs.GetInt("FlickSize", 3);
                currentSettings.TapAlpha = PlayerPrefs.GetInt("TapAlpha", 3);
                currentSettings.FreeFlickAlpha = PlayerPrefs.GetInt("FreeFlickAlpha", 3);
                currentSettings.FlickAlpha = PlayerPrefs.GetInt("FlickAlpha", 3);
                currentSettings.MaxFPS = PlayerPrefs.GetInt("MaxFPS", 60);
                currentSettings.GameEffectParamEQLevel = PlayerPrefs.GetInt("ParamEQ", 10);
                currentSettings.GameEffectGaterLevel = PlayerPrefs.GetInt("Gater", 10);
                currentSettings.GameEffectTap = PlayerPrefs.GetInt("TapEffect", 10);
                currentSettings.FCAPIndicator = PlayerPrefs.GetInt("FCAPIndicator", 0) == 1;
                currentSettings.Offset = PlayerPrefs.GetFloat("noteOffset", 0);
                currentSettings.OBSRecord = PlayerPrefs.GetInt("OBSRecord", 0) == 1;
                currentSettings.SongSpeed = PlayerPrefs.GetInt("SongSpeed", 0);
                currentSettings.NoteSpeed = PlayerPrefs.GetInt("noteSpeed", 12);
                currentSettings.HPBarType = PlayerPrefs.GetInt("HPBarType", 0);
                currentSettings.NoteJudgeRange = PlayerPrefs.GetInt("JudgeTime", 2);
                currentSettings.IsAuto = PlayerPrefs.GetInt("isAuto", 1) == 1;
                currentSettings.IsMirror = PlayerPrefs.GetInt("isMirror", 0) == 1;
                currentSettings.SkillCheckMode = PlayerPrefs.GetInt("SkillCheck", 0) == 1;
                currentSettings.HardMode = PlayerPrefs.GetInt("HardMode", 0) == 1;
                currentSettings.AutoplayHint = PlayerPrefs.GetInt("AutoplayHint", 1);
                PlayerPrefs.DeleteKey("TapSize");
                PlayerPrefs.DeleteKey("FreeFlickSize");
                PlayerPrefs.DeleteKey("FlickSize");
                PlayerPrefs.DeleteKey("TapAlpha");
                PlayerPrefs.DeleteKey("FreeFlickAlpha");
                PlayerPrefs.DeleteKey("FlickAlpha");
                PlayerPrefs.DeleteKey("MaxFPS");
                PlayerPrefs.DeleteKey("ParamEQ");
                PlayerPrefs.DeleteKey("Gater");
                PlayerPrefs.DeleteKey("TapEffect");
                PlayerPrefs.DeleteKey("FCAPIndicator");
                PlayerPrefs.DeleteKey("noteOffset");
                PlayerPrefs.DeleteKey("OBSRecord");
                PlayerPrefs.DeleteKey("SongSpeed");
                PlayerPrefs.DeleteKey("noteSpeed");
                PlayerPrefs.DeleteKey("HPBarType");
                PlayerPrefs.DeleteKey("JudgeTime");
                PlayerPrefs.DeleteKey("isAuto");
                PlayerPrefs.DeleteKey("isMirror");
                PlayerPrefs.DeleteKey("SkillCheck");
                PlayerPrefs.DeleteKey("HardMode");
                PlayerPrefs.DeleteKey("AutoplayHint");
                PlayerPrefs.Save();
            }
            if (currentSettings.SaveVersion < 1)
            {
                currentSettings.SaveVersion++;
                if (currentSettings.NoteJudgeRange >= 2) currentSettings.NoteJudgeRange++;
            }
            GlobalSettings.CurrentSettings = currentSettings;
            GlobalSettings.Save();
        }

        public void Update()
        {
            if (!Input.GetMouseButtonDown(0) || IsOutScreen(Input.mousePosition) || !inited || loadingStarted) return;
            loadingStarted = true;
            music.Stop();
            FadeManager.Instance.JumpScene("main");
        }

        private static bool IsOutScreen(Vector3 position)
        {
            return position.x < 0 || position.x > Screen.width || position.y < 0 || position.y > Screen.height;
        }
    }
}