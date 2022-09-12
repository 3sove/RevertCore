using Revert.Core.Common.Types.Tries;
using Revert.Core.Extensions;
using Revert.Core.Graph.Vertices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revert.Core.GuitarProReader
{
    public static class TabParser
    {
        public static int[] EmptyNoteArray { get; } = new int[] { -1 };

        public static void ParseTabs(string tabDirectory)
        {
            var tabsDirectory = new System.IO.DirectoryInfo(tabDirectory);

            var tabFiles = tabsDirectory.GetFiles("*.gp*", System.IO.SearchOption.AllDirectories).OrderByDescending(item => item.Length);

            //var bpmDistribution = new DistributionBuilder<TabFile, int>(tab => tab.Header?.AdditionalInfo?.Tempo != 0, tab => tab.Header.AdditionalInfo.Tempo);
            //var keyDistribution = new DistributionBuilder<TabFile, GuitarProReader.GuitarPro.Key>(tab => tab.Header?.AdditionalInfo?.Key != null, tab => tab.Header.AdditionalInfo.Key);
            //bpmDistribution.AddSample(tab);
            //keyDistribution.AddSample(tab);


            var measureNoteTrie = new Trie<int[]>(new IntArrayComparer());

            Trie<string, TabMetaData> chordTrie = new Trie<string, TabMetaData>();
            Dictionary<IEnumerable<string>, int> allPatterns = new Dictionary<IEnumerable<string>, int>(new StringEnumerableComparer());

            foreach (var tabFile in tabFiles)
            {
                ParseTab(tabFile, measureNoteTrie, chordTrie, allPatterns);
            }

            var chordKeys = chordTrie.AggregateKeys().OrderByDescending(item => item.Count * item.Last().KeyThree).ToList();
            RemoveRepeats(chordKeys);

            var orderedPatterns = allPatterns.OrderByDescending(item => item.Key.Count() * item.Value).ToList();
            RemoveRepeats(orderedPatterns);

            Console.WriteLine(orderedPatterns);
            Console.WriteLine(measureNoteTrie);
        }

        public static void ParseTab(System.IO.FileInfo tabFile, Trie<int[]> measureNoteTrie, Trie<string, TabMetaData> chordTrie, Dictionary<IEnumerable<string>, int> allPatterns)
        {
            try
            {
                using (var tabStream = tabFile.OpenRead())
                {
                    TabFile tab = null;
                    tab = TabFactory.CreateFromGp(tabStream);
                    if (tab == null) return;

                    var tabEntity = new Entity($"{tab.Header.Tablature.Author}:{tab.Header.Tablature.Title}", "Tabliture");

                    if (tab.Header?.AdditionalInfo?.Tempo != null) tabEntity.Features.AddDiscrete("Tempo", tab.Header.AdditionalInfo.Tempo);
                    if (tab.Header?.AdditionalInfo?.Key != null) tabEntity.Features.AddDiscrete("Key", (int)tab.Header.AdditionalInfo.Key);

                    var metaData = new TabMetaData() { Author = tab.Header.Tablature.Interpret, Title = tab.Header.Tablature.Title };


                    foreach (var track in tab.Tracks)
                    {
                        var trackChords = new List<string>();

                        foreach (var measure in track.Measures)
                        {
                            List<int[]> measureNotes = new List<int[]>();
                            int filledBeats = 0;

                            foreach (var beat in measure.Beats)
                            {
                                //Console.WriteLine(beat.NotesString);
                                var beatNotes = new List<int>();
                                for (int i = 0; i < beat.Notes.Count; i++)
                                {
                                    var gString = beat.Notes[i];
                                    if (gString == null || gString.Fret == -1) continue;

                                    int stringTuning = track.Tuning[i];

                                    beatNotes.Add(stringTuning + gString.Fret);
                                }
                                if (beatNotes.Any())
                                {
                                    filledBeats++;
                                    var noteArray = beatNotes.OrderBy(b => b).ToArray();
                                    measureNotes.Add(noteArray);

                                    if (beat.ChordDiagram != null)
                                    {
                                        trackChords.Add(CleanChordName(beat.ChordDiagram.Name));
                                        //Console.WriteLine(beat.ChordDiagram.Name);
                                    }
                                }
                                else
                                {
                                    measureNotes.Add(EmptyNoteArray);
                                }
                            }

                            if (filledBeats > 0)
                            {
                                measureNoteTrie.Add(measureNotes.ToArray());
                            }
                        }

                        if (trackChords.Any())
                        {
                            KeyValuePair<IEnumerable<string>, int>[] patterns = GetPatterns(trackChords);
                            foreach (var pattern in patterns)
                            {
                                int patternCount;
                                allPatterns.TryGetValue(pattern.Key, out patternCount);
                                allPatterns[pattern.Key] = patternCount + pattern.Value;

                                //Console.WriteLine($"{pattern.Value} - {pattern.Key.Combine(", ")}");
                            }

                            chordTrie = PopulateTrie(trackChords, metaData, 10, chordTrie);

                            Console.WriteLine(chordTrie);
                        }

                    }

                    if (tab.Header != null)
                        Console.WriteLine($"{tab.Header.Tablature.Interpret} - {tab.Header.Tablature.Title}");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetBaseException().Message);
            }
        }

        public static bool IsLoop<T>(T[] items)
        {
            var loopIndex = items.GetIndex(items[0], 1);
            if (loopIndex == -1) return false;
            //if (loopIndex == items.Length - 1) return false; //repeating the starting chord at the end is acceptable/meaningful

            for (int i = loopIndex; i < items.Length; i++)
                if (!items[i].Equals(items[i % loopIndex]))
                {
                    return false;
                }

            return true;
        }

        private static string[] GetRepeatedPattern(string[] input, int totalLength)
        {
            string[] output = new string[totalLength];

            int position = 0;
            while (position < output.Length)
            {
                output[position] = input[position % input.Length];
                position++;
            }

            return output;
        }

        private static void RemoveRepeats(List<KeyValuePair<IEnumerable<string>, int>> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var pattern = items[i];
                if (IsLoop(pattern.Key.ToArray()))
                {
                    Console.WriteLine($"Loop Detected: {pattern.Key.Combine(", ")}");
                    items.RemoveAt(i);
                    i--;
                }
            }
        }

        private static void RemoveRepeats(List<KeyAggregate<string, TabMetaData>> chordKeys)
        {
            for (int i = 0; i < chordKeys.Count; i++)
            {
                var item = chordKeys[i];
                if (IsLoop(item.Select(subItem => subItem.KeyOne).ToArray()))
                {
                    chordKeys.RemoveAt(i);
                    i--;
                }
            }
        }

        public static KeyValuePair<IEnumerable<string>, int>[] GetPatterns(List<string> strings)
        {
            Dictionary<IEnumerable<string>, int> patterns = new Dictionary<IEnumerable<string>, int>(new StringEnumerableComparer());
            for (int maxLength = 3; maxLength <= 10; maxLength++)
            {
                var newPatterns = GetPatterns(strings, maxLength);

                foreach (var pattern in newPatterns)
                {
                    int patternCount;
                    patterns.TryGetValue(pattern.Key, out patternCount);
                    patterns[pattern.Key] = patternCount + pattern.Value;
                }
            }
            var sortedPatterns = patterns.Where(item => item.Key.Count() >= 3 && item.Value > 5).OrderByDescending(item => item.Key.Count() * Math.Pow(item.Value, 2)).ToArray();
            return sortedPatterns;
        }

        public static Trie<string, TabMetaData> PopulateTrie(List<string> strings, TabMetaData metaData, int maxLength, Trie<string, TabMetaData> trie = null)
        {
            if (trie == null) trie = new Trie<string, TabMetaData>();

            for (int i = 0; i < strings.Count; i++)
            {
                var subSequence = strings.Skip(i).Take(maxLength.OrIfSmaller(strings.Count - i)).ToArray();
                trie.Add(subSequence, metaData);
            }

            return trie;
        }


        public static Dictionary<IEnumerable<string>, int> GetPatterns(List<string> strings, int maxLength)
        {
            var combinations = new List<List<string>>();
            //var trie = new Trie<string>();

            for (int i = 0; i < strings.Count; i++)
            {
                var currentString = strings[i];
                foreach (var combo in combinations)
                {
                    if (combo.Count < maxLength) combo.Add(currentString);
                }
                combinations.Add(new List<string>() { currentString });
            }

            var countedPatterns = new Dictionary<IEnumerable<string>, int>(new StringEnumerableComparer());

            foreach (var combo in combinations)
            {
                int count = 0;
                countedPatterns.TryGetValue(combo, out count);
                countedPatterns[combo] = count + 1;
            }

            Console.WriteLine(combinations);

            return countedPatterns;
        }


        private static string[] AtoG = new[] { "A", "B", "C", "D", "E", "F", "G" };
        private static string CleanChordName(string name)
        {
            var noteIndex = name.Length;
            foreach (var letter in AtoG)
            {
                var letterIndex = name.IndexOf(letter);
                if (letterIndex >= 0 && letterIndex < noteIndex) noteIndex = letterIndex;
            }
            name = name.Substring(noteIndex);
            name = name.Replace("\0", "");
            return name;
        }
    }
}
