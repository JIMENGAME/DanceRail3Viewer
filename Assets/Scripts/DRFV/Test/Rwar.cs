#if UNITY_EDITOR
using System.IO;
using System.Text;
using DRFV.Game;
using DRFV.Game.HPBars;
using DRFV.inokana;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DRFV.Test
{
    public class Rwar : MonoSingleton<Rwar>
    {
        public Transform simpleNotes;

        [SerializeField] private Material[] _materials;

        [Range(1, 30)] public int noteSpeed = 12;

        public HPManager HpManager;

        // Start is called before the first frame update
        protected override void OnAwake()
        {
            using (FileStream fileStream = new FileStream("E:\\Users\\Administrator\\WebstormProjects\\DanceRail3Viewer\\Temp\\1.json", FileMode.Open, FileAccess.Read))
            {
                byte[] qwq = new byte[fileStream.Length];
                fileStream.Read(qwq);
                JArray jArray = JArray.Parse(Encoding.UTF8.GetString(qwq));
            }
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
                    Graphics.DrawMesh(simpleNote.GetMesh(), Vector3.zero, Quaternion.identity,
                        _materials[Mathf.Abs(simpleNote.materialId) % _materials.Length], 9);
                }
            }
        }
    }
}
#endif