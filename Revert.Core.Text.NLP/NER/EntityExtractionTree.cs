using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revert.Core.Common.Types.Tries;
using Revert.Core.Extensions;
using Revert.Core.Text.NLP.SentenceMaps;

namespace Revert.Core.Text.NLP
{
    //public class EntityExtractionTree : Trie<Word, EntityExtractionTreeNode, SentenceToken>
    //{
    //    public string FrameNetDirectoryPath { get; set; }
    //    public EnglishDictionary EnglishDictionary { get; set; }

    //    public EntityExtractionTree(string frameNetDirectoryPath, EnglishDictionary dictionary)
    //    {
    //        FrameNetDirectoryPath = frameNetDirectoryPath; //NLP\Data\FrameNet\
    //        EnglishDictionary = dictionary;
    //        AnnotatedSentenceMaps = new AnnotatedSentenceMaps(FrameNetDirectoryPath, this);
    //    }
        
    //    public AnnotatedSentenceMaps AnnotatedSentenceMaps { get; }

    //    public void PopulateTree()
    //    {
    //        var maps = AnnotatedSentenceMaps.GetStringRepresentationMaps();
    //        PopulateTree(maps);
    //    }

    //    private void PopulateTree(List<AnnotatedSentenceMap> maps)
    //    {
    //        Console.WriteLine("Populating sentence mapping data into tree.");

    //        //spawn 16 new threads to handle the maps
    //        var mapChunks = maps.SplitCollection();

    //        Parallel.For(0, mapChunks.Count, mapIndex =>
    //            {
    //                List<AnnotatedSentenceMap> mapChunk;
    //                lock (mapChunks)
    //                {
    //                    mapChunk = mapChunks[mapIndex];
    //                    if (mapChunk == null) return;
    //                }

    //                for (var i = 0; i < mapChunk.Count; i++)
    //                {
    //                    if ((i % 1000) == 0)
    //                        Console.WriteLine("Thread {0}: Populating sentence mapping data {1} to {2} of {3}.", mapIndex, i,
    //                                          (i + 1000).OrIfSmaller(mapChunk.Count), mapChunk.Count);

    //                    var map = mapChunk[i];
    //                    var node = GetFirstNode(map);
    //                    node.Populate(map);
    //                }
    //            });

    //        //for (var i = 0; i < maps.Count; i++)
    //        //{
    //        //    if ((i % 1000) == 0) Console.WriteLine("Populating sentence mapping data {0} to {1} of {2}.", i, (i + 1000).OrIfSmaller(maps.Count), maps.Count);

    //        //    var map = maps[i];
    //        //    var node = GetFirstNode(map);
    //        //    node.Populate(map);
    //        //}
    //    }

    //    private EntityExtractionTreeNode GetFirstNode(AnnotatedSentenceMap map)
    //    {
    //        var sentenceToken = map.Tokens[0];
    //        var token = sentenceToken.Word;

    //        EntityExtractionTreeNode nextNode;
    //        if (!TryGetChild(token, out nextNode))
    //        {
    //            nextNode = new EntityExtractionTreeNode { Token = sentenceToken };
    //            AddChild(token, nextNode);
    //        }
    //        return nextNode;
    //    }

    //    public List<EntityExtractionTreeNode> GetChildTreeNodes()
    //    {
    //        lock (Children)
    //        {
    //            return Children.ToList();
    //        }
    //    }

    //    public string StringUpToNode => string.Empty;
    //}
}
