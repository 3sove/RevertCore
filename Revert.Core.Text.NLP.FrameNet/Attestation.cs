using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Text.NLP.FrameNet
{
    /// <summary>
    /// Represents a single attestation for a frame
    /// </summary>
    public class Attestation
    {
        /// <summary>
        /// Gets or sets the frame element bindings for this attestation
        /// </summary>
        public Dictionary<FrameElement, List<AnnotatedSpan>> AnnotatedSpansByFrameElement { get; set; }

        /// <summary>
        /// Gets or sets the targets for this attestation
        /// </summary>
        public List<AnnotatedSpan> Targets { get; set; }

        /// <summary>
        /// Gets or sets the sentence
        /// </summary>
        public string Sentence { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Attestation()
        {
            AnnotatedSpansByFrameElement = new Dictionary<FrameElement, List<AnnotatedSpan>>();
            Targets = new List<AnnotatedSpan>();
        }

        /// <summary>
        /// Gets nicely formatted string for current attestation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var attestation = new StringBuilder();
            attestation.AppendLine(Sentence);

            // add FE bindings
            foreach (var fe in AnnotatedSpansByFrameElement.Keys)
            {
                attestation.AppendLine("\t\"" + fe.Name + "\" bindings:  ");
                foreach (var aSpan in AnnotatedSpansByFrameElement[fe])
                    attestation.AppendLine("\t\t" + aSpan);
            }

            // add target
            foreach (var aSpan in Targets)
                attestation.AppendLine("\tTarget:  " + aSpan);

            return attestation.ToString().Trim();
        }
    }
}
