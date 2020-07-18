using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    const int DEFAULT_RING_CROP_FROM_TARGET_MARGIN = 0;

    // BOOLEAN DEFAULTS
    const int DEFAULT_BOOLEAN_GROW_MARGIN = 0;


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
    public bool hasOneDoseLevel = false;
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
    public int booleanedStructureGrowMargin = DEFAULT_BOOLEAN_GROW_MARGIN;


    public bool createPTVEval = false;

    // lists for objects in the oar, ptv, and ring listboxes (for binding) -- necessary for using checkboxes in listboxes
    public ObservableCollection<StructureItem> oarListBoxItems = new ObservableCollection<StructureItem>();
    public ObservableCollection<StructureItem> ptvListBoxItems = new ObservableCollection<StructureItem>();
    public ObservableCollection<StructureItem> ringListBoxItems = new ObservableCollection<StructureItem>();
    public ObservableCollection<StructureItem> booleanListBoxItems = new ObservableCollection<StructureItem>();
    //public List<lboxItem> ringListBox = new List<lboxItem>();

    //public bool createOptiGTVForSingleLesion = false;
    //public bool createOptiTotal = false;

    #endregion public variables
    //---------------------------------------------------------------------------------
    #region objects used for binding

      // used for listviews to hold id and whether they are checked (listviews are holding checkboxes)
    public class StructureItem
    {
      public StructureItem(string id, bool isSelected=false)
      {
        Id = id;
        IsSelected = isSelected;
      }
      public string Id { get; set; }
      public bool IsSelected{ get; set; }

    }

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

    #region instructions button

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

    #endregion instructions button

    #region avoid structure events

    // cb event
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

    // cb event
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

        DoseLevel1CropMargin_SP.Visibility = Visibility.Collapsed;
      }
      else if (radio2.IsChecked == true)
      {
        DoseLevel1_SP.Visibility = Visibility.Visible;
        DoseLevel2_SP.Visibility = Visibility.Visible;
        DoseLevel3_SP.Visibility = Visibility.Collapsed;
        DoseLevel4_SP.Visibility = Visibility.Collapsed;
        
        DoseLevel1CropMargin_SP.Visibility = Visibility.Visible;
        DoseLevel2CropMargin_SP.Visibility = Visibility.Collapsed;
      }
      else if (radio3.IsChecked == true)
      {
        DoseLevel1_SP.Visibility = Visibility.Visible;
        DoseLevel2_SP.Visibility = Visibility.Visible;
        DoseLevel3_SP.Visibility = Visibility.Visible;
        DoseLevel4_SP.Visibility = Visibility.Collapsed;

        DoseLevel1CropMargin_SP.Visibility = Visibility.Visible;
        DoseLevel2CropMargin_SP.Visibility = Visibility.Visible;
        DoseLevel3CropMargin_SP.Visibility = Visibility.Collapsed;
      }
      else if (radio4.IsChecked == true)
      {
        DoseLevel1_SP.Visibility = Visibility.Visible;
        DoseLevel2_SP.Visibility = Visibility.Visible;
        DoseLevel3_SP.Visibility = Visibility.Visible;
        DoseLevel4_SP.Visibility = Visibility.Visible;

        DoseLevel1CropMargin_SP.Visibility = Visibility.Visible;
        DoseLevel2CropMargin_SP.Visibility = Visibility.Visible;
        DoseLevel3CropMargin_SP.Visibility = Visibility.Visible;
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

    #region ring structure events

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

    #endregion ring structure events

    #region create structures button

    // for design testing
    public void CreateStructures_Btn_Click_DEV(object sender, RoutedEventArgs e)
    {
      // get selected structure items from oar, ptv, and ring lists
      var selectedOarStructureItems = from StructureItem item in OarList_LV.Items
                                      where item.IsSelected == true
                                      select item;
      var selectedPTVStructureItems = from StructureItem item in PTVList_LV.Items
                                      where item.IsSelected == true
                                      select item;
      var selectedRingStructureItems = from StructureItem item in PTVListForRings_LV.Items
                                       where item.IsSelected == true
                                       select item;

      var selectedOARs = from StructureItem item in selectedOarStructureItems.ToList()
                         select item.Id;
      var selectedPTVs = from StructureItem item in selectedPTVStructureItems.ToList()
                         select item.Id;
      var selectedPTVsForRings = from StructureItem item in selectedRingStructureItems.ToList()
                                 select item.Id;

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
        if (AvoidTarget2_Radio.IsChecked == true && (AvoidTarget1_Combo.SelectedIndex == AvoidTarget2_Combo.SelectedIndex)) { MessageBox.Show("The selected Avoid Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
        if (AvoidTarget3_Radio.IsChecked == true && ((AvoidTarget1_Combo.SelectedIndex == AvoidTarget2_Combo.SelectedIndex) || (AvoidTarget1_Combo.SelectedIndex == AvoidTarget3_Combo.SelectedIndex) || (AvoidTarget2_Combo.SelectedIndex == AvoidTarget3_Combo.SelectedIndex))) { MessageBox.Show("Some of the selected Avoid Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
        if (AvoidTarget4_Radio.IsChecked == true && ((AvoidTarget1_Combo.SelectedIndex == AvoidTarget2_Combo.SelectedIndex) || (AvoidTarget1_Combo.SelectedIndex == AvoidTarget3_Combo.SelectedIndex) || (AvoidTarget1_Combo.SelectedIndex == AvoidTarget4_Combo.SelectedIndex) || (AvoidTarget2_Combo.SelectedIndex == AvoidTarget3_Combo.SelectedIndex) || (AvoidTarget2_Combo.SelectedIndex == AvoidTarget4_Combo.SelectedIndex) || (AvoidTarget3_Combo.SelectedIndex == AvoidTarget4_Combo.SelectedIndex))) { MessageBox.Show("Some of the selected Avoid Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
      }
      if (CreateOptis_CB.IsChecked == true && MultipleDoseLevels_CB.IsChecked == true)
      {
        // if selected counts don't match
        if (DoseLevel1_Radio.IsChecked == true && selectedPTVs.ToList().Count != 1) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
        if (DoseLevel2_Radio.IsChecked == true && selectedPTVs.ToList().Count != 2) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
        if (DoseLevel3_Radio.IsChecked == true && selectedPTVs.ToList().Count != 3) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
        if (DoseLevel4_Radio.IsChecked == true && selectedPTVs.ToList().Count != 4) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
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
        if (DoseLevel2_Radio.IsChecked == true && (DoseLevel1_Combo.SelectedIndex == DoseLevel2_Combo.SelectedIndex)) { MessageBox.Show("The selected Dose Level Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
        if (DoseLevel3_Radio.IsChecked == true && ((DoseLevel1_Combo.SelectedIndex == DoseLevel2_Combo.SelectedIndex) || (DoseLevel1_Combo.SelectedIndex == DoseLevel3_Combo.SelectedIndex) || (DoseLevel2_Combo.SelectedIndex == DoseLevel3_Combo.SelectedIndex))) { MessageBox.Show("Some of the selected Dose Level Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); }
        if (DoseLevel4_Radio.IsChecked == true && ((DoseLevel1_Combo.SelectedIndex == DoseLevel2_Combo.SelectedIndex) || (DoseLevel1_Combo.SelectedIndex == DoseLevel3_Combo.SelectedIndex) || (DoseLevel1_Combo.SelectedIndex == DoseLevel4_Combo.SelectedIndex) || (DoseLevel2_Combo.SelectedIndex == DoseLevel3_Combo.SelectedIndex) || (DoseLevel2_Combo.SelectedIndex == DoseLevel4_Combo.SelectedIndex) || (DoseLevel3_Combo.SelectedIndex == DoseLevel4_Combo.SelectedIndex))) { MessageBox.Show("Some of the selected Dose Level Targets match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); }

      }
      if (CreateAvoids_CB.IsChecked == true && selectedOARs.ToList().Count == 0)
      {
        MessageBox.Show("Oops, it appears you'd like to create avoid structures but haven't selected any structures to create avoids for.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);

      }
      if (CreateOptis_CB.IsChecked == true && selectedPTVs.ToList().Count == 0)
      {
        MessageBox.Show("Oops, it appears you'd like to create opti ptv structures but haven't selected any targets to create optis for.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);

      }
      if (CreateRings_CB.IsChecked == true && selectedPTVsForRings.ToList().Count == 0)
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

      string msg = "Selected Items:\r\n\r\n\t";
      foreach (var item in selectedOarStructureItems.ToList())
      {
        //var i = item as StructureItem;
        if (item.IsSelected == true) { msg += item.Id + "\r\n\t"; }

      }
      foreach (var item in selectedPTVStructureItems.ToList())
      {
        //var i = item as StructureItem;
        if (item.IsSelected == true) { msg += item.Id + "\r\n\t"; }

      }
      foreach (var item in selectedRingStructureItems.ToList())
      {
        //var i = item as StructureItem;
        if (item.IsSelected == true) { msg += item.Id + "\r\n\t"; }

      }
      MessageBox.Show(msg);
    }

    // for production
    public void CreateStructures_Btn_Click(object sender, RoutedEventArgs e)
    {
      //// for debug messages
      //var tempCounter = 1;

      //// will be changed to false if any validation logic fails
      var continueToCreateStructures = true;


      // get selected structures
      var selectedOarStructureItems = from StructureItem item in OarList_LV.Items
                                      where item.IsSelected == true
                                      select item;
      var selectedPTVStructureItems = from StructureItem item in PTVList_LV.Items
                                      where item.IsSelected == true
                                      select item;
      var selectedRingStructureItems = from StructureItem item in PTVListForRings_LV.Items
                                       where item.IsSelected == true
                                       select item;


      // get ids of selected structures
      var selectedOARs = from StructureItem item in selectedOarStructureItems.ToList()
                         select item.Id;
      var selectedPTVs = from StructureItem item in selectedPTVStructureItems.ToList()
                         select item.Id;
      var selectedPTVsForRings = from StructureItem item in selectedRingStructureItems.ToList()
                                 select item.Id;


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
        if (DoseLevel1_Radio.IsChecked == true && selectedPTVs.ToList().Count != 1) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
        if (DoseLevel2_Radio.IsChecked == true && selectedPTVs.ToList().Count != 2) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
        if (DoseLevel3_Radio.IsChecked == true && selectedPTVs.ToList().Count != 3) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
        if (DoseLevel4_Radio.IsChecked == true && selectedPTVs.ToList().Count != 4) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
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
      if (CreateAvoids_CB.IsChecked == true && selectedOARs.ToList().Count == 0)
      {
        MessageBox.Show("Oops, it appears you'd like to create avoid structures but haven't selected any structures to create avoids for.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        continueToCreateStructures = false;
      }
      if (CreateOptis_CB.IsChecked == true && selectedPTVs.ToList().Count == 0)
      {
        MessageBox.Show("Oops, it appears you'd like to create opti ptv structures but haven't selected any targets to create optis for.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        continueToCreateStructures = false;
      }
      if (CreateOptis_CB.IsChecked == true && CreatePTVEval_CB.IsChecked == true && selectedPTVs.ToList().Count > 0)
      {
        foreach (var id in selectedPTVs)
        {
          var t = ss.Structures.First(x => x.Id == id);
          var evalId = string.Format("{0}_Eval", Helpers.ProcessStructureId(t.Id.ToString(), MAX_ID_LENGTH - 5));
          var matchedEvalStructure = ss.Structures.Where(st => st.Id == evalId);
          if (matchedEvalStructure.ToList().Count > 0)
          {
            MessageBox.Show(string.Format("Oops, it appears {0} already exists.\r\n\t- Please rename or delete the existing Eval\r\n\tstructure to continue.", evalId), "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            continueToCreateStructures = false;
          }

        }
      }
      if (CreateRings_CB.IsChecked == true && selectedPTVsForRings.ToList().Count == 0)
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
          //if (needHRStructures) { MESSAGES += "\r\n\t- Some of the selected structures are High Res Structures so\r\n\t\ta High Res Body and High Res Opti Total will be created"; }

          #region variables

          // progress counter in case user clicks Create Structures Button more than once during same instance of script
          counter += 1;

          // for high res structures
          // create high res body for cropping
          Structure bodyHR = null;
          string bodyHRId = "zzBODY_HR";

          // create zopti total High Res for booleans/cropping
          Structure zptvTotalHR = null;
          Structure zoptiTotalHR = null;
          string zptvTotalHRId = "zptv total_HR";
          string zoptiTotalHRId = "zopti total_HR";

          // create zptv/opti total if ptv(s) present
          Structure zptvTotal = null;
          Structure zoptiTotal = null;
          string zptvTotalId = "zptv total";
          string zoptiTotalId = "zopti total";

          var targetsToCreateAvoidsFor = new List<string>();

          // for when optis are made
          int zoptiGrowMargin = 0;

          #endregion variables

          // allow modifications
          patient.BeginModifications();

          #region find body

          // find body
          Structure body = null;
          try
          {
            body = Helpers.GetBody(ss);
            //MessageBox.Show(string.Format("{0}", body.IsHighResolution));
            if (body.HasSegment && body.IsEmpty)
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

          if (needHRStructures)
          {
            // remove if already there
            Helpers.RemoveStructure(ss, bodyHRId);
            Helpers.RemoveStructure(ss, zptvTotalHRId);
            Helpers.RemoveStructure(ss, zoptiTotalHRId);

            // add empty structures
            bodyHR = ss.AddStructure(CONTROL_DICOM_TYPE, bodyHRId);

            // copy body to bodyHR
            bodyHR.SegmentVolume = body.SegmentVolume;

            // convert to high res
            if (bodyHR.CanConvertToHighResolution() == true) { bodyHR.ConvertToHighResolution(); /*MESSAGES += "\r\n\t- High Res Body Created";*/ }

            // hr targets not needed for ring structures
            if (CreateAvoids_CB.IsChecked == true || CreateOptis_CB.IsChecked == true)
            {
              // add empty structures
              zptvTotalHR = ss.AddStructure(OPTI_DICOM_TYPE, zptvTotalHRId);
              zoptiTotalHR = ss.AddStructure(OPTI_DICOM_TYPE, zoptiTotalHRId);

              // convert to high res
              if (zptvTotalHR.CanConvertToHighResolution() == true) { zptvTotalHR.ConvertToHighResolution(); /*MESSAGES += "\r\n\t- High Res Target Created";*/ }
              if (zoptiTotalHR.CanConvertToHighResolution() == true) { zoptiTotalHR.ConvertToHighResolution(); /*MESSAGES += "\r\n\t- High Res Target Created";*/ }

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

                  if (t.IsHighResolution != true)
                  {
                    hrTarget.SegmentVolume = t.SegmentVolume;
                    if (hrTarget.CanConvertToHighResolution()) { hrTarget.ConvertToHighResolution(); }
                  }
                  else
                  {
                    if (hrTarget.CanConvertToHighResolution()) { hrTarget.ConvertToHighResolution(); }
                    hrTarget.SegmentVolume = t.SegmentVolume;
                  }


                  zptvTotalHR.SegmentVolume = zptvTotalHR.Or(hrTarget.SegmentVolume);
                }
                zptvTotalHR.SegmentVolume = Helpers.CropOutsideBodyWithMargin(zptvTotalHR, bodyHR, cropFromBodyMargin);
                zoptiTotalHR.SegmentVolume = zptvTotalHR.SegmentVolume;
              }
            }

          }

          #endregion get high res structures

          #region create zopti total


          // opti total shouldn't be needed for ring structures
          if (CreateAvoids_CB.IsChecked == true || CreateOptis_CB.IsChecked == true)
          {
            if (hasSinglePTV || hasMultiplePTVs)
            {
              // remove if already there
              Helpers.RemoveStructure(ss, zptvTotalId);

              // add empty zopti total structure
              zptvTotal = ss.AddStructure(OPTI_DICOM_TYPE, zptvTotalId);
              MESSAGES += string.Format("\r\n\t- Structure Added: {0}", zptvTotalId);
              try
              {
                if (needHRStructures)
                {
                  if (zptvTotal.CanConvertToHighResolution()) { zptvTotal.ConvertToHighResolution(); }
                  zptvTotal.SegmentVolume = zptvTotalHR.SegmentVolume;
                }
                else
                {
                  // boolean ptvs into zopti total
                  zptvTotal.SegmentVolume = sorted_ptvList.Count() > 1 ? Helpers.BooleanStructures(ss, sorted_ptvList) : sorted_ptvList.First().SegmentVolume;
                }
                MESSAGES += string.Format("\r\n\t- Structure Booleaned: {0}", zptvTotalId);
              }
              catch
              {
                MESSAGES += string.Format("\r\n\t- ***Trouble creating {0}***", zptvTotalId);
              }

              // if user is creating opti ptvs, want to crop avoids from the opti ptv total (ptv total w/ margin specified by user)
              if (CreateOptis_CB.IsChecked == true)
              {
                // set grow margin for otpti ptvs
                if (int.TryParse(OptiGrowMargin_TextBox.Text, out zoptiGrowMargin))
                {
                  //parsing successful 
                }
                else
                {
                  //parsing failed. 
                  zoptiGrowMargin = DEFAULT_OPTI_GROW_MARGIN;
                  MessageBox.Show(string.Format("Oops, an invalid value was used for the opti structure grow margin ({0}). The DEFAULT of {1} mm will be used.", OptiGrowMargin_TextBox.Text, DEFAULT_OPTI_GROW_MARGIN));
                }

                // remove if already there
                Helpers.RemoveStructure(ss, zoptiTotalId);

                // add empty zopti total structure
                zoptiTotal = ss.AddStructure(OPTI_DICOM_TYPE, zoptiTotalId);
                MESSAGES += string.Format("\r\n\t- Structure Added: {0}", zoptiTotalId);

                // zopti total
                zoptiTotal.SegmentVolume = zptvTotal.SegmentVolume.Margin(zoptiGrowMargin);
                MESSAGES += string.Format("\r\n\t- {0} mm margin added to {1}", zoptiGrowMargin, zoptiTotalId);

                if (needHRStructures)
                {
                  // zopti total HR
                  zoptiTotalHR.SegmentVolume = zptvTotalHR.SegmentVolume.Margin(zoptiGrowMargin);
                  MESSAGES += string.Format("\r\n\t- {0} mm margin added to {1}", zoptiGrowMargin, zoptiTotalId);
                }
              }

              // crop zptvtotal from body
              try
              {
                if (zptvTotal.IsHighResolution)
                {
                  zptvTotal.SegmentVolume = Helpers.CropOutsideBodyWithMargin(zptvTotal, bodyHR, -DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                  //MESSAGES += string.Format("\r\n\t- {0} Cropped {1} mm From High Res Body Surface", zptvTotal.Id, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                }
                else
                {
                  zptvTotal.SegmentVolume = Helpers.CropOutsideBodyWithMargin(zptvTotal, body, -DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                  //MESSAGES += string.Format("\r\n\t- {0} Cropped {1} mm From Body Surface", zptvTotal.Id, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                }
              }
              catch
              {
                MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From Body Surface***", zptvTotal.Id);
              }

              // crop zopti total from body
              if (CreateOptis_CB.IsChecked == true)
              {
                try
                {
                  if (zoptiTotal.IsHighResolution)
                  {
                    zoptiTotal.SegmentVolume = Helpers.CropOutsideBodyWithMargin(zoptiTotal, bodyHR, -DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                    //MESSAGES += string.Format("\r\n\t- {0} Cropped {1} mm From High Res Body Surface", zoptiTotal.Id, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                  }
                  else
                  {
                    zoptiTotal.SegmentVolume = Helpers.CropOutsideBodyWithMargin(zoptiTotal, body, -DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                    //MESSAGES += string.Format("\r\n\t- {0} Cropped {1} mm From Body Surface", zoptiTotal.Id, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                  }
                }
                catch
                {
                  MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From Body Surface***", zoptiTotal.Id);
                }

                // crop zopti total HR from body
                if (needHRStructures)
                {
                  try
                  {
                    zoptiTotalHR.SegmentVolume = Helpers.CropOutsideBodyWithMargin(zoptiTotalHR, bodyHR, -DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                    //MESSAGES += string.Format("\r\n\t- {0} Cropped {1} mm From High Res Body Surface", zoptiTotal.Id, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                  }
                  catch
                  {
                    MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From Body Surface***", zoptiTotalHR.Id);
                  }
                }
              }

            }
          }
          #endregion create zopti total

          #region avoidance structures

          if (CreateAvoids_CB.IsChecked == true)
          {

            //var avStructuresToMake = OarList_LV.SelectedItems;
            var avStructuresToMake = selectedOARs;
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
                MessageBox.Show(string.Format("Oops, an invalid value was used for the avoidance structure grow margin ({0}). The DEFAULT of {1} mm will be used.", AvoidGrowMargin_TextBox.Text, DEFAULT_AVOIDANCE_GROW_MARGIN));
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
                  MessageBox.Show(string.Format("Oops, an invalid value was used for the avoidance structure crop margin ({0}). The DEFAULT of {1} mm will be used.", AvoidCropMargin_TextBox.Text, DEFAULT_AVOIDANCE_CROP_MARGIN));
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
                  MESSAGES += string.Format("\r\n\t- {0} added w/ {1} mm margin",
                                                          avoidStructure.Id,
                                                          avGrowMargin
                  );

                  try
                  {
                    // if user is creating optis, want to crop avoids from the zopti total that has the user specified margin on the ptv(s)
                    if (CreateOptis_CB.IsChecked == true)
                    {
                      // crop avoid structure from opti total (if ptvs are found)
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

                        MESSAGES += string.Format(" & cropped {0} mm from {1}",
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

                          MESSAGES += string.Format(" & cropped {0} mm from {1}",
                                                          avCropMargin,
                                                          zoptiTotal.Id
                          );
                        }
                        catch
                        {
                          if (sorted_ptvList.Count() >= 1)
                          {
                            MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From Target(s)***", avoidStructure.Id);
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
                    else
                    {
                      // crop avoid structure from ptv total (if ptvs are found)
                      try
                      {
                        if (avoidStructure.IsHighResolution)
                        {
                          avoidStructure.SegmentVolume = Helpers.CropStructure(avoidStructure.SegmentVolume, zptvTotalHR.SegmentVolume, avCropMargin);
                        }
                        else
                        {
                          avoidStructure.SegmentVolume = Helpers.CropStructure(avoidStructure.SegmentVolume, zptvTotal.SegmentVolume, avCropMargin);
                        }

                        MESSAGES += string.Format(" & cropped {0} mm from {1}",
                                                          avCropMargin,
                                                          zptvTotal.Id
                        );
                      }
                      catch
                      {
                        try
                        {
                          if (avoidStructure.CanConvertToHighResolution()) { avoidStructure.ConvertToHighResolution(); }

                          avoidStructure.SegmentVolume = Helpers.CropStructure(avoidStructure.SegmentVolume, zptvTotalHR.SegmentVolume, avCropMargin);

                          MESSAGES += string.Format(" & cropped {0} mm from {1}",
                                                          avCropMargin,
                                                          zptvTotal.Id
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
                    avId = string.Format("{0} {1}-{2}",
                                            avPrefix,
                                            targetNumber,
                                            Helpers.ProcessStructureId(oar.Id.ToString(), MAX_ID_LENGTH - (avPrefix.Length + 2)) // +# to account for strongly typed characters/spaces beyond those already accounted for
                    );

                    var avoidTargetHRId = string.Format("zzz{0}_HR", Helpers.ProcessStructureId(t, MAX_ID_LENGTH - 6));
                    var avoidTargetId = string.Format("zzz{0}_Id", Helpers.ProcessStructureId(t, MAX_ID_LENGTH - 6)); // still minus 5 (same as hr structure) to allow only one id to be defined later when removing these two structures

                    Helpers.RemoveStructure(ss, avoidTargetId);

                    // match ptv in structure set
                    avoidTarget = ss.AddStructure(OPTI_DICOM_TYPE, avoidTargetId);
                    if (needHRStructures)
                    {
                      Helpers.RemoveStructure(ss, avoidTargetHRId);
                      avoidTarget_HR = ss.AddStructure(OPTI_DICOM_TYPE, avoidTargetHRId);
                      avoidTarget_HR.ConvertToHighResolution();
                    }

                    // if creating optis, need to add margin to the temp avoid target for when cropping avoid away
                    if (CreateOptis_CB.IsChecked == true)
                    {
                      avoidTarget.SegmentVolume = ss.Structures.Single(st => st.Id == t).SegmentVolume.Margin(zoptiGrowMargin);
                      if (needHRStructures)
                      {
                        avoidTarget_HR.SegmentVolume = avoidTarget_HR.SegmentVolume.Margin(zoptiGrowMargin);
                      }
                    }
                    else
                    {
                      avoidTarget.SegmentVolume = ss.Structures.Single(st => st.Id == t).SegmentVolume;
                      //avoidTarget_HR.SegmentVolume = avoidTarget_HR.SegmentVolume.Margin(zoptiGrowMargin);
                    }


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
                    MESSAGES += string.Format("\r\n\t- {0} added w/ {1} mm margin",
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

                        MESSAGES += string.Format(" & cropped {0} mm from {1}",
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

                          MESSAGES += string.Format(" & cropped {0} mm from {1}",
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
                MESSAGES += string.Format("\r\n\t- {0} added w/ {1} mm margin",
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


            //var optiStructuresToMake = PTVList_LV.SelectedItems;
            var optiStructuresToMake = selectedPTVs;
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
                  MessageBox.Show(string.Format("Oops, an invalid value was used for the opti crop from body margin ({0}). The DEFAULT of {1} mm will be used.", BodyCropMargin_TextBox.Text, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN));
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
              //MessageBox.Show(string.Format("Oops, an invalid value was used for the opti structure grow margin ({0}). The DEFAULT of {1} mm will be used.", OptiGrowMargin_TextBox.Text, DEFAULT_OPTI_GROW_MARGIN));
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
                  MessageBox.Show(string.Format("Oops, an invalid value was used for the opti structure crop margin ({0}). The DEFAULT of {1} mm will be used.", OptiCropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
                }
              }

              if (DoseLevel1_Radio.IsChecked == true)
              {
                doseLevel1Target = Helpers.GetStructure(ss, DoseLevel1_Combo.SelectedItem.ToString());
                hasOneDoseLevel = true;
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
              //var zztemp = 0;

              if (needHRStructures && ptv.CanConvertToHighResolution())
              {
                ptv.ConvertToHighResolution();
              }



              // remove structure if present in ss
              Helpers.RemoveStructure(ss, optiId);

              // add empty opti structure
              var optiStructure = ss.AddStructure(OPTI_DICOM_TYPE, optiId);

              if (needHRStructures && optiStructure.CanConvertToHighResolution())
              {
                optiStructure.ConvertToHighResolution();
              }

              // copy ptv with defined margin
              optiStructure.SegmentVolume = Helpers.AddMargin(ptv, (double)optiGrowMargin);
              MESSAGES += string.Format("\r\n\t- {0} added w/ {1} mm margin", optiStructure.Id, optiGrowMargin);




              // eval ptv
              if (createPTVEval)
              {
                Helpers.RemoveStructure(ss, evalId);
                evalStructure = ss.AddStructure(OPTI_DICOM_TYPE, evalId);

                if (needHRStructures && evalStructure.CanConvertToHighResolution())
                {
                  evalStructure.ConvertToHighResolution();
                }
              }






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


              //zztemp += 1;
              // PTV Eval structure
              if (evalStructure != null)
              {

                // copy ptv to eval ptv
                evalStructure.SegmentVolume = ptv.SegmentVolume;
                MESSAGES += string.Format("\r\n\t- {0} added", evalStructure.Id);
                //zztemp += 1;

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
              //zztemp += 1;

              // create CI structure
              if (createCI)
              {
                try
                {
                  // ci structure id
                  var ciId = string.Format("{0}_{1}", DEFAULT_CI_PREFIX, Helpers.ProcessStructureId(ptv.Id.ToString(), MAX_ID_LENGTH - DEFAULT_CI_PREFIX.Length));

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
                    MessageBox.Show(string.Format("Oops, an invalid value was used for the CI structure grow margin ({0}). The DEFAULT of {1} mm will be used.", CIMargin_TextBox.Text, DEFAULT_CI_GROW_MARGIN));
                  }

                  // remove structure if present in ss
                  Helpers.RemoveStructure(ss, ciId);

                  // add empty avoid structure
                  var ciStructure = ss.AddStructure(CONTROL_DICOM_TYPE, ciId);

                  if (ptv.IsHighResolution && ciStructure.CanConvertToHighResolution())
                  {
                    ciStructure.ConvertToHighResolution();
                  }

                  // copy ptv with defined margin
                  ciStructure.SegmentVolume = Helpers.AddMargin(ptv, (double)ciGrowMargin);
                  MESSAGES += string.Format("\r\n\t- {0} added with {1} mm margin", ciStructure.Id, ciGrowMargin);
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
                  var r50Id = string.Format("{0}_{1}", DEFAULT_R50_PREFIX, Helpers.ProcessStructureId(ptv.Id.ToString(), MAX_ID_LENGTH - DEFAULT_R50_PREFIX.Length));

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
                    MessageBox.Show(string.Format("Oops, an invalid value was used for the CI structure grow margin ({0}). The DEFAULT of {1} mm will be used.", R50Margin_TextBox.Text, DEFAULT_R50_GROW_MARGIN));
                  }

                  // remove structure if present in ss
                  Helpers.RemoveStructure(ss, r50Id);

                  // add empty avoid structure
                  var r50Structure = ss.AddStructure(CONTROL_DICOM_TYPE, r50Id);

                  if (ptv.IsHighResolution && r50Structure.CanConvertToHighResolution())
                  {
                    r50Structure.ConvertToHighResolution();
                  }

                  // copy ptv with defined margin
                  r50Structure.SegmentVolume = Helpers.AddMargin(ptv, (double)r50GrowMargin);
                  MESSAGES += string.Format("\r\n\t- {0} added with {1} mm margin", r50Structure.Id, r50GrowMargin);
                }
                catch
                {
                  MESSAGES += string.Format("\r\n\t- ***Trouble Creating CI Structure***");
                }
              }

              if (MultipleDoseLevels_CB.IsChecked == true) // TODO: need to check to see if this area has issues when a target is already HR
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
                if (hasOneDoseLevel) { }
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
                      MessageBox.Show(string.Format("Oops, an invalid value was used for the Dose Level 1 crop margin ({0}). The DEFAULT of {1} mm will be used.", DoseLevel1CropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
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
                      MessageBox.Show(string.Format("Oops, an invalid value was used for the Dose Level 1 crop margin ({0}). The DEFAULT of {1} mm will be used.", DoseLevel2CropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
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
                      MessageBox.Show(string.Format("Oops, an invalid value was used for the Dose Level 1 crop margin ({0}). The DEFAULT of {1} mm will be used.", DoseLevel1CropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
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
                      MessageBox.Show(string.Format("Oops, an invalid value was used for the Dose Level 1 crop margin ({0}). The DEFAULT of {1} mm will be used.", DoseLevel2CropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
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
                      MessageBox.Show(string.Format("Oops, an invalid value was used for the Dose Level 1 crop margin ({0}). The DEFAULT of {1} mm will be used.", DoseLevel3CropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
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
                      MessageBox.Show(string.Format("Oops, an invalid value was used for the Dose Level 1 crop margin ({0}). The DEFAULT of {1} mm will be used.", DoseLevel1CropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
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
                      MessageBox.Show(string.Format("Oops, an invalid value was used for the Dose Level 1 crop margin ({0}). The DEFAULT of {1} mm will be used.", DoseLevel2CropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
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
                      MessageBox.Show(string.Format("Oops, an invalid value was used for the Dose Level 1 crop margin ({0}). The DEFAULT of {1} mm will be used.", DoseLevel3CropMargin_TextBox.Text, DEFAULT_OPTI_CROP_MARGIN));
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
                  try
                  {
                    if (hasOneDoseLevel) { }
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
                  }
                  catch
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
            var ringCropFromTargetMargin = DEFAULT_RING_CROP_FROM_TARGET_MARGIN;
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

            // set ring crop from target margin
            if (RingCropMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(RingCropMargin_TextBox.Text))
            {
              ringCropFromTargetMargin = DEFAULT_RING_CROP_FROM_TARGET_MARGIN;
            }
            else
            {
              if (int.TryParse(RingCropMargin_TextBox.Text, out ringCropFromTargetMargin))
              {
                //parsing successful 
              }
              else
              {
                //parsing failed. 
                ringCropFromTargetMargin = DEFAULT_RING_CROP_FROM_TARGET_MARGIN;
                //MessageBox.Show("Oops, please enter a valid Crop Margin for your opti structures.");
                MessageBox.Show(string.Format("Oops, an invalid value was used for the ring count ({0}). The DEFAULT of {1} will be used.", RingCropMargin_TextBox.Text, DEFAULT_RING_CROP_FROM_TARGET_MARGIN));
              }
            }

            // create rings
            foreach (var ptvId in selectedPTVsForRings)
            {

              var target = ss.Structures.Single(st => st.Id == ptvId);
              for (var i = 0; i < ringCount; i++)
              {
                var ringNum = i + 1;
                var ringId = string.Format("{0}_{1}-{2}", ringPrefix, Helpers.ProcessStructureId(target.Id.ToString(), MAX_ID_LENGTH - (ringPrefix.Length + 1 + ringNum.ToString().Length)), ringNum);

                // remove ring structure if present in ss
                Helpers.RemoveStructure(ss, ringId);

                // add empty ring structure
                var ringStructure = ss.AddStructure(RING_DICOM_TYPE, ringId);

                // copy target with defined margin
                ringStructure.SegmentVolume = Helpers.AddMargin(target, (double)ringGrowMargin * ringNum);
                //MESSAGES += string.Format("\r\n\t- {0} added with {1} mm margin to {2}", ringStructure.Id, ringGrowMargin * ringNum, target.Id);

                // crop ring from target (prevents having to loop through again later)
                ringStructure.SegmentVolume = Helpers.CropStructure(ringStructure, target, cropMargin: ringCropFromTargetMargin);
                //MESSAGES += string.Format("\r\n\t- {0} Cropped from INSIDE {1} with {2} mm margin", ringStructure.Id, target.Id, ringCropFromTargetMargin);

                // add to list for cropping later
                ringStructures.Add(ringStructure);
              }

              // crop rings from one another
              for (var i = ringCount; i > 1; i--)
              {
                var currentRingId = string.Format("{0}_{1}-{2}", ringPrefix, Helpers.ProcessStructureId(target.Id.ToString(), MAX_ID_LENGTH - (ringPrefix.Length + 1 + i.ToString().Length)), i);
                var nextLargestRingId = string.Format("{0}_{1}-{2}", ringPrefix, Helpers.ProcessStructureId(target.Id.ToString(), MAX_ID_LENGTH - (ringPrefix.Length + 1 + (i - 1).ToString().Length)), i - 1);
                try
                {
                  var currentRing = ss.Structures.Single(st => st.Id == currentRingId);
                  var nextLargestRing = ss.Structures.Single(st => st.Id == nextLargestRingId);

                  currentRing.SegmentVolume = Helpers.CropStructure(currentRing, nextLargestRing, ringCropMargin);
                  //MESSAGES += string.Format("\r\n\t- {0} Cropped from INSIDE {1} with {2} mm margin", currentRingId, nextLargestRingId, ringCropMargin);
                }
                catch
                {
                  MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From {1}***", currentRingId, nextLargestRingId);
                }
              }
            }

            // crop rings from body structure -- if opton checked
            if (CropRingsFromBody_CB.IsChecked == true)
            {
              foreach (var ring in ringStructures)
              {
                try
                {
                  if (ring.IsHighResolution) { ring.SegmentVolume = Helpers.CropOutsideBodyWithMargin(ring, bodyHR, 0); }
                  else { ring.SegmentVolume = Helpers.CropOutsideBodyWithMargin(ring, body, 0); }
                  MESSAGES += string.Format("\r\n\t- {0} Cropped OUTSIDE {1} with {2} mm margin", ring.Id, body.Id, 0);
                }
                catch
                {
                  MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From Body Surface***", ring.Id);
                }
              }
            }
          }

          #endregion ring structures

          #region clean up structure set

          // remove temporary high res structures
          var toRemove = new List<string> { bodyHRId, zptvTotalHRId, zoptiTotalHRId, "zzzTEMP", "zzzzTEMP" };
          foreach (var id in toRemove)
          {
            try
            {
              Helpers.RemoveStructure(ss, id);
            }
            catch
            {
              MessageBox.Show("Trouble removing " + id);
            }
          }

          if (targetsToCreateAvoidsFor.Count > 0)
          {
            foreach (var t in targetsToCreateAvoidsFor)
            {
              try
              {
                //if (ss.Structures.Where(x => x.Id.ToLower() == t.ToLower()).ToList().Count > 1)
                //{
                var tempId = Helpers.ProcessStructureId(t, MAX_ID_LENGTH - 5);
                try
                {
                  Helpers.RemoveStructure(ss, string.Format("zzz{0}_HR", tempId));
                }
                catch
                {
                }
                try
                {
                  Helpers.RemoveStructure(ss, string.Format("zzz{0}_Id", tempId));
                }
                catch
                {
                }
                //}
              }
              catch
              {

              }

            }
          }
          if (needHRStructures && (CreateOptis_CB.IsChecked == true || CreateAvoids_CB.IsChecked == true))
          {
            foreach (var t in sorted_ptvList)
            {
              try
              {
                //if (ss.Structures.Where(x => x.Id.ToLower() == t.ToLower()).ToList().Count > 1)
                //{
                var tempId = Helpers.ProcessStructureId(t.Id, MAX_ID_LENGTH - 5);
                try
                {
                  Helpers.RemoveStructure(ss, string.Format("zz{0}_HR", tempId));
                }
                catch
                {
                }
                //}
              }
              catch
              {

              }
            }
          }


          #endregion clean up structure set

          MESSAGES += "\r\n\r\n\tNOTE: *** Denotes an issue occured during the task's process\r\n\r\n";

          MessageBox.Show(MESSAGES, "General Steps Completed");
        }
      }
    }

    #endregion create structures button

    #region boolean structure button events

    // create boolean option click event
    private void CreateBoolean_CB_Click(object sender, RoutedEventArgs e)
    {
      // toggle boolean tool section
      if (CreateBoolean_CB.IsChecked == true) { BooleanedStructure_SP.Visibility = Visibility.Visible; }
      else { BooleanedStructure_SP.Visibility = Visibility.Collapsed; }
    }

    // bool operation add cb click event
    private void BoolOperationAdd_CB_Click(object sender, RoutedEventArgs e)
    {
      handleBoolOperationSelection(sender as CheckBox);
    }
    
    // bool operation subtract cb click event
    private void BoolOperationSubtract_CB_Click(object sender, RoutedEventArgs e)
    {
      handleBoolOperationSelection(sender as CheckBox);
    }
    
    // bool operation type event
    private void handleBoolOperationSelection(CheckBox cb)
    {
      var add = "BoolOperationAdd_CB";
      var sub = "BoolOperationSubtract_CB";

      if (cb.IsChecked == true)
      {
        if (cb.Name == add)
        {
          BoolOperationSubtract_CB.IsChecked = false;
        }
        if (cb.Name == sub)
        {
          BoolOperationAdd_CB.IsChecked = false;
        }
      }
      if (cb.IsChecked == false)
      {
        if (cb.Name == add)
        {
          BoolOperationSubtract_CB.IsChecked = true;
        }
        if (cb.Name == sub)
        {
          BoolOperationAdd_CB.IsChecked = true;
        }
      }
    }

    // create boolean structure with margin checkbox
    private void CreateBooleanWitMargin_CB_Click(object sender, RoutedEventArgs e)
    {
      if (BooleanedMarginOptions_SP.Visibility == Visibility.Collapsed)
        BooleanedMarginOptions_SP.Visibility = Visibility.Visible;
      else
        BooleanedMarginOptions_SP.Visibility = Visibility.Collapsed;
    }

    // boolean margin type cb event
    private void handleBoolMarginTypeSelection(CheckBox cb)
    {
      var outer = "OuterMargin_CB";
      var inner = "InnerMargin_CB";

      if (cb.IsChecked == true)
      {
        if (cb.Name == outer)
        {
          InnerMargin_CB.IsChecked = false;
        }
        if (cb.Name == inner)
        {
          OuterMargin_CB.IsChecked = false;
        }
      }
      if (cb.IsChecked == false)
      {
        if (cb.Name == outer)
        {
          InnerMargin_CB.IsChecked = true;
        }
        if (cb.Name == inner)
        {
          OuterMargin_CB.IsChecked = true;
        }
      }
    }

    // outer cb click event
    private void OuterMargin_CB_Click(object sender, RoutedEventArgs e)
    {
      handleBoolMarginTypeSelection(sender as CheckBox);
    }
    // inner cb click event
    private void InnerMargin_CB_Click(object sender, RoutedEventArgs e)
    {
      handleBoolMarginTypeSelection(sender as CheckBox);
    }

    // for production
    private void CreateBooleanedStructure_Btn_Click(object sender, RoutedEventArgs e)
    {
      bool okToContinue = true;
      // Define a regular expression for repeated words.
      Regex rx = new Regex(@"^[a-zA-Z0-9_-]+$",
                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

      var validBoolMargin = true;

      // set ring crop from target margin
      if (BooleanMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(BooleanMargin_TextBox.Text))
      {
        validBoolMargin = false;
      }
      else
      {
        if (int.TryParse(BooleanMargin_TextBox.Text, out booleanedStructureGrowMargin))
        {
          //parsing successful 
          if (InnerMargin_CB.IsChecked == true)
            booleanedStructureGrowMargin = -booleanedStructureGrowMargin;
        }
        else
        {
          //parsing failed. 
          validBoolMargin = false;
        }
      }

      // validation
      if (!rx.IsMatch(BoolStructId_TextBox.Text) || BoolStructId_TextBox.Text == "") { MessageBox.Show("Please enter a valid Structure Id \r\n\t- (hint: can only use letters, numbers, dashes, and underscores)"); okToContinue = false; return; }
      if (BoolBaseStruct_Combo.SelectedIndex == -1) { MessageBox.Show("Please select a base structure"); okToContinue = false; return; }
      if (CreateBooleanWitMargin_CB.IsChecked == true && validBoolMargin == false) { MessageBox.Show("Please enter a valid integer value for the booleaned structure margin"); okToContinue = false; return; }
      if (BoolBaseStruct_Combo.SelectedIndex >= 0)
      {
        // if the structure exists already
        if (ss.Structures.Where(x => x.Id.ToLower() == BoolStructId_TextBox.Text.ToLower()).ToList().Count > 0)
        {
          var result = MessageBox.Show("A structure exists with the same Id and the structure will be replaced.\r\n\r\n\tIs it OK to continue? ", "Some Title", MessageBoxButton.YesNo, MessageBoxImage.Warning);
          if (result == MessageBoxResult.Yes)
          {
            // ok to continue
          }
          else if (result == MessageBoxResult.No)
          {
            // not ok to continue
            okToContinue = false;
            return;
          }
        }
      }

      // if pass validation
      if (okToContinue == true)
      {
        using (new WaitCursor())
        {

          // allow modifications
          patient.BeginModifications();

          bool makeHR = false;
          List<Structure> structuresToAddOrSubtract = new List<Structure>();

          var selectedBooleanStructureItems = from StructureItem item in StructureListForBooleanOperations_LV.Items
                                              where item.IsSelected == true
                                              select item;

          var selectedBooleanStructures = from StructureItem item in selectedBooleanStructureItems.ToList()
                                          select item.Id;

          var newStructureId = Helpers.ProcessStructureId(BoolStructId_TextBox.Text, MAX_ID_LENGTH);
          var baseStructureId = BoolBaseStruct_Combo.SelectedItem.ToString();

          // get base structure
          var baseStructure = sorted_structureList.Single(x => x.Id == baseStructureId);

          // get base structure dicom type
          var baseStructureDicomType = baseStructure.DicomType == "EXTERNAL" || baseStructure.DicomType.Length == 0 ? CONTROL_DICOM_TYPE : baseStructure.DicomType.ToString();


          // get boolean operation type
          var boolOperationAdd = BoolOperationAdd_CB.IsChecked;
          //var boolOperationSubtract = BoolOperationSubtract_CB.IsChecked;

          // initiate new structure
          Structure newStructure = null;
          Structure booleanOfSelectedStructures = null;
          string booleanOfSelectedStructuresId = "tempBoolTot";

          // check and remove structure
          Helpers.RemoveStructure(ss, booleanOfSelectedStructuresId);

          // create new structure
          if (ss.Structures.Where(x => x.Id.ToLower() == newStructureId.ToLower()).ToList().Count > 0) { newStructure = ss.Structures.First(x => x.Id.ToLower() == newStructureId.ToLower()); }
          else { newStructure = ss.AddStructure(baseStructureDicomType, newStructureId); }
          booleanOfSelectedStructures = ss.AddStructure(baseStructureDicomType, booleanOfSelectedStructuresId);

          // check if needs to be High Res
          if (baseStructure.IsHighResolution == true) { makeHR = true; }

          foreach (var id in selectedBooleanStructures)
          {
            var s = ss.Structures.Single(x => x.Id == id);
            if (s.IsHighResolution == true) { makeHR = true; }

            structuresToAddOrSubtract.Add(s);
          }

          // if High Res
          if (makeHR == true)
          {
            // counter for temp hr structures
            var counter = 0;

            // convert base structure to High Res
            if (baseStructure.CanConvertToHighResolution()) { baseStructure.ConvertToHighResolution(); }
            if (newStructure.CanConvertToHighResolution()) { newStructure.ConvertToHighResolution(); }
            if (booleanOfSelectedStructures.CanConvertToHighResolution()) { booleanOfSelectedStructures.ConvertToHighResolution(); }


            List<Structure> tempStructsToBool = new List<Structure>();

            if (structuresToAddOrSubtract.Count > 0)
            {

              foreach (var s in structuresToAddOrSubtract)
              {

                // create temp structure to change to high res to preserve original structure
                var tempId = "temp_" + counter.ToString();
                // remove just in case
                Helpers.RemoveStructure(ss, tempId);

                var tempDicomType = s.DicomType == "EXTERNAL" || baseStructure.DicomType.Length == 0 ? CONTROL_DICOM_TYPE : baseStructure.DicomType.ToString();

                // create new temp structure
                Structure tempStructure = ss.AddStructure(tempDicomType, tempId);
                // copy segment volume
                tempStructure.SegmentVolume = s.SegmentVolume;
                // convert to High Res
                if (tempStructure.CanConvertToHighResolution()) { tempStructure.ConvertToHighResolution(); }


                tempStructsToBool.Add(tempStructure);
                counter += 1;
              }
              // returns a booleaned segment volume given a list of structures
              booleanOfSelectedStructures.SegmentVolume = Helpers.BooleanStructures(ss, tempStructsToBool);
              // created in the helpers.booleanstructures function above
              Helpers.RemoveStructure(ss, "zzzzTEMP");


              // create new structure

              // if add
              if (boolOperationAdd == true)
              {
                // returns a segment volume of the combined structures (OR)
                newStructure.SegmentVolume = Helpers.BooleanStructures(baseStructure, booleanOfSelectedStructures);
              }
              // if subtract
              else
              {
                // returns a segment volume of the CROP of the one structure from the other with the provided margin
                newStructure.SegmentVolume = baseStructure.SegmentVolume.Sub(booleanOfSelectedStructures.SegmentVolume);
                //newStructure.SegmentVolume = Helpers.CropStructure(baseStructure.SegmentVolume, booleanOfSelectedStructures, 0);
              }
              // add margin if option selected
              if (CreateBooleanWitMargin_CB.IsChecked == true) { newStructure.SegmentVolume = Helpers.AddMargin(newStructure, booleanedStructureGrowMargin); }

              // remove temp structs for good
              counter = 0;
              foreach (var s in structuresToAddOrSubtract)
              {
                var tempId = "temp_" + counter.ToString();
                // remove structure
                Helpers.RemoveStructure(ss, tempId);
                counter += 1;
              }
            }
            // if no structures selected to boolean/add or subtract
            else
            {
              // copy base structure
              newStructure.SegmentVolume = baseStructure.SegmentVolume;
              // add margin if option selected
              if (CreateBooleanWitMargin_CB.IsChecked == true) { newStructure.SegmentVolume = Helpers.AddMargin(newStructure, booleanedStructureGrowMargin); }
            }
            // remove temp bool structure
            Helpers.RemoveStructure(ss, booleanOfSelectedStructuresId);

          }
          // if not High Res
          else
          {
            if (structuresToAddOrSubtract.Count > 0)
            {
              // create booleaned structure
              // returns a booleaned seg vol given a list of structures
              booleanOfSelectedStructures.SegmentVolume = Helpers.BooleanStructures(ss, structuresToAddOrSubtract);
              // created in the helpers.booleanstructures function above
              Helpers.RemoveStructure(ss, "zzzzTEMP");

              // if add
              if (boolOperationAdd == true)
              {
                // returns a segment volume of the combined structures (OR)
                newStructure.SegmentVolume = Helpers.BooleanStructures(baseStructure, booleanOfSelectedStructures);
              }
              // if subtract
              else
              {
                // returns a segment volume of the CROP of the one structure from the other with the provided margin
                newStructure.SegmentVolume = baseStructure.SegmentVolume.Sub(booleanOfSelectedStructures.SegmentVolume);
                //newStructure.SegmentVolume = Helpers.CropStructure(baseStructure.SegmentVolume, booleanOfSelectedStructures, 0);
              }
              // add margin if option selected
              if (CreateBooleanWitMargin_CB.IsChecked == true) { newStructure.SegmentVolume = Helpers.AddMargin(newStructure, booleanedStructureGrowMargin); }
            }
            // if no structures selected to add/subtract
            else
            {
              // copy base structure
              newStructure.SegmentVolume = baseStructure.SegmentVolume;
              // add margin if option selected
              if (CreateBooleanWitMargin_CB.IsChecked == true) { newStructure.SegmentVolume = Helpers.AddMargin(newStructure, booleanedStructureGrowMargin); }
            }
            // remove temp struct for good
            Helpers.RemoveStructure(ss, booleanOfSelectedStructuresId);
          }
          MessageBox.Show(string.Format("{0} successfully created", newStructureId));
        }

      }
      else { MessageBox.Show("Sorry, no structures were created or booleaned"); }

    }

    // for design testing
    private void CreateBooleanedStructure_Btn_Click_DEV(object sender, RoutedEventArgs e)
    {
      bool okToContinue = true;
      // Define a regular expression for repeated words.
      Regex rx = new Regex(@"^[a-zA-Z0-9_-]+$",
                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

      var validBoolMargin = true;

      // set ring crop from target margin
      if (BooleanMargin_TextBox.Text == "" || string.IsNullOrWhiteSpace(BooleanMargin_TextBox.Text))
      {
        validBoolMargin = false;
      }
      else
      {
        if (int.TryParse(BooleanMargin_TextBox.Text, out booleanedStructureGrowMargin))
        {
          //parsing successful 
          if (InnerMargin_CB.IsChecked == true)
            booleanedStructureGrowMargin = -booleanedStructureGrowMargin;
        }
        else
        {
          //parsing failed. 
          validBoolMargin = false;
        }
      }


      if (!rx.IsMatch(BoolStructId_TextBox.Text)) { MessageBox.Show("Please enter a valid Structure Id \r\n\t- (hint: can only use letters, numbers, dashes, and underscores)"); okToContinue = false; return; }
      if (BoolBaseStruct_Combo.SelectedIndex == -1) { MessageBox.Show("Please select a base structure"); okToContinue = false; return; }
      if (CreateBooleanWitMargin_CB.IsChecked == true && validBoolMargin == false) { MessageBox.Show("Please enter a valid integer value for the booleaned structure margin"); okToContinue = false; return; }
      if (BoolBaseStruct_Combo.SelectedIndex >= 0)
      {
        // if the structure exists already
        if (ss.Structures.Where(x => x.Id.ToLower() == BoolStructId_TextBox.Text.ToLower()).ToList().Count > 0)
        {
          var result = MessageBox.Show("A structure exists with the same Id and the structure will be replaced.\r\n\r\n\tIs it OK to continue? ", "Some Title", MessageBoxButton.YesNo, MessageBoxImage.Warning);
          if (result == MessageBoxResult.Yes)
          {
            // ok to continue
          }
          else if (result == MessageBoxResult.No)
          {
            // not ok to continue
            okToContinue = false;
            return;
          }
        }
      }
      if (okToContinue == true)
      {

        //// allow modifications
        //patient.BeginModifications();

        bool makeHR = false;
        List<Structure> structuresToAddOrSubtract = new List<Structure>();

        var selectedBooleanStructureItems = from StructureItem item in StructureListForBooleanOperations_LV.Items
                                            where item.IsSelected == true
                                            select item;

        var selectedBooleanStructures = from StructureItem item in selectedBooleanStructureItems.ToList()
                                        select item.Id;

        var newStructureId = Helpers.ProcessStructureId(BoolStructId_TextBox.Text, MAX_ID_LENGTH);
        var baseStructureId = BoolBaseStruct_Combo.SelectedItem.ToString();
        


        var selectedMsg = newStructureId + "\r\n\r\nSelected Structures: \r\n\t";
          var hrMsg = "HighRes Structures: \r\n\t";
          var baseStructureMsg = "Base Structure: \r\n\t";
          var boolOperationMsg = "Bool Operation: \r\n\t";

          var baseStructure = ss.Structures.Single(x => x.Id == baseStructureId);
          var baseStructureDicomType = baseStructure.DicomType;
          MessageBox.Show(baseStructureDicomType.ToString() + " : " + baseStructureDicomType.Length);
        if (baseStructure.IsHighResolution == true) { hrMsg += baseStructure.Id + "\r\n\t"; makeHR = true; }

          baseStructureMsg += baseStructure.Id + "\r\n\t";

          if (selectedBooleanStructureItems.ToList().Count > 0)
          {
            foreach (var id in selectedBooleanStructures)
            {
              var s = ss.Structures.Single(x => x.Id == id);

              if (s.IsHighResolution == true) { hrMsg += s.Id + "\r\n\t"; makeHR = true; }

              selectedMsg += s.Id + "\r\n\t";
            }
          }


          boolOperationMsg += BoolOperationAdd_CB.IsChecked == true ? "Add" : "Subtract";

          MessageBox.Show(string.Format("{0}\r\n{1}\r\n\t{4}\r\n{2}\r\n{3}", selectedMsg, hrMsg, baseStructureMsg, boolOperationMsg, makeHR));

      }
      else { MessageBox.Show("Sorry, no structures were created or booleaned"); }

    }

    #endregion boolean structure button events

    #region approx overlap button events

    private void ApproxOverlap_CB_Click(object sender, RoutedEventArgs e)
    {
      var cb = sender as CheckBox;
      if (cb.IsChecked == true) { ApproximateOverlap_SP.Visibility = Visibility.Visible; }
      else if (cb.IsChecked == false) { ApproximateOverlap_SP.Visibility = Visibility.Collapsed; }
    }

    private void ApproximateOverlap_Btn_Click_DEV(object sender, RoutedEventArgs e)
    {

    }

    private void ApproximateOverlap_Btn_Click(object sender, RoutedEventArgs e)
    {
      if (OverlapStructure1_Combo.SelectedIndex < 0 || OverlapStructure2_Combo.SelectedIndex < 0) { MessageBox.Show("Please ensure both structures are selected"); return; }

      bool noneHR = false;
      bool onlyOneHR = false;
      bool bothHR = false;

      var s1 = ss.Structures.First(x => x.Id == OverlapStructure1_Combo.SelectedItem.ToString());
      var s2 = ss.Structures.First(x => x.Id == OverlapStructure2_Combo.SelectedItem.ToString());

      var tempS1Id = "zzztempS1";
      var tempS2Id = "zzztempS2";


      var volOverlap = 0.0;
      Tuple<double, double> percents = null;
      //var pctS1 = 0.0;
      //var pctS2 = 0.0;

      patient.BeginModifications();

      var overlapRegionId = "zOverlap";
      Helpers.RemoveStructure(ss, overlapRegionId);
      var overlapRegion = ss.AddStructure(CONTROL_DICOM_TYPE, overlapRegionId);

      if (!s1.IsHighResolution && !s2.IsHighResolution) { noneHR = true; }
      else if ((s1.IsHighResolution && !s2.IsHighResolution) || (!s1.IsHighResolution && s2.IsHighResolution)) { onlyOneHR = true; if (overlapRegion.CanConvertToHighResolution()) { overlapRegion.CanConvertToHighResolution(); } }
      else if (s1.IsHighResolution && s2.IsHighResolution) { bothHR = true; }
      else { MessageBox.Show("something's wrong, can't detect High Res status"); }

      if (noneHR || bothHR)
      {
        overlapRegion.SegmentVolume = s1.SegmentVolume.And(s2.SegmentVolume);
        
        volOverlap = Math.Round(overlapRegion.Volume, 3);
        percents = getOVerlapPercentages(overlapRegion.Volume, s1, s2);
        
      }
      else if (onlyOneHR)
      {
        if (s1.IsHighResolution)
        {
          Helpers.RemoveStructure(ss, tempS2Id);
          var tempS2 = ss.AddStructure(s2.DicomType.ToString(), tempS2Id);
          tempS2.SegmentVolume = s2.SegmentVolume;
          if (tempS2.CanConvertToHighResolution()) { tempS2.ConvertToHighResolution(); }

          overlapRegion.SegmentVolume = s1.SegmentVolume.And(tempS2);

          volOverlap = Math.Round(overlapRegion.Volume, 3);
          percents = getOVerlapPercentages(overlapRegion.Volume, s1, tempS2);
        }

        else if (s2.IsHighResolution)
        {
          Helpers.RemoveStructure(ss, tempS1Id);
          var tempS1 = ss.AddStructure(s2.DicomType.ToString(), tempS1Id);
          tempS1.SegmentVolume = s1.SegmentVolume;
          if (tempS1.CanConvertToHighResolution()) { tempS1.ConvertToHighResolution(); }

          overlapRegion.SegmentVolume = s2.SegmentVolume.And(tempS1);

          volOverlap = Math.Round(overlapRegion.Volume, 3);
          percents = getOVerlapPercentages(overlapRegion.Volume, tempS1, s2);
        }
        else { MessageBox.Show("Something's wrong: onlyOneHR"); }
      }

      VolumeOverlapCC_Label.Content = volOverlap.ToString();
      PctStructure1_Label.Content = percents.Item1.ToString();
      PctStructure2_Label.Content = percents.Item2.ToString();

      try { Helpers.RemoveStructure(ss, tempS1Id); } catch { }
      try { Helpers.RemoveStructure(ss, tempS2Id); } catch { }

    }

    /// <summary>
    /// returns item1: pct overlap of the 1st input structure, item2: pct overlap of 2nd input structure
    /// </summary>
    /// <param name="volumeOverlap"></param>
    /// <param name="structure1"></param>
    /// <param name="structure2"></param>
    /// <returns></returns>
    Tuple<double, double> getOVerlapPercentages(double volumeOverlap, Structure structure1, Structure structure2)
    {
        return new Tuple<double, double>(Math.Round(volumeOverlap / structure1.Volume, 3) * 100, Math.Round(volumeOverlap / structure2.Volume, 3) * 100);
    }

    #endregion approx overlap button events

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
