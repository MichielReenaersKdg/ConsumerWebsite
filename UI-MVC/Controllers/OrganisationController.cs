using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using SS.BL.Analyses;
using SS.BL.Domain.Analyses;
using SS.BL.Domain.Users;
using SS.BL.Users;
using SS.DAL.EFUsers;
using SS.UI.Web.MVC.Controllers.Utils;
using SS.UI.Web.MVC.Models;


namespace SS.UI.Web.MVC.Controllers
{
    [Authorize]
    [RoutePrefix("api/Organisation")]
    public class OrganisationController : ApiController
    {
        private readonly IUserManager _userManager;
        private readonly IAnalysisManager _analysisManager;

        public OrganisationController(IUserManager userManager, IAnalysisManager analysisManager)
        {
            this._userManager = userManager;
            this._analysisManager = analysisManager;
        }
        


        //GET api/Organisation/GetAllOrganisations
        [Route("GetAllOrganisations")]
        [HttpGet]
        public IHttpActionResult GetAllOrganisations()
        {
            var organisations = _userManager.ReadAllOrganisations();
            List<OrganisationViewModel> models = new List<OrganisationViewModel>();
            foreach (var organisation in organisations)
            {
                var user = _userManager.ReadUser(organisation.OrganisatorId);
                models.Add(new OrganisationViewModel() {Organisation = organisation, Organisator = user});
        }
            return Ok(models);
        }


        //POST api/Organisation/CreateOrganisation
        [Route("CreateOrganisation")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateOrganisation()
        {
            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            var provider = new MultipartFormDataStreamProvider(root);
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            var name = "";
            var email = "";
            MultipartFileData picture = null;
            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);
                name = provider.FormData.Get("name");
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


            User user = _userManager.ReadUser(email);
         Organisation organisation = _userManager.ReadOrganisationByName(name);
            if (user.Organisation != null)
            {
                return BadRequest("You already have an organisation");

            }
            if (organisation != null)
         {
            return BadRequest("Organisation already exists");
         }
            string imagePath = null;
            if (picture != null && picture.LocalFileName.Length > 0)
            {
                imagePath = FileHelper.GetImagePathFromRequest(picture, "OrganisationsImgPath");
            }
                


            Organisation org = _userManager.CreateOrganisation(name, imagePath, user);
            return Ok(org.Id);
        }
        
        //GET api/Organisation/GetActivityPerUser
        [Route("GetActivityPerUser")]
        [HttpGet]
        public IHttpActionResult GetActivityPerUser(long id)
        {
            var mostActiveList = new List<MostActiveModel>();
            var analyses = _analysisManager.ReadAnalysesForOrganisation(id);
            foreach (var analysis in analyses)
            {
                MostActiveModel mtemp = new MostActiveModel()
                {
                    User = analysis.CreatedBy,
                    NumberOfUserAnalyses = 1
                };
                if (mostActiveList.TrueForAll(p=>p.User!=mtemp.User))
                {
                    mostActiveList.Add(mtemp);
                }
                else
                {
                    mostActiveList[mostActiveList.FindIndex(p => p.User == mtemp.User)].NumberOfUserAnalyses++;
                    
                }
            }
            return Ok(mostActiveList);
        }

        //GET api/Organisation/ReadOrganisationForUser
        [Route("ReadOrganisationForUser")]
        [HttpGet]
        public IHttpActionResult ReadOrganisationForUser([FromUri] long id)
        {
            var user = _userManager.ReadUser(id);
            if (user != null)
            {
                if (user.Organisation != null && !user.Organisation.Blocked)
                {
                    return Ok(user.Organisation);
                }
                return Ok();
            }
            
            return BadRequest("user not found");
        }

        //GET api/Organisation/ReadOrganisation
        [Route("ReadOrganisation")]
        [HttpGet]
        public IHttpActionResult ReadOrganisation(long id)
        {
            var organisation = _userManager.ReadOrganisation(id);
            if (organisation != null && !organisation.Blocked)
            {
                return Ok(organisation);
            }
            return BadRequest("Organisation not found");
        }

        //GET api/Organisation/GetAnalysesForOrganisation
        [Route("GetAnalysesForOrganisation")]
        [HttpGet]
        public IHttpActionResult GetAnalysesForOrganisation(long id)
        {
            return Ok(_analysisManager.ReadAnalysesForOrganisation(id).ToList());
        }

        //GET api/Organisation/GetUsersForOrganisation
        [Route("GetUsersForOrganisation")]
        [HttpGet]
        public IHttpActionResult GetUsersForOrganisation(long id)
        {
            return Ok(_userManager.ReadUsersForOrganisation(id).ToList());
        }

        //POST api/Organisation/AddMemberToOrganisation
        [Route("AddMemberToOrganisation")]
        [HttpPost]
        public IHttpActionResult AddMemberToOrganisation(long organisationId, string email)
        {

            var membersOrganisation = _userManager.ReadUsersForOrganisation(organisationId).ToList();
            if (membersOrganisation.Count() >= 20)
            {
                return BadRequest("You can have a max of 20 members in one organisation");
            }
            var memberToAdd = _userManager.ReadUser(email);
            if (memberToAdd == null)
            {
                return BadRequest("email wasn't found!");
            }
            if (memberToAdd.Organisation != null)
            {
                return BadRequest("Member is already in an organisation");
            }

            if (membersOrganisation.Contains(memberToAdd))
            {
                return BadRequest("Member already in organisation");
            }
            
            _userManager.JoinOrganisation(email, organisationId);
            return Ok("Member has been added to organisation");
        }

        //POST api/Organisation/LeaveOrganisation
        [Route("LeaveOrganisation")]
        [HttpPost]
        public IHttpActionResult LeaveOrganisation(long userId)
        {
            _userManager.LeaveOrganisation(userId);
            return Ok();
        }

        //POST api/Organisation/BlockOrganisation
        [Route("BlockOrganisation")]
        [HttpPost]
        public IHttpActionResult BlockOrganisation(long id)
        {
            _userManager.BlockOrganisation(id);
            return Ok();
        }

        //POST api/Organisation/AllowOrganisation
        [Route("AllowOrganisation")]
        [HttpPost]
        public IHttpActionResult AllowOrganisation(long id)
        {
            _userManager.AllowOrganisation(id);
            return Ok();
        }

        //DELETE api/Organisation/DeleteOrganisation
        [Route("DeleteOrganisation")]
        [HttpDelete]
        public IHttpActionResult DeleteOrganisation(long id)
        {
            _userManager.DeleteOrganisation(id);
            return Ok();
        }



        //GET api/Organisation/GetAnalysesByMonthForOrganisation
        [Route("GetAnalysesByMonthForOrganisation")]
        [HttpGet]
        public IHttpActionResult GetAnalyses(long id)
        {
            return Ok(_analysisManager.ReadAnalysesForOrganisation(id).GroupBy(x => x.DateCreated.Month));
        }

        //POST api/Organisation/CheckPermission
        [Route("CheckPermission")]
        [HttpPost]
        public IHttpActionResult CheckPermission(long userId, long organisationId)
        {
            var user = _userManager.ReadUser(userId);
            var organisation = _userManager.ReadOrganisation(organisationId);
            if (organisation == null || user.Organisation == null)
            {
                return BadRequest("Organisation not found");
            }
            if (user.Organisation.Blocked)
            {
                return BadRequest("Organisation has been blocked");
            }
            if (user.Organisation.Id == organisationId)
            {
                return Ok();
            }
            return BadRequest("Access not granted");
        }

        //GET api/Organisation/ReadOrganiser
        [Route("ReadOrganiser")]
        [HttpGet]
        public IHttpActionResult ReadOrganiser(long id)
        {
            if (_userManager.ReadOrganisation(id) != null)
            {
                return Ok(_userManager.ReadOrganiser(id));
            }
            return BadRequest("organisation not found");
        }

        //POST api/Organisation/ChangeLogo
        [Route("ChangeLogo")]
        [HttpPost]
        public async Task<IHttpActionResult> ChangeLogo()
        {
            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            var provider = new MultipartFormDataStreamProvider(root);
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }
            var id = 0;
            MultipartFileData picture = null;
            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);

                id = Int32.Parse(provider.FormData.Get("id"));
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
                imagePath = FileHelper.GetImagePathFromRequest(picture, "OrganisationsImgPath");
            }

            Organisation organisation = _userManager.ReadOrganisation(id);

            organisation.LogoUrl = imagePath;

            _userManager.UpdateOrganisation(organisation);

            return Ok();
        }
    }


}