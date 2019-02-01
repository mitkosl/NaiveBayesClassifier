using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaiveBayes.models
{
    public class FeaturesStatistics
    {
        public int countOfDocuments;

        //occurences of features and category count
        public Dictionary<String, Dictionary<String, int>> featureCategoryJointCount;

        //how many times caategory is found in dataset
        public Dictionary<String, int> categoryCounts;

        public FeaturesStatistics() {
            countOfDocuments = 0;
            featureCategoryJointCount = new Dictionary<string, Dictionary<string, int>>();
            categoryCounts = new Dictionary<string, int>();
        }

    }
}
