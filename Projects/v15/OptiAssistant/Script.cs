using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;

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

      if (context.StructureSet == null)
      {
        throw new ApplicationException("Oops, there doesn't seem to be an active structureset.");
      }

      StructureSet structureSet = context.StructureSet;
      string pId = context.Patient.Id;
      ProcessIdName.getRandomId(pId, out string rId);
      string course = context.Course != null ? context.Course.Id.ToString().Replace(" ", "_") : "NA";
      string pName = ProcessIdName.processPtName(context.Patient.Name);

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
      window.Title = "Opti Structure Assistant";

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
      var is_grady = MessageBox.Show("Are you accessing this script from the Grady Campus?", "Direct $S Drive Access", MessageBoxButton.YesNo, MessageBoxImage.Question);
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

      if (mainControl.sorted_ptvList.Count() < 1)
      {
        MessageBox.Show("There are no PTVs detected. The tools for Opti PTV and Ring Creation are disabled.");
        mainControl.CreateOptis_CB.IsEnabled = false;
        mainControl.CreateRings_CB.IsEnabled = false;
      }

      // populate listviews with structures on startup
      if (mainControl.sorted_oarList != null) { foreach (Structure s in mainControl.sorted_oarList) { mainControl.OarList_LV.Items.Add(s); } }
      if (mainControl.sorted_ptvList != null) { foreach (Structure t in mainControl.sorted_ptvList) { mainControl.PTVList_LV.Items.Add(t); } }
      if (mainControl.sorted_ptvList != null) { foreach (Structure t in mainControl.sorted_ptvList) { mainControl.PTVListForRings_LV.Items.Add(t); } }
     
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
