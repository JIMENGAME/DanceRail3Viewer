using DRFV.Enums;
using Newtonsoft.Json;
using UnityEngine;

namespace DRFV.Select
{
    public static class SongDataContainerType
    {
        public const int ORIGINAL = 0;
        public const int STORY = 1;
        public const int HADOU_TEST = 2;
        public const int DANKAI = 0;
    }
    public class SongDataContainer : MonoBehaviour
    {
        public TheSelectManager.SongData songData;

        public AudioClip music;
        public int selectedDiff;
        public bool saveAudio;

        public virtual int GetContainerType()
        {
            return SongDataContainerType.ORIGINAL;
        }
    }

    public class NoteJudgeRange
    {
        [JsonProperty("displayName")] public string displayName;
        [JsonProperty("PJ")] public float PJ;
        [JsonProperty("P")] public float P;
        [JsonProperty("G")] public float G;
    }
}