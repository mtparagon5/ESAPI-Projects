using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using static GetJSON.MainControl;


// Do not change namespace and class name
// otherwise Eclipse will not be able to run the script.
namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

		public void Execute(ScriptContext context, Window window)
        {

            //---------------------------------------------------------------------------------
            #region plan context, maincontrol, and window defitions

            #region single / plan sum context variable definitions


            #region old
            //         // to work for plan sum
            //         StructureSet structureSet;
            //         PlanningItem selectedPlanningItem;
            //         PlanSetup planSetup;
            //var PlanName = string.Empty;
            //         double fractions = 0;
            //if (context.PlanSetup == null && context.PlanSumsInScope.Count() > 1)
            //         {
            //             throw new ApplicationException("Please close other plan sums");
            //         }
            //         if (context.PlanSetup == null)
            //         {
            //             PlanSum psum = context.PlanSumsInScope?.First();
            //             planSetup = psum?.PlanSetups.First();
            //             selectedPlanningItem = (PlanningItem)psum;
            //             structureSet = planSetup?.StructureSet;
            //	PlanName = psum.Id.ToString().Replace(" ", "_").Replace(":", "_");
            //             fractions = DvhExtensions.getTotalFractionsForPlanSum(psum);
            //}
            //         else
            //         {
            //             planSetup = context.PlanSetup;
            //             selectedPlanningItem = (PlanningItem)planSetup;
            //             structureSet = planSetup?.StructureSet;
            //	PlanName = planSetup.Id.ToString().Replace(" ", "_").Replace(":", "_");
            //             if (planSetup?.UniqueFractionation.NumberOfFractions != null)
            //	{
            //                 fractions = (double)planSetup?.UniqueFractionation.NumberOfFractions;
            //             }
            //         }
            //planSetup.DoseValuePresentation = DoseValuePresentation.Absolute;
            //double planMaxDose = 0;
            //if (planSetup.Dose != null)
            //{
            //	planMaxDose = Math.Round(planSetup.Dose.DoseMax3D.Dose, 5);
            //}
            //else { planMaxDose = Double.NaN; }
            //var dosePerFx = planSetup?.UniqueFractionation.PrescribedDosePerFraction.Dose;
            //         var rxDose = (double)(dosePerFx * fractions);
            #endregion old

            #endregion
            //---------------------------------------------------------------------------------
            #region window definitions

            // Add existing WPF control to the script window.
            var mainControl = new GetJSON.MainControl();

            window.Content = mainControl;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Title = "Save JSON Data";

            #endregion
            //---------------------------------------------------------------------------------
            #region structure lists

            #endregion structure organization and ordering
            //---------------------------------------------------------------------------------
            #region mainControl variable definitions

            //MessageBox.Show("defining main control variables...");

            //mainControl.Progress_Result.Text = "Creating variables from plan data...";
            var hnEUHM = Tuple.Create("KBM consisting of HN plans planned at EUHM.",
                                        @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\_JsonData_\\KnowledgeBasedLearning\\Models\\HN\\HN_EUHM.json");
            var hnEUH = Tuple.Create("KBM consisting of HN plans planned at EUH.",
                                        @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\_JsonData_\\KnowledgeBasedLearning\\Models\\HN\\HN_EUH.json");
            var hnEUHM2 = Tuple.Create("KBM consisting of HN plans planned at EUH.",
                                        @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\_JsonData_\\KnowledgeBasedLearning\\Models\\HN\\");
            var cardiacSparing = Tuple.Create("KBM consisting of lung plans which optimize cardiac sparing.",
                                                @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\_JsonData_\\KnowledgeBasedLearning\\Models\\CardiacSparing.json");

            mainControl.kbmList.Add(Tuple.Create("Cardiac Sparing", cardiacSparing));
            mainControl.kbmList.Add(Tuple.Create("HN - EUH", hnEUH));
            mainControl.kbmList.Add(Tuple.Create("HN - EUHM", hnEUHM2));
            //mainControl.kbmList.Add(Tuple.Create("HN - EUHM - Master", hnEUHM)); // NOTE: add back to save to hn model master file

            foreach (var kbmTuple in mainControl.kbmList.OrderBy(x => x.Item1))
            {
                mainControl.kbmList_CB.Items.Add(kbmTuple.Item1);
            }

            // plan variables
            mainControl.patient = PPatient.CreatePatient(context);
            mainControl.courseHeader = context.Course.ToString().Split('-').Last().Replace(" ", "_");
            mainControl.courseId = context.PlanSetup.Course.ToString();


            //PPlan plan = PPlan.CreatePPlan(context);

            // create list of all plans and convert to PPlan
            var plans = context.PlansInScope.GetEnumerator();
            var sums = context.PlanSumsInScope.GetEnumerator();
            //var hasMultipleSums = context.PlanSumsInScope.Count() > 1;
            if (context.PlanSumsInScope.Count() > 1) { throw new ApplicationException("Please close other plan sums"); }
            #region original way
            //if (context.PlanSumsInScope.Count() == 1)
            //{
            //    var collectPlanSum = false;
            //    var includeSum = MessageBox.Show("There is a PlanSum open, should it be included in data collection?", "Collect PlanSum Data?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //    if (includeSum == MessageBoxResult.Yes)
            //    {
            //        collectPlanSum = true;
            //    }
            //    else { collectPlanSum = false; }
            //    if (collectPlanSum)
            //    {
            //        var sums = context.PlanSumsInScope.GetEnumerator();
            //        while (sums.MoveNext())
            //        {
            //            var currentSum = sums.Current;
            //            var pps = PPlan.CreatePPlan(context, currentSum);
            //            if (!pplans.Contains(pps))
            //            {
            //                pplans.Add(pps);
            //            }
            //        }
            //    }
            //}
            //while (plans.MoveNext())
            //{
            //    var p = (PlanSetup)plans.Current;
            //    if (p.ApprovalStatus == PlanSetupApprovalStatus.Reviewed ||
            //        p.ApprovalStatus == PlanSetupApprovalStatus.PlanningApproved ||
            //        p.ApprovalStatus == PlanSetupApprovalStatus.TreatmentApproved)
            //    {
            //        var pp = PPlan.CreatePPlan(context, p);
            //        if (!pplans.Contains(pp))
            //        {
            //            pplans.Add(pp);
            //        }
            //    }

            //}
            #endregion original way

            //var pplans = new List<PPlan>();
            if (context.PlanSumsInScope.Count() == 1)
            {
                //var collectPlanSum = false;
                //var includeSum = MessageBox.Show("There is a PlanSum open, should it be included in data collection?", "Collect PlanSum Data?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                //if (includeSum == MessageBoxResult.Yes)
                //{
                //    collectPlanSum = true;
                //}
                //else { collectPlanSum = false; }
                //if (collectPlanSum)
                //{
                    //var sums = context.PlanSumsInScope.GetEnumerator();
                while (sums.MoveNext())
                {
                    var currentSum = sums.Current;
                    var pps = PPlan.CreatePPlan(context, currentSum);
                    if (!mainControl.pplans.Contains(pps))
                    {
                        mainControl.pplans.Add(pps);
                    }
                }
                //}
            }
            while (plans.MoveNext())
            {

                var p = (PlanSetup)plans.Current;
                //if (p.ApprovalStatus == PlanSetupApprovalStatus.Reviewed ||
                //    p.ApprovalStatus == PlanSetupApprovalStatus.PlanningApproved ||
                //    p.ApprovalStatus == PlanSetupApprovalStatus.TreatmentApproved)
                //{
                    var pp = PPlan.CreatePPlan(context, p);
                    if (!mainControl.pplans.Contains(pp))
                    {
                        mainControl.pplans.Add(pp);
                    }
                //}

            }
            // order plans by id
            //mainControl.pplans.OrderBy(x => x.Id);

            foreach (var plan in mainControl.pplans.OrderBy(x => x.Id))
            {
                mainControl.planList_LV.Items.Add(plan.Id);
            }

            // initiate empty list for json objects
            //var JsonObjList = new List<ESJO>();



            //ESJO plans_jo = ESJO.CreateESJO("PlanData", pplans); JsonObjList.Add(plans_jo);

            //if (plans_jo.JsonString == "incomplete")
            //{
            //    //mainControl.Progress_Result.Text = "Sorry, something went wrong...";
            //}
            //else
            //{
            //    string jsonPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\_JsonData_\\" + patient.Id + "_data.json";
            //    var jsonString = string.Empty;

            //    jsonString += "{";
            //    foreach (var jo in JsonObjList)
            //    {
            //        jsonString += jo.JsonString + ",";
            //    }
            //    jsonString = jsonString.TrimEnd(',');
            //    jsonString += "}";
            //    File.WriteAllText(jsonPath, jsonString);

            //    //mainControl.Progress_Result.Text = "JSON Data saved successfully...";
            //}




            #endregion
            //---------------------------------------------------------------------------------

            #endregion
            //---------------------------------------------------------------------------------
            #region Log
            //mainControl.Progress_Result.Text = "Writing Log Data...";

            //string LogPath = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\esapi\\projects\\__Logs\\UserLog.csv";

            //foreach (var plan in pplans.Where(x => x.Type != "PlanSum"))
            //{
            //    Log.CreateLog(context, mainControl.patient, LogPath, "GetJSON", plan.DosePerFraction.ToString(), plan.Fractions.ToString());
            //}


            //mainControl.Progress_Result.Text = "Writing Log Data...Finished";

            #endregion
            //---------------------------------------------------------------------------------

        }
    }
}
