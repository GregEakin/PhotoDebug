﻿// Copyright © 2013-2016. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoTests
// FILE:		ExpectedException.cs
// AUTHOR:		Greg Eakin

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PhotoTests.Utilities
{
    public class ExpectedException
    {
        public static T AssertThrows<T>(Action action) where T : Exception
        {
            try
            {
                action.Invoke();
            }
            catch (T ex)
            {
                if (ex.GetType() != typeof(T)) throw;
                return ex;
            }

            Assert.Fail("Failed to throw exception!");
            return null;
        }
    }
}