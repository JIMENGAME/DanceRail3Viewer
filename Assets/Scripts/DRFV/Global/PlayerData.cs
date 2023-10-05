using System;
using DRFV.Global.Utilities;
using DRFV.inokana;
using UnityEngine;

namespace DRFV.Global
{
    public class PlayerData : MonoSingleton<PlayerData>
    {
        public long PrimeEndTime;
        public int coin;
        public int purpleCard;
        public int exp, masterLevel, level;
        public int changeNickNameCard;
        public int skillTier;
        public bool IsPrime => Util.DateTimeToTimeStamp(DateTime.Now) <= PrimeEndTime;
        [SerializeField] private AnimationCurve ExpLevelCurve;

        protected override void OnAwake()
        {
            coin = PlayerPrefs.GetInt("PlayerCoin", 2000000000);
            exp = PlayerPrefs.GetInt("PlayerExp", 8000);
            PrimeEndTime = long.Parse(PlayerPrefs.GetString("PrimeEndTime", "0"));
            changeNickNameCard = PlayerPrefs.GetInt("ChangeNameCard", 1);
            purpleCard = PlayerPrefs.GetInt("MurasakiCard", 0);
            skillTier = PlayerPrefs.GetInt("PlayerTier", 12);
            for (int index1 = 1; index1 <= 12; ++index1)
            {
                for (int index2 = 0; index2 < 2; ++index2)
                {
                    if (PlayerPrefs.GetInt("skill_check_" + index1 + "0" + index2, 0) != 0 && skillTier < index1)
                    {
                        skillTier = index1;
                    }
                }
            }
            RefreshLevel();
        }

        public void RefreshLevel()
        {
            level = ExpToLevel();
        }

        public int ExpToLevel()
        {
            while (exp >= 10000001)
            {
                ++masterLevel;
                exp -= 10000000;
            }

            return masterLevel * 100 +
                    Mathf.FloorToInt(ExpLevelCurve.Evaluate(exp));
        }
        
        public int NextLevel()
        {
            if (exp >= 10000000)
                return -1;
            int num = Mathf.FloorToInt(ExpLevelCurve.Evaluate(exp));
            int playerExp = exp;
            while (Mathf.FloorToInt(ExpLevelCurve.Evaluate(playerExp)) <= num)
            {
                ++playerExp;
            }

            return playerExp - exp;
        }

        public float ExpToLevelF()
        {
            while (exp >= 10000001)
            {
                ++masterLevel;
                exp -= 10000000;
            }

            Save();

            return masterLevel * 100f + ExpLevelCurve.Evaluate(exp);
        }

        public void BuyPrime(int d)
        {
            PrimeEndTime = Math.Max(Util.DateTimeToTimeStamp(DateTime.Now), PrimeEndTime) + d * 24L * 60L * 60L;
        }

        public void Save()
        {
            PlayerPrefs.SetString("PrimeEndTime", PrimeEndTime + "");
            PlayerPrefs.SetInt("ChangeNameCard", changeNickNameCard);
            PlayerPrefs.SetInt("MurasakiCard", purpleCard);
            PlayerPrefs.SetInt("PlayerCoin", coin);
            PlayerPrefs.SetInt("PlayerExp", exp);
            PlayerPrefs.SetInt("PlayerMasterLevel", masterLevel);
            PlayerPrefs.SetInt("PlayerTier", skillTier);
            PlayerPrefs.Save();
        }

#if false
        private void ExpCurveDump()
        {
            List<Keyframe> keyframes = new List<Keyframe>();
            keyframes.Add(new Keyframe
            {
                time = 0f,
                value = 1f,
                inTangent = 1 / 220f,
                outTangent = 1 / 220f,
                inWeight = 1 / 3f,
                outWeight = 1 / 3f,
                weightedMode = WeightedMode.None
            });
            keyframes.Add(new Keyframe
            {
                time = 2000f,
                value = 10f,
                inTangent = 1 / 220f,
                outTangent = 1 / 600f,
                inWeight = 1 / 3f,
                outWeight = 1 / 3f,
                weightedMode = WeightedMode.Both
            });
            keyframes.Add(new Keyframe
            {
                time = 8000f,
                value = 20f,
                inTangent = 1 / 600f,
                outTangent = 1 / 1200f,
                inWeight = 1 / 3f,
                outWeight = 1 / 3f,
                weightedMode = WeightedMode.None
            });
            keyframes.Add(new Keyframe
            {
                time = 20000f,
                value = 30f,
                inTangent = 1 / 1200f,
                outTangent = 1 / 3000f,
                inWeight = 1 / 3f,
                outWeight = 1 / 3f,
                weightedMode = WeightedMode.None
            });
            keyframes.Add(new Keyframe
            {
                time = 50000f,
                value = 40f,
                inTangent = 1 / 3000f,
                outTangent = 1 / 10000f,
                inWeight = 1 / 3f,
                outWeight = 1 / 3f,
                weightedMode = WeightedMode.None
            });
            keyframes.Add(new Keyframe
            {
                time = 150000f,
                value = 50f,
                inTangent = 1 / 10000f,
                outTangent = 1 / 20000f,
                inWeight = 1 / 3f,
                outWeight = 1 / 3f,
                weightedMode = WeightedMode.None
            });
            keyframes.Add(new Keyframe
            {
                time = 350000f,
                value = 60f,
                inTangent = 3 / 88000f,
                outTangent = 3 / 88000f,
                inWeight = 1 / 3f,
                outWeight = 1 / 3f,
                weightedMode = WeightedMode.None
            });
            keyframes.Add(new Keyframe
            {
                time = 900000f,
                value = 70f,
                inTangent = 1 / 55000f,
                outTangent = 1 / 160000f,
                inWeight = 1 / 3f,
                outWeight = 1 / 3f,
                weightedMode = WeightedMode.None
            });
            keyframes.Add(new Keyframe
            {
                time = 2500000f,
                value = 80f,
                inTangent = 1 / 160000f,
                outTangent = 1 / 200000f,
                inWeight = 1 / 3f,
                outWeight = 1 / 3f,
                weightedMode = WeightedMode.None
            });
            keyframes.Add(new Keyframe
            {
                time = 4500000f,
                value = 90f,
                inTangent = 1 / 200000f,
                outTangent = 1 / 550000f,
                inWeight = 1 / 3f,
                outWeight = 1 / 3f,
                weightedMode = WeightedMode.None
            });
            keyframes.Add(new Keyframe
            {
                time = 10000000f,
                value = 100f,
                inTangent = 1 / 550000f,
                outTangent = 1 / 200f,
                inWeight = 1 / 3f,
                outWeight = 0f,
                weightedMode = WeightedMode.None
            });
            keyframes.Add(new Keyframe
            {
                time = 10002000f,
                value = 110f,
                inTangent = 1 / 200f,
                outTangent = 1 / 1600f,
                inWeight = 0,
                outWeight = 0f,
                weightedMode = WeightedMode.None
            });
            keyframes.Add(new Keyframe
            {
                time = 10050000f,
                value = 140f,
                inTangent = 1 / 1600f,
                outTangent = 0,
                inWeight = 0,
                outWeight = 0f,
                weightedMode = WeightedMode.None
            });
            ExpLevelCurve = new AnimationCurve(keyframes.ToArray());
            ExpLevelCurve.postWrapMode = WrapMode.ClampForever;
            ExpLevelCurve.preWrapMode = WrapMode.ClampForever;
        }
#endif
    }
}