using UnityEngine;

namespace Version._0._5.Player
{
    public class PlayerMono : MonoBehaviour
    {
        public static PlayerMono Player { get; private set; }
    
        void Awake()
        {
            if (!Player) Player = this;
        }
    }
}
