using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SS.BL.Domain.Analyses;

namespace SS.UI.Web.MVC.Controllers.Utils
{
    public class JsonHelper
    {
        public static Algorithm ParseJson(String jsonString, List<MinMaxValue> minMaxValues)
        {
            dynamic jsonModel = JsonConvert.DeserializeObject(jsonString);
            Algorithm algorithm = new Algorithm()
            {
                AlgorithmName = jsonModel.algorithm,
                Models = new List<Model>()
            };
            Model model = new Model()
            {
                Clusters = new List<Cluster>(),
                DataSet = jsonModel.dataSet,
                Date = DateTime.Now,
                NumberOfSolvents = 0,
                NumberOfFeatures = 0,
                ModelPath = jsonModel.modelPath,
                AlgorithmName = jsonModel.algorithm
            };
            foreach (var cluster in jsonModel.clusters)
            {
                Cluster clusterTemp = new Cluster()
                {
                    DistanceToClusters = new List<ClusterDistanceCenter>(),
                    Number = cluster.clusterNumber,
                    Solvents = new List<Solvent>(),
                    VectorData = new List<VectorData>(),
                   
                };
                foreach (var vector in cluster.vectorData)
                {
                    string naam = vector.name.ToString().Replace("(", "").Replace(")", "").Replace("/", "").Replace("=", "").Replace("ø", "");
                    VectorData vectorData = new VectorData()
                    {
                        Value =  vector.value,
                        FeatureName = (FeatureName)Enum.Parse(typeof(FeatureName), naam)
                    };

                    clusterTemp.VectorData.Add(vectorData);
                }
                
                foreach (var distance in cluster.distanceToCluster)
                {
                    ClusterDistanceCenter clusterDistanceCenter = new ClusterDistanceCenter()
                    {
                        ToClusterId = distance.clusterId,
                        Distance = distance.distance
                    };
                    clusterTemp.DistanceToClusters.Add(clusterDistanceCenter);
                }
                
                foreach (var solvent in cluster.solvents)
                {
                    Solvent solventTemp = new Solvent()
                    {
                        CasNumber = solvent.casNumber,
                        Name = solvent.name,
                        DistanceToClusterCenter = solvent.distanceToCluster,
                        Features = new List<Feature>(),
                        MetaData = new SolventMetaData()
                        {
                                Label = solvent.predictLabel,
                                IdCasNr = solvent.iD_CAS_Nr_1,
                                IdEgNr = solvent.iD_EG_Nr,
                                IdEgAnnexNr = solvent.iD_EG_Annex_Nr,
                                Input = solvent.input,
                                IdName1 = solvent.iD_Name_1
                        }
                    };
                    solventTemp.CasNumber = solventTemp.CasNumber.Replace("\"", "");
                    solventTemp.Name = solventTemp.Name.Replace("\"", "");
                    foreach (var feature in solvent.features)
                    {
                        FeatureName featureName;
                        Enum.TryParse<FeatureName>(feature.name.ToString(), out featureName);
                        var value = minMaxValues.FirstOrDefault(a => a.FeatureName == featureName);
                        string naam = feature.name.ToString().Replace("(", "").Replace(")", "").Replace("/", "").Replace("=", "").Replace("ø", "");
                        Feature featureTemp = new Feature()
                        {
                            FeatureName = naam,
                            Value = feature.value
                        };
                        featureTemp.MinMaxValue = value;
                        solventTemp.Features.Add(featureTemp);
                    }
                    clusterTemp.Solvents.Add(solventTemp);
                    model.NumberOfFeatures = solventTemp.Features.Count;
                }
                model.NumberOfSolvents += clusterTemp.Solvents.Count;
                model.Clusters.Add(clusterTemp);
            }
            algorithm.Models.Add(model);
            return algorithm;
        }

        public static ClassifiedInstance ParseJsonToClassifiedInstance(string json)
        {
            dynamic jsonInstance = JsonConvert.DeserializeObject(json);
            ClassifiedInstance classifiedInstance = new ClassifiedInstance()
            {
                DistanceToClusterCenter = jsonInstance.distanceToCluster,
                ClusterNumber = jsonInstance.clusterNumber
            };
            return classifiedInstance;
        }
    }
}