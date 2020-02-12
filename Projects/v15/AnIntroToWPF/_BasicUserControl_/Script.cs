using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;

//these allow you to access the API
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace VMS.TPS
{
  public class Script
  {
    public Script()
    {
    }

    public void Execute(ScriptContext context, Window window)
    {

      // this is a comment

      // you can use var for quick declarations or you can explicitly define the data structure of a variable
      var p = context.Patient;
      Patient p2 = context.Patient;

      

      // display a result or message/warning using a MessageBox
      var message = "This is a test message.";
      var caption = "Message Box Title / Caption";
      MessageBox.Show(message, caption); // if you're using Visual Studio, add a comma after caption to see more options/overloads 
                                         // that can be added to a MessageBox e.g., button, icon, etc.



      // in this particular type of project, you'll likely want to define variables at the start of a script here
      // other logic may need/ideally be put in the UserControl.xaml.cs file
      var mainControl = new VMS.TPS.MainControl(); //it's VMS.TPS because that's the namespace name in the usercontrol.xaml.cs file

      // now you can assign definitions to variables in the usercontrol.xaml.cs file
      mainControl.patient = p;


      // In this project, a window pops up with whatever content you add in the usercontrol.xaml file
      // define the content of the window
      window.Content = mainControl;

      // some window settings
      window.SizeToContent = SizeToContent.WidthAndHeight;
      window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      window.Title = "ESAPI Starter App";

      // context variables:
      // you'll often want/need to define context variables to be used later in your code and they can depend on whether it's a
      // plan sum or not

      // can declare things prior to 
      StructureSet structureSet;
      PlanningItem selectedPlanningItem;
      PlanSetup planSetup;
      PlanSum psum = null;
      //double fractions = 0;
      //string status = "";
      // it can be a hassle to get the context for the exact plansum you want to read data from 
      // so it can be easier to have the user close all other plan sums

      // there are ways to get the exact psum tho with them being open if you want to
      if (context.PlanSetup == null && context.PlanSumsInScope.Count() > 1)
      {
        throw new ApplicationException("Please close other plan sums");
      }
      // a plan setup is an individual plan so if it's null, the current context 
      // is that of a plan sum (i.e., the user has a psum pulled in)
      if (context.PlanSetup == null)
      {
        psum = context.PlanSumsInScope.First();
        planSetup = psum.PlanSetups.First();
        selectedPlanningItem = (PlanningItem)psum;
        structureSet = psum.StructureSet;

        // if you don't want the user to use the script for a psum, you can exit it for them...messages are nice tho :)
        // MessageBox.Show ("This will not work with a Plan Sum");
        // return;
      }
      else
      {
        planSetup = context.PlanSetup;
        selectedPlanningItem = (PlanningItem)planSetup;
        structureSet = planSetup.StructureSet;
      }

      mainControl.ss = structureSet;
      mainControl.pitem = selectedPlanningItem;

      // if you want you can loop through the structureset

      // a \ denotes an escape...so \n is a new line \r is the return key so you may see \r\n in combination
      // \t - tab -- for multiple tabs just link multiple \t's together e.g., \t\t\t\t or \n\n\n\n\n

      // if you don't like the \t, you can create your own spacer:
      string SPACER = new String(' ', 3);

      message = "Structures:\r\n";
      foreach (var s in structureSet.Structures)
      {
        // you can add to a string (and other things) with +=
        message += string.Format("{0}\n", s.Id); 
      }
      // then show a message with the list of structures
      MessageBox.Show(message, "Structures");


      // in the esapi addons reference, there's a method that will create/filter lots of sorted structure lists
      GenerateStructureList.cleanAndOrderStructures(structureSet, out mainControl.sorted_gtvList,
                                                                  out mainControl.sorted_ctvList,
                                                                  out mainControl.sorted_itvList,
                                                                  out mainControl.sorted_ptvList,
                                                                  out mainControl.sorted_targetList,
                                                                  out mainControl.sorted_oarList,
                                                                  out mainControl.sorted_structureList,
                                                                  out mainControl.sorted_emptyStructuresList);

      // now you can loop through specific lists instead of having to loop through the generic structure set list
      message = "Targets";
      foreach (var t in mainControl.sorted_targetList)
      {
        message += string.Format("{0} ({1} cc)\r\n", t.Id, Math.Round(t.Volume, 3));

        // you can access their dvhdata
        var dvh_aa = selectedPlanningItem.GetDVHCumulativeData(t, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, mainControl.DEFAULT_BIN_WIDTH);
        var dvh_ar = selectedPlanningItem.GetDVHCumulativeData(t, DoseValuePresentation.Absolute, VolumePresentation.Relative, mainControl.DEFAULT_BIN_WIDTH);
        var dvh_rr = selectedPlanningItem.GetDVHCumulativeData(t, DoseValuePresentation.Relative, VolumePresentation.Relative, mainControl.DEFAULT_BIN_WIDTH);
        var dvh_ra = selectedPlanningItem.GetDVHCumulativeData(t, DoseValuePresentation.Relative, VolumePresentation.AbsoluteCm3, mainControl.DEFAULT_BIN_WIDTH);

        var targetMaxDose = dvh_aa.MaxDose.Dose;
        // most values returned have to be rounded
        var roundedMaxDose = Math.Round(targetMaxDose, 3);
        // there's alse a DvhExtensions Class in the Esapi Addons reference that has helpful methods as well
        var otherWayToGetMax = DvhExtensions.getMaxDose(dvh_aa);
        var v20 = DvhExtensions.getVolumeAtDose(dvh_aa, 20);

        // you can use variables or define things on the fly
        // and notice things can be out of order in string.Format() and can be used multiple times e.g., {2} below
        message += string.Format("{2}Max:\t{0} Gy\r\n{2}Mean:\t{1} Gy\r\n\n", roundedMaxDose, Math.Round(dvh_aa.MeanDose.Dose, 3), SPACER);

      }
      MessageBox.Show(message, "Targets");


      // you can use fun anonymous one liners to define things quickly when looping through lists, etc.
      Structure BODY = structureSet.Structures.Single(st => st.DicomType == "EXTERNAL");
      var bodyDVH_AA = selectedPlanningItem.GetDVHCumulativeData(BODY, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, mainControl.DEFAULT_BIN_WIDTH);
      // and you don't always have to use string.Format()
      message = "Body Max Dose: " + Math.Round(bodyDVH_AA.MaxDose.Dose, 3) + " Gy";

      MessageBox.Show(message, "Plan Max Dose");


      // can populate the window at startup
      foreach (var t in mainControl.sorted_targetList)
      {
        mainControl.Targets_ListView.Items.Add(t.Id);
      }



    }
  }
}
