using System.Collections.Generic;
using System.Globalization;

namespace Revert.Core.Text.Tokenization
{
    public class SimpleTokenizer : ITokenizer
    {
        protected virtual bool IsTokenValid(string token)
        {
            return true;
        }

        protected virtual string CleanToken(string token)
        {
            return token.ToLowerInvariant();
        }

        private readonly HashSet<char> illegalCharacters = new HashSet<char>
        {
            '_', '-', '=', ' ', '"', '(', '{', '[', ']', '}', ')', '<', '>'
        };

        private bool GenerateToken(Stack<char> tokenCharacters, out string token)
        {
            while (tokenCharacters.Count > 0)
            {
                var character = tokenCharacters.Peek();
                if (!char.IsLetterOrDigit(character) && !char.IsSeparator(character)) tokenCharacters.Pop();
                else break;
            }
            
            var characterArray = tokenCharacters.ToArray();
            tokenCharacters.Clear();
            foreach (var c in characterArray) tokenCharacters.Push(c);

            while (tokenCharacters.Count > 0)
            {
                var character = tokenCharacters.Peek();
                if (char.IsLetterOrDigit(character)
                    || char.IsSeparator(character)
                    || char.IsSurrogate(character)
                    || char.IsSymbol(character)
                    || char.IsControl(character)
                    || (char.IsPunctuation(character) || character == '\'')
                    || illegalCharacters.Contains(character))
                    break;

                tokenCharacters.Pop();
            }

            if (tokenCharacters.Count == 0)
            {
                token = string.Empty;
                return false;
            }

            token = CleanToken(new string(tokenCharacters.ToArray()));
            return IsTokenValid(token);
        }

        public virtual List<string> GetTokens(string inputString)
        {
            var index = 0;
            var tokenCharacters = new Stack<char>();
            var tokens = new List<string>();
            string token;
            while (index < inputString.Length)
            {
                var character = inputString[index];
                var characterUnicodeCategory = char.GetUnicodeCategory(character);

                if (!char.IsLetterOrDigit(character) && (character == ' ' || 
                    characterUnicodeCategory == UnicodeCategory.Control ||
                    characterUnicodeCategory == UnicodeCategory.OpenPunctuation ||
                    characterUnicodeCategory == UnicodeCategory.ClosePunctuation ||
                    characterUnicodeCategory == UnicodeCategory.OtherPunctuation ||
                    char.IsControl(character) || illegalCharacters.Contains(character)))
                {
                    if (GenerateToken(tokenCharacters, out token))
                        tokens.Add(token);
                    tokenCharacters.Clear();
                }
                else
                    tokenCharacters.Push(character);
                index++;
            }

            if (tokenCharacters.Count != 0 && GenerateToken(tokenCharacters, out token)) tokens.Add(token);
            return tokens;
        }

        public Dictionary<int, string> GetTokensWithIndex(string inputString)
        {
            var index = 0;
            var tokenCharacters = new Stack<char>();
            var tokens = new Dictionary<int, string>();

            string token;
            while (index < inputString.Length)
            {
                var character = inputString[index];
                var characterUnicodeCategory = char.GetUnicodeCategory(character);
                
                if (character == ' ' || characterUnicodeCategory == UnicodeCategory.Control || characterUnicodeCategory == UnicodeCategory.ClosePunctuation || characterUnicodeCategory == UnicodeCategory.OtherPunctuation)
                {
                    if (GenerateToken(tokenCharacters, out token))
                    {
                        tokens.Add(index, token);
                    }
                    tokenCharacters.Clear();
                }
                else
                    tokenCharacters.Push(character);

                index++;
            }
            if (tokenCharacters.Count != 0 && GenerateToken(tokenCharacters, out token)) tokens.Add(index, token);

            return tokens;
        }
    }
}
