using UnityEngine;

public class MaterialTestScr : MonoBehaviour
{
    private ComputeBuffer _VerticesBuffer;
    private ComputeBuffer _VerticesMeshBuildUp;
    private ComputeBuffer _TriangleBuffer;
    private ComputeBuffer _UVsBuffer;

    private Material _Material;

    [SerializeField] private ComputeShader _Shader;
    [SerializeField] private MeshFilter _Mesh;
    
    private static readonly int 
        Vertices = Shader.PropertyToID("vertices"), 
        Uvs = Shader.PropertyToID("uvs");

    void Start()
    {
        _Material = GetComponent<Renderer>().material;

        var mesh = _Mesh.mesh;
        _VerticesBuffer = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 4, ComputeBufferType.Structured);
        _VerticesMeshBuildUp = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 3, ComputeBufferType.Structured);
        _UVsBuffer = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 2, ComputeBufferType.Structured);
        _TriangleBuffer = new ComputeBuffer(mesh.vertexCount * 6, sizeof(int), ComputeBufferType.Structured);
        
        _Material.SetBuffer(Vertices, _VerticesBuffer);
        _Material.SetBuffer(Uvs, _UVsBuffer);
        
        _Shader.SetBuffer(0, "VerticesOutput", _VerticesBuffer);
        _Shader.SetBuffer(0, "VerticesMeshBuildup", _VerticesMeshBuildUp);
        _Shader.SetBuffer(0, "TrianglesOutput", _TriangleBuffer);
        _Shader.SetBuffer(0, "UvOutput", _UVsBuffer);
        
        _Shader.Dispatch(0, 32, 1, 32);

        var v_arr = new Vector3[mesh.vertexCount];
        var u_arr = new Vector2[mesh.vertexCount];
        var t_arr = new int[mesh.vertexCount * 6];

        _VerticesMeshBuildUp.GetData(v_arr);
        _UVsBuffer.GetData(u_arr);
        _TriangleBuffer.GetData(t_arr);
        
        mesh.vertices = v_arr;
        mesh.uv = u_arr;
        mesh.triangles = t_arr;
    }

    private void Update()
    {
        // _Shader.Dispatch(0, 32, 1, 32);
    }

    private void OnDestroy()
    {
        _VerticesBuffer?.Dispose();
        _UVsBuffer?.Dispose();
        _VerticesMeshBuildUp?.Dispose();
        _TriangleBuffer?.Dispose();
    }
}
