using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SS.BL.Domain.Users;
using SS.BL.Domain.Analyses;

namespace SS.DAL
{
    public class EFDbInitializer : DropCreateDatabaseIfModelChanges<EFDbContext>
    {
        protected override void Seed(EFDbContext context)
        {
            
            //Create Min & Max values for features
            context.Features.Add(new Feature()
            {
                featureName = "EHS_H_Score",
                minMaxValue = new MinMaxValue()
                {
                    MinValue = -150,
                    MaxValue = 500,
                    Regex = @"^-?\d{0,3}(\.\d{0,5})?$"
                }

        });


            context.Features.Add(new Feature()
            {
                featureName = "Melting_Point_Minimum_DegreesC",
                minMaxValue = new MinMaxValue()
                {
                    MinValue = -200,
                    MaxValue = 20,

                    Regex = @"^-?\d{0,3}(\.\d{0,5})?$"
                }
            });

            context.Features.Add(new Feature()
            {
                featureName = "Flash_Point_Minimum_DegreesC",
                minMaxValue = new MinMaxValue()
                {
                    MinValue = -85,
                    MaxValue = 360,

                    Regex = @"^-?\d{0,3}(\.\d{0,5})?$"
                }
                
            });
            context.Features.Add(new Feature()
            {
                featureName = "Vapour_Pressure_25DegreesC_mmHg",
                minMaxValue = new MinMaxValue()
                {
                    MinValue = 0,
                    MaxValue = 24500,

                    Regex = @"^\d{1,5}(\.\d{0,5})?$"
                }


                
            });
            context.Features.Add(new Feature()
            {
                featureName = "Density_25DegreesC_Minimum_kg_L",
                minMaxValue = new MinMaxValue()
                {
                    MinValue = 0.6,
                    MaxValue = 3.5,

                    Regex = @"^\d{1}(\.\d{0,5})?$"
                }

                
            });
            context.Features.Add(new Feature()
            {
                featureName ="Viscosity_25DegreesC_Minimum_mPa_s",
                minMaxValue = new MinMaxValue()
                {
                    MinValue = 0.01,
                    MaxValue = 1000,

                    Regex = @"^\d{1,4}(\.\d{0,5})?$"
                }
                
            });
            context.Features.Add(new Feature()
            {
                featureName ="Autoignition_Temperature_Minimum_DegreesC",
                minMaxValue = new MinMaxValue()
                {
                    MinValue = 100,
                    MaxValue = 800,

                    Regex = @"^\d{1,3}(\.\d{0,5})?$"
                }
                
            });
            context.Features.Add(new Feature()
            {
                featureName ="Hansen_Delta_D_MPa1_2",
                minMaxValue = new MinMaxValue()
                {
                    MinValue = 0,
                    MaxValue = 30,

                    Regex = @"^\d{1,2}(\.\d{0,5})?$"
                }
                
            });
            context.Features.Add(new Feature()
            {
                featureName = "Hansen_Delta_P_MPa1_2",
                minMaxValue = new MinMaxValue()
                {
                    MinValue = 0,
                    MaxValue = 30,

                    Regex = @"^\d{1,2}(\.\d{0,5})?$"
                }
                
            });
            context.Features.Add(new Feature()
            {
                featureName ="Hansen_Delta_H_MPa1_2",
                minMaxValue = new MinMaxValue()
                {
                    MinValue = 0,
                    MaxValue = 50,

                    Regex = @"^\d{1,2}(\.\d{0,5})?$"
                }
                
            });
            context.Features.Add(new Feature()
            {
                featureName ="Solubility_Water_g_L",
                minMaxValue = new MinMaxValue()
                {
                    MinValue = -1,
                    MaxValue = 1000,

                    Regex = @"^-?\d{0,4}(\.\d{0,5})?$"
                }
                
            });
            context.Features.Add(new Feature()
            {
                featureName ="Dielectric_Constant_20DegreesC",
                minMaxValue = new MinMaxValue()
                {
                    MinValue = 1,
                    MaxValue = 100,

                    Regex = @"^\d{1,3}(\.\d{0,5})?$"
                }
                
            });


            var Lines = SS.DAL.Properties.Resources.datasetqframe.ToString().Split('\n');

           



            foreach (var line in Lines.Skip(2))
            {

                if (line.ToString() == null || line.ToString() == "")
                {
                    break;
                }
                
                    string[] result = line.ToString().Split(';');
                    context.EHSScores.Add(new EHSScore()
                    {
                        Source = result.ElementAt(0).ToString(),
                        CasNumber = result.ElementAt(2).ToString(),
                        IDName = result.ElementAt(1).ToString(),
                        EGNr = result.ElementAt(3).ToString(),
                        EGAnnexNr = result.ElementAt(4).ToString(),
                        EhsSScore = Int32.Parse(result.ElementAt(5)),
                        EhsHScore = Int32.Parse(result.ElementAt(6)),
                        EhsEScore = Int32.Parse(result.ElementAt(7)),
                        EhsColorCode = result.ElementAt(8).ToString(),
                        BoilingPoint = Convert.ToDouble(result.ElementAt(9)),
                        MeltingPoint = Convert.ToDouble(result.ElementAt(10)),
                        VapourPress = Convert.ToDouble(result.ElementAt(11)),
                        FlashPoint = Convert.ToDouble(result.ElementAt(12)),
                        Autoignition = Convert.ToDouble(result.ElementAt(13)),
                        HansenDeltaD = Convert.ToDouble(result.ElementAt(14)),
                        HansenDeltaP = Convert.ToDouble(result.ElementAt(15)),
                        HansenDeltaH = Convert.ToDouble(result.ElementAt(16)),
                        SolubilityWater = Convert.ToDouble(result.ElementAt(17)),
                        Density = Convert.ToDouble(result.ElementAt(18)),
                        Viscosity = Convert.ToDouble(result.ElementAt(19)),
                        RelativeVapDen = Convert.ToDouble(result.ElementAt(20)),
                        LogPOctanol = Convert.ToDouble(result.ElementAt(21)),
                        RefractiveIndex = Convert.ToDouble(result.ElementAt(22)),
                        SurfaceTension = Convert.ToDouble(result.ElementAt(23))


                    });
                




            }



            context.SaveChanges();
        }

    }
}
