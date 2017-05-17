using System;
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




        //[Route("FillAlgorithms")]
        //[HttpGet]
        ////0.4.0 Added method to utilize new dataset in Weka dll instead of API
        ////POST api/Analysis/FillAlgorithm
        //public List<Model> FillAlgorithms(AlgorithmName algorithm)
        //{
        //    int x = 0;
        //    List<Model> modal = new List<Model>();
        //    foreach (var file in _analysisManager.ReadTrainingSets())
        //    {
        //        var pathWithEnv = @"%USERPROFILE%\";
        //        var filePath = Environment.ExpandEnvironmentVariables(pathWithEnv);
        //        com.sussol.domain.utilities.Globals.STORAGE_PATH = filePath;
        //        com.sussol.web.controller.ServiceModel sus = new com.sussol.web.controller.ServiceModel();
        //        JObject jObject = new JObject();
        //        //0.5.0.9
        //        switch (algorithm)
        //        {
        //            case AlgorithmName.CANOPY: jObject = JObject.Parse(sus.canopyModeller(file.dataSet, "", "").ToString()); break;
        //            case AlgorithmName.SOM: jObject = JObject.Parse(sus.somModeller(file.dataSet, "").ToString()); break;
        //            case AlgorithmName.XMEANS: jObject = JObject.Parse(sus.xmeansModeller(file.dataSet, "", "", "").ToString()); break;
        //        }
        //        //var perso = JsonConvert.DeserializeObject<dynamic>();

        //        JToken jModel = jObject["model"];


        //        //0.4.9 _analysisManager.ReadMinMaxValues().ToList()).Models.ToList() -> _analysisManager.ReadFeatures().ToList()).Models.ToList()
        //        List<Model> mod = JsonHelper.ParseJson(jObject.ToString()).Models.ToList();
        //        int y = 0;
        //        foreach (Model m in mod)
        //        {
        //            m.DataSet = algorithm + "_" + x + y;
        //            y++;
        //        }
                
        //        Algorithm algo = new Algorithm()
        //        {
        //            AlgorithmName = algorithm,
        //            Models = mod
        //        };
        //        _analysisManager.CreateAlgorithm(algo);
        //        modal.AddRange(mod);
        //        x++;
        //    }
        //    return modal;
         

        //}

      //[Route("FillAlgorithm")]
      //[HttpGet]
      //0.4.0 Added method to utilize new dataset in Weka dll instead of API
      //POST api/Analysis/FillAlgorithm
      //public List<Model> FillAlgorithms(AlgorithmName algorithm, int Id)
      //{
      //   TrainingSet traingingset = _analysisManager.ReadTrainingSetById(Id);

      //   var pathWithEnv = @"%USERPROFILE%\";
      //   var filePath = Environment.ExpandEnvironmentVariables(pathWithEnv);
      //   com.sussol.domain.utilities.Globals.STORAGE_PATH = filePath;
      //   com.sussol.web.controller.ServiceModel sus = new com.sussol.web.controller.ServiceModel();
      //   JObject jObject = new JObject();
      //   //0.5.0.9
      //   switch (algorithm)
      //   {
      //      case AlgorithmName.CANOPY: jObject = JObject.Parse(sus.canopyModeller(traingingset.dataSet, "", "").ToString()); break;
      //      case AlgorithmName.SOM: jObject = JObject.Parse(sus.somModeller(traingingset.dataSet, "").ToString()); break;
      //      case AlgorithmName.XMEANS: jObject = JObject.Parse(sus.xmeansModeller(traingingset.dataSet, "", "", "").ToString()); break;
      //   }
      //   //var perso = JsonConvert.DeserializeObject<dynamic>();

      //   JToken jModel = jObject["model"];


      //   //0.4.9 _analysisManager.ReadMinMaxValues().ToList()).Models.ToList() -> _analysisManager.ReadFeatures().ToList()).Models.ToList()
      //   List<Model> mod = JsonHelper.ParseJson(jObject.ToString()).Models.ToList();
      //   Algorithm algo = new Algorithm()
      //   {
      //      AlgorithmName = algorithm,
      //      Models = mod
      //   };
      //   _analysisManager.CreateAlgorithm(algo);
      //   return mod;

      //}

      [Route("AddTrainingSet")]
      [HttpPost]
      public IHttpActionResult AddTrainingSet([FromBody]TrainingSet trainingSet)
      {
         TrainingSet set = _analysisManager.CreateTrainingSet(trainingSet);
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
      [Route("DeleteTrainingSet/{id:int}")]
      [HttpPost]
      public IHttpActionResult DeleteTrainingSet(int id)
      {
         TrainingSet trainingSet = _analysisManager.ReadTrainingSetById(id);
         if (trainingSet == null)
         {
            return BadRequest("TrainingSet not found");
         }

         TrainingSet set = _analysisManager.RemoveTrainingSet(trainingSet);
         if (set == null)
         {
            return BadRequest("TrainingSet not deleted");
         }

         return Ok("TrainingSet deleted");
      }


      //POST api/Analysis/Createanalysis
      [Route("CreateAnalysis")]
        [HttpPost]
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
            var instances = _analysisManager.ReadClassifiedInstancesForUser(model.UserId, analysisId).ToList();
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

            using (var client = new WebClient())
            {
                List<AnalysisModel> analysisModels = new List<AnalysisModel>();
                foreach (var analysisModel in model.AnalysisModels)
                {
                    var serialized = JsonConvert.SerializeObject(model.Values);
                    
                    //client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    //var sussol = new com.sussol.web.controller.ServiceModel();
                    //var response = sussol.classifySolvent(analysisModel.Model.ModelPath, serialized);
                    //var classifiedInstance = JsonHelper.ParseJsonToClassifiedInstance(response.ToString());
                    //classifiedInstance.CasNumber = model.CasNumber;
                    //classifiedInstance.Name = model.Name;
                    //classifiedInstance.Features = new List<Feature>();

                    var sussol = new com.sussol.web.controller.ServiceModel();
                    var response = sussol.classifySolvent(analysisModel.Model.ModelPath, serialized);
                    ClassifiedInstance classifiedInstance = new ClassifiedInstance()
                    {
                        DistanceToClusterCenter = (Double)response.getDistanceToCluster(),
                        ClusterNumber = (int)response.getClusterNumber()
                    };
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
                client.Dispose();
                return Ok(analysisModels);
            }
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
                    using (var client = new WebClient())
                    {
                        
                            var serialized = JsonConvert.SerializeObject(values);
                            
                            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                            var sussol = new com.sussol.web.controller.ServiceModel();
                            var response = sussol.classifySolvent(model.Model.ModelPath, serialized);
                            ClassifiedInstance classifiedInstance = new ClassifiedInstance()
                            {
                                DistanceToClusterCenter = (Double)response.getDistanceToCluster(),
                                ClusterNumber = (int)response.getClusterNumber()
                            };
                             sussol.classifySolvent(model.Model.ModelPath, serialized).toString();
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
                        client.Dispose();
                    }
                }
                
            }
            return Ok(_analysisManager.ReadAnalysis(analysisId).AnalysisModels);
        } 
    }
}
