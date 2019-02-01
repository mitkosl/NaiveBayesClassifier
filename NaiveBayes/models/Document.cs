using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaiveBayes.models
{
    public class Document
    {
        public Dictionary<String, int> tokens;

        public List<String> categories;

        public Document() {
            tokens = new Dictionary<string, int>();
        } 
    }
}
