using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Revert.Core.Text.NLP.WordNet
{
    ///<summary>
    ///Provides access to the WordNet resource via two alternative methods, in-memory and disk-based. The first is blazingly
    ///fast but also hugely inefficient in terms of memory consumption. The latter uses essentially zero memory but is slow
    ///because all searches have to be conducted on-disk.
    ///</summary>
    public partial class WordNetEngine
    {
        ///<summary>
        ///SynSet relation symbols that are available for each POS
        ///</summary>
        public static readonly Dictionary<Pos, Dictionary<string, SynSetRelation>> PartOfSpeechSymbolRelation;

        ///<summary>
        ///Static constructor
        ///</summary>
        static WordNetEngine()
        {
            PartOfSpeechSymbolRelation = new Dictionary<Pos, Dictionary<string, SynSetRelation>>();

            // noun relations
            var nounSymbolRelation = new Dictionary<string, SynSetRelation>
            {
                {"!", SynSetRelation.Antonym},
                {"@", SynSetRelation.Hypernym},
                {"@i", SynSetRelation.InstanceHypernym},
                {"~", SynSetRelation.Hyponym},
                {"~i", SynSetRelation.InstanceHyponym},
                {"#m", SynSetRelation.MemberHolonym},
                {"#s", SynSetRelation.SubstanceHolonym},
                {"#p", SynSetRelation.PartHolonym},
                {"%m", SynSetRelation.MemberMeronym},
                {"%s", SynSetRelation.SubstanceMeronym},
                {"%p", SynSetRelation.PartMeronym},
                {"=", SynSetRelation.Attribute},
                {"+", SynSetRelation.DerivationallyRelated},
                {";c", SynSetRelation.TopicDomain},
                {"-c", SynSetRelation.TopicDomainMember},
                {";r", SynSetRelation.RegionDomain},
                {"-r", SynSetRelation.RegionDomainMember},
                {";u", SynSetRelation.UsageDomain},
                {"-u", SynSetRelation.UsageDomainMember}
            };
            PartOfSpeechSymbolRelation.Add(Pos.Noun, nounSymbolRelation);

            // verb relations
            var verbSymbolRelation = new Dictionary<string, SynSetRelation>
            {
                {"!", SynSetRelation.Antonym},
                {"@", SynSetRelation.Hypernym},
                {"~", SynSetRelation.Hyponym},
                {"*", SynSetRelation.Entailment},
                {">", SynSetRelation.Cause},
                {"^", SynSetRelation.AlsoSee},
                {"$", SynSetRelation.VerbGroup},
                {"+", SynSetRelation.DerivationallyRelated},
                {";c", SynSetRelation.TopicDomain},
                {";r", SynSetRelation.RegionDomain},
                {";u", SynSetRelation.UsageDomain}
            };
            PartOfSpeechSymbolRelation.Add(Pos.Verb, verbSymbolRelation);

            // adjective relations
            var adjectiveSymbolRelation = new Dictionary<string, SynSetRelation>
            {
                {"!", SynSetRelation.Antonym},
                {"&", SynSetRelation.SimilarTo},
                {"<", SynSetRelation.ParticipleOfVerb},
                {@"\", SynSetRelation.Pertainym},
                {"=", SynSetRelation.Attribute},
                {"^", SynSetRelation.AlsoSee},
                {";c", SynSetRelation.TopicDomain},
                {";r", SynSetRelation.RegionDomain},
                {";u", SynSetRelation.UsageDomain},
                {"+", SynSetRelation.DerivationallyRelated}
            };
            // not in documentation
            PartOfSpeechSymbolRelation.Add(Pos.Adjective, adjectiveSymbolRelation);

            // adverb relations
            var adverbSymbolRelation = new Dictionary<string, SynSetRelation>
            {
                {"!", SynSetRelation.Antonym},
                {@"\", SynSetRelation.DerivedFromAdjective},
                {";c", SynSetRelation.TopicDomain},
                {";r", SynSetRelation.RegionDomain},
                {";u", SynSetRelation.UsageDomain},
                {"+", SynSetRelation.DerivationallyRelated}
            };
            // not in documentation
            PartOfSpeechSymbolRelation.Add(Pos.Adverb, adverbSymbolRelation);
        }

        ///<summary>
        ///Gets the relation for a given POS and symbol
        ///</summary>
        ///<param name="pos">POS to get relation for</param>
        ///<param name="symbol">Symbol to get relation for</param>
        ///<returns>SynSet relation</returns>
        internal static SynSetRelation GetSynSetRelation(Pos pos, string symbol)
        {
            Dictionary<string, SynSetRelation> relations;
            var relation = SynSetRelation.None;
            if (PartOfSpeechSymbolRelation.TryGetValue(pos, out relations)) relations.TryGetValue(symbol, out relation);

            return relation;
        }

        ///<summary>
        ///Gets the part-of-speech associated with a file path
        ///</summary>
        ///<param name="path">Path to get POS for</param>
        ///<returns>POS</returns>
        private static Pos GetFilePos(string path)
        {
            Pos pos;
            string extension = Path.GetExtension(path);
            if (!string.IsNullOrEmpty(extension)) extension = extension.Trim('.');
            if (extension == "adj")
                pos = Pos.Adjective;
            else if (extension == "adv")
                pos = Pos.Adverb;
            else if (extension == "noun")
                pos = Pos.Noun;
            else if (extension == "verb")
                pos = Pos.Verb;
            else
                throw new Exception("Unrecognized data file extension:  " + extension);

            return pos;
        }

        ///<summary>
        ///Gets synset shells from a word index line. A synset shell is an instance of SynSet with only the POS and Offset
        ///members initialized. These members are enough to look up the full synset within the corresponding data file. This
        ///method is static to prevent inadvertent references to a current WordNetEngine, which should be passed via the 
        ///corresponding parameter.
        ///</summary>
        ///<param name="wordIndexLine">Word index line from which to get synset shells</param>
        ///<param name="pos">POS of the given index line</param>
        ///<param name="mostCommonSynSet">Returns the most common synset for the word</param>
        ///<param name="wordNetEngine">WordNetEngine to pass to the constructor of each synset shell</param>
        ///<returns>Synset shells for the given index line</returns>
        private static HashSet<SynSet> GetSynSetShells(string wordIndexLine, Pos pos, out SynSet mostCommonSynSet, WordNetEngine wordNetEngine)
        {
            var synsets = new HashSet<SynSet>();
            mostCommonSynSet = null;

            // get number of synsets
            string[] parts = wordIndexLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int numSynSets = int.Parse(parts[2]);

            // grab each synset shell, from last to first
            int firstOffsetIndex = parts.Length - numSynSets;
            for (int i = parts.Length - 1; i >= firstOffsetIndex; --i)
            {
                // create synset
                int offset = int.Parse(parts[i]);

                // add synset to collection                        
                var synset = new SynSet(pos, offset, wordNetEngine);
                synsets.Add(synset);

                // if this is the last synset offset to get (since we grabbed them in reverse order), record it as the most common synset
                if (i == firstOffsetIndex)
                    mostCommonSynSet = synset;
            }

            if (mostCommonSynSet == null)
                throw new Exception("Failed to get most common synset");

            return synsets;
        }

        private Dictionary<Pos, BinarySearchTextStream> posIndexWordSearchStream;  // disk-based search streams to get words from the index files
        private Dictionary<Pos, StreamReader> posSynSetDataFile;                   // disk-based synset data files
        private Dictionary<Pos, Dictionary<string, HashSet<SynSet>>> posWordSynSets;   // in-memory pos-word synsets lookup

        ///<summary>
        ///Gets whether or not the data in this WordNetEngine is stored in memory
        ///</summary>
        public bool InMemory { get; }

        ///<summary>
        ///Gets the WordNet data distribution directory
        ///</summary>
        public string WordNetDirectory { get; }

        ///<summary>
        ///Gets all words in WordNet, organized by POS.
        ///</summary>
        public Dictionary<Pos, HashSet<string>> AllWords
        {
            get
            {
                var posWords = new Dictionary<Pos, HashSet<string>>();

                if (InMemory)
                    // grab words from in-memory index
                    foreach (Pos pos in posWordSynSets.Keys)
                        posWords.Add(pos, new HashSet<string>(posWordSynSets[pos].Keys));
                else
                    // read index file for each pos
                    foreach (Pos pos in posIndexWordSearchStream.Keys)
                    {
                        // reset index file to start
                        StreamReader indexFile = posIndexWordSearchStream[pos].Stream;
                        indexFile.SetPosition(0);

                        // read words, skipping header lines
                        var words = new HashSet<string>();
                        string line;
                        while ((line = indexFile.ReadLine()) != null)
                            if (!line.StartsWith(" "))
                                words.Add(line.Substring(0, line.IndexOf(' ')));

                        posWords.Add(pos, words);
                    }

                return posWords;
            }
        }

        public Dictionary<string, SynSet> IdSynset { get; set; }

        ///<summary>
        ///Constructor
        ///</summary>
        ///<param name="wordNetDirectory">Path to WorNet directory (the one with the data and index files in it)</param>
        ///<param name="inMemory">Whether or not to store all data in memory. In-memory storage requires quite a bit of space
        ///but it is also very quick. The alternative (false) will cause the data to be searched on-disk with an efficient
        ///binary search algorithm.</param>
        public WordNetEngine(string wordNetDirectory, bool inMemory)
        {
            WordNetDirectory = wordNetDirectory;
            InMemory = inMemory;
            posIndexWordSearchStream = null;
            posSynSetDataFile = null;

            if (!System.IO.Directory.Exists(WordNetDirectory))
                throw new DirectoryNotFoundException("Non-existent WordNet directory:  " + WordNetDirectory);

            // get data and index paths
            var dataPaths = new[]
            {
                Path.Combine(WordNetDirectory, "data.adj"),
                Path.Combine(WordNetDirectory, "data.adv"),
                Path.Combine(WordNetDirectory, "data.noun"),
                Path.Combine(WordNetDirectory, "data.verb")
            };

            var indexPaths = new[]
            {
                Path.Combine(WordNetDirectory, "index.adj"),
                Path.Combine(WordNetDirectory, "index.adv"),
                Path.Combine(WordNetDirectory, "index.noun"),
                Path.Combine(WordNetDirectory, "index.verb")
            };

            // make sure all files exist
            foreach (string path in dataPaths.Union(indexPaths))
                if (!System.IO.File.Exists(path))
                    throw new FileNotFoundException("Failed to find WordNet file:  " + path);

            #region index file sorting
            string sortFlagPath = Path.Combine(WordNetDirectory, ".sorted_for_dot_net");
            if (!System.IO.File.Exists(sortFlagPath))
            {
                /* make sure the index files are sorted according to the current sort order. the index files in the
                 * wordnet distribution are sorted in the order needed for (presumably) the java api, which uses
                 * a different sort order than the .net runtime. thus, unless we resort the lines in the index 
                 * files, we won't be able to do a proper binary search over the data. */
                foreach (string indexPath in indexPaths)
                {
                    // create temporary file for sorted lines
                    string tempPath = Path.GetTempFileName();
                    using (var tempFile = new StreamWriter(tempPath))
                    {

                        // get number of words (lines) in file
                        int numWords = 0;
                        using (var indexFile = new StreamReader(indexPath))
                        {
                            string line;
                            while (indexFile.TryReadLine(out line))
                                if (!line.StartsWith(" "))
                                    ++numWords;
                        }

                        Dictionary<string, string> wordLine;
                        using (var indexFile = new StreamReader(indexPath))
                        {
                            string line;
                            // get lines in file, sorted by first column (i.e., the word)
                            wordLine = new Dictionary<string, string>(numWords);
                            while (indexFile.TryReadLine(out line))
                                // write header lines to temp file immediately
                                if (line.StartsWith(" "))
                                    tempFile.WriteLine(line);
                                else
                                {
                                    // trim useless blank spaces from line and map line to first column
                                    line = line.Trim();
                                    wordLine.Add(line.Substring(0, line.IndexOf(' ')), line);
                                }
                        }
                        // get sorted words
                        var sortedWords = new List<string>(wordLine.Count);
                        sortedWords.AddRange(wordLine.Keys);
                        sortedWords.Sort();

                        // write lines sorted by word
                        foreach (string word in sortedWords)
                            tempFile.WriteLine(wordLine[word]);

                    }

                    // replace original index file with properly sorted one
                    System.IO.File.Delete(indexPath);
                    System.IO.File.Move(tempPath, indexPath);
                }

                // create flag file, indicating that we've sorted the data
                using (var sortFlagFile = new StreamWriter(sortFlagPath))
                sortFlagFile.WriteLine("This file serves no purpose other than to indicate that the WordNet distribution data in the current directory has been sorted for use by the .NET API.");
            }
            #endregion

            #region engine init
            if (inMemory)
            {
                // pass 1:  get total number of synsets
                int totalSynsets = 0;
                foreach (string dataPath in dataPaths)
                {
                    // scan synset data file for lines that don't start with a space...these are synset definition lines
                    StreamReader dataFile = new StreamReader(dataPath);
                    string line;
                    while (dataFile.TryReadLine(out line))
                    {
                        int firstSpace = line.IndexOf(' ');
                        if (firstSpace > 0)
                            ++totalSynsets;
                    }
                }

                // pass 2:  create synset shells (pos and offset only)
                IdSynset = new Dictionary<string, SynSet>(totalSynsets);
                foreach (string dataPath in dataPaths)
                {
                    Pos pos = GetFilePos(dataPath);

                    // scan synset data file
                    StreamReader dataFile = new StreamReader(dataPath);
                    string line;
                    while (dataFile.TryReadLine(out line))
                    {
                        int firstSpace = line.IndexOf(' ');
                        if (firstSpace > 0)
                        {
                            // get offset and create synset shell
                            int offset = int.Parse(line.Substring(0, firstSpace));
                            SynSet synset = new SynSet(pos, offset, null);

                            IdSynset.Add(synset.Id, synset);
                        }
                    }
                }

                // pass 3:  instantiate synsets (hooks up relations, set glosses, etc.)
                foreach (string dataPath in dataPaths)
                {
                    Pos pos = GetFilePos(dataPath);
                    // scan synset data file
                    StreamReader dataFile = new StreamReader(dataPath);
                    string line;
                    while (dataFile.TryReadLine(out line))
                    {
                        int firstSpace = line.IndexOf(' ');
                        if (firstSpace > 0)
                            // instantiate synset defined on current line, using the instantiated synsets for all references
                            IdSynset[pos + ":" + int.Parse(line.Substring(0, firstSpace))].Instantiate(line, IdSynset);
                    }
                }

                // organize synsets by pos and words...also set most common synset for word-pos pairs that have multiple synsets
                posWordSynSets = new Dictionary<Pos, Dictionary<string, HashSet<SynSet>>>();

                foreach (string indexPath in indexPaths)
                {
                    Pos pos = GetFilePos(indexPath);

                    posWordSynSets.EnsureContainsKey(pos, typeof(Dictionary<string, HashSet<SynSet>>));

                    // scan word index file, skipping header lines
                    StreamReader indexFile = new StreamReader(indexPath);
                    string line;
                    while (indexFile.TryReadLine(out line))
                    {
                        int firstSpace = line.IndexOf(' ');
                        if (firstSpace > 0)
                        {
                            // grab word and synset shells, along with the most common synset
                            string word = line.Substring(0, firstSpace);
                            SynSet mostCommonSynSet;
                            HashSet<SynSet> synsets = GetSynSetShells(line, pos, out mostCommonSynSet, null);

                            // set flag on most common synset if it's ambiguous
                            if (synsets.Count > 1)
                                IdSynset[mostCommonSynSet.Id].SetAsMostCommonSynsetFor(word);

                            // use reference to the synsets that we instantiated in our three-pass routine above

                            var wordSynSet = new HashSet<SynSet>();
                            posWordSynSets[pos].Add(word, wordSynSet);
                            foreach (SynSet synset in synsets)
                                wordSynSet.Add(IdSynset[synset.Id]);
                        }
                    }
                }
            }
            else
            {
                // open binary search streams for index files
                posIndexWordSearchStream = new Dictionary<Pos, BinarySearchTextStream>();
                foreach (string indexPath in indexPaths)
                {
                    // create binary search stream for index file
                    BinarySearchTextStream searchStream = new BinarySearchTextStream(indexPath, delegate(object searchWord, string currentLine)
                    {
                        // if we landed on the header text, search further down
                        if (currentLine[0] == ' ')
                            return 1;

                        // get word on current line
                        string currentWord = currentLine.Substring(0, currentLine.IndexOf(' '));

                        // compare searched-for word to the current word
                        return string.Compare(((string)searchWord), currentWord, StringComparison.Ordinal);
                    });

                    // add search stream for current POS
                    posIndexWordSearchStream.Add(GetFilePos(indexPath), searchStream);
                }

                // open readers for synset data files
                posSynSetDataFile = new Dictionary<Pos, StreamReader>();
                foreach (string dataPath in dataPaths)
                    posSynSetDataFile.Add(GetFilePos(dataPath), new StreamReader(dataPath));
            }
            #endregion
        }

        #region synset retrieval
        ///<summary>
        ///Gets a synset
        ///</summary>
        ///<param name="synsetId">ID of synset in the format returned by SynSet.ID (i.e., POS:Offset)</param>
        ///<returns>SynSet</returns>
        public SynSet GetSynSet(string synsetId)
        {
            SynSet synset;
            if (InMemory)
                synset = IdSynset[synsetId];
            else
            {
                // get POS and offset
                int colonLoc = synsetId.IndexOf(':');
                Pos pos = (Pos)Enum.Parse(typeof(Pos), synsetId.Substring(0, colonLoc));
                int offset = int.Parse(synsetId.Substring(colonLoc + 1));

                // create shell and then instantiate
                synset = new SynSet(pos, offset, this);
                synset.Instantiate();
            }

            return synset;
        }

        ///<summary>
        ///Gets all synsets for a word, optionally restricting the returned synsets to one or more parts of speech. This
        ///method does not perform any morphological analysis to match up the given word. It does, however, replace all 
        ///spaces with underscores and call String.ToLower to normalize case.
        ///</summary>
        ///<param name="word">Word to get SynSets for. This method will replace all spaces with underscores and
        ///call ToLower() to normalize the word's case.</param>
        ///<param name="posRestriction">POSs to search. Cannot contain POS.None. Will search all POSs if no restriction
        ///is given.</param>
        ///<returns>Set of SynSets that contain word</returns>
        public HashSet<SynSet> GetSynSets(string word, params Pos[] posRestriction)
        {
            // use all POSs if none are supplied
            if (posRestriction == null || posRestriction.Length == 0)
                posRestriction = new[] { Pos.Adjective, Pos.Adverb, Pos.Noun, Pos.Verb };

            HashSet<Pos> posSet = new HashSet<Pos>(posRestriction);
            if (posSet.Contains(Pos.None))
                throw new Exception("Invalid SynSet POS request:  " + Pos.None);

            // all words are lower case and space-replaced
            word = word.ToLower().Replace(' ', '_');

            // gather synsets for each POS
            HashSet<SynSet> allSynsets = new HashSet<SynSet>();
            foreach (Pos pos in posSet)
                if (InMemory)
                {
                    // read instantiated synsets from memory
                    HashSet<SynSet> synsets;
                    if (posWordSynSets[pos].TryGetValue(word, out synsets))
                    {
                        // optimization:  if there are no more parts of speech to check, we have all the synsets - so set the return collection and make it read-only. this is faster than calling AddRange on a set.
                        if (posSet.Count == 1) allSynsets = synsets;
                        else foreach (var synset in synsets) allSynsets.Add(synset);
                    }
                }
                else
                {
                    // get index line for word
                    string indexLine = posIndexWordSearchStream[pos].Search(word);

                    // if index line exists, get synset shells and instantiate them
                    if (indexLine != null)
                    {
                        // get synset shells and instantiate them
                        SynSet mostCommonSynset;
                        HashSet<SynSet> synsets = GetSynSetShells(indexLine, pos, out mostCommonSynset, this);
                        foreach (SynSet synset in synsets)
                        {
                            synset.Instantiate();
                            allSynsets.Add(synset);
                        }

                        // we only need to set this flag if there is more than one synset for the word-pos pair
                        if (synsets.Count > 1)
                            mostCommonSynset.SetAsMostCommonSynsetFor(word);
                    }
                }

            return allSynsets;
        }

        ///<summary>
        ///Gets the most common synset for a given word/pos pair. This is only available for memory-based
        ///engines (see constructor).
        ///</summary>
        ///<param name="word">Word to get SynSets for. This method will replace all spaces with underscores and
        ///will call String.ToLower to normalize case.</param>
        ///<param name="pos">Part of speech to find</param>
        ///<returns>Most common synset for given word/pos pair</returns>
        public SynSet GetMostCommonSynSet(string word, Pos pos)
        {
            // all words are lower case and space-replaced...we need to do this here, even though it gets done in GetSynSets (we use it below)
            word = word.ToLower().Replace(' ', '_');

            // get synsets for word-pos pair
            HashSet<SynSet> synsets = GetSynSets(word, pos);

            // get most common synset
            SynSet mostCommon = null;
            if (synsets.Count == 1)
                return synsets.First();
            else if (synsets.Count > 1)
            {
                // one (and only one) of the synsets should be flagged as most common
                foreach (SynSet synset in synsets)
                    if (synset.IsMostCommonSynsetFor(word))
                        if (mostCommon == null)
                            mostCommon = synset;
                        else
                            throw new Exception("Multiple most common synsets found");

                if (mostCommon == null)
                    throw new NullReferenceException("Failed to find most common synset");
            }

            return mostCommon;
        }

        ///<summary>
        ///Gets definition line for synset from data file
        ///</summary>
        ///<param name="pos">POS to get definition for</param>
        ///<param name="offset">Offset into data file</param>
        internal string GetSynSetDefinition(Pos pos, int offset)
        {
            // set data file to synset location
            StreamReader dataFile = posSynSetDataFile[pos];
            dataFile.DiscardBufferedData();
            dataFile.BaseStream.Position = offset;

            // read synset definition
            string synSetDefinition = dataFile.ReadLine();

            // make sure file positions line up
            if (synSetDefinition != null && int.Parse(synSetDefinition.Substring(0, synSetDefinition.IndexOf(' '))) != offset)
                throw new Exception("Position mismatch:  passed " + offset + " and got definition line \"" + synSetDefinition + "\"");

            return synSetDefinition;
        }
        #endregion

        ///<summary>
        ///Closes all files and releases any resources assocated with this WordNet engine
        ///</summary>
        public void Close()
        {
            if (InMemory)
            {
                // release all in-memory resources
                posWordSynSets = null;
                IdSynset = null;
            }
            else
            {
                // close all index files
                foreach (BinarySearchTextStream stream in posIndexWordSearchStream.Values)
                    stream.Close();

                posIndexWordSearchStream = null;

                // close all data files
                foreach (StreamReader dataFile in posSynSetDataFile.Values)
                    dataFile.Close();

                posSynSetDataFile = null;
            }
        }
    }

    public static class WordNetEngineExtensions {
        public static void EnsureContainsKey<KeyType, ValueType>(
          this Dictionary<KeyType, ValueType> dictionary,
          KeyType key,
          Type valueType)
        {
            dictionary.EnsureContainsKey(key, valueType, null);
        }

        public static void EnsureContainsKey<KeyType, ValueType>(
          this Dictionary<KeyType, ValueType> dictionary,
          KeyType key,
          Type valueType,
          params object[] constructorParameters)
        {
            if (dictionary.ContainsKey(key))
                return;
            dictionary.Add(key, (ValueType)Activator.CreateInstance(valueType, constructorParameters));
        }

        public static bool TryReadLine(this StreamReader reader, out string line)
        {
            line = reader.ReadLine();
            if (line == null)
                reader.Close();
            return line != null;
        }

        public static void SetPosition(this StreamReader reader, long position)
        {
            reader.BaseStream.Position = position;
            reader.DiscardBufferedData();
        }
    }

}