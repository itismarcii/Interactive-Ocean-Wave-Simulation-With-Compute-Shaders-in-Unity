using System;
using Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Version._0._4.Base
{
    public class GridField : MonoBehaviour
    {
        [SerializeField] private Vector2Int _GridResolution;
        [SerializeField] private GameObject _PrefabMesh;
        internal Mesh PrefabMesh { get; private set; }
        internal int VertexCount { get; private set; }
        internal Vector3 MeshScale { get; private set; }
        internal Mesh[,] GridMeshes;
        internal Vector2Int GridMeshLengths { get; private set; }
        
        private static bool PrefabCheck(GameObject prefab) => prefab.GetComponent<MeshFilter>() && prefab.GetComponent<MeshRenderer>();

        private void Awake()
        {
            GridMeshLengths = _GridResolution;
            PrefabMesh = _PrefabMesh.GetComponent<MeshFilter>().sharedMesh;
            MeshScale = PrefabMesh.bounds.size;
            VertexCount = PrefabMesh.vertexCount;
        }

        internal void GenerateGridField()
        {
            if(!PrefabCheck(_PrefabMesh)) return;
            
            GridMeshes = new Mesh[_GridResolution.x, _GridResolution.y];
            
            for (var x = 0; x < _GridResolution.x; x++)
            {
                for (var z = 0; z < _GridResolution.y; z++)
                {
                    var meshField = 
                        Instantiate(_PrefabMesh,
                        new Vector3(x * PrefabMesh.bounds.size.x, 0, z * PrefabMesh.bounds.size.z), 
                            transform.rotation, transform);
                    
                    GridMeshes[x, z] = meshField.GetComponent<MeshFilter>().mesh;
                    // meshField.SetActive(false);
                }
            }
        }
    }
}
