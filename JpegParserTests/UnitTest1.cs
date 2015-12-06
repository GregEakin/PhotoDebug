using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using JpegParser;
using System.Collections.Generic;

namespace JpegParserTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void FileExists()
        {
            Assert.IsTrue(File.Exists("JpegBnf.txt"));
        }

        [TestMethod]
        public void CheckFile()
        {
            var stuff = new Class1("JpegBnf.txt");
            var dict = stuff.Dict;

            foreach (var pair in dict)
            {
                foreach (var line in pair.Value.Lines)
                {
                    var x = line.Split(' ');
                    foreach (var y in x)
                    {
                        var token = y.Trim();
                        if (token.StartsWith("<") && token.EndsWith(">"))
                        {
                            Assert.IsTrue(dict.ContainsKey(token), string.Format("Token {0} not found", token));
                        }
                        else if (token.StartsWith("BY"))
                            continue;
                        else
                            Console.WriteLine("{0} → {1}", pair.Key, line);
                    }
                }
            }
        }

        [TestMethod]
        public void DumpFile()
        {
            var stuff = new Class1("JpegBnf.txt");
            var dict = stuff.Dict;

            foreach (var pair in dict)
            {
                var index = 0;
                foreach (var line in pair.Value.Lines)
                {
                    if (index++ == 0)
                        Console.WriteLine("{0} → {1}", pair.Key, line);
                    else
                    {
                        var blank = "".PadRight(pair.Key.Length);
                        Console.WriteLine("{0} | {1}", blank, line);
                    }
                }
            }
        }

        [TestMethod]
        public void DeepDumpFile()
        {
            var stuff = new Class1("JpegBnf.txt");
            var dict = stuff.Dict;

            var key = "<jpeg_data>";
            var value = dict[key];
            var allTokens = DumpBlock(key, value, dict);
            var procTokens = new HashSet<string>();
            while (!allTokens.SetEquals(procTokens))
            {
                var temp = new HashSet<string>(allTokens);
                temp.ExceptWith(procTokens);
                foreach (var tt in temp)
                {
                    var blocks = DumpBlock(tt, dict[tt], dict);
                    allTokens.UnionWith(blocks);
                    procTokens.Add(tt);
                }
            }
        }

        private static HashSet<string> DumpBlock(string key, Class1.Data value, Dictionary<string, Class1.Data> dict)
        {
            var data = new HashSet<string>();

            var index = 0;
            foreach (var line in value.Lines)
            {
                if (index++ == 0)
                {
                    Console.Write("{0} → ", key);
                }
                else
                {
                    var blank = "".PadRight(key.Length);
                    Console.Write("{0} | ", blank);
                }

                data.UnionWith(DumpLine(dict, line));
                Console.WriteLine();
            }

            return data;
        }

        private static HashSet<string> DumpLine(Dictionary<string, Class1.Data> dict, string line)
        {
            var retval = new HashSet<string>();

            if (line.Contains("|"))
            {
                Console.Write(line);
                return retval;
            }

            var x = line.Split(' ');
            foreach (var y in x)
            {
                var token = y.Trim();
                if (token.StartsWith("<") && token.EndsWith(">"))
                {
                    var z = dict[token];
                    if (z.Lines.Count == 1)
                    {
                        Console.Write(" { ");
                        retval.UnionWith(DumpLine(dict, z.Lines[0]));
                        Console.Write(" } ");
                    }
                    else
                    {
                        retval.Add(token);
                        Console.Write(token);
                    }
                }
                else
                {
                    // retval.Add(token);
                    Console.Write(token);
                }
            }

            return retval;
        }

        [TestMethod]
        public void DumpJpeg()
        {
            var stuff = new Class1("JpegBnf.txt");
            var dict = stuff.Dict;

            var key = "<jpeg_data>";
            var data = dict[key];

            var index = 0;
            foreach (var line in data.Lines)
            {
                Console.WriteLine("{0} : {1}  → {2}", index++, key, line);
            }
        }
    }
}
