using Revert.Core.Extensions;
using Revert.Core.Mathematics.Factories;
using Revert.Core.Mathematics.Vectors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Mathematics
{
    public static class Polygons
    {
        public static PolygonModel GetRectangle(Vector2 position, float width, float height)
        {
            var vertices = new List<Vector2>();

            var halfWidth = width * .5f;
            var halfHeight = height * .5f;

            vertices.Add(Vec2Factory.Instance.get(position.x - halfWidth, position.y - halfHeight));
            vertices.Add(Vec2Factory.Instance.get(position.x + halfWidth, position.y - halfHeight));
            vertices.Add(Vec2Factory.Instance.get(position.x + halfWidth, position.y + halfHeight));
            vertices.Add(Vec2Factory.Instance.get(position.x - halfWidth, position.y + halfHeight));

            foreach (var vector in vertices) vector.add(halfWidth, halfHeight);

            var vertexFan = Vertices.GetVertexFan(vertices);
            return GetPolygonModel(vertices);
            //return GetPolygonModel(vertexFan);
        }

        public static List<Vector2> GetTriangleVertices(float x, float y, float width, float height)
        {
            return GetTriangleVertices(Vec2Factory.Instance.get(x, y), width, height);
        }

        public static List<Vector2> GetTriangleVertices(Vector2 position, float width, float height)
        {
            var vertices = new List<Vector2>();

            var halfWidth = width * .5f;
            var halfHeight = height * .5f;

            vertices.Add(Vec2Factory.Instance.get(position.x - halfWidth, position.y - halfHeight));
            vertices.Add(Vec2Factory.Instance.get(position.x + halfWidth, position.y - halfHeight));
            vertices.Add(Vec2Factory.Instance.get(position.x, position.y + halfHeight));

            foreach (var vector in vertices) vector.add(halfWidth, halfHeight);
            return vertices;
        }

        public static List<Vector2> GetTriangleVertices(float width, float height)
        {
            var vertices = new List<Vector2>();

            var halfWidth = width * .5f;
            var halfHeight = height * .5f;

            vertices.Add(Vec2Factory.Instance.get(-halfWidth, -halfHeight));
            vertices.Add(Vec2Factory.Instance.get(halfWidth, -halfHeight));
            vertices.Add(Vec2Factory.Instance.get(0f, halfHeight));

            foreach (var vector in vertices) vector.add(halfWidth, halfHeight);
            return vertices;
        }

        public static PolygonModel GetTriangle(float x, float y, float width, float height)
        {
            return GetTriangle(Vec2Factory.Instance.get(x, y), width, height);
        }

        public static PolygonModel GetTriangle(Vector2 position, float width, float height)
        {
            var vertices = GetTriangleVertices(position, width, height);
            //var vertexFan = Vertices.GetVertexFan(vertices);
            return GetPolygonModel(vertices);// vertexFan);
        }

        public static PolygonModel GetCircle(Vector2 position, float radius, int points)
        {
            var vertices = new List<Vector2>();
            var target = Vec2Factory.Instance.get(position);

            var targetAngle = 0f;
            var angleStep = 360f / points;

            for (int i = 0; i < points; i++)
            {
                var vertex = Vec2Factory.Instance.get((float)Math.Cos(targetAngle.toRadians()) * radius, (float)(Math.Sin(targetAngle.toRadians()) * radius));
                vertex.add(radius, radius);

                vertex.add(target);
                vertices.Add(vertex);
                targetAngle += angleStep;
            }

            //var vertexFan = Vertices.GetVertexFan(vertices);
            return GetPolygonModel(vertices);// vertexFan);
        }

        //public static PolygonModel GetPolygonModel(float[] vertexFan)
        //{
        //    var triangulator = new EarClippingTriangulator();
        //    var triangleIndices = triangulator.computeTriangles(vertexFan);
        //    var triangleVertices = new float[triangleIndices.Count * 2];

        //    var i = 0;
        //    while (i < triangleIndices.Count)
        //    {
        //        //triangleIndices point to pairs (e.g. index 0x = item1, index 0y = item2, index 1x = item3, etc...
        //        var vertexIndex1 = triangleIndices[i]; //triangle pairs are returned clockwise, but box2d needs them counterclockwise
        //        var vertexIndex2 = triangleIndices[i + 1];
        //        var vertexIndex3 = triangleIndices[i + 2];

        //        var vertexIndex = i * 2;

        //        triangleVertices[vertexIndex] = vertexFan[vertexIndex1 * 2];       //1x
        //        triangleVertices[vertexIndex + 1] = vertexFan[vertexIndex1 * 2 + 1];   //1y

        //        triangleVertices[vertexIndex + 2] = vertexFan[vertexIndex2 * 2];       //2x
        //        triangleVertices[vertexIndex + 3] = vertexFan[vertexIndex2 * 2 + 1];   //2y

        //        triangleVertices[vertexIndex + 4] = vertexFan[vertexIndex3 * 2];       //3x
        //        triangleVertices[vertexIndex + 5] = vertexFan[vertexIndex3 * 2 + 1];   //3y
        //        i += 3;
        //    }

        //    return new PolygonModel(vertexFan, triangleVertices, triangleIndices.ToArray());
        //}


        public static PolygonModel GetPolygonModel(List<Vector2> vertices)
        {
            return GetPolygonModel(vertices.ToArray());
        }

        public static PolygonModel GetPolygonModel(Vector2[] vertices)
        {
            var triangulator = new EarClippingTriangulator();
            var triangleIndices = triangulator.computeTriangles(vertices);
            var triangleVertices = new float[triangleIndices.Count * 2];

            var i = 0;
            while (i < triangleIndices.Count)
            {
                //triangleIndices point to pairs (e.g. index 0x = item1, index 0y = item2, index 1x = item3, etc...
                var vertexIndex1 = triangleIndices[i]; //triangle pairs are returned clockwise, but box2d needs them counterclockwise
                var vertexIndex2 = triangleIndices[i + 1];
                var vertexIndex3 = triangleIndices[i + 2];

                var vertexIndex = i * 2;

                var v1 = vertices[vertexIndex1];
                var v2 = vertices[vertexIndex2];
                var v3 = vertices[vertexIndex3];

                triangleVertices[vertexIndex] = v1.x; //vertices[vertexIndex1 * 2];       //1x
                triangleVertices[vertexIndex + 1] = v1.y; //vertices[vertexIndex1 * 2 + 1];   //1y

                triangleVertices[vertexIndex + 2] = v2.x; //vertices[vertexIndex2 * 2];       //2x
                triangleVertices[vertexIndex + 3] = v2.y; //vertices[vertexIndex2 * 2 + 1];   //2y

                triangleVertices[vertexIndex + 4] = v3.x; //vertices[vertexIndex3 * 2];       //3x
                triangleVertices[vertexIndex + 5] = v3.y; //vertices[vertexIndex3 * 2 + 1];   //3y
                i += 3;
            }

            return new PolygonModel(vertices, triangleVertices, triangleIndices.ToArray());
        }

    }
}
