using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace ShiftCalculator
{
    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl : UserControl
    {
        public MainControl()
        {
            InitializeComponent();
        }
        //---------------------------------------------------------------------------------
        #region public variables

        //public Window Window;
        public string script = "ShiftCalculator";
        public PlanSetup ps;
        public PlanningItem pitem;
        public StructureSet ss;
        public string approvalStatus;
        //public IEnumerable<Structure> sorted_markerList;
        public IEnumerator<PlanSetup> plans;
        //public IEnumerable<Beam> fields;
        public double? Fractions;
        public string user;
        public double dosePerFraction;
        public string day;
        public string month;
        public string year;
        public string dayOfWeek;
        public string hour;
        public string minute;
        public string timeStamp;
        public string curredLastName;
        public string curredFirstName;
        public string firstInitial;
        public string lastInitial;
        public string initials;
        public string id;
        //public double idAsDouble;
        public string randomId;
        public string courseName;
        public string planName;
        public bool isGrady;
        public string primaryPhysician;
        public string courseHeader;
        public VMS.TPS.Common.Model.API.Image image;
        public Structure selectedIso;

        public PatientOrientation patientOrientation_plan1;
        public decimal userOriginX_plan1;
        public decimal userOriginY_plan1;
        public decimal userOriginZ_plan1;
        public ExternalPlanSetup currentPlan1;
        public IEnumerable<Beam> planFields1;
        public StructureSet currentSS1;
        public decimal plan1_fieldXdicom;
        public decimal plan1_fieldYdicom;
        public decimal plan1_fieldZdicom;

        public decimal plan1_markerXdicom;
        public decimal plan1_markerYdicom;
        public decimal plan1_markerZdicom;

        public PatientOrientation patientOrientation_plan2;
        public decimal userOriginX_plan2;
        public decimal userOriginY_plan2;
        public decimal userOriginZ_plan2;
        public ExternalPlanSetup currentPlan2;
        public IEnumerable<Beam> planFields2;
        public StructureSet currentSS2;
        public decimal plan2_fieldXdicom;
        public decimal plan2_fieldYdicom;
        public decimal plan2_fieldZdicom;

        
        public PatientOrientation pOrientation;

        public SimIsoCoordinates simIC = new SimIsoCoordinates();
        



        #endregion
        //---------------------------------------------------------------------------------
        #region objects used for binding

        //public string Title { get; set; }

        public class PrimaryPhysician
        {
            public string Name { get; set; }
        }
        public class SimIsoCoordinates
        {
            public string StructureSetId { get; set; }
            public string PatientOrientation { get; set; }
            public string MarkerId { get; set; }
            public decimal SimIsoX { get; set; }
            public decimal SimIsoY { get; set; }
            public decimal SimIsoZ { get; set; }
        }
        public class ShiftInfo
        {
            public string PlanId { get; set; }
            public string StructureSetId { get; set; }
            public string PlanStatus { get; set; }
            public string ShiftDescription { get; set; }
            //public string FieldId { get; set; }
            public decimal FieldIsoX { get; set; }
            public decimal FieldIsoY { get; set; }
            public decimal FieldIsoZ { get; set; }
            public decimal DeltaX { get; set; }
            public string DirectionX { get; set; }
            public decimal DeltaY { get; set; }
            public string DirectionY { get; set; }
            public decimal DeltaZ { get; set; }
            public string DirectionZ { get; set; }
            public string ShiftX { get; set; }
            public string ShiftY { get; set; }
            public string ShiftZ { get; set; }
        }
        

        #endregion
        //---------------------------------------------------------------------------------
        #region paths and string builders for data collection scripts

        #region Script Use Log

        //TODO: add file to collect various data on script used, time, user, etc. 
        public string userLogPath = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\esapi\\projects\\__Logs\\UserLog.csv";

        public StringBuilder userLogCsvContent = new StringBuilder();

        #endregion

        #endregion
        //---------------------------------------------------------------------------------
        #region event controls

        private void Print_Visual(object sender, RoutedEventArgs e)
        {
            // buttons
            PrintVisual_Btn.Visibility = Visibility.Hidden;
            AddNote_Btn.Visibility = Visibility.Hidden;
            ClearNote_Btn.Visibility = Visibility.Hidden;
            PlanAndIsoInput_SP.Visibility = Visibility.Collapsed;

            //MainUC.Width = 1100;
            //Main_SP.Margin = new Thickness(90, 0, 0, 0);
            //Main_SP.VerticalAlignment = VerticalAlignment.Center;
            ShiftInfo_DG.MaxHeight = 1000;

            // stackpanels and groupboxes
            if (Note_TB.Text == "")
            {
                Notes_SP.Visibility = Visibility.Collapsed;
            }
            if (!IsoDicomCooridinates_DG.HasItems)
            {
                IsoDicomCooridinates_SP.Visibility = Visibility.Collapsed;
            }
            var colToRemove1 = IsoDicomCooridinates_DG.ColumnFromDisplayIndex(6);
            var colToRemove2 = ShiftInfo_DG.ColumnFromDisplayIndex(9);
            
            colToRemove1.Visibility = Visibility.Hidden;
            colToRemove2.Visibility = Visibility.Hidden;

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                
                printDialog.PrintVisual(MainGrid, string.Format("{0}_PlanShiftInfo_{1}", id, courseName));
            }
        }
        
        private void IsoTypeSelected(object sender, RoutedEventArgs e)
        {
            if (MarkerOrField_CB.SelectedItem.ToString() == "")
            {
                Plan1_CB.SelectedItem = "";
                Plan2_CB.SelectedItem = "";
                StartingIso_CB.Items.Clear();

                ClearSimIsoCoordinates_Clicked(sender, e);
                ClearSimIsoCoordinates2_Clicked(sender, e);
            }
            if (MarkerOrField_CB.SelectedItem.ToString() == "Marker")
            {
                Plan1Selector_SP.Visibility = Visibility.Visible;
                Marker_SP.Visibility = Visibility.Visible;
                Plan2Selector_SP.Visibility = Visibility.Collapsed;

                Plan1_CB.SelectedItem = "";
                Plan2_CB.SelectedItem = "";

                ClearSimIsoCoordinates2_Clicked(sender, e);

                Plan1Label_TB.Text = "Plan Id:";
            }
            if (MarkerOrField_CB.SelectedItem.ToString() == "Field")
            {
                ClearSimIsoCoordinates_Clicked(sender, e);
                ClearSimIsoCoordinates2_Clicked(sender, e);
                StartingIso_CB.Items.Clear();
                
                Plan1_CB.SelectedItem = "";
                Plan2_CB.SelectedItem = "";

                Plan1Selector_SP.Visibility = Visibility.Visible;
                Plan2Selector_SP.Visibility = Visibility.Visible;
                Marker_SP.Visibility = Visibility.Collapsed;

                Plan1Label_TB.Text = "Plan with starting Iso:";
            }
        }

        private void Plan1Selected(object sender, RoutedEventArgs e)
        {
            if (MarkerOrField_CB.SelectedItem == null)
            {
                MessageBox.Show("Please select whether you are shifting from a field or marker.");
                return;
            }
            if (Plan1_CB.SelectedItem.ToString() == "")
            {
                X_Input.Text = string.Empty;
                Y_Input.Text = string.Empty;
                Z_Input.Text = string.Empty;
            }
            else
            {
                Plan1Iso_SP.Visibility = Visibility.Visible;
                X_Input.Text = string.Empty;
                Y_Input.Text = string.Empty;
                Z_Input.Text = string.Empty;
                currentPlan1 = (ExternalPlanSetup)Plan1_CB.SelectedItem;
                planFields1 = currentPlan1.Beams;
                currentSS1 = currentPlan1.StructureSet;

                patientOrientation_plan1 = currentSS1.Image.ImagingOrientation;
				MessageBox.Show(patientOrientation_plan1.ToString());
                if (MarkerOrField_CB.SelectedItem.ToString() == "Marker")
                {
                    Marker_SP.Visibility = Visibility.Visible;

                    StartingIso_CB.Items.Clear();

                    var markerList = new List<Structure>();
                    IEnumerable<Structure> sorted_markerList;
                    foreach (var s in currentSS1.Structures)
                    {
                        if (!s.HasSegment)
                        {
                            markerList.Add(s);
                        }
                    }
                    sorted_markerList = markerList?.OrderBy(x => x.Id);
                    foreach (var m in sorted_markerList)
                    {
                        StartingIso_CB.Items.Add(m);
                    }
                    StartingIso_CB.SelectedIndex = 0;
                    selectedIso = (Structure)StartingIso_CB.SelectedItem;

                    var txFields = new List<Beam>();
                    foreach (var field in planFields1)
                    {
                        if (!field.IsSetupField)
                        {
                            txFields.Add(field);
                        }
                    }
                    var txField_01 = txFields[0];

                    plan1_fieldXdicom = Math.Round((decimal)txField_01.IsocenterPosition.x / 10, 2);
                    plan1_fieldYdicom = Math.Round((decimal)txField_01.IsocenterPosition.y / 10, 2);
                    plan1_fieldZdicom = Math.Round((decimal)txField_01.IsocenterPosition.z / 10, 2);

                }
                else if (MarkerOrField_CB.SelectedItem.ToString() == "Field")
                {
                    
                    Plan1Iso_SP.Visibility = Visibility.Visible;
                    Marker_SP.Visibility = Visibility.Collapsed;
                    //MarkerOrField_CB.SelectedItem = "";
                    //X_Input.Text = string.Empty;
                    //Y_Input.Text = string.Empty;
                    //Z_Input.Text = string.Empty;

                    var txFields = new List<Beam>();
                    foreach (var field in planFields1)
                    {
                        if (!field.IsSetupField)
                        {
                            txFields.Add(field);
                        }
                    }
                    var txField_01 = txFields[0];

                    plan1_fieldXdicom = Math.Round((decimal)txField_01.IsocenterPosition.x / 10, 2);
                    plan1_fieldYdicom = Math.Round((decimal)txField_01.IsocenterPosition.y / 10, 2);
                    plan1_fieldZdicom = Math.Round((decimal)txField_01.IsocenterPosition.z / 10, 2);
                    X_Input.Text = plan1_fieldXdicom.ToString();
                    Y_Input.Text = plan1_fieldYdicom.ToString();
                    Z_Input.Text = plan1_fieldZdicom.ToString();

                }
            }
        }

        private void IsoSelected(object sender, RoutedEventArgs e)
        {
            if (StartingIso_CB.SelectedIndex >= 0)
            {
                Plan1Iso_SP.Visibility = Visibility.Visible;
				//MessageBox.Show(pOrientation.ToString());
				Structure m = (Structure)StartingIso_CB.SelectedItem;
                plan1_markerXdicom = (decimal)Math.Round(m.CenterPoint.x / 10, 2);
                plan1_markerYdicom = (decimal)Math.Round(m.CenterPoint.y / 10, 2);
                plan1_markerZdicom = (decimal)Math.Round(m.CenterPoint.z / 10, 2);
                X_Input.Text = plan1_markerXdicom.ToString();
                Y_Input.Text = plan1_markerYdicom.ToString();
                Z_Input.Text = plan1_markerZdicom.ToString();
                //simIC.MarkerId = m.Id.ToString().Split(':').First();

                //MessageBox.Show(string.Format("Selected Marker's Coordinates (x, y, z):\n\t({0}, {1}, {2})", (Math.Round(m.CenterPoint.x / 10, 2)), (Math.Round(m.CenterPoint.y / 10, 2)), (Math.Round(m.CenterPoint.z / 10, 2))));
            }
        }

        private void Plan2Selected(object sender, RoutedEventArgs e)
        {
            if (Plan2_CB.SelectedItem.ToString() == "")
            {
                X2_Input.Text = string.Empty;
                Y2_Input.Text = string.Empty;
                Z2_Input.Text = string.Empty;
            }
            else
            {
                Plan2Iso_SP.Visibility = Visibility.Visible;
                X2_Input.Text = string.Empty;
                Y2_Input.Text = string.Empty;
                Z2_Input.Text = string.Empty;
                currentPlan2 = (ExternalPlanSetup)Plan2_CB.SelectedItem;
                planFields2 = currentPlan2.Beams;
                currentSS2 = currentPlan2.StructureSet;

                patientOrientation_plan2 = currentSS2.Image.ImagingOrientation;


                Plan1Iso_SP.Visibility = Visibility.Visible;

                var txFields = new List<Beam>();
                foreach (var field in planFields2)
                {
                    if (!field.IsSetupField)
                    {
                        txFields.Add(field);
                    }
                }
                var txField_01 = txFields[0];

                plan2_fieldXdicom = Math.Round((decimal)txField_01.IsocenterPosition.x / 10, 2);
                plan2_fieldYdicom = Math.Round((decimal)txField_01.IsocenterPosition.y / 10, 2);
                plan2_fieldZdicom = Math.Round((decimal)txField_01.IsocenterPosition.z / 10, 2);
                X2_Input.Text = plan2_fieldXdicom.ToString();
                Y2_Input.Text = plan2_fieldYdicom.ToString();
                Z2_Input.Text = plan2_fieldZdicom.ToString();
            }
            

        }

        private void CalcShifts_Clicked(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                userOriginX_plan1 = Math.Round(((decimal)currentSS1.Image.UserOrigin.x / 10), 2);
                userOriginY_plan1 = Math.Round(((decimal)currentSS1.Image.UserOrigin.y / 10), 2);
                userOriginZ_plan1 = Math.Round(((decimal)currentSS1.Image.UserOrigin.z / 10), 2);

                decimal inputX = Decimal.Zero;
                decimal inputY = Decimal.Zero;
                decimal inputZ = Decimal.Zero;
                if (X_Input.Text != "")
                {
                    Decimal.TryParse(X_Input.Text, out inputX);
                }
                if (Y_Input.Text != "")
                {
                    Decimal.TryParse(Y_Input.Text, out inputY);
                }
                if (Z_Input.Text != "")
                {
                    Decimal.TryParse(Z_Input.Text, out inputZ);
                }

                //MessageBox.Show(string.Format("UserOrgin (x, y, z) = {0}, {1}, {2}", userOriginX, userOriginY, userOriginZ));
                if (MarkerOrField_CB.SelectedItem.ToString() == "Marker")
                {
                    simIC.MarkerId = ((Structure)StartingIso_CB.SelectedItem).Id.ToString().Split(':').First();

                    if ((!Double.IsNaN((double)inputX)) && (!Double.IsNaN((double)inputY)) && (!Double.IsNaN((double)inputZ)))
                    {
                        simIC.SimIsoX = inputX;
                        simIC.SimIsoY = inputY;
                        simIC.SimIsoZ = inputZ;

                        #region old
                        //string tempSsId1 = string.Empty;
                        //string tempSsId2 = string.Empty;
                        //if (planList_LV.SelectedItems.Count > 1)
                        //{
                        //    var planSetup0 = (ExternalPlanSetup)planList_LV.SelectedItems[0];
                        //    tempSsId1 = planSetup0.StructureSet.Id;
                        //    for (int i = 0; i < planList_LV.SelectedItems.Count; i++)
                        //    {
                        //        var planSetupi = (ExternalPlanSetup)planList_LV.SelectedItems[i];
                        //        tempSsId2 = planSetupi.StructureSet.Id;
                        //        if ((tempSsId1 != string.Empty) && (tempSsId2 != string.Empty) && (tempSsId1 != tempSsId2))
                        //        {
                        //            MessageBox.Show("At least two of the selected plans have different assigned structure sets. Please calculate their shifts separately.");
                        //            return;
                        //        }
                        //    }
                        //}
                        #endregion

                        simIC.StructureSetId = currentSS1.Id.ToString().Split(':').First();
                        simIC.PatientOrientation = patientOrientation_plan1.ToString();
                        IsoDicomCooridinates_DG.Items.Add(simIC);

                        //foreach (var plan in planList_LV.SelectedItems)
                        //{
                        //currentPlan = (ExternalPlanSetup)plan;
                        //planFields = currentPlan.Beams;
                        //currentSS = currentPlan.StructureSet;
                        //var currentPatientOrientation = currentSS1.Image.ImagingOrientation;
                        if (patientOrientation_plan1 != pOrientation)
                        {
                            MessageBox.Show("Some plans may have different patient orientations. Please verify shift directions carefully.");
                        }
                        //var field1 = planFields.First();
                        //var txFields = new List<Beam>();
                        //foreach (var field in planFields1)
                        //{
                        //    if (!field.IsSetupField)
                        //    {
                        //        txFields.Add(field);
                        //    }
                        //}
                        //var txField_01 = txFields[0];

                        var fieldXiso = (plan1_fieldXdicom - userOriginX_plan1);
                        var fieldYiso = (plan1_fieldYdicom - userOriginY_plan1);
                        var fieldZiso = (plan1_fieldZdicom - userOriginZ_plan1);

						if (patientOrientation_plan1 == PatientOrientation.HeadFirstProne)
						{
							fieldXiso = -(plan1_fieldXdicom - userOriginX_plan1);
							fieldYiso = -(plan1_fieldYdicom - userOriginY_plan1);
							fieldZiso = (plan1_fieldZdicom - userOriginZ_plan1);
						}

						ShiftInfo si = new ShiftInfo();
                        si.PlanId = currentPlan1.Id.ToString().Split(':').First();
                        si.StructureSetId = currentSS1.Id.ToString().Split(':').First();
                        si.PlanStatus = currentPlan1.ApprovalStatus.ToString();
                        //si.FieldId = txField_01.Id.ToString().Split(':').First();

                        si.ShiftDescription = string.Format("{0} marker to {1} Iso", simIC.MarkerId, si.PlanId);

                        si.FieldIsoX = Math.Round(fieldXiso, 2);
                        si.FieldIsoY = Math.Round(fieldYiso, 2);
                        si.FieldIsoZ = Math.Round(fieldZiso, 2);

                        var shiftX = Math.Round(((plan1_fieldXdicom - inputX)), 1);
                        var shiftY = Math.Round(((plan1_fieldYdicom - inputY)), 1);
                        var shiftZ = Math.Round(((plan1_fieldZdicom - inputZ)), 1);

                        si.DeltaX = Math.Abs(shiftX);
                        si.DeltaY = Math.Abs(shiftY);
                        si.DeltaZ = Math.Abs(shiftZ);
                        //var fieldX = (((decimal)field.IsocenterPosition.x / 10) - userOriginX);
                        //var fieldY = (((decimal)field.IsocenterPosition.y / 10) - userOriginY);
                        //var fieldZ = (((decimal)field.IsocenterPosition.z / 10) - userOriginZ);
                        //ShiftInfo si = new ShiftInfo();
                        //si.PlanId = currentPlan.Id.ToString().Split(':').First();
                        //si.StructureSetId = currentPlan.StructureSet.Id.ToString().Split(':').First();
                        //si.PlanStatus = currentPlan.ApprovalStatus.ToString();
                        //si.FieldId = field.Id.ToString().Split(':').First();

                        //si.FieldIsoX = Math.Round(fieldX, 1);
                        //si.FieldIsoY = Math.Round(fieldY, 1);
                        //si.FieldIsoZ = Math.Round(fieldZ, 1);

                        //si.DeltaX = Math.Round((((field.IsocenterPosition.x / 10) - inputX) * (1)), 1);
                        //si.DeltaY = Math.Round((((field.IsocenterPosition.y / 10) - inputY) * (1)), 1);
                        //si.DeltaZ = Math.Round((((field.IsocenterPosition.z / 10) - inputZ) * (1)), 1);

                        if (patientOrientation_plan1 == PatientOrientation.HeadFirstSupine)
                        {
                            if (shiftX == 0) { si.DirectionX = "-"; }
                            else if (shiftX > 0) { si.DirectionX = "Left"; }
                            else { si.DirectionX = "Right"; }

                            if (shiftY == 0) { si.DirectionY = "-"; }
                            else if (shiftY > 0) { si.DirectionY = "Post"; }
                            else { si.DirectionY = "Ant"; }

                            if (shiftZ == 0) { si.DirectionZ = "-"; }
                            else if (shiftZ > 0) { si.DirectionZ = "Sup"; }
                            else { si.DirectionZ = "Inf"; }

                            si.ShiftX = string.Format("{0} cm {1}", si.DeltaX, si.DirectionX);
                            si.ShiftY = string.Format("{0} cm {1}", si.DeltaY, si.DirectionY);
                            si.ShiftZ = string.Format("{0} cm {1}", si.DeltaZ, si.DirectionZ);
                        }
                        else if (patientOrientation_plan1 == PatientOrientation.HeadFirstProne)
                        {

							if (shiftX == 0) { si.DirectionX = "-"; }
							else if (shiftX > 0) { si.DirectionX = "Left"; }
							else { si.DirectionX = "Right"; }

							if (shiftY == 0) { si.DirectionY = "-"; }
							else if (shiftY > 0) { si.DirectionY = "Post"; }
							else { si.DirectionY = "Ant"; }

							if (shiftZ == 0) { si.DirectionZ = "-"; }
							else if (shiftZ > 0) { si.DirectionZ = "Sup"; }
							else { si.DirectionZ = "Inf"; }

							si.ShiftX = string.Format("{0} cm {1}", si.DeltaX, si.DirectionX);
                            si.ShiftY = string.Format("{0} cm {1}", si.DeltaY, si.DirectionY);
                            si.ShiftZ = string.Format("{0} cm {1}", si.DeltaZ, si.DirectionZ);
                        }
                        else if (patientOrientation_plan1 == PatientOrientation.FeetFirstSupine)
                        {
                            if (shiftX == 0) { si.DirectionX = "-"; }
                            else if (shiftX > 0) { si.DirectionX = "Right"; }
                            else { si.DirectionX = "Left"; }

                            if (shiftY == 0) { si.DirectionY = "-"; }
                            else if (shiftY > 0) { si.DirectionY = "Post"; }
                            else { si.DirectionY = "Ant"; }

                            if (shiftZ == 0) { si.DirectionZ = "-"; }
                            else if (shiftZ > 0) { si.DirectionZ = "Inf"; }
                            else { si.DirectionZ = "Sup"; }

                            si.ShiftX = string.Format("{0} cm {1}", si.DeltaX, si.DirectionX);
                            si.ShiftY = string.Format("{0} cm {1}", si.DeltaY, si.DirectionY);
                            si.ShiftZ = string.Format("{0} cm {1}", si.DeltaZ, si.DirectionZ);
                        }
                        else if (patientOrientation_plan1 == PatientOrientation.FeetFirstProne)
                        {
                            if (shiftX == 0) { si.DirectionX = "-"; }
                            else if (shiftX > 0) { si.DirectionX = "Right"; }
                            else { si.DirectionX = "Left"; }

                            if (shiftY == 0) { si.DirectionY = "-"; }
                            else if (shiftY > 0) { si.DirectionY = "Ant"; }
                            else { si.DirectionY = "Post"; }

                            if (shiftZ == 0) { si.DirectionZ = "-"; }
                            else if (shiftZ > 0) { si.DirectionZ = "Inf"; }
                            else { si.DirectionZ = "Sup"; }

                            si.ShiftX = string.Format("{0} cm {1}", si.DeltaX, si.DirectionX);
                            si.ShiftY = string.Format("{0} cm {1}", si.DeltaY, si.DirectionY);
                            si.ShiftZ = string.Format("{0} cm {1}", si.DeltaZ, si.DirectionZ);
                        }
                        else
                        {
                            MessageBox.Show(string.Format("At this time, shifts will need to be calculated manually for {0} positioning.", patientOrientation_plan1));
                        }
                        ShiftInfo_DG.Items.Add(si);
                        //}
                        //}
                    }
                }

                if (MarkerOrField_CB.SelectedItem.ToString() == "Field")
                {
                    //simIC.MarkerId = string.Format("{0} Iso", currentPlan1.Id.ToString().Split(':').First());
                    if ((Plan1_CB.SelectedIndex >= 0) && (Plan2_CB.SelectedIndex >= 0) &&
                        (X_Input.Text != "") && (Y_Input.Text != "") && (Z_Input.Text != "") &&
                        (X2_Input.Text != "") && (Y2_Input.Text != "") && (Z2_Input.Text != ""))
                    {

                        userOriginX_plan2 = Math.Round(((decimal)currentSS2.Image.UserOrigin.x / 10), 2);
                        userOriginY_plan2 = Math.Round(((decimal)currentSS2.Image.UserOrigin.y / 10), 2);
                        userOriginZ_plan2 = Math.Round(((decimal)currentSS2.Image.UserOrigin.z / 10), 2);

                        decimal inputX2 = Decimal.Zero;
                        decimal inputY2 = Decimal.Zero;
                        decimal inputZ2 = Decimal.Zero;
                        if (X2_Input.Text != "")
                        {
                            Decimal.TryParse(X2_Input.Text, out inputX2);
                        }
                        if (Y2_Input.Text != "")
                        {
                            Decimal.TryParse(Y2_Input.Text, out inputY2);
                        }
                        if (Z2_Input.Text != "")
                        {
                            Decimal.TryParse(Z2_Input.Text, out inputZ2);
                        }
                        if ((!Double.IsNaN((double)inputX)) && (!Double.IsNaN((double)inputY)) && (!Double.IsNaN((double)inputZ)) &&
                            (!Double.IsNaN((double)inputX2)) && (!Double.IsNaN((double)inputY2)) && (!Double.IsNaN((double)inputZ2)))
                        {
                            //simIC.SimIsoX = inputX2;
                            //simIC.SimIsoY = inputY2;
                            //simIC.SimIsoZ = inputZ2;

                            #region old
                            //string tempSsId1 = string.Empty;
                            //string tempSsId2 = string.Empty;
                            //if (planList_LV.SelectedItems.Count > 1)
                            //{
                            //    var planSetup0 = (ExternalPlanSetup)planList_LV.SelectedItems[0];
                            //    tempSsId1 = planSetup0.StructureSet.Id;
                            //    for (int i = 0; i < planList_LV.SelectedItems.Count; i++)
                            //    {
                            //        var planSetupi = (ExternalPlanSetup)planList_LV.SelectedItems[i];
                            //        tempSsId2 = planSetupi.StructureSet.Id;
                            //        if ((tempSsId1 != string.Empty) && (tempSsId2 != string.Empty) && (tempSsId1 != tempSsId2))
                            //        {
                            //            MessageBox.Show("At least two of the selected plans have different assigned structure sets. Please calculate their shifts separately.");
                            //            return;
                            //        }
                            //    }
                            //}
                            #endregion

                            //simIC.StructureSetId = currentSS2.Id.ToString().Split(':').First();
                            //simIC.PatientOrientation = patientOrientation_plan1.ToString();
                            //IsoDicomCooridinates_DG.Items.Add(simIC);

                            //foreach (var plan in planList_LV.SelectedItems)
                            //{
                            //currentPlan = (ExternalPlanSetup)plan;
                            //planFields = currentPlan.Beams;
                            //currentSS = currentPlan.StructureSet;
                            //var currentPatientOrientation = currentSS1.Image.ImagingOrientation;
                            if (patientOrientation_plan1 != patientOrientation_plan2)
                            {
                                MessageBox.Show("These plans may have different patient orientations. Please select plans with the same patient orientation.");
                                return;
                            }
                            //var field1 = planFields.First();
                            //var txFields = new List<Beam>();
                            //foreach (var field in planFields1)
                            //{
                            //    if (!field.IsSetupField)
                            //    {
                            //        txFields.Add(field);
                            //    }
                            //}
                            //var txField_01 = txFields[0];
                            else
                            {
                                if (userOriginX_plan1 != userOriginX_plan2) { MessageBox.Show("These plans have different user origins. Please verify the images are registered and the shifts and shift directions carefully."); }

                                var fieldXiso = (plan1_fieldXdicom - userOriginX_plan1);
                                var fieldYiso = (plan1_fieldYdicom - userOriginY_plan1);
                                var fieldZiso = (plan1_fieldZdicom - userOriginZ_plan1);

                                var fieldXiso2 = (plan2_fieldXdicom - userOriginX_plan2);
                                var fieldYiso2 = (plan2_fieldYdicom - userOriginY_plan2);
                                var fieldZiso2 = (plan2_fieldZdicom - userOriginZ_plan2);

								if (patientOrientation_plan1 == PatientOrientation.HeadFirstProne)
								{
									fieldXiso = -(plan1_fieldXdicom - userOriginX_plan1);
									fieldYiso = -(plan1_fieldYdicom - userOriginY_plan1);
									fieldZiso = (plan1_fieldZdicom - userOriginZ_plan1);

									fieldXiso2 = -(plan2_fieldXdicom - userOriginX_plan2);
									fieldYiso2 = -(plan2_fieldYdicom - userOriginY_plan2);
									fieldZiso2 = (plan2_fieldZdicom - userOriginZ_plan2);
								}

								ShiftInfo si = new ShiftInfo();
                                si.PlanId = currentPlan2.Id.ToString().Split(':').First();
                                si.StructureSetId = currentSS2.Id.ToString().Split(':').First();
                                si.PlanStatus = currentPlan2.ApprovalStatus.ToString();
                                //si.FieldId = txField_01.Id.ToString().Split(':').First();
                                string plan1Id = currentPlan1.Id.ToString().Split(':').First();

                                si.ShiftDescription = string.Format("{0} Iso to {1} Iso", plan1Id, si.PlanId);

                                si.FieldIsoX = Math.Round(fieldXiso2, 2);
                                si.FieldIsoY = Math.Round(fieldYiso2, 2);
                                si.FieldIsoZ = Math.Round(fieldZiso2, 2);

                                var shiftX = Math.Round(((inputX2 - inputX)), 1);
                                var shiftY = Math.Round(((inputY2 - inputY)), 1);
                                var shiftZ = Math.Round(((inputZ2 - inputZ)), 1);

                                si.DeltaX = Math.Abs(shiftX);
                                si.DeltaY = Math.Abs(shiftY);
                                si.DeltaZ = Math.Abs(shiftZ);
                                //var fieldX = (((decimal)field.IsocenterPosition.x / 10) - userOriginX);
                                //var fieldY = (((decimal)field.IsocenterPosition.y / 10) - userOriginY);
                                //var fieldZ = (((decimal)field.IsocenterPosition.z / 10) - userOriginZ);
                                //ShiftInfo si = new ShiftInfo();
                                //si.PlanId = currentPlan.Id.ToString().Split(':').First();
                                //si.StructureSetId = currentPlan.StructureSet.Id.ToString().Split(':').First();
                                //si.PlanStatus = currentPlan.ApprovalStatus.ToString();
                                //si.FieldId = field.Id.ToString().Split(':').First();

                                //si.FieldIsoX = Math.Round(fieldX, 1);
                                //si.FieldIsoY = Math.Round(fieldY, 1);
                                //si.FieldIsoZ = Math.Round(fieldZ, 1);

                                //si.DeltaX = Math.Round((((field.IsocenterPosition.x / 10) - inputX) * (1)), 1);
                                //si.DeltaY = Math.Round((((field.IsocenterPosition.y / 10) - inputY) * (1)), 1);
                                //si.DeltaZ = Math.Round((((field.IsocenterPosition.z / 10) - inputZ) * (1)), 1);

                                if (patientOrientation_plan1 == PatientOrientation.HeadFirstSupine)
                                {
                                    if (shiftX == 0) { si.DirectionX = "-"; }
                                    else if (shiftX > 0) { si.DirectionX = "Left"; }
                                    else { si.DirectionX = "Right"; }

                                    if (shiftY == 0) { si.DirectionY = "-"; }
                                    else if (shiftY > 0) { si.DirectionY = "Post"; }
                                    else { si.DirectionY = "Ant"; }

                                    if (shiftZ == 0) { si.DirectionZ = "-"; }
                                    else if (shiftZ > 0) { si.DirectionZ = "Sup"; }
                                    else { si.DirectionZ = "Inf"; }

                                    si.ShiftX = string.Format("{0} cm {1}", si.DeltaX, si.DirectionX);
                                    si.ShiftY = string.Format("{0} cm {1}", si.DeltaY, si.DirectionY);
                                    si.ShiftZ = string.Format("{0} cm {1}", si.DeltaZ, si.DirectionZ);
                                }
                                else if (patientOrientation_plan1 == PatientOrientation.HeadFirstProne)
                                {
									if (shiftX == 0) { si.DirectionX = "-"; }
									else if (shiftX > 0) { si.DirectionX = "Left"; }
									else { si.DirectionX = "Right"; }

									if (shiftY == 0) { si.DirectionY = "-"; }
									else if (shiftY > 0) { si.DirectionY = "Post"; }
									else { si.DirectionY = "Ant"; }

									if (shiftZ == 0) { si.DirectionZ = "-"; }
									else if (shiftZ > 0) { si.DirectionZ = "Sup"; }
									else { si.DirectionZ = "Inf"; }

									si.ShiftX = string.Format("{0} cm {1}", si.DeltaX, si.DirectionX);
                                    si.ShiftY = string.Format("{0} cm {1}", si.DeltaY, si.DirectionY);
                                    si.ShiftZ = string.Format("{0} cm {1}", si.DeltaZ, si.DirectionZ);
                                }
                                else if (patientOrientation_plan1 == PatientOrientation.FeetFirstSupine)
                                {
                                    if (shiftX == 0) { si.DirectionX = "-"; }
                                    else if (shiftX > 0) { si.DirectionX = "Right"; }
                                    else { si.DirectionX = "Left"; }

                                    if (shiftY == 0) { si.DirectionY = "-"; }
                                    else if (shiftY > 0) { si.DirectionY = "Post"; }
                                    else { si.DirectionY = "Ant"; }

                                    if (shiftZ == 0) { si.DirectionZ = "-"; }
                                    else if (shiftZ > 0) { si.DirectionZ = "Inf"; }
                                    else { si.DirectionZ = "Sup"; }

                                    si.ShiftX = string.Format("{0} cm {1}", si.DeltaX, si.DirectionX);
                                    si.ShiftY = string.Format("{0} cm {1}", si.DeltaY, si.DirectionY);
                                    si.ShiftZ = string.Format("{0} cm {1}", si.DeltaZ, si.DirectionZ);
                                }
                                else if (patientOrientation_plan1 == PatientOrientation.FeetFirstProne)
                                {
                                    if (shiftX == 0) { si.DirectionX = "-"; }
                                    else if (shiftX > 0) { si.DirectionX = "Right"; }
                                    else { si.DirectionX = "Left"; }

                                    if (shiftY == 0) { si.DirectionY = "-"; }
                                    else if (shiftY > 0) { si.DirectionY = "Ant"; }
                                    else { si.DirectionY = "Post"; }

                                    if (shiftZ == 0) { si.DirectionZ = "-"; }
                                    else if (shiftZ > 0) { si.DirectionZ = "Inf"; }
                                    else { si.DirectionZ = "Sup"; }

                                    si.ShiftX = string.Format("{0} cm {1}", si.DeltaX, si.DirectionX);
                                    si.ShiftY = string.Format("{0} cm {1}", si.DeltaY, si.DirectionY);
                                    si.ShiftZ = string.Format("{0} cm {1}", si.DeltaZ, si.DirectionZ);
                                }
                                else
                                {
                                    MessageBox.Show(string.Format("At this time, shifts will need to be calculated manually for {0} positioning.", patientOrientation_plan2));
                                }
                                ShiftInfo_DG.Items.Add(si);
                                //}
                                //}
                            }
                            MessageBox.Show(string.Format("Shifts are with respect to the patient and their positioning."));
                        }
                    }
                    else { MessageBox.Show(string.Format("Input is missing or incorrect.")); return; }
                }
                
            }
        }

       
        #region input changed events

        #region basic events

        private void ShiftInfoRemoveBtn_Clicked(object sender, RoutedEventArgs e)
        {
            ShiftInfo_DG.Items.RemoveAt(ShiftInfo_DG.SelectedIndex);
        }
        private void DicomCoordRemoveBtn_Clicked(object sender, RoutedEventArgs e)
        {
            IsoDicomCooridinates_DG.Items.RemoveAt(IsoDicomCooridinates_DG.SelectedIndex);
        }
        private void ClearSimIsoCoordinates_Clicked(object sender, RoutedEventArgs e)
        {
            X_Input.Text = "";
            Y_Input.Text = "";
            Z_Input.Text = "";
        }
        private void ClearSimIsoCoordinates2_Clicked(object sender, RoutedEventArgs e)
        {
            X2_Input.Text = "";
            Y2_Input.Text = "";
            Z2_Input.Text = "";
        }

        // add note event
        private void AddNote_Clicked(Object sender, RoutedEventArgs e)
        {
            if (Notes_GB.Visibility == Visibility.Visible)
            {
                Notes_GB.Visibility = Visibility.Hidden;
                AddNote_Btn.Content = "Add Note";
            }
            else
            {
                if (Notes_SP.Visibility == Visibility.Collapsed)
                {
                    Notes_SP.Visibility = Visibility.Visible;
                }
                Notes_GB.Visibility = Visibility.Visible;
                AddNote_Btn.Content = "Hide Note";
            }
        }
        private void ClearNote_Clicked(Object sender, RoutedEventArgs e)
        {
            Note_TB.Text = "";
        }



        #endregion

        #endregion

        #endregion
        //---------------------------------------------------------------------------------
        #region helper methods

        public void LogUser(string script)
        {
            #region User Stats

            // add headers if the file doesn't exist
            // list of target headers for desired dose stats
            // in this case I want to display the headers every time so i can verify which target the distance is being measured for
            // this is due to the inconsistency in target naming (PTV1/2 vs ptv45/79.2) -- these can be removed later when cleaning up the data
            if (!File.Exists(userLogPath))
            {
                List<string> dataHeaderList = new List<string>();
                dataHeaderList.Add("User");
                dataHeaderList.Add("Script");
                dataHeaderList.Add("Date");
                dataHeaderList.Add("DayOfWeek");
                dataHeaderList.Add("Time");
                dataHeaderList.Add("ID");
                dataHeaderList.Add("RandomID");
                dataHeaderList.Add("CourseName");
                dataHeaderList.Add("PlanName");
                dataHeaderList.Add("DosePerFraction");
                dataHeaderList.Add("NumberOfFractions");

                string concatDataHeader = string.Join(",", dataHeaderList.ToArray());

                userLogCsvContent.AppendLine(concatDataHeader);
            }

            List<object> userStatsList = new List<object>();

            string userId = user;
            string scriptId = script;
            string date = string.Format("{0}/{1}/{2}", day, month, year);
            string dayOfWeek = day;
            string time = string.Format("{0}:{1}", hour, minute);
            string ptId = id;
            string randomPtId = randomId;
            string course = day;
            string plan = planName;
            string dosePerFx = dosePerFraction.ToString();
            string numFx = Fractions.ToString();

            userStatsList.Add(userId);
            userStatsList.Add(scriptId);
            userStatsList.Add(date);
            userStatsList.Add(dayOfWeek);
            userStatsList.Add(time);
            userStatsList.Add(ptId);
            userStatsList.Add(randomPtId);
            userStatsList.Add(course);
            userStatsList.Add(plan);
            userStatsList.Add(dosePerFx);
            userStatsList.Add(numFx);

            string concatUserStats = string.Join(",", userStatsList.ToArray());

            userLogCsvContent.AppendLine(concatUserStats);


            #endregion Target Dose Stats

            #region Write Files

            File.AppendAllText(userLogPath, userLogCsvContent.ToString());

            #endregion

            #region Collection Complete Message

            //MessageBox.Show(" Data collection complete. Thanks for helping us collect data!\n\n\t\t\t:)");

            #endregion
        }
        public class WaitCursor : IDisposable
        {
            private Cursor _previousCursor;

            public WaitCursor()
            {
                _previousCursor = Mouse.OverrideCursor;

                Mouse.OverrideCursor = Cursors.Wait;
            }

            #region IDisposable Members

            public void Dispose()
            {
                Mouse.OverrideCursor = _previousCursor;
            }

            #endregion
        }
        #endregion
        //---------------------------------------------------------------------------------
    }
}
