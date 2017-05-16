using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.BL.Domain.Analyses
{
   public class VectorData
   {
      [Key]
      public int id { get; set; }

      public double value { get; set; }
   }
}
