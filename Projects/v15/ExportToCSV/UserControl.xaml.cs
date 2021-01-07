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

namespace ExportToCSV
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  /// 
  
  public partial class MainControl : UserControl
  {
    public MainControl()
    {
        InitializeComponent();
    }

    //---------------------------------------------------------------------------------
    #region default constants

    const string DEFAULT_DVH_EXPORT_PATH = ".../RO PHI PHYSICS/__DVH Script Data/DVH Data/{username}/{datafile.csv}";
    const string CUSTOM_DVH_EXPORT_PATH = ".../RO PHI PHYSICS/__DVH Script Data/DVH Data/{username}/";
    const string BASE_DVH_EXPORT_PATH = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\_DVH Script Data\\DVH Data\\";

    const DoseValuePresentation DOSE_ABS = DoseValuePresentation.Absolute;
    const DoseValuePresentation DOSE_REL = DoseValuePresentation.Relative;
    const VolumePresentation VOL_ABS = VolumePresentation.AbsoluteCm3;
    const VolumePresentation VOL_REL = VolumePresentation.Relative;

    #endregion default constants
    //---------------------------------------------------------------------------------
    #region public variables

    // PUBLIC VARIABLES
    //public Window Window;
    public string script = "ExportToCSV";
    public Patient patient;
    public PlanSetup ps;
    public PlanningItem pitem;
    public StructureSet ss;
    public double dosePerFraction;
    public double numFractions;
    public double prescriptionDose;
    public IEnumerable<Structure> sorted_gtvList;
    public IEnumerable<Structure> sorted_ctvList;
    public IEnumerable<Structure> sorted_itvList;
    public IEnumerable<Structure> sorted_ptvList;
    public IEnumerable<Structure> sorted_targetList;
    public IEnumerable<Structure> sorted_oarList;
    public IEnumerable<Structure> sorted_structureList;
    public IEnumerable<Structure> sorted_emptyStructuresList;
    public string userName;
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
    public string patientId;
    public string courseId;
    public string ssId;
    public bool isGrady;

    // dvh options
    public DoseValuePresentation doseValuePresentation = DOSE_ABS;
    public VolumePresentation volumePresentation = VOL_REL;
    // dvhs (dose volume)
    public DVHData dvh_AA; // abs dose and abs vol
    public DVHData dvh_AR; // abs dose and rel vol
    public DVHData dvh_RA; // rel dose and abs vol
    public DVHData dvh_RR; // rel dose and rel vol
    // bin size
    public double bin_size = 0.01;
    public int doseDecimalsToRoundTo = 2;

    public List<string> resolutionOptionsList = new List<string> {
        "0.1 Gy (10 cGy => Fastest)",
        "0.01 Gy (1 cGy => Intermediate)",
        "0.001 Gy (0.1 cGy => Slowest)",
      };

    // csv path
    public string csvDataFilePath = string.Empty;
    public StringBuilder dvhCsvContent = new StringBuilder();

    private bool collectData = true;
    public bool useDefaultPath = true;



    // lists for objects in the structures list box (for binding) -- necessary for using checkboxes in listboxes
    public ObservableCollection<StructureItem> structureListBoxItems = new ObservableCollection<StructureItem>();

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

    public string userLogPath = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\esapi\\projects\\__Logs\\DVHToCSV_UserLog.csv";


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

    #region dvh export option events

    // event fired when user selects/unselects dvh data export checkbox
    private void DVHData_CB_Click(object sender, RoutedEventArgs e)
    {
      var cb = sender as CheckBox;
      if (cb.IsChecked == false) { MessageBox.Show("Sorry, DVH Data collection is all that is avaliable at this time. Let Matt know if there is something else you'd like to export."); }
    }

    private void HandleResolutionSelection(object sender, RoutedEventArgs e)
    {
      if (ResolutionSelector_Combo.SelectedIndex == 0)
      {
        bin_size = 0.1;
        doseDecimalsToRoundTo = 1;
        MessageBox.Show("Depending on your needs, setting the dose resolution to 0.1 Gy may result in data loss.\r\n\r\nIf you experience data integrity issues, try selecting 0.01 Gy or 0.001 Gy.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
      }
      else if (ResolutionSelector_Combo.SelectedIndex == 1)
      {
        bin_size = 0.01;
        doseDecimalsToRoundTo = 2;
      }
      else if (ResolutionSelector_Combo.SelectedIndex == 2)
      {
        bin_size = 0.001;
        doseDecimalsToRoundTo = 3;
        MessageBox.Show("Depending on your machine, setting the dose resolution to 0.001 Gy will cause data collection to be slow.\r\n\r\nIf you experience performance issues, try selecting 0.01 Gy or 0.1 Gy.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
      }
    }

    
    // event fired when dvh options selected/unselected
    private void HandleDVHExportOptions(object sender, RoutedEventArgs e)
    {
      var cb = sender as CheckBox;

      var absoluteDose = "AbsoluteDose_CB";
      var relativeDose = "RelativeDose_CB";
      var absoluteVolume = "AbsoluteVolume_CB";
      var relativeVolume = "RelativeVolume_CB";

      var defaultFolder = "UseDefaultFolder_CB";
      var customFolder = "UseCustomFolder_CB";

      

      if (cb.IsChecked == true)
      {
        // dose
        if (cb.Name == absoluteDose)
        {
          RelativeDose_CB.IsChecked = false;
          doseValuePresentation = DOSE_ABS;
        }
        if (cb.Name == relativeDose)
        {
          AbsoluteDose_CB.IsChecked = false;
          doseValuePresentation = DOSE_REL;
        }
        // volume options
        if (cb.Name == absoluteVolume)
        {
          RelativeVolume_CB.IsChecked = false;
          volumePresentation = VOL_ABS;
        }
        if (cb.Name == relativeVolume)
        {
          AbsoluteVolume_CB.IsChecked = false;
          volumePresentation = VOL_REL;
        }
        // path options
        if (cb.Name == defaultFolder) // if default folder to be used
        {
          UseCustomFolder_CB.IsChecked = false;
          PathDisplay_Label.Content = DEFAULT_DVH_EXPORT_PATH;
          CustomFolder_SP.Visibility = Visibility.Collapsed;
          useDefaultPath = true;
        }
        if (cb.Name == customFolder) // if custom folder to be used
        {
          UseDefaultFolder_CB.IsChecked = false;
          PathDisplay_Label.Content = CUSTOM_DVH_EXPORT_PATH;
          CustomFolder_SP.Visibility = Visibility.Visible;
          useDefaultPath = false;
        }
      }
      if (cb.IsChecked == false)
      {
        // dose options
        if (cb.Name == absoluteDose)
        {
          RelativeDose_CB.IsChecked = true;
          doseValuePresentation = DOSE_REL;
        }
        if (cb.Name == relativeDose)
        {
          AbsoluteDose_CB.IsChecked = true;
          doseValuePresentation = DOSE_ABS;
        }
        // volume options
        if (cb.Name == absoluteVolume)
        {
          RelativeVolume_CB.IsChecked = true;
          volumePresentation = VOL_REL;
        }
        if (cb.Name == relativeVolume)
        {
          AbsoluteVolume_CB.IsChecked = true;
          volumePresentation = VOL_ABS;
        }
        // path options
        if (cb.Name == defaultFolder) // if custom folder to be used
        {
          UseCustomFolder_CB.IsChecked = true;
          PathDisplay_Label.Content = CUSTOM_DVH_EXPORT_PATH;
          CustomFolder_SP.Visibility = Visibility.Visible;
          useDefaultPath = false;
        }
        if (cb.Name == customFolder) // if default folder to be used
        {
          UseDefaultFolder_CB.IsChecked = true;
          PathDisplay_Label.Content = DEFAULT_DVH_EXPORT_PATH;
          CustomFolder_SP.Visibility = Visibility.Collapsed;
          useDefaultPath = true;
        }
      }

    }

    


    #endregion dvh export option events

    #region prox stat events

    private void ProximityStats_CB_Click(object sender, RoutedEventArgs e)
    {
      MessageBox.Show("Let Matt know if this is something that would be useful.");
    }

    #endregion prox stat events


    #region export data button event

    private void ExportData_Btn_Click(object sender, RoutedEventArgs e)
    {
      var planId = pitem.Id.ToString().Replace(":", "-").Replace(" ", "_");
      

      // get selected structures
      var selectedStructureItems = from StructureItem item in StructureList_LV.Items
                                   where item.IsSelected == true
                                   select item;
      // validation
      if (selectedStructureItems.Count() == 0)
      {
        MessageBox.Show("Sorry, no structures have been selected for data collection.", "Form Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        collectData = false;
      }
      else
      {
        collectData = true;
      }

      // if valid
      if (collectData == true)
      {
        // clear any previous data if button clicked more than once
        dvhCsvContent.Clear();

        // get ids of selected structures
        var selectedStructures = from StructureItem item in selectedStructureItems.ToList()
                                 select item.Id;

        #region set headers

        List<string> dataHeaderList = new List<string>();
        dataHeaderList.Add("MRN");
        dataHeaderList.Add("Course_Id");
        dataHeaderList.Add("Plan_Id");
        dataHeaderList.Add("Num_Fractions");
        dataHeaderList.Add("Dose_Per_Fraction");
        dataHeaderList.Add("Prescription_Dose");
        dataHeaderList.Add("Structure_Id");
        dataHeaderList.Add("Structure_Name");
        dataHeaderList.Add("Structure_RGBA");
        dataHeaderList.Add("Structure_RGB");
        dataHeaderList.Add("Structure_Type");
        dataHeaderList.Add("Structure_Volume");
        dataHeaderList.Add("DVH_Dose_Presentation");
        dataHeaderList.Add("DVH_Volume_Presentation");
        dataHeaderList.Add("Min_Dose");
        dataHeaderList.Add("Min_0_03cc_Dose");
        dataHeaderList.Add("Max_Dose");
        dataHeaderList.Add("Max_0_03cc_Dose");
        dataHeaderList.Add("Mean_Dose");
        dataHeaderList.Add("Median_Dose");
        dataHeaderList.Add(doseValuePresentation == DoseValuePresentation.Absolute ? "Std_Dev_Gy" : "Std_Dev_Pct");
        dataHeaderList.Add("Dose_Coverage_Pct");
        dataHeaderList.Add("Sampling_Coverage_Pct");

        // get body and body dvh to add dose headers up to max dose
        var bodyStructure = ss.Structures.Single(st => st.DicomType == "EXTERNAL");
        var bodyDVH = pitem.GetDVHCumulativeData(bodyStructure, doseValuePresentation, volumePresentation, bin_size);
        // add header for each dose value in dvh curveData                                                                  //TODO: VERIFY THE DOSE VALUES ARE THE SAME FOR ALL STRUCTURES SINCE DVH IS UNIQUE TO BODY
        foreach (var point in bodyDVH.CurveData)
        {
          dataHeaderList.Add("" + Math.Round(point.DoseValue.Dose, doseDecimalsToRoundTo).ToString());
        }

        dvhCsvContent.AppendLine(string.Join(",", dataHeaderList.ToArray()));

        #endregion set headers

        var mrn = patient.Id;

        foreach (var structure in selectedStructures)
        {
          // get the structure
          var s = ss.Structures.Single(st => st.Id == structure);

          // get structure dvh
          var dvh = pitem.GetDVHCumulativeData(s, doseValuePresentation, volumePresentation, bin_size);
          var dvh_AbsVol = pitem.GetDVHCumulativeData(s, doseValuePresentation, VolumePresentation.AbsoluteCm3, .001);

          // plan variables

          // define structure related variables
          var sId = s.Id;
          var sName = s.Name;
          var sVolume = Math.Round(s.Volume, 5).ToString();
          var sRGBA = string.Format("rgba({0}-{1}-{2}-{3})", s.Color.R, s.Color.G, s.Color.B, s.Color.A);
          var sRGB = string.Format("rgb({0}-{1}-{2})", s.Color.R, s.Color.G, s.Color.B);
          var sType = s.DicomType;
          // dose variables
          var min = Math.Round(dvh.MinDose.Dose, 5).ToString();
          var min_0_03cc = Math.Round(DvhExtensions.getDoseAtVolume(dvh_AbsVol, s.Volume - 0.03), 5).ToString();
          var max = Math.Round(dvh.MaxDose.Dose, 5).ToString();
          var max_0_03cc = Math.Round(DvhExtensions.getDoseAtVolume(dvh_AbsVol, 0.03), 5).ToString();
          var mean = Math.Round(dvh.MeanDose.Dose, 5).ToString();
          var median = Math.Round(dvh.MedianDose.Dose, 5).ToString();
          var samplingCoverage = Math.Round(dvh.SamplingCoverage * 100, 1).ToString();
          var doseCoverage = Math.Round(dvh.Coverage * 100, 1).ToString();
          var stdDev = doseValuePresentation == DoseValuePresentation.Absolute ? Math.Round(dvh.StdDev, 3).ToString() : Math.Round(dvh.StdDev * 100, 1).ToString();


          // COLUMN ORDER
          //dataHeaderList.Add("MRN");
          //dataHeaderList.Add("Course_Id");
          //dataHeaderList.Add("Plan_Id");
          //dataHeaderList.Add("Num_Fractions");
          //dataHeaderList.Add("Dose_Per_Fraction");
          //dataHeaderList.Add("Prescription_Dose");
          //dataHeaderList.Add("Structure_Id");
          //dataHeaderList.Add("Structure_Name");
          //dataHeaderList.Add("Structure_RGBA");
          //dataHeaderList.Add("Structure_RGB");
          //dataHeaderList.Add("Structure_Type");
          //dataHeaderList.Add("Structure_Volume");
          //dataHeaderList.Add("DVH_Dose_Presentation");
          //dataHeaderList.Add("DVH_Volume_Presentation");
          //dataHeaderList.Add("Structure_Volume");
          //dataHeaderList.Add("Min_Dose");
          //dataHeaderList.Add("Min_0_03cc_Dose");
          //dataHeaderList.Add("Max_Dose");
          //dataHeaderList.Add("Max_0_03cc_Dose");
          //dataHeaderList.Add("Mean_Dose");
          //dataHeaderList.Add("Median_Dose");
          //dataHeaderList.Add("Std_Dev");
          //dataHeaderList.Add("Dose_Coverage");
          //dataHeaderList.Add("Sampling_Coverage");

          List<string> dataList = new List<string>();
          dataList.Add(mrn);
          dataList.Add(courseId);
          dataList.Add(planId);
          dataList.Add(numFractions.ToString());
          dataList.Add(dosePerFraction.ToString());
          dataList.Add(prescriptionDose.ToString());
          dataList.Add(sId);
          dataList.Add(sName);
          dataList.Add(sRGBA);
          dataList.Add(sRGB);
          dataList.Add(sType);
          dataList.Add(sVolume);
          dataList.Add(doseValuePresentation == 0 ? "Relative" : "Absolute");
          dataList.Add(volumePresentation == 0 ? "Relative" : "Absolute");
          dataList.Add(min);
          dataList.Add(min_0_03cc);
          dataList.Add(max);
          dataList.Add(max_0_03cc);
          dataList.Add(mean);
          dataList.Add(median);
          dataList.Add(stdDev);
          dataList.Add(doseCoverage);
          dataList.Add(samplingCoverage);


          foreach (var point in dvh.CurveData)
          {
            dataList.Add(Math.Round(point.Volume, 5).ToString());
          }

          // add structure data line to content string builder
          dvhCsvContent.AppendLine(string.Join(",", dataList.ToArray()));

        }

        if (useDefaultPath == true)
        {
          csvDataFilePath = BASE_DVH_EXPORT_PATH + userName;
        }
        else
        {
          var cleaned_customPath = Helpers.RemoveInvalidFileNameChars(CustomFolder_TextBox.Text);
          if (CustomFolder_TextBox.Text != cleaned_customPath) { MessageBox.Show("Invalid Custom Folder Name: Your custom path has been changed. See Summary for your new path."); }
          csvDataFilePath = BASE_DVH_EXPORT_PATH + userName + @"\\" + cleaned_customPath;
        }

        // create directory if necessary
        Directory.CreateDirectory(csvDataFilePath); // will create directories if they don't already exist
        
        // write data to file
        var fileName = string.Format("{0}_{1}_{2}.csv", patientId, courseId, planId);
        File.WriteAllText(csvDataFilePath + "\\" + fileName, dvhCsvContent.ToString());

        // structures collected data for
        var structures = string.Empty;
        foreach (var s in selectedStructures)
        {
          structures += "\t-" + s + "\r\n\t";
        }

        var msg = string.Format("Summary of data collected:\r\n\r\n\tDose Presentation: {0}\r\n\tVolume Presentation: {1}\r\n\tDose Resolution: {2}\r\n\tSelected Structures:\r\n\t{3}",
          doseValuePresentation == 0 ? "Relative" : "Absolute",
          volumePresentation == 0 ? "Relative" : "Absolute",
          bin_size,
          structures
        );

        MessageBox.Show(msg + "\r\n\r\nYour data file should be available for review here: " + csvDataFilePath);

      }
      
    }

    #endregion export data button event


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
        dataHeaderList.Add("CourseName");
        dataHeaderList.Add("StructureSet");

        string concatDataHeader = string.Join(",", dataHeaderList.ToArray());

        userLogCsvContent.AppendLine(concatDataHeader);
      }

      List<object> userStatsList = new List<object>();

      string userId = userName;
      string scriptId = script;
      string date = string.Format("{0}/{1}/{2}", day, month, year);
      string dayOfWeek = day;
      string time = string.Format("{0}:{1}", hour, minute);
      string ptId = patientId;
      string course = day;
      string structureSetId = ssId;

      userStatsList.Add(userId);
      userStatsList.Add(scriptId);
      userStatsList.Add(date);
      userStatsList.Add(dayOfWeek);
      userStatsList.Add(time);
      userStatsList.Add(ptId);
      userStatsList.Add(course);
      userStatsList.Add(structureSetId);

      string concatUserStats = string.Join(",", userStatsList.ToArray());

      userLogCsvContent.AppendLine(concatUserStats);


      #endregion User Stats

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
