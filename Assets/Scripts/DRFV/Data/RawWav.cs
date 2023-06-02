using UnityEngine;

namespace DRFV.Data
{
    public class RawWav : ScriptableObject
    {
        [HideInInspector] public byte[] data;

        public void SetData(byte[] data)
        {
            this.data = data;
            var wav = new WAV(data);
        }
    }
}