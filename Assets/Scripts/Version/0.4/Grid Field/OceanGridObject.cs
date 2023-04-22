using System;
using System.Collections.Generic;
using UnityEngine;

namespace Version._0._4.Grid_Field
{
    [RequireComponent(typeof(MeshFilter))]
    public class OceanGridObject : MonoBehaviour
    {
        public static readonly Dictionary<int, Dictionary<int, MeshInformation>> HashTable =
            new Dictionary<int, Dictionary<int, MeshInformation>>();
        
        private MeshInformation _Information;
        
        private static readonly int 
            VerticesPropertyID = Shader.PropertyToID("vertices"), 
            UVsPropertyID = Shader.PropertyToID("uvs");

        public void Setup(int index, int x, int z, Vector2 shift)
        {
            var mesh = GetComponent<MeshFilter>().mesh;
            
            _Information = new MeshInformation()
            {
                Mesh = mesh,
                VerticesBuffer = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 3),
                Shift = shift
            };
            
            if (HashTable.TryGetValue(x, out var value))
            {
                value.Add(z, _Information);
            }
            else
            {
                HashTable.Add(x, new Dictionary<int, MeshInformation> {{z, _Information}});
            }

            var material = GetComponent<Renderer>().material;
            material.SetBuffer(VerticesPropertyID, _Information.VerticesBuffer);
        }

        public MeshInformation GetMeshInformation() => _Information;

        private void OnDestroy()
        {
            _Information.VerticesBuffer?.Dispose();
        }
    }
}
