﻿// Copyright (c) 2008 Jeffrey Powers for Fluxcapacity Open Source.
// Under the MIT License, details: License.txt.

namespace FJCore.Decoder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class JpegScan
    {
        private readonly List<JpegComponent> components = new List<JpegComponent>();
        public IList<JpegComponent> Components { get { return components.AsReadOnly(); } }

        private int maxV = 0, maxH = 0;
        internal int MaxH { get { return maxH; } }
        internal int MaxV { get { return maxV; } }

        public void AddComponent(byte id, byte factorHorizontal, byte factorVertical,
                                 byte quantizationID, byte colorMode)
        {
            JpegComponent component = new JpegComponent(this,
                id, factorHorizontal, factorVertical, quantizationID, colorMode);

            components.Add(component);

            // Defined in Annex A
            maxH = components.Max(x => x.factorH);
            maxV = components.Max(x => x.factorV);
        }

        public JpegComponent GetComponentById(byte Id)
        {
            return components.First(x => x.component_id == Id);
        }
    }
}