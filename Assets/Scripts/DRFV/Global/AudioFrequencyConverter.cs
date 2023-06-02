using System.IO;
using DRFV.Data;
using NAudio.Wave;
using UnityEngine;

namespace DRFV.Global
{
    public class AudioFrequencyConverter
    {
        public static AudioClip ConvertAudioClip(AudioClip clip, int freq, string clipName = "")
        {
            return WavToAudioClip(ConvertWavFreqToWav(SavWav.AudioClipToByteArray(clip), freq), clipName);
        }

        public static AudioClip ConvertWavToAudioClip(byte[] data, int freq, string clipName = "")
        {
            return WavToAudioClip(ConvertWavFreqToWav(data, freq), clipName);
        }

        private static AudioClip WavToAudioClip(byte[] data, string clipName)
        {
#if UNITY_EDITOR_WIN
            using (FileStream fileStream = new FileStream(StaticResources.Instance.dataPath + "test.wav",
                       FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                fileStream.Write(data);
            }

#endif

            WAV wav = new WAV(data);

            AudioClip audioClip = AudioClip.Create(clipName, wav.SampleCount, wav.ChannelCount, wav.Frequency, false);
            audioClip.SetData(wav.ChannelCount == 2 ? wav.StereoChannel : wav.LeftChannel, 0);
            return audioClip;
        }

        private static byte[] ConvertWavFreqToWav(byte[] data, int freq)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            using MemoryStream readStream = new MemoryStream(data);
            using WaveFileReader waveFileReader = new(readStream);
            WaveFormat waveFormat = waveFileReader.WaveFormat;
            using MemoryStream memoryStream = new();
            using WaveFormatConversionStream conversionStream =
                new(new WaveFormat(freq, waveFormat.BitsPerSample, waveFormat.Channels), waveFileReader);
            WaveFileWriter.WriteWavFileToStream(memoryStream, conversionStream);
            return memoryStream.ToArray();
#else
            return data;
#endif
        }
    }
}