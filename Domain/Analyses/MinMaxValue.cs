﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.BL.Domain.Analyses
{
    public class MinMaxValue
    {
        public long Id { get; set; }
        public FeatureName FeatureName { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double StepValue { get; set; }
    }
}
