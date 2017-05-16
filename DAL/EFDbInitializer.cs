using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.BL.Domain.Users;
using SS.BL.Domain.Analyses;
using SS.DAL.Properties;
using SS.DAL.Utilities;
using SS.DAL.EFAnalyses;
using Newtonsoft.Json.Linq;
using System.Web.Hosting;
using System.Diagnostics;

namespace SS.DAL
{
    public class EFDbInitializer : DropCreateDatabaseIfModelChanges<EFDbContext>
    {
        private AnalysisRepository repo;
        protected override void Seed(EFDbContext context)
        {
            repo = new AnalysisRepository(context);
         string directory = HostingEnvironment.MapPath("~/DefaultTrainingSets/");
         string[] fileEntries = Directory.GetFiles(directory);
         foreach (string file in fileEntries)
         {
            TrainingSet set = new TrainingSet()
            {
               Name = Path.GetFileName(file),
               dataSet = File.ReadAllText(file)
            };
            repo.createNewModelsFromTrainingsfile(set);
         }
      }
      //   TrainingSet set = new TrainingSet()
      //   {
      //      Name = Path.GetFileName(fileEntries[0]),
      //      dataSet = File.ReadAllText(fileEntries[0])
      //   };
      //   repo.createNewModelsFromTrainingsfile(set);
      //}

   }
}
