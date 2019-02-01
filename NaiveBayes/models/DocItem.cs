using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaiveBayes.models
{
    public class DocItem
    {
        public string title;
        public string body;
        public string date;
        public string[] topics;
        public string[] places;
        public string[] people;
        public int id;
    }
}


//  {
//  "title": "NATIONAL AVERAGE PRICES FOR FARMER-OWNED RESERVE",
//  "body": "The U.S. Agriculture bla bla blah",
//  "date": "26-FEB-1987 15:10:44.60",
//  "topics": [
//    "grain",
//    "wheat",
//    "corn",
//    "barley",
//    "oat",
//    "sorghum"
//  ],
//  "places": [
//    "usa"
//  ],
//  "id": "5"
//},
