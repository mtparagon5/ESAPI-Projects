using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Text;

namespace VMS.TPS
{
	public class ViewJawPositionsScript
    {
		public ViewJawPositionsScript()
		{
		}
		//---------------------------------------------------------------------------------------------  
		public void Execute(ScriptContext context)
		{
			#region mlc properties

			var fields = context.ExternalPlanSetup.Beams;
			var controlPointList = new List<ControlPoint>();

			StringBuilder controlPointStringBuilder = new StringBuilder();
			string controlPointString = "";

            //foreach (var field in fields)
            //{
            //	var controlPoints = field.ControlPoints;
            //	foreach (var cP in controlPoints)
            //	{
            //		var x1 = cP.JawPositions.X1;
            //		var x2 = cP.JawPositions.X2;
            //		var y1 = cP.JawPositions.Y1;
            //		var y2 = cP.JawPositions.Y2;
            //		controlPointString = string.Format("");
            //	}

            //}
            controlPointString = "Field Sizes:\n";

            ControlPoint cp1 = null;
            ControlPoint cp2 = null;
            foreach (var f in fields)
            {
                //if (f.Technique.Id == "ARC")
                //{
                //    if (f.controlpoints)
                //    cp1 = f.ControlPoints[0];
                //    cp2 = f.ControlPoints[10];

                //    if ((cp1.JawPositions.X1 != cp2.JawPositions.X1) ||
                //        (cp1.JawPositions.X2 != cp2.JawPositions.X2) ||
                //        (cp1.JawPositions.Y1 != cp2.JawPositions.Y1) ||
                //        (cp1.JawPositions.Y2 != cp2.JawPositions.Y2))
                //    {
                //        controlPointString = string.Format("NOTE: Plan utilizes Jaw Tracking\n\n" +
                //                                            "Initial Field Sizes:\n");
                //    }
                //    else
                //    {
                //        controlPointString = "Field Sizes:";
                //    }
                //    break;
                //}
                //MessageBox.Show(f.MLCPlanType.ToString());


                if ((f.MLCPlanType.ToString() == "VMAT") || (f.MLCPlanType.ToString() == "DoseDynamic"))
                {
                    if (f.ControlPoints[10] != null)
                    {

                        cp1 = f.ControlPoints[0];
                        cp2 = f.ControlPoints[10];

                        if ((cp1.JawPositions.X1 != cp2.JawPositions.X1) ||
                            (cp1.JawPositions.X2 != cp2.JawPositions.X2) ||
                            (cp1.JawPositions.Y1 != cp2.JawPositions.Y1) ||
                            (cp1.JawPositions.Y2 != cp2.JawPositions.Y2))
                        {
                            //controlPointString = string.Format("NOTE: Plan utilizes Jaw Tracking\n\n" +
                            //                                    "Initial Field Sizes (cm):\n");
                            controlPointString = f.Meterset.ToString();
                        }
                    }
                    break;
                }
            }
            foreach (var field in fields)
			{
                if ((field.MLCPlanType.ToString() == "VMAT") || (field.MLCPlanType.ToString() == "DoseDynamic"))
                {
                    var fieldName = field.Id;
                    var controlPoints = field.ControlPoints;
                    var cP = controlPoints[0];
                    //var mu = field.Meterset.Value;
                    //MessageBox.Show(mu.ToString());

                    var x1 = Math.Round((cP.JawPositions.X1) / -10, 1);
                    var y1 = Math.Round((cP.JawPositions.Y1) / -10, 1);
                    var y2 = Math.Round((cP.JawPositions.Y2) / 10, 1);
                    var x2 = Math.Round((cP.JawPositions.X2) / 10, 1);
                    controlPointString = controlPointString +
                                            string.Format("\n{0}:\n" +
                                                            "\tX1\tX2\tY1\tY2\n\n" +
                                                            "\t{1:N1}\t{2:N1}\t{3:N1}\t{4:N1}\n\n", fieldName, x1, x2, y1, y2);
                }
			}
			MessageBox.Show(controlPointString);

			#endregion
		}
	}
}
