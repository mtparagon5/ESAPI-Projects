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

    // DEFAULTS
    const string DEFAULT_AVOIDANCE_PREFIX = "zav ";
    const int DEFAULT_AVOIDANCE_GROW_MARGIN = 2;
    const int DEFAULT_AVOIDANCE_CROP_MARGIN = 2;
    const string AVOIDANCE_DICOM_TYPE = "AVOIDANCE";

    const string DEFAULT_OPTI_PREFIX = "zopti ";
    const int DEFAULT_OPTI_GROW_MARGIN = 1;
    const int DEFAULT_OPTI_CROP_MARGIN = 2;
    const int DEFAULT_OPTI_CROP_FROM_BODY_MARGIN = 4;
    const string OPTI_DICOM_TYPE = "PTV";

    const string DEFAULT_RING_PREFIX = "zring ";
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

    #region basic events

    private void CreateAvoids_CB_Click(object sender, RoutedEventArgs e)
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

    private void CreateRingss_CB_Click(object sender, RoutedEventArgs e)
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

    private void CreateOptis_CB_Click(object sender, RoutedEventArgs e)
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

    private void Instructions_Button_Click(object sender, RoutedEventArgs e)
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

    private void CreateStructures_Btn_Click(object sender, RoutedEventArgs e)
    {
      patient.BeginModifications();

      var avStructures = OarList_LV.SelectedItems;
      string avPrefix;
      int avGrowMargin;
      int avCropMargin;

      #region set/parse variables
      // set prefix
      if (AvoidGrowMargin_TextBox.Text == "")
      {
        avGrowMargin = DEFAULT_AVOIDANCE_GROW_MARGIN;
      }
      if (AvoidPrefix_TextBox.Text == "") { avPrefix = DEFAULT_AVOIDANCE_PREFIX; } else { avPrefix = AvoidPrefix_TextBox.Text; }

      // set crop margin
      if (AvoidCropMargin_TextBox.Text == "") { avCropMargin = DEFAULT_AVOIDANCE_GROW_MARGIN; }
      else
      {
        if (int.TryParse(AvoidCropMargin_TextBox.Text, out avCropMargin))
        {
          //parsing successful 
        }
        else
        {
          //parsing failed. 
          MessageBox.Show("Oops, please enter a valid Crop Margin for your avoidance structures.");
        }
      }

      // set grow margin
      if (int.TryParse(AvoidGrowMargin_TextBox.Text, out avGrowMargin))
      {
        //parsing successful 
      }
      else
      {
        //parsing failed. 
        MessageBox.Show("Oops, please enter a valid Margin for your avoidance structures.");
      }

      #endregion set/parse variables

      foreach (var s in avStructures)
      {
        var oar = (Structure)s;
        var avId = avPrefix + oar.Id.ToString();

        // remove structure if present in ss
        RemoveStructure(ss, avId);

        // add structure
        var avoid = ss.AddStructure(AVOIDANCE_DICOM_TYPE, avId);
        avoid.SegmentVolume = addMargin(oar, (double)avGrowMargin);
        avoid.SegmentVolume = cropStructure(avoid, oar, avCropMargin);


        // add margin to existing structure








        // TODO: for opti structures

        //// get body structure
        //Structure body;
        //try
        //{
        //  body = ss.Structures.FirstOrDefault(x => x.Id.ToLower() == "body" || x.Id.ToLower() == "external");
        //}
        //catch
        //{
        //  throw new Exception("Can't identify a Body contour.");
        //}

        //// remove previous body-margin structure
        //RemoveStructure(ss, "BODY-MARGIN");
        //// create body-margin structure and add internal margin for cropping targets outside body plus margin
        //var bodyLessMargin = ss.AddStructure("AVOIDANCE", "BODY-MARGIN");
        //bodyLessMargin.SegmentVolume = body.Margin(-DEFAULT_OPTI_CROP_MARGIN);
      }

    }

    #endregion

    #endregion

    #endregion
    //---------------------------------------------------------------------------------
    #region helper methods

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

    

    /// <summary>
    /// function to remove structure
    /// </summary>
    /// <param name="ss">StructureSet</param>
    /// <param name="structureId">Structure Id to match</param>
    private static void RemoveStructure(StructureSet ss, string structureId)
    {
      if (ss.Structures.Any(st => st.Id == structureId))
      {
        var st = ss.Structures.Single(x => x.Id == structureId);
        ss.RemoveStructure(st);
      }
    }

   /// <summary>
   /// Add margin to existing structure
   /// </summary>
   /// <param name="structure"></param>
   /// <param name="margin"></param>
   /// <returns></returns>
    private static SegmentVolume addMargin(Structure structure, double margin)
    {
      return structure.SegmentVolume.Margin(margin);
    }

    /// <summary>
    /// Crop structure from another structure
    /// </summary>
    /// <param name="structureToCrop"></param>
    /// <param name="StructureToCropFrom"></param>
    /// <param name="cropMargin"></param>
    /// <returns></returns>
    private static SegmentVolume cropStructure(Structure structureToCrop, Structure StructureToCropFrom, double cropMargin)
    {
      return structureToCrop.SegmentVolume.Sub(StructureToCropFrom.SegmentVolume.Margin(cropMargin));
    }

    /// <summary>
    /// Crop opti ptv from higher dose ptv with given margin -- default margin = 1.0 mm
    /// </summary>
    /// <param name="opti"></param>
    /// <param name="ptv"></param>
    /// <param name="cropMargin"></param>
    /// <returns></returns>
    private static SegmentVolume cropOpti(Structure opti, Structure ptv, double cropMargin = 1)
    {
      return opti.SegmentVolume.Sub(ptv.SegmentVolume.Margin(cropMargin));
    }

    /// <summary>
    /// Crop structure outside body with given margin -- default margin = -4.0 mm (e.g. for targets)
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="body"></param>
    /// <param name="cropMargin"></param>
    /// <returns></returns>
    private static SegmentVolume cropOutsideBodyWithMargin(Structure structure, Structure body, double cropMargin = -4)
    {
      return structure.SegmentVolume.And(body.SegmentVolume.Margin(cropMargin));
    }

    /// <summary>
    /// Crop structure outside body structure that already has an internal margin
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="bodyLessMargin"></param>
    /// <returns></returns>
    private static SegmentVolume cropOutsideBodyLessMargin(Structure structure, Structure bodyLessMargin)
    {
      return structure.SegmentVolume.And(bodyLessMargin.SegmentVolume);
    }

    /// <summary>
    /// Boolean two structures
    /// </summary>
    /// <param name="structure1"></param>
    /// <param name="structure2"></param>
    /// <returns></returns>
    private static SegmentVolume booleanStructures(Structure structure1, Structure structure2)
    {
      return structure1.SegmentVolume.Or(structure2.SegmentVolume);
    }

    /// <summary>
    /// Boolean a list of structures
    /// </summary>
    /// <param name="structuresToBoolean"></param>
    /// <returns></returns>
    private static Structure booleanStructures(List<Structure> structuresToBoolean)
    {
      Structure combinedStructure = structuresToBoolean[0];
      foreach (var s in structuresToBoolean)
      {
        combinedStructure.SegmentVolume = combinedStructure.SegmentVolume.Or(s.SegmentVolume);
      }
      return combinedStructure;
    }

    #endregion
    //---------------------------------------------------------------------------------



  }
}
