using UnityEngine;

namespace Version._0._4.Grid_Field
{
    internal struct MeshGrid
    {
        internal enum Circle
        {
            _Inner_,
            _Middle_,
            _Outer_
        }
        
        internal GameObject SceneObject { get; private set; }
        internal Mesh GridMesh { get; private set; }
        internal Vector2 MeshShift { get; private set; }
        internal int GridIndex { get; private set; }
        internal int Resolution { get; private set; }
        internal Circle CircleStage { get; private set; }
        internal int VertexCount { get; private set; }
        

        public MeshGrid(GameObject sceneObject, Mesh gridMesh, int resolution, Vector2 meshShift, int gridIndex, Circle circle)
        {
            SceneObject = sceneObject;
            GridMesh = gridMesh;
            Resolution = resolution;
            MeshShift = meshShift;
            GridIndex = gridIndex;
            CircleStage = circle;
            VertexCount = gridMesh.vertexCount;
        }
    }
}
