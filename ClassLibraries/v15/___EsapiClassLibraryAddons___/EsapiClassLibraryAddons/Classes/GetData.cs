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
  using System.Diagnostics;

  /// <summary>
  /// A collection of methods that can be used to collect JSON and CSV data for the various plan types: single plan, all plans in given context, single plan sum, and all plan sums in given context.
  /// </summary>
  public class GetData
  {

    /// <summary>
    /// adaptation for not using gui
    /// </summary>
    /// <param name="plan"></param>
    /// <param name="plannedTxSite">e.g., "HN"</param>
    public static void GetResidentPlanData(PlanSetup plan, string plannedTxSite)
    {
      //---------------------------------------------------------------------------------
      #region plan context, maincontrol, and window defitions

      //---------------------------------------------------------------------------------
      #region variable definitions

      #region private internal variables

      #region data collection variables

      PPlan pp;
      Patient patient;

      List<ESJO> JsonObjList = new List<ESJO>();
      List<PPlan> pplans = new List<PPlan>();
      List<Tuple<string, string>> txSiteList = new List<Tuple<string, string>>
      {
      //  tuple including planned tx site and directory associated with the site
      //  NOTE: will actually just save all to the same directory
      Tuple.Create("HN", @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\_JsonData_\\ResidentEducation\\"),
      Tuple.Create("Prostate", @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\_JsonData_\\ResidentEducation\\")
      };

      // path variables
      string resEdDBPath = string.Empty;
      string resEdDBJsVarPath = string.Empty;
      string resEdTxSiteId = string.Empty;

      bool saveToResident = true; // always true since will always collect

      // course info
      string courseHeader = string.Empty;
      string courseId = string.Empty;

      // resident info
      string resId = string.Empty;

      // time info
      string date = string.Empty;
      string time = string.Empty;

      #endregion

      #endregion

      // time variables
      date = string.Format("{0}_{1}_{2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
      time = DateTime.Now.TimeOfDay.Hours + ":" + DateTime.Now.TimeOfDay.Minutes + ":" + DateTime.Now.TimeOfDay.Seconds; ;

      // user variables
      resId = plan.StructureSet.HistoryUserName; // maybe?

      // plan variables
      patient = plan.Course.Patient;
      courseHeader = plan.Course.Id.ToString().Split('-').Last().Replace(" ", "_");
      courseId = plan.Course.Id.ToString();

      pp = PPlan.CreatePPlan(plan);
      if (!pplans.Contains(pp))
      {
        pplans.Add(pp);
      }

      foreach (var txSite in txSiteList)
      {
        if (txSite.Item1 == plannedTxSite)
        {
          resEdTxSiteId = txSite.Item1;
          resEdDBPath = txSite.Item2 + resId + "_" + date + "_" + time.Replace(':', '-') + ".json";
          resEdDBJsVarPath = resEdDBPath.Replace(".json", ".js");

          break;
        }
      }


      #endregion
      //---------------------------------------------------------------------------------

      #endregion
      //---------------------------------------------------------------------------------
      #region Logic for collecting json data

      var residentId = ESJO.CreateESJO("Resident", resId); JsonObjList.Add(residentId);
      var txSiteId = ESJO.CreateESJO("TxSite", resEdTxSiteId); JsonObjList.Add(txSiteId);
      var dateTime = ESJO.CreateESJO("DateTime", date); JsonObjList.Add(dateTime);

      // patient info 
      var pName_jo = ESJO.CreateESJO("PatientName", patient.Name); JsonObjList.Add(pName_jo);
      var pId_jo = ESJO.CreateESJO("PatientId", patient.Id); JsonObjList.Add(pId_jo);

      // course info
      var course_jo = ESJO.CreateESJO("CourseId", courseId); JsonObjList.Add(course_jo);
      var courseHeader_jo = ESJO.CreateESJO("CourseHeader", courseHeader); JsonObjList.Add(courseHeader_jo);

      // diagnoses info -- prob don't need for resident data
      var dxList = new List<Diagnosis>();
      try
      {
        foreach (var course in patient.Courses.ToList())
        {
          if (course.ToString() == courseId)
          {
            dxList = course.Diagnoses.ToList();
          }
        }

        var courseDx_jo = ESJO.CreateESJO("Diagnoses", dxList); JsonObjList.Add(courseDx_jo);
      }
      catch
      {
        Trace.WriteLine("There was a problem collecting the Diagnoses");
      }

      // plan data
      // will continue to collect all the same data to prevent rewriting the CreateESJO() method used for plan data

      ESJO plans_jo = ESJO.CreateESJO("PlanData", pplans); JsonObjList.Add(plans_jo);


      // if plan data collection fails and returns "incomplete"
      if (plans_jo.JsonString == "incomplete")
      {
        Trace.WriteLine("Sorry, something went wrong while collecting plan data...");
      }
      // else, save data
      else
      {
        if (saveToResident)
        {
          // paths 
          // NOTE: don't need js files since concatenating with python and saving to single js data file
          string jsonPath = resEdDBPath;
          //string jsVarPath = resEdDBJsVarPath;
          if (File.Exists(jsonPath))
          {
            //var jsVarString = "var resJson = ";

            var existingJsonString = File.ReadAllText(jsonPath).Replace("\r\n", string.Empty).Replace("\t", string.Empty);
            existingJsonString = existingJsonString.TrimEnd(']');
            existingJsonString += ",{";
            foreach (var jo in JsonObjList)
            {
              existingJsonString += jo.JsonString + ",";
            }
            existingJsonString = existingJsonString.TrimEnd(',');
            existingJsonString += "}]";

            //jsVarString += existingJsonString;

            File.WriteAllText(jsonPath, existingJsonString);
            //File.WriteAllText(jsVarPath, jsVarString);
          }
          else
          {
            var jsonString = string.Empty;
            //var jsVarString = "var resJson = ";

            jsonString += "[{";
            foreach (var jo in JsonObjList)
            {
              jsonString += jo.JsonString + ",";
            }
            jsonString = jsonString.TrimEnd(',');
            jsonString += "}]";

            //jsVarString += jsonString;

            File.WriteAllText(jsonPath, jsonString);
            //File.WriteAllText(jsVarPath, jsVarString);
          }
        }
        Trace.WriteLine("JSON data has been collected successfully for the generated plans...");

        //progressResult_TB.Text = "JSON data has been collected successfully for the selected plans...";
      }

      #endregion
      //---------------------------------------------------------------------------------
    }

    //---------------------------------------------------------------------------------

    //---------------------------------------------------------------------------------

    /// <summary>
    /// For a single plan, write structure specific csv files in both row and column format as well as save them to both patient and physician specific folders. Patient Id is not associated with the file, only a RandomId.
    /// </summary>
    /// <param name="currentPlan"></param>
    /// <param name="sorted_structureList"></param>
    /// <param name="folderPathForDvhCsv"></param>
    /// <param name="randomId"></param>
    /// <param name="primaryPhysician"></param>
    public static void writeDvhData_CurrentPlan(PlanSetup currentPlan, IEnumerable<Structure> sorted_structureList, string folderPathForDvhCsv, string randomId, string primaryPhysician)
    {
      currentPlan.DoseValuePresentation = DoseValuePresentation.Absolute;

      double planMaxDose = 0;
      if (currentPlan.Dose != null)
      {
        planMaxDose = Math.Round(currentPlan.Dose.DoseMax3D.Dose, 3);
      }
      else { planMaxDose = Double.NaN; }

      // patient specific paths
      string finalColsDvhFolderPath = folderPathForDvhCsv + "\\" + currentPlan.Id + "_DVH\\columns";
      string finalRowsDvhFolderPath = folderPathForDvhCsv + "\\" + currentPlan.Id + "_DVH\\rows";

      // physician specific paths organized by randomId_planId
      string physicianSpecificFolderPath_cols = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__PhysicianSpecificData__\\" + primaryPhysician + "\\" + currentPlan.Id + "_DVH\\colums";
      string physicianSpecificFolderPath_rows = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__PhysicianSpecificData__\\" + primaryPhysician + "\\" + currentPlan.Id + "_DVH\\rows";

      // structure specific paths -- rows only
      string structureSpecificFolderPath_rows_csv = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__StructureSpecificData__";
      string physicianAndStructureSpecificFolderPath_rows_csv = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__PhysicianSpecificData__\\" + primaryPhysician + "\\__StructureData__";

      // structure specific json path
      string structureSpecificFolderPath_json = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__StructureSpecificData__\\__json__";

      if (!Directory.Exists(finalColsDvhFolderPath))
      {
        Directory.CreateDirectory(finalColsDvhFolderPath);
      }
      if (!Directory.Exists(finalRowsDvhFolderPath))
      {
        Directory.CreateDirectory(finalRowsDvhFolderPath);
      }
      if (!Directory.Exists(physicianSpecificFolderPath_cols))
      {
        Directory.CreateDirectory(physicianSpecificFolderPath_cols);
      }
      if (!Directory.Exists(physicianSpecificFolderPath_rows))
      {
        Directory.CreateDirectory(physicianSpecificFolderPath_rows);
      }
      if (!Directory.Exists(structureSpecificFolderPath_rows_csv))
      {
        Directory.CreateDirectory(structureSpecificFolderPath_rows_csv);
      }
      if (!Directory.Exists(physicianAndStructureSpecificFolderPath_rows_csv))
      {
        Directory.CreateDirectory(physicianAndStructureSpecificFolderPath_rows_csv);
      }
      if (!Directory.Exists(structureSpecificFolderPath_json))
      {
        Directory.CreateDirectory(structureSpecificFolderPath_json);
      }
      StringBuilder colDvhCsvStringBuilder = new StringBuilder();
      StringBuilder rowDvhCsvStringBuilder = new StringBuilder();
      StringBuilder structureSpecificDvhCsvStringBuilder = new StringBuilder();
      StringBuilder pysicianAndStructureSpecificDvhCsvStringBuilder = new StringBuilder();

      foreach (var s in sorted_structureList)
      {
        // clear string builders
        colDvhCsvStringBuilder.Clear();
        rowDvhCsvStringBuilder.Clear();
        structureSpecificDvhCsvStringBuilder.Clear();
        pysicianAndStructureSpecificDvhCsvStringBuilder.Clear();

        // variables
        string lowerId = s.Id.ToString().ToLower().Replace(" ", string.Empty).Split(':').First();
        double volume = Math.Round(s.Volume, 3);
        string color = "#" + s.Color.ToString().Substring(3, 6);
        string structureSpecificJsonString = string.Empty;

        // define final paths
        string finalColDvhCsvPath = finalColsDvhFolderPath + "\\" + lowerId + "_cols.csv";
        string finalRowDvhCsvPath = finalRowsDvhFolderPath + "\\" + lowerId + "_rows.csv";
        string finalColDvhCsvPath_physSpec = physicianSpecificFolderPath_cols + "\\" + lowerId + "_cols.csv";
        string finalRowDvhCsvPath_physSpec = physicianSpecificFolderPath_rows + "\\" + lowerId + "_rows.csv";
        string finalColDvhCsvPath_structureSpec = structureSpecificFolderPath_rows_csv + "\\" + lowerId + "_rows.csv";
        string finalRowDvhCsvPath_physAndStructureSpec = physicianAndStructureSpecificFolderPath_rows_csv + "\\" + lowerId + "_rows.csv";

        // structure specific json
        string finalRowDvhJsonPath_structureSpec = structureSpecificFolderPath_json + "\\" + lowerId + ".json";

        if (!File.Exists(finalRowDvhJsonPath_structureSpec))
        {
          structureSpecificJsonString = "[";
        }
        else
        {
          structureSpecificJsonString = structureSpecificJsonString.TrimEnd(']');
          structureSpecificJsonString = ",";
        }

        // dvh data
        DVHData dvhAR = currentPlan.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
        DVHData dvhAA = currentPlan.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

        // define strings
        //dvhCsvStringBuilder.AppendLine("patientId: " + patientId);
        colDvhCsvStringBuilder.AppendLine("randomId,\t" + randomId);
        colDvhCsvStringBuilder.AppendLine("primaryPhysician,\t\t" + primaryPhysician);
        colDvhCsvStringBuilder.AppendLine("planId,\t" + currentPlan.Id);
        colDvhCsvStringBuilder.AppendLine("approvalStatus," + currentPlan.ApprovalStatus);
        colDvhCsvStringBuilder.AppendLine("planMaxDose,\t" + planMaxDose);
        colDvhCsvStringBuilder.AppendLine("structureId,\t" + lowerId);
        colDvhCsvStringBuilder.AppendLine("structureColor,\t" + color);
        colDvhCsvStringBuilder.AppendLine("structureVolume," + volume);

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

          // json string to write
          structureSpecificJsonString = structureSpecificJsonString + "{\"structureData\":[" +
                                                                      "{\"randomId\":\"" + currentPlan.Id + "\"," +
                                                                      "\"planId\":\"" + currentPlan.Id + "\"," +
                                                                      "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
                                                                      "\"planMaxDose\":" + planMaxDose + "," +
                                                                      "\"structureId\":\"" + s.Id + "\"," +
                                                                      "\"color\":\"" + color + "\"," +
                                                                      "\"structureVolume\":" + volume + "," +
                                                                      "\"min03\":" + min03 + "," +
                                                                      "\"minDose\":" + minDose + "," +
                                                                      "\"meanDose\":" + meanDose + "," +
                                                                      "\"max03\":" + max03 + "," +
                                                                      "\"maxDose\":" + maxDose + "," +
                                                                      "\"medianDose\":" + medianDose + "," +
                                                                      "\"std\":" + std + "," +
                                                                      "\"dvh\":[";

          colDvhCsvStringBuilder.AppendLine("min03,\t\t" + min03);
          colDvhCsvStringBuilder.AppendLine("minDose,\t" + minDose);
          colDvhCsvStringBuilder.AppendLine("max03,\t\t" + max03);
          colDvhCsvStringBuilder.AppendLine("maxDose,\t" + maxDose);
          colDvhCsvStringBuilder.AppendLine("meanDose,\t" + meanDose);
          colDvhCsvStringBuilder.AppendLine("medianDose,\t" + medianDose);
          colDvhCsvStringBuilder.AppendLine("std,\t\t" + std);
          colDvhCsvStringBuilder.AppendLine("dvh:");
          colDvhCsvStringBuilder.AppendLine("Dose(Gy),\tVolume(cc),\tVolume(pct)");

          for (double i = 0; i <= dvhAR.MaxDose.Dose + .1; i += .1)
          {
            string dose = string.Format("{0:N1}", i);
            string relVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString();
            string absVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAA, i), 4).ToString();
            // json string to write
            structureSpecificJsonString = structureSpecificJsonString + "[" + dose + "," + relVolAtDose + "],";
            // csv string to write
            colDvhCsvStringBuilder.AppendLine(string.Format("{0},\t\t{1},\t\t{2}", dose, absVolAtDose, relVolAtDose));
            // lists for csv string rows
            doseList.Add("V" + dose);
            relVolumeList.Add(relVolAtDose);
          }
          string doseListString = string.Join(",", doseList.ToArray());
          string relVolumeListString = string.Join(",", relVolumeList.ToArray());

          // append headers
          rowDvhCsvStringBuilder.AppendLine("RandomId,PrimaryPhysician,PlanId,ApprovalStatus,PlanMaxDose,StructureId,StructureVolume,Min03,MinDose,Max03,MaxDose,MeanDose,MedianDose,Std," + doseListString);
          if (!File.Exists(finalColDvhCsvPath_structureSpec))
          {
            structureSpecificDvhCsvStringBuilder.AppendLine("RandomId,PrimaryPhysician,PlanId,ApprovalStatus,PlanMaxDose,StructureId,StructureVolume,Min03,MinDose,Max03,MaxDose,MeanDose,MedianDose,Std," + doseListString);
          }
          if (!File.Exists(finalRowDvhCsvPath_physAndStructureSpec))
          {
            pysicianAndStructureSpecificDvhCsvStringBuilder.AppendLine("RandomId,PrimaryPhysician,PlanId,ApprovalStatus,PlanMaxDose,StructureId,StructureVolume,Min03,MinDose,Max03,MaxDose,MeanDose,MedianDose,Std," + doseListString);
          }

          // append data
          rowDvhCsvStringBuilder.AppendLine(randomId + "," + primaryPhysician + "," + currentPlan.Id + "," + currentPlan.ApprovalStatus + "," + planMaxDose + "," + lowerId + "," +
                                              volume + "," + min03 + "," + minDose + "," + max03 + "," + maxDose + "," + meanDose + "," + medianDose + "," + std + "," + relVolumeListString);
          structureSpecificDvhCsvStringBuilder.AppendLine(randomId + "," + primaryPhysician + "," + currentPlan.Id + "," + currentPlan.ApprovalStatus + "," + planMaxDose + "," + lowerId + "," +
                                                              volume + "," + min03 + "," + minDose + "," + max03 + "," + maxDose + "," + meanDose + "," + medianDose + "," + std + "," + relVolumeListString);
          pysicianAndStructureSpecificDvhCsvStringBuilder.AppendLine(randomId + "," + primaryPhysician + "," + currentPlan.Id + "," + currentPlan.ApprovalStatus + "," + planMaxDose + "," + lowerId + "," +
                                                              volume + "," + min03 + "," + minDose + "," + max03 + "," + maxDose + "," + meanDose + "," + medianDose + "," + std + "," + relVolumeListString);

          // json string to write
          structureSpecificJsonString = structureSpecificJsonString.TrimEnd(',');
          structureSpecificJsonString = structureSpecificJsonString + "]]}]";
        }
        // write files
        File.WriteAllText(finalColDvhCsvPath, colDvhCsvStringBuilder.ToString());
        File.WriteAllText(finalRowDvhCsvPath, rowDvhCsvStringBuilder.ToString());
        // ph
        File.AppendAllText(finalColDvhCsvPath_physSpec, colDvhCsvStringBuilder.ToString());
        File.AppendAllText(finalRowDvhCsvPath_physSpec, rowDvhCsvStringBuilder.ToString());
        // structure specific files
        File.AppendAllText(finalColDvhCsvPath_structureSpec, structureSpecificDvhCsvStringBuilder.ToString());
        File.AppendAllText(finalRowDvhCsvPath_physAndStructureSpec, pysicianAndStructureSpecificDvhCsvStringBuilder.ToString());
        // structure specific json file
        File.AppendAllText(finalRowDvhJsonPath_structureSpec, structureSpecificJsonString);
      }
    }

    /// <summary>
    /// For all plans in the current context, write structure specific csv files in both row and column format as well as save them to both patient and physician specific folders. Patient Id is not associated with the file, only a RandomId.
    /// </summary>
    /// <param name="plans"></param>
    /// <param name="folderPathForDvhCsv"></param>
    /// <param name="randomId"></param>
    /// <param name="primaryPhysician"></param>
    public static void getDvhCsvAllPlans(IEnumerator plans, string folderPathForDvhCsv, string randomId, string primaryPhysician)
    {
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

        // patient specific paths
        string finalColsDvhFolderPath = folderPathForDvhCsv + "\\" + currentPlan.Id + "_DVH\\columns";
        string finalRowsDvhFolderPath = folderPathForDvhCsv + "\\" + currentPlan.Id + "_DVH\\rows";

        // physician specific paths organized by randomId_planId
        string physicianSpecificFolderPath_cols = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__PhysicianSpecificData__\\" + primaryPhysician + "\\" + currentPlan.Id + "_DVH\\colums";
        string physicianSpecificFolderPath_rows = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__PhysicianSpecificData__\\" + primaryPhysician + "\\" + currentPlan.Id + "_DVH\\rows";

        // structure specific paths -- rows only
        string structureSpecificFolderPath_csv = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__StructureSpecificData__";
        string physicianAndStructureSpecificFolderPath_rows_csv = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__PhysicianSpecificData__\\" + primaryPhysician + "\\__StructureData__";

        // structure specific json path
        string structureSpecificFolderPath_json = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__StructureSpecificData__\\__json__";

        // create directories
        if (!Directory.Exists(finalColsDvhFolderPath))
        {
          Directory.CreateDirectory(finalColsDvhFolderPath);
        }
        if (!Directory.Exists(finalRowsDvhFolderPath))
        {
          Directory.CreateDirectory(finalRowsDvhFolderPath);
        }
        if (!Directory.Exists(physicianSpecificFolderPath_cols))
        {
          Directory.CreateDirectory(physicianSpecificFolderPath_cols);
        }
        if (!Directory.Exists(physicianSpecificFolderPath_rows))
        {
          Directory.CreateDirectory(physicianSpecificFolderPath_rows);
        }
        if (!Directory.Exists(structureSpecificFolderPath_csv))
        {
          Directory.CreateDirectory(structureSpecificFolderPath_csv);
        }
        if (!Directory.Exists(physicianAndStructureSpecificFolderPath_rows_csv))
        {
          Directory.CreateDirectory(physicianAndStructureSpecificFolderPath_rows_csv);
        }
        if (!Directory.Exists(structureSpecificFolderPath_json))
        {
          Directory.CreateDirectory(structureSpecificFolderPath_json);
        }
        StringBuilder colDvhCsvStringBuilder = new StringBuilder();
        StringBuilder rowDvhCsvStringBuilder = new StringBuilder();
        StringBuilder structureSpecificDvhCsvStringBuilder = new StringBuilder();
        StringBuilder pysicianAndStructureSpecificDvhCsvStringBuilder = new StringBuilder();

        foreach (var s in zsorted_structureList)
        {
          // clear string builders
          colDvhCsvStringBuilder.Clear();
          rowDvhCsvStringBuilder.Clear();
          structureSpecificDvhCsvStringBuilder.Clear();
          pysicianAndStructureSpecificDvhCsvStringBuilder.Clear();

          // variables
          string lowerId = s.Id.ToString().ToLower().Replace(" ", string.Empty).Split(':').First();
          double volume = Math.Round(s.Volume, 3);
          string color = "#" + s.Color.ToString().Substring(3, 6);
          string structureSpecificJsonString = string.Empty;

          // define final paths
          string finalColDvhCsvPath = finalColsDvhFolderPath + "\\" + lowerId + "_cols.csv";
          string finalRowDvhCsvPath = finalRowsDvhFolderPath + "\\" + lowerId + "_rows.csv";
          string finalColDvhCsvPath_physSpec = physicianSpecificFolderPath_cols + "\\" + lowerId + "_cols.csv";
          string finalRowDvhCsvPath_physSpec = physicianSpecificFolderPath_rows + "\\" + lowerId + "_rows.csv";
          string finalColDvhCsvPath_structureSpec = structureSpecificFolderPath_csv + "\\" + lowerId + "_rows.csv";
          string finalRowDvhCsvPath_physAndStructureSpec = physicianAndStructureSpecificFolderPath_rows_csv + "\\" + lowerId + "_rows.csv";

          // structure specific json
          string finalRowDvhJsonPath_structureSpec = structureSpecificFolderPath_json + "\\" + lowerId + ".json";

          if (!File.Exists(finalRowDvhJsonPath_structureSpec))
          {
            structureSpecificJsonString = "[";
          }
          else
          {
            structureSpecificJsonString = structureSpecificJsonString.TrimEnd(']');
            structureSpecificJsonString = ",";
          }

          // dvh data
          DVHData dvhAR = currentPlan.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
          DVHData dvhAA = currentPlan.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

          // define strings
          //dvhCsvStringBuilder.AppendLine("patientId: " + patientId);
          colDvhCsvStringBuilder.AppendLine("randomId,\t" + randomId);
          colDvhCsvStringBuilder.AppendLine("primaryPhysician,\t\t" + primaryPhysician);
          colDvhCsvStringBuilder.AppendLine("planId,\t" + currentPlan.Id);
          colDvhCsvStringBuilder.AppendLine("approvalStatus," + currentPlan.ApprovalStatus);
          colDvhCsvStringBuilder.AppendLine("planMaxDose,\t" + planMaxDose);
          colDvhCsvStringBuilder.AppendLine("structureId,\t" + lowerId);
          colDvhCsvStringBuilder.AppendLine("structureColor,\t" + color);
          colDvhCsvStringBuilder.AppendLine("structureVolume," + volume);

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

            // json string to write
            structureSpecificJsonString = structureSpecificJsonString + "{\"structureData\":[" +
                                                                        "{\"randomId\":\"" + currentPlan.Id + "\"," +
                                                                        "\"planId\":\"" + currentPlan.Id + "\"," +
                                                                        "\"approvalStatus\":\"" + currentPlan.ApprovalStatus + "\"," +
                                                                        "\"planMaxDose\":" + planMaxDose + "," +
                                                                        "\"structureId\":\"" + s.Id + "\"," +
                                                                        "\"color\":\"" + color + "\"," +
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
            colDvhCsvStringBuilder.AppendLine("min03,\t\t" + min03);
            colDvhCsvStringBuilder.AppendLine("minDose,\t" + minDose);
            colDvhCsvStringBuilder.AppendLine("max03,\t\t" + max03);
            colDvhCsvStringBuilder.AppendLine("maxDose,\t" + maxDose);
            colDvhCsvStringBuilder.AppendLine("meanDose,\t" + meanDose);
            colDvhCsvStringBuilder.AppendLine("medianDose:,\t" + medianDose);
            colDvhCsvStringBuilder.AppendLine("std,\t\t" + std);
            colDvhCsvStringBuilder.AppendLine("dvh:");
            colDvhCsvStringBuilder.AppendLine("Dose(Gy),\tVolume(cc),\tVolume(pct)");

            for (double i = 0; i <= dvhAR.MaxDose.Dose + .1; i += .1)
            {
              string dose = string.Format("{0:N1}", i);
              string relVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString();
              string absVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAA, i), 4).ToString();

              // json string to write
              structureSpecificJsonString = structureSpecificJsonString + "[" + dose + "," + relVolAtDose + "],";
              // csv string to write
              colDvhCsvStringBuilder.AppendLine(string.Format("{0},\t\t{1},\t\t{2}", dose, absVolAtDose, relVolAtDose));
              // lists for csv string rows
              doseList.Add("V" + dose);
              relVolumeList.Add(relVolAtDose);
            }
            string doseListString = string.Join(",", doseList.ToArray());
            string relVolumeListString = string.Join(",", relVolumeList.ToArray());

            // append headers
            rowDvhCsvStringBuilder.AppendLine("RandomId,PrimaryPhysician,PlanId,ApprovalStatus,PlanMaxDose,StructureId,StructureVolume,Min03,MinDose,Max03,MaxDose,MeanDose,MedianDose,Std," + doseListString);
            if (!File.Exists(finalColDvhCsvPath_structureSpec))
            {
              structureSpecificDvhCsvStringBuilder.AppendLine("RandomId,PrimaryPhysician,PlanId,ApprovalStatus,PlanMaxDose,StructureId,StructureVolume,Min03,MinDose,Max03,MaxDose,MeanDose,MedianDose,Std," + doseListString);
            }
            if (!File.Exists(finalRowDvhCsvPath_physAndStructureSpec))
            {
              pysicianAndStructureSpecificDvhCsvStringBuilder.AppendLine("RandomId,PrimaryPhysician,PlanId,ApprovalStatus,PlanMaxDose,StructureId,StructureVolume,Min03,MinDose,Max03,MaxDose,MeanDose,MedianDose,Std," + doseListString);
            }

            // append data
            rowDvhCsvStringBuilder.AppendLine(randomId + "," + primaryPhysician + "," + currentPlan.Id + "," + currentPlan.ApprovalStatus + "," + planMaxDose + "," + lowerId + "," +
                                                volume + "," + min03 + "," + minDose + "," + max03 + "," + maxDose + "," + meanDose + "," + medianDose + "," + std + "," + relVolumeListString);
            structureSpecificDvhCsvStringBuilder.AppendLine(randomId + "," + primaryPhysician + "," + currentPlan.Id + "," + currentPlan.ApprovalStatus + "," + planMaxDose + "," + lowerId + "," +
                                                                volume + "," + min03 + "," + minDose + "," + max03 + "," + maxDose + "," + meanDose + "," + medianDose + "," + std + "," + relVolumeListString);
            pysicianAndStructureSpecificDvhCsvStringBuilder.AppendLine(randomId + "," + primaryPhysician + "," + currentPlan.Id + "," + currentPlan.ApprovalStatus + "," + planMaxDose + "," + lowerId + "," +
                                                                volume + "," + min03 + "," + minDose + "," + max03 + "," + maxDose + "," + meanDose + "," + medianDose + "," + std + "," + relVolumeListString);

            // json string to write
            structureSpecificJsonString = structureSpecificJsonString.TrimEnd(',');
            structureSpecificJsonString = structureSpecificJsonString + "]]}]";
          }
          // write files
          File.WriteAllText(finalColDvhCsvPath, colDvhCsvStringBuilder.ToString());
          File.WriteAllText(finalRowDvhCsvPath, rowDvhCsvStringBuilder.ToString());
          // ph
          File.AppendAllText(finalColDvhCsvPath_physSpec, colDvhCsvStringBuilder.ToString());
          File.AppendAllText(finalRowDvhCsvPath_physSpec, rowDvhCsvStringBuilder.ToString());
          // structure specific csv files
          File.AppendAllText(finalColDvhCsvPath_structureSpec, structureSpecificDvhCsvStringBuilder.ToString());
          File.AppendAllText(finalRowDvhCsvPath_physAndStructureSpec, pysicianAndStructureSpecificDvhCsvStringBuilder.ToString());
          // structure specific json file
          File.AppendAllText(finalRowDvhJsonPath_structureSpec, structureSpecificJsonString);
        }
      }
    }

    /// <summary>
    /// UNFINISHED. For the current selected Plan Sum, write structure specific csv files in both row and column format as well as save them to both patient and physician specific folders. Patient Id is not associated with the file, only a RandomId.
    /// </summary>
    /// <param name="currentPlanSum"></param>
    /// <param name="sorted_structureList"></param>
    /// <param name="folderPathForDvhCsv"></param>
    /// <param name="randomId"></param>
    /// <param name="primaryPhysician"></param>
    public static void getDvhCsvCurrentPlanSum(PlanSum currentPlanSum, IEnumerable<Structure> sorted_structureList, string folderPathForDvhCsv, string randomId, string primaryPhysician)
    {
      double sumMaxDose = 0;
      if (currentPlanSum.Dose != null)
      {
        sumMaxDose = Math.Round(currentPlanSum.Dose.DoseMax3D.Dose, 3);
      }
      else { sumMaxDose = Double.NaN; }

      // patient specific paths
      string finalColsDvhFolderPath = folderPathForDvhCsv + "\\" + currentPlanSum.Id + "_DVH\\columns";
      string finalRowsDvhFolderPath = folderPathForDvhCsv + "\\" + currentPlanSum.Id + "_DVH\\rows";

      // physician specific paths organized by randomId_planId
      string physicianSpecificFolderPath_cols = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__PhysicianSpecificData__\\" + primaryPhysician + "\\" + currentPlanSum.Id + "_DVH\\colums";
      string physicianSpecificFolderPath_rows = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__PhysicianSpecificData__\\" + primaryPhysician + "\\" + currentPlanSum.Id + "_DVH\\rows";

      // structure specific paths -- rows only
      string structureSpecificFolderPath_rows = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__StructureSpecificData__";
      string physicianAndStructureSpecificFolderPath_rows = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__PhysicianSpecificData__\\" + primaryPhysician + "\\__StructureData__";

      if (!Directory.Exists(finalColsDvhFolderPath))
      {
        Directory.CreateDirectory(finalColsDvhFolderPath);
      }
      if (!Directory.Exists(finalRowsDvhFolderPath))
      {
        Directory.CreateDirectory(finalRowsDvhFolderPath);
      }
      if (!Directory.Exists(physicianSpecificFolderPath_cols))
      {
        Directory.CreateDirectory(physicianSpecificFolderPath_cols);
      }
      StringBuilder colDvhCsvStringBuilder = new StringBuilder();
      StringBuilder rowDvhCsvStringBuilder = new StringBuilder();
      StringBuilder specificStructureDvhCsvStringBuilder = new StringBuilder();
      StringBuilder specificPysicianAndStructureDvhCsvStringBuilder = new StringBuilder();

      foreach (var s in sorted_structureList)
      {
        // clear string builders
        colDvhCsvStringBuilder.Clear();
        rowDvhCsvStringBuilder.Clear();
        specificStructureDvhCsvStringBuilder.Clear();
        specificPysicianAndStructureDvhCsvStringBuilder.Clear();

        // variables
        string lowerId = s.Id.ToString().ToLower().Replace(" ", string.Empty).Split(':').First();
        double volume = Math.Round(s.Volume, 3);
        string color = "#" + s.Color.ToString().Substring(3, 6);
        string status = "PlanSum";

        // define final paths
        string finalColDvhCsvPath = finalColsDvhFolderPath + "\\" + lowerId + "_cols.csv";
        string finalRowDvhCsvPath = finalRowsDvhFolderPath + "\\" + lowerId + "_rows.csv";
        string finalColDvhCsvPath_physSpec = physicianSpecificFolderPath_cols + "\\" + lowerId + "_cols.csv";
        string finalRowDvhCsvPath_physSpec = physicianSpecificFolderPath_rows + "\\" + lowerId + "_rows.csv";
        string finalColDvhCsvPath_structureSpec = structureSpecificFolderPath_rows + "\\" + lowerId + "_rows.csv";
        string finalRowDvhCsvPath_physAndStructureSpec = physicianAndStructureSpecificFolderPath_rows + "\\" + lowerId + "_rows.csv";

        // dvh data
        DVHData dvhAR = currentPlanSum.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
        DVHData dvhAA = currentPlanSum.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

        // define strings
        //dvhCsvStringBuilder.AppendLine("patientId: " + patientId);
        colDvhCsvStringBuilder.AppendLine("RandomId,\t" + randomId);
        colDvhCsvStringBuilder.AppendLine("PrimaryPhysician,\t\t" + primaryPhysician);
        colDvhCsvStringBuilder.AppendLine("PlanId,\t" + currentPlanSum.Id);
        colDvhCsvStringBuilder.AppendLine("ApprovalStatus," + status);
        colDvhCsvStringBuilder.AppendLine("PlanMaxDose,\t" + sumMaxDose);
        colDvhCsvStringBuilder.AppendLine("StructureId,\t" + lowerId);
        colDvhCsvStringBuilder.AppendLine("StructureColor,\t" + color);
        colDvhCsvStringBuilder.AppendLine("StructureVolume," + volume);

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

          colDvhCsvStringBuilder.AppendLine("Min03,\t\t" + min03);
          colDvhCsvStringBuilder.AppendLine("MinDose,\t" + minDose);
          colDvhCsvStringBuilder.AppendLine("Max03,\t\t" + max03);
          colDvhCsvStringBuilder.AppendLine("MaxDose,\t" + maxDose);
          colDvhCsvStringBuilder.AppendLine("MeanDose,\t" + meanDose);
          colDvhCsvStringBuilder.AppendLine("MedianDose:,\t" + medianDose);
          colDvhCsvStringBuilder.AppendLine("Std,\t\t" + std);
          colDvhCsvStringBuilder.AppendLine("DVH:");
          colDvhCsvStringBuilder.AppendLine("Dose(Gy),\tVolume(cc),\tVolume(pct)");

          for (double i = 0; i <= dvhAR.MaxDose.Dose + .1; i += .1)
          {
            string dose = string.Format("{0:N1}", i);
            string relVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString();
            string absVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAA, i), 4).ToString();
            colDvhCsvStringBuilder.AppendLine(string.Format("{0},\t\t{1},\t\t{2}", dose, absVolAtDose, relVolAtDose));
            doseList.Add("V" + dose);
            relVolumeList.Add(relVolAtDose);
          }
          string doseListString = string.Join(",", doseList.ToArray());
          string relVolumeListString = string.Join(",", relVolumeList.ToArray());

          // append headers
          rowDvhCsvStringBuilder.AppendLine("RandomId,PrimaryPhysician,PlanId,ApprovalStatus,PlanMaxDose,StructureId,StructureVolume,Min03,MinDose,Max03,MaxDose,MeanDose,MedianDose,Std," + doseListString);
          if (!File.Exists(finalColDvhCsvPath_structureSpec))
          {
            specificStructureDvhCsvStringBuilder.AppendLine("RandomId,PrimaryPhysician,PlanId,ApprovalStatus,PlanMaxDose,StructureId,StructureVolume,Min03,MinDose,Max03,MaxDose,MeanDose,MedianDose,Std," + doseListString);
          }
          if (!File.Exists(finalRowDvhCsvPath_physAndStructureSpec))
          {
            specificPysicianAndStructureDvhCsvStringBuilder.AppendLine("RandomId,PrimaryPhysician,PlanId,ApprovalStatus,PlanMaxDose,StructureId,StructureVolume,Min03,MinDose,Max03,MaxDose,MeanDose,MedianDose,Std," + doseListString);
          }

          // append data
          rowDvhCsvStringBuilder.AppendLine(randomId + "," + primaryPhysician + "," + currentPlanSum.Id + "," + status + "," + sumMaxDose + "," + lowerId + "," +
                                              volume + "," + min03 + "," + minDose + "," + max03 + "," + maxDose + "," + meanDose + "," + medianDose + "," + std + "," + relVolumeListString);
          specificStructureDvhCsvStringBuilder.AppendLine(randomId + "," + primaryPhysician + "," + currentPlanSum.Id + "," + status + "," + sumMaxDose + "," + lowerId + "," +
                                                              volume + "," + min03 + "," + minDose + "," + max03 + "," + maxDose + "," + meanDose + "," + medianDose + "," + std + "," + relVolumeListString);
          specificPysicianAndStructureDvhCsvStringBuilder.AppendLine(randomId + "," + primaryPhysician + "," + currentPlanSum.Id + "," + status + "," + sumMaxDose + "," + lowerId + "," +
                                                              volume + "," + min03 + "," + minDose + "," + max03 + "," + maxDose + "," + meanDose + "," + medianDose + "," + std + "," + relVolumeListString);
        }
        // write files
        File.WriteAllText(finalColDvhCsvPath, colDvhCsvStringBuilder.ToString());
        File.WriteAllText(finalRowDvhCsvPath, rowDvhCsvStringBuilder.ToString());
        // ph
        File.AppendAllText(finalColDvhCsvPath_physSpec, colDvhCsvStringBuilder.ToString());
        File.AppendAllText(finalRowDvhCsvPath_physSpec, rowDvhCsvStringBuilder.ToString());
        // structure specific files
        File.AppendAllText(finalColDvhCsvPath_structureSpec, specificStructureDvhCsvStringBuilder.ToString());
        File.AppendAllText(finalRowDvhCsvPath_physAndStructureSpec, specificPysicianAndStructureDvhCsvStringBuilder.ToString());
      }
    }

    /// <summary>
    /// UNFINISHED. For all PlanSums in the current context, write structure specific csv files in both row and column format as well as save them to both patient and physician specific folders. Patient Id is not associated with the file, only a RandomId.
    /// </summary>
    /// <param name="sums"></param>
    /// <param name="folderPathForDvhCsv"></param>
    /// <param name="randomId"></param>
    /// <param name="primaryPhysician"></param>
    public static void getDvhCsvAllPlanSums(IEnumerator sums, string folderPathForDvhCsv, string randomId, string primaryPhysician)
    {
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

        // patient specific paths
        string finalColsDvhFolderPath = folderPathForDvhCsv + "\\" + currentPlanSum.Id + "_DVH\\columns";
        string finalRowsDvhFolderPath = folderPathForDvhCsv + "\\" + currentPlanSum.Id + "_DVH\\rows";

        // physician specific paths organized by randomId_planId
        string physicianSpecificFolderPath_cols = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__PhysicianSpecificData__\\" + primaryPhysician + "\\" + currentPlanSum.Id + "_DVH\\colums";
        string physicianSpecificFolderPath_rows = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__PhysicianSpecificData__\\" + primaryPhysician + "\\" + currentPlanSum.Id + "_DVH\\rows";

        // structure specific paths -- rows only
        string structureSpecificFolderPath_rows = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__StructureSpecificData__";
        string physicianAndStructureSpecificFolderPath_rows = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__PhysicianSpecificData__\\" + primaryPhysician + "\\__StructureData__";

        if (!Directory.Exists(finalColsDvhFolderPath))
        {
          Directory.CreateDirectory(finalColsDvhFolderPath);
        }
        if (!Directory.Exists(finalRowsDvhFolderPath))
        {
          Directory.CreateDirectory(finalRowsDvhFolderPath);
        }
        if (!Directory.Exists(physicianSpecificFolderPath_cols))
        {
          Directory.CreateDirectory(physicianSpecificFolderPath_cols);
        }
        StringBuilder colDvhCsvStringBuilder = new StringBuilder();
        StringBuilder rowDvhCsvStringBuilder = new StringBuilder();
        StringBuilder specificStructureDvhCsvStringBuilder = new StringBuilder();
        StringBuilder specificPysicianAndStructureDvhCsvStringBuilder = new StringBuilder();

        foreach (var s in zsorted_structureList)
        {
          // clear string builders
          colDvhCsvStringBuilder.Clear();
          rowDvhCsvStringBuilder.Clear();
          specificStructureDvhCsvStringBuilder.Clear();
          specificPysicianAndStructureDvhCsvStringBuilder.Clear();

          // variables
          string lowerId = s.Id.ToString().ToLower().Replace(" ", string.Empty).Split(':').First();
          double volume = Math.Round(s.Volume, 3);
          string color = "#" + s.Color.ToString().Substring(3, 6);
          string status = "PlanSum";

          // define final paths
          string finalColDvhCsvPath = finalColsDvhFolderPath + "\\" + lowerId + "_cols.csv";
          string finalRowDvhCsvPath = finalRowsDvhFolderPath + "\\" + lowerId + "_rows.csv";
          string finalColDvhCsvPath_physSpec = physicianSpecificFolderPath_cols + "\\" + lowerId + "_cols.csv";
          string finalRowDvhCsvPath_physSpec = physicianSpecificFolderPath_rows + "\\" + lowerId + "_rows.csv";
          string finalColDvhCsvPath_structureSpec = structureSpecificFolderPath_rows + "\\" + lowerId + "_rows.csv";
          string finalRowDvhCsvPath_physAndStructureSpec = physicianAndStructureSpecificFolderPath_rows + "\\" + lowerId + "_rows.csv";

          // dvh data
          DVHData dvhAR = currentPlanSum.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
          DVHData dvhAA = currentPlanSum.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

          // define strings
          //dvhCsvStringBuilder.AppendLine("patientId: " + patientId);
          colDvhCsvStringBuilder.AppendLine("RandomId,\t" + randomId);
          colDvhCsvStringBuilder.AppendLine("PrimaryPhysician,\t\t" + primaryPhysician);
          colDvhCsvStringBuilder.AppendLine("PlanId,\t" + currentPlanSum.Id);
          colDvhCsvStringBuilder.AppendLine("ApprovalStatus," + status);
          colDvhCsvStringBuilder.AppendLine("PlanMaxDose,\t" + sumMaxDose);
          colDvhCsvStringBuilder.AppendLine("StructureId,\t" + lowerId);
          colDvhCsvStringBuilder.AppendLine("StructureColor,\t" + color);
          colDvhCsvStringBuilder.AppendLine("StructureVolume," + volume);

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

            colDvhCsvStringBuilder.AppendLine("Min03,\t\t" + min03);
            colDvhCsvStringBuilder.AppendLine("MinDose,\t" + minDose);
            colDvhCsvStringBuilder.AppendLine("Max03,\t\t" + max03);
            colDvhCsvStringBuilder.AppendLine("MaxDose,\t" + maxDose);
            colDvhCsvStringBuilder.AppendLine("MeanDose,\t" + meanDose);
            colDvhCsvStringBuilder.AppendLine("MedianDose:,\t" + medianDose);
            colDvhCsvStringBuilder.AppendLine("Std,\t\t" + std);
            colDvhCsvStringBuilder.AppendLine("DVH:");
            colDvhCsvStringBuilder.AppendLine("Dose(Gy),\tVolume(cc),\tVolume(pct)");

            for (double i = 0; i <= dvhAR.MaxDose.Dose + .1; i += .1)
            {
              string dose = string.Format("{0:N1}", i);
              string relVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAR, i), 2).ToString();
              string absVolAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAA, i), 4).ToString();
              colDvhCsvStringBuilder.AppendLine(string.Format("{0},\t\t{1},\t\t{2}", dose, absVolAtDose, relVolAtDose));
              doseList.Add("V" + dose);
              relVolumeList.Add(relVolAtDose);
            }
            string doseListString = string.Join(",", doseList.ToArray());
            string relVolumeListString = string.Join(",", relVolumeList.ToArray());

            // append headers
            rowDvhCsvStringBuilder.AppendLine("RandomId,PrimaryPhysician,PlanId,ApprovalStatus,PlanMaxDose,StructureId,StructureVolume,Min03,MinDose,Max03,MaxDose,MeanDose,MedianDose,Std," + doseListString);
            if (!File.Exists(finalColDvhCsvPath_structureSpec))
            {
              specificStructureDvhCsvStringBuilder.AppendLine("RandomId,PrimaryPhysician,PlanId,ApprovalStatus,PlanMaxDose,StructureId,StructureVolume,Min03,MinDose,Max03,MaxDose,MeanDose,MedianDose,Std," + doseListString);
            }
            if (!File.Exists(finalRowDvhCsvPath_physAndStructureSpec))
            {
              specificPysicianAndStructureDvhCsvStringBuilder.AppendLine("RandomId,PrimaryPhysician,PlanId,ApprovalStatus,PlanMaxDose,StructureId,StructureVolume,Min03,MinDose,Max03,MaxDose,MeanDose,MedianDose,Std," + doseListString);
            }

            // append data
            rowDvhCsvStringBuilder.AppendLine(randomId + "," + primaryPhysician + "," + currentPlanSum.Id + "," + status + "," + sumMaxDose + "," + lowerId + "," +
                                                volume + "," + min03 + "," + minDose + "," + max03 + "," + maxDose + "," + meanDose + "," + medianDose + "," + std + "," + relVolumeListString);
            specificStructureDvhCsvStringBuilder.AppendLine(randomId + "," + primaryPhysician + "," + currentPlanSum.Id + "," + status + "," + sumMaxDose + "," + lowerId + "," +
                                                                volume + "," + min03 + "," + minDose + "," + max03 + "," + maxDose + "," + meanDose + "," + medianDose + "," + std + "," + relVolumeListString);
            specificPysicianAndStructureDvhCsvStringBuilder.AppendLine(randomId + "," + primaryPhysician + "," + currentPlanSum.Id + "," + status + "," + sumMaxDose + "," + lowerId + "," +
                                                                volume + "," + min03 + "," + minDose + "," + max03 + "," + maxDose + "," + meanDose + "," + medianDose + "," + std + "," + relVolumeListString);
          }
          // write files
          File.WriteAllText(finalColDvhCsvPath, colDvhCsvStringBuilder.ToString());
          File.WriteAllText(finalRowDvhCsvPath, rowDvhCsvStringBuilder.ToString());
          // ph
          File.AppendAllText(finalColDvhCsvPath_physSpec, colDvhCsvStringBuilder.ToString());
          File.AppendAllText(finalRowDvhCsvPath_physSpec, rowDvhCsvStringBuilder.ToString());
          // structure specific files
          File.AppendAllText(finalColDvhCsvPath_structureSpec, specificStructureDvhCsvStringBuilder.ToString());
          File.AppendAllText(finalRowDvhCsvPath_physAndStructureSpec, specificPysicianAndStructureDvhCsvStringBuilder.ToString());
        }
      }
    }
  }


}
