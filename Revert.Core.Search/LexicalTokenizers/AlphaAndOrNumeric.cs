namespace Revert.Core.Search
{
    public class AlphaAndOrNumeric : LexicalTokenizer
    {
        // An AlphaNumeric is a string of letters(A-Z) (case-INsensitive), and digits(0-9) 
        public static readonly AlphaAndOrNumeric Parser = new AlphaAndOrNumeric();

        private AlphaAndOrNumeric() : base() { }

        public override LexicalTokenizer TryParse(string inputStr, int position)
        {
            bool alpha = false;
            bool numeric = false;
            bool decimalPoint = false;
            int wild = 0;

            int i = 0;
            for (i = position; i < inputStr.Length; i++)
            {
                char c = inputStr[i];
                if (char.IsLetter(c))
                {
                    if (decimalPoint)
                    {
                        if (numeric && !alpha && WildCardToken.ValidWild(inputStr.Substring(position, i - position), position, i - 1, wild))
                        {
                            //A decimal number like: 3.4
                            return new Numeric(inputStr.Substring(position, i - position), position, i - 1, wild);
                        }
                        else
                        {
                            return null; //Found a character after a decimal like: .A
                        }
                    }
                    alpha = true;
                }
                else if (char.IsDigit(c)) //IsNumber would trigger on roman numerals and fractions etc, see MSDN for details
                {
                    numeric = true;
                }
                else if (c == '*')
                {
                    wild++;
                }
                else
                {
                    break;
                }
            }

            int endPosi = i - 1;
            string tokenFound = inputStr.Substring(position, i - position);

            //Not something handlable, so lets make what we have with what we have
            if (alpha)
            {
                if (numeric && WildCardToken.ValidWild(tokenFound, position, endPosi, wild))
                {
                    return new AlphaNumeric(tokenFound, position, endPosi, wild);
                }
                else
                {
                    return CheckForAlphaKeywords(tokenFound, position, endPosi) ?? new Alpha(tokenFound, position, endPosi, wild);
                }
            }
            else
            {
                if (numeric && WildCardToken.ValidWild(tokenFound, position, endPosi, wild))
                {
                    return new Numeric(tokenFound, position, endPosi, wild);
                }
                else
                {
                    if ((wild != 0) && WildCardToken.ValidWild(tokenFound, position, endPosi, wild))
                    {
                        return new Alpha(tokenFound, position, endPosi, wild);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private static LexicalTokenizer CheckForAlphaKeywords(string matchStr, int position, int endPosition)
        {
            if (string.Compare(matchStr, "OR", true) == 0)
            {
                return new OR("OR", position, endPosition);
            }

            if (string.Compare(matchStr, "AND", true) == 0)
            {
                return new AND("AND", position, endPosition);
            }

            if (string.Compare(matchStr, "NOT", true) == 0)
            {
                return new NOT("NOT", position, endPosition);
            }

            return null;
        }
    }
}
