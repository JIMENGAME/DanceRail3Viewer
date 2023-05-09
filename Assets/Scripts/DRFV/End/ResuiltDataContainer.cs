using DRFV.Enums;
using UnityEngine;

namespace DRFV.End
{
    public class ResultDataContainer : MonoBehaviour
    {
        public float SCORE = 0;
        public int MAXCOMBO = 0;
        public int PERFECT_J = 0;
        public int PERFECT = 0;
        public int GOOD = 0;
        public int MISS = 0;
        public int FAST = 0;
        public int SLOW = 0;
        public EndType endType;
        public string md5;
        public Color? bgColor;
        public float hp;
        public float Accuracy;
        public int noteTotal;
    }
}