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
        private static List<string> eanCodes = new List<string>();

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
            var result = new List<Category>();

            Console.WriteLine("Do you want all category features? (yes/no/some)");
            var input = Console.ReadLine().Trim().ToLower();
            var all = input == "yes" || input == "y";
            var some = input == "some" || input == "s";
            var outputType = AskForOutputType();
            var languagid = AskForLanguage();

            var selectedCategories = new List<XElement>();

            if (all)
            {
                selectedCategories = categories.ToList();
            }
            else if(some)
            {
                var parser = new Microsoft.VisualBasic.FileIO.TextFieldParser("codima-selection.csv");
                parser.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
                parser.SetDelimiters(new string[] { ";" });

                while (!parser.EndOfData)
                {
                    string categoryId = parser.ReadFields()[0];
                    selectedCategories.Add(categories.SingleOrDefault(e => e.Attribute("ID").Value == categoryId));
                }
            }
            else
            {
                XElement category = AskForCategory();
                if (category != null)
                {
                    selectedCategories.Add(category);
                }
                
                XElement extraCategory;
                bool stop;
                do
                {
                    extraCategory = StopOrContinue(out stop);
                    if (extraCategory == null){
                        Console.WriteLine("Category was NOT found.");
                    }
                    else
                    {
                        selectedCategories.Add(extraCategory);
                    }
                } while (!stop);
            }

            foreach(var category in selectedCategories)
            {
                var categoryNames = category.Elements("Name");
                var id = category.Attribute("ID").Value;
                var name = id;
                if (categoryNames != null && categoryNames.Any())
                {
                    name = categoryNames.SingleOrDefault(
                        n => n.Attribute("langid").Value == languagid).Attribute("Value").Value;
                }

                Console.WriteLine("Category " + name + " found.");
                
                var groups = category.Elements("CategoryFeatureGroup");
                var featureGroups = new List<FeatureGroup>();
                foreach (var group in groups)
                {
                    var featuregroup = group.Element("FeatureGroup");
                    if (featuregroup != null) { 
                        var names = featuregroup.Elements("Name");
                        var groupName = names.SingleOrDefault(n => n.Attribute("langid").Value == languagid).Attribute("Value").Value;
                        var groupId = group.Attribute("ID").Value;
                        var featureGroup = new FeatureGroup(Convert.ToInt32(groupId), groupName);

                        featureGroups.Add(featureGroup);

                        var features =
                            category.Elements("Feature").Where(
                                f => f.Attribute("CategoryFeatureGroup_ID").Value == groupId);

                        foreach (var feature in features)
                        {
                            var featureNameElement = feature.Elements("Name").SingleOrDefault(
                                n => n.Attribute("langid").Value == languagid);
                            var featureName = string.Empty;
                            if(featureNameElement != null)
                            {
                                featureName = featureNameElement.Attribute("Value").Value;
                            }
                            var featureId = feature.Attribute("ID").Value;
                            if (!string.IsNullOrEmpty(featureName))
                            {
                                featureGroup.Features.Add(new Feature(Convert.ToInt32(featureId), featureName));
                            }
                        }
                    }
                }
                
                Console.WriteLine(result.Count + " groups found for category " + name);
                result.Add(new Category(id,name){Groups = featureGroups});
            }

            switch (outputType)
            {
                case OutputType.Console:
                    foreach(var item in result)
                    {
                        Console.WriteLine("================================================");
                        Console.WriteLine(item.Name + " (" + item.Id + ")");
                        Console.WriteLine("================================================");

                        foreach (var fgroup in item.Groups)
                        {
                            Console.WriteLine(fgroup.Name);
                            Console.WriteLine("******************************");

                            foreach (var feature in fgroup.Features)
                            {
                                Console.WriteLine(feature.Name);
                            }

                            Console.WriteLine(Environment.NewLine + Environment.NewLine);
                        }
                    }
                    break;
                case OutputType.CSV:

                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icecat.csv");

                    string delimiter = ";";
                        var output = new StringBuilder();
                        output.AppendLine(string.Join(delimiter, "CategoyId", "CategoryName", "GroupId", "GroupName", "FeatureId",
                                                      "FeatureName"));
                    foreach(var item in result)
                    {
                        
                        foreach (var fgroup in item.Groups)
                        {
                            foreach (var feature in fgroup.Features)
                            {
                                output.AppendLine(string.Join(delimiter, item.Id, item.Name, fgroup.Id, fgroup.Name, feature.Id,
                                                              feature.Name));
                            }
                        }
                        
                    }
                    File.WriteAllText(filePath, output.ToString());

                    break;
            }
        }

        private static string AskForLanguage()
        {
            Console.WriteLine("Please enter the language ID you'd like to return. (e.g. 1 for English, 2 for Dutch, ...)");
            return Console.ReadLine().Trim();
        }

        private static XElement AskForCategory()
        {
            Console.WriteLine("Please enter category ID.");
            var categoryid = Console.ReadLine().Trim();

            return categories.SingleOrDefault(
                e => e.Attribute("ID").Value == categoryid);
        }

        private static XElement StopOrContinue(out bool stop)
        {
            Console.WriteLine("Do you want more categories? (yes/no)");
            var answer = Console.ReadLine();

            stop = false;
            switch(answer.ToLower().Trim())
            {
                case "yes":
                    return AskForCategory();
                case "no":
                    stop = true;
                    return null;
                default:
                    Console.WriteLine("We need yes or no");
                    return StopOrContinue(out stop);
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
