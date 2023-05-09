using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Story
{
    public class SectionItem : MonoBehaviour
    {
        private SectionData sectionData;
        public Text tSection;
        private StoryListManager storyListManager;
        private bool inited;

        public void Init(StoryListManager storyListManager, SectionData sectionData)
        {
            this.sectionData = sectionData;
            this.storyListManager = storyListManager;
            Button button = gameObject.GetComponent<Button>();
            tSection.text = "SECTION " + (sectionData.id + 1);
            string lastSection = storyListManager.chapter == 0 ? "" : sectionData.id == 0 ? storyListManager.chapter - 1 + "" : storyListManager.chapter + "." + (sectionData.id - 1);
            if (sectionData.unlock != "" && PlayerPrefs.GetInt("story_" + sectionData.unlock, 0) == 0 || lastSection != "" && PlayerPrefs.GetInt("story_read_" + lastSection, 0) == 0)
            {
                tSection.color = button.colors.disabledColor;
            }
            else
            {
                tSection.color = Color.white;
                if (!string.IsNullOrEmpty(sectionData.name)) tSection.text += ": " + sectionData.name;
            }

            inited = true;
        }

        public void SelectStory()
        {
            if (!inited) return;
            storyListManager.storyContentManager.Init(new StoryData
            {
                chapter = storyListManager.chapter, section = sectionData.id, totalPage = sectionData.pageCount,
                unlock = sectionData.unlock, totalSection = storyListManager.sectionCount, ignoreLastRead = sectionData.ignoreLastRead
            });
            storyListManager.panel.DOAnchorPosX(-2000, 0.5f).SetEase(Ease.OutExpo);
        }
    }

    public struct SectionData
    {
        public int id;
        public string name;
        public int pageCount;
        public string unlock;
        public bool ignoreLastRead;
    }
}