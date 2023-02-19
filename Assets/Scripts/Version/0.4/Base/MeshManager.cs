using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Extensions;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Version._0._4.Base
{
    public class MeshManager : MonoBehaviour
    {
        [SerializeField] private int _WaveMultiplier = 1;
        [Space]
        
        private const int _KERNEL_ID_X = 32, _KERNEL_ID_Y = 1, _KERNEL_ID_Z = 32;

        [SerializeField] private ComputeShader _ComputeShader;
        [Space]
        [SerializeField] private MeshFilter _MeshFilter;
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
            _ScalingPropertyId = Shader.PropertyToID("scaling");

        private Vector4[] _WaveArray = Array.Empty<Vector4>();
        
        private float _GlobalTime = 0;
        private int _MeshResolution;
        private int _VertexCount;
        private Mesh _Mesh;
        
        private Stopwatch _Stopwatch = new Stopwatch();
        private float _StartTime = 0;
        private const int _END_TIME = 60;
        private int _CalculationAmount = 0;
        private TimeSpan _DurationCalculation;
        
        private void OnDisable()
        {
            _VerticesBuffer?.Dispose();
            _UVBuffer?.Dispose();
            _WaveParameterBuffer?.Dispose();
        }

        private void Start()
        {
            AlternateArray();
            _Mesh = _MeshFilter.mesh;
            Setup();
        }
        
        private void Update()
        {
            if (_StartTime >= _END_TIME)
            {
                var average = _DurationCalculation / _CalculationAmount;
                Debug.Log($"Total: {_DurationCalculation}  Avg: {average}  AvgInMilli: {average.Milliseconds}");
                return;
            }
            
            _Stopwatch.Start();

            MeshUpdate(out var vertices,out var uvs);
            _Mesh.vertices = vertices;
            _Mesh.uv = uvs;
            
            _Stopwatch.Stop();
            _DurationCalculation += _Stopwatch.Elapsed;
            _Stopwatch.Reset();
            _CalculationAmount++;

            _StartTime += Time.deltaTime;
        }
        
        private void AlternateArray()
        {
            var newArrayList = new List<WaveParameter>();

            for (int i = 0; i < _WaveMultiplier; i++)
            {
                newArrayList.AddRange(_WaveParameters);
            }

            _WaveParameters = newArrayList.ToArray();
            Debug.Log($"Wave count: {_WaveParameters.Length}");
        }

        private void Setup()
        {
            MeshTable.SetupTable(1000);

            _VertexCount = _Mesh.vertexCount;
            _MeshResolution = MeshTable.GetFraction(_VertexCount);
            _ComputeShader.SetInt(_MeshResolutionPropertyID, _MeshResolution);
            _ComputeShader.SetInt(_WaveInformationArrayLengthPropertyID, _WaveParameters.Length);
            _ComputeShader.SetFloat(_ScalingPropertyId, (10 / (float) _MeshResolution) * _Scaling);

            _VerticesBuffer = new ComputeBuffer(_VertexCount, sizeof(float) * 3);
            _UVBuffer = new ComputeBuffer(_VertexCount, sizeof(float) * 2);
            _WaveParameterBuffer = new ComputeBuffer(_WaveParameters.Length, sizeof(float) * 5);

            using var triangleBuffer = new ComputeBuffer(_VertexCount * 3, sizeof(int));
                _MeshFilter.mesh.triangles = GetBufferData(1, triangleBuffer, _TriangleOutputBufferPropertyID,
                    _MeshFilter.mesh.triangles);
        }
        
        private void MeshUpdate(out Vector3[] vertices, out Vector2[] uvs)
        {
            _ComputeShader.SetFloat(_GlobalTimePropertyID, _GlobalTime);
            _ComputeShader.SetFloat(_MaxHeightAmplifierPropertyID, MaxHeightAmplifier);
            _WaveParameterBuffer.SetData(_WaveParameters);

            _ComputeShader.SetBuffer(0, _VerticesOutputBufferPropertyID, _VerticesBuffer);
            _ComputeShader.SetBuffer(0, _UVOutputBufferPropertyID, _UVBuffer);
            _ComputeShader.SetBuffer(0, _WaveParameterBufferPropertyID, _WaveParameterBuffer);
            _ComputeShader.Dispatch(0, 32, 1, 32);
            
            vertices = new Vector3[_VertexCount];
            uvs = new Vector2[_VertexCount];
            
            _VerticesBuffer.GetData(vertices);
            _UVBuffer.GetData(uvs);
            
            _GlobalTime += Time.deltaTime;
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
