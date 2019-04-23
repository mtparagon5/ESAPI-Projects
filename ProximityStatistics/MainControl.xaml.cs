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
        public string script = "ProximityStatistics";
        public PlanSetup ps;
        public PlanningItem pitem;
        public StructureSet ss;
        public string approvalStatus;
        public string jsonFilePath;
        public string jsonFilePath_randomized;
        public string colsCsvFilePath_randomized_patientSpecific;
        public string rowsCsvFilePath_randomized_patientSpecific;
        public string rowsCsvFilePath_randomized_structureSpecific;
		public string rowsCsvFilePath_randomized_master;
		public string dashboardPath;
        //int canvasCounter = 0;
        public IEnumerable<Structure> sorted_gtvList;
        public IEnumerable<Structure> sorted_ctvList;
        public IEnumerable<Structure> sorted_itvList;
        public IEnumerable<Structure> sorted_ptvList;
        public IEnumerable<Structure> sorted_targetList;
        public IEnumerable<Structure> sorted_oarList;
        public IEnumerable<Structure> sorted_structureList;
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
        public double planMaxDose;
        public bool isGrady;
        public string primaryPhysician;
        public string courseHeader;

        #endregion
        //---------------------------------------------------------------------------------
        #region objects used for binding

        public class PrimaryPhysician
        {
            public string Name { get; set; }
        }
        public class TargetStats
        {
            public Structure target { get; set; }
            public string targetId { get; set; }
            public string targetVolume { get; set; }
            public string segments { get; set; }
            public string isHighResolution { get; set; }
            //public string color { get; set; }
            //public double d95 { get; set; }
            //public double min { get; set; }
            //public double min03 { get; set; }
            //public double max { get; set; }
            //public double max03 { get; set; }
            //public double mean { get; set; }
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
            public double maxDose { get; set; }
            public double meanDose { get; set; }
            public double medianDose { get; set; }
        }

        #endregion
        //---------------------------------------------------------------------------------
        #region paths and string builders for data collection scripts

        #region LogUser

        //TODO: add file to collect various data on script used, time, user, etc. 
        public string userLogPath = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\esapi\\projects\\__Logs\\UserLog.csv";

        public StringBuilder userLogCsvContent = new StringBuilder();

        #endregion

        #endregion
        //---------------------------------------------------------------------------------
        #region event controls

        // various event controls
        private void OverlapRemoveButton_Clicked(object sender, RoutedEventArgs e)
        {
            OverlapInfo_DG.Items.RemoveAt(OverlapInfo_DG.SelectedIndex);
        }
        private void TargetDoseRemoveButton_Clicked(object sender, RoutedEventArgs e)
        {
            TargetStats_DG.Items.RemoveAt(TargetStats_DG.SelectedIndex);
        }
        private void Print_Visual(object sender, RoutedEventArgs e)
        {
            PrintVisual_Btn.Visibility = Visibility.Hidden;
            var colToRemove1 = OverlapInfo_DG.ColumnFromDisplayIndex(7);
            var colToRemove2 = TargetStats_DG.ColumnFromDisplayIndex(2);
            var colToRemove3 = TargetStats_DG.ColumnFromDisplayIndex(3);

            colToRemove1.Visibility = Visibility.Hidden;
            colToRemove2.Visibility = Visibility.Hidden;
            colToRemove3.Visibility = Visibility.Hidden;

            MainUC.Width = 800;

            Header.Margin = new Thickness(235, 30, 0, 10);
            planInfo_SP.Margin = new Thickness(50, 0, 0, 0);

            TargetStats_GB.Width = 207;
            TargetStats_GB.HorizontalAlignment = HorizontalAlignment.Left;
            TargetStats_GB.Margin = new Thickness(35, 10, 0, 10);

            OverlapInfo_DG.MaxHeight = 1100;
            TargetProx_GB.Margin = new Thickness(35, 0, 0, 0);

            proximityStats_SP.Children.Remove(structureList_SP);

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(MainGrid, string.Format("{0}_ProximityStatistics_{1}", id, planName));
            }
        }
        private void CalculateOverlap_Clicked(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                if (isGrady == false)
                {
                    StringBuilder randomized_rowCsvStringBuilder_patientSpecific = new StringBuilder();
                    StringBuilder randomized_colCsvStringBuilder_patientSpecific = new StringBuilder();
                    StringBuilder randomized_rowCsvStringBuilder_structureSpecific = new StringBuilder();
					StringBuilder randomized_rowCsvStringBuilder_master = new StringBuilder();

					string finalDashboardPath_patientSpecificJson_proxStats = dashboardPath + "\\" + id + "_" + planName + "_ProximityStats.json";


                    string proxStatsJsonArray = "[{\"patientId\":\"" + id + "\", " +
                                                    "\"randomId\":\"" + randomId + "\", " +
                                                    "\"primaryPhysician\":\"" + primaryPhysician + "\", " +
                                                    "\"courseHeader\":\"" + courseHeader + "\", " +
                                                    "\"courseId\":\"" + courseName + "\", " +
                                                    "\"planData\":[";
                    string proxStatsJsonArray_randomized = "[{\"randomId\":\"" + randomId + "\", " +
                                                            "\"primaryPhysician\":\"" + primaryPhysician + "\", " +
                                                            "\"courseHeader\":\"" + courseHeader + "\", " +
                                                            "\"courseId\":\"" + courseName + "\", " +
                                                            "\"planData\":[";
                    string proxStatsJsonArray_randomized_structure = "[{\"randomId\":\"" + randomId + "\", " +
                                                                        "\"primaryPhysician\":\"" + primaryPhysician + "\", " +
                                                                        "\"courseHeader\":\"" + courseHeader + "\", " +
                                                                        "\"courseId\":\"" + courseName + "\", " +
                                                                        "\"planData\":[";

                    proxStatsJsonArray = proxStatsJsonArray + "{\"planId\":\"" + planName + "\"," +
                                                                    "\"approvalStatus\":\"" + approvalStatus + "\"," +
                                                                    "\"targetData\":[";

                    proxStatsJsonArray_randomized = proxStatsJsonArray_randomized + "{\"planId\":\"" + planName + "\"," +
                                                                                        "\"approvalStatus\":\"" + approvalStatus + "\"," +
                                                                                        "\"targetData\":[";

                    foreach (var target in TargetStats_DG.Items)
                    {
                        var ttarget = target as TargetStats;
                        Structure t = ttarget.target;

                        string tName = t.Id.ToString().ToLower().Replace(" ", string.Empty).Replace(".", "_").Replace("/", "_").Replace("\\", "_").Split(':').First();
                        double tVolume = Math.Round(t.Volume, 3);
                        string tColor = "#" + t.Color.ToString().Substring(3, 6);
                        proxStatsJsonArray = proxStatsJsonArray + "{\"target\":\"" + tName + "\", " +
                                                                    "\"targetColor\":\"" + tColor + "\"," + 
                                                                    "\"targetStats\":[{" +
                                                                    "\"targetVolume\":" + (Math.Round(t.Volume, 3).ToString() + "," +
                                                                    "\"isHighResolution\":" + t.IsHighResolution.ToString().ToLower() + "," +
                                                                    "\"segments\":" + t.GetNumberOfSeparateParts().ToString()) + "," +
                                                                    "\"proximityStats\":[";

                        proxStatsJsonArray_randomized = proxStatsJsonArray_randomized + "{\"target\":\"" + tName + "\", " +
                                                                                            "\"targetColor\":\"" + tColor + "\"," + 
                                                                                            "\"targetStats\":[{" +
                                                                                            "\"targetVolume\":" + (Math.Round(t.Volume, 3).ToString() + "," +
                                                                                            "\"isHighResolution\":" + t.IsHighResolution.ToString().ToLower() + "," +
                                                                                            "\"segments\":" + t.GetNumberOfSeparateParts().ToString()) + "," +
                                                                                            "\"proximityStats\":[";

                        foreach (var item in structureList_LV.SelectedItems)
                        {
                            Structure s = item as Structure;
                            string sName = s.Id.ToString().ToLower().Replace(" ", string.Empty).Replace(".", "_").Replace("/", "_").Replace("\\", "_").Split(':').First();
                            double sVolume = Math.Round(s.Volume, 3);
                            string sColor = "#" + s.Color.ToString().Substring(3, 6);

                            //DVHData dvhAA = 

                            //if ()

                            OverlapStats overlapInfo = new OverlapStats();
                            overlapInfo.structureId = s.Id.ToString().Split(':').First();
                            overlapInfo.structureVolume = Math.Round(s.Volume, 3);
                            overlapInfo.ptvId = t.Id.ToString().Split(':').First();
                            overlapInfo.structureOverlapAbs = Math.Round(CalculateOverlap.VolumeOverlap(t, s), 3);
                            overlapInfo.structureOverlapPct = overlapInfo.structureOverlapAbs == 0 ? 0 : Math.Round(CalculateOverlap.PercentOverlap(s, overlapInfo.structureOverlapAbs), 1);
                            overlapInfo.targetOverlapPct = overlapInfo.structureOverlapAbs == 0 ? 0 : Math.Round(CalculateOverlap.PercentOverlap(t, overlapInfo.structureOverlapAbs), 1);
                            overlapInfo.distance = overlapInfo.structureOverlapAbs > 0 ? 0 : Math.Round(CalculateOverlap.ShortestDistance(t, s), 1);

                            // fill proximity stats data grid
                            OverlapInfo_DG.Items.Add(overlapInfo);

                            // structure info for json string (foreach ptv)
                            proxStatsJsonArray = proxStatsJsonArray + "{\"structure\":\"" + overlapInfo.structureId + "\", " +
                                                                        "\"structureColor\":\"" + sColor + "\"," +
                                                                        "\"structureStats\":[{" +
                                                                        "\"structureVolume\":" + overlapInfo.structureVolume + "," +
                                                                        "\"distanceFromTarget\":" + overlapInfo.distance + "," +
                                                                        "\"volumeOverlap\":" + overlapInfo.structureOverlapAbs + "," +
                                                                        "\"percentStructureOverlap\":" + overlapInfo.structureOverlapPct + "," +
                                                                        "\"percentTargetOverlap\":" + overlapInfo.targetOverlapPct + "}]";

                            proxStatsJsonArray_randomized = proxStatsJsonArray_randomized + "{\"structure\":\"" + overlapInfo.structureId + "\", " +
                                                                                            "\"structureColor\":\"" + sColor + "\"," + 
                                                                                            "\"structureStats\":[{" +
                                                                                            "\"structureVolume\":" + overlapInfo.structureVolume + "," +
                                                                                            "\"distanceFromTarget\":" + overlapInfo.distance + "," +
                                                                                            "\"volumeOverlap\":" + overlapInfo.structureOverlapAbs + "," +
                                                                                            "\"percentStructureOverlap\":" + overlapInfo.structureOverlapPct + "," +
                                                                                            "\"percentTargetOverlap\":" + overlapInfo.targetOverlapPct + "}]";
                            #region csv data

                            string rows_finalCsvPath_patientSpecific = rowsCsvFilePath_randomized_patientSpecific + "\\" + sName + "_" + tName + "_proximity_rows.csv";
                            string cols_finalCsvPath_patientSpecific = colsCsvFilePath_randomized_patientSpecific + "\\" + sName + "_" + tName + "_proximity_cols.csv";
                            string rows_finalCsvPath_structureSpecific = rowsCsvFilePath_randomized_structureSpecific + "\\" + courseHeader + "_" + sName + "_" + tName + "_proximity.csv";
							string rows_finalCsvPath_master = rowsCsvFilePath_randomized_master + "\\" + "AllPlansProxStatsData_rows.csv";


							// rows
							randomized_rowCsvStringBuilder_patientSpecific.Clear();
                            randomized_rowCsvStringBuilder_structureSpecific.Clear();
							randomized_rowCsvStringBuilder_master.Clear();

							randomized_rowCsvStringBuilder_patientSpecific.AppendLine("randomId,primaryPhysician,courseHeader,planId,approvalStatus,proximalTarget,targetColor,proximalTargetVolume,structureId,structureColor,structureVolume,distanceFrom_" + tName + ",overlapAbsWith_" + tName + ",structureOverlapPctWith_" + tName + "," + tName + "OverlapPct");
                            randomized_rowCsvStringBuilder_patientSpecific.AppendLine(randomId + "," + primaryPhysician + "," + courseHeader + "," + planName + "," + approvalStatus + "," + tName + "," + tColor + "," + tVolume + "," +
                                                                        sName + "," + sColor + "," + sVolume + "," + overlapInfo.distance + "," + overlapInfo.structureOverlapAbs + "," + overlapInfo.structureOverlapPct + "," + overlapInfo.targetOverlapPct);

							if (!File.Exists(rows_finalCsvPath_master))
							{
								randomized_rowCsvStringBuilder_master.AppendLine("randomId,primaryPhysician,courseHeader,planId,approvalStatus,proximalTarget,targetColor,proximalTargetVolume,structureId,structureColor,structureVolume,distanceFromTarget,overlapAbsWithTarget,structureOverlapPctWithTarget,OverlapPct");
							}
							randomized_rowCsvStringBuilder_master.AppendLine(randomId + "," + primaryPhysician + "," + courseHeader + "," + planName + "," + approvalStatus + "," + tName + "," + tColor + "," + tVolume + "," +
																		sName + "," + sColor + "," + sVolume + "," + overlapInfo.distance + "," + overlapInfo.structureOverlapAbs + "," + overlapInfo.structureOverlapPct + "," + overlapInfo.targetOverlapPct);

							if (!File.Exists(rows_finalCsvPath_structureSpecific))
                            {
                                randomized_rowCsvStringBuilder_structureSpecific.AppendLine("randomId,primaryPhysician,courseHeader,planId,approvalStatus,proximalTarget,targetColor,proximalTargetVolume,structureId,structureColor,structureVolume,distanceFrom_" + tName + ",overlapAbsWith_" + tName + ",structureOverlapPctWith_" + tName + "," + tName + "OverlapPct");
                            }
                            randomized_rowCsvStringBuilder_structureSpecific.AppendLine(randomId + "," + primaryPhysician + "," + courseHeader + "," + planName + "," + approvalStatus + "," + tName + "," + tColor + "," + tVolume + "," +
                                                                        sName + "," + sColor + "," + sVolume + "," + overlapInfo.distance + "," + overlapInfo.structureOverlapAbs + "," + overlapInfo.structureOverlapPct + "," + overlapInfo.targetOverlapPct);

                            // columns
                            randomized_colCsvStringBuilder_patientSpecific.Clear();
                            randomized_colCsvStringBuilder_patientSpecific.AppendLine("randomId,\t\t\t" + randomId);
                            randomized_colCsvStringBuilder_patientSpecific.AppendLine("primaryPhysician,\t\t" + primaryPhysician);
                            randomized_colCsvStringBuilder_patientSpecific.AppendLine("courseHeader,\t\t" + courseHeader);
                            randomized_colCsvStringBuilder_patientSpecific.AppendLine("planId,\t\t\t" + planName);
                            randomized_colCsvStringBuilder_patientSpecific.AppendLine("approvalStatus,\t\t" + approvalStatus);
                            randomized_colCsvStringBuilder_patientSpecific.AppendLine("proximalTarget,\t\t" + tName);
                            randomized_colCsvStringBuilder_patientSpecific.AppendLine("targetColor,\t\t" + tColor);
                            randomized_colCsvStringBuilder_patientSpecific.AppendLine("proximalTargetVolume,\t" + tVolume);
                            randomized_colCsvStringBuilder_patientSpecific.AppendLine("structureId,\t\t\t" + sName);
                            randomized_colCsvStringBuilder_patientSpecific.AppendLine("structureColor,\t\t" + sColor);
                            randomized_colCsvStringBuilder_patientSpecific.AppendLine("structureVolume,\t\t" + sVolume);
                            randomized_colCsvStringBuilder_patientSpecific.AppendLine("distanceFrom_" + tName + ",\t\t" + overlapInfo.distance);
                            randomized_colCsvStringBuilder_patientSpecific.AppendLine("overlapAbsWith_" + tName + ",\t\t" + overlapInfo.structureOverlapAbs);
                            randomized_colCsvStringBuilder_patientSpecific.AppendLine("structureOverlapPctWith_" + tName + ",\t" + overlapInfo.structureOverlapPct);
                            randomized_colCsvStringBuilder_patientSpecific.AppendLine(tName + "OverlapPct" + ",\t\t" + overlapInfo.targetOverlapPct);

                            // write csv files
                            File.WriteAllText(rows_finalCsvPath_patientSpecific, randomized_rowCsvStringBuilder_patientSpecific.ToString());
                            File.WriteAllText(cols_finalCsvPath_patientSpecific, randomized_colCsvStringBuilder_patientSpecific.ToString());
                            File.AppendAllText(rows_finalCsvPath_structureSpecific, randomized_rowCsvStringBuilder_structureSpecific.ToString());
							File.AppendAllText(rows_finalCsvPath_master, randomized_rowCsvStringBuilder_master.ToString());

                            #endregion

                            proxStatsJsonArray = proxStatsJsonArray + "},";

                            proxStatsJsonArray_randomized = proxStatsJsonArray_randomized + "},";
                        }
                        proxStatsJsonArray = proxStatsJsonArray.TrimEnd(',');
                        proxStatsJsonArray = proxStatsJsonArray + "]}]},";

                        proxStatsJsonArray_randomized = proxStatsJsonArray_randomized.TrimEnd(',');
                        proxStatsJsonArray_randomized = proxStatsJsonArray_randomized + "]}]},";
                    }
                    proxStatsJsonArray = proxStatsJsonArray.TrimEnd(',');
                    proxStatsJsonArray = proxStatsJsonArray + "]}]}]";

                    proxStatsJsonArray_randomized = proxStatsJsonArray_randomized.TrimEnd(',');
                    proxStatsJsonArray_randomized = proxStatsJsonArray_randomized + "]}]}]";

                    // write json files
                    File.WriteAllText(jsonFilePath, proxStatsJsonArray);
                    File.WriteAllText(finalDashboardPath_patientSpecificJson_proxStats, proxStatsJsonArray);
                    File.WriteAllText(jsonFilePath_randomized, proxStatsJsonArray_randomized);

                    structureList_LV.SelectedItems.Clear();
                }
                else
                {
                    foreach (var target in TargetStats_DG.Items)
                    {
                        var ttarget = target as TargetStats;
                        Structure t = ttarget.target;
                        //Structure t = target as Structure;

                        foreach (var item in structureList_LV.SelectedItems)
                        {
                            Structure s = item as Structure;

                            OverlapStats overlapInfo = new OverlapStats();
                            overlapInfo.structureId = s.Id.ToString().Split(':').First();
                            overlapInfo.structureVolume = Math.Round(s.Volume, 3);
                            overlapInfo.ptvId = t.Id.ToString().Split(':').First();
                            overlapInfo.structureOverlapAbs = Math.Round(CalculateOverlap.VolumeOverlap(t, s), 3);
                            overlapInfo.structureOverlapPct = overlapInfo.structureOverlapAbs == 0 ? 0 : Math.Round(CalculateOverlap.PercentOverlap(s, overlapInfo.structureOverlapAbs), 1);
                            overlapInfo.targetOverlapPct = overlapInfo.structureOverlapAbs == 0 ? 0 : Math.Round(CalculateOverlap.PercentOverlap(t, overlapInfo.structureOverlapAbs), 1);
                            overlapInfo.distance = overlapInfo.structureOverlapAbs > 0 ? 0 : Math.Round(CalculateOverlap.ShortestDistance(t, s), 1);

                            // fill proximity stats data grid
                            OverlapInfo_DG.Items.Add(overlapInfo);
                        }
                    }
                    structureList_LV.SelectedItems.Clear();
                }
            }
        }

        #endregion
        //---------------------------------------------------------------------------------
        #region helper methods

        // methods used in event handlers
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
