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

            if (context.Patient != null)
            {
                mainControl.PatientId.Text = context.Patient.Id;
            }
            else
            {
                mainControl.PatientId.Text = string.Empty;
            }

            mainControl.FileSuffix.Items.Add("");
            mainControl.FileSuffix.Items.Add("Isodoses_4 Windows");
            mainControl.FileSuffix.Items.Add("Isodoses_Transversal");
            mainControl.FileSuffix.Items.Add("Isodoses_Frontal");
            mainControl.FileSuffix.Items.Add("Isodoses_Sagital");
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
        }
    }
}
