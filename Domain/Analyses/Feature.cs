using SS.BL.Domain.Analyses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SS.BL.Domain.Analyses
{
    public class Feature
    {
        
        [Key]
        public long Id { get; set; }
        public string FeatureName { get; set; }
        public double Value { get; set; }
        public MinMaxValue MinMaxValue { get; set; }
    }

   


}
