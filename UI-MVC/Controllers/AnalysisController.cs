﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using SS.BL.Analyses;
using SS.BL.Domain.Analyses;
using SS.BL.Domain.Users;
using SS.BL.Users;
using SS.UI.Web.MVC.Controllers.Utils;
using SS.UI.Web.MVC.Models;

namespace SS.UI.Web.MVC.Controllers
{
    [Authorize]
    [RoutePrefix("api/Analysis")]
    public class AnalysisController : ApiController
    {
        private readonly IAnalysisManager _analysisManager;
        private readonly IUserManager _userManager;
        private readonly List<String> _csvLocations; 
        public AnalysisController(IAnalysisManager analysisManager, IUserManager userManager)
        {
            this._analysisManager = analysisManager;
            this._userManager = userManager;

            _csvLocations = Directory.EnumerateFiles(HttpContext.Current.Server.MapPath("~/Content/Csv/")).ToList();

        }

        //GET api/Analysis/GetAnalysis
        [Route("GetAnalysis")]
        public Analysis GetAnalysis([FromUri]long id)
        {
            return _analysisManager.ReadAnalysis(id);
        }

        //GET api/Analysis/GetAnalysesForUser
        [Route("GetAnalysesForUser")]
        public List<Analysis> GetAnalysesForUser([FromUri] long id)
        {
            var user = _userManager.ReadUser(id);
            return _analysisManager.ReadAnalysesForUser(user).ToList();
        } 

        //GET api/Analysis/GetAnalysesForOrganisation
        [Route("GetAnalysesForOrganisation")]
        public List<Analysis> GetAnalysesForOrganisation(long id)
        {
            return _analysisManager.ReadAnalysesForOrganisation(id).ToList();
        }

        //GET api/Analysis/GetAnalysesByMonth
        [Route("GetAnalysesByMonth")]
        public IEnumerable<IGrouping<int, Analysis>> GetAnalyses()
        {
            return _analysisManager.ReadAnalyses().GroupBy(x => x.DateCreated.Month);
        }
        
        //GET api/Analysis/GetAnalysesDivision
        [Route("GetAnalysesDivision")]
        public List<Analysis> GetAnalysesDivision()
        {
            var analyses = _analysisManager.ReadFullAnalyses();
            return analyses.ToList();
        }

        //POST api/Analysis/ChangeName
        [Route("ChangeName")]
        public IHttpActionResult ChangeName([FromUri] string name, [FromUri] long analysisId)
        {
            if (_analysisManager.ReadAnalysis(name) != null)
            {
                return BadRequest("Name is already in use!");
            }
            var analysis = _analysisManager.ReadAnalysis(analysisId);
            analysis.Name = name;
            _analysisManager.UpdateAnalysis(analysis);
            return Ok("Name has been changed");
        }

        //GET api/Analysis/GetSolvents
        [Route("GetSolvents")]
        public List<Solvent> GetSolvents(long id)
        {
            List<Solvent> solvents = new List<Solvent>();
            var analysis = _analysisManager.ReadAnalysis(id);
            foreach (var cluster in analysis.AnalysisModels[0].Model.Clusters)
            {
                foreach (var solvent in cluster.Solvents)
                {
                    solvents.Add(solvent);
                }
            }
            return solvents;
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

        //POST api/Analysis/Createanalysis
        [Route("CreateAnalysis")]
        public IHttpActionResult CreateAnalysis([FromUri] List<string> algorithms, [FromUri] string dataSet, [FromUri] string name)
        {
            if (_analysisManager.ReadAnalysis(name) != null)
            {
                return BadRequest("Name already in use!");
            }
            List<Model> models = GetFullModels(algorithms, dataSet);
            Analysis analysis = new Analysis()
            {
                Name = name,
                DateCreated = DateTime.Now,
                AnalysisModels = new List<AnalysisModel>(),
                NumberOfSolvents = models[0].NumberOfSolvents
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
            return Ok(analysis);
        }

        //GET api/Analysis/SetStringsToAlgorithmNames
        [Route("SetStringsToAlgorithmNames")]
        public List<AlgorithmName> SetStringsToAlgorithmNames(List<string> algorithms)
        {
            List<AlgorithmName> algorithmNames = new List<AlgorithmName>();
            foreach (String algorithm in algorithms)
            {
                switch (algorithm.ToUpper())
                {
                    case "CANOPY":
                        algorithmNames.Add(AlgorithmName.CANOPY);
                        break;
                    case "KMEANS":
                        algorithmNames.Add(AlgorithmName.KMEANS);
                        break;
                    case "XMEANS":
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
                    await CreateModels(algorithm);
                                
                }
                models.AddRange(_analysisManager.ReadModelsForAlgorithm(algorithm)); 
            }
            return models.GroupBy(x => x.DataSet).Select(y => y.First()).ToList();
        }

        //POST api/Analysis/CreateModel
        [AllowAnonymous]
        [Route("CreateModel")]
        public async Task<IHttpActionResult> CreateModels(AlgorithmName algorithmName)
        {
            var minMaxValues = _analysisManager.ReadMinMaxValues();
            try
            {
                using (var client = new WebClient())
                {
                    foreach (var csvLocation in _csvLocations)
                    {
                        var response = client.UploadFile(new Uri("http://api-sussolkdg.rhcloud.com/api/model/" + algorithmName.ToString().ToLower()),
                        csvLocation);
                        //creatie van model binnen algoritme
                        var jsonResponse = Encoding.Default.GetString(response);
                        var algorithm = JsonHelper.ParseJson(jsonResponse, minMaxValues.ToList());
                        _analysisManager.CreateAlgorithm(algorithm);
                    }
                    client.Dispose();
                    return Ok();
                }
            }
            catch (Exception e)
            {
                return BadRequest("An error occurred while generating the model.");
            }
        }
        
        //POST api/Analysis/ShareWithOrganisation
        [Route("ShareWithOrganisation")]
        public IHttpActionResult ShareWithOrganisation(long organisationId, long analysisId)
        {
            var analysis = _analysisManager.ShareWithOrganisation(organisationId, analysisId);
            return Ok(analysis);
        }

        //POST api/Analysis/CheckPermission
        [Route("CheckPermission")]
        public IHttpActionResult CheckPermission(long userId, long analysisId)
        {
            var analyses = _analysisManager.ReadAnalysesForUserPermission(userId);
            var analysis = _analysisManager.ReadAnalysis(analysisId);
            if (analyses.Contains(analysis))
            {
                return Ok();
            }
            return BadRequest("Access not granted");
        }

        //POST api/Analysis/UndoShare
        [Route("UndoShare")]
        public IHttpActionResult UndoShare(long id)
        {
            var analysis = _analysisManager.UndoShare(id);
            return Ok(analysis);
        }


        //GET api/Analysis/ReadMinMaxValues
        [Route("ReadMinMaxValues")]
        public List<MinMaxValue> ReadMinMaxValues(long analysisId)
        {
            return _analysisManager.ReadMinMaxValues(analysisId).ToList();
        }

        //GET api/Analysis/ReadClassifiedInstances
        [Route("ReadClassifiedInstances")]
        public List<ClassifiedInstance> ReadClassifiedInstances(long userId, long analysisId)
        {
            return _analysisManager.ReadClassifiedInstancesForUser(userId, analysisId).ToList();
        }

        //POST api/Analysis/ClassifyNewSolvent
        [Route("ClassifyNewSolvent")]
        public IHttpActionResult ClassifyNewSolvent([FromBody]ClassifySolventModel model, [FromUri] long analysisId)
        {
            var instances = _analysisManager.ReadClassifiedInstancesForUser(model.UserId, analysisId).ToList();
            foreach (var cluster in model.AnalysisModels[0].Model.Clusters)
            {
                if (cluster.Solvents.FirstOrDefault(a => a.CasNumber.Equals(model.CasNumber)) != null)
                {
                    return BadRequest("Solvent is already in training set!");
                }
            }

            if (instances.FirstOrDefault(a => a.Name.Equals(model.Name)) != null)
            {
                return BadRequest("Name already in use!");
            }
            if (instances.FirstOrDefault(a => a.CasNumber == model.CasNumber) != null)
            {
                return BadRequest("Cas number already in use!");
            }

            using (var client = new WebClient())
            {
                List<AnalysisModel> analysisModels = new List<AnalysisModel>();
                foreach (var analysisModel in model.AnalysisModels)
                {
                    var serialized = JsonConvert.SerializeObject(model.Values);
                    String parameters = "path="+analysisModel.Model.ModelPath+"&featureValues=" + serialized;
                    client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    var classifiedInstance = JsonHelper.ParseJsonToClassifiedInstance(client.UploadString(new Uri("http://api-sussolkdg.rhcloud.com/api/classify"), parameters));
                    classifiedInstance.CasNumber = model.CasNumber;
                    classifiedInstance.Name = model.Name;
                    classifiedInstance.Features = new List<Feature>();
                    
                    for (int i = 0; i < model.FeatureNames.Length; i++)
                    {
                        model.FeatureNames[i] =
                            model.FeatureNames[i].Replace("°", "Degrees").Replace('.', '_').Replace('/', '_');
                        Feature f = new Feature()
                        {
                            FeatureName = (FeatureName)Enum.Parse(typeof(FeatureName), model.FeatureNames[i]),
                            Value = model.Values[i]
                        };
                        classifiedInstance.Features.Add(f);
                    }
                    analysisModels.Add(_analysisManager.CreateClassifiedInstance(analysisModel.Id,model.UserId, classifiedInstance));
                    
                }
                client.Dispose();
                return Ok(analysisModels);
            }
        }

        //POST api/Analysis/SetClassifiedSolvent
        [Route("SetClassifiedSolvent")]
        public List<AnalysisModel> SetClassifiedSolvent(string name, long analysisId)
        {
            var analysis = _analysisManager.ReadAnalysis(analysisId);
            var classifiedInstances = _analysisManager.ReadAllClassifiedInstances(analysis.CreatedBy.Id, name).ToList();
            foreach (var model in analysis.AnalysisModels)
            {
                var instance = classifiedInstances.FirstOrDefault(a => a.AnalysisModelId == model.Id);
                if (instance != null)
                {
                    _analysisManager.SetClassifiedSolvent(model.Id, instance.Id);
                }
                else
                {
                    ArrayList values = new ArrayList();
                    var featureNames = new ArrayList();
                    foreach (var feature in classifiedInstances.First().Features)
                    {
                        values.Add(feature.Value);
                        featureNames.Add(feature.FeatureName);
                    }
                    using (var client = new WebClient())
                    {
                        
                            var serialized = JsonConvert.SerializeObject(values);
                            String parameters = "path=" + model.Model.ModelPath + "&featureValues=" + serialized;
                            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                            var classifiedInstance = JsonHelper.ParseJsonToClassifiedInstance(client.UploadString(new Uri("http://api-sussolkdg.rhcloud.com/api/classify"), parameters));
                            classifiedInstance.CasNumber = classifiedInstances.First().CasNumber;
                            classifiedInstance.Name = classifiedInstances.First().Name;
                            classifiedInstance.Features = new List<Feature>();

                            for (int i = 0; i < featureNames.Count; i++)
                            {
                            featureNames[i] =
                            featureNames[i].ToString().Replace("°", "Degrees").Replace('.', '_').Replace('/', '_');
                            Feature f = new Feature()
                                {
                                    FeatureName = (FeatureName)Enum.Parse(typeof(FeatureName), featureNames[i].ToString()),
                                    Value = Double.Parse(values[i].ToString())
                                };
                                classifiedInstance.Features.Add(f);
                            }
                            _analysisManager.CreateClassifiedInstance(model.Id, analysis.CreatedBy.Id, classifiedInstance);
                        client.Dispose();
                    }
                }
                
            }
            return _analysisManager.ReadAnalysis(analysisId).AnalysisModels;
        } 
    }
}
