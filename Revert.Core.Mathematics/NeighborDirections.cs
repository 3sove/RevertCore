namespace Revert.Core.Mathematics
{
    public static class NeighborDirections
    {
        public const int BOTTOM_LEFT = 0;
        public const int BOTTOM = 1;
        public const int BOTTOM_RIGHT = 2;
        public const int RIGHT = 3;
        public const int TOP_RIGHT = 4;
        public const int TOP = 5;
        public const int TOP_LEFT = 6;
        public const int LEFT = 7;
        public const int DIRECTION_COUNT = 8;

        public static int GetDirection(float fromX, float fromY, float toX, float toY)
        {
            if (toX == fromX)
            {
                if (toY == fromY) return DIRECTION_COUNT;
                if (toY > fromY) return TOP;
                else return BOTTOM;
            }

            if (toX > fromX)
            { //forward
                if (toY == fromY) return RIGHT;
                if (toY > fromY) return TOP_RIGHT;
                else return BOTTOM_RIGHT;
            }

            //backward
            if (toY == fromY) return LEFT;
            if (toY > fromY) return TOP_LEFT;
            return BOTTOM_LEFT;
        }

        public static int GetNextDirection(int direction)
        {
            return (direction + 1) % DIRECTION_COUNT;
        }

        public static int GetOppositeDirection(int direction)
        {
            return (direction + 4) % DIRECTION_COUNT;
        }

        public static int GetNeighborX(int x, int direction)
        {
            if (direction == LEFT || direction == BOTTOM_LEFT || direction == TOP_LEFT) return x - 1;
            else if (direction == RIGHT || direction == BOTTOM_RIGHT || direction == TOP_RIGHT) return x + 1;
            return x;
        }

        public static int GetNeighborY(int y, int direction)
        {
            if (direction == BOTTOM || direction == BOTTOM_LEFT || direction == BOTTOM_RIGHT) return y - 1;
            else if (direction == TOP || direction == TOP_LEFT || direction == TOP_RIGHT) return y + 1;
            return y;
        }
    }
}
