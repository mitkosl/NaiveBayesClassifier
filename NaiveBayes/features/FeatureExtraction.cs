using NaiveBayes.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaiveBayes.features
{
    public class FeatureExtraction
    {

        public FeaturesStatistics extractFeatureStatistics(List<Document> dataset)
        {
            FeaturesStatistics statistics = new FeaturesStatistics();

            int categoryCount;
            List<String> categories;
            int featureCategoryCount;
            Dictionary<String, int> featureCategoryCounts;
            foreach(Document doc in dataset)
            {
                statistics.countOfDocuments++; //increase the number of documents
                categories = doc.categories;
                foreach (var category in categories)
                {
                    //increase the category counter by one
                    if(statistics.categoryCounts.ContainsKey(category))
                    {
                        categoryCount = statistics.categoryCounts[category];
                        statistics.categoryCounts[category] = ++categoryCount;
                    }
                    else
                    {
                        statistics.categoryCounts.Add(category, 1); 
                    }

                    foreach(var feature in doc.tokens)
                    {
                        //get the counts of the feature in the categories
                        if (statistics.featureCategoryJointCount.ContainsKey(feature.Key))
                        {
                            featureCategoryCounts = statistics.featureCategoryJointCount[feature.Key];
                        }
                        else
                        { 
                            //initialize it if it does not exist
                            statistics.featureCategoryJointCount.Add(feature.Key, new Dictionary<String, int>());
                        }

                        if (statistics.featureCategoryJointCount[feature.Key].ContainsKey(category))
                        {
                            featureCategoryCount = statistics.featureCategoryJointCount[feature.Key][category];
                            statistics.featureCategoryJointCount[feature.Key][category] = ++featureCategoryCount;
                        } else {
                            featureCategoryCount = 0;
                            statistics.featureCategoryJointCount[feature.Key].Add(category, ++featureCategoryCount);
                        }
                    }
                }
            }
            return statistics;
        }

        public Dictionary<String, Double> select(FeaturesStatistics statistics, int numberOfFeatures = 50)
        {
            Dictionary<String, Double> selectedFeatures = new Dictionary<string, double>();

            String feature;
            String category;
            Dictionary<String, int> categoryList;

            double Ndot0, Ndot1, N1dot, N0dot, N00, N01, N10, N11;
            double N = statistics.countOfDocuments + 0.0;
            double chisquareScore;
            double score;
            //Double previousScore;

            Dictionary<string, Dictionary<string, double>> selectedFeaturesForCategory = new Dictionary<string, Dictionary<string, double>>();
            foreach (var featureCategoryCounts in statistics.featureCategoryJointCount)
            {
                feature = featureCategoryCounts.Key;
                categoryList = featureCategoryCounts.Value;

                //calculate the N1. (number of documents that have the feature)
                N1dot = 0;
                foreach (int count in categoryList.Values)
                {
                    N1dot += count;
                }

                //also the N0. (number of documents that DONT have the feature)
                N0dot = statistics.countOfDocuments - N1dot;

                if (feature == "coffe")
                {
                    Console.WriteLine(String.Format("N0.={0}, N0.={1}\n N={2}", N0dot, N1dot, N));
                }

                foreach (var categoryCounts in categoryList)
                {
                    category = categoryCounts.Key;
                    N11 = categoryCounts.Value; //N11 is the number of documents that have the feature and belong on the specific category
                    N01 = statistics.categoryCounts[category] - N11; //N01 is the total number of documents that do not have the particular feature BUT they belong to the specific category

                    N00 = N0dot - N01; //N00 counts the number of documents that don't have the feature and don't belong to the specific category
                    N10 = N1dot - N11; //N10 counts the number of documents that have the feature and don't belong to the specific category

                    N10 = N10 == 0.0 ? 1.0 : N10;
                    N01 = N01 == 0.0 ? 1.0 : N01;

                    Ndot0 = N10 + N00;
                    Ndot1 = N11 + N01;
                    //calculate the chisquare score based on the above statistics
                    chisquareScore = N * Math.Pow(N11 * N00 - N10 * N01, 2) / ((N11 + N01) * (N11 + N10) * (N10 + N00) * (N01 + N00));

                    score = (N11 / N) * Math.Log((N * N11) / (N1dot * Ndot1),2);
                    score += (N01 / N) * Math.Log((N * N01) / (N0dot * Ndot1),2);
                    score += (N10 / N) * Math.Log((N * N10) / (N1dot * Ndot0),2);
                    score += (N00 / N) * Math.Log((N * N00) / (N0dot * Ndot0),2);

                    if (category == "coffee") {
                        Console.WriteLine(String.Format("N01={0}, N11={1}\n N00={2}, N10={3} \n Ndot0={4}, Ndot1={5} \n N0dot={6}, N1dot={7} \n score={8}", N01, N11,N00, N10, Ndot0, Ndot1, N0dot, N1dot, score));
                    }

                    if(selectedFeaturesForCategory.ContainsKey(category))
                    {
                        if(!selectedFeaturesForCategory[category].ContainsKey(feature))
                            selectedFeaturesForCategory[category].Add(feature, score);
                    } else {
                        selectedFeaturesForCategory.Add(category, new Dictionary<string, double>());
                        selectedFeaturesForCategory[category].Add(feature, score);
                    }

                    ////if the score is larger than the critical value then add it in the list
                    //if (chisquareScore >= numberOfFeatures)
                    //{
                    //    previousScore = selectedFeatures[feature];
                    //    if (previousScore == null || chisquareScore > previousScore)
                    //    {
                    //        selectedFeatures.Add(feature, chisquareScore);
                    //    }
                    //}
                }
            }

            foreach (var cat in selectedFeaturesForCategory.Keys)
            {
                Dictionary<string, double> features = selectedFeaturesForCategory[cat];
                List<KeyValuePair<String, double>> list = features.ToList();
                list.Sort((pair1, pair2) => { return pair2.Value.CompareTo(pair1.Value); });

                foreach (var l in list.Take(numberOfFeatures))
                {
                    //if (cat == "coffee") {
                    //    foreach (var f in selectedFeaturesForCategory[cat])
                    //        Console.WriteLine(String.Format("{0},{1}",f.Key,f.Value));
                    //    }
                    if(!selectedFeatures.ContainsKey(l.Key))
                        selectedFeatures.Add(l.Key, l.Value);
                }
            }
            return selectedFeatures;
        }

    }
}
