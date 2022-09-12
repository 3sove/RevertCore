using Revert.Core.Extensions;
using Revert.Core.Mathematics.Vectors;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Revert.Core.Mathematics.Geometry
{
    public class Polygon
    {
        private float[] localVertices;
        private float[] worldVertices;
        public float x { get; set; }
        public float y { get; set; }
        public float originX { get; set; }
        public float originY { get; set; }

        public float rotation;
        public float scaleX { get; set; } = 1;
        public float scaleY { get; set; } = 1;
        public bool dirty = true;
        public Rectangle bounds { get; set; }

        // Constructs a new polygon with no vertices. 
        public Polygon()
        {
            localVertices = new float[0];
        }

        //Constructs a new polygon from a float array of parts of vertex points.
        //@param vertices an array where every even element represents the horizontal part of a point, and the following element representing the vertical part
        //@throws IllegalArgumentException if less than 6 elements, representing 3 points, are provided
        public Polygon(float[] vertices)
        {
            if (vertices.Length < 6) throw new ArgumentException("polygons must contain at least 3 points.");
            localVertices = vertices;
        }

        //Returns the polygon's local vertices without scaling or rotation and without being offset by the polygon position. 
        public float[] getVertices()
        {
            return localVertices;
        }

        //Calculates and returns the vertices of the polygon after scaling, rotation, and positional translations have been applied, as they are position within the world.
        //returns vertices scaled, rotated, and offset by the polygon position. 
        public float[] getTransformedVertices()
        {
            if (!dirty) return this.worldVertices;
            dirty = false;

            float[] localVertices = this.localVertices;
            float[] worldVertices = this.worldVertices?.copy();

            if (worldVertices == null || worldVertices.Length != localVertices.Length) worldVertices = new float[localVertices.Length];

            float positionX = x;
            float positionY = y;
            float originX = this.originX;
            float originY = this.originY;
            float scaleX = this.scaleX;
            float scaleY = this.scaleY;
            bool scale = scaleX != 1 || scaleY != 1;
            float rotation = this.rotation;

            var rotationInRads = rotation.toRadians();

            float cos = (float)Math.Cos(rotationInRads);
            float sin = (float)Math.Sin(rotationInRads);

            for (int i = 0, n = localVertices.Length; i < n; i += 2)
            {
                float x = localVertices[i] - originX;
                float y = localVertices[i + 1] - originY;

                // scale if needed
                if (scale)
                {
                    x *= scaleX;
                    y *= scaleY;
                }

                // rotate if needed
                if (rotation != 0)
                {
                    float oldX = x;
                    x = cos * x - sin * y;
                    y = sin * oldX + cos * y;
                }

                worldVertices[i] = positionX + x + originX;
                worldVertices[i + 1] = positionY + y + originY;
            }
            return worldVertices;
        }

        //Sets the origin point to which all of the polygon's local vertices are relative to. 
        public void setOrigin(float originX, float originY)
        {
            this.originX = originX;
            this.originY = originY;
            dirty = true;
        }

        //Sets the polygon's position within the world. 
        public void setPosition(float x, float y)
        {
            this.x = x;
            this.y = y;
            dirty = true;
        }

        //Sets the polygon's local vertices relative to the origin point, without any scaling, rotating or translations being applied.
        //vertices = float array where every even element represents the x-coordinate of a vertex, and the proceeding element representing the y-coordinate.
        //@throws IllegalArgumentException if less than 6 elements, representing 3 points, are provided 
        public void setVertices(float[] vertices)
        {
            if (vertices.Length < 6) throw new ArgumentException("polygons must contain at least 3 points.");
            localVertices = vertices;
            dirty = true;
        }

        //Translates the polygon's position by the specified horizontal and vertical amounts. 
        public void translate(float x, float y)
        {
            this.x += x;
            this.y += y;
            dirty = true;
        }

        //Sets the polygon to be rotated by the supplied degrees. 
        public void setRotation(float degrees)
        {
            rotation = degrees;
            dirty = true;
        }

        //Applies additional rotation to the polygon by the supplied degrees. 
        public void rotate(float degrees)
        {
            rotation += degrees;
            dirty = true;
        }

        //Sets the amount of scaling to be applied to the polygon.
        public void setScale(float scaleX, float scaleY)
        {
            this.scaleX = scaleX;
            this.scaleY = scaleY;
            dirty = true;
        }

        //Applies additional scaling to the polygon by the supplied amount.
        public void scale(float amount)
        {
            scaleX += amount;
            scaleY += amount;
            dirty = true;
        }

        //Returns the area contained within the polygon.
        public float area()
        {
            float[] vertices = getTransformedVertices();
            return GeometryUtils.polygonArea(vertices, 0, vertices.Length);
        }

        //Returns an axis-aligned bounding box of this polygon.
        //Note the returned Rectangle is cached in this polygon, and will be reused if this Polygon is changed.
        //returns this polygon's bounding box {@link Rectangle} 
        public Rectangle getBoundingRectangle()
        {
            float[] vertices = getTransformedVertices();

            float minX = vertices[0];
            float minY = vertices[1];
            float maxX = vertices[0];
            float maxY = vertices[1];

            int numFloats = vertices.Length;
            for (int i = 2; i < numFloats; i += 2)
            {
                minX = minX > vertices[i] ? vertices[i] : minX;
                minY = minY > vertices[i + 1] ? vertices[i + 1] : minY;
                maxX = maxX < vertices[i] ? vertices[i] : maxX;
                maxY = maxY < vertices[i + 1] ? vertices[i + 1] : maxY;
            }

            if (bounds == null) bounds = new Rectangle();
            bounds.x = minX;
            bounds.y = minY;
            bounds.width = maxX - minX;
            bounds.height = maxY - minY;

            return bounds;
        }

        // Returns whether an x, y pair is contained within the polygon. 
        public bool Contains(float x, float y)
        {
            float[] vertices = getTransformedVertices();
            int numFloats = vertices.Length;
            int intersects = 0;

            for (int i = 0; i < numFloats; i += 2)
            {
                float x1 = vertices[i];
                float y1 = vertices[i + 1];
                float x2 = vertices[(i + 2) % numFloats];
                float y2 = vertices[(i + 3) % numFloats];
                if ((y1 <= y && y < y2 || y2 <= y && y < y1) && x < (x2 - x1) / (y2 - y1) * (y - y1) + x1) intersects++;
            }
            return (intersects & 1) == 1;
        }

        public bool Contains(Vector2 point)
        {
            return Contains(point.x, point.y);
        }

    }
}
