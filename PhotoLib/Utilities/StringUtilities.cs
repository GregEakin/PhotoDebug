// Project Photo Library 0.1
// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
// FILE:		StringUtilities.cs
// AUTHOR:		Greg Eakin

using System.Collections;
using System.Globalization;
using System.Text;

namespace PhotoLib.Utilities
{
    public static class StringUtilities
    {
        public static string ToReadableString(this IEnumerable list)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            foreach (var l in list)
                sb.Append(l + ", ");

            if (sb.Length > 2)
                sb.Remove(sb.Length - 2, 2);

            sb.Append("]");
            return sb.ToString();
        }
    }
}