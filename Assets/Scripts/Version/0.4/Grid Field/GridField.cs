using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Extensions;
using UnityEngine;
using Version._0._4.Player;

namespace Version._0._4.Grid_Field
{
    public class GridField : MonoBehaviour
    {
        [SerializeField] private Vector2Int _GridResolution;
        
        [SerializeField] private GameObject _PrefabMeshInnerCircle;
        [SerializeField] private int _InnerRingSize;
        
        [SerializeField] private GameObject _PrefabMeshMiddleCircle;
        [SerializeField] private int _MiddleRingSize;
        
        [SerializeField] private GameObject _PrefabMeshOuterCircle;
        [SerializeField] private int _OuterRingSize;
        
        [SerializeField] private GameObject _InactiveReplacement;

        internal List<MeshGrid> MeshGridList { get; private set; } = new List<MeshGrid>();
        internal Mesh InnerMesh { get; private set; }
        internal Mesh MiddleMesh { get; private set; }
        internal Mesh OuterMesh { get; private set; }
        internal Vector3 MeshScale { get; private set; }
        internal MeshGrid[] InnerMeshGrids { get; private set; }
        internal MeshGrid[] MiddleMeshGrids { get; private set; }
        internal MeshGrid[] OuterMeshGrids { get; private set; }
        internal GameObject[] FlatPlanes { get; private set; }
        internal List<MeshGrid> ActiveMeshes = new List<MeshGrid>();
        internal Vector2Int GridMeshLengths { get; private set; }
        private int _GridCount;
        private int _RowColActiveEnd;
        private int _RowColActiveStart;
        private float _Scaling = 1;
        private Vector2 _BoundSize2D;
        
        internal void SetScaling(float scale) => _Scaling = scale;

        private void Awake()
        {
            GridMeshLengths = _GridResolution;
            _GridCount = _GridResolution.x * _GridResolution.y;
        }

        internal void GenerateGridField()
        {
            var arraySize = _GridResolution.x * _GridResolution.y;
            InnerMeshGrids = new MeshGrid[arraySize];
            MiddleMeshGrids = new MeshGrid[arraySize];
            OuterMeshGrids = new MeshGrid[arraySize];
            FlatPlanes = new GameObject[arraySize];
            
            // var bounds = new Bounds
            // {
            //     size = prefabBounds.size * _Scaling,
            //     max = prefabBounds.max * _Scaling,
            //     min = prefabBounds.min * _Scaling,
            //     center = prefabBounds.center * _Scaling,
            //     extents = prefabBounds.extents * _Scaling
            // };

            var innerPrefabMesh = _PrefabMeshInnerCircle.GetComponent<MeshFilter>().sharedMesh;
            InnerMesh = 
                PrepareMesh(innerPrefabMesh, "InnerCircleMesh");
            MiddleMesh = 
                PrepareMesh(_PrefabMeshMiddleCircle.GetComponent<MeshFilter>().sharedMesh, "MiddleCircleMesh");
            OuterMesh = 
                PrepareMesh(_PrefabMeshOuterCircle.GetComponent<MeshFilter>().sharedMesh, "OuterCircleMesh");
            
            MeshScale = innerPrefabMesh.bounds.size;
            _BoundSize2D = new Vector2(MeshScale.x, MeshScale.z) * _Scaling;
            
            var rotation = transform.rotation;
            
            for (var x = 0; x < _GridResolution.x; x++)
            {
                for (var z = 0; z < _GridResolution.y; z++)
                {
                    var index = x + z * _GridResolution.x;
                    var position = new Vector3(x * _BoundSize2D.x, 0, z * _BoundSize2D.y);
                    var meshShift = new Vector2(x * _Scaling, z * _Scaling);

                    GenerateMesh(InnerMesh, _PrefabMeshInnerCircle, InnerMeshGrids, index, position, rotation,
                        meshShift, MeshGrid.Circle._Inner_);
                    GenerateMesh(MiddleMesh, _PrefabMeshMiddleCircle, MiddleMeshGrids, index, position, rotation,
                        meshShift, MeshGrid.Circle._Middle_);
                    GenerateMesh(OuterMesh, _PrefabMeshOuterCircle, OuterMeshGrids, index, position, rotation,
                        meshShift, MeshGrid.Circle._Outer_);
                    
                    var flatPlane = Instantiate(_InactiveReplacement, position, rotation, transform);
                    flatPlane.transform.localScale *= _Scaling;
                    FlatPlanes[index] = flatPlane;
                }
            }
            
            UpdateVisibleGird();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Mesh PrepareMesh(Mesh mesh, string meshName = "Instance") =>  new Mesh
        {
            name = meshName,
            vertices = mesh.vertices,
            triangles = mesh.triangles,
            uv = mesh.uv,
            uv2 = mesh.uv2,
            colors = mesh.colors,
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GenerateMesh(Mesh mesh, GameObject prefab, IList<MeshGrid> gridArray, int index, Vector3 position,
            Quaternion rotation, Vector2 shift, MeshGrid.Circle circle)
        {
            var meshField = Instantiate(prefab, position, rotation, transform);
            var meshFieldFilter = meshField.GetComponent<MeshFilter>();
            meshFieldFilter.mesh = mesh;
            var meshGrid = new MeshGrid(meshField, meshFieldFilter.mesh, 
                MeshTable.GetFraction(mesh.vertexCount), shift, index, circle);
            gridArray[index] = meshGrid;
            MeshGridList.Add(meshGrid);
            meshField.SetActive(false);
        }

        #region Update

                private void Update()
        {
            UpdateVisibleGird();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateVisibleGird()
        {
            if(!PlayerMono.Player) return;
            var playerPos = PlayerMono.Player.transform.position;
            var gridPos = new Vector2Int(Mathf.RoundToInt(playerPos.x / _BoundSize2D.x),Mathf.RoundToInt(playerPos.z / _BoundSize2D.y));

            foreach (var activeMesh in ActiveMeshes)
            {
                if (!activeMesh.SceneObject) continue;
                activeMesh.SceneObject.SetActive(false);
                FlatPlanes[activeMesh.GridIndex].SetActive(true);
            }
            
            ActivateCircle(gridPos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ActivateCircle(Vector2Int gridPosition)
        {
            var activeGridArray = new List<MeshGrid>();

            for (var x = -_InnerRingSize + 1; x < _InnerRingSize; ++x)
            {
                for (var z = -_InnerRingSize + 1; z < _InnerRingSize; ++z)
                {
                    var gridIndex = x + gridPosition.x + (z + gridPosition.y) * _GridResolution.y;
                    if(gridIndex >= _GridCount || gridIndex < 0 || x + gridPosition.x > _GridResolution.x - 1 || x + gridPosition.x < 0) continue;
                    
                    var template = InnerMeshGrids[gridIndex];
                    template.SceneObject.SetActive(true);
                    activeGridArray.Add(template);
                    
                    FlatPlanes[gridIndex].SetActive(false);
                }
            }
            
            for (var x = -_MiddleRingSize + 1; x < _MiddleRingSize; ++x)
            {
                for (var z = -_MiddleRingSize + 1; z < _MiddleRingSize; ++z)
                {
                    if(x > -_InnerRingSize + 1 && x < _InnerRingSize && z > -_InnerRingSize + 1 && z < _InnerRingSize) continue;
                    var gridIndex = x + gridPosition.x + (z + gridPosition.y) * _GridResolution.y;
                    if(gridIndex >= _GridCount || gridIndex < 0 || x + gridPosition.x > _GridResolution.x - 1 || x + gridPosition.x < 0) continue;
                    
                    var template = MiddleMeshGrids[gridIndex];
                    template.SceneObject.SetActive(true);
                    activeGridArray.Add(template);
                    
                    FlatPlanes[gridIndex].SetActive(false);
                }
            }
            
            for (var x = -_OuterRingSize + 1; x < _OuterRingSize; ++x)
            {
                for (var z = -_OuterRingSize + 1; z < _OuterRingSize; ++z)
                {
                    if(x > -_MiddleRingSize && x < _MiddleRingSize && z > -_MiddleRingSize && z < _MiddleRingSize) continue;
                    var gridIndex = x + gridPosition.x + (z + gridPosition.y) * _GridResolution.y;
                    if(gridIndex >= _GridCount || gridIndex < 0 || x + gridPosition.x > _GridResolution.x - 1 || x + gridPosition.x < 0) continue;
                    
                    var template = OuterMeshGrids[gridIndex];
                    template.SceneObject.SetActive(true);
                    activeGridArray.Add(template);
                    
                    FlatPlanes[gridIndex].SetActive(false);
                }
            }
            
            ActiveMeshes = activeGridArray;
        }

        #endregion
    }
}
