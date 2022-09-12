using System;
using System.Diagnostics;

namespace Revert.Core.Text.NLP.FrameNet
{
    [DebuggerDisplay("Span<{Name} : {Value}>")]
    public class AnnotatedSpan
    {
        public int Start { get; }

        public int End
        {
            get { return Start + Length - 1; }
        }

        public int Length
        {
            get { return Value.Length; }
        }

        public string Value { get; }

        public string Name { get; }

        public AnnotatedSpan(int start, string value, string name)
        {
            if (start < 0 || value == null)
                throw new Exception("Invalid span parameters");

            this.Start = start;
            this.Value = value;
            this.Name = name;
        }

        /// <summary>
        /// Returns the value of the span
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value;
        }
    }
}
