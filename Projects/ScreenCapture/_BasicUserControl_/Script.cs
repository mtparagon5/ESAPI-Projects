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
            #region window definitions

            // Add existing WPF control to the script window.
            var mainControl = new MainControl();

            window.Content = mainControl;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Title = "Aria Screen Capture Tool";

            #endregion

            // patient id for path
            if (context.Patient != null)
            {
                mainControl.PatientId.Text = context.Patient.Id;
            }
            else
            {
                mainControl.PatientId.Text = string.Empty;
            }

            // course id for path
            mainControl.courseId = context.Course.Id;

            // file suffix for path
            mainControl.FileSuffix.Items.Add("");
            mainControl.FileSuffix.Items.Add("Isodoses_1");
            mainControl.FileSuffix.Items.Add("Isodoses_2");
            mainControl.FileSuffix.Items.Add("Isodoses_3");
            mainControl.FileSuffix.Items.Add("Isodoses_4");
            mainControl.FileSuffix.Items.Add("Isodoses_5");
            mainControl.FileSuffix.Items.Add("BEV1");
            mainControl.FileSuffix.Items.Add("BEV2");
            mainControl.FileSuffix.Items.Add("BEV3");
            mainControl.FileSuffix.Items.Add("BEV4");
            mainControl.FileSuffix.Items.Add("BEV5");
            mainControl.FileSuffix.Items.Add("BEV6");
            mainControl.FileSuffix.Items.Add("BEV7");
            mainControl.FileSuffix.Items.Add("BEV8");
            mainControl.FileSuffix.Items.Add("BEV9");
            mainControl.FileSuffix.Items.Add("BEV10");

            // original way -- saving for future ref since some images have this path
            //mainControl.FileSuffix.Items.Add("Isodoses_4 Windows"); 
            //mainControl.FileSuffix.Items.Add("Isodoses_Transversal");
            //mainControl.FileSuffix.Items.Add("Isodoses_Frontal");
            //mainControl.FileSuffix.Items.Add("Isodoses_Sagital");
            // plus BEV#s
        }
    }
}
