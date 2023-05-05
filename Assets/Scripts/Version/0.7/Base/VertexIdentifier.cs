using System;
using UnityEngine;
using Version._0._7.Grid_Field;
using Vector3 = UnityEngine.Vector3;

namespace Version._0._7.Base
{
    
    
    public static class VertexIdentifier
    {
        public static Vector3 GetClosestVertex(Vector3 position, out int index)
        {
            var x = position.x / (GridField.MeshScale.x * GridField.Scaling);
            var z = position.z / (GridField.MeshScale.z * GridField.Scaling);
            
            var xGridPos = (int) Math.Round(x);
            var zGridPos = (int) Math.Round(z);
            
            var information = GetMeshInformation(xGridPos, zGridPos);

            var xIndex = (x - xGridPos) % GridField.MeshScale.x;
            var zIndex = (z - zGridPos) % GridField.MeshScale.z;
            
            index = Mathf.Abs((int)(GridField.MeshResolution * xIndex + (GridField.MeshResolution * zIndex) * GridField.MeshResolution));
            
            return information == null ? Vector3.zero : SelectVertex((MeshInformation) information, position, xGridPos, zGridPos, ref index);
        }


        public static Vector3 GetClosestVertex(ref int index, Vector3 position)
        {
            var xPos = (int) position.x / 10;
            var zPos = (int) position.z / 10;
            
            var information = GetMeshInformation(xPos, zPos);

            return information == null ? Vector3.zero : SelectVertex((MeshInformation)information, position, xPos, zPos, ref index);
        }

        private static Vector3 SelectVertex(MeshInformation meshInfo, Vector3 position, int xPos, int zPos, ref int index)
        {
            var minDistance = float.MaxValue;

            var runs = 0;

            while (true)
            {
                var indexMemory = index;
                runs++;
                // 0 = current | 1 right | 2 left | 3 bottom | 4 up
                var vertices = new Vector3[5];
                vertices[0] = meshInfo.VerticesData[index];

                var meshInfos = new MeshInformation?[5] {meshInfo, null, null, null, null};

                var indices = new int[5] {index, index + 1, index - 1, index - GridField.MeshResolution, index + GridField.MeshResolution};

                // Check edge case
                // Right Edge
                
                if (index % (GridField.MeshResolution - 2) == 0 && index > 0)
                {
                    if (xPos < GridField._GridFieldResolution.x)
                    {
                        meshInfos[1] = GetMeshInformation(xPos + 1, zPos);
                        if (meshInfos[1] != null)
                        {
                            var meshInfoRight = (MeshInformation) meshInfos[1];
                            vertices[1] = meshInfoRight.VerticesData[index - GridField.MeshResolution + 2] + meshInfoRight.GridShift;
                            indices[1] = index - GridField.MeshResolution + 2;
                        }
                    }
                    else
                    {
                        vertices[1] = meshInfo.VerticesData[indices[0]] + meshInfo.GridShift;
                        indices[1] = indices[0];
                    }
                }
                else vertices[1] = meshInfo.VerticesData[indices[1]] + meshInfo.GridShift;


                // Left Edge
                if (index % GridField.MeshResolution == 0)
                {
                    if (xPos != 0)
                    {
                        meshInfos[2] = GetMeshInformation(xPos - 1, zPos);
                        if (meshInfos[2] != null)
                        {
                            var meshInfoLeft = (MeshInformation) meshInfos[2];
                            vertices[2] = meshInfoLeft.VerticesData[index + GridField.MeshResolution - 2] + meshInfoLeft.GridShift;
                            indices[2] = index + GridField.MeshResolution - 2;
                        }
                    }
                    else
                    {
                        vertices[2] = meshInfo.VerticesData[indices[0]] + meshInfo.GridShift;
                        indices[2] = indices[0];
                    }
                }
                else vertices[2] = meshInfo.VerticesData[indices[2]] + meshInfo.GridShift;
                
                // Bottom Edge
                if (index < GridField.MeshResolution)
                {
                    if (zPos != 0)
                    {
                        meshInfos[3] = GetMeshInformation(xPos, zPos - 1);
                        if (meshInfos[3] != null)
                        {
                            var meshInfoBottom = (MeshInformation) meshInfos[3];
                            vertices[3] = meshInfoBottom.VerticesData[GridField.MeshVertexCount - index % GridField.MeshResolution - GridField.MeshResolution] + meshInfoBottom.GridShift;
                            indices[3] = GridField.MeshVertexCount - index % GridField.MeshResolution - GridField.MeshResolution;
                        }
                    }
                    else
                    {
                        vertices[3] = meshInfo.VerticesData[indices[0]] + meshInfo.GridShift;
                        indices[3] = indices[0];
                    }
                }
                else vertices[3] = meshInfo.VerticesData[indices[3]] + meshInfo.GridShift;
                
                // Up Edge
                if (index >= GridField.MeshVertexCount - 1 - GridField.MeshResolution * 2)
                {
                    if (zPos < GridField._GridFieldResolution.y)
                    {
                        meshInfos[4] = GetMeshInformation(xPos + 1, zPos);
                        if (meshInfos[4] != null)
                        {
                            var meshInfoUp = (MeshInformation) meshInfos[4];
                            vertices[4] = meshInfoUp.VerticesData[index % GridField.MeshResolution + GridField.MeshResolution] + meshInfoUp.GridShift;
                            indices[4] = index % GridField.MeshResolution + GridField.MeshResolution;
                        }
                    }
                    else
                    {
                        vertices[4] = meshInfo.VerticesData[indices[0]] + meshInfo.GridShift;
                        indices[4] = indices[0];
                    }
                }
                else vertices[4] = meshInfo.VerticesData[indices[4]] + meshInfo.GridShift;

                var closestVector = vertices[0];
                var closestMesh = meshInfo;

                for (var i = 0; i < 5; i++)
                {
                    if(indices[i] < 0 || indices[i] > GridField.MeshVertexCount - 1) continue;
                    var distance = Vector2.Distance(
                        new Vector2(position.x,position.z), 
                        new Vector2(vertices[i].x, vertices[i].z));

                    // Debug.Log($"index: {indices[i]}  distance: {distance}");
                    
                    if (!(distance < minDistance)) continue;

                    minDistance = distance;
                    closestVector = vertices[i];
                    index = indices[i];
                    if (meshInfos[i] != null) closestMesh = (MeshInformation) meshInfos[i];
                }
                
                if (index == indexMemory) return closestVector;
                meshInfo = closestMesh;
            }
        }

        private static MeshInformation? GetMeshInformation(int xPos, int zPos)
        {
            var hashTable = OceanGridObject.HashTable;

            if (!hashTable.TryGetValue(xPos, out var zTable))
            {
                if (!hashTable.TryGetValue(xPos - 1, out var zTableAlternative)) return null;
                if (!zTableAlternative.TryGetValue(zPos, out var meshXAlternative)) return null;
                
                if (!(Math.Abs(meshXAlternative.LastUpdateTime - Time.time) > Time.deltaTime)) return meshXAlternative;
            
                MeshDisplacer.GetMeshDataVertices(ref meshXAlternative);
                meshXAlternative.LastUpdateTime = Time.time;
                hashTable[xPos - 1][zPos] = meshXAlternative;

                return meshXAlternative;
            }
            
            if (!zTable.TryGetValue(zPos, out var meshInformation))
            {
                if (!zTable.TryGetValue(zPos - 1, out var meshZAlternative)) return null;
                if (!(Math.Abs(meshZAlternative.LastUpdateTime - Time.time) > Time.deltaTime)) return meshZAlternative;
            
                MeshDisplacer.GetMeshDataVertices(ref meshZAlternative);
                meshZAlternative.LastUpdateTime = Time.time;
                hashTable[xPos][zPos - 1] = meshZAlternative;

                return meshZAlternative;
            }

            if (!(Math.Abs(meshInformation.LastUpdateTime - Time.time) > Time.deltaTime)) return meshInformation;
            
            MeshDisplacer.GetMeshDataVertices(ref meshInformation);
            meshInformation.LastUpdateTime = Time.time;
            hashTable[xPos][zPos] = meshInformation;

            return meshInformation;
        }
    }
}

