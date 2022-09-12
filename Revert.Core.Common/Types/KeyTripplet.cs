namespace Revert.Core.Common.Types
{
    public struct KeyTripplet<TKeyOne, TKeyTwo, TKeyThree>
    {
        private const int KeyDistributor = 16777619;
        private int hashCode;
        public TKeyOne KeyOne;
        public TKeyTwo KeyTwo;
        public TKeyThree KeyThree;

        public KeyTripplet(TKeyOne keyOne, TKeyTwo keyTwo, TKeyThree keyThree)
        {
            KeyOne = keyOne;
            KeyTwo = keyTwo;
            KeyThree = keyThree;
            hashCode = (((((keyOne.GetHashCode() * KeyDistributor) << 16) 
                         ^ (keyTwo.GetHashCode() * KeyDistributor)) 
                         << 16) ^ (keyThree.GetHashCode() * KeyDistributor));
        }

        public override bool Equals(object obj)
        {
            var keyTripple = (KeyTripplet<TKeyOne, TKeyTwo, TKeyThree>)obj;
            return (KeyOne.Equals(keyTripple.KeyOne) && KeyTwo.Equals(keyTripple.KeyTwo) && KeyThree.Equals(keyTripple.KeyThree));
        }

        public override int GetHashCode()
        {
            return hashCode;
        }
    }
}
