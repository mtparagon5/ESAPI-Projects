using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace VMS.TPS {
  public class Script {
    public Script () { }

    public void Execute (ScriptContext context /*, System.Windows.Window window*/ ) {

      #region context variable definitions

      // to work for plan sum
      StructureSet structureSet;
      PlanningItem selectedPlanningItem;
      PlanSetup planSetup;
      PlanSum psum = null;
      //double fractions = 0;
      //string status = "";
      if (context.PlanSetup == null && context.PlanSumsInScope.Count () > 1) {
        throw new ApplicationException ("Please close other plan sums");
      }
      if (context.PlanSetup == null) {
        psum = context.PlanSumsInScope.First ();
        planSetup = psum.PlanSetups.First ();
        selectedPlanningItem = (PlanningItem) psum;
        structureSet = psum.StructureSet;
        //fractions = DvhExtensions.getTotalFractionsForPlanSum(psum);
        //status = "PlanSum";
        MessageBox.Show ("This will not work with a Plan Sum");
        return;
      } else {
        planSetup = context.PlanSetup;
        selectedPlanningItem = (PlanningItem) planSetup;
        structureSet = planSetup.StructureSet;
        //if (planSetup.UniqueFractionation.NumberOfFractions != null)
        //{
        //	fractions = (double)planSetup.UniqueFractionation.NumberOfFractions;
        //}
        //status = planSetup.ApprovalStatus.ToString();

      }
      //var dosePerFx = planSetup.UniqueFractionation.PrescribedDosePerFraction.Dose;
      //var rxDose = (double)(dosePerFx * fractions);

      //structureSet = planSetup != null  planSetup.StructureSet : psum.StructureSet;/*psum.PlanSetups.Last().StructureSet;*/ // changed from first to last in case it's broken on next build
      //string pId = context.Patient.Id;
      //string course = context.Course.Id.ToString().Replace(" ", "_"); ;
      //string pName = ProcessIdName.processPtName(context.Patient.Name);

      #endregion

      #region organize structures into ordered lists
      // lists for structures
      List<Structure> gtvList = new List<Structure> ();
      List<Structure> ctvList = new List<Structure> ();
      List<Structure> itvList = new List<Structure> ();
      List<Structure> ptvList = new List<Structure> ();
      List<Structure> oarList = new List<Structure> ();
      List<Structure> targetList = new List<Structure> ();
      List<Structure> structureList = new List<Structure> ();
      List<Structure> ciStructureList = new List<Structure> ();

      IEnumerable<Structure> sorted_gtvList = new List<Structure> ();
      IEnumerable<Structure> sorted_ctvList = new List<Structure> ();
      IEnumerable<Structure> sorted_itvList = new List<Structure> ();
      IEnumerable<Structure> sorted_ptvList = new List<Structure> ();
      IEnumerable<Structure> sorted_oarList = new List<Structure> ();
      IEnumerable<Structure> sorted_targetList = new List<Structure> ();
      IEnumerable<Structure> sorted_structureList = new List<Structure> ();
      IEnumerable<Structure> sorted_ciStructureList = new List<Structure> ();

      foreach (var structure in structureSet.Structures) {
        // conditions for adding any structure
        if ((!structure.IsEmpty) &&
          (structure.HasSegment) &&
          (!structure.Id.Contains ("*")) &&
          (!structure.Id.ToLower ().Contains ("markers")) &&
          (!structure.Id.ToLower ().Contains ("avoid")) &&
          (!structure.Id.ToLower ().Contains ("dose")) &&
          (!structure.Id.ToLower ().Contains ("contrast")) &&
          (!structure.Id.ToLower ().Contains ("couch")) &&
          (!structure.Id.ToLower ().Contains ("air")) &&
          (!structure.Id.ToLower ().Contains ("dens")) &&
          (!structure.Id.ToLower ().Contains ("bolus")) &&
          (!structure.Id.ToLower ().Contains ("suv")) &&
          (!structure.Id.ToLower ().Contains ("match")) &&
          (!structure.Id.ToLower ().Contains ("wire")) &&
          (!structure.Id.ToLower ().Contains ("scar")) &&
          (!structure.Id.ToLower ().Contains ("chemo")) &&
          (!structure.Id.ToLower ().Contains ("pet")) &&
          (!structure.Id.ToLower ().Contains ("dnu")) &&
          (!structure.Id.ToLower ().Contains ("fiducial")) &&
          (!structure.Id.ToLower ().Contains ("artifact")) &&
          (!structure.Id.StartsWith ("z", StringComparison.InvariantCultureIgnoreCase)) &&
          (!structure.Id.StartsWith ("hs", StringComparison.InvariantCultureIgnoreCase)) &&
          (!structure.Id.StartsWith ("av", StringComparison.InvariantCultureIgnoreCase)) &&
          (!structure.Id.StartsWith ("opti ", StringComparison.InvariantCultureIgnoreCase)) &&
          (!structure.Id.StartsWith ("opti-", StringComparison.InvariantCultureIgnoreCase)))
        //(structure.Id.Contains("CI-", StringComparison.InvariantCultureIgnoreCase) == false) && 
        //(structure.Id.Contains("R50-", StringComparison.InvariantCultureIgnoreCase) == false) &&
        //(structure.Id.Contains("CI_", StringComparison.InvariantCultureIgnoreCase) == false) && 
        //(structure.Id.Contains("R50_", StringComparison.InvariantCultureIgnoreCase) == false))
        {
          //if (structure.DicomType.ToLower() == "external") { body = structure; }

          if (structure.Id.StartsWith ("GTV", StringComparison.InvariantCultureIgnoreCase)) {
            gtvList.Add (structure);
            structureList.Add (structure);
            targetList.Add (structure);
          }
          if ((structure.Id.StartsWith ("CTV", StringComparison.InvariantCultureIgnoreCase)) ||
            (structure.Id.StartsWith ("Prost", StringComparison.InvariantCultureIgnoreCase))) {
            ctvList.Add (structure);
            structureList.Add (structure);
            targetList.Add (structure);
          }
          if (structure.Id.StartsWith ("ITV", StringComparison.InvariantCultureIgnoreCase)) {
            itvList.Add (structure);
            structureList.Add (structure);
            targetList.Add (structure);
          }
          if (structure.Id.StartsWith ("PTV", StringComparison.InvariantCultureIgnoreCase)) {
            ptvList.Add (structure);
            structureList.Add (structure);
            targetList.Add (structure);
          }
          if (structure.Id.StartsWith ("CI", StringComparison.InvariantCultureIgnoreCase)) {
            ciStructureList.Add (structure);
          }
          // conditions for adding breast plan targets
          if ((structure.Id.StartsWith ("Level I", StringComparison.InvariantCultureIgnoreCase)) ||
            (structure.Id.StartsWith ("IM LN", StringComparison.InvariantCultureIgnoreCase)) ||
            (structure.Id.StartsWith ("Cavity", StringComparison.InvariantCultureIgnoreCase)) ||
            (structure.Id.StartsWith ("Supraclav", StringComparison.InvariantCultureIgnoreCase))) {
            targetList.Add (structure);
            structureList.Add (structure);
          }
          // conditions for adding oars
          if ((!structure.Id.StartsWith ("GTV", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith ("CTV", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith ("ITV", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith ("PTV", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith ("Level I", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith ("IM LN", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith ("Cavity", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith ("Supraclav", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith ("Scar", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.ToLower ().Contains ("carina"))) {
            oarList.Add (structure);
            structureList.Add (structure);
          }
        }
      }
      sorted_gtvList = gtvList.OrderBy (x => x.Id);
      sorted_ctvList = ctvList.OrderBy (x => x.Id);
      sorted_itvList = itvList.OrderBy (x => x.Id);
      sorted_ptvList = ptvList.OrderBy (x => x.Id);
      sorted_targetList = targetList.OrderBy (x => x.Id);
      sorted_oarList = oarList.OrderBy (x => x.Id);
      sorted_structureList = structureList.OrderBy (x => x.Id);
      sorted_ciStructureList = ciStructureList.OrderBy (x => x.Id);

      #endregion structure organization and ordering

      //double bsMax = 0;
      //double onLMax = 0;
      //double onRMax = 0;
      //double globeLMax = 0;
      //double globeRMax = 0;
      //double chiasmMax = 0;
      //double bodyVRx = 0;

      double rxDose = context.PlanSetup.TotalDose.Dose;
      double bodyV50Pct = 0;
      double bodyV100Pct = 0;
      double ci = 0;
      double r50 = 0;
      double r50VarAccLowerLimit = 0;
      double r50VarAccUpperLimit = 0;
      double limit3 = 0;
      double limit4 = 0;
      var spacer = new string (' ', 5);
      var oarMessage = "\tRatio\tIdeal\tVarAcc\r\n\r\n";
      //var ciMessage = "";

      foreach (var s in sorted_oarList) {
        DVHData dvhAA = selectedPlanningItem.GetDVHCumulativeData (s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

        if (s.DicomType == "EXTERNAL") { bodyV50Pct = getVolumeAtDose (dvhAA, rxDose / 2); }
        if (s.DicomType == "EXTERNAL") { bodyV100Pct = getVolumeAtDose (dvhAA, rxDose); }
      }

      foreach (var t in sorted_ptvList) {
        R50Constraint.LimitsFromVolume (t.Volume, out r50VarAccLowerLimit, out r50VarAccUpperLimit, out limit3, out limit4);
        ci = Math.Round ((bodyV100Pct / t.Volume), 2);
        r50 = Math.Round ((bodyV50Pct / t.Volume), 2);

        oarMessage += string.Format ("{0}: {1} cc\r\n{2}CI:\t{3}\t< 1.5\t< 2.0\r\n{4}R50:\t{5}\t< {6}\t< {7}\r\n\r\n", t.Id.ToString (), Math.Round (t.Volume, 3), spacer, ci, spacer, r50,
          Math.Round (r50VarAccLowerLimit, 2), Math.Round (r50VarAccUpperLimit, 2));
      }
      if (sorted_ptvList.Count () > 1) {
        oarMessage += string.Format ("\r\n\r\n\r\nNOTE: There are multiple PTVs -- Verify accuracy\r\nBody:\r\n{0}V{1} = {2} cc\r\n{3}V{4} = {5} cc", spacer, rxDose, Math.Round (bodyV100Pct, 3), spacer, (rxDose / 2), Math.Round (bodyV50Pct, 3));
      }

      MessageBox.Show (oarMessage, "Ratio Calc: Single Lesion Plans");
      //var listOfLists = new List<List<Tuple<Structure, double>>>();

      //if (rxDose == 21)
      //{
      //	List<Tuple<Structure, double>> targetCiList12 = new List<Tuple<Structure, double>>();
      //	List<Tuple<Structure, double>> targetCiList14 = new List<Tuple<Structure, double>>();
      //	List<Tuple<Structure, double>> targetCiList15 = new List<Tuple<Structure, double>>();
      //	List<Tuple<Structure, double>> targetCiList21 = new List<Tuple<Structure, double>>();
      //	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 12, out targetCiList12);
      //	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 14, out targetCiList14);
      //	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 15, out targetCiList15);
      //	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 21, out targetCiList14);
      //}
      //else if (rxDose == 25)
      //{
      //	List<Tuple<Structure, double>> targetCiList25 = new List<Tuple<Structure, double>>();
      //	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 25, out targetCiList25);
      //}
      //else if (rxDose == 30)
      //{
      //	List<Tuple<Structure, double>> targetCiList25 = new List<Tuple<Structure, double>>();
      //	List<Tuple<Structure, double>> targetCiList30 = new List<Tuple<Structure, double>>();
      //	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 25, out targetCiList25);
      //	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 30, out targetCiList30);
      //}

    }

    public static double getVolumeAtDose (DVHData dvh, double DoseLim) {
      for (int i = 0; i < dvh.CurveData.Length; i++) {
        if (dvh.CurveData[i].DoseValue.Dose >= DoseLim) {
          return dvh.CurveData[i].Volume;
        }
      }
      return 0;
    }

    public static void calcCi (PlanningItem selectedPlanningItem, IEnumerable<Structure> sorted_ptvList, IEnumerable<Structure> sorted_ciStructureList, double rxDose, out List<Tuple<Structure, double>> targetCiList) {
      targetCiList = new List<Tuple<Structure, double>> ();
      foreach (var t in sorted_ptvList) {
        foreach (var ciStructure in sorted_ciStructureList) {
          var ciNum = ciStructure.ToString ().Substring (ciStructure.ToString ().ToLower ().Replace ("_", string.Empty).Replace ("-", string.Empty).IndexOf ('i'));
          var tNum = t.ToString ().Substring (t.ToString ().ToLower ().Replace ("_", string.Empty).Replace ("-", string.Empty).IndexOf ("v"), 2);
          var tNumIsAllDigits = tNum.All (char.IsDigit);
          if (tNumIsAllDigits) {
            continue;
          } else {
            tNum = tNum.Substring (0, 1);
          }

          if (ciNum == tNum) {
            var tVol = t.Volume;
            DVHData s_dvhAA = selectedPlanningItem.GetDVHCumulativeData (ciStructure, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

            var ci = Math.Round ((getVolumeAtDose (s_dvhAA, rxDose)), 2);
            if ((ci >= 3.0) || (ci < 0.5)) continue;
            else { targetCiList.Add (Tuple.Create (t, ci)); }

            //targetCiList.Add(Tuple.Create(t.Id.ToString().Remove(t.Id.ToString().IndexOf(':')), ci));

          }
        }
      }
    }

    public class R50Constraint {
      public static void LimitsFromVolume (double volume, out double limit1, out double limit2, out double limit3, out double limit4) {
        // larger tah last in the table
        limit1 = 5.9;
        limit2 = 7.5;
        limit3 = 50;
        limit4 = 57;

        if ((volume >= 1.8) && (volume < 3.8)) {
          limit1 = 5.9 + (volume - 1.8) * (5.5 - 5.9) / (3.8 - 1.8);
          limit2 = 7.5 + (volume - 1.8) * (6.5 - 7.5) / (3.8 - 1.8);
          limit3 = 50 + (volume - 1.8) * (50 - 50) / (3.8 - 1.8);
          limit4 = 57 + (volume - 1.8) * (57 - 57) / (3.8 - 1.8);
        }

        if ((volume >= 3.8) && (volume < 7.4)) {
          limit1 = 5.5 + (volume - 3.8) * (5.1 - 5.5) / (7.4 - 3.8);
          limit2 = 6.5 + (volume - 3.8) * (6.0 - 6.5) / (7.4 - 3.8);
          limit3 = 50 + (volume - 3.8) * (50 - 50) / (7.4 - 3.8);
          limit4 = 57 + (volume - 3.8) * (58 - 57) / (7.4 - 3.8);
        }

        if ((volume >= 7.4) && (volume < 13.2)) {
          limit1 = 5.1 + (volume - 7.4) * (4.7 - 5.1) / (13.2 - 7.4);
          limit2 = 6.0 + (volume - 7.4) * (5.8 - 6.0) / (13.2 - 7.4);
          limit3 = 50 + (volume - 7.4) * (54 - 50) / (13.2 - 7.4);
          limit4 = 58 + (volume - 7.4) * (58 - 58) / (13.2 - 7.4);;
        }

        if ((volume > 13.2) && (volume < 22.0)) {
          limit1 = 4.7 + (volume - 13.2) * (4.5 - 4.7) / (22.0 - 13.2);
          limit2 = 5.8 + (volume - 13.2) * (5.5 - 5.8) / (22.0 - 13.2);
          limit3 = 50 + (volume - 13.2) * (54 - 50) / (22.0 - 13.2);
          limit4 = 58 + (volume - 13.2) * (63 - 58) / (22.0 - 13.2);
        }

        if ((volume > 22.0) && (volume < 34.0)) {
          limit1 = 4.5 + (volume - 22.0) * (4.3 - 4.5) / (34.0 - 22.0);
          limit2 = 5.5 + (volume - 22.0) * (5.3 - 5.5) / (34.0 - 22.0);
          limit3 = 54 + (volume - 22.0) * (58 - 54) / (34.0 - 22.0);
          limit4 = 63 + (volume - 22.0) * (68 - 63) / (34.0 - 22.0);
        }

        if ((volume > 34.0) && (volume < 50.0)) {
          limit1 = 4.3 + (volume - 34.0) * (4.0 - 4.3) / (50.0 - 34.0);
          limit2 = 5.3 + (volume - 34.0) * (5.0 - 5.3) / (50.0 - 34.0);
          limit3 = 58 + (volume - 34.0) * (62 - 58) / (50.0 - 34.0);
          limit4 = 68 + (volume - 34.0) * (77 - 68) / (50.0 - 34.0);
        }

        if ((volume > 50.0) && (volume < 70.0)) {
          limit1 = 4.0 + (volume - 50.0) * (3.5 - 4.0) / (70.0 - 50.0);
          limit2 = 5.0 + (volume - 50.0) * (4.8 - 5.0) / (70.0 - 50.0);
          limit3 = 62 + (volume - 50.0) * (66 - 62) / (70.0 - 50.0);
          limit4 = 77 + (volume - 50.0) * (86 - 77) / (70.0 - 50.0);
        }

        if ((volume > 70.0) && (volume < 95.0)) {
          limit1 = 3.5 + (volume - 70.0) * (3.3 - 3.5) / (95.0 - 70.0);
          limit2 = 4.8 + (volume - 70.0) * (4.4 - 4.8) / (95.0 - 70.0);
          limit3 = 66 + (volume - 70.0) * (70 - 66) / (95.0 - 70.0);
          limit4 = 86 + (volume - 70.0) * (89 - 86) / (95.0 - 70.0);
        }

        if ((volume > 95.0) && (volume < 126.0)) {
          limit1 = 3.3 + (volume - 95.0) * (3.1 - 3.3) / (126.0 - 95.0);
          limit2 = 4.4 + (volume - 95.0) * (4.0 - 4.4) / (126.0 - 95.0);
          limit3 = 70 + (volume - 95.0) * (73 - 70) / (126.0 - 95.0);
          limit4 = 89 + (volume - 95.0) * (91 - 89) / (126.0 - 95.0);
        }

        if ((volume > 126.0) && (volume < 163.0)) {
          limit1 = 3.1 + (volume - 126.0) * (2.9 - 3.1) / (163.0 - 126.0);
          limit2 = 4.0 + (volume - 126.0) * (3.7 - 4.0) / (163.0 - 126.0);
          limit3 = 73 + (volume - 126.0) * (77 - 73) / (163.0 - 126.0);
          limit4 = 91 + (volume - 126.0) * (94 - 91) / (163.0 - 126.0);
        }

        if ((volume > 163.0)) {
          limit1 = 2.9;
          limit2 = 3.7;
          limit3 = 77;
          limit4 = 94;
        }
      }
    }
  }
}