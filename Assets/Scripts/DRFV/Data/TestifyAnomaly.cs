namespace DRFV.Data
{
    public class TestifyAnomaly
    {
        public float minEffect;
        public float maxEffect;
        public float strength;
        public int sampleCount;
        public TestifyAnomalyArguments[] args;
    }

    public class TestifyAnomalyArguments
    {
        public int startTime;
        public int duration;
        public float startStrength;
        public float deltaStrength;
        public StrengthType strengthType;
    }

    public enum StrengthType
    {
        Linear = 0,
        SineOut = 1
    }
}