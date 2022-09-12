namespace Revert.Core.Common
{
    public struct MatchedCoordinate
    {
        public int Beginning;
        public int Ending;

        public MatchedCoordinate(int beginning, int ending)
        {
            Beginning = beginning;
            Ending = ending;
        }
    }
}
