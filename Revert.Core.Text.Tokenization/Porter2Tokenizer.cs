using Revert.Core.Text.Tokenization.Porter2;

namespace Revert.Core.Text.Tokenization
{
    public class Porter2Tokenizer : SimpleTokenizer
    {
        protected override string CleanToken(string token)
        {
            var word = new EnglishWord(token);
            return word.Stem;
        }
    }
}
