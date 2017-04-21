using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SS.BL.Users;
using SS.DAL;
using SS.DAL.EFUsers;
using SS.UI.Web.MVC.Models;

namespace SS.UI.Web.MVC.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            AddUserAndRole(context);
            
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