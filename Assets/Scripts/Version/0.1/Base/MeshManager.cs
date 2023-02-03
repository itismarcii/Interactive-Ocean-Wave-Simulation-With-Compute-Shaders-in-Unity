using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Version._0._1.Base;

public class MeshManager : MonoBehaviour
{
    [SerializeField] private MeshFilter _MeshFilter;
    [SerializeField] private float MaxHeightAmplifier = 0;
    [SerializeField] private WaveInformation[] _Wave;
    private int _MeshVertexCount = 0;

    private void Start()
    {
        GerstnerWave.SetBaseGrid(_MeshFilter.mesh.vertices);
        _MeshVertexCount = _MeshFilter.mesh.vertexCount;
    }

    // Update is called once per frame
    void Update()
    {
        GerstnerWave.UpdateTime(Time.deltaTime);
        GerstnerWave.MaxHeightAmplifier = MaxHeightAmplifier;
        
        var meshVertices = new Vector3[_MeshVertexCount];
        
        foreach (var waveInformation in _Wave)
        {
            GerstnerWave.GerstnerWaveDisplacement(ref meshVertices, waveInformation);
        }
        
        _MeshFilter.mesh = GerstnerWave.ApplyDisplacement(ref meshVertices, _MeshFilter.mesh);;

    }
}
