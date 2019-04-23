
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Xml;
using System.Xml.Linq;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using System.Windows.Media.Media3D;


namespace VMS.TPS
{
    public class ExtractDoseStatsScript
    {
        public ExtractDoseStatsScript()
        {
        }
        public void Execute(ScriptContext context /*, System.Windows.Window window*/)
        {
            if (context.PlanSumsInScope != null || context.PlanSetup != null)
            {
                //Retrieve the count of plans displayed in Scope Window
                int scopePlanCount = context.PlansInScope.Count();
                int scopePlanSumsCount = context.PlanSumsInScope.Count();
                if ((scopePlanCount == 0) && (scopePlanSumsCount == 0))
                    throw new ApplicationException("Please open a plan first.");

                StructureSet structureSet;
                PlanningItem SelectedPlanningItem;
                PlanSetup plan;
                if (context.PlanSetup == null)
                {
                    PlanSum psum = context.PlanSumsInScope.First();
                    plan = psum.PlanSetups.First();
                    SelectedPlanningItem = (PlanningItem)psum;
                    structureSet = plan.StructureSet;
                }
                else
                {
                    plan = context.PlanSetup;
                    SelectedPlanningItem = (PlanningItem)plan;
                    structureSet = plan.StructureSet;
                }

                string curredLastName = context.Patient.LastName.Replace(" ", "_");
                string curredFirstName = context.Patient.FirstName.Replace(" ", "_");
                string firstInitial = context.Patient.FirstName[0].ToString();
                string lastInitial = context.Patient.LastName[0].ToString();
                string initials = firstInitial + lastInitial;
                string id = context.Patient.Id;
                string courseName = context.Course.Id;
                string planName = SelectedPlanningItem.Id;

                // ask to close other plansums since can only take first/firstordefault plansum
                if (context.PlanSetup == null && context.PlanSumsInScope.Count() > 1)
                {
                    throw new ApplicationException("Please close other plan sums");
                }

                // this will cause the script to close if there is no dose calculated
                if (SelectedPlanningItem.Dose == null)
                {
                    throw new ApplicationException("Dose has not been calculated.");
                }

                //StringBuilder csvIndividualContent = new StringBuilder();
                StringBuilder csvOarContent = new StringBuilder();
                StringBuilder csvTargetContent = new StringBuilder();

                //string csvIndividualReportPath = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\" + initials + "_" + id + "_" + planName + "_DoseStats.csv";
                string csvOarPath = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\OAR_DoseStats.csv";
                string csvTargetPath = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\Target_DoseStats.csv";

                if (plan is ExternalPlanSetup)
                {
                    // StructureSet ?
                    //StructureSet structureSet = plan.StructureSet;
                    //if (structureSet == null)
                    //    throw new ApplicationException("The selected plan does not have a structure set.");

                    // lists for structures
                    List<Structure> gtvList = new List<Structure>();
                    List<Structure> ctvList = new List<Structure>();
                    List<Structure> itvList = new List<Structure>();
                    List<Structure> ptvList = new List<Structure>();
                    List<Structure> oarList = new List<Structure>();

                    // initialize items needed for calculating distance and overlap
                    double volumeIntersection = 0;
                    //double diceCoefficient = 0;
                    double shortestDistance = 0;
                    double percentOverlap = 0;

                    foreach (var structure in structureSet.Structures)
                    {
                        // conditions for adding any structure
                        if ((structure.HasSegment == true) && (structure.IsEmpty == false) &&
                            (structure.Id.Contains("z-") == false) && (structure.Id.Contains("av ") == false) &&
                            (structure.Id.Contains("avoid") == false) && (structure.Id.Contains("Dose") == false) &&
                            (structure.Id.Contains("CI") == false) && (structure.Id.Contains("R50") == false) &&
                            (structure.Id.Contains("opti") == false) && (structure.Id.Contains("*") == false) &&
                            (structure.Id.Contains("OPTI") == false) && (structure.Id.Contains("AV ") == false) &&
                            (structure.Id.Contains("AVOID") == false) && (structure.Id.Contains("AV-") == false) &&
                            (structure.Id.Contains("Z-") == false) && (structure.Id.Contains("av-") == false))
                        {
                            if (structure.Id.Contains("GTV"))
                            {
                                gtvList.Add(structure);
                            }
                            if (structure.Id.Contains("CTV"))
                            {
                                ctvList.Add(structure);
                            }
                            if (structure.Id.Contains("ITV"))
                            {
                                itvList.Add(structure);
                            }
                            if (structure.Id.Contains("PTV"))
                            {
                                ptvList.Add(structure);
                            }
                            // conditions for adding oars
                            if ((structure.Id.Contains("GTV") == false) && (structure.Id.Contains("CTV") == false) &&
                                (structure.Id.Contains("ITV") == false) && (structure.Id.Contains("PTV") == false))
                            {
                                oarList.Add(structure);
                            }
                        }
                    }
                    if ((gtvList.Count == 0) && (ctvList.Count == 0) && (itvList.Count == 0) && (ptvList.Count == 0))
                    {
                        throw new ApplicationException("The selected plan does not have a GTV, CTV, ITV, or PTV or they are not labeled with all caps.");
                    }

                    #region Target Dose Stats

                    // add headers if the file doesn't exist
                    // list of target headers for desired dose stats
                    if (!File.Exists(@"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\Target_DoseStats.csv"))
                    {
                        List<string> targetHeaderList = new List<string>();
                        targetHeaderList.Add("FirstName");
                        targetHeaderList.Add("LastName");
                        targetHeaderList.Add("ID");
                        targetHeaderList.Add("CourseName");
                        targetHeaderList.Add("PlanName");
                        targetHeaderList.Add("Target");
                        targetHeaderList.Add("Volume");
                        targetHeaderList.Add("D95%");
                        targetHeaderList.Add("Mean");
                        targetHeaderList.Add("Max");
                        targetHeaderList.Add("Max (0.03cc)");
                        targetHeaderList.Add("Min");
                        targetHeaderList.Add("Min (0.03cc)");

                        string concatTargetHeader = string.Join(",", targetHeaderList.ToArray());

                        csvTargetContent.AppendLine(concatTargetHeader);
                    }
                    foreach (var gtv in gtvList)
                    {
                        List<object> gtvStatsList = new List<object>();
                        var tempStruct = gtv.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        gtvStatsList.Add(curredFirstName);
                        gtvStatsList.Add(curredLastName);
                        gtvStatsList.Add(id);
                        gtvStatsList.Add(courseName);
                        gtvStatsList.Add(planName);
                        gtvStatsList.Add(tempStruct);

                        DVHData dvhR = SelectedPlanningItem.GetDVHCumulativeData(gtv, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
                        DVHData dvhA = SelectedPlanningItem.GetDVHCumulativeData(gtv, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

                        double volume = gtv.Volume;
                        double d95 = DvhExtensions.getDoseAtVolume(dvhR, 95.0);
                        double mean = dvhA.MeanDose.Dose;
                        double max = dvhA.MaxDose.Dose;
                        double max03 = DvhExtensions.getDoseAtVolume(dvhA, 0.03);
                        double min = dvhA.MinDose.Dose;
                        double min03 = DvhExtensions.getDoseAtVolume(dvhA, (gtv.Volume - 0.03));
                        gtvStatsList.Add(volume);
                        gtvStatsList.Add(d95);
                        gtvStatsList.Add(mean);
                        gtvStatsList.Add(max);
                        gtvStatsList.Add(max03);
                        gtvStatsList.Add(min);
                        gtvStatsList.Add(min03);

                        string concatGtvStats = string.Join(",", gtvStatsList.ToArray());

                        csvTargetContent.AppendLine(concatGtvStats);
                    }
                    foreach (var ctv in ctvList)
                    {
                        List<object> ctvStatsList = new List<object>();
                        var tempStruct = ctv.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        ctvStatsList.Add(curredFirstName);
                        ctvStatsList.Add(curredLastName);
                        ctvStatsList.Add(id);
                        ctvStatsList.Add(courseName);
                        ctvStatsList.Add(planName);
                        ctvStatsList.Add(tempStruct);

                        DVHData dvhR = SelectedPlanningItem.GetDVHCumulativeData(ctv, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
                        DVHData dvhA = SelectedPlanningItem.GetDVHCumulativeData(ctv, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

                        double volume = ctv.Volume;
                        double d95 = DvhExtensions.getDoseAtVolume(dvhR, 95.0);
                        double mean = dvhA.MeanDose.Dose;
                        double max = dvhA.MaxDose.Dose;
                        double max03 = DvhExtensions.getDoseAtVolume(dvhA, 0.03);
                        double min = dvhA.MinDose.Dose;
                        double min03 = DvhExtensions.getDoseAtVolume(dvhA, (ctv.Volume - 0.03));
                        ctvStatsList.Add(volume);
                        ctvStatsList.Add(d95);
                        ctvStatsList.Add(mean);
                        ctvStatsList.Add(max);
                        ctvStatsList.Add(max03);
                        ctvStatsList.Add(min);
                        ctvStatsList.Add(min03);

                        string concatCtvStats = string.Join(",", ctvStatsList.ToArray());

                        csvTargetContent.AppendLine(concatCtvStats);
                    }
                    foreach (var itv in itvList)
                    {
                        List<object> itvStatsList = new List<object>();
                        var tempStruct = itv.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        itvStatsList.Add(curredFirstName);
                        itvStatsList.Add(curredLastName);
                        itvStatsList.Add(id);
                        itvStatsList.Add(courseName);
                        itvStatsList.Add(planName);
                        itvStatsList.Add(tempStruct);

                        DVHData dvhR = SelectedPlanningItem.GetDVHCumulativeData(itv, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
                        DVHData dvhA = SelectedPlanningItem.GetDVHCumulativeData(itv, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

                        double volume = itv.Volume;
                        double d95 = DvhExtensions.getDoseAtVolume(dvhR, 95.0);
                        double mean = dvhA.MeanDose.Dose;
                        double max = dvhA.MaxDose.Dose;
                        double max03 = DvhExtensions.getDoseAtVolume(dvhA, 0.03);
                        double min = dvhA.MinDose.Dose;
                        double min03 = DvhExtensions.getDoseAtVolume(dvhA, (itv.Volume - 0.03));
                        itvStatsList.Add(volume);
                        itvStatsList.Add(d95);
                        itvStatsList.Add(mean);
                        itvStatsList.Add(max);
                        itvStatsList.Add(max03);
                        itvStatsList.Add(min);
                        itvStatsList.Add(min03);

                        string concatItvStats = string.Join(",", itvStatsList.ToArray());

                        csvTargetContent.AppendLine(concatItvStats);
                    }
                    foreach (var ptv in ptvList)
                    {
                        List<object> ptvStatsList = new List<object>();
                        var tempStruct = ptv.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        ptvStatsList.Add(curredFirstName);
                        ptvStatsList.Add(curredLastName);
                        ptvStatsList.Add(id);
                        ptvStatsList.Add(courseName);
                        ptvStatsList.Add(planName);
                        ptvStatsList.Add(tempStruct);

                        DVHData dvhR = SelectedPlanningItem.GetDVHCumulativeData(ptv, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
                        DVHData dvhA = SelectedPlanningItem.GetDVHCumulativeData(ptv, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

                        double volume = ptv.Volume;
                        double d95 = DvhExtensions.getDoseAtVolume(dvhR, 95.0);
                        double mean = dvhA.MeanDose.Dose;
                        double max = dvhA.MaxDose.Dose;
                        double max03 = DvhExtensions.getDoseAtVolume(dvhA, 0.03);
                        double min = dvhA.MinDose.Dose;
                        double min03 = DvhExtensions.getDoseAtVolume(dvhA, (ptv.Volume - 0.03));
                        ptvStatsList.Add(volume);
                        ptvStatsList.Add(d95);
                        ptvStatsList.Add(mean);
                        ptvStatsList.Add(max);
                        ptvStatsList.Add(max03);
                        ptvStatsList.Add(min);
                        ptvStatsList.Add(min03);

                        string concatPtvStats = string.Join(",", ptvStatsList.ToArray());

                        csvTargetContent.AppendLine(concatPtvStats);
                    }
                    #endregion Target Dose Stats

                    #region OAR Dose Stats

                    // oar header list (to separate oar dose stats from target stats)
                    if (!File.Exists(@"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\OAR_DoseStats.csv"))
                    {
                        List<string> oarHeaderList = new List<string>();
                        oarHeaderList.Add("FirstName");
                        oarHeaderList.Add("LastName");
                        oarHeaderList.Add("ID");
                        oarHeaderList.Add("CourseName");
                        oarHeaderList.Add("PlanName");
                        oarHeaderList.Add("OAR");
                        oarHeaderList.Add("Volume");
                        oarHeaderList.Add("V5");
                        oarHeaderList.Add("V10");
                        oarHeaderList.Add("V15");
                        oarHeaderList.Add("V20");
                        oarHeaderList.Add("V25");
                        oarHeaderList.Add("V30");
                        oarHeaderList.Add("V35");
                        oarHeaderList.Add("V40");
                        oarHeaderList.Add("V45");
                        oarHeaderList.Add("V50");
                        oarHeaderList.Add("V55");
                        oarHeaderList.Add("V60");
                        oarHeaderList.Add("V65");
                        oarHeaderList.Add("V70");
                        oarHeaderList.Add("V75");
                        oarHeaderList.Add("V80");
                        oarHeaderList.Add("V85");
                        oarHeaderList.Add("Mean");
                        oarHeaderList.Add("Max");
                        oarHeaderList.Add("Max (0.03cc)");
                        oarHeaderList.Add("Min");
                        oarHeaderList.Add("Min (0.03cc)");
                        foreach (var ptv in ptvList)
                        {
                            var tempStruct = ptv.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }
                            oarHeaderList.Add(tempStruct + "_Shortest Distance (cm)");
                            oarHeaderList.Add(tempStruct + "_Volume Overlap (cc)");
                            oarHeaderList.Add(tempStruct + "_% Overlap");
                        }
                        string concatOarHeader = string.Join(",", oarHeaderList.ToArray());

                        csvOarContent.AppendLine(concatOarHeader);
                    }
                    foreach (var oar in oarList)
                    {
                        List<object> oarStatList = new List<object>();
                        var tempStruct = oar.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        oarStatList.Add(curredFirstName);
                        oarStatList.Add(curredLastName);
                        oarStatList.Add(id);
                        oarStatList.Add(courseName);
                        oarStatList.Add(planName);
                        oarStatList.Add(tempStruct);

                        DVHData dvhR = SelectedPlanningItem.GetDVHCumulativeData(oar, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
                        DVHData dvhA = SelectedPlanningItem.GetDVHCumulativeData(oar, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

                        double volume = oar.Volume;
                        double v5 = DvhExtensions.getVolumeAtDose(dvhR, 5.0);
                        double v10 = DvhExtensions.getVolumeAtDose(dvhR, 10.0);
                        double v15 = DvhExtensions.getVolumeAtDose(dvhR, 15.0);
                        double v20 = DvhExtensions.getVolumeAtDose(dvhR, 20.0);
                        double v25 = DvhExtensions.getVolumeAtDose(dvhR, 25.0);
                        double v30 = DvhExtensions.getVolumeAtDose(dvhR, 30.0);
                        double v35 = DvhExtensions.getVolumeAtDose(dvhR, 35.0);
                        double v40 = DvhExtensions.getVolumeAtDose(dvhR, 40.0);
                        double v45 = DvhExtensions.getVolumeAtDose(dvhR, 45.0);
                        double v50 = DvhExtensions.getVolumeAtDose(dvhR, 50.0);
                        double v55 = DvhExtensions.getVolumeAtDose(dvhR, 55.0);
                        double v60 = DvhExtensions.getVolumeAtDose(dvhR, 60.0);
                        double v65 = DvhExtensions.getVolumeAtDose(dvhR, 65.0);
                        double v70 = DvhExtensions.getVolumeAtDose(dvhR, 70.0);
                        double v75 = DvhExtensions.getVolumeAtDose(dvhR, 75.0);
                        double v80 = DvhExtensions.getVolumeAtDose(dvhR, 80.0);
                        double v85 = DvhExtensions.getVolumeAtDose(dvhR, 85.0);
                        double mean = dvhA.MeanDose.Dose;
                        double max = dvhA.MaxDose.Dose;
                        double max03 = DvhExtensions.getDoseAtVolume(dvhA, 0.03);
                        double min = dvhA.MinDose.Dose;
                        double min03 = DvhExtensions.getDoseAtVolume(dvhA, (oar.Volume - 0.03));
                        oarStatList.Add(volume);
                        oarStatList.Add(v5);
                        oarStatList.Add(v10);
                        oarStatList.Add(v15);
                        oarStatList.Add(v20);
                        oarStatList.Add(v25);
                        oarStatList.Add(v30);
                        oarStatList.Add(v35);
                        oarStatList.Add(v40);
                        oarStatList.Add(v45);
                        oarStatList.Add(v50);
                        oarStatList.Add(v55);
                        oarStatList.Add(v60);
                        oarStatList.Add(v65);
                        oarStatList.Add(v70);
                        oarStatList.Add(v75);
                        oarStatList.Add(v80);
                        oarStatList.Add(v85);
                        oarStatList.Add(mean);
                        oarStatList.Add(max);
                        oarStatList.Add(max03);
                        oarStatList.Add(min);
                        oarStatList.Add(min03);

                        if (ptvList.Count > 0)
                        {
                            foreach (var ptv in ptvList)
                            {
                                volumeIntersection = CalculateOverlap.VolumeOverlap(ptv, oar);
                                percentOverlap = CalculateOverlap.PercentOverlap(oar, volumeIntersection);
                                shortestDistance = CalculateOverlap.ShortestDistance(ptv, oar);
                                //diceCoefficient = CalculateOverlap.DiceCoefficient(SelectedStructure, SelectedStructure2);
                                oarStatList.Add(shortestDistance);
                                oarStatList.Add(volumeIntersection);
                                oarStatList.Add(percentOverlap);
                            }
                        }
                        string concatOarStats = string.Join(",", oarStatList.ToArray());

                        csvOarContent.AppendLine(concatOarStats);
                    }
                    #endregion OAR Dose Stats

                    File.AppendAllText(csvOarPath, csvOarContent.ToString());
                    File.AppendAllText(csvTargetPath, csvTargetContent.ToString());
                }

                else // if not an external plan
                {
                    string message = string.Format("Plan {0} is not an external beam plan. ", plan.Name);
                    MessageBox.Show(message);
                }
            }
        }
    }
}
