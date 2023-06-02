#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using DRFV.Game;
using DRFV.Game.HPBars;
using DRFV.Global;
using DRFV.inokana;
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

        public Button pjskLongTest;
        public AudioSource longSource;
        private Text componentInChildren;

        // Start is called before the first frame update
        protected override void OnAwake()
        {
            HpManager.Init(new HPBarDefault());
            DrawMesh();
            
            componentInChildren = pjskLongTest.GetComponentInChildren<Text>();
            pjskLongTest.onClick.AddListener(UpdateLongSource);
        }

        // Update is called once per frame
        void Update()
        {
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

        public void UpdateLongSource()
        {
            if (longSource.isPlaying)
            {
                longSource.Stop();
                componentInChildren.text = "开始播放";

            }
            else {
                longSource.Play();
                componentInChildren.text = "停止播放";
            }
        }
    }
}
#endif