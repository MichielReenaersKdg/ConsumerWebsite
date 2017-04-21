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
        public string Name { get; set; }
        public string CasNumber { get; set; }
        public double DistanceToClusterCenter { get; set; }
        public ICollection<Feature> Features { get; set; }
    }
}
