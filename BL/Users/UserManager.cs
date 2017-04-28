using SS.DAL.EFUsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.BL.Domain.Analyses;
using SS.BL.Domain.Users;

namespace SS.BL.Users
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository repo;

        public UserManager(IUserRepository repository)
        {
            this.repo = repository;
        }

        public User ReadUser(string email)
        {
            return repo.ReadUser(email);
        }

        public User ReadUser(long id)
        {
            return repo.ReadUser(id);
        }

        public User UpdateUser(User user)
        {
            return repo.UpdateUser(user);
        }

        public User LeaveOrganisation(long id)
        {
            return repo.LeaveOrganisation(id);
        }

        public User JoinOrganisation(string email, long id)
        {
            return repo.JoinOrganisation(email, id);
        }

        public User ReadOrganiser(long id)
        {
            return repo.ReadOrganiser(id);
        }

        public IEnumerable<User> ReadAllUsers()
        {
            return repo.ReadAllUsers();
        }

        public IEnumerable<User> ReadUsersForOrganisation(long id)
        {
            return repo.ReadUsersForOrganisation(id);
        }

        public void DeleteUser(User user)
        {
            repo.DeleteUser(user);
        }

        public Organisation CreateOrganisation(string name, string logoUrl, User user)
        {
            Organisation organisation = new Organisation()
            {
                Name = name,
                LogoUrl = logoUrl,
                Blocked = true,
                OrganisatorId = user.Id,
                DateCreated = null
            };
            return repo.CreateOrganisation(organisation, user);
        }

        public Organisation ReadOrganisation(long id)
        {
            return repo.ReadOrganisation(id);
        }
         public Organisation ReadOrganisationByName(String name)
      {
         return repo.ReadOrganisationByName(name);
      }
        public IEnumerable<Organisation> ReadAllOrganisations()
        {
            return repo.ReadAllOrganisations();
        }

        public Organisation UpdateOrganisation(Organisation organisation)
        {
            return repo.UpdateOrganisation(organisation);
        }

        public void BlockOrganisation(long id)
        {
            repo.BlockOrganisation(id);
        }

        public void AllowOrganisation(long id)
        {
            repo.AllowOrganisation(id);
        }

        public void DeleteOrganisation(long id)
        {
            repo.DeleteOrganisation(id);
        }

        public User CreateUser(string firstname, string lastname, string email, string avatarUrl)
        {
            User user = new User()
            {
                Firstname = firstname,
                Lastname = lastname,
                Email = email,
                AvatarUrl = avatarUrl,
                Organisation = null,
                
            };
            user.ClassifiedInstances = new List<ClassifiedInstance>();
            return repo.CreateUser(user);
        }
    }
}
