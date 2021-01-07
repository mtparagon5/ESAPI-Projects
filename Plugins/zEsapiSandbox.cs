
using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace VMS.TPS
{
  #region Class Definitions //

  public static class DvhExtensions
  {
    public static DoseValue GetDoseAtVolume(this PlanningItem pitem, Structure structure, double volume, VolumePresentation volumePresentation, DoseValuePresentation requestedDosePresentation)
    {
      if (pitem is PlanSetup)
      {
        return ((PlanSetup)pitem).GetDoseAtVolume(structure, volume, volumePresentation, requestedDosePresentation);
      }
      else
      {
        if (requestedDosePresentation != DoseValuePresentation.Absolute)
          throw new ApplicationException("Only absolute dose supported for Plan Sums");
        DVHData dvh = pitem.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, volumePresentation, 0.001);
        return DvhExtensions.DoseAtVolume(dvh, volume);
      }
    }
    public static double GetVolumeAtDose(this PlanningItem pitem, Structure structure, DoseValue dose, VolumePresentation requestedVolumePresentation)
    {
      if (pitem is PlanSetup)
      {
        return ((PlanSetup)pitem).GetVolumeAtDose(structure, dose, requestedVolumePresentation);
      }
      else
      {
        DVHData dvh = pitem.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, requestedVolumePresentation, 0.001);
        return DvhExtensions.VolumeAtDose(dvh, dose.Dose);
      }
    }

    public static DoseValue DoseAtVolume(DVHData dvhData, double volume)
    {
      if (dvhData == null || dvhData.CurveData.Count() == 0)
        return DoseValue.UndefinedDose();
      double absVolume = dvhData.CurveData[0].VolumeUnit == "%" ? volume * dvhData.Volume * 0.01 : volume;
      if (volume < 0.0 /*|| absVolume > dvhData.Volume*/)
        return DoseValue.UndefinedDose();

      DVHPoint[] hist = dvhData.CurveData;
      for (int i = 0; i < hist.Length; i++)
      {
        if (hist[i].Volume < volume)
          return hist[i].DoseValue;
      }
      return DoseValue.UndefinedDose();
    }

    public static double VolumeAtDose(DVHData dvhData, double dose)
    {
      if (dvhData == null)
        return Double.NaN;

      DVHPoint[] hist = dvhData.CurveData;
      int index = (int)(hist.Length * dose / dvhData.MaxDose.Dose);
      if (index < 0 || index > hist.Length)
        return 0.0;//Double.NaN;
      else
        return hist[index].Volume;
    }
    public static double getDoseAtVolume(DVHData dvh, double VolumeLim)
    {
      for (int i = 0; i < dvh.CurveData.Length; i++)
      {
        if (dvh.CurveData[i].Volume <= VolumeLim)
        {
          return dvh.CurveData[i].DoseValue.Dose;
        }
      }
      return 0;
    }
    public static double getVolumeAtDose(DVHData dvh, double DoseLim)
    {
      for (int i = 0; i < dvh.CurveData.Length; i++)
      {
        if (dvh.CurveData[i].DoseValue.Dose >= DoseLim)
        {
          return dvh.CurveData[i].Volume;

        }
      }
      return 0;
    }
  }
  // NOTE: This is used to calculate the acceptable R50 Range for a Lung SBRT Plan based on PTV Volume -- From RTOG 0915
  public class R50Constraint
  {
    public static void LimitsFromVolume(double volume, out double limit1, out double limit2, out double limit3, out double limit4)
    {
      // larger tah last in the table
      limit1 = 5.9;
      limit2 = 7.5;
      limit3 = 50;
      limit4 = 57;

      if ((volume >= 1.8) && (volume < 3.8))
      {
        limit1 = 5.9 + (volume - 1.8) * (5.5 - 5.9) / (3.8 - 1.8);
        limit2 = 7.5 + (volume - 1.8) * (6.5 - 7.5) / (3.8 - 1.8);
        limit3 = 50 + (volume - 1.8) * (50 - 50) / (3.8 - 1.8);
        limit4 = 57 + (volume - 1.8) * (57 - 57) / (3.8 - 1.8);
      }

      if ((volume >= 3.8) && (volume < 7.4))
      {
        limit1 = 5.5 + (volume - 3.8) * (5.1 - 5.5) / (7.4 - 3.8);
        limit2 = 6.5 + (volume - 3.8) * (6.0 - 6.5) / (7.4 - 3.8);
        limit3 = 50 + (volume - 3.8) * (50 - 50) / (7.4 - 3.8);
        limit4 = 57 + (volume - 3.8) * (58 - 57) / (7.4 - 3.8);
      }

      if ((volume >= 7.4) && (volume < 13.2))
      {
        limit1 = 5.1 + (volume - 7.4) * (4.7 - 5.1) / (13.2 - 7.4);
        limit2 = 6.0 + (volume - 7.4) * (5.8 - 6.0) / (13.2 - 7.4);
        limit3 = 50 + (volume - 7.4) * (54 - 50) / (13.2 - 7.4);
        limit4 = 58 + (volume - 7.4) * (58 - 58) / (13.2 - 7.4); ;
      }

      if ((volume > 13.2) && (volume < 22.0))
      {
        limit1 = 4.7 + (volume - 13.2) * (4.5 - 4.7) / (22.0 - 13.2);
        limit2 = 5.8 + (volume - 13.2) * (5.5 - 5.8) / (22.0 - 13.2);
        limit3 = 50 + (volume - 13.2) * (54 - 50) / (22.0 - 13.2);
        limit4 = 58 + (volume - 13.2) * (63 - 58) / (22.0 - 13.2);
      }

      if ((volume > 22.0) && (volume < 34.0))
      {
        limit1 = 4.5 + (volume - 22.0) * (4.3 - 4.5) / (34.0 - 22.0);
        limit2 = 5.5 + (volume - 22.0) * (5.3 - 5.5) / (34.0 - 22.0);
        limit3 = 54 + (volume - 22.0) * (58 - 54) / (34.0 - 22.0);
        limit4 = 63 + (volume - 22.0) * (68 - 63) / (34.0 - 22.0);
      }

      if ((volume > 34.0) && (volume < 50.0))
      {
        limit1 = 4.3 + (volume - 34.0) * (4.0 - 4.3) / (50.0 - 34.0);
        limit2 = 5.3 + (volume - 34.0) * (5.0 - 5.3) / (50.0 - 34.0);
        limit3 = 58 + (volume - 34.0) * (62 - 58) / (50.0 - 34.0);
        limit4 = 68 + (volume - 34.0) * (77 - 68) / (50.0 - 34.0);
      }

      if ((volume > 50.0) && (volume < 70.0))
      {
        limit1 = 4.0 + (volume - 50.0) * (3.5 - 4.0) / (70.0 - 50.0);
        limit2 = 5.0 + (volume - 50.0) * (4.8 - 5.0) / (70.0 - 50.0);
        limit3 = 62 + (volume - 50.0) * (66 - 62) / (70.0 - 50.0);
        limit4 = 77 + (volume - 50.0) * (86 - 77) / (70.0 - 50.0);
      }

      if ((volume > 70.0) && (volume < 95.0))
      {
        limit1 = 3.5 + (volume - 70.0) * (3.3 - 3.5) / (95.0 - 70.0);
        limit2 = 4.8 + (volume - 70.0) * (4.4 - 4.8) / (95.0 - 70.0);
        limit3 = 66 + (volume - 70.0) * (70 - 66) / (95.0 - 70.0);
        limit4 = 86 + (volume - 70.0) * (89 - 86) / (95.0 - 70.0);
      }

      if ((volume > 95.0) && (volume < 126.0))
      {
        limit1 = 3.3 + (volume - 95.0) * (3.1 - 3.3) / (126.0 - 95.0);
        limit2 = 4.4 + (volume - 95.0) * (4.0 - 4.4) / (126.0 - 95.0);
        limit3 = 70 + (volume - 95.0) * (73 - 70) / (126.0 - 95.0);
        limit4 = 89 + (volume - 95.0) * (91 - 89) / (126.0 - 95.0);
      }

      if ((volume > 126.0) && (volume < 163.0))
      {
        limit1 = 3.1 + (volume - 126.0) * (2.9 - 3.1) / (163.0 - 126.0);
        limit2 = 4.0 + (volume - 126.0) * (3.7 - 4.0) / (163.0 - 126.0);
        limit3 = 73 + (volume - 126.0) * (77 - 73) / (163.0 - 126.0);
        limit4 = 91 + (volume - 126.0) * (94 - 91) / (163.0 - 126.0);
      }

      if ((volume > 163.0))
      {
        limit1 = 2.9;
        limit2 = 3.7;
        limit3 = 77;
        limit4 = 94;
      }
    }
  }
  // This class can be used to calculate:
  //      volume overlap of two structures, percent overlap, shortest distance, 
  //      average distance inside given radius, and average distance outside a given radius
  // TODO: need to find meaningful representation of data that can be calculated
  public class CalculateOverlap
  {
    public static double VolumeOverlap(Structure structure1, Structure structure2)
    {
      // initialize items needed for calculating distance
      VVector p = new VVector();
      double volumeIntersection = 0;
      int intersectionCount = 0;

      Rect3D structure1Bounds = structure1.MeshGeometry.Bounds;
      Rect3D structure2Bounds = structure2.MeshGeometry.Bounds;
      Rect3D combinedRectBounds = Rect3D.Union(structure1Bounds, structure2Bounds);

      // to allow the resolution to be on the same scale in each direction
      double startZ = Math.Floor(combinedRectBounds.Z - 1);
      double endZ = (startZ + Math.Round(combinedRectBounds.SizeZ + 2));
      double startX = Math.Floor(combinedRectBounds.X - 1);
      double endX = (startX + Math.Round(combinedRectBounds.SizeX + 2));
      double startY = Math.Floor(combinedRectBounds.Y - 1);
      double endY = (startY + Math.Round(combinedRectBounds.SizeY + 2));

      if (structure1Bounds.Contains(structure2Bounds))
      {
        volumeIntersection = structure2.Volume;
      }
      else if (structure2Bounds.Contains(structure1Bounds))
      {
        volumeIntersection = structure1.Volume;
      }
      // using the bounds of each rectangle as the ROI for calculating overlap
      else
      {
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
    public static double PercentOverlap(Structure structure, double volumeIntersection)
    {
      double percentOverlap = (volumeIntersection / structure.Volume) * 100;
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
      Rect3D combinedRectBounds = Rect3D.Union(structure1Bounds, structure2Bounds);
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
    public static double DiceCoefficient(Structure structure1, Structure structure2, double volumeOverlap)
    {
      return Math.Round((2 * volumeOverlap) / (structure1.Volume + structure2.Volume), 3);
    }
    public static double ShortestDistance(Structure structure1, Structure structure2)
    {
      // calculate the shortest distance between each structure
      Rect3D structure1Bounds = structure1.MeshGeometry.Bounds;
      Rect3D structure2Bounds = structure2.MeshGeometry.Bounds;
      Point3DCollection vertexesStructure1 = new Point3DCollection();
      Point3DCollection vertexesStructure2 = new Point3DCollection();
      vertexesStructure1 = structure1.MeshGeometry.Positions;
      vertexesStructure2 = structure2.MeshGeometry.Positions;
      double shortestDistance = 2000000;
      if (structure1 != structure2)
      {
        if (structure1Bounds.Contains(structure2Bounds))
        {
          shortestDistance = 0;
          return shortestDistance;
        }
        else if (structure2Bounds.Contains(structure1Bounds))
        {
          shortestDistance = 0;
          return shortestDistance;
        }
        else
        {
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
        }
      }
      else
      {
        shortestDistance = 0;
        return shortestDistance;
      }
    }
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
    #region Average Distances inside/outside radius -- may not be useful
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

  public class JawTracking
  {
    public static void IsJawTracking(IEnumerator<PlanSetup> availablePlans, out string isJawTracking)
    {
      //	public class JawPositions
      //{
      //	public string _planId { get; set; }
      //	public string _planStatus { get; set; }
      //	public string _fieldId { get; set; }
      //	public string _isJawTracking { get; set; }
      //	public string _x1 { get; set; }
      //	public string _x2 { get; set; }
      //	public string _y1 { get; set; }
      //	public string _y2 { get; set; }
      //	public double _mu { get; set; }
      //}

      //List<string> fieldsNotIMRT = new List<string>();
      List<string> plansWithJawTracking = new List<string>();
      plansWithJawTracking.Add("Plan Id\t\t|\tJawTracking");

      List<string> plansWithOutJawTracking = new List<string>();
      plansWithOutJawTracking.Add("Plan Id\t\t|\tJawTracking");

      bool usesJawTracking;
      var result = "";

      while (availablePlans.MoveNext())
      {
        var currentPlan = (ExternalPlanSetup)availablePlans.Current;
        var planFields = currentPlan.Beams;
        int counter = 0;

        //MessageBox.Show(currentPlan.Id);

        usesJawTracking = false;

        foreach (var field in planFields)
        {
          //JawPositions jP = new JawPositions();
          //jP._planId = currentPlan.Id;
          //jP._planStatus = currentPlan.ApprovalStatus.ToString();

          if ((field.MLCPlanType.ToString() == "VMAT") || (field.MLCPlanType.ToString() == "DoseDynamic"))
          {
            counter++;
            if (field.ControlPoints.Count > 9)
            {
              var cpNum = field.ControlPoints.Count - 1;

              if ((field.ControlPoints[(int)(Math.Floor((double)(cpNum / 3)))].JawPositions != null) && (field.ControlPoints[(int)(Math.Floor((double)(cpNum / 5)))].JawPositions != null))
              {
                var cp1 = field.ControlPoints[cpNum / 1];
                var cp2 = field.ControlPoints[(int)(Math.Floor((double)(cpNum / 3)))];
                var cp3 = field.ControlPoints[(int)(Math.Floor((double)(cpNum / 5)))];
                var cp4 = field.ControlPoints[(int)(Math.Floor((double)(cpNum / 7)))];

                if ((cp1.JawPositions.X1 != cp2.JawPositions.X1) ||
                    (cp1.JawPositions.X2 != cp2.JawPositions.X2) ||
                    (cp1.JawPositions.Y1 != cp2.JawPositions.Y1) ||
                    (cp1.JawPositions.Y2 != cp2.JawPositions.Y2) ||
                    (cp1.JawPositions.X1 != cp3.JawPositions.X1) ||
                    (cp1.JawPositions.X2 != cp3.JawPositions.X2) ||
                    (cp1.JawPositions.Y1 != cp3.JawPositions.Y1) ||
                    (cp1.JawPositions.Y2 != cp3.JawPositions.Y2) ||
                    (cp2.JawPositions.X1 != cp4.JawPositions.X1) ||
                    (cp2.JawPositions.X2 != cp4.JawPositions.X2) ||
                    (cp2.JawPositions.Y1 != cp4.JawPositions.Y1) ||
                    (cp2.JawPositions.Y2 != cp4.JawPositions.Y2))
                {

                  usesJawTracking = true;

                  //jP._isJawTracking = "Yes";
                  //jP._fieldId = field.Id;
                  //var controlPoints = field.ControlPoints;
                  //var cP = controlPoints[0];

                  //jP._x1 = string.Format("{0:N1}", Math.Round((decimal)(cP.JawPositions.X1) / -10, 2));
                  //jP._y1 = string.Format("{0:N1}", Math.Round((decimal)(cP.JawPositions.Y1) / -10, 2));
                  //jP._y2 = string.Format("{0:N1}", Math.Round((decimal)(cP.JawPositions.Y2) / 10, 2));
                  //jP._x2 = string.Format("{0:N1}", Math.Round((decimal)(cP.JawPositions.X2) / 10, 2));
                  //jP._mu = Math.Round(field.Meterset.Value);

                  //JawPositions_DG.Items.Add(jP);
                  //jawTrackingCsvContent.AppendLine(string.Format("{0},\t{1},\t\t{2},\t\t\t{3},\t\t{4}", id, jP._planId, jP._planStatus, jP._fieldId, jP._mu));
                }
                //else
                //{
                //	jP._isJawTracking = "No";
                //	jP._fieldId = field.Id;
                //	var controlPoints = field.ControlPoints;
                //	var cP = controlPoints[0];

                //	jP._x1 = string.Format("{0:N1}", Math.Round((decimal)(cP.JawPositions.X1) / -10, 2));
                //	jP._y1 = string.Format("{0:N1}", Math.Round((decimal)(cP.JawPositions.Y1) / -10, 2));
                //	jP._y2 = string.Format("{0:N1}", Math.Round((decimal)(cP.JawPositions.Y2) / 10, 2));
                //	jP._x2 = string.Format("{0:N1}", Math.Round((decimal)(cP.JawPositions.X2) / 10, 2));
                //	jP._mu = Math.Round(field.Meterset.Value);

                //	JawPositions_DG.Items.Add(jP);
                //}
              }
            }
            //else
            //{
            //	fieldsNotIMRT.Add(field.Id.ToString());
            //}
          }
        }

        if (usesJawTracking == true)
        {
          plansWithJawTracking.Add(string.Format("{0}\t\tYes", currentPlan.Id));
        }

        if (usesJawTracking == false)
        {
          plansWithOutJawTracking.Add(string.Format("{0}\t\tNo", currentPlan.Id));
        }


        //if (fieldsNotIMRT.Count > 0)
        //{
        //	string message = "";
        //	foreach (var f in fieldsNotIMRT)
        //	{
        //		message += "\n\t- " + f;
        //	}
        //	MessageBox.Show(string.Format("The following fields are FIF but will not be added:\n{0}", message), string.Format("{0}", currentPlan.Id));
        //}
        //if (counter < 1) { MessageBox.Show("None of the fields have multiple control points.", string.Format("{0}", currentPlan.Id)); }
      }

      if (plansWithJawTracking.Count > 1)
      {
        foreach (var s in plansWithJawTracking)
        {

          result += s + '\n';

        }
      }

      if (plansWithOutJawTracking.Count > 1)
      {
        foreach (var s in plansWithOutJawTracking)
        {

          result += s + '\n';

        }
      }

      //result = result.Remove(result.LastIndexOf('\n'));

      isJawTracking = result;
    }
  }

  public class PatientPositioningInformation
  {
    public static void patientOrientation(VMS.TPS.Common.Model.API.Image image, out PatientOrientation patientOrientation)
    {
      patientOrientation = image.ImagingOrientation;
    }

    public static void isocenterShiftFromOrigin(VMS.TPS.Common.Model.API.Image image, PlanSetup plan, out string isSingleIso,
                                                                                out string shiftX,
                                                                                out string shiftY,
                                                                                out string shiftZ)
    {
      IEnumerable<Beam> planFields = plan.Beams;
      List<Beam> fieldsList = new List<Beam>();
      isSingleIso = "Yes; Single Isocenter";
      shiftX = "";
      shiftY = "";
      shiftZ = "";

      foreach (var field in planFields)
      {
        fieldsList.Add(field);
      }

      decimal field1_dicomX = Math.Round((decimal)fieldsList[0].IsocenterPosition.x / 10, 2);
      decimal field1_dicomY = Math.Round((decimal)fieldsList[0].IsocenterPosition.y / 10, 2);
      decimal field1_dicomZ = Math.Round((decimal)fieldsList[0].IsocenterPosition.z / 10, 2);

      decimal userOriginY = Math.Round(((decimal)image.UserOrigin.y / 10), 2);
      decimal userOriginZ = Math.Round(((decimal)image.UserOrigin.z / 10), 2);
      decimal userOriginX = Math.Round(((decimal)image.UserOrigin.x / 10), 2);

      var fieldXiso = (field1_dicomX - userOriginX);
      var fieldYiso = (field1_dicomY - userOriginY);
      var fieldZiso = (field1_dicomZ - userOriginZ);

      if (image.ImagingOrientation == PatientOrientation.HeadFirstProne)
      {
        fieldXiso = -(field1_dicomX - userOriginX);
        fieldYiso = -(field1_dicomY - userOriginY);
        fieldZiso = (field1_dicomZ - userOriginZ);
      }

      foreach (var f in fieldsList)
      {
        if ((f.IsocenterPosition.x != fieldsList[0].IsocenterPosition.x) ||
            (f.IsocenterPosition.y != fieldsList[0].IsocenterPosition.y) ||
            (f.IsocenterPosition.x != fieldsList[0].IsocenterPosition.x))
        {
          isSingleIso = "Warning: Multiple Isocenters - Verify Shifts";
        }
      }

      if (image.ImagingOrientation == PatientOrientation.HeadFirstSupine)
      {
        if (fieldXiso > 0) { shiftX = string.Format("{0} cm Left", Math.Round((Math.Abs(fieldXiso)), 1)); }
        else if (fieldXiso < 0) { shiftX = string.Format("{0} cm Right", Math.Round((Math.Abs(fieldXiso)), 1)); }
        else if (fieldXiso == 0) { shiftX = string.Format(" - "); }

        if (fieldYiso > 0) { shiftY = string.Format("{0} cm Posterior", Math.Round((Math.Abs(fieldYiso)), 1)); }
        else if (fieldYiso < 0) { shiftY = string.Format("{0} cm Anterior", Math.Round((Math.Abs(fieldYiso)), 1)); }
        else if (fieldYiso == 0) { shiftY = string.Format(" - "); }

        if (fieldZiso > 0) { shiftZ = string.Format("{0} cm Superior", Math.Round((Math.Abs(fieldZiso)), 1)); }
        else if (fieldZiso < 0) { shiftZ = string.Format("{0} cm Inferior", Math.Round((Math.Abs(fieldZiso)), 1)); }
        else if (fieldZiso == 0) { shiftZ = string.Format(" - "); }
      }

      else if (image.ImagingOrientation == PatientOrientation.HeadFirstProne)
      {
        if (fieldXiso > 0) { shiftX = string.Format("{0} cm Left", Math.Round((Math.Abs(fieldXiso)), 1)); }
        else if (fieldXiso < 0) { shiftX = string.Format("{0} cm Right", Math.Round((Math.Abs(fieldXiso)), 1)); }
        else if (fieldXiso == 0) { shiftX = string.Format(" - "); }

        if (fieldYiso > 0) { shiftY = string.Format("{0} cm Posterior", Math.Round((Math.Abs(fieldYiso)), 1)); }
        else if (fieldYiso < 0) { shiftY = string.Format("{0} cm Anterior", Math.Round((Math.Abs(fieldYiso)), 1)); }
        else if (fieldYiso == 0) { shiftY = string.Format(" - "); }

        if (fieldZiso > 0) { shiftZ = string.Format("{0} cm Superior", Math.Round((Math.Abs(fieldZiso)), 1)); }
        else if (fieldZiso < 0) { shiftZ = string.Format("{0} cm Inferior", Math.Round((Math.Abs(fieldZiso)), 1)); }
        else if (fieldZiso == 0) { shiftZ = string.Format(" - "); }
      }

      else if (image.ImagingOrientation == PatientOrientation.FeetFirstSupine)
      {
        if (fieldXiso > 0) { shiftX = string.Format("{0} cm Right", Math.Round((Math.Abs(fieldXiso)), 1)); }
        else if (fieldXiso < 0) { shiftX = string.Format("{0} cm Left", Math.Round((Math.Abs(fieldXiso)), 1)); }
        else if (fieldXiso == 0) { shiftX = string.Format(" - "); }

        if (fieldYiso > 0) { shiftY = string.Format("{0} cm Posterior", Math.Round((Math.Abs(fieldYiso)), 1)); }
        else if (fieldYiso < 0) { shiftY = string.Format("{0} cm Anterior", Math.Round((Math.Abs(fieldYiso)), 1)); }
        else if (fieldYiso == 0) { shiftY = string.Format(" - "); }

        if (fieldZiso > 0) { shiftZ = string.Format("{0} cm Inferior", Math.Round((Math.Abs(fieldZiso)), 1)); }
        else if (fieldZiso < 0) { shiftZ = string.Format("{0} cm Superior", Math.Round((Math.Abs(fieldZiso)), 1)); }
        else if (fieldZiso == 0) { shiftZ = string.Format(" - "); }
      }

      else if (image.ImagingOrientation == PatientOrientation.FeetFirstProne)
      {
        if (fieldXiso > 0) { shiftX = string.Format("{0} cm Right", Math.Round((Math.Abs(fieldXiso)), 1)); }
        else if (fieldXiso < 0) { shiftX = string.Format("{0} cm Left", Math.Round((Math.Abs(fieldXiso)), 1)); }
        else if (fieldXiso == 0) { shiftX = string.Format(" - "); }

        if (fieldYiso > 0) { shiftY = string.Format("{0} cm Posterior", Math.Round((Math.Abs(fieldYiso)), 1)); }
        else if (fieldYiso < 0) { shiftY = string.Format("{0} cm Anterior", Math.Round((Math.Abs(fieldYiso)), 1)); }
        else if (fieldYiso == 0) { shiftY = string.Format(" - "); }

        if (fieldZiso > 0) { shiftZ = string.Format("{0} cm Inferior", Math.Round((Math.Abs(fieldZiso)), 1)); }
        else if (fieldZiso < 0) { shiftZ = string.Format("{0} cm Superior", Math.Round((Math.Abs(fieldZiso)), 1)); }
        else if (fieldZiso == 0) { shiftZ = string.Format(" - "); }
      }

      else
      {
        shiftX = "Patient Orientation Undefined: Please Verify";
        shiftY = "Patient Orientation Undefined: Please Verify";
        shiftZ = "Patient Orientation Undefined: Please Verify";
      }

    }
  }

  public class GetPrimaryPhysician
  {
    public static void primaryPhysician(string tempPhysicianId, out string primaryPhysician)
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
      else primaryPhysician = "Unknown";
    }
  }

  public class GenerateStructureLists
  {

    //	public static void cleanAndOrderStructures(StructureSet ss, out IEnumerable<Structure> sorted_gtvList,
    //															out IEnumerable<Structure> sorted_ctvList,
    //															out IEnumerable<Structure> sorted_itvList,
    //															out IEnumerable<Structure> sorted_ptvList,
    //															out IEnumerable<Structure> sorted_targetList,
    //															out IEnumerable<Structure> sorted_oarList,
    //															out IEnumerable<Structure> sorted_structureList)
    //	{

    //		#region organize structures into ordered lists

    //		// lists for structures
    //		List<Structure> gtvList = new List<Structure>();
    //		List<Structure> ctvList = new List<Structure>();
    //		List<Structure> itvList = new List<Structure>();
    //		List<Structure> ptvList = new List<Structure>();
    //		List<Structure> oarList = new List<Structure>();
    //		List<Structure> targetList = new List<Structure>();
    //		List<Structure> structureList = new List<Structure>();

    //		foreach (var structure in ss.Structures)
    //		{
    //			// conditions for adding any structure
    //			if ((!structure.IsEmpty) &&
    //				(structure.HasSegment) &&
    //				(!structure.Id.Contains("*")) &&
    //				(!structure.Id.ToLower().Contains("markers")) &&
    //				(!structure.Id.ToLower().Contains("avoid")) &&
    //				(!structure.Id.ToLower().Contains("dose")) &&
    //				(!structure.Id.ToLower().Contains("contrast")) &&
    //				(!structure.Id.ToLower().Contains("air")) &&
    //				(!structure.Id.ToLower().Contains("dens")) &&
    //				(!structure.Id.ToLower().Contains("bolus")) &&
    //				(!structure.Id.ToLower().Contains("suv")) &&
    //				(!structure.Id.ToLower().Contains("match")) &&
    //				(!structure.Id.ToLower().Contains("wire")) &&
    //				(!structure.Id.ToLower().Contains("scar")) &&
    //				(!structure.Id.ToLower().Contains("chemo")) &&
    //				(!structure.Id.ToLower().Contains("pet")) &&
    //				(!structure.Id.ToLower().Contains("dnu")) &&
    //				(!structure.Id.ToLower().Contains("fiducial")) &&
    //				(!structure.Id.ToLower().Contains("artifact")) &&
    //				(!structure.Id.StartsWith("z", StringComparison.InvariantCultureIgnoreCase)) &&
    //				(!structure.Id.StartsWith("hs", StringComparison.InvariantCultureIgnoreCase)) &&
    //				(!structure.Id.StartsWith("av", StringComparison.InvariantCultureIgnoreCase)) &&
    //				(!structure.Id.StartsWith("opti ", StringComparison.InvariantCultureIgnoreCase)) &&
    //				(!structure.Id.StartsWith("opti-", StringComparison.InvariantCultureIgnoreCase)))
    //			//(structure.Id.Contains("CI-", StringComparison.InvariantCultureIgnoreCase) == false) && 
    //			//(structure.Id.Contains("R50-", StringComparison.InvariantCultureIgnoreCase) == false) &&
    //			//(structure.Id.Contains("CI_", StringComparison.InvariantCultureIgnoreCase) == false) && 
    //			//(structure.Id.Contains("R50_", StringComparison.InvariantCultureIgnoreCase) == false))
    //			{
    //				if (structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase))
    //				{
    //					gtvList.Add(structure);
    //					structureList.Add(structure);
    //					targetList.Add(structure);
    //				}
    //				if ((structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) ||
    //					(structure.Id.StartsWith("Prost", StringComparison.InvariantCultureIgnoreCase)))
    //				{
    //					ctvList.Add(structure);
    //					structureList.Add(structure);
    //					targetList.Add(structure);
    //				}
    //				if (structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase))
    //				{
    //					itvList.Add(structure);
    //					structureList.Add(structure);
    //					targetList.Add(structure);
    //				}
    //				if (structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase))
    //				{
    //					ptvList.Add(structure);
    //					structureList.Add(structure);
    //					targetList.Add(structure);
    //				}
    //				// conditions for adding breast plan targets
    //				if ((structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) ||
    //					(structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) ||
    //					(structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) ||
    //					(structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)))
    //				{
    //					targetList.Add(structure);
    //					structureList.Add(structure);
    //				}
    //				// conditions for adding oars
    //				if ((!structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase)) &&
    //					(!structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) &&
    //					(!structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase)) &&
    //					(!structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase)) &&
    //					(!structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) &&
    //					(!structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) &&
    //					(!structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase)) &&
    //					(!structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)) &&
    //					(!structure.Id.StartsWith("Scar", StringComparison.InvariantCultureIgnoreCase)) &&
    //					(!structure.Id.ToLower().Contains("carina")))
    //				{
    //					oarList.Add(structure);
    //					structureList.Add(structure);
    //				}
    //			}
    //		}
    //		sorted_gtvList = gtvList.OrderBy(x => x.Id);
    //		sorted_ctvList = ctvList.OrderBy(x => x.Id);
    //		sorted_itvList = itvList.OrderBy(x => x.Id);
    //		sorted_ptvList = ptvList.OrderBy(x => x.Id);
    //		sorted_targetList = targetList.OrderBy(x => x.Id);
    //		sorted_oarList = oarList.OrderBy(x => x.Id);
    //		sorted_structureList = structureList.OrderBy(x => x.Id);

    //		#endregion structure organization and ordering

    //	}
    //}

    public static void cleanAndOrderStructures(StructureSet ss, out IEnumerable<Structure> sorted_gtvList,
                                                        out IEnumerable<Structure> sorted_ctvList,
                                                        out IEnumerable<Structure> sorted_itvList,
                                                        out IEnumerable<Structure> sorted_ptvList,
                                                        out IEnumerable<Structure> sorted_targetList,
                                                        out IEnumerable<Structure> sorted_oarList,
                                                        out IEnumerable<Structure> sorted_structureList,
                                                        out IEnumerable<Structure> sorted_emptyStructuresList)
    {

      #region organize structures into ordered lists

      // lists for structures
      List<Structure> gtvList = new List<Structure>();
      List<Structure> ctvList = new List<Structure>();
      List<Structure> itvList = new List<Structure>();
      List<Structure> ptvList = new List<Structure>();
      List<Structure> oarList = new List<Structure>();
      List<Structure> targetList = new List<Structure>();
      List<Structure> structureList = new List<Structure>();
      List<Structure> emptyStructuresList = new List<Structure>();

      foreach (var structure in ss.Structures)
      {
        // empty contours list
        if (structure.IsEmpty)
        {
          emptyStructuresList.Add(structure);
        }
        // conditions for adding any structure
        else if ((!structure.IsEmpty) &&
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
              (structure.Id.StartsWith("LN_", StringComparison.InvariantCultureIgnoreCase)) ||
              (structure.Id.StartsWith("Cavity", StringComparison.InvariantCultureIgnoreCase) && !structure.Id.ToString().ToLower().Contains("oral")) ||
              (structure.Id.StartsWith("av", StringComparison.InvariantCultureIgnoreCase) && structure.Id.ToLower().Contains("avm")) ||
              (structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)))
          {
            targetList.Add(structure);
            structureList.Add(structure);
          }
          // conditions for adding oars
          if (((!structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("Level I", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("LN_", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("IM LN", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("Supraclav", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("Scar", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.StartsWith("av", StringComparison.InvariantCultureIgnoreCase)) &&
              (!structure.Id.ToLower().Contains("carina"))) ||
              (structure.Id.ToLower().Contains("cavity") && structure.Id.ToLower().Contains("oral")))
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
      sorted_emptyStructuresList = emptyStructuresList.OrderBy(x => x.Id);

      #endregion structure organization and ordering
    }

  }

  #endregion Class Definitions 

  public class Script
  {
    public Script()
    {
    }
    //---------------------------------------------------------------------------------------------  
    #region execute

    public string idMessages;
    public string idMessage;
    public string targetIds;
    public string volumeMessage;
    public string overlaps;
    public string distances;

    public IEnumerable<Structure> sorted_gtvList;
    public IEnumerable<Structure> sorted_ctvList;
    public IEnumerable<Structure> sorted_itvList;
    public IEnumerable<Structure> sorted_ptvList;
    public IEnumerable<Structure> sorted_targetList;
    public IEnumerable<Structure> sorted_oarList;
    public IEnumerable<Structure> sorted_structureList;
    public IEnumerable<Structure> sorted_emptyStructuresList;

    public void Execute(ScriptContext context)
    {
      PlanSetup plan = context.PlanSetup;

      IEnumerator<PlanSetup> availablePlans = null; // context.PlanSumsInScope.GetEnumerator();

      availablePlans = context.PlansInScope.GetEnumerator();

      // NOTE: this has been commented out due to it causing the script to not run for plan sums. 
      // If you want to access this property, please add condition in the event psum is defined as well. 
      // It may  require the TotalPrescribedDose property instead for a psum.
      // thank you!
      //double planPrescribedPercentage = plan.PrescribedPercentage;
      PlanSum psum = context.PlanSumsInScope.FirstOrDefault();
      //         if (context.PlanSetup == null && context.PlanSumsInScope.Count() > 1)
      //         {
      //             throw new ApplicationException("Please close other plan sums");
      //         }
      //if (plan == null && psum == null)
      //{
      //	return;
      //}
      // NOTE: Plans in plansum can have different structuresets but here we only use structureset to allow chosing one structure
      //SelectedPlanningItem = plan != null ? (PlanningItem)plan : (PlanningItem)psum;
      //SelectedStructureSet = plan != null ? plan.StructureSet : psum.PlanSetups.First().StructureSet;

      // NOTE: this will cause the script to close if there is no dose calculated
      // removed so overlap can be calculated before running a plan
      //if (SelectedPlanningItem.Dose == null)
      //    return;

      var bodyStructure = context.StructureSet.Structures.Single(st => st.DicomType == "EXTERNAL");
      var bodyDVH = plan.GetDVHCumulativeData(bodyStructure, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.01);

      var msg = string.Empty;

      var counter = 0;


      foreach (var point in bodyDVH.CurveData)
      {
        if (counter < 20)
        {
          msg += "D_" + Math.Round(point.DoseValue.Dose, 2).ToString() + "\r\n";
        }
        counter += 1;
      }

      MessageBox.Show(msg);

      //var dvh = plan.GetDVHCumulativeData(s, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);





      #region isocenter test

      //         var userOriginX = Math.Round(context.Image.UserOrigin.x / 10, 2);
      //         var userOriginY = Math.Round(context.Image.UserOrigin.y / 10, 2);
      //         var userOriginZ = Math.Round(context.Image.UserOrigin.z / 10, 2);

      //         var fields = plan.Beams;

      //         string fieldName = string.Empty;

      //         double isoX;
      //         double isoY;
      //         double isoZ;

      //         string message = string.Format("User Origin:\n\tX: {0}\n\tY: {1}\n\tZ: {2}\n\n", userOriginX, userOriginY, userOriginZ);

      //var orientation = context.Image.ImagingOrientation;

      //if (orientation == PatientOrientation.HeadFirstProne)
      //{

      //	MessageBox.Show("head first prone");

      //}
      //	foreach (var field in fields)
      //         {
      //             fieldName = field.Id.ToString().Split(':').First();
      //             isoX = Math.Round(field.IsocenterPosition.x, 3);
      //             isoY = Math.Round(field.IsocenterPosition.y, 3);
      //             isoZ = Math.Round(field.IsocenterPosition.z, 3);

      //             message += string.Format("{0} Isocenter:\n\tX:{1}\n\tY:{2}\n\tZ:{3}\n\nOrientation:\n\t{4}", fieldName, isoX, isoY, isoZ, orientation);
      //         }

      //         MessageBox.Show(message, "Isocenter Test");



      #endregion

      #region jaw tracking test

      #region current method


      var result = "";

      var usesJawTracking = plan.OptimizationSetup.UseJawTracking;

      //var opti = plan.OptimizationSetup.Parameters.ToList();
      var opti = plan.OptimizationSetup.Objectives.ToList();
      var test = string.Empty;
      foreach (var val in opti)
        test += val.ToString() + "\r\n";

      //MessageBox.Show(usesJawTracking == true ? "true" : "false");

      if (usesJawTracking)
      {
        var fields = plan.Beams;

        foreach (var field in plan.Beams)
        {
          // get control points
          var cps = field.ControlPoints;
          var cp1 = cps[0];

          // define each x / y position for first control point
          var _x1 = string.Format("{0:N1}", Math.Round((decimal)(cp1.JawPositions.X1) / -10, 2));
          var _y1 = string.Format("{0:N1}", Math.Round((decimal)(cp1.JawPositions.Y1) / -10, 2));
          var _y2 = string.Format("{0:N1}", Math.Round((decimal)(cp1.JawPositions.Y2) / 10, 2));
          var _x2 = string.Format("{0:N1}", Math.Round((decimal)(cp1.JawPositions.X2) / 10, 2));

          // get smallest equivalent field size
          #region smallest equivalent field size

          double maxX = 0;
          double maxY = 0;
          double maxXY = 0;
          double maxX1 = 0;
          double maxX2 = 0;
          double maxY1 = 0;
          double maxY2 = 0;
          double maxEquiv = 0;
          var maxCp = 0;

          double minX = 1000000000000000;
          double minY = 1000000000000000;
          double minXY = 1000000000000000;
          double minX1 = 1000000000000000;
          double minX2 = 1000000000000000;
          double minY1 = 1000000000000000;
          double minY2 = 1000000000000000;
          double minEquiv = 1000000000000000;
          var minCp = 0;

          for (var i = 0; i < cps.Count(); i++)
          {
            maxX = (Math.Abs(cps[i].JawPositions.X1) + Math.Abs(cps[i].JawPositions.X2));
            maxY = (Math.Abs(cps[i].JawPositions.Y1) + Math.Abs(cps[i].JawPositions.Y2));
            if (maxX * maxY >= maxXY)
            {
              maxXY = maxX * maxY;
              maxX1 = cps[i].JawPositions.X1 / 10;
              maxX2 = cps[i].JawPositions.X2 / 10;
              maxY1 = cps[i].JawPositions.Y1 / 10;
              maxY2 = cps[i].JawPositions.Y2 / 10;

              maxEquiv = Math.Round(Math.Sqrt((maxX * maxY)) / 10, 1);
              maxCp = i + 1;
            }

            minX = (Math.Abs(cps[i].JawPositions.X1) + Math.Abs(cps[i].JawPositions.X2));
            minY = (Math.Abs(cps[i].JawPositions.Y1) + Math.Abs(cps[i].JawPositions.Y2));
            if (minX * minY <= minXY)
            {
              minXY = minX * minY;
              minX1 = cps[i].JawPositions.X1 / 10;
              minX2 = cps[i].JawPositions.X2 / 10;
              minY1 = cps[i].JawPositions.Y1 / 10;
              minY2 = cps[i].JawPositions.Y2 / 10;

              minEquiv = Math.Round(Math.Sqrt((minX * minY)) / 10, 1);
              minCp = i + 1;
            }
          }

          #endregion smallest equivalent field size

          if (usesJawTracking)
          {
            result += string.Format("\r\nJawTracking IS used: X1: {0}\tX2: {1}\tY1: {2}\tY2: {3}\t",
                                         _x1, _x2, _y1, _y2);
          }
          else { result += "\r\nJawTracking is NOT used."; }

          // define test result string
          result += string.Format("{0}: Yes\r\n" +
                                      "Initial FS:\tX1: {1}\r\n\t\tX2: {2}\r\n\t\tY1: {3}\r\n\t\tY2: {4}\r\n" +
                                      "Smallest Eq FS:\t{6}x{7} CP[{5}]\r\n" +
                                      "Largest Eq FS:\t{9}x{10} CP[{8}]\r\n\r\n", field.Id, _x1, _x2, _y1, _y2,
                                      minCp, minEquiv, minEquiv, maxCp, maxEquiv, maxEquiv);
        }
        result = result.Remove(result.LastIndexOf("\r\n"));
      }
      else { result = plan.Id + "\r\nJawTracking: No"; }

      //MessageBox.Show(result);

      #endregion current method

      #region old jawtracking code
      //string isJawTracking = "";

      //JawTracking.IsJawTracking(availablePlans, out isJawTracking);

      //MessageBox.Show(isJawTracking);
      //var planFields = plan.Beams;
      //foreach (var field in planFields)
      //{
      //	bool usesJawTracking = false;

      //	if ((field.MLCPlanType.ToString() == "VMAT") || (field.MLCPlanType.ToString() == "DoseDynamic"))
      //	{
      //		var cp1 = field.ControlPoints[0];
      //		var _x1 = string.Format("{0:N1}", Math.Round((decimal)(cp1.JawPositions.X1) / -10, 2));
      //		var _y1 = string.Format("{0:N1}", Math.Round((decimal)(cp1.JawPositions.Y1) / -10, 2));
      //		var _y2 = string.Format("{0:N1}", Math.Round((decimal)(cp1.JawPositions.Y2) / 10, 2));
      //		var _x2 = string.Format("{0:N1}", Math.Round((decimal)(cp1.JawPositions.X2) / 10, 2));

      //		var controlPoints = field.ControlPoints.ToList();
      //		for (var i = 1; i < controlPoints.Count - 1; i++)
      //		{
      //			if (controlPoints[i].JawPositions.X1 != controlPoints[i - 1].JawPositions.X1)
      //			{
      //				usesJawTracking = true;
      //				continue;
      //			}
      //			if (controlPoints[i].JawPositions.X2 != controlPoints[i - 1].JawPositions.X2)
      //			{
      //				usesJawTracking = true;
      //				continue;
      //			}
      //			if (controlPoints[i].JawPositions.Y1 != controlPoints[i - 1].JawPositions.Y1)
      //			{
      //				usesJawTracking = true;
      //				continue;
      //			}
      //			if (controlPoints[i].JawPositions.Y2 != controlPoints[i - 1].JawPositions.Y2)
      //			{
      //				usesJawTracking = true;
      //				continue;
      //			}
      //		}
      //		if (usesJawTracking) { MessageBox.Show(string.Format("JawTracking IS used: X1: {0}\tX2: {1}\tY1: {2}\tY2: {3}\t",
      //															_x1, _x2, _y1, _y2), "JawTracking Test"); }
      //		//else { MessageBox.Show("JawTracking is NOT used."); }
      //	}
      //}
      #endregion old

      #endregion

      #region patient positioning information test

      #region orientation
      //         PatientOrientation pOrientation;

      //PatientPositioningInformation.patientOrientation(context.Image, out pOrientation);

      //MessageBox.Show(pOrientation.ToString());
      #endregion patient orientation

      #region isocenter shift from origin

      //string isSingleIso = "";
      //var shiftX = "";
      //var shiftY = "";
      //var shiftZ = "";

      //PatientPositioningInformation.isocenterShiftFromOrigin(context.Image, plan, out isSingleIso, out shiftX, out shiftY, out shiftZ);

      //MessageBox.Show(string.Format("Is Single Iso:\t{0}\n" +
      //								"Lateral Shift:\t{1}\n" +
      //								"Vertical Shift:\t{2}\n" +
      //								"Longitudinal Shift:\t{3}", isSingleIso, shiftX, shiftY, shiftZ));

      #endregion

      #endregion

      #region primary physician test

      //string tempPhysicianId = context.Patient.PrimaryOncologistId;
      //string primaryOnc = "";

      //GetPrimaryPhysician.primaryPhysician(tempPhysicianId, out primaryOnc);

      //MessageBox.Show("Primary Physician:\t" + primaryOnc, "Primary Physician Test");

      #endregion

      #region structure lists test

      //IEnumerable<Structure> sorted_gtvList;
      //IEnumerable<Structure> sorted_ctvList;
      //IEnumerable<Structure> sorted_itvList;
      //IEnumerable<Structure> sorted_ptvList;
      //IEnumerable<Structure> sorted_targetList;
      //IEnumerable<Structure> sorted_oarList;
      //IEnumerable<Structure> sorted_structureList;

      //GenerateStructureLists.cleanAndOrderStructures(context.PlanSetup.StructureSet, out sorted_gtvList, out sorted_ctvList, out sorted_itvList,
      //																				out sorted_ptvList, out sorted_targetList, out sorted_oarList, out sorted_structureList, out sorted_emptyStructuresList);
      //         var result = string.Empty;
      //         foreach (var t in sorted_targetList)
      //         {
      //             result += t.Id + "\n";
      //         }
      //         MessageBox.Show(result);

      //var gtvMessage = "GTVs:\t\t";
      //var ctvMessage = "CTVs:\t\t";
      //var itvMessage = "ITVs:\t\t";
      //var ptvMessage = "PTVs:\t\t";
      //var targetsMessage = "Targets:\t\t";
      //var oarsMessage = "OARs:\t\t";
      //var structuresMessage = "Structures:\t";

      //foreach (var item in sorted_gtvList)
      //{
      //	gtvMessage += item.Id + ", ";
      //}
      //gtvMessage = gtvMessage.TrimEnd(' ');
      //gtvMessage = gtvMessage.TrimEnd(',');

      //foreach (var item in sorted_ctvList)
      //{
      //	ctvMessage += item.Id + ", ";
      //}
      //ctvMessage = ctvMessage.TrimEnd(' ');
      //ctvMessage = ctvMessage.TrimEnd(',');

      //foreach (var item in sorted_itvList)
      //{
      //	itvMessage += item.Id + ", ";
      //}
      //itvMessage = itvMessage.TrimEnd(' ');
      //itvMessage = itvMessage.TrimEnd(',');

      //foreach (var item in sorted_ptvList)
      //{
      //	ptvMessage += item.Id + ", ";
      //}
      //ptvMessage = ptvMessage.TrimEnd(' ');
      //ptvMessage = ptvMessage.TrimEnd(',');

      //foreach (var item in sorted_targetList)
      //{
      //	targetsMessage += item.Id + ", ";
      //}
      //targetsMessage = targetsMessage.TrimEnd(' ');
      //targetsMessage = targetsMessage.TrimEnd(',');

      //foreach (var item in sorted_oarList)
      //{
      //	oarsMessage += item.Id + ", ";
      //}
      //oarsMessage = oarsMessage.TrimEnd(' ');
      //oarsMessage = oarsMessage.TrimEnd(',');

      //foreach (var item in sorted_structureList)
      //{
      //	structuresMessage += item.Id + ", ";
      //}
      //structuresMessage = structuresMessage.TrimEnd(' ');
      //structuresMessage = structuresMessage.TrimEnd(',');

      //var result = gtvMessage + "\n" +
      //				ctvMessage + "\n" +
      //				itvMessage + "\n" +
      //				ptvMessage + "\n" +
      //				targetsMessage + "\n" +
      //				oarsMessage + "\n" +
      //				structuresMessage;

      //MessageBox.Show(result, "Structure List Test");


      #endregion

      ////var originalMessage = "";
      ////var newMessage = "";
      //idMessage = "test";
      //targetIds = "";
      //volumeMessage = "";
      //overlaps = "";
      //distances = "";

      ////Thread t1 = new Thread(getStructureIds);
      ////t1.Start();

      #region arc direction check test

      //string technique = "";

      //if (plan is BrachyPlanSetup)
      //{
      //	string returnText = "";
      //	BrachyPlanSetup brachy = plan as BrachyPlanSetup;
      //	if (brachy.NumberOfPdrPulses != null)
      //	{
      //		returnText = "PDR";
      //	}
      //	else
      //	{
      //		Catheter c = brachy.Catheters.FirstOrDefault();
      //		if (c != null)
      //		{
      //			returnText = c.TreatmentUnit.DoseRateMode;
      //		}
      //	}
      //}
      //else
      //{
      //	foreach (Beam beam in plan.Beams)
      //	{
      //		if (!beam.IsSetupField)
      //		{
      //			if (beam.GantryDirection != VMS.TPS.Common.Model.Types.GantryDirection.None)
      //			{
      //				if (beam.MLCPlanType == VMS.TPS.Common.Model.Types.MLCPlanType.VMAT) technique = "VMAT";
      //				else technique = "ARC";
      //			}
      //			else
      //			{
      //				if (beam.MLCPlanType == VMS.TPS.Common.Model.Types.MLCPlanType.DoseDynamic) technique = "IMRT";
      //				else technique = "STATIC";
      //			}
      //		}
      //	}
      //}



      //if ((technique == "VMAT") || (technique == "ARC"))
      //{
      //	string arcId = "";
      //	string gantryDirection = "";
      //	int arcCount = 0;
      //	int ccwCounter = 0;
      //	int cwCounter = 0;
      //	//List<string> arcDirections = new List<string>();
      //	string arcDirectionWarning = "Arc Directions OK";

      //	foreach (Beam beam in plan.Beams)
      //	{
      //		arcCount++;
      //		arcId = beam.Id.ToString();
      //		gantryDirection = beam.GantryDirection.ToString();
      //		//arcDirections.Add(gantryDirection);

      //		var controlPoints = beam.ControlPoints;
      //		var arcStartAngle = controlPoints[0].GantryAngle;
      //		var arcStopAngle = controlPoints.Last().GantryAngle;
      //		var arcLength = beam.ArcLength;

      //		if (gantryDirection == "CounterClockwise") { ccwCounter++; }
      //		if (gantryDirection == "Clockwise") { cwCounter++; }

      //		MessageBox.Show(string.Format("Arc {0}:\r\n" +
      //										"\tId:\t{1}\r\n" +
      //										"\tDirection:\t{2}\r\n" +
      //										"\tStart:\t{3}\r\n" +
      //										"\tStop:\t{4}\r\n", arcCount, arcId, gantryDirection, arcStartAngle.ToString(), arcStopAngle.ToString()), "Arc Information");
      //	}

      //	if (arcCount == 2)
      //	{
      //		if (ccwCounter == 2) { arcDirectionWarning = "Both arcs are CCW"; }
      //		else if (cwCounter == 2) { arcDirectionWarning = "Both arcs are CW"; }
      //	}

      //	else if (arcCount == 3)
      //	{
      //		if (ccwCounter == 3) { arcDirectionWarning = "All arcs are CCW"; }
      //		else if (cwCounter == 3) { arcDirectionWarning = "All arcs are CW"; }
      //	}

      //	MessageBox.Show(arcDirectionWarning);
      //}

      #endregion

      #region isAppa test

      ////possible alternate:
      //var beams = plan.Beams.ToList();
      //var message = "";
      ////if (beams.Count() == 2)
      ////{
      ////	var gaBeam1 = beams[0].ControlPoints[0].GantryAngle;
      ////	var gaBeam2 = beams[1].ControlPoints[0].GantryAngle;

      ////	MessageBox.Show(string.Format("Gantry 1:\t{0}\r\n" +
      ////									"Gantry 2:\t{1}", gaBeam1, gaBeam2));

      ////	if (((((gaBeam1 >= 359) && (gaBeam1 < 360)) || ((gaBeam1 >= 0) && (gaBeam1 < 1))) && (((gaBeam2 > 179) && (gaBeam2 <= 180)) || ((gaBeam2 > 180) && (gaBeam2 < 181)))) ||
      ////		((((gaBeam2 > 359) && (gaBeam2 < 360)) || ((gaBeam2 >= 0) && (gaBeam2 < 1))) && (((gaBeam1 > 179) && (gaBeam1 <= 180)) || ((gaBeam1 > 180) && (gaBeam1 < 181)))))
      ////	{
      ////		message = "APPA";
      ////	}
      ////}

      ////is APPPA ? - current method - returns appa always if beams are opposed +- 1 degree, no matter the direction (laterals still return appa)
      //double angle1 = -1001;
      //double angle2 = -1001;
      //foreach (Beam beam in plan.Beams)
      //{
      //	if (!beam.IsSetupField)
      //	{
      //		if (beam.GantryDirection == VMS.TPS.Common.Model.Types.GantryDirection.None) //if not an arc
      //		{

      //			if (angle1 < -1000) angle1 = beam.ControlPoints.FirstOrDefault().GantryAngle;
      //			else angle2 = beam.ControlPoints.FirstOrDefault().GantryAngle; // does this return the same angle for angle2 as angle1? (matt)

      //			MessageBox.Show(string.Format("Gantry 1:\t{0}\r\n" +
      //											"Gantry 2:\t{1}", angle1, angle2));

      //			//NOTE: possible alternative... (matt)
      //			//var beams = plan.Beams.ToList();
      //			//var message = "";
      //			//if (beams.Count() == 2)
      //			//{
      //			//	var gaBeam1 = beams[0].ControlPoints[0].GantryAngle;
      //			//	var gaBeam2 = beams[1].ControlPoints[0].GantryAngle;

      //			//	MessageBox.Show(string.Format("Gantry 1:\t{0}\r\n" +
      //			//									"Gantry 2:\t{1}", gaBeam1, gaBeam2));

      //			//	if (((((gaBeam1 >= 359) && (gaBeam1 < 360)) || ((gaBeam1 >= 0) && (gaBeam1 < 1))) && (((gaBeam2 > 179) && (gaBeam2 <= 180)) || ((gaBeam2 > 180) && (gaBeam2 < 181)))) ||
      //			//		((((gaBeam2 > 359) && (gaBeam2 < 360)) || ((gaBeam2 >= 0) && (gaBeam2 < 1))) && (((gaBeam1 > 179) && (gaBeam1 <= 180)) || ((gaBeam1 > 180) && (gaBeam1 < 181)))))
      //			//	{
      //			//		message = "APPA";
      //			//	}
      //			//}
      //		}
      //	}
      //}

      //if ((Math.Abs(angle1 - angle2) > 179) && (Math.Abs(angle1 - angle2) < 181)) message = "APPA"; // does this return appa if the angles were 90 and 270? (matt) 

      //MessageBox.Show(message);

      #endregion

      #region beam description test

      //PatientOrientation txOrientation = plan.TreatmentOrientation;
      ////string beamDescription = "";
      //var fields = plan.Beams.ToList();
      //var message = "";
      //var pass = "";
      //result = "";

      //foreach (Beam field in fields)
      //{
      //	var fieldDescriptor = "";


      //	var beamId = field.Id.ToString();
      //	var gantryAngle = field.ControlPoints[0].GantryAngle;
      //	var fieldTechnique = field.Technique;

      //	var ga_359to1 = (((gantryAngle >= 359) && (gantryAngle <= 360)) || ((gantryAngle >= 0) && (gantryAngle <= 1)));
      //	var ga_181to179 = ((gantryAngle >= 179) && (gantryAngle <= 181));
      //	var ga_269to271 = ((gantryAngle >= 269) && (gantryAngle <= 271));
      //	var ga_89to91 = ((gantryAngle >= 89) && (gantryAngle <= 91));
      //	var ga_359to271 = ((gantryAngle < 359) && (gantryAngle > 271));
      //	var ga_1to89 = ((gantryAngle > 1) && (gantryAngle < 89));
      //	var ga_269to181 = ((gantryAngle < 269) && (gantryAngle > 181));
      //	var ga_91to179 = ((gantryAngle > 91) && (gantryAngle < 179));

      //	#region field descriptor logic

      //	if (txOrientation == PatientOrientation.HeadFirstSupine)
      //	{
      //		if (ga_359to1)
      //		{
      //			fieldDescriptor = "AP";
      //		}
      //		else if (ga_181to179)
      //		{
      //			fieldDescriptor = "PA";
      //		}
      //		else if (ga_269to271)
      //		{
      //			fieldDescriptor = "RLAT";
      //		}
      //		else if (ga_89to91)
      //		{
      //			fieldDescriptor = "LLAT";
      //		}
      //		else if (ga_359to271)
      //		{
      //			fieldDescriptor = "RAO";
      //		}
      //		else if (ga_1to89)
      //		{
      //			fieldDescriptor = "LAO";
      //		}
      //		else if (ga_269to181)
      //		{
      //			fieldDescriptor = "RPO";
      //		}
      //		else if (ga_91to179)
      //		{
      //			fieldDescriptor = "LPO";
      //		}
      //	}
      //	else if (txOrientation == PatientOrientation.HeadFirstProne)
      //	{
      //		if (ga_359to1)
      //		{
      //			fieldDescriptor = "PA";
      //		}
      //		else if (ga_181to179)
      //		{
      //			fieldDescriptor = "AP";
      //		}
      //		else if (ga_269to271)
      //		{
      //			fieldDescriptor = "LLAT";
      //		}
      //		else if (ga_89to91)
      //		{
      //			fieldDescriptor = "RLAT";
      //		}
      //		else if (ga_359to271)
      //		{
      //			fieldDescriptor = "RPO";
      //		}
      //		else if (ga_1to89)
      //		{
      //			fieldDescriptor = "LPO";
      //		}
      //		else if (ga_269to181)
      //		{
      //			fieldDescriptor = "RAO";
      //		}
      //		else if (ga_91to179)
      //		{
      //			fieldDescriptor = "LAO";
      //		}
      //	}
      //	else if (txOrientation == PatientOrientation.FeetFirstSupine)
      //	{
      //		if (ga_359to1)
      //		{
      //			fieldDescriptor = "AP";
      //		}
      //		else if (ga_181to179)
      //		{
      //			fieldDescriptor = "PA";
      //		}
      //		else if (ga_269to271)
      //		{
      //			fieldDescriptor = "LLAT";
      //		}
      //		else if (ga_89to91)
      //		{
      //			fieldDescriptor = "RLAT";
      //		}
      //		else if (ga_359to271)
      //		{
      //			fieldDescriptor = "LAO";
      //		}
      //		else if (ga_1to89)
      //		{
      //			fieldDescriptor = "RAO";
      //		}
      //		else if (ga_269to181)
      //		{
      //			fieldDescriptor = "LPO";
      //		}
      //		else if (ga_91to179)
      //		{
      //			fieldDescriptor = "RPO";
      //		}
      //	}
      //	else if (txOrientation == PatientOrientation.FeetFirstProne)
      //	{
      //		if (ga_359to1)
      //		{
      //			fieldDescriptor = "PA";
      //		}
      //		else if (ga_181to179)
      //		{
      //			fieldDescriptor = "AP";
      //		}
      //		else if (ga_269to271)
      //		{
      //			fieldDescriptor = "RLAT";
      //		}
      //		else if (ga_89to91)
      //		{
      //			fieldDescriptor = "LLAT";
      //		}
      //		else if (ga_359to271)
      //		{
      //			fieldDescriptor = "RPO";
      //		}
      //		else if (ga_1to89)
      //		{
      //			fieldDescriptor = "LPO";
      //		}
      //		else if (ga_269to181)
      //		{
      //			fieldDescriptor = "RAO";
      //		}
      //		else if (ga_91to179)
      //		{
      //			fieldDescriptor = "LAO";
      //		}
      //	}
      //	else if (txOrientation == PatientOrientation.NoOrientation)
      //	{
      //		fieldDescriptor = "Patient Orientation not defined. Verify field orientation.";
      //	}
      //	else if (txOrientation == PatientOrientation.Sitting)
      //	{
      //		fieldDescriptor = "Patient is sitting. Verify field orientation.";
      //	}
      //	else
      //	{
      //		fieldDescriptor = "Patient in decubitus position. Verify field orientation.";
      //	}

      //	#endregion

      //	#region checks for ID and Field Orientation agreement

      //	if (field.Id.ToString().ToLower().Contains("rlat"))
      //	{
      //		if (fieldDescriptor != "RLAT") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor); }
      //	}
      //	else if (field.Id.ToString().ToLower().Contains("llat"))
      //	{
      //		if (fieldDescriptor != "LLAT") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor); }
      //	}
      //	else if (field.Id.ToString().ToLower().Contains("ap"))
      //	{
      //		if (fieldDescriptor != "AP") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor); }
      //	}
      //	else if (field.Id.ToString().ToLower().Contains("pa"))
      //	{
      //		if (fieldDescriptor != "PA") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor); }
      //	}
      //	else if (field.Id.ToString().ToLower().Contains("rao"))
      //	{
      //		if (fieldDescriptor != "RAO") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor); }
      //	}
      //	else if (field.Id.ToString().ToLower().Contains("rpo"))
      //	{
      //		if (fieldDescriptor != "RPO") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor); }
      //	}
      //	else if (field.Id.ToString().ToLower().Contains("lao"))
      //	{
      //		if (fieldDescriptor != "LAO") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor); }
      //	}
      //	else if (field.Id.ToString().ToLower().Contains("lpo"))
      //	{
      //		if (fieldDescriptor != "LPO") { pass = "Warning"; result += string.Format("{0}: Orientation ({1}) and ID do not agree\r\n\r\n", field.Id.ToString(), fieldDescriptor); }
      //	}

      //	#endregion

      //	message += string.Format("{0}:          \t{1}\t{2}\t{3}\r\n", beamId, gantryAngle, fieldDescriptor, fieldTechnique);


      //	//message += string.Format("beam: {0}\r\n\tdirection: {1}\r\n", field.Id, field.GantryDirection);

      //}
      //if (pass == "") { }
      //if (result == "") { }
      //message += "\r\n";

      //foreach (Beam field in fields)
      //{

      //	var fieldDescriptor = "";
      //	var beamId = field.Id.ToString();
      //	var startAngle = field.ControlPoints[0].GantryAngle;
      //	var stopAngle = field.ControlPoints.Last().GantryAngle;
      //	var gantryDirection = field.GantryDirection;

      //	if (gantryDirection != GantryDirection.None)
      //	{
      //		if (gantryDirection == GantryDirection.Clockwise) { fieldDescriptor = "CW"; }
      //		else if (gantryDirection == GantryDirection.CounterClockwise) { fieldDescriptor = "CCW"; }
      //		//else if (gantryDirection == GantryDirection.None) { fieldDescriptor = "Gantry Direction not defined."; }

      //		if (field.Id.ToString().ToLower().Contains("ccw"))
      //		{
      //			if (fieldDescriptor != "CCW") { fieldDescriptor = "Warning: Arc Direction and ID do not agree."; }
      //		}
      //		if (field.Id.ToString().ToLower().Contains("cw"))
      //		{
      //			if (fieldDescriptor != "CW") { fieldDescriptor = "Warning: Arc Direction and ID do not agree."; }
      //		}

      //		message += string.Format("{0}:      \t{1};\tStart: {2}\tStop: {3};\r\n", beamId, fieldDescriptor, startAngle, stopAngle);
      //	}
      //}

      //MessageBox.Show(message, "Beam Description Test");

      #endregion

      #region dicomType test

      ////foreach (Structure s in plan.StructureSet.Structures.Where(x => x.DicomType.ToString().ToLower() == "ptv")
      //message = "\r\n\r\n";
      //foreach (Structure s in plan.StructureSet.Structures)
      //{
      //	message += string.Format("{0}: {1}\r\n", s.Id, s.DicomType);
      //}
      //MessageBox.Show(message, "DicomType Test");

      //double offBodyCenter = 0;
      //bool isRightSided = false;

      //checkLaterality(plan, 0, out offBodyCenter, out isRightSided);



      #endregion

      #region multithread test


      //Thread t1 = new Thread(() =>
      //{
      //	getStructureIds(sorted_structureList, idMessage);
      //	MessageBox.Show(idMessage, "MultiThread Test - Thread1");
      //});
      //t1.Start();



      //try
      //{
      //	Thread t1 = new Thread(getStructureIds);
      //	t1.Start();
      //}
      //catch (Exception e)
      //{
      //	MessageBox.Show(e.ToString());
      //}

      //getVolumes();
      //try
      //{
      //	Thread t2 = new Thread(getTargetIds);
      //	t2.Start();
      //}
      //catch (Exception e)
      //{
      //	MessageBox.Show(e.ToString());
      //}



      //Thread t3 = new Thread(getVolumes);
      //t3.Start();

      //Thread t4 = new Thread(getOverlaps);
      //t4.Start();

      //Thread t5 = new Thread(getDistances);
      //t5.Start();


      //MessageBox.Show(idMessage, "MultiThread Test - Thread1");
      //MessageBox.Show(targetIds, "MultiThread Test - Thread2");
      //MessageBox.Show(volumeMessage, "MultiThread Test - Thread3");
      //MessageBox.Show(overlaps, "MultiThread Test - Thread4");
      //MessageBox.Show(distances, "MultiThread Test - Thread5");



      #endregion

      #region emptyContourCheck test

      //result = "";
      //foreach (Structure s in (context.PlanSetup.StructureSet.Structures).Where(x => x.IsEmpty))
      //{
      //	result += string.Format("{0}\r\n", s.Id.ToString());
      //}
      //MessageBox.Show(result, "Empty Contour Check");

      #endregion

      #region show MUs

      var spacer = new string(' ', 4);

      //var _fields = plan.Beams.ToList();
      //var _m = string.Empty;

      //foreach (var field in _fields)
      //{
      //	var mu = Math.Round(field.Meterset.Value, 3);
      //	_m += string.Format("{0}{1}{2}\n", field.Id, spacer, mu);
      //}
      //MessageBox.Show(_m);

      #endregion

      #region field properties example

      ////var _m = string.Empty;
      //var tuplesList = new List<List<Tuple<string, string>>>();

      //foreach (var beam in context.PlanSetup.Beams.ToList().Where(x => !x.IsSetupField))
      //{
      //	var tuples = new List<Tuple<string, string>>();
      //	var cps = beam.ControlPoints.ToList();
      //	var beamId = beam.Id; tuples.Add(Tuple.Create("Field Id", beamId));
      //	var technique = beam.Technique.Id; tuples.Add(Tuple.Create("Technique", technique));
      //	var machine = beam.TreatmentUnit.Id; tuples.Add(Tuple.Create("Machine", machine));
      //	var scale = beam.TreatmentUnit.MachineScaleDisplayName; tuples.Add(Tuple.Create("Scale", scale));
      //	var energy = beam.EnergyModeDisplayName; tuples.Add(Tuple.Create("Energy", energy));
      //	var doseRate = beam.DoseRate; tuples.Add(Tuple.Create("Dose Rate [MU/min]", doseRate.ToString()));
      //	var mu = beam.Meterset.Value; tuples.Add(Tuple.Create("MU", mu.ToString("0.000")));
      //	var ssd = beam.SSD; tuples.Add(Tuple.Create("SSD", ssd.ToString("0.0")));
      //	var gantryAngle = beam.ControlPoints[0].GantryAngle; tuples.Add(Tuple.Create("Gantry Angle", gantryAngle.ToString("0.0")));
      //	var collAngle = beam.ControlPoints[0].CollimatorAngle; tuples.Add(Tuple.Create("Coll. Angle", collAngle.ToString("0.0")));
      //	var x1 = Math.Round(beam.ControlPoints[0].JawPositions.X1 / 10, 1); //tuples.Add(Tuple.Create("X1", x1.ToString("0.0")));
      //	var x2 = Math.Round(beam.ControlPoints[0].JawPositions.X2 / 10, 1); //tuples.Add(Tuple.Create("X2", x2.ToString("0.0")));
      //	var y1 = Math.Round(beam.ControlPoints[0].JawPositions.Y1 / 10, 1); //tuples.Add(Tuple.Create("Y1", y1.ToString("0.0")));
      //	var y2 = Math.Round(beam.ControlPoints[0].JawPositions.Y2 / 10, 1); //tuples.Add(Tuple.Create("Y2", y2.ToString("0.0")));

      //	var isJawTracking = false;
      //	for (var i = 1; i < cps.Count(); i++)
      //	{
      //		if (cps[i] != cps[i - 1]) { isJawTracking = true; continue; }
      //	}
      //	if (isJawTracking)
      //	{
      //		double maxX = 0;
      //		double maxY = 0;
      //		double maxXY = 0;
      //		double maxX1 = 0;
      //		double maxX2 = 0;
      //		double maxY1 = 0;
      //		double maxY2 = 0;
      //		double maxEquiv = 0;
      //		var maxCp = 0;

      //		double minX = 1000000000000000;
      //		double minY = 1000000000000000;
      //		double minXY = 1000000000000000;
      //		double minX1 = 1000000000000000;
      //		double minX2 = 1000000000000000;
      //		double minY1 = 1000000000000000;
      //		double minY2 = 1000000000000000;
      //		double minEquiv = 1000000000000000;
      //		var minCp = 0;

      //		for (var i = 0; i < cps.Count(); i++)
      //		{
      //			maxX = (Math.Abs(cps[i].JawPositions.X1) + Math.Abs(cps[i].JawPositions.X2));
      //			maxY = (Math.Abs(cps[i].JawPositions.Y1) + Math.Abs(cps[i].JawPositions.Y2));
      //			if (maxX * maxY >= maxXY)
      //			{
      //				maxXY = maxX * maxY;
      //				maxX1 = cps[i].JawPositions.X1 / 10;
      //				maxX2 = cps[i].JawPositions.X2 / 10;
      //				maxY1 = cps[i].JawPositions.Y1 / 10;
      //				maxY2 = cps[i].JawPositions.Y2 / 10;

      //				maxEquiv = Math.Round(Math.Sqrt((maxX * maxY)) / 10, 1);
      //				maxCp = i + 1;
      //			}

      //			minX = (Math.Abs(cps[i].JawPositions.X1) + Math.Abs(cps[i].JawPositions.X2));
      //			minY = (Math.Abs(cps[i].JawPositions.Y1) + Math.Abs(cps[i].JawPositions.Y2));
      //			if (minX * minY <= minXY)
      //			{
      //				minXY = minX * minY;
      //				minX1 = cps[i].JawPositions.X1 / 10;
      //				minX2 = cps[i].JawPositions.X2 / 10;
      //				minY1 = cps[i].JawPositions.Y1 / 10;
      //				minY2 = cps[i].JawPositions.Y2 / 10;

      //				minEquiv = Math.Round(Math.Sqrt((minX * minY)) / 10, 1);
      //				minCp = i + 1;
      //			}


      //			//if (Math.Abs(cps[i].JawPositions.X1) > maxX1) {	maxX1 = cps[i].JawPositions.X1; }
      //			//if (Math.Abs(cps[i].JawPositions.X2) > maxX1) { maxX2 = cps[i].JawPositions.X2; }
      //			//if (Math.Abs(cps[i].JawPositions.Y1) > maxY1) { maxY1 = cps[i].JawPositions.Y1; }
      //			//if (Math.Abs(cps[i].JawPositions.Y2) > maxY2) { maxY2 = cps[i].JawPositions.Y2; }
      //			//if (Math.Abs(cps[i].JawPositions.X1) < minX1) { minX1 = cps[i].JawPositions.X1; }
      //			//if (Math.Abs(cps[i].JawPositions.X2) < minX2) { minX2 = cps[i].JawPositions.X2; }
      //			//if (Math.Abs(cps[i].JawPositions.Y1) < minY1) { minY1 = cps[i].JawPositions.Y1; }
      //			//if (Math.Abs(cps[i].JawPositions.Y2) < minY2) { minY2 = cps[i].JawPositions.Y2; }
      //		}
      //		tuples.Add(Tuple.Create("JawTracking", "Yes\r\n-----------------"));
      //		tuples.Add(Tuple.Create("Initial FS", string.Format("X1: {0}, X2: {1}, Y1: {2}, Y2: {3}\r\n", Math.Abs(x1), Math.Abs(x2), Math.Abs(y1), Math.Abs(y2))));
      //		tuples.Add(Tuple.Create("Smallest FS", string.Format("X1: {0}, X2: {1}, Y1: {2}, Y2: {3}, ControlPoint: {4}", Math.Abs(minX1), Math.Abs(minX2), Math.Abs(minY1), Math.Abs(minY2), minCp)));
      //		tuples.Add(Tuple.Create("Smallest EquivFS", string.Format("{0} x {1}\r\n", minEquiv, minEquiv)));
      //		tuples.Add(Tuple.Create("Largest FS", string.Format("X1: {0}, X2: {1}, Y1: {2}, Y2: {3}, ControlPoint: {4}", Math.Abs(maxX1), Math.Abs(maxX2), Math.Abs(maxY1), Math.Abs(maxY2), maxCp)));
      //		tuples.Add(Tuple.Create("Largest EquivFS", string.Format("{0} x {1}\r\n-----------------", maxEquiv, maxEquiv)));
      //	}
      //	else
      //	{
      //		tuples.Add(Tuple.Create("JawTracking", "No"));
      //		tuples.Add(Tuple.Create("X1", x1.ToString()));
      //		tuples.Add(Tuple.Create("X2", x2.ToString()));
      //		tuples.Add(Tuple.Create("Y1", y1.ToString()));
      //		tuples.Add(Tuple.Create("Y2", y2.ToString()));
      //	}
      //	var mlcModel = beam.MLC.Model; tuples.Add(Tuple.Create("MLC Model", mlcModel));
      //	var mlcPlanType = beam.MLCPlanType; tuples.Add(Tuple.Create("MLC Plan Type", mlcPlanType.ToString()));
      //	if (beam.Wedges.ToList().Count > 0)
      //	{
      //		var wedgeId = beam.Wedges.ToList()[0].Id; tuples.Add(Tuple.Create("Wedge", wedgeId));
      //	}
      //	else
      //	{
      //		tuples.Add(Tuple.Create("Wedge", ""));
      //	}
      //	//if (beam.Applicator.Id.First())
      //	//{
      //	//	var applicator = beam.Applicator.Id; tuples.Add(Tuple.Create("Applicator", applicator));
      //	//}
      //	var boluses = beam.Boluses.ToList();
      //	if ((boluses.Count() - 1) >= 0) { for (var i = 0; i < boluses.Count(); i++) { tuples.Add(Tuple.Create("Bolus " + i.ToString(), boluses[i].Id)); } }
      //	else { tuples.Add(Tuple.Create("Bolus", "")); }
      //	var couchRtn = beam.ControlPoints.ToList()[0].PatientSupportAngle; tuples.Add(Tuple.Create("Couch Rtn", couchRtn.ToString()));
      //	var weight = beam.WeightFactor; tuples.Add(Tuple.Create("Weight", weight.ToString("0.000")));
      //	var userX = context.Image.UserOrigin.x / 10;
      //	var userY = context.Image.UserOrigin.y / 10;
      //	var userZ = context.Image.UserOrigin.z / 10;
      //	tuples.Add(Tuple.Create("User Origin", string.Format("{0}, {1}, {2}", Math.Round(userX, 1), Math.Round(userY, 1), Math.Round(userZ, 1))));
      //	var xpos = beam.IsocenterPosition.x / 10; /*tuples.Add(Tuple.Create("Xpos", xpos.ToString()));*/
      //	var ypos = beam.IsocenterPosition.y / 10; /*tuples.Add(Tuple.Create("Ypos", ypos.ToString()));*/
      //	var zpos = beam.IsocenterPosition.z / 10; /*tuples.Add(Tuple.Create("Zpos", zpos.ToString()));*/
      //	tuples.Add(Tuple.Create("Dicom Iso", string.Format("{0}, {1}, {2}", Math.Round(xpos, 1), Math.Round(ypos, 1), Math.Round(zpos, 1))));
      //	tuples.Add(Tuple.Create("Field Iso", string.Format("{0}, {1}, {2}", Math.Round(xpos - userX, 1), Math.Round(ypos - userY, 1), Math.Round(zpos - userZ, 1))));


      //	tuplesList.Add(tuples);
      //	//foreach (var cp in cps)
      //	//{
      //	//	var mlc = cp.LeafPositions;
      //	//}
      //}
      //foreach (var tupleList in tuplesList)
      //{
      //	var m = string.Empty;
      //	var field = string.Empty;
      //	foreach (var tuple in tupleList)
      //	{
      //		if (tuple.Item1 == "Field Id") { field = tuple.Item2; }
      //		m += string.Format("{0}: {1}\r\n", tuple.Item1, tuple.Item2);
      //	}
      //	MessageBox.Show(m, string.Format("Field Prop.- {0}", field));
      //}



      #endregion

      #region optimization parameters test

      //var JawTracking = context.PlanSetup.OptimizationSetup.UseJawTracking;
      //var optiObjectives = context.PlanSetup.OptimizationSetup.Objectives;
      //var optiParameters = context.PlanSetup.OptimizationSetup.Parameters;
      //var m = string.Empty;

      //var xmlPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\_XmlData_\\" + context.Patient.Id + ".xml";

      //XmlTextWriter writer = new XmlTextWriter(xmlPath, System.Text.Encoding.UTF8);
      //writer.Formatting = Formatting.Indented;

      //writer.WriteStartDocument();

      //writer.WriteComment("XML File for Optimization Parameters");

      //writer.WriteStartElement("Optimization");

      #region structure and beam xml
      // Structure and Beam WriteXml don't work
      //foreach (var structure in sorted_structureList)
      //{
      //	MessageBox.Show(structure.GetSchema().ToString());
      //	//structure.WriteXml(writer);
      //}

      //foreach (var beam in context.PlanSetup.Beams.ToList())
      //{
      //	beam.WriteXml(writer);

      //	//var fluence = beam.GetOptimalFluence();

      //	////var pixels = fluence.GetPixels();
      //	////var rowLength = pixels.GetLength(0);
      //	////MessageBox.Show(rowLength.ToString(), beam.Id);
      //	////var colLength = pixels.GetLength(1);
      //	////MessageBox.Show(colLength.ToString(), beam.Id);
      //	//var arrayStr = string.Empty;
      //	//arrayStr += fluence.XSizeMM + "\n";
      //	////for (int i = 0; i < rowLength; i++)
      //	////{
      //	////	for (int j = 0; j < colLength; j++)
      //	////	{
      //	////		arrayStr += string.Format("{0}\n", pixels[i, j]);
      //	////	}
      //	////}
      //	//MessageBox.Show(arrayStr, beam.Id);
      //}
      #endregion structure and beam xml

      //foreach (var obj in optiObjectives)
      //{
      //	obj.WriteXml(writer);

      //	//m += string.Format("{0}\n", obj.Structure);
      //	//m += string.Format("{0}\n", obj.StructureId);
      //}

      //foreach (var param in optiParameters)
      //{
      //	param.WriteXml(writer);

      //	//m += string.Format("{0}\n", optiParameters.First());
      //	//m += string.Format("{0}\n", param.);
      //}
      //writer.WriteEndElement();
      //writer.WriteEndDocument();
      //writer.Flush();
      //writer.Close();

      //XmlReader reader = XmlReader.Create(xmlPath);
      //while (reader.Read())
      //{
      //	//if ((reader.NodeType == XmlNodeType.Text)/* && (reader.Name == "Dose")*/)
      //	//{
      //	//if (reader.HasAttributes)
      //	//{
      //	//m += reader.LocalName + "\n";
      //	//m += reader.GetAttribute(0) + "\n";
      //	//m += reader.GetAttribute("Dose") + "\n";
      //	//m += reader.ReadElementContentAsString() + "\n";

      //	//if (reader.IsStartElement())
      //	//{
      //	//	//return only when you have START tag  
      //	//	switch (reader.NodeType)
      //	//	{
      //	//		case XmlNodeType.Element:
      //	//			m += "Element1 : " + reader.LocalName + "\n";
      //	//			//m += "Text : " + reader.ReadString() + "\n";
      //	//			break;
      //	//		case XmlNodeType.Text:
      //	//			//m += "Element : " + reader.ReadString() + "\n";
      //	//			m += "Text : " + reader.Value + "\n";
      //	//			break;
      //	//	}
      //	//} 
      //	//while (reader.Read())
      //	//{
      //	if (reader.IsStartElement())
      //	{
      //		if (reader.IsEmptyElement) { }
      //			//m += string.Format("empty<{0}/>\n", reader.Name);
      //		else
      //		{
      //			m += string.Format("\"{0}\":\"", reader.Name);
      //			//m += string.Format("{0}\",\n", reader.Value);  //Read the text content of the element.
      //			reader.Read(); // Read the start tag.
      //			if (reader.IsStartElement())  // Handle nested elements.
      //				m += string.Format("{0}\":\"", reader.Name);
      //				m += string.Format("{0}\",\n", reader.Value);  //Read the text content of the element.
      //		}
      //	}
      //	//}

      //	//}

      //}



      //MessageBox.Show("Structures\n-----------------------------------------------------------------------------------\n" + m);

      #endregion opti params test

      #region coverage test

      //var m = string.Empty;
      //foreach (var s in sorted_targetList)
      //{
      //        DVHData dvhAR = ((PlanningItem)context.PlanSetup).GetDVHCumulativeData(s, DoseValuePresentation.Relative, VolumePresentation.AbsoluteCm3, 0.001);
      //    m += string.Format("{0} : {1}\n", s.Id, dvhAR.Coverage);
      //}
      //MessageBox.Show(m);
      #endregion

      #region isodoses test

      //m = string.Empty;
      //plan.DoseValuePresentation = DoseValuePresentation.Absolute;

      //foreach (var dose in plan.Dose.Isodoses)
      //{
      //    //DVHData dvhAR = ((PlanningItem)context.PlanSetup).GetDVHCumulativeData(s, DoseValuePresentation.Relative, VolumePresentation.AbsoluteCm3, 0.001);
      //    var doseLevel = dose.Level.Dose.ToString();
      //    if (dose.MeshGeometry != null)
      //    {
      //        var dbounds = dose.MeshGeometry.Bounds;
      //        if (dbounds != null)
      //        {
      //            var x = dbounds.Size.X;
      //            MessageBox.Show(dbounds.ToString());
      //        }
      //    }
      //    MessageBox.Show(doseLevel);



      //    //m += string.Format("{0} : {1}, {2}, {3}\n", dose.Level.Dose, dose.MeshGeometry.Bounds.Location.X, dose.MeshGeometry.Bounds.Location.Y, dose.MeshGeometry.Bounds.Location.Z);
      //}
      //MessageBox.Show(m);
      #endregion

      #region is3D test

      //var message = "";

      //var hasArc = false;
      //foreach (Beam beam in plan.Beams)
      //{
      //    if (!beam.IsSetupField)
      //    {
      //        if (beam.GantryDirection != VMS.TPS.Common.Model.Types.GantryDirection.None) //if an arc
      //        {
      //            hasArc = true;
      //        }
      //    }
      //}

      //message = "Has Arc: " + hasArc;

      //MessageBox.Show(message);

      #endregion

      #region screen capture test

      //ScreenCapture sc = new ScreenCapture();
      //// capture entire screen, and save it to a file
      //Image img = sc.CaptureScreen();
      //// display image in a Picture control named imageDisplay
      //this.imageDisplay.Image = img;
      //// capture this window, and save it
      //sc.CaptureWindowToFile(this.Handle, "C:\\temp2.gif", ImageFormat.Gif);

      #endregion
    }

    static bool hasArc(PlanSetup plan)
    {
      var hasArc = false;
      foreach (Beam beam in plan.Beams)
      {
        if (!beam.IsSetupField)
        {
          if (beam.GantryDirection != VMS.TPS.Common.Model.Types.GantryDirection.None) //if an arc
          {
            hasArc = true;
          }
        }
      }
      return hasArc;
    }



    #region iso laterality check - for pa vs ap kV - can adapt for use to check possible arc collisions

    public void checkLaterality(PlanSetup plan, double threshold, out double offBodyCenter, out bool isRightSided)
    {
      offBodyCenter = 0; isRightSided = false;

      var beams = plan.Beams;

      Structure s_Body = plan.StructureSet.Structures.Where(x => (x.DicomType.ToLower() == "external")).FirstOrDefault(); // sometimes it may be called external for protocols (matt)

      VVector bodyCenter = s_Body.CenterPoint;


      bool flipped = ((plan.TreatmentOrientation == PatientOrientation.FeetFirstSupine) ||
                (plan.TreatmentOrientation == PatientOrientation.HeadFirstProne) ||
                (plan.TreatmentOrientation == PatientOrientation.HeadFirstProne)); // should this say feet first prone? (matt)


      int componentToCompare = 0;
      if ((plan.TreatmentOrientation == PatientOrientation.HeadFirstDecubitusRight) ||
         (plan.TreatmentOrientation == PatientOrientation.HeadFirstDecubitusLeft) ||
         (plan.TreatmentOrientation == PatientOrientation.FeetFirstDecubitusRight) ||
         (plan.TreatmentOrientation == PatientOrientation.FeetFirstDecubitusLeft))
      {
        componentToCompare = 1;

        flipped = ((plan.TreatmentOrientation == PatientOrientation.HeadFirstDecubitusLeft) ||
              (plan.TreatmentOrientation == PatientOrientation.FeetFirstDecubitusRight));
      }


      var txField = plan.Beams.Where(x => !x.IsSetupField).FirstOrDefault();
      VVector txFieldIso = txField.IsocenterPosition;

      if (flipped)
      {
        if (txFieldIso[componentToCompare] > (bodyCenter[componentToCompare]))
        {
          var shift = Math.Round((txFieldIso[componentToCompare] - bodyCenter[componentToCompare]) / 10, 2);
          if (Math.Abs(shift) > threshold)
          {
            MessageBox.Show(string.Format("Tx Iso is Left sided and > {0} cm from Patient Center; may need AP kV", threshold), "Laterality Test");
          }
          MessageBox.Show(string.Format("txField: {0}\r\n" +
                          "txFieldIso: {1}\r\n" +
                          "bodyCenter: {2}\r\n" +
                          "shift: {3}", txField.Id, txFieldIso[componentToCompare], bodyCenter[componentToCompare], shift), "Laterality Test");
        };
      }
      else
      {
        if (txFieldIso[componentToCompare] < (bodyCenter[componentToCompare]))
        {
          var shift = Math.Round((txFieldIso[componentToCompare] - bodyCenter[componentToCompare]) / 10, 2);
          if (Math.Abs(shift) > threshold)
          {
            MessageBox.Show(string.Format("Tx Iso is right sided and > {0} cm from Patient Center; may need PA kV", threshold), "Laterality Test");
          }
          MessageBox.Show(string.Format("txField: {0}\r\n" +
                          "txFieldIso: {1}\r\n" +
                          "bodyCenter: {2}\r\n" +
                          "shift: {3}", txField.Id, txFieldIso[componentToCompare], bodyCenter[componentToCompare], shift), "Laterality Test");
        };
      }

    }

    #endregion

    static void calculateOverlap(List<Structure> structureList, List<Structure> targetList)
    {
      foreach (Structure target in targetList)
      {
        //Thread t = new Thread(volOverlapFunction(structureList, target));
        //t.Start();
        //foreach (Structure s in structureList)
        //{
        //	var volOverlap = Math.Round(VolumeOverlap(t, s), 3);
        //}
      }





    }
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

    public static double volOverlapFunction(List<Structure> structureList, Structure t)
    {
      double result = 0;
      foreach (Structure s in structureList)
      {
        var volOverlap = Math.Round(VolumeOverlap(t, s), 3);

      }
      return result;

    }

    //public static string getStructureIds(List<Structure> structureList, string originalMessage, out string newMessage)
    //{
    //	newMessage = originalMessage;
    //	foreach(Structure s in structureList)
    //	{
    //		newMessage += s.Id.ToString() + "\r\n";
    //	}
    //	originalMessage = newMessage;
    //}

    //public static string getStructureIds(Structure s, string originalMessage, out string newMessage)
    //{
    //	newMessage = originalMessage;

    //	newMessage += s.Id.ToString() + "\r\n";

    //	originalMessage = newMessage;
    //}

    //public static void getStructureIds()
    //{
    //	MessageBox.Show("test", "Multi Thread Test");
    //}
    //static void getStructureIds(IEnumerable<Structure> structureList, string message)
    //{
    //	var tempmessage = "";
    //	lock(structureList)
    //	{
    //		lock (message)
    //		{
    //			foreach (Structure s in structureList)
    //			{
    //				tempmessage += string.Format("{0}\r\n", s.Id);
    //			}
    //		}

    //	}

    //	//return message;
    //}
    //static void getTargetIds()
    //{
    //	foreach (Structure t in sorted_targetList)
    //	{
    //		targetIds += string.Format("{0}\r\n", t.Id);
    //	}
    //	MessageBox.Show(targetIds, "MultiThread Test - Thread2");

    //}
    //void getVolumes()
    //{
    //	foreach (Structure s in sorted_structureList)
    //	{
    //		volumeMessage += string.Format("{0}: {1}\r\n", s.Id, Math.Round(s.Volume, 3));
    //	}
    //	MessageBox.Show(volumeMessage, "MultiThread Test - Thread3");
    //}
    //void getOverlaps()
    //{
    //	foreach (Structure t in sorted_ptvList)
    //	{
    //		foreach (Structure s in sorted_structureList)
    //		{
    //			overlaps += string.Format("{0} cc of {1} overlaps with {2}\r\n", Math.Round(CalculateOverlap.VolumeOverlap(t, s), 3), s.Id, t.Id, Math.Round(s.Volume, 3));
    //		}
    //	}
    //	MessageBox.Show(overlaps, "MultiThread Test - Thread4");
    //}
    //void getDistances()
    //{
    //	foreach (Structure t in sorted_ptvList)
    //	{
    //		foreach (Structure s in sorted_structureList)
    //		{
    //			overlaps += string.Format("{0} cm between {1} and {2}\r\n", Math.Round(CalculateOverlap.ShortestDistance(t, s), 2), s.Id, t.Id, Math.Round(s.Volume, 3));
    //		}
    //	}
    //	MessageBox.Show(distances, "MultiThread Test - Thread5");
    //}


    #endregion execute
    //---------------------------------------------------------------------------------------------  
    //---------------------------------------------------------------------------------------------  
    //---------------------------------------------------------------------------------------------  
    //---------------------------------------------------------------------------------------------  
    //---------------------------------------------------------------------------------------------  
    //---------------------------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------------  
    //---------------------------------------------------------------------------------------------  
    //---------------------------------------------------------------------------------------------  
  }
}
