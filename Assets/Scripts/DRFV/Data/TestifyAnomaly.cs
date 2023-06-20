using System.Collections.Generic;
using Newtonsoft.Json;

namespace DRFV.Data
{
    public class TestifyAnomaly
    {
        public float minEffect;
        public float maxEffect;
        public float strength;
        public int sampleCount;
        public List<TestifyAnomalyArguments> args;
    }

    public class TestifyAnomalyArguments
    {
        public int startTime;
        public int duration;
        [JsonIgnore]
        public int endTime;
        public float startStrength;
        public float deltaStrength;
        [JsonIgnore]
        public float endStrength;
        public StrengthType strengthType;
    }

    public enum StrengthType
    {
        Linear = 0,
        SineOut = 1
    }
}