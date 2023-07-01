using Newtonsoft.Json;

namespace DRFV.Data
{
    public class ResultData : IDeepCloneable<ResultData>
    {
        [JsonProperty("keyword")] public string keyword;
        [JsonProperty("score")] public int score;
        [JsonProperty("type")] public string endType;
        [JsonProperty("perfectj")] public int perfectJ;
        [JsonProperty("perfect")] public int perfect;
        [JsonProperty("good")] public int good;
        [JsonProperty("miss")] public int miss;
        [JsonProperty("fast")] public int fast;
        [JsonProperty("slow")] public int slow;
        [JsonProperty("hp")] public float hp;
        [JsonProperty("acc")] public float acc;
        [JsonProperty("rate")] public float rate;

        public ResultData()
        {
            keyword = "";
            score = 0;
            endType = "cp";
            acc = perfectJ = perfect = good = miss = fast = slow = 0;
            hp = 114.514f;
        }

        public ResultData DeepClone()
        {
            return new ResultData
            {
                score = score,
                endType = endType,
                perfectJ = perfectJ,
                perfect = perfect,
                good = good,
                miss = miss,
                fast = fast,
                slow = slow,
                hp = hp,
                acc = acc,
                rate = rate
            };
        }
    }
}