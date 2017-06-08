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
using Newtonsoft.Json.Linq;


namespace SS.UI.Web.MVC.Controllers
{
    [Authorize]
    [RoutePrefix("api/Analysis")]
    public class AnalysisController : ApiController
    {
        
        private readonly IAnalysisManager _analysisManager;
        private readonly IUserManager _userManager;
      //private readonly List<string> _datasets; 
      public AnalysisController(IAnalysisManager analysisManager, IUserManager userManager)
        {
            this._analysisManager = analysisManager;
            this._userManager = userManager;

           // _datasets.Add(Properties.Resources.datasetqframe);

        }

        //GET api/Analysis/GetAnalysis
        [Route("GetAnalysis")]
        [HttpGet]
        public IHttpActionResult GetAnalysis([FromUri]long id)
        {
            return Ok(_analysisManager.ReadAnalysis(id));
        }

        //GET api/Analysis/GetAnalysesForUser
        [Route("GetAnalysesForUser")]
        [HttpGet]
        public IHttpActionResult GetAnalysesForUser([FromUri] long id)
        {
            var user = _userManager.ReadUser(id);
            return Ok(_analysisManager.ReadAnalysesForUser(user).ToList());
        } 

        //GET api/Analysis/GetAnalysesForOrganisation
        [Route("GetAnalysesForOrganisation")]
        [HttpGet]
        public IHttpActionResult GetAnalysesForOrganisation(long id)
        {
            return Ok(_analysisManager.ReadAnalysesForOrganisation(id).ToList());
        }
      //GET api/Analysis/GetAnalysesByMonth
      [Route("GetAnalysesByMonth")]
        [HttpGet]
        public IHttpActionResult GetAnalyses()
        {
            return Ok(_analysisManager.ReadAnalyses().GroupBy(x => x.DateCreated.Month));
        }
        
        //GET api/Analysis/GetAnalysesDivision
        [Route("GetAnalysesDivision")]
        [HttpGet]
        public IHttpActionResult GetAnalysesDivision()
        {
            var analyses = _analysisManager.ReadFullAnalyses();
            return Ok(analyses.ToList());
        }

        //POST api/Analysis/ChangeName
        [Route("ChangeName")]
        [HttpPost]
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
        [HttpGet]
        public IHttpActionResult GetSolvents(long id)
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
            return Ok(solvents);
        }

        //GET api/Analysis/GetFullModels
        [Route("GetFullModels")]
        [HttpGet]
        public List<Model> GetFullModels(List<string> algorithms, int trainingsFileID)
        {
            List<AlgorithmName> algorithmNames = SetStringsToAlgorithmNames(algorithms);
            List<Model> models = new List<Model>();
            foreach (AlgorithmName name in algorithmNames)
            {
                models.Add(_analysisManager.ReadModel(trainingsFileID, name));
            }
            return models;
        }

        //GET api/Analysis/GetSolvents
        [Route("GetSolvents")]
        [HttpGet]
        public List<Solvent> getSolvents()
        {
            return _analysisManager.ReadSolvents().ToList();
        }

        //GET api/Analysis/GetFeatures
        [Route("GetFeatures")]
        [HttpGet]
        public List<Feature> getFeatures()
        {
            return _analysisManager.ReadFeatures().ToList();
        }

      [Route("AddTrainingSet")]
      [HttpPost]
      public IHttpActionResult AddTrainingSet([FromBody]TrainingSet trainingSet)
      {
         TrainingSet set = _analysisManager.createNewModelsFromTrainingsfile(trainingSet);
         return Ok(set);
      }

      [Route("GetTrainingSets")]
      [HttpGet]
      public List<TrainingSet> GetTrainingSets()
      {
         List<TrainingSet> sets = _analysisManager.ReadTrainingSets().ToList();
         return sets;
      }
      //DELETE api/Analysis/DeleteTrainingSet
      [Route("DeleteTrainingSet")]
      [HttpDelete]
      public IHttpActionResult DeleteTrainingSet([FromBody] TrainingSet trainingSet)
      {
        _analysisManager.RemoveTrainingSet(trainingSet);
         return Ok("TrainingSet deleted");
      }


      //POST api/Analysis/Createanalysis
      [Route("CreateAnalysis")]
        [HttpPost]
        public IHttpActionResult CreateAnalysis([FromUri] List<string> algorithms, [FromUri] string trainingsFileID, [FromUri] string name)
        {
            if (_analysisManager.ReadAnalysis(name) != null)
            {
                return BadRequest("Name already in use!");
            }
            List<Model> models = GetFullModels(algorithms, Int32.Parse(trainingsFileID));
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

        //GET api/Analysis/GetAlgorithmNames
        [Route("GetAlgorithmNames")]
        [HttpGet]
        public List<string> getAlgorithmNames()
        {
         AlgorithmName[] alnames = (AlgorithmName[])Enum.GetValues(typeof(AlgorithmName));
         List<string> result = new List<string>();
         foreach(AlgorithmName name in alnames)
         {
            result.Add(name.ToString());
         }
         return result;
        }

        //GET api/Analysis/SetStringsToAlgorithmNames
        [Route("SetStringsToAlgorithmNames")]
        [HttpGet]
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
                    case "XMEANS":
                        algorithmNames.Add(AlgorithmName.XMEANS);
                        break;
                    case "SOM":
                        algorithmNames.Add(AlgorithmName.SOM);
                        break;
                }
            }
            return algorithmNames;
        }

        //POST api/Analysis/StartAnalysis
        [Route("StartAnalysis")]
        [HttpPost]
        public async Task<IHttpActionResult> StartAnalysis([FromUri] List<string> algorithms )
        {
            List<AlgorithmName> algorithmNames = SetStringsToAlgorithmNames(algorithms);
            List<Model> models = new List<Model>();
            foreach (AlgorithmName algorithm in algorithmNames)
            {
                //var modelsTemp = _analysisManager.ReadModelsForAlgorithm(algorithm); ;
                
                models.AddRange(_analysisManager.ReadModelsForAlgorithm(algorithm)); 
            }
            return Ok(models.GroupBy(x => x.DataSet).Select(y => y.First()).ToList());
        }

        ////POST api/Analysis/CreateModel
        //[AllowAnonymous]
        //[Route("CreateModel")]
        //[HttpPost]
        //public async Task<IHttpActionResult> CreateModels(AlgorithmName algorithmName)
        //{
        //    //0.4.9 var minMaxValues = _analysisManager.ReadMinMaxValues();
        //    try
        //    {
        //        using (var client = new WebClient())
        //        {
        //            FillAlgorithms(algorithmName);
        //            client.Dispose();
        //            return Ok();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        return BadRequest("An error occurred while generating the model.");
        //    }
        //}
        
        //POST api/Analysis/ShareWithOrganisation
        [Route("ShareWithOrganisation")]
        [HttpPost]
        public IHttpActionResult ShareWithOrganisation(long organisationId, long analysisId)
        {
            var analysis = _analysisManager.ShareWithOrganisation(organisationId, analysisId);
            return Ok(analysis);
        }

        //POST api/Analysis/CheckPermission
        [Route("CheckPermission")]
        [HttpPost]
        public IHttpActionResult CheckPermission(long userId, long analysisId)
        {
            var analyses = _analysisManager.ReadAnalysesForUserPermission(userId);
            var analysis = _analysisManager.ReadAnalysis(analysisId);
            if (analysis == null)
            {
                return BadRequest("Analysis not found");
            }
            if (analyses.Contains(analysis))
            {
                return Ok();
            }
            return BadRequest("Access not granted");
        }

        //POST api/Analysis/UndoShare
        [Route("UndoShare")]
        [HttpPost]
        public IHttpActionResult UndoShare(long id)
        {
            var analysis = _analysisManager.UndoShare(id);
            return Ok(analysis);
        }




        //DELETE api/Analysis/Delete
        [Route("Delete/{id:int}")]
        [HttpPost]
        public IHttpActionResult Delete(int id)
        {
            _analysisManager.DeleteAnalysis(id);

            return Ok("Analysis deleted");
        }

        //GET api/Analysis/ReadClassifiedInstances
        [Route("ReadClassifiedInstances")]
        [HttpGet]
        public IHttpActionResult ReadClassifiedInstances(long userId, long analysisId)
        {
            return Ok(_analysisManager.ReadClassifiedInstancesForUser(userId, analysisId).ToList());
        }

        //POST api/Analysis/ClassifyNewSolvent
        [Route("ClassifyNewSolvent")]
        [HttpPost]
        public IHttpActionResult ClassifyNewSolvent([FromBody]ClassifySolventModel model, [FromUri] long analysisId)
        {
         //Turned off validation
         //var instances = _analysisManager.ReadClassifiedInstancesForUser(model.UserId, analysisId).ToList();
         //Turned off validation   
         //foreach (var cluster in model.AnalysisModels[0].Model.Clusters)
            //{
            //    if (cluster.Solvents.FirstOrDefault(a => a.CasNumber.Equals(model.CasNumber)) != null)
            //    {
            //        return BadRequest("Cas number is already in use!");
            //    }
            //    if (cluster.Solvents.FirstOrDefault(a => a.Name.Equals(model.Name)) != null)
            //    {
            //        return BadRequest("Name is already in use!");
            //    }
            //}

            //if (instances.FirstOrDefault(a => a.Name.Equals(model.Name)) != null)
            //{
            //    return BadRequest("You used this name already for a classified solvent!");
            //}
            //if (instances.FirstOrDefault(a => a.CasNumber == model.CasNumber) != null)
            //{
            //    return BadRequest("You used this cas nr already for a classified solvent!");
            //}

            
                List<AnalysisModel> analysisModels = new List<AnalysisModel>();
                foreach (var analysisModel in model.AnalysisModels)
                {
                    var serialized = JsonConvert.SerializeObject(model.Values);

                    ClassifiedInstance classifiedInstance = _analysisManager.ClassifyNewSolvent(analysisModel.Model.ModelPath, serialized);
                    classifiedInstance.CasNumber = model.CasNumber;
                    classifiedInstance.Name = model.Name;
                    classifiedInstance.Features = new List<Feature>();

                    for (int i = 0; i < model.FeatureNames.Length; i++)
                    {
                        model.FeatureNames[i] = model.FeatureNames[i].Replace("°", "Degrees").Replace('.', '_').Replace('/', '_');
                        Feature f = new Feature()
                        {
                            featureName = model.FeatureNames[i].ToString(),
                            value = model.Values[i]
                            
                        };
                        classifiedInstance.Features.Add(f);
                    }
                    analysisModels.Add(_analysisManager.CreateClassifiedInstance(analysisModel.Id,model.UserId, classifiedInstance));
          
                  }
         return Ok(analysisModels);
      }

        //POST api/Analysis/SetClassifiedSolvent
        [Route("SetClassifiedSolvent")]
        [HttpPost]
        public IHttpActionResult SetClassifiedSolvent(string name, long analysisId, long userId)
        {
            var analysis = _analysisManager.ReadAnalysis(analysisId);
            var classifiedInstances = _analysisManager.ReadAllClassifiedInstances(userId, name).ToList();
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
                        values.Add(feature.value);
                        featureNames.Add(feature.featureName);
                    }
               var serialized = JsonConvert.SerializeObject(values);


               ClassifiedInstance classifiedInstance = _analysisManager.ClassifyNewSolvent(model.Model.ModelPath, serialized);
                            classifiedInstance.CasNumber = classifiedInstances.First().CasNumber;
                            classifiedInstance.Name = classifiedInstances.First().Name;
                            classifiedInstance.Features = new List<Feature>();

                            for (int i = 0; i < featureNames.Count; i++)
                            {
                            featureNames[i] =
                            featureNames[i].ToString().Replace("°", "Degrees").Replace('.', '_').Replace('/', '_');
                            Feature f = new Feature()
                            {
                                featureName = featureNames[i].ToString(),
                                value = Double.Parse(values[i].ToString())
                                };
                                    
                                classifiedInstance.Features.Add(f);
                            }
                            _analysisManager.CreateClassifiedInstance(model.Id, analysis.CreatedBy.Id, classifiedInstance);
                    }
                }
                
            return Ok(_analysisManager.ReadAnalysis(analysisId).AnalysisModels);
        } 
    }
}
