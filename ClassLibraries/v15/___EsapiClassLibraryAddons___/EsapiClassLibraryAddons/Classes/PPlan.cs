namespace VMS.TPS
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Windows;
  using VMS.TPS.Common.Model.API;
  using VMS.TPS.Common.Model.Types;

  /// <summary>
  /// Plan Class - tailored version of ESAPI Patient Class;
  /// </summary>
  [Serializable]
  public class PPlan
  {
    private string id;
    public string Id { get { return id; } }

    private string type;
    public string Type { get { return type; } }

    private PlanningItem pItem;
    public PlanningItem PItem { get { return pItem; } }

    private PlanSetup pSetup;
    public PlanSetup PSetup { get { return pSetup; } }

    private PlanSum pSum;
    public PlanSum PSum { get { return pSum; } }

    private IEnumerable<PlanSetup> pSetups;
    public IEnumerable<PlanSetup> PSetups { get { return pSetups; } }

    private StructureSet structureSet;
    public StructureSet StructureSet { get { return structureSet; } }

    private List<StructureSet> structureSets;
    public List<StructureSet> StructureSets { get { return structureSets; } }

    private double fractions;
    public double Fractions { get { return fractions; } }

    private List<Tuple<string, Tuple<int, double>>> planFractionList;
    public List<Tuple<string, Tuple<int, double>>> PlanFractionList { get { return planFractionList; } }

    private IEnumerable<Beam> beams;
    public IEnumerable<Beam> Beams { get { return beams; } }

    private List<Tuple<string, List<Beam>>> beamsList;
    public List<Tuple<string, List<Beam>>> BeamsList { get { return beamsList; } }

    private IEnumerable<string> seriesList;
    public IEnumerable<string> SeriesList { get { return seriesList; } }

    private PlanSetupApprovalStatus approvalStatus;
    public PlanSetupApprovalStatus ApprovalStatus { get { return approvalStatus; } }

    private Course course;
    public Course Course { get { return course; } }

    private double maxDose;
    public double MaxDose { get { return maxDose; } }

    private double dosePerFraction;
    public double DosePerFraction { get { return dosePerFraction; } }

    private double rxDose;
    public double RxDose { get { return rxDose; } }

    public PPlan()
    {
      id = null;
      type = null;
      id = null;
      pItem = null;
      pSetup = null;
      structureSet = null;
      fractions = Double.NaN;
      beams = null;
      course = null;
      maxDose = Double.NaN;
      dosePerFraction = Double.NaN;
      rxDose = Double.NaN;
    }

    /// <summary>
    /// Creates new instance of PPlan Class; tailored version of ESAPI Plan Class;
    /// </summary>
    /// <param name="plan">PlanSetup</param>
    /// <returns>PPlan</returns>
    public static PPlan CreatePPlan(PlanSetup plan)
    {
      PPlan p = new PPlan();

      //         if (context.PlanSetup == null)
      //{
      //	p.type = "PlanSum";
      //	PlanSum psum = context.PlanSumsInScope.First();
      //	p.pSetup = psum.PlanSetups.First();
      //	p.pItem = (PlanningItem)psum;
      //	p.structureSet = context.StructureSet;
      //	p.id = psum.Id.ToString().Replace(" ", "_").Replace(":", "_");
      //	p.fractions = DvhExtensions.getTotalFractionsForPlanSum(psum);
      //}

      //else
      //{
      p.type = "Plan";
      p.pSetup = plan;
      p.pItem = (PlanningItem)plan;
      p.structureSet = plan.StructureSet;
      p.id = plan.Id.ToString().Replace(" ", "_").Replace(":", "_");
      if (plan.NumberOfFractions != null)
      {
        p.fractions = (double)plan.NumberOfFractions;
      }
      //}

      try
      {
        plan.DoseValuePresentation = DoseValuePresentation.Absolute;
        if (plan.Dose != null)
        {
          p.maxDose = Math.Round(plan.Dose.DoseMax3D.Dose, 5);
        }
        else { p.maxDose = Double.NaN; }
        p.dosePerFraction = plan.DosePerFraction.Dose;
        p.rxDose = (double)(p.dosePerFraction * p.fractions);
      }
      catch
      {
        MessageBox.Show("Cannot determine Dose per Fraction or Number of Fractions", "Plan Setup Error");
      }

      p.course = plan.Course;
      p.approvalStatus = plan.ApprovalStatus;
      p.beams = plan.Beams.Where(x => !x.IsSetupField);

      return p;
    }

    /// <summary>
    /// Creates new instance of PPlan Class; tailored version of ESAPI Plan Class;
    /// </summary>
    /// <param name="context"></param>
    /// <param name="plan"></param>
    /// <returns></returns>
    public static PPlan CreatePPlan(ScriptContext context, PlanSetup plan)
    {
      PPlan p = new PPlan();

      //         if (context.PlanSetup == null)
      //{
      //	p.type = "PlanSum";
      //	PlanSum psum = context.PlanSumsInScope.First();
      //	p.pSetup = psum.PlanSetups.First();
      //	p.pItem = (PlanningItem)psum;
      //	p.structureSet = context.StructureSet;
      //	p.id = psum.Id.ToString().Replace(" ", "_").Replace(":", "_");
      //	p.fractions = DvhExtensions.getTotalFractionsForPlanSum(psum);
      //}

      //else
      //{
      p.type = "Plan";
      p.pSetup = plan;
      p.pItem = (PlanningItem)plan;
      p.structureSet = plan.StructureSet;
      p.id = plan.Id.ToString().Replace(" ", "_").Replace(":", "_");
      if (plan.NumberOfFractions != null)
      {
        p.fractions = (double)plan.NumberOfFractions;
      }
      //}

      try
      {
        plan.DoseValuePresentation = DoseValuePresentation.Absolute;
        if (plan.Dose != null)
        {
          p.maxDose = Math.Round(plan.Dose.DoseMax3D.Dose, 5);
        }
        else { p.maxDose = Double.NaN; }
        p.dosePerFraction = plan.DosePerFraction.Dose;
        p.rxDose = (double)(p.dosePerFraction * p.fractions);
      }
      catch
      {
        MessageBox.Show("Cannot determine Dose per Fraction or Number of Fractions", "Plan Setup Error");
      }

      p.course = plan.Course;
      p.approvalStatus = plan.ApprovalStatus;
      p.beams = plan.Beams.Where(x => !x.IsSetupField);

      return p;
    }

    /// <summary>
    /// Creates new instance of PPlan Class; tailored version of ESAPI Plan Class;
    /// </summary>
    /// <param name="sum">PlanSum</param>
    /// <returns>PPlan</returns>
    public static PPlan CreatePPlan(PlanSum sum)
    {
      PPlan p = new PPlan();
      var structureSets = new List<StructureSet>();
      var planFractions = new List<Tuple<string, Tuple<int, double>>>();
      var beamsList = new List<Tuple<string, List<Beam>>>();
      var seriesList = new List<string>();

      //         if (context.PlanSetup == null)
      //{
      p.type = "PlanSum";
      PlanSum psum = (PlanSum)sum;
      p.pSum = psum;
      p.pSetup = psum.PlanSetups.First();
      p.pSetups = psum.PlanSetups;
      p.pItem = (PlanningItem)psum;
      p.structureSet = p.pSetup.StructureSet;
      p.id = psum.Id.ToString().Replace(" ", "_").Replace(":", "_");
      p.fractions = DvhExtensions.getTotalFractionsForPlanSum(psum);
      p.course = p.pSetup.Course;
      p.approvalStatus = PlanSetupApprovalStatus.Unknown;

      foreach (var ps in p.pSetups)
      {
        structureSets.Add(ps.StructureSet);
        var fxTuple = Tuple.Create((int)ps.NumberOfFractions, ps.DosePerFraction.Dose);
        planFractions.Add(Tuple.Create(ps.Id.ToString().Replace(" ", "_").Replace(":", "_"), fxTuple));
        //foreach(var beam in ps.Beams)
        //{
        //    beamsList.Add(beam);
        //}
        beamsList.Add(Tuple.Create(ps.Id.ToString().Replace(" ", "_"), ps.Beams.Where(x => !x.IsSetupField).ToList()));
        if (!seriesList.Contains(ps.Series.Id.ToString().Replace(" ", "_")))
        {
          seriesList.Add(ps.Series.Id.ToString().Replace(" ", "_"));
        }
      }
      p.structureSets = structureSets;
      p.planFractionList = planFractions;
      p.beamsList = beamsList;
      p.seriesList = seriesList;

      try
      {
        p.pSetup.DoseValuePresentation = DoseValuePresentation.Absolute;
        if (p.pSetup.Dose != null)
        {
          p.maxDose = Math.Round(psum.Dose.DoseMax3D.Dose, 5);
        }
        else { p.maxDose = Double.NaN; }
        //p.dosePerFraction = p.pSetup.DosePerFraction.Dose;
        //p.rxDose = (double)(p.dosePerFraction * p.fractions);
      }
      catch
      {
        MessageBox.Show("Cannot determine Dose per Fraction or Number of Fractions", "Plan Setup Error");
      }


      return p;
    }

    /// <summary>
    /// Creates new instance of PPlan Class; tailored version of ESAPI Plan Class;
    /// </summary>
    /// <param name="context"></param>
    /// <param name="sum"></param>
    /// <returns></returns>
    public static PPlan CreatePPlan(ScriptContext context, PlanSum sum)
    {
      PPlan p = new PPlan();
      var structureSets = new List<StructureSet>();
      var planFractions = new List<Tuple<string, Tuple<int, double>>>();
      var beamsList = new List<Tuple<string, List<Beam>>>();
      var seriesList = new List<string>();

      //         if (context.PlanSetup == null)
      //{
      p.type = "PlanSum";
      PlanSum psum = (PlanSum)sum;
      p.pSum = psum;
      p.pSetup = psum.PlanSetups.First();
      p.pSetups = psum.PlanSetups;
      p.pItem = (PlanningItem)psum;
      p.structureSet = p.pSetup.StructureSet;
      p.id = psum.Id.ToString().Replace(" ", "_").Replace(":", "_");
      p.fractions = DvhExtensions.getTotalFractionsForPlanSum(psum);
      p.course = p.pSetup.Course;
      p.approvalStatus = PlanSetupApprovalStatus.Unknown;

      foreach (var ps in p.pSetups)
      {
        structureSets.Add(ps.StructureSet);
        var fxTuple = Tuple.Create((int)ps.NumberOfFractions, ps.DosePerFraction.Dose);
        planFractions.Add(Tuple.Create(ps.Id.ToString().Replace(" ", "_").Replace(":", "_"), fxTuple));
        //foreach(var beam in ps.Beams)
        //{
        //    beamsList.Add(beam);
        //}
        beamsList.Add(Tuple.Create(ps.Id.ToString().Replace(" ", "_"), ps.Beams.Where(x => !x.IsSetupField).ToList()));
        if (!seriesList.Contains(ps.Series.Id.ToString().Replace(" ", "_")))
        {
          seriesList.Add(ps.Series.Id.ToString().Replace(" ", "_"));
        }
      }
      p.structureSets = structureSets;
      p.planFractionList = planFractions;
      p.beamsList = beamsList;
      p.seriesList = seriesList;

      try
      {
        p.pSetup.DoseValuePresentation = DoseValuePresentation.Absolute;
        if (p.pSetup.Dose != null)
        {
          p.maxDose = Math.Round(psum.Dose.DoseMax3D.Dose, 5);
        }
        else { p.maxDose = Double.NaN; }
        //p.dosePerFraction = p.pSetup.DosePerFraction.Dose;
        //p.rxDose = (double)(p.dosePerFraction * p.fractions);
      }
      catch
      {
        MessageBox.Show("Cannot determine Dose per Fraction or Number of Fractions", "Plan Setup Error");
      }


      return p;
    }


  }
}
