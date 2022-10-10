using Revert.Core.Common.Factories;
using Revert.Port.LibGDX.Mathematics.Vectors;

namespace Revert.Port.LibGDX.Mathematics.Factories
{
    public class Vec2Factory : Factory<Vector2, Vec2Factory>
    {
        protected override int initialCapacity { get => 500000; }

        protected override Vector2 getNewItem()
        {
            return new Vector2();
        }

        public Vector2 get(float x = 0f, float y = 0f)
        {
            var point = get();
            point.x = x;
            point.y = y;
            return point;
        }

        public Vector2 get(Vector2 vector)
        {
            var point = get();
            point.x = vector.x;
            point.y = vector.y;
            return point;
        }


        public override void dispose(Vector2 item)
        {
            item.set(0f, 0f);
            base.dispose(item);
        }

        private static Vector2 empty = new Vector2(0f, 0f);

        public static Vector2 getEmpty()
        {
            empty.x = 0f;
            empty.y = 0f;
            return empty;
        }
    }
}
