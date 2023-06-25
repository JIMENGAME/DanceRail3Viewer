using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = System.Random;

namespace DRFV.Global
{
    public static class MethodExtensions
    {
        public static Color ModifyAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        public static Vector3 ModifyX(this Vector3 position, float x)
        {
            return new Vector3(x, position.y, position.z);
        }
        
        public static float Variance(this float[] data)
        {
            float average = data.Average();
            return (float)data.Sum(x => Math.Pow(x - average, 2)) / data.Length;
        }

        public static float StandardDeviation(this float[] data)
        {
            return Mathf.Pow(data.Variance(), 0.5f);
        }
        
        public static AudioClip MonoToStereo(this AudioClip audioClip)
        {
            if (audioClip == null || audioClip.channels == 2) return audioClip;
            if (audioClip.channels != 1) throw new Exception();
            float[] data = new float[audioClip.samples], output = new float[audioClip.samples * 2];
            audioClip.GetData(data, 0);
            for (int i = 0; i < data.Length; i++)
            {
                output[2 * i] = data[i];
                output[2 * i + 1] = data[i];
            }

            AudioClip clip = AudioClip.Create(audioClip.name, audioClip.samples, 2, audioClip.frequency, false);
            clip.SetData(output, 0);
            return clip;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool InRange(this float value, Single min, Single max)
            => value >= min && value <= max;
        
        public static bool NextBool(this Random random)
        {
            return random.Next(0, 100) < 50;
        }
    }
}