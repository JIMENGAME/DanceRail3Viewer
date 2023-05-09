using System;
using System.Collections;
using DG.Tweening;
using DRFV.Global;
using DRFV.Language;
using DRFV.Setting;
using UnityEngine;
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
            Util.Init();
            DateTime now = DateTime.Now;
            bool isAprilFool = now.Month == 4 && now.Day == 1;
#if UNITY_EDITOR
            isAprilFool = aprilFoolTest || isAprilFool;
#endif
            entryMask.color = isAprilFool ? CameraColor : MaskColor;
            Camera.main.backgroundColor = isAprilFool ? MaskColor : CameraColor;
            if (isAprilFool) music.clip = aprilFoolClip;
            StartCoroutine(LoadEntry());
        }

        IEnumerator LoadEntry()
        {
            GlobalSettings currentSettings = GlobalSettings.CurrentSettings;
            if (currentSettings.SaveVersion < 0)
            {
                MoveOldSettingsStructToNew();
                currentSettings = GlobalSettings.CurrentSettings;
            }

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
            GlobalSettings.CurrentSettings = currentSettings;
        }

        public void Update()
        {
            if (!Input.GetMouseButtonDown(0) || IsOutScreen(Input.mousePosition) || !inited || loadingStarted) return;
            loadingStarted = true;
            music.Stop();
            FadeManager.Instance.LoadScene("main");
        }

        private static bool IsOutScreen(Vector3 position)
        {
            return position.x < 0 || position.x > Screen.width || position.y < 0 || position.y > Screen.height;
        }
    }
}