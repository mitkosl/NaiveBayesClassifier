using NaiveBayes.classifiers;
using NaiveBayes.features;
using NaiveBayes.models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Configuration;

namespace NaiveBayes
{
    public class Program
    {
        public const string datasetsDirectory = "dataset";

        public static int DisplayMenu()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("---News Categorization---");
            Console.WriteLine();
            Console.WriteLine("1. Train new Naive Bayes Classifier");
            Console.WriteLine("2. Classify all documents in " + datasetsDirectory + " folder");
            Console.WriteLine("3. Classify sigle file");
            Console.WriteLine("4. Evaluate");
            Console.WriteLine("5. Clear");
            Console.WriteLine("6. Exit");
            
            var res = Console.ReadLine();
            return Convert.ToInt32(res);
        }

        public static Dictionary<String, List<String>> Classify(NaiveBayesClassifier nb, List<DocItem> docItems)
        {
            Dictionary<String, List<String>> result = new Dictionary<string, List<string>>();
            foreach (var item in docItems)
            {
                var res = nb.predict(item.title + " " + item.body);
                result.Add(item.title, res);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(String.Format("'{0}' is categorised as: ", item.title));
                for (int i = 0; i < res.Count; i++)
                {
                    if (i != res.Count - 1)
                        Console.Write(res[i].ToUpper() + ", ");
                    else
                        Console.WriteLine(res[i].ToUpper());
                }
            }
            return result;
        }

        public static void Main(String[] args)
        {
            string baseDirPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            string fileName = baseDirPath + "\\" + ConfigurationManager.AppSettings["knowledgeBase"];

            Console.WriteLine("Hello, Welcome to the Multinomial Naive Bayes Classifier\n");          

            NaiveBayesClassifier nb = new NaiveBayesClassifier();
            KnowledgeBase knowledgeBase = null;
            //nb.setChisquareCriticalValue(6.63); //0.01 pvalue

            if (File.Exists(fileName))
            {
                string knowledgeBaseStr = File.ReadAllText(fileName, Encoding.UTF8);
                knowledgeBase = JsonConvert.DeserializeObject<KnowledgeBase>(knowledgeBaseStr);
            }

            if (knowledgeBase == null)
                nb.train();
            else
                nb = new NaiveBayesClassifier(knowledgeBase);

            int userInput;
            do
            {
                userInput = DisplayMenu();
                switch (userInput)
                {
                    case 1:
                        nb.train();
                        Console.ReadKey();
                        break;
                    case 2:
                        {
                            Console.WriteLine("Calculating predictions...");
                            foreach (string file in Directory.EnumerateFiles(baseDirPath + @"\test", "*.json"))//Environment.CurrentDirectory
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("\n\nPress any key to classify news in file: " + file.Substring(file.LastIndexOf('\\') + 1).ToUpper());
                                Console.ReadKey();

                                string json = File.ReadAllText(file);
                                List<DocItem> docItems = JsonConvert.DeserializeObject<List<DocItem>>(json);
                                Classify(nb, docItems);
                            }

                            foreach (string file in Directory.EnumerateFiles(baseDirPath + @"\test", "*.txt"))//Environment.CurrentDirectory
                            {
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("\n\nPress any key to classify news in file: " + file.Substring(file.LastIndexOf('\\') + 1).ToUpper());
                                ConsoleKeyInfo resp = Console.ReadKey();

                                string text = File.ReadAllText(file);
                                List<DocItem> docItems = nb.parseSimpleTextFile(text);
                                Classify(nb, docItems);
                                Console.ReadKey();
                            }
                        }
                        break;
                    case 3:
                        {
                            Console.WriteLine("Ënter filename: (*.txt) or (*.json)");
                            var file = Console.ReadLine();
                            var filePath = baseDirPath + "\\test\\" + file;
                            if (File.Exists(filePath))
                            {
                                string text = File.ReadAllText(filePath);
                                List<DocItem> docItems;
                                if (file.Contains("json"))
                                    docItems = JsonConvert.DeserializeObject<List<DocItem>>(text);
                                else
                                    docItems = nb.parseSimpleTextFile(text);

                                Classify(nb, docItems);
                            }
                            else
                            {
                                Console.WriteLine("file " + filePath + " does not exist");
                            }
                            Console.ReadKey();
                        }
                        break;
                    case 4:
                        nb.evaluate();
                        Console.ReadKey();
                        break;
                    case 5:
                        Console.Clear();
                        break;
                }
            } while (userInput != 6);

            //string appSettingValue = ConfigurationManager.AppSettings["sampleApplication"];
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Good Bye !");
            Console.ReadKey();

            //String exampleEn = "I am English";
            //String outputEn = nb.predict(exampleEn);
            //System.out.format("The sentense \"%s\" was classified as \"%s\".%n", exampleEn, outputEn);

            //String exampleFr = "Je suis Français";
            //String outputFr = nb.predict(exampleFr);
            //System.out.format("The sentense \"%s\" was classified as \"%s\".%n", exampleFr, outputFr);

            //String exampleDe = "Ich bin Deutsch";
            //String outputDe = nb.predict(exampleDe);
            //System.out.format("The sentense \"%s\" was classified as \"%s\".%n", exampleDe, outputDe);
        }
    }
}