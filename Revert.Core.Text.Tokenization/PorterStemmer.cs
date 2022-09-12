using Revert.Core.Common.Modules;
using System;

namespace Revert.Core.Text.Tokenization
{
    /// <summary>
    /// Stemmer, implementing the Porter Stemming algorithm
    /// </summary>
    public class PorterStemmer : FunctionalModule<PorterStemmer>
    {
        private int currentPosition,
            // offset into _wordToStem
            // offset to end of stemmed word
            j,
            positionEndOfString;

        private const int Growth = 32;

        public int Length { get; private set; }
        public char[] Buffer { get; private set; }

        public PorterStemmer()
        {
            Buffer = new char[Growth];
            currentPosition = 0;
            Length = 0;
        }

        /// <summary>
        /// Add a character to the word.
        /// </summary>
        public void Add(char character)
        {
            if (currentPosition == Buffer.Length)
            {
                var newBuffer = new char[Buffer.Length + Growth];
                Array.Copy(Buffer, newBuffer, Buffer.Length);
                Buffer = newBuffer;
            }
            Buffer[currentPosition++] = character;
        }

        public override string ToString()
        {
            return new string(Buffer, 0, Length);
        }

        private bool isConsonant(int i)
        {
            switch (Buffer[i])
            {
                case 'a':
                case 'e':
                case 'i':
                case 'o':
                case 'u':
                    return false;
                case 'y':
                    return (i == 0) || !isConsonant(i - 1);
                default:
                    return true;
            }
        }

        /// <summary>
        /// Measures the number of consonant sequences between 0 and j.  If c is a consonant
        /// sequence and v a vowel sequence, and <..> indicates arbitrary presence,
        /// 
        ///		<c><v>			gives 0
        ///		<c>vc<v>		gives 1
        ///		<c>vCvc<v>		gives 2
        ///		<c>vCvcvc<v>	gives 3
        ///		....
        ///		
        /// </summary>
        private int CountConsonantSequences()
        {
            var n = 0;
            var i = 0;
            while (true)
            {
                if (i > j)
                    return n;
                if (!isConsonant(i))
                    break;
                i++;
            }
            i++;
            while (true)
            {
                while (true)
                {
                    if (i > j)
                    {
                        return n;
                    }
                    if (isConsonant(i))
                    {
                        break;
                    }
                    i++;
                }
                i++;
                n++;
                while (true)
                {
                    if (i > j)
                    {
                        return n;
                    }
                    if (isConsonant(i))
                    {
                        break;
                    }
                    i++;
                }
                i++;
            }
        }

        /// <summary>
        /// isVowelInStem() is true <=> 0,...j contains a vowel
        /// </summary>
        /// <returns></returns>
        private bool isVowelInStem()
        {
            int i;
            for (i = 0; i <= j; i++)
                if (!isConsonant(i)) return true;

            return false;
        }

        /// <summary>
        /// Returns true if the buffer contain a double matching consonant at j and j-1 
        /// </summary>
        private bool containsDoubleConsonant(int j)
        {
            if (j < 1)
                return false;
            if (Buffer[j] != Buffer[j - 1])
                return false;
            return isConsonant(j);
        }

        /// <summary>
        /// Cvc(currentPosition) is true <=> currentPosition-2,currentPosition-1,currentPosition has the form consonant - vowel - consonant
        /// and also if the second c is not w, x or y.this is used when trying to restore an e at the end of a short word. e.g.
        /// cav(e), lov(e), hop(e), crim(e), but snow, box, tray.
        /// </summary>
        private bool Cvc(int i)
        {
            if (i < 2 || !isConsonant(i) || isConsonant(i - 1) || !isConsonant(i - 2))
            {
                return false;
            }
            int ch = Buffer[i];
            if (ch == 'w' || ch == 'x' || ch == 'y')
            {
                return false;
            }
            return true;
        }

        private bool End(string s)
        {
            var l = s.Length;
            var o = positionEndOfString - l + 1;
            if (o < 0)
            {
                return false;
            }

            for (var i = 0; i < l; i++)
            {
                if (Buffer[o + i] != s[i])
                {
                    return false;
                }
            }
            j = positionEndOfString - l;
            return true;
        }

        /* setto(s) sets (j+1),...positionEndOfString to the characters in the string s, readjusting
           positionEndOfString. */

        private void SetTo(string s)
        {
            var o = j + 1;
            for (var i = 0; i < s.Length; i++)
                Buffer[o + i] = s[i];
            positionEndOfString = j + s.Length;
        }

        private void R(string s)
        {
            if (CountConsonantSequences() > 0)
                SetTo(s);
        }

        /// <summary>
        /// Removes plurals and -ed or -ing. e.g. 
        /// caresses  ->  caress
        /// ponies    ->  poni
        /// ties      ->  ti
        /// caress    ->  caress
        /// cats      ->  cat
        /// feed      ->  feed
        /// agreed    ->  agree
        /// disabled  ->  disable
        /// matting   ->  mat
        /// mating    ->  mate
        /// meeting   ->  meet
        /// milling   ->  mill
        /// messing   ->  mess
        /// meetings  ->  meet
        /// </summary>
        private void Step1()
        {
            if (Buffer[positionEndOfString] == 's')
            {
                if (End("sses"))
                    positionEndOfString -= 2;
                else if (End("ies"))
                    SetTo("currentPosition");
                else if (Buffer[positionEndOfString - 1] != 's')
                    positionEndOfString--;
            }
            if (End("eed"))
            {
                if (CountConsonantSequences() > 0)
                    positionEndOfString--;
            }
            else if ((End("ed") || End("ing")) && isVowelInStem())
            {
                positionEndOfString = j;
                if (End("at"))
                    SetTo("ate");
                else if (End("bl"))
                    SetTo("ble");
                else if (End("iz"))
                    SetTo("ize");
                else if (containsDoubleConsonant(positionEndOfString))
                {
                    positionEndOfString--;
                    {
                        int ch = Buffer[positionEndOfString];
                        if (ch == 'l' || ch == 's' || ch == 'z')
                            positionEndOfString++;
                    }
                }
                else if (CountConsonantSequences() == 1 && Cvc(positionEndOfString))
                    SetTo("e");
            }
        }

        /// <summary>
        /// Turns terminal y to currentPosition when there is another vowel in the stem.
        /// </summary>
        private void Step2()
        {
            if (End("y") && isVowelInStem())
                Buffer[positionEndOfString] = 'i';
        }

        /// <summary>
        /// Maps double suffices to single ones. so -ization ( = -ize plus -ation) maps to -ize etc.
        /// Note that the string before the suffix must give m() > 0.
        /// </summary>
        private void Step3()
        {
            if (positionEndOfString == 0)
                return; /* For Bug 1 */
            switch (Buffer[positionEndOfString - 1])
            {
                case 'a':
                    if (End("ational"))
                    {
                        R("ate");
                        break;
                    }
                    if (End("tional"))
                    {
                        R("tion");
                    }
                    break;
                case 'c':
                    if (End("enci"))
                    {
                        R("ence");
                        break;
                    }
                    if (End("anci"))
                    {
                        R("ance");
                    }
                    break;
                case 'e':
                    if (End("izer"))
                    {
                        R("ize");
                    }
                    break;
                case 'l':
                    if (End("bli"))
                    {
                        R("ble");
                        break;
                    }
                    if (End("alli"))
                    {
                        R("al");
                        break;
                    }
                    if (End("entli"))
                    {
                        R("ent");
                        break;
                    }
                    if (End("eli"))
                    {
                        R("e");
                        break;
                    }
                    if (End("ousli"))
                    {
                        R("ous");
                    }
                    break;
                case 'o':
                    if (End("ization"))
                    {
                        R("ize");
                        break;
                    }
                    if (End("ation"))
                    {
                        R("ate");
                        break;
                    }
                    if (End("ator"))
                    {
                        R("ate");
                    }
                    break;
                case 's':
                    if (End("alism"))
                    {
                        R("al");
                        break;
                    }
                    if (End("iveness"))
                    {
                        R("ive");
                        break;
                    }
                    if (End("fulness"))
                    {
                        R("ful");
                        break;
                    }
                    if (End("ousness"))
                    {
                        R("ous");
                    }
                    break;
                case 't':
                    if (End("aliti"))
                    {
                        R("al");
                        break;
                    }
                    if (End("iviti"))
                    {
                        R("ive");
                        break;
                    }
                    if (End("biliti"))
                    {
                        R("ble");
                    }
                    break;
                case 'g':
                    if (End("logi"))
                    {
                        R("log");
                    }
                    break;
            }
        }

        /// <summary>
        /// Deals with -ic-, -full, -ness etc. similar strategy to Step3.
        /// </summary>
        private void Step4()
        {
            switch (Buffer[positionEndOfString])
            {
                case 'e':
                    if (End("icate"))
                    {
                        R("ic");
                        break;
                    }
                    if (End("ative"))
                    {
                        R("");
                        break;
                    }
                    if (End("alize"))
                    {
                        R("al");
                    }
                    break;
                case 'i':
                    if (End("iciti"))
                    {
                        R("ic");
                    }
                    break;
                case 'l':
                    if (End("ical"))
                    {
                        R("ic");
                        break;
                    }
                    if (End("ful"))
                    {
                        R("");
                    }
                    break;
                case 's':
                    if (End("ness"))
                    {
                        R("");
                    }
                    break;
            }
        }

        /// <summary>
        /// Takes off -ant, -ence etc., in context <c>vCvc<v>.
        /// </summary>
        private void Step5()
        {
            if (positionEndOfString == 0)
                return; /* for Bug 1 */
            switch (Buffer[positionEndOfString - 1])
            {
                case 'a':
                    if (End("al"))
                        break;
                    return;
                case 'c':
                    if (End("ance"))
                        break;
                    if (End("ence"))
                        break;
                    return;
                case 'e':
                    if (End("er"))
                        break;
                    return;
                case 'i':
                    if (End("ic"))
                        break;
                    return;
                case 'l':
                    if (End("able"))
                        break;
                    if (End("ible"))
                        break;
                    return;
                case 'n':
                    if (End("ant"))
                        break;
                    if (End("ement"))
                        break;
                    if (End("ment"))
                        break;
                    /* element etc. not stripped before the m */
                    if (End("ent"))
                        break;
                    return;
                case 'o':
                    if (End("ion") && j >= 0 && (Buffer[j] == 's' || Buffer[j] == 't'))
                        break;
                    /* j >= 0 fixes Bug 2 */
                    if (End("ou"))
                        break;
                    return;
                    /* takes care of -ous */
                case 's':
                    if (End("ism"))
                        break;
                    return;
                case 't':
                    if (End("ate"))
                        break;
                    if (End("iti"))
                        break;
                    return;
                case 'u':
                    if (End("ous"))
                        break;
                    return;
                case 'v':
                    if (End("ive"))
                        break;
                    return;
                case 'z':
                    if (End("ize"))
                        break;
                    return;
                default:
                    return;
            }
            if (CountConsonantSequences() > 1)
                positionEndOfString = j;
        }

        /// <summary>
        /// Removes a -e if m() > 1.
        /// </summary>
        private void Step6()
        {
            j = positionEndOfString;
            if (Buffer[positionEndOfString] == 'e')
            {
                var a = CountConsonantSequences();
                if (a > 1 || a == 1 && !Cvc(positionEndOfString - 1))
                    positionEndOfString--;
            }
            if (Buffer[positionEndOfString] == 'l' && containsDoubleConsonant(positionEndOfString) && CountConsonantSequences() > 1)
                positionEndOfString--;
        }

        /// <summary>
        /// Stem the word placed into the Stemmer buffer through calls to add(). 
        /// Returns true if the stemming process resulted in a word different 
        /// from the input.You can retrieve the result with getResultLength()/getResultBuffer() or tostring().
        /// </summary>
        public void Stem()
        {
            positionEndOfString = currentPosition - 1;
            if (positionEndOfString > 1)
            {
                Step1();
                Step2();
                Step3();
                Step4();
                Step5();
                Step6();
            }
            Length = positionEndOfString + 1;
            currentPosition = 0;
        }

        /// <summary>
        /// Adds the parameter 'word' to the stem buffer, then calls Stem()
        /// </summary>
        /// <param name="word">Word to be stemmed</param>
        /// <returns>Stemmed word</returns>
        public string Stem(string word)
        {
            foreach (var c in word) Add(c);
            Stem();
            return new string(Buffer, 0, Length);
        }
    }
}