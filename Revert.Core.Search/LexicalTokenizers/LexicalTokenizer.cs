using System;
using System.Collections.Generic;

namespace Revert.Core.Search
{
    public abstract class LexicalTokenizer
    {
        private static System.Threading.ReaderWriterLockSlim rwLock = new System.Threading.ReaderWriterLockSlim(System.Threading.LockRecursionPolicy.SupportsRecursion);
        private const int LOCKTIMEOUT = 2000;

        public string Token;
        public int Begin = 0;
        public int End = 0;
        private string CSSClass = null;
        public virtual string HTMLEncodedDisplayText
        {
            get { return String.Format("<span class='{0}'>{1}&nbsp;</span>", CSSClass, Token); }
        }

        protected LexicalTokenizer() { }

        protected LexicalTokenizer(string token, int begin, int end, string cssClass)
        {
            Token = token;
            Begin = begin;
            End = end;
            CSSClass = cssClass;
        }

        public virtual LexicalTokenizer TryParse(string inputStr, int position)
        {
            return null;
        }

        public static void GetTokens(string consoleInput, ref List<LexicalTokenizer> tokens, ref List<LexicalTokenizer> illegalCharacters)
        {
            List<LexicalTokenizer> parsers = new List<LexicalTokenizer>();
            InitializeParsers(ref parsers);

            int position = 0;
            while (position < consoleInput.Length)
            {
                foreach (LexicalTokenizer parser in parsers)
                {
                    LexicalTokenizer token = parser.TryParse(consoleInput, position);
                    if (token == null) continue;

                    // Skip spaces
                    if (token is Space)
                    {
                        position += token.Token.Length;
                        break;
                    }

                    // Check for an illegal character
                    if (token is Illegal)
                    {
                        illegalCharacters.Add(token);
                        position += token.Token.Length;
                        break;
                    }

                    // Add token to the Tokens list
                    tokens.Add(token);

                    position += token.Token.Length;

                    /*
                     * QuotedString is special because we skipped over the outer quotes, AND, 
                     * if they exist, the delimiters for inner quotes
                     */
                    if (token is QuotedString)
                    {
                        QuotedString quotedStringToken = (QuotedString)token;
                        position += quotedStringToken.SkippedChars;
                    }
                    break;
                }
            }
        }

        private static void InitializeParsers(ref List<LexicalTokenizer> Parsers)
        {
            Parsers.Add(Space.Parser);
            //Terms
            Parsers.Add(AlphaAndOrNumeric.Parser);

            //Special characters
            Parsers.Add(Wildcard.Parser);
            Parsers.Add(Dot.Parser);
            Parsers.Add(LeftParenthesis.Parser);
            Parsers.Add(RightParenthesis.Parser);
            Parsers.Add(Colon.Parser);
            Parsers.Add(Comma.Parser);
            Parsers.Add(QuotedString.Parser);
            Parsers.Add(Illegal.Parser);
        }
    }
}
