using NaiveBayes.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NaiveBayes.features
{
    public class TextTokenizer
    {
        public static String preprocess(String text)
        {
            Regex rgx = new Regex("\\p{P}");
            string result = rgx.Replace(text, " ");
            Regex rgx2 = new Regex("\\s+");
            result = rgx2.Replace(result, " ");
            return result.ToLower();
        }

        public static String[] extractKeywords(String text)
        {
            return text.Split(' ');
        }

        public static Dictionary<String, int> getKeywordCounts(String[] keywordArray)
        {
            Dictionary<String, int> counts = new Dictionary<string, int>();

            Stemmer stemmer = new Stemmer();
            int counter = 0;
            for (int i = 0; i < keywordArray.Length; ++i)
            {
                string stemmedWord = stemmer.stemTerm(keywordArray[i]);
                if (stopWords.Contains(stemmedWord))
                    continue;
                if (stemmedWord.Length <= 2)
                    continue;

                if (counts.ContainsKey(stemmedWord))
                {
                    counter = counts[stemmedWord];
                }
                else
                {
                    counter = 0;
                }
                counts[stemmedWord] = ++counter; //increase counter for the keyword
            }
            //for (int index = 0; index < counts.Count; index++)
            //{
            //    var item = counts.ElementAt(index);
            //    counts[item.Key] = Math.Log10(1.0 + item.Value);
            //}
            return counts;
        }

        public static Document tokenize(String text)
        {
            String preprocessedText = preprocess(text);
            String[] keywordArray = extractKeywords(preprocessedText);

            Document doc = new Document();
            doc.tokens = getKeywordCounts(keywordArray);
            return doc;
        }
        //174 stop words from https://www.ranks.nl/stopwords
        public static List<string> stopWords = new List<string> { "a", "about", "above", "after", "again", "against", "all", "am", "an", "and", "any", "are", "aren't", "as", "at", "be", "because", "been", "before", "being", "below", "between", "both", "but", "by", "can't", "cannot", "could", "couldn't", "did", "didn't", "do", "does", "doesn't", "doing", "don't", "down", "during", "each", "few", "for", "from", "further", "had", "hadn't", "has", "hasn't", "have", "haven't", "having", "he", "he'd", "he'll", "he's", "her", "here", "here's", "hers", "herself", "him", "himself", "his", "how", "how's", "i", "i'd", "i'll", "i'm", "i've", "if", "in", "into", "is", "isn't", "it", "it's", "its", "itself", "let's", "me", "more", "most", "mustn't", "my", "myself", "no", "nor", "not", "of", "off", "on", "once", "only", "or", "other", "ought", "our", "ours", "ourselves", "out", "over", "own", "same", "shan't", "she", "she'd", "she'll", "she's", "should", "shouldn't", "so", "some", "such", "than", "that", "that's", "the", "their", "theirs", "them", "themselves", "then", "there", "there's", "these", "they", "they'd", "they'll", "they're", "they've", "this", "those", "through", "to", "too", "under", "until", "up", "very", "was", "wasn't", "we", "we'd", "we'll", "we're", "we've", "were", "weren't", "what", "what's", "when", "when's", "where", "where's", "which", "while", "who", "who's", "whom", "why", "why's", "with", "won't", "would", "wouldn't", "you", "you'd", "you'll", "you're", "you've", "your", "yours", "yourself", "yourselves" };
    }
}
