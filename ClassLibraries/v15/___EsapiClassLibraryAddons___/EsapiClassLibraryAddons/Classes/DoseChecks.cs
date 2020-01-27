namespace VMS.TPS
{
  using VMS.TPS.Common.Model.API;
  using VMS.TPS.Common.Model.Types;
    
  /// <summary>
  /// Various methods for checking dose values
  /// </summary>
  public class DoseChecks
  {
    public static double getTotalFractionsForPlanSum(PlanSum planSum)
    {
      double numFractions = 0;
      var Sums = planSum.PlanSetups.GetEnumerator();
      while (Sums.MoveNext())
      {
        numFractions = numFractions + (double)Sums.Current.NumberOfFractions;
      }
      return numFractions;
    }

    public static double getMaxDose(DVHData dvh)
    {
      double maxDose = dvh.MaxDose.Dose;
      return maxDose;
    }
    public static bool checkDose(double Dose, double DoseLim)
    {
      if (Dose < DoseLim)
      {
        return true;
      }
      else
      {
        return false;
      }
    }
    public static bool checkPTVDose(double Dose, double DoseLim)
    {
      // 0.001 is added because when normalizing 100% Rx Dose to 95% Target Volume, 
      // sometimes it actually rounds to just shy of the 95%, which would cause a false negative.
      if (Dose + .001 >= DoseLim)
      {
        return true;
      }

      else
      {
        return false;
      }
    }
    public static bool checkCIvol(double volDose)
    {
      if (volDose > 0)
      {
        return true;
      }
      else
      {
        return false;
      }
    }
    public static bool checkBodyMaxDose(double bodyMaxDose)
    {
      if (bodyMaxDose > 0)
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    public static double getMeanDose(DVHData dvh)
    {

      double newVolume = 0;
      double oldVolume = 0;
      double MeanDose = 0;
      double doseDiff = 0;
      oldVolume = dvh.CurveData[0].Volume;
      for (int i = 0; i < dvh.CurveData.Length; i++)
      {
        doseDiff = (dvh.CurveData[1].DoseValue.Dose - dvh.CurveData[0].DoseValue.Dose) / 2;
        DVHPoint pt = dvh.CurveData[i];
        newVolume = pt.Volume / dvh.CurveData[0].Volume;
        MeanDose = MeanDose + pt.DoseValue.Dose * (oldVolume - newVolume);
        // MessageBox.Show("Nubmer of beams id is " + bCount + " Dose Max " + DVH.DoseMax3D + Environment.NewLine + "  DVH point " + pt.DoseValue.Dose + Environment.NewLine + " Volume " + pt.Volume / dvhd.CurveData[0].Volume * 100);
        oldVolume = newVolume;
      }
      MeanDose = MeanDose - doseDiff;
      return MeanDose;

    }

    public static bool checkMinDose(double Dose, double DoseLim)
    {

      Dose = Dose + .01;
      if (Dose >= DoseLim)
      {
        return true;
      }
      else
      {
        return false;
      }
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

    public static double getRelativeVolumeAtDose(DVHData dvh, double DoseLim)
    {
      for (int i = 0; i < dvh.CurveData.Length; i++)
      {
        double relativeVol = (dvh.CurveData[i].Volume / dvh.CurveData[0].Volume) * 100;
        if (dvh.CurveData[i].DoseValue.Dose >= DoseLim)
        {
          return relativeVol;
        }
      }
      return 0;

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

    public static double getDoseAtRelativeVolume(DVHData dvh, double VolumeLim)
    {
      for (int i = 0; i < dvh.CurveData.Length; i++)
      {
        double relativeVol = (dvh.CurveData[i].Volume / dvh.CurveData[0].Volume) * 100;
        if (relativeVol < VolumeLim)
        {
          return dvh.CurveData[i].DoseValue.Dose;
        }
      }
      return 0;

    }

    public static double getEQD2(double Dose, double Fractions, double AlphaBeta)
    {
      double doseFx = Dose / Fractions;
      double EQD2 = AlphaBeta * (doseFx + AlphaBeta) / (2 + AlphaBeta);
      return EQD2;
    }
  }
}
