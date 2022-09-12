using Revert.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Clusters
{
    public class GenericCluster
    {
        public static int[] GetNeighbors(int[][] map, MapItem item)
        {
            var items = new int[8];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = GetNeighbor(map, item, i);
            }
            return items;
        }
        
        public static int GetNeighbor(int[][] map, MapItem item, int direction) {
            var targetX = item.xIndex;
            var targetY = item.yIndex;

            //X Coordinate
            if (direction == NeighborDirections.TOP_RIGHT || direction == NeighborDirections.RIGHT || direction == NeighborDirections.BOTTOM_RIGHT) targetX++;
            else if (direction == NeighborDirections.TOP_LEFT || direction == NeighborDirections.LEFT || direction == NeighborDirections.BOTTOM_LEFT) targetX--;

            if (targetX < 0 || targetX >= map[0].Length) return 0;

            //Y Coordinate
            if (direction == NeighborDirections.TOP_RIGHT || direction == NeighborDirections.TOP || direction == NeighborDirections.TOP_LEFT) targetY++;
            else if (direction == NeighborDirections.BOTTOM_RIGHT || direction == NeighborDirections.BOTTOM || direction == NeighborDirections.BOTTOM_LEFT) targetY--;

            if (targetY < 0 || targetY >= map.Length) return 0;
            return map[targetY][targetX];
        }
         
    }
}
