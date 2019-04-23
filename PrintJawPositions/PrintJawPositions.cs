using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using static PrintJawPositions.MainControl;


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
            #region context variable definitions

            IEnumerator<PlanSetup> availablePlans = null; // context.PlanSumsInScope.GetEnumerator();
            //if (context.PlanSetup == null && context.PlanSumsInScope.Count() > 1)
            //{
            //    throw new ApplicationException("Please close other plan sums");
            //}
            if (context.PlanSetup == null)
            {
                availablePlans = context.PlansInScope.GetEnumerator();
            }
            else
            {
                availablePlans = context.PlansInScope.GetEnumerator();
            }
            string pId = context.Patient.Id;
			ProcessIdName.getRandomId(pId, out string rId);
            string course = context.Course.Id;
            string pName = ProcessIdName.processPtName(context.Patient.Name);

            #endregion
            //---------------------------------------------------------------------------------
            #region window definitions

            // Add existing WPF control to the script window.
            var mainControl = new PrintJawPositions.MainControl();
            window.Content = mainControl;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Title = "Jaw Positions for " + context.Patient + " \\ " + course;

            while (availablePlans.MoveNext())
            {
                mainControl.planList_LV.Items.Add(availablePlans.Current);
            }

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
            mainControl.id = context.Patient.Id;
            //mainControl.idAsDouble = Convert.ToDouble(mainControl.id);
            mainControl.randomId = rId;
            mainControl.courseName = course;
            mainControl.plans = availablePlans;
            string tempPhysicianId = context.Patient.PrimaryOncologistId;
            PrimaryPhysician PrimaryPhysician = new PrimaryPhysician();
            PrimaryPhysician.Name = GetPrimary.Physician(tempPhysicianId);
            mainControl.PrimaryOnc.Text = PrimaryPhysician.Name.ToString();
            mainControl.PatientId.Text = pId;
            mainControl.PatientName.Text = pName;
            mainControl.CourseId.Text = course;

            #endregion
            //---------------------------------------------------------------------------------
            #region log

            var is_grady = MessageBox.Show("Are you accessing this script from the Grady Campus?", "Direct $S Drive Access", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //var result = MessageBox.Show("Would you like to add this sales data to the data grid below?", "Add to DataGrid", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (is_grady == MessageBoxResult.Yes)
            {
                mainControl.isGrady = true;
            }
            else { mainControl.isGrady = false; }
            if (mainControl.isGrady == false)
            {
                mainControl.LogUser(mainControl.script);
            }

            #endregion
            //---------------------------------------------------------------------------------
        }
    }
}
