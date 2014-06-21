// Project Console Application 0.1
// Copyright © 2014-2014. All Rights Reserved.
// 
// SUBSYSTEM:	PhotoDebug
// FILE:		TestStuff.cs
// AUTHOR:		Greg Eakin
namespace PhotoTests.Jpeg
{
    using System;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TestStuff
    {
        public void DecodeImage()
        {
            if (!SoiMarker())
            {
                throw new Exception("Error");
            }

            while (!SofMarker())
            {
                this.InterpretMarkers();
            }

            this.DecodeFrame();
        }

        private bool SofMarker()
        {
            throw new NotImplementedException();
        }

        private bool SoiMarker()
        {
            throw new NotImplementedException();
        }

        public void DecodeFrame()
        {
            this.InterpretFrameHeader();
            do
            {
                while (!SosMarker())
                {
                    this.InterpretMarkers();
                }

                this.DecodeScan();
            }
            while (!EoiMarker());
        }

        private void InterpretMarkers()
        {
            // DHT
            // DAC
            // DQT
            // DRI
            // APP
            // COM
            throw new System.NotImplementedException();
        }

        private bool SosMarker()
        {
            throw new System.NotImplementedException();
        }

        private bool EoiMarker()
        {
            throw new System.NotImplementedException();
        }

        private void InterpretFrameHeader()
        {
            throw new System.NotImplementedException();
        }

        private void DecodeScan()
        {
            InterpretScanHeader();
            InitializeDecoder();
            do
            {
                DecodeRestartInterval();
            }
            while (MoreIntervals());
        }

        private void InitializeDecoder()
        {
            throw new System.NotImplementedException();
        }

        private void InterpretScanHeader()
        {
            throw new System.NotImplementedException();
        }

        private bool MoreIntervals()
        {
            // true when the expected number of restart intervals has been decoded.
            throw new System.NotImplementedException();
        }

        private void DecodeRestartInterval()
        {
            ResetDecoder();
            do
            {
                this.DecodeMcu();
            }
            while (MoreMcu());

            FindMarker();
        }

        private void DecodeMcu()
        {

            for (var n = 0; n < Nb; n++)
            {
                this.DecodeDataUnit();
            }
        }

        //nb is number of data units in a MCU
        private const int Nb = 0;

        private void DecodeDataUnit()
        {
            throw new NotImplementedException();
        }

        private void FindMarker()
        {
            throw new System.NotImplementedException();
        }

        private void ResetDecoder()
        {
            throw new System.NotImplementedException();
        }

        private bool MoreMcu()
        {
            throw new System.NotImplementedException();
        }

        [TestMethod]
        public void Test1()
        {

        }
    }
}