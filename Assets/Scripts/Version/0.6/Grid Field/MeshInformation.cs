using UnityEngine;

namespace Version._0._6.Grid_Field
{
    public struct MeshInformation
    {
        public Mesh Mesh;
        public ComputeBuffer VerticesBuffer;
        public Vector2 Shift;
        public Vector3 GridShift;
        public Vector3[] VerticesData;
        public float LastUpdateTime;
    }
}
