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

namespace SS.DAL
{
    public class EFDbInitializer : DropCreateDatabaseIfModelChanges<EFDbContext>
    {
        protected override void Seed(EFDbContext context)
        {
            IAnalysisRepository an = new IAnalysisRepository();
            an.createNewModelsFromTrainingsfile();
        }

    }
}
