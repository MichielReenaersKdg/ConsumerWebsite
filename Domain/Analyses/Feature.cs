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
        [Required]
        public string FeatureName { get; set; }
        public IEnumerable<double> Values { get; set; }
        //public MinMaxValue MinMaxValue { get; set; }
    }

   


}
