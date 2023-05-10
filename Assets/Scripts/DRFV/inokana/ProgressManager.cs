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

        public delegate void OnResolutionError();

        public delegate float Pitch();

        private OnResolutionError _runtimeError;
        private Pitch _pitch;

        public void Init(OnResolutionError initError, OnResolutionError runtimeError, Pitch pitch)
        {
            _runtimeError = runtimeError;
            _pitch = pitch;
            if (!Stopwatch.IsHighResolution)
            {
                initError.Invoke();
            }
        }

        public void ResetTiming()
        {
            _stopwatch.Stop();
            _stopwatch.Reset();
            delay = pauseTime = startDspTime = lastUpdateDspTime = 0;
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
                    _runtimeError.Invoke();

                    Debug.LogWarning($"当前时差为{differenceTime}ms,Dsp炸啦！！！");
                }
            }

            var tempT = _stopwatch.ElapsedMilliseconds - delay;
            NowTime = (tempT < pauseTime ? pauseTime : tempT) * _pitch.Invoke();
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
