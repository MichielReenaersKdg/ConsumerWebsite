using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.BL.Domain.Analyses
{
    public class Model
    {
        [Key]
        public long Id { get; set; }
        [Index("IX_SetAlgorithm",1, IsUnique = true)]
        [MaxLength(200)]
        public string DataSet { get; set; }
        public DateTime Date { get; set; }
        [Index("IX_SetAlgorithm",2, IsUnique = true)]
        public AlgorithmName AlgorithmName { get; set; }
        public string ModelPath { get; set; }

        public int NumberOfSolvents { get; set; }
        public int NumberOfFeatures { get; set; }
        public ICollection<Cluster> Clusters { get; set; }
        public TrainingSet trainingSet { get; set; }
    }
}
