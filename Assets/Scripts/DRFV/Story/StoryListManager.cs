using System;
using DRFV.Global;
using DRFV.Global.Managers;
using DRFV.Login;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Story
{
    public class StoryListManager : MonoBehaviour
    {
        public RectTransform panel;

        public StoryContentManager storyContentManager;

        public int chapter;

        private JArray storyListJo;

        public GameObject sectionItemPrefab;

        public RectTransform songlistContentPanel;

        public Text tChapter;

        public Button bPrev, bNext;

        public GameObject LockedPanel;
        public Text tLockedCondition;
        public int sectionCount;

        // Start is called before the first frame update
        void Start()
        {
            bPrev.onClick.AddListener(Prev);
            bNext.onClick.AddListener(Next);
            panel.anchoredPosition = Vector2.zero;
            AccountInfo.Instance.UpdateAccountPanel();

            storyListJo = JArray.Parse(Resources.Load<TextAsset>("STORY/storylist").text);
            if (storyListJo.Count == 0)
            {
                Back();
                return;
            }

            chapter = PlayerPrefs.GetInt("LastStorySelect", 0);
            if (chapter >= storyListJo.Count) chapter = 0;
            UpdateButtons();
            RefreshStoryList();
        }

        private void RefreshStoryList()
        {
            if (storyListJo == null) return;
            if (songlistContentPanel.childCount > 0)
            {
                for (int i = 0; i < songlistContentPanel.childCount; i++)
                {
                    Destroy(songlistContentPanel.GetChild(i).gameObject);
                }
            }

            LockedPanel.SetActive(false);
            JObject chapterJo = storyListJo[chapter].ToObject<JObject>();
            if (chapterJo == null)
            {
                Back();
                return;
            }

            if (chapterJo.ContainsKey("unlock") && chapterJo.ContainsKey("unlock_disp"))
            {
                if (!CheckSectionUnlocked(chapterJo["unlock"].ToString()))
                {
                    tChapter.text = "Chapter " + chapter + ": ???";
                    tLockedCondition.text = "<size=75>未解锁</size>\n解锁条件：" + chapterJo["unlock_disp"];
                    LockedPanel.SetActive(true);
                    return;
                }

                LockedPanel.SetActive(false);
            }
            else
            {
                LockedPanel.SetActive(false);
            }
            
            tChapter.text = "Chapter " + chapter + ": " + chapterJo["name"];
            JArray sections = chapterJo["section"].ToObject<JArray>();
            sectionCount = sections.Count;
            for (int i = 0; i < sectionCount; i++)
            {
                JObject section = sections[i].ToObject<JObject>();
                string unlockedStr = "";
                if (section.ContainsKey("unlock"))
                {
                    unlockedStr = section["unlock"].ToString();
                }

                bool ignoreLastRead = false;
                if (section.ContainsKey("ignore_last_read"))
                {
                    ignoreLastRead = section["ignore_last_read"].ToObject<bool>();
                }

                string sectionName = section.ContainsKey("name") ? section["name"].ToString() : "";
                var sectionData = new SectionData
                {
                    id = i,
                    name = sectionName,
                    pageCount = section["page_count"].ToObject<int>(),
                    unlock = unlockedStr,
                    ignoreLastRead = ignoreLastRead
                };
                GameObject sectionItem = Instantiate(sectionItemPrefab, songlistContentPanel);
                SectionItem item = sectionItem.GetComponent<SectionItem>();
                item.Init(this, sectionData);
                PlayerPrefs.SetInt("LastStorySelect", chapter);
                PlayerPrefs.Save();
            }
        }

        private bool CheckSectionUnlocked(object condition)
        {
            return condition == null || String.IsNullOrEmpty(condition.ToString());
        }

        private void UpdateButtons()
        {
            bool hasPrev = chapter > 0;
            bPrev.interactable = hasPrev;
            bPrev.gameObject.transform.GetChild(0).GetComponent<Text>().color =
                hasPrev ? Color.white : bPrev.colors.disabledColor;
            bool hasNext = chapter < storyListJo.Count - 1;
            bNext.interactable = hasNext;
            bNext.gameObject.transform.GetChild(0).GetComponent<Text>().color =
                hasNext ? Color.white : bNext.colors.disabledColor;
        }

        private void Prev()
        {
            chapter -= 1;
            if (chapter < 0) chapter = 0;
            UpdateButtons();
            RefreshStoryList();
        }

        private void Next()
        {
            chapter += 1;
            if (chapter >= storyListJo.Count) chapter = storyListJo.Count - 1;
            UpdateButtons();
            RefreshStoryList();
        }

        public void Back()
        {
            Resources.UnloadUnusedAssets();
            FadeManager.Instance.LoadScene("main");
        }
    }
}