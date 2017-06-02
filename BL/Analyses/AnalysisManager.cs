﻿using SS.DAL.EFAnalyses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.BL.Domain.Analyses;
using SS.BL.Domain.Users;
using System.Collections.ObjectModel;

namespace SS.BL.Analyses
{
    public class AnalysisManager : IAnalysisManager
    {
        private readonly IAnalysisRepository repo;

      //0.5.0 Removing MinMaxValues
        public AnalysisManager(IAnalysisRepository iAnalysisRepository)
        {
            this.repo = iAnalysisRepository;
        }

        public Algorithm CreateAlgorithm(Algorithm algorithm)
        {
            return repo.CreateAlgorithm(algorithm);
        }

      public Analysis CreateAnalysis(string name, DateTime dateCreated, User createdBy)
        {
            Analysis analysis = new Analysis()
            {
                Name = name,
                DateCreated = dateCreated
            };
            return repo.CreateAnalysis(analysis, createdBy);
        }

        public Analysis ReadAnalysis(long id)
        {
            return repo.ReadAnalysis(id);
        }

        public Analysis ReadAnalysis(string name)
        {
            return repo.ReadAnalysis(name);
        }

        public Analysis CreateAnalysis(Analysis analysis, string email)
        { 
            return repo.CreateAnalysis(analysis, email);
        }

        public IEnumerable<Analysis> ReadAnalysesForUser(User user)
        {
            return repo.ReadAnalysesForUser(user);
        }

        public IEnumerable<Analysis> ReadAnalyses()
        {
            return repo.ReadAnalyses();
        }

        public IEnumerable<Analysis> ReadAnalysesForOrganisation(long id)
        {
            return repo.ReadAnalysesForOrganisation(id);
        }

        public IEnumerable<Analysis> ReadAnalysesForUserPermission(long userId)
        {
            return repo.ReadAnalysesForUserPermission(userId);
        }

        public Analysis UpdateAnalysis(Analysis analysis)
        {
            return repo.UpdateAnalysis(analysis);
        }

        public Analysis UndoShare(long id)
        {
            return repo.UndoShare(id);
        }

        public Analysis ShareWithOrganisation(long organisationId, long analysisId)
        {
            return repo.ShareWithOrganisation(organisationId, analysisId);
        }

        public void DeleteAnalysis(long analysisId)
        {
            repo.DeleteAnalysis(analysisId);
        }

        public List<Model> ReadModelsForAlgorithm(AlgorithmName algorithmName)
        {
            return repo.ReadModelsForAlgorithm(algorithmName);
        }

        public Model ReadModel(int trainingsFileID, AlgorithmName algorithmName)
        {
            return repo.ReadModel(trainingsFileID, algorithmName);
        }

        public IEnumerable<ClassifiedInstance> ReadAllClassifiedInstances(long userId, string name)
        {
            return repo.ReadAllClassifiedInstances(userId, name);
        }

        public IEnumerable<ClassifiedInstance> ReadClassifiedInstancesForUser(long userId, long analysisId)
        {
            return repo.ReadClassifiedInstancesForUser(userId, analysisId);
        }

        public AnalysisModel CreateClassifiedInstance(long modelId, long userId ,ClassifiedInstance classifiedInstance)
        {
            return repo.CreateClassifiedInstance(modelId, userId,classifiedInstance);
        }

        public AnalysisModel SetClassifiedSolvent(long modelId, long instanceId)
        {
            return repo.SetClassifiedSolvent(modelId, instanceId);
        }


        public IEnumerable<Analysis> ReadFullAnalyses()
        {
            return repo.ReadFullAnalyses();
        }


        public Boolean CheckCasnumber(String casnummer) {
            return repo.CheckCasNumber(casnummer);
        }

        //0.4.9 - Add featurefunctionality In order to solve new architecture (Dynamic Database)

        public IEnumerable<Feature> ReadFeatures()
        {
            return repo.ReadFeatures();
        }

        public IEnumerable<Solvent> ReadSolvents()
        {
            return repo.ReadSolvents();
        }

      public TrainingSet CreateTrainingSet(TrainingSet set)
      {
         return repo.addTrainingSet(set);
      }

      public IEnumerable<TrainingSet> ReadTrainingSets()
      {
         return repo.ReadTrainingSets();
      }

      public TrainingSet ReadTrainingSetById(int id)
      {
         return repo.ReadTrainingSetById(id);
      }

      public TrainingSet createNewModelsFromTrainingsfile(TrainingSet training)
      {
         return repo.createNewModelsFromTrainingsfile(training);
      }

      public void RemoveTrainingSet(TrainingSet trainingset)
      {
         IEnumerable<Model> models = repo.readModelsForTrainingSet(trainingset.ID);
         IEnumerable<Analysis> analysis = repo.ReadFullAnalyses();
         repo.removeTrainingSet(models.ToList(), analysis.ToList(), trainingset);
      }

      public ClassifiedInstance ClassifyNewSolvent(string modelPath, string serialized)
      {
         return repo.ClassifyNewSolvent(modelPath, serialized);
      }
   }
}
