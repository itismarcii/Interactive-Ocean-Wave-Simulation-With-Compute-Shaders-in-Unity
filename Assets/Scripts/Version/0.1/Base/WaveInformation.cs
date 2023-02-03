using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Version._0._1.Base
{
    [Serializable]
    public struct WaveInformation
    {
        [SerializeField] internal Vector2 Direction;
        [FormerlySerializedAs("Steepness")] [SerializeField] internal float Amplitude;
        [SerializeField] internal float WaveLength;
    }
}
