using System;
using UnityEngine;

namespace Version._0._7.Base
{
    [Serializable]
    public struct WaveParameter
    {
        [SerializeField] internal Vector2 Direction;
        [SerializeField] internal float Amplitude;
        [SerializeField] internal float WaveLength;
        [SerializeField] internal float SpeedModifer;
    }
}
