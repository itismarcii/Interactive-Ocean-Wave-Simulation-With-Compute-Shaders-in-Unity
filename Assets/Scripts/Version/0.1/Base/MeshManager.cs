using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Version._0._1.Base;

public class MeshManager : MonoBehaviour
{
    [SerializeField] private MeshFilter _MeshFilter;
    [SerializeField] private WaveInformation _Wave;

    private void Start()
    {
        GerstnerWave.SetBaseGrid(_MeshFilter.mesh.vertices);
    }

    // Update is called once per frame
    void Update()
    {
        var meshFilterMesh = _MeshFilter.mesh;
        GerstnerWave.UpdateTime(Time.deltaTime);
        _MeshFilter.mesh = GerstnerWave.UpdateWave(meshFilterMesh, _Wave);
    }
}
