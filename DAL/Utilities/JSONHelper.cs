using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SS.BL.Domain.Analyses;

namespace SS.DAL.Utilities
{
    public class JsonHelper
    {
        //0.4.9 - Change List<MinMaxValue> to List<Feature> In order to solve new architecture problems (Dynamic Database)
        public static Algorithm ParseJson(String jsonString)
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
                        Value = vector.value,
                        //0.4.9 Changed to new Feature
                        feature = new Feature()
                        {
                            featureName = naam
                        }

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
                        Name = solvent.iD_Name,
                        Source = solvent.source,
                        EgNumber = solvent.iD_EG_Nr,
                        EgAnnexNumber = solvent.iD_EG_Annex_Nr,
                        EHS_E_SCORE = solvent.ehs_E_SCORE,
                        EHS_H_SCORE = solvent.ehs_H_SCORE,
                        EHS_S_SCORE = solvent.ehs_S_SCORE,
                        EHS_Color_Code = solvent.ehs_COLOR_CODE,
                        DistanceToClusterCenter = solvent.distanceToCluster,
                        Features = new List<Feature>(),
                        //0.5.0.1 Move metadata to feature
                        //MetaData = new SolventMetaData()
                        //{
                        //        Label = solvent.predictLabel,
                        //        IdCasNr = solvent.iD_CAS_Nr_1,
                        //        IdEgNr = solvent.iD_EG_Nr,
                        //        IdEgAnnexNr = solvent.iD_EG_Annex_Nr,
                        //        Input = solvent.input,
                        //        IdName1 = solvent.iD_Name_1
                        //}
                    };
                    //meta-data toevoegen aan feature

                    solventTemp.CasNumber = solventTemp.CasNumber.Replace("\"", "");
                    solventTemp.Name = solventTemp.Name.Replace("\"", "");
                    solventTemp.EHS_Color_Code = solventTemp.EHS_Color_Code.Replace("\"", "");

                    foreach (var feature in solvent.features)
                    {
                        string featureName;
                        //0.4.9 - Changed TryParse of FeatureName to .ToString() as featureName was changed from type FeatureName to a string
                        featureName = feature.name.ToString();
                        //0.4.9 - Changed from minMaxValues to features In order to solve new architecture problems (Dynamic Database)
                        //0.5.0.3 
                        string naam = feature.name.ToString().Replace("(", "").Replace(")", "").Replace("/", "").Replace("=", "").Replace("ø", "");

                        Feature featureTemp = new Feature()
                        {
                            featureName = naam,
                            value = feature.value,
                            //0.5.0.1 Added PrimaryData

                        };
                        //0.5.0 featureTemp.minMaxValue = value.minMaxValue;
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

        //public static ClassifiedInstance ParseJsonToClassifiedInstance(Cla response)
        //{
        //    ClassifiedInstance classifiedInstance = new ClassifiedInstance()
        //    {
        //        DistanceToClusterCenter = distanceToCluster,
        //        ClusterNumber = jsonInstance.clusterNumber
        //    };
        //    return classifiedInstance;
        //} 
    }
}