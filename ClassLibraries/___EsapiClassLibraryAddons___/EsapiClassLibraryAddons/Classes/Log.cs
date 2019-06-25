namespace VMS.TPS
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Text;
  using VMS.TPS.Common.Model.API;

  /// <summary>
  /// A class that can be added to projects in order to create a log
  /// </summary>
  public class Log
  {
    private string path;
    public string Path { get { return path; } set { this.path = value; } }

    private string scriptId;
    public string ScriptId { get { return scriptId; } set { this.scriptId = value; } }

    private string userId;
    public string UserId { get { return userId; } set { this.userId = value; } }

    private string userName;
    public string UserName { get { return userName; } set { this.userName = value; } }

    /// <summary>
    /// Log Object
    /// </summary>
    /// <param name="context"></param>
    /// <param name="inputPath"></param>
    /// <param name="inputScriptId"></param>
    public Log(ScriptContext context, string inputPath, string inputScriptId)
    {
      path = inputPath;
      userId = context.CurrentUser.Id;
      userName = context.CurrentUser.Name;
    }

    /// <summary>
    /// Method used to create or add to a log file given various inputs.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="patient"></param>
    /// <param name="inputPath"></param>
    /// <param name="inputScriptId"></param>
    /// <param name="dosePerFraction"></param>
    /// <param name="fractions"></param>
    public static void CreateLog(ScriptContext context, PPatient patient, string inputPath, string inputScriptId, string dosePerFraction, string fractions)
    {
      Log log = new Log(context, inputPath, inputScriptId);

      #region Variables

      StringBuilder userLogCsvContent = new StringBuilder();
      List<object> userStatsList = new List<object>();
      string date = string.Format("{0}/{1}/{2}",
                                  DateTime.Now.ToLocalTime().Day.ToString(),
                                  DateTime.Now.ToLocalTime().Month.ToString(),
                                  DateTime.Now.ToLocalTime().Year.ToString());
      string time = string.Format("{0}:{1}",
                                  DateTime.Now.ToLocalTime().Hour.ToString(),
                                  DateTime.Now.ToLocalTime().Minute.ToString());

      #endregion Variables

      #region Log Info

      // add headers if the file doesn't exist
      // list of target headers for desired dose stats
      // in this case I want to display the headers every time so i can verify which target the distance is being measured for
      // this is due to the inconsistency in target naming (PTV1/2 vs ptv45/79.2) -- these can be removed later when cleaning up the data
      if (!File.Exists(log.Path))
      {
        List<string> dataHeaderList = new List<string>();
        dataHeaderList.Add("UserId");
        dataHeaderList.Add("UserName");
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

      userStatsList.Add(log.UserId);
      userStatsList.Add(log.UserName);
      userStatsList.Add(log.ScriptId);
      userStatsList.Add(date);
      userStatsList.Add(DateTime.Now.ToLocalTime().Day.ToString());
      userStatsList.Add(time);
      userStatsList.Add(patient.Id);
      userStatsList.Add(patient.RandomId);
      userStatsList.Add(context.PlanSetup.Course.Id.ToString());
      userStatsList.Add(context.PlanSetup.Id.ToString());
      userStatsList.Add(dosePerFraction);
      userStatsList.Add(fractions);

      string concatUserStats = string.Join(",", userStatsList.ToArray());

      userLogCsvContent.AppendLine(concatUserStats);

      #endregion Target Dose Stats

      #region Write File

      File.AppendAllText(log.Path, userLogCsvContent.ToString());

      #endregion
    }
  }
}
