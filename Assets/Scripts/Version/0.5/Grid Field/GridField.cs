using System;
using Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Version._0._5.Grid_Field
{
    public class GridField : MonoBehaviour
    {
        [SerializeField] private OceanGridObject WaveTemplate;
        [SerializeField] private Vector2Int GridFieldResolution;

        public bool IsGenerated { get; private set; } = false;
        public static int MeshResolution { get; private set; } = 0;
        public static int MeshVertexCount { get; private set; } = 0;
        public static Vector3 MeshScale { get; private set; }

        public Vector2Int GetGridFieldResolution() => GridFieldResolution;
        
        public void GenerateGrid(float scaling = 1)
        {
            if(IsGenerated) return;
            
            MeshTable.SetupTable(1000);

            transform.position = Vector3.zero;
            
            var mesh = WaveTemplate.GetComponent<MeshFilter>().mesh;
            var shift = new Vector2(mesh.bounds.size.x, mesh.bounds.size.z) * scaling;

            MeshVertexCount = mesh.vertexCount;
            MeshResolution = MeshTable.GetFraction(MeshVertexCount);
            MeshScale = mesh.bounds.size;

            var bounds = new Bounds()
            {
                center = mesh.bounds.center,
                size = mesh.bounds.size * scaling,
            };
            
            
            for (var x = 0; x < GridFieldResolution.x; x++)
            {
                for (var z = 0; z < GridFieldResolution.y; z++)
                {
                    var obj = Instantiate(WaveTemplate, transform);
                    obj.Setup(x + z * GridFieldResolution.y, x, z, new Vector2(x * scaling, z * scaling) * new Vector2(MeshScale.x, MeshScale.z));
                    obj.transform.position = new Vector3(x * shift.x, transform.position.y, z * shift.y);
                    obj.GetMeshInformation().Mesh.bounds = bounds;
                }
            }
            
            WaveTemplate.gameObject.SetActive(false);

            IsGenerated = true;
        }
        
        public void ClearGrid()
        {
            OceanGridObject.HashTable.Clear();

            for (var i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            WaveTemplate.gameObject.SetActive(true);

            IsGenerated = false;
        }
    }
}
