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

        public AudioSource longSource;

        // Start is called before the first frame update
        protected override void OnAwake()
        {
            HpManager.Init(new HPBarDefault());
            DrawMesh();
        }

        private IEnumerator Qwq()
        {
            yield return null;
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
    }
}
#endif