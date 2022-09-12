using System;
using System.Collections.Generic;
using System.IO;
using Revert.Core.Common.Text;
using Revert.Core.Extensions;
using System.Xml.Linq;

namespace Revert.Core.Text.NLP.FrameNet
{
    /// <summary>
    /// Provides indexing and retrieval functionality for the FrameNet database
    /// </summary>
    public class FrameNetEngine
    {
        /// <summary>
        /// FrameNet version number
        /// </summary>
        public enum Version
        {
            /// <summary>
            /// FrameNet 1.3
            /// </summary>
            FrameNet13,

            /// <summary>
            /// FrameNet 1.5
            /// </summary>
            FrameNet15,

            FrameNet17
        };

        private readonly Dictionary<string, Frame> frameNameFrame;
        private readonly Dictionary<int, FrameElement> frameElementIdFrameElement;
        private readonly LexicalUnitAnnotationEngine lexicalUnitAnnotationEngine;
        private readonly Dictionary<string, HashSet<int>> lexemeLexicalUnitIDs;
        private readonly Dictionary<int, Frame> lexicalUnitIdFrame;
        private readonly Dictionary<string, HashSet<int>> lexicalUnitLexicalUnitIDs;
        private readonly Dictionary<int, LexicalUnit> lexicalUnitIdLexicalUnit;

        /// <summary>
        /// Gets all frames
        /// </summary>
        public IEnumerable<Frame> Frames
        {
            get { return frameNameFrame.Values; }
        }

        /// <summary>
        /// Gets all frame names
        /// </summary>
        public IEnumerable<string> FrameNames
        {
            get { return frameNameFrame.Keys; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="frameNetDirectory">Path to FrameNet distribution directory</param>
        /// <param name="version">FrameNet version</param>
        public FrameNetEngine(string frameNetDirectory)
        {
            if (!Directory.Exists(frameNetDirectory))
                throw new DirectoryNotFoundException("Invalid FrameNet directory");

            frameNameFrame = new Dictionary<string, Frame>();
            frameElementIdFrameElement = new Dictionary<int, FrameElement>();
            lexemeLexicalUnitIDs = new Dictionary<string, HashSet<int>>();
            lexicalUnitIdFrame = new Dictionary<int, Frame>();
            lexicalUnitLexicalUnitIDs = new Dictionary<string, HashSet<int>>();
            lexicalUnitIdLexicalUnit = new Dictionary<int, LexicalUnit>();

            // init annotation engine
            lexicalUnitAnnotationEngine = new LexicalUnitAnnotationEngine(frameNetDirectory.AddFilePath("lu"), Version.FrameNet15);

            #region get frames
            foreach (var framePath in Directory.GetFiles(frameNetDirectory.AddFilePath("frame"), "*.xml"))
            {
                // create frame
                //var fP = System.Xml.XmlReader.Create(framePath);

                var frameDocument = XDocument.Parse(framePath.ReadText());
                var frameRoot = frameDocument.Root;
                var frameNameSpace = frameRoot.GetDefaultNamespace().NamespaceName;

                var frameID = int.Parse(frameRoot.Attribute("ID").Value); //int.Parse(frameP.AttributeValue("frame", "ID"));
                var frameName = frameRoot.Attribute("name").Value;
                var frameDefinition = frameRoot.Element(XName.Get("definition", frameNameSpace)).Value;

                var frame = new Frame(frameName, frameDefinition, frameID);

                // add to frame index index
                frameNameFrame.Add(frame.Name.ToLower(), frame);

                var frameElements = frameRoot.Elements(XName.Get("FE", frameNameSpace));
                // get frame elements
                foreach (var fe in frameElements)
                {
                    var feId = int.Parse(fe.Attribute("ID").Value);
                    var feName = fe.Attribute("name").Value;
                    var feDefinition = fe.Element(XName.Get("definition", frameNameSpace)).Value;
                    var frameElement = new FrameElement(feId, feName, feDefinition, frame);
                    frame.FrameElements.Add(frameElement);
                }

                var luXmls = frameRoot.Elements(XName.Get("lexUnit", frameNameSpace));

                foreach (var lu in luXmls)
                {
                    var luPos = lu.Attribute("POS").Value;
                    var luName = lu.Attribute("name").Value;
                    var luId = int.Parse(lu.Attribute("ID").Value);
                    var luDef = lu.Element(XName.Get("definition", frameNameSpace)).Value;

                    var lexemes = new List<Lexeme>();

                    var lexs = lu.Elements(XName.Get("lexeme", frameNameSpace));

                    foreach (var lex in lexs)
                    {
                        var head = bool.Parse(lex.Attribute("headword").Value);
                        var breakBefore = bool.Parse(lex.Attribute("breakBefore").Value);
                        var pos = lex.Attribute("POS").Value;
                        var lexName = lex.Attribute("name").Value;
                        var lexOrder = int.Parse(lex.Attribute("order").Value);
                        var lexeme = new Lexeme(lexName, pos, breakBefore, head, lexOrder);
                        lexemes.Add(lexeme);
                    }

                    var lexicalUnit = new LexicalUnit(luId, luName, luPos, luDef, lexemes);
                    frame.LexicalUnits.Add(lexicalUnit);

                    var lexemeString = lexicalUnit.ToString();

                    HashSet<int> lexemeUnitIDsSet;
                    if (!lexemeLexicalUnitIDs.TryGetValue(lexemeString, out lexemeUnitIDsSet))
                        lexemeLexicalUnitIDs[lexemeString] = (lexemeUnitIDsSet = new HashSet<int>());
                    lexemeUnitIDsSet.Add(luId);

                    // add map from lexical unit to frame
                    lexicalUnitIdFrame.Add(lexicalUnit.ID, frame);

                    // add map from lexical unit to lexical unit id
                    HashSet<int> lexicalUnitIds;

                    if (!lexicalUnitLexicalUnitIDs.TryGetValue(lexicalUnit.Name, out lexicalUnitIds))
                        lexicalUnitLexicalUnitIDs[lexicalUnit.Name] = (lexicalUnitIds = new HashSet<int>());
                    lexicalUnitIds.Add(lexicalUnit.ID);

                    // add map from lexical unit ID to lexical unit
                    lexicalUnitIdLexicalUnit.Add(lexicalUnit.ID, lexicalUnit);
                }
            }
            #endregion

            #region get relations

            var relationDocument = XDocument.Parse(frameNetDirectory.AddFilePath("frRelation.xml").ReadText());
            var relationRoot = relationDocument.Root;
            var relationNameSpace = relationRoot.GetDefaultNamespace().NamespaceName;

            var relationTypes = relationRoot.Elements(XName.Get("frameRelationType", relationNameSpace));
            foreach (var relationType in relationTypes)
            {
                var relation = Frame.GetFrameRelation(relationType.Attribute("name").Value);

                var frameRelations = relationType.Elements(XName.Get("frameRelation", relationNameSpace));
                foreach (var frameRelation in frameRelations)
                {
                    var subFrame = frameNameFrame[frameRelation.Attribute("subFrameName").Value.ToLower()];
                    var superFrame = frameNameFrame[frameRelation.Attribute("superFrameName").Value.ToLower()];

                    subFrame.GetSuperFrames(relation).Add(superFrame);
                    superFrame.GetSubFrames(relation).Add(subFrame);

                    var feRelations = frameRelation.Elements(XName.Get("FERelation", relationNameSpace));
                    foreach (var feRelation in feRelations)
                    {
                        var subFe = subFrame.FrameElements.Get(int.Parse(feRelation.Attribute("subID").Value));
                        var superFe = superFrame.FrameElements.Get(int.Parse(feRelation.Attribute("supID").Value));

                        subFe.AddSuperFrameElement(superFe, relation);
                        superFe.AddSubFrameElement(subFe, relation);
                    }
                }
            }

            #endregion

            foreach (var frame in Frames)
            {
                frame.CleanRelationalFrames();
                foreach (var frameElement in frame.FrameElements)
                {
                    frameElement.CleanRelationalFrames();
                }
            }

        }

        /// <summary>
        /// Gets a frame by name
        /// </summary>
        /// <param name="name">Name of frame to get</param>
        /// <returns>Frame</returns>
        public Frame GetFrame(string name)
        {
            name = name.ToLower();

            return frameNameFrame[name];
        }

        /// <summary>
        /// Tries to get a frame by name
        /// </summary>
        /// <param name="name">Name of frame to get</param>
        /// <param name="frame">Frame</param>
        /// <returns>Whether the frame was found</returns>
        public bool TryGetFrame(string name, out Frame frame)
        {
            return frameNameFrame.TryGetValue(name, out frame);
        }

        /// <summary>
        /// Gets a frame element
        /// </summary>
        /// <param name="frameName">Frame for which to get frame element</param>
        /// <param name="frameElementName">Name of frame element to get</param>
        /// <returns>Frame element</returns>
        public FrameElement GetFrameElement(string frameName, string frameElementName)
        {
            return GetFrame(frameName).FrameElements.Get(frameElementName);
        }

        /// <summary>
        /// Gets a frame element
        /// </summary>
        /// <param name="frameElement">Frame element, in Frame.FrameElement notation</param>
        /// <returns>Frame element</returns>
        public FrameElement GetFrameElement(string frameElement)
        {
            var dotLoc = frameElement.IndexOf('.');
            return GetFrame(frameElement.Substring(0, dotLoc)).FrameElements.Get(frameElement.Substring(dotLoc + 1));
        }

        /// <summary>
        /// Checks whether or not a frame exists in the database
        /// </summary>
        /// <param name="name">Name of frame to check for</param>
        /// <returns>True if frame exists, false otherwise</returns>
        public bool Contains(string name)
        {
            name = name.ToLower();

            return frameNameFrame.ContainsKey(name);
        }

        /// <summary>
        /// Gets all attestations for a frame
        /// </summary>
        /// <param name="frame">Frame to get attestations for</param>
        /// <returns>List of attestations</returns>
        public List<Attestation> GetAttestationsForFrame(Frame frame)
        {
            return GetAttestationsForFrame(frame, PartsOfSpeech.All);
        }

        /// <summary>
        /// Gets all attestations for a frame
        /// </summary>
        /// <param name="frame">Frame to get attestations for</param>
        /// <param name="partOfSpeech">Part of speech for attestation</param>
        /// <returns>List of attestations</returns>
        public List<Attestation> GetAttestationsForFrame(Frame frame, PartsOfSpeech partOfSpeech)
        {
            var attestations = new List<Attestation>();
            foreach (var lu in frame.LexicalUnits)
                if (partOfSpeech.HasFlag(lu.PartOfSpeech))
                    attestations.AddRange(lexicalUnitAnnotationEngine.GetAttestations(frame, lu.ID));

            return attestations;
        }

        /// <summary>
        /// Gets the set of frames evoked by a lexeme
        /// </summary>
        /// <param name="lexeme">Lexeme to get frames for</param>
        /// <returns>Set of frames</returns>
        public HashSet<Frame> GetFramesForLexeme(string lexeme)
        {
            var frames = new HashSet<Frame>();

            HashSet<int> luIDs;
            if (lexemeLexicalUnitIDs.TryGetValue(lexeme.ToLower(), out luIDs))
                foreach (var luID in luIDs)
                    frames.Add(lexicalUnitIdFrame[luID]);
            else
                throw new Exception("FrameNet does not contain lexeme");

            return frames;
        }

        /// <summary>
        /// Gets the set of frames evoked by a lexical unit (includes frames for all contained lexemes)
        /// </summary>
        /// <param name="lexicalUnit">Lexical unit to get frames for</param>
        /// <returns>Set of frames</returns>
        public HashSet<Frame> GetFramesForLexicalUnit(string lexicalUnit)
        {
            var frames = new HashSet<Frame>();

            HashSet<int> luIDs;
            if (lexicalUnitLexicalUnitIDs.TryGetValue(lexicalUnit, out luIDs))
                foreach (var luID in luIDs)
                    frames.Add(lexicalUnitIdFrame[luID]);
            else
                throw new Exception("FrameNet does not contain lexical unit");

            return frames;
        }

        /// <summary>
        /// Checks whether or not a lexeme is in FrameNet
        /// </summary>
        /// <param name="lexeme">Lexeme to check</param>
        /// <returns>True if lexeme is contained, false otherwise</returns>
        public bool ContainsLexeme(string lexeme)
        {
            return lexemeLexicalUnitIDs.ContainsKey(lexeme);
        }

        /// <summary>
        /// Checks whether or not a lexical unit is in FrameNet
        /// </summary>
        /// <param name="lexicalUnit"></param>
        /// <returns></returns>
        public bool ContainsLexicalUnit(string lexicalUnit)
        {
            return lexicalUnitLexicalUnitIDs.ContainsKey(lexicalUnit);
        }
    }
}
