using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Version._0._4.Player;

namespace Version._0._4.Grid_Field
{
    public class GridField : MonoBehaviour
    {
        [SerializeField] private Vector2Int _GridResolution;
        [SerializeField] private GameObject _PrefabMesh;
        [Space] [SerializeField] private int ActiveMeshRow = 4;
        internal Mesh PrefabMesh { get; private set; }
        internal int VertexCount { get; private set; }
        internal Vector3 MeshScale { get; private set; }
        internal MeshGrid[] MeshGrids;
        internal MeshGrid[] ActiveMeshes;
        internal Vector2Int GridMeshLengths { get; private set; }
        private int _GridCount;
        private int _RowColActiveEnd;
        private int _RowColActiveStart;
        
        private static bool PrefabCheck(GameObject prefab) => prefab.GetComponent<MeshFilter>() && prefab.GetComponent<MeshRenderer>();

        private void Awake()
        {
            GridMeshLengths = _GridResolution;
            PrefabMesh = _PrefabMesh.GetComponent<MeshFilter>().sharedMesh;
            MeshScale = PrefabMesh.bounds.size;
            VertexCount = PrefabMesh.vertexCount;
            ActiveMeshes = new MeshGrid[ActiveMeshRow * ActiveMeshRow];
            _GridCount = _GridResolution.x * _GridResolution.y;
            _RowColActiveEnd = (ActiveMeshRow / 2) + 2;
            _RowColActiveStart = -(ActiveMeshRow - ActiveMeshRow / 2) + 1;
        }

        internal void GenerateGridField()
        {
            if(!PrefabCheck(_PrefabMesh)) return;
            
            MeshGrids = new MeshGrid[_GridResolution.x * _GridResolution.y];
            
            for (var x = 0; x < _GridResolution.x; x++)
            {
                for (var z = 0; z < _GridResolution.y; z++)
                {
                    var meshField = 
                        Instantiate(_PrefabMesh,
                        new Vector3(x * PrefabMesh.bounds.size.x, 0, z * PrefabMesh.bounds.size.z), 
                            transform.rotation, transform);
                    
                    MeshGrids[x + z * _GridResolution.x] =
                        new MeshGrid(meshField, meshField.GetComponent<MeshFilter>().mesh, new Vector2(x, z));
                    meshField.SetActive(false);
                }
            }
            
            UpdateVisibleGird();
        }

        private void Update()
        {
            UpdateVisibleGird();
        }

        private void UpdateVisibleGird()
        {
            if(!PlayerMono.Player) return;
            var playerPos = PlayerMono.Player.transform.position;
            var gridPos = new Vector2((int)(playerPos.x / MeshScale.x),(int)(playerPos.z / MeshScale.z));
            
            foreach (var activeMesh in ActiveMeshes)
                if(activeMesh.SceneObject) activeMesh.SceneObject.SetActive(false);
            
            var newActiveMesh = new List<MeshGrid>();

            for (var x = _RowColActiveStart; x < _RowColActiveEnd; x++)
            {
                for (var z = _RowColActiveStart; z < _RowColActiveEnd; z++)
                {
                    var gridIndex = (int) (x + gridPos.x + (z + gridPos.y) * _GridResolution.y);
                    if(gridIndex >= _GridCount || gridIndex < 0 || x + gridPos.x > _GridResolution.x - 1 || x + gridPos.x < 0) continue;
                    var template = MeshGrids[gridIndex];
                    template.SceneObject.SetActive(true);
                    newActiveMesh.Add(template);
                }
            }

            ActiveMeshes = newActiveMesh.ToArray();
        }
    }
}
