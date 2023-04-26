using System;
using UnityEngine;
using UnityEngine.UI;
using Version._0._5.Grid_Field;

namespace Version._0._5.Base
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

        [Space(15), SerializeField] private NoiseParameter _NoiseParameter = new NoiseParameter()
        {
            Scale = 10f,
            Intensity = 1f,
            Mean = .5f,
            StdDev = .1f
        };
        
        private Vector2Int _GridResolution;

        public Image _Image;
        
        void Start()
        {
            _GridField.GenerateGrid(_MeshDisplacer.GetScaling());
            _GridResolution = _GridField.GetGridFieldResolution();
            
            _MeshDisplacer.VertexCount = GridField.MeshVertexCount;
            _MeshDisplacer.Setup(GridField.MeshResolution);
            _MeshDisplacer.SetCenter(GridField.MeshScale.x * _MeshDisplacer.GetScaling() / 2);
            _MeshDisplacer.SetScaling((10 / (float) (GridField.MeshResolution - 1)) * _MeshDisplacer.GetScaling());

            var noise = GaussianNoiseGenerator.Generate(
                (int)(GridField.MeshResolution * _GridField.GetGridFieldResolution().x),
                (int)(GridField.MeshResolution * _GridField.GetGridFieldResolution().y),
                _NoiseParameter.Scale,
                _NoiseParameter.Intensity,
                _NoiseParameter.Mean,
                _NoiseParameter.StdDev);

            _Image.sprite = Sprite.Create(noise, new Rect(0,0,noise.width, noise.height), Vector2.zero);
            
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
