using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUIshka
{
    public class SearchResult
    {
        public string Match { get; set; }
        public int Line { get; set; }
        public int Position { get; set; }
        public int Length { get; set; }
        public int AbsoluteIndex { get; set; }
    }

    public enum SearchType
    {
        Numbers,  
        BitcoinAddress, 
        Time            
    }

    public class RegexSearchEngine
    {
        private readonly Regex numbersRegex = new Regex(
            @"-?\d+(?:,\d+)?",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );

        private readonly Regex bitcoinRegex = new Regex(
            @"\b[13][a-km-zA-HJ-NP-Z1-9]{25,34}\b",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );

        private readonly Regex timeRegex = new Regex(
            @"\b(?:[0-1][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]\b",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );

        public List<SearchResult> Search(string text, SearchType searchType)
        {
            List<SearchResult> results = new List<SearchResult>();

            if (string.IsNullOrEmpty(text))
                return results;

            Regex regex = GetRegexByType(searchType);

            if (regex == null)
                return results;

            string[] lines = text.Split('\n');

            int lineOffset = 0;

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];

                MatchCollection matches = regex.Matches(line);

                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        results.Add(new SearchResult
                        {
                            Match = match.Value,
                            Line = lineIndex + 1,
                            Position = match.Index + 1,
                            Length = match.Length,
                            AbsoluteIndex = lineOffset + match.Index
                        });
                    }
                }

                lineOffset += line.Length + 1;
            }

            return results;
        }

        private Regex GetRegexByType(SearchType searchType)
        {
            switch (searchType)
            {
                case SearchType.Numbers:
                    return numbersRegex;
                case SearchType.BitcoinAddress:
                    return bitcoinRegex;
                case SearchType.Time:
                    return timeRegex;
                default:
                    return null;
            }
        }

        public string GetRegexDescription(SearchType searchType)
        {
            switch (searchType)
            {
                case SearchType.Numbers:
                    return @"-?\d+(?:,\d+)?\n\n" +
                           "Пояснение:\n" +
                           "-? - необязательный знак минуса\n" +
                           @"\d+ - одна или более цифр\n" +
                           @"(?:,\d+)? - необязательная группа: запятая и цифры";

                case SearchType.BitcoinAddress:
                    return @"[13][a-km-zA-HJ-NP-Z1-9]{25,34}\n\n" +
                           "Пояснение:\n" +
                           "[13] - адрес начинается с 1 или 3\n" +
                           "[a-km-zA-HJ-NP-Z1-9] - символы Base58 (без 0, O, I, l)\n" +
                           @"{25,34} - длина 25-34 символа";

                case SearchType.Time:
                    return @"(?:[0-1][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]\n\n" +
                           "Пояснение:\n" +
                           "(?:[0-1][0-9]|2[0-3]) - часы от 00 до 23\n" +
                           "[0-5][0-9] - минуты от 00 до 59\n" +
                           "[0-5][0-9] - секунды от 00 до 59";

                default:
                    return "";
            }
        }

        public Dictionary<string, bool> TestRegex(SearchType searchType, string[] testStrings)
        {
            Dictionary<string, bool> results = new Dictionary<string, bool>();
            Regex regex = GetRegexByType(searchType);

            foreach (string test in testStrings)
            {
                bool isMatch = regex.IsMatch(test);
                results[test] = isMatch;
            }

            return results;
        }
    }
}
