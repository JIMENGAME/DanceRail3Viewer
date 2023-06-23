using System.Collections;
using DG.Tweening;
using DRFV.Game;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Game.SceneControl
{
    public class Aegleseeker : global::DRFV.Game.SceneControl.SceneControl
    {
        public Image BackTexture;
        public Image DimmingOverlay;
        public Image TopDimmerOverlay;
        public Text Text;
        public Text TextShadow;
        public Image Line1, Line2;
        public Image Dark;
        public Image TextBlur1, TextBlur2, TextBlur3;

        public GameObject CanvasSongData, CanvasScoreData, CanvasPause, CanvasProgramInfo;

        public float FadeStart,
            FadeEnd,
            DarkStart,
            DarkEnd,
            DFStartAnd,
            AndIn,
            AndInThat,
            AndInThatLight,
            AndInThatLightI,
            EndAndInThatLighI,
            DFP1AndInThatLightIFind,
            EndAndInThatLightIFind,
            DFP2AndInThatLightIFindDe,
            AndInThatLightIFindDeliver,
            DarkFadeEnd,
            TextEnd;

        private GameObject AegleseekerMain;

        public void Init(TheGameManager theGameManager, float time, Transform aegleseekerObject)
        {
            this.theGameManager = theGameManager;
            AegleseekerMain = aegleseekerObject.gameObject;
            BackTexture = aegleseekerObject.Find("back_texture").GetComponent<Image>();
            DimmingOverlay = aegleseekerObject.Find("dimming_overlay").GetComponent<Image>();
            TopDimmerOverlay = aegleseekerObject.Find("top_dimmer_overlay").GetComponent<Image>();
            Line1 = aegleseekerObject.Find("LineLeft").GetComponent<Image>();
            Line2 = aegleseekerObject.Find("LineRight").GetComponent<Image>();
            Text = aegleseekerObject.Find("Text").GetComponent<Text>();
            TextShadow = aegleseekerObject.Find("TextShadow").GetComponent<Text>();
            Dark = aegleseekerObject.Find("dark").GetComponent<Image>();
            CanvasSongData = GameObject.Find("CanvasSongData");
            CanvasScoreData = GameObject.Find("CanvasScoreData");
            CanvasProgramInfo = GameObject.Find("CanvasProgramInfo");
            CanvasPause = GameObject.Find("CanvasPause");
            FadeStart = theGameManager.drbfile.CalculateDRBFileTime(time);
            FadeEnd = theGameManager.drbfile.CalculateDRBFileTime(time + 8);
            DarkStart = theGameManager.drbfile.CalculateDRBFileTime(time + 13.25f);
            DarkEnd = theGameManager.drbfile.CalculateDRBFileTime(time + 13.5f);
            DFStartAnd = theGameManager.drbfile.CalculateDRBFileTime(time + 13.875f);
            AndIn = theGameManager.drbfile.CalculateDRBFileTime(time + 14f);
            AndInThat = theGameManager.drbfile.CalculateDRBFileTime(time + 14.25f);
            AndInThatLight = theGameManager.drbfile.CalculateDRBFileTime(time + 14.5f);
            AndInThatLightI = theGameManager.drbfile.CalculateDRBFileTime(time + 14.625f);
            EndAndInThatLighI = theGameManager.drbfile.CalculateDRBFileTime(time + 14.75f);
            DFP1AndInThatLightIFind = theGameManager.drbfile.CalculateDRBFileTime(time + 14.875f);
            EndAndInThatLightIFind = theGameManager.drbfile.CalculateDRBFileTime(time + 15f);
            DFP2AndInThatLightIFindDe = theGameManager.drbfile.CalculateDRBFileTime(time + 15.25f);
            AndInThatLightIFindDeliver = theGameManager.drbfile.CalculateDRBFileTime(time + 15.5f);
            DarkFadeEnd = theGameManager.drbfile.CalculateDRBFileTime(time + 15.75f);
            TextEnd  = theGameManager.drbfile.CalculateDRBFileTime(time + 15.875f);
            TextBlur1 = GameObject.Find("TextBlur1").GetComponent<Image>();
            TextBlur2 = GameObject.Find("TextBlur2").GetComponent<Image>();
            TextBlur3 = GameObject.Find("TextBlur3").GetComponent<Image>();
#if UNITY_EDITOR

#else
        if (!theGameManager.DebugMode) CanvasPause.SetActive(false);
#endif
            StartCoroutine(AegleseekerAnomaly());
            StartCoroutine(ShadowMoving());
        }

        private IEnumerator AegleseekerAnomaly()
        {
            while (theGameManager.progressManager.NowTime < FadeStart)
            {
                yield return null;
            }

            DimmingOverlay.DOColor(new Color(1, 1, 1, 1), (FadeEnd - theGameManager.progressManager.NowTime) / 1000f);

            while (theGameManager.progressManager.NowTime < DarkStart)
            {
                yield return null;
            }

            TopDimmerOverlay.DOColor(new Color(0, 0, 0, 1), (DarkEnd - theGameManager.progressManager.NowTime) / 1000f);

            while (theGameManager.progressManager.NowTime < DarkEnd)
            {
                yield return null;
            }

            Dark.color = new Color(0, 0, 0, 1);
            CanvasSongData.SetActive(false);
            CanvasScoreData.SetActive(false);
            CanvasProgramInfo.SetActive(false);
            theGameManager.autoPlayHint.SetActive(false);
            CanvasPause.SetActive(false);

            while (theGameManager.progressManager.NowTime < DFStartAnd)
            {
                yield return null;
            }

            BackTexture.color = new Color(1, 1, 1, 1);
            TopDimmerOverlay.DOColor(new Color(0, 0, 0, 0.75f),
                (DFP1AndInThatLightIFind - theGameManager.progressManager.NowTime) / 1000f);
            DimmingOverlay.DOColor(new Color(1, 1, 1, 0.75f),
                (DFP1AndInThatLightIFind - theGameManager.progressManager.NowTime) / 1000f);
            Dark.color = new Color(1, 1, 1, 0);
            BackTexture.DOColor(new Color(1, 1, 1, 0.75f),
                (DFP1AndInThatLightIFind - theGameManager.progressManager.NowTime) / 1000f);
            BackTexture.rectTransform.DOAnchorPosX(-50f, (DarkFadeEnd - theGameManager.progressManager.NowTime) / 1000f);
            TextShadow.text = Text.text = "and ";

            while (theGameManager.progressManager.NowTime < AndIn)
            {
                yield return null;
            }

            TextShadow.text = Text.text = "and in ";

            while (theGameManager.progressManager.NowTime < AndInThat)
            {
                yield return null;
            }

            TextShadow.text = Text.text = "and in that ";

            while (theGameManager.progressManager.NowTime < AndInThatLight)
            {
                yield return null;
            }

            TextShadow.text = Text.text = "and in that light, ";
            TextBlur1.DOColor(new Color(1, 1, 1, 0.25f), 0.1f);
            TextBlur1.rectTransform.DOAnchorPosX(0, (DarkFadeEnd - theGameManager.progressManager.NowTime) / 1000f);

            while (theGameManager.progressManager.NowTime < AndInThatLightI)
            {
                yield return null;
            }

            TextShadow.text = Text.text = "and in that light, I ";
            TextBlur2.DOColor(new Color(1, 1, 1, 0.25f), 0.1f);
            TextBlur2.rectTransform.DOAnchorPosY(50, (DarkFadeEnd - theGameManager.progressManager.NowTime) / 1000f);

            while (theGameManager.progressManager.NowTime < DFP1AndInThatLightIFind)
            {
                yield return null;
            }

            TextShadow.text = Text.text = "and in that light, I find ";
            TopDimmerOverlay.DOColor(new Color(0, 0, 0, 0.5f),
                (DFP1AndInThatLightIFind - theGameManager.progressManager.NowTime) / 1000f);
            DimmingOverlay.DOColor(new Color(1, 1, 1, 0.5f),
                (DFP2AndInThatLightIFindDe - theGameManager.progressManager.NowTime) / 1000f);
            BackTexture.DOColor(new Color(1, 1, 1, 0.5f),
                (DFP2AndInThatLightIFindDe - theGameManager.progressManager.NowTime) / 1000f);
            TextBlur3.DOColor(new Color(1, 1, 1, 0.25f), 0.1f);
            TextBlur3.rectTransform.DOAnchorPosX(-100, (DarkFadeEnd - theGameManager.progressManager.NowTime) / 1000f);

            while (theGameManager.progressManager.NowTime < DFP2AndInThatLightIFindDe)
            {
                yield return null;
            }

            TextShadow.text = Text.text = "and in that light, I find deliverance.";
            TopDimmerOverlay.DOColor(new Color(0, 0, 0, 0f),
                (DFP1AndInThatLightIFind - theGameManager.progressManager.NowTime) / 1000f);
            DimmingOverlay.DOColor(new Color(1, 1, 1, 0f), (DarkFadeEnd - theGameManager.progressManager.NowTime) / 1000f);
            BackTexture.DOColor(new Color(1, 1, 1, 0f), (DarkFadeEnd - theGameManager.progressManager.NowTime) / 1000f);
            TextShadow.DOColor(new Color(1, 1, 1, 0), (DarkFadeEnd - theGameManager.progressManager.NowTime) / 1000f);
            Line1.DOColor(new Color(1, 1, 1, 0), (DarkFadeEnd - theGameManager.progressManager.NowTime) / 1000f);
            Line2.DOColor(new Color(1, 1, 1, 0), (DarkFadeEnd - theGameManager.progressManager.NowTime) / 1000f);
            TextBlur1.DOColor(new Color(1, 1, 1, 0f), 0.1f);
            TextBlur2.DOColor(new Color(1, 1, 1, 0f), 0.1f);
            TextBlur3.DOColor(new Color(1, 1, 1, 0f), 0.1f);

            while (theGameManager.progressManager.NowTime < DarkFadeEnd)
            {
                yield return null;
            }
            Text.DOColor(new Color(1, 1, 1, 0), (TextEnd - theGameManager.progressManager.NowTime) / 1000f);
        
            while (theGameManager.progressManager.NowTime < TextEnd)
            {
                yield return null;
            }
            AegleseekerMain.SetActive(false);
        }

        private IEnumerator ShadowMoving()
        {
            void MoveTextBlur(float endms)
            {
                Text.rectTransform.anchoredPosition = new Vector2(5, Text.rectTransform.anchoredPosition.y);
                Text.rectTransform.DOAnchorPosX(-5, (endms - theGameManager.progressManager.NowTime) / 1000f);
            }

            while (theGameManager.progressManager.NowTime < DFStartAnd)
            {
                yield return null;
            }

            MoveTextBlur(AndIn);
            while (theGameManager.progressManager.NowTime < AndIn)
            {
                yield return null;
            }

            MoveTextBlur(AndInThat);
            while (theGameManager.progressManager.NowTime < AndInThat)
            {
                yield return null;
            }

            MoveTextBlur(AndInThatLight);
            while (theGameManager.progressManager.NowTime < AndInThatLight)
            {
                yield return null;
            }

            MoveTextBlur(EndAndInThatLighI);
            while (theGameManager.progressManager.NowTime < EndAndInThatLighI)
            {
                yield return null;
            }

            MoveTextBlur(EndAndInThatLightIFind);
            while (theGameManager.progressManager.NowTime < EndAndInThatLightIFind)
            {
                yield return null;
            }

            MoveTextBlur(DFP2AndInThatLightIFindDe);
            while (theGameManager.progressManager.NowTime < DFP2AndInThatLightIFindDe)
            {
                yield return null;
            }

            MoveTextBlur(AndInThatLightIFindDeliver);
            while (theGameManager.progressManager.NowTime < AndInThatLightIFindDeliver)
            {
                yield return null;
            }

            MoveTextBlur(DarkFadeEnd);
        }
    }
}