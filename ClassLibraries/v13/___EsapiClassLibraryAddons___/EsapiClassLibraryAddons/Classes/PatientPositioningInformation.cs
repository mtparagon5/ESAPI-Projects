namespace VMS.TPS
{
  using System;
  using System.Collections.Generic;
  using VMS.TPS.Common.Model.API;
  using VMS.TPS.Common.Model.Types;

  public class PatientPositioningInformation
	{
		public static void patientOrientation(Image image, out PatientOrientation patientOrientation)
		{
			patientOrientation = image.ImagingOrientation;
		}

		

		public static void isocenterShiftFromOrigin(Image image, PlanSetup plan, out string isSingleIso,
																					out string shiftX,
																					out string shiftY,
																					out string shiftZ)
		{
			IEnumerable<Beam> planFields = plan.Beams;
			List<Beam> fieldsList = new List<Beam>();
			isSingleIso = "Yes; Single Isocenter";
			shiftX = "";
			shiftY = "";
			shiftZ = "";

			foreach (var field in planFields)
			{
				fieldsList.Add(field);
			}

			decimal field1_dicomX = Math.Round((decimal)fieldsList[0].IsocenterPosition.x / 10, 2);
			decimal field1_dicomY = Math.Round((decimal)fieldsList[0].IsocenterPosition.y / 10, 2);
			decimal field1_dicomZ = Math.Round((decimal)fieldsList[0].IsocenterPosition.z / 10, 2);

			decimal userOriginY = Math.Round(((decimal)image.UserOrigin.y / 10), 2);
			decimal userOriginZ = Math.Round(((decimal)image.UserOrigin.z / 10), 2);
			decimal userOriginX = Math.Round(((decimal)image.UserOrigin.x / 10), 2);

			var fieldXiso = (field1_dicomX - userOriginX);
			var fieldYiso = (field1_dicomY - userOriginY);
			var fieldZiso = (field1_dicomZ - userOriginZ);

			if (image.ImagingOrientation == PatientOrientation.HeadFirstProne)
			{
				fieldXiso = -(field1_dicomX - userOriginX);
				fieldYiso = -(field1_dicomY - userOriginY);
				fieldZiso = (field1_dicomZ - userOriginZ);
			}

			foreach (var f in fieldsList)
			{
				if ((f.IsocenterPosition.x != fieldsList[0].IsocenterPosition.x) ||
					(f.IsocenterPosition.y != fieldsList[0].IsocenterPosition.y) ||
					(f.IsocenterPosition.x != fieldsList[0].IsocenterPosition.x))
				{
					isSingleIso = "Warning: Multiple Isocenters - Verify Shifts";
				}
			}

			if (image.ImagingOrientation == PatientOrientation.HeadFirstSupine)
			{
				if (fieldXiso > 0) { shiftX = string.Format("{0} cm Left", Math.Round((Math.Abs(fieldXiso)), 1)); }
				else if (fieldXiso < 0) { shiftX = string.Format("{0} cm Right", Math.Round((Math.Abs(fieldXiso)), 1)); }
				else if (fieldXiso == 0) { shiftX = string.Format(" - "); }

				if (fieldYiso > 0) { shiftY = string.Format("{0} cm Posterior", Math.Round((Math.Abs(fieldYiso)), 1)); }
				else if (fieldYiso < 0) { shiftY = string.Format("{0} cm Anterior", Math.Round((Math.Abs(fieldYiso)), 1)); }
				else if (fieldYiso == 0) { shiftY = string.Format(" - "); }

				if (fieldZiso > 0) { shiftZ = string.Format("{0} cm Superior", Math.Round((Math.Abs(fieldZiso)), 1)); }
				else if (fieldZiso < 0) { shiftZ = string.Format("{0} cm Inferior", Math.Round((Math.Abs(fieldZiso)), 1)); }
				else if (fieldZiso == 0) { shiftZ = string.Format(" - "); }
			}

			else if (image.ImagingOrientation == PatientOrientation.HeadFirstProne)
			{
				if (fieldXiso > 0) { shiftX = string.Format("{0} cm Left", Math.Round((Math.Abs(fieldXiso)), 1)); }
				else if (fieldXiso < 0) { shiftX = string.Format("{0} cm Right", Math.Round((Math.Abs(fieldXiso)), 1)); }
				else if (fieldXiso == 0) { shiftX = string.Format(" - "); }

				if (fieldYiso > 0) { shiftY = string.Format("{0} cm Posterior", Math.Round((Math.Abs(fieldYiso)), 1)); }
				else if (fieldYiso < 0) { shiftY = string.Format("{0} cm Anterior", Math.Round((Math.Abs(fieldYiso)), 1)); }
				else if (fieldYiso == 0) { shiftY = string.Format(" - "); }

				if (fieldZiso > 0) { shiftZ = string.Format("{0} cm Superior", Math.Round((Math.Abs(fieldZiso)), 1)); }
				else if (fieldZiso < 0) { shiftZ = string.Format("{0} cm Inferior", Math.Round((Math.Abs(fieldZiso)), 1)); }
				else if (fieldZiso == 0) { shiftZ = string.Format(" - "); }
			}

			else if (image.ImagingOrientation == PatientOrientation.FeetFirstSupine)
			{
				if (fieldXiso > 0) { shiftX = string.Format("{0} cm Right", Math.Round((Math.Abs(fieldXiso)), 1)); }
				else if (fieldXiso < 0) { shiftX = string.Format("{0} cm Left", Math.Round((Math.Abs(fieldXiso)), 1)); }
				else if (fieldXiso == 0) { shiftX = string.Format(" - "); }

				if (fieldYiso > 0) { shiftY = string.Format("{0} cm Posterior", Math.Round((Math.Abs(fieldYiso)), 1)); }
				else if (fieldYiso < 0) { shiftY = string.Format("{0} cm Anterior", Math.Round((Math.Abs(fieldYiso)), 1)); }
				else if (fieldYiso == 0) { shiftY = string.Format(" - "); }

				if (fieldZiso > 0) { shiftZ = string.Format("{0} cm Inferior", Math.Round((Math.Abs(fieldZiso)), 1)); }
				else if (fieldZiso < 0) { shiftZ = string.Format("{0} cm Superior", Math.Round((Math.Abs(fieldZiso)), 1)); }
				else if (fieldZiso == 0) { shiftZ = string.Format(" - "); }
			}

			else if (image.ImagingOrientation == PatientOrientation.FeetFirstProne)
			{
				if (fieldXiso > 0) { shiftX = string.Format("{0} cm Right", Math.Round((Math.Abs(fieldXiso)), 1)); }
				else if (fieldXiso < 0) { shiftX = string.Format("{0} cm Left", Math.Round((Math.Abs(fieldXiso)), 1)); }
				else if (fieldXiso == 0) { shiftX = string.Format(" - "); }

				if (fieldYiso > 0) { shiftY = string.Format("{0} cm Posterior", Math.Round((Math.Abs(fieldYiso)), 1)); }
				else if (fieldYiso < 0) { shiftY = string.Format("{0} cm Anterior", Math.Round((Math.Abs(fieldYiso)), 1)); }
				else if (fieldYiso == 0) { shiftY = string.Format(" - "); }

				if (fieldZiso > 0) { shiftZ = string.Format("{0} cm Inferior", Math.Round((Math.Abs(fieldZiso)), 1)); }
				else if (fieldZiso < 0) { shiftZ = string.Format("{0} cm Superior", Math.Round((Math.Abs(fieldZiso)), 1)); }
				else if (fieldZiso == 0) { shiftZ = string.Format(" - "); }
			}

			else
			{
				shiftX = "Patient Orientation Undefined: Please Verify";
				shiftY = "Patient Orientation Undefined: Please Verify";
				shiftZ = "Patient Orientation Undefined: Please Verify";
			}

		}
	}

	#region examples

	#region patientOrientation

	//PatientOrientation pOrientation;

	//PatientPositioningInformation.patientOrientation(context.Image, out pOrientation);

	//MessageBox.Show(pOrientation.ToString());

	#endregion

	#region isocenter shift from origin

	//string isSingleIso = "";
	//var shiftX = "";
	//var shiftY = "";
	//var shiftZ = "";

	//PatientPositioningInformation.isocenterShiftFromOrigin(context.Image, plan, out isSingleIso, out shiftX, out shiftY, out shiftZ);

	//		MessageBox.Show(string.Format("Is Single Iso:\t{0}\n" +
	//										"Lateral Shift:\t{1}\n" +
	//										"Vertical Shift:\t{2}\n" +
	//										"Longitudinal Shift:\t{3}", isSingleIso, shiftX, shiftY, shiftZ));

	#endregion

	#endregion

}
