using System;
using UnityEngine;

namespace Version._0._2.Base
{
    [Serializable]
    public struct WaveInformation
    {
        [SerializeField] internal Vector2 Direction;
        [SerializeField] internal float Amplitude;
        [SerializeField] internal float WaveLength;

        public Vector4 ToVector4() => new Vector4(Direction.x, Direction.y, Amplitude, WaveLength);
    }
}
