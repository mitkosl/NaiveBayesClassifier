using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaiveBayes.models
{
    public class KnowledgeBase
    {
        public int countOfDocuments = 0;

        public int numberOfCategories = 0;

        public int numberOfFeatures = 0;

        public const double featureSelectionBoundary = 0.6; // filter out anything less than this boudary

        // log(P(c))
        public Dictionary<String, Double> logPriors = new Dictionary<string, double>();

        //log (P(x|c) / P(x|!c))
        public Dictionary<String, Dictionary<String, Double>> logConditionalProbability = new Dictionary<string, Dictionary<string, double>>();
    }
}
