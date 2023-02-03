using System;
using UnityEngine;

namespace Version._0._1.Base
{
    public static class GerstnerWave
    {
        private static float _TIME_ = 1;
        private const float PI = 3.14159265358979323846f;
        private static Vector3[] _BaseGrid = Array.Empty<Vector3>();
        internal static float MaxHeightAmplifier = 0;

        public static void UpdateTime(float deltaTime, float multiplier = 1) => _TIME_ += deltaTime * multiplier;
        public static void SetBaseGrid(Vector3[] grid) => _BaseGrid = grid;
        
        public static void GerstnerWaveDisplacement(ref Vector3[] displacements, WaveInformation wave)
        {
            var maxHeight = (wave.WaveLength / 7) + MaxHeightAmplifier;
            
            // Wave Number
            var k = 2 * PI / wave.WaveLength;
                
            // Amplitude
            var A = wave.Amplitude * 2 > maxHeight ? maxHeight / 2: wave.Amplitude;
                
            // Wave Speed
            var c = Mathf.Sqrt(PI / k);
            
            // Normalized Wave Direction
            var normalizedDirection = wave.Direction.normalized;
            
            for (var index = 0; index < _BaseGrid.Length; index++)
            {
                var position = _BaseGrid[index];
                
                // Position to Vector2
                var pos = new Vector2(position.x, position.z);
                
                // Wave Phase
                var f = k * Vector2.Dot(normalizedDirection, pos) - c * _TIME_;

                displacements[index] += new Vector3(
                    A * normalizedDirection.x * Mathf.Cos(f),
                    A * Mathf.Sin(f),
                    A * normalizedDirection.y * Mathf.Cos(f));
            }
        }

        public static Mesh ApplyDisplacement(ref Vector3[] displacement, Mesh mesh)
        {
            var meshVertices = mesh.vertices;
            var meshUvs = mesh.uv;
            
            for (var index = 0; index < _BaseGrid.Length; index++)
            {
                meshVertices[index] = _BaseGrid[index] + displacement[index];
                meshUvs[index] = new Vector2(meshVertices[index].x, meshVertices[index].z);
            }

            mesh.vertices = meshVertices;
            mesh.uv = meshUvs;

            return mesh;
        }
    }
}
