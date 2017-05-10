using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SS.BL.Users;
using SS.DAL;
using SS.DAL.EFUsers;
using SS.UI.Web.MVC.Models;
using SS.BL.Analyses;
using SS.BL.Domain.Analyses;
using SS.DAL.EFAnalyses;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace SS.UI.Web.MVC.Migrations
{


    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
         AddUserAndRole(context);
         AddTrainingset(context);
        }

      private void AddTrainingset(ApplicationDbContext context)
      {
            //First
         String golden = Properties.Resources.datasetqframe.ToString();
         var training = new TrainingSet();
         training.Name = "Golden standard";
         training.dataSet = golden;
         var manager = new AnalysisManager(new AnalysisRepository(new EFDbContext()));
         var trainingSet = manager.CreateTrainingSet(training);
         if (trainingSet == null)
         {
            throw new Exception("trainingSet not created");
         }

            //Second
            String notgolden = Properties.Resources.datasetqframe.ToString();
            var training2 = new TrainingSet();
            training2.Name = "Not golden";
            training2.dataSet = notgolden;
            var manager2 = new AnalysisManager(new AnalysisRepository(new EFDbContext()));
            var trainingSet2 = manager.CreateTrainingSet(training2);
            if (trainingSet2 == null)
            {
                throw new Exception("trainingSet not created");
            }
        }

      bool AddUserAndRole(ApplicationDbContext context)
        {
            IdentityResult ir;
            IdentityResult ir1;
            var rm = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            ir = rm.Create(new IdentityRole("SuperAdministrator"));
            ir1 = rm.Create(new IdentityRole("User"));
            var um = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var user = new ApplicationUser()
            {
                UserName = "sussolvents@admin.com",
                Email = "sussolvents@admin.com"
            };
            var user1 = new ApplicationUser()
            {
                UserName = "sussolvents@user.com",
                Email = "sussolvents@user.com"

            };
            ir = um.Create(user, "sussolvents");
            ir = um.Create(user1, "sussolvents");

            if (ir.Succeeded == false)
                return ir.Succeeded;
            ir = um.AddToRole(user.Id, "SuperAdministrator");
            if (ir1.Succeeded == false)
                return ir1.Succeeded;
            ir1 = um.AddToRole(user1.Id, "User");
            var userManager = new UserManager(new UserRepository(new EFDbContext()));
            userManager.CreateUser("admin", "admin", "sussolvents@admin.com", "");
            userManager.CreateUser("user", "user", "sussolvents@user.com", "");
            return ir.Succeeded;
        }
    }
}