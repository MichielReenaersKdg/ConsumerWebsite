using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.BL.Domain.Analyses
{
    public class MinMaxValue
    {
        public long Id { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        //0.4.9 Removed FeatureName as Circular dependency was present when Enum FeatureName changed to String FeatureName in Features. 
        public string Regex { get; set; }
    }
}
