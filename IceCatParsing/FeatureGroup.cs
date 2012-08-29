using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IceCatParsing
{
    public class FeatureGroup
    {
        public FeatureGroup(int id, string name)
        {
            Features = new List<Feature>();
            Id = id;
            Name = name;
        }

        public IList<Feature> Features { get; set; }
        public int Id { get; set; }
       public string Name { get; set; }
    } 
}
