using System;
using System.Collections;
using DRFV.Game;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Game
{
    public class Question : MonoBehaviour
    {
        private TheGameManager theGameManager;
        public float ms, endMs;
        public Text questionText;
        public Text[] choiceTexts;
        public RectMask2D mask;
        public void Init(TheGameManager theGameManager, float ms, float endMs, string question, params string[] choices)
        {
            this.theGameManager = theGameManager;
            this.ms = ms;
            this.endMs = endMs;
            questionText.text = question;
            int length = Math.Min(choices.Length, choiceTexts.Length);
            for (int i = 0; i < length; i++)
            {
                choiceTexts[i].text = choices[i];
            }
            StartCoroutine(Listen());
        }
        private IEnumerator Listen()
        {
            yield return new WaitWhile(() => theGameManager.progressManager.NowTime < ms);
            mask.enabled = false;
            yield return new WaitWhile(() => theGameManager.progressManager.NowTime < endMs);
            mask.enabled = true;
            Destroy(gameObject);
        }
    }
}
