using DRFV.Game;
using DRFV.Game.HPBars;
using DRFV.inokana;
using UnityEngine;

namespace DRFV.Test
{
    public class Rwar : MonoSingleton<Rwar>
    {
        public Transform simpleNotes;

        [SerializeField]
        private Material[] _materials;

        [Range(1, 30)]
        public int noteSpeed = 12;

        public HPManager HpManager;

        // Start is called before the first frame update
        protected override void OnAwake()
        {
            HpManager.Init(new HPBarDefault());
            DrawMesh();
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
                    Graphics.DrawMesh(simpleNote.GetMesh(), Vector3.zero, Quaternion.identity, _materials[Mathf.Abs(simpleNote.materialId) % _materials.Length], 9);

                }
            }
        }
    }
}