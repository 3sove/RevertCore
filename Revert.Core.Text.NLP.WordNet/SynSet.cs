﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Revert.Core.Text.NLP.WordNet
{
    ///<summary>
    ///Represents a WordNet synset
    ///</summary>
    public class SynSet
    {
        #region static members

        ///<summary>
        ///Checks whether two synsets are equal
        ///</summary>
        ///<param name="synset1">First synset</param>
        ///<param name="synset2">Second synset</param>
        ///<returns>True if synsets are equal, false otherwise</returns>
        public static bool operator == (SynSet synset1, SynSet synset2)
        {
            // check object reference
            if (ReferenceEquals(synset1, synset2))
                return true;

            // check if either (but not both) are null
            if (synset2 is null ^ synset1 is null)
                return false;

            return synset1.Equals(synset2);
        }

        ///<summary>
        ///Checks whether two synsets are unequal
        ///</summary>
        ///<param name="synset1">First synset</param>
        ///<param name="synset2">Second synset</param>
        ///<returns>True if synsets are unequal, false otherwise</returns>
        public static bool operator != (SynSet synset1, SynSet synset2)
        {
            return !(synset1 == synset2);
        }

        #endregion

        private readonly int hashCode;
        private HashSet<string> isMostCommonSynsetForWords;
        // words for which the current synset is the most common sense

        private Dictionary<WordNetEngine.SynSetRelation, Dictionary<SynSet, Dictionary<int, HashSet<int>>>> lexicalRelations;
        private Dictionary<WordNetEngine.SynSetRelation, HashSet<SynSet>> relationSynSets;
        private WordNetEngine wordNetEngine;

        // the following must never change...hashing depends on them

        ///<summary>
        ///Gets semantic relations that exist between this synset and other synsets
        ///</summary>
        public IEnumerable<WordNetEngine.SynSetRelation> SemanticRelations => relationSynSets.Keys;

        ///<summary>
        ///Gets lexical relations that exist between words in this synset and words in another synset
        ///</summary>
        public IEnumerable<WordNetEngine.SynSetRelation> LexicalRelations => lexicalRelations.Keys;

        ///<summary>
        ///Gets the lexicographer file name for this synset (see the lexnames file in the WordNet distribution).
        ///</summary>
        public WordNetEngine.LexicographerFileName LexicographerFileName { get; private set; }

        ///<summary>
        ///Gets whether or not the current synset has been instantiated
        ///</summary>
        internal bool Instantiated { get; private set; }

        ///<summary>
        ///Gets or sets the back-pointer used when searching WordNet
        ///</summary>
        internal SynSet SearchBackPointer { get; set; }

        ///<summary>
        ///Gets the POS of the current synset
        ///</summary>
        public WordNetEngine.Pos POS { get; }

        ///<summary>
        ///Gets the byte offset of synset definition within the data file
        ///</summary>
        public int Offset { get; }

        ///<summary>
        ///Gets the gloss of the current SynSet
        ///</summary>
        public string Gloss { get; private set; }

        ///<summary>
        ///Gets the words in the current SynSet
        ///</summary>
        public List<string> Words { get; private set; }

        ///<summary>
        ///Gets the ID of this synset in the form POS:Offset
        ///</summary>
        public string Id { get; }

        #region construction
        ///<summary>
        ///Constructor. Creates the shell of a SynSet without any actual information. To gain access to SynSet words, gloss,
        ///and related SynSets, call SynSet.Instantiate.
        ///</summary>
        ///<param name="pos">POS of SynSet</param>
        ///<param name="offset">Byte location of SynSet definition within data file</param>
        ///<param name="wordNetEngine">
        ///WordNet engine used to instantiate this synset. This should be non-null only when constructing
        ///synsets for disk-based WordNet engines.
        ///</param>
        internal SynSet(WordNetEngine.Pos pos, int offset, WordNetEngine wordNetEngine)
        {
            POS = pos;
            Offset = offset;
            this.wordNetEngine = wordNetEngine;
            Instantiated = false;

            if (this.wordNetEngine != null && this.wordNetEngine.InMemory)
                throw new Exception("Don't need to pass a non-null WordNetEngine when using in-memory storage");

            // precompute the ID and hash code for efficiency
            Id = POS + ":" + Offset;
            hashCode = Id.GetHashCode();
        }

        ///<summary>
        ///Instantiates the current synset. Any related synsets are created as synset shells. These shells only contain
        ///the POS and offset. Other members can be initialized by calling Instantiate on the shells. This should only
        ///be called when _not_ using in-memory storage. When using in-memory storage, all synsets are instantiated in
        ///the WordNetEngine constructor.
        ///</summary>
        internal void Instantiate()
        {
            // instantiate with definition line from disk
            Instantiate(wordNetEngine.GetSynSetDefinition(POS, Offset), null);
        }

        ///<summary>
        ///Instantiates the current synset. If idSynset is non-null, related synsets references are set to those from
        ///idSynset; otherwise, related synsets are created as shells.
        ///</summary>
        ///<param name="definition">Definition line of synset from data file</param>
        ///<param name="idSynset">Lookup for related synsets. If null, all related synsets will be created as shells.</param>
        internal void Instantiate(string definition, Dictionary<string, SynSet> idSynset)
        {
            // don't re-instantiate
            if (Instantiated)
                throw new Exception("Synset has already been instantiated");

            /* get lexicographer file name...the enumeration lines up precisely with the wordnet spec (see the lexnames file) except that
             * it starts with None, so we need to add 1 to the definition line's value to get the correct file name */
            int lexicographerFileNumber = int.Parse(GetField(definition, 1)) + 1;
            if (lexicographerFileNumber <= 0)
                throw new Exception("Invalid lexicographer file name number. Should be >= 1.");

            LexicographerFileName = (WordNetEngine.LexicographerFileName) lexicographerFileNumber;

            // get number of words in the synset and the start character of the word list
            int wordStart;
            int numWords = int.Parse(GetField(definition, 3, out wordStart), NumberStyles.HexNumber);
            wordStart = definition.IndexOf(' ', wordStart) + 1;

            // get words in synset
            Words = new List<string>(numWords);
            for (int i = 0; i < numWords; ++i)
            {
                int wordEnd = definition.IndexOf(' ', wordStart + 1) - 1;
                int wordLen = wordEnd - wordStart + 1;
                string word = definition.Substring(wordStart, wordLen);
                if (word.Contains(' '))
                    throw new Exception("Unexpected space in word:  " + word);

                Words.Add(word);

                // skip lex_id field
                wordStart = definition.IndexOf(' ', wordEnd + 2) + 1;
            }

            // get gloss
            Gloss = definition.Substring(definition.IndexOf('|') + 1).Trim();
            if (Gloss.Contains('|'))
                throw new Exception("Unexpected pipe in gloss");

            // get number and start of relations
            int relationCountField = 3 + (Words.Count*2) + 1;
            int relationFieldStart;
            int numRelations = int.Parse(GetField(definition, relationCountField, out relationFieldStart));
            relationFieldStart = definition.IndexOf(' ', relationFieldStart) + 1;

            // grab each related synset
            relationSynSets = new Dictionary<WordNetEngine.SynSetRelation, HashSet<SynSet>>();
            lexicalRelations =
                new Dictionary<WordNetEngine.SynSetRelation, Dictionary<SynSet, Dictionary<int, HashSet<int>>>>();
            for (int relationNum = 0; relationNum < numRelations; ++relationNum)
            {
                string relationSymbol = null;
                int relatedSynSetOffset = -1;
                var relatedSynSetPos = WordNetEngine.Pos.None;
                int sourceWordIndex = -1;
                int targetWordIndex = -1;

                // each relation has four columns
                for (int relationField = 0; relationField <= 3; ++relationField)
                {
                    int fieldEnd = definition.IndexOf(' ', relationFieldStart + 1) - 1;
                    int fieldLen = fieldEnd - relationFieldStart + 1;
                    string fieldValue = definition.Substring(relationFieldStart, fieldLen);

                    // relation symbol
                    if (relationField == 0)
                        relationSymbol = fieldValue;
                        // related synset offset
                    else if (relationField == 1)
                        relatedSynSetOffset = int.Parse(fieldValue);
                        // related synset POS
                    else if (relationField == 2)
                        relatedSynSetPos = GetPOS(fieldValue);
                        // source/target word for lexical relation
                    else if (relationField == 3)
                    {
                        sourceWordIndex = int.Parse(fieldValue.Substring(0, 2), NumberStyles.HexNumber);
                        targetWordIndex = int.Parse(fieldValue.Substring(2), NumberStyles.HexNumber);
                    }
                    else
                        throw new Exception();

                    relationFieldStart = definition.IndexOf(' ', relationFieldStart + 1) + 1;
                }

                // get related synset...create shell if we don't have a lookup
                SynSet relatedSynSet = idSynset == null
                    ? new SynSet(relatedSynSetPos, relatedSynSetOffset, wordNetEngine)
                    : idSynset[relatedSynSetPos + ":" + relatedSynSetOffset];

                // get relation
                WordNetEngine.SynSetRelation relation = WordNetEngine.GetSynSetRelation(POS, relationSymbol);

                // add semantic relation if we have neither a source nor a target word index
                if (sourceWordIndex == 0 && targetWordIndex == 0)
                {
                    HashSet<SynSet> synSet;
                    if (!relationSynSets.TryGetValue(relation, out synSet))
                    {
                        synSet = new HashSet<SynSet>();
                        relationSynSets[relation] = synSet;
                    }

                    synSet.Add(relatedSynSet);
                }
                    // add lexical relation
                else
                {
                    Dictionary<SynSet, Dictionary<int, HashSet<int>>> setsBySynSet;
                    Dictionary<int, HashSet<int>> relatedSets;
                    HashSet<int> setIds;
                    if (!lexicalRelations.TryGetValue(relation, out setsBySynSet))
                    {
                        setsBySynSet = new Dictionary<SynSet, Dictionary<int, HashSet<int>>>();
                        lexicalRelations[relation] = setsBySynSet;
                    }

                    if (!setsBySynSet.TryGetValue(relatedSynSet, out relatedSets))
                    {
                        relatedSets = new Dictionary<int, HashSet<int>>();
                        setsBySynSet[relatedSynSet] = relatedSets;
                    }

                    if (!relatedSets.TryGetValue(sourceWordIndex, out setIds))
                    {
                        setIds = new HashSet<int>();
                        relatedSets[sourceWordIndex] = setIds;
                    }

                    if (!setIds.Contains(targetWordIndex)) setIds.Add(targetWordIndex);
                }
            }

            // release the wordnet engine if we have one...don't need it anymore
            wordNetEngine = null;
            Instantiated = true;
        }

        ///<summary>
        ///Gets a space-delimited field from a synset definition line
        ///</summary>
        ///<param name="line">SynSet definition line</param>
        ///<param name="fieldNum">Number of field to get</param>
        ///<returns>Field value</returns>
        private string GetField(string line, int fieldNum)
        {
            int dummy;
            return GetField(line, fieldNum, out dummy);
        }

        ///<summary>
        ///Gets a space-delimited field from a synset definition line
        ///</summary>
        ///<param name="line">SynSet definition line</param>
        ///<param name="fieldNum">Number of field to get</param>
        ///<param name="startIndex">Start index of field within the line</param>
        ///<returns>Field value</returns>
        private string GetField(string line, int fieldNum, out int startIndex)
        {
            if (fieldNum < 0)
                throw new Exception("Invalid field number:  " + fieldNum);

            // scan fields until we hit the one we want
            int currField = 0;
            startIndex = 0;
            while (true)
            {
                if (currField == fieldNum)
                {
                    // get the end of the field
                    int endIndex = line.IndexOf(' ', startIndex + 1) - 1;

                    // watch out for end of line
                    if (endIndex < 0)
                        endIndex = line.Length - 1;

                    // get length of field
                    int fieldLen = endIndex - startIndex + 1;

                    // return field value
                    return line.Substring(startIndex, fieldLen);
                }

                // move to start of next field (one beyond next space)
                startIndex = line.IndexOf(' ', startIndex) + 1;

                // if there are no more spaces and we haven't found the field, the caller requested an invalid field
                if (startIndex == 0)
                    throw new Exception("Failed to get field number:  " + fieldNum);

                ++currField;
            }
        }

        private static WordNetEngine.Pos GetPOS(string posString)
        {
            WordNetEngine.Pos relatedPos;
            switch (posString)
            {
                case "n":
                    relatedPos = WordNetEngine.Pos.Noun;
                    break;
                case "v":
                    relatedPos = WordNetEngine.Pos.Verb;
                    break;
                case "s":
                case "a":
                    relatedPos = WordNetEngine.Pos.Adjective;
                    break;
                case "r":
                    relatedPos = WordNetEngine.Pos.Adverb;
                    break;
                default:
                    throw new Exception("Unexpected POS:  " + posString);
            }

            return relatedPos;
        }

        #endregion

        ///<summary>
        ///Gets the number of synsets related to the current one by the given relation
        ///</summary>
        ///<param name="relation">Relation to check</param>
        ///<returns>Number of synset related to the current one by the given relation</returns>
        public int GetRelatedSynSetCount(WordNetEngine.SynSetRelation relation)
        {
            if (!relationSynSets.ContainsKey(relation))
                return 0;

            return relationSynSets[relation].Count;
        }

        ///<summary>
        ///Gets synsets related to the current synset
        ///</summary>
        ///<param name="relation">Synset relation to follow</param>
        ///<param name="recursive">Whether or not to follow the relation recursively for all related synsets</param>
        ///<returns>Synsets related to the given one by the given relation</returns>
        public HashSet<SynSet> GetRelatedSynSets(WordNetEngine.SynSetRelation relation, bool recursive)
        {
            return GetRelatedSynSets(new[] {relation}, recursive);
        }

        ///<summary>
        ///Gets synsets related to the current synset
        ///</summary>
        ///<param name="relations">Synset relations to follow</param>
        ///<param name="recursive">Whether or not to follow the relations recursively for all related synsets</param>
        ///<returns>Synsets related to the given one by the given relations</returns>
        public HashSet<SynSet> GetRelatedSynSets(IEnumerable<WordNetEngine.SynSetRelation> relations, bool recursive)
        {
            var synsets = new HashSet<SynSet>();
            GetRelatedSynSets(relations, recursive, synsets);
            return synsets;
        }

        ///<summary>
        ///Private version of GetRelatedSynSets that avoids cyclic paths in WordNet. The current synset must itself be instantiated.
        ///</summary>
        ///<param name="relations">Synset relations to get</param>
        ///<param name="recursive">Whether or not to follow the relation recursively for all related synsets</param>
        ///<param name="currSynSets">Current collection of synsets, which we'll add to.</param>
        private void GetRelatedSynSets(IEnumerable<WordNetEngine.SynSetRelation> relations, bool recursive,
                                       HashSet<SynSet> currSynSets)
        {
            // try each relation
            WordNetEngine.SynSetRelation[] synSetRelations = relations as WordNetEngine.SynSetRelation[] ??
                                                             relations.ToArray();
            foreach (WordNetEngine.SynSetRelation relation in synSetRelations)
                if (relationSynSets.ContainsKey(relation))
                    foreach (SynSet relatedSynset in relationSynSets[relation])
                        // only add synset if it isn't already present (wordnet contains cycles)
                        if (!currSynSets.Contains(relatedSynset))
                        {
                            // instantiate synset if it isn't already (for disk-based storage)
                            if (!relatedSynset.Instantiated) relatedSynset.Instantiate();

                            currSynSets.Add(relatedSynset);

                            if (recursive) relatedSynset.GetRelatedSynSets(synSetRelations, true, currSynSets);
                        }
        }

        ///<summary>
        ///Gets the shortest path from the current synset to another, following the given synset relations.
        ///</summary>
        ///<param name="destination">Destination synset</param>
        ///<param name="relations">Relations to follow, or null for all relations.</param>
        ///<returns>Synset path, or null if none exists.</returns>
        public List<SynSet> GetShortestPathTo(SynSet destination, IEnumerable<WordNetEngine.SynSetRelation> relations)
        {
            // make sure the backpointer on the current synset is null - can't predict what other functions might do
            SearchBackPointer = null;

            // avoid cycles
            var synsetsEncountered = new HashSet<SynSet> {this};

            // start search queue
            var searchQueue = new Queue<SynSet>();
            searchQueue.Enqueue(this);

            // run search
            List<SynSet> path = null;
            while (searchQueue.Count > 0 && path == null)
            {
                SynSet currSynSet = searchQueue.Dequeue();

                // see if we've finished the search
                if (currSynSet == destination)
                {
                    // gather synsets along path
                    path = new List<SynSet>();
                    while (currSynSet != null)
                    {
                        path.Add(currSynSet);
                        currSynSet = currSynSet.SearchBackPointer;
                    }

                    // reverse for the correct order
                    path.Reverse();
                }
                    // expand the search one level
                else
                    foreach (SynSet synset in currSynSet.GetRelatedSynSets(relations ?? Enum.GetValues(typeof(WordNetEngine.SynSetRelation)) as WordNetEngine.SynSetRelation[], false))
                        if (!synsetsEncountered.Contains(synset))
                        {
                            synset.SearchBackPointer = currSynSet;
                            searchQueue.Enqueue(synset);

                            synsetsEncountered.Add(synset);
                        }
            }

            // null-out all search backpointers
            foreach (SynSet synset in synsetsEncountered)
                synset.SearchBackPointer = null;

            return path;
        }

        ///<summary>
        ///Gets the closest synset that is reachable from the current and another synset along the given relations. For example,
        ///given two synsets and the Hypernym relation, this will return the lowest synset that is a hypernym of both synsets. If
        ///the hypernym hierarchy forms a tree, this will be the lowest common ancestor.
        ///</summary>
        ///<param name="synset">Other synset</param>
        ///<param name="relations">Relations to follow</param>
        ///<returns>Closest mutually reachable synset</returns>
        public SynSet GetClosestMutuallyReachableSynset(SynSet synset, IEnumerable<WordNetEngine.SynSetRelation> relations)
        {
            WordNetEngine.SynSetRelation[] localRelations = relations as WordNetEngine.SynSetRelation[] ?? Enum.GetValues(typeof (WordNetEngine.SynSetRelation)) as WordNetEngine.SynSetRelation[];

            // avoid cycles
            var synsetsEncountered = new HashSet<SynSet> {this};

            // start search queue
            var searchQueue = new Queue<SynSet>();
            searchQueue.Enqueue(this);

            // run search
            SynSet closest = null;
            while (searchQueue.Count > 0 && closest == null)
            {
                SynSet currSynSet = searchQueue.Dequeue();

                /* check for a path between the given synset and the current one. if such a path exists, the current
                 * synset is the closest mutually reachable synset. */
                if (synset.GetShortestPathTo(currSynSet, localRelations) != null)
                    closest = currSynSet;
                    // otherwise, expand the search along the given relations
                else
                    foreach (SynSet relatedSynset in currSynSet.GetRelatedSynSets(localRelations, false))
                        if (!synsetsEncountered.Contains(relatedSynset))
                        {
                            searchQueue.Enqueue(relatedSynset);
                            synsetsEncountered.Add(relatedSynset);
                        }
            }

            return closest;
        }

        ///<summary>
        ///Computes the depth of the current synset following a set of relations. Returns the minimum of all possible depths. Root nodes
        ///have a depth of zero.
        ///</summary>
        ///<param name="relations">Relations to follow</param>
        ///<returns>Depth of current SynSet</returns>
        public int GetDepth(IEnumerable<WordNetEngine.SynSetRelation> relations)
        {
            var synsets = new HashSet<SynSet> {this};
            return GetDepth(relations, ref synsets);
        }

        ///<summary>
        ///Computes the depth of the current synset following a set of relations. Returns the minimum of all possible depths. Root
        ///nodes have a depth of zero.
        ///</summary>
        ///<param name="relations">Relations to follow</param>
        ///<param name="synsetsEncountered">Synsets that have already been encountered. Prevents cycles from being entered.</param>
        ///<returns>Depth of current SynSet</returns>
        private int GetDepth(IEnumerable<WordNetEngine.SynSetRelation> relations, ref HashSet<SynSet> synsetsEncountered)
        {
            // get minimum depth through all relatives
            int minimumDepth = -1;
            var synSetRelations = relations as IList<WordNetEngine.SynSetRelation> ?? relations.ToArray();
            foreach (SynSet relatedSynset in GetRelatedSynSets(synSetRelations, false))
                if (!synsetsEncountered.Contains(relatedSynset))
                {
                    // add this before recursing in order to avoid cycles
                    synsetsEncountered.Add(relatedSynset);

                    // get depth from related synset
                    int relatedDepth = relatedSynset.GetDepth(synSetRelations, ref synsetsEncountered);

                    // use depth if it's the first or it's less than the current best
                    if (minimumDepth == -1 || relatedDepth < minimumDepth)
                        minimumDepth = relatedDepth;
                }

            // depth is one plus minimum depth through any relative synset...for synsets with no related synsets, this will be zero
            return minimumDepth + 1;
        }

        ///<summary>
        ///Gets lexically related words for the current synset. Many of the relations in WordNet are lexical instead of semantic. Whereas
        ///the latter indicate relations between entire synsets (e.g., hypernym), the former indicate relations between specific
        ///words in synsets. This method retrieves all lexical relations and the words related thereby.
        ///</summary>
        ///<returns>Mapping from relations to mappings from words in the current synset to related words in the related synsets</returns>
        public Dictionary<WordNetEngine.SynSetRelation, Dictionary<string, HashSet<string>>> GetLexicallyRelatedWords()
        {
            var relatedWords = new Dictionary<WordNetEngine.SynSetRelation, Dictionary<string, HashSet<string>>>();
            foreach (WordNetEngine.SynSetRelation relation in lexicalRelations.Keys)
            {
                Dictionary<string, HashSet<string>> relatedWordSetByWord;
                if (!relatedWords.TryGetValue(relation, out relatedWordSetByWord))
                {
                    relatedWordSetByWord = new Dictionary<string, HashSet<string>>();
                    relatedWords[relation] = relatedWordSetByWord;
                }

                foreach (SynSet relatedSynSet in lexicalRelations[relation].Keys)
                {
                    // make sure related synset is initialized
                    if (!relatedSynSet.Instantiated) relatedSynSet.Instantiate();

                    foreach (int sourceWordIndex in lexicalRelations[relation][relatedSynSet].Keys)
                    {
                        string sourceWord = Words[sourceWordIndex - 1];

                        HashSet<string> relatedWordSet;
                        if (!relatedWordSetByWord.TryGetValue(sourceWord, out relatedWordSet))
                        {
                            relatedWordSet = new HashSet<string>();
                            relatedWordSetByWord[sourceWord] = relatedWordSet;
                        }

                        foreach (int targetWordIndex in lexicalRelations[relation][relatedSynSet][sourceWordIndex])
                        {
                            relatedWordSet.Add(relatedSynSet.Words[targetWordIndex - 1]);
                        }
                    }
                }
            }

            return relatedWords;
        }

        ///<summary>
        ///Gets hash code for this synset
        ///</summary>
        ///<returns>Hash code</returns>
        public override int GetHashCode()
        {
            return hashCode;
        }

        ///<summary>
        ///Checks whether the current synset equals another
        ///</summary>
        ///<param name="obj">Other synset</param>
        ///<returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is SynSet))
                return false;

            var synSet = (SynSet)obj;

            return POS == synSet.POS && Offset == synSet.Offset;
        }

        ///<summary>
        ///Gets description of synset
        ///</summary>
        ///<returns>Description</returns>
        public override string ToString()
        {
            var desc = new StringBuilder();

            // if the synset is instantiated, include words and gloss
            if (Instantiated)
            {
                desc.Append("{");
                bool prependComma = false;
                foreach (string word in Words)
                {
                    desc.Append((prependComma ? ", " : "") + word);
                    prependComma = true;
                }

                desc.Append("}:  " + Gloss);
            }
                // if it's not instantiated, just include the ID
            else
                desc.Append(Id);

            return desc.ToString();
        }

        ///<summary>
        ///Checks whether this is the most common synset for a word
        ///</summary>
        ///<param name="word">Word to check</param>
        ///<returns>True if this is the most common synset, false otherwise</returns>
        internal bool IsMostCommonSynsetFor(string word)
        {
            return isMostCommonSynsetForWords != null && isMostCommonSynsetForWords.Contains(word);
        }

        ///<summary>
        ///Set the current synset as the most common for a word
        ///</summary>
        ///<param name="word">Word to set</param>
        internal void SetAsMostCommonSynsetFor(string word)
        {
            if (isMostCommonSynsetForWords == null)
                isMostCommonSynsetForWords = new HashSet<string>();

            isMostCommonSynsetForWords.Add(word);
        }
    }
}