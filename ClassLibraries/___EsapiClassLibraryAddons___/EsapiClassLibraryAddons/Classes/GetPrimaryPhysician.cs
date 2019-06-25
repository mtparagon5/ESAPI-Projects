namespace VMS.TPS
{
  public class GetPrimary
  {
    /// <summary>
    /// Returns the Primary Physician given their Physician ID or username (which differs physician to physician).
    /// NOTE: Needs to be updated.
    /// </summary>
    /// <param name="tempPhysicianId"></param>
    /// <returns></returns>
    public static string Physician(string tempPhysicianId)
    {
      string primaryPhysician = "";
      if (tempPhysicianId == "1265536635") primaryPhysician = "Dr. Beitler";
      else if (tempPhysicianId == "1275640260") primaryPhysician = "Dr. Eng";
      else if (tempPhysicianId == "1326060773") primaryPhysician = "Dr. Curran";
      else if (tempPhysicianId == "1023301082") primaryPhysician = "Dr. Lin";
      else if (tempPhysicianId == "1144408287") primaryPhysician = "Dr. McDonald";
      else if (tempPhysicianId == "1306803051") primaryPhysician = "Dr. Esiashvili";
      else if (tempPhysicianId == "1093721029") primaryPhysician = "Dr. Godette";
      else if (tempPhysicianId == "1730353327") primaryPhysician = "Dr. Higgins";
      else if (tempPhysicianId == "1346280575") primaryPhysician = "Dr. Jani";
      else if (tempPhysicianId == "1487823654") primaryPhysician = "Dr. Khan";
      else if (tempPhysicianId == "1659387702") primaryPhysician = "Dr. Landry";
      else if (tempPhysicianId == "1750543807") primaryPhysician = "Dr. Patel";
      else if (tempPhysicianId == "1952316697") primaryPhysician = "Dr. Shu";
      else if (tempPhysicianId == "1326214479") primaryPhysician = "Dr. Torres";
      else if (tempPhysicianId == "1861629107") primaryPhysician = "Dr. Eaton";
      else if (tempPhysicianId == "bruce") primaryPhysician = "Dr. Hershatter";
      else if (tempPhysicianId == "dsyu") primaryPhysician = "Dr. Yu";
      else if (tempPhysicianId == "jwshelt") primaryPhysician = "Dr. Shelton";
      else if (tempPhysicianId == "1275710899") primaryPhysician = "Dr. Kahn";
      else if (tempPhysicianId == "1245440205") primaryPhysician = "Dr. Kirkpatrick";
      else if (tempPhysicianId == "1417391962") primaryPhysician = "Dr. Zhong";
      else if (tempPhysicianId == "n064848") primaryPhysician = "Dr. Sagar Patel";
      else if (tempPhysicianId == "1811330731") primaryPhysician = "Dr. Stokes";
      else { primaryPhysician = "Unknown"; }

      return primaryPhysician;
    }

    /// <summary>
    /// Returns the Primary Physician given their Physician ID or username (which differs physician to physician) -- utilizes out.
    /// NOTE: Needs to be updated.
    /// </summary>
    /// <param name="tempPhysicianId"></param>
    /// <param name="primaryPhysician"></param>
    public static void Physician(string tempPhysicianId, out string primaryPhysician)
    {
      if (tempPhysicianId == "1265536635") primaryPhysician = "Dr. Beitler";
      else if (tempPhysicianId == "1275640260") primaryPhysician = "Dr. Eng";
      else if (tempPhysicianId == "1326060773") primaryPhysician = "Dr. Curran";
      else if (tempPhysicianId == "1023301082") primaryPhysician = "Dr. Lin";
      else if (tempPhysicianId == "1144408287") primaryPhysician = "Dr. McDonald";
      else if (tempPhysicianId == "1306803051") primaryPhysician = "Dr. Esiashvili";
      else if (tempPhysicianId == "1093721029") primaryPhysician = "Dr. Godette";
      else if (tempPhysicianId == "1730353327") primaryPhysician = "Dr. Higgins";
      else if (tempPhysicianId == "1346280575") primaryPhysician = "Dr. Jani";
      else if (tempPhysicianId == "1487823654") primaryPhysician = "Dr. Khan";
      else if (tempPhysicianId == "1659387702") primaryPhysician = "Dr. Landry";
      else if (tempPhysicianId == "1750543807") primaryPhysician = "Dr. Patel";
      else if (tempPhysicianId == "1952316697") primaryPhysician = "Dr. Shu";
      else if (tempPhysicianId == "1326214479") primaryPhysician = "Dr. Torres";
      else if (tempPhysicianId == "1861629107") primaryPhysician = "Dr. Eaton";
      else if (tempPhysicianId == "bruce") primaryPhysician = "Dr. Hershatter";
      else if (tempPhysicianId == "dsyu") primaryPhysician = "Dr. Yu";
      else if (tempPhysicianId == "jwshelt") primaryPhysician = "Dr. Shelton";
      else if (tempPhysicianId == "1275710899") primaryPhysician = "Dr. Kahn";
      else if (tempPhysicianId == "1245440205") primaryPhysician = "Dr. Kirkpatrick";
      else if (tempPhysicianId == "1417391962") primaryPhysician = "Dr. Zhong";
      else if (tempPhysicianId == "n064848") primaryPhysician = "Dr. Sagar Patel";
      else if (tempPhysicianId == "1811330731") primaryPhysician = "Dr. Stokes";
      else primaryPhysician = "Unknown" + tempPhysicianId;
    }
  }

  #region examples

  #region primary physician

  //string tempPhysicianId = context.Patient.PrimaryOncologistId;
  //string primaryOnc = "";

  //GetPrimaryPhysician.primaryPhysician(tempPhysicianId, out primaryOnc);

  //MessageBox.Show("Primary Physician:\t" + primaryOnc);

  #endregion

  #endregion
}
