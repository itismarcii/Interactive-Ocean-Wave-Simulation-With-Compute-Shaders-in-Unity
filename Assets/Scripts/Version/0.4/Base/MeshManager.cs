using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Extensions;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Version._0._4.Grid_Field;
using Debug = UnityEngine.Debug;

namespace Version._0._4.Base
{
    public class MeshManager : MonoBehaviour
    {
        private const int _KERNEL_ID_X = 32, _KERNEL_ID_Y = 1, _KERNEL_ID_Z = 32;

        [SerializeField] private GridField _GridField;
        [SerializeField] private ComputeShader _ComputeShader;
        [Space]
        // [SerializeField] private MeshFilter _MeshFilter;
        [SerializeField] private float MaxHeightAmplifier = 0;
        [SerializeField] private float _Scaling = 1;
        [SerializeField] private WaveParameter[] _WaveParameters;

        private ComputeBuffer _VerticesBuffer, _UVBuffer, _WaveParameterBuffer;

        private readonly int
            _VerticesOutputBufferPropertyID = Shader.PropertyToID("VerticesOutput"),
            _UVOutputBufferPropertyID = Shader.PropertyToID("UvOutput"),
            _TriangleOutputBufferPropertyID = Shader.PropertyToID("TriangleOutput"),
            _GlobalTimePropertyID = Shader.PropertyToID("_TIME_"),
            _MeshResolutionPropertyID = Shader.PropertyToID("mesh_resolution"),
            _WaveParameterBufferPropertyID = Shader.PropertyToID("WaveParameters"),
            _WaveInformationArrayLengthPropertyID = Shader.PropertyToID("wave_parameter_count"),
            _MaxHeightAmplifierPropertyID = Shader.PropertyToID("max_height_amplifier"),
            _ScalingPropertyId = Shader.PropertyToID("scaling"),
            _MeshShiftPropertyId = Shader.PropertyToID("meshShift");

        private Vector4[] _WaveArray = Array.Empty<Vector4>();
        
        private float _GlobalTime = 0;
        private int _MeshResolution;
        private Vector3 _MeshScale;
        private Vector2 _MeshScale2D;
        private int _VertexCount;
        private Mesh _Mesh;
        private Vector2 _GridMeshLengths;
        
        private void OnDisable()
        {
            _VerticesBuffer?.Dispose();
            _UVBuffer?.Dispose();
            _WaveParameterBuffer?.Dispose();
        }

        private void Start()
        {
            MeshTable.SetupTable(1000);
            Setup(_GridField.PrefabMesh);
            _GridField.GenerateGridField();

            _GridMeshLengths = _GridField.GridMeshLengths;
            _MeshScale = _GridField.MeshScale;
            _MeshScale2D = new Vector2(_MeshScale.x, _MeshScale.z);

            foreach (var meshGridInfo in _GridField.ActiveMeshes)  
            { 
                UpdateMesh(meshGridInfo.GridMesh, meshGridInfo.Shift * _MeshScale2D);
            }
            
        }

        private void Update()
        {
            UpdateGrid();
        }

        private void UpdateGrid()
        {
            _ComputeShader.SetFloat(_GlobalTimePropertyID, _GlobalTime);
            _ComputeShader.SetFloat(_MaxHeightAmplifierPropertyID, MaxHeightAmplifier);
            
            foreach (var meshGridInfo in _GridField.ActiveMeshes)  
            {
                UpdateMesh(meshGridInfo.GridMesh, meshGridInfo.Shift * _MeshScale2D);
            }
            
            _GlobalTime += Time.deltaTime;
        }

        private void UpdateMesh(Mesh mesh, Vector2 shift)
        {
            MeshUpdate(out var vertices,out var uvs, in shift);
            mesh.vertices = vertices;
            mesh.uv = uvs;
        }
        
        private void Setup(Mesh mesh)
        {
            _Mesh = mesh;
            _VertexCount = mesh.vertexCount;
            _MeshResolution = MeshTable.GetFraction(_VertexCount);
            
            _ComputeShader.SetInt(_MeshResolutionPropertyID, _MeshResolution);
            _ComputeShader.SetInt(_WaveInformationArrayLengthPropertyID, _WaveParameters.Length);
            _ComputeShader.SetFloat(_ScalingPropertyId, (10 / (float) (_MeshResolution - 1)) * _Scaling);

            _VerticesBuffer = new ComputeBuffer(_VertexCount, sizeof(float) * 3);
            _UVBuffer = new ComputeBuffer(_VertexCount, sizeof(float) * 2);
            _WaveParameterBuffer = new ComputeBuffer(_WaveParameters.Length, sizeof(float) * 5);

            using var triangleBuffer = new ComputeBuffer(_VertexCount * 3, sizeof(int));
                mesh.triangles = GetBufferData(1, triangleBuffer, _TriangleOutputBufferPropertyID, mesh.triangles);

        }
        
        private void MeshUpdate(out Vector3[] vertices, out Vector2[] uvs, in Vector2 shift)
        {
            _ComputeShader.SetVector(_MeshShiftPropertyId, shift);
            _WaveParameterBuffer.SetData(_WaveParameters);

            _ComputeShader.SetBuffer(0, _VerticesOutputBufferPropertyID, _VerticesBuffer);
            _ComputeShader.SetBuffer(0, _UVOutputBufferPropertyID, _UVBuffer);
            _ComputeShader.SetBuffer(0, _WaveParameterBufferPropertyID, _WaveParameterBuffer);
            _ComputeShader.Dispatch(0, 32, 1, 32);
            
            vertices = new Vector3[_VertexCount];
            uvs = new Vector2[_VertexCount];
            
            _VerticesBuffer.GetData(vertices);
            _UVBuffer.GetData(uvs);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T[] GetBufferData<T>(int kernel, ComputeBuffer buffer, int propertyId, IReadOnlyCollection<T> data)
        {
            if (buffer == null) Debug.Log("No buffer");
        
            using (buffer = new ComputeBuffer(data.Count, Marshal.SizeOf<T>()))
            {
                _ComputeShader.SetBuffer(kernel, propertyId, buffer);
                _ComputeShader.Dispatch(kernel, _KERNEL_ID_X , _KERNEL_ID_Y, _KERNEL_ID_Z);
                var bufferData = new T[data.Count];
                buffer.GetData(bufferData);
                return bufferData;
            }
        }
    }
}
