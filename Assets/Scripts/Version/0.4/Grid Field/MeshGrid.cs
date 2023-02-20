using UnityEngine;

namespace Version._0._4.Grid_Field
{
    internal struct MeshGrid
    {
        internal GameObject SceneObject { get; private set; }
        internal Mesh GridMesh { get; private set; }
        internal Vector2 Shift { get; private set; }

        public MeshGrid(GameObject sceneObject, Mesh gridMesh, Vector2 shift)
        {
            GridMesh = gridMesh;
            Shift = shift;
            SceneObject = sceneObject;
        }
    }
}
