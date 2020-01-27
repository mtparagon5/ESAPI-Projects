namespace VMS.TPS
{
  /// <summary>
  /// Method for Calculating R50 ranges -- Based on RTOG Protocol for Lung SBRT
  /// Author: Edi Schreibmann
  /// </summary>
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
}
