using System;
using System.Linq;
using System.Text;
using System.Windows;
//using System.Windows.Forms;
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
				MessageBox.Show("This will not work with a Plan Sum");
				return;
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

			double rxDose = context.PlanSetup.TotalPrescribedDose.Dose;

			if (MessageBox.Show("Do you have combined CI structures or PTVs?", "Overlapping CI Structures", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				var option1 = "CI-comb23-24";
				var option2 = "CI_comb23_24";
				var option3 = "CI-comb2324";
				var option4 = "CI_comb2324";
				var option5 = "CIcomb2324";
				var note = "NOTE: Caps and dashes/underscores don't matter.\r\nMake sure \"comb\" is between CI and the targets combined.\r\n\r\n*Same for overlapping PTV structures (e.g., PTVcomb23-24, etc.)";
				var question = string.Format("Are they named like one of these examples?\r\n\r\n\t- {0}\r\n\t- {1}\r\n\t- {2}\r\n\t- {3}\r\n\t- {4}\r\n\r\n{5}", option1, option2, option3, option4, option5, note);
				if (MessageBox.Show(question, "Overlapping CI Structures", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
				{
					MessageBox.Show("Please rename structures to ensure accuracy.\r\n\r\n*Or you can try using one of the other scripts that aids in calculating CI:\r\n\t - DVHLookups_v07\r\n\t - PlanReview");
					return;
				}
			}
			#region calcCi

			#region determine dose levels to calculate CIs for

			var is12 = false;
			var is14 = false;
			var is15 = false;
			var is17 = false;
			var is175 = false;
			var is18 = false;
			var is20 = false;
			var is21 = false;
			var is25 = false;
			var is30 = false;

			if (rxDose <= 21)
			{

				if (rxDose == 12)
				{
					is12 = true;
				}
				else if (rxDose == 14)
				{
					is14 = true;
					if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
					{
						if (MessageBox.Show("Do you have PTVs receiving 12 Gy?", "12 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							is12 = true;
						}
					}
				}
				else if (rxDose == 15)
				{
					is15 = true;
					if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
					{
						if (MessageBox.Show("Do you have PTVs receiving 14 Gy?", "14 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							is14 = true;
						}
					}
					if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
					{
						if (MessageBox.Show("Do you have PTVs receiving 12 Gy?", "12 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							is12 = true;
						}
					}
				}
				else if (rxDose == 17)
				{
					is17 = true;
					if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
					{
						if (MessageBox.Show("Do you have PTVs receiving 15 Gy?", "15 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							is15 = true;
						}
					}
					if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
					{
						if (MessageBox.Show("Do you have PTVs receiving 14 Gy?", "14 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							is14 = true;
						}
					}
					if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
					{
						if (MessageBox.Show("Do you have PTVs receiving 12 Gy?", "12 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							is12 = true;
						}
					}

				}
				else if (rxDose == 17.5)
				{
					is175 = true;
					if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
					{
						if (MessageBox.Show("Do you have PTVs receiving 17 Gy?", "17 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							is17 = true;
						}
						if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							if (MessageBox.Show("Do you have PTVs receiving 15 Gy?", "15 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								is15 = true;
							}
						}
						if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							if (MessageBox.Show("Do you have PTVs receiving 14 Gy?", "14 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								is14 = true;
							}
						}
						if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							if (MessageBox.Show("Do you have PTVs receiving 12 Gy?", "12 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								is12 = true;
							}
						}
					}
				}
				else if (rxDose == 18)
				{
					is18 = true;
					if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
					{
						if (MessageBox.Show("Do you have PTVs receiving 17.5 Gy?", "17.5 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							is175 = true;
						}
						if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							if (MessageBox.Show("Do you have PTVs receiving 17 Gy?", "17 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								is17 = true;
							}
							if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								if (MessageBox.Show("Do you have PTVs receiving 15 Gy?", "15 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
								{
									is15 = true;
								}
							}
							if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								if (MessageBox.Show("Do you have PTVs receiving 14 Gy?", "14 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
								{
									is14 = true;
								}
							}
							if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								if (MessageBox.Show("Do you have PTVs receiving 12 Gy?", "12 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
								{
									is12 = true;
								}
							}
						}
					}
				}
				else if (rxDose == 20)
				{
					is20 = true;
					if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
					{
						if (MessageBox.Show("Do you have PTVs receiving 18 Gy?", "18 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							is18 = true;
						}
						if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							if (MessageBox.Show("Do you have PTVs receiving 17.5 Gy?", "17.5 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								is175 = true;
							}
							if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								if (MessageBox.Show("Do you have PTVs receiving 17 Gy?", "17 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
								{
									is17 = true;
								}
								if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
								{
									if (MessageBox.Show("Do you have PTVs receiving 15 Gy?", "15 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
									{
										is15 = true;
									}
								}
								if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
								{
									if (MessageBox.Show("Do you have PTVs receiving 14 Gy?", "14 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
									{
										is14 = true;
									}
								}
								if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
								{
									if (MessageBox.Show("Do you have PTVs receiving 12 Gy?", "12 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
									{
										is12 = true;
									}
								}
							}
						}
					}
				}
				else if (rxDose == 21)
				{
					is21 = true;
					if (MessageBox.Show("CIs will be calculated for the 21 Gy IDL.\r\nDo you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
					{
						if (MessageBox.Show("Do you have PTVs receiving 20 Gy?", "20 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							is20 = true;
						}
						if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							if (MessageBox.Show("Do you have PTVs receiving 18 Gy?", "18 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								is18 = true;
							}
							if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								if (MessageBox.Show("Do you have PTVs receiving 17.5 Gy?", "17.5 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
								{
									is175 = true;
								}
								if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
								{
									if (MessageBox.Show("Do you have PTVs receiving 17 Gy?", "17 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
									{
										is17 = true;
									}
									if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
									{
										if (MessageBox.Show("Do you have PTVs receiving 15 Gy?", "15 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
										{
											is15 = true;
										}
									}
									if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
									{
										if (MessageBox.Show("Do you have PTVs receiving 14 Gy?", "14 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
										{
											is14 = true;
										}
										if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
										{
											if (MessageBox.Show("Do you have PTVs receiving 12 Gy?", "12 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
											{
												is12 = true;
											}
										}
									}
								}
							}
						}
					}
				}
				else
				{
					System.Windows.MessageBox.Show("Rx Dose not recognized for SRS tx - ask Matt T to add if necessary");
				}
			}
			else if (rxDose > 21)
			{
				if (rxDose == 25)
				{
					is25 = true;
					if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
					{
						System.Windows.MessageBox.Show("Lower dose levels not recognized for SRT; Ask Matt T to add if necessary :)");
					}
				}
				else if (rxDose == 30)
				{
					is30 = true;
					if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
					{
						if (MessageBox.Show("Do you have PTVs receiving 25 Gy?", "25 Gy Rx", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							is25 = true;
							if (MessageBox.Show("Do you have other dose levels?", "Other Dose Levels", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
							{
								System.Windows.MessageBox.Show("Lower dose levels not recognized for SRT; Ask Matt to add if necessary :)");
							}
						}
					}
				}
				else
				{
					System.Windows.MessageBox.Show("Lower dose levels not recognized for SRT; Ask Matt to add if necessary :)");
				}
			}

			#endregion

			#region two options to get CIs / Messages to display

			#region this way...only gives target info and ci

			//var listOfLists = new List<List<Tuple<Structure, double>>>();
			//var targetCiList = new List<Tuple<Structure, double>>();

			//if (is12)
			//{
			//	List<Tuple<Structure, double>> targetCiList12 = new List<Tuple<Structure, double>>();
			//	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 12, out targetCiList12);
			//	listOfLists.Add(targetCiList12);
			//}
			//if (is14)
			//{
			//	List<Tuple<Structure, double>> targetCiList14 = new List<Tuple<Structure, double>>();
			//	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 14, out targetCiList14);
			//	listOfLists.Add(targetCiList14);
			//}
			//if (is15)
			//{
			//	List<Tuple<Structure, double>> targetCiList15 = new List<Tuple<Structure, double>>();
			//	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 15, out targetCiList15);
			//	listOfLists.Add(targetCiList15);
			//}
			//if (is17)
			//{
			//	List<Tuple<Structure, double>> targetCiList17 = new List<Tuple<Structure, double>>();
			//	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 17, out targetCiList17);
			//	listOfLists.Add(targetCiList17);
			//}
			//if (is175)
			//{
			//	List<Tuple<Structure, double>> targetCiList175 = new List<Tuple<Structure, double>>();
			//	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 17.5, out targetCiList175);
			//	listOfLists.Add(targetCiList175);
			//}
			//if (is18)
			//{
			//	List<Tuple<Structure, double>> targetCiList18 = new List<Tuple<Structure, double>>();
			//	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 18, out targetCiList18);
			//	listOfLists.Add(targetCiList18);
			//}
			//if (is20)
			//{
			//	List<Tuple<Structure, double>> targetCiList20 = new List<Tuple<Structure, double>>();
			//	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 20, out targetCiList20);
			//	listOfLists.Add(targetCiList20);
			//}
			//if (is21)
			//{
			//	List<Tuple<Structure, double>> targetCiList21 = new List<Tuple<Structure, double>>();
			//	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 21, out targetCiList21);
			//	listOfLists.Add(targetCiList21);
			//}
			//if (is25)
			//{
			//	List<Tuple<Structure, double>> targetCiList25 = new List<Tuple<Structure, double>>();
			//	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 25, out targetCiList25);
			//	listOfLists.Add(targetCiList25);
			//}
			//if (is30)
			//{
			//	List<Tuple<Structure, double>> targetCiList30 = new List<Tuple<Structure, double>>();
			//	calcCi(selectedPlanningItem, sorted_ptvList, sorted_ciStructureList, 30, out targetCiList30);
			//	listOfLists.Add(targetCiList30);
			//}

			////var _temp = "";
			//foreach (var tupleList in listOfLists)
			//{
			//	//_temp += tupleList.Count + "\r\n";
			//	var _message = string.Empty;
			//	for (var i = 0; i < tupleList.Count; i++)
			//	{
			//		_message += string.Format("{0}: {1}cc\r\n\tVcirx:\t{2} cc\r\n\tCI:\t{3}\n", tupleList[i].Item1.Id, Math.Round(tupleList[i].Item1.Volume, 3), Math.Round(tupleList[i].Item1.Volume, 3), tupleList[i].Item2);
			//	}

			//	//var tupleList = new List<Tuple<Structure, double>>();
			//	//foreach (var tuple in tupleList)
			//	//{
			//	//	_message += string.Format("{0}:\r\n\tVol:\t{1} cc\r\n\tCI:\t{2}\n", tuple.Item1, Math.Round(tuple.Item1.Volume, 3), tuple.Item2);
			//	//}
			//	System.Windows.MessageBox.Show(_message, "CI Calc");
			//}

			#endregion

			#region or this way...provides more info for Vrx in CI structure to allow for verifying accuracy

			var listOfMessages = new List<string>();
			var matchedTargetCiList = new List<Tuple<Structure, Structure>>();
			matchTargetsWithCiStructures(sorted_ptvList, sorted_ciStructureList, out matchedTargetCiList);

			if (is12)
			{
				var _m12 = string.Empty;
				calcCi(selectedPlanningItem, matchedTargetCiList, 12, out _m12);
				listOfMessages.Add(_m12);
			}
			if (is14)
			{
				var _m14 = string.Empty;
				calcCi(selectedPlanningItem, matchedTargetCiList, 14, out _m14);
				listOfMessages.Add(_m14);
			}
			if (is15)
			{
				var _m15 = string.Empty;
				calcCi(selectedPlanningItem, matchedTargetCiList, 15, out _m15);
				listOfMessages.Add(_m15);
			}
			if (is17)
			{
				var _m17 = string.Empty;
				calcCi(selectedPlanningItem, matchedTargetCiList, 17, out _m17);
				listOfMessages.Add(_m17);
			}
			if (is175)
			{
				var _m175 = string.Empty;
				calcCi(selectedPlanningItem, matchedTargetCiList, 17.5, out _m175);
				listOfMessages.Add(_m175);
			}
			if (is18)
			{
				var _m18 = string.Empty;
				calcCi(selectedPlanningItem, matchedTargetCiList, 18, out _m18);
				listOfMessages.Add(_m18);
			}
			if (is20)
			{
				var _m20 = string.Empty;
				calcCi(selectedPlanningItem, matchedTargetCiList, 20, out _m20);
				listOfMessages.Add(_m20);
			}
			if (is21)
			{
				var _m21 = string.Empty;
				calcCi(selectedPlanningItem, matchedTargetCiList, 21, out _m21);
				listOfMessages.Add(_m21);
			}
			if (is25)
			{
				var _m25 = string.Empty;
				calcCi(selectedPlanningItem, matchedTargetCiList, 25, out _m25);
				listOfMessages.Add(_m25);
			}
			if (is30)
			{
				var _m30 = string.Empty;
				calcCi(selectedPlanningItem, matchedTargetCiList, 30, out _m30);
				listOfMessages.Add(_m30);
			}

			foreach (var m in listOfMessages)
			{
				//_temp += tupleList.Count + "\r\n";
				var _message = string.Empty;

				_message += string.Format("{0}\r\n\r\n", m);

				MessageBox.Show(_message, "CI Calc");
			}

			#endregion

			#endregion

			#endregion


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
				var tNum = t.ToString().Substring(t.ToString().ToLower().Replace("_", string.Empty).Replace("-", string.Empty).IndexOf("v") + 1, 2);
				var tNumIsAllDigits = tNum.All(char.IsDigit);
				if (!tNumIsAllDigits)
				{
					tNum = tNum.Substring(0, 1);
				}

				foreach (var ciStructure in sorted_ciStructureList)
				{
					var ciNum = ciStructure.ToString().Replace("_", string.Empty).Replace("-", string.Empty).Substring((ciStructure.ToString().ToLower().Replace("_", string.Empty).Replace("-", string.Empty).IndexOf('i')) + 1);
					if (ciNum == tNum)
					{
						var tVol = t.Volume;
						DVHData s_dvhAA = selectedPlanningItem.GetDVHCumulativeData(ciStructure, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);
						var ci = Math.Round(((getVolumeAtDose(s_dvhAA, rxDose)) / tVol), 2);

						if ((ci >= 5.0) || (ci < 0.5)) {  }
						else { targetCiList.Add(Tuple.Create(t, ci)); }
					}
				}
			}
		}

		public static void matchTargetsWithCiStructures(IEnumerable<Structure> sorted_ptvList, IEnumerable<Structure> sorted_ciStructureList, out List<Tuple<Structure, Structure>> targetCiList)
		{
			targetCiList = new List<Tuple<Structure, Structure>>();
			string tNum;
			string ciNum;
			bool tNumIsAllDigits;
			foreach (var t in sorted_ptvList)
			{
				if (!t.Id.ToString().ToLower().Contains("comb"))
				{
					tNum = t.ToString().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty).Substring(t.ToString().ToLower().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty).IndexOf("v") + 1, 2);
					tNumIsAllDigits = tNum.All(char.IsDigit);
					if (!tNumIsAllDigits)
					{
						tNum = tNum.Substring(0, 1);
					}
				}
				else
				{
					tNum = t.ToString().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty).Substring(t.ToString().ToLower().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty).IndexOf("b") + 1);
					tNumIsAllDigits = tNum.All(char.IsDigit);
					if (!tNumIsAllDigits)
					{
						tNum = tNum.Substring(0, 1);
					}
				}

				foreach (var ciStructure in sorted_ciStructureList)
				{
					if (!ciStructure.Id.ToString().ToLower().Contains("comb"))
					{
						ciNum = ciStructure.ToString().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty).Substring((ciStructure.ToString().ToLower().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty).IndexOf('i')) + 1);
						if (ciNum == tNum)
						{
							targetCiList.Add(Tuple.Create(t, ciStructure));
						}
					}
					else
					{
						
						ciNum = ciStructure.ToString().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty).Substring((ciStructure.ToString().ToLower().Replace(" ", string.Empty).Replace("_", string.Empty).Replace("-", string.Empty).IndexOf('b')) + 1);
						
						if (ciNum == tNum)
						{
							targetCiList.Add(Tuple.Create(t, ciStructure));
						}
					}
				}
			}
		}

		public static void calcCi(PlanningItem selectedPlanningItem, List<Tuple<Structure, Structure>> matchedTargetCiList, double rxDose, out string _message)
		{
			_message = string.Empty;
			for (var i = 0; i < matchedTargetCiList.Count; i++)
			{
				var t = matchedTargetCiList[i].Item1;
				var tVol = t.Volume;
				var ciStructure = matchedTargetCiList[i].Item2;
				
				DVHData s_dvhAA = selectedPlanningItem.GetDVHCumulativeData(ciStructure, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);

				var vRxForCiStructure = getVolumeAtDose(s_dvhAA, rxDose);

				var ci = Math.Round((vRxForCiStructure / tVol), 2);

				if ((ci >= 5.0) || (ci < 0.5)) {  }
				else
				{
					_message += string.Format("{0}: {1}cc\r\n\tCI Vrx:\t{2} cc\r\n\tCI:\t{3}\n", t.Id,
																						 Math.Round(t.Volume, 3),
																						 Math.Round(vRxForCiStructure, 3),
																						 ci);
				}
			}
		}

		public class R50Constraint
		{
			public static void LimitsFromVolume(double volume, out double limit1, out double limit2, out double limit3, out double limit4)
			{
				// larger tah last in the table
				limit1 = 5.9;
				limit2 = 7.5;
				limit3 = 50;
				limit4 = 57;

				if ((volume >= 1.8) && (volume < 3.8))
				{
					limit1 = 5.9 + (volume - 1.8) * (5.5 - 5.9) / (3.8 - 1.8);
					limit2 = 7.5 + (volume - 1.8) * (6.5 - 7.5) / (3.8 - 1.8);
					limit3 = 50 + (volume - 1.8) * (50 - 50) / (3.8 - 1.8);
					limit4 = 57 + (volume - 1.8) * (57 - 57) / (3.8 - 1.8);
				}

				if ((volume >= 3.8) && (volume < 7.4))
				{
					limit1 = 5.5 + (volume - 3.8) * (5.1 - 5.5) / (7.4 - 3.8);
					limit2 = 6.5 + (volume - 3.8) * (6.0 - 6.5) / (7.4 - 3.8);
					limit3 = 50 + (volume - 3.8) * (50 - 50) / (7.4 - 3.8);
					limit4 = 57 + (volume - 3.8) * (58 - 57) / (7.4 - 3.8);
				}

				if ((volume >= 7.4) && (volume < 13.2))
				{
					limit1 = 5.1 + (volume - 7.4) * (4.7 - 5.1) / (13.2 - 7.4);
					limit2 = 6.0 + (volume - 7.4) * (5.8 - 6.0) / (13.2 - 7.4);
					limit3 = 50 + (volume - 7.4) * (54 - 50) / (13.2 - 7.4);
					limit4 = 58 + (volume - 7.4) * (58 - 58) / (13.2 - 7.4); ;
				}

				if ((volume > 13.2) && (volume < 22.0))
				{
					limit1 = 4.7 + (volume - 13.2) * (4.5 - 4.7) / (22.0 - 13.2);
					limit2 = 5.8 + (volume - 13.2) * (5.5 - 5.8) / (22.0 - 13.2);
					limit3 = 50 + (volume - 13.2) * (54 - 50) / (22.0 - 13.2);
					limit4 = 58 + (volume - 13.2) * (63 - 58) / (22.0 - 13.2);
				}

				if ((volume > 22.0) && (volume < 34.0))
				{
					limit1 = 4.5 + (volume - 22.0) * (4.3 - 4.5) / (34.0 - 22.0);
					limit2 = 5.5 + (volume - 22.0) * (5.3 - 5.5) / (34.0 - 22.0);
					limit3 = 54 + (volume - 22.0) * (58 - 54) / (34.0 - 22.0);
					limit4 = 63 + (volume - 22.0) * (68 - 63) / (34.0 - 22.0);
				}

				if ((volume > 34.0) && (volume < 50.0))
				{
					limit1 = 4.3 + (volume - 34.0) * (4.0 - 4.3) / (50.0 - 34.0);
					limit2 = 5.3 + (volume - 34.0) * (5.0 - 5.3) / (50.0 - 34.0);
					limit3 = 58 + (volume - 34.0) * (62 - 58) / (50.0 - 34.0);
					limit4 = 68 + (volume - 34.0) * (77 - 68) / (50.0 - 34.0);
				}

				if ((volume > 50.0) && (volume < 70.0))
				{
					limit1 = 4.0 + (volume - 50.0) * (3.5 - 4.0) / (70.0 - 50.0);
					limit2 = 5.0 + (volume - 50.0) * (4.8 - 5.0) / (70.0 - 50.0);
					limit3 = 62 + (volume - 50.0) * (66 - 62) / (70.0 - 50.0);
					limit4 = 77 + (volume - 50.0) * (86 - 77) / (70.0 - 50.0);
				}

				if ((volume > 70.0) && (volume < 95.0))
				{
					limit1 = 3.5 + (volume - 70.0) * (3.3 - 3.5) / (95.0 - 70.0);
					limit2 = 4.8 + (volume - 70.0) * (4.4 - 4.8) / (95.0 - 70.0);
					limit3 = 66 + (volume - 70.0) * (70 - 66) / (95.0 - 70.0);
					limit4 = 86 + (volume - 70.0) * (89 - 86) / (95.0 - 70.0);
				}

				if ((volume > 95.0) && (volume < 126.0))
				{
					limit1 = 3.3 + (volume - 95.0) * (3.1 - 3.3) / (126.0 - 95.0);
					limit2 = 4.4 + (volume - 95.0) * (4.0 - 4.4) / (126.0 - 95.0);
					limit3 = 70 + (volume - 95.0) * (73 - 70) / (126.0 - 95.0);
					limit4 = 89 + (volume - 95.0) * (91 - 89) / (126.0 - 95.0);
				}

				if ((volume > 126.0) && (volume < 163.0))
				{
					limit1 = 3.1 + (volume - 126.0) * (2.9 - 3.1) / (163.0 - 126.0);
					limit2 = 4.0 + (volume - 126.0) * (3.7 - 4.0) / (163.0 - 126.0);
					limit3 = 73 + (volume - 126.0) * (77 - 73) / (163.0 - 126.0);
					limit4 = 91 + (volume - 126.0) * (94 - 91) / (163.0 - 126.0);
				}

				if ((volume > 163.0))
				{
					limit1 = 2.9;
					limit2 = 3.7;
					limit3 = 77;
					limit4 = 94;
				}
			}
		}
	}
}
