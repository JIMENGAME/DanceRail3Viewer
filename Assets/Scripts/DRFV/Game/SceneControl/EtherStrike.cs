using System;
using System.Collections;
using DG.Tweening;
using DRFV.Game.HPBars;
using DRFV.Game;
using DRFV.Setting;
using UnityEngine;
using UnityEngine.UI;

namespace DRFV.Game.SceneControl
{
    public class EtherStrike : MonoBehaviour
    {
        public Animator animator;
        private TheGameManager _theGameManager;
        private AudioClip anomaly;
        public Image o;

        public void Init(TheGameManager theGameManager) {
            if (theGameManager.SongKeyword != "etherstrike" || theGameManager.SongHard != 10)
            {
                Destroy(gameObject);
                return;
            }

            _theGameManager = theGameManager;
            anomaly = Resources.Load<AudioClip>("STORY/SONGS/etherstrike_anomaly");
            StartCoroutine(Qwq());
        }

        private IEnumerator Qwq()
        {
            GameObject pauseCanvas = GameObject.Find("CanvasPause");
            int Play = Animator.StringToHash("Play");
            yield return new WaitWhile(() => _theGameManager.progressManager.NowTime < 86827f);
            if (!_theGameManager.isComputerInStory && (_theGameManager.hpManager.HpNow > 70f || _theGameManager.hpManager.isCheap))
            {
                Destroy(gameObject);
                yield break;
            }

            _theGameManager.SetTimeToEnd(149231f);
            animator.SetTrigger(Play);
            pauseCanvas.SetActive(false);
            _theGameManager.currentSettings.HardMode = _theGameManager.isHard = true;
            float hp = Math.Max(_theGameManager.hpManager.HpNow, 50);
            _theGameManager.hpManager.Init(new HPBarEtherStrike(_theGameManager.hpManager));
            _theGameManager.hpManager.HpNow = hp;
            var bgmManager = _theGameManager.BGMManager;
            bgmManager.Pause();
            float time = bgmManager.time;
            bgmManager.clip = anomaly;
            bgmManager.time = time;
            bgmManager.Play();
            yield return new WaitWhile(() => _theGameManager.progressManager.NowTime < 87692f);
            o.DOFade(1, (112308f - _theGameManager.progressManager.NowTime) / 1000f / _theGameManager.BGMManager.pitch);
            yield return new WaitWhile(() => _theGameManager.progressManager.NowTime < 136800f);
            _theGameManager.DoSCDown();
            yield return new WaitWhile(() => _theGameManager.progressManager.NowTime < 136923f);
            o.DOFade(0, (138462f - _theGameManager.progressManager.NowTime) / 1000f / _theGameManager.BGMManager.pitch);
        }
    }
}
