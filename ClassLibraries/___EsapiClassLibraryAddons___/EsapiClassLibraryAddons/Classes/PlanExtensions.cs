namespace VMS.TPS
{
  using System;
  using System.Linq;
  using System.Windows;
  using VMS.TPS.Common.Model.API;
  using System.Text.RegularExpressions;

  public static class PlanExtensions
  {
    static public string RemoveWhitespace(string input)
    {
      return new string(input.ToCharArray().Where(c => !Char.IsWhiteSpace(c)).ToArray());
    }

    public static Structure GetStructure(string inPrescription, PlanSetup plan)
    {
      try
      {


        char test = inPrescription[inPrescription.Length - 2];
        int ansiChar = (int)test;


        string searchString = inPrescription;

        if (ansiChar == 13)
          searchString = inPrescription.Substring(0, inPrescription.Length - 2);


        //is it a direct match ?
        foreach (var structure in plan.StructureSet.Structures)
        {
          if (Regex.IsMatch(structure.Name, searchString, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace))
            return structure;
        }


        if (searchString.IndexOf("left kidney", StringComparison.OrdinalIgnoreCase) >= 0) searchString = "Kidney LT";
        if (searchString.IndexOf("right kidney", StringComparison.OrdinalIgnoreCase) >= 0) searchString = "Kidney RT";
        if (searchString.IndexOf("Lt kidney", StringComparison.OrdinalIgnoreCase) >= 0) searchString = "Kidney LT";
        if (searchString.IndexOf("Rt kidney", StringComparison.OrdinalIgnoreCase) >= 0) searchString = "Kidney RT";


        if (searchString.IndexOf("Brachial Plexus LT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "BrachialPlexus", "Brachial Plexus LT", "Brachial LT", "Plexus LT" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Brachial Plexus RT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "BrachialPlexus", "Brachial Plexus RT", "Brachial RT", "Plexus RT" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Brainstem", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "BrainStem(2)", "Brainstem" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Cochlea LT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Cochlea_L", "Cochlea LT", "CochleaL", "CochleaLeft", "LCochlea" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Cochlea RT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Cochlea_R", "Cochlea RT", "CochleaR", "CochleaRight", "RCochlea" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Constrictor Inferior", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Constrictor Inferior" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Constrictor Sup/Mid", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "ConstrMuscle", "Constrictor Sup/Mid" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Esophagus", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Esophagus", "Esophagus_PROX" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Glottis", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Glottis", }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Lips", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Lips", }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Mandible", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Mandible", }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }

        if (searchString.IndexOf("Larynx", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Larynx", "Lrnx", "**Lrnx" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }

        if (searchString.IndexOf("Parotid LT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Parotid_L", "Parotid LT", "ParotidLT", "ParotidL", "LeftParotid", "LParotid" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Parotid RT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Parotid_R", "Parotid RT", "ParotidRT", "ParotidR", "RightParotid", "RParotid" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Spinal Cord PRV_5mm", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "SpinalCord+5mm", "Spinal_Cord+5mm", "Cord - Grow  5.0", "SpinalCord_05" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Spinal Cord", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Cord", "Spinal", "SpinalCord", "Spinal cord", "Spinal_Cord" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Thyroid", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Thyroid" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("GreatVessels", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Great_Vessels", "Great Vessels", "GreatVessels" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }

        //BRAIN 
        if (searchString.IndexOf("Globe LT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Eye_L", "Orbit_LT", "Orbit LT", "Globe_L", "GlobeLeft", "GlobeL", "LGlobe", "Eye LT", "EyeLT" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Globe RT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Eye_R", "Orbit_RT", "Orbit RT", "Globe_R", "GlobeRight", "GlobeR", "RGlobe", "Eye RT", "EyeRT" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Lens LT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Lens_L", "LensL", "LensLeft", "LeftLens", "Lens LT", "LensLT" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Lens RT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Lens_R", "LensR", "LensRight", "RightLens", "Lens RT", "LensRT" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Optic Nerve LT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "opitc nerve_L", "Optic nerve_LT", "OpticNerve_L(2)", "Optic Nerve LT", "OpticNerve_L", "Optic Nerve-L", "Optic NerveL", "Optic Nerve_L" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Optic Nerve RT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "opitc nerve_R", "Optic nerve_RT", "OpticNerve_R(2)", "Optic Nerve RT", "OpticNerve_R", "Optic Nerve-R", "Optic NerveR", "Optic Nerve_R" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Optic Chiasm PRV_3mm", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "3mm", "Chiasm3" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Optic Chiasm", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "optic chiasm", "Chiasm", }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Submandibular LT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Submandibular_L", "Subman LT", "Submandibular LT", "SubmandibularLT", "SubmandibularL", "LeftSubmandibular", "LSubmandibular" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Submandibular RT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Submandibular_R", "Subman RT", "Submandibular RT", "SubmandibularRT", "SubmandibularR", "RightSubmandibular", "RSubmandibular" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Thyroid", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Thyroid", "Thyr" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Oral Cavity", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Oral", "oral Cavity", "OralCavity" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }

        if (searchString.IndexOf("Pharynx", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Pharynx" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }

        //ABDOMEN
        if (searchString.IndexOf("Liver", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Liver" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Duodenum", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Duodenum" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Stomach", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Stomach" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Small Bowel", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Small Bowel", "SmallBowel", "Bowel" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Large Bowel", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Large Bowel" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Lung", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Lungs", "Lung SUM", "LungSUM", "Lungs SUM", "Lung", "Lung_Total", "Lungs_Total", "Lung Total" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Lungs", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Lung SUM", "Lungs", "Lung_Total", "Lungs_Total", "Lung Total" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Left Lung", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Lung Left", "Lung_L", "Lung_Left", "LLung", "LeftLung", "Left_Lung" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Right Lung", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Lung Right", "Lung_R", "Lung_Right", "RLung", "RightLung", "Right_Lung" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Heart", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Heart" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }


        //PROSTATE
        if (searchString.IndexOf("Femoral Head LT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "FemHeadNeck_L", "FemoralH_L", "Femoral Head LT", "Femur_L", "Femur_LT", "LT Femur", "Femur LT" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Femoral Head RT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "FemHeadNeck_R", "FemoralH_R", "Femoral Head RT", "Femur_R", "Femur_RT", "RT Femur", "Femur RT" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Femoral Heads", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Heads" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Prostate", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Prostate" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Bladder", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Bladder" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Rectum", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Rectum" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Penile Bulb", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Penile" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Kidney LT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Kidney_LT", "Kidney LT", "KidneyLeft", "KidneyL", "LKidney", "KidneyLT", "Kidney_L" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Kidney RT", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Kidney_RT", "Kidney RT", "Kidney_R", "KidneyRight", "KidneyR", "RKidney", "KidneyRT" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("Penile", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Penile", "PenileBulb", "Penile Bulb" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }

        if (searchString.IndexOf("Skin", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Skin" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("kidney, right", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Kidney_RT", "Kidney RT", "Kidney_R", "KidneyRight", "KidneyR", "RKidney", "KidneyRT" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }

        if (searchString.IndexOf("ovary PRV, left", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Ovary PRV, LT" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }
        if (searchString.IndexOf("ovary PRV, right", StringComparison.OrdinalIgnoreCase) >= 0) { string[] output = { "Ovary PRV, RT" }; foreach (string id in output) { Structure structure = plan.StructureSet.Structures.Where(s => (s.Id.ToLower() == id.ToLower())).FirstOrDefault(); if (structure != null) return structure; } }


        //for each structure in plan
        foreach (var structure in plan.StructureSet.Structures.Where(s => Regex.IsMatch(s.Id, searchString, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace)))
        {
          if (Regex.IsMatch(structure.Id, "z_", RegexOptions.IgnoreCase)) continue;
          if ((Regex.IsMatch(structure.Id, "opti", RegexOptions.IgnoreCase)) &&
              (!Regex.IsMatch(structure.Id, "optic", RegexOptions.IgnoreCase))) continue;

          return structure;
        }

        return null;
      }
      catch { return null; };

    }

    public static string GetPlanTechnique(PlanSetup plan)
    {


      string returnText = "";

      if (plan is BrachyPlanSetup)
      {
        BrachyPlanSetup brachy = plan as BrachyPlanSetup;
        if (brachy.NumberOfPdrPulses != null)
        {
          returnText = "PDR";
        }
        else
        {
          Catheter c = brachy.Catheters.FirstOrDefault();
          if (c != null)
          {
            returnText = c.TreatmentUnit.DoseRateMode;
          }
        }
      }
      else
      {
        foreach (Beam beam in plan.Beams)
        {

          if (!beam.IsSetupField)
          {

            if (beam.GantryDirection != VMS.TPS.Common.Model.Types.GantryDirection.None)
            {
              if (beam.MLCPlanType == VMS.TPS.Common.Model.Types.MLCPlanType.VMAT) returnText = "VMAT";
              else returnText = "ARC";
            }
            else
            {
              if (beam.MLCPlanType == VMS.TPS.Common.Model.Types.MLCPlanType.DoseDynamic) returnText = "IMRT";
              else returnText = "STATIC";
            }
          }
        }
      }


      // NOTE: New isAPPA
      var beams = plan.Beams.ToList();
      if ((beams.Count() == 2) && (returnText == "STATIC"))
      {
        var gaBeam1 = beams[0].ControlPoints[0].GantryAngle;
        var gaBeam2 = beams[1].ControlPoints[0].GantryAngle;

        /* MessageBox.Show(string.Format("Gantry 1:\t{0}\r\n" +
												"Gantry 2:\t{1}", gaBeam1, gaBeam2)); */

        if (((((gaBeam1 >= 359) && (gaBeam1 < 360)) || ((gaBeam1 >= 0) && (gaBeam1 < 1))) && (((gaBeam2 > 179) && (gaBeam2 <= 180)) || ((gaBeam2 > 180) && (gaBeam2 < 181)))) ||
          ((((gaBeam2 > 359) && (gaBeam2 < 360)) || ((gaBeam2 >= 0) && (gaBeam2 < 1))) && (((gaBeam1 > 179) && (gaBeam1 <= 180)) || ((gaBeam1 > 180) && (gaBeam1 < 181)))))
        {
          returnText = "APPA";
        }
      }


      return returnText;
    }

    public static string GetPlanEnergy(PlanSetup plan)
    {
      string result = "";
      foreach (Beam b in plan.Beams)
      {
        if (!b.IsSetupField)
        {
          if (Regex.IsMatch(result, b.EnergyModeDisplayName.ToString()))
            if (result.Length < 2) result = b.EnergyModeDisplayName.ToString();
            else result = result + "\\" + b.EnergyModeDisplayName.ToString();

        }
      }

      return result;
    }

    public static bool GetPlanHasBolus(PlanSetup plan)
    {
      foreach (Beam b in plan.Beams)
      {
        if (!b.IsSetupField)
        {
          return (b.Boluses.Count() > 0);
        }
      }

      return false;
    }

    public static ExternalPlanSetup GetPlan(ScriptContext context)
    {
      Patient pt = context.Patient;
      if (pt == null)
      {
        string info = "No patient is currently open. Open a patient before executing this script.";
        string caption = "No patient available";
        MessageBox.Show(info, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        return null;
      }

      Course cs = context.Course;
      if (cs == null)
      {
        string info = "No course is currently open. Open a course before executing this script.";
        string caption = "No course available";
        MessageBox.Show(info, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        return null;
      }

      ExternalPlanSetup plan = context.ExternalPlanSetup;

      return plan;
    }

    public static string GetTreatmentCenter(ScriptContext context)
    {

      //What satelite is this ? 
      string treatmentCenter = "Unknown";
      string machineID = "Unknown";

      ExternalPlanSetup plan = GetPlan(context);
      if (plan == null) return treatmentCenter;

      Beam firstBeam = plan.Beams.FirstOrDefault();
      if (firstBeam != null) machineID = firstBeam.TreatmentUnit.Id;


      if ((Regex.IsMatch(machineID, "TEC1", RegexOptions.IgnoreCase)) ||
          (Regex.IsMatch(machineID, "TEC2", RegexOptions.IgnoreCase)) ||
          (Regex.IsMatch(machineID, "TEC3", RegexOptions.IgnoreCase)) ||
          (Regex.IsMatch(machineID, "TEC4", RegexOptions.IgnoreCase))) treatmentCenter = "Clifton";


      if ((Regex.IsMatch(machineID, "Grady", RegexOptions.IgnoreCase)) ||
          (Regex.IsMatch(machineID, "SL-75", RegexOptions.IgnoreCase))) treatmentCenter = "Grady";

      if ((Regex.IsMatch(machineID, "Truebeam_EUHM", RegexOptions.IgnoreCase)) ||
          (Regex.IsMatch(machineID, "CWL_2100", RegexOptions.IgnoreCase)) ||
          (Regex.IsMatch(machineID, "CWL_Trilogy", RegexOptions.IgnoreCase))) treatmentCenter = "Midtown";

      // to do :add ESJH

      return treatmentCenter;


    }

    public static bool GetTreatmentCenterUsesPrescription(ScriptContext context)
    {
      string treatmentCenter = GetTreatmentCenter(context);
      return ((Regex.IsMatch(treatmentCenter, "Clifton", RegexOptions.IgnoreCase)) ||
              (Regex.IsMatch(treatmentCenter, "Midtown", RegexOptions.IgnoreCase)));
    }
  }
}