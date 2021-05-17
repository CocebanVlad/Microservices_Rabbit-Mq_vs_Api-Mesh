using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace TextProcessor.Tests
{
    [TestClass]
    public class TextProcessorServiceTests
    {
        private readonly TextProcessorService service = new TextProcessorService();

        [TestMethod]
        public void Split_Text_MustReturnExpectedResult()
        {
            CollectionAssert.AreEqual(new List<string>() { "vlad", "is", "the", "best" }, service.Split("vlad is the best"));
        }
    }
}