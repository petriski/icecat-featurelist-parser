using System.Collections.Generic;

namespace IceCatParsing
{
    public class Category
    {
        public Category(string id, string name)
        {
            Groups = new List<FeatureGroup>();
            Id = id;
            Name = name;
        }

        public IList<FeatureGroup> Groups { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
