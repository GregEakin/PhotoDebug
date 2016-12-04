// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		XmpData.cs
// AUTHOR:		Greg Eakin

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhotoLib.Tiff;
using System.IO;
using System.Linq;
using System.Xml;

namespace PhotoTests.Prototypes
{
    [TestClass]
    public class XmpData
    {
        [TestMethod]
        public void DumpXmpData()
        {
            const string fileName = @"C:..\..\Photos\5DIIIhigh.CR2";
            DumpXmpInfo(fileName);
        }

        private static void DumpXmpInfo(string fileName)
        {
            using (var fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                var rawImage = new RawImage(binaryReader);
                var image = rawImage.Directories.First();

                var imageFileEntry02BC = image.Entries.Single(e => e.TagId == 0x02BC && e.TagType == 1);
                // Assert.AreEqual(8192u, imageFileEntry02BC.NumberOfValue);
                // Assert.AreEqual(72132u, imageFileEntry02BC.ValuePointer);
                var xmpData = RawImage.ReadBytes(binaryReader, imageFileEntry02BC);
                var xmp = System.Text.Encoding.UTF8.GetString(xmpData);

                DumpXmp(xmp);
            }
        }

        private static void DumpXmp(string xmp)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmp);

            // <?xpacket begin = '﻿' id='W5M0MpCehiHzreSzNTczkc9d'?>
            //   <x:xmpmeta xmlns:x="adobe:ns:meta/">
            //     <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
            //       <rdf:Description rdf:about="" xmlns:xmp="http://ns.adobe.com/xap/1.0/">
            //         <xmp:Rating>0</xmp:Rating>
            //       </rdf:Description>
            //     </rdf:RDF>
            //   </x:xmpmeta>  

            var manager = new XmlNamespaceManager(xmlDoc.NameTable);
            // var dic = manager.GetNamespacesInScope(XmlNamespaceScope.All);
            manager.AddNamespace("x", "adobe:ns:meta/");
            manager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
            manager.AddNamespace("xmp", "http://ns.adobe.com/xap/1.0/");

            const string query = "x:xmpmeta/rdf:RDF/rdf:Description/xmp:Rating";
            var nodes = xmlDoc.SelectNodes(query, manager);
            Assert.IsNotNull(nodes);
            CollectionAssert.AreEqual(new[] {"0"}, nodes.Cast<XmlNode>().Select(n => n.InnerText).ToArray());
        }
    }
}
