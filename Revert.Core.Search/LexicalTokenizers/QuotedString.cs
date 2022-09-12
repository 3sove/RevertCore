using System.Text;

namespace Revert.Core.Search
{
    public class QuotedString : WildCardToken
    {
        public const char ELEMENT = '"';
        public const char INNER_QUOTE_DELIMETER = '\\';
        public override string HTMLEncodedDisplayText
        {
            get { return "<span class='quotedStringToken'>&nbsp;&quot;" + Token + "&quot;&nbsp;</span>"; }
        }

        public int SkippedChars;

        public static readonly QuotedString Parser = new QuotedString();

        private QuotedString() : base() { }
        public QuotedString(string token, int skippedChars, int begin, int end)
            : base(token, begin, end, "quotedStringToken", 0)
        {
            SkippedChars = skippedChars;
        }

        public override LexicalTokenizer TryParse(string inputStr, int position)
        {
            //Check that the first character is a qoute.  If not, go no further. If it IS a quote, search for the end quote.  If no end quote, it is an error.  
            //If an end quote is found, gather up all of the characters between the quotes and return them in a string in a new instance of the QuotedString class
            if (inputStr[position] != ELEMENT) return null;

            string inputSubStr = inputStr.Substring(position);
            int subStrLen = inputSubStr.Length;

            bool innerQuoteStarted = false;
            int skippedChars = 0;

            StringBuilder sb = new StringBuilder();

            //Copy each character in the quoted string to a new string.
            //If a '\' is encountered, it could be followed by a quote and in that case, do not copy the '\'.  Also, look for the end quote.

            skippedChars++; //Skip first quote
            int i = 1;
            while (i < subStrLen)
            {
                //First check each character for a '\'
                if (inputSubStr[i] == INNER_QUOTE_DELIMETER)
                {
                    if (inputSubStr[i + 1] == ELEMENT)
                    {
                        innerQuoteStarted = !innerQuoteStarted;
                        i++;
                        skippedChars++; //Skip backslash
                    }

                    //There was a backslash, but no quote following it so don't skip the backslash
                    sb.Append(inputSubStr[i++]);
                    continue;
                }

                if (inputSubStr[i] == ELEMENT)
                {
                    skippedChars++; //Skip closing quote
                    break;
                }
                else
                {
                    sb.Append(inputSubStr[i++]);
                }
            }

            //If a closing quote was not specified, then the whole string is the quoted string.
            //If an inner quote was specified, but was never closed, then we're adding an inner closing quote to the end of the string.
            if (innerQuoteStarted) sb.Append('"');
            string quotedString = sb.ToString();
            return new QuotedString(quotedString, skippedChars, position, position + quotedString.Length - 1);
        }
    }
}
