using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace OptiAssistant
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
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
    public string script = "OptiAssistant";
    public Patient patient;
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
    public string ssId;
    public bool isGrady;
    public bool cropFromBody = false;
    public int cropFromBodyMargin = DEFAULT_OPTI_CROP_FROM_BODY_MARGIN;
    public bool hasNoPTV = false;
    public bool hasSinglePTV = false;
    public bool hasMultiplePTVs = false;
    public bool hasTwoDoseLevels = false;
    public bool hasThreeDoseLevels = false;
    public bool hasFourDoseLevels = false;


    //public bool createOptiGTVForSingleLesion = false;
    //public bool createOptiTotal = false;

    // DEFAULTS
    const string DEFAULT_AVOIDANCE_PREFIX = "zav";
    const int DEFAULT_AVOIDANCE_GROW_MARGIN = 2;
    const int DEFAULT_AVOIDANCE_CROP_MARGIN = 2;
    const string AVOIDANCE_DICOM_TYPE = "AVOIDANCE";

    const string DEFAULT_OPTI_PREFIX = "zopti";
    const int DEFAULT_OPTI_GROW_MARGIN = 1;
    const int DEFAULT_OPTI_CROP_MARGIN = 2;
    const int DEFAULT_OPTI_CROP_FROM_BODY_MARGIN = 4;
    const string OPTI_DICOM_TYPE = "PTV";

    const string DEFAULT_RING_PREFIX = "zring";
    const int DEFAULT_RING_GROW_MARGIN = 10;
    const int DEFAULT_RING_COUNT = 0;
    const int DEFAULT_RING_CROP_MARGIN = 0;
    const string RING_DICOM_TYPE = "CONTROL";



    #endregion
    //---------------------------------------------------------------------------------
    #region objects used for binding

    #endregion
    //---------------------------------------------------------------------------------
    #region paths and string builders for data collection scripts

    #region Script Use Log

    public string userLogPath = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\esapi\\projects\\__Logs\\OptiAssistant_UserLog.csv";

    public StringBuilder userLogCsvContent = new StringBuilder();

    #endregion

    #endregion
    //---------------------------------------------------------------------------------
    #region event controls

    #region input changed events

    #region button / checkbox events

    // check boxes
    public void CreateAvoids_CB_Click(object sender, RoutedEventArgs e)
    {
      var cb = sender as CheckBox;
      if (cb.IsChecked == true)
      {
        AvoidanceStructure_SP.Visibility = Visibility.Visible;
      }
      else
      {
        AvoidanceStructure_SP.Visibility = Visibility.Collapsed;
      }

    }

    public void CreateRingss_CB_Click(object sender, RoutedEventArgs e)
    {
      var cb = sender as CheckBox;
      if (cb.IsChecked == true)
      {
        RingStructure_SP.Visibility = Visibility.Visible;
      }
      else
      {
        RingStructure_SP.Visibility = Visibility.Collapsed;
      }
    }

    public void CreateOptis_CB_Click(object sender, RoutedEventArgs e)
    {
      var cb = sender as CheckBox;
      if (cb.IsChecked == true)
      {
        OptiStructure_SP.Visibility = Visibility.Visible;
      }
      else
      {
        OptiStructure_SP.Visibility = Visibility.Collapsed;
      }
    }

    // show/hide instructions buttion
    public void Instructions_Button_Click(object sender, RoutedEventArgs e)
    {
      if (Instructions_SP.Visibility == Visibility.Visible)
      {
        Instructions_SP.Visibility = Visibility.Collapsed;
        Instructions_Button.Content = "Show Instructions";
      }
      else
      {
        Instructions_SP.Visibility = Visibility.Visible;
        Instructions_Button.Content = "Hide Instructions";
      }
    }

    // temp create structures function to allow for design testing
    public void CreateStructures_Btn_Click(object sender, RoutedEventArgs e) { }

    // create structures button
    public void CreateStructures_Btn_ClickLive(object sender, RoutedEventArgs e)
    {

      // validation
      if (CreateAvoids_CB.IsChecked == false && CreateOptis_CB.IsChecked == false && CreateRings_CB.IsChecked == false)
      {
          MessageBox.Show("Oops, it seems no tasks were selected.\n\nPlease select which structures you would like assistance in creating.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
      }
      else if (CreateOptis_CB.IsChecked == true && MultipleDoseLevels_CB.IsChecked == true)
      {
        if (DoseLevel1_Radio.IsChecked == true && DoseLevel1_LB.SelectedIndex < 0)
        {
          MessageBox.Show("One Dose Level has been selected:\n\n\t- At least one target should be selected.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        if (DoseLevel2_Radio.IsChecked == true && (DoseLevel1_LB.SelectedIndex < 0 || DoseLevel2_LB.SelectedIndex < 0))
        {
          MessageBox.Show("Two Dose Levels have been selected:\n\n\t- Targets for at least two dose levels should be selected.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        if (DoseLevel3_Radio.IsChecked == true && (DoseLevel1_LB.SelectedIndex < 0 || DoseLevel2_LB.SelectedIndex < 0 || DoseLevel3_LB.SelectedIndex < 0))
        {
          MessageBox.Show("Three Dose Levels have been selected:\n\n\t- Targets for at least three dose levels should be selected.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        if (DoseLevel4_Radio.IsChecked == true && (DoseLevel1_LB.SelectedIndex < 0 || DoseLevel2_LB.SelectedIndex < 0 || DoseLevel3_LB.SelectedIndex < 0 || DoseLevel4_LB.SelectedIndex < 0))
        {
          MessageBox.Show("Four Dose Levels have been selected:\n\n\t- Targets for at least four dose levels should be selected.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
      }

      // NOTE: various other validation can be done. To reduce the need for some validation, when user input for margins is invalid, a warning will show informing the user of the invalid entry and that the default value will instead be used. 

      else
      {

        patient.BeginModifications();

        // find body
        Structure body = null;
        try
        {
          body = Helpers.GetBody(ss);
        }
        catch
        {
          body = ss.CreateAndSearchBody(ss.GetDefaultSearchBodyParameters());

          var message = "Sorry, could not find a structure of type BODY:\n\n\t- A Body Structure will be generated using the default Search Body Parameters. Please verify accuracy.";
          var title = "Body Structure Generation";

          MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // create zopti total if ptv(s) present
        Structure zoptiTotal = null;
        if (hasSinglePTV || hasMultiplePTVs)
        {
          string zoptiTotalId = "zopti total";
          // add empty zopti total structure
          zoptiTotal = ss.AddStructure(AVOIDANCE_DICOM_TYPE, zoptiTotalId);
          zoptiTotal.SegmentVolume = Helpers.BooleanStructures(sorted_ptvList);
          if (body != null)
          {
            zoptiTotal.SegmentVolume = Helpers.CropOutsideBodyWithMargin(zoptiTotal, body, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
          }
        }

        #region avoidance structures

        if (CreateAvoids_CB.IsChecked == true)
        {

          var avStructuresToMake = OarList_LV.SelectedItems;
          string avPrefix;
          int avGrowMargin;
          int avCropMargin;

          // set prefix
          if (AvoidPrefix_TextBox.Text == "") { avPrefix = DEFAULT_AVOIDANCE_PREFIX; } else { avPrefix = AvoidPrefix_TextBox.Text; }

          // set grow margin
          if (AvoidGrowMargin_TextBox.Text == "") { avGrowMargin = DEFAULT_AVOIDANCE_GROW_MARGIN; }
          else
          {
            if (int.TryParse(AvoidGrowMargin_TextBox.Text, out avGrowMargin))
            {
              //parsing successful 
            }
            else
            {
              //parsing failed. 
              avGrowMargin = DEFAULT_AVOIDANCE_GROW_MARGIN;
              //MessageBox.Show("Oops, please enter a valid Margin for your avoidance structures.");
              MessageBox.Show(string.Format("Oops, an invalid value was used for the avoidance structure grow margin ({0}). The DEFAULT of {1}mm will be used.", AvoidGrowMargin_TextBox.Text, DEFAULT_AVOIDANCE_GROW_MARGIN));
            }
          }

          // set crop margin
          if (AvoidCropMargin_TextBox.Text == "") { avCropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN; }
          else
          {
            if (int.TryParse(AvoidCropMargin_TextBox.Text, out avCropMargin))
            {
              //parsing successful 
            }
            else
            {
              //parsing failed. 
              avCropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
              //MessageBox.Show("Oops, please enter a valid Crop Margin for your avoidance structures.");
              MessageBox.Show(string.Format("Oops, an invalid value was used for the avoidance structure crop margin ({0}). The DEFAULT of {1}mm will be used.", AvoidCropMargin_TextBox.Text, DEFAULT_AVOIDANCE_CROP_MARGIN));
            }
          }

          foreach (var s in avStructuresToMake)
          {

            var oar = (Structure)s;
            var avId = string.Format("{0} {1}", avPrefix, oar.Id.ToString());

            // remove structure if present in ss
            Helpers.RemoveStructure(ss, avId);

            // add empty avoid structure
            var avoidStructure = ss.AddStructure(AVOIDANCE_DICOM_TYPE, avId);

            // copy oar with defined margin
            avoidStructure.SegmentVolume = Helpers.AddMargin(oar, (double)avGrowMargin);

            // crop avoid structure from ptv total (if ptvs are found)
            if (zoptiTotal != null) { avoidStructure.SegmentVolume = Helpers.CropStructure(avoidStructure, zoptiTotal, avCropMargin); }

          }

        }

        #endregion avoidance structures

        #region opti structures

        if (CreateOptis_CB.IsChecked == true)
        {

          var optiStructuresToMake = PTVList_LV.SelectedItems;
          string optiPrefix = string.Empty;
          int optiGrowMargin = DEFAULT_OPTI_GROW_MARGIN;
          int optiCropMargin = DEFAULT_OPTI_CROP_MARGIN;
          int optiCropFromBodyMargin = DEFAULT_OPTI_CROP_FROM_BODY_MARGIN;
          List<Structure> optisMade = new List<Structure>();

          // for multiple dose levels
          Structure doseLevel1Target = null;
          Structure doseLevel2Target = null;
          Structure doseLevel3Target = null;
          Structure doseLevel4Target = null;
          Structure opti1 = null;
          Structure opti2 = null;
          Structure opti3 = null;
          Structure opti4 = null;

          // set prefix
          if (OptiPrefix_TextBox.Text == "") { optiPrefix = DEFAULT_OPTI_PREFIX; } 
          else { optiPrefix = OptiPrefix_TextBox.Text; }

          // set crop from body margin
          if (cropFromBody)
          {
            if (BodyCropMargin_TextBox.Text == "") { optiCropFromBodyMargin = DEFAULT_OPTI_CROP_FROM_BODY_MARGIN; }
            else
            {
              if (int.TryParse(BodyCropMargin_TextBox.Text, out optiCropFromBodyMargin))
              {
                //parsing successful 
              }
              else
              {
                optiCropFromBodyMargin = DEFAULT_OPTI_CROP_FROM_BODY_MARGIN;
                MessageBox.Show(string.Format("Oops, an invalid value was used for the opti crop from body margin ({0}). The DEFAULT of {1}mm will be used.", OptiCropMargin_TextBox.Text, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN));
              }
            }
          }

          // set grow margin
          if (int.TryParse(OptiGrowMargin_TextBox.Text, out optiGrowMargin))
          {
            //parsing successful 
          }
          else
          {
            //parsing failed. 
            optiGrowMargin = DEFAULT_OPTI_GROW_MARGIN;
            //MessageBox.Show("Oops, please enter a valid Margin for your avoidance structures.");
            MessageBox.Show(string.Format("Oops, an invalid value was used for the opti structure grow margin ({0}). The DEFAULT of {1}mm will be used.", OptiCropMargin_TextBox.Text, DEFAULT_OPTI_GROW_MARGIN));
          }

          // if multiple dose levels need to set opti crop margin as well as determine which targets correspond to which dose levels
          if (MultipleDoseLevels_CB.IsChecked == true)
          {
            // set opti crop margin
            if (OptiCropMargin_TextBox.Text == "")
            {
              optiCropMargin = DEFAULT_OPTI_CROP_MARGIN;
            }
            else
            {
              if (int.TryParse(OptiCropMargin_TextBox.Text, out optiCropMargin))
              {
                //parsing successful 
              }
              else
              {
                //parsing failed. 
                optiCropMargin = DEFAULT_OPTI_CROP_MARGIN;
                //MessageBox.Show("Oops, please enter a valid Crop Margin for your opti structures.");
                MessageBox.Show(string.Format("Oops, an invalid value was used for the opti structure crop margin ({0}). The DEFAULT of {1}mm will be used.", OptiCropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
              }
            }

            if (DoseLevel1_Radio.IsChecked == true)
            {
              doseLevel1Target = (Structure)DoseLevel1_LB.SelectedItem;
            }
            else if (DoseLevel2_Radio.IsChecked == true)
            {
              doseLevel1Target = (Structure)DoseLevel1_LB.SelectedItem;
              doseLevel2Target = (Structure)DoseLevel2_LB.SelectedItem;

              hasTwoDoseLevels = true;
            }
            else if (DoseLevel3_Radio.IsChecked == true)
            {
              doseLevel1Target = (Structure)DoseLevel1_LB.SelectedItem;
              doseLevel2Target = (Structure)DoseLevel2_LB.SelectedItem;
              doseLevel3Target = (Structure)DoseLevel3_LB.SelectedItem;

              hasThreeDoseLevels = true;
            }
            else if (DoseLevel4_Radio.IsChecked == true)
            {
              doseLevel1Target = (Structure)DoseLevel1_LB.SelectedItem;
              doseLevel2Target = (Structure)DoseLevel2_LB.SelectedItem;
              doseLevel3Target = (Structure)DoseLevel3_LB.SelectedItem;
              doseLevel4Target = (Structure)DoseLevel4_LB.SelectedItem;

              hasFourDoseLevels = true;
            }
          }

          // create optis
          foreach (var s in optiStructuresToMake)
          {
            var ptv = (Structure)s;
            var optiId = string.Format("{0} {1}", optiPrefix, ptv.Id.ToString());

            // remove structure if present in ss
            Helpers.RemoveStructure(ss, optiId);

            // add empty avoid structure
            var optiStructure = ss.AddStructure(OPTI_DICOM_TYPE, optiId);

            // copy ptv with defined margin
            optiStructure.SegmentVolume = Helpers.AddMargin(ptv, (double)optiGrowMargin);

            // crop OPTI structure outside body (if body found)
            if (body != null)
            {
              optiStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(optiStructure, body, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
            }

            if (MultipleDoseLevels_CB.IsChecked == true)
            {
              if (ptv.Id == doseLevel1Target.Id) { opti1 = optiStructure; }
              else if (ptv.Id == doseLevel2Target.Id) { opti2 = optiStructure; }
              else if (ptv.Id == doseLevel3Target.Id) { opti3 = optiStructure; }
              else if (ptv.Id == doseLevel4Target.Id) { opti4 = optiStructure; }
            }

            optisMade.Add(optiStructure);

            // TODO...add logic/functionality to allow user to select which optis should be cropped from which and with which margins

          }

          // crop optis from one another when there are multiple dose levels
          if (MultipleDoseLevels_CB.IsChecked == true)
          {
            // loop through the optis that were made
            foreach (var opti in optisMade)
            {
              // if two dose levels defined
              if (hasTwoDoseLevels)
              {
                if (opti1 != null && opti2 != null)
                {
                  // crop opti 1 from opti 2
                  if (opti.Id == opti1.Id)
                  {
                    opti.SegmentVolume = Helpers.CropOpti(opti, opti2, optiCropMargin);
                  }
                }
              }
              // if three dose levels defined
              else if (hasThreeDoseLevels)
              {
                if (opti1 != null && opti2 != null && opti3 != null)
                {
                  // crop opti 1 from optis 2 and 3
                  if (opti.Id == opti1.Id)
                  {
                    opti.SegmentVolume = Helpers.CropOpti(opti, opti2, optiCropMargin);
                    opti.SegmentVolume = Helpers.CropOpti(opti, opti3, optiCropMargin);
                  }
                  // crop opti 2 from opti 3
                  if (opti.Id == opti2.Id)
                  {
                    opti.SegmentVolume = Helpers.CropOpti(opti, opti3, optiCropMargin);
                  }
                }
              }
              // if four dose levels defined
              else if (hasFourDoseLevels)
              {
                if (opti1 != null && opti2 != null && opti3 != null && opti4 != null)
                {
                  // crop opti 1 from optis 2, 3, and 4
                  if (opti.Id == opti1.Id)
                  {
                    opti.SegmentVolume = Helpers.CropOpti(opti, opti2, optiCropMargin);
                    opti.SegmentVolume = Helpers.CropOpti(opti, opti3, optiCropMargin);
                    opti.SegmentVolume = Helpers.CropOpti(opti, opti4, optiCropMargin);
                  }
                  // crop opti 2 from optis 3 and 4
                  if (opti.Id == opti2.Id)
                  {
                    opti.SegmentVolume = Helpers.CropOpti(opti, opti3, optiCropMargin);
                    opti.SegmentVolume = Helpers.CropOpti(opti, opti4, optiCropMargin);
                  }
                  // crop opti 3 from opti 4
                  if (opti.Id == opti3.Id)
                  {
                    opti.SegmentVolume = Helpers.CropOpti(opti, opti4, optiCropMargin);
                  }
                }
              }
            }
          }

        }

        #endregion opti structures

        #region ring structures

        if (CreateRings_CB.IsChecked == true)
        {
          
          // TODO: add ring structure logic

        }

        #endregion ring structures

      }

    }

    #region opti structure section events

    // event fired when opti option selected/unselected - crop from body option || multiple dose levels option
    private void HandleOptiOptionsSelection(object sender, RoutedEventArgs e)
    {
      if (CropFromBody_CB.IsChecked == true || MultipleDoseLevels_CB.IsChecked == true)
      {

        // show section if checked
        CropFromBody_SP.Visibility = CropFromBody_CB.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        CropFromOptis_SP.Visibility = MultipleDoseLevels_CB.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        MultiDoseLevelOptions_SP.Visibility = MultipleDoseLevels_CB.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

        // show options if either is checked
        if (OptiOptions_SP.Visibility == Visibility.Collapsed) { OptiOptions_SP.Visibility = Visibility.Visible; }

        cropFromBody = CropFromBody_CB.IsChecked == true ? true : false;

      }
      if (CropFromBody_CB.IsChecked == false && MultipleDoseLevels_CB.IsChecked == false)
      {
        // show options if either is checked
        OptiOptions_SP.Visibility = Visibility.Collapsed;

        // collapese sections
        CropFromBody_SP.Visibility = Visibility.Collapsed;
        CropFromOptis_SP.Visibility = Visibility.Collapsed;
        MultiDoseLevelOptions_SP.Visibility = Visibility.Collapsed;

      }
    }

    // event fired when different number of dose levels is defined
    private void HandleDoseLevelCount(object sender, RoutedEventArgs e)
    {
      var radio1 = DoseLevel1_Radio;
      var radio2 = DoseLevel2_Radio;
      var radio3 = DoseLevel3_Radio;
      var radio4 = DoseLevel4_Radio;

      if (radio1.IsChecked == true)
      {
        DoseLevel1_SP.Visibility = Visibility.Visible;
        DoseLevel2_SP.Visibility = Visibility.Collapsed;
        DoseLevel3_SP.Visibility = Visibility.Collapsed;
        DoseLevel4_SP.Visibility = Visibility.Collapsed;
      }
      else if (radio2.IsChecked == true)
      {
        DoseLevel1_SP.Visibility = Visibility.Visible;
        DoseLevel2_SP.Visibility = Visibility.Visible;
        DoseLevel3_SP.Visibility = Visibility.Collapsed;
        DoseLevel4_SP.Visibility = Visibility.Collapsed;
      }
      else if (radio3.IsChecked == true)
      {
        DoseLevel1_SP.Visibility = Visibility.Visible;
        DoseLevel2_SP.Visibility = Visibility.Visible;
        DoseLevel3_SP.Visibility = Visibility.Visible;
        DoseLevel4_SP.Visibility = Visibility.Collapsed;
      }
      else if (radio4.IsChecked == true)
      {
        DoseLevel1_SP.Visibility = Visibility.Visible;
        DoseLevel2_SP.Visibility = Visibility.Visible;
        DoseLevel3_SP.Visibility = Visibility.Visible;
        DoseLevel4_SP.Visibility = Visibility.Visible;
      }
    }

    // not used
    #region not used

    //// event fired when opti option selected/unselected - crop from body option
    //private void CropFromBody_CB_Click(object sender, RoutedEventArgs e)
    //{
    //  var cb = sender as CheckBox;

    //  if (cb.IsChecked == true) { cropFromBody = true; CropFromBody_SP.Visibility = Visibility.Visible; }
    //  else { cropFromBody = false; CropFromBody_SP.Visibility = Visibility.Collapsed; }


    //}

    //// event fired when opti option selected/unselected - multiple dose levels option
    //private void MultipleDoseLevels_CB_Click(object sender, RoutedEventArgs e)
    //{
    //  var cb = sender as CheckBox;
    //  if (cb.IsChecked == true) { MultiDoseLevelOptions_SP.Visibility = Visibility.Visible; }
    //  else { MultiDoseLevelOptions_SP.Visibility = Visibility.Collapsed; }
    //}

    //private void CreateOptiGTV_CB_Click(object sender, RoutedEventArgs e)
    //{
    //  var cb = sender as CheckBox;
    //  if (cb.IsChecked == true) { createOptiGTVForSingleLesion = true; }
    //  else { createOptiGTVForSingleLesion = false; }
    //}

    //private void CreateOptiTotal_CB_Click(object sender, RoutedEventArgs e)
    //{
    //  var cb = sender as CheckBox;
    //  if (cb.IsChecked == true) { createOptiTotal  = true; }
    //  else { createOptiTotal = false; }
    //}

    #endregion not used

    #endregion opti structure section events

    #endregion button / checkbox events

    #endregion input changed events

    #endregion event controls
    //---------------------------------------------------------------------------------
    #region helper methods

    #region miscelaneous events

    // methods used in even handlers
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
        dataHeaderList.Add("StructureSet");

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
      string structureSetId = ssId;

      userStatsList.Add(userId);
      userStatsList.Add(scriptId);
      userStatsList.Add(date);
      userStatsList.Add(dayOfWeek);
      userStatsList.Add(time);
      userStatsList.Add(ptId);
      userStatsList.Add(randomPtId);
      userStatsList.Add(course);
      userStatsList.Add(structureSetId);

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

  



    #endregion miscelaneous events

    #endregion helper methods
    //---------------------------------------------------------------------------------



  }
}
