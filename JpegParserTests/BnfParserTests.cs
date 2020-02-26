// Copyright © 2013-2019. All Rights Reserved.
// 
// SUBSYSTEM: JpegParserTests
// FILE:  BnfParserTests.cs
// AUTHOR:  Greg Eakin

using System;
using System.Collections.Generic;
using System.IO;
using JpegParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JpegParserTests
{
    [TestClass]
    public class BnfParserTests
    {
        [TestMethod]
        public void FileExists()
        {
            Assert.IsTrue(File.Exists("JpegBnf.txt"));
        }

        [TestMethod]
        public void CheckFile()
        {
            var stuff = new BnfParser("JpegBnf.txt");
            var dict = stuff.Dict;

            foreach (var pair in dict)
            foreach (var line in pair.Value.Lines)
            {
                var x = line.Split(' ');
                foreach (var y in x)
                {
                    var token = y.Trim();
                    if (token == "|" || token == "…" || token == "ϵ" || token.StartsWith("BY") || token.EndsWith("()"))
                        continue;
                    if (token.StartsWith("<") && token.EndsWith(">"))
                        Assert.IsTrue(dict.ContainsKey(token), $"Token {token} not found");
                    else
                        Console.WriteLine("{0} → {1}  ==> {2}", pair.Key, line, token);
                }
            }
        }

        [TestMethod]
        public void DumpFile()
        {
            var stuff = new BnfParser("JpegBnf.txt");
            var dict = stuff.Dict;

            foreach (var pair in dict)
            {
                var index = 0;
                foreach (var line in pair.Value.Lines)
                    if (index++ == 0)
                    {
                        Console.WriteLine("{0} → {1}", pair.Key, line);
                    }
                    else
                    {
                        var blank = "".PadRight(pair.Key.Length);
                        Console.WriteLine("{0} | {1}", blank, line);
                    }
            }
        }

        [TestMethod]
        public void DeepDumpFile()
        {
            var stuff = new BnfParser("JpegBnf.txt");
            var dict = stuff.Dict;

            var allTokens = DumpBlock("<jpeg_data>", dict);
            var procTokens = new HashSet<string>();
            while (!allTokens.SetEquals(procTokens))
            {
                var temp = new HashSet<string>(allTokens);
                temp.ExceptWith(procTokens);
                foreach (var tt in temp)
                {
                    var blocks = DumpBlock(tt, dict);
                    allTokens.UnionWith(blocks);
                    procTokens.Add(tt);
                }
            }

            Console.WriteLine("======= Proc Tokens");
            foreach (var x in procTokens)
                Console.WriteLine(x);
        }

        private static HashSet<string> DumpBlock(string key, Dictionary<string, BnfParser.Data> dict)
        {
            var data = new HashSet<string>();
            var index = 0;
            foreach (var line in dict[key].Lines)
            {
                if (index++ == 0)
                    Console.Write("{0} → ", key);
                else
                    Console.Write("{0} | ", "".PadRight(key.Length));

                data.UnionWith(DumpLine(dict, line));
                Console.WriteLine();
            }

            Console.WriteLine();
            return data;
        }

        private static HashSet<string> DumpLine(Dictionary<string, BnfParser.Data> dict, string line)
        {
            var tokens = new HashSet<string>();

            if (line.Contains("|"))
            {
                Console.Write("{ ");
                Console.Write(line);
                Console.Write(" } ");
                return tokens;
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
                        tokens.UnionWith(DumpLine(dict, z.Lines[0]));
                    }
                    else
                    {
                        tokens.Add(token);
                        Console.Write(token);
                        Console.Write(" ");
                    }
                }
                else
                {
                    // retval.Add(token);
                    Console.Write(token);
                    Console.Write(" ");
                }
            }

            return tokens;
        }

        [TestMethod]
        public void DumpJpeg()
        {
            var stuff = new BnfParser("JpegBnf.txt");
            var dict = stuff.Dict;

            var key = "<jpeg_data>";
            var data = dict[key];

            var index = 0;
            foreach (var line in data.Lines)
                Console.WriteLine("{0} : {1}  → {2}", index++, key, line);
        }
    }
}