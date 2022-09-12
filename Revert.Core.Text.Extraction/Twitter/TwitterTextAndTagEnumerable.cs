using System;
using System.Collections.Generic;
using System.Linq;
using Revert.Core.Extensions;
using Revert.Core.IO.Files;
using Revert.Core.IO.Serialization.Twitter;

namespace Revert.Core.Text.Extraction.Twitter
{
    public class TwitterTextAndTagEnumeratorLite : IEnumerator<Tuple<string, IEnumerable<string>>>
    {
        private GenericFileEnumerator genericFileEnumerator;
        public TwitterTextAndTagEnumeratorLite(string twitterFilePath)
        {
            genericFileEnumerator = new GenericFileEnumerator(new GenericFileEnumeratorModel
            {
                FilePath = twitterFilePath
            });
        }

        public Tuple<string, IEnumerable<string>> Current
        {
            get
            {
                var tweet = genericFileEnumerator.Current;

                //"hashtags":[{"text":"NY","indices":[25,28]},{"text":"people","indices":[29,36]}]
                const string startHashTagText = "\"hashtags\":[";
                var startHashTagIndex = tweet.IndexOf(startHashTagText) + startHashTagText.Length;
                var endHashTagIndex = tweet.IndexOf("]", startHashTagIndex);
                if (endHashTagIndex == -1) endHashTagIndex = tweet.IndexOf("]}", startHashTagIndex);

                var hashTagsRawText = tweet.Substring(startHashTagIndex, endHashTagIndex - startHashTagIndex);

                var hashTags = new List<string>();
                if (hashTagsRawText.Trim() != string.Empty)
                {
                    var hashTagStrings = hashTagsRawText.Contains("]},{") ? hashTagsRawText.Split(new[] { "]},{" }, StringSplitOptions.RemoveEmptyEntries).ToList()
                                                                          : new List<string> { hashTagsRawText };

                    const string textSearchString = "text\":\"";
                    foreach (var hashTagString in hashTagStrings)
                    {
                        var startTextIndex = hashTagString.IndexOf(textSearchString) + textSearchString.Length;
                        var endTextIndex = hashTagString.IndexOf("\",\"indices", startTextIndex);
                        var text = hashTagString.Substring(startTextIndex, endTextIndex - startTextIndex);
                        hashTags.Add(text);
                    }
                }

                var body = string.Format("{0} {1}",
                           tweet.GetBetween("\"summary\":\"", "\",\""),
                           tweet.GetBetween("\"body\":\"", "\",\""));

                return new Tuple<string, IEnumerable<string>>(body, hashTags);
            }
        }

        public void Dispose()
        {
            genericFileEnumerator.Dispose();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            return genericFileEnumerator.MoveNext();
        }

        public void Reset()
        {
            genericFileEnumerator.Reset();
        }
    }
    

    public class TwitterTextAndTagEnumerableLite : IEnumerable<Tuple<string, IEnumerable<string>>>
    {
        private IEnumerator<Tuple<string, IEnumerable<string>>> enumerator;

        public TwitterTextAndTagEnumerableLite(string filePath)
        {
            enumerator = new TwitterTextAndTagEnumeratorLite(filePath);
        }

        public IEnumerator<Tuple<string, IEnumerable<string>>> GetEnumerator()
        {
            return enumerator;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return enumerator;
        }
    }

    public class TwitterTextAndTagEnumerator : IEnumerator<Tuple<string, IEnumerable<string>>>
    {
        private TwitterEnumerator twitterEnumerator;

        public TwitterTextAndTagEnumerator(string twitterFilePath)
        {
            twitterEnumerator = new TwitterEnumerator(twitterFilePath, tweet => (tweet.twitter_entities.hashtags != null && tweet.twitter_entities.hashtags.Any()));
        }

        public Tuple<string, IEnumerable<string>> Current
        {
            get
            {
                var tweet = twitterEnumerator.Current;
                return new Tuple<string, IEnumerable<string>>(tweet.body, tweet.twitter_entities.hashtags.Select(hashTag => hashTag.text));
            }
        }

        public void Dispose()
        {
            twitterEnumerator.Dispose();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            return twitterEnumerator.MoveNext();
        }

        public void Reset()
        {
            twitterEnumerator.Reset();
        }
    }

    public class TwitterTextAndTagEnumerable : IEnumerable<Tuple<string, IEnumerable<string>>>
    {
        private IEnumerator<Tuple<string, IEnumerable<string>>> enumerator;

        public TwitterTextAndTagEnumerable(string filePath)
        {
            enumerator = new TwitterTextAndTagEnumerator(filePath);
        }

        public IEnumerator<Tuple<string, IEnumerable<string>>> GetEnumerator()
        {
            return enumerator;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return enumerator;
        }
    }
}
