using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using static PlanReview.MainControl;


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

            //---------------------------------------------------------------------------------
            #region context variable definitions
            //int n = 0;
            //MessageBox.Show(n.ToString());
            //++n;

            // to work for plan sum
            StructureSet structureSet;
            PlanningItem selectedPlanningItem;
            PlanSetup planSetup;
            PlanSum psum = null;
            string status = "";
            double? fractions = 0;
            if (context.PlanSetup == null && context.PlanSumsInScope.Count() > 1)
            {
                throw new ApplicationException("Please close other plan sums");
            }
            if (context.PlanSetup == null)
            {
                psum = context.PlanSumsInScope?.First();
                planSetup = psum?.PlanSetups.First();
                selectedPlanningItem = (PlanningItem)psum;
                structureSet = planSetup?.StructureSet;
                fractions = DvhExtensions.getTotalFractionsForPlanSum(psum);
                status = "PlanSum";
            }
            else
            {
                planSetup = context.PlanSetup;
                selectedPlanningItem = (PlanningItem)planSetup;
                structureSet = planSetup?.StructureSet;
                if (planSetup?.UniqueFractionation.NumberOfFractions != null)
                {
                    fractions = (double)planSetup?.UniqueFractionation.NumberOfFractions;
                }
                status = planSetup.ApprovalStatus.ToString();
            }
            var dosePerFx = planSetup?.UniqueFractionation.PrescribedDosePerFraction.Dose;
            //var rxDose = (double)(dosePerFx * fractions);
            structureSet = planSetup != null ? planSetup.StructureSet : psum.PlanSetups.Last().StructureSet; // changed from first to last in case it's broken on next build
            string pId = context.Patient.Id;
			ProcessIdName.getRandomId(pId, out string rId);
            string course = context.Course.Id.ToString().Replace(" ", "_"); ;
            string plan = context.PlanSetup.Id.ToString().Replace(" ", "_"); ;
            string pName = ProcessIdName.processPtName(context.Patient.Name);
            //MessageBox.Show(n.ToString());
            //++n;
            #endregion
            //---------------------------------------------------------------------------------
            #region window definitions

            // Add existing WPF control to the script window.
            var mainControl = new PlanReview.MainControl();
            //mainControl.Window = window;
            //window.WindowStyle = WindowStyle.None;
            window.Content = mainControl;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Title = "Target Proximity Statistics for " + selectedPlanningItem?.Id;
            //mainControl.Title = "DVH Review for " + selectedPlanningItem?.Id;

            #endregion
            //---------------------------------------------------------------------------------
            #region mainControl variable definitions

            mainControl.ps = planSetup;
            mainControl.pitem = selectedPlanningItem;
            mainControl.ss = structureSet;
            //mainControl.Fractions = fractions;
            //if ((planSetup?.UniqueFractionation.NumberOfFractions == null) && (mainControl.pitem.Dose != null))
            //{
            //    MessageBox.Show("Dose is calculated but Dose/Fraction and Number of Fractions have not been defined.");
            //    return;
            //}
            //if (mainControl.pitem.Dose == null)
            //    MessageBox.Show("Dose is not calculated:\nPTV proximity statistics can still be calculated by selecting an OAR.");

            mainControl.user = context.CurrentUser.ToString();
            mainControl.day = DateTime.Now.ToLocalTime().Day.ToString();
            mainControl.month = DateTime.Now.ToLocalTime().Month.ToString();
            mainControl.year = DateTime.Now.ToLocalTime().Year.ToString();
            mainControl.hour = DateTime.Now.ToLocalTime().Hour.ToString();
            mainControl.minute = DateTime.Now.ToLocalTime().Minute.ToString();
            mainControl.timeStamp = string.Format("{0}", DateTime.Now.ToLocalTime().ToString());
            mainControl.curredLastName = context.Patient.LastName.Replace(" ", "_");
            mainControl.curredFirstName = context.Patient.FirstName.Replace(" ", "_");
            mainControl.firstInitial = context.Patient.FirstName[0].ToString();
            mainControl.lastInitial = context.Patient.LastName[0].ToString();
            mainControl.initials = mainControl.firstInitial + mainControl.lastInitial;
            mainControl.id = context.Patient.Id;
            //mainControl.idAsDouble = Convert.ToDouble(mainControl.id);
			mainControl.randomId = rId;
            mainControl.courseName = context.Course.Id.ToString().Replace(" ", "_");
            mainControl.courseHeader = context.Course.Id.ToString().Split('-').Last().Replace(" ", "_");
            mainControl.planName = selectedPlanningItem.Id.ToString().Replace(" ", "_").Replace(":","_");
            mainControl.approvalStatus = status;
            //mainControl.planMaxDose = (double)selectedPlanningItem?.Dose.DoseMax3D.Dose;
            mainControl.dosePerFraction = (double)dosePerFx;
            mainControl.Fractions = (double)fractions;

            string tempPhysicianId = context.Patient.PrimaryOncologistId;
            PrimaryPhysician PrimaryPhysician = new PrimaryPhysician();
            PrimaryPhysician.Name = GetPrimary.Physician(tempPhysicianId);

            mainControl.primaryPhysician = PrimaryPhysician.Name.ToString();
            mainControl.PrimaryOnc.Text = mainControl.primaryPhysician;
            mainControl.PatientId.Text = pId;
            mainControl.PatientName.Text = pName;
            mainControl.PlanId.Text = selectedPlanningItem.Id;

            // isGrady -- they don't have direct access to S Drive (to write files)
            var is_grady = MessageBox.Show("Are you accessing this script from the Grady Campus?", "Direct $S Drive Access", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (is_grady == MessageBoxResult.Yes)
            {
                mainControl.isGrady = true;
            }
            else { mainControl.isGrady = false; }


            #endregion

            #endregion
            //---------------------------------------------------------------------------------
            #region json / csv

            if (mainControl.isGrady == false)
            {
                //---------------------------------------------------------------------------------
                #region directories

                #region directories

                #region patient specific directories

                #region base directory

                // patientSpecificDirectory
                string planDataDirectory = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanData__";
                if (!Directory.Exists(planDataDirectory))
                {
                    Directory.CreateDirectory(planDataDirectory);
                }
                string masterPlanDataDirectory = planDataDirectory + "\\_MasterData_";
                if (!Directory.Exists(masterPlanDataDirectory))
                {
                    Directory.CreateDirectory(masterPlanDataDirectory);
                }

                // patientSpecificDirectory
                string patientSpecificDirectory = planDataDirectory + "\\_PatientSpecific_\\" + pId + "\\" + course;
                if (!Directory.Exists(patientSpecificDirectory))
                {
                    Directory.CreateDirectory(patientSpecificDirectory);
                }

                // DASHBOARD DIRECTORY
                string dashboardDiretory = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanDataDashboard__";
                if (!Directory.Exists(dashboardDiretory))
                {
                    Directory.CreateDirectory(dashboardDiretory);
                }

                #endregion

                #region proximity statistics

                // patientSpecificProximityStatsDirectory
                string patientSpecificProximityStatsDirectory = patientSpecificDirectory + "\\TargetProximityStats";
                if (!Directory.Exists(patientSpecificProximityStatsDirectory))
                {
                    Directory.CreateDirectory(patientSpecificProximityStatsDirectory);
                }

                // patientSpecificProximityStatsDirectory_randomized
                string patientSpecificProximityStatsDirectory_randomized = patientSpecificProximityStatsDirectory + "\\Randomized";
                if (!Directory.Exists(patientSpecificProximityStatsDirectory_randomized))
                {
                    Directory.CreateDirectory(patientSpecificProximityStatsDirectory_randomized);
                }

                // patientSpecificProximityStatsDirectory_randomizedJson
                string patientSpecificProximityStatsDirectory_randomizedJson = patientSpecificProximityStatsDirectory_randomized + "\\JSON";
                if (!Directory.Exists(patientSpecificProximityStatsDirectory_randomizedJson))
                {
                    Directory.CreateDirectory(patientSpecificProximityStatsDirectory_randomizedJson);
                }

                // patientSpecificProximityStatsDirectory_randomizedCsvRows
                string patientSpecificProximityStatsDirectory_randomizedCsvRows = patientSpecificProximityStatsDirectory_randomized + "\\CsvRows";
                if (!Directory.Exists(patientSpecificProximityStatsDirectory_randomizedCsvRows))
                {
                    Directory.CreateDirectory(patientSpecificProximityStatsDirectory_randomizedCsvRows);
                }

                // structureSpecificProximityStatsDirectory_randomizedCsvCols
                string patientSpecificProximityStatsDirectory_randomizedCsvCols = patientSpecificProximityStatsDirectory_randomized + "\\CsvColumns";
                if (!Directory.Exists(patientSpecificProximityStatsDirectory_randomizedCsvCols))
                {
                    Directory.CreateDirectory(patientSpecificProximityStatsDirectory_randomizedCsvCols);
                }
                

                #endregion

                #region dvh data

                //// patientSpecificDvhDataDirectory
                //string patientSpecificDvhDataDirectory = patientSpecificDirectory + "\\DvhData";
                //if (!Directory.Exists(patientSpecificDvhDataDirectory))
                //{
                //    Directory.CreateDirectory(patientSpecificDvhDataDirectory);
                //}

                //// patientSpecificDvhDataDirectory_plans
                //string patientSpecificDvhDataDirectory_plans = patientSpecificDvhDataDirectory + "\\Plans";
                //if (!Directory.Exists(patientSpecificDvhDataDirectory_plans))
                //{
                //    Directory.CreateDirectory(patientSpecificDvhDataDirectory_plans);
                //}

                //// patientSpecificDvhDataDirectory_sums
                //string patientSpecificDvhDataDirectory_sums = patientSpecificDvhDataDirectory + "\\Sums";
                //if (!Directory.Exists(patientSpecificDvhDataDirectory_sums))
                //{
                //    Directory.CreateDirectory(patientSpecificDvhDataDirectory_sums);
                //}

                //// patientSpecificDvhDataDirectory_randomized
                //string patientSpecificDvhDataDirectory_randomized = patientSpecificDvhDataDirectory + "\\Randomized";
                //if (!Directory.Exists(patientSpecificDvhDataDirectory_randomized))
                //{
                //    Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomized);
                //}

                //// patientSpecificDvhDataDirectory_randomizedJson
                //string patientSpecificDvhDataDirectory_randomizedJson = patientSpecificDvhDataDirectory_randomized + "\\JSON";
                //if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedJson))
                //{
                //    Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedJson);
                //}
                //string finalDvhJsonPath_patientSpecific = patientSpecificDvhDataDirectory_plans;
                //string finalDvhJsonPath_randomizedJson_patientSpecific = patientSpecificDvhDataDirectory_randomizedJson;

                //// patientSpecificDvhDataDirectory_randomizedCsvRows
                //string patientSpecificDvhDataDirectory_randomizedCsvRows = patientSpecificDvhDataDirectory_randomized + "\\CsvRows";
                //if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedCsvRows))
                //{
                //    Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedCsvRows);
                //}

                //// patientSpecificDvhDataDirectory_randomizedCsvCols
                //string patientSpecificDvhDataDirectory_randomizedCsvCols = patientSpecificDvhDataDirectory_randomized + "\\CsvColumns";
                //if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedCsvCols))
                //{
                //    Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedCsvCols);
                //}

                #endregion

                #endregion

                #region physician specific

                //// physician folder
                //string physicianSpecificDirectory = planDataDirectory + "\\_PhysicianSpecific_\\" + PrimaryPhysician.Name.ToString();
                //if (!Directory.Exists(physicianSpecificDirectory))
                //{
                //    Directory.CreateDirectory(physicianSpecificDirectory);
                //}

                //string physicianSpecificStructureDvhDirectory = physicianSpecificDirectory + "\\StructureDvhData\\" + mainControl.courseHeader;
                //if (!Directory.Exists(physicianSpecificStructureDvhDirectory))
                //{
                //    Directory.CreateDirectory(physicianSpecificStructureDvhDirectory);
                //}

                //string physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns = physicianSpecificDirectory + "\\_PlanDvhData_\\" + currentPlan.Id + "\\CsvColumns";
                //if (!Directory.Exists(physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns))
                //{
                //    Directory.CreateDirectory(physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns);
                //}

                //string physicianSpecificPlanDvhDataDirectory_randomizedCsvRows = physicianSpecificDirectory + "\\_PlanDvhData_\\" + currentPlan.Id + "\\CsvRows";
                //if (!Directory.Exists(physicianSpecificPlanDvhDataDirectory_randomizedCsvRows))
                //{
                //    Directory.CreateDirectory(physicianSpecificPlanDvhDataDirectory_randomizedCsvRows);
                //}

                #endregion

                #region structure specific

                // structure specific directory
                string structureSpecificDirectory = planDataDirectory + "\\_StructureSpecific_";
                if (!Directory.Exists(structureSpecificDirectory))
                {
                    Directory.CreateDirectory(structureSpecificDirectory);
                }

                // structure specific target prox stats
                string structureSpecificProximityStatsDirectory = structureSpecificDirectory + "\\TargetProximityStats\\" + mainControl.courseHeader;
                if (!Directory.Exists(structureSpecificProximityStatsDirectory))
                {
                    Directory.CreateDirectory(structureSpecificProximityStatsDirectory);
                }

                //// structure specific dvhdata
                //string structureSpecificDvhDataDirectory = structureSpecificDirectory + "\\DvhData\\" + mainControl.courseHeader;
                //if (!Directory.Exists(structureSpecificDvhDataDirectory))
                //{
                //    Directory.CreateDirectory(structureSpecificDvhDataDirectory);
                //}

                //// structure specific dvhdata json
                //string structureSpecificDvhDataDirectory_randomizedJson = structureSpecificDvhDataDirectory + "\\JSON";
                //if (!Directory.Exists(structureSpecificDvhDataDirectory_randomizedJson))
                //{
                //    Directory.CreateDirectory(structureSpecificDvhDataDirectory_randomizedJson);
                //}

                //// structure specific dvhdata csv
                //string structureSpecificDvhDataDirectory_randomizedCsvRows = structureSpecificDvhDataDirectory + "\\CsvRows";
                //if (!Directory.Exists(structureSpecificDvhDataDirectory_randomizedCsvRows))
                //{
                //    Directory.CreateDirectory(structureSpecificDvhDataDirectory_randomizedCsvRows);
                //}

                #endregion

                #endregion

                #region old
                //// json directories
                //string json_directoryPatientPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\" + pId;
                //if (!Directory.Exists(json_directoryPatientPath))
                //{
                //    Directory.CreateDirectory(json_directoryPatientPath);
                //}
                //string json_directoryPatientCoursePath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\" + pId + "\\" + course;
                //if (!Directory.Exists(json_directoryPatientCoursePath))
                //{
                //    Directory.CreateDirectory(json_directoryPatientCoursePath);
                //}
                //string json_directoryProximityStatisticsPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\" + pId + "\\" + course + "\\_ProximityStatistics";
                //if (!Directory.Exists(json_directoryProximityStatisticsPath))
                //{
                //    Directory.CreateDirectory(json_directoryProximityStatisticsPath);
                //}
                //string json_directoryProximityStatisticsForPlanPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\" + pId + "\\" + course + "\\_ProximityStatistics\\" + mainControl.planName;
                //if (!Directory.Exists(json_directoryProximityStatisticsForPlanPath))
                //{
                //    Directory.CreateDirectory(json_directoryProximityStatisticsForPlanPath);
                //}
                //string csv_directoryProximityStatisticsForPlanPath = json_directoryProximityStatisticsForPlanPath + "\\csv";
                //if (!Directory.Exists(csv_directoryProximityStatisticsForPlanPath))
                //{
                //    Directory.CreateDirectory(csv_directoryProximityStatisticsForPlanPath);
                //}
                //string csvRandomized_directoryProximityStatisticsForPlanPath = csv_directoryProximityStatisticsForPlanPath + "\\randomized";
                //if (!Directory.Exists(csvRandomized_directoryProximityStatisticsForPlanPath))
                //{
                //    Directory.CreateDirectory(csvRandomized_directoryProximityStatisticsForPlanPath);
                //}
                //string rowsCsvRandomized_path = csvRandomized_directoryProximityStatisticsForPlanPath + "\\rows";
                //if (!Directory.Exists(rowsCsvRandomized_path))
                //{
                //    Directory.CreateDirectory(rowsCsvRandomized_path);
                //}
                //string columnsCsvRandomized_path = csvRandomized_directoryProximityStatisticsForPlanPath + "\\cols";
                //if (!Directory.Exists(columnsCsvRandomized_path))
                //{
                //    Directory.CreateDirectory(columnsCsvRandomized_path);
                //}
                //string structureSpecificFolderPath_rows = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__StructureSpecificData__\\__ProximityStatistics__";
                //if (!Directory.Exists(structureSpecificFolderPath_rows))
                //{
                //    Directory.CreateDirectory(structureSpecificFolderPath_rows);
                //}
                #endregion

                #region original -- old

                // structure specific csv paths -- rows only
                //string structureSpecificFolderPath_csv = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_JsonArrays\\_StructureSpecificData_\\_csv_";
                //string physicianAndStructureSpecificFolderPath_rows_csv = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_JsonArrays\\_PhysicianSpecificData_\\" + primaryPhysician + "\\_StructureData_\\_csv_";
                // patient specific paths
                //string finalColsDvhFolderPath = folderPathForDvhCsv + "\\" + currentPlan.Id + "_DVH\\columns";
                //string finalRowsDvhFolderPath = folderPathForDvhCsv + "\\" + currentPlan.Id + "_DVH\\rows";
                // physician specific csv paths
                //string physicianSpecificFolderPath_cols = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_JsonArrays\\_PhysicianSpecificData_\\" + primaryPhysician + "\\" + currentPlan.Id + "_DVH\\colums";
                //string physicianSpecificFolderPath_rows = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_JsonArrays\\_PhysicianSpecificData_\\" + primaryPhysician + "\\" + currentPlan.Id + "_DVH\\rows";
                // create directories
                //if (!Directory.Exists(finalColsDvhFolderPath))
                //{
                //    Directory.CreateDirectory(finalColsDvhFolderPath);
                //}
                //if (!Directory.Exists(finalRowsDvhFolderPath))
                //{
                //    Directory.CreateDirectory(finalRowsDvhFolderPath);
                //}
                //if (!Directory.Exists(physicianSpecificFolderPath_cols))
                //{
                //    Directory.CreateDirectory(physicianSpecificFolderPath_cols);
                //}
                //if (!Directory.Exists(physicianSpecificFolderPath_rows))
                //{
                //    Directory.CreateDirectory(physicianSpecificFolderPath_rows);
                //}
                //if (!Directory.Exists(structureSpecificFolderPath_csv))
                //{
                //    Directory.CreateDirectory(structureSpecificFolderPath_csv);
                //}
                //if (!Directory.Exists(physicianAndStructureSpecificFolderPath_rows_csv))
                //{
                //    Directory.CreateDirectory(physicianAndStructureSpecificFolderPath_rows_csv);
                //}
                //if (!Directory.Exists(structureSpecificDvhDataDirectory_randomizedJson))
                //{
                //    Directory.CreateDirectory(structureSpecificDvhDataDirectory_randomizedJson);
                //}
                #endregion

                #region old

                //// new structure
                //string patientSpecificDirectory = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanData__\\__PatientSpecific__\\" + pId + "\\" + course;
                //if (!Directory.Exists(patientSpecificDirectory))
                //{
                //    Directory.CreateDirectory(patientSpecificDirectory);
                //}
                //string patientSpecificProximityStatsDirectory = patientSpecificDirectory + "\\__TargetProximityStats__";
                //if (!Directory.Exists(patientSpecificProximityStatsDirectory))
                //{
                //    Directory.CreateDirectory(patientSpecificProximityStatsDirectory);
                //}
                //string patientSpecificProximityStatsDirectory_randomized = patientSpecificProximityStatsDirectory + "\\__Randomized__";
                //if (!Directory.Exists(patientSpecificProximityStatsDirectory_randomized))
                //{
                //    Directory.CreateDirectory(patientSpecificProximityStatsDirectory_randomized);
                //}
                //string patientSpecificProximityStatsDirectory_randomizedJson = patientSpecificProximityStatsDirectory_randomized + "\\__JSON__";
                //if (!Directory.Exists(patientSpecificProximityStatsDirectory_randomizedJson))
                //{
                //    Directory.CreateDirectory(patientSpecificProximityStatsDirectory_randomizedJson);
                //}

                //string patientSpecificProximityStatsDirectory_csvRows = patientSpecificProximityStatsDirectory_randomized + "\\__CsvRows__";
                //if (!Directory.Exists(patientSpecificProximityStatsDirectory_csvRows))
                //{
                //    Directory.CreateDirectory(patientSpecificProximityStatsDirectory_csvRows);
                //}
                //string structureSpecificProximityStatsDirectory_cols = patientSpecificProximityStatsDirectory_randomized + "\\__CsvColumns__";
                //if (!Directory.Exists(structureSpecificProximityStatsDirectory_cols))
                //{
                //    Directory.CreateDirectory(structureSpecificProximityStatsDirectory_cols);
                //}
                //string patientSpecificDvhDataDirectory = patientSpecificDirectory + "\\__DvhData__";
                //if (!Directory.Exists(patientSpecificDvhDataDirectory))
                //{
                //    Directory.CreateDirectory(patientSpecificDvhDataDirectory);
                //}
                //string patientSpecificDvhDataDirectory_randomized = patientSpecificDirectory + "\\__DvhData__\\__Randomized__";
                //if (!Directory.Exists(patientSpecificDvhDataDirectory_randomized))
                //{
                //    Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomized);
                //}
                //string structureSpecificDirectory = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanData__\\__StructureSpecific__";
                //if (!Directory.Exists(structureSpecificDirectory))
                //{
                //    Directory.CreateDirectory(structureSpecificDirectory);
                //}
                //string structureSpecificProximityStatsDirectory = structureSpecificDirectory + "\\__TargetProximityStats__";
                //if (!Directory.Exists(structureSpecificProximityStatsDirectory))
                //{
                //    Directory.CreateDirectory(structureSpecificProximityStatsDirectory);
                //}
                //string structureSpecificProximityStatsDirectory_randomized = structureSpecificProximityStatsDirectory + "\\__Randomized__";
                //if (!Directory.Exists(structureSpecificProximityStatsDirectory_randomized))
                //{
                //    Directory.CreateDirectory(structureSpecificProximityStatsDirectory_randomized);
                //}
                //string structureSpecificProximityStatsDirectory_randomizedJson = structureSpecificProximityStatsDirectory_randomized + "\\__JSON__";
                //if (!Directory.Exists(structureSpecificProximityStatsDirectory_randomizedJson))
                //{
                //    Directory.CreateDirectory(structureSpecificProximityStatsDirectory_randomizedJson);
                //}

                //string structureSpecificProximityStatsDirectory_randomizedCsvRows = structureSpecificProximityStatsDirectory_randomized + "\\__CsvRows__";
                //if (!Directory.Exists(structureSpecificProximityStatsDirectory_randomizedCsvRows))
                //{
                //    Directory.CreateDirectory(structureSpecificProximityStatsDirectory_randomizedCsvRows);
                //}
                ////string structureSpecificProximityStatsDirectory_randomizedCsvCols = structureSpecificProximityStatsDirectory_randomized + "\\__CsvColumns__";
                ////if (!Directory.Exists(structureSpecificProximityStatsDirectory_randomizedCsvCols))
                ////{
                ////    Directory.CreateDirectory(structureSpecificProximityStatsDirectory_randomizedCsvCols);
                ////}
                //string structureSpecificDirectory = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanData__\\__StructureSpecific__";
                //if (!Directory.Exists(structureSpecificDirectory))
                //{
                //    Directory.CreateDirectory(structureSpecificDirectory);
                //}
                //string structureSpecificDvhDataDirectory = structureSpecificDirectory + "\\__DvhData__";
                //if (!Directory.Exists(structureSpecificDvhDataDirectory))
                //{
                //    Directory.CreateDirectory(structureSpecificDvhDataDirectory);
                //}
                //string physicianSpecificDirectory = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanData__\\__PhysicianSpecific__" + mainControl.planName;
                //if (!Directory.Exists(physicianSpecificDirectory))
                //{
                //    Directory.CreateDirectory(physicianSpecificDirectory);
                //}
                //string physicianSpecificStructureDvhDirectory = physicianSpecificDirectory + "\\__StructureDvhData__";
                //if (!Directory.Exists(physicianSpecificStructureDvhDirectory))
                //{
                //    Directory.CreateDirectory(physicianSpecificStructureDvhDirectory);
                //}
                //string physicianSpecificPlanDvhDataDirectory = physicianSpecificDirectory + "\\__PlanDvhData__";
                //if (!Directory.Exists(physicianSpecificPlanDvhDataDirectory))
                //{
                //    Directory.CreateDirectory(physicianSpecificPlanDvhDataDirectory);
                //}

                #endregion

                #endregion
                //---------------------------------------------------------------------------------
                #region paths

                // json path
                string jsonPath = "";
                string jsonPath_randomized = "";
                jsonPath = patientSpecificProximityStatsDirectory + "\\" + pId + "_" + mainControl.planName + "_ProximityStatistics.json";
                jsonPath_randomized = patientSpecificProximityStatsDirectory_randomizedJson + "\\" + mainControl.randomId + "_" + mainControl.planName + "_Rand_ProximityStatistics.json";
                mainControl.jsonFilePath = jsonPath;
                mainControl.jsonFilePath_randomized = jsonPath_randomized;

                // csv path
                mainControl.rowsCsvFilePath_randomized_patientSpecific = patientSpecificProximityStatsDirectory_randomizedCsvRows;
                mainControl.colsCsvFilePath_randomized_patientSpecific = patientSpecificProximityStatsDirectory_randomizedCsvCols;
                mainControl.rowsCsvFilePath_randomized_structureSpecific = structureSpecificProximityStatsDirectory;
                mainControl.dashboardPath = dashboardDiretory;
				mainControl.rowsCsvFilePath_randomized_master = masterPlanDataDirectory;

				#endregion
				//---------------------------------------------------------------------------------
			}

            #endregion
            //---------------------------------------------------------------------------------
            #region organize structures into ordered lists
            // lists for structures
            List<Structure> gtvList = new List<Structure>();
            List<Structure> ctvList = new List<Structure>();
            List<Structure> itvList = new List<Structure>();
            List<Structure> ptvList = new List<Structure>();
            List<Structure> oarList = new List<Structure>();
            List<Structure> targetList = new List<Structure>();
            List<Structure> structureList = new List<Structure>();

            foreach (var structure in structureSet?.Structures)
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
                        gtvList.Add(structure);
                        structureList.Add(structure);
                        targetList.Add(structure);
                    }
                    if ((structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) ||
                        (structure.Id.StartsWith("Prost", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        ctvList.Add(structure);
                        structureList.Add(structure);
                        targetList.Add(structure);
                    }
                    if (structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase))
                    {
                        itvList.Add(structure);
                        structureList.Add(structure);
                        targetList.Add(structure);
                    }
                    if ((structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase)) ||
                        (structure.Id.StartsWith("cavity", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        ptvList.Add(structure);
                        structureList.Add(structure);
                        targetList.Add(structure);
                    }
                    // conditions for adding breast plan targets
                    if ((structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) ||
                        (structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) ||
                        (structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) ||
                        (structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        targetList.Add(structure);
                        structureList.Add(structure);
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
                        oarList.Add(structure);
                        structureList.Add(structure);
                    }
                }
            }
            mainControl.sorted_gtvList = gtvList?.OrderBy(x => x.Id);
            mainControl.sorted_ctvList = ctvList?.OrderBy(x => x.Id);
            mainControl.sorted_itvList = itvList?.OrderBy(x => x.Id);
            mainControl.sorted_ptvList = ptvList?.OrderBy(x => x.Id);
            mainControl.sorted_targetList = targetList?.OrderBy(x => x.Id);
            mainControl.sorted_oarList = oarList?.OrderBy(x => x.Id);
            mainControl.sorted_structureList = structureList?.OrderBy(x => x.Id);

            foreach (var s in mainControl.sorted_oarList)
            {
                if ((!s.ToString().ToLower().Contains("body")) &&
                    (!s.ToString().ToLower().Contains("external")))
                {
                    mainControl.structureList_LV.Items.Add(s/*.Id.ToString().Split(':').First()*/);
                }
            }
            mainControl.structureList_LV.SelectAll();


            #endregion structure organization and ordering
            //---------------------------------------------------------------------------------
            #region data populated on startup

            //---------------------------------------------------------------------------------
            #region proximity statistics

            foreach (var t in mainControl.sorted_ptvList)
            {
                TargetStats tInfo = new TargetStats();
                tInfo.target = t;
                tInfo.targetId = t.Id.ToString().Split(':').First();
                tInfo.targetVolume = (Math.Round(t.Volume, 3)).ToString();
                int numSegments = t.GetNumberOfSeparateParts();
                if (numSegments == 1)
                    tInfo.segments = string.Format("1");
                else { tInfo.segments = string.Format(">1"); }

                mainControl.TargetStats_DG.Items.Add(tInfo);
            }

            #endregion
            //---------------------------------------------------------------------------------

            #endregion
            //---------------------------------------------------------------------------------
            #region log

            if (mainControl.isGrady == false)
            {
                mainControl.LogUser(mainControl.script);
            }

            #endregion
            //---------------------------------------------------------------------------------
        }
    }
}
