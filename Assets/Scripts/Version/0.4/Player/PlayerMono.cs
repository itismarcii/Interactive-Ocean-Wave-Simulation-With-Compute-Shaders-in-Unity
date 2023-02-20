using UnityEngine;

namespace Version._0._4.Player
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
