using Revert.Core.Mathematics.Vectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revert.Core.Mathematics
{
    public class EarClippingTriangulator
    {
        static private int CONCAVE = -1;
        static private int CONVEX = 1;

        private List<short> indicesArray = new List<short>();
        private short[] indices;
        private float[] vertexFan;
        private int vertexCount;
        private List<int> vertexTypes = new List<int>();
        private List<short> triangles = new List<short>();

        public List<short> computeTriangles(IEnumerable<Vector2> vertices)
        {
            this.vertexFan = vertices.flatten();

            List<short> indicesArray = this.indicesArray;
            indicesArray.Clear();
            //indicesArray.ensureCapacity(vertexCount);
            //indicesArray.size = vertexCount;
            indices = indicesArray.ToArray();
            if (GeometryUtils.isClockwise(vertexFan, 0, vertices.Count()))
            {
                for (short i = 0; i < vertexCount; i++)
                    indices[i] = (short)(i);
            }
            else
            {
                for (int i = 0, n = vertexCount - 1; i < vertexCount; i++)
                    indices[i] = (short)(n - i); // Reversed.
            }

            var vertexTypes = this.vertexTypes;
            vertexTypes.Clear();
            //vertexTypes.ensureCapacity(vertexCount);
            for (int i = 0, n = vertexCount; i < n; ++i)
                vertexTypes.Add(classifyVertex(i));

            // A polygon with n vertices has a triangulation of n-2 triangles.
            var triangles = this.triangles;
            triangles.Clear();
            //triangles.ensureCapacity(Math.max(0, vertexCount - 2) * 3);
            triangulate();
            return triangles;
        }

        /** @see #computeTriangles(float[], int, int) */
        public List<short> computeTriangles(List<float> vertices)
        {
            return computeTriangles(vertices.ToArray(), 0, vertices.Count);
        }

        /** @see #computeTriangles(float[], int, int) */
        public List<short> computeTriangles(float[] vertices)
        {
            return computeTriangles(vertices, 0, vertices.Length);
        }

        /** Triangulates the given (convex or concave) simple polygon to a list of triangle vertices.
         * @param vertices pairs describing vertices of the polygon, in either clockwise or counterclockwise order.
         * @return triples of triangle indices in clockwise order. Note the returned array is reused for later calls to the same
         *         method. */
        public List<short> computeTriangles(float[] vertices, int offset, int count)
        {
            this.vertexFan = vertices;
            int vertexCount = this.vertexCount = count / 2;
            int vertexOffset = offset / 2;

            List<short> indicesArray = this.indicesArray;
            indicesArray.Clear();
            //indicesArray.ensureCapacity(vertexCount);
            //indicesArray.size = vertexCount;
            indices = indicesArray.ToArray();
            if (GeometryUtils.isClockwise(vertices, offset, count))
            {
                for (short i = 0; i < vertexCount; i++)
                    indices[i] = (short)(vertexOffset + i);
            }
            else
            {
                for (int i = 0, n = vertexCount - 1; i < vertexCount; i++)
                    indices[i] = (short)(vertexOffset + n - i); // Reversed.
            }

            var vertexTypes = this.vertexTypes;
            vertexTypes.Clear();
            //vertexTypes.ensureCapacity(vertexCount);
            for (int i = 0, n = vertexCount; i < n; ++i)
                vertexTypes.Add(classifyVertex(i));

            // A polygon with n vertices has a triangulation of n-2 triangles.
            var triangles = this.triangles;
            triangles.Clear();
            //triangles.ensureCapacity(Math.max(0, vertexCount - 2) * 3);
            triangulate();
            return triangles;
        }

        private void triangulate()
        {
            int[] vertexTypes = this.vertexTypes.ToArray();

            while (vertexCount > 3)
            {
                int earTipIndex = findEarTip();
                cutEarTip(earTipIndex);

                // The type of the two vertices adjacent to the clipped vertex may have changed.
                int previous = previousIndex(earTipIndex);
                int nextIndex = earTipIndex == vertexCount ? 0 : earTipIndex;
                vertexTypes[previous] = classifyVertex(previous);
                vertexTypes[nextIndex] = classifyVertex(nextIndex);
            }

            if (vertexCount == 3)
            {
                var triangles = this.triangles;
                short[] indices = this.indices;
                triangles.Add(indices[0]);
                triangles.Add(indices[1]);
                triangles.Add(indices[2]);
            }
        }

        /** @return {@link #CONCAVE} or {@link #CONVEX} */
        private int classifyVertex(int index)
        {
            short[] indices = this.indices;
            int previous = indices[previousIndex(index)] * 2;
            int current = indices[index] * 2;
            int next = indices[nextIndex(index)] * 2;
            float[] vertices = this.vertexFan;
            return computeSpannedAreaSign(vertices[previous], vertices[previous + 1], vertices[current], vertices[current + 1],
                vertices[next], vertices[next + 1]);
        }

        private int findEarTip()
        {
            int vertexCount = this.vertexCount;
            for (int i = 0; i < vertexCount; i++)
                if (isEarTip(i)) return i;

            // Desperate mode: if no vertex is an ear tip, we are dealing with a degenerate polygon (e.g. nearly collinear).
            // Note that the input was not necessarily degenerate, but we could have made it so by clipping some valid ears.

            // Idea taken from Martin Held, "FIST: Fast industrial-strength triangulation of polygons", Algorithmica (1998),
            // http://citeseerx.ist.psu.edu/viewdoc/summary?doi=10.1.1.115.291

            // Return a convex or tangential vertex if one exists.
            int[] vertexTypes = this.vertexTypes.ToArray();
            for (int i = 0; i < vertexCount; i++)
                if (vertexTypes[i] != CONCAVE) return i;
            return 0; // If all vertices are concave, just return the first one.
        }

        private bool isEarTip(int earTipIndex)
        {
            int[] vertexTypes = this.vertexTypes.ToArray();
            if (vertexTypes[earTipIndex] == CONCAVE) return false;

            int prev = previousIndex(earTipIndex);
            int next = nextIndex(earTipIndex);
            short[] indices = this.indices;
            int p1 = indices[prev] * 2;
            int p2 = indices[earTipIndex] * 2;
            int p3 = indices[next] * 2;
            float[] vertices = this.vertexFan;
            float p1x = vertices[p1], p1y = vertices[p1 + 1];
            float p2x = vertices[p2], p2y = vertices[p2 + 1];
            float p3x = vertices[p3], p3y = vertices[p3 + 1];

            // Check if any point is inside the triangle formed by previous, current and next vertices.
            // Only consider vertices that are not part of this triangle, or else we'll always find one inside.
            for (int i = nextIndex(next); i != prev; i = nextIndex(i))
            {
                // Concave vertices can obviously be inside the candidate ear, but so can tangential vertices
                // if they coincide with one of the triangle's vertices.
                if (vertexTypes[i] != CONVEX)
                {
                    int v = indices[i] * 2;
                    float vx = vertices[v];
                    float vy = vertices[v + 1];
                    // Because the polygon has clockwise winding order, the area sign will be positive if the point is strictly inside.
                    // It will be 0 on the edge, which we want to include as well.
                    // note: check the edge defined by p1->p3 first since this fails _far_ more then the other 2 checks.
                    if (computeSpannedAreaSign(p3x, p3y, p1x, p1y, vx, vy) >= 0)
                    {
                        if (computeSpannedAreaSign(p1x, p1y, p2x, p2y, vx, vy) >= 0)
                        {
                            if (computeSpannedAreaSign(p2x, p2y, p3x, p3y, vx, vy) >= 0) return false;
                        }
                    }
                }
            }
            return true;
        }

        private void cutEarTip(int earTipIndex)
        {
            short[] indices = this.indices;
            var triangles = this.triangles;

            triangles.Add(indices[previousIndex(earTipIndex)]);
            triangles.Add(indices[earTipIndex]);
            triangles.Add(indices[nextIndex(earTipIndex)]);

            indicesArray.RemoveAt(earTipIndex); 
            vertexTypes.RemoveAt(earTipIndex);
            vertexCount--;
        }

        private int previousIndex(int index)
        {
            return (index == 0 ? vertexCount : index) - 1;
        }

        private int nextIndex(int index)
        {
            return (index + 1) % vertexCount;
        }

        static private int computeSpannedAreaSign(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            float area = x1 * (y3 - y2);
            area += x2 * (y1 - y3);
            area += x3 * (y2 - y1);
            return Math.Sign(area);
        }
    }
}
