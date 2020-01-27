using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace GetJSON
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

        #region startup variables

        #endregion

        #region structure lists

        #endregion

        #region data collection variables

        public List<ESJO> JsonObjList = new List<ESJO>();
        public List<ESJO> KbmJOList = new List<ESJO>();
        public List<ESJO> ModelJsonObjList = new List<ESJO>();
        public PPatient patient = new PPatient();
        public List<PPlan> pplans = new List<PPlan>();
        public List<Tuple<string, Tuple<string, string>>> kbmList = new List<Tuple<string, Tuple<string, string>>>();

        private string modelPath = string.Empty;
        private string modelJsVarPath = string.Empty;
        private string modelId = string.Empty;
        private string modelDescription = string.Empty;
        private bool saveToModel = false;

        private bool saveToPatient = false;

        private bool saveToMaster = false;
        private string masterPath = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\_JsonData_\\MasterPlanData.json";
        private string masterJsVarPath = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\_JsonData_\\master-plan-data.js";
        private string masterJsVarHtmlPath = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\ConstraintFinder\\data\\test\\master-plan-data.js";

        public string courseHeader = string.Empty;
        public string courseId = string.Empty;
        

        #endregion

        #endregion
        //---------------------------------------------------------------------------------
        #region paths and string builders for data collection scripts

        #region Log

        //TODO: add file to collect various data on script used, time, user, etc. 
        public string userLogPath = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\esapi\\projects\\__Logs\\UserLog.csv";

        //public StringBuilder userLogCsvContent = new StringBuilder();

            

        #endregion Log

        #region HN Model

        public string hnModelData_Path = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\RapidPlan\\Models\\HN";
        
        public StringBuilder hnModelData_SB = new StringBuilder();

		#endregion HN Model

		#region Generic Plan Data

		public string planData_Path = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\RapidPlan\\Models\\HN";

		public StringBuilder planData_SB = new StringBuilder();

        //private void collectJSON_Btn_Click(object sender, RoutedEventArgs e)
        //{
        //}

        #endregion Generic Plan Data

        #endregion
        //---------------------------------------------------------------------------------
        #region objects used for binding 

        #endregion
        //---------------------------------------------------------------------------------
        #region event controls

        private void collectJSON_Btn_Clicked(object sender, RoutedEventArgs e)
        {
            if (planList_LV.SelectedItems.Count > 0)
            {
                //if (saveToMaster == true || saveToModel == true || saveToPatient == true)
                //{
                //    progressResult_TB.Text = "Data is being collected...please wait...";
                //    progressResult_TB.UpdateLayout();
                //}
                if (saveToModel && kbmList_CB.SelectedIndex < 0)
                {
                    MessageBox.Show("Specific Model not selected", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (/*saveToMaster == false &&*/ saveToPatient == false && saveToModel == false)
                {
                    MessageBox.Show("Please select where you would like the data saved to", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    using (new WaitCursor())
                    {
                        //if (!File.Exists(modelPath))// NOTE: add back conditions if saving to master model file
                        //{
                            var kbmId = ESJO.CreateESJO("ModelId", modelId); KbmJOList.Add(kbmId);
                            var kbmDescription = ESJO.CreateESJO("ModelDescription", modelDescription); KbmJOList.Add(kbmDescription);
                        //}

                        // patient level 
                        var pName_jo = ESJO.CreateESJO("PatientName", patient.Name); JsonObjList.Add(pName_jo); ModelJsonObjList.Add(pName_jo);
                        var pId_jo = ESJO.CreateESJO("PatientId", patient.Id); JsonObjList.Add(pId_jo); ModelJsonObjList.Add(pId_jo);
                        var randomId_jo = ESJO.CreateESJO("RandomId", patient.RandomId); JsonObjList.Add(randomId_jo); ModelJsonObjList.Add(randomId_jo);

                        // course variables
                        var course_jo = ESJO.CreateESJO("CourseId", courseId); JsonObjList.Add(course_jo); ModelJsonObjList.Add(course_jo);
                        var courseHeader_jo = ESJO.CreateESJO("CourseHeader", courseHeader); JsonObjList.Add(courseHeader_jo); ModelJsonObjList.Add(courseHeader_jo);

                        var dxList = new List<Diagnosis>();
                        try
                        {
                            foreach (var course in patient.Courses.ToList())
                            {
                                if (course.ToString() == courseId)
                                {
                                    dxList = course.Diagnoses.ToList();
                                }
                            }
                        
                            var courseDx_jo = ESJO.CreateESJO("Diagnoses", dxList); JsonObjList.Add(courseDx_jo); ModelJsonObjList.Add(courseDx_jo);
                        }
                        catch
                        {
                            MessageBox.Show("There was a problem collecting the Diagnoses");
                        }

                        //var pplans = new List<PPlan>();
                        var plansToCollectDataFor = new List<PPlan>();
                        foreach (var planId in planList_LV.SelectedItems)
                        {
                            foreach (var pp in pplans)
                            {
                                if (pp.Id == (string)planId)
                                {
                                    plansToCollectDataFor.Add(pp);
                                }
                            }
                            //pplans.Add((PPlan)plan);
                        }

                        ESJO plans_jo = ESJO.CreateESJO("PlanData", plansToCollectDataFor); JsonObjList.Add(plans_jo); ModelJsonObjList.Add(plans_jo);

                        //if (!File.Exists(modelPath))// NOTE: add back conditions if saving to master model file
                        //{
                        ESJO modelPlans_jo = ESJO.CreateESJO("ModelData", ModelJsonObjList); KbmJOList.Add(modelPlans_jo);
                        //}
                        //else// NOTE: add back conditions if saving to master model file
                        //{
                        //    ESJO modelPlans_jo = ESJO.CreateESJO(ModelJsonObjList, "{", "}"); KbmJOList.Add(modelPlans_jo);

                        //}

                        if (plans_jo.JsonString == "incomplete")
                        {
                            //.Text = "Sorry, something went wrong...";
                            progressResult_TB.Text = "Sorry, something went wrong...";

                        }
                        else
                        {
                            if (saveToPatient)
                            {
                                if (saveToModel)
                                {
                                    ESJO isForModel = ESJO.CreateESJO("IsForModel", "true"); JsonObjList.Add(isForModel);
                                    ESJO model = ESJO.CreateESJO("ModelId", modelId); JsonObjList.Add(model);
                                }
                                else
                                {
                                    ESJO isForModel = ESJO.CreateESJO("IsForModel", "false"); JsonObjList.Add(isForModel);
                                    ESJO model = ESJO.CreateESJO("ModelId", "N/A"); JsonObjList.Add(model);
                                }
                                string jsonPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\_JsonData_\\" + patient.Id + "_data.json";
                                string cleanJsonPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\_JsonData_\\CleanedData\\" + patient.Id + "_data.json";
                                string jsVarPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\_JsonData_\\js\\" + patient.Id + "_data.js";
                                //string jsVarPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\_JsonData_\\js" + patient.Id + "_data.js";
                                if (File.Exists(jsonPath))
                                {
                                    var jsVarString = "var ptJson = ";

                                    var existingJsonString = File.ReadAllText(jsonPath).Replace("\r\n", string.Empty).Replace("\t", string.Empty);
                                    existingJsonString = existingJsonString.TrimEnd(']');
                                    existingJsonString += ",{";
                                    foreach (var jo in JsonObjList)
                                    {
                                        existingJsonString += jo.JsonString + ",";
                                    }
                                    existingJsonString = existingJsonString.TrimEnd(',');
                                    existingJsonString += "}]";

                                    jsVarString += existingJsonString;

                                    File.WriteAllText(jsonPath, existingJsonString);
                                    File.WriteAllText(cleanJsonPath, existingJsonString);
                                    File.WriteAllText(jsVarPath, jsVarString);
                                }
                                else
                                {
                                    var jsonString = string.Empty;
                                    var jsVarString = "var ptJson = ";

                                    jsonString += "[{";
                                    foreach (var jo in JsonObjList)
                                    {
                                        jsonString += jo.JsonString + ",";
                                    }
                                    jsonString = jsonString.TrimEnd(',');
                                    jsonString += "}]";

                                    jsVarString += jsonString;

                                    File.WriteAllText(jsonPath, jsonString);
                                    File.WriteAllText(cleanJsonPath, jsonString);
                                    File.WriteAllText(jsVarPath, jsVarString);
                                }
                            }

                            // model data //
                            if (saveToModel)
                            {
                                if (modelPath != string.Empty)
                                {
                                    //if (File.Exists(modelPath))// NOTE: add back conditions if saving to master model file
                                    //{
                                    //    var jsVarString = "var modelJson = ";

                                    //    var existingModelData = File.ReadAllText(modelPath).Replace("\r\n", string.Empty).Replace("\t", string.Empty);
                                    //    existingModelData = existingModelData.TrimEnd('}');
                                    //    existingModelData = existingModelData.TrimEnd(']');
                                    //    //existingModelData = existingModelData.TrimEnd(']');
                                    //    existingModelData += ",";
                                    //    foreach (var jo in KbmJOList)
                                    //    {
                                    //        existingModelData += jo.JsonString + ",";
                                    //    }
                                    //    existingModelData = existingModelData.TrimEnd(',');
                                    //    existingModelData += "]}";

                                    //    jsVarString += existingModelData;

                                    //    File.WriteAllText(modelPath, existingModelData);
                                    //    File.WriteAllText(modelJsVarPath, jsVarString);

                                    //}
                                    //else NOTE: add back conditions if saving to master model file
                                    //{
                                    //var jsVarString = "var modelJson = ";//
                                    //var jsVarString = "var " + patient.Id + " = ";//

                                    modelPath += patient.Id + ".json";

                                        var modelData = "{";
                                        foreach (var jo in KbmJOList)
                                        {
                                            modelData += jo.JsonString + ",";
                                        }
                                        modelData = modelData.TrimEnd(',');
                                        modelData += "}";

                                        //jsVarString += modelData;//

                                        File.WriteAllText(modelPath, modelData);
                                        //File.WriteAllText(modelJsVarPath, jsVarString);//
                                    //}
                                }
                            }

                            // model data (master) // 
                            // NOTE: saves to a master file each time instead of individual files
                            //          need to uncomment logic above for adding items only if the file doesn't exist, etc.

                            #region for model data master
                            //if (saveToModel)
                            //{
                            //    if (modelPath != string.Empty)//
                            //    {
                            //        if (File.Exists(modelPath))
                            //        {
                            //            var jsVarString = "var modelJson = ";

                            //            var existingModelData = File.ReadAllText(modelPath).Replace("\r\n", string.Empty).Replace("\t", string.Empty);
                            //            existingModelData = existingModelData.TrimEnd('}');
                            //            existingModelData = existingModelData.TrimEnd(']');
                            //            //existingModelData = existingModelData.TrimEnd(']');
                            //            existingModelData += ",";
                            //            foreach (var jo in KbmJOList)
                            //            {
                            //                existingModelData += jo.JsonString + ",";
                            //            }
                            //            existingModelData = existingModelData.TrimEnd(',');
                            //            existingModelData += "]}";

                            //            jsVarString += existingModelData;

                            //            File.WriteAllText(modelPath, existingModelData);
                            //            File.WriteAllText(modelJsVarPath, jsVarString);

                            //        }
                            //        else
                            //        {
                            //            var jsVarString = "var modelJson = ";//
                            //            //var jsVarString = "var " + patient.Id + " = ";

                            //            //modelPath += patient.Id + ".json";

                            //            var modelData = "{";
                            //            foreach (var jo in KbmJOList)
                            //            {
                            //                modelData += jo.JsonString + ",";
                            //            }
                            //            modelData = modelData.TrimEnd(',');
                            //            modelData += "}";

                            //            jsVarString += modelData;//

                            //            File.WriteAllText(modelPath, modelData);
                            //            File.WriteAllText(modelJsVarPath, jsVarString);//
                            //        }
                            //    }
                            //}
                            #endregion


                            //// master data //
                            //if (saveToMaster)
                            //{
                            //    if (File.Exists(masterPath))
                            //    {
                            //        var jsVarString = "var masterJson = ";

                            //        var existingMasterData = File.ReadAllText(masterPath).Replace("\r\n", string.Empty).Replace("\t", string.Empty);
                            //        existingMasterData = existingMasterData.TrimEnd(']');
                            //        existingMasterData += ",{";
                            //        foreach (var jo in JsonObjList)
                            //        {
                            //            existingMasterData += jo.JsonString + ",";
                            //        }
                            //        existingMasterData = existingMasterData.TrimEnd(',');
                            //        existingMasterData += "}]";

                            //        jsVarString += existingMasterData;

                            //        File.WriteAllText(masterPath, existingMasterData);
                            //        File.WriteAllText(masterJsVarPath, jsVarString);
                            //        File.WriteAllText(masterJsVarHtmlPath, jsVarString);

                            //    }
                            //    else
                            //    {
                            //        var jsVarString = "var masterJson = ";

                            //        var masterData = "[{";
                            //        foreach (var jo in JsonObjList)
                            //        {
                            //            masterData += jo.JsonString + ",";
                            //        }
                            //        masterData = masterData.TrimEnd(',');
                            //        masterData += "}]";

                            //        jsVarString += masterData;

                            //        File.WriteAllText(masterPath, masterData);
                            //        File.WriteAllText(masterJsVarPath, jsVarString);
                            //        File.WriteAllText(masterJsVarHtmlPath, jsVarString);

                            //    }
                            //}

                            //mainControl.Progress_Result.Text = "JSON Data saved successfully...";
                            progressResult_TB.Text = "JSON data has been collected successfully for the selected plans...";
                        }

                    }
                }
            }
            else
            {
                MessageBox.Show("Plan not selected", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            
        }

        private void kbmSelected(object sender, RoutedEventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            var selectedKBM = (string)senderComboBox.SelectedItem;

            foreach (var kbm in kbmList)
            {
                if (kbm.Item1 == selectedKBM)
                {
                    kbmDescription_TB.Text = kbm.Item2.Item1;

                    modelId = kbm.Item1;
                    modelDescription = kbm.Item2.Item1;
                    modelPath = kbm.Item2.Item2;
                    modelJsVarPath = modelPath.Replace(".json", ".js");

                    break;
                }
            }
        }

        private void showKbmSelector(object sender, RoutedEventArgs e)
        {
            kbmSelector_SP.Visibility = Visibility.Visible;
            saveToModel = true;
        }
        private void hideKbmSelector(object sender, RoutedEventArgs e)
        {
            kbmSelector_SP.Visibility = Visibility.Hidden;
            saveToModel = false;
        }

        private void saveToMasterChecked(object sender, RoutedEventArgs e)
        {
            saveToMaster = true;
        }
        private void saveToMasterUnChecked(object sender, RoutedEventArgs e)
        {
            saveToMaster = false;
        }

        private void saveToPatientChecked(object sender, RoutedEventArgs e)
        {
            saveToPatient = true;
        }
        private void saveToPatientUnChecked(object sender, RoutedEventArgs e)
        {
            saveToPatient = false;
        }


        #endregion
        //---------------------------------------------------------------------------------
        #region helper methods


        #endregion
        //---------------------------------------------------------------------------------
        #region Data Collection Methods

        #endregion
        //---------------------------------------------------------------------------------
    }
}
