namespace VMS.TPS
{
    using System;
    using System.Collections.Generic;
    using VMS.TPS.Common.Model.API;
    class GetDvh
    {
        public static void getDosePoints(DVHData dynamicDvh_01, double doseResolution, out List<double> doseResolutionList)
        {
            List<double> doseList = new List<double>();
            for (double i = 0; i < dynamicDvh_01.CurveData.Length - 1; i += doseResolution)
            {
                doseList.Add(i);
            }
            doseResolutionList = doseList;
        }
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
