using SS.BL.Domain.Users;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.DAL.EFUsers
{
    public interface IUserRepository
    {
        //User
        User CreateUser(User user);
        User ReadUser(string email);
        User ReadUser(long id);
        IEnumerable<User> ReadAllUsers();
        IEnumerable<User> ReadUsersForOrganisation(long id);
        User ReadOrganiser(long id);
        User LeaveOrganisation(long id);
        User JoinOrganisation(string email, long id);
        User UpdateUser(User user);
        void DeleteUser(User user);

        //Organisation
        Organisation CreateOrganisation(Organisation organisation, User user);
        Organisation ReadOrganisation(long id);
        Organisation ReadOrganisationByName(string name);
        IEnumerable<Organisation> ReadAllOrganisations();
        Organisation UpdateOrganisation(Organisation organisation);
        void BlockOrganisation(long id);
        void AllowOrganisation(long id);
        void DeleteOrganisation(long id);



    }
}
