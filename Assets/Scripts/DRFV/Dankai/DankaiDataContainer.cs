using System.Collections.Generic;
using DRFV.Select;
using UnityEngine;

namespace DRFV.Dankai
{
    public class DankaiDataContainer : MonoBehaviour
    {
        public int nowId = 0;
        public float hpMax = 0;
        public float hpNow = 0;
        public SongDataContainer[] songs;
        public bool IsFinished => nowId == songs.Length;
        public List<DankaiResultData> results = new List<DankaiResultData>();
    }

    public class DankaiResultData
    {
        public int pj, p, g, m;
    }
}
