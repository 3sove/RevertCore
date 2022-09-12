using System;

namespace Revert.Core.Common.Text
{
    [Flags]
    public enum PartsOfSpeech
    {
        None = 0,
        All = int.MaxValue,
        Unknown = 1,
        Noun = 1 << 1,
        Plural = 1 << 2,
        NounPhrase = 1 << 3,
        Verb = 1 << 4,
        TransitiveVerb = 1 << 5,
        IntransitiveVerb = 1 << 6,
        Adjective = 1 << 7,
        Adverb = 1 << 8,
        Conjunction = 1 << 9,
        Preposition = 1 << 10,
        Interjection = 1 << 11,
        Pronoun = 1 << 12,
        DefiniteArticle = 1 << 13,
        IndefiniteArticle = 1 << 14,
        Nominative = 1 << 15,
        SubordinateConjunction = 1 << 16,
        Number = 1 << 17,
        WordNetFlags = Noun | Verb | Adverb | Adjective,
        NonWordNetFlags = Unknown | Plural | NounPhrase | TransitiveVerb | IntransitiveVerb | Conjunction | Preposition | Interjection | Pronoun | DefiniteArticle | IndefiniteArticle | Nominative
    }
}
