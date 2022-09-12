using System.Collections.Generic;
using System.Text;
using Revert.Core.Common.Text;

namespace Revert.Core.Text.NLP.FrameNet
{
    /// <summary>
    /// Represents a lexical unit
    /// </summary>
    public class LexicalUnit
    {
        private readonly int id;
        private readonly int hashCode;
        private readonly string definition;
        private readonly string name;
        private readonly PartsOfSpeech partOfSpeech;
        private readonly List<Lexeme> lexemes;

        /// <summary>
        /// Gets the lexemes on this lexical unit
        /// </summary>
        public List<Lexeme> Lexemes
        {
            get { return lexemes; }
        }

        /// <summary>
        /// Gets the ID
        /// </summary>
        public int ID
        {
            get { return id; }
        }

        /// <summary>
        /// Gets the name
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the partOfSpeech
        /// </summary>
        public PartsOfSpeech PartOfSpeech
        {
            get { return partOfSpeech; }
        }

        /// <summary>
        /// Gets the definition
        /// </summary>
        public string Definition
        {
            get { return definition; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">ID for LU</param>
        /// <param name="name">Name of LU</param>
        /// <param name="partOfSpeech">partOfSpeech of LU</param>
        /// <param name="definition">Definition of LU</param>
        /// <param name="lexemes">Lexemes on this lexical unit</param>
        public LexicalUnit(int id, string name, string partOfSpeech, string definition, List<Lexeme> lexemes)
        {
            this.id = id;
            this.name = name;
            this.partOfSpeech = Lexeme.PartsOfSpeechByFrameNetString[partOfSpeech];
            this.definition = definition;
            this.lexemes = lexemes;
            this.lexemes.Sort();
            hashCode = this.id.GetHashCode();
        }

        /// <summary>
        /// Gets a concatenation of all lexemes defined on this lexunit
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var text = new StringBuilder();
            var prependSpace = false;
            foreach (var lexeme in lexemes)
            {
                text.Append((prependSpace ? " " : "") + lexeme);
                prependSpace = true;
            }

            return text.ToString();
        }

        /// <summary>
        /// Gets hash code for this lexical unit
        /// </summary>
        /// <returns>Hash code for this lexical unit</returns>
        public override int GetHashCode()
        {
            return hashCode;
        }

        /// <summary>
        /// Checks whether or not this lexical unit equals another
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var lu = obj as LexicalUnit;
            return lu != null && id == lu.ID;
        }
    }
}
