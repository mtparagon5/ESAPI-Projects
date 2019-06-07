namespace VMS.TPS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VMS.TPS.Common.Model.API;
    class GenerateStructureList
    {
        #region public methods

        /// <summary>
        /// Method that generates organized and sorted structure lists 
        /// </summary>
        public static void generatAllStructureLists(StructureSet ss,
                                                    out List<Structure> sortedGtvList,
                                                    out List<Structure> sortedCtvList,
                                                    out List<Structure> sortedItvList,
                                                    out List<Structure> sortedPtvList,
                                                    out List<Structure> sortedTargetList,
                                                    out List<Structure> sortedOarList,
                                                    out List<Structure> sortedStructureList)
        {
            // lists for structures
            List<Structure> gtvList = new List<Structure>();
            List<Structure> ctvList = new List<Structure>();
            List<Structure> itvList = new List<Structure>();
            List<Structure> ptvList = new List<Structure>();
            List<Structure> oarList = new List<Structure>();
            List<Structure> targetList = new List<Structure>();
            List<Structure> structureList = new List<Structure>();

            foreach (var structure in ss.Structures)
            {
                // conditions for adding any structure
                if ((!structure.IsEmpty) &&
                    (structure.HasSegment) &&
                    (!structure.Id.Contains("*")) &&
                    (!structure.Id.ToLower().Contains("markers")) &&
                    (!structure.Id.ToLower().Contains("avoid")) &&
                    (!structure.Id.ToLower().Contains("dose")) &&
                    (!structure.Id.ToLower().Contains("contrast")) &&
                    (!structure.Id.ToLower().Contains("air")) &&
                    (!structure.Id.ToLower().Contains("dens")) &&
                    (!structure.Id.StartsWith("z", StringComparison.InvariantCultureIgnoreCase)) &&
                    (!structure.Id.StartsWith("av", StringComparison.InvariantCultureIgnoreCase)) &&
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
                    // conditions for adding oars
                    if ((!structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase)) &&
                        (!structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) &&
                        (!structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase)) &&
                        (!structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        oarList.Add(structure);
                        structureList.Add(structure);
                    }
                }
            }
            sortedGtvList = (List<Structure>)gtvList.OrderBy(x => x.Id);
            sortedCtvList = (List<Structure>)ctvList.OrderBy(x => x.Id);
            sortedItvList = (List<Structure>)itvList.OrderBy(x => x.Id);
            sortedPtvList = (List<Structure>)ptvList.OrderBy(x => x.Id);
            sortedTargetList = (List<Structure>)targetList.OrderBy(x => x.Id);
            sortedOarList = (List<Structure>)oarList.OrderBy(x => x.Id);
            sortedStructureList = (List<Structure>)structureList.OrderBy(x => x.Id);
        }

        /// <summary>
        /// Method that generates a sorted oar structure lists
        /// </summary>
        public static void generateListOfOars(StructureSet ss, out List<Structure> sortedOarList)
        {
            // lists for structures
            List<Structure> oarList = new List<Structure>();

            foreach (var structure in ss.Structures)
            {
                // conditions for adding any structure
                if ((!structure.IsEmpty) &&
                    (structure.HasSegment) &&
                    (!structure.Id.Contains("*")) &&
                    (!structure.Id.ToLower().Contains("markers")) &&
                    (!structure.Id.ToLower().Contains("avoid")) &&
                    (!structure.Id.ToLower().Contains("dose")) &&
                    (!structure.Id.ToLower().Contains("contrast")) &&
                    (!structure.Id.ToLower().Contains("air")) &&
                    (!structure.Id.ToLower().Contains("dens")) &&
                    (!structure.Id.StartsWith("z", StringComparison.InvariantCultureIgnoreCase)) &&
                    (!structure.Id.StartsWith("av", StringComparison.InvariantCultureIgnoreCase)) &&
                    (!structure.Id.StartsWith("opti ", StringComparison.InvariantCultureIgnoreCase)) &&
                    (!structure.Id.StartsWith("opti-", StringComparison.InvariantCultureIgnoreCase)))
                //(structure.Id.Contains("CI-", StringComparison.InvariantCultureIgnoreCase) == false) && 
                //(structure.Id.Contains("R50-", StringComparison.InvariantCultureIgnoreCase) == false) &&
                //(structure.Id.Contains("CI_", StringComparison.InvariantCultureIgnoreCase) == false) && 
                //(structure.Id.Contains("R50_", StringComparison.InvariantCultureIgnoreCase) == false))
                {
                    // conditions for adding oars
                    if ((!structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase)) &&
                        (!structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) &&
                        (!structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase)) &&
                        (!structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        oarList.Add(structure);
                    }
                }
            }
            sortedOarList = (List<Structure>)oarList.OrderBy(x => x.Id);
        }

        /// <summary>
        /// Method that generates organized and sorted target lists 
        /// </summary>
        public static void generatListsOfAllTargets(StructureSet ss,
                                                    out List<Structure> sortedGtvList,
                                                    out List<Structure> sortedCtvList,
                                                    out List<Structure> sortedItvList,
                                                    out List<Structure> sortedPtvList,
                                                    out List<Structure> sortedTargetList)
        {
            // lists for structures
            List<Structure> gtvList = new List<Structure>();
            List<Structure> ctvList = new List<Structure>();
            List<Structure> itvList = new List<Structure>();
            List<Structure> ptvList = new List<Structure>();
            List<Structure> targetList = new List<Structure>();
            
            foreach (var structure in ss.Structures)
            {
                if (structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase))
                {
                    gtvList.Add(structure);
                    targetList.Add(structure);
                }
                if ((structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) ||
                    (structure.Id.StartsWith("Prost", StringComparison.InvariantCultureIgnoreCase)))
                {
                    ctvList.Add(structure);
                    targetList.Add(structure);
                }
                if (structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase))
                {
                    itvList.Add(structure);
                    targetList.Add(structure);
                }
                if (structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase))
                {
                    ptvList.Add(structure);
                    targetList.Add(structure);
                }
            }
            sortedGtvList = (List<Structure>)gtvList.OrderBy(x => x.Id);
            sortedCtvList = (List<Structure>)ctvList.OrderBy(x => x.Id);
            sortedItvList = (List<Structure>)itvList.OrderBy(x => x.Id);
            sortedPtvList = (List<Structure>)ptvList.OrderBy(x => x.Id);
            sortedTargetList = (List<Structure>)targetList.OrderBy(x => x.Id);
        }

        /// <summary>
        /// Method that generates organized and sorted target lists 
        /// </summary>
        public static void generatListsOfPtvs(StructureSet ss, out List<Structure> sortedPtvList)
        {
            // lists for structures
            List<Structure> ptvList = new List<Structure>();

            foreach (var structure in ss.Structures)
            {
                if (structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase))
                {
                    ptvList.Add(structure);
                }   
            }
            sortedPtvList = (List<Structure>)ptvList.OrderBy(x => x.Id);
        }

        /// <summary>
        /// Method that generates organized and sorted target lists 
        /// </summary>
        public static void generatGeneralListOfTargets(StructureSet ss, out List<Structure> sortedTargetList)
        {
            // lists for structures
            List<Structure> targetList = new List<Structure>();

            foreach (var structure in ss.Structures)
            {
                // conditions for adding any structure
                if ((!structure.IsEmpty) &&
                    (structure.HasSegment) &&
                    (!structure.Id.Contains("*")) &&
                    (!structure.Id.ToLower().Contains("markers")) &&
                    (!structure.Id.ToLower().Contains("avoid")) &&
                    (!structure.Id.ToLower().Contains("dose")) &&
                    (!structure.Id.ToLower().Contains("contrast")) &&
                    (!structure.Id.ToLower().Contains("air")) &&
                    (!structure.Id.ToLower().Contains("dens")) &&
                    (!structure.Id.StartsWith("z", StringComparison.InvariantCultureIgnoreCase)) &&
                    (!structure.Id.StartsWith("av", StringComparison.InvariantCultureIgnoreCase)) &&
                    (!structure.Id.StartsWith("opti ", StringComparison.InvariantCultureIgnoreCase)) &&
                    (!structure.Id.StartsWith("opti-", StringComparison.InvariantCultureIgnoreCase)))
                //(structure.Id.Contains("CI-", StringComparison.InvariantCultureIgnoreCase) == false) && 
                //(structure.Id.Contains("R50-", StringComparison.InvariantCultureIgnoreCase) == false) &&
                //(structure.Id.Contains("CI_", StringComparison.InvariantCultureIgnoreCase) == false) && 
                //(structure.Id.Contains("R50_", StringComparison.InvariantCultureIgnoreCase) == false))
                {
                    if (structure.Id.StartsWith("GTV", StringComparison.InvariantCultureIgnoreCase) ||
                        (structure.Id.StartsWith("CTV", StringComparison.InvariantCultureIgnoreCase)) ||
                        (structure.Id.StartsWith("Prost", StringComparison.InvariantCultureIgnoreCase)) ||
                        (structure.Id.StartsWith("ITV", StringComparison.InvariantCultureIgnoreCase)) ||
                        (structure.Id.StartsWith("PTV", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        targetList.Add(structure);
                    }
                }
            }
            sortedTargetList = (List<Structure>)targetList.OrderBy(x => x.Id);
        }

        /// <summary>
        /// Method that generates a list of sorted structures in the structure set
        /// </summary>
        public static void generateListOfStructures(StructureSet ss, out List<Structure> sortedStructureList)
        {
            // lists for structures
            List<Structure> structureList = new List<Structure>();

            foreach (var structure in ss.Structures)
            {
                // conditions for adding any structure
                if ((!structure.IsEmpty) &&
                    (structure.HasSegment) &&
                    (!structure.Id.Contains("*")) &&
                    (!structure.Id.ToLower().Contains("markers")) &&
                    (!structure.Id.ToLower().Contains("avoid")) &&
                    (!structure.Id.ToLower().Contains("dose")) &&
                    (!structure.Id.ToLower().Contains("contrast")) &&
                    (!structure.Id.ToLower().Contains("air")) &&
                    (!structure.Id.ToLower().Contains("dens")) &&
                    (!structure.Id.StartsWith("z", StringComparison.InvariantCultureIgnoreCase)) &&
                    (!structure.Id.StartsWith("av", StringComparison.InvariantCultureIgnoreCase)) &&
                    (!structure.Id.StartsWith("opti ", StringComparison.InvariantCultureIgnoreCase)) &&
                    (!structure.Id.StartsWith("opti-", StringComparison.InvariantCultureIgnoreCase)))
                //(structure.Id.Contains("CI-", StringComparison.InvariantCultureIgnoreCase) == false) && 
                //(structure.Id.Contains("R50-", StringComparison.InvariantCultureIgnoreCase) == false) &&
                //(structure.Id.Contains("CI_", StringComparison.InvariantCultureIgnoreCase) == false) && 
                //(structure.Id.Contains("R50_", StringComparison.InvariantCultureIgnoreCase) == false))
                {
                    structureList.Add(structure);
                }
            }
            sortedStructureList = (List<Structure>)structureList.OrderBy(x => x.Id);
        }

        #endregion
    }
}
