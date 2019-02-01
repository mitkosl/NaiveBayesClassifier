using NaiveBayes.features;
using NaiveBayes.models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace NaiveBayes.classifiers
{
    public class NaiveBayesClassifier
    {
        //if we want to use already trained knowledge base
        public NaiveBayesClassifier(KnowledgeBase knowledgeBase)
        {
            this.knowledgeBase = knowledgeBase;
        }

        //needed if we want to train new classifier
        public NaiveBayesClassifier() : this(null) { }

        private KnowledgeBase knowledgeBase;
        public KnowledgeBase KnowledgeBase
        {
            get { return this.knowledgeBase; }
        }

        public List<DocItem> parseSimpleTextFile(string allText)
        {
            List<DocItem> result = new List<DocItem>();
            string[] docs = allText.Split(new[] { "\r\n\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string doc in docs)
            {
                DocItem item = new DocItem();

                string docc = doc.Trim(new Char[] { '\r', '\n' });
                int index = docc.IndexOf("\r\n");
                string title = docc.Substring(0, index);
                string body = docc.Substring(index + 2);
                item.title = title;
                item.body = body;
                result.Add(item);
            }

            return result;
        }

        private List<Document> preprocessDataset(String directoryUrl)
        {
            List<Document> dataset = new List<Document>();

            string baseDirPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            foreach (string file in Directory.EnumerateFiles(baseDirPath + @"\dataset", "*.json"))
            {
                string json = File.ReadAllText(file);
                List<DocItem> docItems = JsonConvert.DeserializeObject<List<DocItem>>(json);

                Document document;

                foreach (var item in docItems)
                {
                    if (item.topics == null || item.topics.Length < 0)
                    {
                        continue;
                    }
                    //for each doc - tokenize its body and convert it into a Document object.
                    document = TextTokenizer.tokenize(item.title + " " + item.body);
                    document.categories = item.topics.ToList<String>();
                    dataset.Add(document);
                }
            }
            return dataset;
        }
 
        //Gathers the required counts for the features and performs feature selection
        private FeaturesStatistics selectFeatures(List<Document> dataset, int numberOfFeatures = 30)
        {
            FeatureExtraction featureExtractor = new FeatureExtraction();

            //the FeatureStatistica object contains statistics about all the features found in the documents
            FeaturesStatistics statistics = featureExtractor.extractFeatureStatistics(dataset);

            //we pass this information to the feature selection algorithm and we get a list with the selected features
            Dictionary<String, Double> selectedFeatures = featureExtractor.select(statistics, numberOfFeatures);


            Dictionary<String, Dictionary<String, int>> newfeatureCategoryJointCount = new Dictionary<string, Dictionary<string, int>>();
            //clip from the stats all the features that are not selected
            foreach (var arr in statistics.featureCategoryJointCount)
            {
                    if (selectedFeatures.ContainsKey(arr.Key))
                    {
                        newfeatureCategoryJointCount.Add(arr.Key,arr.Value);
                    }
            }

            statistics.featureCategoryJointCount = newfeatureCategoryJointCount;
            return statistics;
        }

        public void train(double featureSelectionBoundary = 0.6)
        {
            train(null);
        }

        public void train(Dictionary<String, Double> categoryPriors, int numberOfFeatures = 1000)
        {
            Console.WriteLine("Training......");
            //preprocess the given dataset
            List<Document> dataset = preprocessDataset(Program.datasetsDirectory);

            //produce the feature stats and select the best features
            FeaturesStatistics featureStatistics = selectFeatures(dataset);

            //intiliaze the knowledgeBase of the classifier
            knowledgeBase = new KnowledgeBase();
            knowledgeBase.countOfDocuments = featureStatistics.countOfDocuments; //number of observations
            knowledgeBase.numberOfFeatures = featureStatistics.featureCategoryJointCount.Count; //number of features

            //check is prior probabilities are given
            if (categoryPriors == null)
            {
                //if not estimate the priors from the sample
                knowledgeBase.numberOfCategories = featureStatistics.categoryCounts.Count; //number of cateogries
                knowledgeBase.logPriors = new Dictionary<string, double>();

                foreach (var item in featureStatistics.categoryCounts)
                {
                    //knowledgeBase.logPriors.Add(item.Key, Math.Log10((double)item.Value / knowledgeBase.countOfDocuments));
                    knowledgeBase.logPriors.Add(item.Key, (double)item.Value / knowledgeBase.countOfDocuments);
                }
            }
            else
            {
                knowledgeBase.numberOfCategories = categoryPriors.Count;

                //make sure that the given priors are valid
                if (knowledgeBase.numberOfCategories != featureStatistics.categoryCounts.Count)
                {
                    throw new ArgumentException("Invalid priors Array: Make sure you pass a prior probability for every supported category.");
                }
                foreach (var item in categoryPriors)
                {
                    if (item.Value < 0)
                    {
                        throw new ArgumentException("Invalid priors Array: Prior probabilities should be between 0 and 1.");
                    }
                    //knowledgeBase.logPriors.Add(item.Key, Math.Log10(item.Value));
                    knowledgeBase.logPriors.Add(item.Key, item.Value);
                }
            }

            //We are performing laplace smoothing (also known as add-1). This requires to estimate the total feature occurrences in each category
            Dictionary<String, int> featureOccurrencesInCategory = new Dictionary<string, int>();
            int featureOccSum, allWordsCount = 0;
            foreach (var category in knowledgeBase.logPriors)
            {
                featureOccSum = 0;
                foreach (var categoryListOccurrences in featureStatistics.featureCategoryJointCount.Values)
                {
                    if (categoryListOccurrences.ContainsKey(category.Key))
                    {
                        int occurrences = categoryListOccurrences[category.Key];
                        featureOccSum += occurrences;
                        allWordsCount += occurrences;
                    }
                }
                if (featureOccurrencesInCategory.ContainsKey(category.Key))
                {
                    featureOccurrencesInCategory[category.Key] = featureOccSum;
                }
                else
                {
                    featureOccurrencesInCategory.Add(category.Key, featureOccSum);
                }
            }

            //estimate log likelihoods
            int count, negativeCount, allWordsInCategory;
            Dictionary<String, int> featureCategoryCounts = new Dictionary<string, int>();
            double logLikelihood;
            foreach (String category in knowledgeBase.logPriors.Keys)
            {                           ///<feature, <category, count>>
                foreach (var entry in featureStatistics.featureCategoryJointCount)
                {
                    featureCategoryCounts = entry.Value;
                    negativeCount = 0;
                    foreach (string cat in featureCategoryCounts.Keys)
                    {
                        if (cat != category)
                        {
                            negativeCount += featureCategoryCounts[cat];
                        }
                    }
                    if (featureCategoryCounts.ContainsKey(category))
                    {
                        count = featureCategoryCounts[category];
                    }
                    else
                    {
                        count = 0;
                    }

                    if (featureOccurrencesInCategory.ContainsKey(category))
                    {
                        allWordsInCategory = featureOccurrencesInCategory[category];
                    }
                    else
                    {
                        allWordsInCategory = 0;
                    }
                    double PInClassC = (count + 1.0) / (allWordsInCategory + knowledgeBase.numberOfFeatures);
                    double PisNotInClassC = (negativeCount + 1.0) / ((allWordsCount - allWordsInCategory) + knowledgeBase.numberOfFeatures);
                    logLikelihood = Math.Log10(PInClassC / (PisNotInClassC)); //log(x/y) = logX - logY
                    if (knowledgeBase.logConditionalProbability.ContainsKey(entry.Key))
                    {
                        knowledgeBase.logConditionalProbability[entry.Key].Add(category, logLikelihood);
                    }
                    else
                    {
                        knowledgeBase.logConditionalProbability.Add(entry.Key, new Dictionary<String, Double>());
                        knowledgeBase.logConditionalProbability[entry.Key].Add(category, logLikelihood);
                    }
                }
            }
            featureOccurrencesInCategory = null;

            string baseDirPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            string fileName = baseDirPath + "\\" + ConfigurationManager.AppSettings["knowledgeBase"];
            var knowledgeBaseStr = JsonConvert.SerializeObject(this.knowledgeBase);
            File.WriteAllText(fileName, knowledgeBaseStr, Encoding.UTF8);

            Console.WriteLine("\n\nDone Training !");
            Console.Beep();
        }

        public void evaluate()
        {
            Console.WriteLine("Evaluating.... ");
            //List<Document> dataset = preprocessDataset(Program.evaluationDirectory);
            string baseDirPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            double accuracy = 0.0;
            double precision = 0.0;
            double recall = 0.0;
            int documentsCount = 0;
            int TP = 0; // True Positive
            int FP = 0; // False Positive - false alarm (incorrect)
            int FN = 0; //False Negative (miss)

            foreach (string file in Directory.EnumerateFiles(baseDirPath + @"\eval", "*.json"))//Environment.CurrentDirectory
            {
                //    Console.ForegroundColor = ConsoleColor.White;
                //    Console.WriteLine("\n\nPress any key to classify news in file: " + file.Substring(file.LastIndexOf('\\') + 1).ToUpper());
                //    Console.ReadKey();

                string json = File.ReadAllText(file);
                List<DocItem> docItems = JsonConvert.DeserializeObject<List<DocItem>>(json);

                foreach (var item in docItems)
                {
                    if (item.topics != null && item.topics.Length > 0)
                    {
                        documentsCount++;
                        var resTopics = this.predict(item.title + " " + item.body);
                        if (resTopics.Count > 0)
                        {
                            int tp = 0; // True Positive
                            int fp = 0; // False Positive - false alarm (incorrect)
                            int fn = 0; //False Negative (miss)
                            foreach (var topic in resTopics)
                            {
                                if (item.topics.Contains(topic))
                                    tp++;
                                else
                                    fp++;
                            }
                            fn = item.topics.Length - tp;

                            tp /= item.topics.Length;
                            fp /= resTopics.Count;
                            fn /= item.topics.Length;
                            accuracy += tp;

                            TP += tp;
                            FP += fp;
                            FN += fn;
                        }
                    }
                }
            }
            accuracy /= documentsCount;
            accuracy *= 100;
            precision = (double)TP / (TP + FP);
            recall = (double)TP / (TP + FN);

            Console.Clear();
            Console.WriteLine("Evaluation: \n");
            Console.WriteLine(String.Format("TP={0} FP={1} FN={2}", TP, FP, FN));
            Console.WriteLine(String.Format("Accuracy={0:0.00#}%", accuracy));
            Console.WriteLine(String.Format("Precision={0:0.000}", precision));
            Console.WriteLine(String.Format("Recall={0:0.000}", recall));
        }

        public List<String> predict(String text, int topKCategories = 3)
        {
            if (knowledgeBase == null)
            {
                throw new ArgumentException("Knowledge Bases missing: Make sure you train first a classifier before you use it.");
            }

            //Tokenizes the text and creates a new document
            Document doc = TextTokenizer.tokenize(text);
            double occurrences;

            //String maxScoreCategory = null;
            //Double maxScore = Double.MinValue;

            Dictionary<String, double> predictionScores = new Dictionary<string, double>();
            foreach (var categoryCounts in knowledgeBase.logPriors)
            {
                double logprob = categoryCounts.Value;
                //foreach feature of the document
                foreach (var tokenCount in doc.tokens)
                {
                    if (!knowledgeBase.logConditionalProbability.ContainsKey(tokenCount.Key))
                    {
                        continue; //if the feature does not exist just skip it
                    }

                    occurrences = tokenCount.Value; //get its occurrences in text

                    if (knowledgeBase.logConditionalProbability[tokenCount.Key].ContainsKey(categoryCounts.Key))
                    {
                        logprob += knowledgeBase.logConditionalProbability[tokenCount.Key][categoryCounts.Key]; //multiply loglikelihood score with occurrences
                    }
                }
                predictionScores.Add(categoryCounts.Key, logprob);

                //if (categoryCounts.Value > maxScore)
                //{
                //    maxScore = categoryCounts.Value;
                //    maxScoreCategory = categoryCounts.Key;
                //}
            }

            var list = predictionScores.ToList();
            list.Sort((pair1, pair2) => { return pair2.Value.CompareTo(pair1.Value); });
            List<string> result = new List<string>();
            foreach (var l in list)
            {
                if (l.Value > 0.0)
                {
                    result.Add(l.Key);
                }
            }
            return result.Count >= topKCategories ? result.GetRange(0, topKCategories) : result; //return the categoies with positive odds
        }
    }
}
