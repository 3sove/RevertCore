namespace Revert.Core.Text.NLP.WordNet
{
    public partial class WordNetEngine
    {
        ///<summary>
        ///SynSet relations
        ///</summary>
        public enum SynSetRelation
        {
            None,
            AlsoSee,
            Antonym,
            Attribute,
            Cause,
            DerivationallyRelated,
            DerivedFromAdjective,
            Entailment,
            Hypernym,
            Hyponym,
            InstanceHypernym,
            InstanceHyponym,
            MemberHolonym,
            MemberMeronym,
            PartHolonym,
            ParticipleOfVerb,
            PartMeronym,
            Pertainym,
            RegionDomain,
            RegionDomainMember,
            SimilarTo,
            SubstanceHolonym,
            SubstanceMeronym,
            TopicDomain,
            TopicDomainMember,
            UsageDomain,
            UsageDomainMember,
            VerbGroup,
        }
    }
}