using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace PrintJawPositions
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
        public string script = "PrintJawPositions";
        public PlanSetup ps;
        public PlanningItem pitem;
        public StructureSet ss;
        public string approvalStatus;
        public IEnumerator<PlanSetup> plans;
        public IEnumerable<Beam> fields;
        //int canvasCounter = 0;
        public double Fractions;
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

        #endregion
        //---------------------------------------------------------------------------------
        #region objects used for binding

        public class PrimaryPhysician
        {
            public string Name { get; set; }
        }
        public class JawPositions
        {
            public string planId { get; set; }
            public string planStatus { get; set; }
            public string fieldId { get; set; }
            public string isJawTracking { get; set; }
            public string x1 { get; set; }
            public string x2 { get; set; }
            public string y1 { get; set; }
            public string y2 { get; set; }
            public double mu { get; set; }
        }

        #endregion
        //---------------------------------------------------------------------------------
        #region paths and string builders for data collection scripts

        #region LogUser

        //TODO: add file to collect various data on script used, time, user, etc. 
        public string userLogPath = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\esapi\\projects\\__Logs\\UserLog.csv";

        public StringBuilder userLogCsvContent = new StringBuilder();

        #endregion

        #region Plans with JawTracking

        //TODO: add file to collect various data on script used, time, user, etc. 
        public string jawTrackingPath = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\esapi\\projects\\__Logs\\PlansWithJawTracking.csv";

        public StringBuilder jawTrackingCsvContent = new StringBuilder();

        #endregion

        #endregion
        //---------------------------------------------------------------------------------
        #region event controls

        // various event controls
        private void JawPositionRemoveButton_Clicked(object sender, RoutedEventArgs e)
        {
            JawPositions_DG.Items.RemoveAt(JawPositions_DG.SelectedIndex);
        }
        private void Print_Visual(object sender, RoutedEventArgs e)
        {
            PrintVisual_Btn.Visibility = Visibility.Hidden;
            var colToRemove1 = JawPositions_DG.ColumnFromDisplayIndex(9);
            colToRemove1.Visibility = Visibility.Hidden;

            MainUC.Width = 800;

            Header.Margin = new Thickness(275, 100, 0, 50);
            planInfo_SP.Margin = new Thickness(50, 0, 0, 0);
            JawPositions_DG.Width = 645;
            JawPositions_GB.HorizontalAlignment = HorizontalAlignment.Left;
            JawPositions_GB.Margin = new Thickness(35, 0, 0, 0);
            //Note_SP.Margin = new Thickness(50, 0, 0, 0);

            proximityStats_SP.Children.Remove(planList_SP);

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(MainGrid, string.Format("{0}_JawPositions_{1}", id, courseName));
            }
        }
        private void viewJawPositions_Clicked(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                #region jaw positions

                foreach (var plan in planList_LV.SelectedItems)
                {
                    var currentPlan = (ExternalPlanSetup)plan;
                    var planFields = currentPlan.Beams;
                    List<string> fieldsNotIMRT = new List<string>();
                    int counter = 0;

                    foreach (var field in planFields)
                    {
                        JawPositions jP = new JawPositions();
                        jP.planId = currentPlan.Id;
                        jP.planStatus = currentPlan.ApprovalStatus.ToString();

                        if ((field.MLCPlanType.ToString() == "VMAT") || (field.MLCPlanType.ToString() == "DoseDynamic"))
                        {
                            counter++;
                            if (field.ControlPoints.Count > 9)
                            {
                                var cpNum = field.ControlPoints.Count - 1;

                                if ((field.ControlPoints[(int)(Math.Floor((double)(cpNum / 3)))].JawPositions != null) && (field.ControlPoints[(int)(Math.Floor((double)(cpNum / 5)))].JawPositions != null))
                                {
                                    var cp1 = field.ControlPoints[cpNum / 1];
                                    var cp2 = field.ControlPoints[(int)(Math.Floor((double)(cpNum / 3)))];
                                    var cp3 = field.ControlPoints[(int)(Math.Floor((double)(cpNum / 5)))];
                                    var cp4 = field.ControlPoints[(int)(Math.Floor((double)(cpNum / 7)))];

                                    if ((cp1.JawPositions.X1 != cp2.JawPositions.X1) ||
                                        (cp1.JawPositions.X2 != cp2.JawPositions.X2) ||
                                        (cp1.JawPositions.Y1 != cp2.JawPositions.Y1) ||
                                        (cp1.JawPositions.Y2 != cp2.JawPositions.Y2) ||
                                        (cp1.JawPositions.X1 != cp3.JawPositions.X1) ||
                                        (cp1.JawPositions.X2 != cp3.JawPositions.X2) ||
                                        (cp1.JawPositions.Y1 != cp3.JawPositions.Y1) ||
                                        (cp1.JawPositions.Y2 != cp3.JawPositions.Y2) ||
                                        (cp2.JawPositions.X1 != cp4.JawPositions.X1) ||
                                        (cp2.JawPositions.X2 != cp4.JawPositions.X2) ||
                                        (cp2.JawPositions.Y1 != cp4.JawPositions.Y1) ||
                                        (cp2.JawPositions.Y2 != cp4.JawPositions.Y2))
                                    {
                                        jP.isJawTracking = "Yes";
                                        jP.fieldId = field.Id;
                                        var controlPoints = field.ControlPoints;
                                        var cP = controlPoints[0];

                                        jP.x1 = string.Format("{0:N1}", Math.Round((decimal)(cP.JawPositions.X1) / -10, 2));
                                        jP.y1 = string.Format("{0:N1}", Math.Round((decimal)(cP.JawPositions.Y1) / -10, 2));
                                        jP.y2 = string.Format("{0:N1}", Math.Round((decimal)(cP.JawPositions.Y2) / 10, 2));
                                        jP.x2 = string.Format("{0:N1}", Math.Round((decimal)(cP.JawPositions.X2) / 10, 2));
                                        jP.mu = Math.Round(field.Meterset.Value);

                                        JawPositions_DG.Items.Add(jP);
                                        jawTrackingCsvContent.AppendLine(string.Format("{0},\t{1},\t\t{2},\t\t\t{3},\t\t{4}", id, jP.planId, jP.planStatus, jP.fieldId, jP.mu));
                                    }
                                    else
                                    {
                                        jP.isJawTracking = "No";
                                        jP.fieldId = field.Id;
                                        var controlPoints = field.ControlPoints;
                                        var cP = controlPoints[0];

                                        jP.x1 = string.Format("{0:N1}", Math.Round((decimal)(cP.JawPositions.X1) / -10, 2));
                                        jP.y1 = string.Format("{0:N1}", Math.Round((decimal)(cP.JawPositions.Y1) / -10, 2));
                                        jP.y2 = string.Format("{0:N1}", Math.Round((decimal)(cP.JawPositions.Y2) / 10, 2));
                                        jP.x2 = string.Format("{0:N1}", Math.Round((decimal)(cP.JawPositions.X2) / 10, 2));
                                        jP.mu = Math.Round(field.Meterset.Value);

                                        JawPositions_DG.Items.Add(jP);
                                    }
                                }
                            }
                            else
                            {
                                fieldsNotIMRT.Add(field.Id.ToString());
                            }
                        }
                    }
                    if (fieldsNotIMRT.Count > 0)
                    {
                        string message = "";
                        foreach (var f in fieldsNotIMRT)
                        {
                            message += "\n\t- " + f;
                        }
                        MessageBox.Show(string.Format("The following fields are FIF but will not be added:\n{0}", message), string.Format("{0}", currentPlan.Id));
                    }
                    if (counter < 1) { MessageBox.Show("None of the fields have multiple control points.", string.Format("{0}", currentPlan.Id)); }
                }
                // log if isJawTracking == "Yes"
                if (isGrady == false)
                {
                    File.AppendAllText(jawTrackingPath, jawTrackingCsvContent.ToString());
                }

                #endregion
            }
            planList_LV.SelectedItems.Clear();
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
            string course = courseName;
            string plan = "NA"; ;
            string dosePerFx = "NA";
            string numFx = "NA";

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
