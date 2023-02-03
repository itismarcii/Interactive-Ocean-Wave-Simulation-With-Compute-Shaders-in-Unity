using System;
using UnityEngine;

namespace Version._0._1.Base
{
    public static class GerstnerWave
    {
        private static float _TIME_ = 1;
        private const float PI = 3.14159265358979323846f;
        private static Vector3[] BaseGrid;
        private static float MaxHeightAmplifier = 0;

        public static void UpdateTime(float deltaTime, float multiplier = 1) => _TIME_ += deltaTime * multiplier;
        public static void SetBaseGrid(Vector3[] grid) => BaseGrid = grid;
        public static void SetMaxHeightAmplifier(float value) => MaxHeightAmplifier = value;
        
        public static Mesh UpdateWave(Mesh mesh, WaveInformation wave)
        {
            var meshVertices = mesh.vertices;
            var meshUvs = mesh.uv;

            var maxHeight = (wave.WaveLength / 7) + MaxHeightAmplifier;
            
            // Wave Number
            var k = 2 * PI / wave.WaveLength;
                
            // Amplitude
            var A = wave.Amplitude * 2 > maxHeight ? maxHeight / 2: wave.Amplitude;
                
            // Wave Speed
            var c = Mathf.Sqrt(PI / k);
            
            // Normalized Wave Direction
            var normalizedDirection = wave.Direction.normalized;
            
            for (var index = 0; index < mesh.vertexCount; index++)
            {
                var position = BaseGrid[index];
                
                // Position to Vector2
                var pos = new Vector2(position.x, position.z);
                
                // Wave Phase
                var f = k * Vector2.Dot(normalizedDirection, pos) - c * _TIME_;

                var displacement = new Vector3(
                    A * normalizedDirection.x * Mathf.Cos(f),
                    A * Mathf.Sin(f),
                    A * normalizedDirection.y * Mathf.Cos(f));
                
                meshVertices[index] = position + displacement;
                meshUvs[index] = new Vector2(position.x, position.z);
            }

            mesh.vertices = meshVertices;
            mesh.uv = meshUvs;
            return mesh;
        }
    }
}
