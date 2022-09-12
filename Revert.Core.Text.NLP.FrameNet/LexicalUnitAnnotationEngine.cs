using Revert.Core.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Revert.Core.Text.NLP.FrameNet
{
    /// <summary>
    /// Lexical unit annotation engine
    /// </summary>
    public class LexicalUnitAnnotationEngine
    {
        private readonly string annotationDirectory;
        private readonly FrameNetEngine.Version version;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="annotationDirectory">Lexical unit annotation directory</param>
        /// <param name="version">FrameNet version</param>
        public LexicalUnitAnnotationEngine(string annotationDirectory, FrameNetEngine.Version version)
        {
            if (!Directory.Exists(annotationDirectory))
                throw new DirectoryNotFoundException("Invalid lexical unit annotation directory");

            this.annotationDirectory = annotationDirectory;
            this.version = version;
        }

        /// <summary>
        /// Gets annotations for a lexical unit
        /// </summary>
        /// <param name="frame">Frame for which we're getting annotations</param>
        /// <param name="lexicalUnitID">ID of lexical unit for which to get annotations</param>
        /// <returns>Annotation information</returns>
        public List<Attestation> GetAttestations(Frame frame, int lexicalUnitID)
        {
            var attestations = new List<Attestation>();

            var attestationFilePath = annotationDirectory.AddFilePath($"lu{lexicalUnitID}.xml");
            if (!File.Exists(attestationFilePath))
                return attestations;

            var attestationDocument = XDocument.Parse(attestationFilePath.ReadText());
            var attestationRoot = attestationDocument.Root;
            var attestationNameSpace = attestationRoot.GetDefaultNamespace().NamespaceName;

            if (version == FrameNetEngine.Version.FrameNet15)
            {
                var subCorpi = attestationRoot.Elements(XName.Get("subCorpus", attestationNameSpace));

                foreach (var subCorpus in subCorpi)
                {
                    var sentences = subCorpus.Elements(XName.Get("sentence", attestationNameSpace));

                    foreach (var sentence in sentences)
                    {
                        var attestation = new Attestation();
                        attestations.Add(attestation);
                        attestation.Sentence = sentence.Element(XName.Get("text", attestationNameSpace)).Value;
                        var annotationSets = sentence.Elements(XName.Get("annotationSet", attestationNameSpace));

                        foreach (var annotationSet in annotationSets)
                        {
                            var layers = annotationSet.Elements(XName.Get("layer", attestationNameSpace));

                            foreach (var layer in layers)
                            {
                                var layerName = layer.Attribute(XName.Get("name")).Value;

                                if (layerName != "FE" && layerName != "BNC" && layerName != "Target") continue;

                                var labels = layer.Elements(XName.Get("label", attestationNameSpace));
                                foreach (var label in labels)
                                {
                                    if (!label.Attributes(XName.Get("start")).Any())
                                        continue;

                                    var start = int.Parse(label.Attribute(XName.Get("start")).Value);
                                    var end = int.Parse(label.Attribute(XName.Get("end")).Value);
                                    var name = label.Attribute(XName.Get("name")).Value;
                                    var text = attestation.Sentence.Substring(start, end - start + 1);
                                    var span = new AnnotatedSpan(start, text, name);
                                    attestation.Targets.Add(span);

                                    if (layerName == "FE")
                                    {
                                        var id = int.Parse(label.Attribute(XName.Get("feID")).Value);

                                        if (frame.FrameElements.Contains(id))
                                        {
                                            var frameElement = frame.FrameElements.Get(id);
                                            List<AnnotatedSpan> annotatedSpans;
                                            if (!attestation.AnnotatedSpansByFrameElement.TryGetValue(frameElement, out annotatedSpans))
                                                attestation.AnnotatedSpansByFrameElement[frameElement] = (annotatedSpans = new List<AnnotatedSpan>());
                                            annotatedSpans.Add(span);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Unrecognized FrameNet version:  " + version);
            }
            return attestations;
        }
    }
}
