namespace VMS.TPS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VMS.TPS.Common.Model.API;
    public class GenerateStructureList
    {
    /// <summary>
    /// Returns a cleaned, and ordered set of structure lists that can then be used later on instead in a more clean and focused way.
    /// </summary>
    /// <param name="ss"></param>
    /// <param name="sorted_gtvList"></param>
    /// <param name="sorted_ctvList"></param>
    /// <param name="sorted_itvList"></param>
    /// <param name="sorted_ptvList"></param>
    /// <param name="sorted_targetList"></param>
    /// <param name="sorted_oarList"></param>
    /// <param name="sorted_structureList"></param>
    /// <param name="sorted_emptyStructuresList"></param>
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
					  (!structure.Id.ToLower().Contains("couch")) &&
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
					  (!structure.Id.StartsWith("_", StringComparison.InvariantCultureIgnoreCase)) &&
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
    #region examples

	  #region structure lists test

	    //IEnumerable<Structure> sorted_gtvList;
	    //IEnumerable<Structure> sorted_ctvList;
	    //IEnumerable<Structure> sorted_itvList;
	    //IEnumerable<Structure> sorted_ptvList;
	    //IEnumerable<Structure> sorted_targetList;
	    //IEnumerable<Structure> sorted_oarList;
	    //IEnumerable<Structure> sorted_structureList;

	    //GenerateStructureLists.cleanAndOrderStructures(context.PlanSetup.StructureSet, out sorted_gtvList, out sorted_ctvList, out sorted_itvList,
	    //																				out sorted_ptvList, out sorted_targetList, out sorted_oarList, out sorted_structureList);

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

	    //MessageBox.Show(result);


	    #endregion

	  #endregion
}
