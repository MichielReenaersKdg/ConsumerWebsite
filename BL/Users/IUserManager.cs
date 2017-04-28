using SS.BL.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.BL.Users
{
    public interface IUserManager
    {
        //User
        User CreateUser(string firstname, string lastname, string email, string avatarUrl);
        User ReadUser(string email);
        User ReadUser(long id);
        User UpdateUser(User user);
        User LeaveOrganisation(long id);
        User JoinOrganisation(string email, long id);
        User ReadOrganiser(long id);
        IEnumerable<User> ReadAllUsers();
        IEnumerable<User> ReadUsersForOrganisation(long id);
        void DeleteUser(User user);

        //Organisation
        Organisation CreateOrganisation(string name, string logoUrl, User user);
        Organisation ReadOrganisation(long id);
      Organisation ReadOrganisationByName(string name);
      IEnumerable<Organisation> ReadAllOrganisations();
        Organisation UpdateOrganisation(Organisation organisation);
        void BlockOrganisation(long id);
        void AllowOrganisation(long id);
        void DeleteOrganisation(long id);

    }
}
