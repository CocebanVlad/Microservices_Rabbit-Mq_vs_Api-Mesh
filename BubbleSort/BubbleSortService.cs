using System;
using System.Collections.Generic;
using System.Linq;

namespace BubbleSort
{
    public class BubbleSortService
    {
        public List<string> Sort(List<string> list)
        {
            var listCopy = list.ToList();

            for (var i = 0; i < list.Count; i++)
            {
                for (var j = i; j < list.Count; j++)
                {
                    if (string.Compare(listCopy[i], listCopy[j], StringComparison.InvariantCultureIgnoreCase) > 0)
                    {
                        (listCopy[i], listCopy[j]) = (listCopy[j], listCopy[i]);
                    }
                }
            }

            return listCopy;
        }
    }
}