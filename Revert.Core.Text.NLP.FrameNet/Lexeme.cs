using System;
using System.Collections.Generic;
using Revert.Core.Common.Text;

namespace Revert.Core.Text.NLP.FrameNet
{
    /// <summary>
    /// Represents a lexeme
    /// </summary>
    public class Lexeme : IComparable<Lexeme>
    {
        private readonly string value;
        private readonly int hashCode;
        private readonly PartsOfSpeech partOfSpeech;
        private readonly bool breakBefore;
        private readonly bool head;
        private readonly int order;

        /// <summary>
        /// Gets the partOfSpeech
        /// </summary>
        public PartsOfSpeech PartOfSpeech
        {
            get { return partOfSpeech; }
        }

        /// <summary>
        /// Gets the value of this lexeme
        /// </summary>
        public string Value
        {
            get { return value; }
        }

        /// <summary>
        /// Gets whether or not words may be inserted before this lexeme (for multi-word LUs)
        /// </summary>
        public bool BreakBefore
        {
            get { return breakBefore; }
        }

        /// <summary>
        /// Gets whether or not this lexeme is the head of its lexical unit
        /// </summary>
        public bool Head
        {
            get { return head; }
        }

        /// <summary>
        /// Gets the order of this lexeme
        /// </summary>
        public int Order
        {
            get { return order; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="value">KeyTwo of lexeme</param>
        /// <param name="partOfSpeech">Part of speech</param>
        /// <param name="breakBefore">Whether or not words may be inserted before this lexeme (for multi-word LUs)</param>
        /// <param name="head">Whether or not this lexeme is the head of its LU</param>
        /// <param name="order">Order of multi-lexeme lexical unit</param>
        public Lexeme(string value, string partOfSpeech, bool breakBefore, bool head, int order)
        {
            this.value = value;
            this.partOfSpeech = PartsOfSpeechByFrameNetString[partOfSpeech];
            this.breakBefore = breakBefore;
            this.head = head;
            this.order = order;
            hashCode = this.value.GetHashCode();
        }

        public static Dictionary<string, PartsOfSpeech> PartsOfSpeechByFrameNetString = new Dictionary<string, PartsOfSpeech>()
        {
            { "V", PartsOfSpeech.Verb },
            { "N", PartsOfSpeech.Noun },
            { "A", PartsOfSpeech.Adjective },
            { "PREP", PartsOfSpeech.Preposition },
            { "ADV", PartsOfSpeech.Adverb },
            { "C", PartsOfSpeech.Conjunction },
            { "ART", PartsOfSpeech.DefiniteArticle | PartsOfSpeech.IndefiniteArticle },
            { "INTJ", PartsOfSpeech.Interjection },
            { "PRON", PartsOfSpeech.Pronoun },
            { "NUM", PartsOfSpeech.Number },
            { "SCON", PartsOfSpeech.SubordinateConjunction }
        };


        /// <summary>
        /// Checks whether or not this lexeme equals another
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Lexeme))
                return false;

            var lexeme = obj as Lexeme;

            return value == lexeme.Value && breakBefore == lexeme.BreakBefore && head == lexeme.Head && partOfSpeech == lexeme.PartOfSpeech && order == lexeme.Order;
        }

        /// <summary>
        /// Gets hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return hashCode;
        }

        /// <summary>
        /// Returns value of lexeme
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return value;
        }

        /// <summary>
        /// Compares this lexeme to another one
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Lexeme other)
        {
            return order.CompareTo(other.Order);
        }
    }
}
