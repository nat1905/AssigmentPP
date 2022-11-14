using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory
{
    public static class Extensions
    {
        private static Random randomValue = new Random();

        public static T ChooseRandomValue<T>(this IList<T> source)
        {
            int randomIndexOfValue = randomValue.Next(source.Count);
            return source[randomIndexOfValue];
        }
    }


}