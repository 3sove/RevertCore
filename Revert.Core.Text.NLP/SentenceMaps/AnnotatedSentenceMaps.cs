using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revert.Core.Extensions;
using Revert.Core.Text.NLP.FrameNet;

namespace Revert.Core.Text.NLP.SentenceMaps
{
    public class AnnotatedSentenceMaps
    {
        private static readonly HashSet<char> spanStartCharacterBlacklist = new HashSet<char>() { ',', ' ' };
        private FrameNetEngine frameNetEngine;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frameNetEnginePath">Location of the \NLP\Data\FrameNet\ folder</param>
        /// <param name="entityExtractionTree"></param>
        public AnnotatedSentenceMaps(string frameNetEnginePath, EnglishDictionary englishDictionary)
        {
            FrameNetEnginePath = frameNetEnginePath;
            EnglishDictionary = englishDictionary;
        }

        /// <summary>
        /// Location of the \NLP\Data\FrameNet\ folder.
        /// </summary>
        public string FrameNetEnginePath { get; set; } 
        //public EntityExtractionTree EntityExtractionTree { get; set; }

            public EnglishDictionary EnglishDictionary { get; set; }

        public List<AnnotatedSentenceMap> GetStringRepresentationMaps()
        {
            if (frameNetEngine == null) frameNetEngine = new FrameNetEngine(FrameNetEnginePath);
            var topLevelFrames = frameNetEngine.Frames.Where(f => f.SuperFrames.Count == 0).ToList();
            var annotatedSentenceMaps = new List<AnnotatedSentenceMap>();

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var frameChunks = topLevelFrames.SplitCollection();

            Parallel.For(0, frameChunks.Count, chunkIndex =>
                {
                    var frameChunk = frameChunks[chunkIndex];

                    for (var i = 0; i < frameChunk.Count; i++)
                    {
                        var frame = frameChunk[i];
                        var evaluatedFrames = new HashSet<Frame>();
                        Console.WriteLine("Thread: {0}, evaluating top level frame {1} of {2}.", chunkIndex, i + 1, frameChunk.Count);
                        List<AnnotatedSentenceMap> maps;
                        if (!EvaluateSubFrame(frame, null, frameNetEngine, ref evaluatedFrames, out maps)) continue;
                        
                        lock (annotatedSentenceMaps)
                        {
                            annotatedSentenceMaps.AddRange(maps);
                        }
                    };
                });

            sw.Stop();
            Console.WriteLine("Executed in {0}ms", sw.ElapsedMilliseconds.ToString("#,#"));
            
            return annotatedSentenceMaps;
        }

        private bool EvaluateSubFrame(Frame frame, Frame superFrame, FrameNetEngine frameNetEngine, ref HashSet<Frame> evaluatedFrames, out List<AnnotatedSentenceMap> stringRepresentationMaps)
        {
            stringRepresentationMaps = new List<AnnotatedSentenceMap>();
            if (!evaluatedFrames.Add(frame)) return false; //already evaluated

            var attestations = frameNetEngine.GetAttestationsForFrame(frame);

            EvaluateAttestations(stringRepresentationMaps, attestations);

            foreach (var subFrame in frame.SubFrames)
            {
                List<AnnotatedSentenceMap> maps;
                if (EvaluateSubFrame(subFrame, frame, frameNetEngine, ref evaluatedFrames, out maps))
                    stringRepresentationMaps.AddRange(maps);
            }
            return true;
        }

        private void EvaluateAttestations(List<AnnotatedSentenceMap> annotatedSentenceMaps, List<Attestation> attestations)
        {
            foreach (var attestation in attestations)
            {
                var tokenCategorizationMaps = new List<TokenCategorizationMap>();
                var sentenceTokens = Tokenizer.GetSentenceTokens(attestation.Sentence, EnglishDictionary);

                EvaluateAnnotatedSpansByFrameElmenet(attestation, sentenceTokens, tokenCategorizationMaps);

                annotatedSentenceMaps.Add(new AnnotatedSentenceMap
                    {
                        TokenCategorizationMaps = tokenCategorizationMaps,
                        TokenCategorizationMapsByStartingPosition = tokenCategorizationMaps.ToMultiDictionary(m => m.StartTokenIndex),
                        Tokens = Tokenizer.GetSentenceTokens(attestation.Sentence, EnglishDictionary),
                        OriginalString = attestation.Sentence
                    });
            }
        }

        private void EvaluateAnnotatedSpansByFrameElmenet(Attestation attestation, List<SentenceToken> sentenceTokens, List<TokenCategorizationMap> tokenRepresentationMaps)
        {
            foreach (var item in attestation.AnnotatedSpansByFrameElement)
            {
                var map = new TokenCategorizationMap { Category = item.Key.Name };
                EvaluateAnnotatedSpans(sentenceTokens, item, map);
                if (map.StartTokenIndex != -1) tokenRepresentationMaps.Add(map);
            }
        }

        private void EvaluateAnnotatedSpans(List<SentenceToken> sentenceTokens, KeyValuePair<FrameElement, List<AnnotatedSpan>> item, TokenCategorizationMap map)
        {
            foreach (var span in item.Value)
            {
                map.SpanTokens = Tokenizer.GetSentenceTokens(span.Value, EnglishDictionary);
                map.StartTokenIndex = GetSpanTokenStartIndex(sentenceTokens, span);
            }
        }

        private static int GetSpanTokenStartIndex(List<SentenceToken> sentenceTokens, AnnotatedSpan span)
        {
            var firstCharacterOffset = 0;
            var firstCharacter = span.Value[firstCharacterOffset];

            while (spanStartCharacterBlacklist.Contains(firstCharacter))
            {
                firstCharacter = span.Value[++firstCharacterOffset];
            }

            for (var sentenceTokenIndex = 0; sentenceTokenIndex < sentenceTokens.Count; sentenceTokenIndex++)
            {
                if (sentenceTokens[sentenceTokenIndex].RawStartingPosition == span.Start + firstCharacterOffset) return sentenceTokenIndex;
            }

            if (!span.Value.StartsWith("ly")) //strange adverb logic in framenet, which isn't wanted or needed.  This span is probably a bug.
                throw new Exception($"Could not find span \"{span.Value}\" start index within sentence \"{sentenceTokens.Select(s => s.Word.Value).Combine(" ")}\"");
            return -1;
        }

        //public bool EvaluateString(string value, out List<SentenceEvaluationMatch> matches)
        //{
        //    //categoryMatches = new List<TrieMatch<List<string>>>();
        //    var sentenceTokens = Tokenizer.GetSentenceTokens(value, EnglishDictionary);
        //    return Evaluate(sentenceTokens, out matches);
        //}

        //public bool Evaluate(List<SentenceToken> sentenceTokens, out List<SentenceEvaluationMatch> matches)
        //{
        //    matches = new List<SentenceEvaluationMatch>();
        //    if (EntityExtractionTree.GetChildTreeNodes()Children.Count == 0)
        //        return false;

        //    EntityExtractionTreeNode child;
        //    return model.EntityExtractionTree.TryGetChild(sentenceTokens[0].Word, out child) && child.Evaluate(sentenceTokens, out matches);
        //}
    }
}