using System;
using System.Collections.Generic;
using UnityEngine;

namespace Version._0._6.Grid_Field
{
    [RequireComponent(typeof(MeshFilter))]
    public class OceanGridObject : MonoBehaviour
    {
        public static readonly Dictionary<int, Dictionary<int, MeshInformation>> HashTable =
            new Dictionary<int, Dictionary<int, MeshInformation>>();
        
        private MeshInformation _MeshInformation;

        private static readonly int
            VerticesPropertyID = Shader.PropertyToID("vertices"),
            UVsPropertyID = Shader.PropertyToID("uvs"),
            ResolutionPropertyID = Shader.PropertyToID("mesh_resolution");

        public void Setup(int index, int x, int z, Vector2 shift)
        {
            var mesh = GetComponent<MeshFilter>().mesh;
            
            _MeshInformation = new MeshInformation()
            {
                Mesh = mesh,
                VerticesBuffer = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 3),
                Shift = shift,
                GridShift = new Vector3(shift.x, 1, shift.y),
                LastUpdateTime = Time.time,
                VerticesData = new Vector3[mesh.vertexCount]
            };
            
            if (HashTable.TryGetValue(x, out var value))
            {
                value.Add(z, _MeshInformation);
            }
            else
            {
                HashTable.Add(x, new Dictionary<int, MeshInformation> {{z, _MeshInformation}});
            }

            var material = GetComponent<Renderer>().material;
            material.SetBuffer(VerticesPropertyID, _MeshInformation.VerticesBuffer);
            material.SetInt(ResolutionPropertyID, GridField.MeshResolution);
        }

        public MeshInformation GetMeshInformation() => _MeshInformation;

        private void OnDestroy()
        {
            _MeshInformation.VerticesBuffer?.Dispose();
        }
    }
}
