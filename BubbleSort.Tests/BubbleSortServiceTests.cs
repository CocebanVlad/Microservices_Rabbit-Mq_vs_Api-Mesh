using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace BubbleSort.Tests
{
    [TestClass]
    public class BubbleSortServiceTests
    {
        private readonly BubbleSortService service = new BubbleSortService();

        [TestMethod]
        public void Sort_Chars_MustReturnExpectedResult()
        {
            var chars = "abcdefghigklmnopqvrstxyz";
            Assert.AreEqual(chars, string.Join("", service.Sort(chars.Split().Reverse().ToList())));
        }

        [TestMethod]
        public void Sort_Word_MustReturnExpectedResult()
        {
            var words = new List<string>() { "vlad", "coceban", "bucovina", "watch", "iasi", "ialoveni" };
            CollectionAssert.AreEqual(new List<string>() { "bucovina", "coceban", "ialoveni", "iasi", "vlad", "watch" }, service.Sort(words));
        }
    }
}