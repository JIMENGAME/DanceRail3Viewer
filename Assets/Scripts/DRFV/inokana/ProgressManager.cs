using System;
using System.Diagnostics;
using DRFV.Game;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DRFV.inokana
{
    public class ProgressManager : MonoBehaviour
    {
        Stopwatch _stopwatch = new();

        public float NowTime;

        //下面是dsp!
        private float startDspTime;
        private float lastUpdateDspTime;

        private TheGameManager _theGameManager;

        public void Init(TheGameManager theGameManager)
        {
            _theGameManager = theGameManager;
            if (!Stopwatch.IsHighResolution)
            {
                NotificationBarManager.Instance.Show("计时器精准度可能有问题，请与inokana取得联系");
                if (_theGameManager) _theGameManager.QuitDirectly();;
            }
        }

        public void StartTiming()
        {
            _stopwatch.Start();
            startDspTime = lastUpdateDspTime = (float)AudioSettings.dspTime;
        }

        public void StopTiming()
        {
            _stopwatch.Stop();
        }

        public void ContinueTiming()
        {
            _stopwatch.Start();
        }

        public void OnUpdate()
        {

            var currentDspTime = (float)AudioSettings.dspTime;
            if (Math.Abs(lastUpdateDspTime - currentDspTime) > 0.001f)
            {
                lastUpdateDspTime = currentDspTime;
                //仅在真正dsp时间更新的时候比对
                var differenceTime = (currentDspTime - startDspTime) * 1000f - _stopwatch.ElapsedMilliseconds;
                if (differenceTime < -500f)
                {
                    if (_theGameManager) _theGameManager.QuitDirectly();

                    //NotificationBarManager.Instance.Show("DspBuffer数值过小，请前往设置调整。");

                    Debug.LogWarning($"当前时差为{differenceTime}ms,Dsp炸啦！！！");
                }
            }

            var tempT = _stopwatch.ElapsedMilliseconds - delay;
            NowTime = (tempT < pauseTime ? pauseTime : tempT) * _theGameManager.BGMManager.pitch;
        }

        private float pauseTime;
        private float delay;
        public void AddStartDelay(float msdelay)
        {
            delay += msdelay;
            pauseTime = NowTime;
        }

        public void AddDelay(float msdelay)
        {
            delay += msdelay;
        }
    }
}
