namespace VMS.TPS
{
	using System;
	using System.Linq;
	using System.Text;
	using System.Collections.Generic;

	using VMS.TPS.Common.Model.API;

	public class JawTracking
	{
  /// <summary>
  /// checks the use of jaw tracking for a single plan and returns a result for the physics doublecheck script
  /// </summary>
  /// <param name="plan"></param>
  /// <param name="pass"></param>
  /// <param name="result"></param>
		public static void isJawTracking(PlanSetup plan, out string pass, out string result)
		{
			pass = "Pass";

			bool usesJawTracking;
			result = "";
			
			var fields = plan.Beams;
			//int counter = 0;

			usesJawTracking = false;

			foreach (var field in fields)
			{
				if ((field.MLCPlanType.ToString() == "VMAT") || (field.MLCPlanType.ToString() == "DoseDynamic"))
				{
					// NEW CODE:
					// NOTE: need to finish adding result message
					var cp1 = field.ControlPoints[0];
					var _x1 = string.Format("{0:N1}", Math.Round((decimal)(cp1.JawPositions.X1) / -10, 2));
					var _y1 = string.Format("{0:N1}", Math.Round((decimal)(cp1.JawPositions.Y1) / -10, 2));
					var _y2 = string.Format("{0:N1}", Math.Round((decimal)(cp1.JawPositions.Y2) / 10, 2));
					var _x2 = string.Format("{0:N1}", Math.Round((decimal)(cp1.JawPositions.X2) / 10, 2));

					var cps = field.ControlPoints.ToList();
					for (var i = 1; i < cps.Count(); i++)
					{
						if (cps[i].JawPositions.X1 != cps[i - 1].JawPositions.X1) { usesJawTracking = true; continue; }
						else if (cps[i].JawPositions.X2 != cps[i - 1].JawPositions.X2) { usesJawTracking = true; continue; }
						else if (cps[i].JawPositions.Y1 != cps[i - 1].JawPositions.Y1) { usesJawTracking = true; continue; }
						else if (cps[i].JawPositions.Y2 != cps[i - 1].JawPositions.Y2) { usesJawTracking = true; continue; }
					}
					if (usesJawTracking)
					{
						double maxX = 0;
						double maxY = 0;
						double maxXY = 0;
						double maxX1 = 0;
						double maxX2 = 0;
						double maxY1 = 0;
						double maxY2 = 0;
						double maxEquiv = 0;
						var maxCp = 0;

						double minX = 1000000000000000;
						double minY = 1000000000000000;
						double minXY = 1000000000000000;
						double minX1 = 1000000000000000;
						double minX2 = 1000000000000000;
						double minY1 = 1000000000000000;
						double minY2 = 1000000000000000;
						double minEquiv = 1000000000000000;
						var minCp = 0;

						for (var i = 0; i < cps.Count(); i++)
						{
							maxX = (Math.Abs(cps[i].JawPositions.X1) + Math.Abs(cps[i].JawPositions.X2));
							maxY = (Math.Abs(cps[i].JawPositions.Y1) + Math.Abs(cps[i].JawPositions.Y2));
							if (maxX * maxY >= maxXY)
							{
								maxXY = maxX * maxY;
								maxX1 = cps[i].JawPositions.X1 / 10;
								maxX2 = cps[i].JawPositions.X2 / 10;
								maxY1 = cps[i].JawPositions.Y1 / 10;
								maxY2 = cps[i].JawPositions.Y2 / 10;

								maxEquiv = Math.Round(Math.Sqrt((maxX * maxY)) / 10, 1);
								maxCp = i + 1;
							}

							minX = (Math.Abs(cps[i].JawPositions.X1) + Math.Abs(cps[i].JawPositions.X2));
							minY = (Math.Abs(cps[i].JawPositions.Y1) + Math.Abs(cps[i].JawPositions.Y2));
							if (minX * minY <= minXY)
							{
								minXY = minX * minY;
								minX1 = cps[i].JawPositions.X1 / 10;
								minX2 = cps[i].JawPositions.X2 / 10;
								minY1 = cps[i].JawPositions.Y1 / 10;
								minY2 = cps[i].JawPositions.Y2 / 10;

								minEquiv = Math.Round(Math.Sqrt((minX * minY)) / 10, 1);
								minCp = i + 1;
							}
						}


						result += string.Format("{0}: Yes\r\n" +
												"Initial FS:\tX1: {1}\r\n\t\tX2: {2}\r\n\t\tY1: {3}\r\n\t\tY2: {4}\r\n" +
												"Smallest Eq FS:\t{6}x{7} CP[{5}]\r\n" +
												"Largest Eq FS:\t{9}x{10} CP[{8}]\r\n\r\n", field.Id, _x1, _x2, _y1, _y2,
												minCp, minEquiv, minEquiv, maxCp, maxEquiv, maxEquiv);
					}
				}
			}
			if (result.Contains("\r\n")) { result = result.Remove(result.LastIndexOf("\r\n")); }
			else { result = "No"; }
		}

    /// <summary>
    /// checks the use of jaw tracking for all plans and returns a result for the physics doublecheck script
    /// </summary>
    /// <param name="availablePlans"></param>
    /// <param name="pass"></param>
    /// <param name="result"></param>
		public static void isJawTrackingAllPlans(IEnumerator<PlanSetup> availablePlans, out string pass, out string result)
		{
			pass = "PASS";

			bool usesJawTracking;
			result = "";

			while (availablePlans.MoveNext())
			{
				var currentPlan = (ExternalPlanSetup)availablePlans.Current;
				var planFields = currentPlan.Beams;
				//int counter = 0;

				usesJawTracking = false;

				foreach (var field in planFields)
				{
					if ((field.MLCPlanType.ToString() == "VMAT") || (field.MLCPlanType.ToString() == "DoseDynamic"))
					{
						// NEW CODE:
						// NOTE: need to finish adding result message
						var cp1 = field.ControlPoints[0];
						var _x1 = string.Format("{0:N1}", Math.Round((decimal)(cp1.JawPositions.X1) / -10, 2));
						var _y1 = string.Format("{0:N1}", Math.Round((decimal)(cp1.JawPositions.Y1) / -10, 2));
						var _y2 = string.Format("{0:N1}", Math.Round((decimal)(cp1.JawPositions.Y2) / 10, 2));
						var _x2 = string.Format("{0:N1}", Math.Round((decimal)(cp1.JawPositions.X2) / 10, 2));

						var controlPoints = field.ControlPoints.ToList();
						for (var i = 1; i < controlPoints.Count - 1; i++)
						{
							if (controlPoints[i].JawPositions.X1 != controlPoints[i - 1].JawPositions.X1)
							{
								usesJawTracking = true;
								continue;
							}
							if (controlPoints[i].JawPositions.X2 != controlPoints[i - 1].JawPositions.X2)
							{
								usesJawTracking = true;
								continue;
							}
							if (controlPoints[i].JawPositions.Y1 != controlPoints[i - 1].JawPositions.Y1)
							{
								usesJawTracking = true;
								continue;
							}
							if (controlPoints[i].JawPositions.Y2 != controlPoints[i - 1].JawPositions.Y2)
							{
								usesJawTracking = true;
								continue;
							}
						}
						if (usesJawTracking)
						{
							result += string.Format("\r\nJawTracking IS used: X1: {0}\tX2: {1}\tY1: {2}\tY2: {3}\t",
														 _x1, _x2, _y1, _y2);
						}
						else { result += "\r\nJawTracking is NOT used."; }

					}
				}
			}
		}
	}

		

	#region examples

	#region jaw tracking test

	//string isJawTracking = "";

	//JawTracking.IsJawTracking(availablePlans, out isJawTracking);

	//MessageBox.Show(isJawTracking);

	#endregion

	#endregion

}
