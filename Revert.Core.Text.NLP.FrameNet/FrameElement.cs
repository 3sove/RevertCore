using System;
using System.Collections.Generic;

namespace Revert.Core.Text.NLP.FrameNet
{
    /// <summary>
    /// Represents a frame element within FrameNet
    /// </summary>
    public class FrameElement
    {
        private Dictionary<Frame.FrameRelation, FrameElementSet> relationSubFrameElements;
        private Dictionary<Frame.FrameRelation, FrameElementSet> relationSuperFrameElements;

        // used for searching to track the path back to the originating frame elemetn
        private FrameElement frameElementSearchBackPointer;
        private Frame.FrameRelation frameRelationSearchBackPointer;
        private Frame.FrameRelationDirection frameRelationDirectionSearchBackPointer;

        /// <summary>
        /// Gets the ID
        /// </summary>
        public int Id { get; private set; }

        private int HashCode { get; set; }

        /// <summary>
        /// Gets the name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the frame that contains this frame element
        /// </summary>
        public Frame Frame { get; private set; }




        /// <summary>
        /// Gets the definition
        /// </summary>
        public string Definition { get; private set; }


        private static readonly HashSet<FrameElement> emptyHashSet = new HashSet<FrameElement>();
        public HashSet<FrameElement> SuperFrameElements
        {
            get
            {
                FrameElementSet frameElementSet;
                if (!relationSuperFrameElements.TryGetValue(Frame.FrameRelation.Inheritance, out frameElementSet)) return emptyHashSet;
                return frameElementSet.FrameElements ?? emptyHashSet;
            }
        }

        public HashSet<FrameElement> SubFrameElements
        {
            get
            {
                FrameElementSet frameElementSet;
                if (!relationSubFrameElements.TryGetValue(Frame.FrameRelation.Inheritance, out frameElementSet)) return emptyHashSet;
                return frameElementSet.FrameElements ?? emptyHashSet;
            }
        }



        public void CleanRelationalFrames()
        {
            var updatedRelatedFrames = new Dictionary<Frame.FrameRelation, FrameElementSet>();
            foreach (var item in relationSubFrameElements)
            {
                if (item.Value.Count != 0) updatedRelatedFrames[item.Key] = item.Value;
            }
            relationSubFrameElements = updatedRelatedFrames;

            updatedRelatedFrames = new Dictionary<Frame.FrameRelation, FrameElementSet>();
            foreach (var item in relationSuperFrameElements)
            {
                if (item.Value.Count != 0) updatedRelatedFrames[item.Key] = item.Value;
            }
            relationSuperFrameElements = updatedRelatedFrames;
        }

        private static Dictionary<string, int> frameElementNameCountByName = new Dictionary<string, int>(); 
        public static Dictionary<string, int> FrameElementNameCountByName
        {
            get { return frameElementNameCountByName; }
            set { frameElementNameCountByName = value; }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">ID of FE</param>
        /// <param name="name">Name of FE</param>
        /// <param name="definition">Definition of FE</param>
        /// <param name="frame">Frame that contains this frame element</param>
        public FrameElement(int id, string name, string definition, Frame frame)
        {
            Id = id;
            HashCode = Id.GetHashCode();
            Name = name;

            int count;
            frameElementNameCountByName.TryGetValue(name, out count);
            frameElementNameCountByName[name] = ++count;

            Definition = definition;
            Frame = frame;

            relationSubFrameElements = new Dictionary<Frame.FrameRelation, FrameElementSet>();
            relationSuperFrameElements = new Dictionary<Frame.FrameRelation, FrameElementSet>();

            // initialize empty lists of related frame elements
            foreach (Frame.FrameRelation relation in Enum.GetValues(typeof(Frame.FrameRelation)))
            {
                relationSubFrameElements.Add(relation, new FrameElementSet());
                relationSuperFrameElements.Add(relation, new FrameElementSet());
            }
        }

        /// <summary>
        /// Equals override
        /// </summary>
        /// <param name="obj">Object to compare this one to</param>
        /// <returns>True if object is a FE with the same ID, False otherwise</returns>
        public override bool Equals(object obj)
        {
            var fe = obj as FrameElement;
            return Id == fe?.Id;
        }

        /// <summary>
        /// Gets the name of the frame element in Frame.FrameElement notation
        /// </summary>
        /// <returns>Name of this FE</returns>
        public override string ToString()
        {
            return $"{Frame}.{Name}";
        }

        /// <summary>
        /// GetHashCode override
        /// </summary>
        /// <returns>Hashcode for this FE</returns>
        public override int GetHashCode()
        {
            return HashCode;
        }

        /// <summary>
        /// Adds a frame element to the sub-FE collection for a relation
        /// </summary>
        /// <param name="frameElement">Frame element to add</param>
        /// <param name="relation">Relation between current and given frame element</param>
        internal void AddSubFrameElement(FrameElement frameElement, Frame.FrameRelation relation)
        {
            relationSubFrameElements[relation].Add(frameElement);
        }

        /// <summary>
        /// Adds a frame element to the super-FE collection for a relation
        /// </summary>
        /// <param name="frameElement">Frame element to add</param>
        /// <param name="relation">Relation between current and given frame element</param>
        internal void AddSuperFrameElement(FrameElement frameElement, Frame.FrameRelation relation)
        {
            relationSuperFrameElements[relation].Add(frameElement);
        }

        /// <summary>
        /// Gets a list of related frame element
        /// </summary>
        /// <param name="relation">Type of relation to fetch</param>
        /// <param name="relationDirection">Relation direction</param>
        /// <param name="recursive">Whether or not to recursively get related frame elements</param>        
        /// <returns>List of FEs</returns>
        public FrameElementSet GetRelatedFrameElements(Frame.FrameRelation relation, Frame.FrameRelationDirection relationDirection, bool recursive)
        {
            var relatedFEs = new FrameElementSet();
            GetRelatedFrameElements(relation, relationDirection, recursive, relatedFEs);
            return relatedFEs;
        }

        /// <summary>
        /// Gets list of related frame elements
        /// </summary>
        /// <param name="relation">Type of relation to fetch</param>
        /// <param name="relationDirection">Relation direction</param>
        /// <param name="recursive">Whether or not to recursively get related frame elements</param>
        /// <param name="currentFEs">Current list of related FEs</param>
        private void GetRelatedFrameElements(Frame.FrameRelation relation, Frame.FrameRelationDirection relationDirection, bool recursive, FrameElementSet currentFEs)
        {
            // add sub-FEs
            if (relationDirection == Frame.FrameRelationDirection.Sub || relationDirection == Frame.FrameRelationDirection.Both)
            {
                FrameElementSet frameElements;
                if (relationSubFrameElements.TryGetValue(relation, out frameElements))
                foreach (var subFe in frameElements)
                    if (!currentFEs.Contains(subFe))
                    {
                        currentFEs.Add(subFe);

                        // recursively add sub-FEs
                        if (recursive)
                            subFe.GetRelatedFrameElements(relation, relationDirection, true, currentFEs);
                    }
            }

            // add super-FEs
            if (relationDirection == Frame.FrameRelationDirection.Super || relationDirection == Frame.FrameRelationDirection.Both)
            {
                FrameElementSet frameElements;
                if (relationSuperFrameElements.TryGetValue(relation, out frameElements))
                foreach (var superFe in frameElements)
                    if (!currentFEs.Contains(superFe))
                    {
                        currentFEs.Add(superFe);

                        // recursively add super-FEs
                        if (recursive)
                            superFe.GetRelatedFrameElements(relation, relationDirection, true, currentFEs);
                    }
            }
        }

        /// <summary>
        /// Gets the shortest network path from the current frame element to another frame element
        /// </summary>
        /// <param name="destinationFrameElement">Destination frame element</param>
        /// <param name="searchRelations">Relations to search</param>
        /// <param name="searchDirection">Relation direction to search</param>
        /// <param name="maxDepth">Maximum depth to search within the network (i.e., maximum distance destination frame element can be from the current one)</param>
        /// <param name="frameElementPath">Path from this frame element to the destination frame element, or null for no path</param>
        /// <param name="relationPath">Relation path between this frame element and the destination frame element, or null for no path</param>
        /// <param name="relationDirectionPath">Relation direction path between this frame element and the destination frame element, or null for no path</param>
        /// <returns>True if path exists, false otherwise</returns>
        public bool GetShortestPathTo(FrameElement destinationFrameElement,
                                      HashSet<Frame.FrameRelation> searchRelations,
                                      Frame.FrameRelationDirection searchDirection,
                                      int maxDepth,
                                      out List<FrameElement> frameElementPath,
                                      out List<Frame.FrameRelation> relationPath,
                                      out List<Frame.FrameRelationDirection> relationDirectionPath)
        {
            frameElementPath = null;
            relationPath = null;
            relationDirectionPath = null;

            // breadth-first search originating at the current frame element
            var searchQueue = new Queue<FrameElement>();
            frameElementSearchBackPointer = null;  // make sure to null out the source frame element back pointer
            searchQueue.Enqueue(this);

            var frameElementsEncountered = new HashSet<FrameElement> { this };  // keep track of frame elements we see so we don't enter any cycles

            var currentDepth = 0;                // tracks current search depth
            var nodesAtCurrentDepth = 1;         // tracks nodes at current search depth
            var nodesAtCurrentDepthPlusOne = 0;  // tracks nodes at one beyond the current search depth

            while (searchQueue.Count > 0 && currentDepth <= maxDepth)
            {
                var currentFrameElement = searchQueue.Dequeue();

                // check for destination frame element
                if (Equals(currentFrameElement, destinationFrameElement))
                {
                    // create path by following backpointers
                    frameElementPath = new List<FrameElement>();
                    relationPath = new List<Frame.FrameRelation>();
                    relationDirectionPath = new List<Frame.FrameRelationDirection>();
                    while (destinationFrameElement != null)
                    {
                        frameElementPath.Add(destinationFrameElement);

                        // back up to previous frame element
                        var previousFrameElement = destinationFrameElement.frameElementSearchBackPointer;

                        // if the previous frame element isn't null, record the relationship
                        if (previousFrameElement != null)
                        {
                            relationPath.Add(destinationFrameElement.frameRelationSearchBackPointer);
                            relationDirectionPath.Add(destinationFrameElement.frameRelationDirectionSearchBackPointer);
                        }

                        destinationFrameElement = previousFrameElement;
                    }

                    // reverse paths to be from the current to the destination frame elements
                    frameElementPath.Reverse();
                    relationPath.Reverse();
                    relationDirectionPath.Reverse();

                    if (!Equals(frameElementPath[0], this))
                        throw new Exception("Path should start at current frame element");

                    if (frameElementPath.Count != relationPath.Count + 1 || frameElementPath.Count != relationDirectionPath.Count + 1)
                        throw new Exception("Path length mismatch between frame elements and relations/directions");

                    if (frameElementPath.Count - 1 > maxDepth)
                        throw new Exception("Exceeded maximum allowed search depth");

                    return true;
                }

                // queue up frame elements related to the current one by any of the given relations
                var nodesAdded = 0;
                foreach (var searchRelation in searchRelations)
                {
                    // add sub-FEs
                    if (searchDirection == Frame.FrameRelationDirection.Sub || searchDirection == Frame.FrameRelationDirection.Both)
                    {
                        FrameElementSet set;
                        if (!currentFrameElement.relationSubFrameElements.TryGetValue(searchRelation, out set)) continue;
                        foreach (var subFe in set)
                            if (!frameElementsEncountered.Contains(subFe))
                            {
                                subFe.frameElementSearchBackPointer = currentFrameElement;
                                subFe.frameRelationSearchBackPointer = searchRelation;
                                subFe.frameRelationDirectionSearchBackPointer = Frame.FrameRelationDirection.Sub;

                                searchQueue.Enqueue(subFe);
                                frameElementsEncountered.Add(subFe);

                                ++nodesAdded;
                            }
                    }

                    // add super-FEs
                    if (searchDirection == Frame.FrameRelationDirection.Super || searchDirection == Frame.FrameRelationDirection.Both)
                        foreach (var superFe in currentFrameElement.relationSuperFrameElements[searchRelation])
                            if (!frameElementsEncountered.Contains(superFe))
                            {
                                superFe.frameElementSearchBackPointer = currentFrameElement;
                                superFe.frameRelationSearchBackPointer = searchRelation;
                                superFe.frameRelationDirectionSearchBackPointer = Frame.FrameRelationDirection.Super;

                                searchQueue.Enqueue(superFe);
                                frameElementsEncountered.Add(superFe);

                                ++nodesAdded;
                            }
                }

                // all generated search nodes belong in the next depth level
                nodesAtCurrentDepthPlusOne += nodesAdded;

                // if there aren't any nodes left at the current depth level, move to next level out
                if (--nodesAtCurrentDepth == 0)
                {
                    nodesAtCurrentDepth = nodesAtCurrentDepthPlusOne;
                    nodesAtCurrentDepthPlusOne = 0;
                    currentDepth++;
                }
            }

            return false;
        }
    }
}
