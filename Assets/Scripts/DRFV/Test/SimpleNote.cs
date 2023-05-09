using UnityEngine;

namespace DRFV.Test
{
    public class SimpleNote : MonoBehaviour
    {
        public float parentPos = 0f, pos = 0f, parentWidth = 16.0f, width = 16.0f;
        public int materialId;
        private Mesh _mesh;
        private float z, pz;

        public Mesh GetMesh()
        {
            if (!_mesh)
            {
                _mesh = new Mesh
                {
                    vertices = new Vector3[]
                    {
                        new(1000, 1000, 0),
                        new(1000, 1000, 0),
                        new(1000, 1000, 0),
                        new(1000, 1000, 0),
                        new(1000, 1000, 0),
                        new(1000, 1000, 0),
                        new(1000, 1000, 0),
                        new(1000, 1000, 0),
                    },
                    triangles = new[]
                    {
                        0, 1, 2,
                        2, 1, 3,
                        2, 3, 4,
                        4, 3, 5,
                        4, 5, 6,
                        6, 5, 7,
                    },
                    normals = new Vector3[]
                    {
                        new(0, 0, -1),
                        new(0, 0, -1),
                        new(0, 0, -1),
                        new(0, 0, -1),
                        new(0, 0, -1),
                        new(0, 0, -1),
                        new(0, 0, -1),
                        new(0, 0, -1),
                    },
                    uv = new Vector2[]
                    {
                        new(0, 0),
                        new(0, 1),
                        new(0.25f, 0),
                        new(0.25f, 1),
                        new(0.75f, 0),
                        new(0.75f, 1),
                        new(1, 0),
                        new(1, 1),
                    }
                };
            }

            z = 10 * Rwar.Instance.noteSpeed;
            pz = 0;
            _mesh.vertices = new Vector3[]
            {
                new(parentPos - 8.0f, -0.1f, pz),
                new(pos - 8.0f, -0.1f, z),
                new(parentPos + 0.4f - 8.0f, -0.1f, pz),
                new(pos + 0.4f - 8.0f, -0.1f, z),
                new(parentPos + parentWidth - 0.4f - 8.0f, -0.1f, pz),
                new(pos + width - 0.4f - 8.0f, -0.1f, z),
                new(parentPos + parentWidth - 8.0f, -0.1f, pz),
                new(pos + width - 8.0f, -0.1f, z),
            };
            _mesh.RecalculateBounds();
            return _mesh;
        }
    }
}