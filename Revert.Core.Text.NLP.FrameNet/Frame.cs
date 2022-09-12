using System;
using System.Collections.Generic;
using System.Linq;
using Revert.Core.Extensions;

namespace Revert.Core.Text.NLP.FrameNet
{
    /// <summary>
    /// Represents a FrameNet frame
    /// </summary>
    public class Frame
    {
        #region static members
        private static readonly Dictionary<string, FrameRelation> stringFrameRelations;

        public string GetNameWithInheritanceString()
        {
            if (SuperFrames.Count == 0) return Name;

            if (SuperFrames.Count == 1) return $"{Name} : {SuperFrames.First().GetNameWithInheritanceString()}";
            return $"{Name} : {{ {SuperFrames.Select(f => f.GetNameWithInheritanceString()).Combine(", ")} }}";
        }

        /// <summary>
        /// Different relations that can hold between two frames
        /// </summary>
        public enum FrameRelation
        {
            Inheritance,
            Subframe,
            Using,
            SeeAlso,
            ReframingMapping,
            CoreSet,
            Excludes,
            Requires,
            InchoativeOf,
            CausativeOf,
            Precedes,
            PerspectiveOn
        };

        /// <summary>
        /// Directions for frame relations
        /// </summary>
        public enum FrameRelationDirection
        {
            /// <summary>
            /// Super frame
            /// </summary>
            Super,

            /// <summary>
            /// Sub frame
            /// </summary>
            Sub,

            /// <summary>
            /// Indicate both super- and sub-frames
            /// </summary>
            Both
        }

        /// <summary>
        /// Equality operator
        /// </summary>
        public static bool operator == (Frame f1, Frame f2)
        {
            if (f1 == null) return (f2 == null);
            return f2 != null && f1.Equals(f2);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        public static bool operator !=(Frame f1, Frame f2)
        {
            return !(f1 == f2);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        static Frame()
        {
            stringFrameRelations = new Dictionary<string, FrameRelation>
                {
                    {"Inheritance", FrameRelation.Inheritance},
                    {"Subframe", FrameRelation.Subframe},
                    {"Using", FrameRelation.Using},
                    {"See_also", FrameRelation.SeeAlso},
                    {"ReFraming_Mapping", FrameRelation.ReframingMapping},
                    {"CoreSet", FrameRelation.CoreSet},
                    {"Excludes", FrameRelation.Excludes},
                    {"Requires", FrameRelation.Requires},
                    {"Inchoative_of", FrameRelation.InchoativeOf},
                    {"Causative_of", FrameRelation.CausativeOf},
                    {"Precedes", FrameRelation.Precedes},
                    {"Perspective_on", FrameRelation.PerspectiveOn}
                };
        }

        /// <summary>
        /// Gets a frame relation from it's XML attribute name
        /// </summary>
        /// <param name="relation">Relation name</param>
        /// <returns>Frame relation</returns>
        public static FrameRelation GetFrameRelation(string relation)
        {
            return stringFrameRelations[relation];
        }
        #endregion

        private readonly int hashCode;

        /// <summary>
        /// Gets frame elements in this frame
        /// </summary>
        public FrameElementSet FrameElements { get; private set; }

        /// <summary>
        /// Gets lexical units in this frame
        /// </summary>
        public HashSet<LexicalUnit> LexicalUnits { get; private set; }

        /// <summary>
        /// Gets the name of the frame. Both the ID and Name are unique identifiers.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the definition
        /// </summary>
        public string Definition { get; private set; }

        /// <summary>
        /// Gets ID for frame. Both the ID and Name are unique identifiers.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Relational sub frames
        /// </summary>
        public Dictionary<FrameRelation, HashSet<Frame>> RelationSubFramesByFrameRelation { get; private set; }
        
        /// <summary>
        /// Relational super frames
        /// </summary>
        public Dictionary<FrameRelation, HashSet<Frame>> RelationSuperFramesByFrameRelation { get; private set; }

        private static readonly HashSet<Frame> emptyHashSet = new HashSet<Frame>();
        public HashSet<Frame> SuperFrames 
        { 
            get
            {
                HashSet<Frame> frames;
                if (!RelationSuperFramesByFrameRelation.TryGetValue(FrameRelation.Inheritance, out frames)) return emptyHashSet;
                return frames ?? emptyHashSet;
            }
        }

        public HashSet<Frame> SubFrames
        {
            get
            {
                HashSet<Frame> frames;
                if (!RelationSubFramesByFrameRelation.TryGetValue(FrameRelation.Inheritance, out frames)) return emptyHashSet;
                return frames ?? emptyHashSet;
            }
        }

        public void CleanRelationalFrames()
        {
            var updatedRelatedFrames = new Dictionary<FrameRelation, HashSet<Frame>>();
            foreach (var item in RelationSubFramesByFrameRelation)
                if (item.Value.Count != 0) updatedRelatedFrames[item.Key] = item.Value;

            RelationSubFramesByFrameRelation = updatedRelatedFrames;

            updatedRelatedFrames = new Dictionary<FrameRelation, HashSet<Frame>>();
            foreach (var item in RelationSuperFramesByFrameRelation)
                if (item.Value.Count != 0) updatedRelatedFrames[item.Key] = item.Value;
            
            RelationSuperFramesByFrameRelation = updatedRelatedFrames;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of frame</param>
        /// <param name="definition">Definition of frame</param>
        /// <param name="id">ID of frame</param>
        public Frame(string name, string definition, int id)
        {
            Name = name;
            hashCode = Name.GetHashCode();
            Definition = definition;
            Id = id;
            FrameElements = new FrameElementSet();
            LexicalUnits = new HashSet<LexicalUnit>();

            // initialize empty sets of related frames
            RelationSubFramesByFrameRelation = new Dictionary<FrameRelation, HashSet<Frame>>();
            RelationSuperFramesByFrameRelation = new Dictionary<FrameRelation, HashSet<Frame>>();
            foreach (FrameRelation relation in Enum.GetValues(typeof(FrameRelation)))
            {
                // version 1.3 of framenet contains duplicate frame-frame relation mappings, so allow duplicate elements to be added to these sets
                RelationSubFramesByFrameRelation.Add(relation, new HashSet<Frame>());
                RelationSuperFramesByFrameRelation.Add(relation, new HashSet<Frame>());
            }
        }

        /// <summary>
        /// Gets a list of related frames
        /// </summary>
        /// <param name="relation">Type of relation to fetch</param>
        /// <param name="relationDirection">Relation direction</param>
        /// <param name="recursive">Whether or not to get related frames recursively</param>
        /// <returns>Set of frames</returns>
        public HashSet<Frame> GetRelatedFrames(FrameRelation relation, FrameRelationDirection relationDirection, bool recursive)
        {
            var frames = new HashSet<Frame>();
            GetRelatedFrames(relation, relationDirection, recursive, frames);
            return frames;
        }

        /// <summary>
        /// Gets a list of related frames
        /// </summary>
        /// <param name="relation">Type of relation to fetch</param>
        /// <param name="relationDirection">Relation direction</param>
        /// <param name="recursive">Whether or not to get related frames recursively</param>
        /// <param name="currentFrames">Current set of frames</param>
        /// <returns>Set of frames</returns>
        private void GetRelatedFrames(FrameRelation relation, FrameRelationDirection relationDirection, bool recursive, HashSet<Frame> currentFrames)
        {
            // add sub-frames
            if (relationDirection == FrameRelationDirection.Sub || relationDirection == FrameRelationDirection.Both)
            {
                HashSet<Frame> subFrames;
                if (RelationSubFramesByFrameRelation.TryGetValue(relation, out subFrames))
                foreach (var subFrame in subFrames)
                    if (!currentFrames.Contains(subFrame))
                    {
                        currentFrames.Add(subFrame);

                        // recursively add sub-frames
                        if (recursive)
                            subFrame.GetRelatedFrames(relation, relationDirection, true, currentFrames);
                    }
            }

            // add super-frames
            if (relationDirection == FrameRelationDirection.Super || relationDirection == FrameRelationDirection.Both)
            {
                HashSet<Frame> superFrames;
                if (RelationSuperFramesByFrameRelation.TryGetValue(relation, out superFrames))
                foreach (var superFrame in superFrames)
                    if (!currentFrames.Contains(superFrame))
                    {
                        currentFrames.Add(superFrame);

                        // recursively add super-frames
                        if (recursive) superFrame.GetRelatedFrames(relation, relationDirection, true, currentFrames);
                    }
            }
        }

        /// <summary>
        /// Gets the name of this frame
        /// </summary>
        /// <returns>Name of this frame</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Gets whether this frame equals another
        /// </summary>
        /// <param name="obj">Object for comparison</param>
        /// <returns>True if frames are equal, False otherwise</returns>
        public override bool Equals(object obj)
        {
            var frame = obj as Frame;
            return frame != null && Name == frame.Name;
        }

        /// <summary>
        /// Gets hashcode for this frame
        /// </summary>
        /// <returns>Hashcode for this frame</returns>
        public override int GetHashCode()
        {
            return hashCode;
        }

        /// <summary>
        /// Gets super-frames
        /// </summary>
        /// <param name="relation">Relation to query</param>
        /// <returns>Super-frames</returns>
        public HashSet<Frame> GetSuperFrames(FrameRelation relation)
        {
            return RelationSuperFramesByFrameRelation[relation];
        }

        /// <summary>
        /// Gets sub-frames
        /// </summary>
        /// <param name="relation">Relation to query</param>
        /// <returns>Sub-frames</returns>
        public HashSet<Frame> GetSubFrames(FrameRelation relation)
        {
            return RelationSubFramesByFrameRelation[relation];
        }
    }
}
