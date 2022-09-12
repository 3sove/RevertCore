using System;
using System.Collections.Generic;
using System.Text;
using Revert.Core.Mathematics.Interpolations;

namespace Revert.Core.Mathematics.Vectors
{
    public class Vector2 : IVector<Vector2>
    {
        public float x;
        public float y;

        public Vector2(float x = 0f, float y = 0f)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2 set(float x, float y)
        {
            this.x = x;
            this.y = y;
            return this;
        }

        public Vector2 sub(Vector2 v)
        {
            x -= v.x;
            y -= v.y;
            return this;
        }

        public Vector2 sub(float x, float y)
        {
            this.x -= x;
            this.y -= y;
            return this;
        }

        public Vector2 nor()
        {
            float length = len();
            if (length != 0)
            {
                x /= length;
                y /= length;
            }
            return this;
        }

        public Vector2 add(Vector2 v)
        {
            x += v.x;
            y += v.y;
            return this;
        }

        /** Adds the given components to this vector
         * @param x The x-component
         * @param y The y-component
         * @return This vector for chaining */
        public Vector2 add(float x, float y)
        {
            this.x += x;
            this.y += y;
            return this;
        }


        public static float dot(float x1, float y1, float x2, float y2)
        {
            return x1 * x2 + y1 * y2;
        }

        public float dot(Vector2 v)
        {
            return x * v.x + y * v.y;
        }

        public float dot(float ox, float oy)
        {
            return x * ox + y * oy;
        }

        public static float dot(Vector2 a, Vector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }


        public static float distance(float x1, float y1, float x2, float y2)
        {
            float x_d = x2 - x1;
            float y_d = y2 - y1;
            return (float)Math.Sqrt(x_d * x_d + y_d * y_d);
        }

        public float distance(Vector2 v)
        {
            float x_d = v.x - x;
            float y_d = v.y - y;
            return (float)Math.Sqrt(x_d * x_d + y_d * y_d);
        }

        public static float distance(Vector2 a, Vector2 b)
        {
            float x_d = b.x - a.x;
            float y_d = b.y - a.y;
            return (float)Math.Sqrt(x_d * x_d + y_d * y_d);
        }


        /** @param x The x-component of the other vector
         * @param y The y-component of the other vector
         * @return the distance between this and the other vector */
        public float distance(float x, float y)
        {
            float x_d = x - this.x;
            float y_d = y - this.y;
            return (float)Math.Sqrt(x_d * x_d + y_d * y_d);
        }

        public static float distanceSquared(float x1, float y1, float x2, float y2)
        {
            float x_d = x2 - x1;
            float y_d = y2 - y1;
            return x_d * x_d + y_d * y_d;
        }

        public float distanceSquared(Vector2 v)
        {
            float x_d = v.x - x;
            float y_d = v.y - y;
            return x_d * x_d + y_d * y_d;
        }

        /** @param x The x-component of the other vector
         * @param y The y-component of the other vector
         * @return the squared distance between this and the other vector */
        public float distanceSquared(float x, float y)
        {
            float x_d = x - this.x;
            float y_d = y - this.y;
            return x_d * x_d + y_d * y_d;
        }


        public Vector2 scl(float scalar)
        {
            x *= scalar;
            y *= scalar;
            return this;
        }

        public Vector2 scl(float x, float y)
        {
            this.x *= x;
            this.y *= y;
            return this;
        }

        public Vector2 scl(Vector2 v)
        {
            x *= v.x;
            y *= v.y;
            return this;
        }

        public Vector2 mulAdd(Vector2 vec, float scalar)
        {
            x += vec.x * scalar;
            y += vec.y * scalar;
            return this;
        }

        public Vector2 mulAdd(Vector2 vec, Vector2 mulVec)
        {
            x += vec.x * mulVec.x;
            y += vec.y * mulVec.y;
            return this;
        }

        public static float len(float x, float y)
        {
            return (float)Math.Sqrt(x * x + y * y);
        }

        public float len()
        {
            return (float)Math.Sqrt(x * x + y * y);
        }

        public static float len2(float x, float y)
        {
            return x * x + y * y;
        }

        public float len2()
        {
            return x * x + y * y;
        }

        public bool isZero()
        {
            return x == 0 && y == 0;
        }

        public bool isZero(float margin)
        {
            return len2() < margin;
        }

        public bool isOnLine(Vector2 other)
        {
            return Maths.isZero(x * other.y - y * other.x);
        }

        public bool isOnLine(Vector2 other, float epsilon)
        {
            return Maths.isZero(x * other.y - y * other.x, epsilon);
        }

        public bool isCollinear(Vector2 other, float epsilon)
        {
            return isOnLine(other, epsilon) && dot(other) > 0f;
        }

        public bool isCollinear(Vector2 other)
        {
            return isOnLine(other) && dot(other) > 0f;
        }

        public bool isCollinearOpposite(Vector2 other, float epsilon)
        {
            return isOnLine(other, epsilon) && dot(other) < 0f;
        }

        public bool isCollinearOpposite(Vector2 other)
        {
            return isOnLine(other) && dot(other) < 0f;
        }

        public bool isPerpendicular(Vector2 vector)
        {
            return Maths.isZero(dot(vector));
        }

        public bool isPerpendicular(Vector2 vector, float epsilon)
        {
            return Maths.isZero(dot(vector), epsilon);
        }

        public bool hasSameDirection(Vector2 vector)
        {
            return dot(vector) > 0;
        }

        public bool hasOppositeDirection(Vector2 vector)
        {
            return dot(vector) < 0;
        }

        public Vector2 setZero()
        {
            x = 0;
            y = 0;
            return this;
        }

        public Vector2 cpy()
        {
            return new Vector2(x, y);
        }

        public Vector2 limit(float limit)
        {
            return limit2(limit * limit);
        }

        public Vector2 limit2(float limit2)
        {
            float length2 = len2();
            if (length2 > limit2)
            {
                return scl((float)Math.Sqrt(limit2 / length2));
            }
            return this;
        }

        public Vector2 setLength(float len)
        {
            return setLength2(len * len);
        }

        public Vector2 setLength2(float len2)
        {
            float oldLen2 = this.len2();
            return (oldLen2 == 0 || oldLen2 == len2) ? this : scl((float)Math.Sqrt(len2 / oldLen2));
        }

        public Vector2 clamp(float min, float max)
        {
            float len2 = this.len2();
            if (len2 == 0f) return this;
            float max2 = max * max;
            if (len2 > max2) return scl((float)Math.Sqrt(max2 / len2));
            float min2 = min * min;
            if (len2 < min2) return scl((float)Math.Sqrt(min2 / len2));
            return this;
        }

        public Vector2 set(Vector2 v)
        {
            x = v.x;
            y = v.y;
            return this;
        }

        public static float dst(float x1, float y1, float x2, float y2)
        {
            float x_d = x2 - x1;
            float y_d = y2 - y1;
            return (float)Math.Sqrt(x_d * x_d + y_d * y_d);
        }

        public float dst(Vector2 v)
        {
            float x_d = v.x - x;
            float y_d = v.y - y;
            return (float)Math.Sqrt(x_d * x_d + y_d * y_d);
        }

        /** @param x The x-component of the other vector
         * @param y The y-component of the other vector
         * @return the distance between this and the other vector */
        public float dst(float x, float y)
        {
            float x_d = x - this.x;
            float y_d = y - this.y;
            return (float)Math.Sqrt(x_d * x_d + y_d * y_d);
        }

        public static float dst2(float x1, float y1, float x2, float y2)
        {
            float x_d = x2 - x1;
            float y_d = y2 - y1;
            return x_d * x_d + y_d * y_d;
        }

        public float dst2(Vector2 v)
        {
            float x_d = v.x - x;
            float y_d = v.y - y;
            return x_d * x_d + y_d * y_d;
        }

        /** @param x The x-component of the other vector
         * @param y The y-component of the other vector
         * @return the squared distance between this and the other vector */
        public float dst2(float x, float y)
        {
            float x_d = x - this.x;
            float y_d = y - this.y;
            return x_d * x_d + y_d * y_d;
        }

        public Vector2 lerp(Vector2 target, float alpha)
        {
            float invAlpha = 1.0f - alpha;
            x = (x * invAlpha) + (target.x * alpha);
            y = (y * invAlpha) + (target.y * alpha);
            return this;
        }

        public Vector2 interpolate(Vector2 target, float alpha, Interpolation interpolator)
        {
            return lerp(target, interpolator.apply(alpha));
        }

        public Vector2 setToRandomDirection()
        {
            float theta = Maths.randomFloat(0f, Maths.PI2);
            return set((float)Math.Cos(theta), (float)Math.Sin(theta));
        }

        public bool isUnit()
        {
            return isUnit(0.000000001f);
        }

        public bool isUnit(float margin)
        {
            return Math.Abs(len2() - 1f) < margin;
        }

        public bool epsilonEquals(Vector2 other, float epsilon)
        {
            if (other == null) return false;
            if (Math.Abs(other.x - x) > epsilon) return false;
            if (Math.Abs(other.y - y) > epsilon) return false;
            return true;
        }
    }
}