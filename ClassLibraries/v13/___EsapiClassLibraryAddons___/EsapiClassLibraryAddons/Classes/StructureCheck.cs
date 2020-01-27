namespace VMS.TPS
{
  using System.Linq;
  using System.Collections.Generic;
  using VMS.TPS.Common.Model.API;

  public class StructureCheck
	{

		public static void isEmpty(IEnumerable<Structure> emptyStructuresList, out string pass, out string result)
		{
			pass = "Pass";
			result = "All Structures Include Contours";
			if (emptyStructuresList.Count() > 0)
			{
				pass = "Warning";
				result = "Empty Contours:\r\n";
				foreach (Structure s in emptyStructuresList)
				{
					result += string.Format("\t{0}\r\n", s.Id.ToString());
				}
			}
		}
	}

	#region examples


	#endregion

}