using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using static ShiftCalculator.MainControl;


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
            #region context, maincontrol, and window defitions

            #region context variable definitions

            // to work for plan sum
            StructureSet structureSet;
            PlanningItem selectedPlanningItem;
            PlanSetup planSetup;
            PlanSum psum = null;
            //double? fractions = 0;
            //string status = "";

            //if (context.PlanSetup == null && context.PlanSumsInScope.Count() > 1)
            //{
            //    throw new ApplicationException("Please close other plan sums");
            //}
            IEnumerator<PlanSetup> availablePlans = null;
            if (context.PlanSetup == null)
            {
                psum = context.PlanSumsInScope?.First();
                planSetup = psum?.PlanSetups.First();
                selectedPlanningItem = (PlanningItem)psum;
                structureSet = planSetup?.StructureSet;
                ////fractions = DvhExtensions.getTotalFractionsForPlanSum(psum);
                //status = "PlanSum";
                availablePlans = context.PlansInScope.GetEnumerator();
                
            }
            else
            {
                availablePlans = context.PlansInScope.GetEnumerator();
                planSetup = context.PlanSetup;
                selectedPlanningItem = (PlanningItem)planSetup;
                structureSet = planSetup?.StructureSet;
                //if (planSetup?.UniqueFractionation.NumberOfFractions != null)
                //{
                //    fractions = (double)planSetup?.UniqueFractionation.NumberOfFractions;
                //}
                //status = planSetup.ApprovalStatus.ToString();

            }
            //var dosePerFx = planSetup?.UniqueFractionation.PrescribedDosePerFraction.Dose;
            //var rxDose = (double)(dosePerFx * fractions);
            structureSet = planSetup != null ? planSetup.StructureSet : psum.PlanSetups.Last().StructureSet; // changed from first to last in case it's broken on next build
            string pId = context.Patient.Id;
			      ProcessIdName.getRandomId(pId, out string rId);
            string course = context.Course.Id.ToString().Replace(" ", "_"); ;
            string pName = ProcessIdName.processPtName(context.Patient.Name);
            

            #endregion
            //---------------------------------------------------------------------------------
            #region window definitions

            // Add existing WPF control to the script window.
            var mainControl = new ShiftCalculator.MainControl();
            window.Content = mainControl;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Title = "Isocenter Shift Information for " + course;

            #endregion
            //---------------------------------------------------------------------------------
            #region mainControl variable definitions

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
			      mainControl.randomId = rId;
            mainControl.courseName = context.Course.Id.ToString().Replace(" ", "_");
            mainControl.courseHeader = course.Split('-').Last().Replace(" ", "_");
            mainControl.plans = availablePlans;
            mainControl.image = context.Image;


            // primary physician
            string tempPhysicianId = context.Patient.PrimaryOncologistId;
            PrimaryPhysician PrimaryPhysician = new PrimaryPhysician();
            PrimaryPhysician.Name = GetPrimary.Physician(tempPhysicianId);
            mainControl.primaryPhysician = PrimaryPhysician.Name.ToString();

            // ui textblocks
            mainControl.PrimaryOnc.Text = mainControl.primaryPhysician;
            mainControl.PatientId.Text = pId;
            mainControl.PatientName.Text = pName;
            mainControl.CourseId.Text = course;

            mainControl.Plan1_CB.Items.Add(string.Empty);
            mainControl.Plan2_CB.Items.Add(string.Empty);
            while (availablePlans.MoveNext())
            {
                mainControl.Plan1_CB.Items.Add(availablePlans.Current);
                mainControl.Plan2_CB.Items.Add(availablePlans.Current);
            }

			      mainControl.pOrientation = context.Image.ImagingOrientation;


			      #endregion

			      #endregion
			      //---------------------------------------------------------------------------------

			      #region combo boxes

			      mainControl.MarkerOrField_CB.Items.Add("");
            mainControl.MarkerOrField_CB.Items.Add("Marker");
            mainControl.MarkerOrField_CB.Items.Add("Field");

            #endregion  

        }
    }
}
