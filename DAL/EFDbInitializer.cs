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
            
        //    //Create Min & Max values for features
        //    context.Features.Add(new Feature()
        //    {
        //        featureName = "EHS_H_Score",
        //        minMaxValue = new MinMaxValue()
        //        {
        //            MinValue = -150,
        //            MaxValue = 500,
        //            Regex = @"^-?\d{0,3}(\.\d{0,5})?$"
        //        }

        //});


        //    context.Features.Add(new Feature()
        //    {
        //        featureName = "Melting_Point_Minimum_DegreesC",
        //        minMaxValue = new MinMaxValue()
        //        {
        //            MinValue = -200,
        //            MaxValue = 20,

        //            Regex = @"^-?\d{0,3}(\.\d{0,5})?$"
        //        }
        //    });

        //    context.Features.Add(new Feature()
        //    {
        //        featureName = "Flash_Point_Minimum_DegreesC",
        //        minMaxValue = new MinMaxValue()
        //        {
        //            MinValue = -85,
        //            MaxValue = 360,

        //            Regex = @"^-?\d{0,3}(\.\d{0,5})?$"
        //        }
                
        //    });
        //    context.Features.Add(new Feature()
        //    {
        //        featureName = "Vapour_Pressure_25DegreesC_mmHg",
        //        minMaxValue = new MinMaxValue()
        //        {
        //            MinValue = 0,
        //            MaxValue = 24500,

        //            Regex = @"^\d{1,5}(\.\d{0,5})?$"
        //        }


                
        //    });
        //    context.Features.Add(new Feature()
        //    {
        //        featureName = "Density_25DegreesC_Minimum_kg_L",
        //        minMaxValue = new MinMaxValue()
        //        {
        //            MinValue = 0.6,
        //            MaxValue = 3.5,

        //            Regex = @"^\d{1}(\.\d{0,5})?$"
        //        }

                
        //    });
        //    context.Features.Add(new Feature()
        //    {
        //        featureName ="Viscosity_25DegreesC_Minimum_mPa_s",
        //        minMaxValue = new MinMaxValue()
        //        {
        //            MinValue = 0.01,
        //            MaxValue = 1000,

        //            Regex = @"^\d{1,4}(\.\d{0,5})?$"
        //        }
                
        //    });
        //    context.Features.Add(new Feature()
        //    {
        //        featureName ="Autoignition_Temperature_Minimum_DegreesC",
        //        minMaxValue = new MinMaxValue()
        //        {
        //            MinValue = 100,
        //            MaxValue = 800,

        //            Regex = @"^\d{1,3}(\.\d{0,5})?$"
        //        }
                
        //    });
        //    context.Features.Add(new Feature()
        //    {
        //        featureName ="Hansen_Delta_D_MPa1_2",
        //        minMaxValue = new MinMaxValue()
        //        {
        //            MinValue = 0,
        //            MaxValue = 30,

        //            Regex = @"^\d{1,2}(\.\d{0,5})?$"
        //        }
                
        //    });
        //    context.Features.Add(new Feature()
        //    {
        //        featureName = "Hansen_Delta_P_MPa1_2",
        //        minMaxValue = new MinMaxValue()
        //        {
        //            MinValue = 0,
        //            MaxValue = 30,

        //            Regex = @"^\d{1,2}(\.\d{0,5})?$"
        //        }
                
        //    });
        //    context.Features.Add(new Feature()
        //    {
        //        featureName ="Hansen_Delta_H_MPa1_2",
        //        minMaxValue = new MinMaxValue()
        //        {
        //            MinValue = 0,
        //            MaxValue = 50,

        //            Regex = @"^\d{1,2}(\.\d{0,5})?$"
        //        }
                
        //    });
        //    context.Features.Add(new Feature()
        //    {
        //        featureName ="Solubility_Water_g_L",
        //        minMaxValue = new MinMaxValue()
        //        {
        //            MinValue = -1,
        //            MaxValue = 1000,

        //            Regex = @"^-?\d{0,4}(\.\d{0,5})?$"
        //        }
                
        //    });
        //    context.Features.Add(new Feature()
        //    {
        //        featureName ="Dielectric_Constant_20DegreesC",
        //        minMaxValue = new MinMaxValue()
        //        {
        //            MinValue = 1,
        //            MaxValue = 100,

        //            Regex = @"^\d{1,3}(\.\d{0,5})?$"
        //        }
                
        //    });


            var Lines = SS.DAL.Properties.Resources.datasetqframe.ToString().Split('\n');

            context.SaveChanges();
        }

    }
}
