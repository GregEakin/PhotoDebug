// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	JpegParser
// FILE:		Class1.cs
// AUTHOR:		Greg Eakin

using System.Collections.Generic;
using System.IO;

namespace JpegParser
{
    public class Class1
    {
        readonly Dictionary<string, Data> dict = new Dictionary<string, Data>();

        public class Data
        {
            readonly List<string> lines = new List<string>();

            public void AddLine(string line)
            {
                lines.Add(line);
            }

            public List<string> Lines
            {
                get
                {
                    return lines;
                }
            }
        }

        public Class1(string fileName)
        {
            // Read the file and display it line by line.
            using (var file = new StreamReader(fileName))
            {
                Data last = null;
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("//"))
                        continue;

                    if (line.Contains("→"))
                    {
                        var x = line.Split('→');
                        var key = x[0].Trim();
                        var data = x[1].Trim();

                        last = new Data();
                        last.AddLine(data);

                        // Assert.IsFalse(dict.ContainsKey(key));
                        dict.Add(key, last);
                    }
                    else if (line.StartsWith("|"))
                    {
                        var x = line.Split('|');
                        var data = x[1].Trim();
                        last.AddLine(data);
                    }
                    // else
                    //    Assert.Fail("bad line");
                }
            }
        }

        public Dictionary<string, Data> Dict
        {
            get
            {
                return dict;
            }
        }
    }
}
