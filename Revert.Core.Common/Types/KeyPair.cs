using System.Collections.Generic;

namespace Revert.Core.Common.Types
{
    public interface IKeyPair<TKeyOne, TKeyTwo>
    {
        TKeyOne KeyOne { get; set; }
        TKeyTwo KeyTwo { get; set; }
    }

    public class KeyPair<TKeyOne, TKeyTwo> : IKeyPair<TKeyOne, TKeyTwo>
    {
        public TKeyOne KeyOne { get; set; }
        public TKeyTwo KeyTwo { get; set; }

        public KeyPair()
        {
        }

        public KeyPair(TKeyOne keyOne, TKeyTwo keyTwo)
        {
            KeyOne = keyOne;
            KeyTwo = keyTwo;
        }

        public override int GetHashCode()
        {
            return KeyOne.GetHashCode() ^ KeyTwo.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var pair = obj as KeyPair<TKeyOne, TKeyTwo>;
            if (pair == null) return false;
            return Equals(KeyOne, pair.KeyOne) && Equals(KeyTwo, pair.KeyTwo);
        }
    }
}
