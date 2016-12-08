// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	JpegParser
// FILE:		Class1.cs
// AUTHOR:		Greg Eakin

using System;
using System.Collections.Generic;
using System.IO;

namespace JpegParser
{
    public class BnfParser
    {
        public class Data
        {
            public void AddLine(string line)
            {
                Lines.Add(line);
            }

            public List<string> Lines { get; } = new List<string>();
        }

        public BnfParser(string fileName)
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

                        if (Dict.ContainsKey(key))
                            throw new Exception($"Duplicate entry {key}.");

                        Dict.Add(key, last);
                    }
                    else if (line.StartsWith("|"))
                    {
                        if (last == null)
                            throw new Exception("Last is null.");

                        var x = line.Split('|');
                        var data = x[1].Trim();
                        last.AddLine(data);
                    }
                    else
                        throw new Exception("bad line");
                }
            }
        }

        public Dictionary<string, Data> Dict { get; } = new Dictionary<string, Data>();
    }
}
