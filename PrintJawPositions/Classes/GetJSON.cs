namespace VMS.TPS
{
    using VMS.TPS.Common.Model.API;
    using VMS.TPS.Common.Model.Types;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Collections;

    public class GetJSON
    {
        // proximity stats json
        //public static string getProximityStatsJsonCurrentPlan(PlanSetup currentPlan, IEnumerable<Structure> sorted_targetList, IEnumerable<Structure> sorted_structureList)
        //{
        //    string currentPlanJsonString = "{\"planId\":\"" + currentPlan.Id + "\"," +
        //                                        "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
        //                                        "\"targetData\":[";
        //    foreach (var t in sorted_targetList)
        //    {
        //        currentPlanJsonString = currentPlanJsonString + "{\"targetId\":\"" + t.Id.ToString().Split(':').First() + "\"," +
        //                                                            "\"targetVolume\":\"" + (Math.Round(t.Volume, 3).ToString() + "\"," +
        //                                                            "\"isHighResolution\":\"" + t.IsHighResolution + "\"," +
        //                                                            "\"segments\":\"" + t.GetNumberOfSeparateParts().ToString()) + "\"," +
        //                                                            "\"proximityStats\":[";

        //        foreach (var s in sorted_structureList)
        //        {
        //            DVHData dvhAR = currentPlan.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
        //            if (dvhAR != null && dvhAR.CurveData.Length > 0)
        //            {
        //                string color = "#" + s.Color.ToString().Substring(3, 6);
        //                currentPlanJsonString = currentPlanJsonString + "{\"structureId\":\"" + s.Id + "\"," +
        //                                                            "\"color\":\"" + color + "\"," +
        //                                                            "\"volume\":" + Math.Round(s.Volume, 3) + "," +
        //                                                            "\"distanceFromTarget\":" + Math.Round(dvhAR.MeanDose.Dose, 3) + "," +
        //                                                            "\"volumeOverlap\":" + Math.Round(dvhAR.MaxDose.Dose, 3) + "," +
        //                                                            "\"percentOverlap\":" + Math.Round(dvhAR.MaxDose.Dose, 3) + "," +
        //                                                            "\"dvh\":[";
        //                for (double i = 0; i <= dvhAR.MaxDose.Dose; i += .1)
        //                {
        //                    currentPlanJsonString = currentPlanJsonString + "[" + Math.Round(i, 2).ToString() + "," + Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString() + "],";
        //                }
        //                currentPlanJsonString = currentPlanJsonString.TrimEnd(',');
        //                currentPlanJsonString = currentPlanJsonString + "]},";
        //            }
        //        }
        //    }
        //    //currentPlanJsonString = currentPlanJsonString + "]},";
        //    currentPlanJsonString = currentPlanJsonString.TrimEnd(',');
        //    currentPlanJsonString = currentPlanJsonString + "]}]}";
        //    return currentPlanJsonString;
        //}


        public static string getDvhJsonCurrentPlan(PlanSetup currentPlan, IEnumerable<Structure> sorted_structureList)
        {
            currentPlan.DoseValuePresentation = DoseValuePresentation.Absolute;

            double planMaxDose = 0;
            if (currentPlan.Dose != null)
            {
                planMaxDose = Math.Round(currentPlan.Dose.DoseMax3D.Dose, 3);
            }
            else { planMaxDose = Double.NaN; }

            string currentPlanJsonString = "{\"planId\":\"" + currentPlan.Id + "\"," +
                                                "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
                                                "\"planMaxDose\":" + planMaxDose + "," +
                                                "\"structureData\":[";
            foreach (var s in sorted_structureList)
            {
                DVHData dvhAR = currentPlan.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
                if (dvhAR != null && dvhAR.CurveData.Length > 0)
                {
                    string color = "#" + s.Color.ToString().Substring(3, 6);
                    currentPlanJsonString = currentPlanJsonString + "{\"structureId\":\"" + s.Id + "\"," +
                                                                "\"color\":\"" + color + "\"," +
                                                                "\"volume\":" + Math.Round(s.Volume, 3) + "," +
                                                                "\"meanDose\":" + Math.Round(dvhAR.MeanDose.Dose, 3) + "," +
                                                                "\"maxDose\":" + Math.Round(dvhAR.MaxDose.Dose, 3) + "," +
                                                                "\"dvh\":[";
                    for (double i = 0; i <= dvhAR.MaxDose.Dose; i += .1)
                    {
                        currentPlanJsonString = currentPlanJsonString + "[" + Math.Round(i, 2).ToString() + "," + Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString() + "],";
                    }
                    currentPlanJsonString = currentPlanJsonString.TrimEnd(',');
                    currentPlanJsonString = currentPlanJsonString + "]},";
                }
            }
            //currentPlanJsonString = currentPlanJsonString + "]},";
            currentPlanJsonString = currentPlanJsonString.TrimEnd(',');
            currentPlanJsonString = currentPlanJsonString + "]}]}";
            return currentPlanJsonString;
        }
        public static string getDvhJsonAllPlans(IEnumerator plans)
        {
            string allPlansDvhString = "";
            while (plans.MoveNext())
            {
                PlanSetup currentPlan = (PlanSetup)plans.Current;
                currentPlan.DoseValuePresentation = DoseValuePresentation.Absolute;

                #region organize structures into ordered lists
                // lists for structures
                List<Structure> zgtvList = new List<Structure>();
                List<Structure> zctvList = new List<Structure>();
                List<Structure> zitvList = new List<Structure>();
                List<Structure> zptvList = new List<Structure>();
                List<Structure> zoarList = new List<Structure>();
                List<Structure> ztargetList = new List<Structure>();
                List<Structure> zstructureList = new List<Structure>();
                IEnumerable<Structure> zsorted_gtvList;
                IEnumerable<Structure> zsorted_ctvList;
                IEnumerable<Structure> zsorted_itvList;
                IEnumerable<Structure> zsorted_ptvList;
                IEnumerable<Structure> zsorted_targetList;
                IEnumerable<Structure> zsorted_oarList;
                IEnumerable<Structure> zsorted_structureList;

                foreach (var structure in currentPlan.StructureSet.Structures)
                {
                    // conditions for adding any structure
                    if ((!structure.IsEmpty) &&
                        (structure.HasSegment) &&
                        (!structure.Id.Contains("*")) &&
                        (!structure.Id.ToLower().Contains("markers")) &&
                        (!structure.Id.ToLower().Contains("avoid")) &&
                        (!structure.Id.ToLower().Contains("dose")) &&
                        (!structure.Id.ToLower().Contains("contrast")) &&
                        (!structure.Id.ToLower().Contains("air")) &&
                        (!structure.Id.ToLower().Contains("dens")) &&
                        (!structure.Id.ToLower().Contains("bolus")) &&
                        (!structure.Id.ToLower().Contains("suv")) &&
                        (!structure.Id.ToLower().Contains("match")) &&
                        (!structure.Id.ToLower().Contains("wire")) &&
                        (!structure.Id.ToLower().Contains("scar")) &&
                        (!structure.Id.ToLower().Contains("chemo")) &&
                        (!structure.Id.ToLower().Contains("pet")) &&
                        (!structure.Id.ToLower().Contains("dnu")) &&
                        (!structure.Id.ToLower().Contains("fiducial")) &&
                        (!structure.Id.ToLower().Contains("artifact")) &&
                        (!structure.Id.ToLower().Contains("ci-")) &&
                        (!structure.Id.ToLower().Contains("ci_")) &&
                        (!structure.Id.ToLower().Contains("r50")) &&
                        (!structure.Id.StartsWith("z", StringComparison.InvariantCultureIgnoreCase)) &&
                        (!structure.Id.StartsWith("hs", StringComparison.InvariantCultureIgnoreCase)) &&
                        (!structure.Id.StartsWith("av", StringComparison.InvariantCultureIgnoreCase)) &&
                        (!structure.Id.StartsWith("opti ", StringComparison.InvariantCultureIgnoreCase)) &&
                        (!structure.Id.StartsWith("opti-", StringComparison.InvariantCultureIgnoreCase)))
                    //(structure.Id.Contains("CI-", StringComparison.InvariantCultureIgnoreCase) == false) && 
                    //(structure.Id.Contains("R50-", StringComparison.InvariantCultureIgnoreCase) == false) &&
                    //(structure.Id.Contains("CI_", StringComparison.InvariantCultureIgnoreCase) == false) && 
                    //(structure.Id.Contains("R50_", StringComparison.InvariantCultureIgnoreCase) == false))
                    {
                        if (structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase))
                        {
                            zgtvList.Add(structure);
                            zstructureList.Add(structure);
                            ztargetList.Add(structure);
                        }
                        if ((structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) ||
                            (structure.Id.StartsWith("Prost", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            zctvList.Add(structure);
                            zstructureList.Add(structure);
                            ztargetList.Add(structure);
                        }
                        if (structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase))
                        {
                            zitvList.Add(structure);
                            zstructureList.Add(structure);
                            ztargetList.Add(structure);
                        }
                        if (structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase))
                        {
                            zptvList.Add(structure);
                            zstructureList.Add(structure);
                            ztargetList.Add(structure);
                        }
                        // conditions for adding breast plan targets
                        if ((structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) ||
                            (structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) ||
                            (structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) ||
                            (structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            ztargetList.Add(structure);
                            zstructureList.Add(structure);
                        }
                        // conditions for adding oars
                        if ((!structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.StartsWith("Scar", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.ToLower().Contains("carina")))
                        {
                            zoarList.Add(structure);
                            zstructureList.Add(structure);
                        }
                    }
                }
                zsorted_gtvList = zgtvList.OrderBy(x => x.Id);
                zsorted_ctvList = zctvList.OrderBy(x => x.Id);
                zsorted_itvList = zitvList.OrderBy(x => x.Id);
                zsorted_ptvList = zptvList.OrderBy(x => x.Id);
                zsorted_targetList = ztargetList.OrderBy(x => x.Id);
                zsorted_oarList = zoarList.OrderBy(x => x.Id);
                zsorted_structureList = zstructureList.OrderBy(x => x.Id);

                #endregion structure organization and ordering

                double planMaxDose = 0;
                if (currentPlan.Dose != null)
                {
                    planMaxDose = Math.Round(currentPlan.Dose.DoseMax3D.Dose, 3);
                }
                else { planMaxDose = Double.NaN; }

                allPlansDvhString = allPlansDvhString + "{\"planId\":\"" + currentPlan.Id + "\"," +
                                                            "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
                                                            "\"planMaxDose\":" + planMaxDose + "," +
                                                            "\"structureData\":[";
                foreach (var s in zsorted_structureList)
                {
                    //allPlansDvhString = allPlansDvhString + "]},";
                    DVHData dvhAR = currentPlan.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
                    if (dvhAR != null && dvhAR.CurveData.Length > 0)
                    {
                        string color = "#" + s.Color.ToString().Substring(3, 6);
                        allPlansDvhString = allPlansDvhString + "{\"structureId\":\"" + s.Id + "\"," +
                                                                    "\"color\":\"" + color + "\"," +
                                                                    "\"volume\":" + Math.Round(s.Volume, 3) + "," +
                                                                    "\"meanDose\":" + Math.Round(dvhAR.MeanDose.Dose, 3) + "," +
                                                                    "\"maxDose\":" + Math.Round(dvhAR.MaxDose.Dose, 3) + "," +
                                                                    "\"dvh\":[";
                        for (double i = 0; i <= dvhAR.MaxDose.Dose; i += .1)
                        {
                            allPlansDvhString = allPlansDvhString + "[" + Math.Round(i, 2).ToString() + "," + Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString() + "],";
                        }
                        allPlansDvhString = allPlansDvhString.TrimEnd(',');
                        allPlansDvhString = allPlansDvhString + "]},";
                    }
                }
                allPlansDvhString = allPlansDvhString.TrimEnd(',');
                allPlansDvhString = allPlansDvhString + "]},";
            }
            allPlansDvhString = allPlansDvhString.TrimEnd(',');
            allPlansDvhString = allPlansDvhString + "]}";
            return allPlansDvhString;
        }
        public static string getDvhJsonCurrentPlanSum(PlanSum currentPlanSum, IEnumerable<Structure> sorted_structureList)
        {
            double sumMaxDose = 0;
            if (currentPlanSum.Dose != null)
            {
                sumMaxDose = Math.Round(currentPlanSum.Dose.DoseMax3D.Dose, 3);
            }
            else { sumMaxDose = Double.NaN; }

            string currentPlanSumJsonString = "{\"planId\":\"" + currentPlanSum.Id + "\"," +
                                                "\"approvalStatus\":\"" + "PlanSum" + "\"," +
                                                "\"planMaxDose\":" + sumMaxDose + "," +
                                                "\"structureData\":[";
            foreach (var s in sorted_structureList)
            {
                DVHData dvhAR = currentPlanSum.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
                if (dvhAR != null && dvhAR.CurveData.Length > 0)
                {
                    string color = "#" + s.Color.ToString().Substring(3, 6);
                    currentPlanSumJsonString = currentPlanSumJsonString + "{\"structureId\":\"" + s.Id + "\"," +
                                                                            "\"color\":\"" + color + "\"," +
                                                                            "\"volume\":" + Math.Round(s.Volume, 3) + "," +
                                                                            "\"meanDose\":" + Math.Round(dvhAR.MeanDose.Dose, 3) + "," +
                                                                            "\"maxDose\":" + Math.Round(dvhAR.MaxDose.Dose, 3) + "," +
                                                                            "\"dvh\":[";
                    for (double i = 0; i <= dvhAR.MaxDose.Dose; i += .1)
                    {
                        currentPlanSumJsonString = currentPlanSumJsonString + "[" + Math.Round(i, 2).ToString() + "," + Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString() + "],";
                    }
                    currentPlanSumJsonString = currentPlanSumJsonString.TrimEnd(',');
                    currentPlanSumJsonString = currentPlanSumJsonString + "]},";
                }
            }
            //currentPlanSumJsonString = currentPlanSumJsonString + "]},";
            currentPlanSumJsonString = currentPlanSumJsonString.TrimEnd(',');
            currentPlanSumJsonString = currentPlanSumJsonString + "]}]}";
            return currentPlanSumJsonString;
        }
        public static string getDvhJsonAllPlanSums(IEnumerator sums)
        {
            string allPlanSumsDvhString = "";
            while (sums.MoveNext())
            {
                PlanSum currentPlanSum = (PlanSum)sums.Current;

                #region organize structures into ordered lists
                // lists for structures
                List<Structure> zgtvList = new List<Structure>();
                List<Structure> zctvList = new List<Structure>();
                List<Structure> zitvList = new List<Structure>();
                List<Structure> zptvList = new List<Structure>();
                List<Structure> zoarList = new List<Structure>();
                List<Structure> ztargetList = new List<Structure>();
                List<Structure> zstructureList = new List<Structure>();
                IEnumerable<Structure> zsorted_gtvList;
                IEnumerable<Structure> zsorted_ctvList;
                IEnumerable<Structure> zsorted_itvList;
                IEnumerable<Structure> zsorted_ptvList;
                IEnumerable<Structure> zsorted_targetList;
                IEnumerable<Structure> zsorted_oarList;
                IEnumerable<Structure> zsorted_structureList;

                foreach (var structure in currentPlanSum.StructureSet.Structures)
                {
                    // conditions for adding any structure
                    if ((!structure.IsEmpty) &&
                        (structure.HasSegment) &&
                        (!structure.Id.Contains("*")) &&
                        (!structure.Id.ToLower().Contains("markers")) &&
                        (!structure.Id.ToLower().Contains("avoid")) &&
                        (!structure.Id.ToLower().Contains("dose")) &&
                        (!structure.Id.ToLower().Contains("contrast")) &&
                        (!structure.Id.ToLower().Contains("air")) &&
                        (!structure.Id.ToLower().Contains("dens")) &&
                        (!structure.Id.ToLower().Contains("bolus")) &&
                        (!structure.Id.ToLower().Contains("suv")) &&
                        (!structure.Id.ToLower().Contains("match")) &&
                        (!structure.Id.ToLower().Contains("wire")) &&
                        (!structure.Id.ToLower().Contains("scar")) &&
                        (!structure.Id.ToLower().Contains("chemo")) &&
                        (!structure.Id.ToLower().Contains("pet")) &&
                        (!structure.Id.ToLower().Contains("dnu")) &&
                        (!structure.Id.ToLower().Contains("fiducial")) &&
                        (!structure.Id.ToLower().Contains("artifact")) &&
                        (!structure.Id.ToLower().Contains("ci-")) &&
                        (!structure.Id.ToLower().Contains("ci_")) &&
                        (!structure.Id.ToLower().Contains("r50")) &&
                        (!structure.Id.StartsWith("z", StringComparison.InvariantCultureIgnoreCase)) &&
                        (!structure.Id.StartsWith("hs", StringComparison.InvariantCultureIgnoreCase)) &&
                        (!structure.Id.StartsWith("av", StringComparison.InvariantCultureIgnoreCase)) &&
                        (!structure.Id.StartsWith("opti ", StringComparison.InvariantCultureIgnoreCase)) &&
                        (!structure.Id.StartsWith("opti-", StringComparison.InvariantCultureIgnoreCase)))
                    //(structure.Id.Contains("CI-", StringComparison.InvariantCultureIgnoreCase) == false) && 
                    //(structure.Id.Contains("R50-", StringComparison.InvariantCultureIgnoreCase) == false) &&
                    //(structure.Id.Contains("CI_", StringComparison.InvariantCultureIgnoreCase) == false) && 
                    //(structure.Id.Contains("R50_", StringComparison.InvariantCultureIgnoreCase) == false))
                    {
                        if (structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase))
                        {
                            zgtvList.Add(structure);
                            zstructureList.Add(structure);
                            ztargetList.Add(structure);
                        }
                        if ((structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) ||
                            (structure.Id.StartsWith("Prost", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            zctvList.Add(structure);
                            zstructureList.Add(structure);
                            ztargetList.Add(structure);
                        }
                        if (structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase))
                        {
                            zitvList.Add(structure);
                            zstructureList.Add(structure);
                            ztargetList.Add(structure);
                        }
                        if (structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase))
                        {
                            zptvList.Add(structure);
                            zstructureList.Add(structure);
                            ztargetList.Add(structure);
                        }
                        // conditions for adding breast plan targets
                        if ((structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) ||
                            (structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) ||
                            (structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) ||
                            (structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            ztargetList.Add(structure);
                            zstructureList.Add(structure);
                        }
                        // conditions for adding oars
                        if ((!structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.StartsWith("Scar", StringComparison.InvariantCultureIgnoreCase)) &&
                            (!structure.Id.ToLower().Contains("carina")))
                        {
                            zoarList.Add(structure);
                            zstructureList.Add(structure);
                        }
                    }
                }
                zsorted_gtvList = zgtvList.OrderBy(x => x.Id);
                zsorted_ctvList = zctvList.OrderBy(x => x.Id);
                zsorted_itvList = zitvList.OrderBy(x => x.Id);
                zsorted_ptvList = zptvList.OrderBy(x => x.Id);
                zsorted_targetList = ztargetList.OrderBy(x => x.Id);
                zsorted_oarList = zoarList.OrderBy(x => x.Id);
                zsorted_structureList = zstructureList.OrderBy(x => x.Id);

                #endregion structure organization and ordering

                double sumMaxDose = 0;
                if (currentPlanSum.Dose != null)
                {
                    sumMaxDose = Math.Round(currentPlanSum.Dose.DoseMax3D.Dose, 3);
                }
                else { sumMaxDose = Double.NaN; }

                allPlanSumsDvhString = allPlanSumsDvhString + "{\"planId\":\"" + currentPlanSum.Id + "\"," +
                                                                "\"approvalStatus\":\"" + "PlanSum" + "\"," +
                                                                "\"planMaxDose\":" + sumMaxDose + "," +
                                                                "\"structureData\":[";
                foreach (var s in zsorted_structureList)
                {
                    DVHData dvhAR = currentPlanSum.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
                    if (dvhAR != null && dvhAR.CurveData.Length > 0)
                    {
                        string color = "#" + s.Color.ToString().Substring(3, 6);
                        allPlanSumsDvhString = allPlanSumsDvhString + "{\"structureId\":\"" + s.Id + "\"," +
                                                                        "\"color\":\"" + color + "\"," +
                                                                        "\"volume\":" + Math.Round(s.Volume, 3) + "," +
                                                                        "\"meanDose\":" + Math.Round(dvhAR.MeanDose.Dose, 3) + "," +
                                                                        "\"maxDose\":" + Math.Round(dvhAR.MaxDose.Dose, 3) + "," +
                                                                        "\"dvh\":[";
                        for (double i = 0; i <= dvhAR.MaxDose.Dose; i += .1)
                        {
                            allPlanSumsDvhString = allPlanSumsDvhString + "[" + Math.Round(i, 2).ToString() + "," + Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString() + "],";
                        }
                        allPlanSumsDvhString = allPlanSumsDvhString.TrimEnd(',');
                        allPlanSumsDvhString = allPlanSumsDvhString + "]},";
                    }
                }
                allPlanSumsDvhString = allPlanSumsDvhString.TrimEnd(',');
                allPlanSumsDvhString = allPlanSumsDvhString + "]},";
            }
            allPlanSumsDvhString = allPlanSumsDvhString.TrimEnd(',');
            allPlanSumsDvhString = allPlanSumsDvhString + "]}";
            return allPlanSumsDvhString;
        }
    }
}
