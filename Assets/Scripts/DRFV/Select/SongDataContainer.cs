using DRFV.Enums;
using UnityEngine;

namespace DRFV.Select
{
    public static class SongDataContainerType
    {
        public const int ORIGINAL = 0;
        public const int STORY = 1;
        public const int HADOU_TEST = 2;
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

    public struct NoteJudgeRange
    {
        public string displayName;
        public float PJ;
        public float P;
        public float G;
    }
}