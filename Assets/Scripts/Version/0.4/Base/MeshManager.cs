using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Extensions;
using UnityEngine;
using UnityEngine.Rendering;
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
        [SerializeField] private float MaxHeightAmplifier;
        [SerializeField] private float _Scaling = 1;
        [SerializeField] private WaveParameter[] _WaveParameters;

        private ComputeBuffer 
            _VerticesBufferInnerCircle, _VerticesBufferMiddleCircle, _VerticesBufferOuterCircle, 
            _UVBufferInnerCircle, _UVBufferMiddleCircle, _UVBufferOuterCircle,
            _WaveParameterBuffer;

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
            _MeshShiftPropertyId = Shader.PropertyToID("meshShift"),
            _MeshCenterShiftPropertyId = Shader.PropertyToID("meshCenter");
        
        private static float _GlobalTime;
        private int _MeshResolution;
        private Vector3 _MeshScale;
        private Vector2 _MeshScale2D;
        private int _VertexCount;
        private Mesh _Mesh;
        
        private void OnDisable()
        {
            _VerticesBufferInnerCircle?.Dispose();
            _VerticesBufferMiddleCircle?.Dispose();
            _VerticesBufferOuterCircle?.Dispose();
            
            _UVBufferInnerCircle?.Dispose();
            _UVBufferMiddleCircle?.Dispose();
            _UVBufferOuterCircle?.Dispose();
            
            _WaveParameterBuffer?.Dispose();
        }

        private void Start()
        {
            MeshTable.SetupTable(1000);
            _GridField.SetScaling(_Scaling);
            _GridField.GenerateGridField();
            Setup();

            _MeshScale = _GridField.MeshScale;
            _MeshScale2D = new Vector2(_MeshScale.x, _MeshScale.z);
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
                UpdateMesh(meshGridInfo);

            _GlobalTime += Time.deltaTime;
        }

        private void UpdateMesh(MeshGrid meshInfo)
        {
            var shift = meshInfo.MeshShift * _MeshScale2D;
            
            switch (meshInfo.CircleStage)
            {
                case MeshGrid.Circle._Inner_:
                    UpdateMesh(meshInfo, shift, _VerticesBufferInnerCircle, _UVBufferInnerCircle);
                    return;
                case MeshGrid.Circle._Middle_:
                    UpdateMesh(meshInfo, shift, _VerticesBufferMiddleCircle, _UVBufferMiddleCircle);
                    return;
                case MeshGrid.Circle._Outer_:
                    UpdateMesh(meshInfo, shift, _VerticesBufferOuterCircle, _UVBufferOuterCircle);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMesh(MeshGrid meshInfo, Vector2 shift , in ComputeBuffer verticesBuffer, in ComputeBuffer uvBuffer)
        {
            MeshUpdate(meshInfo, shift, in verticesBuffer, in uvBuffer);
        }

        private void MeshUpdate(MeshGrid meshInfo, Vector2 shift, in ComputeBuffer verticesBuffer, in ComputeBuffer uvBuffer)
        {
            _ComputeShader.SetVector(_MeshShiftPropertyId, shift);
            _WaveParameterBuffer.SetData(_WaveParameters);
            _ComputeShader.SetInt(_MeshResolutionPropertyID, meshInfo.Resolution);
            _ComputeShader.SetFloat(_ScalingPropertyId, (10 / (float) (meshInfo.Resolution - 1)) * _Scaling);

            _ComputeShader.SetBuffer(0, _VerticesOutputBufferPropertyID, verticesBuffer);
            _ComputeShader.SetBuffer(0, _UVOutputBufferPropertyID, uvBuffer);
            _ComputeShader.SetBuffer(0, _WaveParameterBufferPropertyID, _WaveParameterBuffer);
            _ComputeShader.Dispatch(0, 32, 1, 32);

            AsyncGPUReadback.Request(verticesBuffer, (request) =>
            {
                try
                {
                    meshInfo.GridMesh.vertices = request.GetData<Vector3>().ToArray();
                }
                catch (Exception e) 
                { 
                    // ignored
                }
                
            });
            
            AsyncGPUReadback.Request(uvBuffer, (request) =>
            {
                try
                {
                    meshInfo.GridMesh.uv = request.GetData<Vector2>().ToArray();
                }
                catch (Exception e)
                {
                    // ignored
                }
            });
        }

        private void Setup()
        {            
            _ComputeShader.SetFloat(_MeshCenterShiftPropertyId, (_GridField.MeshScale.x * _Scaling) / 2);
            _ComputeShader.SetInt(_WaveInformationArrayLengthPropertyID, _WaveParameters.Length);
            
            foreach (var meshGrid in _GridField.MeshGridList)
            {
                var mesh = meshGrid.GridMesh;
                _ComputeShader.SetInt(_MeshResolutionPropertyID, meshGrid.Resolution);
                
                using var triangleBuffer = new ComputeBuffer(meshGrid.VertexCount * 3, sizeof(int));
                    mesh.triangles = GetBufferData(1, triangleBuffer, _TriangleOutputBufferPropertyID, mesh.triangles);
            }

            _VerticesBufferInnerCircle = new ComputeBuffer(_GridField.InnerMesh.vertexCount, sizeof(float) * 3);
            _VerticesBufferMiddleCircle = new ComputeBuffer(_GridField.MiddleMesh.vertexCount, sizeof(float) * 3);
            _VerticesBufferOuterCircle = new ComputeBuffer(_GridField.OuterMesh.vertexCount, sizeof(float) * 3);
            
            _UVBufferInnerCircle = new ComputeBuffer(_GridField.InnerMesh.vertexCount, sizeof(float) * 2);
            _UVBufferMiddleCircle = new ComputeBuffer(_GridField.MiddleMesh.vertexCount, sizeof(float) * 2);
            _UVBufferOuterCircle = new ComputeBuffer(_GridField.OuterMesh.vertexCount, sizeof(float) * 2);

            _WaveParameterBuffer = new ComputeBuffer(_WaveParameters.Length, sizeof(float) * 5);
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
