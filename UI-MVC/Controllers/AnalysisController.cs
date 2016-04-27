﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.ModelBinding;
using Newtonsoft.Json;
using SS.BL.Analyses;
using SS.BL.Domain.Analyses;
using SS.BL.Domain.Users;
using SS.BL.Users;
using SS.UI.Web.MVC.Controllers.Utils;

namespace SS.UI.Web.MVC.Controllers
{
    [Authorize]
    [RoutePrefix("api/Analysis")]
    public class AnalysisController : ApiController
    {
        private readonly AnalysisManager _analysisManager = new AnalysisManager();
        private readonly UserManager _userManager = new UserManager();

        public AnalysisController()
        {
        }

        //GET api/Analysis/GetAnalysis
        [Route("GetAnalysis")]
        public Analysis GetAnalysis([FromUri]long id)
        {
            return _analysisManager.ReadAnalysis(id);
        }

        //GET api/Analysis/GetAnalysesForUser
        [Route("GetAnalysesForUser")]
        public List<Analysis> GetAnalysesForUser([FromUri] string email)
        {
            var user = _userManager.ReadUser(email);
            return _analysisManager.ReadAnalysesForUser(user).ToList();
        } 

        //GET api/Analysis/GetAnalysesForOrganisation
        [Route("GetAnalysesForOrganisation")]
        public List<Analysis> GetAnalysesForOrganisation(long id)
        {
            var organisation = _userManager.ReadOrganisation(id);
            return _analysisManager.ReadAnalysesForOrganisation(organisation).ToList();
        } 

        //GET api/Analysis/GetFullModels
        [Route("GetFullModels")]
        public List<Model> GetFullModels(List<string> algorithms, string dataSet)
        {
            List<AlgorithmName> algorithmNames = SetStringsToAlgorithmNames(algorithms);
            List<Model> models = new List<Model>();
            foreach (AlgorithmName name in algorithmNames)
            {
                models.Add(_analysisManager.ReadModel(dataSet, name));
            }
            return models;
        }

        //GET api/Analysis/Createanalysis
        [Route("CreateAnalysis")]
        public Analysis CreateAnalysis([FromUri] List<string> algorithms, [FromUri] string dataSet)
        {
            List<Model> models = GetFullModels(algorithms, dataSet);
            Analysis analysis = new Analysis()
            {
                Name = models.First().DataSet,
                DateCreated = DateTime.Now,
                AnalysisModels = new List<AnalysisModel>()
            };
            foreach (Model m in models)
            {
                AnalysisModel analysisModel = new AnalysisModel()
                {
                   Model = m
                };
                analysis.AnalysisModels.Add(analysisModel);
                
            }
            analysis = _analysisManager.CreateAnalysis(analysis, User.Identity.Name);
            return analysis;
        }

        //GET api/Analysis/SetStringsToAlgorithmNames
        [Route("SetStringsToAlgorithmNames")]
        public List<AlgorithmName> SetStringsToAlgorithmNames(List<string> algorithms)
        {
            List<AlgorithmName> algorithmNames = new List<AlgorithmName>();
            foreach (String algorithm in algorithms)
            {
                switch (algorithm)
                {
                    case "Cobweb":
                        algorithmNames.Add(AlgorithmName.COBWEB);
                        break;
                    case "Canopy":
                        algorithmNames.Add(AlgorithmName.CANOPY);
                        break;
                    case "KMeans":
                        algorithmNames.Add(AlgorithmName.KMEANS);
                        break;
                    case "XMeans":
                        algorithmNames.Add(AlgorithmName.XMEANS);
                        break;
                    case "EM":
                        algorithmNames.Add(AlgorithmName.EM);
                        break;
                    case "SOM":
                        algorithmNames.Add(AlgorithmName.SOM);
                        break;
                }
            }
            return algorithmNames;
        }

        //GET api/Analysis/StartAnalysis
        [Route("StartAnalysis")]
        public async Task<List<Model>> StartAnalysis([FromUri] List<string> algorithms )
        {
            List<AlgorithmName> algorithmNames = SetStringsToAlgorithmNames(algorithms);
            List<Model> models = new List<Model>();
            foreach (AlgorithmName algorithm in algorithmNames)
            {
                var modelsTemp = _analysisManager.ReadModelsForAlgorithm(algorithm);
                if (modelsTemp.Count == 0)
                {
                    await CreateModel(algorithm);
                                
                }
                models.AddRange(_analysisManager.ReadModelsForAlgorithm(algorithm)); 
            }
            return models;
        }

        //POST api/Analysis/CreateModel
        [AllowAnonymous]
        [Route("CreateModel")]
        public async Task<IHttpActionResult> CreateModel(AlgorithmName algorithmName)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var response = client.UploadFile(new Uri("http://api-sussolkdg.rhcloud.com/api/model/" + algorithmName.ToString().ToLower()),
                        HttpContext.Current.Server.MapPath("~/Content/Csv/defaultmatrix.csv"));
                    //creatie van model binnen algoritme
                    var jsonResponse = Encoding.Default.GetString(response);
                    var algorithm = JsonHelper.ParseJson(jsonResponse);
                    _analysisManager.CreateAlgorithm(algorithm);
                    client.Dispose();
                    return Ok();
                }
            }
            catch (Exception e)
            {
                return BadRequest("An error occurred while generating the model.");
            }
        }  
    }
}
