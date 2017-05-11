using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.BL.Domain.Users;
using SS.BL.Domain.Analyses;

namespace SS.DAL
{
    public class EFDbInitializer : DropCreateDatabaseIfModelChanges<EFDbContext>
    {
        protected override void Seed(EFDbContext context)
        {
         //First
         String golden = Properties.Resources.datasetqframe.ToString();
         var training = new TrainingSet();
         training.Name = "Golden standard";
         training.dataSet = golden;

         context.TrainingSet.Add(training);
         
         //Second
         //String notgolden = Properties.Resources.datasetqframe.ToString();
         //var training2 = new TrainingSet();
         //training2.Name = "Not golden";
         //training2.dataSet = notgolden;

         //context.TrainingSet.Add(training2);

         context.SaveChanges();
        }

    }
}
