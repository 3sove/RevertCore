using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revert.Core.Search
{
    public class TextSearchable : ISearchable<string, string>
    {
        public IEnumerable<string> AllKeys => throw new NotImplementedException();

        public IEnumerable<string> AllValues => throw new NotImplementedException();

        public string GetKeyFromString(string value)
        {
            throw new NotImplementedException();
        }

        public string[] GetKeys(string vertexId)
        {
            throw new NotImplementedException();
        }

        public List<string> GetKeysFromLeadingWildCard(string postWildCardValue)
        {
            throw new NotImplementedException();
        }

        public string[] GetKeysFromString(string value)
        {
            throw new NotImplementedException();
        }

        public List<string> GetKeysFromTrailingWildCard(string preWildCardValue)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetLiteralMatches(string[] keyVector)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetLiteralMatches(string literalString)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetMatches(string key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetMatches(string[] keyVector, bool intersect = true)
        {
            throw new NotImplementedException();
        }
    }
}
