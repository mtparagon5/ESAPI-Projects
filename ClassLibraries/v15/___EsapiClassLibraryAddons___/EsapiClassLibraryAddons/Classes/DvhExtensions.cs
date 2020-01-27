namespace VMS.TPS
{
    using System;
    using System.Linq;
    using VMS.TPS.Common.Model.API;
    using VMS.TPS.Common.Model.Types;

    /// <summary>
    /// A collection of DVH Extensions provided by ESAPI examples, as well as a few others. Sometimes, their example methods don't return values if the sample volumes are too small.
    /// </summary>
    public static class DvhExtensions
    {
        public static DoseValue GetDoseAtVolume(this PlanningItem pitem, Structure structure, double volume, VolumePresentation volumePresentation, DoseValuePresentation requestedDosePresentation)
        {
          if (pitem is PlanSetup)
          {
            return ((PlanSetup)pitem).GetDoseAtVolume(structure, volume, volumePresentation, requestedDosePresentation);
          }
          else
          {
            if (requestedDosePresentation != DoseValuePresentation.Absolute)
              return DoseValue.UndefinedDose();
            // throw new ApplicationException("Only absolute dose supported for Plan Sums, please set the dose in all plans to absolute. ");

            DVHData dvh = pitem.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, volumePresentation, 0.001);
            return DvhExtensions.DoseAtVolume(dvh, volume);
          }
        }
        public static double GetVolumeAtDose(this PlanningItem pitem, Structure structure, DoseValue dose, VolumePresentation requestedVolumePresentation)
        {
          if (pitem is PlanSetup)
          {
            return ((PlanSetup)pitem).GetVolumeAtDose(structure, dose, requestedVolumePresentation);
          }
          else
          {
            DVHData dvh = pitem.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, requestedVolumePresentation, 0.001);
            return DvhExtensions.VolumeAtDose(dvh, dose.Dose);
          }
        }
        public static DoseValue DoseAtVolume(DVHData dvhData, double volume)
        {
          if (dvhData == null || dvhData.CurveData.Count() == 0)
            return DoseValue.UndefinedDose();
          double absVolume = dvhData.CurveData[0].VolumeUnit == "%" ? volume * dvhData.Volume * 0.01 : volume;
          if (volume < 0.0 || absVolume > dvhData.Volume)
            return DoseValue.UndefinedDose();

          DVHPoint[] hist = dvhData.CurveData;
          for (int i = 0; i < hist.Length; i++)
          {
            if (hist[i].Volume < volume)
              return hist[i].DoseValue;
          }
          return DoseValue.UndefinedDose();
        }
        public static double VolumeAtDose(DVHData dvhData, double dose)
        {
          if (dvhData == null)
            return Double.NaN;

          DVHPoint[] hist = dvhData.CurveData;
          int index = (int)(hist.Length * dose / dvhData.MaxDose.Dose);
          if (index < 0 || index > hist.Length)
            return 0.0;//Double.NaN;
          else
            return hist[index].Volume;
        }
        public static double GetMaxDose(this PlanningItem pitem, Structure structure, DoseValuePresentation doseValuePresentation, VolumePresentation requestedVolumePresentation)
        {
          DVHData dvhStat = pitem.GetDVHCumulativeData(structure, doseValuePresentation, requestedVolumePresentation, 0.1);
          if (dvhStat != null) return dvhStat.MaxDose.Dose;
          else return Double.NaN;
        }
        public static double GetMinDose(this PlanningItem pitem, Structure structure, DoseValuePresentation doseValuePresentation, VolumePresentation requestedVolumePresentation)
        {
          DVHData dvhStat = pitem.GetDVHCumulativeData(structure, doseValuePresentation, requestedVolumePresentation, 0.1);
          if (dvhStat != null) return dvhStat.MinDose.Dose;
          else return Double.NaN;
        }
        public static double GetMeanDose(this PlanningItem pitem, Structure structure, DoseValuePresentation doseValuePresentation, VolumePresentation requestedVolumePresentation)
        {
          DVHData dvhStat = pitem.GetDVHCumulativeData(structure, doseValuePresentation, requestedVolumePresentation, 0.001);
          if (dvhStat != null) return dvhStat.MeanDose.Dose;
          else return Double.NaN;
        }
        public static double GetCoverage(this PlanningItem pitem, Structure structure, DoseValuePresentation doseValuePresentation, VolumePresentation requestedVolumePresentation)
        {
          DVHData dvhStat = pitem.GetDVHCumulativeData(structure, doseValuePresentation, requestedVolumePresentation, 0.001);
          if (dvhStat != null) return dvhStat.Coverage;
          else return Double.NaN;
        }
        public static double VolumeOfIsodose(PlanSetup planItem, double isodose)
        {


          Dose planDose = planItem.Dose;
          int count = 0;
          double volumeDoseTotal;

          int[,] dosePlaneVoxels = new int[planDose.XSize, planDose.YSize];


          double comparisonDose = 100 * isodose;

          for (int z = 0; z < planDose.ZSize; z++)
          {
            planDose.GetVoxels(z, dosePlaneVoxels);
            for (int y = 0; y < planDose.YSize; y++)
            {
              for (int x = 0; x < planDose.XSize; x++)
              {
                DoseValue voxelDose = planDose.VoxelToDoseValue(dosePlaneVoxels[x, y]);
                if (voxelDose.Dose >= comparisonDose)
                {
                  count++;
                } // End if voxelDose >= _doseToCompare
              } // End for to cycle through x dimension
            } // End for to cycle through y dimension
          } // End for loop to cycle through z dose planes

          volumeDoseTotal = (count * planDose.XRes * planDose.YRes * planDose.ZRes);

          // convert to cubic cm
          volumeDoseTotal *= 0.1;
          volumeDoseTotal *= 0.1;
          volumeDoseTotal *= 0.1;

          return volumeDoseTotal;

        }

        public static double getDoseAtVolume(DVHData dvh, double VolumeLim)
        {
            for (int i = 0; i < dvh.CurveData.Length; i++)
            {
                if (dvh.CurveData[i].Volume <= VolumeLim)
                {
                    return dvh.CurveData[i].DoseValue.Dose;
                }
            }
            return 0;
        }
        public static double getVolumeAtDose(DVHData dvh, double DoseLim)
        {
            for (int i = 0; i < dvh.CurveData.Length; i++)
            {
                if (dvh.CurveData[i].DoseValue.Dose >= DoseLim)
                {
                    return dvh.CurveData[i].Volume;

                }
            }
            return 0;
        }
        public static double getMaxDose(DVHData dvh)
        {
            return dvh.MaxDose.Dose;
        }
        public static int getTotalFractionsForPlanSum(PlanSum planSum)
        {
            int totalFractions = 0;
            var sums = planSum.PlanSetups.GetEnumerator();
            while (sums.MoveNext())
            {
                totalFractions += (int)sums.Current.NumberOfFractions;
            }
            return totalFractions;
        }

        public static void LimitsFromVolume(double volume, out double limit1, out double limit2, out double limit3, out double limit4)
        {
          // larger volume last in the table
          limit1 = 5.9;
          limit2 = 7.5;
          limit3 = 50;
          limit4 = 57;

          if ((volume >= 1.8) && (volume < 3.8))
          {
            limit1 = 5.9 + (volume - 1.8) * (5.5 - 5.9) / (3.8 - 1.8);
            limit2 = 7.5 + (volume - 1.8) * (6.5 - 7.5) / (3.8 - 1.8);
            limit3 = 50 + (volume - 1.8) * (50 - 50) / (3.8 - 1.8);
            limit4 = 57 + (volume - 1.8) * (57 - 57) / (3.8 - 1.8);
          }

          if ((volume >= 3.8) && (volume < 7.4))
          {
            limit1 = 5.5 + (volume - 3.8) * (5.1 - 5.5) / (7.4 - 3.8);
            limit2 = 6.5 + (volume - 3.8) * (6.0 - 6.5) / (7.4 - 3.8);
            limit3 = 50 + (volume - 3.8) * (50 - 50) / (7.4 - 3.8);
            limit4 = 57 + (volume - 3.8) * (58 - 57) / (7.4 - 3.8);
          }

          if ((volume >= 7.4) && (volume < 13.2))
          {
            limit1 = 5.1 + (volume - 7.4) * (4.7 - 5.1) / (13.2 - 7.4);
            limit2 = 6.0 + (volume - 7.4) * (5.8 - 6.0) / (13.2 - 7.4);
            limit3 = 50 + (volume - 7.4) * (54 - 50) / (13.2 - 7.4);
            limit4 = 58 + (volume - 7.4) * (58 - 58) / (13.2 - 7.4); ;
          }

          if ((volume > 13.2) && (volume < 22.0))
          {
            limit1 = 4.7 + (volume - 13.2) * (4.5 - 4.7) / (22.0 - 13.2);
            limit2 = 5.8 + (volume - 13.2) * (5.5 - 5.8) / (22.0 - 13.2);
            limit3 = 50 + (volume - 13.2) * (54 - 50) / (22.0 - 13.2);
            limit4 = 58 + (volume - 13.2) * (63 - 58) / (22.0 - 13.2);
          }

          if ((volume > 22.0) && (volume < 34.0))
          {
            limit1 = 4.5 + (volume - 22.0) * (4.3 - 4.5) / (34.0 - 22.0);
            limit2 = 5.5 + (volume - 22.0) * (5.3 - 5.5) / (34.0 - 22.0);
            limit3 = 54 + (volume - 22.0) * (58 - 54) / (34.0 - 22.0);
            limit4 = 63 + (volume - 22.0) * (68 - 63) / (34.0 - 22.0);
          }

          if ((volume > 34.0) && (volume < 50.0))
          {
            limit1 = 4.3 + (volume - 34.0) * (4.0 - 4.3) / (50.0 - 34.0);
            limit2 = 5.3 + (volume - 34.0) * (5.0 - 5.3) / (50.0 - 34.0);
            limit3 = 58 + (volume - 34.0) * (62 - 58) / (50.0 - 34.0);
            limit4 = 68 + (volume - 34.0) * (77 - 68) / (50.0 - 34.0);
          }

          if ((volume > 50.0) && (volume < 70.0))
          {
            limit1 = 4.0 + (volume - 50.0) * (3.5 - 4.0) / (70.0 - 50.0);
            limit2 = 5.0 + (volume - 50.0) * (4.8 - 5.0) / (70.0 - 50.0);
            limit3 = 62 + (volume - 50.0) * (66 - 62) / (70.0 - 50.0);
            limit4 = 77 + (volume - 50.0) * (86 - 77) / (70.0 - 50.0);
          }

          if ((volume > 70.0) && (volume < 95.0))
          {
            limit1 = 3.5 + (volume - 70.0) * (3.3 - 3.5) / (95.0 - 70.0);
            limit2 = 4.8 + (volume - 70.0) * (4.4 - 4.8) / (95.0 - 70.0);
            limit3 = 66 + (volume - 70.0) * (70 - 66) / (95.0 - 70.0);
            limit4 = 86 + (volume - 70.0) * (89 - 86) / (95.0 - 70.0);
          }

          if ((volume > 95.0) && (volume < 126.0))
          {
            limit1 = 3.3 + (volume - 95.0) * (3.1 - 3.3) / (126.0 - 95.0);
            limit2 = 4.4 + (volume - 95.0) * (4.0 - 4.4) / (126.0 - 95.0);
            limit3 = 70 + (volume - 95.0) * (73 - 70) / (126.0 - 95.0);
            limit4 = 89 + (volume - 95.0) * (91 - 89) / (126.0 - 95.0);
          }

          if ((volume > 126.0) && (volume < 163.0))
          {
            limit1 = 3.1 + (volume - 126.0) * (2.9 - 3.1) / (163.0 - 126.0);
            limit2 = 4.0 + (volume - 126.0) * (3.7 - 4.0) / (163.0 - 126.0);
            limit3 = 73 + (volume - 126.0) * (77 - 73) / (163.0 - 126.0);
            limit4 = 91 + (volume - 126.0) * (94 - 91) / (163.0 - 126.0);
          }

          if ((volume > 163.0))
          {
            limit1 = 2.9;
            limit2 = 3.7;
            limit3 = 77;
            limit4 = 94;
          }
        }

    }
}
