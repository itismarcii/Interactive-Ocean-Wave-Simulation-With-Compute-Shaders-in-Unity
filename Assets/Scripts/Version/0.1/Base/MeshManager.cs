using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Version._0._1.Base
{
    public class MeshManager : MonoBehaviour
    {
        [SerializeField] private MeshFilter _MeshFilter;
        [SerializeField] private float MaxHeightAmplifier = 0;
        [SerializeField] private WaveInformation[] _Wave;
        private int _MeshVertexCount = 0;
        [Space, SerializeField] private int _AmountOfWaveCalculations = 1;

        private readonly Stopwatch _Stopwatch = new Stopwatch();
        private float _StartTime = 0;
        private const float _END_TIME = 60;
        private TimeSpan _DurationCalculations;
        private int _CalculationAmount = 0;

        private void Start()
        {
            GerstnerWave.SetBaseGrid(_MeshFilter.mesh.vertices);
            _MeshVertexCount = _MeshFilter.mesh.vertexCount;
        }

        // Update is called once per frame
        void Update()
        {
            if (_StartTime >= _END_TIME)
            {
                var average = _DurationCalculations / _CalculationAmount;
                Debug.Log($"Total: {_DurationCalculations}  Avg: {average}  AvgInMilli: {average.Milliseconds}");
                return;
            }
        

            GerstnerWave.UpdateTime(Time.deltaTime);
            GerstnerWave.MaxHeightAmplifier = MaxHeightAmplifier;

            _Stopwatch.Start();
            TestEnvironment();
            _Stopwatch.Stop();
            _DurationCalculations += _Stopwatch.Elapsed;
            _CalculationAmount++;
        
            _Stopwatch.Reset();

            _StartTime += Time.deltaTime;
        }

        private void TestEnvironment()
        {
            var meshVertices = new Vector3[_MeshVertexCount];

            for (var i = 0; i < _AmountOfWaveCalculations; i++)
            {
                foreach (var waveInformation in _Wave)
                {
                    GerstnerWave.GerstnerWaveDisplacement(ref meshVertices, waveInformation);
                }
            }

            _MeshFilter.mesh = GerstnerWave.ApplyDisplacement(ref meshVertices, _MeshFilter.mesh);
        }
    }
}
