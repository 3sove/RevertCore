using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Mathematics.Vectors
{
    public static class Vertices
    {
        public static float[] GetVertexFan(List<Vector2> vertices)
        {
            var vertexFan = new float[vertices.Count * 2];
            var index = 0;
            foreach (var vertex in vertices)
            {
                vertexFan[index++] = vertex.x;
                vertexFan[index++] = vertex.y;
            }
            return vertexFan;
        }

        public static float[] GetVertexFan3(List<Vector2> vertices)
        {
            var vertexFan = new float[vertices.Count * 3];
            var index = 0;
            foreach (var vertex in vertices)
            {
                vertexFan[index++] = vertex.x;
                vertexFan[index++] = vertex.y;
                vertexFan[index++] = 0f;
            }
            return vertexFan;
        }

        public static float[] ExpandVertexFan2to3(float[] vertices)
        {
            var vertexFan = new float[vertices.Length / 2 * 3];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertexFan[i * 3] = vertices[i * 2];
                vertexFan[i * 3 + 1] = vertices[i * 2 + 1];
                vertexFan[i * 3 + 2] = 0f;
            }
            return vertexFan;
        }

        public static float[] ExpandVertexFan(float[] vertices, int startingComponents, int endingComponents)
        {
            var vertexFan = new float[vertices.Length / startingComponents * endingComponents];
            for (int i = 0; i < vertices.Length / startingComponents; i++)
            {
                for (int s = 0; s < startingComponents; s++)
                {
                    vertexFan[i * endingComponents + s] = vertices[i * startingComponents + s];
                }

                for (int e = 0; e < endingComponents - startingComponents; e++)
                {
                    vertexFan[i * endingComponents + startingComponents + e] = 0f;
                }
            }
            return vertexFan;
        }

        public static float[] ExpandVertexFan2to4(float[] vertices)
        {
            var vertexFan = new float[(int)(vertices.Length / 2f * 4f)];
            var i = 0;
            while (i < vertices.Length / 2f)
            {
                vertexFan[i * 4] = vertices[i * 2];
                vertexFan[i * 4 + 1] = vertices[i * 2 + 1];
                vertexFan[i * 4 + 2] = 0f;
                vertexFan[i * 4 + 3] = 0f;
                i++;
            }
            return vertexFan;
        }

        public static float[] ExpandVertexFan2to5(float[] vertices)
        {
            var vertexFan = new float[vertices.Length / 2 * 5];
            for (int i = 0; i < vertices.Length / 2; i++)
            {
                vertexFan[i * 5] = vertices[i * 2];
                vertexFan[i * 5 + 1] = vertices[i * 2 + 1];
                vertexFan[i * 5 + 2] = 0f;
                vertexFan[i * 5 + 3] = 0f;
                vertexFan[i * 5 + 4] = 0f;
            }
            return vertexFan;
        }

        public static float GetDistance(List<Vector2> vertices)
        {
            var distance = 0f;
            Vector2 previous = null;
            foreach (var vector in vertices)
            {
                if (previous != null) distance += previous.distance(vector);
                previous = vector;
            }
            return distance;
        }
    }
}
