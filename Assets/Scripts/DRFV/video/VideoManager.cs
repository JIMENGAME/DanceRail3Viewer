using System;
using System.Collections;
using System.Diagnostics;
using DG.Tweening;
using DRFV.Enums;
using DRFV.Global;
using DRFV.Global.Managers;
using DRFV.inokana;
using DRFV.Story;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace DRFV.video
{
    public class VideoManager : MonoBehaviour
    {
        public VideoPlayer videoPlayer;
        public AudioSource audioSource;

        public Sprite tierBgLight;
        public Sprite tierBgDark;
        public Sprite coverBgLight;
        public Sprite coverBgDark;

        private readonly Stopwatch _stopwatch = new();

        public float timeToShow, timeToEnter;

        public bool enter;

        public GameObject entrancePanel;
        public Image coverBG;
        public Image cover;
        public Image tierBG;
        public Text tier;
        private string key;

        // Start is called before the first frame update
        void Start()
        {
            if (!Stopwatch.IsHighResolution)
            {
                NotificationBarManager.Instance.Show("计时器精准度可能有问题，请与inokana取得联系");
            }

            GameObject songDataObject = GameObject.FindWithTag("SongData");
            if (!songDataObject) FadeManager.Instance.Back();
            StoryChallengeContainer storyChallengeContainer = songDataObject.GetComponent<StoryChallengeContainer>();
            if (!storyChallengeContainer) FadeManager.Instance.Back();
            key = $"story_video_{storyChallengeContainer.songData.keyword}";
#if false
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = Resources.Load<VideoClip>($"STORY/VIDEOS/{storyChallengeContainer.songData.keyword}");
            videoPlayer.url = "";
#else
            videoPlayer.source = VideoSource.Url;
            videoPlayer.clip = null;
            videoPlayer.url = ExternalResources.GetVideoClipPath($"STORY/VIDEOS/{storyChallengeContainer.songData.keyword}");
#endif
            audioSource.clip =
                ExternalResources.LoadAudioClip($"STORY/VIDEOS/{storyChallengeContainer.songData.keyword}");
            Sprite coverSpr = ExternalResources.LoadSprite($"STORY/SONGS/{storyChallengeContainer.songData.keyword}");
            if (coverSpr)
            {
                cover.sprite = coverSpr;
            }

            coverBG.sprite = storyChallengeContainer.gameSide switch
            {
                GameSide.LIGHT => coverBgLight,
                GameSide.DARK => coverBgDark,
                GameSide.COLORLESS => coverBgLight,
                _ => throw new ArgumentOutOfRangeException()
            };
            tierBG.sprite = storyChallengeContainer.gameSide switch
            {
                GameSide.LIGHT => tierBgLight,
                GameSide.DARK => tierBgDark,
                GameSide.COLORLESS => tierBgLight,
                _ => throw new ArgumentOutOfRangeException()
            };
            tier.text = storyChallengeContainer.selectedDiff + (storyChallengeContainer.ratingPlus ? "+" : "");
            if (!videoPlayer.clip || !audioSource.clip) FadeManager.Instance.JumpScene("game");
            timeToShow = storyChallengeContainer.timeToVideoShow * 1000f;
            timeToEnter = storyChallengeContainer.timeToEnter * 1000f;
            videoPlayer.targetTexture.Release();
            videoPlayer.renderMode = VideoRenderMode.APIOnly;
            _stopwatch.Reset();
            videoPlayer.errorReceived += (_, _) => FadeManager.Instance.JumpScene("game");
            videoPlayer.prepareCompleted += source =>
            {
                StartCoroutine(StartPlay());
                RectTransform rectTransform = source.GetComponent<RectTransform>();
                videoPlayer.targetTexture.Release();
                int width = source.targetTexture.width = (int)source.width;
                int height = source.targetTexture.height = (int)source.height;
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                rectTransform.sizeDelta = Screen.width * 1f / Screen.height < width * 1f / height
                    ? new Vector2(Screen.width, height * 1f * Screen.width / width)
                    : new Vector2(width * 1f * Screen.height / height, Screen.height);
            };
            videoPlayer.Prepare();
        }

#if UNITY_EDITOR
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) FadeManager.Instance.JumpScene("game");
        }
#endif

        private IEnumerator StartPlay()
        {
            if (FadeManager.Instance.isFading()) yield return new WaitWhile(() => FadeManager.Instance.isFading());
            _stopwatch.Start();
            audioSource.Play();
            videoPlayer.Play();
            yield return null;
            float offset = audioSource.time * 1000f - _stopwatch.ElapsedMilliseconds;
            // if (MathF.Abs(audioSource.time - (float) videoPlayer.time) > 0.1f) videoPlayer.time = audioSource.time;
            EnableEntrance();
            if (!(timeToShow < 0)) yield return new WaitWhile(() => _stopwatch.ElapsedMilliseconds < timeToShow);
            ShowEntrance();
            yield return new WaitWhile(() => !enter && _stopwatch.ElapsedMilliseconds + offset < timeToEnter);
            DOVirtual.Float(1f, 0f, 1f, value =>
            {
                if (audioSource) audioSource.volume = value;
            }).SetEase(Ease.Linear);
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            FadeManager.Instance.JumpScene("game");
        }

        private void EnableEntrance()
        {
            if (timeToShow < 0) return;
            entrancePanel.SetActive(true);
        }

        private void ShowEntrance()
        {
            if (timeToShow < 0) return;
            coverBG.DOFade(1f, 0.6f).SetEase(Ease.Linear);
            cover.DOFade(1f, 0.6f).SetEase(Ease.Linear);
            tierBG.DOFade(1f, 0.6f).SetEase(Ease.Linear);
            tier.DOFade(1f, 0.6f).SetEase(Ease.Linear);
            StartCoroutine(ListenClickEvent());
        }

        private IEnumerator ListenClickEvent()
        {
            while (!enter)
            {
                if (Input.GetKeyDown(KeyCode.Space) ||
                    Input.GetMouseButton(0) && !IsOutScreen(Input.mousePosition)) EnterGame();
                yield return null;
            }
        }

        private void EnterGame()
        {
            if (_stopwatch.ElapsedMilliseconds < timeToShow + 1.6f) return;
            enter = true;
        }

        private static bool IsOutScreen(Vector3 position)
        {
            return position.x < 0 || position.x > Screen.width || position.y < 0 || position.y > Screen.height;
        }
    }
}