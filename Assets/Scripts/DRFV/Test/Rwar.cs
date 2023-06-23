#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using DRFV.Game;
using DRFV.Game.HPBars;
using DRFV.Global;
using DRFV.inokana;
using DRFV.Setting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DRFV.Test
{
    public class Rwar : MonoSingleton<Rwar>
    {
        public Transform simpleNotes;

        [SerializeField] private Material[] _materials;

        [Range(1, 30)] public int noteSpeed = 12;

        public HPManager HpManager;

        public Text tRate;
        public Slider score, hard;

        // Start is called before the first frame update
        private void Start()
        {
            HpManager.Init(new HPBarDefault());

            DrawMesh();
            UpdateRate();
        }

        public void UpdateRate()
        {
            float original = StaticResources.Instance.ScoreToRate(score.value, (int) hard.value, 1.5f);
            float codeOnly = Util.ScoreToRate(score.value, (int) hard.value, 1.5f);
            tRate.text = $"分数：{Mathf.RoundToInt(score.value)}\n难度：{(int)hard.value}\n原始算法：{original:0.#####}\n纯代码算法：{codeOnly:0.#####}\n误差：{(original-codeOnly):0.#####}";
        }

        private IEnumerator Qwq()
        {
            yield return null;
        }

        private const int Delta = 10;

        // Update is called once per frame
        void Update()
        {
            int scoreValue = (int) score.value;
            if (scoreValue + Delta > score.maxValue || scoreValue < 2400000) score.value = 2400000f;
            else score.value = scoreValue + Delta;
            UpdateRate();
            DrawMesh();
        }

        private void DrawMesh()
        {
            for (int i = 0; i < simpleNotes.childCount; i++)
            {
                if (simpleNotes.GetChild(i).TryGetComponent(out SimpleNote simpleNote))
                {
                    Graphics.DrawMesh(simpleNote.GetMesh(), Vector3.zero, Quaternion.identity,
                        _materials[Mathf.Abs(simpleNote.materialId) % _materials.Length], 9);
                }
            }
        }
    }
}
#endif