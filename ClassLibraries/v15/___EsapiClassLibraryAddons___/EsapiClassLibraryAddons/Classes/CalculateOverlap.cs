namespace VMS.TPS
{
  using System.Windows.Media.Media3D;
  using VMS.TPS.Common.Model.API;
  using VMS.TPS.Common.Model.Types;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  public class CalculateOverlap
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
      return volumeIntersection;
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
}
