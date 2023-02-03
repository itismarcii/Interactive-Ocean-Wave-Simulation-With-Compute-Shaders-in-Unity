using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Extensions;
using UnityEngine;

namespace Version._0._2.Base
{
    public class MeshManager : MonoBehaviour
    {

        private const int _KERNEL_ID_X = 32, _KERNEL_ID_Y = 1, _KERNEL_ID_Z = 32;

        [SerializeField] private ComputeShader _ComputeShader;
        [Space]
        [SerializeField] private MeshFilter _MeshFilter;
        [SerializeField] private float MaxHeightAmplifier = 0;
        [SerializeField] private WaveInformation _WaveInformation;

        private ComputeBuffer _VerticesBuffer, _UVBuffer;

        private readonly int
            _VerticesOutputBufferPropertyID = Shader.PropertyToID("VerticesOutput"),
            _UVOutputBufferPropertyID = Shader.PropertyToID("UvOutput"),
            _TriangleOutputBufferPropertyID = Shader.PropertyToID("TriangleOutput"),
            _GlobalTimePropertyID = Shader.PropertyToID("_TIME_"),
            _MeshResolutionPropertyID = Shader.PropertyToID("mesh_resolution"),
            _WaveInformationPropertyID = Shader.PropertyToID("wave_information"),
            _MaxHeightAmplifierPropertyID = Shader.PropertyToID("max_height_amplifier");

        private static float _GlobalTime = 0;
        private static int _MeshResolution;
        private static int _VertexCount;
        private static Mesh _Mesh;

        private void OnDisable()
        {
            _VerticesBuffer?.Dispose();
            _UVBuffer?.Dispose();
        }

        private void Start()
        {
            _Mesh = _MeshFilter.mesh;
            Setup();
        }

        private void Update()
        {
            MeshUpdate(out var vertices,out var uvs);
            _Mesh.vertices = vertices;
            _Mesh.uv = uvs;
        }

        private void Setup()
        {
            MeshTable.SetupTable(1000);

            _VertexCount = _Mesh.vertexCount;
            _MeshResolution = MeshTable.GetFraction(_VertexCount);
            _ComputeShader.SetInt(_MeshResolutionPropertyID, _MeshResolution);
            _ComputeShader.SetFloat(_MaxHeightAmplifierPropertyID, MaxHeightAmplifier);
            
            _VerticesBuffer = new ComputeBuffer(_VertexCount, sizeof(float) * 3);
            _UVBuffer = new ComputeBuffer(_VertexCount, sizeof(float) * 2);

            using (var triangleBuffer = new ComputeBuffer(_VertexCount * 3, sizeof(int)))
            {
                _MeshFilter.mesh.triangles = GetBufferData(1, triangleBuffer, _TriangleOutputBufferPropertyID,
                    _MeshFilter.mesh.triangles);
            }
        }

        private void MeshUpdate(out Vector3[] vertices, out Vector2[] uvs)
        {
            _ComputeShader.SetFloat(_GlobalTimePropertyID, _GlobalTime);
            _ComputeShader.SetVector(_WaveInformationPropertyID, _WaveInformation.ToVector4());

            _ComputeShader.SetBuffer(0, _VerticesOutputBufferPropertyID, _VerticesBuffer);
            _ComputeShader.SetBuffer(0, _UVOutputBufferPropertyID, _UVBuffer);
            _ComputeShader.Dispatch(0, 32, 1, 32);

            vertices = new Vector3[_VertexCount];
            uvs = new Vector2[_VertexCount];
            
            _VerticesBuffer.GetData(vertices);
            _UVBuffer.GetData(uvs);

            _GlobalTime += Time.deltaTime;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T[] GetBufferData<T>(int kernel, ComputeBuffer buffer, int propertyId, T[] data)
        {
            if (buffer == null) Debug.Log("No buffer");
        
            using (buffer = new ComputeBuffer(data.Length, Marshal.SizeOf<T>()))
            {
                _ComputeShader.SetBuffer(kernel, propertyId, buffer);
                _ComputeShader.Dispatch(kernel, _KERNEL_ID_X , _KERNEL_ID_Y, _KERNEL_ID_Z);
                var bufferData = new T[data.Length];
                buffer.GetData(bufferData);
                return bufferData;
            }
        }
        
    }
}
