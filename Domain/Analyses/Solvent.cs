using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.BL.Domain.Analyses
{
    public class Solvent
    {
        [Key]
        public long Id { get; set; }
      public string Source { get; set; }
        public string Name { get; set; }
        public string CasNumber { get; set; }
      public string EgNumber { get; set; }
      public string EgAnnexNumber { get; set; }
      public string EHS_S_SCORE { get; set; }
      public string EHS_E_SCORE { get; set; }
      public string EHS_H_SCORE { get; set; }
      public string EHS_Color_Code { get; set; }
      public double DistanceToClusterCenter { get; set; }
        public ICollection<Feature> Features { get; set; }
        public TrainingSet trainingSet { get; set; }
    }
}
