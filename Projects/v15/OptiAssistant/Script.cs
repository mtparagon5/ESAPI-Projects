using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;

// TODO: Replace the following version attributes by creating AssemblyInfo.cs. You can do this in the properties of the Visual Studio project.
//[assembly: AssemblyVersion("1.0.0.1")]
//[assembly: AssemblyFileVersion("1.0.0.1")]
//[assembly: AssemblyInformationalVersion("1.0")]

// TODO: Uncomment the following line if the script requires write access.
[assembly: ESAPIScript(IsWriteable = true)]


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

      #region context variable definitions

      if (context.StructureSet == null)
      {
        throw new ApplicationException("Oops, there doesn't seem to be an active structureset.");
      }

      StructureSet structureSet = context.StructureSet;
      string pId = context.Patient.Id;
      ProcessIdName.getRandomId(pId, out string rId);
      string course = context.Course != null ? context.Course.Id.ToString().Replace(" ", "_") : "NA";
      string pName = ProcessIdName.processPtName(context.Patient.Name);

      #region unused
      //PlanningItem selectedPlanningItem;
      //PlanSetup planSetup;
      //if (context.PlanSetup == null)
      //{
      //  throw new ApplicationException("Please pull in a single plan or structure set.");
      //}
      //else
      //{
      //  planSetup = context.PlanSetup;
      //  selectedPlanningItem = (PlanningItem)planSetup;
      //  structureSet = planSetup?.StructureSet;
      //}
      #endregion unused

      #endregion
      //---------------------------------------------------------------------------------
      #region window definitions

      // Add existing WPF control to the script window.
      var mainControl = new OptiAssistant.MainControl();
      //mainControl.Window = window;
      //window.WindowStyle = WindowStyle.None;
      window.Content = mainControl;
      window.SizeToContent = SizeToContent.WidthAndHeight;
      window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      window.Title = "Opti Assistant - Create Optimization Structures With Ease";

      #endregion
      //---------------------------------------------------------------------------------
      #region mainControl variable definitions

      mainControl.ss = structureSet;
      mainControl.patient = context.Patient;

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
      mainControl.id = pId;
      //mainControl.idAsDouble = Convert.ToDouble(mainControl.id);
      mainControl.randomId = rId;
      mainControl.courseName = course;

      // isGrady -- they don't have direct access to S Drive (to write files)
      var is_grady = MessageBox.Show("Are you accessing this script from the Grady Campus AND/OR from an Eclipse TBox?", "Direct $S Drive Access", MessageBoxButton.YesNo, MessageBoxImage.Question);
      if (is_grady == MessageBoxResult.Yes)
      {
        mainControl.isGrady = true;
      }
      else { mainControl.isGrady = false; }

      #endregion

      #endregion
      //---------------------------------------------------------------------------------
      #region structure listviews

      //---------------------------------------------------------------------------------
      #region organize structures into ordered lists
      // lists for structures

      GenerateStructureList.cleanAndOrderStructures(structureSet, out mainControl.sorted_gtvList,
                                                                  out mainControl.sorted_ctvList,
                                                                  out mainControl.sorted_itvList,
                                                                  out mainControl.sorted_ptvList,
                                                                  out mainControl.sorted_targetList,
                                                                  out mainControl.sorted_oarList,
                                                                  out mainControl.sorted_structureList,
                                                                  out mainControl.sorted_emptyStructuresList);


      #endregion structure organization and ordering
      //---------------------------------------------------------------------------------
      #region populate listviews

      // warn user if there are High Res Structures
      mainControl.highresMessage = string.Empty;
      foreach (var s in mainControl.sorted_structureList)
      {
        if (s.IsHighResolution) { mainControl.highresMessage += string.Format("- {0}\r\n\t", s.Id); mainControl.hasHighRes = true; }
      }
      foreach (var t in mainControl.sorted_ptvList)
      {
        if (t.IsHighResolution) { mainControl.needHRStructures = true; }
      }

      if (mainControl.hasHighRes)
      {
        MessageBox.Show(string.Format("The Following Are High Res Structures:\r\n\t{0}\r\n\r\nVerify accuracy of any avoidance, opti, or ring structures you create involving these structures.\r\n\r\nSometimes there can be issues when High Res Structures are involved.", mainControl.highresMessage));
      }

      // populate option listviews
      if (mainControl.sorted_ptvList.Count() < 1)
      {
        MessageBox.Show("There are no PTVs detected. The tools for Opti PTV and Ring Creation are disabled.");
        mainControl.CreateOptis_CB.IsEnabled = false;
        mainControl.CreateRings_CB.IsEnabled = false;
        mainControl.hasNoPTV = true;

        mainControl.BooleanAllTargets_CB.IsEnabled = false;
        mainControl.MultipleAvoidTargets_CB.IsEnabled = false;
      }
      else if (mainControl.sorted_ptvList.Count() == 1)
      {
        mainControl.MultipleDoseLevels_CB.IsEnabled = false;
        mainControl.hasSinglePTV  = true;

        // check and disable boolean all targets cb && disable multiple avoid targets cb
        mainControl.BooleanAllTargets_CB.IsChecked = true;
        mainControl.BooleanAllTargets_CB.IsEnabled = false;
        mainControl.MultipleAvoidTargets_CB.IsEnabled = false;
      }
      else
      {
        mainControl.CropAvoidsFromPTVs.Visibility = Visibility.Visible;
        mainControl.hasMultiplePTVs = true;
        mainControl.DoseLevel1_Radio.IsChecked = true;
        mainControl.BooleanAllTargets_CB.IsChecked = true;

        foreach (var s in mainControl.sorted_ptvList)
        {
          mainControl.DoseLevel1_Combo.Items.Add(s.Id); mainControl.AvoidTarget1_Combo.Items.Add(s.Id);
          mainControl.DoseLevel2_Combo.Items.Add(s.Id); mainControl.AvoidTarget2_Combo.Items.Add(s.Id);
          mainControl.DoseLevel3_Combo.Items.Add(s.Id); mainControl.AvoidTarget3_Combo.Items.Add(s.Id);
          mainControl.DoseLevel4_Combo.Items.Add(s.Id); mainControl.AvoidTarget4_Combo.Items.Add(s.Id);
        }
      }

      // populate listviews with structures on startup
      if (mainControl.sorted_oarList != null) { foreach (Structure s in mainControl.sorted_oarList) { mainControl.OarList_LV.Items.Add(s.Id); } }
      if (mainControl.sorted_ptvList != null) { foreach (Structure t in mainControl.sorted_ptvList) { mainControl.PTVList_LV.Items.Add(t.Id); mainControl.PTVListForRings_LV.Items.Add(t.Id); } }
     
      #endregion
      //---------------------------------------------------------------------------------

      #endregion
      //---------------------------------------------------------------------------------
      #region data populated on startup

      //---------------------------------------------------------------------------------
      #region log

      if (mainControl.isGrady == false)
        {
          mainControl.LogUser(mainControl.script);
        }

      #endregion
      //---------------------------------------------------------------------------------

      #endregion
      //---------------------------------------------------------------------------------

    }
  }
}
