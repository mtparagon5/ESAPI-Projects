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

    public void CreateStructures_Btn_Click(object sender, RoutedEventArgs e)
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
        var message = "Sorry, could not find a structure of type BODY:\n\n\t- Structures will need to be manually cropped outside of the Body";
        var title = "Structure Error";

        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
      }

      // create zopti total
      Structure zoptiTotal = null;
      if (sorted_ptvList.Count() > 0)
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

        foreach (var s in avStructuresToMake)
        {

          var oar = (Structure)s;
          var avId = avPrefix + oar.Id.ToString();

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
      // TODO...change copied code to work for optis
      if (CreateOptis_CB.IsChecked == true)
      {

        var optiStructuresToMake = PTVList_LV.SelectedItems;
        string optiPrefix;
        int optiGrowMargin;
        int optiCropMargin;

        // set prefix
        if (AvoidGrowMargin_TextBox.Text == "")
        {
          optiGrowMargin = DEFAULT_AVOIDANCE_GROW_MARGIN;
        }
        if (OptiPrefix_TextBox.Text == "") { optiPrefix = DEFAULT_OPTI_PREFIX; } else { optiPrefix = OptiPrefix_TextBox.Text; }

        // set crop margin
        if (OptiCropMargin_TextBox.Text == "") { optiCropMargin = DEFAULT_OPTI_GROW_MARGIN; }
        else
        {
          if (int.TryParse(OptiCropMargin_TextBox.Text, out optiCropMargin))
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
        if (int.TryParse(OptiGrowMargin_TextBox.Text, out optiGrowMargin))
        {
          //parsing successful 
        }
        else
        {
          //parsing failed. 
          MessageBox.Show("Oops, please enter a valid Margin for your avoidance structures.");
        }

        foreach (var s in optiStructuresToMake)
        {

          var ptv = (Structure)s;
          var optiId = optiPrefix + ptv.Id.ToString();

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

          // TODO...add logic/functionality to allow user to select which optis should be cropped from which and with which margins

        }

      }

      #endregion opti structures

      
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

    #endregion
    //---------------------------------------------------------------------------------



  }
}
