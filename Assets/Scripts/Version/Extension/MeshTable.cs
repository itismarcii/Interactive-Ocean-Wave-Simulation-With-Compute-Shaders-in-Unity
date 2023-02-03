using UnityEngine;

namespace Extensions
{
    public static class MeshTable
    {
        private static int[] _Table = new int[] { };

        public static void SetupTable(int amount)
        {
            _Table = new int[amount];

            if (amount % 4 == 0)
            {
                for (var i = 0; i < amount;)
                {
                    _Table[i++] = i * i;
                    _Table[i++] = i * i;
                    _Table[i++] = i * i;
                    _Table[i++] = i * i;
                }
                return;
            }
        
            if (amount % 2 == 0)
            {
                for (var i = 0; i < amount;)
                {
                    _Table[i++] = i * i;
                    _Table[i++] = i * i;
                }
                return;
            }

            for (var i = 0; i < amount; i++)
            {
                _Table[i] = i * i;
            }
        }

        public static int GetFraction(int verticesCount)
        {
            if (_Table.Length == 0)
            {
                SetupTable(100);
                Debug.LogWarning("Please setup MeshTable before searching for the fraction.");
            }

            if (_Table.Length % 4 == 0)
            {
                for (var i = 0; i < _Table.Length;)
                {
                    if (_Table[i++] == verticesCount) return i;
                    if (_Table[i++] == verticesCount) return i;                
                    if (_Table[i++] == verticesCount) return i;
                    if (_Table[i++] == verticesCount) return i;
                }
            }
        
            if (_Table.Length % 2 == 0)
            {
                for (var i = 0; i < _Table.Length;)
                {
                    if (_Table[i++] == verticesCount) return i;
                    if (_Table[i++] == verticesCount) return i;
                }
            }
        
            for (var i = 0; i < _Table.Length; i++)
            {
                if (_Table[i] == verticesCount) return i;
            }

            Debug.LogError("VerticesCount not inside the table. Make sure its a square number (Example: 4x4)");
            return 0;
        }
    }
}
