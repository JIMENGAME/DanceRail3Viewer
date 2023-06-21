using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DRFV.Enums;
using DRFV.Game;
using DRFV.Global;
using DRFV.inokana;
using UnityEngine;

namespace DRFV
{
    public class A : MonoSingleton<A>
    {
        private TheGameManager _theGameManager;
        public List<float> pures = new(), fars = new(), losts = new();
        public float hpNow = 100f; // 假装这个是总HP
        private static readonly float ln2 = Mathf.Log(2);
        private ProgressManager ProgressManager => _theGameManager.progressManager;
        private DRBFile DrbFile => _theGameManager.drbfile;
        private float hpDelta = 0f;
        private float StartTime, ShutterOffset;
        private float lostTime;
        private float offset;
        private float AudioTiming => ProgressManager.NowTime + offset;
        private bool prepared;

        public void Init(TheGameManager theGameManager)
        {
            _theGameManager = theGameManager;
            offset = DrbFile.offset * 1000f;
            StartTime = 6000 - offset;
            ShutterOffset = 6000 + offset;
            lostTime = DrbFile.SingleHp * DrbFile.TotalNotes / (DrbFile.LastNoteTime / 1000f + 0.5f);
            prepared = true;
        }

        public void Judge(JudgeType judgeType, float judgeTime)
        {
            switch (judgeType)
            {
                case JudgeType.PERFECT_J:
                    pures.Add(judgeTime);
                    break;
                case JudgeType.PERFECT:
                case JudgeType.GOOD:
                    fars.Add(judgeTime);
                    break;
                case JudgeType.MISS:
                    losts.Add(judgeTime);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(judgeType), judgeType, null);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CanCalculateTempestHp(float hp)
        {
            return (hp + hpDelta).InRange(0, 100) && _theGameManager.IsPlaying;
        }

        private float GetHpNow()
        {
            float hp = 100f;

            hp -= Mathf.Min(DrbFile.LastNoteTime + ShutterOffset, AudioTiming + StartTime) / 1000f * lostTime;

            foreach (var t in pures)
            {
                if (CanCalculateTempestHp(hp)) hpDelta += ln2 * (2 * DrbFile.SingleHp) * Mathf.Pow(2, t - ProgressManager.NowTime) * Time.deltaTime;
            }

            foreach (var t in pures)
            {
                if (CanCalculateTempestHp(hp)) hpDelta += ln2 * DrbFile.SingleHp * Mathf.Pow(2, t - ProgressManager.NowTime) * Time.deltaTime;
            }

            foreach (var t in losts)
            {
                if (CanCalculateTempestHp(hp)) hpDelta -= 0.5f * ln2 * 18 * Mathf.Pow(2, 0.5f * (t - ProgressManager.NowTime)) * Time.deltaTime;
            }

            hp += hpDelta;
            
            return Math.Clamp(hp, 0, 100);
        }

        private void Update()
        {
            if (!prepared) return;
            float hp = GetHpNow();
            Debug.Log((hp >= hpNow ? "蓝色" : "红色") + hp);
            hpNow = hp;
        }
    }
}