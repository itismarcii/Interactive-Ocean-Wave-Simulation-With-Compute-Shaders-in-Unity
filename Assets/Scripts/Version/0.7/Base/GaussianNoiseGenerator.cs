using UnityEngine;

namespace Version._0._7.Base
{
    public static class GaussianNoiseGenerator
    {
        public static Texture2D Generate(int resolutionX, int resolutionY, float noiseScale, float noiseIntensity, float noiseMean, float noiseStdDev)
        {
            var noiseTexture = new Texture2D(resolutionX, resolutionY);
            
            for (var x = 0; x < resolutionX; x++)
            {
                for (var y = 0; y < resolutionY; y++)
                {
                    var u = (float) x / resolutionX;
                    var v = (float) y / resolutionY;

                    var noiseValue = Mathf.PerlinNoise(u * noiseScale, v * noiseScale);
                    noiseValue = noiseMean + noiseIntensity * (noiseValue * 2f - 1f) + RandomGaussian() * noiseStdDev;
                
                    noiseTexture.SetPixel(x, y, new Color(noiseValue, noiseValue, noiseValue, 1));
                }
            }

            noiseTexture.Apply();

            return noiseTexture;
        }

        private static float RandomGaussian()
        {
            float u, s;
            do
            {
                u = 2f * Random.value - 1f;
                var v = 2f * Random.value - 1f;
                s = u * u + v * v;
            } while (s is >= 1f or 0f);

            var fac = Mathf.Sqrt(-2f * Mathf.Log(s) / s);
            return u * fac;
        }
    }
}
