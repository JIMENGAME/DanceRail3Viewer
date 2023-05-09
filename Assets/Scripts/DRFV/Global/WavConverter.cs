using System.IO;
using DRFV.Data;
using NAudio.Wave;
using UnityEngine;

namespace DRFV.Global
{
    public class WavConverter
    {
        private static byte[] ConvertWavToWav(byte[] data, int freq, int bit, int channels, string clipName)
        {
            string instanceCachePath = StaticResources.Instance.cachePath + $"conversationTmp_{clipName}_{freq}_{bit}_{channels}.wav";
            if (!File.Exists(instanceCachePath))
            {
                using var waveStream = new RawSourceWaveStream(new MemoryStream(data), new WaveFormat());
                using var conversionStream = new WaveFormatConversionStream(new WaveFormat(freq, bit, channels), waveStream);
                WaveFileWriter.CreateWaveFile(instanceCachePath, conversionStream);
            }
            FileStream fileStream = new FileStream(instanceCachePath, FileMode.Open, FileAccess.Read);
            byte[] qwq = new byte[fileStream.Length];
            fileStream.Read(qwq);
            fileStream.Close();
            return qwq;
        }

        public static AudioClip ConvertWavToAudioClip(byte[] data, int freq, int bit, int channels, string clipName = "")
        {
            var dfjk = ConvertWavToWav(data, freq, bit, channels, clipName);

            WAV wav = new WAV(dfjk);

            AudioClip audioClip = AudioClip.Create(clipName, wav.SampleCount, wav.ChannelCount, wav.Frequency, false);
            audioClip.SetData(wav.ChannelCount == 2 ? wav.StereoChannel : wav.LeftChannel, 0);
            return audioClip;
        }
    }

    public static class StreamExtension
    {
        public static byte[] ToArray(this Stream stream)
        {
            byte[] buffer = new byte[4096];
            int reader = 0;
            MemoryStream memoryStream = new MemoryStream();
            while ((reader = stream.Read(buffer, 0, buffer.Length)) != 0)
                memoryStream.Write(buffer, 0, reader);
            return memoryStream.ToArray();
        }
    }
}