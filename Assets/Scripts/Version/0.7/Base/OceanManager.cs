using System;
using UnityEngine;
using Version._0._7.Grid_Field;

namespace Version._0._7.Base
{
    public class OceanManager : MonoBehaviour
    {
        [Serializable]
        public struct NoiseParameter
        {
            public float Scale;
            public float Intensity;
            public float Mean;
            public float StdDev;
        }
        
        [SerializeField] private GridField _GridField;
        [SerializeField] private MeshDisplacer _MeshDisplacer;

        [SerializeField] private bool UsShaderRendering = true;
        
        private Vector2Int _GridResolution;
        public static bool IsSetup { get; private set; } = false;
        
        [Space(15), SerializeField] private NoiseParameter _NoiseParameter = new NoiseParameter()
        {
            Scale = 10f,
            Intensity = 1f,
            Mean = .5f,
            StdDev = .1f
        };
        
        void Start()
        {
            _GridField.GenerateGrid(_MeshDisplacer.GetScaling());
            _GridResolution = _GridField.GetGridFieldResolution();
            
            _MeshDisplacer.VertexCount = GridField.MeshVertexCount;
            _MeshDisplacer.Setup(GridField.MeshResolution);
            _MeshDisplacer.SetScaling((10 / (float) (GridField.MeshResolution - 1)) * _MeshDisplacer.GetScaling());

            // Noise generation
            var noise = GaussianNoiseGenerator.Generate(
                (int)(GridField.MeshResolution * _GridField.GetGridFieldResolution().x),
                (int)(GridField.MeshResolution * _GridField.GetGridFieldResolution().y),
                _NoiseParameter.Scale,
                _NoiseParameter.Intensity,
                _NoiseParameter.Mean,
                _NoiseParameter.StdDev);
            
            _MeshDisplacer.SetGuassianNoise(noise);
            
            for (var x = 0; x < _GridResolution.x; x++)
            {
                for (var z = 0; z < _GridResolution.y; z++)
                {
                    var meshInfo = OceanGridObject.HashTable[x][z];
                    _MeshDisplacer.TriangleSetup(ref meshInfo);
                    _MeshDisplacer.MeshUpdate(ref meshInfo);
                }
            }

            IsSetup = true;
        }

        void Update()
        {
            _MeshDisplacer.SetGlobalTime();
            
            for (var x = 0; x < _GridResolution.x; x++)
            {
                for (var z = 0; z < _GridResolution.y; z++)
                {
                    var meshInfo = OceanGridObject.HashTable[x][z];
                    if (UsShaderRendering) _MeshDisplacer.MeshUpdate(meshInfo);
                    else _MeshDisplacer.MeshUpdate(ref meshInfo);
                }
            }
            
            _MeshDisplacer.IncreaseTime();
        }
    }
}
