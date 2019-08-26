using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace TagDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            //var doc = new XmlDocument();
            //doc.Load("tags.xml");
            //if (doc.DocumentElement == null) return;
            //foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            //{
            //    var text = node.InnerText;
            //    Console.WriteLine(text);
            //}

            var xml = XDocument.Load("tags.xml");
            if (xml.Root == null) return;

            foreach (var node in xml.Root.Elements())
            {
                Console.WriteLine("===============");
                Console.Write(node.Name);
                var name = node.Attribute("name");
                if (name == null) continue;
                Console.Write(", {0}", name.Value);

                var result = node.Descendants("desc")
                    .FirstOrDefault(el => el.Attribute("lang")?.Value == "en");
                if (result == null) continue;
                Console.Write(",  {0}", result.Value);

                Console.WriteLine();


                ////////////////////////////////////////
                if (name.Value != "Exif::Main") continue;

                var groups = new HashSet<string>();
                foreach (var tags in node.Elements())
                {
                    //Console.Write(tags.Name);

                    //var name1 = tags.Attribute("name");
                    //if (name1 != null)
                    //    Console.Write(", {0}", name1.Value);

                    var g1 = tags.Attribute("g1");
                    if (g1 == null)continue;
                    //if (g1 != null)
                        //    Console.WriteLine(", {0}", g1);
                        // groups.Add(g1.Value);

                    var g2 = tags.Attribute("g2");
                    if (g2 == null) continue;
                    Console.WriteLine(" {0}, {1} ", g1, g2);

                    //var id = tags.Attribute("id");
                    //if (id != null)
                    //    Console.Write(",  0x{0:X}", int.Parse(id.Value));

                    //var result2 = tags.Descendants("desc")
                    //    .FirstOrDefault(el => el.Attribute("lang")?.Value == "en");
                    //if (result2 == null) continue;
                    //Console.Write(",  {0}", result2.Value);


                    //Console.WriteLine();
                }

                foreach (var @group in groups)
                {
                    Console.WriteLine(@group);
                }
            }

            //var query = from c in xml.Root.Descendants("contact")
            //    where (int)c.Attribute("id") < 4
            //    select c.Element("firstName")?.Value + " " +
            //           c.Element("lastName")?.Value;

            //foreach (var name in query)
            //{
            //    Console.WriteLine("Contact's Full Name: {0}", name);
            //}
        }
    }
}