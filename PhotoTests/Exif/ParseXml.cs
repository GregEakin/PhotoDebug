using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;
using System.Xml.Linq;

namespace PhotoTests.Exif
{
    [TestClass]
    public class ParseXml
    {
        [TestMethod]
        public void AATest1()
        {
            using (var reader = XmlReader.Create(@"..\..\..\Samples\data3.xml"))
            {
                reader.ReadToFollowing("taginfo");
                var x = reader.AttributeCount;
                Assert.AreEqual(0, x);
            }
        }

        [TestMethod]
        public void AATest2()
        {
            var doc = XDocument.Load(@"..\..\..\Samples\data3.xml");
            var authors = doc.Descendants("table");
            foreach (var author in authors)
            {
                Console.WriteLine(author.Value);
            }
        }
    }
}
