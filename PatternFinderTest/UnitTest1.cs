using Microsoft.VisualStudio.TestTools.UnitTesting;
using PatternFinder;

namespace PatternFinderTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestFind()
        {
            var pattern = Pattern.Transform("456?89?B");

            var data1 = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };
            Assert.IsTrue(Pattern.Find(data1, pattern, out var o1) && o1 == 2);

            var data2 = new byte[] { 0x01, 0x23, 0x45, 0x66, 0x89, 0x6B, 0xCD, 0xEF };
            Assert.IsTrue(Pattern.Find(data2, pattern, out var o2) && o2 == 2);

            var data3 = new byte[] { 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11 };
            Assert.IsFalse(Pattern.Find(data3, pattern, out _));
        }

        [TestMethod]
        public void TestScan()
        {
            var data = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF };
            var p1 = new Signature("pattern1", "456?89?B");
            var p2 = new Signature("pattern2", "1111111111");
            var p3 = new Signature("pattern3", "AB??EF");
            var signatures = new[] { p1, p2, p3 };
            
            var result = Pattern.Scan(data, signatures);
            Assert.IsTrue(result.Length == 2);
            CollectionAssert.Contains(result, p1);
            CollectionAssert.DoesNotContain(result, p2);
            CollectionAssert.Contains(result, p3);
        }
    }
}