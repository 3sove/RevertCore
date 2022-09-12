using Revert.Core.Mathematics.Vectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revert.Core.Mathematics
{
    public class PolygonModel
    {
        public Vector2[] BorderVertices { get; }
        public float[] TriangleVertices { get; }
        public short[] TriangleIndices { get; }

        public PolygonModel(Vector2[] borderVertices, float[] triangleVertices, short[] triangleIndices)
        {
            BorderVertices = borderVertices;
            TriangleVertices = triangleVertices;
            TriangleIndices = triangleIndices;
        }

        public PolygonModel scale(float value)
        {
            var scaled = BorderVertices.ToArray();
            for (int i = 0; i < scaled.Length; i++)
                scaled[i].scl(value);
            return Polygons.GetPolygonModel(scaled);
        }
    }
}
