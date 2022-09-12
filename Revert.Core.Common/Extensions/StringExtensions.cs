using Revert.Core.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Revert.Core.Extensions
{
    public static class StringExtensions
    {
        private const string Quote = "\"";
        private const string EscapedQuote = "\"\"";
        private static readonly char[] charactersThatMustBeQuoted = { ',', '"', '\n' };

        private static readonly StringBuilder stringBuilder = new StringBuilder();

        private static readonly HashSet<char> safeStartingCharacters = new HashSet<char> { '#', '@', '\'', '"', '“', '”', '‘', '’', '«', '»', '‹', '›', '„', '“', '‚', '‘', '(', '[', '{', '.', '*', '-' };
        private static readonly HashSet<char> safeCharacters = new HashSet<char> { '#', '@', '-', '\'', '"', '“', '”', '‘', '’', '«', '»', '‹', '›', '„', '“', '‚', '‘', '(', ')', '[', ']', '{', '}', '.', '*' };
        private static readonly HashSet<char> invalidEndingCharacters = new HashSet<char> { '.', '\n', '\r' };

        private static readonly StringBuilder sbCombiner = new StringBuilder();

        private static readonly string[] OrdinalNumberSuffixes = { "st", "nd", "rd", "th" };
        public static int GetInteger(this string value, int defaultValue = 0)
        {
            foreach (var suffix in OrdinalNumberSuffixes)
                if (value.EndsWith(suffix))
                    value = value.Substring(0, value.Length - 2);

            int returnValue = 0;
            if (!int.TryParse(value.Replace(",", "").Replace("$", "").Replace(" ", ""), out returnValue))
                return defaultValue;
            return returnValue;
        }


        public static string NormalizePhoneNumber(this string phoneNumber)
        {
            if (phoneNumber == null) return string.Empty;
            var number = new string(phoneNumber.Trim().Where(char.IsNumber).ToArray());
            number = number.TrimStart('0');
            if (number.Length > 12) number = number.TrimEnd('0');
            return number;
        }

        public static string ToCsvString(this string s)
        {
            if (s == null) return string.Empty;
            if (s.Contains(Quote)) s = s.Replace(Quote, EscapedQuote);
            if (s.IndexOfAny(charactersThatMustBeQuoted) > -1) s = Quote + s + Quote;
            return s;
        }


        public static string ToSentenceCase(this string s, bool ignoreMixedCase = false)
        {
            if (ignoreMixedCase)
                if (!s.Any(char.IsLower) || !s.Any(char.IsUpper)) return s;

            var delimiter = ' ';
            // Check null/empty
            if (string.IsNullOrEmpty(s))
                return s;

            s = s.Trim();
            if (string.IsNullOrEmpty(s))
                return s;

            // Only 1 token
            if (s.IndexOf(delimiter) < 0)
            {
                s = s.ToLower();
                s = s[0].ToString().ToUpper() + s.Substring(1);
                return s;
            }

            // More than 1 token.
            var tokens = s.Split(delimiter);
            var buffer = new StringBuilder();

            foreach (var token in tokens)
            {
                var currentToken = token.ToLower();
                currentToken = currentToken[0].ToString().ToUpper() + currentToken.Substring(1);
                buffer.Append(currentToken + delimiter);
            }

            s = buffer.ToString();
            return s.TrimEnd(delimiter);
        }

        public static List<string> GetSentences(this string textToSummarize)
        {
            var sentences = new List<string>();
            if (string.IsNullOrWhiteSpace(textToSummarize)) return sentences;
            var firstChar = -1;

            //Create a list of sentences from the TextToSummarize string
            for (var i = 0; i < textToSummarize.Length; i++)
            {
                var c = textToSummarize[i];

                //Skip over spaces, tabs, etc... that could be at the beginning of a sentence
                if (firstChar == -1)
                {
                    if (c == ' ' || c == '\t' || c == '\r' || c == '\n') continue;
                    firstChar = i;
                }

                //Check for punctuation signifying the end of a sentence
                if (c == '.' || c == '!' || c == '?')
                {
                    //Pointing at last char?
                    if (i == textToSummarize.Length - 1)
                    {
                        // Last char, build sentence
                        sentences.Add(textToSummarize.Substring_From_To(firstChar, i));
                    }
                    else
                    {
                        //Not last char, check for space or tab or carriage return
                        var cn = textToSummarize[i + 1];
                        if (cn == ' ' || cn == '\t' || cn == '\r' || cn == '\n')
                        {
                            // Got one; sentence complete.
                            sentences.Add(textToSummarize.Substring_From_To(firstChar, i));
                            firstChar = i + 1;
                        }
                    }
                }
                else if (c == '\r')
                {
                    if (textToSummarize.Length >= i + 4)
                    {
                        if (textToSummarize.Substring(i, 4) == "\r\n\r\n")
                        {
                            sentences.Add(textToSummarize.Substring_From_To(firstChar, i));
                            firstChar = 0;
                        }
                    }
                }
            }
            //We've gone through the TextToSummarize, but if there was no punctuation at the end, then we don't have the last sentence.  
            //So, check if the last sentence was created, if not, create it and add it to the list.
            if (firstChar != 0 || sentences.Count == 0) sentences.Add(textToSummarize.Substring(firstChar));
            return sentences;
        }

        public static string ToConcatenatedString(this IEnumerable<char> charSequence)
        {
            lock (stringBuilder)
            {
                stringBuilder.Clear();
                foreach (var c in charSequence)
                {
                    stringBuilder.Append(c);
                }
                return stringBuilder.ToString();
            }
        }


        public static string RemoveCdataTags(this string original)
        {
            if (original.StartsWith("<![CDATA["))
                original = original.Replace("<![CDATA[", string.Empty);

            if (original.EndsWith("]]"))
                original = original.Replace("]]", string.Empty);
            else if (original.EndsWith("]]>"))
                original = original.Replace("]]>", string.Empty);

            return original.Replace("\"", "\'");
        }

        public static string DePascalCase(this string value)
        {
            var newChars = new List<char>();

            var previousLower = true;
            var previousSpace = false;
            foreach (var character in value)
            {
                var c = character;
                if (newChars.Count == 0)
                {
                    if (char.IsLower(c)) c = char.ToUpper(c);
                }
                else
                {
                    if (char.IsUpper(c) && previousSpace == false && previousLower)
                    {
                        newChars.Add(' ');
                    }
                }
                previousLower = char.IsLower(c);
                previousSpace = c == ' ';
                newChars.Add(c);
            }
            return new string(newChars.ToArray());
        }

        public static string GetBetween(this string baseString, string startString, string endString)
        {
            var startIndex = baseString.IndexOf(startString, StringComparison.Ordinal) + startString.Length;
            if (startIndex < 0) return string.Empty;
            var endIndex = baseString.IndexOf(endString, startIndex, StringComparison.Ordinal);
            if (endIndex < 0) return string.Empty;
            return baseString.Substring(startIndex, endIndex - startIndex);
        }

        public static string FormatLineBreaksForWeb(this string original)
        {
            original = original.Replace("\n\r", "<br />");
            return original.Replace("\n", "<br />");
        }

        public static string Substring_From_To(this string originalString, int fromCharacterPosition, int toCharacterPosition)
        {
            return originalString.Substring(fromCharacterPosition, (toCharacterPosition - fromCharacterPosition) + 1);
        }

        public static string Truncate(this string stringToTruncate, int length, bool loose = true, bool chopWordIfLessThan50Percent = true, bool appendDotsIfTruncated = false)
        {
            if (stringToTruncate.Length <= length) return stringToTruncate;
            if (length == 0) return string.Empty;

            var zeroBasedLength = length - 1;

            var sentanceTerminators = new List<char>(4) { ' ', '.', '!', '?' };

            if (sentanceTerminators.Contains(stringToTruncate[zeroBasedLength]))
            {
                while (sentanceTerminators.Contains(stringToTruncate[zeroBasedLength]))
                {
                    zeroBasedLength--;
                }
                zeroBasedLength++;
            }
            else
            {
                if (loose)
                {
                    if (sentanceTerminators.Contains(stringToTruncate[zeroBasedLength + 1]) == false)
                    {
                        while (zeroBasedLength < stringToTruncate.Length && sentanceTerminators.Contains(stringToTruncate[zeroBasedLength]) == false)
                        {
                            zeroBasedLength++;
                        }
                    }
                }
                else
                {
                    if (sentanceTerminators.Contains(stringToTruncate[zeroBasedLength + 1]) == false)
                    {
                        while (zeroBasedLength != 0 && sentanceTerminators.Contains(stringToTruncate[zeroBasedLength]) == false)
                            zeroBasedLength--;
                        zeroBasedLength++;
                    }
                }
            }

            if (chopWordIfLessThan50Percent && ((zeroBasedLength * 2) < length))
                zeroBasedLength = length;

            if (zeroBasedLength <= 1) return string.Empty;

            var stringToReturn = stringToTruncate.Substring(0, zeroBasedLength).Trim();
            if (appendDotsIfTruncated) stringToReturn += "...";
            return stringToReturn;
        }

        public static bool Contains(this string source, string value, StringComparison comparison)
        {
            return source.IndexOf(value, comparison) != -1;
        }

        public static string Combine(this string originalString, string stringToAdd, char separatorCharacter, bool spaceBeforeSeparator = false, bool spaceAfterSeparator = true)
        {
            if (originalString == null) originalString = string.Empty;

            if (originalString != string.Empty)
            {
                if (spaceBeforeSeparator) originalString += " ";
                originalString += separatorCharacter;
                if (spaceAfterSeparator) originalString += " ";
            }
            originalString += stringToAdd;
            return originalString;
        }

        public static bool IsDigits(this string possibleDigitsString)
        {
            for (var i = 0; i < possibleDigitsString.Length; i++)
                if (!char.IsDigit(possibleDigitsString, i))
                    return false;

            return true;
        }

        public static string OrIfEmpty(this string first, string contingencyString)
        {
            if (string.IsNullOrEmpty(first) == false) return first;
            return contingencyString;
        }

        public static string AddFilePath(this string value, string additionalFilePath)
        {
            if (string.IsNullOrEmpty(additionalFilePath)) return value;
            value = value.Trim();
            additionalFilePath = additionalFilePath.Trim();
            if (value[value.Length - 1] != '\\') value += '\\';
            return additionalFilePath[0] == '\\'
                ? value + additionalFilePath.Substring(1, additionalFilePath.Length - 1)
                : value + additionalFilePath;
        }

        public static List<MatchedCoordinate> FindMatchedLocations(this string str, string searchValue, StringComparison comparison, bool leadingWild = true)
        {
            var index = str.IndexOf(searchValue, comparison);
            var matches = new List<MatchedCoordinate>();

            if (searchValue.Length != 0)
            {
                int searchLocation = 0;
                while (index != -1)
                {
                    if (index == 0 || (!leadingWild && !char.IsLetterOrDigit(str[index - 1])) || (leadingWild && !char.IsLetterOrDigit(str[index + searchValue.Length])))
                        matches.Add(new MatchedCoordinate(index, index + searchValue.Length));

                    index = str.IndexOf(searchValue, searchLocation, comparison);
                    searchLocation = index + searchValue.Length;
                }
            }
            return matches;
        }


        public static string ReplaceFirst(this string value, string search, string replace)
        {
            var pos = value.IndexOf(search, StringComparison.Ordinal);
            if (pos == -1) return value;
            return value.Substring(0, pos) + replace + value.Substring(pos + search.Length);
        }

        public static Stream ToStream(this string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value));
        }

        public static string TrimCarriageReturns(this string value)
        {
            var previousLength = value.Length;
            while (value.StartsWith("\r\n"))
            {
                value = value.Substring(2, value.Length - 2);
            }

            while (value.StartsWith("\n"))
            {
                value = value.Substring(1, value.Length - 1);
            }

            while (value.EndsWith("\r\n"))
            {
                value = value.Substring(0, value.Length - 2);
            }

            while (value.EndsWith("\n"))
            {
                value = value.Substring(0, value.Length - 1);
            }

            return value;
        }

        public static string RemoveDoubleCarriageReturns(this string value)
        {
            var previousLength = value.Length;
            while (true)
            {
                previousLength = value.Length;
                value = value.Replace("\r\n", "\n");
                if (previousLength == value.Length) break;
            }

            while (true)
            {
                previousLength = value.Length;
                value = value.Replace("\n\n", "\n");
                if (previousLength == value.Length) break;
            }

            return value.Replace("\n", "\r\n");
        }

        public static string RemoveDoubleCharacters(this string value, string valueToDeDuplicate)
        {
            var previousLength = value.Length;
            while (true)
            {
                previousLength = value.Length;
                value = value.Replace(valueToDeDuplicate + valueToDeDuplicate, valueToDeDuplicate);
                if (previousLength == value.Length) break;
            }

            while (true)
            {
                previousLength = value.Length;
                value = value.Replace(valueToDeDuplicate + valueToDeDuplicate, valueToDeDuplicate);
                if (previousLength == value.Length) break;
            }

            return value;
        }

        public static string RemoveDoubleSpaces(this string value)
        {
            var previousLength = value.Length;
            while (true)
            {
                previousLength = value.Length;
                value = value.Replace("  ", " ");
                if (previousLength == value.Length) break;
            }

            while (true)
            {
                previousLength = value.Length;
                value = value.Replace("  ", " ");
                if (previousLength == value.Length) break;
            }

            return value;
        }

        public static string GetFileNameWithoutExtension(this string fileName)
        {
            if (fileName.Contains("\\"))
            {
                var lastSlashPosition = fileName.LastIndexOf("\\", StringComparison.Ordinal);
                fileName = fileName.Substring(lastSlashPosition + 1, fileName.Length - (lastSlashPosition + 1));
            }

            return fileName.Substring(0, fileName.Length - (GetFileExtension(fileName).Length + 1));
        }

        public static string GetFileExtension(this string fileName)
        {
            if (!fileName.Contains(".")) return string.Empty;
            var lastDotLoc = fileName.LastIndexOf(".", StringComparison.Ordinal);
            return fileName.Substring(lastDotLoc + 1);
        }


        /// <summary>
        /// Split the string into tokens.
        /// </summary>
        /// <param name="inputString">String to be tokenized.</param>
        /// <param name="strict">Will exclude special characters</param>
        /// <returns>List of tokens in string form.</returns>
        public static Dictionary<int, string> GetTokens(this string inputString, bool strict = false)
        {
            var index = 0;
            var tokenCharacters = new Stack<char>();
            var tokens = new Dictionary<int, string>();

            if (strict)
            {
                char[] cleanTokens = new char[inputString.Length];
                for (int i = 0; i < inputString.Length; i++)
                {
                    var c = inputString[i];
                    if (char.IsPunctuation(c)) cleanTokens[i] = ' ';
                    else cleanTokens[i] = c;
                }
                inputString = new string(cleanTokens);
            }

            var startingIndex = 0;
            var previousCharacter = ' ';
            while (index < inputString.Length)
            {
                var character = inputString[index];
                var characterUnicodeCategory = char.GetUnicodeCategory(character);

                switch (tokenCharacters.Count)
                {
                    case 0:
                        if (characterUnicodeCategory.HasFlag(UnicodeCategory.Control))
                            break;

                        if (char.IsLetterOrDigit(character) ||
                            (characterUnicodeCategory.HasFlag(UnicodeCategory.OtherNumber) && character != ' ') ||
                            safeStartingCharacters.Contains(character) ||
                            characterUnicodeCategory.HasFlag(UnicodeCategory.CurrencySymbol))
                        {
                            tokenCharacters.Push(character);
                            startingIndex = index;
                        }
                        break;
                    default:
                        if (!characterUnicodeCategory.HasFlag(UnicodeCategory.Control) && (
                            char.IsLetterOrDigit(character) ||
                            (characterUnicodeCategory.HasFlag(UnicodeCategory.OtherNumber) && character != ' ') ||
                            safeCharacters.Contains(character) ||
                            characterUnicodeCategory.HasFlag(UnicodeCategory.CurrencySymbol) ||
                            char.IsSymbol(character) && char.IsNumber(previousCharacter)))
                        {
                            tokenCharacters.Push(character);
                        }
                        else
                        {
                            while (tokenCharacters.Count != 0 && invalidEndingCharacters.Contains(tokenCharacters.Peek())) tokenCharacters.Pop();
                            if (tokenCharacters.Count == 0) continue;

                            var charArray = tokenCharacters.ToArray();
                            Array.Reverse(charArray);

                            if (charArray.Length > 1 && (!strict || isStrictToken(charArray)))
                                tokens.Add(startingIndex, new string(charArray));

                            tokenCharacters.Clear();
                            previousCharacter = ' ';
                            continue;
                        }
                        break;
                }
                index++;
            }

            if (tokenCharacters.Count > 1)
            {
                var charArray = tokenCharacters.ToArray();
                if (!strict || isStrictToken(charArray))
                    tokens.Add(startingIndex, new string(charArray));
            }

            return tokens;
        }

        private static bool isStrictToken(char[] charArray)
        {
            foreach (var c in charArray)
                switch (char.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.LetterNumber:
                    case UnicodeCategory.OtherNumber:
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.DashPunctuation:
                    case UnicodeCategory.MathSymbol:
                    case UnicodeCategory.CurrencySymbol:
                        break;
                    default:
                        return false;
                }
            return true;
        }

        /// <summary>
        /// Combines the elements of a List of strings into one string
        /// </summary>
        public static string Combine(this List<string> stringList, bool useAppendLine = false)
        {
            var sb = new StringBuilder();
            if (useAppendLine) stringList.ForEach(s => sb.AppendLine(s));
            else stringList.ForEach(s => sb.Append(s));
            return sb.ToString();
        }

        /// <summary>
        /// Combines the elements of a List of strings into one string, with separator
        /// </summary>
        /// <param name="stringList">List of string</param>
        /// <param name="separator">Separator string</param>
        /// <returns></returns>
        public static string Combine<T>(this IEnumerable<T> stringList, string separator)
        {
            var items = stringList.ToArray();
            if (!items.Any()) return string.Empty;

            lock (sbCombiner)
            {
                sbCombiner.Clear();
                foreach (var item in items)
                {
                    if (sbCombiner.Length != 0) sbCombiner.Append(separator);
                    sbCombiner.Append(item);
                }

                return sbCombiner.ToString();
            }
        }

        public static string Replace(this string str, string oldValue, string newValue, StringComparison comparison)
        {
            StringBuilder sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }

        public static string CombineAsString(this List<object> values)
        {
            var returnString = string.Empty;
            values.ForEach(obj => returnString = returnString.Combine(obj.ToString(), ','));
            return returnString;
        }

        public static int CountChar(this string str, char searchTerm)
        {
            var count = 0;
            var len = str.Length;
            for (var i = 0; i < len; i++)
                if (str[i] == searchTerm)
                    count++;
            return count;
        }

        #region Char Extensions

        public static char ToPhoneNumberChar(this char input)
        {
            if (char.IsDigit(input)) return input;
            if (char.IsLower(input)) input = char.ToUpperInvariant(input);

            switch (input)
            {
                case 'A':
                case 'B':
                case 'C':
                    return '2';
                case 'D':
                case 'E':
                case 'F':
                    return '3';
                case 'G':
                case 'H':
                case 'I':
                    return '4';
                case 'J':
                case 'K':
                case 'L':
                    return '5';
                case 'M':
                case 'N':
                case 'O':
                    return '6';
                case 'P':
                case 'R':
                case 'S':
                    return '7';
                case 'T':
                case 'U':
                case 'V':
                    return '8';
                case 'W':
                case 'X':
                case 'Y':
                    return '9';
                default:
                    return '0';
            }
        }

        #endregion

        #region Statistical Functions

        public static KeyValuePair<TKey, TValue>[] ToFeatureVector<TKey, TValue>(this string value)
        {
            return null;
        }

        #endregion
    }
}