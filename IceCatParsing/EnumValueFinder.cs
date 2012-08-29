using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IceCatParsing
{
    public static class EnumValueFinder
    {
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
