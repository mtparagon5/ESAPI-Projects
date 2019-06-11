using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Media.Media3D;

namespace VMS.TPS
{
  #region classes

  public static class CalculateOverlap
  {
    /// <summary>
    /// Calculate volume overlap of Structure1 with Structure2
    /// </summary>
    /// <param name="structure1"></param>
    /// <param name="structure2"></param>
    /// <returns></returns>
    public static double VolumeOverlap(Structure structure1, Structure structure2)
    {
      // initialize items needed for calculating distance
      VVector p = new VVector();
      double volumeIntersection = 0;
      int intersectionCount = 0;
      if (structure1.Id.ToLower().Contains("body") || structure1.Id.ToLower().Contains("external"))
      {
        volumeIntersection = structure2.Volume;
      }
      else if (structure2.Id.ToLower().Contains("body") || structure2.Id.ToLower().Contains("external"))
      {
        volumeIntersection = structure1.Volume;
      }
      else if ((structure1.Id.ToLower().Contains("skin") && !structure2.Id.ToLower().Contains("cavity")) ||
        (structure2.Id.ToLower().Contains("skin") && !structure2.Id.ToLower().Contains("cavity")))
      {
        volumeIntersection = Double.NaN;
      }
      else if (structure1.Id.Equals(structure2.Id))
      {
        volumeIntersection = structure1.Volume;
      }
      // using the bounds of each rectangle as the ROI for calculating overlap
      else
      {
        Rect3D structure1Bounds = structure1.MeshGeometry.Bounds;
        Rect3D structure2Bounds = structure2.MeshGeometry.Bounds;
        //Rect3D combinedRectBounds = Rect3D.Union(structure1Bounds, structure2Bounds); NOTE: slower performance
        Rect3D combinedRectBounds = Rect3D.Intersect(structure1Bounds, structure2Bounds);
        if (combinedRectBounds == null) { return 0; }

        // to allow the resolution to be on the same scale in each direction
        double startZ = Math.Floor(combinedRectBounds.Z - 1);
        double endZ = (startZ + Math.Round(combinedRectBounds.SizeZ + 2));
        double startX = Math.Floor(combinedRectBounds.X - 1);
        double endX = (startX + Math.Round(combinedRectBounds.SizeX + 2));
        double startY = Math.Floor(combinedRectBounds.Y - 1);
        double endY = (startY + Math.Round(combinedRectBounds.SizeY + 2));
        for (double z = startZ; z < endZ; z += .5)
        {
          //planDose.GetVoxels(z, dosePlaneVoxels);
          for (double y = startY; y < endY; y += 1)
          {
            for (double x = startX; x < endX; x += 1)
            {
              p.x = x;
              p.y = y;
              p.z = z;

              if ((structure2Bounds.Contains(p.x, p.y, p.z)) &&
                  (structure1.IsPointInsideSegment(p)) &&
                  (structure2.IsPointInsideSegment(p)))
              {
                intersectionCount++;
              }
              volumeIntersection = (intersectionCount * 0.001 * .5);
            }
          }
        }
      }
      return Math.Round(volumeIntersection, 3);
    }
    /// <summary>
    /// Returns the percent overlap of a structure and its provided volume of overlap
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="volumeIntersection"></param>
    /// <returns></returns>
    public static double PercentOverlap(Structure structure, double volumeIntersection)
    {
      double percentOverlap = Math.Round((volumeIntersection / structure.Volume) * 100, 1);
      if (percentOverlap > 100)
      {
        percentOverlap = 100;
        return percentOverlap;
      }
      else
      {
        return percentOverlap;
      }
    }
    /// <summary>
    /// Calculates the overlap of two given structures, then returns their Dice Coefficient, or measure of similarity.
    /// </summary>
    /// <param name="structure1"></param>
    /// <param name="structure2"></param>
    /// <returns></returns>
    public static double DiceCoefficient(Structure structure1, Structure structure2)
    {
      // initialize items needed for calculating distance
      VVector p = new VVector();
      double volumeIntersection = 0;
      double volumeStructure1 = 0;
      double volumeStructure2 = 0;
      int intersectionCount = 0;
      int structure1Count = 0;
      int structure2Count = 0;
      double diceCoefficient = 0;

      Rect3D structure1Bounds = structure1.MeshGeometry.Bounds;
      Rect3D structure2Bounds = structure2.MeshGeometry.Bounds;
      //Rect3D combinedRectBounds = Rect3D.Union(structure1Bounds, structure2Bounds);
      Rect3D combinedRectBounds = Rect3D.Intersect(structure1Bounds, structure2Bounds);
      // to allow the resolution to be on the same scale in each direction
      double startZ = Math.Floor(combinedRectBounds.Z - 1);
      double endZ = (startZ + Math.Round(combinedRectBounds.SizeZ + 2));
      double startX = Math.Floor(combinedRectBounds.X - 1);
      double endX = (startX + Math.Round(combinedRectBounds.SizeX + 2));
      double startY = Math.Floor(combinedRectBounds.Y - 1);
      double endY = (startY + Math.Round(combinedRectBounds.SizeY + 2));

      if (structure1 != structure2)
      {

        if (structure1Bounds.Contains(structure2Bounds))
        {
          volumeIntersection = structure2.Volume;
          volumeStructure1 = structure1.Volume;
          volumeStructure2 = structure2.Volume;
        }
        else if (structure2Bounds.Contains(structure1Bounds))
        {
          volumeIntersection = structure1.Volume;
          volumeStructure1 = structure1.Volume;
          volumeStructure2 = structure2.Volume;
        }
        else
        {
          // using the bounds of each rectangle as the ROI for calculating overlap
          for (double z = startZ; z < endZ; z += .5)
          {
            for (double y = startY; y < endY; y += 1)
            {
              for (double x = startX; x < endX; x += 1)
              {
                p.x = x;
                p.y = y;
                p.z = z;

                if ((structure2Bounds.Contains(p.x, p.y, p.z)) &&
                    (structure1.IsPointInsideSegment(p)) &&
                    (structure2.IsPointInsideSegment(p)))
                {
                  intersectionCount++;
                }
                if (structure1.IsPointInsideSegment(p))
                {
                  structure1Count++;
                }
                if (structure2.IsPointInsideSegment(p))
                {
                  structure2Count++;
                }
                volumeIntersection = (intersectionCount * 0.001 * .5);
                volumeStructure1 = (structure1Count * 0.001 * .5);
                volumeStructure2 = (structure2Count * 0.001 * .5);
              }
            }
          }
        }
        diceCoefficient = Math.Round(((2 * volumeIntersection) / (volumeStructure1 + volumeStructure2)), 3);
        return diceCoefficient;
      }
      else
      {
        diceCoefficient = 1;
        return diceCoefficient;
      }
    }
    /// <summary>
    /// Returns the Dice Coefficient, or measure of similarity, of two structures, given their previously calculated volume of overlap.
    /// </summary>
    /// <param name="structure1"></param>
    /// <param name="structure2"></param>
    /// <param name="volumeOverlap"></param>
    /// <returns></returns>
    public static double DiceCoefficient(Structure structure1, Structure structure2, double volumeOverlap)
    {
      return Math.Round((2 * volumeOverlap) / (structure1.Volume + structure2.Volume), 3);
    }

    /// <summary>
    /// Calculates the shortest distance between structure 1 and structure 2
    /// </summary>
    /// <param name="structure1"></param>
    /// <param name="structure2"></param>
    /// <returns></returns>
    public static double ShortestDistance(Structure structure1, Structure structure2)
    {
      double shortestDistance = 2000000;
      if (structure1 != structure2 && !structure1.Id.ToLower().Contains("body") && !structure1.Id.ToLower().Contains("body") &&
          !structure1.Id.ToLower().Contains("external") && !structure2.Id.ToLower().Contains("external") &&
          !structure2.Id.ToLower().Contains("skin"))
      {
        Rect3D structure1Bounds = structure1.MeshGeometry.Bounds;
        Rect3D structure2Bounds = structure2.MeshGeometry.Bounds;
        //Rect3D combinedRectBounds = Rect3D.Intersect(structure1Bounds, structure2Bounds);
        //if (combinedRectBounds != null) { return 0; }
        //if (structure1Bounds.Contains(structure2Bounds))
        //{
        //    shortestDistance = 0;
        //    return shortestDistance;
        //}
        //else if (structure2Bounds.Contains(structure1Bounds))
        //{
        //    shortestDistance = 0;
        //    return shortestDistance;
        //}
        //else
        //{
        // calculate the shortest distance between each structure
        Point3DCollection vertexesStructure1 = new Point3DCollection();
        Point3DCollection vertexesStructure2 = new Point3DCollection();
        vertexesStructure1 = structure1.MeshGeometry.Positions;
        vertexesStructure2 = structure2.MeshGeometry.Positions;
        foreach (Point3D v1 in vertexesStructure1)
        {
          foreach (Point3D v2 in vertexesStructure2)
          {
            double distance = (Math.Sqrt((Math.Pow((v2.X - v1.X), 2)) + (Math.Pow((v2.Y - v1.Y), 2)) + (Math.Pow((v2.Z - v1.Z), 2)))) / 10;
            if (distance < shortestDistance)
            {
              shortestDistance = distance;
            }
          }
        }
        return shortestDistance;
        //}
      }
      else
      {
        shortestDistance = 0;
        return shortestDistance;
      }
    }

    #region May not be useful

    /// <summary>
    /// Calculates the furthest distance between any two points of two structures. (Admittedly not very useful).
    /// </summary>
    /// <param name="structure1"></param>
    /// <param name="structure2"></param>
    /// <returns></returns>
    public static double MaxDistance(Structure structure1, Structure structure2)
    {
      // calculate the max distance between each structure
      Rect3D structure1Bounds = structure1.MeshGeometry.Bounds;
      Rect3D structure2Bounds = structure2.MeshGeometry.Bounds;
      Point3DCollection vertexesStructure1 = new Point3DCollection();
      Point3DCollection vertexesStructure2 = new Point3DCollection();
      vertexesStructure1 = structure1.MeshGeometry.Positions;
      vertexesStructure2 = structure2.MeshGeometry.Positions;
      double maxDistance = 0;

      if (structure1Bounds.Contains(structure2Bounds))
      {
        maxDistance = 0;
        return maxDistance;
      }
      else if (structure2Bounds.Contains(structure1Bounds))
      {
        maxDistance = 0;
        return maxDistance;
      }
      else
      {
        foreach (Point3D v1 in vertexesStructure1)
        {
          foreach (Point3D v2 in vertexesStructure2)
          {
            double distance = (Math.Sqrt((Math.Pow((v2.X - v1.X), 2)) + (Math.Pow((v2.Y - v1.Y), 2)) + (Math.Pow((v2.Z - v1.Z), 2)))) / 10;
            if (distance > maxDistance)
            {
              maxDistance = distance;
            }
          }
        }
        return maxDistance;
      }
    }

    /// <summary>
    /// Calculates the average distance of all points of two structures inside a given radius
    /// </summary>
    /// <param name="structure1"></param>
    /// <param name="structure2"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public static double AverageDistance_InsideRadius(Structure structure1, Structure structure2, double radius)
    {
      // calculate the average distance between each structure inside a designated radius
      List<double> pointsInsideRadius = new List<double>();
      Rect3D structure1Bounds = structure1.MeshGeometry.Bounds;
      Rect3D structure2Bounds = structure2.MeshGeometry.Bounds;
      Point3DCollection vertexesStructure1 = new Point3DCollection();
      Point3DCollection vertexesStructure2 = new Point3DCollection();
      vertexesStructure1 = structure1.MeshGeometry.Positions;
      vertexesStructure2 = structure2.MeshGeometry.Positions;
      double averageDistance = 0;

      if (structure1Bounds.Contains(structure2Bounds))
      {
        averageDistance = 0;
        return averageDistance;
      }
      else if (structure2Bounds.Contains(structure1Bounds))
      {
        averageDistance = 0;
        return averageDistance;
      }
      else
      {
        foreach (Point3D v1 in vertexesStructure1)
        {
          foreach (Point3D v2 in vertexesStructure2)
          {
            double distance = (Math.Sqrt((Math.Pow((v2.X - v1.X), 2)) + (Math.Pow((v2.Y - v1.Y), 2)) + (Math.Pow((v2.Z - v1.Z), 2)))) / 10;
            if (distance <= radius)
            {
              pointsInsideRadius.Add(distance);
            }
          }
        }
        if (pointsInsideRadius.Count == 0)
        {
          averageDistance = 0;
        }
        else
        {
          averageDistance = pointsInsideRadius.Average();
        }
        return averageDistance;
      }
    }
    /// <summary>
    /// Calculates the average distance of all points of two structures outside a given radius
    /// </summary>
    /// <param name="structure1"></param>
    /// <param name="structure2"></param>
    /// <param name="radius"></param>
    /// <returns></returns>
    public static double AverageDistance_OutsideRadius(Structure structure1, Structure structure2, double radius)
    {
      List<double> pointsOutsideRadius = new List<double>();
      Rect3D structure1Bounds = structure1.MeshGeometry.Bounds;
      Rect3D structure2Bounds = structure2.MeshGeometry.Bounds;
      // calculate the average distance between each structure outside a designated radius
      Point3DCollection vertexesStructure1 = new Point3DCollection();
      Point3DCollection vertexesStructure2 = new Point3DCollection();
      vertexesStructure1 = structure1.MeshGeometry.Positions;
      vertexesStructure2 = structure2.MeshGeometry.Positions;
      double averageDistance = 0;

      if (structure1Bounds.Contains(structure2Bounds))
      {
        averageDistance = 0;
        return averageDistance;
      }
      else if (structure2Bounds.Contains(structure1Bounds))
      {
        averageDistance = 0;
        return averageDistance;
      }
      else
      {
        foreach (Point3D v1 in vertexesStructure1)
        {
          //VVector p1 = new VVector();
          foreach (Point3D v2 in vertexesStructure2)
          {
            double distance = (Math.Sqrt((Math.Pow((v2.X - v1.X), 2)) + (Math.Pow((v2.Y - v1.Y), 2)) + (Math.Pow((v2.Z - v1.Z), 2)))) / 10;
            if (distance > radius)
            {
              pointsOutsideRadius.Add(distance);
            }
          }
        }
        if (pointsOutsideRadius.Count == 0)
        {
          averageDistance = 0;
        }
        else
        {
          averageDistance = pointsOutsideRadius.Average();
        }
        return averageDistance;
      }
    }
    #endregion avg distances
  }

  #endregion classes
  public class Script
  {
    public Script() { }

    public void Execute(ScriptContext context /*, System.Windows.Window window*/ )
    {

      #region context variable definitions

      // to work for plan sum
      StructureSet structureSet;
      PlanningItem selectedPlanningItem;
      PlanSetup planSetup;
      PlanSum psum = null;
      //double fractions = 0;
      //string status = "";
      if (context.PlanSetup == null && context.PlanSumsInScope.Count() > 1)
      {
        throw new ApplicationException("Please close other plan sums");
      }
      if (context.PlanSetup == null)
      {
        psum = context.PlanSumsInScope.First();
        planSetup = psum.PlanSetups.First();
        selectedPlanningItem = (PlanningItem)psum;
        structureSet = psum.StructureSet;
        //fractions = DvhExtensions.getTotalFractionsForPlanSum(psum);
        //status = "PlanSum";

      }
      else
      {
        planSetup = context.PlanSetup;
        selectedPlanningItem = (PlanningItem)planSetup;
        structureSet = planSetup.StructureSet;
        //if (planSetup.UniqueFractionation.NumberOfFractions != null)
        //{
        //	fractions = (double)planSetup.UniqueFractionation.NumberOfFractions;
        //}
        //status = planSetup.ApprovalStatus.ToString();

      }
      //var dosePerFx = planSetup.UniqueFractionation.PrescribedDosePerFraction.Dose;
      //var rxDose = (double)(dosePerFx * fractions);

      //structureSet = planSetup != null  planSetup.StructureSet : psum.StructureSet;/*psum.PlanSetups.Last().StructureSet;*/ // changed from first to last in case it's broken on next build
      //string pId = context.Patient.Id;
      //string course = context.Course.Id.ToString().Replace(" ", "_"); ;
      //string pName = ProcessIdName.processPtName(context.Patient.Name);

      #endregion

      #region organize structures into ordered lists
      // lists for structures
      List<Structure> gtvList = new List<Structure>();
      List<Structure> ctvList = new List<Structure>();
      List<Structure> itvList = new List<Structure>();
      List<Structure> ptvList = new List<Structure>();
      List<Structure> oarList = new List<Structure>();
      List<Structure> targetList = new List<Structure>();
      List<Structure> structureList = new List<Structure>();

      IEnumerable<Structure> sorted_gtvList = new List<Structure>();
      IEnumerable<Structure> sorted_ctvList = new List<Structure>();
      IEnumerable<Structure> sorted_itvList = new List<Structure>();
      IEnumerable<Structure> sorted_ptvList = new List<Structure>();
      IEnumerable<Structure> sorted_oarList = new List<Structure>();
      IEnumerable<Structure> sorted_targetList = new List<Structure>();
      IEnumerable<Structure> sorted_structureList = new List<Structure>();

      foreach (var structure in structureSet.Structures)
      {
        // conditions for adding any structure
        if ((!structure.IsEmpty) &&
            (structure.HasSegment) &&
            (!structure.Id.Contains("*")) &&
            (!structure.Id.ToLower().Contains("markers")) &&
            (!structure.Id.ToLower().Contains("avoid")) &&
            (!structure.Id.ToLower().Contains("dose")) &&
            (!structure.Id.ToLower().Contains("contrast")) &&
            (!structure.Id.ToLower().Contains("air")) &&
            (!structure.Id.ToLower().Contains("dens")) &&
            (!structure.Id.ToLower().Contains("bolus")) &&
            (!structure.Id.ToLower().Contains("suv")) &&
            (!structure.Id.ToLower().Contains("match")) &&
            (!structure.Id.ToLower().Contains("wire")) &&
            (!structure.Id.ToLower().Contains("scar")) &&
            (!structure.Id.ToLower().Contains("chemo")) &&
            (!structure.Id.ToLower().Contains("pet")) &&
            (!structure.Id.ToLower().Contains("dnu")) &&
            (!structure.Id.ToLower().Contains("fiducial")) &&
            (!structure.Id.ToLower().Contains("artifact")) &&
            (!structure.Id.StartsWith("z", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith("hs", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith("av", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith("opti ", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith("opti-", StringComparison.InvariantCultureIgnoreCase)))
        //(structure.Id.Contains("CI-", StringComparison.InvariantCultureIgnoreCase) == false) && 
        //(structure.Id.Contains("R50-", StringComparison.InvariantCultureIgnoreCase) == false) &&
        //(structure.Id.Contains("CI_", StringComparison.InvariantCultureIgnoreCase) == false) && 
        //(structure.Id.Contains("R50_", StringComparison.InvariantCultureIgnoreCase) == false))
        {
          if (structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase))
          {
            gtvList.Add(structure);
            structureList.Add(structure);
            targetList.Add(structure);
          }
          if ((structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) ||
              (structure.Id.StartsWith("Prost", StringComparison.InvariantCultureIgnoreCase)))
          {
            ctvList.Add(structure);
            structureList.Add(structure);
            targetList.Add(structure);
          }
          if (structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase))
          {
            itvList.Add(structure);
            structureList.Add(structure);
            targetList.Add(structure);
          }
          if (structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase))
          {
            ptvList.Add(structure);
            structureList.Add(structure);
            targetList.Add(structure);
          }
          // conditions for adding breast plan targets
          if ((structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) ||
              (structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) ||
              (structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) ||
              (structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)))
          {
            targetList.Add(structure);
            structureList.Add(structure);
          }
          // conditions for adding oars
          if ((!structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("Scar", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.ToLower().Contains("carina")))
          {
            oarList.Add(structure);
            structureList.Add(structure);
          }
        }
      }
      sorted_gtvList = gtvList.OrderBy(x => x.Id);
      sorted_ctvList = ctvList.OrderBy(x => x.Id);
      sorted_itvList = itvList.OrderBy(x => x.Id);
      sorted_ptvList = ptvList.OrderBy(x => x.Id);
      sorted_targetList = targetList.OrderBy(x => x.Id);
      sorted_oarList = oarList.OrderBy(x => x.Id);
      sorted_structureList = structureList.OrderBy(x => x.Id);

      #endregion structure organization and ordering


      var result = string.Empty;
      var boxTitle = string.Empty;

      var messages = new List<string>();
      var sMsg = string.Empty;

      var sOverlapAbs = double.NaN;
      var sOverlapPct = double.NaN;


      var willCalcOverlap = false;

      if (MessageBox.Show("Calculate Target/OAR overlap?", "Calculate Overlap?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
      {
        willCalcOverlap = true;
      }
      else
      {
        willCalcOverlap = false;
      }

      if (willCalcOverlap)
      {
        boxTitle = "Overlap Stats";
        sMsg += "Overlap Stats\r\n---------------------\r\n";
        foreach (var s in sorted_oarList)
        {
          sMsg = string.Format("{0}:", s.Id);
          foreach (var t in sorted_ptvList)
          {
            sOverlapAbs = CalculateOverlap.VolumeOverlap(s, t);
            sOverlapPct = CalculateOverlap.PercentOverlap(s, sOverlapAbs);

            sMsg += string.Format("\r\n\tOverlap with {0}:\t{1} cc\t({2} %)", t.Id, sOverlapAbs, sOverlapPct);
          }
          sMsg += "\r\n---------------------\r\n";
          messages.Add(sMsg);
        }

        foreach (var msg in messages)
        {
          result += msg + "\r\n";
        }
      }
      else
      {
        boxTitle = "We're Usless";
        result = "Sorry we couldn't help you :(";
      }

      MessageBox.Show(result, boxTitle);
    }
  }
}
