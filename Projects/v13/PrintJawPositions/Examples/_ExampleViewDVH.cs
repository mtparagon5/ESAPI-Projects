using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


// Do not change namespace and class name
// otherwise Eclipse will not be able to run the script.
namespace VMS.TPS
{
    //public class ViewDVHScript
    //{
    //    public ViewDVHScript()
    //    {
    //    }

    //    public void Execute(ScriptContext context)
    //    {
    //        if (context.PlanSumsInScope != null || context.PlanSetup != null)
    //        {
    //            //---------------------------------------------------------------------------------
    //            #region context variables

    //            // to work for plan sum
    //            IEnumerator sums = null;
    //            IEnumerator plans = null; // context.PlanSumsInScope.GetEnumerator();
    //            StructureSet structureSet;
    //            PlanningItem selectedPlanningItem;
    //            PlanSetup planSetup;
    //            PlanSum psum = null;
    //            string PlanName = "";
    //            double? fractions = 0;

    //            if (context.PlanSetup == null)
    //            {
    //                psum = context.PlanSumsInScope?.First();
    //                planSetup = psum?.PlanSetups.First();
    //                selectedPlanningItem = (PlanningItem)psum;
    //                structureSet = planSetup?.StructureSet;
    //                fractions = DvhExtensions.getTotalFractionsForPlanSum(psum);
    //                PlanName = psum.Id;
    //            }
    //            else
    //            {
    //                planSetup = context.PlanSetup;
    //                selectedPlanningItem = (PlanningItem)planSetup;
    //                structureSet = planSetup?.StructureSet;
    //                if (planSetup?.UniqueFractionation.NumberOfFractions != null)
    //                {
    //                    fractions = (double)planSetup?.UniqueFractionation.NumberOfFractions;
    //                }
    //                PlanName = planSetup.Id;
    //            }
    //            var dosePerFraction = planSetup?.UniqueFractionation.PrescribedDosePerFraction.Dose;
    //            var rxDose = (double)(dosePerFraction * fractions);
    //            if (PlanName.Contains(":"))
    //            {
    //                PlanName = PlanName.Replace(":", "_");
    //            }
    //            if (context.PlanSetup == null)
    //            {
    //                sums = context.PlanSumsInScope.GetEnumerator();
    //            }
    //            else
    //            {
    //                plans = context.PlansInScope.GetEnumerator();
    //            }
    //            string pId = context.Patient.Id;
    //            double pIdAsDouble = Convert.ToDouble(pId);
    //            string randomId = Math.Ceiling(Math.Sqrt(pIdAsDouble * 5)).ToString();
    //            string pName = ProcessIdName.processPtName(context.Patient.Name);
    //            string course = context.Course.Id;

    //            #endregion
    //            //---------------------------------------------------------------------------------
    //            #region PrimaryPhysician definition

    //            string tempPhysicianId = context.Patient.PrimaryOncologistId;
    //            string PrimaryPhysician = "";
    //            //Dr. Curran
    //            //Dr. Rossi
    //            //Dr. Yu
    //            //Dr. Shelton
    //            //Dr. Hearshatter
    //            if (tempPhysicianId == "1265536635") PrimaryPhysician = "Dr. Beitler";
    //            if (tempPhysicianId == "1023301082") PrimaryPhysician = "Dr. Lin";
    //            if (tempPhysicianId == "1144408287") PrimaryPhysician = "Dr. McDonald";
    //            if (tempPhysicianId == "1306803051") PrimaryPhysician = "Dr. Esiashvili";
    //            if (tempPhysicianId == "1093721029") PrimaryPhysician = "Dr. Godette";
    //            if (tempPhysicianId == "1730353327") PrimaryPhysician = "Dr. Higgins";
    //            if (tempPhysicianId == "1346280575") PrimaryPhysician = "Dr. Jani";
    //            if (tempPhysicianId == "1487823654") PrimaryPhysician = "Dr. Khan";
    //            if (tempPhysicianId == "1659387702") PrimaryPhysician = "Dr. Landry";
    //            if (tempPhysicianId == "1750543807") PrimaryPhysician = "Dr. Patel";
    //            if (tempPhysicianId == "1952316697") PrimaryPhysician = "Dr. Shu";
    //            if (tempPhysicianId == "1326214479") PrimaryPhysician = "Dr. Torres";
    //            if (tempPhysicianId == "1861629107") PrimaryPhysician = "Dr. Eaton";

    //            #endregion
    //            //---------------------------------------------------------------------------------
    //            #region organize structures into ordered lists
    //            // lists for structures
    //            List<Structure> gtvList = new List<Structure>();
    //            List<Structure> ctvList = new List<Structure>();
    //            List<Structure> itvList = new List<Structure>();
    //            List<Structure> ptvList = new List<Structure>();
    //            List<Structure> oarList = new List<Structure>();
    //            List<Structure> targetList = new List<Structure>();
    //            List<Structure> structureList = new List<Structure>();
    //            IEnumerable<Structure> sorted_gtvList;
    //            IEnumerable<Structure> sorted_ctvList;
    //            IEnumerable<Structure> sorted_itvList;
    //            IEnumerable<Structure> sorted_ptvList;
    //            IEnumerable<Structure> sorted_targetList;
    //            IEnumerable<Structure> sorted_oarList;
    //            IEnumerable<Structure> sorted_structureList;

    //            foreach (var structure in structureSet?.Structures)
    //            {
    //                // conditions for adding any structure
    //                if ((!structure.IsEmpty) &&
    //                    (structure.HasSegment) &&
    //                    (!structure.Id.Contains("*")) &&
    //                    (!structure.Id.ToLower().Contains("markers")) &&
    //                    (!structure.Id.ToLower().Contains("avoid")) &&
    //                    (!structure.Id.ToLower().Contains("dose")) &&
    //                    (!structure.Id.ToLower().Contains("contrast")) &&
    //                    (!structure.Id.ToLower().Contains("air")) &&
    //                    (!structure.Id.ToLower().Contains("dens")) &&
    //                    (!structure.Id.ToLower().Contains("bolus")) &&
    //                    (!structure.Id.ToLower().Contains("suv")) &&
    //                    (!structure.Id.ToLower().Contains("match")) &&
    //                    (!structure.Id.ToLower().Contains("wire")) &&
    //                    (!structure.Id.ToLower().Contains("scar")) &&
    //                    (!structure.Id.ToLower().Contains("chemo")) &&
    //                    (!structure.Id.ToLower().Contains("pet")) &&
    //                    (!structure.Id.ToLower().Contains("dnu")) &&
    //                    (!structure.Id.ToLower().Contains("fiducial")) &&
    //                    (!structure.Id.ToLower().Contains("artifcavt")) &&
    //                    (!structure.Id.ToLower().Contains("ci-")) &&
    //                    (!structure.Id.ToLower().Contains("ci_")) &&
    //                    (!structure.Id.ToLower().Contains("r50")) &&
    //                    (!structure.Id.StartsWith("z", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("av", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("opti ", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("opti-", StringComparison.InvariantCultureIgnoreCase)))
    //                //(structure.Id.Contains("CI-", StringComparison.InvariantCultureIgnoreCase) == false) && 
    //                //(structure.Id.Contains("R50-", StringComparison.InvariantCultureIgnoreCase) == false) &&
    //                //(structure.Id.Contains("CI_", StringComparison.InvariantCultureIgnoreCase) == false) && 
    //                //(structure.Id.Contains("R50_", StringComparison.InvariantCultureIgnoreCase) == false))
    //                {
    //                    if (structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase))
    //                    {
    //                        gtvList.Add(structure);
    //                        structureList.Add(structure);
    //                        targetList.Add(structure);
    //                    }
    //                    if ((structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) ||
    //                        (structure.Id.StartsWith("Prost", StringComparison.InvariantCultureIgnoreCase)))
    //                    {
    //                        ctvList.Add(structure);
    //                        structureList.Add(structure);
    //                        targetList.Add(structure);
    //                    }
    //                    if (structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase))
    //                    {
    //                        itvList.Add(structure);
    //                        structureList.Add(structure);
    //                        targetList.Add(structure);
    //                    }
    //                    if (structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase))
    //                    {
    //                        ptvList.Add(structure);
    //                        structureList.Add(structure);
    //                        targetList.Add(structure);
    //                    }
    //                    // conditions for adding breast plan targets
    //                    if ((structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) ||
    //                        (structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) ||
    //                        (structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) ||
    //                        (structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)))
    //                    {
    //                        targetList.Add(structure);
    //                        structureList.Add(structure);
    //                    }
    //                    // conditions for adding oars
    //                    if ((!structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase)) &&
    //                        (!structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) &&
    //                        (!structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase)) &&
    //                        (!structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase)) &&
    //                        (!structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) &&
    //                        (!structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) &&
    //                        (!structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) &&
    //                        (!structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)) &&
    //                        (!structure.Id.StartsWith("Scar", StringComparison.InvariantCultureIgnoreCase)) &&
    //                        (!structure.Id.ToLower().Contains("carina")))
    //                    {
    //                        oarList.Add(structure);
    //                        structureList.Add(structure);
    //                    }
    //                }
    //            }
    //            sorted_gtvList = gtvList?.OrderBy(x => x.Id);
    //            sorted_ctvList = ctvList?.OrderBy(x => x.Id);
    //            sorted_itvList = itvList?.OrderBy(x => x.Id);
    //            sorted_ptvList = ptvList?.OrderBy(x => x.Id);
    //            sorted_targetList = targetList?.OrderBy(x => x.Id);
    //            sorted_oarList = oarList?.OrderBy(x => x.Id);
    //            sorted_structureList = structureList?.OrderBy(x => x.Id);

    //            #endregion structure organization and ordering
    //            //---------------------------------------------------------------------------------
    //            #region json

    //            #region directories

    //            // json directories
    //            string json_directoryPatientPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\" + pId;
    //            if (!Directory.Exists(json_directoryPatientPath))
    //            {
    //                Directory.CreateDirectory(json_directoryPatientPath);
    //            }
    //            string json_directoryPatientCoursePath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\" + pId + "\\" + course;
    //            if (!Directory.Exists(json_directoryPatientCoursePath))
    //            {
    //                Directory.CreateDirectory(json_directoryPatientCoursePath);
    //            }
    //            string json_directoryPatientPlansPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\" + pId + "\\" + course + "\\Plans";
    //            if (!Directory.Exists(json_directoryPatientPlansPath))
    //            {
    //                Directory.CreateDirectory(json_directoryPatientPlansPath);
    //            }
    //            string json_directoryPatientSumsPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\" + pId + "\\" + course + "\\Sums";
    //            if (!Directory.Exists(json_directoryPatientSumsPath))
    //            {
    //                Directory.CreateDirectory(json_directoryPatientSumsPath);
    //            }
    //            // html directories
    //            string html_directoryPatientPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__ViewDVH\\" + pId;
    //            if (!Directory.Exists(html_directoryPatientPath))
    //            {
    //                Directory.CreateDirectory(html_directoryPatientPath);
    //            }
    //            string html_directoryPatientCoursePath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__ViewDVH\\" + pId + "\\" + course;
    //            if (!Directory.Exists(html_directoryPatientCoursePath))
    //            {
    //                Directory.CreateDirectory(html_directoryPatientCoursePath);
    //            }
    //            string html_directoryPatientPlansPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__ViewDVH\\" + pId + "\\" + course + "\\Plans";
    //            if (!Directory.Exists(html_directoryPatientPlansPath))
    //            {
    //                Directory.CreateDirectory(html_directoryPatientPlansPath);
    //            }
    //            string html_directoryPatientSumsPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__ViewDVH\\" + pId + "\\" + course + "\\Sums";
    //            if (!Directory.Exists(html_directoryPatientSumsPath))
    //            {
    //                Directory.CreateDirectory(html_directoryPatientSumsPath);
    //            }

    //            #endregion

    //            #region paths

    //            string jsonPath = "";
    //            string jsonPath_randomized = "";
    //            string htmlPath = "";
    //            string linkToCopy = "";
    //            string initialSumsDvhLink = "\\\\Client/S:/shares/RadOnc/ePHI/RO PHI PHYSICS/DosimetricReview/__ViewDVH/" + pId + "/" + course + "/Sums";
    //            string initialPlansDvhLink = "\\\\Client/S:/shares/RadOnc/ePHI/RO PHI PHYSICS/DosimetricReview/__ViewDVH/" + pId + "/" + course + "/Plans";
    //            if (context.PlanSetup == null)
    //            {
    //                jsonPath = json_directoryPatientSumsPath + "\\CourseJsonArray_" + pId + "_" + course + ".json";
    //                jsonPath_randomized = json_directoryPatientSumsPath + "\\RandomizedCourseJsonArray_" + randomId + "_" + course + ".json";
    //                htmlPath = html_directoryPatientSumsPath + "\\__ViewDVH_" + pId + "_" + course + ".html";
    //                linkToCopy = initialSumsDvhLink + "/__ViewDVH_" + pId + "_" + course + ".html";
    //            }
    //            else
    //            {
    //                jsonPath = json_directoryPatientPlansPath + "\\CourseJsonArray_" + pId + "_" + course + ".json";
    //                jsonPath_randomized = json_directoryPatientPlansPath + "\\RandomizedCourseJsonArray_" + randomId + "_" + course + ".json";
    //                htmlPath = html_directoryPatientPlansPath + "\\__ViewDVH_" + pId + "_" + course + ".html";
    //                linkToCopy = initialPlansDvhLink + "/__ViewDVH_" + pId + "_" + course + ".html";
    //            }

    //            //string jsonPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\" + "CourseJsonArray_" + pId + "_" + course + ".json"; // "S:\shares\RadOnc\ePHI\RO PHI PHYSICS\DosimetricReview\__JsonArrays"
    //            //string htmlPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\\\__ViewDVH\\" + pId + "_" + course + "_DVH.html"; // "S:\shares\RadOnc\ePHI\RO PHI PHYSICS\DosimetricReview\_ViewDVH" file:///S:/shares/RadOnc/ePHI/RO%20PHI%20PHYSICS/DosimetricReview/__ViewDVH/52584559_A-HN_DVH.html
    //            //linkToCopy = "";

    //            #endregion

    //            #region define json string

    //            //StreamWriter stream = new StreamWriter(htmlPath);

    //            //string plansJsonArray = "[{\"patientId\":\"" + pId + "\", \"randomId\":\"" + randomId + "\", \"courseId\":\"" + course + "\", \"planData\":[";
    //            //string plansJsonArray_randomized = "[{\"randomId\":\"" + randomId + "\", \"courseId\":\"" + course + "\", \"planData\":[";
    //            //string varPlanJSONArray = "var PlanJSONArray = ";

    //            //MessageBoxResult result = System.Windows.MessageBox.Show("Select Yes for ALL plans or No for the CURRENT plan:", "Collect DVH for All or CURRENT", MessageBoxButton.YesNo, MessageBoxImage.Question);
    //            //if (result == MessageBoxResult.Yes)
    //            //{
    //            //    if (context.PlanSetup == null)
    //            //    {
    //            //        plansJsonArray = plansJsonArray + GetJSON.getDvhJsonAllPlanSums(sums) + "]";
    //            //        plansJsonArray_randomized = plansJsonArray_randomized + GetJSON.getDvhJsonAllPlanSums(sums) + "]";
    //            //        varPlanJSONArray = varPlanJSONArray + plansJsonArray;
    //            //        //linkToCopy = "\\\\Client/S:/shares/RadOnc/ePHI/RO PHI PHYSICS/DosimetricReview/__ViewDVH/" + pId + "/" + pId + "_" + course + "_DVH.html";
    //            //        string message = "File Location:\n" + htmlPath + "\n\nClick OK to copy the link, then paste it into a Google Chrome browser.\n(if already open, just refresh)";
    //            //        if (System.Windows.Forms.MessageBox.Show(message, "OK to copy link", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
    //            //        { System.Windows.Forms.Clipboard.SetText(linkToCopy); }
    //            //    }
    //            //    else
    //            //    {
    //            //        plansJsonArray = plansJsonArray + GetJSON.getDvhJsonAllPlans(plans) + "]";
    //            //        plansJsonArray_randomized = plansJsonArray_randomized + GetJSON.getDvhJsonAllPlans(plans) + "]";
    //            //        varPlanJSONArray = varPlanJSONArray + plansJsonArray;
    //            //        //linkToCopy = "\\\\Client/S:/shares/RadOnc/ePHI/RO PHI PHYSICS/DosimetricReview/__ViewDVH/" + pId + "_" + course + "_DVH.html";
    //            //        string message = "File Location:\n" + htmlPath + "\n\nClick OK to copy the link, then paste it into a Google Chrome browser.\n(if already open, just refresh)";
    //            //        if (System.Windows.Forms.MessageBox.Show(message, "OK to copy link", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
    //            //        { System.Windows.Forms.Clipboard.SetText(linkToCopy); }
    //            //    }
    //            //}
    //            //if (result == MessageBoxResult.No)
    //            //{
    //            //    if (context.PlanSetup == null)
    //            //    {
    //            //        if (context.PlanSumsInScope.Count() > 1)
    //            //        {
    //            //            throw new ApplicationException("Please close other plan sums");
    //            //        }
    //            //        plansJsonArray = plansJsonArray + GetJSON.getDvhJsonCurrentPlanSum(psum, sorted_structureList) + "]";
    //            //        plansJsonArray_randomized = plansJsonArray_randomized + GetJSON.getDvhJsonCurrentPlanSum(psum, sorted_structureList) + "]";
    //            //        varPlanJSONArray = varPlanJSONArray + plansJsonArray;
    //            //        //linkToCopy = "\\\\Client/S:/shares/RadOnc/ePHI/RO PHI PHYSICS/DosimetricReview/__ViewDVH/" + pId + "_" + course + "_DVH.html";
    //            //        string message = "File Location:\n" + htmlPath + "\n\nClick OK to copy the link, then paste it into a Google Chrome browser.\n(if already open, just refresh)";
    //            //        if (System.Windows.Forms.MessageBox.Show(message, "OK to copy link", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
    //            //        { System.Windows.Forms.Clipboard.SetText(linkToCopy); }
    //            //    }
    //            //    else
    //            //    {
    //            //        plansJsonArray = plansJsonArray + GetJSON.getDvhJsonCurrentPlan(planSetup, sorted_structureList) + "]";
    //            //        plansJsonArray_randomized = plansJsonArray_randomized + GetJSON.getDvhJsonCurrentPlan(planSetup, sorted_structureList) + "]";
    //            //        varPlanJSONArray = varPlanJSONArray + plansJsonArray;
    //            //        //linkToCopy = "\\\\Client/S:/shares/RadOnc/ePHI/RO PHI PHYSICS/DosimetricReview/__ViewDVH/" + pId + "_" + course + "_DVH.html";
    //            //        string message = "File Location:\n" + htmlPath + "\n\nClick OK to copy the link, then paste it into a Google Chrome browser.\n(if already open, just refresh)";
    //            //        if (System.Windows.Forms.MessageBox.Show(message, "OK to copy link", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
    //            //        { System.Windows.Forms.Clipboard.SetText(linkToCopy); }
    //            //    }
    //            //}

    //            #endregion

    //            #endregion
    //            //---------------------------------------------------------------------------------
    //            #region write html

    //            //using (stream)
    //            //{
    //            //    stream.WriteLine(@"<!DOCTYPE html>");
    //            //    stream.WriteLine(@"<script src = 'https://ajax.googleapis.com/ajax/libs/jquery/3.1.1/jquery.min.js'></script>");
    //            //    stream.WriteLine(@"<link href='https://fonts.googleapis.com/css?family=PT+Sans' rel='stylesheet'>");
    //            //    stream.WriteLine(@"<html>");
    //            //    stream.WriteLine(@"<head>");
    //            //    stream.WriteLine(@"<meta charset='utf-8'/>");
    //            //    stream.WriteLine(@"<link rel='stylesheet' type='text/css' href='S:\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__ViewDVH\\__style__\\pageStyle.css'>");
    //            //    stream.WriteLine(@"<title>" + pId + " - DVH Review</title>");
    //            //    stream.WriteLine(@"</head>");
    //            //    stream.WriteLine(@"<body>");
    //            //    stream.WriteLine(@"<div class = 'ptNameDisplay' id = 'ptBlockDislay'>");
    //            //    stream.WriteLine(@"<button id = 'print' class='pBtn btn-primary btn-sml' onclick='PrepareAndPrint()'>Print</button>");
    //            //    stream.WriteLine(@"<p>Physician:<span class='tabPhysician'>" + PrimaryPhysician + "</span></p>");
    //            //    stream.WriteLine(@"<p>Patient:<span class='tabPtName'>" + pName + "</span></p>");
    //            //    stream.WriteLine(@"<p>MRN:<span class='tabMrn'>" + pId + "</span></p>");
    //            //    stream.WriteLine(@"<p>CurrentPlan:<span class='tabPlanName'>" + PlanName + "</span></p>");
    //            //    stream.WriteLine(@"<p>ApprovalStatus:<span class='tabPlanStatus'>" + planSetup.ApprovalStatus + "</span></p>");
    //            //    stream.WriteLine(@"</div>");
    //            //    stream.WriteLine(@"<div id='dvh'></div>");
    //            //    stream.WriteLine(@"</body>");
    //            //    stream.WriteLine(@"</html>");
    //            //    stream.WriteLine(@"<script>");
    //            //    stream.WriteLine(varPlanJSONArray);
    //            //    stream.WriteLine(@"$(document).ready(function() {
    //            //                            var options = {

    //            //                                chart: {
    //            //                                    renderTo: 'dvh',
    //            //                                    type: 'line',
    //            //                                    zoomType: 'xy',
    //            //                                    panning: true,
    //            //                                    panKey: 'shift',
    //            //                                },
    //            //                                exporting: {
    //            //                                    buttons: {
    //            //                                        contextButton: {
    //            //                                            enabled: false
    //            //                                        }
    //            //                                    }
    //            //                                },
    //            //                                xAxis: {
    //            //                                    title: {
    //            //                                        text: 'Dose (Gy)'
    //            //                                    },
    //            //                                    crosshair: true,
    //            //                                    maxPadding: 0.02
    //            //                                },
    //            //                                plotOptions: {
    //            //                                    series: {
    //            //                                        marker: {
    //            //                                            enabled: false
    //            //                                        },
    //            //                                        allowPointSelect: true,
    //            //                                        states: {
    //            //                                            hover: {
    //            //                                                enabled: true,
    //            //                                                lineWidth: 5
    //            //                                            }
    //            //                                        }
    //            //                                    },
    //            //                                    boxplot: {
    //            //                                     fillColor: '#505053'
    //            //                                    },
    //            //                                    candlestick: {
    //            //                                        lineColor: 'white'
    //            //                                    },
    //            //                                    errorbar: {
    //            //                                        color: 'white'
    //            //                                    }
    //            //                                },
    //            //                                yAxis: {
    //            //                                    labels: {
    //            //                                        format: '{value} %'
    //            //                                    },
    //            //                                    floor: 0,
    //            //                                    ceiling: 100,
    //            //                                    title: {
    //            //                                        text: 'Volume (%)'
    //            //                                    },
    //            //                                    crosshair: true,
    //            //                                    gridLineDashStyle: 'ShortDash',
    //            //                                    gridLineColor: '#aaaaaa'
    //            //                                },
    //            //                                tooltip: {
    //            //                                    shared: true,
    //            //                                    useHTML: true,
    //            //                                    headerFormat: '<table>',
    //            //                                    pointFormat: '<tr><td style=\""color:{series.color}; text-shadow: -1px 0 #353839, 0 1px #353839, 1px 0 #353839, 0 -1px #353839;\"">{series.name}: </td><td style=\""text-align: left; color:#282827\"">V{point.x} Gy = {point.y} %</td></tr>',
    //            //                                    footerFormat: '</table>',
    //            //                                    <!--valueDecimals: 1-->
    //            //                                },
    //            //                                title: {
    //            //                                    text: 'DVH',
    //            //                                    x: -150
    //            //                                },
    //            //                                subtitle: {
    //            //                                    text: 'Click and drag to zoom in. Hold down shift key to pan.',
    //            //                                    x:-150
    //            //                                },
    //            //                                legend: {
    //            //                                    layout: 'horizontal',
    //            //                                    align: 'right',
    //            //                                    verticalAlign: 'middle',
    //            //                                    borderWidth: 0,
    //            //                                    floating: false,
    //            //                                    width:420,
    //            //                                    itemWidth:210,
    //            //                                    itemStyle: {
    //            //                                    width:205
    //            //                                    },
    //            //                                    itemHiddenStyle: {
    //            //                                        color: '#ff4d4d'
    //            //                                    }
    //            //                                },

    //            //                                series: seriesOptions
    //            //                            };
        
    //    		      //                      <!--options.series = PlanJSONArray.planData.structureData-->;
    //            //                            var chart = new Highcharts.Chart(options);
    //            //                            });

    //            //                            var seriesOptions = [],
    //            //                                dashStyles = [
    //            //                                    'Solid',
    //            //                                    'ShortDash',
    //            //                                    'ShortDot',
    //            //                                    'ShortDashDot',
    //            //                                    'ShortDashDotDot',
    //            //                                    'Dot',
    //            //                                    'Dash',
    //            //                                    'LongDash',
    //            //                                    'DashDot',
    //            //                                    'LongDashDot',
    //            //                                    'LongDashDotDot'
    //            //                                ],
    //            //                                seriesCounter = 0
    //            //                            var jsonArray = PlanJSONArray[0]
    //            //                            var planData = PlanJSONArray[0].planData
    //            //                            planData.forEach(function(element, i) {
    //            //                                element.structureData.forEach(function(childElement, j){
    //            //                                    seriesOptions[seriesCounter] = {
    //            //                                            //task: element.subTaskId,
    //            //                                            name: element.planId + '_' + element.structureData[j].structureId,
    //            //                                            data: element.structureData[j].dvh,
    //            //                                            dashStyle: dashStyles[i],
    //            //                                            visible: false,
    //            //                                            color: element.structureData[j].color
    //            //                                        }
    //            //                                        seriesCounter += 1
    //            //                                    <!--console.log(seriesCounter)-->
    //            //                                    childElement.dvh.forEach(function(grandChildElement, h){
    //            //                                    })
    //            //                                })
	   //            //                         })
    //            //                            function PrepareAndPrint()
    //            //                            {
    //            //                                $('.pBtn').remove();
    //            //                                $('.Buttons').remove();
    //            //                                window.print();
    //            //                            }     
    //            //                        </script> ");
    //            //    stream.WriteLine(@"<div>");
    //            //    //stream.WriteLine(varPlanJSONArray);
    //            //    //stream.WriteLine(dvhJson);
    //            //    stream.WriteLine(@"<div>");
    //            //    stream.WriteLine(@"<script src = 'https://code.highcharts.com/highcharts.js'></script >");
    //            //    //stream.WriteLine(@"<script src = 'https://code.highcharts.com/js/highcharts.src.js'></script >");
    //            //    //stream.WriteLine(@"<script src = 'https://code.highcharts.com/modules/data.js'></script > ");
    //            //    //stream.WriteLine(@"<script src = 'https://code.highcharts.com/modules/exporting.js'></script >");
    //            //    //stream.WriteLine(@"<script src = 'https://rawgit.com/mholt/PapaParse/master/papaparse.js'></script>");


    //            //    stream.Flush();
    //            //    stream.Close();
    //            //}

    //            #endregion
    //            //---------------------------------------------------------------------------------
    //            #region write json

    //            //Process.Start(htmlPath); // H:\z-scripting\_projects\__ViewPerturbations\17926155_A-HN_DVH.html
    //            //File.WriteAllText(jsonPath, plansJsonArray);
    //            //File.WriteAllText(jsonPath_randomized, plansJsonArray_randomized);

    //            #endregion
    //            //---------------------------------------------------------------------------------
    //            #region log

    //            string script = "ViewDVH";
    //            string userLogPath = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\esapi\\projects\\__Logs\\UserLog.csv";
    //            StringBuilder userLogCsvContent = new StringBuilder();
    //            string user = context.CurrentUser.ToString();
    //            string day = DateTime.Now.ToLocalTime().Day.ToString();
    //            string month = DateTime.Now.ToLocalTime().Month.ToString();
    //            string year = DateTime.Now.ToLocalTime().Year.ToString();
    //            string hour = DateTime.Now.ToLocalTime().Hour.ToString();
    //            string minute = DateTime.Now.ToLocalTime().Minute.ToString();
    //            double idAsDouble = Convert.ToDouble(pId);
    //            //string randomId = Math.Ceiling(Math.Sqrt(idAsDouble * 5)).ToString();

    //            LogUser(script);

    //            void LogUser(string scriptTitle)
    //            {
    //                #region User Stats

    //                // add headers if the file doesn't exist
    //                // list of target headers for desired dose stats
    //                // in this case I want to display the headers every time so i can verify which target the distance is being measured for
    //                // this is due to the inconsistency in target naming (PTV1/2 vs ptv45/79.2) -- these can be removed later when cleaning up the data
    //                if (!File.Exists(userLogPath))
    //                {
    //                    List<string> dataHeaderList = new List<string>();
    //                    dataHeaderList.Add("User");
    //                    dataHeaderList.Add("Script");
    //                    dataHeaderList.Add("Date");
    //                    dataHeaderList.Add("DayOfWeek");
    //                    dataHeaderList.Add("Time");
    //                    dataHeaderList.Add("ID");
    //                    dataHeaderList.Add("RandomID");
    //                    dataHeaderList.Add("CourseName");
    //                    dataHeaderList.Add("PlanName");
    //                    dataHeaderList.Add("DosePerFraction");
    //                    dataHeaderList.Add("NumberOfFractions");

    //                    string concatDataHeader = string.Join(",", dataHeaderList.ToArray());

    //                    userLogCsvContent.AppendLine(concatDataHeader);
    //                }

    //                List<object> userStatsList = new List<object>();

    //                string userId = user;
    //                string scriptId = script;
    //                string date = string.Format("{0}/{1}/{2}", day, month, year);
    //                string dayOfWeek = day;
    //                string time = string.Format("{0}:{1}", hour, minute);
    //                string ptId = pId;
    //                string randomPtId = randomId;
    //                string plan = PlanName;
    //                string dosePerFx = dosePerFraction.ToString();
    //                string numFx = fractions.ToString();

    //                userStatsList.Add(userId);
    //                userStatsList.Add(scriptId);
    //                userStatsList.Add(date);
    //                userStatsList.Add(dayOfWeek);
    //                userStatsList.Add(time);
    //                userStatsList.Add(ptId);
    //                userStatsList.Add(randomPtId);
    //                userStatsList.Add(course);
    //                userStatsList.Add(plan);
    //                userStatsList.Add(dosePerFx);
    //                userStatsList.Add(numFx);

    //                string concatUserStats = string.Join(",", userStatsList.ToArray());

    //                userLogCsvContent.AppendLine(concatUserStats);


    //                #endregion Target Dose Stats

    //                #region Write Files

    //                File.AppendAllText(userLogPath, userLogCsvContent.ToString());

    //                #endregion

    //                #region Collection Complete Message

    //                //MessageBox.Show(" Data collection complete. Thanks for helping us collect data!\n\n\t\t\t:)");

    //                #endregion
    //            }

    //            #endregion
    //        }
    //        else
    //        {
    //            System.Windows.MessageBox.Show("No patient selected");
    //        }
    //    }
    //}
}
