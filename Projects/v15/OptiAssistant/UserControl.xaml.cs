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
    public string highresMessage;
    public bool hasHighRes = false;
    public bool hasHighResTargets = false;
    public bool needHRStructures = false;
    private string MESSAGES = string.Empty;
    private int counter = 1;


    //public bool createOptiGTVForSingleLesion = false;
    //public bool createOptiTotal = false;

    // DEFAULTS
    const string DEFAULT_AVOIDANCE_PREFIX = "zav";
    const int DEFAULT_AVOIDANCE_GROW_MARGIN = 2;
    const int DEFAULT_AVOIDANCE_CROP_MARGIN = 2;
    const int DEFAULT_AVOID_CROP_FROM_BODY_MARGIN = 0;
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

    const string CONTROL_DICOM_TYPE = "CONTROL";



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
    //public void CreateStructures_Btn_Click(object sender, RoutedEventArgs e) { }

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
      if (CreateOptis_CB.IsChecked == true && MultipleDoseLevels_CB.IsChecked == true)
      {
        if (PTVList_LV.SelectedItems.Count < 1)
        {
          MessageBox.Show("Oops, it appears you'd like to create opti ptv structures but haven't selected any targets to create optis for.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          continueToCreateStructures = false;
        }
        if (DoseLevel1_Radio.IsChecked == true && PTVList_LV.SelectedItems.Count != 1) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
        if (DoseLevel2_Radio.IsChecked == true && PTVList_LV.SelectedItems.Count != 2) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
        if (DoseLevel3_Radio.IsChecked == true && PTVList_LV.SelectedItems.Count != 3) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
        if (DoseLevel4_Radio.IsChecked == true && PTVList_LV.SelectedItems.Count != 4) { MessageBox.Show("Number of Selected Targets and Selected Dose Levels do not match.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning); continueToCreateStructures = false; }
        if (DoseLevel1_Radio.IsChecked == true && DoseLevel1_LB.SelectedIndex < 0)
        {
          MessageBox.Show("One Dose Level has been selected:\n\n\t- At least one target should be selected.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          continueToCreateStructures = false;
        }
        if (DoseLevel2_Radio.IsChecked == true && (DoseLevel1_LB.SelectedIndex < 0 || DoseLevel2_LB.SelectedIndex < 0))
        {
          MessageBox.Show("Two Dose Levels have been selected:\n\n\t- Targets for at least two dose levels should be selected.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          continueToCreateStructures = false;
        }
        if (DoseLevel3_Radio.IsChecked == true && (DoseLevel1_LB.SelectedIndex < 0 || DoseLevel2_LB.SelectedIndex < 0 || DoseLevel3_LB.SelectedIndex < 0))
        {
          MessageBox.Show("Three Dose Levels have been selected:\n\n\t- Targets for at least three dose levels should be selected.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          continueToCreateStructures = false;
        }
        if (DoseLevel4_Radio.IsChecked == true && (DoseLevel1_LB.SelectedIndex < 0 || DoseLevel2_LB.SelectedIndex < 0 || DoseLevel3_LB.SelectedIndex < 0 || DoseLevel4_LB.SelectedIndex < 0))
        {
          MessageBox.Show("Four Dose Levels have been selected:\n\n\t- Targets for at least four dose levels should be selected.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
          continueToCreateStructures = false;
        }
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
      if (CreateRings_CB.IsChecked == true && PTVListForRings_LV.SelectedItems.Count == 0)
      {
        MessageBox.Show("Oops, it appears you'd like to create ring structures but haven't selected any targets to create rings for.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        continueToCreateStructures = false;
      }

      #endregion validation

      // NOTE: various other validation can be done. To reduce the need for some validation, when user input for margins is invalid, a warning will show informing the user of the invalid entry and that the default value will instead be used. 
      if (continueToCreateStructures)
      {
        using (new WaitCursor())
        {


          // add messages description
          MESSAGES += string.Format("Some General Tasks/Issues during script run: {0}", counter);
          if (needHRStructures) { MESSAGES += "\r\n\t- Some of the selected structures are High Res Structures so\r\n\t\ta High Res Body and High Res Opti Total will be created"; }

          // progress counter in case user clicks Create Structures Button more than once during same instance of script
          counter += 1;

          // allow modifications
          patient.BeginModifications();

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

          //MessageBox.Show(string.Format("{0}", tempCounter));
          //tempCounter += 1;

          // create high res body for cropping
          Structure bodyHR = null;
          string bodyHRId = "zzBODY_HR";

          // create zopti total High Res for booleans/cropping
          Structure zoptiTotalHR = null;
          string zoptiTotalHRId = "zzopti total_HR";
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
                var hrId = string.Format("zz{0}_HR", t.Id);
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

          //MessageBox.Show(string.Format("{0}", tempCounter));
          //tempCounter += 1;

          // create zopti total if ptv(s) present
          Structure zoptiTotal = null;
          string zoptiTotalId = "zopti total";

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
              zoptiTotal.SegmentVolume = Helpers.CropOutsideBodyWithMargin(zoptiTotal, body, -DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
              MESSAGES += string.Format("\r\n\t- {0} Cropped {1} mm From Body Surface", zoptiTotal.Id, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
            }
            catch
            {
              {
                try
                {
                  zoptiTotal.SegmentVolume = Helpers.CropOutsideBodyWithMargin(zoptiTotal, bodyHR, -DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                  MESSAGES += string.Format("\r\n\t- {0} Cropped {1} mm From High Res Body Surface", zoptiTotal.Id, DEFAULT_OPTI_CROP_FROM_BODY_MARGIN);
                }
                catch
                {
                  MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From Body Surface***", zoptiTotal.Id);
                }
              }
            }


          }
          //MessageBox.Show(string.Format("{0}", tempCounter));
          //tempCounter += 1;

          #region avoidance structures

          if (CreateAvoids_CB.IsChecked == true)
          {

            var avStructuresToMake = OarList_LV.SelectedItems;
            string avPrefix;
            int avGrowMargin;
            int avCropMargin;

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

            foreach (var s in avStructuresToMake)
            {
              var oar = ss.Structures.Single(st => st.Id == s.ToString());
              //MessageBox.Show(string.Format("Structure Matched: {0}", oar.Id));

              var avId = string.Format("{0} {1}", avPrefix, oar.Id.ToString());

              // remove structure if present in ss
              Helpers.RemoveStructure(ss, avId);

              // add empty avoid structure
              var avoidStructure = ss.AddStructure(AVOIDANCE_DICOM_TYPE, avId);
              MESSAGES += string.Format("\r\n\t- Structure Added: {0}", avoidStructure.Id);

              // copy oar with defined margin
              avoidStructure.SegmentVolume = Helpers.AddMargin(oar, (double)avGrowMargin);
              MESSAGES += string.Format("\r\n\t- {1} mm Margin Added: {0}", avoidStructure.Id, avGrowMargin);

              //MessageBox.Show(string.Format("{0}", tempCounter));
              //tempCounter += 1;

              // crop from body
              try
              {
                avoidStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(avoidStructure, bodyHR, -DEFAULT_AVOID_CROP_FROM_BODY_MARGIN);
                MESSAGES += string.Format("\r\n\t- The {0} is a High Res Structure and has been cropped\r\n\t\tfrom the High Res Body Structure", avoidStructure.Id);
              }
              catch
              {
                try
                {
                  avoidStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(avoidStructure, body, -DEFAULT_AVOID_CROP_FROM_BODY_MARGIN);
                  MESSAGES += string.Format("\r\n\t- {0} Cropped {1} mm From Body Surface", avoidStructure.Id, DEFAULT_AVOID_CROP_FROM_BODY_MARGIN);
                }
                catch
                {
                  MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From Body***", avoidStructure.Id);
                }
              }
              //MessageBox.Show(string.Format("{0}", tempCounter));
              //tempCounter += 1;

              // crop avoid structure from ptv total (if ptvs are found)
              //MessageBox.Show(string.Format("Attempting to crop {0} from target", avoidStructure.Id));
              try
              {
                avoidStructure.SegmentVolume = Helpers.CropStructure(avoidStructure.SegmentVolume, zoptiTotalHR.SegmentVolume, avCropMargin);
                MESSAGES += string.Format("\r\n\t- {0} is a High Res Structure and has been cropped\r\n\t\tfrom the High Res Opti Target", avoidStructure.Id);
              }
              catch
              {
                try
                {
                  avoidStructure.SegmentVolume = Helpers.CropStructure(avoidStructure.SegmentVolume, zoptiTotal.SegmentVolume, avCropMargin);
                  MESSAGES += string.Format("\r\n\t- {0} cropped {1} mm from {2}", avoidStructure.Id, avCropMargin, zoptiTotalId);
                }
                catch
                {
                  try
                  {
                    if (avoidStructure.CanConvertToHighResolution()) { avoidStructure.ConvertToHighResolution(); }
                    MESSAGES += string.Format("\r\n\t- {0} had to be converted to a High Res Structure to be cropped", avoidStructure.Id);

                    avoidStructure.SegmentVolume = Helpers.CropStructure(avoidStructure.SegmentVolume, zoptiTotalHR.SegmentVolume, avCropMargin);
                    MESSAGES += string.Format("\r\n\t- {0} is a High Res Structure and has been cropped\r\n\t\tfrom the High Res Opti Target", avoidStructure.Id);
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
              }
            }
          }

          #endregion avoidance structures

          #region opti structures

          if (CreateOptis_CB.IsChecked == true)
          {

            if (CropFromBody_CB.IsChecked == true) { cropFromBody = true; }
            else { cropFromBody = false; }

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
                doseLevel1Target = Helpers.GetStructure(ss, DoseLevel1_LB.SelectedItem.ToString());
                MessageBox.Show(string.Format("Dose Level 1 Set to: {0}", doseLevel1Target.Id));
              }
              else if (DoseLevel2_Radio.IsChecked == true)
              {
                doseLevel1Target = Helpers.GetStructure(ss, DoseLevel1_LB.SelectedItem.ToString());
                doseLevel2Target = Helpers.GetStructure(ss, DoseLevel2_LB.SelectedItem.ToString());

                hasTwoDoseLevels = true;
              }
              else if (DoseLevel3_Radio.IsChecked == true)
              {
                doseLevel1Target = Helpers.GetStructure(ss, DoseLevel1_LB.SelectedItem.ToString());
                doseLevel2Target = Helpers.GetStructure(ss, DoseLevel2_LB.SelectedItem.ToString());
                doseLevel3Target = Helpers.GetStructure(ss, DoseLevel3_LB.SelectedItem.ToString());

                hasThreeDoseLevels = true;
              }
              else if (DoseLevel4_Radio.IsChecked == true)
              {
                doseLevel1Target = Helpers.GetStructure(ss, DoseLevel1_LB.SelectedItem.ToString());
                doseLevel2Target = Helpers.GetStructure(ss, DoseLevel2_LB.SelectedItem.ToString());
                doseLevel3Target = Helpers.GetStructure(ss, DoseLevel3_LB.SelectedItem.ToString());
                doseLevel4Target = Helpers.GetStructure(ss, DoseLevel4_LB.SelectedItem.ToString());

                hasFourDoseLevels = true;
              }
            }

            // create optis
            foreach (var s in optiStructuresToMake)
            {
              var ptv = ss.Structures.Single(st => st.Id == s.ToString());
              var optiId = string.Format("{0} {1}", optiPrefix, ptv.Id.ToString());

              if (needHRStructures && ptv.CanConvertToHighResolution())
              {
                ptv.ConvertToHighResolution();
              }

              // remove structure if present in ss
              Helpers.RemoveStructure(ss, optiId);

              // add empty avoid structure
              var optiStructure = ss.AddStructure(OPTI_DICOM_TYPE, optiId);

              //if (needHRStructures && optiStructure.CanConvertToHighResolution())
              //{
              //  optiStructure.ConvertToHighResolution();
              //}

              // copy ptv with defined margin
              optiStructure.SegmentVolume = Helpers.AddMargin(ptv, (double)optiGrowMargin);
              MESSAGES += string.Format("\r\n\t- {1} mm Margin Added: {0}", optiStructure.Id, optiGrowMargin);

              // crop OPTI structure from body surface (if body found)
              if (cropFromBody)
              {
                try
                {
                  optiStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(optiStructure, bodyHR, -optiCropFromBodyMargin);
                  MESSAGES += string.Format("\r\n\t- The {0} is a High Res Structure and has been cropped from\r\n\t\tthe High Res Body Structure", optiStructure.Id);
                }
                catch
                {
                  try
                  {
                    optiStructure.SegmentVolume = Helpers.CropOutsideBodyWithMargin(optiStructure, body, -optiCropFromBodyMargin);
                    MESSAGES += string.Format("\r\n\t- {0} Cropped {1} mm From Body Surface", optiStructure.Id, optiCropFromBodyMargin);
                  }
                  catch
                  {
                    MESSAGES += string.Format("\r\n\t- ***Trouble Cropping {0} From Body***", optiStructure.Id);
                  }
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

                else
                {
                  MessageBox.Show("Oops, something went wrong while Cropping Opti PTVs");
                }
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
                var ringId = string.Format("{0} {1} {2}", ringPrefix, target.Id, ringNum);

                // remove ring structure if present in ss
                Helpers.RemoveStructure(ss, ringId);

                // add empty ring structure
                var ringStructure = ss.AddStructure(RING_DICOM_TYPE, ringId);

                // copy target with defined margin
                ringStructure.SegmentVolume = Helpers.AddMargin(target, (double)ringGrowMargin * ringNum);
                MESSAGES += string.Format("\r\n\t- {0} Added with {1} mm margin to {2}", ringStructure.Id, ringGrowMargin * ringNum, target.Id);

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
                var currentRingId = string.Format("{0} {1} {2}", ringPrefix, target.Id, i);
                var nextLargestRingId = string.Format("{0} {1} {2}", ringPrefix, target.Id, i - 1);
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
          Helpers.RemoveStructure(ss, "zzzTEMP");
          foreach (var t in sorted_ptvList)
          {
            Helpers.RemoveStructure(ss, string.Format("zz{0}_HR", t.Id));
          }

          #endregion clean up structure set

          MESSAGES += "\r\n\r\n\tNOTE: *** Denotes there was an issue occur during the tasks process\r\n\r\n";

          MessageBox.Show(MESSAGES, "General Steps Completed");
        }
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
