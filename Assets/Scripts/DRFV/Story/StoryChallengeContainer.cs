using DRFV.Enums;
using DRFV.Select;
using UnityEngine;

namespace DRFV.Story
{
    public class StoryChallengeContainer : SongDataContainer
    {
        public string source;
        public string unlock;
        public bool isComputer;
        public bool hasCustomMover;
        public bool hasCustomHeight;
        public float timeToVideoShow;
        public float timeToEnter;
        public bool hasTextBeforeStart;
        public string tierIdentifier;
        public Color? customTierColor;
        public bool ratingPlus;

        public float songSpeed;
        public int NoteJudgeRange;
        public GameSide gameSide;

        public override int GetContainerType()
        {
            return SongDataContainerType.STORY;
        }
    }
}