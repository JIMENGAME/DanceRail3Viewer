using System;
using System.Collections;
using System.Collections.Generic;
using DRFV.inokana;
using UnityEngine;

namespace DRFV
{
    public class A
    {
        public List<float> pures = new(), fars = new(), losts = new();
        private float singleRecall = 0.901754385965f, lastNoteTime = 114514f, noteCount = 114; //假装我算好了
        private bool isPlaying = true;
        public float hpNow = 100f; // 假装这个是总HP
        private static readonly float ln2 = Mathf.Log(2);
        private ProgressManager _progressManager;

        public void Judge(string judgeType, float judgeTime)
        {
            switch (judgeType)
            {
                case "pure":
                    pures.Add(judgeTime);
                    break;
                case "far":
                    fars.Add(judgeTime);
                    break;
                case "lost":
                    losts.Add(judgeTime);
                    break;
                default:
                    throw new Exception();
            }
        }

        public float GetFadingSpeed()
        {
            float hpDelta = 0f;
            foreach (float t in pures)
            {
                hpDelta += ln2 * (2 * singleRecall) * Mathf.Pow(2, t - _progressManager.NowTime);
            }
            foreach (float t in pures)
            {
                hpDelta += ln2 * singleRecall * Mathf.Pow(2, t - _progressManager.NowTime);
            }
            foreach (float t in losts)
            {
                hpDelta -= 0.5f * ln2 * 18 * Mathf.Pow(2, 0.5f * (t - _progressManager.NowTime));
            }

            return hpDelta;
        }
        
        public IEnumerator NaturalFading()
        {
            while (isPlaying)
            {
                hpNow -= singleRecall * noteCount / (lastNoteTime + 0.5f) * Time.deltaTime;
                yield return null;
            }
        }
    }
}