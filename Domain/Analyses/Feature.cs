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
        //0.4.9 Changed FeatureName to string in order to comply with demand of a database where one can add nw featurenames. 
        public string featureName { get; set; }
        //public double Value { get; set; }
        //0.4.9 Changed Double Value to FeatureValue value in order to have multiple values per Feature when enum feature name changed to string featurename
        public FeatureValue value { get; set; }
        public MinMaxValue minMaxValue { get; set; }
    }

   


}
