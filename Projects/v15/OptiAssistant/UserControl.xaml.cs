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
    #region default constants

    // MAX STRING LENGTH DEFAULTS
    const int MAX_PREFIX_LENGTH = 7;
    const int MAX_ID_LENGTH = 15; // reduced from 16 to 15 to account for space bw prefix and id

    // AVOIDANCE STRUCTURE DEFAULTS
    const string DEFAULT_AVOIDANCE_PREFIX = "zav";
    const int DEFAULT_AVOIDANCE_GROW_MARGIN = 2;
    const int DEFAULT_AVOIDANCE_CROP_MARGIN = 2;
    const int DEFAULT_AVOID_CROP_FROM_BODY_MARGIN = 0;
    const string AVOIDANCE_DICOM_TYPE = "AVOIDANCE";

    // OPTI STRUCTURE DEFAULTS
    const string DEFAULT_OPTI_PREFIX = "zopti";
    const int DEFAULT_OPTI_GROW_MARGIN = 1;
    const int DEFAULT_OPTI_CROP_MARGIN = 2;
    const int DEFAULT_OPTI_CROP_FROM_BODY_MARGIN = 4;
    const string OPTI_DICOM_TYPE = "PTV";

    // RING STRUCTURE DEFAULTS
    const string DEFAULT_RING_PREFIX = "zring";
    const int DEFAULT_RING_GROW_MARGIN = 10;
    const int DEFAULT_RING_COUNT = 3;
    const int DEFAULT_RING_CROP_MARGIN = 0;
    const string RING_DICOM_TYPE = "CONTROL";

    // MISC DICOM TYPE DEFAULTS
    const string CONTROL_DICOM_TYPE = "CONTROL";

    // CI STRUCTURE DEFAULTS
    const string DEFAULT_CI_PREFIX = "zCI";
    const int DEFAULT_CI_GROW_MARGIN = 5;

    // R50 STRUCTURE DEFAULTS
    const string DEFAULT_R50_PREFIX = "zR50";
    const int DEFAULT_R50_GROW_MARGIN = 30;

    #endregion default constants
    //---------------------------------------------------------------------------------
    #region public variables

    // PUBLIC VARIABLES
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
    public string highresMessage;
    public bool hasHighRes = false;
    public bool hasHighResTargets = false;
    public bool needHRStructures = false;
    private string MESSAGES = string.Empty;
    private int counter = 1;
    public bool createCI = false;
    public bool createR50 = false;
    public int doseLevel1CropMargin = DEFAULT_OPTI_CROP_MARGIN;
    public int doseLevel2CropMargin = DEFAULT_OPTI_CROP_MARGIN;
    public int doseLevel3CropMargin = DEFAULT_OPTI_CROP_MARGIN;

    public bool boolAllTargetsForAvoids = false;
    public bool createAvoidsForMultipleTargets = false;
    public int avoidTarget1CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
    public int avoidTarget2CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
    public int avoidTarget3CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
    public int avoidTarget4CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;

    public bool createPTVEval = false;

    //public bool createOptiGTVForSingleLesion = false;
    //public bool createOptiTotal = false;

    #endregion public variables
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
    public void CreateStructures_Btn_Click_DEV(object sender, RoutedEventArgs e) 
    {
      #region validation

      // validation
      if (CreateAvoids_CB.IsChecked == false && CreateOptis_CB.IsChecked == false && CreateRings_CB.IsChecked == false)
      {
        MessageBox.Show("Oops, it seems no tasks were selected.\n\nPlease select which structures you would like assistance in creating.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        
      }
      if (CreateAvoids_CB.IsChecked == true && MultipleAvoidTargets_CB.IsChecked == true)
      {
        // if avoid targets aren't selected
        if (AvoidTarget1_Radio.IsChecked == true && AvoidTarget1_Combo.SelectedIndex < 0)
        {
          MessageBox.Show("It appears you'd like to create unique avoidance structures for one target:\n\n\t- At least one target should be specified.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          
        }
        if (AvoidTarget2_Radio.IsChecked == true && (AvoidTarget1_Combo.SelectedIndex < 0 || AvoidTarget2_Combo.SelectedIndex < 0))
        {
          MessageBox.Show("It appears you'd like to create unique avoidance structures for two targets:\n\n\t- At least two targets should be specified.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          
        }
        if (AvoidTarget3_Radio.IsChecked == true && (AvoidTarget1_Combo.SelectedIndex < 0 || AvoidTarget2_Combo.SelectedIndex < 0 || AvoidTarget3_Combo.SelectedIndex < 0))
        {
          MessageBox.Show("It appears you'd like to create unique avoidance structures for three targets:\n\n\t- At least three targets should be specified.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          
        }
        if (AvoidTarget4_Radio.IsChecked == true && (AvoidTarget1_Combo.SelectedIndex < 0 || AvoidTarget2_Combo.SelectedIndex < 0 || AvoidTarget3_Combo.SelectedIndex < 0 || AvoidTarget4_Combo.SelectedIndex < 0))
        {
          MessageBox.Show("It appears you'd like to create unique avoidance structures for four targets:\n\n\t- At least four targets should be specified.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          
        }
        // if selected dose level targets are the same
        if (AvoidTarget2_Radio.IsChecked == true && (AvoidTarget1_Combo.SelectedIndex == AvoidTarget2_Combo.SelectedIndex)) { MessageBox.Show("The selected Avoid Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);  }
        if (AvoidTarget3_Radio.IsChecked == true && ((AvoidTarget1_Combo.SelectedIndex == AvoidTarget2_Combo.SelectedIndex) || (AvoidTarget1_Combo.SelectedIndex == AvoidTarget3_Combo.SelectedIndex) || (AvoidTarget2_Combo.SelectedIndex == AvoidTarget3_Combo.SelectedIndex))) { MessageBox.Show("Some of the selected Avoid Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);  }
        if (AvoidTarget4_Radio.IsChecked == true && ((AvoidTarget1_Combo.SelectedIndex == AvoidTarget2_Combo.SelectedIndex) || (AvoidTarget1_Combo.SelectedIndex == AvoidTarget3_Combo.SelectedIndex) || (AvoidTarget1_Combo.SelectedIndex == AvoidTarget4_Combo.SelectedIndex) || (AvoidTarget2_Combo.SelectedIndex == AvoidTarget3_Combo.SelectedIndex) || (AvoidTarget2_Combo.SelectedIndex == AvoidTarget4_Combo.SelectedIndex) || (AvoidTarget3_Combo.SelectedIndex == AvoidTarget4_Combo.SelectedIndex))) { MessageBox.Show("Some of the selected Avoid Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);  }
      }
      if (CreateOptis_CB.IsChecked == true && MultipleDoseLevels_CB.IsChecked == true)
      {
        // if selected counts don't match
        if (DoseLevel1_Radio.IsChecked == true && PTVList_LV.SelectedItems.Count != 1) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);  }
        if (DoseLevel2_Radio.IsChecked == true && PTVList_LV.SelectedItems.Count != 2) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);  }
        if (DoseLevel3_Radio.IsChecked == true && PTVList_LV.SelectedItems.Count != 3) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);  }
        if (DoseLevel4_Radio.IsChecked == true && PTVList_LV.SelectedItems.Count != 4) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);  }
        // dose level targets aren't selected
        if (DoseLevel1_Radio.IsChecked == true && DoseLevel1_Combo.SelectedIndex < 0)
        {
          MessageBox.Show("One Dose Level has been selected:\n\n\t- At least one target should be selected.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          
        }
        if (DoseLevel2_Radio.IsChecked == true && (DoseLevel1_Combo.SelectedIndex < 0 || DoseLevel2_Combo.SelectedIndex < 0))
        {
          MessageBox.Show("Two Dose Levels have been selected:\n\n\t- Targets for at least two dose levels should be selected.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          
        }
        if (DoseLevel3_Radio.IsChecked == true && (DoseLevel1_Combo.SelectedIndex < 0 || DoseLevel2_Combo.SelectedIndex < 0 || DoseLevel3_Combo.SelectedIndex < 0))
        {
          MessageBox.Show("Three Dose Levels have been selected:\n\n\t- Targets for at least three dose levels should be selected.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          
        }
        if (DoseLevel4_Radio.IsChecked == true && (DoseLevel1_Combo.SelectedIndex < 0 || DoseLevel2_Combo.SelectedIndex < 0 || DoseLevel3_Combo.SelectedIndex < 0 || DoseLevel4_Combo.SelectedIndex < 0))
        {
          MessageBox.Show("Four Dose Levels have been selected:\n\n\t- Targets for at least four dose levels should be selected.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          
        }
        // if selected dose level targets are the same
        if (DoseLevel2_Radio.IsChecked == true && (DoseLevel1_Combo.SelectedIndex == DoseLevel2_Combo.SelectedIndex)) { MessageBox.Show("The selected Dose Level Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);  }
        if (DoseLevel3_Radio.IsChecked == true && ((DoseLevel1_Combo.SelectedIndex == DoseLevel2_Combo.SelectedIndex) || (DoseLevel1_Combo.SelectedIndex == DoseLevel3_Combo.SelectedIndex) || (DoseLevel2_Combo.SelectedIndex == DoseLevel3_Combo.SelectedIndex))) { MessageBox.Show("Some of the selected Dose Level Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);  }
        if (DoseLevel4_Radio.IsChecked == true && ((DoseLevel1_Combo.SelectedIndex == DoseLevel2_Combo.SelectedIndex) || (DoseLevel1_Combo.SelectedIndex == DoseLevel3_Combo.SelectedIndex) || (DoseLevel1_Combo.SelectedIndex == DoseLevel4_Combo.SelectedIndex) || (DoseLevel2_Combo.SelectedIndex == DoseLevel3_Combo.SelectedIndex) || (DoseLevel2_Combo.SelectedIndex == DoseLevel4_Combo.SelectedIndex) || (DoseLevel3_Combo.SelectedIndex == DoseLevel4_Combo.SelectedIndex))) { MessageBox.Show("Some of the selected Dose Level Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);  }

      }
      if (CreateAvoids_CB.IsChecked == true && OarList_LV.SelectedItems.Count == 0)
      {
        MessageBox.Show("Oops, it appears you'd like to create avoid structures but haven't selected any structures to create avoids for.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        
      }
      if (CreateOptis_CB.IsChecked == true && PTVList_LV.SelectedItems.Count == 0)
      {
        MessageBox.Show("Oops, it appears you'd like to create opti ptv structures but haven't selected any targets to create optis for.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        
      }
      if (CreateRings_CB.IsChecked == true && PTVListForRings_LV.SelectedItems.Count == 0)
      {
        MessageBox.Show("Oops, it appears you'd like to create ring structures but haven't selected any targets to create rings for.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        
      }
      if (AvoidPrefix_TextBox.Text.Length > MAX_PREFIX_LENGTH)
      {
        MessageBox.Show(string.Format("Oops, it appears you've chosen a Prefix for your Avoid Structures that is {0} characters in length:\r\n\r\n\t- Please limit your prefix to a max of {1} characters", AvoidPrefix_TextBox.Text.Length, MAX_PREFIX_LENGTH), "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        
      }
      if (OptiPrefix_TextBox.Text.Length > MAX_PREFIX_LENGTH)
      {
        MessageBox.Show(string.Format("Oops, it appears you've chosen a Prefix for your Opti Structures that is {0} in length:\r\n\r\n\t- Please limit your prefix to a max of {1} characters", OptiPrefix_TextBox.Text.Length, MAX_PREFIX_LENGTH), "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        
      }
      if (RingPrefix_TextBox.Text.Length > MAX_PREFIX_LENGTH)
      {
        MessageBox.Show(string.Format("Oops, it appears you've chosen a Prefix for your Ring Structures that is {0} in length:\r\n\r\n\t- Please limit your prefix to a max of {1} characters", RingPrefix_TextBox.Text.Length, MAX_PREFIX_LENGTH), "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        
      }

      #endregion validation
    }

    // create structures button
    public void CreateStructures_Btn_Click(object sender, RoutedEventArgs e)
    {
      // for debug messages
      //var tempCounter = 1;
      
      // will be changed to false if any validation logic fails
      var continueToCreateStructures = true;

      // determine further whether High Res Structures are needed
      foreach (var s in OarList_LV.SelectedItems)
      {
        if (highresMessage.Contains(s.ToString()))
        {
          needHRStructures = true;
        }
      }

      foreach (var s in PTVListForRings_LV.SelectedItems)
      {
        if (highresMessage.Contains(s.ToString()))
        {
          needHRStructures = true;
        }
      }

      foreach (var s in PTVList_LV.SelectedItems)
      {
        if (highresMessage.Contains(s.ToString()))
        {
          needHRStructures = true;
          hasHighResTargets = true;
        }
      }


      #region validation

      // validation
      if (CreateAvoids_CB.IsChecked == false && CreateOptis_CB.IsChecked == false && CreateRings_CB.IsChecked == false)
      {
        MessageBox.Show("Oops, it seems no tasks were selected.\n\nPlease select which structures you would like assistance in creating.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        continueToCreateStructures = false;
      }
      if (CreateAvoids_CB.IsChecked == true && MultipleAvoidTargets_CB.IsChecked == true)
      {
        // if avoid targets aren't selected
        if (AvoidTarget1_Radio.IsChecked == true && AvoidTarget1_Combo.SelectedIndex < 0)
        {
          MessageBox.Show("It appears you'd like to create unique avoidance structures for one target:\n\n\t- At least one target should be specified.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          continueToCreateStructures = false;
        }
        if (AvoidTarget2_Radio.IsChecked == true && (AvoidTarget1_Combo.SelectedIndex < 0 || AvoidTarget2_Combo.SelectedIndex < 0))
        {
          MessageBox.Show("It appears you'd like to create unique avoidance structures for two targets:\n\n\t- At least two targets should be specified.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          continueToCreateStructures = false;
        }
        if (AvoidTarget3_Radio.IsChecked == true && (AvoidTarget1_Combo.SelectedIndex < 0 || AvoidTarget2_Combo.SelectedIndex < 0 || AvoidTarget3_Combo.SelectedIndex < 0))
        {
          MessageBox.Show("It appears you'd like to create unique avoidance structures for three targets:\n\n\t- At least three targets should be specified.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          continueToCreateStructures = false;
        }
        if (AvoidTarget4_Radio.IsChecked == true && (AvoidTarget1_Combo.SelectedIndex < 0 || AvoidTarget2_Combo.SelectedIndex < 0 || AvoidTarget3_Combo.SelectedIndex < 0 || AvoidTarget4_Combo.SelectedIndex < 0))
        {
          MessageBox.Show("It appears you'd like to create unique avoidance structures for four targets:\n\n\t- At least four targets should be specified.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          continueToCreateStructures = false;
        }
        // if selected dose level targets are the same
        if (AvoidTarget2_Radio.IsChecked == true && (AvoidTarget1_Combo.SelectedIndex == AvoidTarget2_Combo.SelectedIndex)) { MessageBox.Show("The selected Avoid Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
        if (AvoidTarget3_Radio.IsChecked == true && ((AvoidTarget1_Combo.SelectedIndex == AvoidTarget2_Combo.SelectedIndex) || (AvoidTarget1_Combo.SelectedIndex == AvoidTarget3_Combo.SelectedIndex) || (AvoidTarget2_Combo.SelectedIndex == AvoidTarget3_Combo.SelectedIndex))) { MessageBox.Show("Some of the selected Avoid Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
        if (AvoidTarget4_Radio.IsChecked == true && ((AvoidTarget1_Combo.SelectedIndex == AvoidTarget2_Combo.SelectedIndex) || (AvoidTarget1_Combo.SelectedIndex == AvoidTarget3_Combo.SelectedIndex) || (AvoidTarget1_Combo.SelectedIndex == AvoidTarget4_Combo.SelectedIndex) || (AvoidTarget2_Combo.SelectedIndex == AvoidTarget3_Combo.SelectedIndex) || (AvoidTarget2_Combo.SelectedIndex == AvoidTarget4_Combo.SelectedIndex) || (AvoidTarget3_Combo.SelectedIndex == AvoidTarget4_Combo.SelectedIndex))) { MessageBox.Show("Some of the selected Avoid Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
      }
      if (CreateOptis_CB.IsChecked == true && MultipleDoseLevels_CB.IsChecked == true)
      {
        // if selected counts don't match
        if (DoseLevel1_Radio.IsChecked == true && PTVList_LV.SelectedItems.Count != 1) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
        if (DoseLevel2_Radio.IsChecked == true && PTVList_LV.SelectedItems.Count != 2) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
        if (DoseLevel3_Radio.IsChecked == true && PTVList_LV.SelectedItems.Count != 3) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
        if (DoseLevel4_Radio.IsChecked == true && PTVList_LV.SelectedItems.Count != 4) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
        // dose level targets aren't selected
        if (DoseLevel1_Radio.IsChecked == true && DoseLevel1_Combo.SelectedIndex < 0)
        {
          MessageBox.Show("One Dose Level has been selected:\n\n\t- At least one target should be selected.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          continueToCreateStructures = false;
        }
        if (DoseLevel2_Radio.IsChecked == true && (DoseLevel1_Combo.SelectedIndex < 0 || DoseLevel2_Combo.SelectedIndex < 0))
        {
          MessageBox.Show("Two Dose Levels have been selected:\n\n\t- Targets for at least two dose levels should be selected.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          continueToCreateStructures = false;
        }
        if (DoseLevel3_Radio.IsChecked == true && (DoseLevel1_Combo.SelectedIndex < 0 || DoseLevel2_Combo.SelectedIndex < 0 || DoseLevel3_Combo.SelectedIndex < 0))
        {
          MessageBox.Show("Three Dose Levels have been selected:\n\n\t- Targets for at least three dose levels should be selected.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          continueToCreateStructures = false;
        }
        if (DoseLevel4_Radio.IsChecked == true && (DoseLevel1_Combo.SelectedIndex < 0 || DoseLevel2_Combo.SelectedIndex < 0 || DoseLevel3_Combo.SelectedIndex < 0 || DoseLevel4_Combo.SelectedIndex < 0))
        {
          MessageBox.Show("Four Dose Levels have been selected:\n\n\t- Targets for at least four dose levels should be selected.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          continueToCreateStructures = false;
        }
        // if selected dose level targets are the same
        if (DoseLevel2_Radio.IsChecked == true && (DoseLevel1_Combo.SelectedIndex == DoseLevel2_Combo.SelectedIndex)) { MessageBox.Show("The selected Dose Level Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
        if (DoseLevel3_Radio.IsChecked == true && ((DoseLevel1_Combo.SelectedIndex == DoseLevel2_Combo.SelectedIndex) || (DoseLevel1_Combo.SelectedIndex == DoseLevel3_Combo.SelectedIndex) || (DoseLevel2_Combo.SelectedIndex == DoseLevel3_Combo.SelectedIndex))) { MessageBox.Show("Some of the selected Dose Level Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
        if (DoseLevel4_Radio.IsChecked == true && ((DoseLevel1_Combo.SelectedIndex == DoseLevel2_Combo.SelectedIndex) || (DoseLevel1_Combo.SelectedIndex == DoseLevel3_Combo.SelectedIndex) || (DoseLevel1_Combo.SelectedIndex == DoseLevel4_Combo.SelectedIndex) || (DoseLevel2_Combo.SelectedIndex == DoseLevel3_Combo.SelectedIndex) || (DoseLevel2_Combo.SelectedIndex == DoseLevel4_Combo.SelectedIndex) || (DoseLevel3_Combo.SelectedIndex == DoseLevel4_Combo.SelectedIndex))) { MessageBox.Show("Some of the selected Dose Level Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }

      }
      if (CreateAvoids_CB.IsChecked == true && OarList_LV.SelectedItems.Count == 0)
      {
        MessageBox.Show("Oops, it appears you'd like to create avoid structures but haven't selected any structures to create avoids for.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        continueToCreateStructures = false;
      }
      if (CreateOptis_CB.IsChecked == true && PTVList_LV.SelectedItems.Count == 0)
      {
        MessageBox.Show("Oops, it appears you'd like to create opti ptv structures but haven't selected any targets to create optis for.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        continueToCreateStructures = false;
      }
      if (CreateOptis_CB.IsChecked == true && CreatePTVEval_CB.IsChecked == true && PTVList_LV.SelectedItems.Count != 0)
      {
        foreach (var t in PTVList_LV.SelectedItems)
        {
          var evalId = string.Format("{0}_Eval", Helpers.ProcessStructureId(t.ToString(), MAX_ID_LENGTH - 5));
          var matchedEvalStructure = ss.Structures.SingleOrDefault(st => st.Id == evalId);
          if (matchedEvalStructure != null)
          {
            MessageBox.Show(string.Format("Oops, it appears {0} already exists.\r\n\t- Please rename the PTV or delete the existing Eval\r\n\tstructure to continue.", evalId), "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            continueToCreateStructures = false;
          }

        }
      }
      if (CreateRings_CB.IsChecked == true && PTVListForRings_LV.SelectedItems.Count == 0)
      {
        MessageBox.Show("Oops, it appears you'd like to create ring structures but haven't selected any targets to create rings for.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        continueToCreateStructures = false;
      }
      if (AvoidPrefix_TextBox.Text.Length > MAX_PREFIX_LENGTH) 
      {
        MessageBox.Show(string.Format("Oops, it appears you've chosen a Prefix for your Avoid Structures that is {0} characters in length:\r\n\r\n\t- Please limit your prefix to a max of {1} characters", AvoidPrefix_TextBox.Text.Length, MAX_PREFIX_LENGTH), "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        continueToCreateStructures = false;
      }
      if (OptiPrefix_TextBox.Text.Length > MAX_PREFIX_LENGTH)
      {
        MessageBox.Show(string.Format("Oops, it appears you've chosen a Prefix for your Opti Structures that is {0} in length:\r\n\r\n\t- Please limit your prefix to a max of {1} characters", OptiPrefix_TextBox.Text.Length, MAX_PREFIX_LENGTH), "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        continueToCreateStructures = false;
      }
      if (RingPrefix_TextBox.Text.Length > MAX_PREFIX_LENGTH)
      {
        MessageBox.Show(string.Format("Oops, it appears you've chosen a Prefix for your Ring Structures that is {0} in length:\r\n\r\n\t- Please limit your prefix to a max of {1} characters", RingPrefix_TextBox.Text.Length, MAX_PREFIX_LENGTH), "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        continueToCreateStructures = false;
      }
      
      #endregion validation

      // NOTE: various other validation can be done. To reduce the need for some validation, when user input for margins is invalid, a warning will show informing the user of the invalid entry and that the default value will instead be used. 
      if (continueToCreateStructures)
      {
        using (new WaitCursor())
        {
          // DUBUG: samle message for debugging -- rest have been removed
          //MessageBox.Show(string.Format("{0}", tempCounter));
          //tempCounter += 1;

          // add messages description
          MESSAGES += string.Format("Some General Tasks/Issues during script run: {0}", counter);
          if (needHRStructures) { MESSAGES += "\r\n\t- Some of the selected structures are High Res Structures so\r\n\t\ta High Res Body and High Res Opti Total will be created"; }

          // progress counter in case user clicks Create Structures Button more than once during same instance of script
          counter += 1;

          // allow modifications
          patient.BeginModifications();

          #region find body

          // find body
          Structure body = null;
          try
          {
            body = Helpers.GetBody(ss);
            //MessageBox.Show(string.Format("{0}", body.IsHighResolution));
            if (!body.HasSegment && !body.IsEmpty)
            {
              body = ss.CreateAndSearchBody(ss.GetDefaultSearchBodyParameters());

              var message = "Sorry, the BODY or EXTERNAL Structure was empty:\n\n\t- A Body Structure will be generated using the default Search Body Parameters. Please verify accuracy.";
              var title = "Body Structure Generation";

              MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
          }
          catch
          {
            body = ss.CreateAndSearchBody(ss.GetDefaultSearchBodyParameters());

            var message = "Sorry, could not find a structure of type BODY:\n\n\t- A Body Structure will be generated using the default Search Body Parameters. Please verify accuracy.";
            var title = "Body Structure Generation";

            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
          }

          #endregion find body

          #region get high res structures

          // create high res body for cropping
          Structure bodyHR = null;
          string bodyHRId = "zzBODY_HR";

          // create zopti total High Res for booleans/cropping
          Structure zoptiTotalHR = null;
          string zoptiTotalHRId = "zptv total_HR";
          if (needHRStructures)
          {
            // remove if already there
            Helpers.RemoveStructure(ss, bodyHRId);
            Helpers.RemoveStructure(ss, zoptiTotalHRId);

            // add empty structures
            bodyHR = ss.AddStructure(CONTROL_DICOM_TYPE, bodyHRId);
            zoptiTotalHR = ss.AddStructure(OPTI_DICOM_TYPE, zoptiTotalHRId);

            // copy body to bodyHR
            bodyHR.SegmentVolume = body.SegmentVolume;

            // convert to high res
            if (bodyHR.CanConvertToHighResolution() == true) { bodyHR.ConvertToHighResolution(); MESSAGES += "\r\n\t- High Res Body Created"; }
            if (zoptiTotalHR.CanConvertToHighResolution() == true) { zoptiTotalHR.ConvertToHighResolution(); MESSAGES += "\r\n\t- High Res Target Created"; }

            if (hasSinglePTV || hasMultiplePTVs)
            {
              foreach (var t in sorted_ptvList)
              {
                Structure hrTarget = null;
                var hrId = string.Format("zz{0}_HR", Helpers.ProcessStructureId(t.Id.ToString(), MAX_ID_LENGTH - 5));
                // remove if already there
                Helpers.RemoveStructure(ss, hrId);

                // add empty zopti total structure
                hrTarget = ss.AddStructure(OPTI_DICOM_TYPE, hrId);
                hrTarget.SegmentVolume = t.SegmentVolume;
                if (hrTarget.CanConvertToHighResolution()) { hrTarget.ConvertToHighResolution(); }


                zoptiTotalHR.SegmentVolume = zoptiTotalHR.Or(hrTarget.SegmentVolume);
              }
              zoptiTotalHR.SegmentVolume = Helpers.CropOutsideBodyWithMargin(zoptiTotalHR, bodyHR, cropFromBodyMargin);
            }
          }

          #endregion get high res structures

          #region create zopti total

          // create zopti total if ptv(s) present
          Structure zoptiTotal = null;
          string zoptiTotalId = "zptv total";

          if (hasSinglePTV || hasMultiplePTVs)
          {
            // remove if already there
            Helpers.RemoveStructure(ss, zoptiTotalId);

            // add empty zopti total structure
            zoptiTotal = ss.AddStructure(OPTI_DICOM_TYPE, zoptiTotalId);
            MESSAGES += string.Format("\r\n\t- Structure Added: {0}", zoptiTotalId);

            // boolean ptvs into zopti total
            zoptiTotal.SegmentVolume = sorted_ptvList.Count() > 1 ? Helpers.BooleanStructures(ss, sorted_ptvList) : sorted_ptvList.First().SegmentVolume;
            MESSAGES += string.Format("\r\n\t- Structure Booleaned: {0}", zoptiTotalId);

            // crop from body
            try
            {
              if (zoptiTotal.IsHighResolution)
              {
                zoptiTotal.SegmentVolume = Helpers.CropOutsideBodyWithMargin(zoptiTotal, bodyHR, -DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                MESSAGES += string.Format("\r\n\t- {0} Cropped {1} mm From High Res Body Surface", zoptiTotal.Id, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
              }
              else
              {
                zoptiTotal.SegmentVolume = Helpers.CropOutsideBodyWithMargin(zoptiTotal, body, -DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                MESSAGES += string.Format("\r\n\t- {0} Cropped {1} mm From Body Surface", zoptiTotal.Id, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
              }
            }
            catch
            {
              MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From Body Surface***", zoptiTotal.Id);
            }


          }

          #endregion create zopti total

          #region avoidance structures

          if (CreateAvoids_CB.IsChecked == true)
          {
            
            var avStructuresToMake = OarList_LV.SelectedItems;
            string avPrefix;
            int avGrowMargin;
            int avCropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;

            // set prefix
            if (AvoidPrefix_TextBox.Text == "" || string.IsNullOrWhiteSpace(AvoidPrefix_TextBox.Text)) { avPrefix = DEFAULT_AVOIDANCE_PREFIX; } else { avPrefix = AvoidPrefix_TextBox.Text; }

            // set grow margin
            if (AvoidGrowMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(AvoidGrowMargin_TextBox.Text)) { avGrowMargin = DEFAULT_AVOIDANCE_GROW_MARGIN; }
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
                MessageBox.Show(string.Format("Oops, an invalid value was used for the avoidance structure grow margin ({0}). The DEFAULT of {1}mm will be used.", AvoidGrowMargin_TextBox.Text, DEFAULT_AVOIDANCE_GROW_MARGIN));
              }
            }

            // set crop margin
            if (hasSinglePTV || BooleanAllTargets_CB.IsChecked == true)
            {
              if (AvoidCropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(AvoidCropMargin_TextBox.Text)) { avCropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN; }
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
                  MessageBox.Show(string.Format("Oops, an invalid value was used for the avoidance structure crop margin ({0}). The DEFAULT of {1}mm will be used.", AvoidCropMargin_TextBox.Text, DEFAULT_AVOIDANCE_CROP_MARGIN));
                }
              }
            }

            foreach (var s in avStructuresToMake)
            {
              var oar = ss.Structures.Single(st => st.Id == s.ToString());
              Structure avoidStructure = null;
              var avId = string.Empty;
              // if one or more PTVs detected
              if (hasSinglePTV || hasMultiplePTVs)
              {
                if (BooleanAllTargets_CB.IsChecked == true)
                {
                  avId = string.Format("{0} {1}", avPrefix, Helpers.ProcessStructureId(oar.Id.ToString(), MAX_ID_LENGTH - avPrefix.Length));

                  // remove structure if present in ss
                  Helpers.RemoveStructure(ss, avId);

                  // add empty avoid structure
                  avoidStructure = ss.AddStructure(AVOIDANCE_DICOM_TYPE, avId);

                  // copy oar with defined margin
                  avoidStructure.SegmentVolume = Helpers.AddMargin(oar, (double)avGrowMargin);
                  MESSAGES += string.Format("\r\n\t- {0} added w/ {1}mm margin",
                                                          avoidStructure.Id,
                                                          avGrowMargin
                  );

                  try
                  {

                    // crop avoid structure from ptv total (if ptvs are found)
                    try
                    {
                      if (avoidStructure.IsHighResolution)
                      {
                        avoidStructure.SegmentVolume = Helpers.CropStructure(avoidStructure.SegmentVolume, zoptiTotalHR.SegmentVolume, avCropMargin);
                      }
                      else
                      {
                        avoidStructure.SegmentVolume = Helpers.CropStructure(avoidStructure.SegmentVolume, zoptiTotal.SegmentVolume, avCropMargin);
                      }

                      MESSAGES += string.Format(" & cropped {0}mm from {1}",
                                                        avCropMargin,
                                                        zoptiTotal.Id
                      );
                    }
                    catch
                    {
                      try
                      {
                        if (avoidStructure.CanConvertToHighResolution()) { avoidStructure.ConvertToHighResolution(); }

                        avoidStructure.SegmentVolume = Helpers.CropStructure(avoidStructure.SegmentVolume, zoptiTotalHR.SegmentVolume, avCropMargin);

                        MESSAGES += string.Format(" & cropped {0}mm from {1}",
                                                        avCropMargin,
                                                        zoptiTotal.Id
                        );
                      }
                      catch
                      {
                        if (sorted_ptvList.Count() >= 1)
                        {
                          MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From Targets***", avoidStructure.Id);
                        }
                        else
                        {
                          MESSAGES += string.Format("\r\n\t- ***No PTVs to crop avoid structures from***", avoidStructure.Id);
                        }
                      }
                    }


                    // crop from body
                    try
                    {
                      if (avoidStructure.IsHighResolution)
                      {
                        avoidStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(avoidStructure, bodyHR, -DEFAULT_AVOID_CROP_FROM_BODY_MARGIN);
                      }
                      else
                      {
                        avoidStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(avoidStructure, body, -DEFAULT_AVOID_CROP_FROM_BODY_MARGIN);
                      }
                    }
                    catch
                    {
                      MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From Body***", avoidStructure.Id);
                    }

                  }
                  catch
                  {
                    MESSAGES += string.Format("\r\n\t- ***Trouble creating {0}***", avoidStructure.Id);
                  }



                }
                else if (MultipleAvoidTargets_CB.IsChecked == true)
                {
                  string avoidTarget1 = null;
                  string avoidTarget2 = null;
                  string avoidTarget3 = null;
                  string avoidTarget4 = null;
                  Structure avoidTarget = null;
                  Structure avoidTarget_HR = null;

                  int avoidTarget1CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
                  int avoidTarget2CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
                  int avoidTarget3CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
                  int avoidTarget4CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;

                  var targetNumber = 1;
                  var targetsToCreateAvoidsFor = new List<string>();

                  // define avoid targets
                  if (AvoidTarget1_Radio.IsChecked == true)
                  {
                    targetsToCreateAvoidsFor.Add(AvoidTarget1_Combo.SelectedItem.ToString());
                    avoidTarget1 = AvoidTarget1_Combo.SelectedItem.ToString();
                  }
                  else if (AvoidTarget2_Radio.IsChecked == true)
                  {
                    targetsToCreateAvoidsFor.Add(AvoidTarget1_Combo.SelectedItem.ToString());
                    targetsToCreateAvoidsFor.Add(AvoidTarget2_Combo.SelectedItem.ToString());
                    avoidTarget1 = AvoidTarget1_Combo.SelectedItem.ToString();
                    avoidTarget2 = AvoidTarget2_Combo.SelectedItem.ToString();
                  }
                  else if (AvoidTarget3_Radio.IsChecked == true)
                  {
                    targetsToCreateAvoidsFor.Add(AvoidTarget1_Combo.SelectedItem.ToString());
                    targetsToCreateAvoidsFor.Add(AvoidTarget2_Combo.SelectedItem.ToString());
                    targetsToCreateAvoidsFor.Add(AvoidTarget3_Combo.SelectedItem.ToString());
                    avoidTarget1 = AvoidTarget1_Combo.SelectedItem.ToString();
                    avoidTarget2 = AvoidTarget2_Combo.SelectedItem.ToString();
                    avoidTarget3 = AvoidTarget3_Combo.SelectedItem.ToString();
                  }
                  else if (AvoidTarget4_Radio.IsChecked == true)
                  {
                    targetsToCreateAvoidsFor.Add(AvoidTarget1_Combo.SelectedItem.ToString());
                    targetsToCreateAvoidsFor.Add(AvoidTarget2_Combo.SelectedItem.ToString());
                    targetsToCreateAvoidsFor.Add(AvoidTarget3_Combo.SelectedItem.ToString());
                    targetsToCreateAvoidsFor.Add(AvoidTarget4_Combo.SelectedItem.ToString());
                    avoidTarget1 = AvoidTarget1_Combo.SelectedItem.ToString();
                    avoidTarget2 = AvoidTarget2_Combo.SelectedItem.ToString();
                    avoidTarget3 = AvoidTarget3_Combo.SelectedItem.ToString();
                    avoidTarget4 = AvoidTarget4_Combo.SelectedItem.ToString();
                  }

                  // get crop margins
                  if (targetsToCreateAvoidsFor.Count == 1)
                  {
                    // set crop margin
                    if (AvoidTarget1CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(AvoidTarget1CropMargin_TextBox.Text)) { avoidTarget1CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN; }
                    else
                    {
                      if (int.TryParse(AvoidTarget1CropMargin_TextBox.Text, out avoidTarget1CropMargin))
                      {
                        //parsing successful 
                      }
                      else
                      {
                        //parsing failed. 
                        avoidTarget1CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
                        MessageBox.Show(string.Format("Oops, an invalid value ({0}) was used for cropping avoidance structures from {1}. The DEFAULT of {2}mm will be used.",
                            AvoidTarget1CropMargin_TextBox.Text,
                            avoidTarget1,
                            DEFAULT_AVOIDANCE_CROP_MARGIN));
                      }
                    }
                  }
                  else if (targetsToCreateAvoidsFor.Count == 2)
                  {
                    // set crop margin
                    // 1
                    if (AvoidTarget1CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(AvoidTarget1CropMargin_TextBox.Text)) { avoidTarget1CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN; }
                    else
                    {
                      if (int.TryParse(AvoidTarget1CropMargin_TextBox.Text, out avoidTarget1CropMargin))
                      {
                        //parsing successful 
                      }
                      else
                      {
                        //parsing failed. 
                        avoidTarget1CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
                        MessageBox.Show(string.Format("Oops, an invalid value ({0}) was used for cropping avoidance structures from {1}. The DEFAULT of {2}mm will be used.",
                            AvoidTarget1CropMargin_TextBox.Text,
                            avoidTarget1,
                            DEFAULT_AVOIDANCE_CROP_MARGIN));
                      }
                    }

                    // 2
                    if (AvoidTarget2CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(AvoidTarget2CropMargin_TextBox.Text)) { avoidTarget2CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN; }
                    else
                    {
                      if (int.TryParse(AvoidTarget2CropMargin_TextBox.Text, out avoidTarget2CropMargin))
                      {
                        //parsing successful 
                      }
                      else
                      {
                        //parsing failed. 
                        avoidTarget2CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
                        MessageBox.Show(string.Format("Oops, an invalid value ({0}) was used for cropping avoidance structures from {1}. The DEFAULT of {2}mm will be used.",
                            AvoidTarget2CropMargin_TextBox.Text,
                            avoidTarget2,
                            DEFAULT_AVOIDANCE_CROP_MARGIN));
                      }
                    }
                  }
                  else if (targetsToCreateAvoidsFor.Count == 3)
                  {
                    // set crop margin
                    // 1
                    if (AvoidTarget1CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(AvoidTarget1CropMargin_TextBox.Text)) { avoidTarget1CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN; }
                    else
                    {
                      if (int.TryParse(AvoidTarget1CropMargin_TextBox.Text, out avoidTarget1CropMargin))
                      {
                        //parsing successful 
                      }
                      else
                      {
                        //parsing failed. 
                        avoidTarget1CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
                        MessageBox.Show(string.Format("Oops, an invalid value ({0}) was used for cropping avoidance structures from {1}. The DEFAULT of {2}mm will be used.",
                            AvoidTarget1CropMargin_TextBox.Text,
                            avoidTarget1,
                            DEFAULT_AVOIDANCE_CROP_MARGIN));
                      }
                    }

                    // 2
                    if (AvoidTarget2CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(AvoidTarget2CropMargin_TextBox.Text)) { avoidTarget2CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN; }
                    else
                    {
                      if (int.TryParse(AvoidTarget2CropMargin_TextBox.Text, out avoidTarget2CropMargin))
                      {
                        //parsing successful 
                      }
                      else
                      {
                        //parsing failed. 
                        avoidTarget2CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
                        MessageBox.Show(string.Format("Oops, an invalid value ({0}) was used for cropping avoidance structures from {1}. The DEFAULT of {2}mm will be used.",
                            AvoidTarget2CropMargin_TextBox.Text,
                            avoidTarget2,
                            DEFAULT_AVOIDANCE_CROP_MARGIN));
                      }
                    }

                    // 3
                    if (AvoidTarget3CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(AvoidTarget3CropMargin_TextBox.Text)) { avoidTarget3CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN; }
                    else
                    {
                      if (int.TryParse(AvoidTarget3CropMargin_TextBox.Text, out avoidTarget3CropMargin))
                      {
                        //parsing successful 
                      }
                      else
                      {
                        //parsing failed. 
                        avoidTarget3CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
                        MessageBox.Show(string.Format("Oops, an invalid value ({0}) was used for cropping avoidance structures from {1}. The DEFAULT of {2}mm will be used.",
                            AvoidTarget3CropMargin_TextBox.Text,
                            avoidTarget3,
                            DEFAULT_AVOIDANCE_CROP_MARGIN));
                      }
                    }

                  }
                  else if (targetsToCreateAvoidsFor.Count == 4)
                  {
                    // set crop margin
                    // 1
                    if (AvoidTarget1CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(AvoidTarget1CropMargin_TextBox.Text)) { avoidTarget1CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN; }
                    else
                    {
                      if (int.TryParse(AvoidTarget1CropMargin_TextBox.Text, out avoidTarget1CropMargin))
                      {
                        //parsing successful 
                      }
                      else
                      {
                        //parsing failed. 
                        avoidTarget1CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
                        MessageBox.Show(string.Format("Oops, an invalid value ({0}) was used for cropping avoidance structures from {1}. The DEFAULT of {2}mm will be used.",
                            AvoidTarget1CropMargin_TextBox.Text,
                            avoidTarget1,
                            DEFAULT_AVOIDANCE_CROP_MARGIN));
                      }
                    }

                    // 2
                    if (AvoidTarget2CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(AvoidTarget2CropMargin_TextBox.Text)) { avoidTarget2CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN; }
                    else
                    {
                      if (int.TryParse(AvoidTarget2CropMargin_TextBox.Text, out avoidTarget2CropMargin))
                      {
                        //parsing successful 
                      }
                      else
                      {
                        //parsing failed. 
                        avoidTarget2CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
                        MessageBox.Show(string.Format("Oops, an invalid value ({0}) was used for cropping avoidance structures from {1}. The DEFAULT of {2}mm will be used.",
                            AvoidTarget2CropMargin_TextBox.Text,
                            avoidTarget2,
                            DEFAULT_AVOIDANCE_CROP_MARGIN));
                      }
                    }

                    // 3
                    if (AvoidTarget3CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(AvoidTarget3CropMargin_TextBox.Text)) { avoidTarget3CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN; }
                    else
                    {
                      if (int.TryParse(AvoidTarget3CropMargin_TextBox.Text, out avoidTarget3CropMargin))
                      {
                        //parsing successful 
                      }
                      else
                      {
                        //parsing failed. 
                        avoidTarget3CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
                        MessageBox.Show(string.Format("Oops, an invalid value ({0}) was used for cropping avoidance structures from {1}. The DEFAULT of {2}mm will be used.",
                            AvoidTarget3CropMargin_TextBox.Text,
                            avoidTarget3,
                            DEFAULT_AVOIDANCE_CROP_MARGIN));
                      }
                    }

                    // 4
                    if (AvoidTarget4CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(AvoidTarget4CropMargin_TextBox.Text)) { avoidTarget4CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN; }
                    else
                    {
                      if (int.TryParse(AvoidTarget4CropMargin_TextBox.Text, out avoidTarget4CropMargin))
                      {
                        //parsing successful 
                      }
                      else
                      {
                        //parsing failed. 
                        avoidTarget4CropMargin = DEFAULT_AVOIDANCE_CROP_MARGIN;
                        MessageBox.Show(string.Format("Oops, an invalid value ({0}) was used for cropping avoidance structures from {1}. The DEFAULT of {2}mm will be used.",
                            AvoidTarget4CropMargin_TextBox.Text,
                            avoidTarget4,
                            DEFAULT_AVOIDANCE_CROP_MARGIN));
                      }
                    }

                  }

                  foreach (var t in targetsToCreateAvoidsFor)
                  {
                    avId = string.Format("{0} {1} {2}",
                                            avPrefix,
                                            targetNumber,
                                            Helpers.ProcessStructureId(oar.Id.ToString(), MAX_ID_LENGTH - (avPrefix.Length + 2)) // +# to account for strongly typed characters/spaces beyond those already accounted for
                    );

                    var avoidTargetHRId = string.Format("zz{0}_HR", Helpers.ProcessStructureId(t, MAX_ID_LENGTH - 5));

                    // match ptv in structure set
                    avoidTarget = ss.Structures.Single(st => st.Id == t);
                    avoidTarget_HR = ss.Structures.Single(st => st.Id == avoidTargetHRId);

                    // match crop margin
                    if (targetNumber == 1) { avCropMargin = avoidTarget1CropMargin; }
                    else if (targetNumber == 2) { avCropMargin = avoidTarget2CropMargin; }
                    else if (targetNumber == 3) { avCropMargin = avoidTarget3CropMargin; }
                    else if (targetNumber == 4) { avCropMargin = avoidTarget4CropMargin; }

                    // remove structure if present in ss
                    Helpers.RemoveStructure(ss, avId);

                    // add empty avoid structure
                    avoidStructure = ss.AddStructure(AVOIDANCE_DICOM_TYPE, avId);


                    // copy oar with defined margin
                    avoidStructure.SegmentVolume = Helpers.AddMargin(oar, (double)avGrowMargin);
                    MESSAGES += string.Format("\r\n\t- {0} added w/ {1}mm margin",
                                                          avoidStructure.Id,
                                                          avGrowMargin
                    );

                    try
                    {

                      // crop avoid structure from avoid ptv
                      try
                      {
                        if (avoidStructure.IsHighResolution)
                        {
                          avoidStructure.SegmentVolume = Helpers.CropStructure(avoidStructure.SegmentVolume, avoidTarget_HR.SegmentVolume, avCropMargin);
                        }
                        else
                        {
                          avoidStructure.SegmentVolume = Helpers.CropStructure(avoidStructure.SegmentVolume, avoidTarget.SegmentVolume, avCropMargin);
                        }

                        MESSAGES += string.Format(" & cropped {0}mm from {1}",
                                                          avCropMargin,
                                                          avoidTarget.Id
                        );
                      }
                      catch
                      {
                        try
                        {
                          if (avoidStructure.CanConvertToHighResolution()) { avoidStructure.ConvertToHighResolution(); }

                          avoidStructure.SegmentVolume = Helpers.CropStructure(avoidStructure.SegmentVolume, avoidTarget_HR.SegmentVolume, avCropMargin);

                          MESSAGES += string.Format(" & cropped {0}mm from {1}",
                                                          avCropMargin,
                                                          avoidTarget.Id
                          );
                        }
                        catch
                        {
                          MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From {1}***", avoidStructure.Id, avoidTarget.Id);
                        }
                      }

                      // crop from body
                      try
                      {
                        if (avoidStructure.IsHighResolution)
                        {
                          avoidStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(avoidStructure, bodyHR, -DEFAULT_AVOID_CROP_FROM_BODY_MARGIN);
                        }
                        else
                        {
                          avoidStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(avoidStructure, body, -DEFAULT_AVOID_CROP_FROM_BODY_MARGIN);
                        }
                      }
                      catch
                      {
                        MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From Body***", avoidStructure.Id);
                      }
                    }
                    catch
                    {
                      MESSAGES += string.Format("\r\n\t- ***Trouble creating {0}***", avoidStructure.Id);
                    }

                    // increment target number for avoid structure naming/grouping by the target it's cropped away from
                    targetNumber += 1;
                  }
                }
              }
              // if no PTVs detected (no structures that start with PTV)
              else 
              {
                avId = string.Format("{0} {1}", avPrefix, Helpers.ProcessStructureId(oar.Id.ToString(), MAX_ID_LENGTH - avPrefix.Length));

                // remove structure if present in ss
                Helpers.RemoveStructure(ss, avId);

                // add empty avoid structure
                avoidStructure = ss.AddStructure(AVOIDANCE_DICOM_TYPE, avId);

                // copy oar with defined margin
                avoidStructure.SegmentVolume = Helpers.AddMargin(oar, (double)avGrowMargin);
                MESSAGES += string.Format("\r\n\t- {0} added w/ {1}mm margin",
                                                        avoidStructure.Id,
                                                        avGrowMargin
                );

                try
                {

                  // crop from body
                  try
                  {
                    if (avoidStructure.IsHighResolution)
                    {
                      avoidStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(avoidStructure, bodyHR, -DEFAULT_AVOID_CROP_FROM_BODY_MARGIN);
                    }
                    else
                    {
                      avoidStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(avoidStructure, body, -DEFAULT_AVOID_CROP_FROM_BODY_MARGIN);
                    }
                  }
                  catch
                  {
                    MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From Body***", avoidStructure.Id);
                  }

                }
                catch
                {
                  MESSAGES += string.Format("\r\n\t- ***Trouble creating {0}***", avoidStructure.Id);
                }



              }
            }
          }

          #endregion avoidance structures

          #region opti structures

          if (CreateOptis_CB.IsChecked == true)
          {
            
            cropFromBody = (bool)CropFromBody_CB.IsChecked;
            createCI = (bool)CreateCI_CB.IsChecked;
            createR50 = (bool)CreateR50_CB.IsChecked;
            createPTVEval = (bool)CreatePTVEval_CB.IsChecked;


            var optiStructuresToMake = PTVList_LV.SelectedItems;
            string optiPrefix = DEFAULT_OPTI_PREFIX;
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
            if (OptiPrefix_TextBox.Text == "" || string.IsNullOrWhiteSpace(OptiPrefix_TextBox.Text)) { optiPrefix = DEFAULT_OPTI_PREFIX; }
            else { optiPrefix = OptiPrefix_TextBox.Text; }

            // set crop from body margin
            if (cropFromBody)
            {
              if (BodyCropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(BodyCropMargin_TextBox.Text)) { optiCropFromBodyMargin = DEFAULT_OPTI_CROP_FROM_BODY_MARGIN; }
              else
              {
                if (int.TryParse(BodyCropMargin_TextBox.Text, out optiCropFromBodyMargin))
                {
                  //parsing successful 
                }
                else
                {
                  optiCropFromBodyMargin = DEFAULT_OPTI_CROP_FROM_BODY_MARGIN;
                  MessageBox.Show(string.Format("Oops, an invalid value was used for the opti crop from body margin ({0}). The DEFAULT of {1}mm will be used.", BodyCropMargin_TextBox.Text, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN));
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
              MessageBox.Show(string.Format("Oops, an invalid value was used for the opti structure grow margin ({0}). The DEFAULT of {1}mm will be used.", OptiGrowMargin_TextBox.Text, DEFAULT_OPTI_GROW_MARGIN));
            }

            // if multiple dose levels need to set opti crop margin as well as determine which targets correspond to which dose levels
            if (MultipleDoseLevels_CB.IsChecked == true)
            {
              // set opti crop margin
              if (OptiCropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(OptiCropMargin_TextBox.Text))
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
                doseLevel1Target = Helpers.GetStructure(ss, DoseLevel1_Combo.SelectedItem.ToString());
                MessageBox.Show(string.Format("Dose Level 1 Set to: {0}", doseLevel1Target.Id));
              }
              else if (DoseLevel2_Radio.IsChecked == true)
              {
                doseLevel1Target = Helpers.GetStructure(ss, DoseLevel1_Combo.SelectedItem.ToString());
                doseLevel2Target = Helpers.GetStructure(ss, DoseLevel2_Combo.SelectedItem.ToString());

                hasTwoDoseLevels = true;
              }
              else if (DoseLevel3_Radio.IsChecked == true)
              {
                doseLevel1Target = Helpers.GetStructure(ss, DoseLevel1_Combo.SelectedItem.ToString());
                doseLevel2Target = Helpers.GetStructure(ss, DoseLevel2_Combo.SelectedItem.ToString());
                doseLevel3Target = Helpers.GetStructure(ss, DoseLevel3_Combo.SelectedItem.ToString());

                hasThreeDoseLevels = true;
              }
              else if (DoseLevel4_Radio.IsChecked == true)
              {
                doseLevel1Target = Helpers.GetStructure(ss, DoseLevel1_Combo.SelectedItem.ToString());
                doseLevel2Target = Helpers.GetStructure(ss, DoseLevel2_Combo.SelectedItem.ToString());
                doseLevel3Target = Helpers.GetStructure(ss, DoseLevel3_Combo.SelectedItem.ToString());
                doseLevel4Target = Helpers.GetStructure(ss, DoseLevel4_Combo.SelectedItem.ToString());

                hasFourDoseLevels = true;
              }
            }

            // create optis
            foreach (var s in optiStructuresToMake)
            {
              var ptv = ss.Structures.Single(st => st.Id == s.ToString());
              var optiId = string.Format("{0} {1}", optiPrefix, Helpers.ProcessStructureId(ptv.Id.ToString(), MAX_ID_LENGTH - optiPrefix.Length));
              var evalId = string.Format("{0}_Eval", Helpers.ProcessStructureId(ptv.Id.ToString(), MAX_ID_LENGTH - 5));
              Structure evalStructure = null;
              var zztemp = 0;

              if (needHRStructures && ptv.CanConvertToHighResolution())
              {
                ptv.ConvertToHighResolution();
              }

              // remove structure if present in ss
              Helpers.RemoveStructure(ss, optiId);

              // add empty opti structure
              var optiStructure = ss.AddStructure(OPTI_DICOM_TYPE, optiId);

              // eval ptv
              if (createPTVEval)
              {
                Helpers.RemoveStructure(ss, evalId);
                evalStructure = ss.AddStructure(OPTI_DICOM_TYPE, evalId);
              }

              // copy ptv with defined margin
              optiStructure.SegmentVolume = Helpers.AddMargin(ptv, (double)optiGrowMargin);
              MESSAGES += string.Format("\r\n\t- {0} added w/ {1}mm margin", optiStructure.Id, optiGrowMargin);
              

              // crop OPTI structure from body surface
              if (cropFromBody)
              {
                try
                {
                  if (optiStructure.IsHighResolution)
                  {
                    optiStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(optiStructure, bodyHR, -optiCropFromBodyMargin);
                    MESSAGES += string.Format("\r\n\t- {0} Cropped {1} mm From Body Surface", optiStructure.Id, optiCropFromBodyMargin);
                  }
                  else
                  {
                    optiStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(optiStructure, body, -optiCropFromBodyMargin);
                    MESSAGES += string.Format("\r\n\t- {0} Cropped {1} mm From Body Surface", optiStructure.Id, optiCropFromBodyMargin);
                  }
                  

                  if (evalStructure != null)
                  {
                    if (evalStructure.IsHighResolution)
                    {
                      evalStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(optiStructure, bodyHR, -DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                      MESSAGES += string.Format("\r\n\t- {0} Cropped {1} mm From Body Surface", evalStructure.Id, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                    }
                  }
                }
                catch
                {
                  MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From Body***", optiStructure.Id);
                }
              }

              zztemp += 1;
              // PTV Eval structure
              if (evalStructure != null)
              {

                // copy ptv to eval ptv
                evalStructure.SegmentVolume = ptv.SegmentVolume;
                MESSAGES += string.Format("\r\n\t- {0} added", evalStructure.Id);
                zztemp += 1;

                try
                {
                  if (evalStructure.IsHighResolution)
                  {
                    evalStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(evalStructure, bodyHR, -DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                    MESSAGES += string.Format(" and cropped {1} mm From Body Surface", evalStructure.Id, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                  }
                  else
                  {
                    evalStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(evalStructure, body, -DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                    MESSAGES += string.Format(" and cropped {1} mm From Body Surface", evalStructure.Id, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                  }
                }
                catch
                {
                  MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From Body***", evalStructure.Id);
                }
              }
              zztemp += 1;

              // create CI structure
              if (createCI)
              {
                try
                {
                  // ci structure id
                  var ciId = string.Format("{0} {1}", DEFAULT_CI_PREFIX, Helpers.ProcessStructureId(ptv.Id.ToString(), MAX_ID_LENGTH - DEFAULT_CI_PREFIX.Length));

                  // set ci grow margin
                  var ciGrowMargin = DEFAULT_CI_GROW_MARGIN;
                  if (int.TryParse(CIMargin_TextBox.Text, out ciGrowMargin))
                  {
                    //parsing successful 
                  }
                  else
                  {
                    //parsing failed. 
                    ciGrowMargin = DEFAULT_OPTI_GROW_MARGIN;
                    MessageBox.Show(string.Format("Oops, an invalid value was used for the CI structure grow margin ({0}). The DEFAULT of {1}mm will be used.", CIMargin_TextBox.Text, DEFAULT_CI_GROW_MARGIN));
                  }

                  // remove structure if present in ss
                  Helpers.RemoveStructure(ss, ciId);

                  // add empty avoid structure
                  var ciStructure = ss.AddStructure(CONTROL_DICOM_TYPE, ciId);

                  // copy ptv with defined margin
                  ciStructure.SegmentVolume = Helpers.AddMargin(ptv, (double)ciGrowMargin);
                  MESSAGES += string.Format("\r\n\t- {0} added with {1}mm margin", ciStructure.Id, ciGrowMargin);
                }
                catch
                {
                  MESSAGES += string.Format("\r\n\t- ***Trouble Creating CI Structure***");
                }
              }

              // create CI structure
              if (createR50)
              {
                try
                {
                  // r50 structure id
                  var r50Id = string.Format("{0} {1}", DEFAULT_R50_PREFIX, Helpers.ProcessStructureId(ptv.Id.ToString(), MAX_ID_LENGTH - DEFAULT_R50_PREFIX.Length));

                  // set r50 grow margin
                  var r50GrowMargin = DEFAULT_R50_GROW_MARGIN;
                  if (int.TryParse(R50Margin_TextBox.Text, out r50GrowMargin))
                  {
                    //parsing successful 
                  }
                  else
                  {
                    //parsing failed. 
                    r50GrowMargin = DEFAULT_OPTI_GROW_MARGIN;
                    MessageBox.Show(string.Format("Oops, an invalid value was used for the CI structure grow margin ({0}). The DEFAULT of {1}mm will be used.", R50Margin_TextBox.Text, DEFAULT_R50_GROW_MARGIN));
                  }

                  // remove structure if present in ss
                  Helpers.RemoveStructure(ss, r50Id);

                  // add empty avoid structure
                  var r50Structure = ss.AddStructure(CONTROL_DICOM_TYPE, r50Id);

                  // copy ptv with defined margin
                  r50Structure.SegmentVolume = Helpers.AddMargin(ptv, (double)r50GrowMargin);
                  MESSAGES += string.Format("\r\n\t- {0} added with {1}mm margin", r50Structure.Id, r50GrowMargin);
                }
                catch
                {
                  MESSAGES += string.Format("\r\n\t- ***Trouble Creating CI Structure***");
                }
              }

              if (MultipleDoseLevels_CB.IsChecked == true)
              {
                // NOTE: may not be necessary to convert these to HR since only cropping from one another...may be necessary tho if original target was high res
                if (hasHighResTargets)
                {
                  if (ptv.Id == doseLevel1Target.Id) { opti1 = optiStructure; if (opti1.CanConvertToHighResolution()) { opti1.ConvertToHighResolution(); } }
                  else if (ptv.Id == doseLevel2Target.Id) { opti2 = optiStructure; if (opti2.CanConvertToHighResolution()) { opti2.ConvertToHighResolution(); } }
                  else if (ptv.Id == doseLevel3Target.Id) { opti3 = optiStructure; if (opti3.CanConvertToHighResolution()) { opti3.ConvertToHighResolution(); } }
                  else if (ptv.Id == doseLevel4Target.Id) { opti4 = optiStructure; if (opti4.CanConvertToHighResolution()) { opti4.ConvertToHighResolution(); } }
                  if (optiStructure.CanConvertToHighResolution()) { optiStructure.ConvertToHighResolution(); }
                }
                else
                {
                  if (ptv.Id == doseLevel1Target.Id) { opti1 = optiStructure; }
                  else if (ptv.Id == doseLevel2Target.Id) { opti2 = optiStructure; }
                  else if (ptv.Id == doseLevel3Target.Id) { opti3 = optiStructure; }
                  else if (ptv.Id == doseLevel4Target.Id) { opti4 = optiStructure; }
                }
              }

              optisMade.Add(optiStructure);

            }

            // crop optis from one another when there are multiple dose levels
            if (MultipleDoseLevels_CB.IsChecked == true)
            {
              try
              {
                // if multiple dose levels need to set opti crop margin as well as determine which targets correspond to which dose levels
                if (hasTwoDoseLevels)
                {
                  // set opti crop margins for the dose levels
                  
                  // 1
                  if (DoseLevel1CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(DoseLevel1CropMargin_TextBox.Text))
                  {
                    doseLevel1CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                  }
                  else
                  {
                    if (int.TryParse(DoseLevel1CropMargin_TextBox.Text, out doseLevel1CropMargin))
                    {
                      //parsing successful 
                    }
                    else
                    {
                      //parsing failed. 
                      doseLevel1CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                      //MessageBox.Show("Oops, please enter a valid Crop Margin for your opti structures.");
                      MessageBox.Show(string.Format("Oops, an invalid value was used for the Dose Level 1 crop margin ({0}). The DEFAULT of {1}mm will be used.", DoseLevel1CropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
                    }
                  }

                  // 2
                  if (DoseLevel2CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(DoseLevel2CropMargin_TextBox.Text))
                  {
                    doseLevel2CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                  }
                  else
                  {
                    if (int.TryParse(DoseLevel2CropMargin_TextBox.Text, out doseLevel2CropMargin))
                    {
                      //parsing successful 
                    }
                    else
                    {
                      //parsing failed. 
                      doseLevel2CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                      //MessageBox.Show("Oops, please enter a valid Crop Margin for your opti structures.");
                      MessageBox.Show(string.Format("Oops, an invalid value was used for the Dose Level 1 crop margin ({0}). The DEFAULT of {1}mm will be used.", DoseLevel2CropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
                    }
                  }

                }

                else if (hasThreeDoseLevels)
                {
                  // set opti crop margins for the dose levels

                  // 1
                  if (DoseLevel1CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(DoseLevel1CropMargin_TextBox.Text))
                  {
                    doseLevel1CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                  }
                  else
                  {
                    if (int.TryParse(DoseLevel1CropMargin_TextBox.Text, out doseLevel1CropMargin))
                    {
                      //parsing successful 
                    }
                    else
                    {
                      //parsing failed. 
                      doseLevel1CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                      //MessageBox.Show("Oops, please enter a valid Crop Margin for your opti structures.");
                      MessageBox.Show(string.Format("Oops, an invalid value was used for the Dose Level 1 crop margin ({0}). The DEFAULT of {1}mm will be used.", DoseLevel1CropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
                    }
                  }
                  // 2
                  if (DoseLevel2CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(DoseLevel2CropMargin_TextBox.Text))
                  {
                    doseLevel2CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                  }
                  else
                  {
                    if (int.TryParse(DoseLevel2CropMargin_TextBox.Text, out doseLevel2CropMargin))
                    {
                      //parsing successful 
                    }
                    else
                    {
                      //parsing failed. 
                      doseLevel2CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                      //MessageBox.Show("Oops, please enter a valid Crop Margin for your opti structures.");
                      MessageBox.Show(string.Format("Oops, an invalid value was used for the Dose Level 1 crop margin ({0}). The DEFAULT of {1}mm will be used.", DoseLevel2CropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
                    }
                  }
                  // 3
                  if (DoseLevel3CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(DoseLevel3CropMargin_TextBox.Text))
                  {
                    doseLevel3CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                  }
                  else
                  {
                    if (int.TryParse(DoseLevel3CropMargin_TextBox.Text, out doseLevel3CropMargin))
                    {
                      //parsing successful 
                    }
                    else
                    {
                      //parsing failed. 
                      doseLevel3CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                      //MessageBox.Show("Oops, please enter a valid Crop Margin for your opti structures.");
                      MessageBox.Show(string.Format("Oops, an invalid value was used for the Dose Level 1 crop margin ({0}). The DEFAULT of {1}mm will be used.", DoseLevel3CropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
                    }
                  }
                }

                else if (hasFourDoseLevels)
                {
                  // set opti crop margins for the dose levels
                  
                  // 1
                  if (DoseLevel1CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(DoseLevel1CropMargin_TextBox.Text))
                  {
                    doseLevel1CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                  }
                  else
                  {
                    if (int.TryParse(DoseLevel1CropMargin_TextBox.Text, out doseLevel1CropMargin))
                    {
                      //parsing successful 
                    }
                    else
                    {
                      //parsing failed. 
                      doseLevel1CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                      //MessageBox.Show("Oops, please enter a valid Crop Margin for your opti structures.");
                      MessageBox.Show(string.Format("Oops, an invalid value was used for the Dose Level 1 crop margin ({0}). The DEFAULT of {1}mm will be used.", DoseLevel1CropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
                    }
                  }
                  // 2
                  if (DoseLevel2CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(DoseLevel2CropMargin_TextBox.Text))
                  {
                    doseLevel2CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                  }
                  else
                  {
                    if (int.TryParse(DoseLevel2CropMargin_TextBox.Text, out doseLevel2CropMargin))
                    {
                      //parsing successful 
                    }
                    else
                    {
                      //parsing failed. 
                      doseLevel2CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                      //MessageBox.Show("Oops, please enter a valid Crop Margin for your opti structures.");
                      MessageBox.Show(string.Format("Oops, an invalid value was used for the Dose Level 1 crop margin ({0}). The DEFAULT of {1}mm will be used.", DoseLevel2CropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
                    }
                  }
                  // 3
                  if (DoseLevel3CropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(DoseLevel3CropMargin_TextBox.Text))
                  {
                    doseLevel3CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                  }
                  else
                  {
                    if (int.TryParse(DoseLevel3CropMargin_TextBox.Text, out doseLevel3CropMargin))
                    {
                      //parsing successful 
                    }
                    else
                    {
                      //parsing failed. 
                      doseLevel3CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                      //MessageBox.Show("Oops, please enter a valid Crop Margin for your opti structures.");
                      MessageBox.Show(string.Format("Oops, an invalid value was used for the Dose Level 1 crop margin ({0}). The DEFAULT of {1}mm will be used.", DoseLevel3CropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
                    }
                  }
                }

              }
              catch
              {
                MESSAGES += string.Format("\r\n\t- ***Trouble Parsing Dose Level Crop Margins -- Default Values of {0} mm used***", DEFAULT_OPTI_CROP_MARGIN);
                doseLevel1CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                doseLevel2CropMargin = DEFAULT_OPTI_CROP_MARGIN;
                doseLevel3CropMargin = DEFAULT_OPTI_CROP_MARGIN;
              }
              try
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
                        opti.SegmentVolume = Helpers.CropOpti(opti, opti2, doseLevel1CropMargin);
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
                        opti.SegmentVolume = Helpers.CropOpti(opti, opti2, doseLevel1CropMargin);
                        opti.SegmentVolume = Helpers.CropOpti(opti, opti3, doseLevel1CropMargin);
                      }
                      // crop opti 2 from opti 3
                      if (opti.Id == opti2.Id)
                      {
                        opti.SegmentVolume = Helpers.CropOpti(opti, opti3, doseLevel2CropMargin);
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
                        opti.SegmentVolume = Helpers.CropOpti(opti, opti2, doseLevel1CropMargin);
                        opti.SegmentVolume = Helpers.CropOpti(opti, opti3, doseLevel1CropMargin);
                        opti.SegmentVolume = Helpers.CropOpti(opti, opti4, doseLevel1CropMargin);
                      }
                      // crop opti 2 from optis 3 and 4
                      if (opti.Id == opti2.Id)
                      {
                        opti.SegmentVolume = Helpers.CropOpti(opti, opti3, doseLevel2CropMargin);
                        opti.SegmentVolume = Helpers.CropOpti(opti, opti4, doseLevel2CropMargin);
                      }
                      // crop opti 3 from opti 4
                      if (opti.Id == opti3.Id)
                      {
                        opti.SegmentVolume = Helpers.CropOpti(opti, opti4, doseLevel3CropMargin);
                      }
                    }
                  }

                  else
                  {
                    MessageBox.Show("Oops, something went wrong while Cropping Opti PTVs");
                  }
                }
              }
              catch
              {
                MESSAGES += string.Format("\r\n\t- ***Trouble Cropping Multiple Dose Level Opti Structures***");
              }

              }

          }

          #endregion opti structures

          #region ring structures

          if (CreateRings_CB.IsChecked == true)
          {

            // TODO: add ring structure logic
            var ringGrowMargin = DEFAULT_RING_GROW_MARGIN;
            var ringCropMargin = DEFAULT_RING_CROP_MARGIN;
            var ringCount = DEFAULT_RING_COUNT;
            var ringPrefix = DEFAULT_RING_PREFIX;
            List<Structure> ringStructures = new List<Structure>();

            if (RingPrefix_TextBox.Text == "" || string.IsNullOrWhiteSpace(RingPrefix_TextBox.Text)) { ringPrefix = DEFAULT_RING_PREFIX; }
            else { ringPrefix = RingPrefix_TextBox.Text; }

            // set number of rings to be created
            if (RingCount_TextBox.Text == "" || string.IsNullOrWhiteSpace(RingCount_TextBox.Text))
            {
              ringCount = DEFAULT_RING_COUNT;
            }
            else
            {
              if (int.TryParse(RingCount_TextBox.Text, out ringCount))
              {
                //parsing successful 
              }
              else
              {
                //parsing failed. 
                ringCount = DEFAULT_RING_COUNT;
                //MessageBox.Show("Oops, please enter a valid Crop Margin for your opti structures.");
                MessageBox.Show(string.Format("Oops, an invalid value was used for the ring count ({0}). The DEFAULT of {1} will be used.", RingCount_TextBox.Text, DEFAULT_RING_COUNT));
              }
            }

            // set ring grow margin
            if (RingGrowMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(RingGrowMargin_TextBox.Text))
            {
              ringGrowMargin = DEFAULT_RING_GROW_MARGIN;
            }
            else
            {
              if (int.TryParse(RingGrowMargin_TextBox.Text, out ringGrowMargin))
              {
                //parsing successful 
              }
              else
              {
                //parsing failed. 
                ringGrowMargin = DEFAULT_RING_GROW_MARGIN;
                //MessageBox.Show("Oops, please enter a valid Crop Margin for your opti structures.");
                MessageBox.Show(string.Format("Oops, an invalid value was used for the ring count ({0}). The DEFAULT of {1} will be used.", RingGrowMargin_TextBox.Text, DEFAULT_RING_GROW_MARGIN));
              }
            }
            foreach (var ptv in PTVListForRings_LV.SelectedItems)
            {
              var target = ss.Structures.Single(st => st.Id == ptv.ToString());
              for (var i = 0; i < ringCount; i++)
              {
                var ringNum = i + 1;
                var ringId = string.Format("{0} {1} {2}", ringPrefix, Helpers.ProcessStructureId(target.Id.ToString(), MAX_ID_LENGTH - (ringPrefix.Length + 1 + ringNum.ToString().Length)), ringNum);

                // remove ring structure if present in ss
                Helpers.RemoveStructure(ss, ringId);

                // add empty ring structure
                var ringStructure = ss.AddStructure(RING_DICOM_TYPE, ringId);

                // copy target with defined margin
                ringStructure.SegmentVolume = Helpers.AddMargin(target, (double)ringGrowMargin * ringNum);
                MESSAGES += string.Format("\r\n\t- {0} added with {1} mm margin to {2}", ringStructure.Id, ringGrowMargin * ringNum, target.Id);

                // crop ring from target (prevents having to loop through again later)
                ringStructure.SegmentVolume = Helpers.CropStructure(ringStructure, target, cropMargin: ringCropMargin);
                MESSAGES += string.Format("\r\n\t- {0} Cropped from INSIDE {1} with {2} mm margin", ringStructure.Id, target.Id, ringCropMargin);

                // crop ring outside body
                try
                {
                  ringStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(ringStructure, body, cropMargin: 0);
                  MESSAGES += string.Format("\r\n\t- {0} Cropped OUTSIDE {1} with {2} mm margin", ringStructure.Id, body.Id, 0);
                }
                catch
                {
                  try
                  {
                    ringStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(ringStructure, bodyHR, cropMargin: 0);
                    MESSAGES += string.Format("\r\n\t- {0} Cropped OUTSIDE {1} with {2} mm margin", ringStructure.Id, bodyHR.Id, 0);
                  }
                  catch
                  {
                    MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From Body Surface***", ringStructure.Id);
                  }
                }


                ringStructures.Add(ringStructure);

              }


              for (var i = ringCount; i > 1; i--)
              {
                var currentRingId = string.Format("{0} {1} {2}", ringPrefix, Helpers.ProcessStructureId(target.Id.ToString(), MAX_ID_LENGTH - (ringPrefix.Length + 1 + i.ToString().Length)), i);
                var nextLargestRingId = string.Format("{0} {1} {2}", ringPrefix, Helpers.ProcessStructureId(target.Id.ToString(), MAX_ID_LENGTH - (ringPrefix.Length + 1 + (i-1).ToString().Length)), i - 1);
                try
                {
                  var currentRing = ss.Structures.Single(st => st.Id == currentRingId);
                  var nextLargestRing = ss.Structures.Single(st => st.Id == nextLargestRingId);

                  currentRing.SegmentVolume = Helpers.CropStructure(currentRing, nextLargestRing, ringCropMargin);
                  MESSAGES += string.Format("\r\n\t- {0} Cropped from INSIDE {1} with {2} mm margin", currentRingId, nextLargestRingId, ringCropMargin);
                }
                catch
                {
                  MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From {1}***", currentRingId, nextLargestRingId);
                }
              }
            }
          }

          #endregion ring structures

          #region clean up structure set

          // remove temporary high res structures
          Helpers.RemoveStructure(ss, bodyHRId);
          Helpers.RemoveStructure(ss, zoptiTotalHRId);
          Helpers.RemoveStructure(ss, "zzzTEMP"); // keep!!! used when booleaning structures -- Helpers.BooleanStructures()
          foreach (var t in sorted_ptvList)
          {
            try
            {
              Helpers.RemoveStructure(ss, string.Format("zz{0}_HR", Helpers.ProcessStructureId(t.Id.ToString(), MAX_ID_LENGTH - 5)));
            }
            catch
            {
            }

          }

          #endregion clean up structure set

          MESSAGES += "\r\n\r\n\tNOTE: *** Denotes an issue occured during the task's process\r\n\r\n";

          MessageBox.Show(MESSAGES, "General Steps Completed");
        }
      }
    }

    #region avoid structure option events

    // event fired when avoid option selected/unselected - boolean all targets || create avoids for multiple targets
    private void HandleAvoidOptionsSelection(object sender, RoutedEventArgs e)
    {
      var cb = sender as CheckBox;

      var booleanAll = "BooleanAllTargets_CB";
      var multipleAvoidTargets = "MultipleAvoidTargets_CB";

      if (cb.IsChecked == true)
      {
        if (cb.Name == booleanAll)
        {
          MultipleAvoidTargets_CB.IsChecked = false;
          MultipleAvoidTargets_SP.Visibility = Visibility.Collapsed;
        }
        if (cb.Name == multipleAvoidTargets)
        {
          BooleanAllTargets_CB.IsChecked = false;
          MultipleAvoidTargets_SP.Visibility = Visibility.Visible;

        }
      }
      if (cb.IsChecked == false)
      {
        if (cb.Name == booleanAll)
        {
          MultipleAvoidTargets_CB.IsChecked = true;
          MultipleAvoidTargets_SP.Visibility = Visibility.Visible;
        }
        if (cb.Name == multipleAvoidTargets)
        {
          BooleanAllTargets_CB.IsChecked = true;
          MultipleAvoidTargets_SP.Visibility = Visibility.Collapsed;
        }
      }

    }

    // event fired when different number of dose levels is defined
    private void HandleAvoidTargetCount(object sender, RoutedEventArgs e)
    {
      var radio1 = AvoidTarget1_Radio;
      var radio2 = AvoidTarget2_Radio;
      var radio3 = AvoidTarget3_Radio;
      var radio4 = AvoidTarget4_Radio;

      if (radio1.IsChecked == true)
      {
        AvoidTarget1_SP.Visibility = Visibility.Visible;
        AvoidTarget2_SP.Visibility = Visibility.Collapsed;
        AvoidTarget3_SP.Visibility = Visibility.Collapsed;
        AvoidTarget4_SP.Visibility = Visibility.Collapsed;
      }
      else if (radio2.IsChecked == true)
      {
        AvoidTarget1_SP.Visibility = Visibility.Visible;
        AvoidTarget2_SP.Visibility = Visibility.Visible;
        AvoidTarget3_SP.Visibility = Visibility.Collapsed;
        AvoidTarget4_SP.Visibility = Visibility.Collapsed;
      }
      else if (radio3.IsChecked == true)
      {
        AvoidTarget1_SP.Visibility = Visibility.Visible;
        AvoidTarget2_SP.Visibility = Visibility.Visible;
        AvoidTarget3_SP.Visibility = Visibility.Visible;
        AvoidTarget4_SP.Visibility = Visibility.Collapsed;
      }
      else if (radio4.IsChecked == true)
      {
        AvoidTarget1_SP.Visibility = Visibility.Visible;
        AvoidTarget2_SP.Visibility = Visibility.Visible;
        AvoidTarget3_SP.Visibility = Visibility.Visible;
        AvoidTarget4_SP.Visibility = Visibility.Visible;
      }
    }


    #endregion avoid structure option events

    #region opti structure section events

    // event fired when opti option selected/unselected - crop from body option || multiple dose levels option
    private void HandleOptiOptionsSelection(object sender, RoutedEventArgs e)
    {
      // if any opti structure options checked : show the options section
      if (CropFromBody_CB.IsChecked == true || MultipleDoseLevels_CB.IsChecked == true || CreateCI_CB.IsChecked == true || CreateR50_CB.IsChecked == true)
      {
        if (OptiOptions_SP.Visibility == Visibility.Collapsed) { OptiOptions_SP.Visibility = Visibility.Visible; }
      }

      // if crop or multi dose levels checked
      if (CropFromBody_CB.IsChecked == true || MultipleDoseLevels_CB.IsChecked == true)
      {

        // show section if checked
        CropFromBody_SP.Visibility = CropFromBody_CB.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        CropFromOptis_SP.Visibility = MultipleDoseLevels_CB.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        MultiDoseLevelOptions_SP.Visibility = MultipleDoseLevels_CB.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

        // show options if either is checked
        if (CropOptions_SP.Visibility == Visibility.Collapsed) { CropOptions_SP.Visibility = Visibility.Visible; }

      }

      // if crop and multi dose levels unchecked
      if (CropFromBody_CB.IsChecked == false && MultipleDoseLevels_CB.IsChecked == false)
      {
        if (CropOptions_SP.Visibility == Visibility.Visible) { CropOptions_SP.Visibility = Visibility.Collapsed; }

        // collapese sections
        CropFromBody_SP.Visibility = Visibility.Collapsed;
        CropFromOptis_SP.Visibility = Visibility.Collapsed;
        MultiDoseLevelOptions_SP.Visibility = Visibility.Collapsed;
      }

      // if create ci or r50 structure checked
      if (CreateCI_CB.IsChecked == true || CreateR50_CB.IsChecked == true)
      {
        // reveal/hide options first
        CI_R50_SP.Visibility = CreateCI_CB.IsChecked == true || CreateR50_CB.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        CIMargin_SP.Visibility = CreateCI_CB.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        R50Margin_SP.Visibility = CreateR50_CB.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

        // then show options if either is checked
        if (OptiOptions_SP.Visibility == Visibility.Collapsed) { OptiOptions_SP.Visibility = Visibility.Visible; }
      }

      // if none of the options are checked
      if (CropFromBody_CB.IsChecked == false && MultipleDoseLevels_CB.IsChecked == false && CreateCI_CB.IsChecked == false && CreateR50_CB.IsChecked == false)
      {
        // show options if either is checked
        OptiOptions_SP.Visibility = Visibility.Collapsed;

        // collapese sections
        CropFromBody_SP.Visibility = Visibility.Collapsed;
        CropFromOptis_SP.Visibility = Visibility.Collapsed;
        MultiDoseLevelOptions_SP.Visibility = Visibility.Collapsed;
        CI_R50_SP.Visibility = Visibility.Collapsed;
        CIMargin_SP.Visibility = Visibility.Collapsed;
        R50Margin_SP.Visibility = Visibility.Collapsed;
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

    #region highlight text on tab focus
    void SelectAllText(object sender, RoutedEventArgs e)
    {
      var textBox = e.OriginalSource as TextBox;
      if (textBox != null)
        textBox.SelectAll();
    }
    #endregion highlight text on tab focus

    #endregion helper methods
    //---------------------------------------------------------------------------------
  }
}
