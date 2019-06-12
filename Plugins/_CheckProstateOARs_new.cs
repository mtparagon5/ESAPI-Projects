using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace VMS.TPS {
  #region classes

  public static class DvhExtensions {
    public static DoseValue GetDoseAtVolume (this PlanningItem pitem, Structure structure, double volume, VolumePresentation volumePresentation, DoseValuePresentation requestedDosePresentation) {
      if (pitem is PlanSetup) {
        return ((PlanSetup) pitem).GetDoseAtVolume (structure, volume, volumePresentation, requestedDosePresentation);
      } else {
        if (requestedDosePresentation != DoseValuePresentation.Absolute)
          throw new ApplicationException ("Only absolute dose supported for Plan Sums");
        DVHData dvh = pitem.GetDVHCumulativeData (structure, DoseValuePresentation.Absolute, volumePresentation, 0.001);
        return DvhExtensions.DoseAtVolume (dvh, volume);
      }
    }
    public static double GetVolumeAtDose (this PlanningItem pitem, Structure structure, DoseValue dose, VolumePresentation requestedVolumePresentation) {
      if (pitem is PlanSetup) {
        return ((PlanSetup) pitem).GetVolumeAtDose (structure, dose, requestedVolumePresentation);
      } else {
        DVHData dvh = pitem.GetDVHCumulativeData (structure, DoseValuePresentation.Absolute, requestedVolumePresentation, 0.001);
        return DvhExtensions.VolumeAtDose (dvh, dose.Dose);
      }
    }

    public static DoseValue DoseAtVolume (DVHData dvhData, double volume) {
      if (dvhData == null || dvhData.CurveData.Count () == 0)
        return DoseValue.UndefinedDose ();
      double absVolume = dvhData.CurveData[0].VolumeUnit == "%" ? volume * dvhData.Volume * 0.01 : volume;
      if (volume < 0.0 /*|| absVolume > dvhData.Volume*/ )
        return DoseValue.UndefinedDose ();

      DVHPoint[] hist = dvhData.CurveData;
      for (int i = 0; i < hist.Length; i++) {
        if (hist[i].Volume < volume)
          return hist[i].DoseValue;
      }
      return DoseValue.UndefinedDose ();
    }

    public static double VolumeAtDose (DVHData dvhData, double dose) {
      if (dvhData == null)
        return Double.NaN;

      DVHPoint[] hist = dvhData.CurveData;
      int index = (int) (hist.Length * dose / dvhData.MaxDose.Dose);
      if (index < 0 || index > hist.Length)
        return 0.0; //Double.NaN;
      else
        return hist[index].Volume;
    }
    public static double getDoseAtVolume (DVHData dvh, double VolumeLim) {
      for (int i = 0; i < dvh.CurveData.Length; i++) {
        if (dvh.CurveData[i].Volume <= VolumeLim) {
          return dvh.CurveData[i].DoseValue.Dose;
        }
      }
      return 0;
    }
    public static double getVolumeAtDose (DVHData dvh, double DoseLim) {
      for (int i = 0; i < dvh.CurveData.Length; i++) {
        if (dvh.CurveData[i].DoseValue.Dose >= DoseLim) {
          return dvh.CurveData[i].Volume;

        }
      }
      return 0;
    }
  }

  public static class CalculateOverlap {

    /// <summary>
    /// Calculate volume overlap of Structure1 with Structure2
    /// </summary>
    /// <param name="structure1"></param>
    /// <param name="structure2"></param>
    /// <returns></returns>
    public static double VolumeOverlap (Structure structure1, Structure structure2) {
      // initialize items needed for calculating distance
      VVector p = new VVector ();
      double volumeIntersection = 0;
      int intersectionCount = 0;
      if (structure1.Id.ToLower ().Contains ("body") || structure1.Id.ToLower ().Contains ("external")) {
        volumeIntersection = structure2.Volume;
      } else if (structure2.Id.ToLower ().Contains ("body") || structure2.Id.ToLower ().Contains ("external")) {
        volumeIntersection = structure1.Volume;
      } else if ((structure1.Id.ToLower ().Contains ("skin") && !structure2.Id.ToLower ().Contains ("cavity")) ||
        (structure2.Id.ToLower ().Contains ("skin") && !structure2.Id.ToLower ().Contains ("cavity"))) {
        volumeIntersection = Double.NaN;
      } else if (structure1.Id.Equals (structure2.Id)) {
        volumeIntersection = structure1.Volume;
      }
      // using the bounds of each rectangle as the ROI for calculating overlap
      else {
        Rect3D structure1Bounds = structure1.MeshGeometry.Bounds;
        Rect3D structure2Bounds = structure2.MeshGeometry.Bounds;
        //Rect3D combinedRectBounds = Rect3D.Union(structure1Bounds, structure2Bounds); NOTE: slower performance
        Rect3D combinedRectBounds = Rect3D.Intersect (structure1Bounds, structure2Bounds);
        if (combinedRectBounds == null) { return 0; }

        // to allow the resolution to be on the same scale in each direction
        double startZ = Math.Floor (combinedRectBounds.Z - 1);
        double endZ = (startZ + Math.Round (combinedRectBounds.SizeZ + 2));
        double startX = Math.Floor (combinedRectBounds.X - 1);
        double endX = (startX + Math.Round (combinedRectBounds.SizeX + 2));
        double startY = Math.Floor (combinedRectBounds.Y - 1);
        double endY = (startY + Math.Round (combinedRectBounds.SizeY + 2));
        for (double z = startZ; z < endZ; z += .5) {
          //planDose.GetVoxels(z, dosePlaneVoxels);
          for (double y = startY; y < endY; y += 1) {
            for (double x = startX; x < endX; x += 1) {
              p.x = x;
              p.y = y;
              p.z = z;

              if ((structure2Bounds.Contains (p.x, p.y, p.z)) &&
                (structure1.IsPointInsideSegment (p)) &&
                (structure2.IsPointInsideSegment (p))) {
                intersectionCount++;
              }
              volumeIntersection = (intersectionCount * 0.001 * .5);
            }
          }
        }
      }
      return Math.Round (volumeIntersection, 3);
    }
    /// <summary>
    /// Returns the percent overlap of a structure and its provided volume of overlap
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="volumeIntersection"></param>
    /// <returns></returns>
    public static double PercentOverlap (Structure structure, double volumeIntersection) {
      double percentOverlap = Math.Round ((volumeIntersection / structure.Volume) * 100, 1);
      if (percentOverlap > 100) {
        percentOverlap = 100;
        return percentOverlap;
      } else {
        return percentOverlap;
      }
    }
    /// <summary>
    /// Calculates the overlap of two given structures, then returns their Dice Coefficient, or measure of similarity.
    /// </summary>
    /// <param name="structure1"></param>
    /// <param name="structure2"></param>
    /// <returns></returns>
    public static double DiceCoefficient (Structure structure1, Structure structure2) {
      // initialize items needed for calculating distance
      VVector p = new VVector ();
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
      Rect3D combinedRectBounds = Rect3D.Intersect (structure1Bounds, structure2Bounds);
      // to allow the resolution to be on the same scale in each direction
      double startZ = Math.Floor (combinedRectBounds.Z - 1);
      double endZ = (startZ + Math.Round (combinedRectBounds.SizeZ + 2));
      double startX = Math.Floor (combinedRectBounds.X - 1);
      double endX = (startX + Math.Round (combinedRectBounds.SizeX + 2));
      double startY = Math.Floor (combinedRectBounds.Y - 1);
      double endY = (startY + Math.Round (combinedRectBounds.SizeY + 2));

      if (structure1 != structure2) {

        if (structure1Bounds.Contains (structure2Bounds)) {
          volumeIntersection = structure2.Volume;
          volumeStructure1 = structure1.Volume;
          volumeStructure2 = structure2.Volume;
        } else if (structure2Bounds.Contains (structure1Bounds)) {
          volumeIntersection = structure1.Volume;
          volumeStructure1 = structure1.Volume;
          volumeStructure2 = structure2.Volume;
        } else {
          // using the bounds of each rectangle as the ROI for calculating overlap
          for (double z = startZ; z < endZ; z += .5) {
            for (double y = startY; y < endY; y += 1) {
              for (double x = startX; x < endX; x += 1) {
                p.x = x;
                p.y = y;
                p.z = z;

                if ((structure2Bounds.Contains (p.x, p.y, p.z)) &&
                  (structure1.IsPointInsideSegment (p)) &&
                  (structure2.IsPointInsideSegment (p))) {
                  intersectionCount++;
                }
                if (structure1.IsPointInsideSegment (p)) {
                  structure1Count++;
                }
                if (structure2.IsPointInsideSegment (p)) {
                  structure2Count++;
                }
                volumeIntersection = (intersectionCount * 0.001 * .5);
                volumeStructure1 = (structure1Count * 0.001 * .5);
                volumeStructure2 = (structure2Count * 0.001 * .5);
              }
            }
          }
        }
        diceCoefficient = Math.Round (((2 * volumeIntersection) / (volumeStructure1 + volumeStructure2)), 3);
        return diceCoefficient;
      } else {
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
    public static double DiceCoefficient (Structure structure1, Structure structure2, double volumeOverlap) {
      return Math.Round ((2 * volumeOverlap) / (structure1.Volume + structure2.Volume), 3);
    }

    /// <summary>
    /// Calculates the shortest distance between structure 1 and structure 2
    /// </summary>
    /// <param name="structure1"></param>
    /// <param name="structure2"></param>
    /// <returns></returns>
    public static double ShortestDistance (Structure structure1, Structure structure2) {
      double shortestDistance = 2000000;
      if (structure1 != structure2 && !structure1.Id.ToLower ().Contains ("body") && !structure1.Id.ToLower ().Contains ("body") &&
        !structure1.Id.ToLower ().Contains ("external") && !structure2.Id.ToLower ().Contains ("external") &&
        !structure2.Id.ToLower ().Contains ("skin")) {
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
        Point3DCollection vertexesStructure1 = new Point3DCollection ();
        Point3DCollection vertexesStructure2 = new Point3DCollection ();
        vertexesStructure1 = structure1.MeshGeometry.Positions;
        vertexesStructure2 = structure2.MeshGeometry.Positions;
        foreach (Point3D v1 in vertexesStructure1) {
          foreach (Point3D v2 in vertexesStructure2) {
            double distance = (Math.Sqrt ((Math.Pow ((v2.X - v1.X), 2)) + (Math.Pow ((v2.Y - v1.Y), 2)) + (Math.Pow ((v2.Z - v1.Z), 2)))) / 10;
            if (distance < shortestDistance) {
              shortestDistance = distance;
            }
          }
        }
        return shortestDistance;
        //}
      } else {
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
    public static double MaxDistance (Structure structure1, Structure structure2) {
      // calculate the max distance between each structure
      Rect3D structure1Bounds = structure1.MeshGeometry.Bounds;
      Rect3D structure2Bounds = structure2.MeshGeometry.Bounds;
      Point3DCollection vertexesStructure1 = new Point3DCollection ();
      Point3DCollection vertexesStructure2 = new Point3DCollection ();
      vertexesStructure1 = structure1.MeshGeometry.Positions;
      vertexesStructure2 = structure2.MeshGeometry.Positions;
      double maxDistance = 0;

      if (structure1Bounds.Contains (structure2Bounds)) {
        maxDistance = 0;
        return maxDistance;
      } else if (structure2Bounds.Contains (structure1Bounds)) {
        maxDistance = 0;
        return maxDistance;
      } else {
        foreach (Point3D v1 in vertexesStructure1) {
          foreach (Point3D v2 in vertexesStructure2) {
            double distance = (Math.Sqrt ((Math.Pow ((v2.X - v1.X), 2)) + (Math.Pow ((v2.Y - v1.Y), 2)) + (Math.Pow ((v2.Z - v1.Z), 2)))) / 10;
            if (distance > maxDistance) {
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
    public static double AverageDistance_InsideRadius (Structure structure1, Structure structure2, double radius) {
      // calculate the average distance between each structure inside a designated radius
      List<double> pointsInsideRadius = new List<double> ();
      Rect3D structure1Bounds = structure1.MeshGeometry.Bounds;
      Rect3D structure2Bounds = structure2.MeshGeometry.Bounds;
      Point3DCollection vertexesStructure1 = new Point3DCollection ();
      Point3DCollection vertexesStructure2 = new Point3DCollection ();
      vertexesStructure1 = structure1.MeshGeometry.Positions;
      vertexesStructure2 = structure2.MeshGeometry.Positions;
      double averageDistance = 0;

      if (structure1Bounds.Contains (structure2Bounds)) {
        averageDistance = 0;
        return averageDistance;
      } else if (structure2Bounds.Contains (structure1Bounds)) {
        averageDistance = 0;
        return averageDistance;
      } else {
        foreach (Point3D v1 in vertexesStructure1) {
          foreach (Point3D v2 in vertexesStructure2) {
            double distance = (Math.Sqrt ((Math.Pow ((v2.X - v1.X), 2)) + (Math.Pow ((v2.Y - v1.Y), 2)) + (Math.Pow ((v2.Z - v1.Z), 2)))) / 10;
            if (distance <= radius) {
              pointsInsideRadius.Add (distance);
            }
          }
        }
        if (pointsInsideRadius.Count == 0) {
          averageDistance = 0;
        } else {
          averageDistance = pointsInsideRadius.Average ();
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
    public static double AverageDistance_OutsideRadius (Structure structure1, Structure structure2, double radius) {
      List<double> pointsOutsideRadius = new List<double> ();
      Rect3D structure1Bounds = structure1.MeshGeometry.Bounds;
      Rect3D structure2Bounds = structure2.MeshGeometry.Bounds;
      // calculate the average distance between each structure outside a designated radius
      Point3DCollection vertexesStructure1 = new Point3DCollection ();
      Point3DCollection vertexesStructure2 = new Point3DCollection ();
      vertexesStructure1 = structure1.MeshGeometry.Positions;
      vertexesStructure2 = structure2.MeshGeometry.Positions;
      double averageDistance = 0;

      if (structure1Bounds.Contains (structure2Bounds)) {
        averageDistance = 0;
        return averageDistance;
      } else if (structure2Bounds.Contains (structure1Bounds)) {
        averageDistance = 0;
        return averageDistance;
      } else {
        foreach (Point3D v1 in vertexesStructure1) {
          //VVector p1 = new VVector();
          foreach (Point3D v2 in vertexesStructure2) {
            double distance = (Math.Sqrt ((Math.Pow ((v2.X - v1.X), 2)) + (Math.Pow ((v2.Y - v1.Y), 2)) + (Math.Pow ((v2.Z - v1.Z), 2)))) / 10;
            if (distance > radius) {
              pointsOutsideRadius.Add (distance);
            }
          }
        }
        if (pointsOutsideRadius.Count == 0) {
          averageDistance = 0;
        } else {
          averageDistance = pointsOutsideRadius.Average ();
        }
        return averageDistance;
      }
    }
    #endregion avg distances
  }

  /// <summary>
  /// Used to return common Volume and Dose Limits for Constraints
  /// </summary>
  /// <returns>double</returns>
  public static class Limit {
    // Volume Limits
    public static double vLimit0_2 { get { return 0.2; } }
    public static double vLimit0_35 { get { return 0.35; } }
    public static double vLimit0_5 { get { return 0.5; } }
    public static double vLimit1 { get { return 1; } }
    public static double vLimit1_2 { get { return 1.2; } }
    public static double vLimit3 { get { return 3; } }
    public static double vLimit5 { get { return 5; } }
    public static double vLimit9 { get { return 9; } }
    public static double vLimit10 { get { return 10; } }
    public static double vLimit15 { get { return 15; } }
    public static double vLimit17 { get { return 17; } }
    public static double vLimit20 { get { return 20; } }
    public static double vLimit22 { get { return 22; } }
    public static double vLimit25 { get { return 25; } }
    public static double vLimit30 { get { return 30; } }
    public static double vLimit33 { get { return 33; } }
    public static double vLimit35 { get { return 35; } }
    public static double vLimit37_5 { get { return 37.5; } }
    public static double vLimit50 { get { return 50; } }
    public static double vLimit55 { get { return 55; } }
    public static double vLimit65 { get { return 65; } }
    //public static double vLimit67 {get {return 67;}}
    public static double vLimit70 { get { return 70; } }
    public static double vLimit100 { get { return 100; } }
    public static double vLimit150 { get { return 150; } }
    public static double vLimit195 { get { return 195; } }
    public static double vLimit200 { get { return 200; } }

    // Dose Limits - Gy
    public static double dLimit4 { get { return 4; } }
    public static double dLimit5 { get { return 5; } }
    public static double dLimit7 { get { return 7; } }
    public static double dLimit8 { get { return 8; } }
    public static double dLimit8_4 { get { return 8.4; } }
    public static double dLimit9 { get { return 9; } }
    public static double dLimit10 { get { return 10; } }
    public static double dLimit11_2 { get { return 11.2; } }
    public static double dLimit11_4 { get { return 11.4; } }
    public static double dLimit12 { get { return 12; } }
    public static double dLimit12_3 { get { return 12.3; } }
    public static double dLimit12_4 { get { return 12.4; } }
    public static double dLimit12_5 { get { return 12.5; } }
    public static double dLimit14 { get { return 14; } }
    public static double dLimit14_3 { get { return 14.3; } }
    public static double dLimit14_5 { get { return 14.5; } }
    public static double dLimit15 { get { return 15; } }
    public static double dLimit15_3 { get { return 15.3; } }
    public static double dLimit16 { get { return 16; } }
    public static double dLimit16_5 { get { return 16.5; } }
    public static double dLimit17_1 { get { return 17.1; } }
    public static double dLimit17_4 { get { return 17.4; } }
    public static double dLimit17_5 { get { return 17.5; } }
    public static double dLimit18 { get { return 18; } }
    public static double dLimit18_4 { get { return 18.4; } }
    public static double dLimit20 { get { return 20; } }
    public static double dLimit20_4 { get { return 20.4; } }
    public static double dLimit21_9 { get { return 21.9; } }
    public static double dLimit22 { get { return 22; } }
    public static double dLimit22_2 { get { return 22.2; } }
    public static double dLimit23 { get { return 23; } }
    public static double dLimit23_1 { get { return 23.1; } }
    public static double dLimit24 { get { return 24; } }
    public static double dLimit25 { get { return 25; } }
    public static double dLimit26 { get { return 26; } }
    public static double dLimit27 { get { return 27; } }
    public static double dLimit28 { get { return 28; } }
    public static double dLimit28_2 { get { return 28.2; } }
    public static double dLimit30 { get { return 30; } }
    public static double dLimit30_5 { get { return 30.5; } }
    public static double dLimit31 { get { return 31; } }
    public static double dLimit32 { get { return 32; } }
    public static double dLimit33 { get { return 33; } }
    public static double dLimit34 { get { return 34; } }
    public static double dLimit35 { get { return 35; } }
    public static double dLimit36 { get { return 36; } }
    public static double dLimit36_5 { get { return 36.5; } }
    public static double dLimit38 { get { return 38; } }
    public static double dLimit39 { get { return 39; } }
    public static double dLimit39_5 { get { return 39.5; } }
    public static double dLimit40 { get { return 40; } }
    public static double dLimit41 { get { return 41; } }
    public static double dLimit42 { get { return 42; } }
    public static double dLimit44 { get { return 44; } }
    public static double dLimit45 { get { return 45; } }
    public static double dLimit50 { get { return 50; } }
    public static double dLimit52 { get { return 52; } }
    public static double dLimit54 { get { return 54; } }
    public static double dLimit55 { get { return 55; } }
    public static double dLimit60 { get { return 60; } }
    public static double dLimit65 { get { return 65; } }
    public static double dLimit66 { get { return 66; } }
    public static double dLimit70 { get { return 70; } }
    public static double dLimit75 { get { return 75; } }
    public static double dLimit80 { get { return 80; } }

    // Dose Limits - cGy
    public static double dLimit4_in_cGy { get { return 400; } }
    public static double dLimit5_in_cGy { get { return 500; } }
    public static double dLimit7_in_cGy { get { return 700; } }
    public static double dLimit8_in_cGy { get { return 800; } }
    public static double dLimit8_4_in_cGy { get { return 840; } }
    public static double dLimit9_in_cGy { get { return 900; } }
    public static double dLimit10_in_cGy { get { return 1000; } }
    public static double dLimit11_2_in_cGy { get { return 1120; } }
    public static double dLimit11_4_in_cGy { get { return 1140; } }
    public static double dLimit12_in_cGy { get { return 1200; } }
    public static double dLimit12_3_in_cGy { get { return 1230; } }
    public static double dLimit12_4_in_cGy { get { return 1240; } }
    public static double dLimit12_5_in_cGy { get { return 1250; } }
    public static double dLimit14_in_cGy { get { return 1400; } }
    public static double dLimit14_3_in_cGy { get { return 1430; } }
    public static double dLimit14_5_in_cGy { get { return 1450; } }
    public static double dLimit15_in_cGy { get { return 1500; } }
    public static double dLimit15_3_in_cGy { get { return 1530; } }
    public static double dLimit16_in_cGy { get { return 1600; } }
    public static double dLimit16_5_in_cGy { get { return 1650; } }
    public static double dLimit17_1_in_cGy { get { return 1710; } }
    public static double dLimit17_4_in_cGy { get { return 1740; } }
    public static double dLimit17_5_in_cGy { get { return 1750; } }
    public static double dLimit18_in_cGy { get { return 1800; } }
    public static double dLimit18_4_in_cGy { get { return 1840; } }
    public static double dLimit20_in_cGy { get { return 2000; } }
    public static double dLimit20_4_in_cGy { get { return 2040; } }
    public static double dLimit21_9_in_cGy { get { return 2190; } }
    public static double dLimit22_in_cGy { get { return 2200; } }
    public static double dLimit22_2_in_cGy { get { return 2220; } }
    public static double dLimit23_in_cGy { get { return 2300; } }
    public static double dLimit23_1_in_cGy { get { return 2310; } }
    public static double dLimit24_in_cGy { get { return 2400; } }
    public static double dLimit25_in_cGy { get { return 2500; } }
    public static double dLimit26_in_cGy { get { return 2600; } }
    public static double dLimit27_in_cGy { get { return 2700; } }
    public static double dLimit28_in_cGy { get { return 2800; } }
    public static double dLimit28_2_in_cGy { get { return 2820; } }
    public static double dLimit30_in_cGy { get { return 3000; } }
    public static double dLimit30_5_in_cGy { get { return 3050; } }
    public static double dLimit31_in_cGy { get { return 3100; } }
    public static double dLimit32_in_cGy { get { return 3200; } }
    public static double dLimit33_in_cGy { get { return 3300; } }
    public static double dLimit34_in_cGy { get { return 3400; } }
    public static double dLimit35_in_cGy { get { return 3500; } }
    public static double dLimit36_in_cGy { get { return 3600; } }
    public static double dLimit36_5_in_cGy { get { return 3650; } }
    public static double dLimit38_in_cGy { get { return 3800; } }
    public static double dLimit39_in_cGy { get { return 3900; } }
    public static double dLimit39_5_in_cGy { get { return 3950; } }
    public static double dLimit40_in_cGy { get { return 4000; } }
    public static double dLimit41_in_cGy { get { return 4100; } }
    public static double dLimit42_in_cGy { get { return 4200; } }
    public static double dLimit44_in_cGy { get { return 4400; } }
    public static double dLimit45_in_cGy { get { return 4500; } }
    public static double dLimit50_in_cGy { get { return 5000; } }
    public static double dLimit52_in_cGy { get { return 5200; } }
    public static double dLimit54_in_cGy { get { return 5400; } }
    public static double dLimit55_in_cGy { get { return 5500; } }
    public static double dLimit60_in_cGy { get { return 6000; } }
    public static double dLimit65_in_cGy { get { return 6500; } }
    public static double dLimit66_in_cGy { get { return 6600; } }
    public static double dLimit70_in_cGy { get { return 7000; } }
    public static double dLimit75_in_cGy { get { return 7500; } }
    public static double dLimit80_in_cGy { get { return 8000; } }
  }

  public class Constraint {
    public string Type { get; set; }
    // public string Structure { get; set; }
    public double VolumeLimit { get; set; }
    public double DoseLimit { get; set; }

    public Constraint () {
      Type = string.Empty;
      // Structure = string.Empty;
      VolumeLimit = double.NaN;
      DoseLimit = double.NaN;
    }

    public Constraint (string type, /* string structure,*/ double volLimit, double doseLimit) {
      Type = type;
      // Structure = structure;
      VolumeLimit = volLimit;
      DoseLimit = doseLimit;
    }

    public override string ToString () {
      switch (Type) {
        case "Relative":
          return string.Format ("V{0} Gy <= {1}%", Structure, DoseLimit, VolumeLimit);
        case "Absolute":
          return string.Format ("V{0} Gy <= {1}cc", Structure, DoseLimit, VolumeLimit);
        case "Max":
          return string.Format ("Max <= {0} Gy", Structure, DoseLimit);
        case "Mean":
          return string.Format ("Mean <= {0} Gy", Structure, DoseLimit);
        default:
          return string.Format ("Sorry, the constraint type of \"{0}\" is not recognized :(", Type);
      }
    }
  }

  public class Constraints {
    public string Structure { get; set; }
    public List<Constraint> ConstraintList { get; set; }

    public Constraints () {
      Structure = string.Empty;
      ConstraintList = new List<Constraint> ();
    }

    public Constraints (string structure, List<Constraint> constraints) {
      Structure = structure;
      ConstraintList = constraints;
    }

    public override string ToString () {
      var s = string.Empty;
      foreach (var clist in ConstraintList) {
        s += clist.Structure + ":\r\n";
        foreach (var c in clist) {
          s += string.Format ("\r\n\t{0}", c.ToString ());
        }
      }
      return s;
    }
  }

  public static class ConstraintChecks {

    public static double evaluateConstraint (Constraint constraint, DVHData dvhAA, DVHData dvhAR) {
      switch (constraint.Type) {
        case "Relative":
          // need relative dvh
          return Math.Round (DvhExtensions.getVolumeAtDose (dvhAR, constraint.DoseLimit), 2);
        case "Absolute":
          // need absolute dvh
          return Math.Round (DvhExtensions.getVolumeAtDose (dvhAA, constraint.DoseLimit), 3);
        case "Max":
          // need absolute dvh
          return Math.Round (dvhAA.MaxDose.Dose, 3);
        case "Mean":
          // need absolute dvh
          return Math.Round (dvhAA.MeanDose.Dose, 3);
        default:
          return double.NaN;
      }
    }

    public static string constraintResult (Constraint constraint, double result) {
      switch (constraint.Type) {
        case "Relative":
          return string.Format ("V{0} Gy = {1}%\t(Limit: <={2}%)", constraint.Structure, constraint.DoseLimit, result, constraint.VolumeLimit);
        case "Absolute":
          return string.Format ("V{0} Gy = {1}cc\t(Limit: <={2}cc)", constraint.Structure, constraint.DoseLimit, result, constraint.VolumeLimit);
        case "Max":
          return string.Format ("Max = {0} Gy\t(Limit: <={1} Gy)", constraint.Structure, result, constraint.DoseLimit);
        case "Mean":
          return string.Format ("Mean = {0} Gy\t(Limit: <={1} Gy)", constraint.Structure, result, constraint.DoseLimit);
        default:
          return string.Format ("Sorry, the constraint type of \"{0}\" is not recognized :(", constraint.Type);
      }
    }

    public static string constraintSetMessage (List<Constraints> constraints) {
      var msg = "";
      foreach (var clist in constraints) {
        msg += clist.ToString () + "\r\n---------------------\r\n";
      }
      return msg;
    }

  }

  #endregion classes
  public class Script {
    public Script () { }

    public void Execute (ScriptContext context /*, System.Windows.Window window*/ ) {

      #region context variable definitions

      // to work for plan sum
      StructureSet structureSet;
      PlanningItem selectedPlanningItem;
      PlanSetup planSetup;
      PlanSum psum = null;
      //double fractions = 0;
      //string status = "";
      if (context.PlanSetup == null && context.PlanSumsInScope.Count () > 1) {
        throw new ApplicationException ("Please close other plan sums");
      }
      if (context.PlanSetup == null) {
        psum = context.PlanSumsInScope.First ();
        planSetup = psum.PlanSetups.First ();
        selectedPlanningItem = (PlanningItem) psum;
        structureSet = psum.StructureSet;
        //fractions = DvhExtensions.getTotalFractionsForPlanSum(psum);
        //status = "PlanSum";

      } else {
        planSetup = context.PlanSetup;
        selectedPlanningItem = (PlanningItem) planSetup;
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
      List<Structure> gtvList = new List<Structure> ();
      List<Structure> ctvList = new List<Structure> ();
      List<Structure> itvList = new List<Structure> ();
      List<Structure> ptvList = new List<Structure> ();
      List<Structure> oarList = new List<Structure> ();
      List<Structure> targetList = new List<Structure> ();
      List<Structure> structureList = new List<Structure> ();

      IEnumerable<Structure> sorted_gtvList = new List<Structure> ();
      IEnumerable<Structure> sorted_ctvList = new List<Structure> ();
      IEnumerable<Structure> sorted_itvList = new List<Structure> ();
      IEnumerable<Structure> sorted_ptvList = new List<Structure> ();
      IEnumerable<Structure> sorted_oarList = new List<Structure> ();
      IEnumerable<Structure> sorted_targetList = new List<Structure> ();
      IEnumerable<Structure> sorted_structureList = new List<Structure> ();

      foreach (var structure in structureSet.Structures) {
        // conditions for adding any structure
        if ((!structure.IsEmpty) &&
          (structure.HasSegment) &&
          (!structure.Id.Contains ("*")) &&
          (!structure.Id.ToLower ().Contains ("markers")) &&
          (!structure.Id.ToLower ().Contains ("avoid")) &&
          (!structure.Id.ToLower ().Contains ("dose")) &&
          (!structure.Id.ToLower ().Contains ("contrast")) &&
          (!structure.Id.ToLower ().Contains ("air")) &&
          (!structure.Id.ToLower ().Contains ("dens")) &&
          (!structure.Id.ToLower ().Contains ("bolus")) &&
          (!structure.Id.ToLower ().Contains ("suv")) &&
          (!structure.Id.ToLower ().Contains ("match")) &&
          (!structure.Id.ToLower ().Contains ("wire")) &&
          (!structure.Id.ToLower ().Contains ("scar")) &&
          (!structure.Id.ToLower ().Contains ("chemo")) &&
          (!structure.Id.ToLower ().Contains ("pet")) &&
          (!structure.Id.ToLower ().Contains ("dnu")) &&
          (!structure.Id.ToLower ().Contains ("fiducial")) &&
          (!structure.Id.ToLower ().Contains ("artifact")) &&
          (!structure.Id.StartsWith ("z", StringComparison.InvariantCultureIgnoreCase)) &&
          (!structure.Id.StartsWith ("hs", StringComparison.InvariantCultureIgnoreCase)) &&
          (!structure.Id.StartsWith ("av", StringComparison.InvariantCultureIgnoreCase)) &&
          (!structure.Id.StartsWith ("opti ", StringComparison.InvariantCultureIgnoreCase)) &&
          (!structure.Id.StartsWith ("opti-", StringComparison.InvariantCultureIgnoreCase)))
        //(structure.Id.Contains("CI-", StringComparison.InvariantCultureIgnoreCase) == false) && 
        //(structure.Id.Contains("R50-", StringComparison.InvariantCultureIgnoreCase) == false) &&
        //(structure.Id.Contains("CI_", StringComparison.InvariantCultureIgnoreCase) == false) && 
        //(structure.Id.Contains("R50_", StringComparison.InvariantCultureIgnoreCase) == false))
        {
          if (structure.Id.StartsWith ("GTV", StringComparison.InvariantCultureIgnoreCase)) {
            gtvList.Add (structure);
            structureList.Add (structure);
            targetList.Add (structure);
          }
          if ((structure.Id.StartsWith ("CTV", StringComparison.InvariantCultureIgnoreCase)) ||
            (structure.Id.StartsWith ("Prost", StringComparison.InvariantCultureIgnoreCase))) {
            ctvList.Add (structure);
            structureList.Add (structure);
            targetList.Add (structure);
          }
          if (structure.Id.StartsWith ("ITV", StringComparison.InvariantCultureIgnoreCase)) {
            itvList.Add (structure);
            structureList.Add (structure);
            targetList.Add (structure);
          }
          if (structure.Id.StartsWith ("PTV", StringComparison.InvariantCultureIgnoreCase)) {
            ptvList.Add (structure);
            structureList.Add (structure);
            targetList.Add (structure);
          }
          // conditions for adding breast plan targets
          if ((structure.Id.StartsWith ("Level I", StringComparison.InvariantCultureIgnoreCase)) ||
            (structure.Id.StartsWith ("IM LN", StringComparison.InvariantCultureIgnoreCase)) ||
            (structure.Id.StartsWith ("Cavity", StringComparison.InvariantCultureIgnoreCase)) ||
            (structure.Id.StartsWith ("Supraclav", StringComparison.InvariantCultureIgnoreCase))) {
            targetList.Add (structure);
            structureList.Add (structure);
          }
          // conditions for adding oars
          if ((!structure.Id.StartsWith ("GTV", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith ("CTV", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith ("ITV", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith ("PTV", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith ("Level I", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith ("IM LN", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith ("Cavity", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith ("Supraclav", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.StartsWith ("Scar", StringComparison.InvariantCultureIgnoreCase)) &&
            (!structure.Id.ToLower ().Contains ("carina"))) {
            oarList.Add (structure);
            structureList.Add (structure);
          }
        }
      }
      sorted_gtvList = gtvList.OrderBy (x => x.Id);
      sorted_ctvList = ctvList.OrderBy (x => x.Id);
      sorted_itvList = itvList.OrderBy (x => x.Id);
      sorted_ptvList = ptvList.OrderBy (x => x.Id);
      sorted_targetList = targetList.OrderBy (x => x.Id);
      sorted_oarList = oarList.OrderBy (x => x.Id);
      sorted_structureList = structureList.OrderBy (x => x.Id);

      #endregion structure organization and ordering

      #region prostate constraints

      var rel = "Relative";
      var abs = "Absolute";

      List<Constraints> hypoSet_1 = new List<Constraints> () {
        // Constraint.Constraint(string type, string structure, double volLimit, double doseLimit)
        new Constraints ("Bladder", new List<Constraint> (new Constraint (rel, Limit.dLimit55, Limit.vLimit50), new Constraint (rel, Limit.dLimit75, Limit.vLimit20))),
        new Constraints ("Fem_Head_L", new List<Constraint> (new Constraint (rel, Limit.dLimit42, Limit.vLimit10))),
        new Constraints ("Fem_Head_R", new List<Constraint> (new Constraint (rel, Limit.dLimit42, Limit.vLimit10))),
        new Constraints ("PenileBulb", new List<Constraint> (new Constraint (rel, Limit.dLimit50, Limit.vLimit50))),
        new Constraints ("Rectum", new List<Constraint> (new Constraint (rel, Limit.dLimit55, Limit.vLimit50), new Constraint (rel, Limit.dLimit75, Limit.vLimit20), new Constraint (rel, Limit.dLimit80, Limit.vLimit10))),
      };

      // List<Constraint> hypoSet_1 = new List<Constraint> () {
      //   // Constraint.Constraint(string type, string structure, double volLimit, double doseLimit)
      //   new Constraint (rel, "Bladder", Limit.dLimit55, Limit.vLimit50),
      //   new Constraint (rel, "Bladder", Limit.dLimit75, Limit.vLimit20),
      //   new Constraint (rel, "Fem_Head_L", Limit.dLimit42, Limit.vLimit10),
      //   new Constraint (rel, "Fem_Head_R", Limit.dLimit42, Limit.vLimit10),
      //   new Constraint (rel, "PenileBulb", Limit.dLimit50, Limit.vLimit50),
      //   new Constraint (rel, "Rectum", Limit.dLimit55, Limit.vLimit50),
      //   new Constraint (rel, "Rectum", Limit.dLimit75, Limit.vLimit20),
      //   new Constraint (rel, "Rectum", Limit.dLimit80, Limit.vLimit10),
      // };

      // List<Constraint> hypoSet_2 = new List<Constraint> () {
      //   // Constraint.Constraint(string type, string structure, double volLimit, double doseLimit)
      //   new Constraint (rel, "Bladder", Limit.dLimit55, Limit.vLimit50),
      //   new Constraint (rel, "Bladder", Limit.dLimit75, Limit.vLimit20),
      //   new Constraint (rel, "Fem_Head_L", Limit.dLimit42, Limit.vLimit10),
      //   new Constraint (rel, "Fem_Head_R", Limit.dLimit42, Limit.vLimit10),
      //   new Constraint (rel, "PenileBulb", Limit.dLimit50, Limit.vLimit50),
      //   new Constraint (rel, "Rectum", Limit.dLimit55, Limit.vLimit50),
      //   new Constraint (rel, "Rectum", Limit.dLimit65, Limit.vLimit22),
      //   new Constraint (rel, "Rectum", Limit.dLimit70, Limit.vLimit17),
      //   new Constraint (rel, "LargeBowel", Limit.dLimit65, Limit.vLimit10),
      //   new Constraint (rel, "LargeBowel", Limit.dLimit70, Limit.vLimit5),
      // };

      // List<Constraint> seqSet_1 = new List<Constraint> () {
      //   // Constraint.Constraint(string type, string structure, double volLimit, double doseLimit)
      //   new Constraint (rel, "Bladder-CTV", Limit.dLimit55, Limit.vLimit50),
      //   new Constraint (rel, "Bladder-CTV", Limit.dLimit75, Limit.vLimit20),
      //   new Constraint (rel, "Fem_Head_L", Limit.dLimit50, Limit.vLimit10),
      //   new Constraint (rel, "Fem_Head_R", Limit.dLimit50, Limit.vLimit10),
      //   new Constraint (rel, "Rectum", Limit.dLimit40, Limit.vLimit55),
      //   new Constraint (rel, "Rectum", Limit.dLimit65, Limit.vLimit35),
      //   new Constraint (abs, "Bag_Bowel", Limit.dLimit45, Limit.vLimit150),
      //   // supposed to be named small bowel
      //   // new Constraint(abs,"SmallBowel", Limit.dLimit45, Limit.vLimit150),
      // };

      List<List<ConstraintS>> constraintSets = new List<List<Constraints>> () {
        hypoSet_1,
        hypoSet_2,
        seqSet_1
      };

      #endregion

      #region variables

      var result = string.Empty;
      var boxTitle = string.Empty;

      var messages = new List<string> ();
      var sMsg = string.Empty;
      var calcOverlapMsg = string.Empty;

      var sOverlapAbs = double.NaN;
      var sOverlapPct = double.NaN;

      var dosePerFraction = planSetup.UniqueFractionation.PrescribedDosePerFraction.Dose;

      var willCalcOverlap = false;
      var willCheckConstraints = false;
      var isHypoSet_1 = false;
      var isHypoSet_2 = false;
      // var isSeqSet_1 = false;

      var structsWithConstraints = new List<string> ();

      #endregion variables

      // get list of structures from all constraint sets
      foreach (var cs in constraintSets) {
        foreach (var c in cs) {
          structsWithConstraints.Add (c.Structure);
        }
      }

      // check if user wants to check constraints
      if (MessageBox.Show ("Check for available Prostate constraints?", "Check Constraints?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
        willCheckConstraints = true;
        calcOverlapMsg = "Would you also like to calculate Target/OAR overlap?";
      } else {
        willCheckConstraints = false;
        calcOverlapMsg = "Would you like to calculate Target/OAR overlap?";
      }

      // check if user wants to calc overlap
      if (MessageBox.Show (calcOverlapMsg, "Calculate Overlap?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
        willCalcOverlap = true;
      } else {
        willCalcOverlap = false;
      }

      // check constraints
      if (willCheckConstraints) {
        // if hypo frac
        if (dosePerFraction > 2) {
          boxTitle = "Hypo Fractionated Constraints";

          var isHypo1Msg = ConstraintChecks.constraintSetMessage (hypoSet_1) + "\r\n\r\nDo these constraints look correct?";

          var isHypo2Msg = ConstraintChecks.constraintSetMessage (hypoSet_2) + "\r\nDo these constraints look correct?";
          if (MessageBox.Show (isHypo1Msg, "Hypo Set 1", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
            isHypoSet_1 = true;
          } else {
            isHypoSet_1 = false;
            if (MessageBox.Show (isHypo2Msg, "Hypo Set 2", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
              isHypoSet_2 = true;
            } else {
              isHypoSet_2 = false;
              MessageBox.Show ("Sorry, it seems we don't have the constraints you're looking for :(");
              return;
            }
          }

          if (isHypoSet_1) {
            foreach (var c in hypoSet_1) {
              foreach (var s in sorted_oarList) {
                DVHData dvhAA = selectedPlanningItem.GetDVHCumulativeData (s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);
                DVHData dvhAR = selectedPlanningItem.GetDVHCumulativeData (s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);

                if (s.Id == c.Structure) {
                  sMsg = ConstraintChecks.constraintResult (c, ConstraintChecks.evaluateConstraint (c, dvhAA, dvhAR));

                  if (willCalcOverlap) {
                    foreach (var t in sorted_ptvList) {
                      sOverlapAbs = CalculateOverlap.VolumeOverlap (s, t);
                      sOverlapPct = CalculateOverlap.PercentOverlap (s, sOverlapAbs);

                      sMsg += string.Format ("\r\nOverlap with {0}:\t{1} cc\t({2} %)", t.Id, sOverlapAbs, sOverlapPct);
                    }

                  }

                  sMsg += "\r\n---------------------\r\n";

                  messages.Add (sMsg);
                }
              }
            }
          } else if (isHypoSet_2) {
            foreach (var c in hypoSet_2) {
              foreach (var s in sorted_oarList) {
                DVHData dvhAA = selectedPlanningItem.GetDVHCumulativeData (s, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);
                DVHData dvhAR = selectedPlanningItem.GetDVHCumulativeData (s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);

                if (s.Id == c.Structure) {
                  sMsg = ConstraintChecks.constraintResult (c, ConstraintChecks.evaluateConstraint (c, dvhAA, dvhAR));

                  if (willCalcOverlap) {
                    foreach (var t in sorted_ptvList) {
                      sOverlapAbs = CalculateOverlap.VolumeOverlap (s, t);
                      sOverlapPct = CalculateOverlap.PercentOverlap (s, sOverlapAbs);

                      sMsg += string.Format ("\r\nOverlap with {0}:\t{1} cc\t({2} %)", t.Id, sOverlapAbs, sOverlapPct);
                    }

                  }

                  sMsg += "\r\n---------------------\r\n";

                  messages.Add (sMsg);
                }
              }
            }
          }

          foreach (var msg in messages) {
            result += msg + "\r\n";
          }

          MessageBox.Show (result, boxTitle);

        }
      } else {
        // calculate overlap
        if (willCalcOverlap) {
          boxTitle = "Overlap Stats";
          sMsg += "Overlap Stats\r\n---------------------\r\n";
          foreach (var s in sorted_oarList) {
            foreach (var structure in structsWithConstraints.Distinct ().ToList ()) {
              if (s.Id == structure) {
                sMsg = string.Format ("{0}:", s.Id);
                foreach (var t in sorted_ptvList) {
                  sOverlapAbs = CalculateOverlap.VolumeOverlap (s, t);
                  sOverlapPct = CalculateOverlap.PercentOverlap (s, sOverlapAbs);

                  sMsg += string.Format ("\r\n\tOverlap with {0}:\t{1} cc\t({2} %)", t.Id, sOverlapAbs, sOverlapPct);
                }
                sMsg += "\r\n\r\n---------------------\r\n";
                messages.Add (sMsg);
              }
            }
          }

          foreach (var msg in messages) {
            result += msg + "\r\n";
          }
        } else {
          boxTitle = "We're Useless";
          result = "Sorry we couldn't help you with anything :(";
        }

        MessageBox.Show (result, boxTitle);
      }

    }

  }
}