using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace IceCatParsing
{
    class Program
    {
        private static XDocument doc;
        private static IEnumerable<XElement> categories;

        static void Main()
        {
            LoadFileAndLoadCategories();

            FindCategoryFeatures();
            
            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        private static void FindCategoryFeatures()
        {
            //Result
            var result = new List<FeatureGroup>();

            Console.WriteLine("Please enter category ID.");
            var categoryid = Console.ReadLine().Trim();

            Console.WriteLine("Please enter the language ID you'd like to return. (e.g. 1 for English, 2 for Dutch, ...)");
            var languagid = Console.ReadLine().Trim();

            var category = categories.SingleOrDefault(
                    e => e.Attribute("ID").Value == categoryid);

            if (category != null)
            {
                Console.WriteLine("Category found.");
                var groups = category.Elements("CategoryFeatureGroup");
                foreach (var group in groups)
                {
                    var groupName =
                        group.Element("FeatureGroup").Elements("Name").SingleOrDefault(
                            n => n.Attribute("langid").Value == languagid).Attribute("Value").Value;
                    var groupId = group.Attribute("ID").Value;
                    var featureGroup = new FeatureGroup(Convert.ToInt32(groupId), groupName);

                    result.Add(featureGroup);

                    var features =
                        category.Elements("Feature").Where(
                            f => f.Attribute("CategoryFeatureGroup_ID").Value == groupId);

                    foreach (var feature in features)
                    {
                        var featureName = feature.Elements("Name").SingleOrDefault(
                            n => n.Attribute("langid").Value == languagid).Attribute("Value").Value;
                        var featureId = feature.Attribute("ID").Value;

                        featureGroup.Features.Add(new Feature(Convert.ToInt32(featureId), featureName));
                    }
                }
                Console.WriteLine(result.Count + " groups found.");

                var outputType = AskForOutputType();
                switch (outputType)
                {
                    case OutputType.Console:
                        foreach (var fgroup in result)
                        {
                            Console.WriteLine(fgroup.Name);
                            Console.WriteLine("******************************");

                            foreach (var feature in fgroup.Features)
                            {
                                Console.WriteLine(feature.Name);
                            }

                            Console.WriteLine(Environment.NewLine + Environment.NewLine);
                        }
                        break;
                    case OutputType.CSV:
                        string filePath = category.Attribute("ID").Value + @".csv";
                        string delimiter = ";";
                        var output = new StringBuilder();
                        foreach (var fgroup in result)
                        {
                            foreach (var feature in fgroup.Features)
                            {
                                output.AppendLine(string.Join(delimiter, fgroup.Id, fgroup.Name, feature.Id, feature.Name));
                            }
                        }
                        File.WriteAllText(filePath, output.ToString());
                        break;
                }
            }
            else
            {
                Console.WriteLine("Category was NOT found.");
            }

            Console.WriteLine("Do you want to find another category (yes/no)?");
            StopOrContinue();

        }

        private static void StopOrContinue()
        {
            var answer = Console.ReadLine();

            switch(answer.ToLower().Trim())
            {
                case "yes":
                    FindCategoryFeatures();
                    break;
                case "no":
                    return;
                default:
                    Console.WriteLine("We need yes or no");
                    StopOrContinue();
                    break;
            }
        }

        private static void LoadFileAndLoadCategories()
        {
            //Filename
            const string filename = "CategoryFeaturesList.xml";
            if (!File.Exists(filename))
            {
                Console.WriteLine("Bummer, we can't find the " + filename + " file. Sure you dropped it in the same directory you placed this exe in?");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("We're loading the XML file. Please wait, this can take a while..." + Environment.NewLine + "As long as you don't see the words 'Finished loading.', we're not done.");

            //Gradually work our way down the XML tree
            doc = XDocument.Load(filename);

            var root = doc.Root;
            var response = root.Element("Response");
            var list = response.Element("CategoryFeaturesList");
            categories = list.Elements("Category");

            Console.WriteLine("Finished loading.");
        }

        public static OutputType AskForOutputType()
        {
            Console.WriteLine("How do you want the output? Type one of these (case-senstitive) values:");
            Console.WriteLine(EnumValueFinder.GetValues<OutputType>().Select(e => e.ToString()).Aggregate((a,b) => a + "," + b));

            var chosenOutput = Console.ReadLine();
            OutputType castOutput;
            var parsed = Enum.TryParse(chosenOutput, out castOutput);
            if (parsed)
            {
                return castOutput;
            }
            
            Console.WriteLine("Oops, " + chosenOutput + " is not really a known type of output. Try again. Remember: case-sensitive");
            return AskForOutputType();
        }
    }

    public enum OutputType
    {
        Console,
        CSV
    }
}
