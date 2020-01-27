namespace VMS.TPS
{
  using System;
  using VMS.TPS.Common.Model.API;

  /// <summary>
  /// A collection of relatively antiquaited methods previously used for checking constraints and returning string results.
  /// </summary>
  public class DoseConstraint
  {
    public static string max(Structure structure, double d03cc, double maxDose, double maxDoseLimit)
    {
      if (maxDose <= maxDoseLimit) { return string.Format("\tD(0.03cc):\t{0:F3}Gy\t\t|\tMaxDose:\t{1:F3}Gy\t\t|\tLimit: {2}Gy\t|\tMET\t", d03cc, maxDose, maxDoseLimit); }
      else { return string.Format("\tD(0.03cc):\t{0:F3}Gy|\tMaxDose:\t{1:F3}Gy\t|\tLimit: {2}Gy\t|\tNOT MET\t", d03cc, maxDose, maxDoseLimit); }
    }
    public static string max(string structure, double d03cc, double maxDose, double maxDoseLimit)
    {
      if (maxDose <= maxDoseLimit) { return string.Format("\tD(0.03cc):\t{0:F3}Gy\t\t|\tMaxDose:\t{1:F3}Gy\t\t|\tLimit: {2}Gy\t|\tMET\t", d03cc, maxDose, maxDoseLimit); }
      else { return string.Format("\tD(0.03cc):\t{0:F3}Gy\t\t|\tMaxDose:\t{1:F3}Gy\t\t|\tLimit: {2}Gy\t|\tNOT MET\t", d03cc, maxDose, maxDoseLimit); }
    }
    public static string volume(Structure structure, double dose, double volumeAtDose, string volumeLabel, double volumeLimit)
    {
      if (volumeAtDose <= volumeLimit) { return string.Format("\tV{0}:\t\t{1:F2}{2}\t|\tLimit: {3}{4}\t|\tMET\t", dose, volumeAtDose, volumeLabel, volumeLimit, volumeLabel); }
      else { return string.Format("\tV{0}:\t\t{1:F3}{2}\t|\tLimit: {3}{4}\t|\tNOT MET\t", dose, volumeAtDose, volumeLabel, volumeLimit, volumeLabel); }
    }
    public static string volume(string structure, double dose, double volumeAtDose, string volumeLabel, double volumeLimit)
    {
      if (volumeAtDose <= volumeLimit) { return string.Format("\tV{0}:\t\t{1:F2}{2}\t\t|\tLimit: {3}{4}\t|\tMET\t", dose, volumeAtDose, volumeLabel, volumeLimit, volumeLabel); }
      else { return string.Format("\tV{0}:\t\t{1:F3}{2}\t\t|\tLimit: {3}{4}\t|\tNOT MET\t", dose, volumeAtDose, volumeLabel, volumeLimit, volumeLabel); }
    }
    public static string mean(Structure structure, double meanDose, double meanDoseLimit, string doseLabel)
    {
      if (meanDose <= meanDoseLimit) { return string.Format("\tMeanDose:\t{0:F2}{1}\t\t|\tLimit: {2}{3}\t|\tMET\t", meanDose, doseLabel, meanDoseLimit, doseLabel); }
      else { return string.Format("\tMeanDose:\t{0:F2}{1}\t\t|\tLimit: {2}{3}\t|\tNOT MET\t", meanDose, doseLabel, meanDoseLimit, doseLabel); }
    }
    public static string mean(string structure, double meanDose, double meanDoseLimit, string doseLabel)
    {
      if (meanDose <= meanDoseLimit) { return string.Format("\tMeanDose:\t{0:F2}{1}\t\t|\tLimit: {2}{3}\t|\tMET\t", meanDose, doseLabel, meanDoseLimit, doseLabel); }
      else { return string.Format("\tMeanDose:\t{0:F2}{1}\t\t|\tLimit: {2}{3}\t|\tNOT MET\t", meanDose, doseLabel, meanDoseLimit, doseLabel); }
    }
    public static string displayMaxInfo(string dynamicVolumeLabel, DVHData dvhDynamic, string dynamicDoseLabel)
    {
      return string.Format("\tDmax(0.03{0}):\t{1:F3}{2}\t|\tMaxDose:\t{3:F3}{4}\t",
                              dynamicVolumeLabel,
                              DvhExtensions.getDoseAtVolume(dvhDynamic, 0.03),
                              dynamicDoseLabel,
                              dvhDynamic.MaxDose.Dose,
                              dynamicDoseLabel);
    }
    public static string displayMinInfo(double structureVolume_dynamic, string dynamicVolumeLabel, DVHData dvhDynamic, string dynamicDoseLabel)
    {
      return string.Format("\tDmin(Vptv{0} - 0.03{1}):\t{2:F3}{3}\t|\tMinDose\t=\t{4:F3}{5}\t",
                              dynamicVolumeLabel,
                              dynamicVolumeLabel,
                              DvhExtensions.getDoseAtVolume(dvhDynamic, structureVolume_dynamic - 0.03),
                              dynamicDoseLabel,
                              dvhDynamic.MinDose.Dose,
                              dynamicDoseLabel);
    }
    public static string displayMinInfo(Structure selectedStructure, string dynamicVolumeLabel, DVHData dvhDynamic, string dynamicDoseLabel)
    {
      return string.Format("\tDmin(0.03{0}):\t{1:F3}{2}\t|\tMinDose:\t{3:F3}{4}\t",
                              dynamicVolumeLabel,
                              DvhExtensions.getDoseAtVolume(dvhDynamic, selectedStructure.Volume - 0.03),
                              dynamicDoseLabel,
                              dvhDynamic.MinDose.Dose,
                              dynamicDoseLabel);
    }
    public static string displayStructureVolume(Structure selectedStructure, string dynamicVolumeLabel)
    {
      return string.Format("{0}\n\tVolume:\t{1}{2}", selectedStructure.Id, Math.Round(selectedStructure.Volume, 3), dynamicVolumeLabel);
    }
    public static string displayMeanInfo(DVHData dvhDynamic, string dynamicLabel)
    {
      return string.Format("\tMeanDose:\t{0:F3}{1}",
                      dvhDynamic.MeanDose.Dose, dynamicLabel);
    }
    public static string displayVolumeDose_AR(DVHData dvhAR, double doseLimit)
    {
      return string.Format("\tV{0}Gy:\t{1:F2}%", doseLimit, DvhExtensions.getVolumeAtDose(dvhAR, doseLimit));
    }
    public static string displayVolumeDose_AA(DVHData dvhAA, double doseLimit)
    {
      return string.Format("\tV{0}Gy:\t{1:F2}cc", doseLimit, DvhExtensions.getVolumeAtDose(dvhAA, doseLimit));
    }
    public static string displayVolumeDose_RA(DVHData dvhRA, double doseLimit)
    {
      return string.Format("\tV{0}%:\t{1:F2}cc", doseLimit, DvhExtensions.getVolumeAtDose(dvhRA, doseLimit));
    }
    public static string displayVolumeDose_RR(DVHData dvhRR, double doseLimit)
    {
      return string.Format("\tV{0}%:\t{1:F2}%", doseLimit, DvhExtensions.getVolumeAtDose(dvhRR, doseLimit));
    }
    public static string displayVolumeOfRxDose_dynamic(double rxDose_dynamic, string dynamicDoseLabel, DVHData dvhDynamic, string dynamicVolumeLabel)
    {
      return string.Format("\tV{0:F1}{1}:\t{2:F2}{3}", rxDose_dynamic, dynamicDoseLabel, DvhExtensions.getVolumeAtDose(dvhDynamic, rxDose_dynamic), dynamicVolumeLabel);
    }
    public static string displayVolumeAtDose_dynamic(double doseLimit, string dynamicDoseLabel, DVHData dvhDynamic, string dynamicVolumeLabel)
    {
      return string.Format("\tV{0}{1}:\t{2:F2}{3}", doseLimit, dynamicDoseLabel, DvhExtensions.getVolumeAtDose(dvhDynamic, doseLimit), dynamicVolumeLabel);
    }
    public static string displayDoseAtVolume_dynamic(double volumeLimit_dynamic, string dynamicVolumeLabel, DVHData dvhDynamic, string dynamicDoseLabel)
    {
      return string.Format("\tD{0:F1}{1}\t:\t{2:F2}{3}", volumeLimit_dynamic, dynamicVolumeLabel, DvhExtensions.getDoseAtVolume(dvhDynamic, volumeLimit_dynamic), dynamicDoseLabel);
    }
    public static string displayDoseAtVolume(double volumeLimit, string volumeLabel, DVHData dvhAR, string doseLabel)
    {
      return string.Format("\tD{0:F1}{1}:\t\t{2:F2}{3}", volumeLimit, volumeLabel, DvhExtensions.getDoseAtVolume(dvhAR, volumeLimit), doseLabel);
    }
    public static string displayPtvCoverageStats_dynamic(double structureVolume_dynamic, DVHData dvhDynamic,
                                                            string dynamicDoseLabel, string dynamicVolumeLabel,
                                                            double rxDose_dynamic)
    {
      double d110 = DoseChecks.getDoseAtVolume(dvhDynamic, (1.1 * structureVolume_dynamic));
      double d100 = DoseChecks.getDoseAtVolume(dvhDynamic, (1.0 * structureVolume_dynamic));
      double d98 = DoseChecks.getDoseAtVolume(dvhDynamic, (0.98 * structureVolume_dynamic));
      double d2 = DoseChecks.getDoseAtVolume(dvhDynamic, (0.02 * structureVolume_dynamic));
      double HI = (((d2 - d98) / rxDose_dynamic) * 100.0);
      return string.Format("{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\nHI: {7:F2}",
          displayVolumeAtDose_dynamic((rxDose_dynamic * 1.1), dynamicDoseLabel, dvhDynamic, dynamicVolumeLabel),
          displayVolumeAtDose_dynamic((rxDose_dynamic * 1.0), dynamicDoseLabel, dvhDynamic, dynamicVolumeLabel),
          displayDoseAtVolume_dynamic((structureVolume_dynamic * 0.98), dynamicVolumeLabel, dvhDynamic, dynamicDoseLabel),
          displayMinInfo(structureVolume_dynamic, dynamicVolumeLabel, dvhDynamic, dynamicDoseLabel),
          displayMeanInfo(dvhDynamic, dynamicDoseLabel),
          displayMaxInfo(dynamicVolumeLabel, dvhDynamic, dynamicDoseLabel),
          displayDoseAtVolume_dynamic((structureVolume_dynamic * 0.02), dynamicVolumeLabel, dvhDynamic, dynamicDoseLabel),
          HI);
    }
    public static string displayBasicTargetCoverageStats(Structure selectedStructure, DVHData dvhDynamic, DVHData dvhAR,
                                                            double coverageLimit, string dynamicDoseLabel, string doseLabel,
                                                            string dynamicVolumeLabel, string volumeLabel)
    {
      string minInfo = displayMinInfo(selectedStructure, dynamicVolumeLabel, dvhDynamic, dynamicDoseLabel);
      string meanInfo = displayMeanInfo(dvhDynamic, dynamicDoseLabel);
      string maxInfo = displayMaxInfo(dynamicVolumeLabel, dvhDynamic, dynamicDoseLabel);
      string d95 = displayDoseAtVolume(coverageLimit, volumeLabel, dvhAR, doseLabel);

      return string.Format("{0}\n{1}\n{2}\n{3}",
          d95,
          meanInfo,
          minInfo,
          maxInfo);
    }

    public static string displayBasicStructureStats(Structure selectedStructure, DVHData dvhDynamic,
                                                            string dynamicDoseLabel, string dynamicVolumeLabel)
    {
      string volumeInfo = displayStructureVolume(selectedStructure, dynamicVolumeLabel);
      string meanInfo = displayMeanInfo(dvhDynamic, dynamicDoseLabel);
      string maxInfo = displayMaxInfo(dynamicVolumeLabel, dvhDynamic, dynamicDoseLabel);

      return string.Format("{0}\n{1}\n{2}", volumeInfo, meanInfo, maxInfo);
    }
  }
}
