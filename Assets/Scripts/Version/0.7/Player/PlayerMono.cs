using System;
using UnityEngine;
using Version._0._7.Base;

namespace Version._0._7.Player
{
    public class PlayerMono : MonoBehaviour
    {
        public static PlayerMono Player { get; private set; }
        [SerializeField] private int _MeshIndex;
        private bool _Setup = false;
        
        void Start()
        {
            if (!Player) Player = this;
            
        }

        private void Update()
        {
            if (!_Setup)
            {
                VertexIdentifier.GetClosestVertex(transform.position, out var meshIndex);
                _MeshIndex = meshIndex;
                _Setup = true;
                return;
            }
            
            VertexIdentifier.GetClosestVertex(ref _MeshIndex, transform.position);
        }
    }
}
