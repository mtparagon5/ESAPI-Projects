namespace VMS.TPS
{
  using System;
  using System.Collections.Generic;
  using VMS.TPS.Common.Model.API;

  /// <summary>
  /// PPatient Class - tailored version of ESAPI Patient Class;
  /// </summary>
  [Serializable]
  public class PPatient
  {
    private string name;
    public string Name { get { return name; } }

    private Nullable<DateTime> dateOfBirth;
    public Nullable<DateTime> DateOfBirth { get { return dateOfBirth; } }

    private string id;
    public string Id { get { return id; } }

    private string randomId;
    public string RandomId { get { return randomId; } }

    private Hospital hospital;
    public Hospital Hospital { get { return hospital; } }

    private string primaryOncologistId;
    public string PrimaryOncologistId { get { return primaryOncologistId; } }

    private string primaryOncologistName;
    public string PrimaryOncologistName { get { return primaryOncologistName; } }

    private IEnumerable<Course> courses;
    public IEnumerable<Course> Courses { get { return courses; } }

    private IEnumerable<StructureSet> structureSets;
    public IEnumerable<StructureSet> StructureSets { get { return structureSets; } }

    private IEnumerable<Study> studies;
    public IEnumerable<Study> Studies { get { return studies; } }

    public PPatient()
    {
      name = null;
      dateOfBirth = null;
      id = null;
      randomId = null;
      hospital = null;
      primaryOncologistId = null;
      primaryOncologistName = null;
      courses = null;
      structureSets = null;
      studies = null;
    }

    /// <summary>
    /// Creates new instance of PPatient Class; tailored version of ESAPI Patient Class;
    /// </summary>
    /// <param name="context">ESAPI ScriptContext</param>
    /// <returns>PPatient</returns>
    public static PPatient CreatePatient(ScriptContext context)
    {
      PPatient p = new PPatient();
      var pp = PrimaryPhysician.GetPrimaryPhysician(context.Patient.PrimaryOncologistId.ToString());

      if (context.Patient.MiddleName != string.Empty)
      {
        p.name = string.Format("{0}, {1} {2}", context.Patient.LastName, context.Patient.FirstName, context.Patient.MiddleName[0]);
      }
      else
      {
        p.name = string.Format("{0}, {1}", context.Patient.LastName, context.Patient.FirstName);
      }
      p.dateOfBirth = context.Patient.DateOfBirth;
      p.id = context.Patient.Id;
      ProcessIdName.getRandomId(context.Patient.Id, out p.randomId);
      p.hospital = context.Patient.Hospital;
      p.primaryOncologistId = pp.Id;
      p.primaryOncologistName = pp.Name;
      p.courses = context.Patient.Courses;
      p.structureSets = context.Patient.StructureSets;
      p.studies = context.Patient.Studies;

      return p;
    }
  }
}