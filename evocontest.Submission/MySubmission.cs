using evocontest.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace evocontest.Submission
{
    /// <summary>
    /// Template class for your own submission for the contest.
    /// </summary>
    public class MySubmission : ISolution
    {
        private static string input;
        private static char wordSeparator = ' ';

        public string Solve(string input)
        {
            OffsetRange.source = input;
            MySubmission.input = input;

            IEnumerable<KeyValuePair<string, ArrayList<OffsetRange>>> acronyms = 
                GetPossibleAcronyms(input)
                .AsParallel()
                .Where(x => !AreConflicting(x.Value))
                .Where(kvp => kvp.Value.Count >= 2);

            var result = input;
            foreach (var (acronym, phrases) in acronyms.OrderByDescending(x => x.Key.Length))
            {
                foreach (var phrase in phrases.Select(p => input[p.AsRange()]).Distinct())
                {
                    var replaceRegex = new Regex($"(?<![a-zA-Z]){phrase}(?![a-zA-Z])");
                    result = replaceRegex.Replace(result, acronym);
                }
            }
            return result;
        }

        /// <summary>
        /// Discovers all possible acronyms in an {acronym, [list of phrases]} format.
        /// </summary>
        private static IDictionary<string, ArrayList<OffsetRange>> GetPossibleAcronyms(ReadOnlySpan<char> text)
        {
            var st = new Stopwatch();
            st.Start();
            if (text.IsEmpty) return new Dictionary<string, ArrayList<OffsetRange>>(0);
            var sentenceSeparator = '.';
            var acronyms = new Dictionary<string, ArrayList<OffsetRange>>(16);
            text = text[..^1];
            var _sentences = text.Partition(sentenceSeparator, extraSeparator: 1, buffer: new ArrayList<OffsetRange>(8));

            void job(int thread, int noOfThreads)
            {
                ReadOnlySpan<char> text = input;
                var _words = new ArrayList<OffsetRange>(512);
                var _partition = new ArrayList<OffsetRange>(256);
                var acronym = new ArrayList<char>(30);
                foreach (var _sentence in _sentences.GetNth(noOfThreads, thread))
                {
                    var sentence = text[_sentence.AsRange()];
                    _words.Clear();
                    _words = sentence.Partition(wordSeparator, offset: _sentence, buffer: _words);
                    for (var startIndex = 0; startIndex < _words.Count; startIndex++)
                    {
                        // Add existing acronyms from the text
                        var word = text[_words[startIndex].AsRange()];
                        if (IsAcronym(word)) AddAcronym(acronyms, word, _words[startIndex]);

                        // Add all >=2 word long phrases
                        for (var endIndex = startIndex + 1; endIndex <= _words.Count; endIndex++)
                        {
                            _partition.Clear();
                            ReadOnlySpan<OffsetRange> partition = sentence.Partition(wordSeparator, startIndex, endIndex - startIndex, offset: _sentence, buffer: _partition);
                            foreach (var w in partition)
                            {
                                var wo = text[w.AsRange()];
                                if (IsAcronym(wo)) acronym.AddRange(wo);
                                else acronym.Add(char.ToUpper(wo[0]));
                            }
                            var phrase = text[partition.Aggregate().AsRange()];
                            if (phrase.IndexOf(' ') > 0)
                            {
                                AddAcronym(acronyms, acronym, partition.Aggregate());
                            }
                            acronym.Clear();
                        }
                    }
                }
            }

            job(0, 1);

            return acronyms;
        }

        /// <summary>
        /// Returns true, if any 2 of the given phrases are considered different.
        /// E.g.:
        /// - "aa bb cc", "aa BC"   => false
        /// - "aa bb", "ax bx"      => true
        /// </summary>
        private static ArrayList<ArrayList<OffsetRange>> splitPhrases = new ArrayList<ArrayList<OffsetRange>>(32);
        private static bool AreConflicting(ArrayList<OffsetRange> phrases)
        {
            if (phrases.Count == 1) return false;
            // [aaa BC, axx bb cc] => [[aaa, B, C], [axx, bb, cc]]
            ReadOnlySpan<char> str = input;

            splitPhrases.Clear();

            for (int i = 0; i < phrases.Count; i++)
            {
                OffsetRange phrase = phrases[i];
                ReadOnlySpan<char> phrase1 = input[phrase.AsRange()];
                var words1 = phrase1.Partition(wordSeparator, offset: phrase);
                var splitPhrase = new ArrayList<OffsetRange>(16);
                foreach (var word in words1)
                {
                    if (char.IsLower(str[word.AsRange().Start])) splitPhrase.Add(word);
                    else
                    {
                        for (int j = 0; j < word.Length; j++) splitPhrase.Add(word.offset + word.start.Value + j);
                    }
                }
                splitPhrases.Add(splitPhrase);
            }

            for (var i = 0; i < splitPhrases.Count; i++)
            {
                for (var j = i + 1; j < splitPhrases.Count; j++)
                {
                    var (firstPhrase, secondPhrase) = (splitPhrases[i], splitPhrases[j]);
                    for (var k = 0; k < firstPhrase.Count; k++)
                    {
                        var (firstWord, secondWord) = (firstPhrase[k], secondPhrase[k]);
                        if (!IsAcronym(str[firstWord.AsRange()]) && !IsAcronym(str[secondWord.AsRange()]) && !firstWord.Equals(secondWord))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static void AddAcronym(IDictionary<string, ArrayList<OffsetRange>> possibleAcronyms, ReadOnlySpan<char> acronym, OffsetRange phrase)
        {
            var asString = acronym.ToString();
            if (!possibleAcronyms.ContainsKey(asString))
            {
                possibleAcronyms.Add(asString, new ArrayList<OffsetRange>(8));
            }
            possibleAcronyms[asString].Add(phrase);
        }
        
        private static bool IsAcronym(ReadOnlySpan<char> word)
        {
            foreach (var ch in word) if (!char.IsUpper(ch)) return false;
            return true;
        }
    }

    public static class Extensions
    {
        public static IEnumerable<T> GetNth<T>(this IList<T> list, int n, int skip = 0)
        {
            for (int i = skip; i < list.Count; i += n)
                yield return list[i];
        }

        public static ArrayList<OffsetRange> Partition(this ref ReadOnlySpan<char> str, char separator, int skip = 0, int limit = int.MaxValue, int extraSeparator = 0, int offset = 0, ArrayList<OffsetRange> buffer = null)
        {
            var partition = buffer ?? new ArrayList<OffsetRange>(64);
            int s = 0, t, e = 0, i = 0;
            OffsetRange range;
            do
            {
                t = str[s..].IndexOf(separator);
                e = t + s;
                if (i >= skip)
                {
                    if (t > 0)
                    {
                        range = s..e;
                        range.offset = offset;
                        partition.Add(range);
                    }
                    else
                    {
                        range = s..str.Length;
                        range.offset = offset;
                        partition.Add(range);
                    }
                }
                s = e + 1 + extraSeparator;
                i++;
            }
            while (t > 0 && partition.Count < limit);

            return partition;
        }

        public static OffsetRange Aggregate(this ReadOnlySpan<OffsetRange> ranges)
        {

            OffsetRange range = ranges[0].start..ranges[^1].end;
            range.offset = ranges[0].offset;
            return range;
        }
    }
}
