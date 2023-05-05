using UnityEngine;
using Version._0._7.Base;

namespace Version._0._7.Floater
{
    public class Floater : MonoBehaviour
    {

        [SerializeField] private Rigidbody _Rigidbody;
        [SerializeField] private float _DepthBeforeSubmerged = 1f;
        [SerializeField] private float _DisplacementAmount = 1f;
        [SerializeField] private float _WaterDrag = .99f;
        [SerializeField] private float _WaterAngularDrag = .5f;
        [SerializeField] private uint _FloatersCount = 1;

        [SerializeField]private int _LastIndex = -1;
        private float _WaveHeight;
        private bool _IsSetup = false;
        
        private void Awake()
        {
            _Rigidbody.useGravity = false;
        }

        private void FixedUpdate()
        {
            if (_IsSetup)
            {
                _WaveHeight = VertexIdentifier.GetClosestVertex(ref _LastIndex, transform.position).y;
            }
            else
            {
                _WaveHeight = VertexIdentifier.GetClosestVertex(transform.position, out var index).y;
                _LastIndex = index;
                _IsSetup = true;
            }

            _Rigidbody.AddForceAtPosition(Physics.gravity / _FloatersCount, transform.position, ForceMode.Acceleration);
            
            if (!(transform.position.y < _WaveHeight)) return;
            
            var displacementMultiplier =
                Mathf.Clamp01((_WaveHeight - transform.position.y) / _DepthBeforeSubmerged) * _DisplacementAmount;
            
            _Rigidbody.AddForceAtPosition(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * displacementMultiplier, 0f), transform.position, ForceMode.Acceleration);
            _Rigidbody.AddForce(-_Rigidbody.velocity * (displacementMultiplier * _WaterDrag * Time.fixedDeltaTime), ForceMode.VelocityChange);
            _Rigidbody.AddTorque(-_Rigidbody.angularVelocity * (displacementMultiplier * _WaterAngularDrag * Time.fixedDeltaTime), ForceMode.VelocityChange);
        }
    }
}
