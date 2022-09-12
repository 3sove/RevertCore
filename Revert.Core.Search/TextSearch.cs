using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.Threading.Tasks;
using MongoDB.Bson;
using Revert.Core.Search.Nodes;
using Revert.Core.Search.ErrorReporting;
using Revert.Core.Extensions;
using Revert.Core.Common;

namespace Revert.Core.Search
{
    public class TextSearch
    {
        public List<LexicalTokenizer> AllTokens; // tokens parsed from search string - needed in class for highlighting feature

        public Node NodeTree;

        public bool IsSearchStringValid
        {
            get { return NodeTree != null; }
        }

        public string ErrorMessage { get; } = string.Empty;

        public List<string> ParseOfSearchStringWarningMessages = new List<string>();

        public string OriginalSearchString { get; set; }
        public string SearchString { get; set; }

        internal static ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        public List<LexicalTokenizer> IllegalCharacters;

        /// <summary>
        /// The search class takes a search string.  It then processes that string into tokens.  
        /// Those tokens are used to create a node tree which contains all of the elements of the search string along with the appropriate keywords like AND, OR, and NOT.
        /// </summary>
        /// <param name="SearchString">This is a list of terms that a user is searching for.  
        /// The list can contain terms joined together with the 'AND', 'OR', and 'NOT' keywords.  
        /// Terms can be grouped by parenthesis and these expressions can be nested.
        /// </param>
        public TextSearch(string SearchString)
        {
            this.SearchString = SearchString;
            OriginalSearchString = SearchString;

            AllTokens = new List<LexicalTokenizer>();
            IllegalCharacters = new List<LexicalTokenizer>();
            ErrorTrack errorTrack = new ErrorTrack();

            // Step through the SearchString and create tokens
            LexicalTokenizer.GetTokens(SearchString, ref AllTokens, ref IllegalCharacters);

            // Step through the tokens and create a node tree
            NodeTree = OrNode.Parser.TryParse(AllTokens, errorTrack);

            if (NodeTree == null)
            {
                ErrorMessage = errorTrack.GetError();
            }
            else
            {
                if (NodeTree.RemainingTokens.Count > 0)
                {
                    ParseOfSearchStringWarningMessages.Add("There are characters at the end of the search, beginning at position " +
                        NodeTree.RemainingTokens[0].Begin.ToString() + ", that could not be processed.");
                }

                if (IllegalCharacters.Count > 0)
                {
                    ParseOfSearchStringWarningMessages.Add("There are illegal characters in the search, but they have been removed.");
                    ParseOfSearchStringWarningMessages.Add("The first illegal character is at position " + IllegalCharacters[0].Begin.ToString());
                    ParseOfSearchStringWarningMessages.Add("If you wih to search for these characters, please place them in double quotes.");
                }
            }
        }

        /// <summary>
        /// The Evaluate method does the actual search that a user wants to perform.  Evaluate takes a text string (textToSearch) as input.  
        /// This is the text that the user wants to run their search against.  
        /// The NodeTree that was created in the constructor contains the search terms that will be searched for in the text string (textToSearch) parameter.
        /// </summary>
        /// <param name="textToSearch">This is the block of text that a search should be performed on.</param>
        /// <returns>True is returned if the search was successful.  If the search did not find anything, false is returned.</returns>
        public bool Evaluate(string textToSearch)
        {
            return NodeTree.Eval(textToSearch);
        }

        public bool Evaluate<TValue>(ISearchable<ObjectId, TValue> searchable, out IEnumerable<TValue> results)
        {
            return NodeTree.Evaluate(searchable, out results);
        }
        

        public string HighlightSearchTerms_CSS(string textToSearch, string cssClass = "searchHighlight", List<long> valueIDs = null)
        {
            if (string.IsNullOrWhiteSpace(textToSearch)) return textToSearch;

            // First get a list of terms, from the textToSearch string, that match the search criteria
            List<MatchedCoordinate> matchesList = NodeTree.GetMatchedCoordinates(textToSearch, false, valueIDs);

            // No need to continue if nothing matched
            if (matchesList == null) return textToSearch;

            //Build boolean array representing the textToSearch.  Each element of the array represents a character in the textToSearch string
            bool[] boolTextData = new bool[textToSearch.Length];

            //Go through list of matches; for each match, take the start & end position and set the corresponding array elements to true; all other array elements are false
            foreach (MatchedCoordinate match in matchesList)
            {
                for (int i = match.Beginning; i < match.Ending; i++)
                {
                    boolTextData[i] = true;
                }
            }

            // Build a new list of matches
            List<MatchedCoordinate> newMatches = new List<MatchedCoordinate>();

            bool matchStarted = false;
            MatchedCoordinate newMatch = new MatchedCoordinate();

            //Go through the boolean array.  At each element, check to see if it is true or false.  If it is true we are either at the beginning, or in the middle of a matched term
            for (int i = 0; i < boolTextData.Length; i++)
            {
                if (boolTextData[i])
                {
                    if (!matchStarted)
                    {
                        //The array element is true and we have NOT started a 'match'; so create the new list item ('match') and set its Beginning to the 
                        //element that we are currently on, then set the flag to indicate a 'match' has started.
                        newMatch = new MatchedCoordinate();
                        newMatch.Beginning = i;
                        matchStarted = true;
                    }
                }
                else
                {
                    if (matchStarted)
                    {
                        //Here, the array element was false, but a 'match' was started; so end the 'match' by setting its Ending element to the element that we are currently on,
                        //then set the flag to indicate that a 'match' has NOT been started.  Lastly, add the 'match' to the list.
                        newMatch.Ending = i - 1;
                        matchStarted = false;
                        newMatches.Add(newMatch);
                    }
                }
            }

            //Check for ending match
            if (matchStarted)
            {
                newMatch.Ending = boolTextData.Length - 1;
                matchStarted = false;
                newMatches.Add(newMatch);
            }

            // Now with the new list of matches, wrap the <span> element around each one in the actual TextData
            StringBuilder sb = new StringBuilder();
            int charPosition = 0;

            foreach (MatchedCoordinate matchStr in newMatches)
            {
                //Add elements up to the 'match' into the new string(builder) then add in the opening <span> tag, then the match itself, then the closing </span> tag.
                sb.Append(textToSearch.Substring(charPosition, matchStr.Beginning - charPosition));
                sb.Append($"<span class='{cssClass}'>");
                sb.Append(textToSearch.Substring(matchStr.Beginning, matchStr.Ending - matchStr.Beginning + 1));
                sb.Append("</span>");

                // Lastly, update the charPosition pointer
                charPosition = matchStr.Ending + 1;
            }

            sb.Append(textToSearch.Substring(charPosition, textToSearch.Length - charPosition));

            return sb.ToString();
        }

        /// <summary>
        /// GetHTMLEncodedDisplayText returns a string containing HTML code.  The string is the search terms that were submitted for a search.  
        /// The search terms are surronded with "span" elements which include a CSS class.  The CSS rules can be defined with the 'color:' element.  
        /// This way, the search term will appear in different colors.
        /// </summary>
        /// <returns>The returned string contains HTML code with 'span' tags wrapped around the search terms.</returns>
        public string GetHTMLEncodedDisplayText()
        {
            StringBuilder sb = new StringBuilder();
            foreach (LexicalTokenizer token in AllTokens)
            {
                sb.Append(token.HTMLEncodedDisplayText);
                sb.Append(" ");
            }
            return sb.ToString();
        }

        public string GenerateSummary(string TextToSummarize, int MaxLengthOfTextToReturn, int MIN_LENGTH_PER_TERM)
        {
            //Call BuildSentences() method which will simply go through the TextToSummarize and split it into a list of sentences.
            List<string> sentences = BuildSentences(TextToSummarize);
            string[] ucSentences = new string[sentences.Count];
            for (int i = 0; i < ucSentences.Length; i++)
            {
                ucSentences[i] = sentences[i].ToUpper();
            }

            //Now we have a list of sentences.  Now we need to create a new list of sentences, but this list will only contain sentences that have a search term within them.
            //But first, we will create a new list of tokens.  This new list will only contain data tokens, which means that keyword tokens like 'AND', 'OR', and 'NOT' will not be in this list.
            List<string> searchTermTokens = new List<string>();

            foreach (LexicalTokenizer lexToken in AllTokens)
            {
                if (lexToken is Numeric || lexToken is AlphaNumeric || lexToken is Alpha || lexToken is QuotedString)
                {
                    searchTermTokens.Add(lexToken.Token);
                }
            }

            //OK, now let's create that list of sentences that actually have a search term in them.
            Dictionary<string, List<string>> searchTermSentences = new Dictionary<string, List<string>>();

            //First, we will go through the list of tokens.  For each token, we will go through the list of sentences.  
            //If the particular sentence contains the search term, that sentence will be saved in a list of sentences that contain the particular search term.

            foreach (string token in searchTermTokens)
            {
                string ucToken = token;
                List<string> sentencesWithTerms = new List<string>();

                for (int i = 0; i < ucSentences.Length; i++)
                {
                    if (ucSentences[i].Contains(ucToken)) sentencesWithTerms.Add(sentences[i]);
                }

                //We have gone through all of the sentences and found matches for the current search term.  We now have a list of just those sentences that contained the current search term.
                //Add this list (of sentences) to the dictionary.
                if (sentencesWithTerms.Count > 0)
                {
                    List<string> existingStrings = null;
                    if (searchTermSentences.TryGetValue(token, out existingStrings) == true) existingStrings.AddRange(sentencesWithTerms);
                    else searchTermSentences.Add(token, sentencesWithTerms);
                }
            }

            int maxSentenceLength = MaxLengthOfTextToReturn / searchTermTokens.Count;
            maxSentenceLength = (maxSentenceLength > MIN_LENGTH_PER_TERM) ? maxSentenceLength : MIN_LENGTH_PER_TERM;

            List<string> stringsToReturn = GatherSentences(searchTermSentences, maxSentenceLength);

            string stringOfSentences = string.Empty;

            int charCounter = 0;
            foreach (string tempSentence in stringsToReturn)
            {
                charCounter += tempSentence.Length;
                if (charCounter >= MaxLengthOfTextToReturn) break;

                stringOfSentences += tempSentence;
                stringOfSentences += "\n\n";
            }

            //Just in case nothing was found based on the search terms
            if (stringOfSentences == string.Empty)
            {
                if (TextToSummarize.Length < MaxLengthOfTextToReturn)
                    stringOfSentences = TextToSummarize;
                else
                    stringOfSentences = TextToSummarize.Substring(0, MaxLengthOfTextToReturn - 4) + "...";
            }

            return stringOfSentences;
        }
        
        //Helper function for GenerateSummary() method
        private List<string> BuildSentences(string TextToSummarize)
        {
            List<string> sentences = new List<string>();
            int firstChar = 0;
            string sentence;

            //Create a list of sentences from the TextToSummarize string
            for (int i = 0; i < TextToSummarize.Length; i++)
            {
                char c = TextToSummarize[i];

                //Skip over spaces, tabs, etc... that could be at the beginning of a sentence
                if (firstChar == 0)
                {
                    if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
                        continue;
                    else
                        firstChar = i;
                }

                //Check for punctuation signifying the end of a sentence
                if (c == '.' || c == '!' || c == '?')
                {
                    //Pointing at last char?
                    if (i == TextToSummarize.Length - 1)
                    {
                        // Last char, build sentence
                        sentence = TextToSummarize.Substring_From_To(firstChar, i);
                        sentences.Add(sentence);
                    }
                    else
                    {
                        //Not last char, check for space or tab or carriage return
                        char cn = TextToSummarize[i + 1];
                        if (cn == ' ' || cn == '\t' || cn == '\r' || cn == '\n')
                        {
                            // Got one; sentence complete.
                            sentence = TextToSummarize.Substring_From_To(firstChar, i);
                            sentences.Add(sentence);
                            firstChar = 0;
                        }
                    }
                }
                else
                {
                    //There was no punctuation indicating the end of a sentence, but a blank line 
                    //following this sentence also indicates the end of a sentence.
                    if (c == '\r')
                    {
                        if (TextToSummarize.Length >= i + 4)
                        {
                            if (TextToSummarize.Substring(i, 4) == "\r\n\r\n")
                            {
                                sentence = TextToSummarize.Substring_From_To(firstChar, i);
                                sentences.Add(sentence);
                                firstChar = 0;
                            }
                        }
                    }
                }
                continue;
            }
            //We've gone through the TextToSummarize, but if there was no punctuation at the end, then we don't have the last sentence.  
            //So, check if the last sentence was created, if not, create it and add it to the list.
            sentences.Add(TextToSummarize.Substring(firstChar));
            return sentences;
        }

        // Helper function for GenerateSummary() method
        private List<string> GatherSentences(Dictionary<string, List<string>> searchTermSentences, int maxSentenceLength)
        {
            List<string> stringsToReturn = new List<string>();
            List<string> fullStrings = new List<string>();

            // Find out the maximum number of sentences per token in the dictionary
            int maxNumberOfSentences = 0;
            foreach (string tokenString in searchTermSentences.Keys)
            {
                //Set maxNumberOfSentences to the number of sentences for the particular search term, or itself...whichever is greater.
                maxNumberOfSentences = searchTermSentences[tokenString].Count.OrIfLarger(maxNumberOfSentences);
            }

            for (int i = 0; i < maxNumberOfSentences; i++)
            {
                foreach (string tokenString in searchTermSentences.Keys)
                {
                    //For the particular search term, do we have as many sentences as our current counter?  If not, go to the next term.
                    if (searchTermSentences[tokenString].Count <= i) continue;

                    string singleSentence = searchTermSentences[tokenString][i].ToString();

                    if (fullStrings.Contains(singleSentence)) continue;

                    // Is the current sentence within the maxSentenceLength?
                    fullStrings.Add(singleSentence);
                    if (singleSentence.Length <= maxSentenceLength)
                    {
                        stringsToReturn.Add(singleSentence);
                    }
                    else
                    {
                        // Sentence is too long; find the part of the sentence surrounding the search term
                        int startCharPos = 0;
                        int endCharPos = 0;
                        int startTerm = singleSentence.ToUpper().IndexOf(tokenString.ToUpper());
                        int charsOnEachSide = (maxSentenceLength - tokenString.Length) / 2;

                        //Calculate the starting position using the charsOnEachSide value;
                        //Find the beginning of the term and go back by the amount of charsOnEachSide, but if this is too far, use 0
                        startCharPos = (startTerm - charsOnEachSide < 0) ? 0 : startTerm - charsOnEachSide;

                        //Now that we have the starting character position, adjust it so it starts at the beginning of a word.
                        if (startCharPos > 0)
                        {
                            if (singleSentence[startCharPos] == ' ')
                            {
                                while (startCharPos < singleSentence.Length && singleSentence[startCharPos] == ' ')
                                    startCharPos++;
                            }
                            else
                            {
                                if (singleSentence[startCharPos - 1] != ' ')
                                {
                                    while (startCharPos < singleSentence.Length && singleSentence[startCharPos] != ' ')
                                        startCharPos++;

                                    // Now we are at a space...so advance until we are NOT at a space
                                    while (startCharPos < singleSentence.Length && singleSentence[startCharPos] == ' ')
                                        startCharPos++;
                                }
                            }
                        }

                        //Calculate the ending position using charsOnEachSide;  From the end of the term add charsOnEachSide.  
                        //If this goes beyond the length of the string, then use the last position in the string. Otherwise, use the end of the search term plus charsOnEachSide
                        endCharPos = (startTerm + tokenString.Length + charsOnEachSide > singleSentence.Length) ?
                            singleSentence.Length - 1 : startTerm + tokenString.Length + charsOnEachSide;

                        //Now that we have the starting character position, adjust it so it starts at the beginning of a word.
                        if (endCharPos < singleSentence.Length - 1)
                        {
                            if (singleSentence[endCharPos] == ' ')
                            {
                                while (endCharPos > -1 && singleSentence[endCharPos] == ' ')
                                    while (endCharPos > -1 && (endCharPos < singleSentence.Length && singleSentence[endCharPos] == ' '))
                                        endCharPos--;
                            }
                            else
                            {
                                if (singleSentence[endCharPos + 1] != ' ')
                                {
                                    while (endCharPos > -1 && singleSentence[endCharPos] != ' ')
                                        endCharPos--;

                                    while (endCharPos > -1 && singleSentence[endCharPos] == ' ')
                                        endCharPos--;
                                }
                            }

                            endCharPos++;
                        }

                        // Now, add '...' to the beginning or end of the string if it was truncated on either side.
                        string newSentence = string.Empty;
                        if (startCharPos > 0)
                        {
                            newSentence = "...";
                        }

                        if ((endCharPos - startCharPos) > 1)
                        {
                            newSentence += singleSentence.Substring(startCharPos, endCharPos - startCharPos);
                        }

                        if (endCharPos < singleSentence.Length - 1)
                        {
                            newSentence += "...";
                        }

                        stringsToReturn.Add(newSentence);
                    }
                }
            }
            return stringsToReturn;
        }
    }
}