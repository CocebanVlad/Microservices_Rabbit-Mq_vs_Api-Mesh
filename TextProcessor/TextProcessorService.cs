using System;
using System.Collections.Generic;
using System.Linq;

namespace TextProcessor
{
    public class TextProcessorService
    {
        public List<string> Split(string text)
        {
            return text?.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}