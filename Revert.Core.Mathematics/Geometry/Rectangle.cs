﻿using Revert.Core.Extensions;
using Revert.Core.Mathematics.Vectors;
using System;

namespace Revert.Core.Mathematics.Geometry
{
    public class Rectangle
    {
        /** Static temporary rectangle. Use with care! Use only when sure other code will not also use this. */
        static public readonly Rectangle tmp = new Rectangle();

        /** Static temporary rectangle. Use with care! Use only when sure other code will not also use this. */
        static public readonly Rectangle tmp2 = new Rectangle();

        public float x, y;
        public float width, height;

        /** Constructs a new rectangle with all values set to zero */
        public Rectangle()
        {

        }

        /** Constructs a new rectangle with the given corner point in the bottom left and dimensions.
         * @param x The corner point x-coordinate
         * @param y The corner point y-coordinate
         * @param width The width
         * @param height The height */
        public Rectangle(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        /** Constructs a rectangle based on the given rectangle
         * @param rect The rectangle */
        public Rectangle(Rectangle rect)
        {
            x = rect.x;
            y = rect.y;
            width = rect.width;
            height = rect.height;
        }

        /** @param x bottom-left x coordinate
         * @param y bottom-left y coordinate
         * @param width width
         * @param height height
         * @return this rectangle for chaining */
        public Rectangle set(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;

            return this;
        }

        /** @return the x-coordinate of the bottom left corner */
        public float getX()
        {
            return x;
        }

        /** Sets the x-coordinate of the bottom left corner
         * @param x The x-coordinate
         * @return this rectangle for chaining */
        public Rectangle setX(float x)
        {
            this.x = x;

            return this;
        }

        /** @return the y-coordinate of the bottom left corner */
        public float getY()
        {
            return y;
        }

        /** Sets the y-coordinate of the bottom left corner
         * @param y The y-coordinate
         * @return this rectangle for chaining */
        public Rectangle setY(float y)
        {
            this.y = y;

            return this;
        }

        /** @return the width */
        public float getWidth()
        {
            return width;
        }

        /** Sets the width of this rectangle
         * @param width The width
         * @return this rectangle for chaining */
        public Rectangle setWidth(float width)
        {
            this.width = width;

            return this;
        }

        /** @return the height */
        public float getHeight()
        {
            return height;
        }

        /** Sets the height of this rectangle
         * @param height The height
         * @return this rectangle for chaining */
        public Rectangle setHeight(float height)
        {
            this.height = height;

            return this;
        }

        /** return the Vector2 with coordinates of this rectangle
         * @param position The Vector2 */
        public Vector2 getPosition(Vector2 position)
        {
            return position.set(x, y);
        }

        /** Sets the x and y-coordinates of the bottom left corner from vector
         * @param position The position vector
         * @return this rectangle for chaining */
        public Rectangle setPosition(Vector2 position)
        {
            x = position.x;
            y = position.y;

            return this;
        }

        /** Sets the x and y-coordinates of the bottom left corner
         * @param x The x-coordinate
         * @param y The y-coordinate
         * @return this rectangle for chaining */
        public Rectangle setPosition(float x, float y)
        {
            this.x = x;
            this.y = y;

            return this;
        }

        /** Sets the width and height of this rectangle
         * @param width The width
         * @param height The height
         * @return this rectangle for chaining */
        public Rectangle setSize(float width, float height)
        {
            this.width = width;
            this.height = height;

            return this;
        }

        /** Sets the squared size of this rectangle
         * @param sizeXY The size
         * @return this rectangle for chaining */
        public Rectangle setSize(float sizeXY)
        {
            width = sizeXY;
            height = sizeXY;

            return this;
        }

        /** @return the Vector2 with size of this rectangle
         * @param size The Vector2 */
        public Vector2 getSize(Vector2 size)
        {
            return size.set(width, height);
        }

        /** @param x point x coordinate
         * @param y point y coordinate
         * @return whether the point is contained in the rectangle */
        public bool contains(float x, float y)
        {
            return this.x <= x && this.x + width >= x && this.y <= y && this.y + height >= y;
        }

        /** @param point The coordinates vector
         * @return whether the point is contained in the rectangle */
        public bool contains(Vector2 point)
        {
            return contains(point.x, point.y);
        }

        /** @param circle the circle
         * @return whether the circle is contained in the rectangle */
        public bool contains(Circle circle)
        {
            return circle.X - circle.radius >= x && circle.X + circle.radius <= x + width
                && circle.Y - circle.radius >= y && circle.Y + circle.radius <= y + height;
        }

        /** @param rectangle the other {@link Rectangle}.
         * @return whether the other rectangle is contained in this rectangle. */
        public bool contains(Rectangle rectangle)
        {
            float xmin = rectangle.x;
            float xmax = xmin + rectangle.width;

            float ymin = rectangle.y;
            float ymax = ymin + rectangle.height;

            return xmin > x && xmin < x + width && xmax > x && xmax < x + width
                && ymin > y && ymin < y + height && ymax > y && ymax < y + height;
        }

        /** @param r the other {@link Rectangle}
         * @return whether this rectangle overlaps the other rectangle. */
        public bool overlaps(Rectangle r)
        {
            return x < r.x + r.width && x + width > r.x && y < r.y + r.height && y + height > r.y;
        }

        /** Sets the values of the given rectangle to this rectangle.
         * @param rect the other rectangle
         * @return this rectangle for chaining */
        public Rectangle set(Rectangle rect)
        {
            x = rect.x;
            y = rect.y;
            width = rect.width;
            height = rect.height;

            return this;
        }

        /** Merges this rectangle with the other rectangle. The rectangle should not have negative width or negative height.
         * @param rect the other rectangle
         * @return this rectangle for chaining */
        public Rectangle merge(Rectangle rect)
        {
            float minX = Math.Min(x, rect.x);
            float maxX = Math.Max(x + width, rect.x + rect.width);
            x = minX;
            width = maxX - minX;

            float minY = Math.Min(y, rect.y);
            float maxY = Math.Max(y + height, rect.y + rect.height);
            y = minY;
            height = maxY - minY;

            return this;
        }

        /** Merges this rectangle with a point. The rectangle should not have negative width or negative height.
         * @param x the x coordinate of the point
         * @param y the y coordinate of the point
         * @return this rectangle for chaining */
        public Rectangle merge(float x, float y)
        {
            float minX = Math.Min(this.x, x);
            float maxX = Math.Max(this.x + width, x);
            this.x = minX;
            width = maxX - minX;

            float minY = Math.Min(this.y, y);
            float maxY = Math.Max(this.y + height, y);
            this.y = minY;
            height = maxY - minY;

            return this;
        }

        /** Merges this rectangle with a point. The rectangle should not have negative width or negative height.
         * @param vec the vector describing the point
         * @return this rectangle for chaining */
        public Rectangle merge(Vector2 vec)
        {
            return merge(vec.x, vec.y);
        }

        /** Merges this rectangle with a list of points. The rectangle should not have negative width or negative height.
         * @param vecs the vectors describing the points
         * @return this rectangle for chaining */
        public Rectangle merge(Vector2[] vecs)
        {
            float minX = x;
            float maxX = x + width;
            float minY = y;
            float maxY = y + height;
            for (int i = 0; i < vecs.Length; ++i)
            {
                Vector2 v = vecs[i];
                minX = Math.Min(minX, v.x);
                maxX = Math.Max(maxX, v.x);
                minY = Math.Min(minY, v.y);
                maxY = Math.Max(maxY, v.y);
            }
            x = minX;
            width = maxX - minX;
            y = minY;
            height = maxY - minY;
            return this;
        }

        /** Calculates the aspect ratio ( width / height ) of this rectangle
         * @return the aspect ratio of this rectangle. Returns Float.NaN if height is 0 to avoid ArithmeticException */
        public float getAspectRatio()
        {
            return height == 0 ? float.NaN : width / height;
        }

        /** Calculates the center of the rectangle. Results are located in the given Vector2
         * @param vector the Vector2 to use
         * @return the given vector with results stored inside */
        public Vector2 getCenter(Vector2 vector)
        {
            vector.x = x + width / 2;
            vector.y = y + height / 2;
            return vector;
        }

        /** Moves this rectangle so that its center point is located at a given position
         * @param x the position's x
         * @param y the position's y
         * @return this for chaining */
        public Rectangle setCenter(float x, float y)
        {
            setPosition(x - width / 2, y - height / 2);
            return this;
        }

        /** Moves this rectangle so that its center point is located at a given position
         * @param position the position
         * @return this for chaining */
        public Rectangle setCenter(Vector2 position)
        {
            setPosition(position.x - width / 2, position.y - height / 2);
            return this;
        }

        /** Fits this rectangle around another rectangle while maintaining aspect ratio. This scales and centers the rectangle to the
         * other rectangle (e.g. Having a camera translate and scale to show a given area)
         * @param rect the other rectangle to fit this rectangle around
         * @return this rectangle for chaining
         * @see Scaling */
        public Rectangle fitOutside(Rectangle rect)
        {
            float ratio = getAspectRatio();

            if (ratio > rect.getAspectRatio())
            {
                // Wider than tall
                setSize(rect.height * ratio, rect.height);
            }
            else
            {
                // Taller than wide
                setSize(rect.width, rect.width / ratio);
            }

            setPosition(rect.x + rect.width / 2 - width / 2, rect.y + rect.height / 2 - height / 2);
            return this;
        }

        /** Fits this rectangle into another rectangle while maintaining aspect ratio. This scales and centers the rectangle to the
         * other rectangle (e.g. Scaling a texture within a arbitrary cell without squeezing)
         * @param rect the other rectangle to fit this rectangle inside
         * @return this rectangle for chaining
         * @see Scaling */
        public Rectangle fitInside(Rectangle rect)
        {
            float ratio = getAspectRatio();

            if (ratio < rect.getAspectRatio())
            {
                // Taller than wide
                setSize(rect.height * ratio, rect.height);
            }
            else
            {
                // Wider than tall
                setSize(rect.width, rect.width / ratio);
            }

            setPosition(rect.x + rect.width / 2 - width / 2, rect.y + rect.height / 2 - height / 2);
            return this;
        }

        /** Converts this {@code Rectangle} to a string in the format {@code [x,y,width,height]}.
         * @return a string representation of this object. */
        public string toString()
        {
            return "[" + x + "," + y + "," + width + "," + height + "]";
        }

        /** Sets this {@code Rectangle} to the value represented by the specified string according to the format of {@link #toString()}
         * .
         * @param v the string.
         * @return this rectangle for chaining */
        public Rectangle fromString(string v)
        {
            int s0 = v.IndexOf(',', 1);
            int s1 = v.IndexOf(',', s0 + 1);
            int s2 = v.IndexOf(',', s1 + 1);
            if (s0 != -1 && s1 != -1 && s2 != -1 && v[0] == '[' && v[v.Length - 1] == ']')
            {
                float x = float.Parse(v.Substring(1, s0));
                float y = float.Parse(v.Substring(s0 + 1, s1));
                float width = float.Parse(v.Substring(s1 + 1, s2));
                float height = float.Parse(v.Substring(s2 + 1, v.Length - 1));
                return set(x, y, width, height);
            }
            throw new Exception("Malformed Rectangle: " + v);
        }

        public float area()
        {
            return width * height;
        }

        public float perimeter()
        {
            return 2 * (width + height);
        }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + height.getIntBytes();
            result = prime * result + width.getIntBytes();
            result = prime * result + x.getIntBytes();
            result = prime * result + y.getIntBytes();
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null) return false;
            if (!GetType().Equals(obj.GetType())) return false;
            Rectangle other = (Rectangle)obj;
            if (height.getIntBytes() != other.height.getIntBytes()) return false;
            if (width.getIntBytes() != other.width.getIntBytes()) return false;
            if (x.getIntBytes() != other.x.getIntBytes()) return false;
            if (y.getIntBytes() != other.y.getIntBytes()) return false;
            return true;
        }
    }
}
