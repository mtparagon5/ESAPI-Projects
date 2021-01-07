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
//[assembly: ESAPIScript(IsWriteable = true)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]


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

      // to work for plan sum
      StructureSet structureSet;
      PlanningItem selectedPlanningItem;
      PlanSetup planSetup;
      PlanSum psum = null;
      double fractions = 0;
      if (context.PlanSetup == null && context.PlanSumsInScope.Count() > 1)
      {
        throw new ApplicationException("Please close other plan sums");
      }
      if (context.PlanSetup == null)
      {
        psum = context.PlanSumsInScope?.First();
        planSetup = psum?.PlanSetups.First();
        selectedPlanningItem = (PlanningItem)psum;
        structureSet = psum?.StructureSet;
        fractions = DvhExtensions.getTotalFractionsForPlanSum(psum);

      }
      else
      {
        planSetup = context.PlanSetup;
        selectedPlanningItem = (PlanningItem)planSetup;
        structureSet = planSetup?.StructureSet;
        if (planSetup?.NumberOfFractions != null)
        {
          fractions = (double)planSetup?.NumberOfFractions;
        }
      }
      var dosePerFx = planSetup.DosePerFraction.Dose;
      var rxDose = (double)(dosePerFx * fractions);

      //structureSet = planSetup != null ? planSetup.StructureSet : psum.StructureSet;/*psum.PlanSetups.Last().StructureSet;*/ // changed from first to last in case it's broken on next build
      string pId = context.Patient.Id;
      string course = context.Course.Id.ToString().Replace(" ", "_");
      
      #endregion
      //---------------------------------------------------------------------------------
      #region window definitions

      // Add existing WPF control to the script window.
      var mainControl = new ExportToCSV.MainControl();
      window.Content = mainControl;
      window.SizeToContent = SizeToContent.WidthAndHeight;
      window.WindowState = WindowState.Maximized;
      window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      window.Title = "ExportToCSV - Extract Plan Data to CSV";

      #endregion
      //---------------------------------------------------------------------------------
      #region mainControl variable definitions

      mainControl.ss = structureSet;
      mainControl.patient = context.Patient;
      mainControl.pitem = selectedPlanningItem;
      mainControl.patientId = pId;
      mainControl.courseId = course;
      mainControl.dosePerFraction= dosePerFx;
      mainControl.numFractions = fractions;
      mainControl.prescriptionDose = rxDose;

      mainControl.userName = context.CurrentUser.Name.ToString().Replace(",", "_").Replace(" ", ""); ;
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

      // isGrady -- they don't have direct access to S Drive (to write files)
      var is_grady = MessageBox.Show("Are you accessing this script from Home, the Grady Campus AND/OR from an Eclipse TBox?", "Direct $S Drive Access", MessageBoxButton.YesNo, MessageBoxImage.Question);
      if (is_grady == MessageBoxResult.Yes)
      {
        throw new ApplicationException("Please log in to a machine that has access to the shared S drive.");
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

      // populate listviews with structures on startup
      if (mainControl.sorted_structureList != null) 
      { 
        foreach (Structure s in mainControl.sorted_structureList) 
        { 
          mainControl.structureListBoxItems.Add(new ExportToCSV.MainControl.StructureItem(s.Id)); 
        } 
      }

      mainControl.StructureList_LV.ItemsSource = mainControl.structureListBoxItems;

      #endregion
      //---------------------------------------------------------------------------------
      #region populate comboboxes

      // populate listviews with structures on startup
      mainControl.ResolutionSelector_Combo.ItemsSource = mainControl.resolutionOptionsList;
      mainControl.ResolutionSelector_Combo.SelectedItem = mainControl.resolutionOptionsList[1];

      #endregion

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
