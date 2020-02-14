using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace OptiAssistant
{
  public static class Helpers
  {

    #region helper methods

    /// <summary>
    /// Simple Wait Cursor
    /// </summary>
    public class WaitCursor : IDisposable
    {
      public Cursor _previousCursor;

      public WaitCursor()
      {
        _previousCursor = Mouse.OverrideCursor;

        Mouse.OverrideCursor = Cursors.Wait;
      }

      #region IDisposable Members

      public void Dispose()
      {
        Mouse.OverrideCursor = _previousCursor;
      }

      #endregion
    }

    public static Structure GetBody(StructureSet ss)
    {
      return ss.Structures.Single(st => st.DicomType == "EXTERNAL");

      //return ss.Structures.Single(x => x.Id.ToLower() == "body" || x.Id.ToLower() == "external");
    }

    /// <summary>
    /// Return a structure from a given StructureSet and its Id
    /// </summary>
    /// <param name="ss"></param>
    /// <param name="structureId"></param>
    /// <returns></returns>
    public static Structure GetStructure(StructureSet ss, string structureId)
    {
      return ss.Structures.Single(st => st.Id == structureId);
    }

    /// <summary>
    /// Method to remove a single Structure from a given StructureSet
    /// </summary>
    /// <param name="ss">StructureSet</param>
    /// <param name="structureIdToRemove">Structure Id</param>
    public static void RemoveStructure(StructureSet ss, string structureIdToRemove)
    {
      if (ss.Structures.Any(st => st.Id == structureIdToRemove))
      {
        var st = ss.Structures.Single(x => x.Id == structureIdToRemove);
        ss.RemoveStructure(st);
      }
    }
    /// <summary>
    /// Method to remove multiple structures within a given StructureSet
    /// </summary>
    /// <param name="ss">StructureSet</param>
    /// <param name="structureIdsToRemove">List of Structure Ids</param>
    public static void RemoveStructures(StructureSet ss, List<string> structureIdsToRemove)
    {
      foreach (var id in structureIdsToRemove)
      {
        if (ss.Structures.Any(st => st.Id == id))
        {
          var st = ss.Structures.Single(x => x.Id == id);
          ss.RemoveStructure(st);
        }
      }
    }

    /// <summary>
    /// Add margin to existing structure
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="margin"></param>
    /// <returns></returns>
    public static SegmentVolume AddMargin(Structure structure, double margin)
    {
      return structure.SegmentVolume.Margin(margin);
    }

    /// <summary>
    /// Crop structure from another structure
    /// </summary>
    /// <param name="structureToCrop"></param>
    /// <param name="StructureToCropFrom"></param>
    /// <param name="cropMargin"></param>
    /// <returns></returns>
    public static SegmentVolume CropStructure(SegmentVolume structureToCrop, SegmentVolume structureToCropFrom, double cropMargin)
    {
      return structureToCrop.Sub(structureToCropFrom.Margin(cropMargin));
    }

    /// <summary>
    /// Crop opti ptv from higher dose ptv with given margin -- default margin = 1.0 mm
    /// </summary>
    /// <param name="opti"></param>
    /// <param name="ptv"></param>
    /// <param name="cropMargin"></param>
    /// <returns></returns>
    public static SegmentVolume CropOpti(Structure opti, Structure ptv, double cropMargin = 1)
    {
      return opti.SegmentVolume.Sub(ptv.SegmentVolume.Margin(cropMargin));
    }

    /// <summary>
    /// Crop structure outside body with given margin -- default margin = -4.0 mm (e.g. for targets)
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="body"></param>
    /// <param name="cropMargin"></param>
    /// <returns></returns>
    public static SegmentVolume CropOutsideBodyWithMargin(Structure structure, Structure body, double cropMargin = -4)
    {
      if (cropMargin == 0) { return structure.SegmentVolume.And(body.SegmentVolume); }
      else { return structure.SegmentVolume.And(body.SegmentVolume.Margin(cropMargin)); }
    }

    /// <summary>
    /// Crop structure outside body structure that already has an internal margin
    /// </summary>
    /// <param name="structure"></param>
    /// <param name="bodyLessMargin"></param>
    /// <returns></returns>
    public static SegmentVolume CropOutsideBodyLessMargin(Structure structure, Structure bodyLessMargin)
    {
      return structure.SegmentVolume.And(bodyLessMargin.SegmentVolume);
    }

    /// <summary>
    /// Boolean two structures
    /// </summary>
    /// <param name="structure1"></param>
    /// <param name="structure2"></param>
    /// <returns></returns>
    public static SegmentVolume BooleanStructures(Structure structure1, Structure structure2)
    {
      return structure1.SegmentVolume.Or(structure2.SegmentVolume);
    }

    /// <summary>
    /// Boolean a list of structures
    /// </summary>
    /// <param name="structuresToBoolean"></param>
    /// <returns>Returns a booleaned structure</returns>
    public static Structure BooleanStructures(List<Structure> structuresToBoolean)
    {
      Structure combinedStructure = structuresToBoolean[0];
      combinedStructure.SegmentVolume = structuresToBoolean[0].SegmentVolume;
      if (structuresToBoolean.Count() > 1)
      {
        foreach (var s in structuresToBoolean)
        {
          if (s.IsHighResolution)
          {
            MessageBox.Show(string.Format("The {0} is a High Resolution Structure and may not be booleaned. Please make sure all Targets are either High Res or Not.", s.Id));
          }
          combinedStructure.SegmentVolume = combinedStructure.SegmentVolume.Or(s.SegmentVolume);
        }
      }
      return combinedStructure;
    }

   /// <summary>
   /// Boolean an IEnumerable collection of structures
   /// </summary>
   /// <param name="structuresToBoolean"></param>
   /// <returns>Returns a booleaned structure</returns>
    public static SegmentVolume BooleanStructures(StructureSet ss, IEnumerable<Structure> structuresToBoolean)
    {
      Structure combinedStructure = ss.AddStructure("CONTROL", "zzzTEMP");
      combinedStructure.SegmentVolume = structuresToBoolean.First().SegmentVolume;

      foreach (var s in structuresToBoolean)
      {
        combinedStructure.SegmentVolume = combinedStructure.SegmentVolume.Or(s.SegmentVolume);
      }
      
      return combinedStructure.SegmentVolume;
    }

    /// <summary>
    /// Boolean a list of Structure Ids
    /// </summary>
    /// <param name="structureIdsToBoolean"></param>
    /// <returns>Returns a booleaned structure</returns>
    public static Structure BooleanStructures(StructureSet ss, List<string> structureIdsToBoolean)
    {
      Structure combinedStructure = ss.Structures.Single(st => st.Id == structureIdsToBoolean[0].ToString());
      foreach (var s in structureIdsToBoolean)
      {
        combinedStructure.SegmentVolume = combinedStructure.SegmentVolume.Or(ss.Structures.Single(st => st.Id == s.ToString()).SegmentVolume);
      }
      return combinedStructure;
    }

    /// <summary>
    /// Boolean an IEnumerable collection of Structure Ids
    /// </summary>
    /// <param name="structureIdsToBoolean"></param>
    /// <returns>Returns a booleaned structure</returns>
    public static Structure BooleanStructures(StructureSet ss, IEnumerable<string> structureIdsToBoolean)
    {
      Structure combinedStructure = ss.Structures.Single(st => st.Id == structureIdsToBoolean.First().ToString());
      foreach (var s in structureIdsToBoolean)
      {
        combinedStructure.SegmentVolume = combinedStructure.SegmentVolume.Or(ss.Structures.Single(st => st.Id == s.ToString()).SegmentVolume);
      }
      return combinedStructure;
    }

    #endregion

  }
}
