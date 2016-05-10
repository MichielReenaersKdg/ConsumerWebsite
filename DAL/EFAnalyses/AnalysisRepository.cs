﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.BL.Domain.Analyses;
using SS.BL.Domain.Users;

namespace SS.DAL.EFAnalyses
{
    public class AnalysisRepository : IAnalysisRepository
    {
        private readonly EFDbContext _context;

        public AnalysisRepository(EFDbContext efDbContext)
        {
            this._context = efDbContext;
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

        public Analysis ReadAnalysis(long id)
        {
            return _context.Analyses
                .Include(a => a.AnalysisModels)
                .Include(a => a.AnalysisModels.Select(an => an.Model))
                .Include(a => a.AnalysisModels.Select(an => an.Model).Select(p => p.Clusters.Select(pt => pt.DistanceToClusters))) 
                .Include(a => a.AnalysisModels.Select(an => an.Model).Select(p => p.Clusters.Select(pt => pt.Solvents)))
                .Include(a => a.AnalysisModels.Select(an => an.Model).Select(p => p.Clusters.Select(pt => pt.VectorData)))
                .Include(a => a.AnalysisModels.Select(an => an.Model).Select(p => p.Clusters.Select(pt => pt.Solvents.Select(v => v.Features))))
                .FirstOrDefault(i => i.Id == id);
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
            return _context.Analyses.Where(o => o.SharedWith.Id == id).ToList();
        }

        public Analysis UpdateAnalysis(Analysis analysis)
        {
            var currentAnalysis = _context.Analyses.Find(analysis.Id);

            _context.Entry(currentAnalysis).CurrentValues.SetValues(analysis);
            _context.Entry(currentAnalysis).State = EntityState.Modified;
            _context.SaveChanges();
            return currentAnalysis;
        }

        public void ShareWithOrganisation(long organisationId, long analysisId)
        {
            var organisation = _context.Organisations.Find(organisationId);
            var analysis = _context.Analyses.Find(analysisId);
            analysis.SharedWith = organisation;
            _context.SaveChanges();
        }

        public Cluster CreateCluster(Cluster cluster)
        {
            cluster = _context.Clusters.Add(cluster);
            _context.SaveChanges();
            return cluster;
        }

        public IEnumerable<Cluster> ReadClustersForModel(Model model)
        {
            return _context.Models.Find(model.Id).Clusters.ToList();
        }

        public Feature CreateFeature(Feature feature)
        {
            feature = _context.Features.Add(feature);
            _context.SaveChanges();
            return feature;
        }

        public Model CreateModel(Model model)
        {
            model = _context.Models.Add(model);
            _context.SaveChanges();
            return model;
        }

        public Model ReadModel(long id)
        {
            return _context.Models
                .Include(p => p.Clusters)
                .Include(p => p.Clusters.Select(pt => pt.DistanceToClusters))
                .Include(p => p.Clusters.Select(pt => pt.Solvents))
                .Include(p => p.Clusters.Select(pt => pt.Solvents.Select(v => v.Features)))
                .FirstOrDefault(i => i.Id == id);
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

        public Solvent CreateSolvent(Solvent solvent)
        {
            solvent = _context.Solvents.Add(solvent);
            _context.SaveChanges();
            return solvent;
        }

        public ClusterDistanceCenter CreateClusterDistanceCenter(ClusterDistanceCenter clusterDistanceCenter)
        {
            clusterDistanceCenter = _context.ClusterDistanceCenters.Add(clusterDistanceCenter);
            _context.SaveChanges();
            return clusterDistanceCenter;
        }

        public AnalysisModel CreateAnalysisModel(AnalysisModel analysisModel)
        {
            analysisModel = _context.AnalysisModels.Add(analysisModel);
            _context.SaveChanges();
            return analysisModel;
        }

        public List<Model> ReadModelsForAlgorithm(AlgorithmName algorithmName)
        {
            return _context.Models.Where(m => m.AlgorithmName == algorithmName).ToList();
        }
    }
}
