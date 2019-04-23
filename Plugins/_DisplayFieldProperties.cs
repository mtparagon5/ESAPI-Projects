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
			//// lists for structures
			//List<Structure> gtvList = new List<Structure>();
			//List<Structure> ctvList = new List<Structure>();
			//List<Structure> itvList = new List<Structure>();
			//List<Structure> ptvList = new List<Structure>();
			//List<Structure> oarList = new List<Structure>();
			//List<Structure> targetList = new List<Structure>();
			//List<Structure> structureList = new List<Structure>();

			//IEnumerable<Structure> sorted_gtvList = new List<Structure>();
			//IEnumerable<Structure> sorted_ctvList = new List<Structure>();
			//IEnumerable<Structure> sorted_itvList = new List<Structure>();
			//IEnumerable<Structure> sorted_ptvList = new List<Structure>();
			//IEnumerable<Structure> sorted_oarList = new List<Structure>();
			//IEnumerable<Structure> sorted_targetList = new List<Structure>();
			//IEnumerable<Structure> sorted_structureList = new List<Structure>();


			//foreach (var structure in structureSet.Structures)
			//{
			//	// conditions for adding any structure
			//	if ((!structure.IsEmpty) &&
			//		(structure.HasSegment) &&
			//		(!structure.Id.Contains("*")) &&
			//		(!structure.Id.ToLower().Contains("markers")) &&
			//		(!structure.Id.ToLower().Contains("avoid")) &&
			//		(!structure.Id.ToLower().Contains("dose")) &&
			//		(!structure.Id.ToLower().Contains("contrast")) &&
			//		(!structure.Id.ToLower().Contains("air")) &&
			//		(!structure.Id.ToLower().Contains("dens")) &&
			//		(!structure.Id.ToLower().Contains("bolus")) &&
			//		(!structure.Id.ToLower().Contains("suv")) &&
			//		(!structure.Id.ToLower().Contains("match")) &&
			//		(!structure.Id.ToLower().Contains("wire")) &&
			//		(!structure.Id.ToLower().Contains("scar")) &&
			//		(!structure.Id.ToLower().Contains("chemo")) &&
			//		(!structure.Id.ToLower().Contains("pet")) &&
			//		(!structure.Id.ToLower().Contains("dnu")) &&
			//		(!structure.Id.ToLower().Contains("fiducial")) &&
			//		(!structure.Id.ToLower().Contains("artifact")) &&
			//		(!structure.Id.StartsWith("z", StringComparison.InvariantCultureIgnoreCase)) &&
			//		(!structure.Id.StartsWith("hs", StringComparison.InvariantCultureIgnoreCase)) &&
			//		(!structure.Id.StartsWith("av", StringComparison.InvariantCultureIgnoreCase)) &&
			//		(!structure.Id.StartsWith("opti ", StringComparison.InvariantCultureIgnoreCase)) &&
			//		(!structure.Id.StartsWith("opti-", StringComparison.InvariantCultureIgnoreCase)))
			//	//(structure.Id.Contains("CI-", StringComparison.InvariantCultureIgnoreCase) == false) && 
			//	//(structure.Id.Contains("R50-", StringComparison.InvariantCultureIgnoreCase) == false) &&
			//	//(structure.Id.Contains("CI_", StringComparison.InvariantCultureIgnoreCase) == false) && 
			//	//(structure.Id.Contains("R50_", StringComparison.InvariantCultureIgnoreCase) == false))
			//	{
			//		if (structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase))
			//		{
			//			gtvList.Add(structure);
			//			structureList.Add(structure);
			//			targetList.Add(structure);
			//		}
			//		if ((structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) ||
			//			(structure.Id.StartsWith("Prost", StringComparison.InvariantCultureIgnoreCase)))
			//		{
			//			ctvList.Add(structure);
			//			structureList.Add(structure);
			//			targetList.Add(structure);
			//		}
			//		if (structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase))
			//		{
			//			itvList.Add(structure);
			//			structureList.Add(structure);
			//			targetList.Add(structure);
			//		}
			//		if (structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase))
			//		{
			//			ptvList.Add(structure);
			//			structureList.Add(structure);
			//			targetList.Add(structure);
			//		}
			//		// conditions for adding breast plan targets
			//		if ((structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) ||
			//			(structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) ||
			//			(structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) ||
			//			(structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)))
			//		{
			//			targetList.Add(structure);
			//			structureList.Add(structure);
			//		}
			//		// conditions for adding oars
			//		if ((!structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase)) &&
			//			(!structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) &&
			//			(!structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase)) &&
			//			(!structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase)) &&
			//			(!structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) &&
			//			(!structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) &&
			//			(!structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) &&
			//			(!structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)) &&
			//			(!structure.Id.StartsWith("Scar", StringComparison.InvariantCultureIgnoreCase)) &&
			//			(!structure.Id.ToLower().Contains("carina")))
			//		{
			//			oarList.Add(structure);
			//			structureList.Add(structure);
			//		}
			//	}
			//}
			//sorted_gtvList = gtvList.OrderBy(x => x.Id);
			//sorted_ctvList = ctvList.OrderBy(x => x.Id);
			//sorted_itvList = itvList.OrderBy(x => x.Id);
			//sorted_ptvList = ptvList.OrderBy(x => x.Id);
			//sorted_targetList = targetList.OrderBy(x => x.Id);
			//sorted_oarList = oarList.OrderBy(x => x.Id);
			//sorted_structureList = structureList.OrderBy(x => x.Id);

			#endregion structure organization and ordering

			#region field properties example

			var tuplesList = new List<List<Tuple<string, string>>>();

			foreach (var beam in context.PlanSetup.Beams.ToList().Where(x => !x.IsSetupField))
			{
				var tuples = new List<Tuple<string, string>>();
				var cps = beam.ControlPoints.ToList();
				var beamId = beam.Id; tuples.Add(Tuple.Create("Field Id", beamId));
				var technique = beam.Technique.Id; tuples.Add(Tuple.Create("Technique", technique));
				var machine = beam.TreatmentUnit.Id; tuples.Add(Tuple.Create("Machine", machine));
				var scale = beam.TreatmentUnit.MachineScaleDisplayName; tuples.Add(Tuple.Create("Scale", scale));
				var energy = beam.EnergyModeDisplayName; tuples.Add(Tuple.Create("Energy", energy));
				var doseRate = beam.DoseRate; tuples.Add(Tuple.Create("Dose Rate [MU/min]", doseRate.ToString()));
				var mu = beam.Meterset.Value; tuples.Add(Tuple.Create("MU", mu.ToString("0.000")));
				var ssd = Math.Round((beam.SSD / 10), 1); tuples.Add(Tuple.Create("SSD", ssd.ToString("0.0")));
				if ((beam.MLCPlanType == MLCPlanType.ArcDynamic) || (beam.MLCPlanType == MLCPlanType.VMAT))
				{
					var gantryStartAngle = beam.ControlPoints[0].GantryAngle; tuples.Add(Tuple.Create("Gantry Start Angle", gantryStartAngle.ToString()));
					var gantryStopAngle = beam.ControlPoints.Last().GantryAngle; tuples.Add(Tuple.Create("Gantry Stop Angle", gantryStopAngle.ToString()));
				}
				else
				{
					var gantryAngle = beam.ControlPoints[0].GantryAngle; tuples.Add(Tuple.Create("Gantry Angle", gantryAngle.ToString()));
				}
				var collAngle = beam.ControlPoints[0].CollimatorAngle; tuples.Add(Tuple.Create("Coll. Angle", collAngle.ToString("0.0")));
				var x1 = Math.Round(beam.ControlPoints[0].JawPositions.X1 / -10, 1); //tuples.Add(Tuple.Create("X1", x1.ToString("0.0")));
				var x2 = Math.Round(beam.ControlPoints[0].JawPositions.X2 / 10, 1); //tuples.Add(Tuple.Create("X2", x2.ToString("0.0")));
				var y1 = Math.Round(beam.ControlPoints[0].JawPositions.Y1 / -10, 1); //tuples.Add(Tuple.Create("Y1", y1.ToString("0.0")));
				var y2 = Math.Round(beam.ControlPoints[0].JawPositions.Y2 / 10, 1); //tuples.Add(Tuple.Create("Y2", y2.ToString("0.0")));

				var isJawTracking = false;
				for (var i = 1; i < cps.Count(); i++)
				{
					if (cps[i].JawPositions.X1 != cps[i - 1].JawPositions.X1) { isJawTracking = true; continue; }
					else if (cps[i].JawPositions.X2 != cps[i - 1].JawPositions.X2) { isJawTracking = true; continue; }
					else if (cps[i].JawPositions.Y1 != cps[i - 1].JawPositions.Y1) { isJawTracking = true; continue; }
					else if (cps[i].JawPositions.Y2 != cps[i - 1].JawPositions.Y2) { isJawTracking = true; continue; }
				}
				if (isJawTracking)
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
					tuples.Add(Tuple.Create("JawTracking", "Yes\r\n-----------------"));
					tuples.Add(Tuple.Create("Initial FS", string.Format("X1: {0}, X2: {1}, Y1: {2}, Y2: {3}\r\n", Math.Abs(x1), Math.Abs(x2), Math.Abs(y1), Math.Abs(y2))));
					tuples.Add(Tuple.Create("Smallest FS", string.Format("X1: {0}, X2: {1}, Y1: {2}, Y2: {3}, ControlPoint: {4}", Math.Abs(minX1), Math.Abs(minX2), Math.Abs(minY1), Math.Abs(minY2), minCp)));
					tuples.Add(Tuple.Create("Smallest EquivFS", string.Format("{0} x {1}\r\n", minEquiv, minEquiv)));
					tuples.Add(Tuple.Create("Largest FS", string.Format("X1: {0}, X2: {1}, Y1: {2}, Y2: {3}, ControlPoint: {4}", Math.Abs(maxX1), Math.Abs(maxX2), Math.Abs(maxY1), Math.Abs(maxY2), maxCp)));
					tuples.Add(Tuple.Create("Largest EquivFS", string.Format("{0} x {1}\r\n-----------------", maxEquiv, maxEquiv)));

					//var r = string.Format("{0}: Yes\r\n" +
					//							"Initial FS:\tX1: {1}\r\n\tX2: {2}\r\n\tY1: {3}\r\n\tY2: {4}\r\n" +
					//							"Smallest Equiv FS: {6} x {7}\tCP[{5}]\r\n" +
					//							"Largest Equiv FS: {9} x {10}\tCP[{8}]\r\n", beam.Id, Math.Abs(x1), Math.Abs(x2), Math.Abs(y1), Math.Abs(y2),
					//							minCp, minEquiv, minEquiv, maxCp, maxEquiv, maxEquiv);
					//MessageBox.Show(r);
				}
				else
				{
					tuples.Add(Tuple.Create("JawTracking", "No"));
					tuples.Add(Tuple.Create("X1", x1.ToString()));
					tuples.Add(Tuple.Create("X2", x2.ToString()));
					tuples.Add(Tuple.Create("Y1", y1.ToString()));
					tuples.Add(Tuple.Create("Y2", y2.ToString()));
				}
				var mlcModel = beam.MLC.Model; tuples.Add(Tuple.Create("MLC Model", mlcModel));
				var mlcPlanType = beam.MLCPlanType; tuples.Add(Tuple.Create("MLC Plan Type", mlcPlanType.ToString()));
				if (beam.Wedges.ToList().Count > 0)
				{
					var wedgeId = beam.Wedges.ToList()[0].Id; tuples.Add(Tuple.Create("Wedge", wedgeId));
				}
				else
				{
					tuples.Add(Tuple.Create("Wedge", ""));
				}
				//if (beam.Applicator.Id.First())
				//{
				//	var applicator = beam.Applicator.Id; tuples.Add(Tuple.Create("Applicator", applicator));
				//}
				var boluses = beam.Boluses.ToList();
				if ((boluses.Count() - 1) >= 0) { for (var i = 0; i < boluses.Count(); i++) { tuples.Add(Tuple.Create("Bolus " + i.ToString(), boluses[i].Id)); } }
				else { tuples.Add(Tuple.Create("Bolus", "")); }
				var couchRtn = beam.ControlPoints.ToList()[0].PatientSupportAngle; tuples.Add(Tuple.Create("Couch Rtn", couchRtn.ToString()));
				var weight = beam.WeightFactor; tuples.Add(Tuple.Create("Weight", weight.ToString("0.000")));
				var userX = context.Image.UserOrigin.x / 10;
				var userY = context.Image.UserOrigin.y / 10;
				var userZ = context.Image.UserOrigin.z / 10;
				tuples.Add(Tuple.Create("User Origin", string.Format("{0}, {1}, {2}", Math.Round(userX, 1), Math.Round(userY, 1), Math.Round(userZ, 1))));
				var xpos = beam.IsocenterPosition.x / 10; /*tuples.Add(Tuple.Create("Xpos", xpos.ToString()));*/
				var ypos = beam.IsocenterPosition.y / 10; /*tuples.Add(Tuple.Create("Ypos", ypos.ToString()));*/
				var zpos = beam.IsocenterPosition.z / 10; /*tuples.Add(Tuple.Create("Zpos", zpos.ToString()));*/
				tuples.Add(Tuple.Create("Dicom Iso", string.Format("{0}, {1}, {2}", Math.Round(xpos, 1), Math.Round(ypos, 1), Math.Round(zpos, 1))));
				tuples.Add(Tuple.Create("Field Iso", string.Format("{0}, {1}, {2}", Math.Round(xpos - userX, 1), Math.Round(ypos - userY, 1), Math.Round(zpos - userZ, 1))));

				tuplesList.Add(tuples);
			}

			foreach (var tupleList in tuplesList)
			{
				var m = string.Empty;
				var field = string.Empty;
				foreach (var tuple in tupleList)
				{
					if (tuple.Item1 == "Field Id") { field = tuple.Item2; }
					m += string.Format("{0}: {1}\r\n", tuple.Item1, tuple.Item2);
				}
				MessageBox.Show(m, string.Format("Field Prop.- {0}", field));
			}

			#endregion

		}
	}
}
