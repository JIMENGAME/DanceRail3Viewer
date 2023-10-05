using System;
using DG.Tweening;
using DRFV.Enums;
using DRFV.Game;
using DRFV.Global;
using DRFV.Global.Managers;
using DRFV.Global.Utilities;
using DRFV.Select;
using DRFV.Setting;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Story
{
    public class StoryContentManager : MonoBehaviour
    {
        public Text tStory;
        public int chapter = 0, section = 0;
        public int selectedPage = 0, totalPage = 1;
        public Button bPrev, bNext, bBack, bComplete;
        public RectTransform panel;
        private string unlock;
        private JObject unlocks;
        private int totalSection;

        public GameObject Lock;

        public Text tLock;

        public GameObject[] goToUnlock;

        private RectTransform parent, rectTransform;

        public Button bLockSection;
        //type 0：打歌解锁

        public void Start()
        {
            bPrev.onClick.AddListener(Prev);
            bNext.onClick.AddListener(Next);
            bBack.onClick.AddListener(Back);
            bComplete.onClick.AddListener(Back);
            bLockSection.onClick.AddListener(LockSection);
        }

        public void Init(StoryData storyData)
        {
            bool unlocked = storyData.unlock == "" || PlayerPrefs.GetInt("story_" + storyData.unlock, 0) == 1;
            chapter = storyData.chapter;
            section = storyData.section;
            unlock = storyData.unlock;
            totalSection = storyData.totalSection;
            selectedPage = 0;
            string lastSection = chapter == 0 ? "" :
                section == 0 ? chapter - 1 + "" : chapter + "." + (section - 1);
            bool lastRead = lastSection == "" || PlayerPrefs.GetInt(
                    "story_read_" + lastSection, 0) ==
                1 || storyData.ignoreLastRead;
            totalPage = unlocked && lastRead ? storyData.totalPage : 0;
            parent = tStory.transform.parent.gameObject.GetComponent<RectTransform>();
            rectTransform = tStory.gameObject.GetComponent<RectTransform>();
            parent.anchoredPosition =
                new Vector2(parent.anchoredPosition.x, 0);
            bLockSection.gameObject.SetActive(unlock != "" && unlocked);
            if (!lastRead)
            {
                tLock.text = "<size=75>未解锁</size>\n解锁条件（之一）: 阅读上一SECTION";
                for (int i = 0; i < goToUnlock.Length; i++)
                {
                    goToUnlock[i].SetActive(false);
                }

                tStory.text = "";
                Lock.SetActive(true);
                bPrev.gameObject.SetActive(false);
                bBack.gameObject.SetActive(true);
                bNext.gameObject.SetActive(false);
                bComplete.gameObject.SetActive(false);
                return;
            }

            if (!unlocked)
            {
                if (unlocks == null)
                {
                    unlocks = JObject.Parse(ExternalResources.LoadText("STORY/unlocks").text);
                }

                JObject jObject = unlocks[unlock].ToObject<JObject>();
                tLock.text = "<size=75>未解锁</size>\n解锁条件: " + jObject["display"];
                int type = jObject["type"].ToObject<int>();
                for (int i = 0; i < goToUnlock.Length; i++)
                {
                    goToUnlock[i].SetActive(i == type);
                }

                tStory.text = "";
                Lock.SetActive(true);
                bPrev.gameObject.SetActive(false);
                bBack.gameObject.SetActive(true);
                bNext.gameObject.SetActive(false);
                bComplete.gameObject.SetActive(false);
                return;
            }

            Lock.SetActive(false);
            string key = "story_read_" + (totalSection == section + 1 ? chapter + "" : chapter + "." + section);
            if (PlayerPrefs.GetInt(key, 0) == 0)
            {
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();
            }

            UpdateButton();
            UpdateText();
        }

        private void UpdateText()
        {
            TextAsset storyTextAsset = ExternalResources.LoadText($"STORY/STORIES/{chapter}-{section}.{selectedPage}");
            tStory.text = storyTextAsset == null ? "如果你看到这串文字说明出bug了，试试再点一次SECTION" : storyTextAsset.text;
            float preferredHeight = tStory.preferredHeight;
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, preferredHeight + 20);
            parent.sizeDelta = new Vector2(parent.sizeDelta.x, preferredHeight + 40);
        }

        private void UpdateButton()
        {
            bool hasPrev = selectedPage > 0;
            bPrev.gameObject.SetActive(hasPrev);
            bBack.gameObject.SetActive(!hasPrev);
            bool hasNext = selectedPage < totalPage - 1;
            bNext.gameObject.SetActive(hasNext);
            bComplete.gameObject.SetActive(!hasNext);
        }

        private void Prev()
        {
            selectedPage -= 1;
            if (selectedPage < 0) selectedPage = 0;
            UpdateButton();
            UpdateText();
        }

        private void Next()
        {
            selectedPage += 1;
            if (selectedPage >= totalPage) selectedPage = totalPage - 1;
            UpdateButton();
            UpdateText();
        }

        public void Back()
        {
            panel.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutExpo);
        }

        private void LockSection()
        {
            PlayerPrefs.DeleteKey("story_" + unlock);
            PlayerPrefs.Save();
            Back();
        }

        public void UnlockByPlaySong()
        {
            CheckDataContainers.CleanSongDataContainer();
            JObject jo = unlocks[unlock].ToObject<JObject>();
            GameObject go = new GameObject { name = "StoryChallengeContainer", tag = "SongData" };
            StoryChallengeContainer storyChallengeContainer = go.AddComponent<StoryChallengeContainer>();
            bool hasOverride = jo.ContainsKey("music_override");
            if (hasOverride) hasOverride = jo["music_override"].ToObject<bool>();
            int noteJudgeRange = 2;
            if (jo.ContainsKey("judge_range"))
            {
                noteJudgeRange = jo["judge_range"].ToObject<int>();
            }

            GameSide gameSide = GameSide.DARK;
            if (jo.ContainsKey("side"))
            {
                gameSide = (GameSide)jo["side"].ToObject<int>();
            }

            float songSpeed = 1.0f;
            if (jo.ContainsKey("song_speed")) songSpeed = jo["song_speed"].ToObject<float>();
            storyChallengeContainer.songData = new TheSelectManager.SongData
            {
                keyword = jo["keyword"].ToString(),
                songName = jo["title"].ToString(),
                songArtist = jo["artist"].ToString(),
                bpm = jo["bpm"].ToString(),
                hards = new[] { jo["tier"].ToObject<int>() }
            };
            bool hasCustomMover = false, hasCustomHeight = false;
            if (jo.ContainsKey("has_custom_mover"))
            {
                hasCustomMover = jo["has_custom_mover"].ToObject<bool>();
            }

            if (jo.ContainsKey("has_custom_height"))
            {
                hasCustomHeight = jo["has_custom_height"].ToObject<bool>();
            }

            bool hasTextBeforeStart = false;
            if (jo.ContainsKey("has_text_before_start"))
            {
                hasTextBeforeStart = jo["has_text_before_start"].ToObject<bool>();
            }

            int maxInput = 20;
            if (jo.ContainsKey("max_input"))
            {
                maxInput = jo["max_input"].ToObject<int>();
            }

            bool hasVideo = false;
            if (jo.ContainsKey("has_video"))
            {
                hasVideo = jo["has_video"].ToObject<bool>();
            }

            string tierIdentifier = "Tier";
            if (jo.ContainsKey("custom_tier_identifier"))
            {
                tierIdentifier = jo["custom_tier_identifier"].ToString();
            }

            Color? customTierColor = null;
            if (jo.ContainsKey("custom_tier_color"))
            {
                var jToken = jo["custom_tier_color"];
                try
                {
                    int o = jToken.ToObject<int>();
                    customTierColor = Util.GetTierColor(o);
                }
                catch (Exception)
                {
                    customTierColor = Util.HexToColor(jToken.ToString());
                }
            }

            bool ratingPlus = false;
            if (jo.ContainsKey("rating_plus"))
            {
                ratingPlus = jo["rating_plus"].ToObject<bool>();
            }

            storyChallengeContainer.music = ExternalResources.LoadAudioClip(
                $"STORY/SONGS/{storyChallengeContainer.songData.keyword}{(hasOverride ? $".{storyChallengeContainer.songData.hards[0]}" : "")}");
            storyChallengeContainer.songData.cover =
                ExternalResources.LoadSprite("STORY/SONGS/" + storyChallengeContainer.songData.keyword);
            storyChallengeContainer.isComputer = Application.platform switch
            {
                RuntimePlatform.WindowsEditor => true,
                RuntimePlatform.WindowsPlayer => true,
                RuntimePlatform.LinuxEditor => true,
                RuntimePlatform.LinuxPlayer => true,
                _ => false
            };
            storyChallengeContainer.selectedDiff = storyChallengeContainer.songData.hards[0];
            storyChallengeContainer.saveAudio = false;
            storyChallengeContainer.songSpeed = songSpeed;
            storyChallengeContainer.NoteJudgeRange = noteJudgeRange;
            storyChallengeContainer.unlock = unlock;
            storyChallengeContainer.gameSide = gameSide;
            storyChallengeContainer.hasCustomMover = hasCustomMover;
            storyChallengeContainer.hasCustomHeight = hasCustomHeight;
            storyChallengeContainer.timeToVideoShow = hasVideo ? jo["time_to_video_show"].ToObject<float>() : -1f;
            storyChallengeContainer.timeToEnter = hasVideo ? jo["time_to_enter"].ToObject<float>() : -1f;
            storyChallengeContainer.hasTextBeforeStart = hasTextBeforeStart;
            storyChallengeContainer.tierIdentifier = tierIdentifier;
            storyChallengeContainer.customTierColor = customTierColor;
            storyChallengeContainer.ratingPlus = ratingPlus;
            InputManager.TOUCH_MAX = maxInput;
            DontDestroyOnLoad(storyChallengeContainer);
#if !UNITY_EDITOR
            hasVideo = hasVideo &&
                       PlayerPrefs.GetInt($"story_video_{storyChallengeContainer.songData.keyword}", 0) == 0;
#endif
            FadeManager.Instance.LoadScene(hasVideo ? "video" : "game");
        }
    }
}