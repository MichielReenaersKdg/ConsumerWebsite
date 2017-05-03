using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using SS.BL.Domain.Users;
using SS.BL.Users;
using SS.DAL;
using SS.UI.Web.MVC.Controllers.Utils;
using SS.UI.Web.MVC.Models;
using SS.UI.Web.MVC.Providers;
using UI_MVC;
using UI_MVC.Results;

namespace SS.UI.Web.MVC.Controllers
{
    [System.Web.Http.Authorize]
    [System.Web.Http.RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;
        private readonly IUserManager _userMgr;

        public AccountController(IUserManager userManager)
        {
            this._userMgr = userManager;
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        //GET api/Account/GetRole
        [System.Web.Http.Route("GetRole")]
        public string GetRole(string email)
        {
            var rm = new RoleStore<IdentityRole>(new ApplicationDbContext());
            var roleManager = new RoleManager<IdentityRole>(rm);
            var roleNames = roleManager.Roles.ToList();
            var role = UserManager.FindByEmail(email).Roles.First();
            


           for (var i = 0; i < roleNames.Count; i++)
            {
                if (roleNames[i].Id.Equals(role.RoleId))
                {
                    return roleNames[i].Name;
                }
            }
            return null;
        }

        //POST api/Account/GiveUserAccess
        [System.Web.Http.Route("GiveUserAccess")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult GiveUserAccess(string email)
        {
            ApplicationUser user = UserManager.FindByEmail(email);
            user.LockoutEnabled = false;
            user.LockoutEndDateUtc = null;
            return Ok();
        }

        //POST api/Account/IsAccountEnabled
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("IsAccountEnabled")]
        public IHttpActionResult IsAccountEnabled(string email)
        {
            ApplicationUser user = UserManager.FindByName(email);
            if (user != null)
            {
                if (user.LockoutEnabled)
                {
                    if (user.LockoutEndDateUtc != null)
                    {
                        return Content(HttpStatusCode.BadRequest, "Your account has been blocked.");
                    }
                    return Content(HttpStatusCode.BadRequest, "Your account hasn't been granted access yet.");
                }
            }
            else
            {
                return Content(HttpStatusCode.BadRequest, "The email address isn't registered.");
            }
            return Ok("User has access");
        }

        //GET api/Account/GetUserId
        [System.Web.Http.Route("GetUserId")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetUserId(string email)
        {
            return Ok(_userMgr.ReadUser(email).Id);
        }
        
        //GET api/Account/GetAllAdminInfo
        [System.Web.Http.Route("GetAllAdminInfo")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetAdminInfo()
        {
            AdminInformationModel model = new AdminInformationModel()
            {
                BlockedUsers = new List<User>()
            };
            foreach (var applicationUser in UserManager.Users.Where(u => u.LockoutEnabled).Where(p => p.LockoutEndDateUtc == null))
            {
                model.BlockedUsers.Add(_userMgr.ReadUser(applicationUser.Email));
            }
            return Ok(model);
        }

        //GET api/Account/GetAllUsers
        [System.Web.Http.Route("GetAllUsers")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetAllUsers()
        {
            List<User> users = new List<User>();
            foreach (var applicationUser in UserManager.Users)
            {
                users.Add(_userMgr.ReadUser(applicationUser.Email));
            }
            return Ok(users.GroupBy(x=>x.DateRegistered.Month));
        }

        //GET api/Account/GetAllUsersForAdmin
        [System.Web.Http.Route("GetAllUsersForAdmin")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetAllUsersForAdmin()
        {
            var admin = _userMgr.ReadUser(1);
            IEnumerable<ApplicationUser> users = UserManager.Users.Where(a => !a.Email.Equals(admin.Email)).AsEnumerable();
            return Ok(users.ToList());
        }

        //POST api/Account/AllowUser
        [System.Web.Http.Route("AllowUser")]
        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> AllowUser(string email)
        {
            var user = UserManager.Users.Single(u => u.Email == email);
            await UserManager.SetLockoutEndDateAsync(user.Id, DateTimeOffset.Now);
            await UserManager.SetLockoutEnabledAsync(user.Id, false);
            return Ok(email + " will now have access to Sussol");
        }

        //POST api/Account/AllowUser
        [System.Web.Http.Route("DeleteUser")]
        [System.Web.Http.HttpPost]
        public OkNegotiatedContentResult<string> DeleteUser(string email)
        {
            var userToDelete = _userMgr.ReadUser(email);
            _userMgr.DeleteUser(userToDelete);
            var user = UserManager.Users.Single(u => u.Email == email);
            UserManager.Delete(user);
            return Ok("User is deleted from sussol");
        }

        //POST api/Account/DenyUser
        [System.Web.Http.Route("DenyUser")]
        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> DenyUser(string email)
        {
            
            var user = UserManager.Users.Single(u => u.Email == email);
            await UserManager.SetLockoutEnabledAsync(user.Id, true);
            await UserManager.SetLockoutEndDateAsync(user.Id, DateTimeOffset.MaxValue);
            return Ok(email + " is blocked from Sussol");
        }

        //POST api/Account/BlockUsersForOrganisation
        [System.Web.Http.Route("BlockUsersForOrganisation")]
        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> BlockUsersForOrganisation(long id)
        {
            var users = _userMgr.ReadUsersForOrganisation(id).ToList();
            foreach (var usr in users)
            {
                if (usr.Id != 1)
                {
                    var user = UserManager.Users.Single(u => u.Email.Equals(usr.Email));
                    await UserManager.SetLockoutEnabledAsync(user.Id, true);
                    await UserManager.SetLockoutEndDateAsync(user.Id, DateTimeOffset.MaxValue);

                }
            }
            return Ok();
        }

        //POST api/Account/AllowUsersForOrganisation
        [System.Web.Http.Route("AllowUsersForOrganisation")]
        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> AllowUsersForOrganisation(long id)
        {
            var users = _userMgr.ReadUsersForOrganisation(id).ToList();
            foreach (var usr in users)
            {
                if (usr.Id != 1)
                {
                    var user = UserManager.Users.Single(u => u.Email.Equals(usr.Email));
                    await UserManager.SetLockoutEndDateAsync(user.Id, DateTimeOffset.Now);
                    await UserManager.SetLockoutEnabledAsync(user.Id, false);
                }

            }
            return Ok();
        }


        //GET api/Account/GetUserInfo
        [System.Web.Http.Route("GetUserInfo")]
        [System.Web.Http.HttpGet]
        public IHttpActionResult GetUserInformation(long id)
        {
            User user = _userMgr.ReadUser(id);
            if (user == null)
            {
                return BadRequest("User not found");
            }
            if (User.Identity.Name == user.Email)
            {
                var model = new UserInformationViewModel()
                {
                    Id = user.Id,
                    Firstname = user.Firstname,
                    Lastname = user.Lastname,
                    Picture = null,
                    HasOrganisation = false
                };
                if (user.Organisation != null)
                {
                    model.HasOrganisation = true;
                }
                if (user.AvatarUrl != null)
                {
                    model.Picture = user.AvatarUrl;
                }
                return Ok(model);
            }
            return BadRequest();

        }

        //POST api/Account/ChangeAvatar
        [System.Web.Http.Route("ChangeAvatar")]
        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> ChangeAvatar()
        {
            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            var provider = new MultipartFormDataStreamProvider(root);
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            var email = "";
            MultipartFileData picture = null;
            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);

                email = provider.FormData.Get("email");
                if (provider.FileData.Count != 0)
                {
                    picture = provider.FileData[0];
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            string imagePath = null;
            if (picture != null && picture.LocalFileName.Length > 0)
            {
                imagePath = FileHelper.GetImagePathFromRequest(picture, "UsersImgPath");
            }

            User user = _userMgr.ReadUser(email);

            user.AvatarUrl = imagePath;

            _userMgr.UpdateUser(user);

            return Ok();
        }

        // POST api/Account/Logout
        [System.Web.Http.Route("Logout")]
        [System.Web.Http.HttpPost]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        

        // POST api/Account/ChangePassword
        [System.Web.Http.Route("ChangePassword")]
        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), currentPassword,
                newPassword);
            
            if (!result.Succeeded)
            {
                return Content(HttpStatusCode.BadRequest, "Incorrent password or new password doesn't suffice to the rules." +
                                                          " Password must contain a capital, a number and must consist out atleast 6 characters.");
            }

            return Ok("Your password has been changed.");
        }

        // POST api/Account/SetPassword
        [System.Web.Http.Route("SetPassword")]
        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }
      // GET api/Account/ExternalLogin
      [System.Web.Http.OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.Route("ExternalLogin", Name = "ExternalLogin")]
        [System.Web.Http.HttpGet]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            if (error != null)
            {
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                   OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                Authentication.SignIn(identity);
            }

            return Ok();
        }

        // POST api/Account/Register
        [System.Web.Http.AllowAnonymous]
        [System.Web.Http.Route("Register")]
        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> Register()
        {
            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            var provider = new MultipartFormDataStreamProvider(root);
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            var firstname = "";
            var lastname = "";
            var email= "";
            var password = "";
            MultipartFileData picture = null;
            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);

                firstname = provider.FormData.Get("firstname");
                lastname = provider.FormData.Get("lastname");
                email = provider.FormData.Get("email");
                password = provider.FormData.Get("password");
                if (provider.FileData.Count != 0)
                {
                    picture = provider.FileData[0];
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            string imagePath = null;
            if (picture != null && picture.LocalFileName.Length > 0)
            {
                imagePath = FileHelper.GetImagePathFromRequest(picture, "UsersImgPath");
            }

            User user = _userMgr.ReadUser(email);
            if (user != null)
            {
                return BadRequest("Email address already in use");
            }
  

                var applicationUser = new ApplicationUser() { UserName = email, Email = email, LockoutEnabled = true};
                user = _userMgr.CreateUser(firstname, lastname, email, imagePath);
                IdentityResult result = await UserManager.CreateAsync(applicationUser, password);
                if (result.Succeeded)
                {
                    UserManager.AddToRole(applicationUser.Id, "User");
                    Authentication.SignIn();
                return Ok("Registration was successful. Your application has been sent to an administrator. " +
                              "When you're accepted you can login.");
                }
                _userMgr.DeleteUser(user);
            return BadRequest("Password must contain a capital, a number and must consist out atleast 6 characters");
        }

        // POST api/Account/RegisterExternal
        [System.Web.Http.OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [System.Web.Http.Route("RegisterExternal")]
        [System.Web.Http.HttpPost]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var info = await Authentication.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return InternalServerError();
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            result = await UserManager.AddLoginAsync(user.Id, info.Login);
            if (!result.Succeeded)
            {
                return GetErrorResult(result); 
            }
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion
    }
}
