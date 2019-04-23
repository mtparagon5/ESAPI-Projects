using System;
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
			//double fractions = 0;
			//string status = "";
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
				//fractions = DvhExtensions.getTotalFractionsForPlanSum(psum);
				//status = "PlanSum";

			}
			else
			{
				planSetup = context.PlanSetup;
				selectedPlanningItem = (PlanningItem)planSetup;
				structureSet = planSetup.StructureSet;
				//if (planSetup.UniqueFractionation.NumberOfFractions != null)
				//{
				//	fractions = (double)planSetup.UniqueFractionation.NumberOfFractions;
				//}
				//status = planSetup.ApprovalStatus.ToString();

			}
			//var dosePerFx = planSetup.UniqueFractionation.PrescribedDosePerFraction.Dose;
			//var rxDose = (double)(dosePerFx * fractions);

			//structureSet = planSetup != null  planSetup.StructureSet : psum.StructureSet;/*psum.PlanSetups.Last().StructureSet;*/ // changed from first to last in case it's broken on next build
			//string pId = context.Patient.Id;
			//string course = context.Course.Id.ToString().Replace(" ", "_"); ;
			//string pName = ProcessIdName.processPtName(context.Patient.Name);



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

			IEnumerable<Structure> sorted_gtvList = new List<Structure>();
			IEnumerable<Structure> sorted_ctvList = new List<Structure>();
			IEnumerable<Structure> sorted_itvList = new List<Structure>();
			IEnumerable<Structure> sorted_ptvList = new List<Structure>();
			IEnumerable<Structure> sorted_oarList = new List<Structure>();
			IEnumerable<Structure> sorted_targetList = new List<Structure>();
			IEnumerable<Structure> sorted_structureList = new List<Structure>();


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

			#endregion structure organization and ordering

			double bsMax = 0;
			double onLMax = 0;
			double onRMax = 0;
			double globeLMax = 0;
			double globeRMax = 0;
			double chiasmMax = 0;
			//double bodyVRx = 0;

			

			foreach (var s in sorted_oarList)
			{
				DVHData dvhAA = selectedPlanningItem.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

				if ((s.Id.ToLower().Contains("brainstem")) && (!s.Id.ToLower().Contains("3")))
				{
					bsMax = Math.Round(dvhAA.MaxDose.Dose, 3);
				}

				if (s.Id.ToLower().Contains("chiasm"))
				{
					chiasmMax = Math.Round(dvhAA.MaxDose.Dose, 3);
				}

				if ((s.Id.ToLower().Contains("opticnerve") || (s.Id.ToLower().Contains("optic nerve"))) &&
					((s.Id.ToLower().Contains("_l")) || (s.Id.ToLower().Contains("lt")) || (s.Id.ToLower().Contains("left"))))
				{
					onLMax = Math.Round(dvhAA.MaxDose.Dose, 3);
				}

				if ((s.Id.ToLower().Contains("opticnerve") || (s.Id.ToLower().Contains("optic nerve"))) && 
					((s.Id.ToLower().Contains("_r")) || (s.Id.ToLower().Contains("rt")) || (s.Id.ToLower().Contains("right"))))
				{
					onRMax = Math.Round(dvhAA.MaxDose.Dose, 3);
				}

				if ((s.Id.ToLower().Contains("globe") || (s.Id.ToLower().Contains("eye"))) &&
					((s.Id.ToLower().Contains("r")) || (s.Id.ToLower().Contains("rt")) || (s.Id.ToLower().Contains("right"))))
				{
					globeRMax = Math.Round(dvhAA.MaxDose.Dose, 3);
				}

				if ((s.Id.ToLower().Contains("globe") || (s.Id.ToLower().Contains("eye"))) &&
					((s.Id.ToLower().Contains("_l")) || (s.Id.ToLower().Contains("lt")) || (s.Id.ToLower().Contains("left"))))
				{
					globeLMax = Math.Round(dvhAA.MaxDose.Dose, 3);
				}
			}

			

			var message = "GTV\t\t\tVol (cc)\r\n";
			message += "--------\t\t\t--------\r\n";

			foreach (var gtv in sorted_gtvList)
			{
				message += string.Format("{0}:\t\t{1}\r\n", gtv.Id.ToString(), Math.Round(gtv.Volume, 3));
			}


			message += "\r\nOAR\t\t\tDmax\r\n--------\t\t\t--------" + "\r\n" +
							string.Format("BrainStem\t\t\t{0}", bsMax) + "\r\n" +
							string.Format("Chiasm\t\t\t{0}", chiasmMax) + "\r\n" +
							string.Format("OpticNerve_R\t\t{0}", onRMax) + "\r\n" +
							string.Format("Globe_R\t\t\t{0}", globeRMax) + "\r\n" +
							string.Format("OpticNerve_L\t\t{0}", onLMax) + "\r\n" +
							string.Format("Globe_L\t\t\t{0}", globeLMax);

			MessageBox.Show(message, "Frameless Rx Info");

		}
	}
}
