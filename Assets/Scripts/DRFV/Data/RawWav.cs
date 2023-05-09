using UnityEngine;

namespace DRFV.Data
{
    public class RawWav : ScriptableObject
    {
        [HideInInspector] public byte[] data;

        public int frequency;

        public int channelCount;

        public void SetData(byte[] data)
        {
            this.data = data;
            var wav = new WAV(data);
            frequency = wav.Frequency;
            channelCount = wav.ChannelCount;
        }
    }
}