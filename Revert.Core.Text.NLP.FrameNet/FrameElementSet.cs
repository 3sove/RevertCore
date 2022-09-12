using System;
using System.Collections.Generic;
using System.Linq;

namespace Revert.Core.Text.NLP.FrameNet
{
    /// <summary>
    /// Represents a set of frame elements
    /// </summary>
    public class FrameElementSet
    {
        private readonly Dictionary<int, FrameElement> idFrameElement;
        private readonly Dictionary<string, HashSet<FrameElement>> nameFrameElements;

        /// <summary>
        /// Gets number of frame elements in set
        /// </summary>
        public int Count
        {
            get { return FrameElements.Count; }
        }

        public HashSet<FrameElement> FrameElements { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public FrameElementSet()
        {
            FrameElements = new HashSet<FrameElement>();
            idFrameElement = new Dictionary<int, FrameElement>();
            nameFrameElements = new Dictionary<string, HashSet<FrameElement>>();
        }

        /// <summary>
        /// Adds a frame element to this set
        /// </summary>
        /// <param name="frameElement">Frame element to add</param>
        public void Add(FrameElement frameElement)
        {
            FrameElements.Add(frameElement);
            idFrameElement.Add(frameElement.Id, frameElement);

            var lowerName = frameElement.Name.ToLower();

            HashSet<FrameElement> nameFrameElementItems;
            if (!nameFrameElements.TryGetValue(lowerName, out nameFrameElementItems))
                nameFrameElements[lowerName] = (nameFrameElementItems = new HashSet<FrameElement>());
            nameFrameElementItems.Add(frameElement);
        }

        /// <summary>
        /// Removes a frae element from this set
        /// </summary>
        /// <param name="frameElement">Frame element to remove</param>
        public void Remove(FrameElement frameElement)
        {
            FrameElements.Remove(frameElement);
            idFrameElement.Remove(frameElement.Id);
            nameFrameElements[frameElement.Name.ToLower()].Remove(frameElement);
        }
        
        /// <summary>
        /// Gets a frame element in the set by its ID
        /// </summary>
        /// <param name="id">ID of FE to get</param>
        /// <returns>FE with specified ID</returns>
        public FrameElement Get(int id)
        {
            return idFrameElement[id];
        }

        /// <summary>
        /// Gets a frame element in the set by its name
        /// </summary>
        /// <param name="name">Name of frame element to get</param>
        /// <returns>FE with specified name</returns>
        public FrameElement Get(string name)
        {
            name = name.ToLower();

            if (nameFrameElements[name].Count != 1)
                throw new Exception("Multiple frame elements present!");

            return nameFrameElements[name].First();
        }

        /// <summary>
        /// Checks whether or not this set contains a frame element
        /// </summary>
        /// <param name="name">Name of frame element to check for</param>
        /// <returns>True if frame element is present, false otherwise</returns>
        public bool Contains(string name)
        {
            return nameFrameElements.ContainsKey(name.ToLower());
        }

        /// <summary>
        /// Checks for a frame element
        /// </summary>
        /// <param name="frameElement">Frame element to check for</param>
        /// <returns>True if frame element is present and false otherwise</returns>
        public bool Contains(FrameElement frameElement)
        {
            return FrameElements.Contains(frameElement);
        }

        /// <summary>
        /// Checks for a frame element
        /// </summary>
        /// <param name="id">ID of frame element to check for</param>
        /// <returns>True if frame element is present and false otherwise</returns>
        public bool Contains(int id)
        {
            return idFrameElement.ContainsKey(id);
        }

        /// <summary>
        /// Gets enumerator over frame elements
        /// </summary>
        /// <returns></returns>
        public IEnumerator<FrameElement> GetEnumerator()
        {
            return FrameElements.GetEnumerator();
        }

        /// <summary>
        /// Gets number of frame elements in set
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return FrameElements.ToString();
        }
    }
}
