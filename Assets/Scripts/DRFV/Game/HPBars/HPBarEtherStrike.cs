using System.Collections;
using DRFV.Enums;
using DRFV.Game;
using UnityEngine;

namespace DRFV.Game.HPBars
{
    public class HPBarEtherStrike : HPBar
    {
        public HPBarEtherStrike(HPManager hpManager)
        {
            _hpManager = hpManager;
            HpInit = 100f;
            HpMax = 100f;
            barColorLight = new Color(0.45f, 0f, 0f, 1f);
            barColorDark = Color.red;
        }
        private IEnumerator hardC, anomalyC;
        private HPManager _hpManager;
        private float hardMinValue = 20f, hardDecreaseValue = 6f;
        public override float PerfectJHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 900f * NoteWeight[(int) noteKind] / totalNotesWeigh;
        }
    
        public override float PerfectHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return 75f * NoteWeight[(int) noteKind] / totalNotesWeigh;
        }

        public override float GoodHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            if (theGameManager.hpManager.HpNow > 20f) return 0f;
            return 0.5f * NoteWeight[(int) noteKind] * theGameManager.SkillDamage;
        }
    
        public override float MissHP(NoteKind noteKind, int totalNotesWeigh, TheGameManager theGameManager)
        {
            return NoteWeight[(int) noteKind] * theGameManager.SkillDamage;
        }

        public override void OnInit()
        {
            hardC = Hard();
            anomalyC = OnAnomaly();
            _hpManager.StartCoroutine(hardC);
            _hpManager.StartCoroutine(anomalyC);
        }
    
        public override void OnRefresh()
        {
            _hpManager.StopCoroutine(hardC);
            _hpManager.StopCoroutine(anomalyC);
        }

        private IEnumerator OnAnomaly()
        {
            yield return new WaitWhile(() => _hpManager.manager.progressManager.NowTime < 136923f);
            _hpManager.RefillHp();
            hardDecreaseValue = 4f;
            hardMinValue = 1f;
            barColorDark = Color.yellow;
        }
        private IEnumerator Hard()
        {
            WaitForSeconds waitForSeconds = new WaitForSeconds(1f / _hpManager.manager.BGMManager.pitch);
            if (!_hpManager.manager.pauseable) yield return new WaitUntil(() => _hpManager.manager.pauseable);
            while (!_hpManager.manager.ended)
            {
                if (_hpManager.HpNow > hardMinValue)
                {
                    if (_hpManager.HpNow - hardDecreaseValue > hardMinValue) _hpManager.DecreaseHp(hardDecreaseValue);
                    else _hpManager.SetHp(hardMinValue);
                }
                yield return waitForSeconds;
                if (_hpManager.manager.isPause) yield return new WaitWhile(() => _hpManager.manager.isPause);
            }
        }
    }
}
