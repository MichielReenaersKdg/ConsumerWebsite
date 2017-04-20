using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.BL.Domain.Analyses
{
    public class VectorData
    {
        public long Id { get; set; }
        public double Value { get; set; }
        public Feature feature { get; set; }
    }
}
