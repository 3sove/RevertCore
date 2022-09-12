using Revert.Core.Common.Types;
using Revert.Core.Mathematics;
using Revert.Core.Mathematics.Factories;
using Revert.Core.Mathematics.Extensions;
using Revert.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Revert.Core.Mathematics.Vectors;

namespace Revert.Core.Graphics.Clusters
{
    public class MapClusters
    {
        public static T[] getNeighbors<T>(T[][] map, float x, float y)
        {
            return getNeighbors(map, (int)x, (int)y);
        }

        public static T[] getNeighbors<T>(T[][] map, int x, int y)
        {
            return getNeighbors(map, x, y, false, false);
        }

        public static T[] getNeighbors<T>(T[][] map, float x, float y, bool edgeAwareX, bool edgeAwareY)
        {
            return getNeighbors(map, (int)x, (int)y, edgeAwareX, edgeAwareY);
        }

        public static T[] getNeighbors<T>(T[][] map, int x, int y, bool edgeAwareX, bool edgeAwareY)
        {
            var neighbors = new T[8];

            for (int i = 0; i < neighbors.Length; i++)
            {
                neighbors[i] = getNeighbor(map, x, y, i, edgeAwareX, edgeAwareY);
            }
            return neighbors;
        }

        public static T[] getNeighbors<T>(T[][] map, int x, int y, bool edgeAwareX, bool edgeAwareY, T[] results)
        {
            for (int i = 0; i < 8; i++)
            {
                results[i] = getNeighbor(map, x, y, i, edgeAwareX, edgeAwareY);
            }
            return results;
        }

        public static T getNeighbor<T>(T[][] map, int x, int y, int direction)
        {
            return getNeighbor(map, x, y, direction, false, false);
        }

        public static T getNeighbor<T>(T[][] map, int x, int y, int direction, bool edgeAwareX, bool edgeAwareY)
        {
            var targetX = x;
            var targetY = y;
            if (direction == NeighborDirections.TOP_RIGHT || direction == NeighborDirections.RIGHT || direction == NeighborDirections.BOTTOM_RIGHT) targetX++;
            if (direction == NeighborDirections.TOP_LEFT || direction == NeighborDirections.LEFT || direction == NeighborDirections.BOTTOM_LEFT) targetX--;

            if (edgeAwareX)
            {
                targetX = (map[0].Length + targetX) % map[0].Length;
            }
            else
            {
                if (targetX < 0 || targetX >= map[0].Length)
                {
                    return default(T);
                }
            }

            //Y Coordinate
            if (direction == NeighborDirections.TOP_RIGHT || direction == NeighborDirections.TOP || direction == NeighborDirections.TOP_LEFT) targetY++;
            else if (direction == NeighborDirections.BOTTOM_RIGHT || direction == NeighborDirections.BOTTOM || direction == NeighborDirections.BOTTOM_LEFT) targetY--;

            if (edgeAwareY)
            {
                targetY = (map.Length + targetY) % map.Length;
            }
            else
            {
                if (targetY < 0 || targetY >= map.Length)
                {
                    return default(T);
                }
            }

            //if (neighbor != item.getValue()) neighbor = null; //if the neighbor is a part of a different cluster, simply fill that neighbor's spot with null
            return map[targetY][targetX];
        }

        public static float[] getNeighbors(float[][] map, MapItem item)
        {
            var items = new float[8];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = getNeighbor(map, item, i);
            }
            return items;
        }

        public static int[] getNeighbors(int[][] map, MapItem item)
        {
            var items = new int[8];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = getNeighbor(map, item, i);
            }
            return items;
        }

        public static Vector2 getBottomLeft(float[][] map)
        {
            for (int yIndex = 0; yIndex < map.Length; yIndex++)
            {
                float[] y = map[yIndex];
                for (int xIndex = 0; xIndex < y.Length; xIndex++)
                {
                    float x = y[xIndex];
                    if (x > 0f) return Vec2Factory.Instance.get((float)xIndex, (float)yIndex);
                }
            }
            return Vec2Factory.getEmpty();
        }

        public static int getNeighborDirection(float[][] map, int x, int y, bool flowDownward)
        {
            var bestValue = -1f;
            var bestDirections = new List<int>();

            for (int direction = 0; direction < NeighborDirections.DIRECTION_COUNT; direction++)
            {
                var nValue = getNeighbor(map, x, y, direction);
                if (nValue == -1f) continue;

                if (bestValue == -1f)
                {
                    bestValue = nValue;
                    bestDirections.Add(direction);
                    continue;
                }

                if (nValue == bestValue)
                {
                    bestDirections.Add(direction);
                    continue;
                }

                if (flowDownward && nValue < bestValue || !flowDownward && nValue > bestValue)
                {
                    bestValue = nValue;
                    bestDirections.Clear();
                    bestDirections.Add(direction);
                }
            }

            if (bestDirections.Count == 0) return -1;
            return bestDirections[Maths.randomInt(0, bestDirections.Count)];
        }

        public static float getNeighbor(float[][] map, int x, int y, int direction)
        {
            var targetX = NeighborDirections.GetNeighborX(x, direction);
            var targetY = NeighborDirections.GetNeighborY(y, direction);

            if (targetX < 0 || targetX >= map[0].Length) return -1f;
            if (targetY < 0 || targetY >= map.Length) return -1f;
            return map[targetY][targetX];
        }

        public static float getNeighbor(float[][] map, MapItem item, int direction)
        {
            return getNeighbor(map, item.xIndex, item.yIndex, direction);
        }

        public static int getNeighbor(int[][] map, MapItem item, int direction)
        {
            var targetX = NeighborDirections.GetNeighborX(item.xIndex, direction);
            var targetY = NeighborDirections.GetNeighborY(item.yIndex, direction);

            if (targetX < 0 || targetX >= map[0].Length) return -1;
            if (targetY < 0 || targetY >= map.Length) return -1;
            return map[targetY][targetX];
        }

        public static List<MapCluster> getClusters(float[][] tileMap, int key, float tileSize)
        {
            //MapItem[][] map = GetMap(tileMap);
            var clusters = new List<MapCluster>();

            for (int y = 0; y < tileMap.Length; y++)
            {
                var row = tileMap[y];
                for (int x = 0; x < row.Length; x++)
                {
                    var item = row[x];
                    if (item == 0f || item != (float)key) continue;

                    var clusterItem = new MapItem(x, y, (float)(int)item, tileSize);

                    var clusterFound = false;
                    foreach (var c in clusters)
                    {
                        if (c.contains(clusterItem))
                        {
                            clusterFound = true;
                            break;
                        }
                        else
                        {
                            clusterFound = false;
                        }
                    }
                    if (clusterFound) continue;

                    var cluster = getCluster(clusterItem, tileMap, clusters);
                    if (cluster == null) continue;
                    clusters.Add(cluster);
                }
            }

            return clusters;
        }

        public static MapCluster getLargestCluster(float[][] tileMap, float tileSize = 1f)
        {
            return getClusters(tileMap, tileSize).OrderBy(c => c.size()).Last();
        }

        public static List<MapCluster> getClusters(float[][] tileMap, float tileSize = 1f)
        {
            var clusters = new List<MapCluster>();

            for (int y = 0; y < tileMap.Length; y++)
            {
                var row = tileMap[y];
                for (int x = 0; x < row.Length; x++)
                {
                    var item = row[x];
                    if (item == 0f) continue;

                    var clusterItem = new MapItem(x, y, item, tileSize);

                    var clusterFound = false;
                    foreach (var c in clusters)
                    {
                        if (c.contains(clusterItem))
                        {
                            clusterFound = true;
                            break;
                        }
                        else
                        {
                            clusterFound = false;
                        }
                    }
                    if (clusterFound) continue;

                    var cluster = getCluster(clusterItem, tileMap, clusters);
                    if (cluster == null) continue;
                    clusters.Add(cluster);
                }
            }

            return clusters;
        }

        private static MapCluster getCluster(MapItem item, float[][] map, List<MapCluster> clusters)
        {
            var boundaryPoints = new List<MapItem>();
            var locations = new HashSet<Common.Types.KeyPair<int, int>>();
            var startLocation = 0;
            while (true)
            {
                if (boundaryPoints.Count > 1 && boundaryPoints.First() == item)
                    break;
                var neighbors = MapClusters.getNeighbors(map, item);
                var neighborCount = 0;
                foreach (var neighbor in neighbors)
                    if (neighbor != 0f && neighbor == item.value) neighborCount++;

                if (neighborCount >= 2 && locations.Add(new Common.Types.KeyPair<int, int>(item.xIndex, item.yIndex)))
                    boundaryPoints.Add(item); //each location only gets added once, to deal with one pixel bridges

                var itemFound = false;
                for (int i = startLocation; i < neighbors.Length + startLocation; i++)
                {
                    var neighborValue = neighbors[i % 8];
                    if (neighborValue == 0f) continue;
                    if (neighborValue != item.value) continue;

                    var clusterNeighbor = new MapItem(NeighborDirections.GetNeighborX(item.xIndex, i % 8), NeighborDirections.GetNeighborY(item.yIndex, i % 8), (float)(int)neighborValue, item.tileSize);
                    foreach (var cluster in clusters)
                        if (cluster.contains(clusterNeighbor))
                            return null; //don't return the same cluster multiple times

                    neighborCount = 0;
                    foreach (var neighborNeighbor in MapClusters.getNeighbors(map, clusterNeighbor))
                    {
                        if (neighborNeighbor == item.value) neighborCount++;
                    }

                    if (boundaryPoints.Count != 0 || neighborCount >= 2)
                    {
                        item = clusterNeighbor;
                        startLocation = NeighborDirections.GetOppositeDirection(i) + 1;
                        itemFound = true;
                        break;
                    }
                }
                if (!itemFound) break;
            }

            if (boundaryPoints.Count < 3) return null;
            return new MapCluster(boundaryPoints);
        }

        //expects a counter clockwise list of border points
        public float[] getHeights(List<Vector2> borderPoints, float cellSize)
        {
            var rectangle = borderPoints.getRectangle();

            var width = (int)(rectangle.width / cellSize);
            var heights = new float[width];
            var heightsCount = new int[width];

            foreach (var current in borderPoints)
            {
                if (current.y == rectangle.y) continue;

                int currentX = (int)(current.x - rectangle.x);
                var relativeX = currentX.relativePosition((int)rectangle.width);
                var heightIndex = relativeX.getIndex(heights);

                var currentY = current.y - rectangle.y;
                heights[heightIndex] += currentY;
                heightsCount[heightIndex]++;
            }

            for (int i = 0; i < heights.Length; i++)
            {
                if (heightsCount[i] != 0) heights[i] /= heightsCount[i];
            }

            for (int i = 0; i < heights.Length; i++)
            {
                if (heightsCount[i] == 0)
                {
                    var previous = getPreviousValue(heights, i);
                    var next = getNextValue(heights, i);

                    //y coordinate is the distance
                    var range = previous.y + next.y; //3 + 5 = 8
                    var position = previous.y / range; //3 / 8 = 0.375

                    heights[i] = previous.x.interpolate(next.x, position);
                }
            }

            return heights;
        }

        //x is the value, y is the distance
        private Vector2 getNextValue(float[] values, int index)
        {
            var nextValue = Vec2Factory.Instance.get();
            for (int i = index + 1; i < values.Length; i++)
            {
                var value = values[i];
                if (value != 0f)
                {
                    nextValue.set(value, (i - index));
                    break;
                }
            }
            return nextValue;
        }

        //x is the value, y is the distance
        private Vector2 getPreviousValue(float[] values, int index)
        {
            var nextValue = Vec2Factory.Instance.get();
            for (int i = index - 1; i >= 0; i--)
            {
                var value = values[i];
                if (value != 0f)
                {
                    nextValue.set(value, (index - i));
                    break;
                }
            }
            return nextValue;
        }
    }
}
