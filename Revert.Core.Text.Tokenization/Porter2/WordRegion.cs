using System;

namespace Revert.Core.Text.Tokenization.Porter2
{
    internal class WordRegion
    {
        public WordRegion()
        {
        }

        public WordRegion(int start, int end)
        {
            Start = start;
            End = end;
        }

        public int Start;
        public int End;

        public string Text { get; private set; }


        internal bool Contains(int index)
        {
            return (index >= Start && index <= End);
        }

        internal void GenerateRegion(string text)
        {
            this.Text = text.Length > Start ? text.Substring(Start, Math.Min(End, text.Length) - Start) : string.Empty;
        }
    }
}