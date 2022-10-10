using Revert.Core.Extensions;
using Revert.Port.LibGDX.Mathematics.Vectors;
using System;

namespace Revert.Port.LibGDX.Mathematics.Geometry
{
    public class Circle
    {
        public float X, Y;
        public float radius;

        /** Constructs a new circle with all values set to zero */
        public Circle()
        {

        }

        /** Constructs a new circle with the given X and Y coordinates and the given radius.
         * 
         * @param x X coordinate
         * @param y Y coordinate
         * @param radius The radius of the circle */
        public Circle(float x, float y, float radius)
        {
            X = x;
            Y = y;
            this.radius = radius;
        }

        /** Constructs a new circle using a given {@link Vector2} that contains the desired X and Y coordinates, and a given radius.
         * 
         * @param position The position {@link Vector2}.
         * @param radius The radius */
        public Circle(Vector2 position, float radius)
        {
            X = position.x;
            Y = position.y;
            this.radius = radius;
        }

        /** Copy constructor
         * 
         * @param circle The circle to construct a copy of. */
        public Circle(Circle circle)
        {
            X = circle.X;
            Y = circle.Y;
            radius = circle.radius;
        }

        /** Creates a new {@link Circle} in terms of its center and a point on its edge.
         * 
         * @param center The center of the new circle
         * @param edge Any point on the edge of the given circle */
        public Circle(Vector2 center, Vector2 edge)
        {
            X = center.x;
            Y = center.y;
            radius = new Vector2(center.x - edge.x, center.y - edge.y).len();
        }

        /** Sets a new location and radius for this circle.
         * 
         * @param x X coordinate
         * @param y Y coordinate
         * @param radius Circle radius */
        public void set(float x, float y, float radius)
        {
            X = x;
            Y = y;
            this.radius = radius;
        }

        /** Sets a new location and radius for this circle.
         * 
         * @param position Position {@link Vector2} for this circle.
         * @param radius Circle radius */
        public void set(Vector2 position, float radius)
        {
            X = position.x;
            Y = position.y;
            this.radius = radius;
        }

        /** Sets a new location and radius for this circle, based upon another circle.
         * 
         * @param circle The circle to copy the position and radius of. */
        public void set(Circle circle)
        {
            X = circle.X;
            Y = circle.Y;
            radius = circle.radius;
        }

        /** Sets this {@link Circle}'s values in terms of its center and a point on its edge.
         * 
         * @param center The new center of the circle
         * @param edge Any point on the edge of the given circle */
        public void set(Vector2 center, Vector2 edge)
        {
            X = center.x;
            Y = center.y;
            radius = new Vector2(center.x - edge.x, center.y - edge.y).len();
        }

        /** Sets the x and y-coordinates of circle center from vector
         * @param position The position vector */
        public void setPosition(Vector2 position)
        {
            X = position.x;
            Y = position.y;
        }

        /** Sets the x and y-coordinates of circle center
         * @param x The x-coordinate
         * @param y The y-coordinate */
        public void setPosition(float x, float y)
        {
            X = x;
            Y = y;
        }

        /** Sets the x-coordinate of circle center
         * @param x The x-coordinate */
        public void setX(float x)
        {
            X = x;
        }

        /** Sets the y-coordinate of circle center
         * @param y The y-coordinate */
        public void setY(float y)
        {
            Y = y;
        }

        /** Sets the radius of circle
         * @param radius The radius */
        public void setRadius(float radius)
        {
            this.radius = radius;
        }

        /** Checks whether or not this circle contains a given point.
         * 
         * @param x X coordinate
         * @param y Y coordinate
         * 
         * @return true if this circle contains the given point. */
        public bool contains(float x, float y)
        {
            x = X - x;
            y = Y - y;
            return x * x + y * y <= radius * radius;
        }

        /** Checks whether or not this circle contains a given point.
         * 
         * @param point The {@link Vector2} that contains the point coordinates.
         * 
         * @return true if this circle contains this point; false otherwise. */
        public bool contains(Vector2 point)
        {
            float dx = X - point.x;
            float dy = Y - point.y;
            return dx * dx + dy * dy <= radius * radius;
        }

        /** @param c the other {@link Circle}
         * @return whether this circle contains the other circle. */
        public bool contains(Circle c)
        {
            float radiusDiff = radius - c.radius;
            if (radiusDiff < 0f) return false; // Can't contain bigger circle
            float dx = X - c.X;
            float dy = Y - c.Y;
            float dst = dx * dx + dy * dy;
            float radiusSum = radius + c.radius;
            return !(radiusDiff * radiusDiff < dst) && dst < radiusSum * radiusSum;
        }

        /** @param c the other {@link Circle}
         * @return whether this circle overlaps the other circle. */
        public bool overlaps(Circle c)
        {
            float dx = X - c.X;
            float dy = Y - c.Y;
            float distance = dx * dx + dy * dy;
            float radiusSum = radius + c.radius;
            return distance < radiusSum * radiusSum;
        }

        /** Returns a {@link String} representation of this {@link Circle} of the form {@code x,y,radius}. */
        public override string ToString()
        {
            return X + "," + Y + "," + radius;
        }

        /** @return The circumference of this circle (as 2 * {@link MathUtils#PI2}) * {@code radius} */
        public float circumference()
        {
            return (float)(radius * Math.PI * 2f);
        }

        /** @return The area of this circle (as {@link MathUtils#PI} * radius * radius). */
        public float area()
        {
            return (float)(radius * radius * Math.PI * 2f);
        }

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            if (obj == null || obj.GetType() != GetType()) return false;
            Circle c = (Circle)obj;
            return X == c.X && Y == c.Y && radius == c.radius;
        }

        public override int GetHashCode()
        {
            int prime = 41;
            int result = 1;
            result = prime * result + radius.getIntBytes();
            result = prime * result + X.getIntBytes();
            result = prime * result + Y.getIntBytes();
            return result;
        }

    }
}
