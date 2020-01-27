namespace VMS.TPS
{
  using VMS.TPS.Common.Model.API;
  using VMS.TPS.Common.Model.Types;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Collections;
  using System.IO;
  using System.Text;

  /// <summary>
  /// A collection of methods used for collecting plan data in both JSON and HTML format.
  /// </summary>
  public class GetPlanData
  {
    public static void recordDvhDataForCurrentPlan(string viewDvhPath_searchableJson, string patientName, string course, PlanSetup currentPlan, IEnumerable<Structure> sorted_structureList, string patientId, string randomId, string primaryPhysician)
    {
      string courseHeader = course.Split('-').Last().Replace(" ", "_");
      // force abs dose
      currentPlan.DoseValuePresentation = DoseValuePresentation.Absolute;
      // variables
      double planMaxDose = 0;
      if (currentPlan.Dose != null)
      {
        planMaxDose = Math.Round(currentPlan.Dose.DoseMax3D.Dose, 3);
      }
      else { planMaxDose = Double.NaN; }

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
      string patientSpecificDirectory = planDataDirectory + "\\_PatientSpecific_\\" + patientId + "\\" + course;
      if (!Directory.Exists(patientSpecificDirectory))
      {
        Directory.CreateDirectory(patientSpecificDirectory);
      }

      string htmlDashboardDirectory = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanDataDashboard__\\_HTML_";
      if (!Directory.Exists(htmlDashboardDirectory))
      {
        Directory.CreateDirectory(htmlDashboardDirectory);
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

      // patientSpecificDvhDataDirectory
      string patientSpecificDvhDataDirectory = patientSpecificDirectory + "\\DvhData";
      if (!Directory.Exists(patientSpecificDvhDataDirectory))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory);
      }

      // patientSpecificDvhDataDirectory_plans
      string patientSpecificDvhDataDirectory_plans = patientSpecificDvhDataDirectory + "\\Plans";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_plans))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_plans);
      }

      // patientSpecificDvhDataDirectory_sums
      string patientSpecificDvhDataDirectory_sums = patientSpecificDvhDataDirectory + "\\Sums";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_sums))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_sums);
      }

      // patientSpecificDvhDataDirectory_randomized
      string patientSpecificDvhDataDirectory_randomized = patientSpecificDvhDataDirectory + "\\Randomized";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_randomized))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomized);
      }

      // patientSpecificDvhDataDirectory_randomizedJson
      string patientSpecificDvhDataDirectory_randomizedJson = patientSpecificDvhDataDirectory_randomized + "\\JSON";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedJson))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedJson);
      }
      string finalDvhJsonPath_patientSpecific = patientSpecificDvhDataDirectory_plans;
      string finalDvhJsonPath_randomizedJson_patientSpecific = patientSpecificDvhDataDirectory_randomizedJson;

      // patientSpecificDvhDataDirectory_randomizedCsvRows
      string patientSpecificDvhDataDirectory_randomizedCsvRows = patientSpecificDvhDataDirectory_randomized + "\\CsvRows";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedCsvRows))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedCsvRows);
      }

      // patientSpecificDvhDataDirectory_randomizedCsvCols
      string patientSpecificDvhDataDirectory_randomizedCsvCols = patientSpecificDvhDataDirectory_randomized + "\\CsvCols";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedCsvCols))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedCsvCols);
      }

      #endregion

      #endregion

      #region physician specific

      // physician folder
      string physicianSpecificDirectory = planDataDirectory + "\\_PhysicianSpecific_\\" + primaryPhysician;
      if (!Directory.Exists(physicianSpecificDirectory))
      {
        Directory.CreateDirectory(physicianSpecificDirectory);
      }

      string physicianSpecificStructureDvhDirectory = physicianSpecificDirectory + "\\StructureDvhData\\" + courseHeader;
      if (!Directory.Exists(physicianSpecificStructureDvhDirectory))
      {
        Directory.CreateDirectory(physicianSpecificStructureDvhDirectory);
      }

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
      string structureSpecificProximityStatsDirectory = structureSpecificDirectory + "\\TargetProximityStats\\" + courseHeader;
      if (!Directory.Exists(structureSpecificProximityStatsDirectory))
      {
        Directory.CreateDirectory(structureSpecificProximityStatsDirectory);
      }

      // structure specific dvhdata
      string structureSpecificDvhDataDirectory = structureSpecificDirectory + "\\DvhData\\" + courseHeader;
      if (!Directory.Exists(structureSpecificDvhDataDirectory))
      {
        Directory.CreateDirectory(structureSpecificDvhDataDirectory);
      }

      // structure specific dvhdata json
      string structureSpecificDvhDataDirectory_randomizedJson = structureSpecificDvhDataDirectory + "\\JSON";
      if (!Directory.Exists(structureSpecificDvhDataDirectory_randomizedJson))
      {
        Directory.CreateDirectory(structureSpecificDvhDataDirectory_randomizedJson);
      }

      // structure specific dvhdata csv
      string structureSpecificDvhDataDirectory_randomizedCsvRows = structureSpecificDvhDataDirectory + "\\CsvRows";
      if (!Directory.Exists(structureSpecificDvhDataDirectory_randomizedCsvRows))
      {
        Directory.CreateDirectory(structureSpecificDvhDataDirectory_randomizedCsvRows);
      }

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

      #endregion

      StringBuilder dvhData_randomizedCsvCols_SB = new StringBuilder();
      StringBuilder dvhData_randomizedCsvRows_SB = new StringBuilder();
      StringBuilder dvhData_randomizedCsvRows_SB_master = new StringBuilder();
      StringBuilder dvhData_structureSpecificCsvRows_SB = new StringBuilder();
      StringBuilder dvhData_pysicianAndStructureSpecificCsvRows_SB = new StringBuilder();

      string jsonStringForViewDvh = "[{\"patientName\":\"" + patientName + "\", " +
                                      "\"patientId\":\"" + patientId + "\", " +
                                      "\"randomId\":\"" + randomId + "\", " +
                                      "\"primaryPhysician\":\"" + primaryPhysician + "\", " +
                                      "\"courseId\":\"" + course + "\", " +
                                      "\"courseHeader\":\"" + courseHeader + "\"," +
                                      "\"planData\":[" +
                                      "{\"planId\":\"" + currentPlan.Id + "\"," +
                                      "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
                                      "\"planMaxDose\":" + planMaxDose + "," +
                                      "\"structureData\":[";

      string plansJsonArray_randomized = "[{\"randomId\":\"" + randomId + "\", " +
                                          "\"primaryPhysician\":\"" + primaryPhysician + "\", " +
                                          "\"courseId\":\"" + course + "\", " +
                                          "\"courseHeader\":\"" + courseHeader + "\"," +
                                          "\"planData\":[" +
                                          "{\"planId\":\"" + currentPlan.Id + "\"," +
                                          "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
                                          "\"planMaxDose\":" + planMaxDose + "," +
                                          "\"structureData\":[";

      //jsonStringForViewDvh = "{\"primaryPhysician\":\"" + primaryPhysician + "\"," + 
      //                        "\"planId\":\"" + currentPlan.Id + "\"," +
      //                        "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
      //                        "\"planMaxDose\":" + planMaxDose + "," +
      //                        "\"structureData\":[";


      foreach (var s in sorted_structureList)
      {
        // clear string builders
        dvhData_randomizedCsvCols_SB.Clear();
        dvhData_randomizedCsvRows_SB.Clear();
        dvhData_randomizedCsvRows_SB_master.Clear();
        dvhData_structureSpecificCsvRows_SB.Clear();
        dvhData_pysicianAndStructureSpecificCsvRows_SB.Clear();

        // variables
        string lowerId = s.Id.ToString().ToLower().Replace(" ", string.Empty).Replace("\\", "_").Replace(".", "_").Replace("/", "_").Split(':').First();
        lowerId = lowerId.Replace('/', '_');
        lowerId = lowerId.Replace('\\', '_');
        double volume = Math.Round(s.Volume, 3);
        string color = "#" + s.Color.ToString().Substring(3, 6);
        string structureSpecificJsonString = string.Empty;

        // define final paths
        string finalDvhCsvPath_randomizedCsvCols_patientSpecific = patientSpecificDvhDataDirectory_randomizedCsvCols + "\\" + courseHeader + "_" + lowerId + "_DVH_col.csv";
        string finalDvhCsvPath_randomizedCsvRows_patientSpecific = patientSpecificDvhDataDirectory_randomizedCsvRows + "\\" + courseHeader + "_" + lowerId + "_DVH_row.csv";
        string finalDvhCsvPath_randomizedCsvRows_masterData = masterPlanDataDirectory + "\\AllPlansDvhData_rows.csv";
        //string finalDvhCsvPath_randomizedCsvCols_physSpec =  physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns + "\\" + lowerId + "_cols.csv";
        //string finalDvhCsvPath_randomizedCsvRows_physSpec = physicianSpecificPlanDvhDataDirectory_randomizedCsvRows + "\\" + lowerId + "_rows.csv";
        string finalDvhCsvPath_randomizedCsvRows_structureSpec = structureSpecificDvhDataDirectory_randomizedCsvRows + "\\" + courseHeader + "_" + lowerId + "_DVH_row.csv";
        string finalDvhCsvPath_randomizedCsvRows_physAndStructureSpec = physicianSpecificStructureDvhDirectory + "\\" + courseHeader + "_" + lowerId + "_DVH_row.csv";
        finalDvhJsonPath_patientSpecific = patientSpecificDvhDataDirectory_plans + "\\" + currentPlan.Id + "_DVH.json";
        finalDvhJsonPath_randomizedJson_patientSpecific = patientSpecificDvhDataDirectory_randomizedJson + "\\" + currentPlan.Id + "_DVH.json";

        // structure specific json
        string finalDvhJsonPath_randomizedJson_structureSpec = structureSpecificDvhDataDirectory_randomizedJson + "\\" + courseHeader + "_" + lowerId + "_DVH.json";

        if (!File.Exists(finalDvhJsonPath_randomizedJson_structureSpec))
        {
          structureSpecificJsonString = "[";
        }
        else
        {
          using (StreamReader streamReader = new StreamReader(finalDvhJsonPath_randomizedJson_structureSpec, Encoding.UTF8))
          {
            structureSpecificJsonString = streamReader.ReadToEnd();
          }
          //structureSpecificJsonString = File.ReadAllText(finalDvhJsonPath_randomizedJson_structureSpec);
          structureSpecificJsonString = structureSpecificJsonString.TrimEnd(']');
          structureSpecificJsonString = structureSpecificJsonString + ",";
        }
        //if (!File.Exists(finalDvhJsonPath_randomizedJson_structureSpec))
        //{
        //    structureSpecificJsonString = "[";
        //}
        //else
        //{
        //    structureSpecificJsonString = ",";
        //}

        // dvh data
        DVHData dvhAR = currentPlan.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
        DVHData dvhAA = currentPlan.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

        // define strings
        //dvhCsvStringBuilder.AppendLine("patientId: " + patientId);
        dvhData_randomizedCsvCols_SB.AppendLine("randomId,\t" + randomId);
        dvhData_randomizedCsvCols_SB.AppendLine("primaryPhysician,\t" + primaryPhysician);
        dvhData_randomizedCsvCols_SB.AppendLine("courseHeader,\t" + courseHeader);
        dvhData_randomizedCsvCols_SB.AppendLine("planId,\t" + currentPlan.Id);
        dvhData_randomizedCsvCols_SB.AppendLine("approvalStatus," + currentPlan.ApprovalStatus);
        dvhData_randomizedCsvCols_SB.AppendLine("planMaxDose,\t" + planMaxDose);
        dvhData_randomizedCsvCols_SB.AppendLine("structureId,\t" + lowerId);
        dvhData_randomizedCsvCols_SB.AppendLine("structureColor,\t" + color);
        dvhData_randomizedCsvCols_SB.AppendLine("structureVolume," + volume);

        if (dvhAR != null && dvhAR.CurveData.Length > 0)
        {
          var doseList = new List<string>();
          var relVolumeList = new List<string>();

          double min03 = Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (volume - 0.03)), 3);
          double minDose = Math.Round(dvhAA.MinDose.Dose, 3);
          double meanDose = Math.Round(dvhAR.MeanDose.Dose, 3);
          double max03 = Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (0.03)), 3);
          double maxDose = Math.Round(dvhAR.MaxDose.Dose, 3);
          double medianDose = Math.Round(dvhAR.MedianDose.Dose, 3);
          double std = Math.Round(dvhAR.StdDev, 3);

          // json string to return
          jsonStringForViewDvh = jsonStringForViewDvh + "{\"structureId\":\"" + s.Id + "\"," +
                                                          "\"structureColor\":\"" + color + "\"," +
                                                          "\"structureVolume\":" + volume + "," +
                                                          "\"min03\":" + min03 + "," +
                                                          "\"minDose\":" + minDose + "," +
                                                          "\"meanDose\":" + meanDose + "," +
                                                          "\"max03\":" + max03 + "," +
                                                          "\"maxDose\":" + maxDose + "," +
                                                          "\"medianDose\":" + medianDose + "," +
                                                          "\"std\":" + std + "," +
                                                          "\"dvh\":[";
          plansJsonArray_randomized = plansJsonArray_randomized + "{\"structureId\":\"" + s.Id + "\"," +
                                                                  "\"structureColor\":\"" + color + "\"," +
                                                                  "\"structureVolume\":" + volume + "," +
                                                                  "\"min03\":" + min03 + "," +
                                                                  "\"minDose\":" + minDose + "," +
                                                                  "\"meanDose\":" + meanDose + "," +
                                                                  "\"max03\":" + max03 + "," +
                                                                  "\"maxDose\":" + maxDose + "," +
                                                                  "\"medianDose\":" + medianDose + "," +
                                                                  "\"std\":" + std + "," +
                                                                  "\"dvh\":[";
          // json string to write
          structureSpecificJsonString = structureSpecificJsonString + "{\"structureData\":[" +
                                                                      "{\"randomId\":\"" + randomId + "\"," +
                                                                      "\"primaryPhysician\":\"" + primaryPhysician + "\"," +
                                                                      "\"courseHeader\":\"" + courseHeader + "\"," +
                                                                      "\"planId\":\"" + currentPlan.Id + "\"," +
                                                                      "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
                                                                      "\"planMaxDose\":" + planMaxDose + "," +
                                                                      "\"structureId\":\"" + s.Id + "\"," +
                                                                      "\"structureColor\":\"" + color + "\"," +
                                                                      "\"structureVolume\":" + volume + "," +
                                                                      "\"min03\":" + min03 + "," +
                                                                      "\"minDose\":" + minDose + "," +
                                                                      "\"meanDose\":" + meanDose + "," +
                                                                      "\"max03\":" + max03 + "," +
                                                                      "\"maxDose\":" + maxDose + "," +
                                                                      "\"medianDose\":" + medianDose + "," +
                                                                      "\"std\":" + std + "," +
                                                                      "\"dvh\":[";
          dvhData_randomizedCsvCols_SB.AppendLine("min03,\t\t" + min03);
          dvhData_randomizedCsvCols_SB.AppendLine("minDose,\t" + minDose);
          dvhData_randomizedCsvCols_SB.AppendLine("max03,\t\t" + max03);
          dvhData_randomizedCsvCols_SB.AppendLine("maxDose,\t" + maxDose);
          dvhData_randomizedCsvCols_SB.AppendLine("meanDose,\t" + meanDose);
          dvhData_randomizedCsvCols_SB.AppendLine("medianDose,\t" + medianDose);
          dvhData_randomizedCsvCols_SB.AppendLine("std,\t\t" + std);
          dvhData_randomizedCsvCols_SB.AppendLine("dvh:");
          dvhData_randomizedCsvCols_SB.AppendLine("Dose(Gy),\tVolume(cc),\tVolume(pct)");

          for (double i = 0; i <= dvhAR.MaxDose.Dose + .1; i += .1)
          {
            string dose = string.Format("{0:N1}", i);
            string relVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString();
            string absVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAA, i), 4).ToString();

            // json strings for viewDvh and randomized plandata
            jsonStringForViewDvh = jsonStringForViewDvh + "[" + dose + "," + relVolAtDose + "],";
            plansJsonArray_randomized = plansJsonArray_randomized + "[" + dose + "," + relVolAtDose + "],";
            // json string to write
            structureSpecificJsonString = structureSpecificJsonString + "[" + dose + "," + relVolAtDose + "],";
            // csv string to write
            dvhData_randomizedCsvCols_SB.AppendLine(string.Format("{0},\t\t{1},\t\t{2}", dose, absVolAtDose, relVolAtDose));
            // lists for csv string rows
            doseList.Add("V" + dose);
            relVolumeList.Add(relVolAtDose);
          }
          string doseListString = string.Join(",", doseList.ToArray());
          string relVolumeListString = string.Join(",", relVolumeList.ToArray());

          string headers = "RandomId,PrimaryPhysician,courseHeader,PlanId,ApprovalStatus,PlanMaxDose,StructureId,StructureVolume,Min03,MinDose,Max03,MaxDose,MeanDose,MedianDose,Std," + doseListString;
          var data = randomId + "," + primaryPhysician + "," + courseHeader + "," + currentPlan.Id + "," + currentPlan.ApprovalStatus + "," + planMaxDose + "," + lowerId + "," +
                                    volume + "," + min03 + "," + minDose + "," + max03 + "," + maxDose + "," + meanDose + "," + medianDose + "," + std + "," + relVolumeListString;

          // append csv headers
          dvhData_randomizedCsvRows_SB.AppendLine(headers);
          if (!File.Exists(finalDvhCsvPath_randomizedCsvRows_masterData))
          {
            dvhData_randomizedCsvRows_SB_master.AppendLine(headers);
          }
          if (!File.Exists(finalDvhCsvPath_randomizedCsvRows_structureSpec))
          {
            dvhData_structureSpecificCsvRows_SB.AppendLine(headers);
          }
          if (!File.Exists(finalDvhCsvPath_randomizedCsvRows_physAndStructureSpec))
          {
            dvhData_pysicianAndStructureSpecificCsvRows_SB.AppendLine(headers);
          }

          // append csv data
          dvhData_randomizedCsvRows_SB.AppendLine(data);
          dvhData_randomizedCsvRows_SB_master.AppendLine(data);
          dvhData_structureSpecificCsvRows_SB.AppendLine(data);
          dvhData_pysicianAndStructureSpecificCsvRows_SB.AppendLine(data);

          // json string to return
          jsonStringForViewDvh = jsonStringForViewDvh.TrimEnd(',');
          jsonStringForViewDvh = jsonStringForViewDvh + "]},";
          // plan specific json array
          plansJsonArray_randomized = plansJsonArray_randomized.TrimEnd(',');
          plansJsonArray_randomized = plansJsonArray_randomized + "]},";
          // json string to write
          structureSpecificJsonString = structureSpecificJsonString.TrimEnd(',');
          structureSpecificJsonString = structureSpecificJsonString + "]}]}]";
        }
        // structure specific json
        //string finalDvhJsonPath_randomizedJson_structureSpec = patientSpecificDvhDataDirectory_randomizedJson + "\\" + lowerId + ".json";
        //string patientSpecificDvhDataDirectory_randomizedJson = structureSpecificDvhDataDirectory_randomizedJson + "\\" + lowerId + ".json";

        // write files
        File.WriteAllText(finalDvhCsvPath_randomizedCsvCols_patientSpecific, dvhData_randomizedCsvCols_SB.ToString());
        File.WriteAllText(finalDvhCsvPath_randomizedCsvRows_patientSpecific, dvhData_randomizedCsvRows_SB.ToString());
        File.AppendAllText(finalDvhCsvPath_randomizedCsvRows_masterData, dvhData_randomizedCsvRows_SB_master.ToString());
        // physician specific csv files
        //File.AppendAllText(finalDvhCsvPath_randomizedCsvCols_physSpec, dvhData_randomizedCsvCols_SB.ToString());
        //File.AppendAllText(finalDvhCsvPath_randomizedCsvRows_physSpec, dvhData_randomizedCsvRows_SB.ToString());
        // structure specific csv files
        File.AppendAllText(finalDvhCsvPath_randomizedCsvRows_structureSpec, dvhData_structureSpecificCsvRows_SB.ToString());
        File.AppendAllText(finalDvhCsvPath_randomizedCsvRows_physAndStructureSpec, dvhData_pysicianAndStructureSpecificCsvRows_SB.ToString());
        // structure specific json file
        File.WriteAllText(finalDvhJsonPath_randomizedJson_structureSpec, structureSpecificJsonString);
      }
      jsonStringForViewDvh = jsonStringForViewDvh.TrimEnd(',');
      jsonStringForViewDvh = jsonStringForViewDvh + "]}]}]";

      // plan specific json array
      plansJsonArray_randomized = plansJsonArray_randomized.TrimEnd(',');
      plansJsonArray_randomized = plansJsonArray_randomized + "]}]}]";

      // write json file for ViewDvh folder
      File.WriteAllText(viewDvhPath_searchableJson, jsonStringForViewDvh);
      File.WriteAllText(finalDvhJsonPath_patientSpecific, jsonStringForViewDvh);
      File.WriteAllText(finalDvhJsonPath_randomizedJson_patientSpecific, plansJsonArray_randomized);

      #region write html

      string htmlPath = htmlDashboardDirectory + "\\" + DateTime.Now.ToShortDateString().Replace('/', '_') + "_" + courseHeader + "_r" + randomId + ".html";
      StreamWriter stream = new StreamWriter(htmlPath);
      string varPlanJSONArray = "var PlanJSONArray = " + plansJsonArray_randomized;

      using (stream)
      {
        stream.WriteLine(@"<!DOCTYPE html>");
        stream.WriteLine(@"<script src = 'https://ajax.googleapis.com/ajax/libs/jquery/3.1.1/jquery.min.js'></script>");
        stream.WriteLine(@"<link href='https://fonts.googleapis.com/css?family=PT+Sans' rel='stylesheet'>");
        stream.WriteLine(@"<html>");
        stream.WriteLine(@"<head>");
        stream.WriteLine(@"<meta charset='utf-8'/>");
        stream.WriteLine(@"<link rel='stylesheet' type='text/css' href='S:\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanDataDashboard__\\__Style__\\PageStyle.css'>");
        stream.WriteLine(@"<title>DVH Review - " + currentPlan.Id + "</title>");
        stream.WriteLine(@"</head>");
        stream.WriteLine(@"<body>");
        stream.WriteLine(@"<div class = 'planInfoDisplay' id = 'infoBlockDislay'>");
        stream.WriteLine(@"<button id = 'print' class='pBtn btn-primary btn-sml' onclick='PrepareAndPrint()'>Print</button>");
        stream.WriteLine(@"<p>Physician:<span class='tabPhysician'>" + primaryPhysician + "</span></p>");
        stream.WriteLine(@"<p>CurrentPlan:<span class='tabPlanName'>" + currentPlan.Id + "</span></p>");
        stream.WriteLine(@"<p>ApprovalStatus:<span class='tabPlanStatus'>" + currentPlan.ApprovalStatus + "</span></p>");
        stream.WriteLine(@"</div>");
        stream.WriteLine(@"<div id='dvh'></div>");
        stream.WriteLine(@"</div>");
        stream.WriteLine(@"<div id='planStats'></div>");
        stream.WriteLine(@"</div>");
        stream.WriteLine(@"<div id='structureStats'></div>");
        stream.WriteLine(@"</body>");
        stream.WriteLine(@"</html>");
        stream.WriteLine(@"<script>");
        stream.WriteLine(varPlanJSONArray);
        stream.WriteLine(@"$(document).ready(function () {
                                                            var options1 = {

                                                            chart:
                                                                {
                                                                renderTo: 'dvh',
                                                                    type: 'line',
                                                                    zoomType: 'xy',
                                                                    panning: true,
                                                                    panKey: 'shift',
                                                                },
                                                                exporting:
                                                                {
                                                                buttons:
                                                                    {
                                                                    contextButton:
                                                                        {
                                                                        enabled: false
                                                                        }
                                                                    }
                                                                },
                                                                xAxis:
                                                                {
                                                                title:
                                                                    {
                                                                    text: 'Dose (Gy)'
                                                                    },
                                                                    crosshair: true,
                                                                    maxPadding: 0.02
                                                                },
                                                                plotOptions:
                                                                {
                                                                series:
                                                                    {
                                                                    marker:
                                                                        {
                                                                        enabled: false
                                                                        },
                                                                        allowPointSelect: true,
                                                                        states:
                                                                        {
                                                                        hover:
                                                                            {
                                                                            enabled: true,
                                                                                lineWidth: 5
                                                                            }
                                                                        }
                                                                    },
                                                                    boxplot:
                                                                    {
                                                                    fillColor: '#505053'
                                                                    },
                                                                    candlestick:
                                                                    {
                                                                    lineColor: 'white'
                                                                    },
                                                                    errorbar:
                                                                    {
                                                                    color: 'white'
                                                                    }
                                                                },
                                                                yAxis:
                                                                {
                                                                labels:
                                                                    {
                                                                    format: '{value} %'
                                                                    },
                                                                    floor: 0,
                                                                    ceiling: 100,
                                                                    title:
                                                                    {
                                                                    text: 'Volume (%)'
                                                                    },
                                                                    crosshair: true,
                                                                    gridLineDashStyle: 'ShortDash',
                                                                    gridLineColor: '#aaaaaa'
                                                                },
                                                                tooltip:
                                                                {
                                                                shared: true,
                                                                    useHTML: true,
                                                                    headerFormat: '<table>',
                                                                    pointFormat: '<tr><td style=\""color:{series.color}; text-shadow: 0px 0 #353839, 0 1px #353839, 1px 0 #353839, 0 0px #353839;\"">{series.name}: </td><td style=\""text-align: left; color:#282827\"">V{point.x} Gy = {point.y} %</td></tr>',
                                                                    footerFormat: '</table>',
                                                                },
                                                                title:
                                                                {
                                                                text: 'DVH',
                                                                    x: -150
                                                                },
                                                                subtitle:
                                                                {
                                                                text: 'Click and drag to zoom in. Hold down shift key to pan.',
                                                                    x: -150
                                                                },
                                                                legend:
                                                                {
                                                                layout: 'vertical',
                                                                    align: 'right',
                                                                    verticalAlign: 'middle',
                                                                    borderWidth: 0,
                                                                    floating: false,
                                                                    width: 420,
                                                                    itemWidth: 210,
                                                                    itemStyle:
                                                                    {
                                                                    width: 205
                                                                    },
                                                                    itemHiddenStyle:
                                                                    {
                                                                    color: '#ff4d4d'
                                                                    }
                                                                },

                                                                series: seriesOptions
                                                            };

                                                            var options2 = {

                                                                chart: {
                                                            renderTo: 'planStats',
                                                                    type: 'column',
                                                                    panning: true,
                                                                    panKey: 'shift',
                                                                },
                                                                exporting:
                                                            {
                                                            buttons:
                                                                {
                                                                contextButton:
                                                                    {
                                                                    enabled: false
                                                                        }
                                                                }
                                                            },
                                                                xAxis:
                                                            {
                                                            categories: ['MinDose', 'Min(0.03cc)', 'MeanDose', 'MedianDose', 'Max(0.03cc)', 'MaxDose'],
                                                                    title:
                                                                {
                                                                text: 'DoseStatistic'
                                                                    },
                                                                    crosshair: true,
                                                                    maxPadding: 0.02
                                                                },
                                                                plotOptions:
                                                            {
                                                            series:
                                                                {
                                                                marker:
                                                                    {
                                                                    enabled: false
                                                                        },
                                                                        allowPointSelect: true,
                                                                        states:
                                                                    {
                                                                    hover:
                                                                        {
                                                                        enabled: true,
                                                                                lineWidth: 5
                                                                            }
                                                                    }
                                                                },
                                                                    boxplot:
                                                                {
                                                                fillColor: '#505053'
                                                                    },
                                                                    candlestick:
                                                                {
                                                                lineColor: 'white'
                                                                    },
                                                                    errorbar:
                                                                {
                                                                color: 'white'
                                                                    }
                                                            },
                                                                yAxis:
                                                            {
                                                            floor: 0,
                                                                    title:
                                                                {
                                                                text: 'Dose (Gy)'
                                                                    },
                                                                    crosshair: true,
                                                                    gridLineDashStyle: 'ShortDash',
                                                                    gridLineColor: '#aaaaaa'
                                                                },
                                                                tooltip:
                                                            {
                                                            shared: true,
                                                                    useHTML: true,
                                                                    headerFormat: '<table>',
                                                                    pointFormat: '<tr><td style=\""color:{series.color}; text-shadow: 0px 0 #353839, 0 1px #353839, 1px 0 #353839, 0 0px #353839;\"">{series.name}: </td><td style=\""text-align: right; color:#282827\"">{point.y:.3f} Gy</td></tr>',
                                                                    footerFormat: '</table>',
                                                                },
                                                                title:
                                                            {
                                                            text: 'Structure Dose Statistics',
                                                                    x: -150
                                                                },
                                                                subtitle:
                                                            {
                                                            text: '',
                                                                    x: -150
                                                                },
                                                                legend:
                                                            {
                                                            layout: 'vertical',
                                                                    align: 'right',
                                                                    verticalAlign: 'middle',
                                                                    borderWidth: 0,
                                                                    floating: false,
                                                                    width: 420,
                                                                    itemWidth: 210,
                                                                    itemStyle:
                                                                {
                                                                width: 205
                                                                    },
                                                                    itemHiddenStyle:
                                                                {
                                                                color: '#ff4d4d'
                                                                    }
                                                            },

                                                                series: seriesOptions2
                                                            };

                                                        var options3 = {

                                                                chart: {
                                                                    renderTo: 'structureStats',
                                                                    type: 'column',
                                                                    //zoomType: 'xy',
                                                                    panning: true,
                                                                    panKey: 'shift',
                                                                },
                                                                exporting:
                                                        {
                                                        buttons:
                                                            {
                                                            contextButton:
                                                                {
                                                                enabled: false
                                                                        }
                                                            }
                                                        },
                                                                xAxis:
                                                        {
                                                        categories: [''],
                                                                    title:
                                                            {
                                                            text: 'StructureVolume'
                                                                    },
                                                                    crosshair: true,
                                                                    maxPadding: 0.02
                                                                },
                                                                plotOptions:
                                                        {
                                                        series:
                                                            {
                                                            marker:
                                                                {
                                                                enabled: false
                                                                        },
                                                                        allowPointSelect: true,
                                                                        states:
                                                                {
                                                                hover:
                                                                    {
                                                                    enabled: true,
                                                                                lineWidth: 5
                                                                            }
                                                                }
                                                            },
                                                                    boxplot:
                                                            {
                                                            fillColor: '#505053'
                                                                    },
                                                                    candlestick:
                                                            {
                                                            lineColor: 'white'
                                                                    },
                                                                    errorbar:
                                                            {
                                                            color: 'white'
                                                                    }
                                                        },
                                                                yAxis:
                                                        {
                                                        floor: 0,
                                                                    title:
                                                            {
                                                            text: 'Volume (cc)'
                                                                    },
                                                                    crosshair: true,
                                                                    gridLineDashStyle: 'ShortDash',
                                                                    gridLineColor: '#aaaaaa'
                                                                },
                                                                tooltip:
                                                        {
                                                        shared: true,
                                                                    useHTML: true,
                                                                    headerFormat: '<table>',
                                                                    pointFormat: '<tr><td style=\""color:{series.color}; text-shadow: 0px 0 #353839, 0 1px #353839, 1px 0 #353839, 0 0px #353839;\"">{series.name}: </td><td style=\""text-align: right; color:#282827\"">{point.y:.3f} cc</td></tr>',
                                                                    footerFormat: '</table>',
                                                                },
                                                                title:
                                                        {
                                                        text: 'Structure Statistics',
                                                                    x: -150
                                                                },
                                                                subtitle:
                                                        {
                                                        text: '',
                                                                    x: -150
                                                                },
                                                                legend:
                                                        {
                                                        layout: 'vertical',
                                                                    align: 'right',
                                                                    verticalAlign: 'middle',
                                                                    borderWidth: 0,
                                                                    floating: false,
                                                                    width: 420,
                                                                    itemWidth: 210,
                                                                    itemStyle:
                                                            {
                                                            width: 205
                                                                    },
                                                                    itemHiddenStyle:
                                                            {
                                                            color: '#ff4d4d'
                                                                    }
                                                        },

                                                                series: seriesOptions3
                                                            };

                                                    

                                                    var chart1 = new Highcharts.Chart(options1);
                                                    var chart2 = new Highcharts.Chart(options2);
                                                    var chart3 = new Highcharts.Chart(options3);

                                                });

                                                var seriesOptions = [],
                                                    seriesOptions2 = [],
                                                    seriesOptions3 = [],
                                                    seriesOptions4 = [],
                                                    dashStyles = [
                                                        'Solid',
                                                        'ShortDash',
                                                        'ShortDot',
                                                        'ShortDashDot',
                                                        'ShortDashDotDot',
                                                        'Dot',
                                                        'Dash',
                                                        'LongDash',
                                                        'DashDot',
                                                        'LongDashDot',
                                                        'LongDashDotDot'
                                                    ]

                                                var planData = PlanJSONArray[0].planData,
                                                    seriesCounter = 0,
                                                    planCounter = 0,
                                                    counter = 0

                                                planData.forEach(function (element, i) {

                                                    planCounter += seriesCounter
                                                    counter += 1

                                                    element.structureData.forEach(function (childElement, j) {
                                                        if ((element.structureData[j].structureId != 'Body') &&
                                                            (element.structureData[j].structureId != 'BODY') &&
                                                            (element.structureData[j].structureId != 'External') &&
                                                            (element.structureData[j].structureId != 'EXTERNAL'))    {

                                                            seriesOptions[planCounter] = {
                                                                id: element.planId,
                                                                name: element.planId/* + '_' + element.structureData[0].structureId*/,
                                                                data: /*element.structureData[0].dvh*/[],
                                                                dashStyle: dashStyles[planCounter],
                                                                visible: true,
                                                                color: element.structureData[0].structureColor /*'white'*/
                                                                                                                //linkedTo: ':previous'
                                                }

                                                seriesOptions[counter] = {
                                                                        name: element.planId + '_' + element.structureData[j].structureId,
                                                                        data: element.structureData[j].dvh,
                                                                        dashStyle: dashStyles[i],
                                                                        visible: true,
                                                                        color: element.structureData[j].structureColor,
                                                                        linkedTo: element.planId
                                                                    }

                                                //seriesOptions2[seriesCounter] = {
                                                //    //task: element.subTaskId,
                                                //    name: element.planId + '_' + element.structureData[j].structureId,
                                                //    data: [element.structureData[j].minDose, element.structureData[j].min03, element.structureData[j].meanDose,
                                                //            element.structureData[j].medianDose, element.structureData[j].max03, element.structureData[j].maxDose],
                                                //    dashStyle: dashStyles[i],
                                                //    visible: true,
                                                //    color: element.structureData[j].structureColor
                                                //}

                                                seriesOptions2[planCounter] = {
                                                    id: element.planId,
                                                    name: element.planId/* + '_' + element.structureData[0].structureId*/,
                                                    data: /*element.structureData[0].dvh*/[],
                                                    dashStyle: dashStyles[planCounter],
                                                    visible: true,
                                                    color: element.structureData[0].structureColor /*'white'*/
                                                    //linkedTo: ':previous'
                                                }

                                                seriesOptions2[counter] = {
                                                    name: element.planId + '_' + element.structureData[j].structureId,
                                                    data: [element.structureData[j].minDose, element.structureData[j].min03, element.structureData[j].meanDose,
                                                    element.structureData[j].medianDose, element.structureData[j].max03, element.structureData[j].maxDose],
                                                    dashStyle: dashStyles[i],
                                                    visible: true,
                                                    color: element.structureData[j].structureColor,
                                                    linkedTo: element.planId
                                                }

                                                //seriesOptions3[seriesCounter] = {
                                                //    //task: element.subTaskId,
                                                //    name: element.structureData[j].structureId,
                                                //    data: [element.structureData[j].structureVolume],
                                                //    dashStyle: dashStyles[i],
                                                //    visible: true,
                                                //    color: element.structureData[j].structureColor
                                                //}

                                                seriesOptions3[planCounter] = {
                                                    id: element.planId,
                                                    name: element.planId/* + '_' + element.structureData[0].structureId*/,
                                                    data: /*element.structureData[0].dvh*/[],
                                                    dashStyle: dashStyles[planCounter],
                                                    visible: true,
                                                    color: element.structureData[0].structureColor /*'white'*/
                                                    //linkedTo: ':previous'
                                                }

                                                seriesOptions3[counter] = {
                                                    name: element.structureData[j].structureId,
                                                    data: [element.structureData[j].structureVolume],
                                                    dashStyle: dashStyles[i],
                                                    visible: true,
                                                    color: element.structureData[j].structureColor,
                                                    linkedTo: element.planId
                                                }

                                                //seriesOptions3[seriesCounter] = {
                                                //    //task: element.subTaskId,
                                                //    name: element.structureData[j].structureId,
                                                //    data: [element.structureData[j].structureVolume],
                                                //    dashStyle: dashStyles[i],
                                                //    visible: true,
                                                //    color: element.structureData[j].structureColor
                                                //}


                                                seriesCounter += 1
                                                counter += 1
                                            }
                                        })

                                        planCounter += 1

                                    })
                                    function PrepareAndPrint()
                                    {
                                        $('.pBtn').remove();
                                        $('.Buttons').remove();
                                        window.print();
                                    }     
                                </script> ");
        stream.WriteLine(@"<div>");
        stream.WriteLine(@"<div>");
        stream.WriteLine(@"<script src = 'https://code.highcharts.com/highcharts.js'></script >");



        stream.Flush();
        stream.Close();
      }

      #endregion
    }
    public static void recordDvhDataForAllPlans(string viewDvhPath_searchableJson, string patientName, string course, IEnumerator plans, string patientId, string randomId, string primaryPhysician)
    {
      string courseHeader = course.Split('-').Last().Replace(" ", "_");
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
      string patientSpecificDirectory = planDataDirectory + "\\_PatientSpecific_\\" + patientId + "\\" + course;
      if (!Directory.Exists(patientSpecificDirectory))
      {
        Directory.CreateDirectory(patientSpecificDirectory);
      }

      string htmlDashboardDirectory = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanDataDashboard__\\_HTML_";
      if (!Directory.Exists(htmlDashboardDirectory))
      {
        Directory.CreateDirectory(htmlDashboardDirectory);
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

      // patientSpecificDvhDataDirectory
      string patientSpecificDvhDataDirectory = patientSpecificDirectory + "\\DvhData";
      if (!Directory.Exists(patientSpecificDvhDataDirectory))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory);
      }

      // patientSpecificDvhDataDirectory_plans
      string patientSpecificDvhDataDirectory_plans = patientSpecificDvhDataDirectory + "\\Plans";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_plans))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_plans);
      }
      string finalDvhJsonPath_patientSpecific = patientSpecificDvhDataDirectory_plans;

      // patientSpecificDvhDataDirectory_sums
      string patientSpecificDvhDataDirectory_sums = patientSpecificDvhDataDirectory + "\\Sums";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_sums))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_sums);
      }

      // patientSpecificDvhDataDirectory_randomized
      string patientSpecificDvhDataDirectory_randomized = patientSpecificDvhDataDirectory + "\\Randomized";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_randomized))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomized);
      }

      // patientSpecificDvhDataDirectory_randomizedJson
      string patientSpecificDvhDataDirectory_randomizedJson = patientSpecificDvhDataDirectory_randomized + "\\JSON";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedJson))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedJson);
      }
      string finalDvhJsonPath_randomizedJson_patientSpecific = patientSpecificDvhDataDirectory_randomizedJson;

      // patientSpecificDvhDataDirectory_randomizedCsvRows
      string patientSpecificDvhDataDirectory_randomizedCsvRows = patientSpecificDvhDataDirectory_randomized + "\\CsvRows";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedCsvRows))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedCsvRows);
      }

      // patientSpecificDvhDataDirectory_randomizedCsvCols
      string patientSpecificDvhDataDirectory_randomizedCsvCols = patientSpecificDvhDataDirectory_randomized + "\\CsvColumns";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedCsvCols))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedCsvCols);
      }

      #endregion

      #endregion

      #region physician specific

      // physician folder
      string physicianSpecificDirectory = planDataDirectory + "\\_PhysicianSpecific_\\" + primaryPhysician;
      if (!Directory.Exists(physicianSpecificDirectory))
      {
        Directory.CreateDirectory(physicianSpecificDirectory);
      }

      string physicianSpecificStructureDvhDirectory = physicianSpecificDirectory + "\\StructureDvhData\\" + courseHeader;
      if (!Directory.Exists(physicianSpecificStructureDvhDirectory))
      {
        Directory.CreateDirectory(physicianSpecificStructureDvhDirectory);
      }

      //string physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns = physicianSpecificDirectory + "\\_PlanDvhData_";
      //if (!Directory.Exists(physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns))
      //{
      //    Directory.CreateDirectory(physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns);
      //}

      //string physicianSpecificPlanDvhDataDirectory_randomizedCsvRows = physicianSpecificDirectory + "\\_PlanDvhData_";
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
      string structureSpecificProximityStatsDirectory = structureSpecificDirectory + "\\TargetProximityStats\\" + courseHeader;
      if (!Directory.Exists(structureSpecificProximityStatsDirectory))
      {
        Directory.CreateDirectory(structureSpecificProximityStatsDirectory);
      }

      // structure specific dvhdata
      string structureSpecificDvhDataDirectory = structureSpecificDirectory + "\\DvhData\\" + courseHeader;
      if (!Directory.Exists(structureSpecificDvhDataDirectory))
      {
        Directory.CreateDirectory(structureSpecificDvhDataDirectory);
      }

      // structure specific dvhdata json
      string structureSpecificDvhDataDirectory_randomizedJson = structureSpecificDvhDataDirectory + "\\JSON";
      if (!Directory.Exists(structureSpecificDvhDataDirectory_randomizedJson))
      {
        Directory.CreateDirectory(structureSpecificDvhDataDirectory_randomizedJson);
      }

      // structure specific dvhdata csv
      string structureSpecificDvhDataDirectory_randomizedCsvRows = structureSpecificDvhDataDirectory + "\\CsvRows";
      if (!Directory.Exists(structureSpecificDvhDataDirectory_randomizedCsvRows))
      {
        Directory.CreateDirectory(structureSpecificDvhDataDirectory_randomizedCsvRows);
      }

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

      #endregion

      string jsonStringForViewDvh = "[{\"patientName\":\"" + patientName + "\", " +
                                      "\"patientId\":\"" + patientId + "\", " +
                                      "\"randomId\":\"" + randomId + "\", " +
                                      "\"primaryPhysician\":\"" + primaryPhysician + "\", " +
                                      "\"courseId\":\"" + course + "\", " +
                                      "\"courseHeader\":\"" + courseHeader + "\"," +
                                      "\"planData\":[";
      string plansJsonArray_randomized = "[{\"randomId\":\"" + randomId + "\", " +
                                              "\"primaryPhysician\":\"" + primaryPhysician + "\", " +
                                              "\"courseId\":\"" + course + "\", " +
                                              "\"courseHeader\":\"" + courseHeader + "\"," +
                                              "\"planData\":[";
      while (plans.MoveNext())
      {
        PlanSetup currentPlan = (PlanSetup)plans.Current;

        finalDvhJsonPath_patientSpecific = finalDvhJsonPath_patientSpecific + "\\" + currentPlan.Id + "_DVH_randomized.json";
        finalDvhJsonPath_randomizedJson_patientSpecific = finalDvhJsonPath_randomizedJson_patientSpecific + "\\" + currentPlan.Id + ".json";

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

        #region physician specific directories (include plan name)

        //physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns = physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns + "\\" + currentPlan.Id + "\\CsvColumns";
        //physicianSpecificPlanDvhDataDirectory_randomizedCsvRows = physicianSpecificPlanDvhDataDirectory_randomizedCsvRows + "\\" + currentPlan.Id + "\\CsvRows";

        #endregion

        StringBuilder dvhData_randomizedCsvCols_SB = new StringBuilder();
        StringBuilder dvhData_randomizedCsvRows_SB = new StringBuilder();
        StringBuilder dvhData_randomizedCsvRows_SB_master = new StringBuilder();
        StringBuilder dvhData_structureSpecificCsvRows_SB = new StringBuilder();
        StringBuilder dvhData_pysicianAndStructureSpecificCsvRows_SB = new StringBuilder();


        jsonStringForViewDvh = jsonStringForViewDvh + "{\"planId\":\"" + currentPlan.Id + "\"," +
                                                        "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
                                                        "\"planMaxDose\":" + planMaxDose + "," +
                                                        "\"structureData\":[";


        plansJsonArray_randomized = plansJsonArray_randomized + "{\"planId\":\"" + currentPlan.Id + "\"," +
                                                                "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
                                                                "\"planMaxDose\":" + planMaxDose + "," +
                                                                "\"structureData\":[";

        //// json string to return
        //jsonStringForViewDvh = "{\"primaryPhysician\":\"" + primaryPhysician + "\"," +
        //                        "\"planId\":\"" + currentPlan.Id + "\"," +
        //                        "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
        //                        "\"planMaxDose\":" + planMaxDose + "," +
        //                        "\"structureData\":[";

        foreach (var s in zsorted_structureList)
        {
          // clear string builders
          dvhData_randomizedCsvCols_SB.Clear();
          dvhData_randomizedCsvRows_SB.Clear();
          dvhData_randomizedCsvRows_SB_master.Clear();
          dvhData_structureSpecificCsvRows_SB.Clear();
          dvhData_pysicianAndStructureSpecificCsvRows_SB.Clear();

          // variables
          string lowerId = s.Id.ToString().ToLower().Replace(" ", string.Empty).Replace("\\", "_").Replace(".", "_").Replace("/", "_").Split(':').First();
          lowerId = lowerId.Replace('/', '_');
          lowerId = lowerId.Replace('\\', '_');
          double volume = Math.Round(s.Volume, 3);
          string color = "#" + s.Color.ToString().Substring(3, 6);
          string structureSpecificJsonString = string.Empty;

          // define final paths
          string finalDvhCsvPath_randomizedCsvCols_patientSpecific = patientSpecificDvhDataDirectory_randomizedCsvCols + "\\" + courseHeader + "_" + lowerId + "_DVH_col.csv";
          string finalDvhCsvPath_randomizedCsvRows_patientSpecific = patientSpecificDvhDataDirectory_randomizedCsvRows + "\\" + courseHeader + "_" + lowerId + "_DVH_row.csv";
          string finalDvhCsvPath_randomizedCsvRows_masterData = masterPlanDataDirectory + "\\AllPlansDvhData_rows.csv";
          //string finalDvhCsvPath_randomizedCsvCols_physSpec =  physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns + "\\" + lowerId + "_cols.csv";
          //string finalDvhCsvPath_randomizedCsvRows_physSpec = physicianSpecificPlanDvhDataDirectory_randomizedCsvRows + "\\" + lowerId + "_rows.csv";
          string finalDvhCsvPath_randomizedCsvRows_structureSpec = structureSpecificDvhDataDirectory_randomizedCsvRows + "\\" + courseHeader + "_" + lowerId + "_DVH_row.csv";
          string finalDvhCsvPath_randomizedCsvRows_physAndStructureSpec = physicianSpecificStructureDvhDirectory + "\\" + courseHeader + "_" + lowerId + "_DVH_row.csv";
          finalDvhJsonPath_patientSpecific = patientSpecificDvhDataDirectory_plans + "\\" + currentPlan.Id + "_DVH.json";
          finalDvhJsonPath_randomizedJson_patientSpecific = patientSpecificDvhDataDirectory_randomizedJson + "\\" + currentPlan.Id + "_DVH.json";

          // structure specific json
          string finalDvhJsonPath_randomizedJson_structureSpec = structureSpecificDvhDataDirectory_randomizedJson + "\\" + courseHeader + "_" + lowerId + "_DVH.json";

          if (!File.Exists(finalDvhJsonPath_randomizedJson_structureSpec))
          {
            structureSpecificJsonString = "[";
          }
          else
          {
            using (StreamReader streamReader = new StreamReader(finalDvhJsonPath_randomizedJson_structureSpec, Encoding.UTF8))
            {
              structureSpecificJsonString = streamReader.ReadToEnd();
            }
            //structureSpecificJsonString = File.ReadAllText(finalDvhJsonPath_randomizedJson_structureSpec);
            structureSpecificJsonString = structureSpecificJsonString.TrimEnd(']');
            structureSpecificJsonString = structureSpecificJsonString + ",";
          }

          // dvh data
          DVHData dvhAR = currentPlan.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
          DVHData dvhAA = currentPlan.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

          // define strings
          //dvhCsvStringBuilder.AppendLine("patientId: " + patientId);
          dvhData_randomizedCsvCols_SB.AppendLine("randomId,\t" + randomId);
          dvhData_randomizedCsvCols_SB.AppendLine("primaryPhysician,\t" + primaryPhysician);
          dvhData_randomizedCsvCols_SB.AppendLine("courseHeader,\t" + courseHeader);
          dvhData_randomizedCsvCols_SB.AppendLine("planId,\t" + currentPlan.Id);
          dvhData_randomizedCsvCols_SB.AppendLine("approvalStatus," + currentPlan.ApprovalStatus);
          dvhData_randomizedCsvCols_SB.AppendLine("planMaxDose,\t" + planMaxDose);
          dvhData_randomizedCsvCols_SB.AppendLine("structureId,\t" + lowerId);
          dvhData_randomizedCsvCols_SB.AppendLine("structureColor,\t" + color);
          dvhData_randomizedCsvCols_SB.AppendLine("structureVolume," + volume);

          if (dvhAR != null && dvhAR.CurveData.Length > 0)
          {
            var doseList = new List<string>();
            var relVolumeList = new List<string>();

            double min03 = Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (volume - 0.03)), 3);
            double minDose = Math.Round(dvhAA.MinDose.Dose, 3);
            double meanDose = Math.Round(dvhAR.MeanDose.Dose, 3);
            double max03 = Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (0.03)), 3);
            double maxDose = Math.Round(dvhAR.MaxDose.Dose, 3);
            double medianDose = Math.Round(dvhAR.MedianDose.Dose, 3);
            double std = Math.Round(dvhAR.StdDev, 3);

            // json string to return
            jsonStringForViewDvh = jsonStringForViewDvh + "{\"structureId\":\"" + s.Id + "\"," +
                                                            "\"structureColor\":\"" + color + "\"," +
                                                            "\"structureVolume\":" + volume + "," +
                                                            "\"min03\":" + min03 + "," +
                                                            "\"minDose\":" + minDose + "," +
                                                            "\"meanDose\":" + meanDose + "," +
                                                            "\"max03\":" + max03 + "," +
                                                            "\"maxDose\":" + maxDose + "," +
                                                            "\"medianDose\":" + medianDose + "," +
                                                            "\"std\":" + std + "," +
                                                            "\"dvh\":[";
            plansJsonArray_randomized = plansJsonArray_randomized + "{\"structureId\":\"" + s.Id + "\"," +
                                                                    "\"structureColor\":\"" + color + "\"," +
                                                                    "\"structureVolume\":" + volume + "," +
                                                                    "\"min03\":" + min03 + "," +
                                                                    "\"minDose\":" + minDose + "," +
                                                                    "\"meanDose\":" + meanDose + "," +
                                                                    "\"max03\":" + max03 + "," +
                                                                    "\"maxDose\":" + maxDose + "," +
                                                                    "\"medianDose\":" + medianDose + "," +
                                                                    "\"std\":" + std + "," +
                                                                    "\"dvh\":[";
            // json string to write
            structureSpecificJsonString = structureSpecificJsonString + "{\"structureData\":[" +
                                                                        "{\"randomId\":\"" + randomId + "\"," +
                                                                        "\"primaryPhysician\":\"" + primaryPhysician + "\"," +
                                                                        "\"courseHeader\":\"" + courseHeader + "\"," +
                                                                        "\"planId\":\"" + currentPlan.Id + "\"," +
                                                                        "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
                                                                        "\"planMaxDose\":" + planMaxDose + "," +
                                                                        "\"structureId\":\"" + s.Id + "\"," +
                                                                        "\"structureColor\":\"" + color + "\"," +
                                                                        "\"structureVolume\":" + volume + "," +
                                                                        "\"min03\":" + min03 + "," +
                                                                        "\"minDose\":" + minDose + "," +
                                                                        "\"meanDose\":" + meanDose + "," +
                                                                        "\"max03\":" + max03 + "," +
                                                                        "\"maxDose\":" + maxDose + "," +
                                                                        "\"medianDose\":" + medianDose + "," +
                                                                        "\"std\":" + std + "," +
                                                                        "\"dvh\":[";
            // column csv
            dvhData_randomizedCsvCols_SB.AppendLine("min03,\t\t" + min03);
            dvhData_randomizedCsvCols_SB.AppendLine("minDose,\t" + minDose);
            dvhData_randomizedCsvCols_SB.AppendLine("max03,\t\t" + max03);
            dvhData_randomizedCsvCols_SB.AppendLine("maxDose,\t" + maxDose);
            dvhData_randomizedCsvCols_SB.AppendLine("meanDose,\t" + meanDose);
            dvhData_randomizedCsvCols_SB.AppendLine("medianDose:,\t" + medianDose);
            dvhData_randomizedCsvCols_SB.AppendLine("std,\t\t" + std);
            dvhData_randomizedCsvCols_SB.AppendLine("dvh:");
            dvhData_randomizedCsvCols_SB.AppendLine("Dose(Gy),\tVolume(cc),\tVolume(pct)");

            for (double i = 0; i <= dvhAR.MaxDose.Dose + .1; i += .1)
            {
              string dose = string.Format("{0:N1}", i);
              string relVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString();
              string absVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAA, i), 4).ToString();

              // json string to return
              jsonStringForViewDvh = jsonStringForViewDvh + "[" + dose + "," + relVolAtDose + "],";
              plansJsonArray_randomized = plansJsonArray_randomized + "[" + dose + "," + relVolAtDose + "],";
              // json string to write
              structureSpecificJsonString = structureSpecificJsonString + "[" + dose + "," + relVolAtDose + "],";
              // csv string to write
              dvhData_randomizedCsvCols_SB.AppendLine(string.Format("{0},\t\t{1},\t\t{2}", dose, absVolAtDose, relVolAtDose));
              // lists for csv string rows
              doseList.Add("V" + dose);
              relVolumeList.Add(relVolAtDose);
            }
            string doseListString = string.Join(",", doseList.ToArray());
            string relVolumeListString = string.Join(",", relVolumeList.ToArray());

            string headers = "RandomId,PrimaryPhysician,courseHeader,PlanId,ApprovalStatus,PlanMaxDose,StructureId,StructureVolume,Min03,MinDose,Max03,MaxDose,MeanDose,MedianDose,Std," + doseListString;
            var data = randomId + "," + primaryPhysician + "," + courseHeader + "," + currentPlan.Id + "," + currentPlan.ApprovalStatus + "," + planMaxDose + "," + lowerId + "," +
                            volume + "," + min03 + "," + minDose + "," + max03 + "," + maxDose + "," + meanDose + "," + medianDose + "," + std + "," + relVolumeListString;

            // append csv headers
            dvhData_randomizedCsvRows_SB.AppendLine(headers);
            if (!File.Exists(finalDvhCsvPath_randomizedCsvRows_masterData))
            {
              dvhData_randomizedCsvRows_SB_master.AppendLine(headers);
            }
            if (!File.Exists(finalDvhCsvPath_randomizedCsvRows_structureSpec))
            {
              dvhData_structureSpecificCsvRows_SB.AppendLine(headers);
            }
            if (!File.Exists(finalDvhCsvPath_randomizedCsvRows_physAndStructureSpec))
            {
              dvhData_pysicianAndStructureSpecificCsvRows_SB.AppendLine(headers);
            }

            // append csv data
            dvhData_randomizedCsvRows_SB.AppendLine(data);
            dvhData_randomizedCsvRows_SB_master.AppendLine(data);
            dvhData_structureSpecificCsvRows_SB.AppendLine(data);
            dvhData_pysicianAndStructureSpecificCsvRows_SB.AppendLine(data);

            // json string to return
            jsonStringForViewDvh = jsonStringForViewDvh.TrimEnd(',');
            jsonStringForViewDvh = jsonStringForViewDvh + "]},";

            plansJsonArray_randomized = plansJsonArray_randomized.TrimEnd(',');
            plansJsonArray_randomized = plansJsonArray_randomized + "]},";

            // json string to write
            structureSpecificJsonString = structureSpecificJsonString.TrimEnd(',');
            structureSpecificJsonString = structureSpecificJsonString + "]}]}]";
          }
          // write files
          File.WriteAllText(finalDvhCsvPath_randomizedCsvCols_patientSpecific, dvhData_randomizedCsvCols_SB.ToString());
          File.WriteAllText(finalDvhCsvPath_randomizedCsvRows_patientSpecific, dvhData_randomizedCsvRows_SB.ToString());
          File.AppendAllText(finalDvhCsvPath_randomizedCsvRows_masterData, dvhData_randomizedCsvRows_SB_master.ToString());
          File.AppendAllText(finalDvhCsvPath_randomizedCsvRows_structureSpec, dvhData_structureSpecificCsvRows_SB.ToString());
          File.AppendAllText(finalDvhCsvPath_randomizedCsvRows_physAndStructureSpec, dvhData_pysicianAndStructureSpecificCsvRows_SB.ToString());
          // structure specific json file
          File.WriteAllText(finalDvhJsonPath_randomizedJson_structureSpec, structureSpecificJsonString);
        }
        jsonStringForViewDvh = jsonStringForViewDvh.TrimEnd(',');
        jsonStringForViewDvh = jsonStringForViewDvh + "]},";

        plansJsonArray_randomized = plansJsonArray_randomized.TrimEnd(',');
        plansJsonArray_randomized = plansJsonArray_randomized + "]},";
      }
      jsonStringForViewDvh = jsonStringForViewDvh.TrimEnd(',');
      jsonStringForViewDvh = jsonStringForViewDvh + "]}]";

      plansJsonArray_randomized = plansJsonArray_randomized.TrimEnd(',');
      plansJsonArray_randomized = plansJsonArray_randomized + "]}]";

      // write json file for ViewDvh folder
      File.WriteAllText(viewDvhPath_searchableJson, jsonStringForViewDvh);
      File.WriteAllText(finalDvhJsonPath_patientSpecific, jsonStringForViewDvh);
      File.WriteAllText(finalDvhJsonPath_randomizedJson_patientSpecific, plansJsonArray_randomized);

      #region write html

      string htmlPath = htmlDashboardDirectory + "\\" + DateTime.Now.ToShortDateString().Replace('/', '_') + "_" + courseHeader + "_r" + randomId + ".html";
      StreamWriter stream = new StreamWriter(htmlPath);
      string varPlanJSONArray = "var PlanJSONArray = " + plansJsonArray_randomized;

      using (stream)
      {
        stream.WriteLine(@"<!DOCTYPE html>");
        stream.WriteLine(@"<script src = 'https://ajax.googleapis.com/ajax/libs/jquery/3.1.1/jquery.min.js'></script>");
        stream.WriteLine(@"<link href='https://fonts.googleapis.com/css?family=PT+Sans' rel='stylesheet'>");
        stream.WriteLine(@"<html>");
        stream.WriteLine(@"<head>");
        stream.WriteLine(@"<meta charset='utf-8'/>");
        stream.WriteLine(@"<link rel='stylesheet' type='text/css' href='S:\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanDataDashboard__\\__Style__\\PageStyle.css'>");
        stream.WriteLine(@"<title>DVH Review - " + course + "</title>");
        stream.WriteLine(@"</head>");
        stream.WriteLine(@"<body>");
        stream.WriteLine(@"<div class = 'planInfoDisplay' id = 'infoBlockDislay'>");
        stream.WriteLine(@"<button id = 'print' class='pBtn btn-primary btn-sml' onclick='PrepareAndPrint()'>Print</button>");
        stream.WriteLine(@"<p>Physician:<span class='tabPhysician'>" + primaryPhysician + "</span></p>");
        stream.WriteLine(@"<p>CurrentCourse:<span class='tabPlanName'>" + course + "</span></p>");
        stream.WriteLine(@"</div>");
        stream.WriteLine(@"<div id='dvh'></div>");
        stream.WriteLine(@"</div>");
        stream.WriteLine(@"<div id='planStats'></div>");
        stream.WriteLine(@"</div>");
        stream.WriteLine(@"<div id='structureStats'></div>");
        stream.WriteLine(@"</body>");
        stream.WriteLine(@"</html>");
        stream.WriteLine(@"<script>");
        stream.WriteLine(varPlanJSONArray);
        stream.WriteLine(@"$(document).ready(function () {
                                                            var options1 = {

                                                            chart:
                                                                {
                                                                renderTo: 'dvh',
                                                                    type: 'line',
                                                                    zoomType: 'xy',
                                                                    panning: true,
                                                                    panKey: 'shift',
                                                                },
                                                                exporting:
                                                                {
                                                                buttons:
                                                                    {
                                                                    contextButton:
                                                                        {
                                                                        enabled: false
                                                                        }
                                                                    }
                                                                },
                                                                xAxis:
                                                                {
                                                                title:
                                                                    {
                                                                    text: 'Dose (Gy)'
                                                                    },
                                                                    crosshair: true,
                                                                    maxPadding: 0.02
                                                                },
                                                                plotOptions:
                                                                {
                                                                series:
                                                                    {
                                                                    marker:
                                                                        {
                                                                        enabled: false
                                                                        },
                                                                        allowPointSelect: true,
                                                                        states:
                                                                        {
                                                                        hover:
                                                                            {
                                                                            enabled: true,
                                                                                lineWidth: 5
                                                                            }
                                                                        }
                                                                    },
                                                                    boxplot:
                                                                    {
                                                                    fillColor: '#505053'
                                                                    },
                                                                    candlestick:
                                                                    {
                                                                    lineColor: 'white'
                                                                    },
                                                                    errorbar:
                                                                    {
                                                                    color: 'white'
                                                                    }
                                                                },
                                                                yAxis:
                                                                {
                                                                labels:
                                                                    {
                                                                    format: '{value} %'
                                                                    },
                                                                    floor: 0,
                                                                    ceiling: 100,
                                                                    title:
                                                                    {
                                                                    text: 'Volume (%)'
                                                                    },
                                                                    crosshair: true,
                                                                    gridLineDashStyle: 'ShortDash',
                                                                    gridLineColor: '#aaaaaa'
                                                                },
                                                                tooltip:
                                                                {
                                                                shared: true,
                                                                    useHTML: true,
                                                                    headerFormat: '<table>',
                                                                    pointFormat: '<tr><td style=\""color:{series.color}; text-shadow: 0px 0 #353839, 0 1px #353839, 1px 0 #353839, 0 0px #353839;\"">{series.name}: </td><td style=\""text-align: left; color:#282827\"">V{point.x} Gy = {point.y} %</td></tr>',
                                                                    footerFormat: '</table>',
                                                                },
                                                                title:
                                                                {
                                                                text: 'DVH',
                                                                    x: -150
                                                                },
                                                                subtitle:
                                                                {
                                                                text: 'Click and drag to zoom in. Hold down shift key to pan.',
                                                                    x: -150
                                                                },
                                                                legend:
                                                                {
                                                                layout: 'vertical',
                                                                    align: 'right',
                                                                    verticalAlign: 'middle',
                                                                    borderWidth: 0,
                                                                    floating: false,
                                                                    width: 420,
                                                                    itemWidth: 210,
                                                                    itemStyle:
                                                                    {
                                                                    width: 205
                                                                    },
                                                                    itemHiddenStyle:
                                                                    {
                                                                    color: '#ff4d4d'
                                                                    }
                                                                },

                                                                series: seriesOptions
                                                            };

                                                            var options2 = {

                                                                chart: {
                                                            renderTo: 'planStats',
                                                                    type: 'column',
                                                                    panning: true,
                                                                    panKey: 'shift',
                                                                },
                                                                exporting:
                                                            {
                                                            buttons:
                                                                {
                                                                contextButton:
                                                                    {
                                                                    enabled: false
                                                                        }
                                                                }
                                                            },
                                                                xAxis:
                                                            {
                                                            categories: ['MinDose', 'Min(0.03cc)', 'MeanDose', 'MedianDose', 'Max(0.03cc)', 'MaxDose'],
                                                                    title:
                                                                {
                                                                text: 'DoseStatistic'
                                                                    },
                                                                    crosshair: true,
                                                                    maxPadding: 0.02
                                                                },
                                                                plotOptions:
                                                            {
                                                            series:
                                                                {
                                                                marker:
                                                                    {
                                                                    enabled: false
                                                                        },
                                                                        allowPointSelect: true,
                                                                        states:
                                                                    {
                                                                    hover:
                                                                        {
                                                                        enabled: true,
                                                                                lineWidth: 5
                                                                            }
                                                                    }
                                                                },
                                                                    boxplot:
                                                                {
                                                                fillColor: '#505053'
                                                                    },
                                                                    candlestick:
                                                                {
                                                                lineColor: 'white'
                                                                    },
                                                                    errorbar:
                                                                {
                                                                color: 'white'
                                                                    }
                                                            },
                                                                yAxis:
                                                            {
                                                            floor: 0,
                                                                    title:
                                                                {
                                                                text: 'Dose (Gy)'
                                                                    },
                                                                    crosshair: true,
                                                                    gridLineDashStyle: 'ShortDash',
                                                                    gridLineColor: '#aaaaaa'
                                                                },
                                                                tooltip:
                                                            {
                                                            shared: true,
                                                                    useHTML: true,
                                                                    headerFormat: '<table>',
                                                                    pointFormat: '<tr><td style=\""color:{series.color}; text-shadow: 0px 0 #353839, 0 1px #353839, 1px 0 #353839, 0 0px #353839;\"">{series.name}: </td><td style=\""text-align: right; color:#282827\"">{point.y:.3f} Gy</td></tr>',
                                                                    footerFormat: '</table>',
                                                                },
                                                                title:
                                                            {
                                                            text: 'Structure Dose Statistics',
                                                                    x: -150
                                                                },
                                                                subtitle:
                                                            {
                                                            text: '',
                                                                    x: -150
                                                                },
                                                                legend:
                                                            {
                                                            layout: 'vertical',
                                                                    align: 'right',
                                                                    verticalAlign: 'middle',
                                                                    borderWidth: 0,
                                                                    floating: false,
                                                                    width: 420,
                                                                    itemWidth: 210,
                                                                    itemStyle:
                                                                {
                                                                width: 205
                                                                    },
                                                                    itemHiddenStyle:
                                                                {
                                                                color: '#ff4d4d'
                                                                    }
                                                            },

                                                                series: seriesOptions2
                                                            };

                                                        var options3 = {

                                                                chart: {
                                                                    renderTo: 'structureStats',
                                                                    type: 'column',
                                                                    //zoomType: 'xy',
                                                                    panning: true,
                                                                    panKey: 'shift',
                                                                },
                                                                exporting:
                                                        {
                                                        buttons:
                                                            {
                                                            contextButton:
                                                                {
                                                                enabled: false
                                                                        }
                                                            }
                                                        },
                                                                xAxis:
                                                        {
                                                        categories: [''],
                                                                    title:
                                                            {
                                                            text: 'StructureVolume'
                                                                    },
                                                                    crosshair: true,
                                                                    maxPadding: 0.02
                                                                },
                                                                plotOptions:
                                                        {
                                                        series:
                                                            {
                                                            marker:
                                                                {
                                                                enabled: false
                                                                        },
                                                                        allowPointSelect: true,
                                                                        states:
                                                                {
                                                                hover:
                                                                    {
                                                                    enabled: true,
                                                                                lineWidth: 5
                                                                            }
                                                                }
                                                            },
                                                                    boxplot:
                                                            {
                                                            fillColor: '#505053'
                                                                    },
                                                                    candlestick:
                                                            {
                                                            lineColor: 'white'
                                                                    },
                                                                    errorbar:
                                                            {
                                                            color: 'white'
                                                                    }
                                                        },
                                                                yAxis:
                                                        {
                                                        floor: 0,
                                                                    title:
                                                            {
                                                            text: 'Volume (cc)'
                                                                    },
                                                                    crosshair: true,
                                                                    gridLineDashStyle: 'ShortDash',
                                                                    gridLineColor: '#aaaaaa'
                                                                },
                                                                tooltip:
                                                        {
                                                        shared: true,
                                                                    useHTML: true,
                                                                    headerFormat: '<table>',
                                                                    pointFormat: '<tr><td style=\""color:{series.color}; text-shadow: 0px 0 #353839, 0 1px #353839, 1px 0 #353839, 0 0px #353839;\"">{series.name}: </td><td style=\""text-align: right; color:#282827\"">{point.y:.3f} cc</td></tr>',
                                                                    footerFormat: '</table>',
                                                                },
                                                                title:
                                                        {
                                                        text: 'Structure Statistics',
                                                                    x: -150
                                                                },
                                                                subtitle:
                                                        {
                                                        text: '',
                                                                    x: -150
                                                                },
                                                                legend:
                                                        {
                                                        layout: 'vertical',
                                                                    align: 'right',
                                                                    verticalAlign: 'middle',
                                                                    borderWidth: 0,
                                                                    floating: false,
                                                                    width: 420,
                                                                    itemWidth: 210,
                                                                    itemStyle:
                                                            {
                                                            width: 205
                                                                    },
                                                                    itemHiddenStyle:
                                                            {
                                                            color: '#ff4d4d'
                                                                    }
                                                        },

                                                                series: seriesOptions3
                                                            };

                                                    

                                                    var chart1 = new Highcharts.Chart(options1);
                                                    var chart2 = new Highcharts.Chart(options2);
                                                    var chart3 = new Highcharts.Chart(options3);

                                                });

                                                var seriesOptions = [],
                                                    seriesOptions2 = [],
                                                    seriesOptions3 = [],
                                                    seriesOptions4 = [],
                                                    dashStyles = [
                                                        'Solid',
                                                        'ShortDash',
                                                        'ShortDot',
                                                        'ShortDashDot',
                                                        'ShortDashDotDot',
                                                        'Dot',
                                                        'Dash',
                                                        'LongDash',
                                                        'DashDot',
                                                        'LongDashDot',
                                                        'LongDashDotDot'
                                                    ]

                                                var planData = PlanJSONArray[0].planData,
                                                    seriesCounter = 0,
                                                    planCounter = 0,
                                                    counter = 0

                                                planData.forEach(function (element, i) {

                                                    planCounter += seriesCounter
                                                    counter += 1

                                                    element.structureData.forEach(function (childElement, j) {
                                                        if ((element.structureData[j].structureId != 'Body') &&
                                                            (element.structureData[j].structureId != 'BODY') &&
                                                            (element.structureData[j].structureId != 'External') &&
                                                            (element.structureData[j].structureId != 'EXTERNAL'))    {

                                                            seriesOptions[planCounter] = {
                                                                id: element.planId,
                                                                name: element.planId/* + '_' + element.structureData[0].structureId*/,
                                                                data: /*element.structureData[0].dvh*/[],
                                                                dashStyle: dashStyles[planCounter],
                                                                visible: true,
                                                                color: element.structureData[0].structureColor /*'white'*/
                                                                                                                //linkedTo: ':previous'
                                                }

                                                seriesOptions[counter] = {
                                                                        name: element.planId + '_' + element.structureData[j].structureId,
                                                                        data: element.structureData[j].dvh,
                                                                        dashStyle: dashStyles[i],
                                                                        visible: true,
                                                                        color: element.structureData[j].structureColor,
                                                                        linkedTo: element.planId
                                                                    }

                                                //seriesOptions2[seriesCounter] = {
                                                //    //task: element.subTaskId,
                                                //    name: element.planId + '_' + element.structureData[j].structureId,
                                                //    data: [element.structureData[j].minDose, element.structureData[j].min03, element.structureData[j].meanDose,
                                                //            element.structureData[j].medianDose, element.structureData[j].max03, element.structureData[j].maxDose],
                                                //    dashStyle: dashStyles[i],
                                                //    visible: true,
                                                //    color: element.structureData[j].structureColor
                                                //}

                                                seriesOptions2[planCounter] = {
                                                    id: element.planId,
                                                    name: element.planId/* + '_' + element.structureData[0].structureId*/,
                                                    data: /*element.structureData[0].dvh*/[],
                                                    dashStyle: dashStyles[planCounter],
                                                    visible: true,
                                                    color: element.structureData[0].structureColor /*'white'*/
                                                    //linkedTo: ':previous'
                                                }

                                                seriesOptions2[counter] = {
                                                    name: element.planId + '_' + element.structureData[j].structureId,
                                                    data: [element.structureData[j].minDose, element.structureData[j].min03, element.structureData[j].meanDose,
                                                    element.structureData[j].medianDose, element.structureData[j].max03, element.structureData[j].maxDose],
                                                    dashStyle: dashStyles[i],
                                                    visible: true,
                                                    color: element.structureData[j].structureColor,
                                                    linkedTo: element.planId
                                                }

                                                //seriesOptions3[seriesCounter] = {
                                                //    //task: element.subTaskId,
                                                //    name: element.structureData[j].structureId,
                                                //    data: [element.structureData[j].structureVolume],
                                                //    dashStyle: dashStyles[i],
                                                //    visible: true,
                                                //    color: element.structureData[j].structureColor
                                                //}

                                                seriesOptions3[planCounter] = {
                                                    id: element.planId,
                                                    name: element.planId/* + '_' + element.structureData[0].structureId*/,
                                                    data: /*element.structureData[0].dvh*/[],
                                                    dashStyle: dashStyles[planCounter],
                                                    visible: true,
                                                    color: element.structureData[0].structureColor /*'white'*/
                                                    //linkedTo: ':previous'
                                                }

                                                seriesOptions3[counter] = {
                                                    name: element.structureData[j].structureId,
                                                    data: [element.structureData[j].structureVolume],
                                                    dashStyle: dashStyles[i],
                                                    visible: true,
                                                    color: element.structureData[j].structureColor,
                                                    linkedTo: element.planId
                                                }

                                                //seriesOptions3[seriesCounter] = {
                                                //    //task: element.subTaskId,
                                                //    name: element.structureData[j].structureId,
                                                //    data: [element.structureData[j].structureVolume],
                                                //    dashStyle: dashStyles[i],
                                                //    visible: true,
                                                //    color: element.structureData[j].structureColor
                                                //}


                                                seriesCounter += 1
                                                counter += 1
                                            }
                                        })

                                        planCounter += 1

                                    })
                                    function PrepareAndPrint()
                                    {
                                        $('.pBtn').remove();
                                        $('.Buttons').remove();
                                        window.print();
                                    }     
                                </script> ");
        stream.WriteLine(@"<div>");
        stream.WriteLine(@"<div>");
        stream.WriteLine(@"<script src = 'https://code.highcharts.com/highcharts.js'></script >");



        stream.Flush();
        stream.Close();
      }

      #endregion
    }
    public static void recordDvhDataForCurrentPlanSum(string viewDvhPath_searchableJson, string patientName, string course, PlanSum currentPlanSum, IEnumerable<Structure> sorted_structureList, string patientId, string randomId, string primaryPhysician)
    {
      string courseHeader = course.Split('-').Last().Replace(" ", "_");
      string status = "PlanSum";
      double sumMaxDose = 0;
      if (currentPlanSum.Dose != null)
      {
        sumMaxDose = Math.Round(currentPlanSum.Dose.DoseMax3D.Dose, 3);
      }
      else { sumMaxDose = Double.NaN; }

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
      string patientSpecificDirectory = planDataDirectory + "\\_PatientSpecific_\\" + patientId + "\\" + course;
      if (!Directory.Exists(patientSpecificDirectory))
      {
        Directory.CreateDirectory(patientSpecificDirectory);
      }

      string htmlDashboardDirectory = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanDataDashboard__\\_HTML_";
      if (!Directory.Exists(htmlDashboardDirectory))
      {
        Directory.CreateDirectory(htmlDashboardDirectory);
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

      // patientSpecificDvhDataDirectory
      string patientSpecificDvhDataDirectory = patientSpecificDirectory + "\\DvhData";
      if (!Directory.Exists(patientSpecificDvhDataDirectory))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory);
      }

      // patientSpecificDvhDataDirectory_plans
      string patientSpecificDvhDataDirectory_plans = patientSpecificDvhDataDirectory + "\\Plans";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_plans))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_plans);
      }

      // patientSpecificDvhDataDirectory_sums
      string patientSpecificDvhDataDirectory_sums = patientSpecificDvhDataDirectory + "\\Sums";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_sums))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_sums);
      }

      // patientSpecificDvhDataDirectory_randomized
      string patientSpecificDvhDataDirectory_randomized = patientSpecificDvhDataDirectory + "\\Randomized";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_randomized))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomized);
      }

      // patientSpecificDvhDataDirectory_randomizedJson
      string patientSpecificDvhDataDirectory_randomizedJson = patientSpecificDvhDataDirectory_randomized + "\\JSON";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedJson))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedJson);
      }
      string finalDvhJsonPath_patientSpecific = patientSpecificDvhDataDirectory_plans;
      string finalDvhJsonPath_randomizedJson_patientSpecific = patientSpecificDvhDataDirectory_randomizedJson;

      // patientSpecificDvhDataDirectory_randomizedCsvRows
      string patientSpecificDvhDataDirectory_randomizedCsvRows = patientSpecificDvhDataDirectory_randomized + "\\CsvRows";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedCsvRows))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedCsvRows);
      }

      // patientSpecificDvhDataDirectory_randomizedCsvCols
      string patientSpecificDvhDataDirectory_randomizedCsvCols = patientSpecificDvhDataDirectory_randomized + "\\CsvColumns";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedCsvCols))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedCsvCols);
      }

      #endregion

      #endregion

      #region physician specific

      // physician folder
      string physicianSpecificDirectory = planDataDirectory + "\\_PhysicianSpecific_\\" + primaryPhysician;
      if (!Directory.Exists(physicianSpecificDirectory))
      {
        Directory.CreateDirectory(physicianSpecificDirectory);
      }

      string physicianSpecificStructureDvhDirectory = physicianSpecificDirectory + "\\StructureDvhData\\CsvRows";
      if (!Directory.Exists(physicianSpecificStructureDvhDirectory))
      {
        Directory.CreateDirectory(physicianSpecificStructureDvhDirectory);
      }

      //string physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns = physicianSpecificDirectory + "\\_PlanDvhData_\\" + currentPlanSum.Id + "\\CsvColumns";
      //if (!Directory.Exists(physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns))
      //{
      //    Directory.CreateDirectory(physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns);
      //}

      //string physicianSpecificPlanDvhDataDirectory_randomizedCsvRows = physicianSpecificDirectory + "\\_PlanDvhData_\\" + currentPlanSum.Id + "\\CsvRows";
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
      string structureSpecificProximityStatsDirectory = structureSpecificDirectory + "\\TargetProximityStats\\" + courseHeader;
      if (!Directory.Exists(structureSpecificProximityStatsDirectory))
      {
        Directory.CreateDirectory(structureSpecificProximityStatsDirectory);
      }

      // structure specific dvhdata
      string structureSpecificDvhDataDirectory = structureSpecificDirectory + "\\DvhData\\" + courseHeader;
      if (!Directory.Exists(structureSpecificDvhDataDirectory))
      {
        Directory.CreateDirectory(structureSpecificDvhDataDirectory);
      }

      // structure specific dvhdata json
      string structureSpecificDvhDataDirectory_randomizedJson = structureSpecificDvhDataDirectory + "\\JSON";
      if (!Directory.Exists(structureSpecificDvhDataDirectory_randomizedJson))
      {
        Directory.CreateDirectory(structureSpecificDvhDataDirectory_randomizedJson);
      }

      // structure specific dvhdata csv
      string structureSpecificDvhDataDirectory_randomizedCsvRows = structureSpecificDvhDataDirectory + "\\CsvRows";
      if (!Directory.Exists(structureSpecificDvhDataDirectory_randomizedCsvRows))
      {
        Directory.CreateDirectory(structureSpecificDvhDataDirectory_randomizedCsvRows);
      }

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

      #endregion

      StringBuilder dvhData_randomizedCsvCols_SB = new StringBuilder();
      StringBuilder dvhData_randomizedCsvRows_SB = new StringBuilder();
      StringBuilder dvhData_randomizedCsvRows_SB_master = new StringBuilder();
      StringBuilder dvhData_structureSpecificCsvRows_SB = new StringBuilder();
      StringBuilder dvhData_pysicianAndStructureSpecificCsvRows_SB = new StringBuilder();

      string jsonStringForViewDvh = "[{\"patientName\":\"" + patientName + "\", " +
                                      "\"patientId\":\"" + patientId + "\", " +
                                      "\"randomId\":\"" + randomId + "\", " +
                                      "\"primaryPhysician\":\"" + primaryPhysician + "\", " +
                                      "\"courseId\":\"" + course + "\", " +
                                      "\"courseHeader\":\"" + courseHeader + "\"," +
                                      "\"planData\":[" +
                                      "{\"planId\":\"" + currentPlanSum.Id + "\"," +
                                      "\"approvalStatus\":\"" + status + "\"," +
                                      "\"planMaxDose\":" + sumMaxDose + "," +
                                      "\"structureData\":[";

      string plansJsonArray_randomized = "[{\"randomId\":\"" + randomId + "\", " +
                                          "\"primaryPhysician\":\"" + primaryPhysician + "\", " +
                                          "\"courseId\":\"" + course + "\", " +
                                          "\"courseHeader\":\"" + courseHeader + "\"," +
                                          "\"planData\":[" +
                                          "{\"planId\":\"" + currentPlanSum.Id + "\"," +
                                          "\"approvalStatus\":\"" + status + "\"," +
                                          "\"planMaxDose\":" + sumMaxDose + "," +
                                          "\"structureData\":[";

      //jsonStringForViewDvh = "{\"primaryPhysician\":\"" + primaryPhysician + "\"," + 
      //                        "\"planId\":\"" + currentPlan.Id + "\"," +
      //                        "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
      //                        "\"planMaxDose\":" + planMaxDose + "," +
      //                        "\"structureData\":[";

      foreach (var s in sorted_structureList)
      {
        // clear string builders
        dvhData_randomizedCsvCols_SB.Clear();
        dvhData_randomizedCsvRows_SB.Clear();
        dvhData_randomizedCsvRows_SB_master.Clear();
        dvhData_structureSpecificCsvRows_SB.Clear();
        dvhData_pysicianAndStructureSpecificCsvRows_SB.Clear();

        // variables
        string lowerId = s.Id.ToString().ToLower().Replace(" ", string.Empty).Replace("\\", "_").Replace(".", "_").Replace("/", "_").Split(':').First();
        lowerId = lowerId.Replace('/', '_');
        lowerId = lowerId.Replace('\\', '_');
        double volume = Math.Round(s.Volume, 3);
        string color = "#" + s.Color.ToString().Substring(3, 6);
        string structureSpecificJsonString = string.Empty;

        // define final paths
        string finalDvhCsvPath_randomizedCsvCols_patientSpecific = patientSpecificDvhDataDirectory_randomizedCsvCols + "\\" + courseHeader + "_" + lowerId + "_DVH_col.csv";
        string finalDvhCsvPath_randomizedCsvRows_patientSpecific = patientSpecificDvhDataDirectory_randomizedCsvRows + "\\" + courseHeader + "_" + lowerId + "_DVH_row.csv";
        string finalDvhCsvPath_randomizedCsvRows_masterData = masterPlanDataDirectory + "\\AllPlansDvhData_rows.csv";
        //string finalDvhCsvPath_randomizedCsvCols_physSpec =  physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns + "\\" + lowerId + "_cols.csv";
        //string finalDvhCsvPath_randomizedCsvRows_physSpec = physicianSpecificPlanDvhDataDirectory_randomizedCsvRows + "\\" + lowerId + "_rows.csv";
        string finalDvhCsvPath_randomizedCsvRows_structureSpec = structureSpecificDvhDataDirectory_randomizedCsvRows + "\\" + courseHeader + "_" + lowerId + "_DVH_row.csv";
        string finalDvhCsvPath_randomizedCsvRows_physAndStructureSpec = physicianSpecificStructureDvhDirectory + "\\" + courseHeader + "_" + lowerId + "_DVH_row.csv";
        finalDvhJsonPath_patientSpecific = patientSpecificDvhDataDirectory_plans + "\\" + currentPlanSum.Id + "_DVH.json";
        finalDvhJsonPath_randomizedJson_patientSpecific = patientSpecificDvhDataDirectory_randomizedJson + "\\" + currentPlanSum.Id + "_DVH.json";

        // structure specific json
        string finalDvhJsonPath_randomizedJson_structureSpec = structureSpecificDvhDataDirectory_randomizedJson + "\\" + courseHeader + "_" + lowerId + "_DVH.json";

        if (!File.Exists(finalDvhJsonPath_randomizedJson_structureSpec))
        {
          structureSpecificJsonString = "[";
        }
        else
        {
          using (StreamReader streamReader = new StreamReader(finalDvhJsonPath_randomizedJson_structureSpec, Encoding.UTF8))
          {
            structureSpecificJsonString = streamReader.ReadToEnd();
          }
          //structureSpecificJsonString = File.ReadAllText(finalDvhJsonPath_randomizedJson_structureSpec);
          structureSpecificJsonString = structureSpecificJsonString.TrimEnd(']');
          structureSpecificJsonString = structureSpecificJsonString + ",";
        }

        // dvh data
        DVHData dvhAR = currentPlanSum.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
        DVHData dvhAA = currentPlanSum.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

        // define strings
        //dvhCsvStringBuilder.AppendLine("patientId: " + patientId);
        dvhData_randomizedCsvCols_SB.AppendLine("randomId,\t" + randomId);
        dvhData_randomizedCsvCols_SB.AppendLine("primaryPhysician,\t" + primaryPhysician);
        dvhData_randomizedCsvCols_SB.AppendLine("courseHeader,\t" + courseHeader);
        dvhData_randomizedCsvCols_SB.AppendLine("planId,\t" + currentPlanSum.Id);
        dvhData_randomizedCsvCols_SB.AppendLine("approvalStatus," + status);
        dvhData_randomizedCsvCols_SB.AppendLine("planMaxDose,\t" + sumMaxDose);
        dvhData_randomizedCsvCols_SB.AppendLine("structureId,\t" + lowerId);
        dvhData_randomizedCsvCols_SB.AppendLine("structureColor,\t" + color);
        dvhData_randomizedCsvCols_SB.AppendLine("structureVolume," + volume);

        if (dvhAR != null && dvhAR.CurveData.Length > 0)
        {
          var doseList = new List<string>();
          var relVolumeList = new List<string>();

          double min03 = Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (volume - 0.03)), 3);
          double minDose = Math.Round(dvhAA.MinDose.Dose, 3);
          double meanDose = Math.Round(dvhAR.MeanDose.Dose, 3);
          double max03 = Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (0.03)), 3);
          double maxDose = Math.Round(dvhAR.MaxDose.Dose, 3);
          double medianDose = Math.Round(dvhAR.MedianDose.Dose, 3);
          double std = Math.Round(dvhAR.StdDev, 3);

          // json string to return
          jsonStringForViewDvh = jsonStringForViewDvh + "{\"structureId\":\"" + s.Id + "\"," +
                                                          "\"structureColor\":\"" + color + "\"," +
                                                          "\"structureVolume\":" + volume + "," +
                                                          "\"min03\":" + min03 + "," +
                                                          "\"minDose\":" + minDose + "," +
                                                          "\"meanDose\":" + meanDose + "," +
                                                          "\"max03\":" + max03 + "," +
                                                          "\"maxDose\":" + maxDose + "," +
                                                          "\"medianDose\":" + medianDose + "," +
                                                          "\"std\":" + std + "," +
                                                          "\"dvh\":[";
          plansJsonArray_randomized = plansJsonArray_randomized + "{\"structureId\":\"" + s.Id + "\"," +
                                                                  "\"structureColor\":\"" + color + "\"," +
                                                                  "\"structureVolume\":" + volume + "," +
                                                                  "\"min03\":" + min03 + "," +
                                                                  "\"minDose\":" + minDose + "," +
                                                                  "\"meanDose\":" + meanDose + "," +
                                                                  "\"max03\":" + max03 + "," +
                                                                  "\"maxDose\":" + maxDose + "," +
                                                                  "\"medianDose\":" + medianDose + "," +
                                                                  "\"std\":" + std + "," +
                                                                  "\"dvh\":[";
          // json string to write
          structureSpecificJsonString = structureSpecificJsonString + "{\"structureData\":[" +
                                                                      "{\"randomId\":\"" + randomId + "\"," +
                                                                      "\"primaryPhysician\":\"" + primaryPhysician + "\"," +
                                                                      "\"courseHeader\":\"" + courseHeader + "\"," +
                                                                      "\"planId\":\"" + currentPlanSum.Id + "\"," +
                                                                      "\"approvalStatus\":\"" + status + "\"," +
                                                                      "\"planMaxDose\":" + sumMaxDose + "," +
                                                                      "\"structureId\":\"" + s.Id + "\"," +
                                                                      "\"structureColor\":\"" + color + "\"," +
                                                                      "\"structureVolume\":" + volume + "," +
                                                                      "\"min03\":" + min03 + "," +
                                                                      "\"minDose\":" + minDose + "," +
                                                                      "\"meanDose\":" + meanDose + "," +
                                                                      "\"max03\":" + max03 + "," +
                                                                      "\"maxDose\":" + maxDose + "," +
                                                                      "\"medianDose\":" + medianDose + "," +
                                                                      "\"std\":" + std + "," +
                                                                      "\"dvh\":[";
          // column csv
          dvhData_randomizedCsvCols_SB.AppendLine("min03,\t\t" + min03);
          dvhData_randomizedCsvCols_SB.AppendLine("minDose,\t" + minDose);
          dvhData_randomizedCsvCols_SB.AppendLine("max03,\t\t" + max03);
          dvhData_randomizedCsvCols_SB.AppendLine("maxDose,\t" + maxDose);
          dvhData_randomizedCsvCols_SB.AppendLine("meanDose,\t" + meanDose);
          dvhData_randomizedCsvCols_SB.AppendLine("medianDose:,\t" + medianDose);
          dvhData_randomizedCsvCols_SB.AppendLine("std,\t\t" + std);
          dvhData_randomizedCsvCols_SB.AppendLine("dvh:");
          dvhData_randomizedCsvCols_SB.AppendLine("Dose(Gy),\tVolume(cc),\tVolume(pct)");
          for (double i = 0; i <= dvhAR.MaxDose.Dose + .1; i += .1)
          {
            string dose = string.Format("{0:N1}", i);
            string relVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString();
            string absVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAA, i), 4).ToString();

            // json string to return
            jsonStringForViewDvh = jsonStringForViewDvh + "[" + dose + "," + relVolAtDose + "],";
            plansJsonArray_randomized = plansJsonArray_randomized + "[" + dose + "," + relVolAtDose + "],";
            // json string to write
            structureSpecificJsonString = structureSpecificJsonString + "[" + dose + "," + relVolAtDose + "],";
            // csv string to write
            dvhData_randomizedCsvCols_SB.AppendLine(string.Format("{0},\t\t{1},\t\t{2}", dose, absVolAtDose, relVolAtDose));
            // lists for csv string rows
            doseList.Add("V" + dose);
            relVolumeList.Add(relVolAtDose);
          }
          string doseListString = string.Join(",", doseList.ToArray());
          string relVolumeListString = string.Join(",", relVolumeList.ToArray());

          string headers = "RandomId,PrimaryPhysician,courseHeader,PlanId,ApprovalStatus,PlanMaxDose,StructureId,StructureVolume,Min03,MinDose,Max03,MaxDose,MeanDose,MedianDose,Std," + doseListString;
          var data = randomId + "," + primaryPhysician + "," + courseHeader + "," + currentPlanSum.Id + "," + status + "," + sumMaxDose + "," + lowerId + "," +
                                    volume + "," + min03 + "," + minDose + "," + max03 + "," + maxDose + "," + meanDose + "," + medianDose + "," + std + "," + relVolumeListString;

          // append csv headers
          dvhData_randomizedCsvRows_SB.AppendLine(headers);
          if (!File.Exists(finalDvhCsvPath_randomizedCsvRows_masterData))
          {
            dvhData_randomizedCsvRows_SB_master.AppendLine(headers);
          }
          if (!File.Exists(finalDvhCsvPath_randomizedCsvRows_structureSpec))
          {
            dvhData_structureSpecificCsvRows_SB.AppendLine(headers);
          }
          if (!File.Exists(finalDvhCsvPath_randomizedCsvRows_physAndStructureSpec))
          {
            dvhData_pysicianAndStructureSpecificCsvRows_SB.AppendLine(headers);
          }

          // append csv data
          dvhData_randomizedCsvRows_SB.AppendLine(data);
          dvhData_randomizedCsvRows_SB_master.AppendLine(data);
          dvhData_structureSpecificCsvRows_SB.AppendLine(data);
          dvhData_pysicianAndStructureSpecificCsvRows_SB.AppendLine(data);

          jsonStringForViewDvh = jsonStringForViewDvh.TrimEnd(',');
          jsonStringForViewDvh = jsonStringForViewDvh + "]},";

          plansJsonArray_randomized = plansJsonArray_randomized.TrimEnd(',');
          plansJsonArray_randomized = plansJsonArray_randomized + "]},";

          // json string to write
          structureSpecificJsonString = structureSpecificJsonString.TrimEnd(',');
          structureSpecificJsonString = structureSpecificJsonString + "]}]}]";
        }
        // write files
        File.WriteAllText(finalDvhCsvPath_randomizedCsvCols_patientSpecific, dvhData_randomizedCsvCols_SB.ToString());
        File.WriteAllText(finalDvhCsvPath_randomizedCsvRows_patientSpecific, dvhData_randomizedCsvRows_SB.ToString());
        File.AppendAllText(finalDvhCsvPath_randomizedCsvRows_masterData, dvhData_randomizedCsvRows_SB_master.ToString());
        // ph
        //File.AppendAllText(finalDvhCsvPath_randomizedCsvCols_physSpec, dvhData_randomizedCsvCols_SB.ToString());
        //File.AppendAllText(finalDvhCsvPath_randomizedCsvRows_physSpec, dvhData_randomizedCsvRows_SB.ToString());
        // structure specific csv files
        File.AppendAllText(finalDvhCsvPath_randomizedCsvRows_structureSpec, dvhData_structureSpecificCsvRows_SB.ToString());
        File.AppendAllText(finalDvhCsvPath_randomizedCsvRows_physAndStructureSpec, dvhData_pysicianAndStructureSpecificCsvRows_SB.ToString());
        // structure specific json file
        File.WriteAllText(finalDvhJsonPath_randomizedJson_structureSpec, structureSpecificJsonString);
      }
      jsonStringForViewDvh = jsonStringForViewDvh.TrimEnd(',');
      jsonStringForViewDvh = jsonStringForViewDvh + "]}]}]";

      plansJsonArray_randomized = plansJsonArray_randomized.TrimEnd(',');
      plansJsonArray_randomized = plansJsonArray_randomized + "]}]}]";

      // write json file for ViewDvh folder
      File.WriteAllText(viewDvhPath_searchableJson, jsonStringForViewDvh);
      File.WriteAllText(finalDvhJsonPath_patientSpecific, jsonStringForViewDvh);
      File.WriteAllText(finalDvhJsonPath_randomizedJson_patientSpecific, plansJsonArray_randomized);

      #region write html

      string htmlPath = htmlDashboardDirectory + "\\" + DateTime.Now.ToShortDateString().Replace('/', '_') + "_" + courseHeader + "_r" + randomId + ".html";
      StreamWriter stream = new StreamWriter(htmlPath);
      string varPlanJSONArray = "var PlanJSONArray = " + plansJsonArray_randomized;

      using (stream)
      {
        stream.WriteLine(@"<!DOCTYPE html>");
        stream.WriteLine(@"<script src = 'https://ajax.googleapis.com/ajax/libs/jquery/3.1.1/jquery.min.js'></script>");
        stream.WriteLine(@"<link href='https://fonts.googleapis.com/css?family=PT+Sans' rel='stylesheet'>");
        stream.WriteLine(@"<html>");
        stream.WriteLine(@"<head>");
        stream.WriteLine(@"<meta charset='utf-8'/>");
        stream.WriteLine(@"<link rel='stylesheet' type='text/css' href='S:\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanDataDashboard__\\__Style__\\PageStyle.css'>");
        stream.WriteLine(@"<title>DVH Review - " + currentPlanSum.Id + "</title>");
        stream.WriteLine(@"</head>");
        stream.WriteLine(@"<body>");
        stream.WriteLine(@"<div class = 'planInfoDisplay' id = 'infoBlockDislay'>");
        stream.WriteLine(@"<button id = 'print' class='pBtn btn-primary btn-sml' onclick='PrepareAndPrint()'>Print</button>");
        stream.WriteLine(@"<p>Physician:<span class='tabPhysician'>" + primaryPhysician + "</span></p>");
        stream.WriteLine(@"<p>CurrentPlan:<span class='tabPlanName'>" + currentPlanSum.Id + "</span></p>");
        stream.WriteLine(@"<p>ApprovalStatus:<span class='tabPlanStatus'>" + status + "</span></p>");
        stream.WriteLine(@"</div>");
        stream.WriteLine(@"<div id='dvh'></div>");
        stream.WriteLine(@"</div>");
        stream.WriteLine(@"<div id='planStats'></div>");
        stream.WriteLine(@"</div>");
        stream.WriteLine(@"<div id='structureStats'></div>");
        stream.WriteLine(@"</body>");
        stream.WriteLine(@"</html>");
        stream.WriteLine(@"<script>");
        stream.WriteLine(varPlanJSONArray);
        stream.WriteLine(@"$(document).ready(function () {
                                                            var options1 = {

                                                            chart:
                                                                {
                                                                renderTo: 'dvh',
                                                                    type: 'line',
                                                                    zoomType: 'xy',
                                                                    panning: true,
                                                                    panKey: 'shift',
                                                                },
                                                                exporting:
                                                                {
                                                                buttons:
                                                                    {
                                                                    contextButton:
                                                                        {
                                                                        enabled: false
                                                                        }
                                                                    }
                                                                },
                                                                xAxis:
                                                                {
                                                                title:
                                                                    {
                                                                    text: 'Dose (Gy)'
                                                                    },
                                                                    crosshair: true,
                                                                    maxPadding: 0.02
                                                                },
                                                                plotOptions:
                                                                {
                                                                series:
                                                                    {
                                                                    marker:
                                                                        {
                                                                        enabled: false
                                                                        },
                                                                        allowPointSelect: true,
                                                                        states:
                                                                        {
                                                                        hover:
                                                                            {
                                                                            enabled: true,
                                                                                lineWidth: 5
                                                                            }
                                                                        }
                                                                    },
                                                                    boxplot:
                                                                    {
                                                                    fillColor: '#505053'
                                                                    },
                                                                    candlestick:
                                                                    {
                                                                    lineColor: 'white'
                                                                    },
                                                                    errorbar:
                                                                    {
                                                                    color: 'white'
                                                                    }
                                                                },
                                                                yAxis:
                                                                {
                                                                labels:
                                                                    {
                                                                    format: '{value} %'
                                                                    },
                                                                    floor: 0,
                                                                    ceiling: 100,
                                                                    title:
                                                                    {
                                                                    text: 'Volume (%)'
                                                                    },
                                                                    crosshair: true,
                                                                    gridLineDashStyle: 'ShortDash',
                                                                    gridLineColor: '#aaaaaa'
                                                                },
                                                                tooltip:
                                                                {
                                                                shared: true,
                                                                    useHTML: true,
                                                                    headerFormat: '<table>',
                                                                    pointFormat: '<tr><td style=\""color:{series.color}; text-shadow: 0px 0 #353839, 0 1px #353839, 1px 0 #353839, 0 0px #353839;\"">{series.name}: </td><td style=\""text-align: left; color:#282827\"">V{point.x} Gy = {point.y} %</td></tr>',
                                                                    footerFormat: '</table>',
                                                                },
                                                                title:
                                                                {
                                                                text: 'DVH',
                                                                    x: -150
                                                                },
                                                                subtitle:
                                                                {
                                                                text: 'Click and drag to zoom in. Hold down shift key to pan.',
                                                                    x: -150
                                                                },
                                                                legend:
                                                                {
                                                                layout: 'vertical',
                                                                    align: 'right',
                                                                    verticalAlign: 'middle',
                                                                    borderWidth: 0,
                                                                    floating: false,
                                                                    width: 420,
                                                                    itemWidth: 210,
                                                                    itemStyle:
                                                                    {
                                                                    width: 205
                                                                    },
                                                                    itemHiddenStyle:
                                                                    {
                                                                    color: '#ff4d4d'
                                                                    }
                                                                },

                                                                series: seriesOptions
                                                            };

                                                            var options2 = {

                                                                chart: {
                                                            renderTo: 'planStats',
                                                                    type: 'column',
                                                                    panning: true,
                                                                    panKey: 'shift',
                                                                },
                                                                exporting:
                                                            {
                                                            buttons:
                                                                {
                                                                contextButton:
                                                                    {
                                                                    enabled: false
                                                                        }
                                                                }
                                                            },
                                                                xAxis:
                                                            {
                                                            categories: ['MinDose', 'Min(0.03cc)', 'MeanDose', 'MedianDose', 'Max(0.03cc)', 'MaxDose'],
                                                                    title:
                                                                {
                                                                text: 'DoseStatistic'
                                                                    },
                                                                    crosshair: true,
                                                                    maxPadding: 0.02
                                                                },
                                                                plotOptions:
                                                            {
                                                            series:
                                                                {
                                                                marker:
                                                                    {
                                                                    enabled: false
                                                                        },
                                                                        allowPointSelect: true,
                                                                        states:
                                                                    {
                                                                    hover:
                                                                        {
                                                                        enabled: true,
                                                                                lineWidth: 5
                                                                            }
                                                                    }
                                                                },
                                                                    boxplot:
                                                                {
                                                                fillColor: '#505053'
                                                                    },
                                                                    candlestick:
                                                                {
                                                                lineColor: 'white'
                                                                    },
                                                                    errorbar:
                                                                {
                                                                color: 'white'
                                                                    }
                                                            },
                                                                yAxis:
                                                            {
                                                            floor: 0,
                                                                    title:
                                                                {
                                                                text: 'Dose (Gy)'
                                                                    },
                                                                    crosshair: true,
                                                                    gridLineDashStyle: 'ShortDash',
                                                                    gridLineColor: '#aaaaaa'
                                                                },
                                                                tooltip:
                                                            {
                                                            shared: true,
                                                                    useHTML: true,
                                                                    headerFormat: '<table>',
                                                                    pointFormat: '<tr><td style=\""color:{series.color}; text-shadow: 0px 0 #353839, 0 1px #353839, 1px 0 #353839, 0 0px #353839;\"">{series.name}: </td><td style=\""text-align: right; color:#282827\"">{point.y:.3f} Gy</td></tr>',
                                                                    footerFormat: '</table>',
                                                                },
                                                                title:
                                                            {
                                                            text: 'Structure Dose Statistics',
                                                                    x: -150
                                                                },
                                                                subtitle:
                                                            {
                                                            text: '',
                                                                    x: -150
                                                                },
                                                                legend:
                                                            {
                                                            layout: 'vertical',
                                                                    align: 'right',
                                                                    verticalAlign: 'middle',
                                                                    borderWidth: 0,
                                                                    floating: false,
                                                                    width: 420,
                                                                    itemWidth: 210,
                                                                    itemStyle:
                                                                {
                                                                width: 205
                                                                    },
                                                                    itemHiddenStyle:
                                                                {
                                                                color: '#ff4d4d'
                                                                    }
                                                            },

                                                                series: seriesOptions2
                                                            };

                                                        var options3 = {

                                                                chart: {
                                                                    renderTo: 'structureStats',
                                                                    type: 'column',
                                                                    //zoomType: 'xy',
                                                                    panning: true,
                                                                    panKey: 'shift',
                                                                },
                                                                exporting:
                                                        {
                                                        buttons:
                                                            {
                                                            contextButton:
                                                                {
                                                                enabled: false
                                                                        }
                                                            }
                                                        },
                                                                xAxis:
                                                        {
                                                        categories: [''],
                                                                    title:
                                                            {
                                                            text: 'StructureVolume'
                                                                    },
                                                                    crosshair: true,
                                                                    maxPadding: 0.02
                                                                },
                                                                plotOptions:
                                                        {
                                                        series:
                                                            {
                                                            marker:
                                                                {
                                                                enabled: false
                                                                        },
                                                                        allowPointSelect: true,
                                                                        states:
                                                                {
                                                                hover:
                                                                    {
                                                                    enabled: true,
                                                                                lineWidth: 5
                                                                            }
                                                                }
                                                            },
                                                                    boxplot:
                                                            {
                                                            fillColor: '#505053'
                                                                    },
                                                                    candlestick:
                                                            {
                                                            lineColor: 'white'
                                                                    },
                                                                    errorbar:
                                                            {
                                                            color: 'white'
                                                                    }
                                                        },
                                                                yAxis:
                                                        {
                                                        floor: 0,
                                                                    title:
                                                            {
                                                            text: 'Volume (cc)'
                                                                    },
                                                                    crosshair: true,
                                                                    gridLineDashStyle: 'ShortDash',
                                                                    gridLineColor: '#aaaaaa'
                                                                },
                                                                tooltip:
                                                        {
                                                        shared: true,
                                                                    useHTML: true,
                                                                    headerFormat: '<table>',
                                                                    pointFormat: '<tr><td style=\""color:{series.color}; text-shadow: 0px 0 #353839, 0 1px #353839, 1px 0 #353839, 0 0px #353839;\"">{series.name}: </td><td style=\""text-align: right; color:#282827\"">{point.y:.3f} cc</td></tr>',
                                                                    footerFormat: '</table>',
                                                                },
                                                                title:
                                                        {
                                                        text: 'Structure Statistics',
                                                                    x: -150
                                                                },
                                                                subtitle:
                                                        {
                                                        text: '',
                                                                    x: -150
                                                                },
                                                                legend:
                                                        {
                                                        layout: 'vertical',
                                                                    align: 'right',
                                                                    verticalAlign: 'middle',
                                                                    borderWidth: 0,
                                                                    floating: false,
                                                                    width: 420,
                                                                    itemWidth: 210,
                                                                    itemStyle:
                                                            {
                                                            width: 205
                                                                    },
                                                                    itemHiddenStyle:
                                                            {
                                                            color: '#ff4d4d'
                                                                    }
                                                        },

                                                                series: seriesOptions3
                                                            };

                                                    

                                                    var chart1 = new Highcharts.Chart(options1);
                                                    var chart2 = new Highcharts.Chart(options2);
                                                    var chart3 = new Highcharts.Chart(options3);

                                                });

                                                var seriesOptions = [],
                                                    seriesOptions2 = [],
                                                    seriesOptions3 = [],
                                                    seriesOptions4 = [],
                                                    dashStyles = [
                                                        'Solid',
                                                        'ShortDash',
                                                        'ShortDot',
                                                        'ShortDashDot',
                                                        'ShortDashDotDot',
                                                        'Dot',
                                                        'Dash',
                                                        'LongDash',
                                                        'DashDot',
                                                        'LongDashDot',
                                                        'LongDashDotDot'
                                                    ]

                                                var planData = PlanJSONArray[0].planData,
                                                    seriesCounter = 0,
                                                    planCounter = 0,
                                                    counter = 0

                                                planData.forEach(function (element, i) {

                                                    planCounter += seriesCounter
                                                    counter += 1

                                                    element.structureData.forEach(function (childElement, j) {
                                                        if ((element.structureData[j].structureId != 'Body') &&
                                                            (element.structureData[j].structureId != 'BODY') &&
                                                            (element.structureData[j].structureId != 'External') &&
                                                            (element.structureData[j].structureId != 'EXTERNAL'))    {

                                                            seriesOptions[planCounter] = {
                                                                id: element.planId,
                                                                name: element.planId/* + '_' + element.structureData[0].structureId*/,
                                                                data: /*element.structureData[0].dvh*/[],
                                                                dashStyle: dashStyles[planCounter],
                                                                visible: true,
                                                                color: element.structureData[0].structureColor /*'white'*/
                                                                                                                //linkedTo: ':previous'
                                                }

                                                seriesOptions[counter] = {
                                                                        name: element.planId + '_' + element.structureData[j].structureId,
                                                                        data: element.structureData[j].dvh,
                                                                        dashStyle: dashStyles[i],
                                                                        visible: true,
                                                                        color: element.structureData[j].structureColor,
                                                                        linkedTo: element.planId
                                                                    }

                                                //seriesOptions2[seriesCounter] = {
                                                //    //task: element.subTaskId,
                                                //    name: element.planId + '_' + element.structureData[j].structureId,
                                                //    data: [element.structureData[j].minDose, element.structureData[j].min03, element.structureData[j].meanDose,
                                                //            element.structureData[j].medianDose, element.structureData[j].max03, element.structureData[j].maxDose],
                                                //    dashStyle: dashStyles[i],
                                                //    visible: true,
                                                //    color: element.structureData[j].structureColor
                                                //}

                                                seriesOptions2[planCounter] = {
                                                    id: element.planId,
                                                    name: element.planId/* + '_' + element.structureData[0].structureId*/,
                                                    data: /*element.structureData[0].dvh*/[],
                                                    dashStyle: dashStyles[planCounter],
                                                    visible: true,
                                                    color: element.structureData[0].structureColor /*'white'*/
                                                    //linkedTo: ':previous'
                                                }

                                                seriesOptions2[counter] = {
                                                    name: element.planId + '_' + element.structureData[j].structureId,
                                                    data: [element.structureData[j].minDose, element.structureData[j].min03, element.structureData[j].meanDose,
                                                    element.structureData[j].medianDose, element.structureData[j].max03, element.structureData[j].maxDose],
                                                    dashStyle: dashStyles[i],
                                                    visible: true,
                                                    color: element.structureData[j].structureColor,
                                                    linkedTo: element.planId
                                                }

                                                //seriesOptions3[seriesCounter] = {
                                                //    //task: element.subTaskId,
                                                //    name: element.structureData[j].structureId,
                                                //    data: [element.structureData[j].structureVolume],
                                                //    dashStyle: dashStyles[i],
                                                //    visible: true,
                                                //    color: element.structureData[j].structureColor
                                                //}

                                                seriesOptions3[planCounter] = {
                                                    id: element.planId,
                                                    name: element.planId/* + '_' + element.structureData[0].structureId*/,
                                                    data: /*element.structureData[0].dvh*/[],
                                                    dashStyle: dashStyles[planCounter],
                                                    visible: true,
                                                    color: element.structureData[0].structureColor /*'white'*/
                                                    //linkedTo: ':previous'
                                                }

                                                seriesOptions3[counter] = {
                                                    name: element.structureData[j].structureId,
                                                    data: [element.structureData[j].structureVolume],
                                                    dashStyle: dashStyles[i],
                                                    visible: true,
                                                    color: element.structureData[j].structureColor,
                                                    linkedTo: element.planId
                                                }

                                                //seriesOptions3[seriesCounter] = {
                                                //    //task: element.subTaskId,
                                                //    name: element.structureData[j].structureId,
                                                //    data: [element.structureData[j].structureVolume],
                                                //    dashStyle: dashStyles[i],
                                                //    visible: true,
                                                //    color: element.structureData[j].structureColor
                                                //}


                                                seriesCounter += 1
                                                counter += 1
                                            }
                                        })

                                        planCounter += 1

                                    })
                                    function PrepareAndPrint()
                                    {
                                        $('.pBtn').remove();
                                        $('.Buttons').remove();
                                        window.print();
                                    }     
                                </script> ");
        stream.WriteLine(@"<div>");
        stream.WriteLine(@"<div>");
        stream.WriteLine(@"<script src = 'https://code.highcharts.com/highcharts.js'></script >");



        stream.Flush();
        stream.Close();
      }

      #endregion
    }
    public static void recordDvhDataForAllPlanSums(string viewDvhPath_searchableJson, string patientName, string course, IEnumerator sums, string patientId, string randomId, string primaryPhysician)
    {
      string courseHeader = course.Split('-').Last().Replace(" ", "_");
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
      string patientSpecificDirectory = planDataDirectory + "\\_PatientSpecific_\\" + patientId + "\\" + course;
      if (!Directory.Exists(patientSpecificDirectory))
      {
        Directory.CreateDirectory(patientSpecificDirectory);
      }

      string htmlDashboardDirectory = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanDataDashboard__\\_HTML_";
      if (!Directory.Exists(htmlDashboardDirectory))
      {
        Directory.CreateDirectory(htmlDashboardDirectory);
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

      // patientSpecificDvhDataDirectory
      string patientSpecificDvhDataDirectory = patientSpecificDirectory + "\\DvhData";
      if (!Directory.Exists(patientSpecificDvhDataDirectory))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory);
      }

      // patientSpecificDvhDataDirectory_plans
      string patientSpecificDvhDataDirectory_plans = patientSpecificDvhDataDirectory + "\\Plans";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_plans))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_plans);
      }

      // patientSpecificDvhDataDirectory_sums
      string patientSpecificDvhDataDirectory_sums = patientSpecificDvhDataDirectory + "\\Sums";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_sums))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_sums);
      }
      string finalDvhJsonPath_patientSpecific = patientSpecificDvhDataDirectory_sums;

      // patientSpecificDvhDataDirectory_randomized
      string patientSpecificDvhDataDirectory_randomized = patientSpecificDvhDataDirectory + "\\Randomized";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_randomized))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomized);
      }

      // patientSpecificDvhDataDirectory_randomizedJson
      string patientSpecificDvhDataDirectory_randomizedJson = patientSpecificDvhDataDirectory_randomized + "\\JSON";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedJson))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedJson);
      }
      string finalDvhJsonPath_randomizedJson_patientSpecific = patientSpecificDvhDataDirectory_randomizedJson;

      // patientSpecificDvhDataDirectory_randomizedCsvRows
      string patientSpecificDvhDataDirectory_randomizedCsvRows = patientSpecificDvhDataDirectory_randomized + "\\CsvRows";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedCsvRows))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedCsvRows);
      }

      // patientSpecificDvhDataDirectory_randomizedCsvCols
      string patientSpecificDvhDataDirectory_randomizedCsvCols = patientSpecificDvhDataDirectory_randomized + "\\CsvColumns";
      if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedCsvCols))
      {
        Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedCsvCols);
      }

      #endregion

      #endregion

      #region structure specific

      // structure specific directory
      string structureSpecificDirectory = planDataDirectory + "\\_StructureSpecific_";
      if (!Directory.Exists(structureSpecificDirectory))
      {
        Directory.CreateDirectory(structureSpecificDirectory);
      }

      // structure specific target prox stats
      string structureSpecificProximityStatsDirectory = structureSpecificDirectory + "\\TargetProximityStats\\" + courseHeader;
      if (!Directory.Exists(structureSpecificProximityStatsDirectory))
      {
        Directory.CreateDirectory(structureSpecificProximityStatsDirectory);
      }

      // structure specific dvhdata
      string structureSpecificDvhDataDirectory = structureSpecificDirectory + "\\DvhData\\" + courseHeader;
      if (!Directory.Exists(structureSpecificDvhDataDirectory))
      {
        Directory.CreateDirectory(structureSpecificDvhDataDirectory);
      }

      // structure specific dvhdata json
      string structureSpecificDvhDataDirectory_randomizedJson = structureSpecificDvhDataDirectory + "\\JSON";
      if (!Directory.Exists(structureSpecificDvhDataDirectory_randomizedJson))
      {
        Directory.CreateDirectory(structureSpecificDvhDataDirectory_randomizedJson);
      }

      // structure specific dvhdata csv
      string structureSpecificDvhDataDirectory_randomizedCsvRows = structureSpecificDvhDataDirectory + "\\CsvRows";
      if (!Directory.Exists(structureSpecificDvhDataDirectory_randomizedCsvRows))
      {
        Directory.CreateDirectory(structureSpecificDvhDataDirectory_randomizedCsvRows);
      }

      #endregion

      #region physician specific

      // physician folder
      string physicianSpecificDirectory = planDataDirectory + "\\_PhysicianSpecific_\\" + primaryPhysician;
      if (!Directory.Exists(physicianSpecificDirectory))
      {
        Directory.CreateDirectory(physicianSpecificDirectory);
      }

      string physicianSpecificStructureDvhDirectory = physicianSpecificDirectory + "\\StructureDvhData\\" + courseHeader;
      if (!Directory.Exists(physicianSpecificStructureDvhDirectory))
      {
        Directory.CreateDirectory(physicianSpecificStructureDvhDirectory);
      }

      //string physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns = physicianSpecificDirectory + "\\_PlanDvhData_";
      //if (!Directory.Exists(physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns))
      //{
      //    Directory.CreateDirectory(physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns);
      //}

      //string physicianSpecificPlanDvhDataDirectory_randomizedCsvRows = physicianSpecificDirectory + "\\_PlanDvhData_";
      //if (!Directory.Exists(physicianSpecificPlanDvhDataDirectory_randomizedCsvRows))
      //{
      //    Directory.CreateDirectory(physicianSpecificPlanDvhDataDirectory_randomizedCsvRows);
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

      #endregion

      string jsonStringForViewDvh = "[{\"patientName\":\"" + patientName + "\", " +
                                      "\"patientId\":\"" + patientId + "\", " +
                                      "\"randomId\":\"" + randomId + "\", " +
                                      "\"primaryPhysician\":\"" + primaryPhysician + "\", " +
                                      "\"courseId\":\"" + course + "\", " +
                                      "\"courseHeader\":\"" + courseHeader + "\"," +
                                      "\"planData\":[";
      string plansJsonArray_randomized = "[{\"randomId\":\"" + randomId + "\", " +
                                          "\"primaryPhysician\":\"" + primaryPhysician + "\", " +
                                          "\"courseId\":\"" + course + "\", " +
                                          "\"courseHeader\":\"" + courseHeader + "\"," +
                                          "\"planData\":[";
      string status = "PlanSum";
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

        #region physician specific directories (include plan name)

        //physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns = physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns + "\\" + currentPlanSum.Id + "\\CsvColumns";
        //physicianSpecificPlanDvhDataDirectory_randomizedCsvRows = physicianSpecificPlanDvhDataDirectory_randomizedCsvRows + "\\" + currentPlanSum.Id + "\\CsvRows";

        #endregion

        StringBuilder dvhData_randomizedCsvCols_SB = new StringBuilder();
        StringBuilder dvhData_randomizedCsvRows_SB = new StringBuilder();
        StringBuilder dvhData_randomizedCsvRows_SB_master = new StringBuilder();
        StringBuilder dvhData_structureSpecificCsvRows_SB = new StringBuilder();
        StringBuilder dvhData_pysicianAndStructureSpecificCsvRows_SB = new StringBuilder();

        jsonStringForViewDvh = jsonStringForViewDvh + "{\"planId\":\"" + currentPlanSum.Id + "\"," +
                                                        "\"approvalStatus\":\"" + status + "\"," +
                                                        "\"planMaxDose\":" + sumMaxDose + "," +
                                                        "\"structureData\":[";

        plansJsonArray_randomized = plansJsonArray_randomized + "{\"planId\":\"" + currentPlanSum.Id + "\"," +
                                                                "\"approvalStatus\":\"" + status + "\"," +
                                                                "\"planMaxDose\":" + sumMaxDose + "," +
                                                                "\"structureData\":[";

        //jsonStringForViewDvh = "{\"primaryPhysician\":\"" + primaryPhysician + "\"," + 
        //                        "\"planId\":\"" + currentPlan.Id + "\"," +
        //                        "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
        //                        "\"planMaxDose\":" + planMaxDose + "," +
        //                        "\"structureData\":[";

        foreach (var s in zsorted_structureList)
        {
          // clear string builders
          dvhData_randomizedCsvCols_SB.Clear();
          dvhData_randomizedCsvRows_SB.Clear();
          dvhData_randomizedCsvRows_SB_master.Clear();
          dvhData_structureSpecificCsvRows_SB.Clear();
          dvhData_pysicianAndStructureSpecificCsvRows_SB.Clear();

          // variables
          string lowerId = s.Id.ToString().ToLower().Replace(" ", string.Empty).Replace("\\", "_").Replace(".", "_").Replace("/", "_").Split(':').First();
          lowerId = lowerId.Replace('/', '_');
          lowerId = lowerId.Replace('\\', '_');
          double volume = Math.Round(s.Volume, 3);
          string color = "#" + s.Color.ToString().Substring(3, 6);
          string structureSpecificJsonString = string.Empty;

          // define final paths
          string finalDvhCsvPath_randomizedCsvCols_patientSpecific = patientSpecificDvhDataDirectory_randomizedCsvCols + "\\" + courseHeader + "_" + lowerId + "_DVH_col.csv";
          string finalDvhCsvPath_randomizedCsvRows_patientSpecific = patientSpecificDvhDataDirectory_randomizedCsvRows + "\\" + courseHeader + "_" + lowerId + "_DVH_row.csv";
          string finalDvhCsvPath_randomizedCsvRows_masterData = masterPlanDataDirectory + "\\AllPlansDvhData_rows.csv";
          //string finalDvhCsvPath_randomizedCsvCols_physSpec =  physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns + "\\" + lowerId + "_cols.csv";
          //string finalDvhCsvPath_randomizedCsvRows_physSpec = physicianSpecificPlanDvhDataDirectory_randomizedCsvRows + "\\" + lowerId + "_rows.csv";
          string finalDvhCsvPath_randomizedCsvRows_structureSpec = structureSpecificDvhDataDirectory_randomizedCsvRows + "\\" + courseHeader + "_" + lowerId + "_DVH_row.csv";
          string finalDvhCsvPath_randomizedCsvRows_physAndStructureSpec = physicianSpecificStructureDvhDirectory + "\\" + courseHeader + "_" + lowerId + "_DVH_row.csv";
          finalDvhJsonPath_patientSpecific = patientSpecificDvhDataDirectory_plans + "\\" + currentPlanSum.Id + "_DVH.json";
          finalDvhJsonPath_randomizedJson_patientSpecific = patientSpecificDvhDataDirectory_randomizedJson + "\\" + currentPlanSum.Id + "_DVH.json";

          // structure specific json
          string finalDvhJsonPath_randomizedJson_structureSpec = structureSpecificDvhDataDirectory_randomizedJson + "\\" + courseHeader + "_" + lowerId + "_DVH.json";

          if (!File.Exists(finalDvhJsonPath_randomizedJson_structureSpec))
          {
            structureSpecificJsonString = "[";
          }
          else
          {
            using (StreamReader streamReader = new StreamReader(finalDvhJsonPath_randomizedJson_structureSpec, Encoding.UTF8))
            {
              structureSpecificJsonString = streamReader.ReadToEnd();
            }
            //structureSpecificJsonString = File.ReadAllText(finalDvhJsonPath_randomizedJson_structureSpec);
            structureSpecificJsonString = structureSpecificJsonString.TrimEnd(']');
            structureSpecificJsonString = structureSpecificJsonString + ",";
          }

          // dvh data
          DVHData dvhAR = currentPlanSum.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
          DVHData dvhAA = currentPlanSum.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

          // define strings
          //dvhCsvStringBuilder.AppendLine("patientId: " + patientId);
          dvhData_randomizedCsvCols_SB.AppendLine("randomId,\t" + randomId);
          dvhData_randomizedCsvCols_SB.AppendLine("primaryPhysician,\t" + primaryPhysician);
          dvhData_randomizedCsvCols_SB.AppendLine("courseHeader,\t" + courseHeader);
          dvhData_randomizedCsvCols_SB.AppendLine("planId,\t" + currentPlanSum.Id);
          dvhData_randomizedCsvCols_SB.AppendLine("approvalStatus," + status);
          dvhData_randomizedCsvCols_SB.AppendLine("planMaxDose,\t" + sumMaxDose);
          dvhData_randomizedCsvCols_SB.AppendLine("structureId,\t" + lowerId);
          dvhData_randomizedCsvCols_SB.AppendLine("structureColor,\t" + color);
          dvhData_randomizedCsvCols_SB.AppendLine("structureVolume," + volume);

          if (dvhAR != null && dvhAR.CurveData.Length > 0)
          {
            var doseList = new List<string>();
            var relVolumeList = new List<string>();

            double min03 = Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (volume - 0.03)), 3);
            double minDose = Math.Round(dvhAA.MinDose.Dose, 3);
            double meanDose = Math.Round(dvhAR.MeanDose.Dose, 3);
            double max03 = Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (0.03)), 3);
            double maxDose = Math.Round(dvhAR.MaxDose.Dose, 3);
            double medianDose = Math.Round(dvhAR.MedianDose.Dose, 3);
            double std = Math.Round(dvhAR.StdDev, 3);

            // json string to return
            jsonStringForViewDvh = jsonStringForViewDvh + "{\"structureId\":\"" + s.Id + "\"," +
                                                            "\"structureColor\":\"" + color + "\"," +
                                                            "\"structureVolume\":" + volume + "," +
                                                            "\"min03\":" + min03 + "," +
                                                            "\"minDose\":" + minDose + "," +
                                                            "\"meanDose\":" + meanDose + "," +
                                                            "\"max03\":" + max03 + "," +
                                                            "\"maxDose\":" + maxDose + "," +
                                                            "\"medianDose\":" + medianDose + "," +
                                                            "\"std\":" + std + "," +
                                                            "\"dvh\":[";
            plansJsonArray_randomized = plansJsonArray_randomized + "{\"structureId\":\"" + s.Id + "\"," +
                                                                    "\"structureColor\":\"" + color + "\"," +
                                                                    "\"structureVolume\":" + volume + "," +
                                                                    "\"min03\":" + min03 + "," +
                                                                    "\"minDose\":" + minDose + "," +
                                                                    "\"meanDose\":" + meanDose + "," +
                                                                    "\"max03\":" + max03 + "," +
                                                                    "\"maxDose\":" + maxDose + "," +
                                                                    "\"medianDose\":" + medianDose + "," +
                                                                    "\"std\":" + std + "," +
                                                                    "\"dvh\":[";
            // json string to write
            structureSpecificJsonString = structureSpecificJsonString + "{\"structureData\":[" +
                                                                        "{\"randomId\":\"" + randomId + "\"," +
                                                                        "\"primaryPhysician\":\"" + primaryPhysician + "\"," +
                                                                        "\"courseHeader\":\"" + courseHeader + "\"," +
                                                                        "\"planId\":\"" + currentPlanSum.Id + "\"," +
                                                                        "\"approvalStatus\":\"" + status + "\"," +
                                                                        "\"planMaxDose\":" + sumMaxDose + "," +
                                                                        "\"structureId\":\"" + s.Id + "\"," +
                                                                        "\"structureColor\":\"" + color + "\"," +
                                                                        "\"structureVolume\":" + volume + "," +
                                                                        "\"min03\":" + min03 + "," +
                                                                        "\"minDose\":" + minDose + "," +
                                                                        "\"meanDose\":" + meanDose + "," +
                                                                        "\"max03\":" + max03 + "," +
                                                                        "\"maxDose\":" + maxDose + "," +
                                                                        "\"medianDose\":" + medianDose + "," +
                                                                        "\"std\":" + std + "," +
                                                                        "\"dvh\":[";
            // column csv
            dvhData_randomizedCsvCols_SB.AppendLine("min03,\t\t" + min03);
            dvhData_randomizedCsvCols_SB.AppendLine("minDose,\t" + minDose);
            dvhData_randomizedCsvCols_SB.AppendLine("max03,\t\t" + max03);
            dvhData_randomizedCsvCols_SB.AppendLine("maxDose,\t" + maxDose);
            dvhData_randomizedCsvCols_SB.AppendLine("meanDose,\t" + meanDose);
            dvhData_randomizedCsvCols_SB.AppendLine("medianDose:,\t" + medianDose);
            dvhData_randomizedCsvCols_SB.AppendLine("std,\t\t" + std);
            dvhData_randomizedCsvCols_SB.AppendLine("dvh:");
            dvhData_randomizedCsvCols_SB.AppendLine("Dose(Gy),\tVolume(cc),\tVolume(pct)");
            for (double i = 0; i <= dvhAR.MaxDose.Dose + .1; i += .1)
            {
              string dose = string.Format("{0:N1}", i);
              string relVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString();
              string absVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAA, i), 4).ToString();

              // json string to return
              jsonStringForViewDvh = jsonStringForViewDvh + "[" + dose + "," + relVolAtDose + "],";
              plansJsonArray_randomized = plansJsonArray_randomized + "[" + dose + "," + relVolAtDose + "],";
              // json string to write
              structureSpecificJsonString = structureSpecificJsonString + "[" + dose + "," + relVolAtDose + "],";
              // csv string to write
              dvhData_randomizedCsvCols_SB.AppendLine(string.Format("{0},\t\t{1},\t\t{2}", dose, absVolAtDose, relVolAtDose));
              // lists for csv string rows
              doseList.Add("V" + dose);
              relVolumeList.Add(relVolAtDose);
            }
            string doseListString = string.Join(",", doseList.ToArray());
            string relVolumeListString = string.Join(",", relVolumeList.ToArray());

            string headers = "RandomId,PrimaryPhysician,courseHeader,PlanId,ApprovalStatus,PlanMaxDose,StructureId,StructureVolume,Min03,MinDose,Max03,MaxDose,MeanDose,MedianDose,Std," + doseListString;
            var data = randomId + "," + primaryPhysician + "," + courseHeader + "," + currentPlanSum.Id + "," + status + "," + sumMaxDose + "," + lowerId + "," +
                                            volume + "," + min03 + "," + minDose + "," + max03 + "," + maxDose + "," + meanDose + "," + medianDose + "," + std + "," + relVolumeListString;

            // append csv headers
            dvhData_randomizedCsvRows_SB.AppendLine(headers);
            if (!File.Exists(finalDvhCsvPath_randomizedCsvRows_masterData))
            {
              dvhData_randomizedCsvRows_SB_master.AppendLine(headers);
            }
            if (!File.Exists(finalDvhCsvPath_randomizedCsvRows_structureSpec))
            {
              dvhData_structureSpecificCsvRows_SB.AppendLine(headers);
            }
            if (!File.Exists(finalDvhCsvPath_randomizedCsvRows_physAndStructureSpec))
            {
              dvhData_pysicianAndStructureSpecificCsvRows_SB.AppendLine(headers);
            }

            // append csv data
            dvhData_randomizedCsvRows_SB.AppendLine(data);
            dvhData_randomizedCsvRows_SB_master.AppendLine(data);
            dvhData_structureSpecificCsvRows_SB.AppendLine(data);
            dvhData_pysicianAndStructureSpecificCsvRows_SB.AppendLine(data);

            // json string for viewDvh and patient spec randomized json
            jsonStringForViewDvh = jsonStringForViewDvh.TrimEnd(',');
            jsonStringForViewDvh = jsonStringForViewDvh + "]},";

            plansJsonArray_randomized = plansJsonArray_randomized.TrimEnd(',');
            plansJsonArray_randomized = plansJsonArray_randomized + "]},";

            // json string to write
            structureSpecificJsonString = structureSpecificJsonString.TrimEnd(',');
            structureSpecificJsonString = structureSpecificJsonString + "]}]}]";
          }
          // write files
          File.WriteAllText(finalDvhCsvPath_randomizedCsvCols_patientSpecific, dvhData_randomizedCsvCols_SB.ToString());
          File.WriteAllText(finalDvhCsvPath_randomizedCsvRows_patientSpecific, dvhData_randomizedCsvRows_SB.ToString());
          File.AppendAllText(finalDvhCsvPath_randomizedCsvRows_masterData, dvhData_randomizedCsvRows_SB_master.ToString());
          // ph
          //File.AppendAllText(finalDvhCsvPath_randomizedCsvCols_physSpec, dvhData_randomizedCsvCols_SB.ToString());
          //File.AppendAllText(finalDvhCsvPath_randomizedCsvRows_physSpec, dvhData_randomizedCsvRows_SB.ToString());
          // structure specific csv files
          File.AppendAllText(finalDvhCsvPath_randomizedCsvRows_structureSpec, dvhData_structureSpecificCsvRows_SB.ToString());
          File.AppendAllText(finalDvhCsvPath_randomizedCsvRows_physAndStructureSpec, dvhData_pysicianAndStructureSpecificCsvRows_SB.ToString());
          // structure specific json file
          File.WriteAllText(finalDvhJsonPath_randomizedJson_structureSpec, structureSpecificJsonString);
        }
        jsonStringForViewDvh = jsonStringForViewDvh.TrimEnd(',');
        jsonStringForViewDvh = jsonStringForViewDvh + "]},";

        plansJsonArray_randomized = plansJsonArray_randomized.TrimEnd(',');
        plansJsonArray_randomized = plansJsonArray_randomized + "]},";
      }
      jsonStringForViewDvh = jsonStringForViewDvh.TrimEnd(',');
      jsonStringForViewDvh = jsonStringForViewDvh + "]}]";

      plansJsonArray_randomized = plansJsonArray_randomized.TrimEnd(',');
      plansJsonArray_randomized = plansJsonArray_randomized + "]}]";

      // write json file for ViewDvh folder
      File.WriteAllText(viewDvhPath_searchableJson, jsonStringForViewDvh);
      File.WriteAllText(finalDvhJsonPath_patientSpecific, jsonStringForViewDvh);
      File.WriteAllText(finalDvhJsonPath_randomizedJson_patientSpecific, plansJsonArray_randomized);

      #region write html

      string htmlPath = htmlDashboardDirectory + "\\" + DateTime.Now.ToShortDateString().Replace('/', '_') + "_" + courseHeader + "_r" + randomId + ".html";
      StreamWriter stream = new StreamWriter(htmlPath);
      string varPlanJSONArray = "var PlanJSONArray = " + plansJsonArray_randomized;

      using (stream)
      {
        stream.WriteLine(@"<!DOCTYPE html>");
        stream.WriteLine(@"<script src = 'https://ajax.googleapis.com/ajax/libs/jquery/3.1.1/jquery.min.js'></script>");
        stream.WriteLine(@"<link href='https://fonts.googleapis.com/css?family=PT+Sans' rel='stylesheet'>");
        stream.WriteLine(@"<html>");
        stream.WriteLine(@"<head>");
        stream.WriteLine(@"<meta charset='utf-8'/>");
        stream.WriteLine(@"<link rel='stylesheet' type='text/css' href='S:\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanDataDashboard__\\__Style__\\PageStyle.css'>");
        stream.WriteLine(@"<title>DVH Review - " + course + "</title>");
        stream.WriteLine(@"</head>");
        stream.WriteLine(@"<body>");
        stream.WriteLine(@"<div class = 'planInfoDisplay' id = 'infoBlockDislay'>");
        stream.WriteLine(@"<button id = 'print' class='pBtn btn-primary btn-sml' onclick='PrepareAndPrint()'>Print</button>");
        stream.WriteLine(@"<p>Physician:<span class='tabPhysician'>" + primaryPhysician + "</span></p>");
        stream.WriteLine(@"<p>CurrentCourse:<span class='tabPlanName'>" + course + "</span></p>");
        stream.WriteLine(@"<p>ApprovalStatus:<span class='tabPlanStatus'>" + status + "</span></p>");
        stream.WriteLine(@"</div>");
        stream.WriteLine(@"<div id='dvh'></div>");
        stream.WriteLine(@"</div>");
        stream.WriteLine(@"<div id='planStats'></div>");
        stream.WriteLine(@"</div>");
        stream.WriteLine(@"<div id='structureStats'></div>");
        stream.WriteLine(@"</body>");
        stream.WriteLine(@"</html>");
        stream.WriteLine(@"<script>");
        stream.WriteLine(varPlanJSONArray);
        stream.WriteLine(@"$(document).ready(function () {
                                                            var options1 = {

                                                            chart:
                                                                {
                                                                renderTo: 'dvh',
                                                                    type: 'line',
                                                                    zoomType: 'xy',
                                                                    panning: true,
                                                                    panKey: 'shift',
                                                                },
                                                                exporting:
                                                                {
                                                                buttons:
                                                                    {
                                                                    contextButton:
                                                                        {
                                                                        enabled: false
                                                                        }
                                                                    }
                                                                },
                                                                xAxis:
                                                                {
                                                                title:
                                                                    {
                                                                    text: 'Dose (Gy)'
                                                                    },
                                                                    crosshair: true,
                                                                    maxPadding: 0.02
                                                                },
                                                                plotOptions:
                                                                {
                                                                series:
                                                                    {
                                                                    marker:
                                                                        {
                                                                        enabled: false
                                                                        },
                                                                        allowPointSelect: true,
                                                                        states:
                                                                        {
                                                                        hover:
                                                                            {
                                                                            enabled: true,
                                                                                lineWidth: 5
                                                                            }
                                                                        }
                                                                    },
                                                                    boxplot:
                                                                    {
                                                                    fillColor: '#505053'
                                                                    },
                                                                    candlestick:
                                                                    {
                                                                    lineColor: 'white'
                                                                    },
                                                                    errorbar:
                                                                    {
                                                                    color: 'white'
                                                                    }
                                                                },
                                                                yAxis:
                                                                {
                                                                labels:
                                                                    {
                                                                    format: '{value} %'
                                                                    },
                                                                    floor: 0,
                                                                    ceiling: 100,
                                                                    title:
                                                                    {
                                                                    text: 'Volume (%)'
                                                                    },
                                                                    crosshair: true,
                                                                    gridLineDashStyle: 'ShortDash',
                                                                    gridLineColor: '#aaaaaa'
                                                                },
                                                                tooltip:
                                                                {
                                                                shared: true,
                                                                    useHTML: true,
                                                                    headerFormat: '<table>',
                                                                    pointFormat: '<tr><td style=\""color:{series.color}; text-shadow: 0px 0 #353839, 0 1px #353839, 1px 0 #353839, 0 0px #353839;\"">{series.name}: </td><td style=\""text-align: left; color:#282827\"">V{point.x} Gy = {point.y} %</td></tr>',
                                                                    footerFormat: '</table>',
                                                                },
                                                                title:
                                                                {
                                                                text: 'DVH',
                                                                    x: -150
                                                                },
                                                                subtitle:
                                                                {
                                                                text: 'Click and drag to zoom in. Hold down shift key to pan.',
                                                                    x: -150
                                                                },
                                                                legend:
                                                                {
                                                                layout: 'vertical',
                                                                    align: 'right',
                                                                    verticalAlign: 'middle',
                                                                    borderWidth: 0,
                                                                    floating: false,
                                                                    width: 420,
                                                                    itemWidth: 210,
                                                                    itemStyle:
                                                                    {
                                                                    width: 205
                                                                    },
                                                                    itemHiddenStyle:
                                                                    {
                                                                    color: '#ff4d4d'
                                                                    }
                                                                },

                                                                series: seriesOptions
                                                            };

                                                            var options2 = {

                                                                chart: {
                                                            renderTo: 'planStats',
                                                                    type: 'column',
                                                                    panning: true,
                                                                    panKey: 'shift',
                                                                },
                                                                exporting:
                                                            {
                                                            buttons:
                                                                {
                                                                contextButton:
                                                                    {
                                                                    enabled: false
                                                                        }
                                                                }
                                                            },
                                                                xAxis:
                                                            {
                                                            categories: ['MinDose', 'Min(0.03cc)', 'MeanDose', 'MedianDose', 'Max(0.03cc)', 'MaxDose'],
                                                                    title:
                                                                {
                                                                text: 'DoseStatistic'
                                                                    },
                                                                    crosshair: true,
                                                                    maxPadding: 0.02
                                                                },
                                                                plotOptions:
                                                            {
                                                            series:
                                                                {
                                                                marker:
                                                                    {
                                                                    enabled: false
                                                                        },
                                                                        allowPointSelect: true,
                                                                        states:
                                                                    {
                                                                    hover:
                                                                        {
                                                                        enabled: true,
                                                                                lineWidth: 5
                                                                            }
                                                                    }
                                                                },
                                                                    boxplot:
                                                                {
                                                                fillColor: '#505053'
                                                                    },
                                                                    candlestick:
                                                                {
                                                                lineColor: 'white'
                                                                    },
                                                                    errorbar:
                                                                {
                                                                color: 'white'
                                                                    }
                                                            },
                                                                yAxis:
                                                            {
                                                            floor: 0,
                                                                    title:
                                                                {
                                                                text: 'Dose (Gy)'
                                                                    },
                                                                    crosshair: true,
                                                                    gridLineDashStyle: 'ShortDash',
                                                                    gridLineColor: '#aaaaaa'
                                                                },
                                                                tooltip:
                                                            {
                                                            shared: true,
                                                                    useHTML: true,
                                                                    headerFormat: '<table>',
                                                                    pointFormat: '<tr><td style=\""color:{series.color}; text-shadow: 0px 0 #353839, 0 1px #353839, 1px 0 #353839, 0 0px #353839;\"">{series.name}: </td><td style=\""text-align: right; color:#282827\"">{point.y:.3f} Gy</td></tr>',
                                                                    footerFormat: '</table>',
                                                                },
                                                                title:
                                                            {
                                                            text: 'Structure Dose Statistics',
                                                                    x: -150
                                                                },
                                                                subtitle:
                                                            {
                                                            text: '',
                                                                    x: -150
                                                                },
                                                                legend:
                                                            {
                                                            layout: 'vertical',
                                                                    align: 'right',
                                                                    verticalAlign: 'middle',
                                                                    borderWidth: 0,
                                                                    floating: false,
                                                                    width: 420,
                                                                    itemWidth: 210,
                                                                    itemStyle:
                                                                {
                                                                width: 205
                                                                    },
                                                                    itemHiddenStyle:
                                                                {
                                                                color: '#ff4d4d'
                                                                    }
                                                            },

                                                                series: seriesOptions2
                                                            };

                                                        var options3 = {

                                                                chart: {
                                                                    renderTo: 'structureStats',
                                                                    type: 'column',
                                                                    //zoomType: 'xy',
                                                                    panning: true,
                                                                    panKey: 'shift',
                                                                },
                                                                exporting:
                                                        {
                                                        buttons:
                                                            {
                                                            contextButton:
                                                                {
                                                                enabled: false
                                                                        }
                                                            }
                                                        },
                                                                xAxis:
                                                        {
                                                        categories: [''],
                                                                    title:
                                                            {
                                                            text: 'StructureVolume'
                                                                    },
                                                                    crosshair: true,
                                                                    maxPadding: 0.02
                                                                },
                                                                plotOptions:
                                                        {
                                                        series:
                                                            {
                                                            marker:
                                                                {
                                                                enabled: false
                                                                        },
                                                                        allowPointSelect: true,
                                                                        states:
                                                                {
                                                                hover:
                                                                    {
                                                                    enabled: true,
                                                                                lineWidth: 5
                                                                            }
                                                                }
                                                            },
                                                                    boxplot:
                                                            {
                                                            fillColor: '#505053'
                                                                    },
                                                                    candlestick:
                                                            {
                                                            lineColor: 'white'
                                                                    },
                                                                    errorbar:
                                                            {
                                                            color: 'white'
                                                                    }
                                                        },
                                                                yAxis:
                                                        {
                                                        floor: 0,
                                                                    title:
                                                            {
                                                            text: 'Volume (cc)'
                                                                    },
                                                                    crosshair: true,
                                                                    gridLineDashStyle: 'ShortDash',
                                                                    gridLineColor: '#aaaaaa'
                                                                },
                                                                tooltip:
                                                        {
                                                        shared: true,
                                                                    useHTML: true,
                                                                    headerFormat: '<table>',
                                                                    pointFormat: '<tr><td style=\""color:{series.color}; text-shadow: 0px 0 #353839, 0 1px #353839, 1px 0 #353839, 0 0px #353839;\"">{series.name}: </td><td style=\""text-align: right; color:#282827\"">{point.y:.3f} cc</td></tr>',
                                                                    footerFormat: '</table>',
                                                                },
                                                                title:
                                                        {
                                                        text: 'Structure Statistics',
                                                                    x: -150
                                                                },
                                                                subtitle:
                                                        {
                                                        text: '',
                                                                    x: -150
                                                                },
                                                                legend:
                                                        {
                                                        layout: 'vertical',
                                                                    align: 'right',
                                                                    verticalAlign: 'middle',
                                                                    borderWidth: 0,
                                                                    floating: false,
                                                                    width: 420,
                                                                    itemWidth: 210,
                                                                    itemStyle:
                                                            {
                                                            width: 205
                                                                    },
                                                                    itemHiddenStyle:
                                                            {
                                                            color: '#ff4d4d'
                                                                    }
                                                        },

                                                                series: seriesOptions3
                                                            };

                                                    

                                                    var chart1 = new Highcharts.Chart(options1);
                                                    var chart2 = new Highcharts.Chart(options2);
                                                    var chart3 = new Highcharts.Chart(options3);

                                                });

                                                var seriesOptions = [],
                                                    seriesOptions2 = [],
                                                    seriesOptions3 = [],
                                                    seriesOptions4 = [],
                                                    dashStyles = [
                                                        'Solid',
                                                        'ShortDash',
                                                        'ShortDot',
                                                        'ShortDashDot',
                                                        'ShortDashDotDot',
                                                        'Dot',
                                                        'Dash',
                                                        'LongDash',
                                                        'DashDot',
                                                        'LongDashDot',
                                                        'LongDashDotDot'
                                                    ]

                                                var planData = PlanJSONArray[0].planData,
                                                    seriesCounter = 0,
                                                    planCounter = 0,
                                                    counter = 0

                                                planData.forEach(function (element, i) {

                                                    planCounter += seriesCounter
                                                    counter += 1

                                                    element.structureData.forEach(function (childElement, j) {
                                                        if ((element.structureData[j].structureId != 'Body') &&
                                                            (element.structureData[j].structureId != 'BODY') &&
                                                            (element.structureData[j].structureId != 'External') &&
                                                            (element.structureData[j].structureId != 'EXTERNAL'))    {

                                                            seriesOptions[planCounter] = {
                                                                id: element.planId,
                                                                name: element.planId/* + '_' + element.structureData[0].structureId*/,
                                                                data: /*element.structureData[0].dvh*/[],
                                                                dashStyle: dashStyles[planCounter],
                                                                visible: true,
                                                                color: element.structureData[0].structureColor /*'white'*/
                                                                                                                //linkedTo: ':previous'
                                                }

                                                seriesOptions[counter] = {
                                                                        name: element.planId + '_' + element.structureData[j].structureId,
                                                                        data: element.structureData[j].dvh,
                                                                        dashStyle: dashStyles[i],
                                                                        visible: true,
                                                                        color: element.structureData[j].structureColor,
                                                                        linkedTo: element.planId
                                                                    }

                                                //seriesOptions2[seriesCounter] = {
                                                //    //task: element.subTaskId,
                                                //    name: element.planId + '_' + element.structureData[j].structureId,
                                                //    data: [element.structureData[j].minDose, element.structureData[j].min03, element.structureData[j].meanDose,
                                                //            element.structureData[j].medianDose, element.structureData[j].max03, element.structureData[j].maxDose],
                                                //    dashStyle: dashStyles[i],
                                                //    visible: true,
                                                //    color: element.structureData[j].structureColor
                                                //}

                                                seriesOptions2[planCounter] = {
                                                    id: element.planId,
                                                    name: element.planId/* + '_' + element.structureData[0].structureId*/,
                                                    data: /*element.structureData[0].dvh*/[],
                                                    dashStyle: dashStyles[planCounter],
                                                    visible: true,
                                                    color: element.structureData[0].structureColor /*'white'*/
                                                    //linkedTo: ':previous'
                                                }

                                                seriesOptions2[counter] = {
                                                    name: element.planId + '_' + element.structureData[j].structureId,
                                                    data: [element.structureData[j].minDose, element.structureData[j].min03, element.structureData[j].meanDose,
                                                    element.structureData[j].medianDose, element.structureData[j].max03, element.structureData[j].maxDose],
                                                    dashStyle: dashStyles[i],
                                                    visible: true,
                                                    color: element.structureData[j].structureColor,
                                                    linkedTo: element.planId
                                                }

                                                //seriesOptions3[seriesCounter] = {
                                                //    //task: element.subTaskId,
                                                //    name: element.structureData[j].structureId,
                                                //    data: [element.structureData[j].structureVolume],
                                                //    dashStyle: dashStyles[i],
                                                //    visible: true,
                                                //    color: element.structureData[j].structureColor
                                                //}

                                                seriesOptions3[planCounter] = {
                                                    id: element.planId,
                                                    name: element.planId/* + '_' + element.structureData[0].structureId*/,
                                                    data: /*element.structureData[0].dvh*/[],
                                                    dashStyle: dashStyles[planCounter],
                                                    visible: true,
                                                    color: element.structureData[0].structureColor /*'white'*/
                                                    //linkedTo: ':previous'
                                                }

                                                seriesOptions3[counter] = {
                                                    name: element.structureData[j].structureId,
                                                    data: [element.structureData[j].structureVolume],
                                                    dashStyle: dashStyles[i],
                                                    visible: true,
                                                    color: element.structureData[j].structureColor,
                                                    linkedTo: element.planId
                                                }

                                                //seriesOptions3[seriesCounter] = {
                                                //    //task: element.subTaskId,
                                                //    name: element.structureData[j].structureId,
                                                //    data: [element.structureData[j].structureVolume],
                                                //    dashStyle: dashStyles[i],
                                                //    visible: true,
                                                //    color: element.structureData[j].structureColor
                                                //}


                                                seriesCounter += 1
                                                counter += 1
                                            }
                                        })

                                        planCounter += 1

                                    })
                                    function PrepareAndPrint()
                                    {
                                        $('.pBtn').remove();
                                        $('.Buttons').remove();
                                        window.print();
                                    }     
                                </script> ");
        stream.WriteLine(@"<div>");
        stream.WriteLine(@"<div>");
        stream.WriteLine(@"<script src = 'https://code.highcharts.com/highcharts.js'></script >");



        stream.Flush();
        stream.Close();
      }

      #endregion
    }

    #region old //
    //public static string getDvhJsonCurrentPlan(PlanSetup currentPlan, IEnumerable<Structure> sorted_structureList)
    //{
    //    currentPlan.DoseValuePresentation = DoseValuePresentation.Absolute;

    //    double planMaxDose = 0;
    //    if (currentPlan.Dose != null)
    //    {
    //        planMaxDose = Math.Round(currentPlan.Dose.DoseMax3D.Dose, 3);
    //    }
    //    else { planMaxDose = Double.NaN; }

    //    string currentPlanJsonString = "{\"planId\":\"" + currentPlan.Id + "\"," +
    //                                        "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
    //                                        "\"planMaxDose\":" + planMaxDose + "," +
    //                                        "\"structureData\":[";
    //    foreach (var s in sorted_structureList)
    //    {
    //        DVHData dvhAR = currentPlan.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
    //        DVHData dvhAA = currentPlan.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);
    //        if (dvhAR != null && dvhAR.CurveData.Length > 0)
    //        {
    //            string color = "#" + s.Color.ToString().Substring(3, 6);
    //            double volume = Math.Round(s.Volume, 3);
    //            double min03 = Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (volume - 0.03)), 3);
    //            double minDose = Math.Round(dvhAA.MinDose.Dose, 3);
    //            double meanDose = Math.Round(dvhAR.MeanDose.Dose, 3);
    //            double max03 = Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (0.03)), 3);
    //            double maxDose = Math.Round(dvhAR.MaxDose.Dose, 3);
    //            double medianDose = Math.Round(dvhAR.MedianDose.Dose, 3);
    //            double std = Math.Round(dvhAR.StdDev, 3);
    //            currentPlanJsonString = currentPlanJsonString + "{\"structureId\":\"" + s.Id + "\"," +
    //                                                                "\"structureColor\":\"" + color + "\"," +
    //                                                                "\"structureVolume\":" + volume + "," +
    //                                                                "\"min03\":" + min03 + "," +
    //                                                                "\"minDose\":" + minDose + "," +
    //                                                                "\"meanDose\":" + meanDose + "," +
    //                                                                "\"max03\":" + max03 + "," +
    //                                                                "\"maxDose\":" + maxDose + "," +
    //                                                                "\"medianDose\":" + medianDose + "," +
    //                                                                "\"std\":" + std + "," +
    //                                                                "\"dvh\":[";
    //            for (double i = 0; i <= dvhAR.MaxDose.Dose + .1; i += .1)
    //            {
    //                string dose = string.Format("{0:N1}", i);
    //                string relVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString();
    //                currentPlanJsonString = currentPlanJsonString + "[" + dose + "," + relVolAtDose + "],";
    //            }
    //            currentPlanJsonString = currentPlanJsonString.TrimEnd(',');
    //            currentPlanJsonString = currentPlanJsonString + "]},";
    //        }
    //    }
    //    //currentPlanJsonString = currentPlanJsonString + "]},";
    //    currentPlanJsonString = currentPlanJsonString.TrimEnd(',');
    //    currentPlanJsonString = currentPlanJsonString + "]}]}";
    //    return currentPlanJsonString;
    //}
    //public static string getDvhJsonAllPlans(IEnumerator plans)
    //{
    //    string allPlansDvhString = "";
    //    while (plans.MoveNext())
    //    {
    //        PlanSetup currentPlan = (PlanSetup)plans.Current;
    //        currentPlan.DoseValuePresentation = DoseValuePresentation.Absolute;

    //        #region organize structures into ordered lists
    //        // lists for structures
    //        List<Structure> zgtvList = new List<Structure>();
    //        List<Structure> zctvList = new List<Structure>();
    //        List<Structure> zitvList = new List<Structure>();
    //        List<Structure> zptvList = new List<Structure>();
    //        List<Structure> zoarList = new List<Structure>();
    //        List<Structure> ztargetList = new List<Structure>();
    //        List<Structure> zstructureList = new List<Structure>();
    //        IEnumerable<Structure> zsorted_gtvList;
    //        IEnumerable<Structure> zsorted_ctvList;
    //        IEnumerable<Structure> zsorted_itvList;
    //        IEnumerable<Structure> zsorted_ptvList;
    //        IEnumerable<Structure> zsorted_targetList;
    //        IEnumerable<Structure> zsorted_oarList;
    //        IEnumerable<Structure> zsorted_structureList;

    //        foreach (var structure in currentPlan.StructureSet.Structures)
    //        {
    //            // conditions for adding any structure
    //            if ((!structure.IsEmpty) &&
    //                (structure.HasSegment) &&
    //                (!structure.Id.Contains("*")) &&
    //                (!structure.Id.ToLower().Contains("markers")) &&
    //                (!structure.Id.ToLower().Contains("avoid")) &&
    //                (!structure.Id.ToLower().Contains("dose")) &&
    //                (!structure.Id.ToLower().Contains("contrast")) &&
    //                (!structure.Id.ToLower().Contains("air")) &&
    //                (!structure.Id.ToLower().Contains("dens")) &&
    //                (!structure.Id.ToLower().Contains("bolus")) &&
    //                (!structure.Id.ToLower().Contains("suv")) &&
    //                (!structure.Id.ToLower().Contains("match")) &&
    //                (!structure.Id.ToLower().Contains("wire")) &&
    //                (!structure.Id.ToLower().Contains("scar")) &&
    //                (!structure.Id.ToLower().Contains("chemo")) &&
    //                (!structure.Id.ToLower().Contains("pet")) &&
    //                (!structure.Id.ToLower().Contains("dnu")) &&
    //                (!structure.Id.ToLower().Contains("fiducial")) &&
    //                (!structure.Id.ToLower().Contains("artifact")) &&
    //                (!structure.Id.ToLower().Contains("ci-")) &&
    //                (!structure.Id.ToLower().Contains("ci_")) &&
    //                (!structure.Id.ToLower().Contains("r50")) &&
    //                (!structure.Id.StartsWith("z", StringComparison.InvariantCultureIgnoreCase)) &&
    //                (!structure.Id.StartsWith("hs", StringComparison.InvariantCultureIgnoreCase)) &&
    //                (!structure.Id.StartsWith("av", StringComparison.InvariantCultureIgnoreCase)) &&
    //                (!structure.Id.StartsWith("opti ", StringComparison.InvariantCultureIgnoreCase)) &&
    //                (!structure.Id.StartsWith("opti-", StringComparison.InvariantCultureIgnoreCase)))
    //            //(structure.Id.Contains("CI-", StringComparison.InvariantCultureIgnoreCase) == false) && 
    //            //(structure.Id.Contains("R50-", StringComparison.InvariantCultureIgnoreCase) == false) &&
    //            //(structure.Id.Contains("CI_", StringComparison.InvariantCultureIgnoreCase) == false) && 
    //            //(structure.Id.Contains("R50_", StringComparison.InvariantCultureIgnoreCase) == false))
    //            {
    //                if (structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase))
    //                {
    //                    zgtvList.Add(structure);
    //                    zstructureList.Add(structure);
    //                    ztargetList.Add(structure);
    //                }
    //                if ((structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) ||
    //                    (structure.Id.StartsWith("Prost", StringComparison.InvariantCultureIgnoreCase)))
    //                {
    //                    zctvList.Add(structure);
    //                    zstructureList.Add(structure);
    //                    ztargetList.Add(structure);
    //                }
    //                if (structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase))
    //                {
    //                    zitvList.Add(structure);
    //                    zstructureList.Add(structure);
    //                    ztargetList.Add(structure);
    //                }
    //                if (structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase))
    //                {
    //                    zptvList.Add(structure);
    //                    zstructureList.Add(structure);
    //                    ztargetList.Add(structure);
    //                }
    //                // conditions for adding breast plan targets
    //                if ((structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) ||
    //                    (structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) ||
    //                    (structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) ||
    //                    (structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)))
    //                {
    //                    ztargetList.Add(structure);
    //                    zstructureList.Add(structure);
    //                }
    //                // conditions for adding oars
    //                if ((!structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("Scar", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.ToLower().Contains("carina")))
    //                {
    //                    zoarList.Add(structure);
    //                    zstructureList.Add(structure);
    //                }
    //            }
    //        }
    //        zsorted_gtvList = zgtvList.OrderBy(x => x.Id);
    //        zsorted_ctvList = zctvList.OrderBy(x => x.Id);
    //        zsorted_itvList = zitvList.OrderBy(x => x.Id);
    //        zsorted_ptvList = zptvList.OrderBy(x => x.Id);
    //        zsorted_targetList = ztargetList.OrderBy(x => x.Id);
    //        zsorted_oarList = zoarList.OrderBy(x => x.Id);
    //        zsorted_structureList = zstructureList.OrderBy(x => x.Id);

    //        #endregion structure organization and ordering

    //        double planMaxDose = 0;
    //        if (currentPlan.Dose != null)
    //        {
    //            planMaxDose = Math.Round(currentPlan.Dose.DoseMax3D.Dose, 3);
    //        }
    //        else { planMaxDose = Double.NaN; }

    //        allPlansDvhString = allPlansDvhString + "{\"planId\":\"" + currentPlan.Id + "\"," +
    //                                                    "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
    //                                                    "\"planMaxDose\":" + planMaxDose + "," +
    //                                                    "\"structureData\":[";
    //        foreach (var s in zsorted_structureList)
    //        {
    //            //allPlansDvhString = allPlansDvhString + "]},";
    //            DVHData dvhAR = currentPlan.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
    //            DVHData dvhAA = currentPlan.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);
    //            if (dvhAR != null && dvhAR.CurveData.Length > 0)
    //            {
    //                string color = "#" + s.Color.ToString().Substring(3, 6);
    //                double volume = Math.Round(s.Volume, 3);
    //                double min03 = Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (volume - 0.03)), 3);
    //                double minDose = Math.Round(dvhAA.MinDose.Dose, 3);
    //                double meanDose = Math.Round(dvhAR.MeanDose.Dose, 3);
    //                double max03 = Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (0.03)), 3);
    //                double maxDose = Math.Round(dvhAR.MaxDose.Dose, 3);
    //                double medianDose = Math.Round(dvhAR.MedianDose.Dose, 3);
    //                double std = Math.Round(dvhAR.StdDev, 3);
    //                allPlansDvhString = allPlansDvhString + "{\"structureId\":\"" + s.Id + "\"," +
    //                                                            "\"structureColor\":\"" + color + "\"," +
    //                                                            "\"structureVolume\":" + volume + "," +
    //                                                            "\"min03\":" + min03 + "," +
    //                                                            "\"minDose\":" + minDose + "," +
    //                                                            "\"meanDose\":" + meanDose + "," +
    //                                                            "\"max03\":" + max03 + "," +
    //                                                            "\"maxDose\":" + maxDose + "," +
    //                                                            "\"medianDose\":" + medianDose + "," +
    //                                                            "\"std\":" + std + "," +
    //                                                            "\"dvh\":[";
    //                for (double i = 0; i <= dvhAR.MaxDose.Dose + .1; i += .1)
    //                {
    //                    string dose = string.Format("{0:N1}", i);
    //                    string relVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString();
    //                    allPlansDvhString = allPlansDvhString + "[" + dose + "," + relVolAtDose + "],";
    //                }
    //                allPlansDvhString = allPlansDvhString.TrimEnd(',');
    //                allPlansDvhString = allPlansDvhString + "]},";
    //            }
    //        }
    //        allPlansDvhString = allPlansDvhString.TrimEnd(',');
    //        allPlansDvhString = allPlansDvhString + "]},";
    //    }
    //    allPlansDvhString = allPlansDvhString.TrimEnd(',');
    //    allPlansDvhString = allPlansDvhString + "]}";
    //    return allPlansDvhString;
    //}
    //public static string getDvhJsonCurrentPlanSum(PlanSum currentPlanSum, IEnumerable<Structure> sorted_structureList)
    //{
    //    double sumMaxDose = 0;
    //    if (currentPlanSum.Dose != null)
    //    {
    //        sumMaxDose = Math.Round(currentPlanSum.Dose.DoseMax3D.Dose, 3);
    //    }
    //    else { sumMaxDose = Double.NaN; }

    //    string currentPlanSumJsonString = "{\"planId\":\"" + currentPlanSum.Id + "\"," +
    //                                        "\"approvalStatus\":\"" + "PlanSum" + "\"," +
    //                                        "\"planMaxDose\":" + sumMaxDose + "," +
    //                                        "\"structureData\":[";
    //    foreach (var s in sorted_structureList)
    //    {
    //        DVHData dvhAR = currentPlanSum.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
    //        DVHData dvhAA = currentPlanSum.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);
    //        if (dvhAR != null && dvhAR.CurveData.Length > 0)
    //        {
    //            string color = "#" + s.Color.ToString().Substring(3, 6);
    //            double volume = Math.Round(s.Volume, 3);
    //            double min03 = Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (volume - 0.03)), 3);
    //            double minDose = Math.Round(dvhAA.MinDose.Dose, 3);
    //            double meanDose = Math.Round(dvhAR.MeanDose.Dose, 3);
    //            double max03 = Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (0.03)), 3);
    //            double maxDose = Math.Round(dvhAR.MaxDose.Dose, 3);
    //            double medianDose = Math.Round(dvhAR.MedianDose.Dose, 3);
    //            double std = Math.Round(dvhAR.StdDev, 3);
    //            currentPlanSumJsonString = currentPlanSumJsonString + "{\"structureId\":\"" + s.Id + "\"," +
    //                                                                    "\"structureColor\":\"" + color + "\"," +
    //                                                                    "\"structureVolume\":" + volume + "," +
    //                                                                    "\"min03\":" + min03 + "," +
    //                                                                    "\"minDose\":" + minDose + "," +
    //                                                                    "\"meanDose\":" + meanDose + "," +
    //                                                                    "\"max03\":" + max03 + "," +
    //                                                                    "\"maxDose\":" + maxDose + "," +
    //                                                                    "\"medianDose\":" + medianDose + "," +
    //                                                                    "\"std\":" + std + "," +
    //                                                                    "\"dvh\":[";
    //            for (double i = 0; i <= dvhAR.MaxDose.Dose + .1; i += .1)
    //            {
    //                string dose = string.Format("{0:N1}", i);
    //                string relVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString();
    //                currentPlanSumJsonString = currentPlanSumJsonString + "[" + dose + "," + relVolAtDose + "],";
    //            }
    //            currentPlanSumJsonString = currentPlanSumJsonString.TrimEnd(',');
    //            currentPlanSumJsonString = currentPlanSumJsonString + "]},";
    //        }
    //    }
    //    //currentPlanSumJsonString = currentPlanSumJsonString + "]},";
    //    currentPlanSumJsonString = currentPlanSumJsonString.TrimEnd(',');
    //    currentPlanSumJsonString = currentPlanSumJsonString + "]}]}";
    //    return currentPlanSumJsonString;
    //}
    //public static string getDvhJsonAllPlanSums(IEnumerator sums)
    //{
    //    string allPlanSumsDvhString = "";
    //    while (sums.MoveNext())
    //    {
    //        PlanSum currentPlanSum = (PlanSum)sums.Current;

    //        #region organize structures into ordered lists
    //        // lists for structures
    //        List<Structure> zgtvList = new List<Structure>();
    //        List<Structure> zctvList = new List<Structure>();
    //        List<Structure> zitvList = new List<Structure>();
    //        List<Structure> zptvList = new List<Structure>();
    //        List<Structure> zoarList = new List<Structure>();
    //        List<Structure> ztargetList = new List<Structure>();
    //        List<Structure> zstructureList = new List<Structure>();
    //        IEnumerable<Structure> zsorted_gtvList;
    //        IEnumerable<Structure> zsorted_ctvList;
    //        IEnumerable<Structure> zsorted_itvList;
    //        IEnumerable<Structure> zsorted_ptvList;
    //        IEnumerable<Structure> zsorted_targetList;
    //        IEnumerable<Structure> zsorted_oarList;
    //        IEnumerable<Structure> zsorted_structureList;

    //        foreach (var structure in currentPlanSum.StructureSet.Structures)
    //        {
    //            // conditions for adding any structure
    //            if ((!structure.IsEmpty) &&
    //                (structure.HasSegment) &&
    //                (!structure.Id.Contains("*")) &&
    //                (!structure.Id.ToLower().Contains("markers")) &&
    //                (!structure.Id.ToLower().Contains("avoid")) &&
    //                (!structure.Id.ToLower().Contains("dose")) &&
    //                (!structure.Id.ToLower().Contains("contrast")) &&
    //                (!structure.Id.ToLower().Contains("air")) &&
    //                (!structure.Id.ToLower().Contains("dens")) &&
    //                (!structure.Id.ToLower().Contains("bolus")) &&
    //                (!structure.Id.ToLower().Contains("suv")) &&
    //                (!structure.Id.ToLower().Contains("match")) &&
    //                (!structure.Id.ToLower().Contains("wire")) &&
    //                (!structure.Id.ToLower().Contains("scar")) &&
    //                (!structure.Id.ToLower().Contains("chemo")) &&
    //                (!structure.Id.ToLower().Contains("pet")) &&
    //                (!structure.Id.ToLower().Contains("dnu")) &&
    //                (!structure.Id.ToLower().Contains("fiducial")) &&
    //                (!structure.Id.ToLower().Contains("artifact")) &&
    //                (!structure.Id.ToLower().Contains("ci-")) &&
    //                (!structure.Id.ToLower().Contains("ci_")) &&
    //                (!structure.Id.ToLower().Contains("r50")) &&
    //                (!structure.Id.StartsWith("z", StringComparison.InvariantCultureIgnoreCase)) &&
    //                (!structure.Id.StartsWith("hs", StringComparison.InvariantCultureIgnoreCase)) &&
    //                (!structure.Id.StartsWith("av", StringComparison.InvariantCultureIgnoreCase)) &&
    //                (!structure.Id.StartsWith("opti ", StringComparison.InvariantCultureIgnoreCase)) &&
    //                (!structure.Id.StartsWith("opti-", StringComparison.InvariantCultureIgnoreCase)))
    //            //(structure.Id.Contains("CI-", StringComparison.InvariantCultureIgnoreCase) == false) && 
    //            //(structure.Id.Contains("R50-", StringComparison.InvariantCultureIgnoreCase) == false) &&
    //            //(structure.Id.Contains("CI_", StringComparison.InvariantCultureIgnoreCase) == false) && 
    //            //(structure.Id.Contains("R50_", StringComparison.InvariantCultureIgnoreCase) == false))
    //            {
    //                if (structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase))
    //                {
    //                    zgtvList.Add(structure);
    //                    zstructureList.Add(structure);
    //                    ztargetList.Add(structure);
    //                }
    //                if ((structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) ||
    //                    (structure.Id.StartsWith("Prost", StringComparison.InvariantCultureIgnoreCase)))
    //                {
    //                    zctvList.Add(structure);
    //                    zstructureList.Add(structure);
    //                    ztargetList.Add(structure);
    //                }
    //                if (structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase))
    //                {
    //                    zitvList.Add(structure);
    //                    zstructureList.Add(structure);
    //                    ztargetList.Add(structure);
    //                }
    //                if (structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase))
    //                {
    //                    zptvList.Add(structure);
    //                    zstructureList.Add(structure);
    //                    ztargetList.Add(structure);
    //                }
    //                // conditions for adding breast plan targets
    //                if ((structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) ||
    //                    (structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) ||
    //                    (structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) ||
    //                    (structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)))
    //                {
    //                    ztargetList.Add(structure);
    //                    zstructureList.Add(structure);
    //                }
    //                // conditions for adding oars
    //                if ((!structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.StartsWith("Scar", StringComparison.InvariantCultureIgnoreCase)) &&
    //                    (!structure.Id.ToLower().Contains("carina")))
    //                {
    //                    zoarList.Add(structure);
    //                    zstructureList.Add(structure);
    //                }
    //            }
    //        }
    //        zsorted_gtvList = zgtvList.OrderBy(x => x.Id);
    //        zsorted_ctvList = zctvList.OrderBy(x => x.Id);
    //        zsorted_itvList = zitvList.OrderBy(x => x.Id);
    //        zsorted_ptvList = zptvList.OrderBy(x => x.Id);
    //        zsorted_targetList = ztargetList.OrderBy(x => x.Id);
    //        zsorted_oarList = zoarList.OrderBy(x => x.Id);
    //        zsorted_structureList = zstructureList.OrderBy(x => x.Id);

    //        #endregion structure organization and ordering

    //        double sumMaxDose = 0;
    //        if (currentPlanSum.Dose != null)
    //        {
    //            sumMaxDose = Math.Round(currentPlanSum.Dose.DoseMax3D.Dose, 3);
    //        }
    //        else { sumMaxDose = Double.NaN; }

    //        allPlanSumsDvhString = allPlanSumsDvhString + "{\"planId\":\"" + currentPlanSum.Id + "\"," +
    //                                                        "\"approvalStatus\":\"" + "PlanSum" + "\"," +
    //                                                        "\"planMaxDose\":" + sumMaxDose + "," +
    //                                                        "\"structureData\":[";
    //        foreach (var s in zsorted_structureList)
    //        {
    //            DVHData dvhAR = currentPlanSum.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
    //            DVHData dvhAA = currentPlanSum.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);
    //            if (dvhAR != null && dvhAR.CurveData.Length > 0)
    //            {
    //                string color = "#" + s.Color.ToString().Substring(3, 6);
    //                double volume = Math.Round(s.Volume, 3);
    //                double min03 = Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (volume - 0.03)), 3);
    //                double minDose = Math.Round(dvhAA.MinDose.Dose, 3);
    //                double meanDose = Math.Round(dvhAR.MeanDose.Dose, 3);
    //                double max03 = Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (0.03)), 3);
    //                double maxDose = Math.Round(dvhAR.MaxDose.Dose, 3);
    //                double medianDose = Math.Round(dvhAR.MedianDose.Dose, 3);
    //                double std = Math.Round(dvhAR.StdDev, 3);
    //                allPlanSumsDvhString = allPlanSumsDvhString + "{\"structureId\":\"" + s.Id + "\"," +
    //                                                                "\"structureColor\":\"" + color + "\"," +
    //                                                                "\"structureVolume\":" + volume + "," +
    //                                                                "\"min03\":" + min03 + "," +
    //                                                                "\"minDose\":" + minDose + "," +
    //                                                                "\"meanDose\":" + meanDose + "," +
    //                                                                "\"max03\":" + max03 + "," +
    //                                                                "\"maxDose\":" + maxDose + "," +
    //                                                                "\"medianDose\":" + medianDose + "," +
    //                                                                "\"std\":" + std + "," +
    //                                                                "\"dvh\":[";
    //                for (double i = 0; i <= dvhAR.MaxDose.Dose + .1; i += .1)
    //                {
    //                    string dose = string.Format("{0:N1}", i);
    //                    string relVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString();
    //                    allPlanSumsDvhString = allPlanSumsDvhString + "[" + dose + "," + relVolAtDose + "],";
    //                }
    //                allPlanSumsDvhString = allPlanSumsDvhString.TrimEnd(',');
    //                allPlanSumsDvhString = allPlanSumsDvhString + "]},";
    //            }
    //        }
    //        allPlanSumsDvhString = allPlanSumsDvhString.TrimEnd(',');
    //        allPlanSumsDvhString = allPlanSumsDvhString + "]},";
    //    }
    //    allPlanSumsDvhString = allPlanSumsDvhString.TrimEnd(',');
    //    allPlanSumsDvhString = allPlanSumsDvhString + "]}";
    //    return allPlanSumsDvhString;
    //}
    #endregion
  }
}
