﻿using CGM.Communication.Patterns;
using System;
using System.Collections.Generic;
using System.Text;

namespace CGM.Communication.MiniMed.Requests.Patterns
{
    public class PumpRequestPattern : ReportMultiPatternAnd
    {
        public PumpRequestPattern() 
        {
            this.Patterns.Add(new ReportPattern(new byte[] { 0x00 }, 1));
            this.Patterns.Add(new ReportPattern(new byte[] { 0x51 }, 5));
            this.Patterns.Add(new ReportPattern(new byte[] { 0x05 }, 38));
        }
    }
}
