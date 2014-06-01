// Project Photo Library 0.1
// Copyright © 2013-2014. All Rights Reserved.
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
        #region Public Methods and Operators

        public static string FormatWith(this string mask, params object[] parameters)
        {
            return string.Format(CultureInfo.InvariantCulture, mask, parameters);
        }

        public static string ToReadableString(this IEnumerable list)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            foreach (var l in list)
            {
                sb.Append(l + ", ");
            }
            if (sb.Length > 2)
            {
                sb.Remove(sb.Length - 2, 2);
            }
            sb.Append("]");
            return sb.ToString();
        }

        #endregion
    }
}