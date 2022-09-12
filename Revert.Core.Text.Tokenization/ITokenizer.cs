
using System.Collections.Generic;

namespace Revert.Core.Text.Tokenization
{
    public interface ITokenizer
    {
        /// <summary>
        /// Split the string into tokens.
        /// </summary>
        /// <param name="inputString">String to be tokenized.</param>
        /// <returns>List of tokens in string form.</returns>
        List<string> GetTokens(string inputString);

        /// <summary>
        /// Split the string into tokens.
        /// </summary>
        /// <param name="inputString">String to be tokenized.</param>
        /// <returns>List of tokens in string form with the index from the original string where they were located</returns>
        Dictionary<int, string> GetTokensWithIndex(string inputString);
    }
}