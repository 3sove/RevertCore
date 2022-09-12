//using System.Collections.Generic;
//using System.Linq;
//using Revert.Core.Common.Text;
//using Revert.Core.Common.Types.Tries;
//using Revert.Core.Extensions;
//using Revert.Core.Text.NLP.SentenceMaps;

//namespace Revert.Core.Text.NLP
//{
//    public class EntityExtractionTreeNode : TrieNode<Word, EntityExtractionTreeNode>
//    {
//        public SentenceToken Token { get; set; }

//        public EntityExtractionTreeNode Parent { get; set; }

//        //public IEnumerable<EntityExtractionTreeNode> Siblings => Parent.GetChildTreeNodes();

//        public bool TryGetChild(Word word, out TrieNode<Word, EntityExtractionTreeNode> node)
//        {
//            lock (Children)
//            {
//                return Children.TryGetValue(word, out node);
//            }
//        }

//        public void AddChild(Word word, TrieNode<Word, EntityExtractionTreeNode> node)
//        {
//            lock (Children)
//            {
//                Children[word] = node;
//            }
//        }
//        public List<TokenCategorizationMap> StartingCategorizationMaps { get; set; }

//        public Dictionary<string, int> MatchCountByCategory { get; set; }

//        public EntityExtractionTreeNode()
//        {
//            StartingCategorizationMaps = new List<TokenCategorizationMap>();
//            MatchCountByCategory = new Dictionary<string, int>();
//        }

//        public void Populate(AnnotatedSentenceMap map)
//        {
//            Populate(map, 0);
//        }

//        private void Populate(AnnotatedSentenceMap map, int position)
//        {
//            List<TokenCategorizationMap> tokenCategorizationMaps;
//            if (map.TokenCategorizationMapsByStartingPosition.TryGetValue(position++, out tokenCategorizationMaps))
//                foreach (var categorizationalMap in tokenCategorizationMaps)
//                    StartingCategorizationMaps.Add(categorizationalMap);

//            if (map.Tokens.Count == position) return;
//            var childNode = GetChildNode(map, position) as EntityExtractionTreeNode;
//            childNode?.Populate(map, position);

//            foreach (var categorizationMap in map.TokenCategorizationMaps)
//            {
//                lock (MatchCountByCategory)
//                {
//                    int matchCount;
//                    MatchCountByCategory.TryGetValue(categorizationMap.Category, out matchCount);
//                    MatchCountByCategory[categorizationMap.Category] = ++matchCount;
//                }
//            }
//        }

//        private TrieNode<Word, EntityExtractionTreeNode> GetChildNode(AnnotatedSentenceMap map, int position)
//        {
//            var sentenceToken = map.Tokens[position];

//            TrieNode<Word, EntityExtractionTreeNode> nextNode;
//            if (!TryGetChild(sentenceToken.Word, out nextNode))
//            {
//                nextNode = new EntityExtractionTreeNode { Token = sentenceToken, Parent = this };
//                AddChild(sentenceToken.Word, nextNode);
//            }
//            return nextNode;
//        }

//        public bool Evaluate(List<SentenceToken> tokens, out List<SentenceEvaluationMatch> matches)
//        {
//            var evaluationMatches = new EvaluationMatches();
//            var result = Evaluate(tokens, 0, ref evaluationMatches);
//            matches = evaluationMatches.CompleteMatches;
//            return result;
//        }

//        private bool Evaluate(List<SentenceToken> tokens, int position, ref EvaluationMatches evaluationMatches)
//        {
//            if (tokens.Count <= position) return evaluationMatches.CompleteMatches.Count > 0;
//            return EvaluatePartsOfSpeech(tokens, position, ref evaluationMatches);
//        }

//        private List<EntityExtractionTreeNode> posChildren = new List<EntityExtractionTreeNode>();
//        private HashSet<Word> posChildWords = new HashSet<Word>();
//        private int childCountAtPosPopulation = 0;
//        private bool EvaluatePartsOfSpeech(List<SentenceToken> tokens, int position, ref EvaluationMatches evaluationMatches)
//        {
//            var result = false;
//            var token = tokens[position];
//            if (posChildren.Count == 0 || Children.Count != childCountAtPosPopulation)
//            {
//                lock (Children)
//                {
//                    foreach (var child in Children)
//                    {
//                        if ((child.KeyOne.PartOfSpeech & token.Word.PartOfSpeech) != PartsOfSpeech.None && posChildWords.Add(child.KeyOne))
//                            result |= child.KeyTwo.TryEvaluate(tokens, position + 1,  ref evaluationMatches);
//                    }
//                    childCountAtPosPopulation = Children.Count;
//                }
//            }

//            EvaluateMatches(evaluationMatches, token);
//            return result;
//        }

//        private void EvaluateMatches(EvaluationMatches evaluationMatches, SentenceToken token)
//        {
//            if (evaluationMatches.PartialEvaluationMatchesByCurrentPosition.Count == 0) return;

//            lock (evaluationMatches.PartialEvaluationMatchesByCurrentPosition)
//            {
//                var positions = evaluationMatches.PartialEvaluationMatchesByCurrentPosition.Keys.OrderByDescending(i => i).ToArray();

//                foreach (var position in positions)
//                {
//                    var partialMatches = evaluationMatches.PartialEvaluationMatchesByCurrentPosition[position];
//                    var continuingMatches = new List<SentenceEvaluationMatch>();

//                    foreach (var partialMatch in partialMatches)
//                    {
//                        if (partialMatch.CategorizationMap.SpanTokens.Count == partialMatch.MatchedWords.Count)
//                        {
//                            evaluationMatches.CompleteMatches.Add(partialMatch);
//                        }
//                        else
//                        {
//                            var nextMapWord = partialMatch.CategorizationMap.SpanTokens[partialMatch.MatchedWords.Count].Word;

//                            WordMatchType matchType;
//                            if (nextMapWord.IsMatch(token.Word, out matchType))
//                            {
//                                partialMatch.MatchedWords.Add(new MatchedWord(token, matchType, this));
//                                continuingMatches.Add(partialMatch);
//                            }
//                        }
//                    }

//                    evaluationMatches.PartialEvaluationMatchesByCurrentPosition[position + 1] = continuingMatches;
//                }
//            }

//            EvaluateNewCategorizationMaps(evaluationMatches, token);
//        }

//        private void EvaluateNewCategorizationMaps(EvaluationMatches evaluationMatches, SentenceToken token)
//        {
//            lock (StartingCategorizationMaps)
//            {
//                var matches = new List<SentenceEvaluationMatch>();
//                foreach (var map in StartingCategorizationMaps)
//                {
//                    WordMatchType matchType;
//                    if (token.Word.IsMatch(map.SpanTokens[0].Word, out matchType))
//                    {
//                        var match = new SentenceEvaluationMatch {Category = map.Category, CategorizationMap = map};
//                        match.MatchedWords.Add(new MatchedWord(token, matchType, this));
//                        matches.Add(match);
//                    }
//                }
//                evaluationMatches.PartialEvaluationMatchesByCurrentPosition[0] = matches;
//            }
//        }

//        public override string ToString()
//        {
//            return $"Token: {Token} String: {StringUpToNode}";
//        }

//        public List<TrieNode<Word, EntityExtractionTreeNode>> GetChildTreeNodes()
//        {
//            lock (Children)
//            {
//                return Children.Values.ToList();
//            }
//        }

//        public string StringUpToNode => $"{Parent.StringUpToNode} {Token.Word.KeyTwo}".Trim();
//    }
//}
