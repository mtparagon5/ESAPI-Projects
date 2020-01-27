namespace VMS.TPS
{
  using System;
  using System.Linq;
  using VMS.TPS.Common.Model.API;
  using VMS.TPS.Common.Model.Types;
  using System.Windows;

  public class FieldProperties
	{
		public static void arcDirectionCheck(PlanSetup plan, out string pass, out string result)
		{
            pass = "Warning"; result = "...add message here...";

			string technique = PlanExtensions.GetPlanTechnique(plan);

			if ((technique == "VMAT") || (technique == "ARC"))
			{
				//string arcId = "";
				string gantryDirection = "";
				int arcCount = 0;
				int ccwCounter = 0;
				int cwCounter = 0;
				//List<string> arcDirections = new List<string>();
				string arcDirectionWarning = "Arc Directions OK";

				// NOTE: to index the beams specifically...
				//		useful to create variables for individual beams to display at once
				//var beams = plan.Beams.ToList();
				//var beam1ControlPoints = beams[0].ControlPoints;
				//var beam1CollimatorAngle = beam1ControlPoints[0].CollimatorAngle;
				//var beam1ArcStartAngle = beam1ControlPoints[0].GantryAngle;
				//var beam1ArcStopAngle = beam1ControlPoints.Last().GantryAngle;
				//var beam1ArcLength = beams[0].ArcLength;

				// NOTE: to loop through the beams...
				foreach (Beam beam in plan.Beams)
				{
					arcCount++;
					//arcId = beam.Id.ToString();
					gantryDirection = beam.GantryDirection.ToString();


					//arcDirections.Add(gantryDirection);

					var controlPoints = beam.ControlPoints;
					var collimatorAngle = controlPoints[0].CollimatorAngle;
					//var arcStartAngle = controlPoints[0].GantryAngle;
					//var arcStopAngle = controlPoints.Last().GantryAngle;
					//var arcLength = beam.ArcLength;

					if (gantryDirection == "CounterClockwise") { ccwCounter++; }
					if (gantryDirection == "Clockwise") { cwCounter++; }
				}

				if (arcCount == 2)
				{
					if (ccwCounter == 2) { arcDirectionWarning = "Both arcs are CCW"; }
					else if (cwCounter == 2) { arcDirectionWarning = "Both arcs are CW"; }
				}

				else if (arcCount == 3)
				{
					if (ccwCounter == 3) { arcDirectionWarning = "All arcs are CCW"; }
					else if (cwCounter == 3) { arcDirectionWarning = "All arcs are CW"; }
				}

				result = arcDirectionWarning;
				if (result == "Arc Directions OK") { pass = "Pass"; }

			}

		}

		public static void fieldOrientationAgreementCheck(PlanSetup plan, out string pass, out string result)
		{
			pass = "Pass";
			result = "";

			PatientOrientation txOrientation = plan.TreatmentOrientation;
			//string technique = PlanExtensions.GetPlanTechnique(plan);
			
			var fields = plan.Beams.ToList();
			var fieldDescriptor = "";
			var cwCounter = 0;
			var ccwCounter = 0;
			var txFieldCounter = 0;
			//var cwArcs = new List<Beam>();
			//var ccwArcs = new List<Beam>();

			foreach (Beam field in fields)
			{
				if (field.GantryDirection == VMS.TPS.Common.Model.Types.GantryDirection.None)
				{
					fieldDescriptor = "";
					
					var beamId = field.Id.ToString();
					var gantryAngle = field.ControlPoints[0].GantryAngle;
					var existingPass = pass;
					var existingResult = result;

					getFieldOrientation(plan, field, out fieldDescriptor);
					checkFieldIdAndOrientationAgreement(field, fieldDescriptor, existingPass, existingResult, out pass, out result);
				}

				else
				{
					fieldDescriptor = "";
					var beamId = field.Id.ToString();
					var startAngle = field.ControlPoints[0].GantryAngle;
					var stopAngle = field.ControlPoints.Last().GantryAngle;
					var gantryDirection = field.GantryDirection;
					var existingPass = pass;
					var existingResult = result;

					getFieldOrientation(plan, field, out fieldDescriptor);
					checkFieldIdAndOrientationAgreement(field, fieldDescriptor, existingPass, existingResult, out pass, out result);

					if (gantryDirection == GantryDirection.Clockwise) { cwCounter++; }
					else if (gantryDirection == GantryDirection.CounterClockwise) { ccwCounter++; }
				}

				if (!field.IsSetupField) { txFieldCounter++; }
			}

			//if (((txFieldCounter == 2) && (ccwCounter == 2)) || ((txFieldCounter == 3) && (ccwCounter == 3)))
			if (txFieldCounter == ccwCounter)
			{
				pass = "Warning";
				result += "All Arcs are CCW; Verify intent:\r\n";

				foreach (Beam beam in plan.Beams)
				{
					if (beam.GantryDirection != VMS.TPS.Common.Model.Types.GantryDirection.None)
					{
						result += string.Format("{0}\r\n\tDirection:\t{1}\r\n\tStart Angle:\t{2}\r\n\tStop Angle:\t{3}\r\n",
							beam.Id.ToString(),
							beam.GantryDirection,
							beam.ControlPoints[0].GantryAngle,
							beam.ControlPoints.Last().GantryAngle);
					}
				}
			}

			//else if (((txFieldCounter == 2) && (cwCounter == 2)) || ((txFieldCounter == 3) && (cwCounter == 3)))
			if (txFieldCounter == cwCounter)
			{
				pass = "Warning";
				result += "All Arcs are CW; Verify intent:\r\n";

				foreach (Beam beam in plan.Beams)
				{
					var direction = "";
					
					if (beam.GantryDirection != VMS.TPS.Common.Model.Types.GantryDirection.None)
					{
						if (beam.GantryDirection == GantryDirection.Clockwise) { direction = "CW"; }
						else if (beam.GantryDirection == GantryDirection.CounterClockwise) { direction = "CCW"; }

						result += string.Format("{0}\r\n\tDirection:\t{1}\r\n\tStart Angle:\t{2}\r\n\tStop Angle:\t{3}\r\n",
							beam.Id.ToString(),
							direction,
							beam.ControlPoints[0].GantryAngle,
							beam.ControlPoints.Last().GantryAngle);
					}
				}
			}

			if (result.Contains("\r\n")) { result = result.Remove(result.LastIndexOf("\r\n")); }
			else if (result == "") { result = "Field Orientations OK"; }
		}

		public static void getFieldOrientation(PlanSetup plan, Beam beam, out string fieldOrientation)
		{
			//var txFieldGantryDirection = plan.Beams.ToList().Where(x => !x.IsSetupField).FirstOrDefault().GantryDirection;
			var txOrientation = plan.TreatmentOrientation;
			var gantryAngle = beam.ControlPoints.FirstOrDefault().GantryAngle;
			fieldOrientation = "";

			if (beam.GantryDirection == GantryDirection.None)
			{
				var ga_359to1 = (((gantryAngle >= 359) && (gantryAngle <= 360)) || ((gantryAngle >= 0) && (gantryAngle <= 1)));
				var ga_181to179 = ((gantryAngle >= 179) && (gantryAngle <= 181));
				var ga_269to271 = ((gantryAngle >= 269) && (gantryAngle <= 271));
				var ga_89to91 = ((gantryAngle >= 89) && (gantryAngle <= 91));
				var ga_359to271 = ((gantryAngle < 359) && (gantryAngle > 271));
				var ga_1to89 = ((gantryAngle > 1) && (gantryAngle < 89));
				var ga_269to181 = ((gantryAngle < 269) && (gantryAngle > 181));
				var ga_91to179 = ((gantryAngle > 91) && (gantryAngle < 179));

				#region field descriptor logic

				if (txOrientation == PatientOrientation.HeadFirstSupine)
				{
					if (ga_359to1)
					{
						fieldOrientation = "AP";
					}
					else if (ga_181to179)
					{
						fieldOrientation = "PA";
					}
					else if (ga_269to271)
					{
						fieldOrientation = "RLAT";
					}
					else if (ga_89to91)
					{
						fieldOrientation = "LLAT";
					}
					else if (ga_359to271)
					{
						fieldOrientation = "RAO";
					}
					else if (ga_1to89)
					{
						fieldOrientation = "LAO";
					}
					else if (ga_269to181)
					{
						fieldOrientation = "RPO";
					}
					else if (ga_91to179)
					{
						fieldOrientation = "LPO";
					}
				}
				else if (txOrientation == PatientOrientation.HeadFirstProne)
				{
					if (ga_359to1)
					{
						fieldOrientation = "PA";
					}
					else if (ga_181to179)
					{
						fieldOrientation = "AP";
					}
					else if (ga_269to271)
					{
						fieldOrientation = "LLAT";
					}
					else if (ga_89to91)
					{
						fieldOrientation = "RLAT";
					}
					else if (ga_359to271)
					{
						fieldOrientation = "RPO";
					}
					else if (ga_1to89)
					{
						fieldOrientation = "LPO";
					}
					else if (ga_269to181)
					{
						fieldOrientation = "RAO";
					}
					else if (ga_91to179)
					{
						fieldOrientation = "LAO";
					}
				}
				else if (txOrientation == PatientOrientation.FeetFirstSupine)
				{
					if (ga_359to1)
					{
						fieldOrientation = "AP";
					}
					else if (ga_181to179)
					{
						fieldOrientation = "PA";
					}
					else if (ga_269to271)
					{
						fieldOrientation = "LLAT";
					}
					else if (ga_89to91)
					{
						fieldOrientation = "RLAT";
					}
					else if (ga_359to271)
					{
						fieldOrientation = "LAO";
					}
					else if (ga_1to89)
					{
						fieldOrientation = "RAO";
					}
					else if (ga_269to181)
					{
						fieldOrientation = "LPO";
					}
					else if (ga_91to179)
					{
						fieldOrientation = "RPO";
					}
				}
				else if (txOrientation == PatientOrientation.FeetFirstProne)
				{
					if (ga_359to1)
					{
						fieldOrientation = "PA";
					}
					else if (ga_181to179)
					{
						fieldOrientation = "AP";
					}
					else if (ga_269to271)
					{
						fieldOrientation = "RLAT";
					}
					else if (ga_89to91)
					{
						fieldOrientation = "LLAT";
					}
					else if (ga_359to271)
					{
						fieldOrientation = "RPO";
					}
					else if (ga_1to89)
					{
						fieldOrientation = "LPO";
					}
					else if (ga_269to181)
					{
						fieldOrientation = "RAO";
					}
					else if (ga_91to179)
					{
						fieldOrientation = "LAO";
					}
				}
				else if (txOrientation == PatientOrientation.NoOrientation)
				{
					fieldOrientation = "Patient Orientation not defined. Verify field orientation.";
				}
				else if (txOrientation == PatientOrientation.Sitting)
				{
					fieldOrientation = "Patient is sitting. Verify field orientation.";
				}
				else
				{
					fieldOrientation = "Patient in decubitus position. Verify field orientation.";
				}

				#endregion
			}
			else if (beam.GantryDirection == GantryDirection.Clockwise) { fieldOrientation = "CW"; }
			else if (beam.GantryDirection == GantryDirection.CounterClockwise) { fieldOrientation = "CCW"; }
		}

		public static void checkSetupFieldIdAndOrientationAgreement(Beam setupField, string fieldOrientationDescriptor, string existingPass, string existingResult, out string pass, out string result)
		{
			#region checks for ID and Field Orientation agreement

			pass = existingPass;
			result = existingResult;

			if (setupField.Id.ToString().ToLower().Contains("rlat"))
			{
				if (fieldOrientationDescriptor != "RLAT") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", setupField.Id.ToString(), fieldOrientationDescriptor); }
			}
			else if (setupField.Id.ToString().ToLower().Contains("llat"))
			{
				if (fieldOrientationDescriptor != "LLAT") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", setupField.Id.ToString(), fieldOrientationDescriptor); }
			}
			else if (setupField.Id.ToString().ToLower().Contains("ap"))
			{
				if (fieldOrientationDescriptor != "AP") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", setupField.Id.ToString(), fieldOrientationDescriptor); }
			}
			else if (setupField.Id.ToString().ToLower().Contains("pa"))
			{
				if (fieldOrientationDescriptor != "PA") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", setupField.Id.ToString(), fieldOrientationDescriptor); }
			}

			#endregion
		}

		public static void checkStaticTxFieldIdAndOrientationAgreement(Beam field, string fieldOrientationDescriptor, string existingPass, string existingResult, out string pass, out string result)
		{
			#region checks for ID and Field Orientation agreement

			pass = existingPass;
			result = existingResult;

			if (field.Id.ToString().ToLower().Contains("rlat"))
			{
				if (fieldOrientationDescriptor != "RLAT") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldOrientationDescriptor); }
			}
			else if (field.Id.ToString().ToLower().Contains("llat"))
			{
				if (fieldOrientationDescriptor != "LLAT") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldOrientationDescriptor); }
			}
			else if (field.Id.ToString().ToLower().Contains("ap"))
			{
				if (fieldOrientationDescriptor != "AP") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldOrientationDescriptor); }
			}
			else if (field.Id.ToString().ToLower().Contains("pa"))
			{
				if (fieldOrientationDescriptor != "PA") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldOrientationDescriptor); }
			}
			else if (field.Id.ToString().ToLower().Contains("rao"))
			{
				if (fieldOrientationDescriptor != "RAO") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldOrientationDescriptor); }
			}
			else if (field.Id.ToString().ToLower().Contains("rpo"))
			{
				if (fieldOrientationDescriptor != "RPO") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldOrientationDescriptor); }
			}
			else if (field.Id.ToString().ToLower().Contains("lao"))
			{
				if (fieldOrientationDescriptor != "LAO") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldOrientationDescriptor); }
			}
			else if (field.Id.ToString().ToLower().Contains("lpo"))
			{
				if (fieldOrientationDescriptor != "LPO") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldOrientationDescriptor); }
			}

			#endregion
		}

		public static void checkArcIdAndDirectionAgreement(Beam arc, string fieldOrientationDescriptor, string existingPass, string existingResult, out string pass, out string result)
		{
			#region checks for ID and Field Orientation agreement

			pass = existingPass;
			result = existingResult;

			if ((arc.Id.ToString().ToLower().Contains("ccw")) && (fieldOrientationDescriptor != "CCW"))
			{
				pass = "Warning";
				result += string.Format("{0}: Direction ({1}) and ID do not agree\r\n\r\n", arc.Id.ToString(), fieldOrientationDescriptor);
			}
			if (((arc.Id.ToString().ToLower().Contains("cw")) && (!arc.Id.ToString().ToLower().Contains("ccw")) && (fieldOrientationDescriptor != "CW")))
			{
				pass = "Warning";
				result += string.Format("{0}: Direction ({1}) and ID do not agree\r\n\r\n", arc.Id.ToString(), fieldOrientationDescriptor);
			}

			#endregion
		}

		public static void checkFieldIdAndOrientationAgreement(Beam field, string fieldDescriptor, string existingPass, string existingResult, out string pass, out string result)
		{
			#region checks for ID and Field Orientation agreement

			pass = existingPass;
			result = existingResult;

			if (field.Id.ToString().ToLower().Contains("rlat"))
			{
				if (fieldDescriptor != "RLAT") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor); }
			}
			else if (field.Id.ToString().ToLower().Contains("llat"))
			{
				if (fieldDescriptor != "LLAT") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor); }
			}
			else if (field.Id.ToString().ToLower().Contains("ap"))
			{
				if (fieldDescriptor != "AP") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor); }
			}
			else if (field.Id.ToString().ToLower().Contains("pa"))
			{
				if (fieldDescriptor != "PA") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor); }
			}
			else if (field.Id.ToString().ToLower().Contains("rao"))
			{
				if (fieldDescriptor != "RAO") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor); }
			}
			else if (field.Id.ToString().ToLower().Contains("rpo"))
			{
				if (fieldDescriptor != "RPO") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor); }
			}
			else if (field.Id.ToString().ToLower().Contains("lao"))
			{
				if (fieldDescriptor != "LAO") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor); }
			}
			else if (field.Id.ToString().ToLower().Contains("lpo"))
			{
				if (fieldDescriptor != "LPO") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor); }
			}
			else if ((field.Id.ToString().ToLower().Contains("ccw")) && (fieldDescriptor != "CCW"))
			{
				pass = "Warning";
				result += string.Format("{0}: Direction ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor);
			}
			else if (((field.Id.ToString().ToLower().Contains("cw")) && (!field.Id.ToString().ToLower().Contains("ccw")) && (fieldDescriptor != "CW")))
			{
				pass = "Warning";
				result += string.Format("{0}: Direction ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor);
			}

			#endregion
		}

		public void checkIsoLaterality(PlanSetup plan, double threshold, out double offBodyCenter, out bool isRightSided)
		{
			offBodyCenter = 0; isRightSided = false;

			var beams = plan.Beams;

			Structure s_Body = plan.StructureSet.Structures.Where(x => (x.DicomType.ToLower() == "external")).FirstOrDefault(); // sometimes it may be called external for protocols (matt)

			VVector bodyCenter = s_Body.CenterPoint;


			bool flipped = ((plan.TreatmentOrientation == PatientOrientation.FeetFirstSupine) ||
							  (plan.TreatmentOrientation == PatientOrientation.HeadFirstProne) ||
							  (plan.TreatmentOrientation == PatientOrientation.HeadFirstProne)); // should this say feet first prone? (matt)


			int componentToCompare = 0;
			if ((plan.TreatmentOrientation == PatientOrientation.HeadFirstDecubitusRight) ||
				 (plan.TreatmentOrientation == PatientOrientation.HeadFirstDecubitusLeft) ||
				 (plan.TreatmentOrientation == PatientOrientation.FeetFirstDecubitusRight) ||
				 (plan.TreatmentOrientation == PatientOrientation.FeetFirstDecubitusLeft))
			{
				componentToCompare = 1;

				flipped = ((plan.TreatmentOrientation == PatientOrientation.HeadFirstDecubitusLeft) ||
						  (plan.TreatmentOrientation == PatientOrientation.FeetFirstDecubitusRight));
			}


			var txField = plan.Beams.Where(x => !x.IsSetupField).FirstOrDefault();
			VVector txFieldIso = txField.IsocenterPosition;

			if (flipped)
			{
				if (txFieldIso[componentToCompare] > (bodyCenter[componentToCompare]))
				{
					var shift = Math.Round((txFieldIso[componentToCompare] - bodyCenter[componentToCompare]) / 10, 2);
					if (Math.Abs(shift) > threshold)
					{
						MessageBox.Show(string.Format("Tx Iso is Left sided and > {0} cm from Patient Center; may need AP kV", threshold));
					}
					MessageBox.Show(string.Format("txField: {0}\r\n" +
													"txFieldIso: {1}\r\n" +
													"bodyCenter: {2}\r\n" +
													"shift: {3}", txField.Id, txFieldIso[componentToCompare], bodyCenter[componentToCompare], shift));
				};
			}
			else
			{
				if (txFieldIso[componentToCompare] < (bodyCenter[componentToCompare]))
				{
					var shift = Math.Round((txFieldIso[componentToCompare] - bodyCenter[componentToCompare]) / 10, 2);
					if (Math.Abs(shift) > threshold)
					{
						MessageBox.Show(string.Format("Tx Iso is right sided and > {0} cm from Patient Center; may need PA kV", threshold));
					}
					MessageBox.Show(string.Format("txField: {0}\r\n" +
													"txFieldIso: {1}\r\n" +
													"bodyCenter: {2}\r\n" +
													"shift: {3}", txField.Id, txFieldIso[componentToCompare], bodyCenter[componentToCompare], shift));
				};
			}



		}
	}
}
