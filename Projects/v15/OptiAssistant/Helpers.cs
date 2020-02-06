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
      return ss.Structures.Single(st => st.DicomType == "BODY");

      //return ss.Structures.Single(x => x.Id.ToLower() == "body" || x.Id.ToLower() == "external");
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
    public static SegmentVolume CropStructure(Structure structureToCrop, Structure StructureToCropFrom, double cropMargin)
    {
      return structureToCrop.SegmentVolume.Sub(StructureToCropFrom.SegmentVolume.Margin(cropMargin));
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
      return structure.SegmentVolume.And(body.SegmentVolume.Margin(cropMargin));
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
      foreach (var s in structuresToBoolean)
      {
        combinedStructure.SegmentVolume = combinedStructure.SegmentVolume.Or(s.SegmentVolume);
      }
      return combinedStructure;
    }

   /// <summary>
   /// Boolean an IEnumerable collection of structures
   /// </summary>
   /// <param name="structuresToBoolean"></param>
   /// <returns>Returns a booleaned structure</returns>
    public static Structure BooleanStructures(IEnumerable<Structure> structuresToBoolean)
    {
      Structure combinedStructure = structuresToBoolean.First();
      foreach (var s in structuresToBoolean)
      {
        combinedStructure.SegmentVolume = combinedStructure.SegmentVolume.Or(s.SegmentVolume);
      }
      return combinedStructure;
    }

    #endregion

  }
}
