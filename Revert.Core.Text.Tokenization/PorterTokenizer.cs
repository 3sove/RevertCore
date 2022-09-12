namespace Revert.Core.Text.Tokenization
{
    public class PorterTokenizer : SimpleTokenizer
    {
        protected override string CleanToken(string token)
        {
            return PorterStemmer.Instance.Stem(token).ToUpper();
            //return token.ToUpper();
        }
    }
}
