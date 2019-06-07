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

namespace PlanReview
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
        public string script = "PlanReview";
        public PlanSetup ps;
        public PlanningItem pitem;
        public StructureSet ss;
        public IEnumerable<Structure> sorted_gtvList;
        public IEnumerable<Structure> sorted_ctvList;
        public IEnumerable<Structure> sorted_itvList;
        public IEnumerable<Structure> sorted_ptvList;
        public IEnumerable<Structure> sorted_targetList;
        public IEnumerable<Structure> sorted_oarList;
        public IEnumerable<Structure> sorted_structureList;
        public IEnumerable<Structure> sorted_emptyStructuresList; 
        public double? Fractions;
        public string approvalStatus;
        public string colsCsvFilePath_randomized_patientSpecific;
        public string rowsCsvFilePath_randomized_patientSpecific;
        public string rowsCsvFilePath_randomized_structureSpecific;
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
        public double planMaxDose;
        //public bool volAbs;
        //public bool doseAbs;
        public bool isGrady;
        public string primaryPhysician;
        public ConstraintType constraintType = new ConstraintType();
        public ConformityType conformityType = new ConformityType();
        public double mainbinWidth;
        public string courseHeader;
        
        

        #endregion
        //---------------------------------------------------------------------------------
        #region objects used for binding

        //public string Title { get; set; }

        public class PrimaryPhysician
        {
            public string Name { get; set; }
        }
        public class TargetStats
        {
            public string color { get; set; }
            public string targetId { get; set; }
            public double targetVolume { get; set; }
            public double d95 { get; set; }
            public double min { get; set; }
            public double min03 { get; set; }
            public double max { get; set; }
            public double max03 { get; set; }
            public double mean { get; set; }
            public string segments { get; set; }
        }
        public class StructureStats
        {
            public string color { get; set; }
            public string id { get; set; }
            public double volume { get; set; }
            public double max { get; set; }
            public double max03 { get; set; }
            public double mean { get; set; }
        }
        public class OverlapStats
        {
            public string structureId { get; set; }
            public double structureVolume { get; set; }
            public string ptvId { get; set; }
            public double distance { get; set; }
            public double structureOverlapAbs { get; set; }
            public double structureOverlapPct { get; set; }
            public double targetOverlapPct { get; set; }
        }
        public class ManualDoseCheck
        {
            public string volumeAtDose { get; set; }
            public string doseAtVolume { get; set; }
        }
        public class ConformityType
        {
            public bool CI { get; set; }
            public bool R50 { get; set; }
        }
        public class ConformityIndex
        {
            public string targetId { get; set; }
            public string ratioType { get; set; }
            public double idealLimit { get; set; }
            public double upperLimit { get; set; }
            public double conformity { get; set; }
            public string result { get; set; }
            public string rangeLimit { get; set; }
        }
        public class ConstraintType
        {
            public bool AbsoluteVolume { get; set; }
            public bool RelativeVolume { get; set; }
            public bool MaxDose { get; set; }
            public bool MeanDose { get; set; }
        }
        public class CustomConstraint
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string DoseValue { get; set; }
            public string Limit { get; set; }
            public string Result { get; set; }
            public string Other { get; set; }
        }
        public class VolumeDoseConstraint
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string DoseValue { get; set; }
            public string Limit { get; set; }
            public string Result { get; set; }
            public string Other { get; set; }
            public VolumeDoseConstraint(string id, DVHData dvh, double volumeLimit, double doseLimit, string volumeLabel)
            {
                Id = id;
                double volumeAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvh, doseLimit), 3);
                Type = string.Format("V{0} Gy", doseLimit);
                DoseValue = string.Format("{0} {1}", volumeAtDose, volumeLabel);
                Limit = string.Format("{0} {1}", volumeLimit, volumeLabel);
                Result = volumeAtDose <= volumeLimit ? "MET" : "NOT MET";
                double valueAtLimit = Double.NaN;
                if (volumeLabel == "%" && volumeLimit == 100)
                {
                    valueAtLimit = Math.Round(dvh.MinDose.Dose, 1);
                }
                else
                {
                    valueAtLimit = Math.Round(DoseChecks.getDoseAtVolume(dvh, volumeLimit), 1);
                }
                
                Other = string.Format("D{0}{1} = {2}", volumeLimit, volumeLabel, valueAtLimit);
            }
        }
        public class MaxDoseConstraint
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string DoseValue { get; set; }
            public string Limit { get; set; }
            public string Result { get; set; }
            public string Other { get; set; }
            public MaxDoseConstraint(string sId, DVHData dvh, double doseLimit, string doseLabel)
            {
                Id = sId;
                Type = string.Format("Max");
                double maxDose = Math.Round(dvh.MaxDose.Dose, 3);
                DoseValue = string.Format("{0} {1}", maxDose, doseLabel);
                Limit = string.Format("{0} {1}", doseLimit, doseLabel);
                Result = maxDose <= doseLimit ? "MET" : "NOT MET";
                double valueAtLimit = Math.Round(DoseChecks.getDoseAtVolume(dvh, 0.03), 3);
                Other = string.Format("D0.03cc = {0}", valueAtLimit);
            }
        }
        public class MeanDoseConstraint
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public string DoseValue { get; set; }
            public string Limit { get; set; }
            public string Result { get; set; }
            public string Other { get; set; }
            public MeanDoseConstraint(string id, DVHData dvh, double doseLimit, string doseLabel, string emptySoNotLikeMaxDoseConstraint)
            {
                Id = id;
                Type = string.Format("Mean");
                double meanDose = Math.Round(dvh.MeanDose.Dose, 3);
                DoseValue = string.Format("{0} {1}", meanDose, doseLabel);
                Limit = string.Format("{0} {1}", doseLimit, doseLabel);
                Result = meanDose <= doseLimit ? "MET" : "NOT MET";
            }
        }

        #endregion
        //---------------------------------------------------------------------------------
        #region paths and string builders for data collection scripts

        #region Script Use Log

        //TODO: add file to collect various data on script used, time, user, etc. 
        public string userLogPath = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\esapi\\projects\\__Logs\\PlanReview_UserLog.csv";

        public StringBuilder userLogCsvContent = new StringBuilder();

        #endregion

        #endregion
        //---------------------------------------------------------------------------------
        #region event controls

        // various event controls
        public void Structure_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (new WaitCursor())
            {
                ComboBox senderComboBox = (ComboBox)sender;

                StringBuilder randomized_rowCsvStringBuilder_patientSpecific = new StringBuilder();
                StringBuilder randomized_colCsvStringBuilder_patientSpecific = new StringBuilder();
                StringBuilder randomized_rowCsvStringBuilder_structureSpecific = new StringBuilder();

                // Change the length of the text box depending on what the user has 
                // selected and committed using the SelectionLength property.
                if ((senderComboBox.SelectedIndex >= 0) && (pitem.Dose != null))
                {
                    Structure SelectedStructure = senderComboBox.Items[senderComboBox.SelectedIndex] as Structure;

                    string sName = SelectedStructure.Id.ToString().ToLower().Replace(" ", string.Empty).Split(':').First();
                    double sVolume = Math.Round(SelectedStructure.Volume, 3);
                    string sColor = "#" + SelectedStructure.Color.ToString().Substring(3, 6);

                    string doseLabel = "Gy";
                    DVHData dvhData = pitem?.GetDVHCumulativeData(SelectedStructure,
                                            DoseValuePresentation.Absolute,
                                            VolumePresentation.Relative, 0.1);
                    if (SelectedStructure.Id.ToString().ToLower().Contains("body") ||
                        SelectedStructure.Id.ToString().ToLower().Contains("external"))
                    {
                        StructureVolume_TB.Text = string.Format("Vol: {0:F1}cc  Dmax: {1:F3}", SelectedStructure.Volume,
                                                                                              DoseChecks.getMaxDose(dvhData),
                                                                                            doseLabel);
                    }
                    else
                    {
                        StructureVolume_TB.Text = string.Format("Vol: {0:F3}cc  Dmax: {1:F3}{2}", SelectedStructure.Volume,
                                                                                            DoseChecks.getMaxDose(dvhData),
                                                                                            doseLabel);
                    }
                    if (!sorted_targetList.Contains(SelectedStructure) &&
                        !SelectedStructure.Id.ToString().ToLower().Contains("body") &&
                        !SelectedStructure.Id.ToString().ToLower().Contains("external"))
                    {
                        foreach (Structure target in sorted_ptvList)
                        {
                            string tName = target.Id.ToString().ToLower().Replace(" ", string.Empty).Split(':').First();
                            double tVolume = Math.Round(target.Volume, 3);
                            string tColor = "#" + target.Color.ToString().Substring(3, 6);

                            OverlapStats overlapInfo = new OverlapStats();
                            overlapInfo.structureId = SelectedStructure?.Id.ToString().Split(':').First();
                            overlapInfo.ptvId = target?.Id.ToString().Split(':').First();
                            overlapInfo.structureOverlapAbs = Math.Round(CalculateOverlap.VolumeOverlap(target, SelectedStructure), 3);
                            overlapInfo.structureOverlapPct = overlapInfo.structureOverlapAbs == 0 ? 0 : Math.Round(CalculateOverlap.PercentOverlap(SelectedStructure, overlapInfo.structureOverlapAbs), 1);
                            overlapInfo.targetOverlapPct = overlapInfo.structureOverlapAbs == 0 ? 0 : Math.Round(CalculateOverlap.PercentOverlap(target, overlapInfo.structureOverlapAbs), 1);
                            overlapInfo.distance = overlapInfo.structureOverlapAbs > 0 ? 0 : Math.Round(CalculateOverlap.ShortestDistance(target, SelectedStructure), 1);

                            OverlapInfo_DG.Items.Add(overlapInfo);

                            // write to csv
                            #region target proximity data csv //
                            //if (isGrady == false)
                            //{


                            //// final paths
                            //string rows_finalCsvPath_patientSpecific = rowsCsvFilePath_randomized_patientSpecific + "\\" + sName + "_" + tName + "_proximity_rows.csv";
                            //string cols_finalCsvPath_patientSpecific = colsCsvFilePath_randomized_patientSpecific + "\\" + sName + "_" + tName + "_proximity_cols.csv";
                            //string rows_finalCsvPath_structureSpecific = rowsCsvFilePath_randomized_structureSpecific + "\\" + courseHeader + "_" + sName + "_" + tName + "_proximity.csv";


                            //// rows
                            //randomized_rowCsvStringBuilder_patientSpecific.Clear();
                            //randomized_rowCsvStringBuilder_structureSpecific.Clear();
                            //randomized_rowCsvStringBuilder_patientSpecific.AppendLine("randomId,primaryPhysician,courseHeader,planId,approvalStatus,proximalTarget,targetColor,proximalTargetVolume,structureId,structureColor,structureVolume,distanceFrom_" + tName + ",overlapAbsWith_" + tName + ",structureOverlapPctWith_" + tName + "," + tName + "OverlapPct");
                            //randomized_rowCsvStringBuilder_patientSpecific.AppendLine(randomId + "," + primaryPhysician + "," + courseHeader + "," + planName + "," + approvalStatus + "," + tName + "," + tColor + "," + tVolume + "," +
                            //                                            sName + "," + sColor + "," + sVolume + "," + overlapInfo.distance + "," + overlapInfo.structureOverlapAbs + "," + overlapInfo.structureOverlapPct + "," + overlapInfo.targetOverlapPct);
                            //if (!File.Exists(rows_finalCsvPath_structureSpecific))
                            //{
                            //    randomized_rowCsvStringBuilder_structureSpecific.AppendLine("randomId,primaryPhysician,courseHeader,planId,approvalStatus,proximalTarget,targetColor,proximalTargetVolume,structureId,structureColor,structureVolume,distanceFrom_" + tName + ",overlapAbsWith_" + tName + ",structureOverlapPctWith_" + tName + "," + tName + "OverlapPct");
                            //}
                            //randomized_rowCsvStringBuilder_structureSpecific.AppendLine(randomId + "," + primaryPhysician + "," + courseHeader + "," + planName + "," + approvalStatus + "," + tName + "," + tColor + "," + tVolume + "," +
                            //                                            sName + "," + sColor + "," + sVolume + "," + overlapInfo.distance + "," + overlapInfo.structureOverlapAbs + "," + overlapInfo.structureOverlapPct + "," + overlapInfo.targetOverlapPct);

                            //// columns
                            //randomized_colCsvStringBuilder_patientSpecific.Clear();
                            //randomized_colCsvStringBuilder_patientSpecific.AppendLine("randomId,\t\t\t" + randomId);
                            //randomized_colCsvStringBuilder_patientSpecific.AppendLine("primaryPhysician,\t\t" + primaryPhysician);
                            //randomized_colCsvStringBuilder_patientSpecific.AppendLine("courseHeader,\t\t" + courseHeader);
                            //randomized_colCsvStringBuilder_patientSpecific.AppendLine("planId,\t\t\t" + planName);
                            //randomized_colCsvStringBuilder_patientSpecific.AppendLine("approvalStatus,\t\t" + approvalStatus);
                            //randomized_colCsvStringBuilder_patientSpecific.AppendLine("proximalTarget,\t\t" + tName);
                            //randomized_colCsvStringBuilder_patientSpecific.AppendLine("targetColor,\t\t" + tColor);
                            //randomized_colCsvStringBuilder_patientSpecific.AppendLine("proximalTargetVolume,\t" + tVolume);
                            //randomized_colCsvStringBuilder_patientSpecific.AppendLine("structureId,\t\t\t" + sName);
                            //randomized_colCsvStringBuilder_patientSpecific.AppendLine("structureColor,\t\t" + sColor);
                            //randomized_colCsvStringBuilder_patientSpecific.AppendLine("structureVolume,\t\t" + sVolume);
                            //randomized_colCsvStringBuilder_patientSpecific.AppendLine("distanceFrom_" + tName + ",\t\t" + overlapInfo.distance);
                            //randomized_colCsvStringBuilder_patientSpecific.AppendLine("overlapAbsWith_" + tName + ",\t\t" + overlapInfo.structureOverlapAbs);
                            //randomized_colCsvStringBuilder_patientSpecific.AppendLine("structureOverlapPctWith_" + tName + ",\t" + overlapInfo.structureOverlapPct);
                            //randomized_colCsvStringBuilder_patientSpecific.AppendLine(tName + "OverlapPct" + ",\t\t" + overlapInfo.targetOverlapPct);

                            //// write csv files
                            //File.WriteAllText(rows_finalCsvPath_patientSpecific, randomized_rowCsvStringBuilder_patientSpecific.ToString());
                            //File.WriteAllText(cols_finalCsvPath_patientSpecific, randomized_colCsvStringBuilder_patientSpecific.ToString());
                            //File.AppendAllText(rows_finalCsvPath_structureSpecific, randomized_rowCsvStringBuilder_structureSpecific.ToString());


                            //}
                            #endregion
                        }
                    }
                    UpdateDvh();
                }
                else if ((senderComboBox.SelectedIndex >= 0) && (pitem.Dose == null))
                {
                    Structure SelectedStructure = senderComboBox.Items[senderComboBox.SelectedIndex] as Structure;

                    string sName = SelectedStructure.Id.ToString().ToLower().Replace(" ", string.Empty).Split(':').First();
                    double sVolume = Math.Round(SelectedStructure.Volume, 3);
                    string sColor = "#" + SelectedStructure.Color.ToString().Substring(3, 6);

                    if (SelectedStructure.Id.ToString().ToLower().Contains("body") ||
                        SelectedStructure.Id.ToString().ToLower().Contains("external"))
                    {
                        StructureVolume_TB.Text = string.Format("Vol: {0:F1}cc", SelectedStructure.Volume);
                    }
                    else
                    {
                        StructureVolume_TB.Text = string.Format("Vol: {0:F3}cc", SelectedStructure.Volume);
                    }
                    if (!sorted_targetList.Contains(SelectedStructure) &&
                        !SelectedStructure.Id.ToString().ToLower().Contains("body") &&
                        !SelectedStructure.Id.ToString().ToLower().Contains("external"))
                    {
                        foreach (Structure target in sorted_ptvList)
                        {
                            string tName = target.Id.ToString().ToLower().Replace(" ", string.Empty).Split(':').First();
                            double tVolume = Math.Round(target.Volume, 3);
                            string tColor = "#" + target.Color.ToString().Substring(3, 6);

                            OverlapStats overlapInfo = new OverlapStats();
                            overlapInfo.structureId = SelectedStructure?.Id.ToString().Split(':').First();
                            overlapInfo.ptvId = target?.Id.ToString().Split(':').First();
                            overlapInfo.structureOverlapAbs = Math.Round(CalculateOverlap.VolumeOverlap(target, SelectedStructure), 3);
                            overlapInfo.structureOverlapPct = overlapInfo.structureOverlapAbs == 0 ? 0 : Math.Round(CalculateOverlap.PercentOverlap(SelectedStructure, overlapInfo.structureOverlapAbs), 1);
                            overlapInfo.targetOverlapPct = overlapInfo.structureOverlapAbs == 0 ? 0 : Math.Round(CalculateOverlap.PercentOverlap(target, overlapInfo.structureOverlapAbs), 1);
                            overlapInfo.distance = overlapInfo.structureOverlapAbs > 0 ? 0 : Math.Round(CalculateOverlap.ShortestDistance(target, SelectedStructure), 1);

                            OverlapInfo_DG.Items.Add(overlapInfo);

                            // write to csv
                            #region target proximity data - csv //
                            //if (isGrady == false)
                            //{
                                

                            //    // final paths
                            //    string rows_finalCsvPath_patientSpecific = rowsCsvFilePath_randomized_patientSpecific + "\\" + sName + "_" + tName + "_proximity_rows.csv";
                            //    string cols_finalCsvPath_patientSpecific = colsCsvFilePath_randomized_patientSpecific + "\\" + sName + "_" + tName + "_proximity_cols.csv";
                            //    string rows_finalCsvPath_structureSpecific = rowsCsvFilePath_randomized_structureSpecific + "\\" + courseHeader + "_" + sName + "_" + tName + "_proximity.csv";


                            //    // rows
                            //    randomized_rowCsvStringBuilder_patientSpecific.Clear();
                            //    randomized_rowCsvStringBuilder_structureSpecific.Clear();
                            //    randomized_rowCsvStringBuilder_patientSpecific.AppendLine("randomId,primaryPhysician,courseHeader,planId,approvalStatus,proximalTarget,targetColor,proximalTargetVolume,structureId,structureColor,structureVolume,distanceFrom_" + tName + ",overlapAbsWith_" + tName + ",structureOverlapPctWith_" + tName + "," + tName + "OverlapPct");
                            //    randomized_rowCsvStringBuilder_patientSpecific.AppendLine(randomId + "," + primaryPhysician + "," + courseHeader + "," + planName + "," + approvalStatus + "," + tName + "," + tColor + "," + tVolume + "," +
                            //                                                sName + "," + sColor + "," + sVolume + "," + overlapInfo.distance + "," + overlapInfo.structureOverlapAbs + "," + overlapInfo.structureOverlapPct + "," + overlapInfo.targetOverlapPct);
                            //    if (!File.Exists(rows_finalCsvPath_structureSpecific))
                            //    {
                            //        randomized_rowCsvStringBuilder_structureSpecific.AppendLine("randomId,primaryPhysician,courseHeader,planId,approvalStatus,proximalTarget,targetColor,proximalTargetVolume,structureId,structureColor,structureVolume,distanceFrom_" + tName + ",overlapAbsWith_" + tName + ",structureOverlapPctWith_" + tName + "," + tName + "OverlapPct");
                            //    }
                            //    randomized_rowCsvStringBuilder_structureSpecific.AppendLine(randomId + "," + primaryPhysician + "," + courseHeader + "," + planName + "," + approvalStatus + "," + tName + "," + tColor + "," + tVolume + "," +
                            //                                                sName + "," + sColor + "," + sVolume + "," + overlapInfo.distance + "," + overlapInfo.structureOverlapAbs + "," + overlapInfo.structureOverlapPct + "," + overlapInfo.targetOverlapPct);

                            //    // columns
                            //    randomized_colCsvStringBuilder_patientSpecific.Clear();
                            //    randomized_colCsvStringBuilder_patientSpecific.AppendLine("randomId,\t\t\t" + randomId);
                            //    randomized_colCsvStringBuilder_patientSpecific.AppendLine("primaryPhysician,\t\t" + primaryPhysician);
                            //    randomized_colCsvStringBuilder_patientSpecific.AppendLine("courseHeader,\t\t" + courseHeader);
                            //    randomized_colCsvStringBuilder_patientSpecific.AppendLine("planId,\t\t\t" + planName);
                            //    randomized_colCsvStringBuilder_patientSpecific.AppendLine("approvalStatus,\t\t" + approvalStatus);
                            //    randomized_colCsvStringBuilder_patientSpecific.AppendLine("proximalTarget,\t\t" + tName);
                            //    randomized_colCsvStringBuilder_patientSpecific.AppendLine("targetColor,\t\t" + tColor);
                            //    randomized_colCsvStringBuilder_patientSpecific.AppendLine("proximalTargetVolume,\t" + tVolume);
                            //    randomized_colCsvStringBuilder_patientSpecific.AppendLine("structureId,\t\t\t" + sName);
                            //    randomized_colCsvStringBuilder_patientSpecific.AppendLine("structureColor,\t\t" + sColor);
                            //    randomized_colCsvStringBuilder_patientSpecific.AppendLine("structureVolume,\t\t" + sVolume);
                            //    randomized_colCsvStringBuilder_patientSpecific.AppendLine("distanceFrom_" + tName + ",\t\t" + overlapInfo.distance);
                            //    randomized_colCsvStringBuilder_patientSpecific.AppendLine("overlapAbsWith_" + tName + ",\t\t" + overlapInfo.structureOverlapAbs);
                            //    randomized_colCsvStringBuilder_patientSpecific.AppendLine("structureOverlapPctWith_" + tName + ",\t" + overlapInfo.structureOverlapPct);
                            //    randomized_colCsvStringBuilder_patientSpecific.AppendLine(tName + "OverlapPct" + ",\t\t" + overlapInfo.targetOverlapPct);

                            //    // write csv files
                            //    File.WriteAllText(rows_finalCsvPath_patientSpecific, randomized_rowCsvStringBuilder_patientSpecific.ToString());
                            //    File.WriteAllText(cols_finalCsvPath_patientSpecific, randomized_colCsvStringBuilder_patientSpecific.ToString());
                            //    File.AppendAllText(rows_finalCsvPath_structureSpecific, randomized_rowCsvStringBuilder_structureSpecific.ToString());
                            //    //File.AppendAllText(rows_finalCsvPath_structureSpecific, randomized_rowCsvStringBuilder_structureSpecific.ToString());

                                
                            //}
                            #endregion
                        }
                    }
                }
            }
        }
        private void Print_Visual(object sender, RoutedEventArgs e)
        {
            // buttons
            PrintVisual_Btn.Visibility = Visibility.Hidden;
            AddNote_Btn.Visibility = Visibility.Hidden;
            ClearNote_Btn.Visibility = Visibility.Hidden;
            AddCustomConstraint_Btn.Visibility = Visibility.Collapsed;
            AddConformityToolBox_Btn.Visibility = Visibility.Collapsed;

            Constraints_DG.MaxHeight = 1000;
            StructureInfo_DG.MaxHeight = 1000;
            TargetCoverage_DG.MaxHeight = 1000;
            OverlapInfo_DG.MaxHeight = 1000;

            // stackpanels and groupboxes
            ManualCalc_SP.Visibility = Visibility.Collapsed;
            ToolBox_SP.Visibility = Visibility.Collapsed;
            if (Note_TB.Text == "")
            {
                Notes_GB.Visibility = Visibility.Collapsed;
                if (!Ratio_DG.HasItems)
                {
                    NotesAndConformityDG_SP.Visibility = Visibility.Collapsed;
                }
            }
            if (!Ratio_DG.HasItems)
            {
                Ratio_DG.Visibility = Visibility.Collapsed;
                if (Note_TB.Text == "")
                {
                    NotesAndConformityDG_SP.Visibility = Visibility.Collapsed;
                }
            }
            if (!OverlapInfo_DG.HasItems)
            {
                TargetProx_GB.Visibility = Visibility.Collapsed;
            }
            var colToRemove1 = Constraints_DG.ColumnFromDisplayIndex(6);
            var colToRemove2 = OverlapInfo_DG.ColumnFromDisplayIndex(6);
            var colToRemove3 = StructureInfo_DG.ColumnFromDisplayIndex(5);
            var colToRemove4 = TargetCoverage_DG.ColumnFromDisplayIndex(8);
            var colToRemove5 = Ratio_DG.ColumnFromDisplayIndex(5);

            colToRemove1.Visibility = Visibility.Hidden;
            colToRemove2.Visibility = Visibility.Hidden;
            colToRemove3.Visibility = Visibility.Hidden;
            colToRemove4.Visibility = Visibility.Hidden;
            colToRemove5.Visibility = Visibility.Hidden;

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(MainGrid, string.Format("{0}_PlanOverview_{1}", id, planName));
            }
        }

        #region input changed events

        #region basic events

        public void absDose_CB_Checked(object sender, RoutedEventArgs e)
        {
            UpdateDvh();
        }
        public void absDose_CB_UnChecked(object sender, RoutedEventArgs e)
        {
           UpdateDvh();
        }
        public void absVolume_CB_Checked(object sender, RoutedEventArgs e)
        {
            UpdateDvh();
        }
        public void absVolume_CB_UnChecked(object sender, RoutedEventArgs e)
        {
            UpdateDvh();
        }
        public void volumeAtDose_Input_Changed(object sender, TextChangedEventArgs e)
        {
            UpdateDvh();
        }
        public void doseAtVolume_Input_Changed(object sender, TextChangedEventArgs e)
        {
            UpdateDvh();
        }
        private void OverlapRemoveButton_Clicked(object sender, RoutedEventArgs e)
        {
            OverlapInfo_DG.Items.RemoveAt(OverlapInfo_DG.SelectedIndex);
        }
        private void ConstraintRemoveButton_Clicked(object sender, RoutedEventArgs e)
        {
            Constraints_DG.Items.RemoveAt(Constraints_DG.SelectedIndex);
        }
        private void TargetDoseRemoveButton_Clicked(object sender, RoutedEventArgs e)
        {
            TargetCoverage_DG.Items.RemoveAt(TargetCoverage_DG.SelectedIndex);
        }
        private void StructureInfoRemoveButton_Clicked(object sender, RoutedEventArgs e)
        {
            StructureInfo_DG.Items.RemoveAt(StructureInfo_DG.SelectedIndex);
        }
        private void ConformityItemRemoveButton_Clicked(object sender, RoutedEventArgs e)
        {
            Ratio_DG.Items.RemoveAt(Ratio_DG.SelectedIndex);
        }

        #endregion

        #region toolbox controls

        private void ClearConstraintLimits_Clicked(object sender, RoutedEventArgs e)
        {
            DoseLimit_Input.Text = "";
            VolumeLimit_Input.Text = "";
        }
        // add custom constraint toolbox event
        private void AddCustomConstraint_Clicked(object sender, RoutedEventArgs e)
        {
            if (CustomConstraint_GB.Visibility == Visibility.Visible)
            {
                if (CICalc_GB.Visibility == Visibility.Hidden)
                {
                    ToolBox_SP.Visibility = Visibility.Collapsed;
                }
                CustomConstraint_GB.Visibility = Visibility.Hidden;
                AddCustomConstraint_Btn.Content = "Add Custom Constraints";
            }
            else
            {
                if (ToolBox_SP.Visibility == Visibility.Collapsed)
                {
                    ToolBox_SP.Visibility = Visibility.Visible;
                }
                CustomConstraint_GB.Visibility = Visibility.Visible;
                AddCustomConstraint_Btn.Content = "Hide Custom Constraints ToolBox";
            }
        }
        // add conformity toolbox event
        private void AddConformityToolBox_Clicked(Object sender, RoutedEventArgs e)
        {
            if (CICalc_GB.Visibility == Visibility.Visible)
            {
                if (CustomConstraint_GB.Visibility == Visibility.Hidden)
                {
                    ToolBox_SP.Visibility = Visibility.Collapsed;
                }
                CICalc_GB.Visibility = Visibility.Hidden;
                AddConformityToolBox_Btn.Content = "Add Conformity ToolBox";
            }
            else
            {
                if (ToolBox_SP.Visibility == Visibility.Collapsed)
                {
                    ToolBox_SP.Visibility = Visibility.Visible;
                }
                CICalc_GB.Visibility = Visibility.Visible;
                AddConformityToolBox_Btn.Content = "Hide Confromity ToolBox";
            }
        }
        // add note event
        private void AddNote_Clicked(Object sender, RoutedEventArgs e)
        {
            if (Notes_GB.Visibility == Visibility.Visible)
            {
                if (Ratio_GB.Visibility == Visibility.Hidden)
                {
                    NotesAndConformityDG_SP.Visibility = Visibility.Collapsed;
                }
                Notes_GB.Visibility = Visibility.Hidden;
                AddNote_Btn.Content = "Add Note";
            }
            else
            {
                if (NotesAndConformityDG_SP.Visibility == Visibility.Collapsed)
                {
                    NotesAndConformityDG_SP.Visibility = Visibility.Visible;
                }
                Notes_GB.Visibility = Visibility.Visible;
                AddNote_Btn.Content = "Hide Note";
            }
        }
        private void ClearNote_Clicked(Object sender, RoutedEventArgs e)
        {
            Note_TB.Text = "";
        }
        private void ConstraintTypeSelected(Object sender, RoutedEventArgs e)
        {
            if (ConstraintType_ComboBox.SelectedItem.ToString() ==  "Absolute Volume")
            {
                DoseLimit_SP.Visibility = Visibility.Visible;
                VolumeLimit_SP.Visibility = Visibility.Visible;
                constraintType.AbsoluteVolume = true;
                constraintType.RelativeVolume = false;
                constraintType.MeanDose = false;
                constraintType.MaxDose = false;
                VolumeLimitLabel_TB.Text = "cc";
            }
            if (ConstraintType_ComboBox.SelectedItem.ToString() ==  "Relative Volume")
            {
                DoseLimit_SP.Visibility = Visibility.Visible;
                VolumeLimit_SP.Visibility = Visibility.Visible;
                constraintType.AbsoluteVolume = false;
                constraintType.RelativeVolume = true;
                constraintType.MeanDose = false;
                constraintType.MaxDose = false;
                VolumeLimitLabel_TB.Text = "%";
            }
            if (ConstraintType_ComboBox.SelectedItem.ToString() ==  "Max")
            {
                DoseLimit_SP.Visibility = Visibility.Visible;
                VolumeLimit_SP.Visibility = Visibility.Collapsed;
                constraintType.AbsoluteVolume = false;
                constraintType.RelativeVolume = false;
                constraintType.MaxDose = true;
                constraintType.MeanDose = false;
            }
            if (ConstraintType_ComboBox.SelectedItem.ToString() ==  "Mean")
            {
                DoseLimit_SP.Visibility = Visibility.Visible;
                VolumeLimit_SP.Visibility = Visibility.Collapsed;
                constraintType.AbsoluteVolume = false;
                constraintType.RelativeVolume = false;
                constraintType.MaxDose = false;
                constraintType.MeanDose = true;
            }
        }
        private void AddConstraint_Clicked(Object sender, RoutedEventArgs e)
        {
            if ((ConstraintStructure_ComboBox.SelectedIndex < 0) || 
                (ConstraintType_ComboBox.SelectedIndex < 0) ||
                (DoseLimit_Input.Text == null) || 
                ((constraintType.AbsoluteVolume) && (VolumeLimit_Input.Text == "")) ||
                ((constraintType.RelativeVolume) && (VolumeLimit_Input.Text == "")))
            {
                MessageBox.Show("Required input missing.");
            }
            else
            {
                Structure s = ConstraintStructure_ComboBox.SelectedItem as Structure;

                DVHData dvhAR = pitem.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, mainbinWidth);
                DVHData dvhAA = pitem.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, mainbinWidth);
                bool doseUnitIsGy = dvhAA.MaxDose.Unit == DoseValue.DoseUnit.Gy ? true : false;

                string doseLabel = doseUnitIsGy ? "Gy" : "cGy";
                string ccLabel = "cc";
                string pctLabel = "%";
                string sToString = s.Id.ToString().Split(':').First();
                double inputDoseLimit = Double.NaN;
                double inputVolumeLimit = Double.NaN;
                if (DoseLimit_Input.Text != null)
                {
                    Double.TryParse(DoseLimit_Input.Text, out inputDoseLimit);
                }
                if (VolumeLimit_Input.Text != null)
                {
                    Double.TryParse(VolumeLimit_Input.Text, out inputVolumeLimit);
                }

                if (constraintType.AbsoluteVolume)
                {
                    var constraint = new VolumeDoseConstraint(sToString, dvhAA, inputVolumeLimit, inputDoseLimit, ccLabel);
                    Constraints_DG.Items.Add(constraint);
                }
                else if (constraintType.RelativeVolume)
                {
                    var constraint = new VolumeDoseConstraint(sToString, dvhAR, inputVolumeLimit, inputDoseLimit, pctLabel);
                    Constraints_DG.Items.Add(constraint);
                }
                else if (constraintType.MaxDose)
                {
                    var constraint = new MaxDoseConstraint(sToString, dvhAA, inputDoseLimit, doseLabel);
                    Constraints_DG.Items.Add(constraint);
                }
                else if (constraintType.MeanDose)
                {
                    var constraint = new MeanDoseConstraint(sToString, dvhAA, inputDoseLimit, doseLabel, string.Empty);
                    Constraints_DG.Items.Add(constraint);
                }
            }
        }
        private void RatioType_SelectionChanged(Object sender, RoutedEventArgs e)
        {
            if (RatioType_ComboBox.SelectedItem.ToString() == "R50")
            {
                conformityType.CI = false;
                conformityType.R50 = true;
            }
            if (RatioType_ComboBox.SelectedItem.ToString() == "CI")
            {
                conformityType.CI = true;
                conformityType.R50 = false;
            }
        }
        private void CalculateConformity_Clicked(Object sender, RoutedEventArgs e)
        {
            if ((RatioStructure_ComboBox.SelectedIndex > -1) &&
                (RatioTarget_ComboBox.SelectedIndex > -1) &&
                (RxDose_Input.Text != ""))
            {
                if (NotesAndConformityDG_SP.Visibility == Visibility.Collapsed) { NotesAndConformityDG_SP.Visibility = Visibility.Visible; }
                Ratio_GB.Visibility = Visibility.Visible;

                ConformityIndex index = new ConformityIndex();

                double rx = Double.NaN;
                double volumeAtDose = Double.NaN;

                Structure s = RatioStructure_ComboBox.SelectedItem as Structure;
                Structure t = RatioTarget_ComboBox.SelectedItem as Structure;

                DVHData dvhAA = pitem.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, mainbinWidth);

                index.targetId = t.Id.ToString().Split(':').First();

                if (RxDose_Input.Text != null)
                {
                    Double.TryParse(RxDose_Input.Text, out rx);
                }
                if (conformityType.CI)
                {
                    index.ratioType = "CI";
                    index.idealLimit = 1.5;
                    index.upperLimit = 2.0;
                    index.rangeLimit = "1.0 < CI < 2.0";

                    volumeAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAA, rx), 3);

                    index.conformity = Math.Round((volumeAtDose / t.Volume), 2);
                    if ((index.conformity <= index.idealLimit) && (index.conformity >= 1))
                    {
                        index.result = "MET";
                    }
                    else if ((index.conformity > index.idealLimit) && (index.conformity <= index.upperLimit))
                    {
                        index.result = "Acceptable";
                    }
                    else if (index.conformity < 1)
                    {
                        index.result = "Acceptable";
                    }
                    else { index.result = "NOT MET"; }

                    ConformityRangeLimit_TB.Text = index.rangeLimit;
                    RatioCalcResult_TB.Text = index.conformity.ToString();

                    Ratio_DG.Items.Add(index);
                }
                else if (conformityType.R50)
                {
                    double limit1, limit2, limit3, limit4;
                    index.ratioType = "R50";
                    R50Constraint.LimitsFromVolume(t.Volume, out limit1, out limit2, out limit3, out limit4);
                    index.idealLimit = Math.Round(limit1, 2);
                    index.upperLimit = Math.Round(limit2, 2);
                    index.rangeLimit = string.Format("{0:F2} < R50 < {1:F2}", index.idealLimit, index.upperLimit);

                    volumeAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvhAA, (rx/2)), 3);

                    index.conformity = Math.Round((volumeAtDose / t.Volume), 2);
                    if (index.conformity <= index.idealLimit)
                    {
                        index.result = "MET";
                    }
                    else if ((index.conformity > index.idealLimit) && (index.conformity <= index.upperLimit))
                    {
                        index.result = "Acceptable";
                    }
                    else { index.result = "NOT MET"; }

                    ConformityRangeLimit_TB.Text = index.rangeLimit;
                    RatioCalcResult_TB.Text = index.conformity.ToString();

                    Ratio_DG.Items.Add(index);
                }
            }
            else { MessageBox.Show("Required input missing."); }
        }


        #endregion

        #endregion

        #endregion
        //---------------------------------------------------------------------------------
        #region helper methods

        // methods used in even handlers
        public void UpdateDvh()
        {
            if (Structure_ComboBox.SelectedIndex >= 0 && pitem.Dose != null)
            {
                #region variables

                Structure SelectedStructure = Structure_ComboBox?.SelectedItem as Structure;

                bool doseAbsolute = absDose_CB.IsChecked.Value;
                bool volAbsolute = absVolume_CB.IsChecked.Value;

                DoseValuePresentation dosePres = doseAbsolute ? DoseValuePresentation.Absolute : DoseValuePresentation.Relative;
                VolumePresentation volPres = volAbsolute ? VolumePresentation.AbsoluteCm3 : VolumePresentation.Relative;
                
                double binWidth = 0.001;
                string volumeUnit = volAbsolute ? "cc" : "%";
                DVHData dynamicDvh = pitem.GetDVHCumulativeData(SelectedStructure, dosePres, volPres, binWidth);
                DVHData dvhAR = pitem.GetDVHCumulativeData(SelectedStructure, DoseValuePresentation.Absolute, VolumePresentation.Relative, binWidth);
                DVHData dvhAA = pitem.GetDVHCumulativeData(SelectedStructure, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, binWidth);
                // will need these to work eventually for plan sums
                //DVHData s1_dvhRA = pitem == (PlanningItem)plan ? SelectedPlanningItem.GetDVHCumulativeData(SelectedStructure, DoseValuePresentation.Relative, VolumePresentation.AbsoluteCm3, s_binWidth) :
                //                                                SelectedPlanningItem.GetDVHCumulativeData(SelectedStructure, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, s_binWidth);
                //DVHData s1_dvhRR = SelectedPlanningItem == (PlanningItem)plan ? SelectedPlanningItem.GetDVHCumulativeData(SelectedStructure, DoseValuePresentation.Relative, VolumePresentation.Relative, s_binWidth) :
                //                                                SelectedPlanningItem.GetDVHCumulativeData(SelectedStructure, DoseValuePresentation.Absolute, VolumePresentation.Relative, s_binWidth);
                bool doseUnitIsGy = dvhAA.MaxDose.Unit == DoseValue.DoseUnit.Gy ? true : false;
                string absDoseUnit = doseUnitIsGy ? "Gy" : "cGy";

                #endregion

                #region manual dose checks

                ManualDoseCheck ManualDoseCheck = new ManualDoseCheck();
                double inputDose = Double.NaN;
                if (volumeAtDose_Input.Text != null)
                {
                    Double.TryParse(volumeAtDose_Input.Text, out inputDose);
                }
                if (!Double.IsNaN(inputDose))
                {
                    double structureMax = dynamicDvh.MaxDose.Dose;
                    if (inputDose > structureMax && doseAbsolute)
                    {
                        MessageBox.Show(string.Format("The input dose is greater than the {0}'s Dmax.", SelectedStructure.Id));
                    }
                    //DoseValue val = SelectedPlanningItem.GetDoseAtVolume(SelectedStructure, inputVolume, volPres, dosePres);
                    double result = DvhExtensions.getVolumeAtDose(dynamicDvh, inputDose);
                    ManualDoseCheck.volumeAtDose = string.Format("{0} = {1:F3}{2}", (doseAbsolute ? absDoseUnit : "%"), result, volumeUnit);
                    volumeAtDoseResult_TB.Text = ManualDoseCheck.volumeAtDose;
                }
                double inputVolume = Double.NaN;
                if (doseAtVolume_Input.Text != null)
                {
                    Double.TryParse(doseAtVolume_Input.Text, out inputVolume);
                }
                if (!Double.IsNaN(inputVolume))
                {
                    if (inputVolume > SelectedStructure.Volume && volAbsolute)
                    {
                        MessageBox.Show(string.Format("The input volume is greater than the {0}'s volume.", SelectedStructure.Id));
                    }
                    //DoseValue val = SelectedPlanningItem.GetDoseAtVolume(SelectedStructure, inputVolume, volPres, dosePres);
                    //double result = DvhExtensions.getDoseAtVolume(dynamicDvh, inputVolume);
                    double result = Double.NaN;
                    if (!volAbsolute && inputVolume == 100)
                    {
                        result = dynamicDvh.MinDose.Dose;
                    }
                    else
                    {
                        result = DvhExtensions.getDoseAtVolume(dynamicDvh, inputVolume);
                    }

                    ManualDoseCheck.doseAtVolume = string.Format("{0} = {1:F3}{2}", volumeUnit, 
                                                                                    result,
                                                                                    (doseAbsolute ? absDoseUnit : "%"));
                    doseAtVolumeResult_TB.Text = ManualDoseCheck.doseAtVolume;
                }

                #endregion
            }
        }
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
