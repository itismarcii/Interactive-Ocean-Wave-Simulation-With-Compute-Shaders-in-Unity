using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Version._0._1.Base;

public class MeshManager : MonoBehaviour
{
    [SerializeField] private MeshFilter _MeshFilter;
    [SerializeField] private float MaxHeightAmplifier = 0;
    [SerializeField] private WaveInformation _Wave;

    private void Start()
    {
        GerstnerWave.SetBaseGrid(_MeshFilter.mesh.vertices);
    }

    // Update is called once per frame
    void Update()
    {
        GerstnerWave.UpdateTime(Time.deltaTime);
        GerstnerWave.SetMaxHeightAmplifier(MaxHeightAmplifier);
        _MeshFilter.mesh = GerstnerWave.UpdateWave(_MeshFilter.mesh, _Wave);
    }
}
