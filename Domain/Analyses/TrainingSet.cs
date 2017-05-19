using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SS.BL.Domain.Analyses
{
    public class TrainingSet
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string dataSet { get; set; }
        
    }
}
