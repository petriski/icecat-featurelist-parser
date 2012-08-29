using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IceCatParsing
{
    public class Feature
    {
        public Feature(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; set; }
    }
}
