using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


namespace VMS.TPS
{
	public class Script
	{
		public Script()
		{
		}

		public void Execute(ScriptContext context /*, System.Windows.Window window*/)
		{

			#region context variable definitions

			// to work for plan sum
			StructureSet structureSet;
			PlanningItem selectedPlanningItem;
			PlanSetup planSetup;
			PlanSum psum = null;
			var PlanName = string.Empty;
			if (context.PlanSetup == null && context.PlanSumsInScope.Count() > 1)
			{
				throw new ApplicationException("Please close other plan sums");
			}
			if (context.PlanSetup == null)
			{
				psum = context.PlanSumsInScope.First();
				planSetup = psum.PlanSetups.First();
				selectedPlanningItem = (PlanningItem)psum;
				structureSet = psum.StructureSet;
				PlanName = psum.Id.ToString().Replace(" ", "_").Replace(":", "_");
				MessageBox.Show("This will not work with a Plan Sum");
				return;
			}
			else
			{
				planSetup = context.PlanSetup;
				selectedPlanningItem = (PlanningItem)planSetup;
				structureSet = planSetup.StructureSet;
				PlanName = planSetup.Id.ToString().Replace(" ", "_").Replace(":","_");

			}
			string pId = context.Patient.Id;
			string randomId = getRandomId(pId);
			string course = context.Course.Id.ToString().Replace(" ", "_"); ;
			string pName = processPtName(context.Patient.Name);
			planSetup.DoseValuePresentation = DoseValuePresentation.Absolute;
			double planMaxDose = 0;
			if (planSetup.Dose != null)
			{
				planMaxDose = Math.Round(planSetup.Dose.DoseMax3D.Dose, 5);
			}
			else { planMaxDose = Double.NaN; }

			#endregion

			#region organize structures into ordered lists
			// lists for structures
			List<Structure> gtvList = new List<Structure>();
			List<Structure> ctvList = new List<Structure>();
			List<Structure> itvList = new List<Structure>();
			List<Structure> ptvList = new List<Structure>();
			List<Structure> oarList = new List<Structure>();
			List<Structure> targetList = new List<Structure>();
			List<Structure> structureList = new List<Structure>();
			List<Structure> ciStructureList = new List<Structure>();

			IEnumerable<Structure> sorted_gtvList = new List<Structure>();
			IEnumerable<Structure> sorted_ctvList = new List<Structure>();
			IEnumerable<Structure> sorted_itvList = new List<Structure>();
			IEnumerable<Structure> sorted_ptvList = new List<Structure>();
			IEnumerable<Structure> sorted_oarList = new List<Structure>();
			IEnumerable<Structure> sorted_targetList = new List<Structure>();
			IEnumerable<Structure> sorted_structureList = new List<Structure>();
			IEnumerable<Structure> sorted_ciStructureList = new List<Structure>();

			foreach (var structure in structureSet.Structures)
			{
				// conditions for adding any structure
				if ((!structure.IsEmpty) &&
					(structure.HasSegment) &&
					(!structure.Id.Contains("*")) &&
					(!structure.Id.ToLower().Contains("markers")) &&
					(!structure.Id.ToLower().Contains("avoid")) &&
					(!structure.Id.ToLower().Contains("dose")) &&
					(!structure.Id.ToLower().Contains("contrast")) &&
					(!structure.Id.ToLower().Contains("air")) &&
					(!structure.Id.ToLower().Contains("dens")) &&
					(!structure.Id.ToLower().Contains("bolus")) &&
					(!structure.Id.ToLower().Contains("suv")) &&
					(!structure.Id.ToLower().Contains("match")) &&
					(!structure.Id.ToLower().Contains("wire")) &&
					(!structure.Id.ToLower().Contains("scar")) &&
					(!structure.Id.ToLower().Contains("chemo")) &&
					(!structure.Id.ToLower().Contains("pet")) &&
					(!structure.Id.ToLower().Contains("dnu")) &&
					(!structure.Id.ToLower().Contains("fiducial")) &&
					(!structure.Id.ToLower().Contains("artifact")) &&
					(!structure.Id.StartsWith("z", StringComparison.InvariantCultureIgnoreCase)) &&
					(!structure.Id.StartsWith("hs", StringComparison.InvariantCultureIgnoreCase)) &&
					(!structure.Id.StartsWith("av", StringComparison.InvariantCultureIgnoreCase)) &&
					(!structure.Id.StartsWith("opti ", StringComparison.InvariantCultureIgnoreCase)) &&
					(!structure.Id.StartsWith("opti-", StringComparison.InvariantCultureIgnoreCase)))
				//(structure.Id.Contains("CI-", StringComparison.InvariantCultureIgnoreCase) == false) && 
				//(structure.Id.Contains("R50-", StringComparison.InvariantCultureIgnoreCase) == false) &&
				//(structure.Id.Contains("CI_", StringComparison.InvariantCultureIgnoreCase) == false) && 
				//(structure.Id.Contains("R50_", StringComparison.InvariantCultureIgnoreCase) == false))
				{
					//if (structure.DicomType.ToLower() == "external") { body = structure; }

					if (structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase))
					{
						gtvList.Add(structure);
						structureList.Add(structure);
						targetList.Add(structure);
					}
					if ((structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) ||
						(structure.Id.StartsWith("Prost", StringComparison.InvariantCultureIgnoreCase)))
					{
						ctvList.Add(structure);
						structureList.Add(structure);
						targetList.Add(structure);
					}
					if (structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase))
					{
						itvList.Add(structure);
						structureList.Add(structure);
						targetList.Add(structure);
					}
					if (structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase))
					{
						ptvList.Add(structure);
						structureList.Add(structure);
						targetList.Add(structure);
					}
					if (structure.Id.StartsWith("CI", StringComparison.InvariantCultureIgnoreCase))
					{
						ciStructureList.Add(structure);
					}
					// conditions for adding breast plan targets
					if ((structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) ||
						(structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) ||
						(structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) ||
						(structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)))
					{
						targetList.Add(structure);
						structureList.Add(structure);
					}
					// conditions for adding oars
					if ((!structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase)) &&
						(!structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) &&
						(!structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase)) &&
						(!structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase)) &&
						(!structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) &&
						(!structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) &&
						(!structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) &&
						(!structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)) &&
						(!structure.Id.StartsWith("Scar", StringComparison.InvariantCultureIgnoreCase)) &&
						(!structure.Id.ToLower().Contains("carina")))
					{
						oarList.Add(structure);
						structureList.Add(structure);
					}
				}
			}
			sorted_gtvList = gtvList.OrderBy(x => x.Id);
			sorted_ctvList = ctvList.OrderBy(x => x.Id);
			sorted_itvList = itvList.OrderBy(x => x.Id);
			sorted_ptvList = ptvList.OrderBy(x => x.Id);
			sorted_targetList = targetList.OrderBy(x => x.Id);
			sorted_oarList = oarList.OrderBy(x => x.Id);
			sorted_structureList = structureList.OrderBy(x => x.Id);
			sorted_ciStructureList = ciStructureList.OrderBy(x => x.Id);

			#endregion structure organization and ordering

			string jsonPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\_JsonData_\\" + randomId + "_data.json";
			string jsonString = "[{\"patientName\":\"" + pName + "\", " +
									"\"patientId\":\"" + pId + "\", " +
									"\"randomId\":\"" + randomId + "\", " +
									"\"courseId\":\"" + course + "\", " +
									"\"planData\":[" +
										"{\"planId\":\"" + planSetup.Id + "\"," +
										"\"planMaxDose[Gy]\":" + planMaxDose + "," +
										"\"targetData\":["; // ]}]}]

			string gtv_string = "{\"gtvs\":[";
			string ctv_string = ",{\"ctvs\":[";
			string itv_string = ",{\"itvs\":[";
			string ptv_string = ",{\"ptvs\":[";
			
			gtv_string = getData(selectedPlanningItem, sorted_gtvList, gtv_string);
			ctv_string = getData(selectedPlanningItem, sorted_ctvList, ctv_string);
			itv_string = getData(selectedPlanningItem, sorted_itvList, itv_string);
			ptv_string = getData(selectedPlanningItem, sorted_ptvList, ptv_string);

			jsonString += gtv_string;
			jsonString += ctv_string;
			jsonString += itv_string;
			jsonString += ptv_string;

			jsonString += "]}]}]";

			File.WriteAllText(jsonPath, jsonString);

		}

		public static double getVolumeAtDose(DVHData dvh, double DoseLim)
		{
			for (int i = 0; i < dvh.CurveData.Length; i++)
			{
				if (dvh.CurveData[i].DoseValue.Dose >= DoseLim)
				{
					return dvh.CurveData[i].Volume;
				}
			}
			return 0;
		}

		

		public static void calcCi(PlanningItem selectedPlanningItem, IEnumerable<Structure> sorted_ptvList, IEnumerable<Structure> sorted_ciStructureList, double rxDose, out List<Tuple<Structure, double>> targetCiList)
		{
			targetCiList = new List<Tuple<Structure, double>>();
			foreach (var t in sorted_ptvList)
			{
				foreach (var ciStructure in sorted_ciStructureList)
				{
					var ciNum = ciStructure.ToString().Substring(ciStructure.ToString().ToLower().Replace("_", string.Empty).Replace("-", string.Empty).IndexOf('i'));
					var tNum = t.ToString().Substring(t.ToString().ToLower().Replace("_", string.Empty).Replace("-", string.Empty).IndexOf("v"), 2);
					var tNumIsAllDigits = tNum.All(char.IsDigit);
					if (tNumIsAllDigits)
					{
						continue;
					}
					else
					{
						tNum = tNum.Substring(0, 1);
					}

					if (ciNum == tNum)
					{
						var tVol = t.Volume;
						DVHData s_dvhAA = selectedPlanningItem.GetDVHCumulativeData(ciStructure, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

						var ci = Math.Round((getVolumeAtDose(s_dvhAA, rxDose)), 2);
						if ((ci >= 3.0) || (ci < 0.5)) continue;
						else { targetCiList.Add(Tuple.Create(t, ci)); }

						//targetCiList.Add(Tuple.Create(t.Id.ToString().Remove(t.Id.ToString().IndexOf(':')), ci));
						
					}
				}
			}
		}


		public string getData(PlanningItem selectedPlanningItem, IEnumerable<Structure> sortedTargetList, string result)
		{
			string _result = result;
			foreach (var t in sortedTargetList)
			{
				DVHData dvhAA = selectedPlanningItem.GetDVHCumulativeData(t, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

				_result += "{\"id\": \"" + t.Id + "\", " +
								"\"data\": [{" +
									"\"volume[cc]\": " + Math.Round(t.Volume, 5) + ", " +
									"\"minDose[Gy]\": " + Math.Round(dvhAA.MinDose.Dose, 5) + ", " +
									"\"maxDose[Gy]\": " + Math.Round(dvhAA.MaxDose.Dose, 5) +
									"}]" +
								"},";

			}
			_result = _result.TrimEnd(',');
			_result += "]}";
			return _result;
		}
		public static string getRandomId(string patientId)
		{
			string randomId = "";
			double pIdAsDouble = 0;
			double n = 0;
			var isNumeric = double.TryParse(patientId, out n);
			if (isNumeric)
			{
				pIdAsDouble = Convert.ToDouble(patientId);
				randomId = Math.Ceiling(Math.Sqrt(pIdAsDouble * 5)).ToString();
			}
			else { randomId = patientId; }
			return randomId;
		}
		public static string processPtName(string PTName)
		{
			string OrigName = PTName;
			string newName = "";
			for (int i = 0; i < OrigName.Length; i++)
			{
				if (OrigName[i] == '(')
				{
					newName = OrigName.Substring(0, i);
					return newName;
				}

			}
			return newName;
		}
	}
}
