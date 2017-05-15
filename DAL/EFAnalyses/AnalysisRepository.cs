using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.BL.Domain.Analyses;
using SS.BL.Domain.Users;
using Newtonsoft.Json.Linq;
using SS.DAL.Utilities;

namespace SS.DAL.EFAnalyses
{
    public class AnalysisRepository : IAnalysisRepository
    {
        //Declaring DLL
        static string pathWithEnv = @"%USERPROFILE%\";
        string filePath = Environment.ExpandEnvironmentVariables(pathWithEnv);
        com.sussol.domain.utilities.Globals globals;

        private readonly com.sussol.web.controller.ServiceModel sus;

        private readonly EFDbContext _context;

        public AnalysisRepository(EFDbContext efDbContext)
        {
            this._context = efDbContext;
            com.sussol.domain.utilities.Globals.STORAGE_PATH = filePath;
            sus = new com.sussol.web.controller.ServiceModel();
        }

        public Algorithm CreateAlgorithm(Algorithm algorithm)
        {
            algorithm = _context.Algorithms.Add(algorithm);
            _context.SaveChanges();
            return algorithm;
        }

        public Analysis CreateAnalysis(Analysis analysis, User createdBy)
        {
            analysis.CreatedBy = createdBy;
            analysis = _context.Analyses.Add(analysis);
            _context.SaveChanges();
            return analysis;
        }

      public TrainingSet RemoveTrainingSet(TrainingSet trainingset)
      {
         TrainingSet set = _context.TrainingSet.Remove(trainingset);
         _context.SaveChanges();
         return set;
      }

      public Analysis ReadAnalysis(long id)
        {
            return _context.Analyses
                .Include(a => a.CreatedBy)
                .Include(a => a.SharedWith)
                .Include(a => a.AnalysisModels)
                .Include(a => a.AnalysisModels.Select(an => an.ClassifiedInstance))
                .Include(a => a.AnalysisModels.Select(an => an.Model))
                .Include(a => a.AnalysisModels.Select(an => an.ClassifiedInstance))
                .Include(a => a.AnalysisModels.Select(an => an.Model).Select(p => p.Clusters))
                .Include(a => a.AnalysisModels.Select(an => an.Model).Select(p => p.trainingSet))
                .Include(a => a.AnalysisModels.Select(an => an.ClassifiedInstance).Select(p => p.Features))
                .Include(a => a.AnalysisModels.Select(an => an.Model).Select(p => p.Clusters.Select(pt => pt.DistanceToClusters))) 
                .Include(a => a.AnalysisModels.Select(an => an.Model).Select(p => p.Clusters.Select(pt => pt.Solvents)))
                .Include(a => a.AnalysisModels.Select(an => an.Model).Select(p => p.Clusters.Select(pt => pt.VectorData)))
                .Include(a => a.AnalysisModels.Select(an => an.Model).Select(p => p.Clusters.Select(pt => pt.VectorData.Select(v => v.feature))))
                .Include(a => a.AnalysisModels.Select(an => an.Model).Select(p => p.Clusters.Select(pt => pt.Solvents.Select(v => v.Features))))
                //0.5.0 .Include(a => a.AnalysisModels.Select(an => an.Model).Select(p => p.Clusters.Select(pt => pt.Solvents.Select(v => v.Features.Select(b => b.minMaxValue)))))
                .FirstOrDefault(i => i.Id == id);
        }

        public Analysis ReadAnalysis(string name)
        {
            return _context.Analyses.FirstOrDefault(a => a.Name.Equals(name));
        }

        public Analysis CreateAnalysis(Analysis analysis, string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email.Equals(email));
            analysis.CreatedBy =  user;
            analysis = _context.Analyses.Add(analysis);
            _context.SaveChanges();
            return analysis;
        }

        public IEnumerable<Analysis> ReadAnalysesForUser(User user)
        {
            return _context.Analyses.Where(u => u.CreatedBy.Id == user.Id)
                .Include(p=>p.CreatedBy).ToList();

        }

        public IEnumerable<Analysis> ReadAnalysesForOrganisation(long id)
        {
            return _context.Analyses
                .Include(p => p.CreatedBy)
                .Where(o => o.SharedWith.Id == id).ToList();
        }

        public IEnumerable<Analysis> ReadAnalysesForUserPermission(long userId)
        {
            var organisation = _context.Users
                .Include(o => o.Organisation)
                .Single(u => u.Id == userId).Organisation;
            var analyses = _context.Analyses.Where(a => a.CreatedBy.Id == userId).ToList();

            if (organisation != null)
            {
                var analysesTemp2 = _context.Analyses.Where(a => a.SharedWith.Id == organisation.Id).ToList();
                analyses.AddRange(analysesTemp2);
            }
            return analyses;
        }

        public Analysis UpdateAnalysis(Analysis analysis)
        {
            var currentAnalysis = _context.Analyses.Find(analysis.Id);

            _context.Entry(currentAnalysis).CurrentValues.SetValues(analysis);
            _context.Entry(currentAnalysis).State = EntityState.Modified;
            _context.SaveChanges();
            return currentAnalysis;
        }

        public Analysis UndoShare(long id)
        {
            var analysis = _context.Analyses
                .Include(a => a.SharedWith)
                .Single(a => a.Id == id);
            analysis.SharedWith = null;
            _context.Entry(analysis).State = EntityState.Modified;
            _context.SaveChanges();
            return analysis;
        }

        public Analysis ShareWithOrganisation(long organisationId, long analysisId)
        {
            var organisation = _context.Organisations.Find(organisationId);
            var analysis = _context.Analyses.Find(analysisId);
            analysis.SharedWith = organisation;
            _context.SaveChanges();
            return analysis;
        }

        public Model ReadModel(string dataSet, AlgorithmName algorithmName)
        {
             return _context.Models
                .Include(p => p.Clusters)
                .Include(p => p.Clusters.Select(pt => pt.DistanceToClusters))
                .Include(p => p.Clusters.Select(pt => pt.Solvents))
                .Include(p => p.Clusters.Select(pt => pt.Solvents.Select(v => v.Features)))
                .Where(t => t.DataSet.Equals(dataSet))
                .FirstOrDefault(a => a.AlgorithmName == algorithmName);
        }

        public IEnumerable<Analysis> ReadAnalyses()
        {
            return _context.Analyses.Include(a => a.CreatedBy).ToList();
        }

        public IEnumerable<Analysis> ReadFullAnalyses()
        {
            return _context.Analyses.Include(p => p.AnalysisModels).Include(p => p.AnalysisModels.Select(a => a.Model));
        }

        public List<Model> ReadModelsForAlgorithm(AlgorithmName algorithmName)
        {
            return _context.Models.Where(m => m.AlgorithmName == algorithmName).ToList();
        }

        public IEnumerable<ClassifiedInstance> ReadAllClassifiedInstances(long userId, string name)
        {
            var instances = _context.Users
                .Include(a => a.ClassifiedInstances)
                .Include(a => a.ClassifiedInstances.Select(p => p.Features))
                .Single(a => a.Id == userId).ClassifiedInstances;
            return instances.Where(a => a.Name.Equals(name));
        }

      //0.5.0 Removing MinMaxValue

      public IEnumerable<ClassifiedInstance> ReadClassifiedInstancesForUser(long userId, long analysisId)
        {
            IEnumerable<ClassifiedInstance> instances = _context.Users
                .Include(a => a.ClassifiedInstances)
                .Single(a => a.Id == userId).ClassifiedInstances;
            var instancesDistinct = instances.GroupBy(a => a.Name).Select(group => group.First()).ToList();
            var analysis = ReadAnalysis(analysisId);
            var instancesToReturn = new List<ClassifiedInstance>();
            foreach (var instance in instancesDistinct)
            {
                var model = _context.AnalysisModels 
                    .Include(a => a.Model)
                    .Single(a => a.Id == instance.AnalysisModelId).Model; 
                if (model.DataSet.Equals(analysis.AnalysisModels.First().Model.DataSet))
                {
                    instancesToReturn.Add(instance);
                }
            }
            return instancesToReturn;
        }

        public AnalysisModel CreateClassifiedInstance(long modelId, long userId, ClassifiedInstance classifiedInstance)
        {
            var model = _context.AnalysisModels
                .Include(a => a.Model)
                .Include(a => a.Model.Clusters)
                .Include(a => a.Model.trainingSet)
                .Include(a => a.Model.Clusters.Select(an => an.DistanceToClusters))
                .Include(a => a.Model.Clusters.Select(p => p.VectorData))
                .Include(a => a.Model.Clusters.Select(p => p.VectorData.Select(v => v.feature)))
                .Include(a => a.Model.Clusters.Select(p => p.Solvents))
                .Include(a => a.Model.Clusters.Select(p => p.Solvents.Select(m => m.Features)))
                // 0.5.0 .Include(a => a.Model.Clusters.Select(p => p.Solvents.Select(m => m.Features.Select(o => o.minMaxValue))))
                .Single(a => a.Id == modelId);
            classifiedInstance.AnalysisModelId = modelId;
            model.ClassifiedInstance = classifiedInstance;
            _context.Entry(model).State = EntityState.Modified;
            var user = _context.Users.Include(p=>p.ClassifiedInstances).Single(u => u.Id == userId);
            user.ClassifiedInstances.Add(classifiedInstance);
            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();

            return model;
        }

        public AnalysisModel SetClassifiedSolvent(long modelId, long instanceId)
        {
            var model = _context.AnalysisModels.Find(modelId);
            var classifiedInstance =
                _context.ClassifiedInstances.Find(instanceId);
            model.ClassifiedInstance = classifiedInstance;
            _context.Entry(model).State = EntityState.Modified;
            _context.SaveChanges();

            return model;
        }

        public void DeleteAnalysis(long analysisId)
        {
            var currentAnalysis = _context.Analyses.Include(p => p.AnalysisModels).Single(p => p.Id == analysisId);

            _context.AnalysisModels.RemoveRange(currentAnalysis.AnalysisModels);
            _context.Analyses.Remove(currentAnalysis);
            _context.SaveChanges();
        }
        public Boolean CheckCasNumber(String casnummer)
        {
            //bool contains;
            //if(contains = _context.Solvents.FirstOrDefault(a => a.CasNumber.Equals(casnummer))) 
            //    return true;
            //else return false;
            var result = _context.Solvents.Find(casnummer);
            if (result ==null)
                return true;
            else return false;

        }

        public IEnumerable<Feature> ReadFeatures()
        {
            return _context.Features.ToList();
        }

        public IEnumerable<Solvent> ReadSolvents()
        {
            return _context.Solvents.ToList();
        }

      public TrainingSet addTrainingSet(TrainingSet set)
      {
        TrainingSet trainingSet = _context.TrainingSet.Add(set);
         _context.SaveChanges();
         return trainingSet;
      }

      public IEnumerable<TrainingSet> ReadTrainingSets()
      {
         return _context.TrainingSet.ToList();
      }

      public TrainingSet ReadTrainingSetById(int id)
      {
         return _context.TrainingSet.Find(id);
      }

      public List<Algorithm> createNewModelsFromTrainingsfile(TrainingSet training)
        {
            bool New = false;
            List<Algorithm> algos = new List<Algorithm>();
            if (_context.Algorithms.Count() > 0)
            {
                algos = _context.Algorithms.ToList();
            }
            else
            {
                New = true;
                foreach (AlgorithmName name in Enum.GetValues(typeof(AlgorithmName)))
                {
                    Algorithm al = new Algorithm();
                    al.AlgorithmName = name;
                    algos.Add(al);
                }
            }
            
            foreach(Algorithm l in algos)
            {
                JObject AlgorithmObject = JObject.Parse(sus.createModel((int)l.AlgorithmName, training.dataSet.ToString()));
                int y = 0;
                //JToken jModelC = AlgorithmObject["model"];
                foreach (Model m in JsonHelper.ParseJson(AlgorithmObject.ToString()).Models.ToList())
                {
                    m.DataSet = l.AlgorithmName + "_" + y;
                    m.trainingSet = training;
                    l.Models.Add(m);
                    y++;
                }
                
                
            }

            if (New)
            {
                _context.Algorithms.AddRange(algos);
            }
            else
            {
                _context.Entry(algos).State = EntityState.Modified;
            }
            //Add to context
            
            _context.SaveChanges();


            return algos;
            
        }
   }
}
