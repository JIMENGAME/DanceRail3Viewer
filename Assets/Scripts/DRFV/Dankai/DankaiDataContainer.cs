using System.Collections.Generic;
using DRFV.Select;
using UnityEngine;

namespace DRFV.Dankai
{
    public class DankaiDataContainer : MonoBehaviour
    {
        public string skill;
        public int nowId = 0;
        public float hpMax = 0;
        public float hpNow = 0;
        public SongDataContainer[] songs;
        public List<DankaiResultData> results = new List<DankaiResultData>();
    }

    public class DankaiResultData
    {
        public float score;
        public int pj, p, g, m;
    }
}
