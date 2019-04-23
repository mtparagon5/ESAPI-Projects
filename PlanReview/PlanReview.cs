using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using static PlanReview.MainControl;


// Do not change namespace and class name
// otherwise Eclipse will not be able to run the script.
namespace VMS.TPS
{
    public class Script
    {
        public Script()
        {
        }

        public void Execute(ScriptContext context, Window window)
        {
            
            //---------------------------------------------------------------------------------
            #region plan context, maincontrol, and window defitions

            #region context variable definitions

            // to work for plan sum
            StructureSet structureSet;
            PlanningItem selectedPlanningItem;
            PlanSetup planSetup;
            PlanSum psum = null;
            double? fractions = 0;
            string status = "";
            if (context.PlanSetup == null && context.PlanSumsInScope.Count() > 1)
            {
                throw new ApplicationException("Please close other plan sums");
            }
            if (context.PlanSetup == null)
            {
                psum = context.PlanSumsInScope?.First();
                planSetup = psum?.PlanSetups.First();
                selectedPlanningItem = (PlanningItem)psum;
                structureSet = psum?.StructureSet;
                fractions = DvhExtensions.getTotalFractionsForPlanSum(psum);
                status = "PlanSum";
                
            }
            else
            {
                planSetup = context.PlanSetup;
                selectedPlanningItem = (PlanningItem)planSetup;
                structureSet = planSetup?.StructureSet;
                if (planSetup?.UniqueFractionation.NumberOfFractions != null)
                {
                    fractions = (double)planSetup?.UniqueFractionation.NumberOfFractions;
                }
                status = planSetup.ApprovalStatus.ToString();
                
            }
            var dosePerFx = planSetup?.UniqueFractionation.PrescribedDosePerFraction.Dose;
            var rxDose = (double)(dosePerFx * fractions);
            
            //structureSet = planSetup != null ? planSetup.StructureSet : psum.StructureSet;/*psum.PlanSetups.Last().StructureSet;*/ // changed from first to last in case it's broken on next build
            string pId = context.Patient.Id;
			ProcessIdName.getRandomId(pId, out string rId);
            string course = context.Course.Id.ToString().Replace(" ", "_"); ;
            string pName = ProcessIdName.processPtName(context.Patient.Name);

            

            #endregion
            //---------------------------------------------------------------------------------
            #region window definitions

            // Add existing WPF control to the script window.
            var mainControl = new PlanReview.MainControl();
            //mainControl.Window = window;
            //window.WindowStyle = WindowStyle.None;
            window.Content = mainControl;
            window.SizeToContent = SizeToContent.WidthAndHeight;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.Title = "Plan Overview for " + selectedPlanningItem?.Id;
            //mainControl.Title = "DVH Review for " + selectedPlanningItem?.Id;

            #endregion
            //---------------------------------------------------------------------------------
            #region mainControl variable definitions

            mainControl.ps = planSetup;
            mainControl.pitem = selectedPlanningItem;
            mainControl.ss = structureSet;
            mainControl.Fractions = fractions;
            if ((planSetup?.UniqueFractionation.NumberOfFractions == null) && (mainControl.pitem.Dose != null))
            {
                MessageBox.Show("Dose is calculated but Dose/Fraction and Number of Fractions have not been defined.");
                return;
            }
            if (mainControl.pitem.Dose == null)
            {
                MessageBox.Show("Dose is not calculated.");
                return;
            }
            if (context.PlanSetup == null)
            {
                mainControl.absDose_CB.IsEnabled = false;
            }
            mainControl.user = context.CurrentUser.ToString();
            mainControl.day = DateTime.Now.ToLocalTime().Day.ToString();
            mainControl.month = DateTime.Now.ToLocalTime().Month.ToString();
            mainControl.year = DateTime.Now.ToLocalTime().Year.ToString();
            mainControl.hour = DateTime.Now.ToLocalTime().Hour.ToString();
            mainControl.minute = DateTime.Now.ToLocalTime().Minute.ToString();
            mainControl.timeStamp = string.Format("{0}", DateTime.Now.ToLocalTime().ToString());
            mainControl.curredLastName = context.Patient.LastName.Replace(" ", "_");
            mainControl.curredFirstName = context.Patient.FirstName.Replace(" ", "_");
            mainControl.firstInitial = context.Patient.FirstName[0].ToString();
            mainControl.lastInitial = context.Patient.LastName[0].ToString();
            mainControl.initials = mainControl.firstInitial + mainControl.lastInitial;
            mainControl.id = pId;
            //mainControl.idAsDouble = Convert.ToDouble(mainControl.id);
			mainControl.randomId = rId;
            mainControl.courseName = context.Course.Id.ToString().Replace(" ", "_");
            mainControl.courseHeader = course.Split('-').Last().Replace(" ", "_");
            mainControl.planName = selectedPlanningItem.Id.ToString().Replace(" ", "_");
            mainControl.planMaxDose = (double)selectedPlanningItem?.Dose.DoseMax3D.Dose;
            mainControl.dosePerFraction = (double)dosePerFx;
            mainControl.approvalStatus = status;

            // primary physician
            string tempPhysicianId = context.Patient.PrimaryOncologistId;
            PrimaryPhysician PrimaryPhysician = new PrimaryPhysician();
            PrimaryPhysician.Name = GetPrimary.Physician(tempPhysicianId);
            mainControl.primaryPhysician = PrimaryPhysician.Name.ToString();

            // ui textblocks
            mainControl.PrimaryOnc.Text = mainControl.primaryPhysician;
            mainControl.PatientId.Text = pId;
            mainControl.PatientName.Text = pName;
            mainControl.PlanId.Text = mainControl.planName;
            mainControl.PlanStatus.Text = status;

            // isGrady -- they don't have direct access to S Drive (to write files)
            var is_grady = MessageBox.Show("Are you accessing this script from the Grady Campus?", "Direct $S Drive Access", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (is_grady == MessageBoxResult.Yes)
            {
                mainControl.isGrady = true;
            }
            else { mainControl.isGrady = false; }

            #endregion

            #endregion
            //---------------------------------------------------------------------------------
            #region structure comboboxes
            
            //---------------------------------------------------------------------------------
            #region organize structures into ordered lists
            // lists for structures
           

            GenerateStructureList.cleanAndOrderStructures(structureSet, out mainControl.sorted_gtvList,
                                                                        out mainControl.sorted_ctvList,
                                                                        out mainControl.sorted_itvList,
                                                                        out mainControl.sorted_ptvList,
                                                                        out mainControl.sorted_targetList,
                                                                        out mainControl.sorted_oarList,
                                                                        out mainControl.sorted_structureList,
                                                                        out mainControl.sorted_emptyStructuresList);

            //List<Structure> gtvList = new List<Structure>();
            //List<Structure> ctvList = new List<Structure>();
            //List<Structure> itvList = new List<Structure>();
            //List<Structure> ptvList = new List<Structure>();
            //List<Structure> oarList = new List<Structure>();
            //List<Structure> targetList = new List<Structure>();
            //List<Structure> structureList = new List<Structure>();

            //foreach (var structure in structureSet?.Structures)
            //{
            //    // conditions for adding any structure
            //    if ((!structure.IsEmpty) &&
            //        (structure.HasSegment) &&
            //        (!structure.Id.Contains("*")) &&
            //        (!structure.Id.ToLower().Contains("markers")) &&
            //        (!structure.Id.ToLower().Contains("avoid")) &&
            //        (!structure.Id.ToLower().Contains("dose")) &&
            //        (!structure.Id.ToLower().Contains("contrast")) &&
            //        (!structure.Id.ToLower().Contains("air")) &&
            //        (!structure.Id.ToLower().Contains("dens")) &&
            //        (!structure.Id.ToLower().Contains("bolus")) &&
            //        (!structure.Id.ToLower().Contains("suv")) &&
            //        (!structure.Id.ToLower().Contains("match")) &&
            //        (!structure.Id.ToLower().Contains("wire")) &&
            //        (!structure.Id.ToLower().Contains("scar")) &&
            //        (!structure.Id.ToLower().Contains("chemo")) &&
            //        (!structure.Id.ToLower().Contains("pet")) &&
            //        (!structure.Id.ToLower().Contains("dnu")) &&
            //        (!structure.Id.ToLower().Contains("fiducial")) &&
            //        (!structure.Id.ToLower().Contains("artifact")) &&
            //        (!structure.Id.StartsWith("z", StringComparison.InvariantCultureIgnoreCase)) &&
            //        (!structure.Id.StartsWith("hs", StringComparison.InvariantCultureIgnoreCase)) &&
            //        (!structure.Id.StartsWith("av", StringComparison.InvariantCultureIgnoreCase)) &&
            //        (!structure.Id.StartsWith("opti ", StringComparison.InvariantCultureIgnoreCase)) &&
            //        (!structure.Id.StartsWith("opti-", StringComparison.InvariantCultureIgnoreCase)))
            //    //(structure.Id.Contains("CI-", StringComparison.InvariantCultureIgnoreCase) == false) && 
            //    //(structure.Id.Contains("R50-", StringComparison.InvariantCultureIgnoreCase) == false) &&
            //    //(structure.Id.Contains("CI_", StringComparison.InvariantCultureIgnoreCase) == false) && 
            //    //(structure.Id.Contains("R50_", StringComparison.InvariantCultureIgnoreCase) == false))
            //    {
            //        if (structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase))
            //        {
            //            gtvList.Add(structure);
            //            structureList.Add(structure);
            //            targetList.Add(structure);
            //        }
            //        if ((structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) ||
            //            (structure.Id.StartsWith("Prost", StringComparison.InvariantCultureIgnoreCase)))
            //        {
            //            ctvList.Add(structure);
            //            structureList.Add(structure);
            //            targetList.Add(structure);
            //        }
            //        if (structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase))
            //        {
            //            itvList.Add(structure);
            //            structureList.Add(structure);
            //            targetList.Add(structure);
            //        }
            //        if (structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase))
            //        {
            //            ptvList.Add(structure);
            //            structureList.Add(structure);
            //            targetList.Add(structure);
            //        }
            //        // conditions for adding breast plan targets
            //        if ((structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) ||
            //            (structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) ||
            //            (structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) ||
            //            (structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)))
            //        {
            //            targetList.Add(structure);
            //            structureList.Add(structure);
            //        }
            //        // conditions for adding oars
            //        if ((!structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase)) &&
            //            (!structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) &&
            //            (!structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase)) &&
            //            (!structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase)) &&
            //            (!structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) &&
            //            (!structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) &&
            //            (!structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) &&
            //            (!structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)) &&
            //            (!structure.Id.StartsWith("Scar", StringComparison.InvariantCultureIgnoreCase)) &&
            //            (!structure.Id.ToLower().Contains("carina")))
            //        {
            //            oarList.Add(structure);
            //            structureList.Add(structure);
            //        }
            //    }
            //}
            //mainControl.sorted_gtvList = gtvList?.OrderBy(x => x.Id);
            //mainControl.sorted_ctvList = ctvList?.OrderBy(x => x.Id);
            //mainControl.sorted_itvList = itvList?.OrderBy(x => x.Id);
            //mainControl.sorted_ptvList = ptvList?.OrderBy(x => x.Id);
            //mainControl.sorted_targetList = targetList?.OrderBy(x => x.Id);
            //mainControl.sorted_oarList = oarList?.OrderBy(x => x.Id);
            //mainControl.sorted_structureList = structureList?.OrderBy(x => x.Id);

            #endregion structure organization and ordering
            //---------------------------------------------------------------------------------
            #region populate comboboxes

            // populate comboboxes with structures on startup
            if (mainControl.sorted_structureList != null) { foreach (Structure s in mainControl.sorted_structureList) { mainControl.Structure_ComboBox.Items.Add(s); } }
            if (mainControl.sorted_structureList != null) { foreach (Structure s in mainControl.sorted_oarList) { mainControl.ConstraintStructure_ComboBox.Items.Add(s); } }
            if (mainControl.sorted_targetList != null) { foreach (Structure t in mainControl.sorted_targetList) { mainControl.RatioTarget_ComboBox.Items.Add(t); } }
            if (mainControl.sorted_structureList != null)
            {
                foreach (Structure s in mainControl.sorted_oarList)
                {
                    if ((s.Id.StartsWith("ci", StringComparison.InvariantCultureIgnoreCase)) ||
                    (s.Id.StartsWith("r50", StringComparison.InvariantCultureIgnoreCase)) ||
                    (s.Id.StartsWith("body", StringComparison.InvariantCultureIgnoreCase)) ||
                    (s.Id.StartsWith("external", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        mainControl.RatioStructure_ComboBox.Items.Add(s);
                    }
                }
            }
            
            // fill constraint type combobox
            var constraintTypes = new List<string>();
            constraintTypes.Add("Absolute Volume");
            constraintTypes.Add("Relative Volume");
            constraintTypes.Add("Max");
            constraintTypes.Add("Mean");
            foreach (var constraint in constraintTypes) { mainControl.ConstraintType_ComboBox.Items.Add(constraint); }
            mainControl.constraintType.AbsoluteVolume = false;
            mainControl.constraintType.RelativeVolume = false;
            mainControl.constraintType.MaxDose = false;
            mainControl.constraintType.MeanDose = false;
            
            // fill conformity type combobox
            var conformityTypes = new List<string>();
            conformityTypes.Add("CI");
            conformityTypes.Add("R50");
            foreach (var conformity in conformityTypes) { mainControl.RatioType_ComboBox.Items.Add(conformity); }
            mainControl.conformityType.CI = false;
            mainControl.conformityType.R50 = false;

            

            #endregion
            //---------------------------------------------------------------------------------

            #endregion
            //---------------------------------------------------------------------------------
            #region data populated on startup

            //---------------------------------------------------------------------------------
            #region dose and volume unit variables

            // variables used to determine dynamic dvhdata
            bool doseAbsolute = mainControl.absDose_CB.IsChecked.Value;
            bool volAbsolute = mainControl.absVolume_CB.IsChecked.Value;
            DoseValuePresentation dosePres = doseAbsolute ? DoseValuePresentation.Absolute : DoseValuePresentation.Relative;
            VolumePresentation volPres = volAbsolute ? VolumePresentation.AbsoluteCm3 : VolumePresentation.Relative;
            double binWidth = dosePerFx < 2100 ? 0.001 : 0.1;
            mainControl.mainbinWidth = binWidth;

            #endregion
            //---------------------------------------------------------------------------------
            

            #region data to be calculated for oars

            if (mainControl.sorted_oarList != null)
            {
                //---------------------------------------------------------------------------------
                #region general info for each oar -- structureinfo data grid
                //string message = "";
                foreach (Structure s in mainControl.sorted_oarList)
                {
                    if ((!s.Id.StartsWith("ci", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        //DVHData dvhAA = planSetup.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, binWidth);
                        StructureStats sInfo = new StructureStats();
                        sInfo.id = s.Id.ToString().Split(':').First();
                        if (mainControl.pitem.Dose != null)
                        {
                            DVHData dynamicDvh = mainControl.pitem.GetDVHCumulativeData(s, dosePres, volPres, binWidth);
                            sInfo.max03 = Math.Round(DoseChecks.getDoseAtVolume(dynamicDvh, 0.03), 3);
                            sInfo.max = Math.Round(dynamicDvh.MaxDose.Dose, 3);
                            sInfo.mean = Math.Round(dynamicDvh.MeanDose.Dose, 3);
                            
                        }
                        sInfo.volume = Math.Round(s.Volume, 3);
                        
                        //sInfo.color = s.Color.ToString();

                        mainControl.StructureInfo_DG.Items.Add(sInfo);
                        
                        // draw dvh
                        //if (mainControl.pitem.Dose != null)
                        //{
                        //    DVHData dvhData = selectedPlanningItem?.GetDVHCumulativeData(s,
                        //                    DoseValuePresentation.Absolute,
                        //                    VolumePresentation.Relative, 0.1);
                        //    //mainControl.DrawDVH(dvhData, s);
                        //}
                    }
                }
                #endregion
                //---------------------------------------------------------------------------------
                #region dose constraints -- constraints data grid

                if (mainControl.pitem.Dose != null)
                { 
                    // used to prepopulate the constraints data grid in the main control on startup
                    foreach (Structure s in mainControl.sorted_oarList)
                    {
                        #region DvhData
                        
                        DVHData dvhAR = mainControl.pitem.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, binWidth);
                        DVHData dvhAA = mainControl.pitem.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, binWidth);
                        bool doseUnitIsGy = dvhAA.MaxDose.Unit == DoseValue.DoseUnit.Gy ? true : false;
                        
                        //example:
                        //double limit = doseUnitIsGy ? 40 : 4000;

                        #endregion

                        #region various labels/variables used

                        string doseLabel = "Gy";
                        string ccLabel = "cc";
                        string pctLabel = "%";
                        //double d03cc = 0;
                        double dMax = 0;
                        string sToString = s.Id.ToString().Split(':').First();

                        #endregion

                        #region dose and volume constants

                        //-----------------------------------------------------------------------------------------------
                        #region volume limits

                        double vLimit0_2 = 0.2;
                        double vLimit0_35 = 0.35;
                        double vLimit0_5 = 0.5;
                        double vLimit1 = 1;
                        double vLimit1_2 = 1.2;
                        double vLimit3 = 3;
                        double vLimit5 = 5;
                        double vLimit9 = 9;
                        double vLimit10 = 10;
                        double vLimit15 = 15;
                        double vLimit17 = 17;
                        double vLimit20 = 20;
                        double vLimit25 = 25;
                        double vLimit30 = 30;
                        double vLimit33 = 33;
                        double vLimit35 = 35;
                        double vLimit37_5 = 37.5;
                        double vLimit50 = 50;
                        double vLimit55 = 55;
                        double vLimit65 = 65;
                        //double vLimit67 = 67;
                        double vLimit70 = 70;
                        double vLimit100 = 100;
                        double vLimit150 = 150;
                        double vLimit195 = 195;
                        double vLimit200 = 200;

                        #endregion
                        //-----------------------------------------------------------------------------------------------
                        #region dose limits

                        double dLimit4 = doseUnitIsGy ? 4 : 400;
                        double dLimit5 = doseUnitIsGy ? 5 : 500;
                        double dLimit7 = doseUnitIsGy ? 7 : 700;
                        double dLimit8 = doseUnitIsGy ? 8 : 800;
                        double dLimit8_4 = doseUnitIsGy ? 8.4 : 840;
                        double dLimit9 = doseUnitIsGy ? 9 : 900;
                        double dLimit10 = doseUnitIsGy ? 10 : 1000;
                        double dLimit11_2 = doseUnitIsGy ? 11.2 : 1120;
                        double dLimit11_4 = doseUnitIsGy ? 11.4 : 1140;
                        double dLimit12 = doseUnitIsGy ? 12 : 1200;
                        double dLimit12_3 = doseUnitIsGy ? 12.3 : 1230;
                        double dLimit12_4 = doseUnitIsGy ? 12.4 : 1240;
                        double dLimit12_5 = doseUnitIsGy ? 12.5 : 1250;
                        double dLimit14 = doseUnitIsGy ? 14 : 1400;
                        double dLimit14_3 = doseUnitIsGy ? 14.3 : 1430;
                        double dLimit14_5 = doseUnitIsGy ? 14.5 : 1450;
                        double dLimit15 = doseUnitIsGy ? 15 : 1500;
                        double dLimit15_3 = doseUnitIsGy ? 15.3 : 1530;
                        double dLimit16 = doseUnitIsGy ? 16 : 1600;
                        double dLimit16_5 = doseUnitIsGy ? 16.5 : 1650;
                        double dLimit17_1 = doseUnitIsGy ? 17.1 : 1710;
                        double dLimit17_4 = doseUnitIsGy ? 17.4 : 1740;
                        double dLimit17_5 = doseUnitIsGy ? 17.5 : 1750;
                        double dLimit18 = doseUnitIsGy ? 18 : 1800;
                        double dLimit18_4 = doseUnitIsGy ? 18.4 : 1840;
                        double dLimit20 = doseUnitIsGy ? 20 : 2000;
                        double dLimit20_4 = doseUnitIsGy ? 20.4 : 2040;
                        double dLimit21_9 = doseUnitIsGy ? 21.9 : 2190;
                        double dLimit22 = doseUnitIsGy ? 22 : 2200;
                        double dLimit22_2 = doseUnitIsGy ? 22.2 : 2220;
                        double dLimit23 = doseUnitIsGy ? 23 : 2300;
                        double dLimit23_1 = doseUnitIsGy ? 23.1 : 2310;
                        double dLimit24 = doseUnitIsGy ? 24 : 2400;
                        double dLimit25 = doseUnitIsGy ? 25 : 2500;
                        double dLimit26 = doseUnitIsGy ? 26 : 2600;
                        double dLimit27 = doseUnitIsGy ? 27 : 2700;
                        double dLimit28 = doseUnitIsGy ? 28 : 2800;
                        double dLimit28_2 = doseUnitIsGy ? 28.2 : 2820;
                        double dLimit30 = doseUnitIsGy ? 30 : 3000;
                        double dLimit30_5 = doseUnitIsGy ? 30.5 : 3050;
                        double dLimit31 = doseUnitIsGy ? 31 : 3100;
                        double dLimit32 = doseUnitIsGy ? 32 : 3200;
                        double dLimit33 = doseUnitIsGy ? 33 : 3300;
                        double dLimit34 = doseUnitIsGy ? 34 : 3400;
                        double dLimit35 = doseUnitIsGy ? 35 : 3500;
                        double dLimit36 = doseUnitIsGy ? 36 : 3600;
                        double dLimit36_5 = doseUnitIsGy ? 36.5 : 3650;
                        double dLimit38 = doseUnitIsGy ? 38 : 3800;
                        //double dLimit39 = doseUnitIsGy ? 39 : 3900;
                        double dLimit39_5 = doseUnitIsGy ? 39.5 : 3950;
                        double dLimit40 = doseUnitIsGy ? 40 : 4000;
                        double dLimit41 = doseUnitIsGy ? 41 : 4100;
                        double dLimit42 = doseUnitIsGy ? 42 : 4200;
                        double dLimit44 = doseUnitIsGy ? 44 : 4400;
                        double dLimit45 = doseUnitIsGy ? 45 : 4500;
                        double dLimit50 = doseUnitIsGy ? 50 : 5000;
                        double dLimit52 = doseUnitIsGy ? 52 : 5200;
                        double dLimit54 = doseUnitIsGy ? 54 : 5400;
                        double dLimit55 = doseUnitIsGy ? 55 : 5500;
                        double dLimit60 = doseUnitIsGy ? 60 : 6000;
                        double dLimit65 = doseUnitIsGy ? 65 : 6500;
                        double dLimit66 = doseUnitIsGy ? 66 : 6600;
                        double dLimit70 = doseUnitIsGy ? 70 : 7000;
                        double dLimit75 = doseUnitIsGy ? 75 : 7500;

                        #endregion
                        //-----------------------------------------------------------------------------------------------

                        #endregion

                        #region structure specific constraints

                        // bladder (not including bladder-ctv)
                        if ((s.Id.ToLower().Contains("bladder")) &&
                            (!s.Id.ToLower().Contains("ctv")))
                        {
                            if (fractions > 9)
                            {
                                if (PrimaryPhysician.Name == "Dr. Jani")
                                {
                                    VolumeDoseConstraint Bladder40RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit50, dLimit40, pctLabel);
                                    VolumeDoseConstraint Bladder65RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit25, dLimit65, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(Bladder40RV);
                                    mainControl.Constraints_DG.Items.Add(Bladder65RV);
                                }
                                else
                                {
                                    VolumeDoseConstraint bladder35RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit50, dLimit35, pctLabel);
                                    VolumeDoseConstraint bladder40RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit35, dLimit40, pctLabel);
                                    VolumeDoseConstraint bladder50RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit5, dLimit50, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(bladder35RV);
                                    mainControl.Constraints_DG.Items.Add(bladder40RV);
                                    mainControl.Constraints_DG.Items.Add(bladder50RV);
                                }
                            }
                        }
                        // bladder minus ctv
                        if ((s.Id.ToLower().Contains("bladder")) &&
                            (s.Id.ToLower().Contains("ctv")))
                        {
                            if (fractions > 9)
                            {
                                VolumeDoseConstraint bladderMinusCTV40RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit70, dLimit40, pctLabel);
                                VolumeDoseConstraint bladderMinusCTV65RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit50, dLimit65, pctLabel);
                                mainControl.Constraints_DG.Items.Add(bladderMinusCTV40RV);
                                mainControl.Constraints_DG.Items.Add(bladderMinusCTV65RV);
                            }
                        }
                        // brachial plexus
                        if (s.Id.ToLower().Contains("plexus"))
                        {
                            if (fractions > 6)
                            {
                                MaxDoseConstraint BrachialPlexus66M = new MaxDoseConstraint(sToString, dvhAA, dLimit66, doseLabel);
                                mainControl.Constraints_DG.Items.Add(BrachialPlexus66M);

                                VolumeDoseConstraint BrachialPlexus60RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit5, dLimit60, pctLabel);
                                mainControl.Constraints_DG.Items.Add(BrachialPlexus60RV);
                            }
                            if (fractions < 3)
                            {
                                VolumeDoseConstraint v3cc_1Fx = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit3, dLimit14, ccLabel);
                                mainControl.Constraints_DG.Items.Add(v3cc_1Fx);

                                MaxDoseConstraint max1Fx = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit17_5, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max1Fx);
                            }
                            if (fractions == 3)
                            {
                                VolumeDoseConstraint v3cc_3Fx = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit15, dLimit20_4, ccLabel);
                                mainControl.Constraints_DG.Items.Add(v3cc_3Fx);

                                MaxDoseConstraint max3Fx = new MaxDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, dLimit24, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max3Fx);
                            }
                            if ((fractions > 3) && (fractions < 7))
                            {
                                VolumeDoseConstraint v3cc_5Fx = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit15, dLimit27, ccLabel);
                                mainControl.Constraints_DG.Items.Add(v3cc_5Fx);

                                MaxDoseConstraint max5Fx = new MaxDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, dLimit30_5, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max5Fx);
                            }
                        }
                        // bowel
                        if (s.Id.ToLower().Contains("bowel") &&
                            (!s.Id.ToLower().Contains("small")) &&
                            (!s.Id.ToLower().Contains("large")))
                        {
                            if (fractions > 9)
                            {
                                VolumeDoseConstraint bowelAV = new VolumeDoseConstraint(sToString, dvhAA, vLimit195, dLimit45, ccLabel);
                                mainControl.Constraints_DG.Items.Add(bowelAV);
                            }
                        }
                        // bowel -- small
                        if (s.Id.ToLower().Contains("bowel") &&
                           (s.Id.ToLower().Contains("small")) &&
                           (!s.Id.ToLower().Contains("large")))
                        {
                            if (fractions > 9)
                            {
                                if ((PrimaryPhysician.Name == "Dr. Esiashvili") && (fractions <= 20))
                                {
                                    VolumeDoseConstraint sb45RV = new VolumeDoseConstraint(sToString, dvhAA, vLimit50, dLimit45, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(sb45RV);
                                }
                                else
                                {
                                    VolumeDoseConstraint sb30AV = new VolumeDoseConstraint(sToString, dvhAA, vLimit200, dLimit30, ccLabel);
                                    VolumeDoseConstraint sb35AV = new VolumeDoseConstraint(sToString, dvhAA, vLimit150, dLimit35, ccLabel);
                                    VolumeDoseConstraint sb45AV = new VolumeDoseConstraint(sToString, dvhAA, vLimit20, dLimit45, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(sb30AV);
                                    mainControl.Constraints_DG.Items.Add(sb35AV);
                                    mainControl.Constraints_DG.Items.Add(sb45AV);

                                    MaxDoseConstraint sb50M = new MaxDoseConstraint(sToString, dvhAA, dLimit50, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(sb50M);

                                    if (dvhAA.MaxDose.Dose > 50)
                                    {
                                        MaxDoseConstraint sb55M = new MaxDoseConstraint(sToString, dvhAA, dLimit55, doseLabel);
                                        mainControl.Constraints_DG.Items.Add(sb55M);
                                    }
                                }
                            }
                        }
                        // bowel -- large
                        if (s.Id.ToLower().Contains("bowel") &&
                           (!s.Id.ToLower().Contains("small")) &&
                           (s.Id.ToLower().Contains("large")))
                        {
                            if (fractions > 9)
                            {
                                VolumeDoseConstraint lb30AV = new VolumeDoseConstraint(sToString, dvhAA, vLimit200, dLimit30, ccLabel);
                                VolumeDoseConstraint lb35AV = new VolumeDoseConstraint(sToString, dvhAA, vLimit150, dLimit35, ccLabel);
                                VolumeDoseConstraint lb45AV = new VolumeDoseConstraint(sToString, dvhAA, vLimit20, dLimit45, ccLabel);
                                mainControl.Constraints_DG.Items.Add(lb30AV);
                                mainControl.Constraints_DG.Items.Add(lb35AV);
                                mainControl.Constraints_DG.Items.Add(lb45AV);

                                MaxDoseConstraint lb50M = new MaxDoseConstraint(sToString, dvhAA, dLimit50, doseLabel);
                                mainControl.Constraints_DG.Items.Add(lb50M);

                                if (dvhAA.MaxDose.Dose > 50)
                                {
                                    MaxDoseConstraint lb55M = new MaxDoseConstraint(sToString, dvhAA, dLimit55, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(lb55M);
                                }
                            }
                        }
                        // brain
                        if (s.Id.ToLower().Equals("brain") && (fractions < 4))
                        {
                            VolumeDoseConstraint brainAV = new VolumeDoseConstraint(sToString, dvhAA, vLimit10, dLimit10, ccLabel);
                            mainControl.Constraints_DG.Items.Add(brainAV);
                        }
                        // brainstem
                        if (s.Id.ToLower().Equals("brainstem"))
                        {
                            dMax = dvhAA.MaxDose.Dose;
                            // NOTE: for plan sum
                            if (context.PlanSetup == null)
                            {
                                
                                if (fractions > 9)
                                {
                                    
                                    if (dosePerFx == 2.65)
                                    {
                                        MaxDoseConstraint bs40M = new MaxDoseConstraint(sToString, dvhAA, dLimit40, doseLabel);
                                        mainControl.Constraints_DG.Items.Add(bs40M);
                                    }
                                    else
                                    {
                                        if (dMax <= dLimit54)
                                        {
                                            // for hn plans the ideal BS max is 25 Gy
                                            if (PrimaryPhysician.Name == "Dr. Higgins" || PrimaryPhysician.Name == "Dr. Beitler")
                                            {
                                                MaxDoseConstraint bs25M = new MaxDoseConstraint(sToString, dvhAA, dLimit25, doseLabel);
                                                mainControl.Constraints_DG.Items.Add(bs25M);
                                            }
                                            else
                                            {
                                                MaxDoseConstraint bs54M = new MaxDoseConstraint(sToString, dvhAA, dLimit54, doseLabel);
                                                if (mainControl.Constraints_DG.Items.IndexOf(bs54M) < 0)
                                                    mainControl.Constraints_DG.Items.Add(bs54M);
                                            }
                                        }
                                        else
                                        {
                                            MaxDoseConstraint bs54M = new MaxDoseConstraint(sToString, dvhAA, dLimit54, doseLabel);
                                            MaxDoseConstraint bs60M = new MaxDoseConstraint(sToString, dvhAA, dLimit60, doseLabel);
                                            mainControl.Constraints_DG.Items.Add(bs54M);
                                            mainControl.Constraints_DG.Items.Add(bs60M);

                                            VolumeDoseConstraint bs54RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit5, dLimit54, pctLabel);
                                            VolumeDoseConstraint bs60RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit10, dLimit60, pctLabel);
                                            mainControl.Constraints_DG.Items.Add(bs54RV);
                                            mainControl.Constraints_DG.Items.Add(bs60RV);
                                        }
                                    }
                                }
                                if (fractions <= 3)
                                {
                                    MaxDoseConstraint bs15M = new MaxDoseConstraint(sToString, dvhAA, dLimit15, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(bs15M);

                                    VolumeDoseConstraint bs10AV = new VolumeDoseConstraint(sToString, dvhAA, vLimit0_5, dLimit10, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(bs10AV);
                                }
                                if ((fractions > 3) && (fractions < 9))
                                {
                                    MaxDoseConstraint bs15M = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit15, doseLabel);
                                    MaxDoseConstraint bs23M = new MaxDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, dLimit23_1, doseLabel);
                                    MaxDoseConstraint bs31M = new MaxDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, dLimit31, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(bs15M);
                                    mainControl.Constraints_DG.Items.Add(bs23M);
                                    mainControl.Constraints_DG.Items.Add(bs31M);

                                    VolumeDoseConstraint bs10AV = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit0_5, dLimit10, ccLabel);
                                    VolumeDoseConstraint bs18AV = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit0_5, dLimit18, ccLabel);
                                    VolumeDoseConstraint bs23AV = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit0_5, dLimit23, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(bs10AV);
                                    mainControl.Constraints_DG.Items.Add(bs18AV);
                                    mainControl.Constraints_DG.Items.Add(bs23AV);
                                }
                            }
                            // NOTE: for single plan
                            else
                            {
                                if (fractions > 9)
                                {
                                    if (dMax <= dLimit54)
                                    {
                                        // for hn plans the ideal BS max is 25 Gy
                                        if (PrimaryPhysician.Name == "Dr. Higgins" || PrimaryPhysician.Name == "Dr. Beitler")
                                        {
                                            MaxDoseConstraint bs25M = new MaxDoseConstraint(sToString, dvhAA, dLimit25, doseLabel);
                                            mainControl.Constraints_DG.Items.Add(bs25M);
                                        }
                                        else
                                        {
                                            MaxDoseConstraint bs54M = new MaxDoseConstraint(sToString, dvhAA, dLimit54, doseLabel);
                                            if (mainControl.Constraints_DG.Items.IndexOf(bs54M) < 0)
                                                mainControl.Constraints_DG.Items.Add(bs54M);
                                        }
                                    }
                                    else
                                    {
                                        VolumeDoseConstraint bs54RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit5, dLimit54, pctLabel);
                                        VolumeDoseConstraint bs60RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit10, dLimit60, pctLabel);
                                        mainControl.Constraints_DG.Items.Add(bs54RV);
                                        mainControl.Constraints_DG.Items.Add(bs60RV);

                                        MaxDoseConstraint bs54M = new MaxDoseConstraint(sToString, dvhAA, dLimit54, doseLabel);
                                        MaxDoseConstraint bs60M = new MaxDoseConstraint(sToString, dvhAA, dLimit60, doseLabel);
                                        mainControl.Constraints_DG.Items.Add(bs54M);
                                        mainControl.Constraints_DG.Items.Add(bs60M);
                                    }
                                }
                                if (fractions < 3)
                                {
                                    VolumeDoseConstraint bs10AV = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit0_5, dLimit10, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(bs10AV);

                                    MaxDoseConstraint bs15M = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit15, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(bs15M);
                                }
                                if (fractions == 3)
                                {
                                    VolumeDoseConstraint bs18AV = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit0_5, dLimit18, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(bs18AV);

                                    MaxDoseConstraint bs23M = new MaxDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, dLimit23_1, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(bs23M);
                                }
                                if ((fractions > 3) && (fractions < 9))
                                {
                                    VolumeDoseConstraint bs23AV = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit0_5, dLimit23, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(bs23AV);

                                    MaxDoseConstraint bs31M = new MaxDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, dLimit31, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(bs31M);
                                }
                            }
                        }
                        // chiasm and optic nerves
                        if (s.Id.ToLower().Contains("chiasm") ||
                            s.Id.ToLower().Contains("opticnerve") ||
                            (s.Id.ToLower().Contains("optic nerve")))
                        {
                            dMax = dvhAA.MaxDose.Dose;
                            // NOTE: for plan sum
                            if (context.PlanSetup == null)
                            {
                                if (fractions > 9)
                                {
                                    if (dosePerFx == 2.65)
                                    {
                                        MaxDoseConstraint max40 = new MaxDoseConstraint(sToString, dvhAA, dLimit40, doseLabel);
                                        mainControl.Constraints_DG.Items.Add(max40);
                                    }
                                    else
                                    {
                                        if (dMax <= dLimit54)
                                        {
                                            MaxDoseConstraint max54 = new MaxDoseConstraint(sToString, dvhAA, dLimit54, doseLabel);
                                            mainControl.Constraints_DG.Items.Add(max54);
                                        }
                                        else
                                        {
                                            MaxDoseConstraint max54 = new MaxDoseConstraint(sToString, dvhAA, dLimit54, doseLabel);
                                            MaxDoseConstraint max55 = new MaxDoseConstraint(sToString, dvhAA, dLimit55, doseLabel);
                                            mainControl.Constraints_DG.Items.Add(max54);
                                            mainControl.Constraints_DG.Items.Add(max55);
                                        }
                                    }
                                }
                                if (fractions <= 3)
                                {
                                    VolumeDoseConstraint av8 = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit0_5, dLimit8, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(av8);

                                    MaxDoseConstraint max10 = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit10, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(max10);
                                }
                                if ((fractions > 3) && (fractions < 9))
                                {
                                    VolumeDoseConstraint av1Fx = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit0_5, dLimit8, ccLabel);
                                    VolumeDoseConstraint av3Fx = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit0_2, dLimit15_3, ccLabel);
                                    VolumeDoseConstraint av5Fx = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit0_2, dLimit23, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(av1Fx);
                                    mainControl.Constraints_DG.Items.Add(av3Fx);
                                    mainControl.Constraints_DG.Items.Add(av5Fx);

                                    MaxDoseConstraint max1Fx = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit10, doseLabel);
                                    MaxDoseConstraint max3Fx = new MaxDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, dLimit17_4, doseLabel);
                                    MaxDoseConstraint max5Fx = new MaxDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, dLimit25, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(max1Fx);
                                    mainControl.Constraints_DG.Items.Add(max3Fx);
                                    mainControl.Constraints_DG.Items.Add(max5Fx);
                                }
                            }
                            // NOTE: for single plan
                            else
                            {
                                if (fractions > 9)
                                {
                                    if (dMax <= dLimit54)
                                    {
                                        MaxDoseConstraint max54 = new MaxDoseConstraint(sToString, dvhAA, dLimit54, doseLabel);
                                        mainControl.Constraints_DG.Items.Add(max54);
                                    }
                                    else if ((dMax > dLimit54) && (PrimaryPhysician.Name == "Dr. Shu"))
                                    {
                                        MaxDoseConstraint max55 = new MaxDoseConstraint(sToString, dvhAA, dLimit55, doseLabel);
                                        mainControl.Constraints_DG.Items.Add(max55);
                                    }
                                    else
                                    {
                                        MaxDoseConstraint max54 = new MaxDoseConstraint(sToString, dvhAA, dLimit54, doseLabel);
                                        MaxDoseConstraint max55 = new MaxDoseConstraint(sToString, dvhAA, dLimit55, doseLabel);
                                        mainControl.Constraints_DG.Items.Add(max54);
                                        mainControl.Constraints_DG.Items.Add(max55);
                                    }
                                }
                                if (fractions < 3)
                                {
                                    VolumeDoseConstraint av8 = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit0_5, dLimit8, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(av8);

                                    MaxDoseConstraint max10 = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit10, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(max10);
                                }
                                if (fractions == 3)
                                {
                                    VolumeDoseConstraint av3Fx = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit0_2, dLimit15_3, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(av3Fx);

                                    MaxDoseConstraint max3Fx = new MaxDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, dLimit17_4, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(max3Fx);
                                }
                                if ((fractions > 3) && (fractions < 9))
                                {
                                    VolumeDoseConstraint av5Fx = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit0_2, dLimit23, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(av5Fx);

                                    MaxDoseConstraint max5Fx = new MaxDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, dLimit25, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(max5Fx);
                                }
                            }
                        }
                        // cochleae
                        if (s.Id.ToLower().Contains("cochlea"))
                        {
                            // NOTE: for plan sum
                            if (context.PlanSetup == null)
                            {
                                if (fractions > 9)
                                {
                                    MeanDoseConstraint cochlea45Me = new MeanDoseConstraint(sToString, dvhAA, dLimit45, doseLabel, string.Empty);
                                    mainControl.Constraints_DG.Items.Add(cochlea45Me);

                                    VolumeDoseConstraint cochlea45RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit50, dLimit45, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(cochlea45RV);
                                }
                                if (fractions < 3)
                                {
                                    MaxDoseConstraint max1Fx = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit9, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(max1Fx);
                                }
                                if ((fractions > 3) && (fractions < 9))
                                {
                                    MaxDoseConstraint max1Fx = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit9, doseLabel);
                                    MaxDoseConstraint max3Fx = new MaxDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, dLimit17_1, doseLabel);
                                    MaxDoseConstraint max5Fx = new MaxDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, dLimit25, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(max1Fx);
                                    mainControl.Constraints_DG.Items.Add(max3Fx);
                                    mainControl.Constraints_DG.Items.Add(max5Fx);
                                }
                            }
                            // NOTE: for single plan
                            else
                            {
                                if (fractions > 9)
                                {
                                    MeanDoseConstraint cochlea45Me = new MeanDoseConstraint(sToString, dvhAA, dLimit45, doseLabel, string.Empty);
                                    mainControl.Constraints_DG.Items.Add(cochlea45Me);

                                    if (PrimaryPhysician.Name == "Dr. Higgins")
                                    {
                                        VolumeDoseConstraint cochleaL40RV = new VolumeDoseConstraint(sToString, dvhAA, vLimit100, dLimit40, pctLabel);
                                        mainControl.Constraints_DG.Items.Add(cochleaL40RV);
                                    }

                                    VolumeDoseConstraint cochlea45RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit50, dLimit45, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(cochlea45RV);
                                }
                                if (fractions <= 3)
                                {
                                    MaxDoseConstraint max1Fx = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit9, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(max1Fx);
                                }
                                if (fractions == 3)
                                {
                                    MaxDoseConstraint max3Fx = new MaxDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, dLimit17_1, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(max3Fx);
                                }
                                if ((fractions > 3) && (fractions < 9))
                                {
                                    MaxDoseConstraint max5Fx = new MaxDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, dLimit25, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(max5Fx);
                                }
                            }
                        }
                        // duodenum
                        if (s.Id.ToLower().Contains("duodenum"))
                        {
                            if (fractions > 9)
                            {
                                VolumeDoseConstraint duo45RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit33, dLimit45, pctLabel);
                                mainControl.Constraints_DG.Items.Add(duo45RV);

                                MaxDoseConstraint duo60M = new MaxDoseConstraint(sToString, dvhAA, dLimit60, doseLabel);
                                mainControl.Constraints_DG.Items.Add(duo60M);
                            }
                            if (fractions < 3)
                            {
                                VolumeDoseConstraint v10cc_1Fx = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit10, dLimit9, ccLabel);
                                VolumeDoseConstraint v5cc_1Fx = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit5, dLimit11_2, ccLabel);
                                mainControl.Constraints_DG.Items.Add(v10cc_1Fx);
                                mainControl.Constraints_DG.Items.Add(v5cc_1Fx);

                                MaxDoseConstraint max1Fx = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit12_4, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max1Fx);
                            }
                            if (fractions == 3)
                            {
                                VolumeDoseConstraint v10cc_3Fx = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit10, dLimit11_4, ccLabel);
                                VolumeDoseConstraint v5cc_3Fx = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit5, dLimit16_5, ccLabel);
                                mainControl.Constraints_DG.Items.Add(v10cc_3Fx);
                                mainControl.Constraints_DG.Items.Add(v5cc_3Fx);

                                MaxDoseConstraint max3Fx = new MaxDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, dLimit22_2, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max3Fx);
                            }
                            if ((fractions > 3) && (fractions < 9))
                            {
                                VolumeDoseConstraint v10cc_5Fx = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit10, dLimit12_5, ccLabel);
                                VolumeDoseConstraint v5cc_5Fx = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit5, dLimit18, ccLabel);
                                mainControl.Constraints_DG.Items.Add(v10cc_5Fx);
                                mainControl.Constraints_DG.Items.Add(v5cc_5Fx);

                                MaxDoseConstraint max5Fx = new MaxDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, dLimit32, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max5Fx);
                            }
                        }
                        // duodenum, stomach, and bowel prv -- patel
                        if (((s.Id.ToLower().Contains("duo")) && (s.Id.ToLower().Contains("prv"))) ||
                            ((s.Id.ToLower().Contains("stom")) && (s.Id.ToLower().Contains("prv"))) ||
                            ((s.Id.ToLower().Contains("bow")) && (s.Id.ToLower().Contains("prv"))))
                        {
                            if (fractions > 9)
                            {
                                //VolumeDoseConstraint duo45RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit33, dLimit45, pctLabel);
                                //mainControl.Constraints_DG.Items.Add(duo45RV);

                                //MaxDoseConstraint duo60M = new MaxDoseConstraint(sToString, dvhAA, dLimit60, doseLabel);
                                //mainControl.Constraints_DG.Items.Add(duo60M);
                            }
                            if (fractions < 3)
                            {
                                //VolumeDoseConstraint v10cc_1Fx = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit10, dLimit9, ccLabel);
                                //VolumeDoseConstraint v5cc_1Fx = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit5, dLimit11_2, ccLabel);
                                //mainControl.Constraints_DG.Items.Add(v10cc_1Fx);
                                //mainControl.Constraints_DG.Items.Add(v5cc_1Fx);

                                //MaxDoseConstraint max1Fx = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit12_4, doseLabel);
                                //mainControl.Constraints_DG.Items.Add(max1Fx);
                            }
                            if (fractions == 3)
                            {
                                //VolumeDoseConstraint v10cc_3Fx = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit10, dLimit11_4, ccLabel);
                                //VolumeDoseConstraint v5cc_3Fx = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit5, dLimit16_5, ccLabel);
                                //mainControl.Constraints_DG.Items.Add(v10cc_3Fx);
                                //mainControl.Constraints_DG.Items.Add(v5cc_3Fx);

                                //MaxDoseConstraint max3Fx = new MaxDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, dLimit22_2, doseLabel);
                                //mainControl.Constraints_DG.Items.Add(max3Fx);
                            }
                            if ((fractions > 3) && (fractions < 9))
                            {
                                VolumeDoseConstraint d9cc_5Fx = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit9, dLimit15, ccLabel);
                                VolumeDoseConstraint d3cc_5Fx = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit3, dLimit20, ccLabel);
                                VolumeDoseConstraint d1cc_5Fx = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit1, dLimit33, ccLabel);
                                mainControl.Constraints_DG.Items.Add(d9cc_5Fx);
                                mainControl.Constraints_DG.Items.Add(d3cc_5Fx);
                                mainControl.Constraints_DG.Items.Add(d1cc_5Fx);

                                //MaxDoseConstraint max5Fx = new MaxDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, dLimit32, doseLabel);
                                //mainControl.Constraints_DG.Items.Add(max5Fx);
                            }
                        }
                        // esophagus
                        if (s.Id.ToLower().Contains("esophagus"))
                        {
                            if (fractions > 9)
                            {
                                MeanDoseConstraint Esophagus34Me = new MeanDoseConstraint(sToString, dvhAA, dLimit34, doseLabel, string.Empty);
                                VolumeDoseConstraint Esophagus40RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit100, dLimit40, pctLabel);
                                mainControl.Constraints_DG.Items.Add(Esophagus34Me);
                                mainControl.Constraints_DG.Items.Add(Esophagus40RV);
                            }
                            if (fractions < 3)
                            {
                                MaxDoseConstraint max1Fx = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit9, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max1Fx);
                            }
                            if (fractions == 3)
                            {
                                MaxDoseConstraint max3Fx = new MaxDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, dLimit17_1, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max3Fx);
                            }
                            if ((fractions > 3) && (fractions < 9))
                            {
                                MaxDoseConstraint max5Fx = new MaxDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, dLimit25, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max5Fx);
                            }
                        }
                        // femoral heads
                        if (s.Id.ToLower().Contains("femoral") &&
                           (s.Id.ToLower().Contains("head")))
                        {
                            if (fractions > 9)
                            {
                                if (PrimaryPhysician.Name == "Dr. Jani")
                                {
                                    VolumeDoseConstraint FemoralHead42RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit10, dLimit42, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(FemoralHead42RV);
                                    if (dvhAA.MaxDose.Dose > 50)
                                    {
                                        VolumeDoseConstraint FemoralHead50RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit10, dLimit50, pctLabel);
                                        mainControl.Constraints_DG.Items.Add(FemoralHead50RV);
                                    }
                                }
                                else
                                {
                                    VolumeDoseConstraint femoralHeads30RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit50, dLimit30, pctLabel);
                                    VolumeDoseConstraint femoralHeads40RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit35, dLimit40, pctLabel);
                                    VolumeDoseConstraint femoralHeads44RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit5, dLimit44, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(femoralHeads30RV);
                                    mainControl.Constraints_DG.Items.Add(femoralHeads40RV);
                                    mainControl.Constraints_DG.Items.Add(femoralHeads44RV);
                                }
                            }
                        }
                        // gentialia
                        if (s.Id.ToLower().Contains("genitalia"))
                        {
                            VolumeDoseConstraint genitalia20RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit50, dLimit20, pctLabel);
                            VolumeDoseConstraint genitalia30RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit35, dLimit30, pctLabel);
                            VolumeDoseConstraint genitalia40RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit5, dLimit40, pctLabel);
                            mainControl.Constraints_DG.Items.Add(genitalia20RV);
                            mainControl.Constraints_DG.Items.Add(genitalia30RV);
                            mainControl.Constraints_DG.Items.Add(genitalia40RV);
                        }
                        // globes
                        if (s.Id.ToLower().Contains("globe"))
                        {
                            dMax = dvhAA.MaxDose.Dose;
                            if (dosePerFx == 2.65)
                            {
                                MaxDoseConstraint max30 = new MaxDoseConstraint(sToString, dvhAA, dLimit30, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max30);
                            }
                            else
                            {
                                if (dMax <= dLimit40)
                                {
                                    MaxDoseConstraint globe40M = new MaxDoseConstraint(sToString, dvhAA, dLimit40, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(globe40M);
                                }
                                else
                                {
                                    MaxDoseConstraint globe40M = new MaxDoseConstraint(sToString, dvhAA, dLimit40, doseLabel);
                                    MaxDoseConstraint globe52M = new MaxDoseConstraint(sToString, dvhAA, dLimit52, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(globe40M);
                                    mainControl.Constraints_DG.Items.Add(globe52M);
                                }
                            }
                        }
                        // glottis
                        if (s.Id.ToLower().Contains("glottis"))
                        {
                            if (fractions > 9)
                            {
                                MeanDoseConstraint Glottis20Me = new MeanDoseConstraint(sToString, dvhAA, dLimit20, doseLabel, string.Empty);
                                mainControl.Constraints_DG.Items.Add(Glottis20Me);
                            }
                        }
                        // heart
                        if (s.Id.ToLower().Contains("heart"))
                        {
                            if (fractions > 6)
                            {
                                if (PrimaryPhysician.Name == "Dr. Torres" || PrimaryPhysician.Name == "Dr. Lin")
                                {
                                    MeanDoseConstraint Heart5Me = new MeanDoseConstraint(sToString, dvhAA, dLimit5, doseLabel, string.Empty);
                                    mainControl.Constraints_DG.Items.Add(Heart5Me);
                                }
                                else
                                {
                                    VolumeDoseConstraint Heart30RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit50, dLimit30, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(Heart30RV);
                                }
                            }
                            if (fractions < 3)
                            {
                                VolumeDoseConstraint v15cc_1Fx = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit15, dLimit16, ccLabel);
                                mainControl.Constraints_DG.Items.Add(v15cc_1Fx);

                                MaxDoseConstraint max1Fx = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit22, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max1Fx);
                            }
                            if (fractions == 3)
                            {
                                VolumeDoseConstraint v15cc_3Fx = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit15, dLimit24, ccLabel);
                                mainControl.Constraints_DG.Items.Add(v15cc_3Fx);

                                MaxDoseConstraint max3Fx = new MaxDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, dLimit30, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max3Fx);
                            }
                            if ((fractions > 3) && (fractions < 7))
                            {
                                VolumeDoseConstraint v15cc_5Fx = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit15, dLimit32, ccLabel);
                                mainControl.Constraints_DG.Items.Add(v15cc_5Fx);

                                MaxDoseConstraint max5Fx = new MaxDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, dLimit38, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max5Fx);
                            }
                        }
                        // hippocampus
                        if (s.Id.ToLower().Contains("hippocampus"))
                        {
                        }
                        // hypothalamus
                        if (s.Id.ToLower().Contains("hypothal"))
                        {
                            if (fractions > 9)
                            {
                                MeanDoseConstraint Hypothalamus20Me = new MeanDoseConstraint(sToString, dvhAA, dLimit20, doseLabel, string.Empty);
                                mainControl.Constraints_DG.Items.Add(Hypothalamus20Me);
                            }
                        }
                        // iliac crests
                        if (s.Id.ToLower().Contains("iliac") ||
                            s.Id.ToLower().Contains("crest"))
                        {
                            if (fractions > 9)
                            {
                                VolumeDoseConstraint iliacCrests30RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit50, dLimit30, pctLabel);
                                VolumeDoseConstraint iliacCrests40RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit35, dLimit40, pctLabel);
                                mainControl.Constraints_DG.Items.Add(iliacCrests30RV);
                                mainControl.Constraints_DG.Items.Add(iliacCrests40RV);
                            }
                        }
                        // kidneys
                        if (s.Id.ToLower().Contains("kidney"))
                        {
                            if (fractions > 9)
                            {
                                if (dosePerFx == 1.5)
                                {
                                    MeanDoseConstraint Kidney15Me = new MeanDoseConstraint(sToString, dvhAA, dLimit15, doseLabel, string.Empty);
                                    mainControl.Constraints_DG.Items.Add(Kidney15Me);
                                }
                                if ((PrimaryPhysician.Name == "Dr. Esiashvili") && (fractions <= 20))
                                {
                                    VolumeDoseConstraint Kidney15RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit100, dLimit15, pctLabel);
                                    VolumeDoseConstraint Kidney18RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit50, dLimit18, pctLabel);
                                    VolumeDoseConstraint Kidney25RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit25, dLimit25, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(Kidney15RV);
                                    mainControl.Constraints_DG.Items.Add(Kidney18RV);
                                    mainControl.Constraints_DG.Items.Add(Kidney25RV);
                                }
                                else
                                {
                                    MeanDoseConstraint Kidney18Me = new MeanDoseConstraint(sToString, dvhAA, dLimit18, doseLabel, string.Empty);
                                    mainControl.Constraints_DG.Items.Add(Kidney18Me);

                                    VolumeDoseConstraint Kidney15RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit100, dLimit15, pctLabel);
                                    VolumeDoseConstraint Kidney20RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit50, dLimit20, pctLabel);
                                    VolumeDoseConstraint Kidney25RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit25, dLimit25, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(Kidney15RV);
                                    mainControl.Constraints_DG.Items.Add(Kidney20RV);
                                    mainControl.Constraints_DG.Items.Add(Kidney25RV);
                                }
                            }
                            if (fractions < 3)
                            {
                                VolumeDoseConstraint d200cc_1Fx = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit200, dLimit8_4, ccLabel);
                                mainControl.Constraints_DG.Items.Add(d200cc_1Fx);
                            }
                            if (fractions == 3)
                            {
                                VolumeDoseConstraint d200cc_3Fx = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit200, dLimit16, ccLabel);
                                mainControl.Constraints_DG.Items.Add(d200cc_3Fx);
                            }
                            if ((fractions > 3) && (fractions < 9))
                            {
                                VolumeDoseConstraint d200cc_5Fx = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit200, dLimit17_5, ccLabel);
                                mainControl.Constraints_DG.Items.Add(d200cc_5Fx);

                                VolumeDoseConstraint v12_5Fx = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit35, dLimit12, pctLabel);
                                mainControl.Constraints_DG.Items.Add(v12_5Fx);
                            }
                        }
                        // lacrimal glands
                        if (s.Id.ToLower().Contains("lacrimal"))
                        {
                            if (fractions > 9)
                            {
                                MeanDoseConstraint lacrimal45Me = new MeanDoseConstraint(sToString, dvhAA, dLimit45, doseLabel, string.Empty);
                                mainControl.Constraints_DG.Items.Add(lacrimal45Me);
                            }
                        }
                        // larynx
                        if (s.Id.ToLower().Contains("larynx"))
                        {
                            if (fractions > 9)
                            {
                                VolumeDoseConstraint Larynx40RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit100, dLimit40, pctLabel);
                                mainControl.Constraints_DG.Items.Add(Larynx40RV);
                            }
                        }
                        // lenses
                        if (s.Id.ToLower().Contains("lens"))
                        {
                            dMax = dvhAA.MaxDose.Dose;
                            if (fractions > 9)
                            {
                                if (dosePerFx == 2.65)
                                {
                                    MaxDoseConstraint max4 = new MaxDoseConstraint(sToString, dvhAA, dLimit4, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(max4);
                                }
                                else
                                {
                                    if (dMax <= dLimit5)
                                    {
                                        MaxDoseConstraint lens5M = new MaxDoseConstraint(sToString, dvhAA, dLimit5, doseLabel);
                                        mainControl.Constraints_DG.Items.Add(lens5M);
                                    }
                                    else
                                    {
                                        MaxDoseConstraint lens5M = new MaxDoseConstraint(sToString, dvhAA, dLimit5, doseLabel);
                                        MaxDoseConstraint lens10M = new MaxDoseConstraint(sToString, dvhAA, dLimit10, doseLabel);
                                        mainControl.Constraints_DG.Items.Add(lens5M);
                                        mainControl.Constraints_DG.Items.Add(lens10M);
                                    }
                                }
                            }
                        }
                        // liver
                        if (s.Id.ToLower().Contains("liver"))
                        {
                            if ((fractions > 9) && (dosePerFx == 1.5))
                            {
                                MeanDoseConstraint liver28Me = new MeanDoseConstraint(sToString, dvhAA, dLimit28, doseLabel, string.Empty);
                                mainControl.Constraints_DG.Items.Add(liver28Me);
                            }
                            else if ((fractions > 9) && (dosePerFx != 1.5))
                            {
                                VolumeDoseConstraint Liver30RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit50, dLimit30, pctLabel);
                                VolumeDoseConstraint Liver45RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit25, dLimit45, pctLabel);
                                mainControl.Constraints_DG.Items.Add(Liver30RV);
                                mainControl.Constraints_DG.Items.Add(Liver45RV);
                            }
                            else if (fractions == 5)
                            {
                                VolumeDoseConstraint Liver12RV = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAR, vLimit50, dLimit12, pctLabel);
                                mainControl.Constraints_DG.Items.Add(Liver12RV);

                                MeanDoseConstraint mean5Fx = new MeanDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, 13, doseLabel, string.Empty);
                                mainControl.Constraints_DG.Items.Add(mean5Fx);
                            }
                        }
                        // lungs
                        if (s.Id.ToLower().Contains("lung"))
                        {
                            double v5 = DoseChecks.getVolumeAtDose(dvhAR, 5);
                            double v20 = DoseChecks.getVolumeAtDose(dvhAR, 20);
                            if (fractions > 9)
                            {
                                if (v5 <= 55)
                                {
                                    VolumeDoseConstraint LungSum5RV_55 = new VolumeDoseConstraint(sToString, dvhAR, vLimit55, dLimit5, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(LungSum5RV_55);
                                }
                                else if ((v5 > 55) && (v5 <= 65))
                                {
                                    VolumeDoseConstraint LungSum5RV_65 = new VolumeDoseConstraint(sToString, dvhAR, vLimit65, dLimit5, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(LungSum5RV_65);
                                }
                                else
                                {
                                    VolumeDoseConstraint LungSum5RV_65 = new VolumeDoseConstraint(sToString, dvhAR, vLimit65, dLimit5, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(LungSum5RV_65);

                                    VolumeDoseConstraint LungSum5RV_70 = new VolumeDoseConstraint(sToString, dvhAR, vLimit70, dLimit5, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(LungSum5RV_70);
                                }
                                if (v20 <= 20)
                                {
                                    VolumeDoseConstraint LungSum20RV_20 = new VolumeDoseConstraint(sToString, dvhAR, vLimit20, dLimit20, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(LungSum20RV_20);
                                }
                                else if ((v20 > 20) && (v20 <= 30))
                                {
                                    VolumeDoseConstraint LungSum20RV_30 = new VolumeDoseConstraint(sToString, dvhAR, vLimit30, dLimit20, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(LungSum20RV_30);
                                }
                                else if ((v20 > 30) && (v20 <= 35))
                                {
                                    VolumeDoseConstraint LungSum20RV_35 = new VolumeDoseConstraint(sToString, dvhAR, vLimit35, dLimit20, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(LungSum20RV_35);
                                }
                                else
                                {
                                    VolumeDoseConstraint LungSum20RV_35 = new VolumeDoseConstraint(sToString, dvhAR, vLimit35, dLimit20, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(LungSum20RV_35);

                                    VolumeDoseConstraint LungSum20RV_37_5 = new VolumeDoseConstraint(sToString, dvhAR, vLimit37_5, dLimit20, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(LungSum20RV_37_5);
                                }
                                MeanDoseConstraint Lung20Me = new MeanDoseConstraint(sToString, dvhAA, dLimit20, doseLabel, string.Empty);
                                mainControl.Constraints_DG.Items.Add(Lung20Me);
                            }
                        }
                        // mandible
                        if (s.Id.ToLower().Contains("mandible"))
                        {
                            dMax = dvhAA.MaxDose.Dose;
                            if (rxDose > 60)
                            {
                                MaxDoseConstraint Mandible70M = new MaxDoseConstraint(sToString, dvhAA, 70, doseLabel);
                                mainControl.Constraints_DG.Items.Add(Mandible70M);
                                if (dMax > 70)
                                {
                                    // mobius
                                    VolumeDoseConstraint mandibleAV75 = new VolumeDoseConstraint(sToString, dvhAA, vLimit1, dLimit75, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(mandibleAV75);
                                }
                            }
                        }
                        // oral cavity
                        if ((s.Id.ToLower().Contains("oral")) &&
                            (s.Id.ToLower().Contains("avity")))
                        {
                            if (fractions > 9)
                            {
                                // mobius
                                MeanDoseConstraint oc40Me = new MeanDoseConstraint(sToString, dvhAA, dLimit40, doseLabel, string.Empty);
                                mainControl.Constraints_DG.Items.Add(oc40Me);
                            }
                        }
                        // parotid glands
                        if (s.Id.ToLower().Contains("parotid"))
                        {
                            if (fractions > 9)
                            {
                                MeanDoseConstraint Parotid26Me = new MeanDoseConstraint(sToString, dvhAA, dLimit26, doseLabel, string.Empty);
                                mainControl.Constraints_DG.Items.Add(Parotid26Me);
                            }
                        }
                        // penile bulb
                        if (s.Id.ToLower().Contains("penile") ||
                            s.Id.ToLower().Contains("bulb"))
                        {
                            if (fractions > 9)
                            {
                                VolumeDoseConstraint pb50RV = new VolumeDoseConstraint(sToString, dvhAA, vLimit50, dLimit50, pctLabel);
                                mainControl.Constraints_DG.Items.Add(pb50RV);

                                MeanDoseConstraint pb50Me = new MeanDoseConstraint(sToString, dvhAA, dLimit50, doseLabel, string.Empty);
                                mainControl.Constraints_DG.Items.Add(pb50Me);
                            }
                        }
                        // pituitary
                        if (s.Id.ToLower().Contains("pituitary"))
                        {
                            if (fractions > 9)
                            {
                                MeanDoseConstraint Pituitary35Me = new MeanDoseConstraint(sToString, dvhAA, dLimit35, doseLabel, string.Empty);
                                mainControl.Constraints_DG.Items.Add(Pituitary35Me);
                            }
                        }
                        // rectum
                        if (s.Id.ToLower().Contains("rectum"))
                        {
                            if (fractions > 9)
                            {
                                if (PrimaryPhysician.Name == "Dr. Jani")
                                {
                                    VolumeDoseConstraint Rectum40RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit35, dLimit40, pctLabel);
                                    VolumeDoseConstraint Rectum65RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit17, dLimit65, pctLabel);
                                    VolumeDoseConstraint Rectum70RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit15, dLimit70, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(Rectum40RV);
                                    mainControl.Constraints_DG.Items.Add(Rectum65RV);
                                    mainControl.Constraints_DG.Items.Add(Rectum70RV);

                                    if (dvhAA.MaxDose.Dose > 70)
                                    {
                                        VolumeDoseConstraint Rectum70AV = new VolumeDoseConstraint(sToString, dvhAR, vLimit10, dLimit70, ccLabel);
                                        mainControl.Constraints_DG.Items.Add(Rectum70AV);
                                    }
                                }
                                else
                                {
                                    VolumeDoseConstraint Rectum60RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit50, dLimit60, pctLabel);
                                    VolumeDoseConstraint Rectum65RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit35, dLimit65, pctLabel);
                                    VolumeDoseConstraint Rectum70RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit25, dLimit70, pctLabel);
                                    VolumeDoseConstraint Rectum75RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit15, dLimit75, pctLabel);
                                    mainControl.Constraints_DG.Items.Add(Rectum60RV);
                                    mainControl.Constraints_DG.Items.Add(Rectum65RV);
                                    mainControl.Constraints_DG.Items.Add(Rectum70RV);
                                    mainControl.Constraints_DG.Items.Add(Rectum75RV);
                                }
                            }
                            if (fractions < 3)
                            {
                                VolumeDoseConstraint av1Fx = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit20, dLimit14_3, ccLabel);
                                mainControl.Constraints_DG.Items.Add(av1Fx);

                                MaxDoseConstraint max1Fx = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit18_4, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max1Fx);
                            }
                            if (fractions == 3)
                            {
                                VolumeDoseConstraint av3Fx = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit20, dLimit24, ccLabel);
                                mainControl.Constraints_DG.Items.Add(av3Fx);

                                MaxDoseConstraint max3Fx = new MaxDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, dLimit28_2, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max3Fx);
                            }
                            if ((fractions > 3) && (fractions < 9))
                            {
                                VolumeDoseConstraint av5Fx = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit20, dLimit25, ccLabel);
                                mainControl.Constraints_DG.Items.Add(av5Fx);

                                MaxDoseConstraint max5Fx = new MaxDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, dLimit38, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max5Fx);
                            }
                        }
                        // skin
                        if (s.Id.ToLower().Contains("skin"))
                        {
                            if (fractions < 3)
                            {
                                VolumeDoseConstraint v10cc_1Fx = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit10, dLimit23, ccLabel);
                                mainControl.Constraints_DG.Items.Add(v10cc_1Fx);

                                MaxDoseConstraint max1Fx = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit26, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max1Fx);
                            }
                            if (fractions == 3)
                            {
                                VolumeDoseConstraint v10cc_3Fx = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit10, dLimit30, ccLabel);
                                mainControl.Constraints_DG.Items.Add(v10cc_3Fx);

                                MaxDoseConstraint max3Fx = new MaxDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, dLimit33, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max3Fx);
                            }
                            if ((fractions > 3) && (fractions < 9))
                            {
                                VolumeDoseConstraint v10cc_5Fx = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit10, dLimit36_5, ccLabel);
                                mainControl.Constraints_DG.Items.Add(v10cc_5Fx);

                                MaxDoseConstraint max5Fx = new MaxDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, dLimit39_5, doseLabel);
                                mainControl.Constraints_DG.Items.Add(max5Fx);
                            }
                        }
                        // spinal cord
                        if ((s.Id.ToLower().Contains("cord")) &&
                            ((!s.Id.ToLower().Contains("5")) &&
                            (!s.Id.ToLower().Contains("prv"))))
                        {
                            // NOTE: for plan sum
                            if (context.PlanSetup == null)
                            {
                                dMax = dvhAA.MaxDose.Dose;
                                if (fractions > 9)
                                {
                                    // bid
                                    if (dosePerFx == 1.5)
                                    {
                                        MaxDoseConstraint bid_cord41M = new MaxDoseConstraint(sToString, dvhAA, dLimit41, doseLabel);
                                        mainControl.Constraints_DG.Items.Add(bid_cord41M);
                                    }
                                    else
                                    {
                                        if (dMax <= dLimit45)
                                        {
                                            MaxDoseConstraint cord45M = new MaxDoseConstraint(sToString, dvhAA, dLimit45, doseLabel);
                                            mainControl.Constraints_DG.Items.Add(cord45M);
                                        }
                                        else if (dMax <= dLimit50)
                                        {
                                            MaxDoseConstraint cord45M = new MaxDoseConstraint(sToString, dvhAA, dLimit45, doseLabel);
                                            MaxDoseConstraint cord50M = new MaxDoseConstraint(sToString, dvhAA, dLimit50, doseLabel);
                                            mainControl.Constraints_DG.Items.Add(cord45M);
                                            mainControl.Constraints_DG.Items.Add(cord50M);
                                        }
                                        else
                                        {
                                            MaxDoseConstraint cord45M = new MaxDoseConstraint(sToString, dvhAA, dLimit45, doseLabel);
                                            MaxDoseConstraint cord50M = new MaxDoseConstraint(sToString, dvhAA, dLimit50, doseLabel);
                                            MaxDoseConstraint cord54M = new MaxDoseConstraint(sToString, dvhAA, dLimit54, doseLabel);
                                            mainControl.Constraints_DG.Items.Add(cord45M);
                                            mainControl.Constraints_DG.Items.Add(cord50M);
                                            mainControl.Constraints_DG.Items.Add(cord54M);
                                        }
                                    }
                                }
                                if (fractions <= 3)
                                {
                                    VolumeDoseConstraint av1Fx_1_2 = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit1_2, dLimit7, ccLabel);
                                    VolumeDoseConstraint av1Fx_0_35 = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit0_35, dLimit10, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(av1Fx_1_2);
                                    mainControl.Constraints_DG.Items.Add(av1Fx_0_35);

                                    MaxDoseConstraint max1Fx = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit14, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(max1Fx);
                                }
                                if ((fractions > 3) && (fractions < 9))
                                {
                                    VolumeDoseConstraint av1Fx_1_2 = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit1_2, dLimit7, ccLabel);
                                    VolumeDoseConstraint av1Fx_0_35 = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit0_35, dLimit10, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(av1Fx_1_2);
                                    mainControl.Constraints_DG.Items.Add(av1Fx_0_35);
                                    VolumeDoseConstraint av3Fx_1_2 = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit1_2, dLimit12_3, ccLabel);
                                    VolumeDoseConstraint av3Fx_0_35 = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit0_35, dLimit18, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(av3Fx_1_2);
                                    mainControl.Constraints_DG.Items.Add(av3Fx_0_35);
                                    VolumeDoseConstraint av5Fx_1_2 = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit1_2, dLimit14_5, ccLabel);
                                    VolumeDoseConstraint av5Fx_0_35 = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit0_35, dLimit23, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(av5Fx_1_2);
                                    mainControl.Constraints_DG.Items.Add(av5Fx_0_35);

                                    MaxDoseConstraint max1Fx = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit14, doseLabel);
                                    MaxDoseConstraint max3Fx = new MaxDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, dLimit21_9, doseLabel);
                                    MaxDoseConstraint max5Fx = new MaxDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, dLimit30, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(max1Fx);
                                    mainControl.Constraints_DG.Items.Add(max3Fx);
                                    mainControl.Constraints_DG.Items.Add(max5Fx);
                                }
                            }
                            // NOTE: for single plan
                            else
                            {
                                dMax = dvhAA.MaxDose.Dose;
                                if (fractions > 6)
                                {
                                    // bid
                                    if (dosePerFx == 1.5)
                                    {
                                        MaxDoseConstraint bid_cord41M = new MaxDoseConstraint(sToString, dvhAA, dLimit41, doseLabel);
                                        mainControl.Constraints_DG.Items.Add(bid_cord41M);
                                    }
                                    else
                                    {
                                        if (dMax <= dLimit45)
                                        {
                                            MaxDoseConstraint cord45M = new MaxDoseConstraint(sToString, dvhAA, dLimit45, doseLabel);
                                            mainControl.Constraints_DG.Items.Add(cord45M);
                                        }
                                        else if (dMax <= dLimit50)
                                        {
                                            MaxDoseConstraint cord45M = new MaxDoseConstraint(sToString, dvhAA, dLimit45, doseLabel);
                                            MaxDoseConstraint cord50M = new MaxDoseConstraint(sToString, dvhAA, dLimit50, doseLabel);
                                            mainControl.Constraints_DG.Items.Add(cord45M);
                                            mainControl.Constraints_DG.Items.Add(cord50M);
                                        }
                                        else
                                        {
                                            MaxDoseConstraint cord45M = new MaxDoseConstraint(sToString, dvhAA, dLimit45, doseLabel);
                                            MaxDoseConstraint cord50M = new MaxDoseConstraint(sToString, dvhAA, dLimit50, doseLabel);
                                            MaxDoseConstraint cord54M = new MaxDoseConstraint(sToString, dvhAA, dLimit54, doseLabel);
                                            mainControl.Constraints_DG.Items.Add(cord45M);
                                            mainControl.Constraints_DG.Items.Add(cord50M);
                                            mainControl.Constraints_DG.Items.Add(cord54M);
                                        }
                                    }
                                }
                                if (fractions < 3)
                                {
                                    VolumeDoseConstraint av1Fx_1_2 = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit1_2, dLimit7, ccLabel);
                                    VolumeDoseConstraint av1Fx_0_35 = new VolumeDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, vLimit0_35, dLimit10, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(av1Fx_1_2);
                                    mainControl.Constraints_DG.Items.Add(av1Fx_0_35);

                                    MaxDoseConstraint max1Fx = new MaxDoseConstraint(string.Format("{0}: 1 Fx", sToString), dvhAA, dLimit14, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(max1Fx);
                                }
                                if (fractions == 3)
                                {
                                    VolumeDoseConstraint av3Fx_1_2 = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit1_2, dLimit12_3, ccLabel);
                                    VolumeDoseConstraint av3Fx_0_35 = new VolumeDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, vLimit0_35, dLimit18, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(av3Fx_1_2);
                                    mainControl.Constraints_DG.Items.Add(av3Fx_0_35);

                                    MaxDoseConstraint max3Fx = new MaxDoseConstraint(string.Format("{0}: 3 Fx", sToString), dvhAA, dLimit21_9, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(max3Fx);
                                }
                                if ((fractions > 3) && (fractions < 7))
                                {
                                    VolumeDoseConstraint av5Fx_1_2 = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit1_2, dLimit14_5, ccLabel);
                                    VolumeDoseConstraint av5Fx_0_35 = new VolumeDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, vLimit0_35, dLimit23, ccLabel);
                                    mainControl.Constraints_DG.Items.Add(av5Fx_1_2);
                                    mainControl.Constraints_DG.Items.Add(av5Fx_0_35);

                                    if (PrimaryPhysician.Name == "Dr. Patel")
                                    {
                                        MaxDoseConstraint patel_max5Fx = new MaxDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, dLimit8, doseLabel);
                                        mainControl.Constraints_DG.Items.Add(patel_max5Fx);
                                    }
                                    MaxDoseConstraint max5Fx = new MaxDoseConstraint(string.Format("{0}: 5 Fx", sToString), dvhAA, dLimit30, doseLabel);
                                    mainControl.Constraints_DG.Items.Add(max5Fx);
                                }
                            }
                        }
                        // spinal cord prv
                        if ((s.Id.ToLower().Contains("cord")) &&
                            ((s.Id.ToLower().Contains("5"))))
                        {
                            if (fractions > 9)
                            {
                                MaxDoseConstraint SpinalCordPRV50M = new MaxDoseConstraint(sToString, dvhAA, dLimit50, doseLabel);
                                mainControl.Constraints_DG.Items.Add(SpinalCordPRV50M);
                            }
                        }
                        // stomach
                        if (s.Id.ToLower().Contains("stomach"))
                        {
                            if ((fractions > 9) && (dosePerFx == 1.5))
                            {
                                MaxDoseConstraint Stomach45M = new MaxDoseConstraint(sToString, dvhAA, dLimit45, doseLabel);
                                mainControl.Constraints_DG.Items.Add(Stomach45M);
                            }
                            else if ((fractions > 9) && (dosePerFx != 1.5))
                            {
                                VolumeDoseConstraint Stomach45RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit15, dLimit45, pctLabel);
                                mainControl.Constraints_DG.Items.Add(Stomach45RV);

                                MaxDoseConstraint Stomach55M = new MaxDoseConstraint(sToString, dvhAA, dLimit55, doseLabel);
                                mainControl.Constraints_DG.Items.Add(Stomach55M);
                            }
                        }
                        // submandibular glands
                        if (s.Id.ToLower().Contains("subman"))
                        {
                            if (fractions > 9)
                            {
                                VolumeDoseConstraint SubmandibularL30RV = new VolumeDoseConstraint(sToString, dvhAR, vLimit50, dLimit30, pctLabel);
                                mainControl.Constraints_DG.Items.Add(SubmandibularL30RV);
                            }
                        }
                        // temporal lobes
                        if (s.Id.ToLower().Contains("temporallobe") ||
                            s.Id.ToLower().Contains("temporal lobe"))
                        {
                        }

                        #endregion

                        #region list to add



                        #endregion
                    }
                }

                #endregion
                //---------------------------------------------------------------------------------
            }

            #endregion
            //---------------------------------------------------------------------------------
            #region data to be calculated for targets

            if (mainControl.sorted_targetList != null)
            {
                #region target coverage stats

                // prepopulates the target coverage data grid in the main control on startup
                foreach (Structure t in mainControl.sorted_targetList)
                {
                    TargetStats tInfo = new TargetStats();
                    tInfo.color = t.Color.ToString();
                    tInfo.targetId = t.Id.ToString().Split(':').First();
                    if (mainControl.pitem.Dose != null)
                    {
                        //DVHData dvhAA = planSetup.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, binWidth);
                        DVHData dynamicDvh = mainControl.pitem?.GetDVHCumulativeData(t, dosePres, volPres, binWidth);

                        tInfo.targetVolume = Math.Round(t.Volume, 3);
                        tInfo.d95 = Math.Round(DoseChecks.getDoseAtVolume(dynamicDvh, (t.Volume * .95)), 3);
                        tInfo.min03 = Math.Round(DoseChecks.getDoseAtVolume(dynamicDvh, (t.Volume - 0.03)), 3);
                        tInfo.min = Math.Round(dynamicDvh.MinDose.Dose, 3);
                        tInfo.max03 = Math.Round(DoseChecks.getDoseAtVolume(dynamicDvh, 0.03), 3);
                        tInfo.max = Math.Round(dynamicDvh.MaxDose.Dose, 3);
                        tInfo.mean = Math.Round(dynamicDvh.MeanDose.Dose, 3);
                    }
                    int numSegments = t.GetNumberOfSeparateParts();
                    if (numSegments == 1)
                        tInfo.segments = string.Format("1");
                    else { tInfo.segments = string.Format(">1"); }

                    mainControl.TargetCoverage_DG.Items.Add(tInfo);

                    // draw dvh
                    if (mainControl.pitem.Dose != null)
                    {
                        DVHData dvhData = selectedPlanningItem?.GetDVHCumulativeData(t,
                                        DoseValuePresentation.Absolute,
                                        VolumePresentation.Relative, 0.1);
                        //mainControl.DrawDVH(dvhData, t);
                    }
                }

                #endregion
            }

            #endregion
            //---------------------------------------------------------------------------------
            #region variables for recording data

            if (mainControl.isGrady == false)
            {
                #region directories

                #region patient specific directories

                #region base directory

                // patientSpecificDirectory
                string planDataDirectory = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanData__";
                if (!Directory.Exists(planDataDirectory))
                {
                    Directory.CreateDirectory(planDataDirectory);
                }
                string masterPlanDataDirectory = planDataDirectory + "\\_MasterData_";
                if (!Directory.Exists(masterPlanDataDirectory))
                {
                    Directory.CreateDirectory(masterPlanDataDirectory);
                }

                // patientSpecificDirectory
                string patientSpecificDirectory = planDataDirectory + "\\_PatientSpecific_\\" + pId + "\\" + course;
                if (!Directory.Exists(patientSpecificDirectory))
                {
                    Directory.CreateDirectory(patientSpecificDirectory);
                }

                #endregion

                #region proximity statistics

                // patientSpecificProximityStatsDirectory
                string patientSpecificProximityStatsDirectory = patientSpecificDirectory + "\\TargetProximityStats";
                if (!Directory.Exists(patientSpecificProximityStatsDirectory))
                {
                    Directory.CreateDirectory(patientSpecificProximityStatsDirectory);
                }

                // patientSpecificProximityStatsDirectory_randomized
                string patientSpecificProximityStatsDirectory_randomized = patientSpecificProximityStatsDirectory + "\\Randomized";
                if (!Directory.Exists(patientSpecificProximityStatsDirectory_randomized))
                {
                    Directory.CreateDirectory(patientSpecificProximityStatsDirectory_randomized);
                }

                // patientSpecificProximityStatsDirectory_randomizedJson
                string patientSpecificProximityStatsDirectory_randomizedJson = patientSpecificProximityStatsDirectory_randomized + "\\JSON";
                if (!Directory.Exists(patientSpecificProximityStatsDirectory_randomizedJson))
                {
                    Directory.CreateDirectory(patientSpecificProximityStatsDirectory_randomizedJson);
                }

                // patientSpecificProximityStatsDirectory_randomizedCsvRows
                string patientSpecificProximityStatsDirectory_randomizedCsvRows = patientSpecificProximityStatsDirectory_randomized + "\\CsvRows";
                if (!Directory.Exists(patientSpecificProximityStatsDirectory_randomizedCsvRows))
                {
                    Directory.CreateDirectory(patientSpecificProximityStatsDirectory_randomizedCsvRows);
                }

                // structureSpecificProximityStatsDirectory_randomizedCsvCols
                string patientSpecificProximityStatsDirectory_randomizedCsvCols = patientSpecificProximityStatsDirectory_randomized + "\\CsvColumns";
                if (!Directory.Exists(patientSpecificProximityStatsDirectory_randomizedCsvCols))
                {
                    Directory.CreateDirectory(patientSpecificProximityStatsDirectory_randomizedCsvCols);
                }

                #endregion

                #region dvh data

                //// patientSpecificDvhDataDirectory
                //string patientSpecificDvhDataDirectory = patientSpecificDirectory + "\\DvhData";
                //if (!Directory.Exists(patientSpecificDvhDataDirectory))
                //{
                //    Directory.CreateDirectory(patientSpecificDvhDataDirectory);
                //}

                //// patientSpecificDvhDataDirectory_plans
                //string patientSpecificDvhDataDirectory_plans = patientSpecificDvhDataDirectory + "\\Plans";
                //if (!Directory.Exists(patientSpecificDvhDataDirectory_plans))
                //{
                //    Directory.CreateDirectory(patientSpecificDvhDataDirectory_plans);
                //}

                //// patientSpecificDvhDataDirectory_sums
                //string patientSpecificDvhDataDirectory_sums = patientSpecificDvhDataDirectory + "\\Sums";
                //if (!Directory.Exists(patientSpecificDvhDataDirectory_sums))
                //{
                //    Directory.CreateDirectory(patientSpecificDvhDataDirectory_sums);
                //}

                //// patientSpecificDvhDataDirectory_randomized
                //string patientSpecificDvhDataDirectory_randomized = patientSpecificDvhDataDirectory + "\\Randomized";
                //if (!Directory.Exists(patientSpecificDvhDataDirectory_randomized))
                //{
                //    Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomized);
                //}

                //// patientSpecificDvhDataDirectory_randomizedJson
                //string patientSpecificDvhDataDirectory_randomizedJson = patientSpecificDvhDataDirectory_randomized + "\\JSON";
                //if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedJson))
                //{
                //    Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedJson);
                //}
                //string finalDvhJsonPath_patientSpecific = patientSpecificDvhDataDirectory_plans;
                //string finalDvhJsonPath_randomizedJson_patientSpecific = patientSpecificDvhDataDirectory_randomizedJson;

                //// patientSpecificDvhDataDirectory_randomizedCsvRows
                //string patientSpecificDvhDataDirectory_randomizedCsvRows = patientSpecificDvhDataDirectory_randomized + "\\CsvRows";
                //if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedCsvRows))
                //{
                //    Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedCsvRows);
                //}

                //// patientSpecificDvhDataDirectory_randomizedCsvCols
                //string patientSpecificDvhDataDirectory_randomizedCsvCols = patientSpecificDvhDataDirectory_randomized + "\\CsvColumns";
                //if (!Directory.Exists(patientSpecificDvhDataDirectory_randomizedCsvCols))
                //{
                //    Directory.CreateDirectory(patientSpecificDvhDataDirectory_randomizedCsvCols);
                //}

                #endregion

                #endregion

                #region physician specific

                //// physician folder
                //string physicianSpecificDirectory = planDataDirectory + "\\_PhysicianSpecific_\\" + PrimaryPhysician.Name.ToString();
                //if (!Directory.Exists(physicianSpecificDirectory))
                //{
                //    Directory.CreateDirectory(physicianSpecificDirectory);
                //}

                //string physicianSpecificStructureDvhDirectory = physicianSpecificDirectory + "\\StructureDvhData\\" + mainControl.courseHeader;
                //if (!Directory.Exists(physicianSpecificStructureDvhDirectory))
                //{
                //    Directory.CreateDirectory(physicianSpecificStructureDvhDirectory);
                //}

                //string physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns = physicianSpecificDirectory + "\\_PlanDvhData_\\" + currentPlan.Id + "\\CsvColumns";
                //if (!Directory.Exists(physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns))
                //{
                //    Directory.CreateDirectory(physicianSpecificPlanDvhDataDirectory_randomizedCsvColumns);
                //}

                //string physicianSpecificPlanDvhDataDirectory_randomizedCsvRows = physicianSpecificDirectory + "\\_PlanDvhData_\\" + currentPlan.Id + "\\CsvRows";
                //if (!Directory.Exists(physicianSpecificPlanDvhDataDirectory_randomizedCsvRows))
                //{
                //    Directory.CreateDirectory(physicianSpecificPlanDvhDataDirectory_randomizedCsvRows);
                //}

                #endregion

                #region structure specific

                // structure specific directory
                string structureSpecificDirectory = planDataDirectory + "\\_StructureSpecific_";
                if (!Directory.Exists(structureSpecificDirectory))
                {
                    Directory.CreateDirectory(structureSpecificDirectory);
                }

                // structure specific target prox stats
                string structureSpecificProximityStatsDirectory = structureSpecificDirectory + "\\TargetProximityStats\\" + mainControl.courseHeader;
                if (!Directory.Exists(structureSpecificProximityStatsDirectory))
                {
                    Directory.CreateDirectory(structureSpecificProximityStatsDirectory);
                }

                //// structure specific dvhdata
                //string structureSpecificDvhDataDirectory = structureSpecificDirectory + "\\DvhData\\" + mainControl.courseHeader;
                //if (!Directory.Exists(structureSpecificDvhDataDirectory))
                //{
                //    Directory.CreateDirectory(structureSpecificDvhDataDirectory);
                //}

                //// structure specific dvhdata json
                //string structureSpecificDvhDataDirectory_randomizedJson = structureSpecificDvhDataDirectory + "\\JSON";
                //if (!Directory.Exists(structureSpecificDvhDataDirectory_randomizedJson))
                //{
                //    Directory.CreateDirectory(structureSpecificDvhDataDirectory_randomizedJson);
                //}

                //// structure specific dvhdata csv
                //string structureSpecificDvhDataDirectory_randomizedCsvRows = structureSpecificDvhDataDirectory + "\\CsvRows";
                //if (!Directory.Exists(structureSpecificDvhDataDirectory_randomizedCsvRows))
                //{
                //    Directory.CreateDirectory(structureSpecificDvhDataDirectory_randomizedCsvRows);
                //}

                #endregion

                #endregion

                #region directories old

                //// json directories
                //string json_directoryPatientPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\" + pId;
                //if (!Directory.Exists(json_directoryPatientPath))
                //{
                //    Directory.CreateDirectory(json_directoryPatientPath);
                //}
                //string json_directoryPatientCoursePath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\" + pId + "\\" + course;
                //if (!Directory.Exists(json_directoryPatientCoursePath))
                //{
                //    Directory.CreateDirectory(json_directoryPatientCoursePath);
                //}
                //string json_directoryProximityStatisticsPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\" + pId + "\\" + course + "\\_ProximityStatistics";
                //if (!Directory.Exists(json_directoryProximityStatisticsPath))
                //{
                //    Directory.CreateDirectory(json_directoryProximityStatisticsPath);
                //}
                //string json_directoryProximityStatisticsForPlanPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\" + pId + "\\" + course + "\\_ProximityStatistics\\" + mainControl.planName;
                //if (!Directory.Exists(json_directoryProximityStatisticsForPlanPath))
                //{
                //    Directory.CreateDirectory(json_directoryProximityStatisticsForPlanPath);
                //}
                //string csv_directoryProximityStatisticsForPlanPath = json_directoryProximityStatisticsForPlanPath + "\\csv";
                //if (!Directory.Exists(csv_directoryProximityStatisticsForPlanPath))
                //{
                //    Directory.CreateDirectory(csv_directoryProximityStatisticsForPlanPath);
                //}
                //string csvRandomized_directoryProximityStatisticsForPlanPath = csv_directoryProximityStatisticsForPlanPath + "\\randomized";
                //if (!Directory.Exists(csvRandomized_directoryProximityStatisticsForPlanPath))
                //{
                //    Directory.CreateDirectory(csvRandomized_directoryProximityStatisticsForPlanPath);
                //}
                //string rowsCsvRandomized_path = csvRandomized_directoryProximityStatisticsForPlanPath + "\\rows";
                //if (!Directory.Exists(rowsCsvRandomized_path))
                //{
                //    Directory.CreateDirectory(rowsCsvRandomized_path);
                //}
                //string columnsCsvRandomized_path = csvRandomized_directoryProximityStatisticsForPlanPath + "\\cols";
                //if (!Directory.Exists(columnsCsvRandomized_path))
                //{
                //    Directory.CreateDirectory(columnsCsvRandomized_path);
                //}
                //string structureSpecificFolderPath_rows = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\__JsonArrays\\__StructureSpecificData__\\__ProximityStatistics__";
                //if (!Directory.Exists(structureSpecificFolderPath_rows))
                //{
                //    Directory.CreateDirectory(structureSpecificFolderPath_rows);
                //}

                #endregion
                //---------------------------------------------------------------------------------
                #region paths

                // csv path
                mainControl.rowsCsvFilePath_randomized_patientSpecific = patientSpecificProximityStatsDirectory_randomizedCsvRows;
                mainControl.colsCsvFilePath_randomized_patientSpecific = patientSpecificProximityStatsDirectory_randomizedCsvCols;
                mainControl.rowsCsvFilePath_randomized_structureSpecific = structureSpecificProximityStatsDirectory;

                #region json path //

                //// json path
                //string jsonPath = "";
                //string jsonPath_randomized = "";
                //jsonPath = json_directoryProximityStatisticsForPlanPath + "\\ProximityStatisticsJsonArray_" + pId + "_" + mainControl.planName + ".json";
                //jsonPath_randomized = json_directoryProximityStatisticsForPlanPath + "\\RandomizedProximityStatisticsJsonArray_" + mainControl.randomId + "_" + mainControl.planName + ".json";
                //mainControl.jsonFilePath = jsonPath;
                //mainControl.jsonFilePath_randomized = jsonPath_randomized;

                #endregion

                #endregion
                //---------------------------------------------------------------------------------
                #region log

                if (mainControl.isGrady == false)
                {
                    mainControl.LogUser(mainControl.script);
                }

                #endregion
            }

            #endregion
            //---------------------------------------------------------------------------------
            
            #endregion
            //---------------------------------------------------------------------------------
        }
    }
}
