namespace VMS.TPS
{
  using System;
  using System.Collections.Generic;
  using VMS.TPS.Common.Model.API;

  /// <summary>
  /// A collection of methods to return a list of Dose points and Volumes for a given DVH and resolution.
  /// </summary>
  class GetDvh
  {
    /// <summary>
    /// Returns the list of Dose Values for a given DVH and dose resolution
    /// </summary>
    /// <param name="dynamicDvh_01"></param>
    /// <param name="doseResolution"></param>
    /// <param name="doseResolutionList"></param>
    public static void getDosePoints(DVHData dynamicDvh_01, double doseResolution, out List<double> doseResolutionList)
    {
      List<double> doseList = new List<double>();
      for (double i = 0; i < dynamicDvh_01.CurveData.Length - 1; i += doseResolution)
      {
        doseList.Add(i);
      }
      doseResolutionList = doseList;
    }
    /// <summary>
    /// Returns a list of Volumes for a given DVH and volume resolution.
    /// </summary>
    /// <param name="dynamicDvh_01"></param>
    /// <param name="volumeResolution"></param>
    /// <param name="volumeAtDoseList"></param>
    public static void getVolumePoints(DVHData dynamicDvh_01, double volumeResolution, out List<double> volumeAtDoseList)
    {
      List<double> volumeAtDoseResolutionList = new List<double>();
      double volumeAtDose = 0;
      for (double i = 0; i < dynamicDvh_01.CurveData.Length - 1; i += volumeResolution)
      {
        volumeAtDose = DoseChecks.getVolumeAtDose(dynamicDvh_01, i);
        volumeAtDoseResolutionList.Add(Math.Round(volumeAtDose, 2));
      }
      volumeAtDoseList = volumeAtDoseResolutionList;
    }
  }
}
