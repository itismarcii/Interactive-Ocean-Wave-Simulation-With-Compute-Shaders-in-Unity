using UnityEngine;

public class ComputeManager : MonoBehaviour
{
    public ComputeShader _ComputeScript;

    private ComputeBuffer _InputDataBuffer, _OutputDataBuffer;

    private const float _MULTIPLY_VALUE = 10;
    private Vector3[] _InputData =
    {
        new Vector3(0,0,0), 
        new Vector3(1,0,0), 
        new Vector3(2,0,0),
        new Vector3(3,0,0)
    };

    void Start()
    {
        _InputDataBuffer = new ComputeBuffer(4, sizeof(float) * 3);
        _OutputDataBuffer = new ComputeBuffer(4, sizeof(float) * 3);

        int propertyId = Shader.PropertyToID("multiply_value");
        _ComputeScript.SetFloat(propertyId, _MULTIPLY_VALUE);
        
        _InputDataBuffer.SetData(_InputData);

        int propertyIdInput = Shader.PropertyToID("input_data_buffer");
        int propertyIdOutput = Shader.PropertyToID("output_data_buffer");
        _ComputeScript.SetBuffer(0, propertyIdInput, _InputDataBuffer);
        _ComputeScript.SetBuffer(0, propertyIdOutput, _OutputDataBuffer);
        
        _ComputeScript.Dispatch(0,4,1,1);

        Vector3[] results = new Vector3[4];
        _OutputDataBuffer.GetData(results);

        foreach (var result in results) Debug.Log(result);

        _InputDataBuffer.Dispose();
        _OutputDataBuffer.Dispose();

    }
}
