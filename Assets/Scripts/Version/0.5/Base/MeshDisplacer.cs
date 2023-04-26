using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;
using Version._0._5.Grid_Field;
using Debug = UnityEngine.Debug;

namespace Version._0._5.Base
{
    public class MeshDisplacer : MonoBehaviour
    {
        [SerializeField] private int _WaveMultiplier = 1;
        [Space]
        
        private const int _KERNEL_ID_X = 32, _KERNEL_ID_Y = 1, _KERNEL_ID_Z = 32;

        [SerializeField] private ComputeShader _ComputeShader;
        [Space]
        [SerializeField] private float MaxHeightAmplifier = 0;
        [SerializeField] private float _Scaling = 1;
        [SerializeField] private WaveParameter[] _WaveParameters;

        private ComputeBuffer _WaveParameterBuffer;
        [HideInInspector] public int VertexCount;

        private readonly int
            _VerticesOutputBufferPropertyID = Shader.PropertyToID("VerticesOutput"),
            _VerticesShaderOutputBufferPropertyID = Shader.PropertyToID("VerticesShaderOutput"),
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

        private float _GlobalTime = 0;

        private void OnDisable()
        {
            _WaveParameterBuffer?.Dispose();
        }

        public float GetScaling() => _Scaling;

        public void Setup(int meshResolution)
        {
            _ComputeShader.SetInt(_MeshResolutionPropertyID, meshResolution);
            _ComputeShader.SetInt(_WaveInformationArrayLengthPropertyID, _WaveParameters.Length);
            _ComputeShader.SetFloat(_ScalingPropertyId, (10 / (float) meshResolution) * _Scaling);
            
            _WaveParameterBuffer = new ComputeBuffer(_WaveParameters.Length, sizeof(float) * 5);
        }
        
        public void TriangleSetup(ref MeshInformation meshInformation)
        {
            using var triangleBuffer = new ComputeBuffer(VertexCount * 3, sizeof(int));
                meshInformation.Mesh.triangles = GetBufferData(1, triangleBuffer, _TriangleOutputBufferPropertyID, meshInformation.Mesh.triangles);
        }
        
        public void MeshUpdate(ref MeshInformation meshInformation)
        {
            _ComputeShader.SetVector(_MeshShiftPropertyId, meshInformation.Shift);
            _WaveParameterBuffer.SetData(_WaveParameters);
            _ComputeShader.SetFloat(_MaxHeightAmplifierPropertyID, MaxHeightAmplifier);

            _ComputeShader.SetBuffer(0, _VerticesOutputBufferPropertyID, meshInformation.VerticesBuffer);
            _ComputeShader.SetBuffer(0, _WaveParameterBufferPropertyID, _WaveParameterBuffer);
            _ComputeShader.Dispatch(0, 32, 1, 32);
            
            var vertices = new Vector3[VertexCount];
            var uvs = new Vector2[VertexCount];
            
            meshInformation.VerticesBuffer.GetData(vertices);
            
            meshInformation.Mesh.vertices = vertices;
            meshInformation.Mesh.uv = uvs;
        }
        
        public void MeshUpdate(MeshInformation meshInformation)
        {
            _ComputeShader.SetVector(_MeshShiftPropertyId, meshInformation.Shift);
            _WaveParameterBuffer.SetData(_WaveParameters);
            _ComputeShader.SetFloat(_MaxHeightAmplifierPropertyID, MaxHeightAmplifier);

            _ComputeShader.SetBuffer(0, _VerticesOutputBufferPropertyID, meshInformation.VerticesBuffer);
            _ComputeShader.SetBuffer(0, _WaveParameterBufferPropertyID, _WaveParameterBuffer);
            _ComputeShader.Dispatch(0, 32, 1, 32);
        }

        public void SetScaling(float scaling) => _ComputeShader.SetFloat(_ScalingPropertyId, scaling);

        public void SetCenter(float center) => _ComputeShader.SetFloat(_MeshCenterShiftPropertyId, center);
        public void IncreaseTime() => _GlobalTime += Time.deltaTime;
        public void SetGlobalTime() => _ComputeShader.SetFloat(_GlobalTimePropertyID, _GlobalTime);

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
