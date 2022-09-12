using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics.Clusters
{
    public class MapItem
    {
        public MapItem(int xIndex, int yIndex, float value, float tileSize)
        {
            this.xIndex = xIndex;
            this.yIndex = yIndex;
            this.value = value;
            this.tileSize = tileSize;
        }

        public int xIndex { get; }
        public int yIndex { get; }
        public float value { get; private set; } 
        public float tileSize { get; }

        public int sceneX { get { return (int)( xIndex * tileSize); } }
        public int sceneY { get { return (int)(yIndex * tileSize); } }

        public override int GetHashCode()
        {
            return xIndex ^ yIndex ^ (int)value;
        }

        public override bool Equals(object obj)
        {
            var map = obj as MapItem;
            if (map != null)
            {
                return map.xIndex == xIndex && map.yIndex == yIndex && map.value == map.value;
            }
            return false;
        }

    }
}
