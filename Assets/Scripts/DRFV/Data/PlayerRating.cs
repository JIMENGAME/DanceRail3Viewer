using System.Collections.Generic;
using System.Linq;
using DRFV.Global;
using Newtonsoft.Json;

namespace DRFV.Data
{
    public class PlayerRating
    {
        [JsonProperty("r")]
        public List<ResultData> Recent20List = new();
        [JsonProperty("b")]
        public List<ResultData> Best80List = new();

        private object locker = new();

        public void Clear()
        {
            Recent20List.Clear();
            Best80List.Clear();
        }

        public float GetRate()
        {
            List<float> rates = Best80List.Select(resultData => resultData.rate).ToList();
            rates.AddRange(Recent20List.Select(resultData => resultData.rate));
            while (rates.Count < 100) rates.Add(0f);
            return rates.Average();
        }

        public void PopResult(ResultData resultData)
        {
            lock (locker)
            {
                AppendBest(resultData);
                AppendRecent(resultData);
            }
        }

        private void AppendBest(ResultData resultData)
        {
            List<ResultData> newBests = Best80List.DeepClone().ToList();
            bool hasSame = false;
            for (int i = 0; i < newBests.Count; i++)
            {
                if (newBests[i].keyword != resultData.keyword) continue;
                newBests[i] = resultData;
                hasSame = true;
                break;
            }
            if (!hasSame) newBests.Add(resultData);
            var originalBestsRate = Best80List.Select(data => data.rate).ToList();
            var newBestsRate = newBests.Select(data => data.rate).ToList();
            while (originalBestsRate.Count < 80) originalBestsRate.Add(0f);
            while (newBestsRate.Count < 80) newBestsRate.Add(0f);
            newBestsRate.Sort();
            while (newBestsRate.Count > 80) newBestsRate.RemoveAt(0);
            if (newBestsRate.Average() > originalBestsRate.Average()) Best80List = newBests;
        }

        private void AppendRecent(ResultData resultData)
        {
            List<ResultData> newRecents = Recent20List.DeepClone().ToList();
            bool hasSame = false;
            for (int i = 0; i < newRecents.Count; i++)
            {
                if (newRecents[i].keyword != resultData.keyword) continue;
                newRecents.RemoveAt(i);
                newRecents.Add(resultData);
                hasSame = true;
                break;
            }
            if (!hasSame) newRecents.Add(resultData);
            var originalRecentsRate = Recent20List.Select(data => data.rate).ToList();
            var newRecentsRate = newRecents.Select(data => data.rate).ToList();
            while (originalRecentsRate.Count < 20) originalRecentsRate.Add(0f);
            while (newRecentsRate.Count < 20) newRecentsRate.Add(0f);
            while (newRecentsRate.Count > 20) newRecentsRate.RemoveAt(0);
            if (newRecentsRate.Average() < originalRecentsRate.Average() && resultData.score > 2950000) return;
            Recent20List = newRecents;
        }
    }
}