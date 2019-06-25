namespace VMS.TPS
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using VMS.TPS.Common.Model.API;

  /// <summary>
  /// An old set of methods for cleaning structure names for data collection - likely antiquated.
  /// </summary>
  public class ProcessIdName
  {
    public static string processPtName(string PTName)
    {
      string OrigName = PTName;
      string newName = "";
      for (int i = 0; i < OrigName.Length; i++)
      {
        if (OrigName[i] == '(')
        {
          newName = OrigName.Substring(0, i);
          return newName;
        }

      }
      return newName;
    }
    public static string processPtNameForInfo(string PTName)
    {
      string OrigName = PTName;
      string newName = "";
      bool passedOne = false;
      bool passedTwo = false;
      for (int i = 0; i < OrigName.Length; i++)
      {
        if (OrigName[i] == ',' && passedOne)
        {
          newName = OrigName.Substring(0, i);
          passedTwo = true;
          return newName;
        }
        if (OrigName[i] == ',')
        {
          passedOne = true;
        }
      }
      if (!passedTwo)
      {
        return OrigName;
      }
      return newName;
    }

    // ideal for loops since it can be handled in one line within the loop
    public static string processStructureName(Structure structure)
    {
      #region alternative structure id lists
      var body = new List<string>();
      var bodyNames = new string[] { "body", "external" };
      foreach (var n in bodyNames) { body.Add(n); }

      var bladder = new List<string>();
      var bladderNames = new string[] { "bladder" };
      foreach (var n in bladderNames) { bladder.Add(n); }

      var bladderNonCtv = new List<string>();
      var bladderNonCtvNames = new string[] { "bladder-ctv", "bladdernonctv", "bladderminusctv" };
      foreach (var n in bladderNonCtvNames) { bladderNonCtv.Add(n); }

      var bowel = new List<string>();
      var bowelNames = new string[] { "bowel", "bowelbag", "bowel bag", "bowel space", "bowelspace" };
      foreach (var n in bowelNames) { bowel.Add(n); }

      var smallbowel = new List<string>();
      var smallBowelNames = new string[] { "smallbowel", "bowel_s", "small bowel", "bowel_small" };
      foreach (var n in smallBowelNames) { smallbowel.Add(n); }

      var femoralHeadL = new List<string>();
      var femoralHeadLNames = new string[] {  "femoral head l", "femoral head lt", "femoralhead l", "femoralhead lt", "femoralhead_l", "femoralhead_lt",
                                                "left femoral head", "l femoral head", "lt femoral head", "left femoralhead", "l femoralhead", "lt femoralhead" };
      foreach (var n in femoralHeadLNames) { femoralHeadL.Add(n); }

      var femurL = new List<string>();
      var femurLNames = new string[] { "femur_l", "femur_lt", "femur l", "femur lt" };
      foreach (var n in femurLNames) { femurL.Add(n); }

      var femoralHeadR = new List<string>();
      var femoralHeadRNames = new string[] { "femoral head r", "femoral head rt", "femoralhead r", "femoralhead rt", "femoralhead_r", "femoralhead_rt",
                                                "right femoral head", "r femoral head", "rt femoral head", "right femoralhead", "r femoralhead", "rt femoralhead" };
      foreach (var n in femoralHeadRNames) { femoralHeadR.Add(n); }

      var femurR = new List<string>();
      var femurRNames = new string[] { "femur_r", "femur_rt", "femur r", "femur rt", "rt femur", "r femur" };
      foreach (var n in femurRNames) { femurR.Add(n); }

      var penileBulb = new List<string>();
      var pbNames = new string[] { "penilebulb", "penileBulb", "penile bulb" };
      foreach (var n in pbNames) { penileBulb.Add(n); }

      var rectum = new List<string>();
      var rectumNames = new string[] { "rectum" };
      foreach (var n in rectumNames) { rectum.Add(n); }

      #endregion alternative structure id lists

      string id = structure.ToString().ToLower();

      if (rectumNames.Contains(id)) { id = "Rectum"; return id; }
      else if (bowelNames.Contains(id)) { id = "Bowel"; return id; }
      else if (smallBowelNames.Contains(id)) { id = "Bowel"; return id; } // Change to SmallBowel for other sites where SB is actually used
      else if (bladderNames.Contains(id)) { id = "Bladder"; return id; }
      else if (bladderNonCtvNames.Contains(id)) { id = "Bladder-CTV"; return id; }
      else if (pbNames.Contains(id)) { id = "PenileBulb"; return id; }
      else if (femoralHeadLNames.Contains(id)) { id = "FemoralHead_L"; return id; }
      else if (femurLNames.Contains(id)) { id = "FemoralHead_L"; return id; } // Change to Femur_L in event it's actually a lower portion of the Femur
      else if (femoralHeadRNames.Contains(id)) { id = "FemoralHead_R"; return id; }
      else if (femurRNames.Contains(id)) { id = "FemoralHead_R"; return id; } // Change to Femur_R in event it's actually a lower portion of the Femur
      else if (bodyNames.Contains(id)) { id = "BODY"; return id; }
      else return structure.ToString();
    }

    public static string processStructureName(string structure)
    {
      #region alternative structure id lists
      var body = new List<string>();
      var bodyNames = new string[] { "body", "external" };
      foreach (var n in bodyNames) { body.Add(n); }

      var bladder = new List<string>();
      var bladderNames = new string[] { "bladder" };
      foreach (var n in bladderNames) { bladder.Add(n); }

      var bladderNonCtv = new List<string>();
      var bladderNonCtvNames = new string[] { "bladder-ctv", "bladdernonctv", "bladderminusctv" };
      foreach (var n in bladderNonCtvNames) { bladderNonCtv.Add(n); }

      var bowel = new List<string>();
      var bowelNames = new string[] { "bowel", "bowelbag", "bowel bag", "bowel space", "bowelspace" };
      foreach (var n in bowelNames) { bowel.Add(n); }

      var smallbowel = new List<string>();
      var smallBowelNames = new string[] { "smallbowel", "bowel_s", "small bowel", "bowel_small" };
      foreach (var n in smallBowelNames) { smallbowel.Add(n); }

      var femoralHeadL = new List<string>();
      var femoralHeadLNames = new string[] {  "femoral head l", "femoral head lt", "femoralhead l", "femoralhead lt", "femoralhead_l", "femoralhead_lt",
                                                "left femoral head", "l femoral head", "lt femoral head", "left femoralhead", "l femoralhead", "lt femoralhead" };
      foreach (var n in femoralHeadLNames) { femoralHeadL.Add(n); }

      var femurL = new List<string>();
      var femurLNames = new string[] { "femur_l", "femur_lt", "femur l", "femur lt" };
      foreach (var n in femurLNames) { femurL.Add(n); }

      var femoralHeadR = new List<string>();
      var femoralHeadRNames = new string[] { "femoral head r", "femoral head rt", "femoralhead r", "femoralhead rt", "femoralhead_r", "femoralhead_rt",
                                                "right femoral head", "r femoral head", "rt femoral head", "right femoralhead", "r femoralhead", "rt femoralhead" };
      foreach (var n in femoralHeadRNames) { femoralHeadR.Add(n); }

      var femurR = new List<string>();
      var femurRNames = new string[] { "femur_r", "femur_rt", "femur r", "femur rt", "rt femur", "r femur" };
      foreach (var n in femurRNames) { femurR.Add(n); }

      var lensL = new List<string>();
      var lensLNames = new string[] { "lens_l", "lens_lt", "lens l", "lens lt", "left lens", "lt lens", "left_lens", "lt_lens", "l lens", "l_lens" };
      foreach (var n in lensLNames) { lensL.Add(n); }

      var lensR = new List<string>();
      var lensRNames = new string[] { "lens_r", "lens_rt", "lens r", "lens rt", "right lens", "rt lens", "right_lens", "rt_lens", "r lens", "r_lens" };
      foreach (var n in lensRNames) { lensR.Add(n); }

      var penileBulb = new List<string>();
      var pbNames = new string[] { "penilebulb", "penile_bulb", "penile bulb" };
      foreach (var n in pbNames) { penileBulb.Add(n); }

      var rectum = new List<string>();
      var rectumNames = new string[] { "rectum" };
      foreach (var n in rectumNames) { rectum.Add(n); }

      #endregion alternative structure id lists

      string id = structure.ToLower();

      if (rectumNames.Contains(id)) { id = "Rectum"; return id; }
      else if (bowelNames.Contains(id)) { id = "Bowel"; return id; }
      else if (smallBowelNames.Contains(id)) { id = "Bowel"; return id; } // Change to SmallBowel for other sites where SB is actually used
      else if (bladderNames.Contains(id)) { id = "Bladder"; return id; }
      else if (bladderNonCtvNames.Contains(id)) { id = "Bladder-CTV"; return id; }
      else if (pbNames.Contains(id)) { id = "PenileBulb"; return id; }
      else if (femoralHeadLNames.Contains(id)) { id = "FemoralHead_L"; return id; }
      else if (femurLNames.Contains(id)) { id = "FemoralHead_L"; return id; } // Change to Femur_L in event it's actually a lower portion of the Femur
      else if (femoralHeadRNames.Contains(id)) { id = "FemoralHead_R"; return id; }
      else if (femurRNames.Contains(id)) { id = "FemoralHead_R"; return id; } // Change to Femur_R in event it's actually a lower portion of the Femur
      else if (bodyNames.Contains(id)) { id = "BODY"; return id; }
      else return structure.ToString();
    }
    // not ideal for use in loops -- similar to method below but doesn't require the structure to be converted to a string prior to being used
    public static string processStructureName(Structure structureInQuestion, string subStringToLookFor, string idealStructureName)
    {
      string structureId = structureInQuestion.ToString();
      string lowerId = structureId.ToLower();
      string subString = subStringToLookFor.ToLower();

      if (lowerId.Contains(subString))
      {
        return idealStructureName;
      }
      else return structureInQuestion.ToString();
    }

    // works in the loop -- ideal for cases when structure of concern is not a standard structure name 
    // allows for customization on the fly
    public static string processStructureName(string structureInQuestion, string incorrectPortion, string idealStructureName)
    {
      if (structureInQuestion.ToLower().Contains(incorrectPortion))
      {
        return idealStructureName;
      }
      else return structureInQuestion;
    }

    public static void getRandomId(string patientId, out string randomId)
    {
      randomId = "";
      double pIdAsDouble = 0;
      double n = 0;
      var isNumeric = double.TryParse(patientId, out n);
      if (isNumeric)
      {
        pIdAsDouble = Convert.ToDouble(patientId);
        randomId = Math.Ceiling(Math.Sqrt(pIdAsDouble * 5)).ToString();
      }
      else { randomId = patientId; }
    }
  }
}
