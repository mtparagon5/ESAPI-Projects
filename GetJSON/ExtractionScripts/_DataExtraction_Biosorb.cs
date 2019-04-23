
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Xml;
using System.Xml.Linq;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using System.Windows.Media.Media3D;


namespace VMS.TPS
{
    public class BiosrorbScript
    {
        public BiosrorbScript()
        {
        }
        public void Execute(ScriptContext context /*, System.Windows.Window window*/)
        {
            if (context.PlanSumsInScope != null || context.PlanSetup != null)
            {
                #region plan setup variables
                //Retrieve the count of plans displayed in Scope Window
                int scopePlanCount = context.PlansInScope.Count();
                int scopePlanSumsCount = context.PlanSumsInScope.Count();
                if ((scopePlanCount == 0) && (scopePlanSumsCount == 0))
                    throw new ApplicationException("Please open a plan first.");

                StructureSet structureSet;
                PlanningItem SelectedPlanningItem;
                PlanSetup plan;
                if (context.PlanSetup == null)
                {
                    PlanSum psum = context.PlanSumsInScope.First();
                    plan = psum.PlanSetups.First();
                    SelectedPlanningItem = (PlanningItem)psum;
                    structureSet = plan.StructureSet;
                }
                else
                {
                    plan = context.PlanSetup;
                    SelectedPlanningItem = (PlanningItem)plan;
                    structureSet = plan.StructureSet;
                }

                string curredLastName = context.Patient.LastName.Replace(" ", "_");
                string curredFirstName = context.Patient.FirstName.Replace(" ", "_");
                string firstInitial = context.Patient.FirstName[0].ToString();
                string lastInitial = context.Patient.LastName[0].ToString();
                string initials = firstInitial + lastInitial;
                string id = context.Patient.Id;
                double idAsDouble = Convert.ToDouble(id);
                string randomId = Math.Ceiling(Math.Sqrt(idAsDouble * 5)).ToString();
                string courseName = context.Course.Id;
                string planName = SelectedPlanningItem.Id;

                #endregion plan setup variables

                #region conditions
                // ask to close other plansums since can only take first/firstordefault plansum
                if (context.PlanSetup == null && context.PlanSumsInScope.Count() > 1)
                {
                    throw new ApplicationException("Please close other plan sums");
                }

                // this will cause the script to close if there is no dose calculated
                //if (SelectedPlanningItem.Dose == null)
                //{
                //    throw new ApplicationException("Dose has not been calculated.");
                //}
                #endregion conditions

                #region stringbuilder objects for creating csv content

                StringBuilder bp_csv_Content = new StringBuilder();
                StringBuilder dz_csv_Content = new StringBuilder();
                StringBuilder jj_csv_Content = new StringBuilder();
                StringBuilder jp_csv_Content = new StringBuilder();
                StringBuilder ma_csv_Content = new StringBuilder();
                StringBuilder nm_csv_Content = new StringBuilder();
                StringBuilder rc_csv_Content = new StringBuilder();
                StringBuilder st_csv_Content = new StringBuilder();
                StringBuilder tm_csv_Content = new StringBuilder();
                StringBuilder zb_csv_Content = new StringBuilder();
                StringBuilder org_csv_Content = new StringBuilder();
                StringBuilder total_csv_Content = new StringBuilder();

                #endregion stringbuilder objects

                #region paths for writing csv files

                string bp_Path = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\Biosorb\\Biosorb_BP.csv";
                string dz_Path = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\Biosorb\\Biosorb_DZ.csv";
                string jj_Path = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\Biosorb\\Biosorb_JJ.csv";
                string jp_Path = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\Biosorb\\Biosorb_JP.csv";
                string ma_Path = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\Biosorb\\Biosorb_MA.csv";
                string nm_Path = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\Biosorb\\Biosorb_NM.csv";
                string rc_Path = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\Biosorb\\Biosorb_RC.csv";
                string st_Path = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\Biosorb\\Biosorb_ST.csv";
                string tm_Path = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\Biosorb\\Biosorb_TM.csv";
                string zb_Path = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\Biosorb\\Biosorb_ZB.csv";
                string org_Path = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\Biosorb\\Biosorb_Original.csv";
                string total_Path = @"\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\DosimetricReview\\_DoseStats\\Biosorb\\Biosorb_Total.csv";

                #endregion paths

                #region script execution

                if (plan is ExternalPlanSetup)
                {
                    #region structure lists 
                    // creates the lists needed for the various documents that are created
                    // will allow us to organize the data differently all at once

                    // lists for structures
                    List<Structure> acList = new List<Structure>();
                    List<Structure> bcList = new List<Structure>();
                    List<Structure> totalList = new List<Structure>();
                    List<Structure> bp_List = new List<Structure>();
                    List<Structure> dz_List = new List<Structure>();
                    List<Structure> jj_List = new List<Structure>();
                    List<Structure> jp_List = new List<Structure>();
                    List<Structure> ma_List = new List<Structure>();
                    List<Structure> nm_List = new List<Structure>();
                    List<Structure> org_List = new List<Structure>();
                    List<Structure> rc_List = new List<Structure>();
                    List<Structure> st_List = new List<Structure>();
                    List<Structure> tm_List = new List<Structure>();
                    List<Structure> zb_List = new List<Structure>();

                    foreach (var structure in structureSet.Structures)
                    {
                        // conditions for adding any structure
                        if ((!structure.IsEmpty) && (structure.HasSegment))
                        {
                            if (structure.Id.Contains("_AC"))
                            {
                                acList.Add(structure);
                            }
                            if (structure.Id.Contains("_BC"))
                            {
                                bcList.Add(structure);
                            }
                            if (structure.Id.Contains("_AC") || structure.Id.Contains("_BC"))
                            {
                                totalList.Add(structure);
                            }
                            if ((structure.Id.StartsWith("bp_", StringComparison.InvariantCultureIgnoreCase)))
                            {
                                bp_List.Add(structure);
                            }
                            if ((structure.Id.StartsWith("dz_", StringComparison.InvariantCultureIgnoreCase)))
                            {
                                dz_List.Add(structure);
                            }
                            if ((structure.Id.StartsWith("jj_", StringComparison.InvariantCultureIgnoreCase)))
                            {
                                jj_List.Add(structure);
                            }
                            if ((structure.Id.StartsWith("jp_", StringComparison.InvariantCultureIgnoreCase)))
                            {
                                jp_List.Add(structure);
                            }
                            if ((structure.Id.StartsWith("ma_", StringComparison.InvariantCultureIgnoreCase)))
                            {
                                ma_List.Add(structure);
                            }
                            if ((structure.Id.StartsWith("nm_", StringComparison.InvariantCultureIgnoreCase)))
                            {
                                nm_List.Add(structure);
                            }
                            if ((structure.Id.StartsWith("org_", StringComparison.InvariantCultureIgnoreCase)))
                            {
                                org_List.Add(structure);
                            }
                            if ((structure.Id.StartsWith("rc_", StringComparison.InvariantCultureIgnoreCase)))
                            {
                                rc_List.Add(structure);
                            }
                            if ((structure.Id.StartsWith("st_", StringComparison.InvariantCultureIgnoreCase)))
                            {
                                st_List.Add(structure);
                            }
                            if ((structure.Id.StartsWith("tm_", StringComparison.InvariantCultureIgnoreCase)))
                            {
                                tm_List.Add(structure);
                            }
                            if ((structure.Id.StartsWith("zb_", StringComparison.InvariantCultureIgnoreCase)))
                            {
                                zb_List.Add(structure);
                            }
                        }
                    }

                    #endregion structure lists

                    #region column headers for each csv file
                    // headers for each of the columns
                    // we will add headers each time to ensure the structures and their data are dumped in the same column each time
                    // will allow us to reorganize the data correctly in the event a loop iterates through a list in a different order

                    #region bp_csv headers
                    // change headerlist name
                    List<string> bp_headerList = new List<string>();
                    bp_headerList.Add("FirstName");
                    bp_headerList.Add("LastName");
                    bp_headerList.Add("ID");
                    bp_headerList.Add("Randomized ID");
                    bp_headerList.Add("CourseName");
                    bp_headerList.Add("PlanName");
                    bp_headerList.Add("StructureSet");
                    bp_headerList.Add("Structure");
                    bp_headerList.Add("Structure Volume");

                    // change list
                    foreach (var ac in bp_List)
                    {
                        if (ac.Id.Contains("_AC"))
                        {
                            Structure ac_struct = null;
                            ac_struct = ac;
                            
                            var tempStruct = ac.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            bp_headerList.Add(tempStruct + " Volume (cc)");
                            bp_headerList.Add("Volume Difference (cc)");
                            bp_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            bp_headerList.Add("% Overlap with " + tempStruct);
                            bp_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change list
                    foreach (var bc in bp_List)
                    {
                        if (bc.Id.Contains("_BC"))
                        {
                            Structure bc_struct = null;
                            bc_struct = bc;

                            var tempStruct = bc.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            bp_headerList.Add(tempStruct + " Volume (cc)");
                            bp_headerList.Add("Volume Difference (cc)");
                            bp_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            bp_headerList.Add("% Overlap with " + tempStruct);
                            bp_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change concat name
                    string bp_concatHeaders = string.Join(",", bp_headerList.ToArray());

                    #endregion bp_csv headers

                    #region dz_csv headers
                    // change headerlist name
                    List<string> dz_headerList = new List<string>();
                    dz_headerList.Add("FirstName");
                    dz_headerList.Add("LastName");
                    dz_headerList.Add("ID");
                    dz_headerList.Add("Randomized ID");
                    dz_headerList.Add("CourseName");
                    dz_headerList.Add("PlanName");
                    dz_headerList.Add("StructureSet");
                    dz_headerList.Add("Structure");
                    dz_headerList.Add("Structure Volume");

                    // change list
                    foreach (var ac in dz_List)
                    {
                        if (ac.Id.Contains("_AC"))
                        {
                            Structure ac_struct = null;
                            ac_struct = ac;

                            var tempStruct = ac.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            dz_headerList.Add(tempStruct + " Volume (cc)");
                            dz_headerList.Add("Volume Difference (cc)");
                            dz_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            dz_headerList.Add("% Overlap with " + tempStruct);
                            dz_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change list
                    foreach (var bc in dz_List)
                    {
                        if (bc.Id.Contains("_BC"))
                        {
                            Structure bc_struct = null;
                            bc_struct = bc;

                            var tempStruct = bc.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            dz_headerList.Add(tempStruct + " Volume (cc)");
                            dz_headerList.Add("Volume Difference (cc)");
                            dz_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            dz_headerList.Add("% Overlap with " + tempStruct);
                            dz_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change concat name
                    string dz_concatHeaders = string.Join(",", dz_headerList.ToArray());

                    #endregion dz_csv headers

                    #region jj_csv headers
                    // change headerlist name
                    List<string> jj_headerList = new List<string>();
                    jj_headerList.Add("FirstName");
                    jj_headerList.Add("LastName");
                    jj_headerList.Add("ID");
                    jj_headerList.Add("Randomized ID");
                    jj_headerList.Add("CourseName");
                    jj_headerList.Add("PlanName");
                    jj_headerList.Add("StructureSet");
                    jj_headerList.Add("Structure");
                    jj_headerList.Add("Structure Volume");

                    // change list
                    foreach (var ac in jj_List)
                    {
                        if (ac.Id.Contains("_AC"))
                        {
                            Structure ac_struct = null;
                            ac_struct = ac;

                            var tempStruct = ac.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            jj_headerList.Add(tempStruct + " Volume (cc)");
                            jj_headerList.Add("Volume Difference (cc)");
                            jj_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            jj_headerList.Add("% Overlap with " + tempStruct);
                            jj_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change list
                    foreach (var bc in jj_List)
                    {
                        if (bc.Id.Contains("_BC"))
                        {
                            Structure bc_struct = null;
                            bc_struct = bc;

                            var tempStruct = bc.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            jj_headerList.Add(tempStruct + " Volume (cc)");
                            jj_headerList.Add("Volume Difference (cc)");
                            jj_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            jj_headerList.Add("% Overlap with " + tempStruct);
                            jj_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change concat name
                    string jj_concatHeaders = string.Join(",", jj_headerList.ToArray());

                    #endregion jj_csv headers

                    #region jp_csv headers
                    // change headerlist name
                    List<string> jp_headerList = new List<string>();
                    jp_headerList.Add("FirstName");
                    jp_headerList.Add("LastName");
                    jp_headerList.Add("ID");
                    jp_headerList.Add("Randomized ID");
                    jp_headerList.Add("CourseName");
                    jp_headerList.Add("PlanName");
                    jp_headerList.Add("StructureSet");
                    jp_headerList.Add("Structure");
                    jp_headerList.Add("Structure Volume");

                    // change list
                    foreach (var ac in jp_List)
                    {
                        if (ac.Id.Contains("_AC"))
                        {
                            Structure ac_struct = null;
                            ac_struct = ac;

                            var tempStruct = ac.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            jp_headerList.Add(tempStruct + " Volume (cc)");
                            jp_headerList.Add("Volume Difference (cc)");
                            jp_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            jp_headerList.Add("% Overlap with " + tempStruct);
                            jp_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change list
                    foreach (var bc in jp_List)
                    {
                        if (bc.Id.Contains("_BC"))
                        {
                            Structure bc_struct = null;
                            bc_struct = bc;

                            var tempStruct = bc.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            jp_headerList.Add(tempStruct + " Volume (cc)");
                            jp_headerList.Add("Volume Difference (cc)");
                            jp_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            jp_headerList.Add("% Overlap with " + tempStruct);
                            jp_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change concat name
                    string jp_concatHeaders = string.Join(",", jp_headerList.ToArray());

                    #endregion jp_csv headers

                    #region ma_csv headers
                    // change headerlist name
                    List<string> ma_headerList = new List<string>();
                    ma_headerList.Add("FirstName");
                    ma_headerList.Add("LastName");
                    ma_headerList.Add("ID");
                    ma_headerList.Add("Randomized ID");
                    ma_headerList.Add("CourseName");
                    ma_headerList.Add("PlanName");
                    ma_headerList.Add("StructureSet");
                    ma_headerList.Add("Structure");
                    ma_headerList.Add("Structure Volume");

                    // change list
                    foreach (var ac in ma_List)
                    {
                        if (ac.Id.Contains("_AC"))
                        {
                            Structure ac_struct = null;
                            ac_struct = ac;

                            var tempStruct = ac.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            ma_headerList.Add(tempStruct + " Volume (cc)");
                            ma_headerList.Add("Volume Difference (cc)");
                            ma_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            ma_headerList.Add("% Overlap with " + tempStruct);
                            ma_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change list
                    foreach (var bc in ma_List)
                    {
                        if (bc.Id.Contains("_BC"))
                        {
                            Structure bc_struct = null;
                            bc_struct = bc;

                            var tempStruct = bc.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            ma_headerList.Add(tempStruct + " Volume (cc)");
                            ma_headerList.Add("Volume Difference (cc)");
                            ma_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            ma_headerList.Add("% Overlap with " + tempStruct);
                            ma_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change concat name
                    string ma_concatHeaders = string.Join(",", ma_headerList.ToArray());

                    #endregion ma_csv headers

                    #region nm_csv headers
                    // change headerlist name
                    List<string> nm_headerList = new List<string>();
                    nm_headerList.Add("FirstName");
                    nm_headerList.Add("LastName");
                    nm_headerList.Add("ID");
                    nm_headerList.Add("Randomized ID");
                    nm_headerList.Add("CourseName");
                    nm_headerList.Add("PlanName");
                    nm_headerList.Add("StructureSet");
                    nm_headerList.Add("Structure");
                    nm_headerList.Add("Structure Volume");

                    // change list
                    foreach (var ac in nm_List)
                    {
                        if (ac.Id.Contains("_AC"))
                        {
                            Structure ac_struct = null;
                            ac_struct = ac;

                            var tempStruct = ac.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            nm_headerList.Add(tempStruct + " Volume (cc)");
                            nm_headerList.Add("Volume Difference (cc)");
                            nm_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            nm_headerList.Add("% Overlap with " + tempStruct);
                            nm_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change list
                    foreach (var bc in nm_List)
                    {
                        if (bc.Id.Contains("_BC"))
                        {
                            Structure bc_struct = null;
                            bc_struct = bc;

                            var tempStruct = bc.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            nm_headerList.Add(tempStruct + " Volume (cc)");
                            nm_headerList.Add("Volume Difference (cc)");
                            nm_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            nm_headerList.Add("% Overlap with " + tempStruct);
                            nm_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change concat name
                    string nm_concatHeaders = string.Join(",", nm_headerList.ToArray());

                    #endregion nm_csv headers

                    #region rc_csv headers
                    // change headerlist name
                    List<string> rc_headerList = new List<string>();
                    rc_headerList.Add("FirstName");
                    rc_headerList.Add("LastName");
                    rc_headerList.Add("ID");
                    rc_headerList.Add("Randomized ID");
                    rc_headerList.Add("CourseName");
                    rc_headerList.Add("PlanName");
                    rc_headerList.Add("StructureSet");
                    rc_headerList.Add("Structure");
                    rc_headerList.Add("Structure Volume");

                    // change list
                    foreach (var ac in rc_List)
                    {
                        if (ac.Id.Contains("_AC"))
                        {
                            Structure ac_struct = null;
                            ac_struct = ac;

                            var tempStruct = ac.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            rc_headerList.Add(tempStruct + " Volume (cc)");
                            rc_headerList.Add("Volume Difference (cc)");
                            rc_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            rc_headerList.Add("% Overlap with " + tempStruct);
                            rc_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change list
                    foreach (var bc in rc_List)
                    {
                        if (bc.Id.Contains("_BC"))
                        {
                            Structure bc_struct = null;
                            bc_struct = bc;

                            var tempStruct = bc.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            rc_headerList.Add(tempStruct + " Volume (cc)");
                            rc_headerList.Add("Volume Difference (cc)");
                            rc_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            rc_headerList.Add("% Overlap with " + tempStruct);
                            rc_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change concat name
                    string rc_concatHeaders = string.Join(",", rc_headerList.ToArray());

                    #endregion rc_csv headers

                    #region st_csv headers
                    // change headerlist name
                    List<string> st_headerList = new List<string>();
                    st_headerList.Add("FirstName");
                    st_headerList.Add("LastName");
                    st_headerList.Add("ID");
                    st_headerList.Add("Randomized ID");
                    st_headerList.Add("CourseName");
                    st_headerList.Add("PlanName");
                    st_headerList.Add("StructureSet");
                    st_headerList.Add("Structure");
                    st_headerList.Add("Structure Volume");

                    // change list
                    foreach (var ac in st_List)
                    {
                        if (ac.Id.Contains("_AC"))
                        {
                            Structure ac_struct = null;
                            ac_struct = ac;

                            var tempStruct = ac.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            st_headerList.Add(tempStruct + " Volume (cc)");
                            st_headerList.Add("Volume Difference (cc)");
                            st_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            st_headerList.Add("% Overlap with " + tempStruct);
                            st_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change list
                    foreach (var bc in st_List)
                    {
                        if (bc.Id.Contains("_BC"))
                        {
                            Structure bc_struct = null;
                            bc_struct = bc;

                            var tempStruct = bc.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            st_headerList.Add(tempStruct + " Volume (cc)");
                            st_headerList.Add("Volume Difference (cc)");
                            st_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            st_headerList.Add("% Overlap with " + tempStruct);
                            st_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change concat name
                    string st_concatHeaders = string.Join(",", st_headerList.ToArray());

                    #endregion st_csv headers

                    #region tm_csv headers
                    // change headerlist name
                    List<string> tm_headerList = new List<string>();
                    tm_headerList.Add("FirstName");
                    tm_headerList.Add("LastName");
                    tm_headerList.Add("ID");
                    tm_headerList.Add("Randomized ID");
                    tm_headerList.Add("CourseName");
                    tm_headerList.Add("PlanName");
                    tm_headerList.Add("StructureSet");
                    tm_headerList.Add("Structure");
                    tm_headerList.Add("Structure Volume");

                    // change list
                    foreach (var ac in tm_List)
                    {
                        if (ac.Id.Contains("_AC"))
                        {
                            Structure ac_struct = null;
                            ac_struct = ac;

                            var tempStruct = ac.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            tm_headerList.Add(tempStruct + " Volume (cc)");
                            tm_headerList.Add("Volume Difference (cc)");
                            tm_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            tm_headerList.Add("% Overlap with " + tempStruct);
                            tm_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change list
                    foreach (var bc in tm_List)
                    {
                        if (bc.Id.Contains("_BC"))
                        {
                            Structure bc_struct = null;
                            bc_struct = bc;

                            var tempStruct = bc.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            tm_headerList.Add(tempStruct + " Volume (cc)");
                            tm_headerList.Add("Volume Difference (cc)");
                            tm_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            tm_headerList.Add("% Overlap with " + tempStruct);
                            tm_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change concat name
                    string tm_concatHeaders = string.Join(",", tm_headerList.ToArray());

                    #endregion tm_csv headers

                    #region zb_csv headers
                    // change headerlist name
                    List<string> zb_headerList = new List<string>();
                    zb_headerList.Add("FirstName");
                    zb_headerList.Add("LastName");
                    zb_headerList.Add("ID");
                    zb_headerList.Add("Randomized ID");
                    zb_headerList.Add("CourseName");
                    zb_headerList.Add("PlanName");
                    zb_headerList.Add("StructureSet");
                    zb_headerList.Add("Structure");
                    zb_headerList.Add("Structure Volume");

                    // change list
                    foreach (var ac in zb_List)
                    {
                        if (ac.Id.Contains("_AC"))
                        {
                            Structure ac_struct = null;
                            ac_struct = ac;

                            var tempStruct = ac.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            zb_headerList.Add(tempStruct + " Volume (cc)");
                            zb_headerList.Add("Volume Difference (cc)");
                            zb_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            zb_headerList.Add("% Overlap with " + tempStruct);
                            zb_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change list
                    foreach (var bc in zb_List)
                    {
                        if (bc.Id.Contains("_BC"))
                        {
                            Structure bc_struct = null;
                            bc_struct = bc;

                            var tempStruct = bc.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            zb_headerList.Add(tempStruct + " Volume (cc)");
                            zb_headerList.Add("Volume Difference (cc)");
                            zb_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            zb_headerList.Add("% Overlap with " + tempStruct);
                            zb_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change concat name
                    string zb_concatHeaders = string.Join(",", zb_headerList.ToArray());

                    #endregion zb_csv headers

                    #region org_csv headers
                    // change headerlist name
                    List<string> org_headerList = new List<string>();
                    org_headerList.Add("FirstName");
                    org_headerList.Add("LastName");
                    org_headerList.Add("ID");
                    org_headerList.Add("Randomized ID");
                    org_headerList.Add("CourseName");
                    org_headerList.Add("PlanName");
                    org_headerList.Add("StructureSet");
                    org_headerList.Add("Structure");
                    org_headerList.Add("Structure Volume");

                    // change list
                    foreach (var ac in org_List)
                    {
                        if (ac.Id.Contains("_AC"))
                        {
                            Structure ac_struct = null;
                            ac_struct = ac;

                            var tempStruct = ac.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            org_headerList.Add(tempStruct + " Volume (cc)");
                            org_headerList.Add("Volume Difference (cc)");
                            org_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            org_headerList.Add("% Overlap with " + tempStruct);
                            org_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change list
                    foreach (var bc in org_List)
                    {
                        if (bc.Id.Contains("_BC"))
                        {
                            Structure bc_struct = null;
                            bc_struct = bc;

                            var tempStruct = bc.ToString();
                            if (tempStruct.Contains(":"))
                            {
                                int index = tempStruct.IndexOf(":");
                                tempStruct = tempStruct.Substring(0, index);
                            }

                            org_headerList.Add(tempStruct + " Volume (cc)");
                            org_headerList.Add("Volume Difference (cc)");
                            org_headerList.Add("Volume Overlap (cc) with " + tempStruct);
                            org_headerList.Add("% Overlap with " + tempStruct);
                            org_headerList.Add("DiceCoefficient Relative To " + tempStruct);
                        }
                    }
                    // change concat name
                    string org_concatHeaders = string.Join(",", org_headerList.ToArray());

                    #endregion zb_csv headers

                    #region total_csv headers

                    List<string> total_csv_headerList = new List<string>();
                    total_csv_headerList.Add("FirstName");
                    total_csv_headerList.Add("LastName");
                    total_csv_headerList.Add("ID");
                    total_csv_headerList.Add("Randomized ID");
                    total_csv_headerList.Add("CourseName");
                    total_csv_headerList.Add("PlanName");
                    total_csv_headerList.Add("StructureSet");
                    total_csv_headerList.Add("Physician");
                    total_csv_headerList.Add("AC_Volume (cc)");
                    total_csv_headerList.Add("BC_Volume (cc)");
                    total_csv_headerList.Add("Volume Difference Relative to AC Structure (cc)");
                    total_csv_headerList.Add("Volume Overlap (cc)");
                    total_csv_headerList.Add("% Overlap with AC Structure");
                    total_csv_headerList.Add("DiceCoefficient");

                    string total_csv_concatHeaders = string.Join(",", total_csv_headerList.ToArray());

                    #endregion total_csv headers

                    #endregion headers

                    #region csv files to be created

                    #region BP csv

                    bp_csv_Content.AppendLine(bp_concatHeaders);        // change headers content

                    foreach (var ac_bp in acList)
                    {
                        List<object> cp_AcStatsList = new List<object>();

                        var tempStruct = ac_bp.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double ac_volume = Math.Round(ac_bp.Volume, 3);

                        cp_AcStatsList.Add(curredFirstName);
                        cp_AcStatsList.Add(curredLastName);
                        cp_AcStatsList.Add(id);
                        cp_AcStatsList.Add(randomId);
                        cp_AcStatsList.Add(courseName);
                        cp_AcStatsList.Add(planName);
                        cp_AcStatsList.Add(structureSet.Id);
                        cp_AcStatsList.Add(tempStruct);
                        cp_AcStatsList.Add(ac_volume);

                        foreach (var ac_struct in bp_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;
                                
                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - ac_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, ac_bp), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_bp, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, ac_bp, ac_volumeOverlap);

                                cp_AcStatsList.Add(cpAc_volume);
                                cp_AcStatsList.Add(ac_volumeDifference);
                                cp_AcStatsList.Add(ac_volumeOverlap);
                                cp_AcStatsList.Add(ac_percentOverlap);
                                cp_AcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in bp_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;
                                
                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - ac_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, ac_bp), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_bp, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, ac_bp, bc_volumeOverlap);

                                cp_AcStatsList.Add(cpBc_volume);
                                cp_AcStatsList.Add(bc_volumeDifference);
                                cp_AcStatsList.Add(bc_volumeOverlap);
                                cp_AcStatsList.Add(bc_percentOverlap);
                                cp_AcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatAcStats = string.Join(",", cp_AcStatsList.ToArray());
                        
                        bp_csv_Content.AppendLine(cp_concatAcStats);        // change csv content name
                        //throw new ApplicationException("bp ac");

                    }
                    foreach (var bc_bp in bcList)
                    {
                        List<object> cp_BcStatsList = new List<object>();

                        var tempStruct = bc_bp.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double bc_volume = Math.Round(bc_bp.Volume, 3);

                        cp_BcStatsList.Add(curredFirstName);
                        cp_BcStatsList.Add(curredLastName);
                        cp_BcStatsList.Add(id);
                        cp_BcStatsList.Add(randomId);
                        cp_BcStatsList.Add(courseName);
                        cp_BcStatsList.Add(planName);
                        cp_BcStatsList.Add(structureSet.Id);
                        cp_BcStatsList.Add(tempStruct);
                        cp_BcStatsList.Add(bc_volume);

                        foreach (var ac_struct in bp_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - bc_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, bc_bp), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_bp, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, bc_bp, ac_volumeOverlap);

                                cp_BcStatsList.Add(cpAc_volume);
                                cp_BcStatsList.Add(ac_volumeDifference);
                                cp_BcStatsList.Add(ac_volumeOverlap);
                                cp_BcStatsList.Add(ac_percentOverlap);
                                cp_BcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in bp_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - bc_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, bc_bp), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_bp, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, bc_bp, bc_volumeOverlap);

                                cp_BcStatsList.Add(cpBc_volume);
                                cp_BcStatsList.Add(bc_volumeDifference);
                                cp_BcStatsList.Add(bc_volumeOverlap);
                                cp_BcStatsList.Add(bc_percentOverlap);
                                cp_BcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatBcStats = string.Join(",", cp_BcStatsList.ToArray());

                        bp_csv_Content.AppendLine(cp_concatBcStats);        // change csv content name
                        //throw new ApplicationException("bp bc");

                    }

                    #endregion BP csv

                    #region DZ csv

                    dz_csv_Content.AppendLine(dz_concatHeaders);        // change headers content

                    foreach (var ac_dz in acList)
                    {
                        List<object> cp_AcStatsList = new List<object>();

                        var tempStruct = ac_dz.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double ac_volume = Math.Round(ac_dz.Volume, 3);

                        cp_AcStatsList.Add(curredFirstName);
                        cp_AcStatsList.Add(curredLastName);
                        cp_AcStatsList.Add(id);
                        cp_AcStatsList.Add(randomId);
                        cp_AcStatsList.Add(courseName);
                        cp_AcStatsList.Add(planName);
                        cp_AcStatsList.Add(structureSet.Id);
                        cp_AcStatsList.Add(tempStruct);
                        cp_AcStatsList.Add(ac_volume);

                        foreach (var ac_struct in dz_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - ac_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, ac_dz), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_dz, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, ac_dz, ac_volumeOverlap);

                                cp_AcStatsList.Add(cpAc_volume);
                                cp_AcStatsList.Add(ac_volumeDifference);
                                cp_AcStatsList.Add(ac_volumeOverlap);
                                cp_AcStatsList.Add(ac_percentOverlap);
                                cp_AcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in dz_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - ac_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, ac_dz), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_dz, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, ac_dz, bc_volumeOverlap);

                                cp_AcStatsList.Add(cpBc_volume);
                                cp_AcStatsList.Add(bc_volumeDifference);
                                cp_AcStatsList.Add(bc_volumeOverlap);
                                cp_AcStatsList.Add(bc_percentOverlap);
                                cp_AcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatAcStats = string.Join(",", cp_AcStatsList.ToArray());

                        dz_csv_Content.AppendLine(cp_concatAcStats);        // change csv content name
                        //throw new ApplicationException("dz ac");

                    }
                    foreach (var bc_dz in bcList)
                    {
                        List<object> cp_BcStatsList = new List<object>();

                        var tempStruct = bc_dz.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double bc_volume = Math.Round(bc_dz.Volume, 3);

                        cp_BcStatsList.Add(curredFirstName);
                        cp_BcStatsList.Add(curredLastName);
                        cp_BcStatsList.Add(id);
                        cp_BcStatsList.Add(randomId);
                        cp_BcStatsList.Add(courseName);
                        cp_BcStatsList.Add(planName);
                        cp_BcStatsList.Add(structureSet.Id);
                        cp_BcStatsList.Add(tempStruct);
                        cp_BcStatsList.Add(bc_volume);

                        foreach (var ac_struct in dz_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - bc_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, bc_dz), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_dz, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, bc_dz, ac_volumeOverlap);

                                cp_BcStatsList.Add(cpAc_volume);
                                cp_BcStatsList.Add(ac_volumeDifference);
                                cp_BcStatsList.Add(ac_volumeOverlap);
                                cp_BcStatsList.Add(ac_percentOverlap);
                                cp_BcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in dz_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - bc_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, bc_dz), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_dz, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, bc_dz, bc_volumeOverlap);

                                cp_BcStatsList.Add(cpBc_volume);
                                cp_BcStatsList.Add(bc_volumeDifference);
                                cp_BcStatsList.Add(bc_volumeOverlap);
                                cp_BcStatsList.Add(bc_percentOverlap);
                                cp_BcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatBcStats = string.Join(",", cp_BcStatsList.ToArray());

                        dz_csv_Content.AppendLine(cp_concatBcStats);        // change csv content name
                        //throw new ApplicationException("dz bc");

                    }

                    #endregion DZ csv

                    #region JJ csv

                    jj_csv_Content.AppendLine(jj_concatHeaders);        // change headers content

                    foreach (var ac_jj in acList)
                    {
                        List<object> cp_AcStatsList = new List<object>();

                        var tempStruct = ac_jj.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double ac_volume = Math.Round(ac_jj.Volume, 3);

                        cp_AcStatsList.Add(curredFirstName);
                        cp_AcStatsList.Add(curredLastName);
                        cp_AcStatsList.Add(id);
                        cp_AcStatsList.Add(randomId);
                        cp_AcStatsList.Add(courseName);
                        cp_AcStatsList.Add(planName);
                        cp_AcStatsList.Add(structureSet.Id);
                        cp_AcStatsList.Add(tempStruct);
                        cp_AcStatsList.Add(ac_volume);

                        foreach (var ac_struct in jj_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - ac_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, ac_jj), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_jj, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, ac_jj, ac_volumeOverlap);

                                cp_AcStatsList.Add(cpAc_volume);
                                cp_AcStatsList.Add(ac_volumeDifference);
                                cp_AcStatsList.Add(ac_volumeOverlap);
                                cp_AcStatsList.Add(ac_percentOverlap);
                                cp_AcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in jj_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - ac_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, ac_jj), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_jj, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, ac_jj, bc_volumeOverlap);

                                cp_AcStatsList.Add(cpBc_volume);
                                cp_AcStatsList.Add(bc_volumeDifference);
                                cp_AcStatsList.Add(bc_volumeOverlap);
                                cp_AcStatsList.Add(bc_percentOverlap);
                                cp_AcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatAcStats = string.Join(",", cp_AcStatsList.ToArray());

                        jj_csv_Content.AppendLine(cp_concatAcStats);        // change csv content name
                        //throw new ApplicationException("jj ac");

                    }
                    foreach (var bc_jj in bcList)
                    {
                        List<object> cp_BcStatsList = new List<object>();

                        var tempStruct = bc_jj.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double bc_volume = Math.Round(bc_jj.Volume, 3);

                        cp_BcStatsList.Add(curredFirstName);
                        cp_BcStatsList.Add(curredLastName);
                        cp_BcStatsList.Add(id);
                        cp_BcStatsList.Add(randomId);
                        cp_BcStatsList.Add(courseName);
                        cp_BcStatsList.Add(planName);
                        cp_BcStatsList.Add(structureSet.Id);
                        cp_BcStatsList.Add(tempStruct);
                        cp_BcStatsList.Add(bc_volume);

                        foreach (var ac_struct in jj_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - bc_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, bc_jj), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_jj, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, bc_jj, ac_volumeOverlap);

                                cp_BcStatsList.Add(cpAc_volume);
                                cp_BcStatsList.Add(ac_volumeDifference);
                                cp_BcStatsList.Add(ac_volumeOverlap);
                                cp_BcStatsList.Add(ac_percentOverlap);
                                cp_BcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in jj_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - bc_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, bc_jj), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_jj, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, bc_jj, bc_volumeOverlap);

                                cp_BcStatsList.Add(cpBc_volume);
                                cp_BcStatsList.Add(bc_volumeDifference);
                                cp_BcStatsList.Add(bc_volumeOverlap);
                                cp_BcStatsList.Add(bc_percentOverlap);
                                cp_BcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatBcStats = string.Join(",", cp_BcStatsList.ToArray());

                        jj_csv_Content.AppendLine(cp_concatBcStats);        // change csv content name
                        //throw new ApplicationException("jj bc");

                    }

                    #endregion JJ csv

                    #region JP csv

                    jp_csv_Content.AppendLine(jp_concatHeaders);        // change headers content

                    foreach (var ac_jp in acList)
                    {
                        List<object> cp_AcStatsList = new List<object>();

                        var tempStruct = ac_jp.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double ac_volume = Math.Round(ac_jp.Volume, 3);

                        cp_AcStatsList.Add(curredFirstName);
                        cp_AcStatsList.Add(curredLastName);
                        cp_AcStatsList.Add(id);
                        cp_AcStatsList.Add(randomId);
                        cp_AcStatsList.Add(courseName);
                        cp_AcStatsList.Add(planName);
                        cp_AcStatsList.Add(structureSet.Id);
                        cp_AcStatsList.Add(tempStruct);
                        cp_AcStatsList.Add(ac_volume);

                        foreach (var ac_struct in jp_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - ac_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, ac_jp), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_jp, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, ac_jp, ac_volumeOverlap);

                                cp_AcStatsList.Add(cpAc_volume);
                                cp_AcStatsList.Add(ac_volumeDifference);
                                cp_AcStatsList.Add(ac_volumeOverlap);
                                cp_AcStatsList.Add(ac_percentOverlap);
                                cp_AcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in jp_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - ac_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, ac_jp), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_jp, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, ac_jp, bc_volumeOverlap);

                                cp_AcStatsList.Add(cpBc_volume);
                                cp_AcStatsList.Add(bc_volumeDifference);
                                cp_AcStatsList.Add(bc_volumeOverlap);
                                cp_AcStatsList.Add(bc_percentOverlap);
                                cp_AcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatAcStats = string.Join(",", cp_AcStatsList.ToArray());

                        jp_csv_Content.AppendLine(cp_concatAcStats);        // change csv content name
                        //throw new ApplicationException("jp ac");

                    }
                    foreach (var bc_jp in bcList)
                    {
                        List<object> cp_BcStatsList = new List<object>();

                        var tempStruct = bc_jp.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double bc_volume = Math.Round(bc_jp.Volume, 3);

                        cp_BcStatsList.Add(curredFirstName);
                        cp_BcStatsList.Add(curredLastName);
                        cp_BcStatsList.Add(id);
                        cp_BcStatsList.Add(randomId);
                        cp_BcStatsList.Add(courseName);
                        cp_BcStatsList.Add(planName);
                        cp_BcStatsList.Add(structureSet.Id);
                        cp_BcStatsList.Add(tempStruct);
                        cp_BcStatsList.Add(bc_volume);

                        foreach (var ac_struct in jp_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - bc_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, bc_jp), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_jp, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, bc_jp, ac_volumeOverlap);

                                cp_BcStatsList.Add(cpAc_volume);
                                cp_BcStatsList.Add(ac_volumeDifference);
                                cp_BcStatsList.Add(ac_volumeOverlap);
                                cp_BcStatsList.Add(ac_percentOverlap);
                                cp_BcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in jp_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - bc_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, bc_jp), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_jp, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, bc_jp, bc_volumeOverlap);

                                cp_BcStatsList.Add(cpBc_volume);
                                cp_BcStatsList.Add(bc_volumeDifference);
                                cp_BcStatsList.Add(bc_volumeOverlap);
                                cp_BcStatsList.Add(bc_percentOverlap);
                                cp_BcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatBcStats = string.Join(",", cp_BcStatsList.ToArray());

                        jp_csv_Content.AppendLine(cp_concatBcStats);        // change csv content name
                        //throw new ApplicationException("jp bc");

                    }

                    #endregion JP csv

                    #region MA csv

                    ma_csv_Content.AppendLine(ma_concatHeaders);        // change headers content

                    foreach (var ac_ma in acList)
                    {
                        List<object> cp_AcStatsList = new List<object>();

                        var tempStruct = ac_ma.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double ac_volume = Math.Round(ac_ma.Volume, 3);

                        cp_AcStatsList.Add(curredFirstName);
                        cp_AcStatsList.Add(curredLastName);
                        cp_AcStatsList.Add(id);
                        cp_AcStatsList.Add(randomId);
                        cp_AcStatsList.Add(courseName);
                        cp_AcStatsList.Add(planName);
                        cp_AcStatsList.Add(structureSet.Id);
                        cp_AcStatsList.Add(tempStruct);
                        cp_AcStatsList.Add(ac_volume);

                        foreach (var ac_struct in ma_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - ac_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, ac_ma), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_ma, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, ac_ma, ac_volumeOverlap);

                                cp_AcStatsList.Add(cpAc_volume);
                                cp_AcStatsList.Add(ac_volumeDifference);
                                cp_AcStatsList.Add(ac_volumeOverlap);
                                cp_AcStatsList.Add(ac_percentOverlap);
                                cp_AcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in ma_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - ac_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, ac_ma), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_ma, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, ac_ma, bc_volumeOverlap);

                                cp_AcStatsList.Add(cpBc_volume);
                                cp_AcStatsList.Add(bc_volumeDifference);
                                cp_AcStatsList.Add(bc_volumeOverlap);
                                cp_AcStatsList.Add(bc_percentOverlap);
                                cp_AcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatAcStats = string.Join(",", cp_AcStatsList.ToArray());

                        ma_csv_Content.AppendLine(cp_concatAcStats);        // change csv content name
                        //throw new ApplicationException("ma ac");

                    }
                    foreach (var bc_ma in bcList)
                    {
                        List<object> cp_BcStatsList = new List<object>();

                        var tempStruct = bc_ma.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double bc_volume = Math.Round(bc_ma.Volume, 3);

                        cp_BcStatsList.Add(curredFirstName);
                        cp_BcStatsList.Add(curredLastName);
                        cp_BcStatsList.Add(id);
                        cp_BcStatsList.Add(randomId);
                        cp_BcStatsList.Add(courseName);
                        cp_BcStatsList.Add(planName);
                        cp_BcStatsList.Add(structureSet.Id);
                        cp_BcStatsList.Add(tempStruct);
                        cp_BcStatsList.Add(bc_volume);

                        foreach (var ac_struct in ma_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - bc_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, bc_ma), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_ma, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, bc_ma, ac_volumeOverlap);

                                cp_BcStatsList.Add(cpAc_volume);
                                cp_BcStatsList.Add(ac_volumeDifference);
                                cp_BcStatsList.Add(ac_volumeOverlap);
                                cp_BcStatsList.Add(ac_percentOverlap);
                                cp_BcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in ma_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - bc_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, bc_ma), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_ma, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, bc_ma, bc_volumeOverlap);

                                cp_BcStatsList.Add(cpBc_volume);
                                cp_BcStatsList.Add(bc_volumeDifference);
                                cp_BcStatsList.Add(bc_volumeOverlap);
                                cp_BcStatsList.Add(bc_percentOverlap);
                                cp_BcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatBcStats = string.Join(",", cp_BcStatsList.ToArray());

                        ma_csv_Content.AppendLine(cp_concatBcStats);        // change csv content name
                        //throw new ApplicationException("ma bc");

                    }

                    #endregion MA csv

                    #region NM csv

                    nm_csv_Content.AppendLine(nm_concatHeaders);        // change headers content

                    foreach (var ac_nm in acList)
                    {
                        List<object> cp_AcStatsList = new List<object>();

                        var tempStruct = ac_nm.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double ac_volume = Math.Round(ac_nm.Volume, 3);

                        cp_AcStatsList.Add(curredFirstName);
                        cp_AcStatsList.Add(curredLastName);
                        cp_AcStatsList.Add(id);
                        cp_AcStatsList.Add(randomId);
                        cp_AcStatsList.Add(courseName);
                        cp_AcStatsList.Add(planName);
                        cp_AcStatsList.Add(structureSet.Id);
                        cp_AcStatsList.Add(tempStruct);
                        cp_AcStatsList.Add(ac_volume);

                        foreach (var ac_struct in nm_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - ac_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, ac_nm), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_nm, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, ac_nm, ac_volumeOverlap);

                                cp_AcStatsList.Add(cpAc_volume);
                                cp_AcStatsList.Add(ac_volumeDifference);
                                cp_AcStatsList.Add(ac_volumeOverlap);
                                cp_AcStatsList.Add(ac_percentOverlap);
                                cp_AcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in nm_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - ac_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, ac_nm), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_nm, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, ac_nm, bc_volumeOverlap);

                                cp_AcStatsList.Add(cpBc_volume);
                                cp_AcStatsList.Add(bc_volumeDifference);
                                cp_AcStatsList.Add(bc_volumeOverlap);
                                cp_AcStatsList.Add(bc_percentOverlap);
                                cp_AcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatAcStats = string.Join(",", cp_AcStatsList.ToArray());

                        nm_csv_Content.AppendLine(cp_concatAcStats);        // change csv content name
                        //throw new ApplicationException("nm ac");

                    }
                    foreach (var bc_nm in bcList)
                    {
                        List<object> cp_BcStatsList = new List<object>();

                        var tempStruct = bc_nm.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double bc_volume = Math.Round(bc_nm.Volume, 3);

                        cp_BcStatsList.Add(curredFirstName);
                        cp_BcStatsList.Add(curredLastName);
                        cp_BcStatsList.Add(id);
                        cp_BcStatsList.Add(randomId);
                        cp_BcStatsList.Add(courseName);
                        cp_BcStatsList.Add(planName);
                        cp_BcStatsList.Add(structureSet.Id);
                        cp_BcStatsList.Add(tempStruct);
                        cp_BcStatsList.Add(bc_volume);

                        foreach (var ac_struct in nm_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - bc_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, bc_nm), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_nm, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, bc_nm, ac_volumeOverlap);

                                cp_BcStatsList.Add(cpAc_volume);
                                cp_BcStatsList.Add(ac_volumeDifference);
                                cp_BcStatsList.Add(ac_volumeOverlap);
                                cp_BcStatsList.Add(ac_percentOverlap);
                                cp_BcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in nm_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - bc_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, bc_nm), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_nm, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, bc_nm, bc_volumeOverlap);

                                cp_BcStatsList.Add(cpBc_volume);
                                cp_BcStatsList.Add(bc_volumeDifference);
                                cp_BcStatsList.Add(bc_volumeOverlap);
                                cp_BcStatsList.Add(bc_percentOverlap);
                                cp_BcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatBcStats = string.Join(",", cp_BcStatsList.ToArray());

                        nm_csv_Content.AppendLine(cp_concatBcStats);        // change csv content name
                        //throw new ApplicationException("nm bc");

                    }

                    #endregion NM csv

                    #region RC csv

                    rc_csv_Content.AppendLine(rc_concatHeaders);        // change headers content

                    foreach (var ac_rc in acList)
                    {
                        List<object> cp_AcStatsList = new List<object>();

                        var tempStruct = ac_rc.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double ac_volume = Math.Round(ac_rc.Volume, 3);

                        cp_AcStatsList.Add(curredFirstName);
                        cp_AcStatsList.Add(curredLastName);
                        cp_AcStatsList.Add(id);
                        cp_AcStatsList.Add(randomId);
                        cp_AcStatsList.Add(courseName);
                        cp_AcStatsList.Add(planName);
                        cp_AcStatsList.Add(structureSet.Id);
                        cp_AcStatsList.Add(tempStruct);
                        cp_AcStatsList.Add(ac_volume);

                        foreach (var ac_struct in rc_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - ac_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, ac_rc), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_rc, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, ac_rc, ac_volumeOverlap);

                                cp_AcStatsList.Add(cpAc_volume);
                                cp_AcStatsList.Add(ac_volumeDifference);
                                cp_AcStatsList.Add(ac_volumeOverlap);
                                cp_AcStatsList.Add(ac_percentOverlap);
                                cp_AcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in rc_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - ac_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, ac_rc), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_rc, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, ac_rc, bc_volumeOverlap);

                                cp_AcStatsList.Add(cpBc_volume);
                                cp_AcStatsList.Add(bc_volumeDifference);
                                cp_AcStatsList.Add(bc_volumeOverlap);
                                cp_AcStatsList.Add(bc_percentOverlap);
                                cp_AcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatAcStats = string.Join(",", cp_AcStatsList.ToArray());

                        rc_csv_Content.AppendLine(cp_concatAcStats);        // change csv content name
                        //throw new ApplicationException("rc ac");

                    }
                    foreach (var bc_rc in bcList)
                    {
                        List<object> cp_BcStatsList = new List<object>();

                        var tempStruct = bc_rc.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double bc_volume = Math.Round(bc_rc.Volume, 3);

                        cp_BcStatsList.Add(curredFirstName);
                        cp_BcStatsList.Add(curredLastName);
                        cp_BcStatsList.Add(id);
                        cp_BcStatsList.Add(randomId);
                        cp_BcStatsList.Add(courseName);
                        cp_BcStatsList.Add(planName);
                        cp_BcStatsList.Add(structureSet.Id);
                        cp_BcStatsList.Add(tempStruct);
                        cp_BcStatsList.Add(bc_volume);

                        foreach (var ac_struct in rc_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - bc_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, bc_rc), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_rc, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, bc_rc, ac_volumeOverlap);

                                cp_BcStatsList.Add(cpAc_volume);
                                cp_BcStatsList.Add(ac_volumeDifference);
                                cp_BcStatsList.Add(ac_volumeOverlap);
                                cp_BcStatsList.Add(ac_percentOverlap);
                                cp_BcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in rc_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - bc_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, bc_rc), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_rc, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, bc_rc, bc_volumeOverlap);

                                cp_BcStatsList.Add(cpBc_volume);
                                cp_BcStatsList.Add(bc_volumeDifference);
                                cp_BcStatsList.Add(bc_volumeOverlap);
                                cp_BcStatsList.Add(bc_percentOverlap);
                                cp_BcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatBcStats = string.Join(",", cp_BcStatsList.ToArray());

                        rc_csv_Content.AppendLine(cp_concatBcStats);        // change csv content name
                        //throw new ApplicationException("rc bc");

                    }

                    #endregion RC csv

                    #region ST csv

                    st_csv_Content.AppendLine(st_concatHeaders);        // change headers content

                    foreach (var ac_st in acList)
                    {
                        List<object> cp_AcStatsList = new List<object>();

                        var tempStruct = ac_st.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double ac_volume = Math.Round(ac_st.Volume, 3);

                        cp_AcStatsList.Add(curredFirstName);
                        cp_AcStatsList.Add(curredLastName);
                        cp_AcStatsList.Add(id);
                        cp_AcStatsList.Add(randomId);
                        cp_AcStatsList.Add(courseName);
                        cp_AcStatsList.Add(planName);
                        cp_AcStatsList.Add(structureSet.Id);
                        cp_AcStatsList.Add(tempStruct);
                        cp_AcStatsList.Add(ac_volume);

                        foreach (var ac_struct in st_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - ac_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, ac_st), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_st, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, ac_st, ac_volumeOverlap);

                                cp_AcStatsList.Add(cpAc_volume);
                                cp_AcStatsList.Add(ac_volumeDifference);
                                cp_AcStatsList.Add(ac_volumeOverlap);
                                cp_AcStatsList.Add(ac_percentOverlap);
                                cp_AcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in st_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - ac_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, ac_st), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_st, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, ac_st, bc_volumeOverlap);

                                cp_AcStatsList.Add(cpBc_volume);
                                cp_AcStatsList.Add(bc_volumeDifference);
                                cp_AcStatsList.Add(bc_volumeOverlap);
                                cp_AcStatsList.Add(bc_percentOverlap);
                                cp_AcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatAcStats = string.Join(",", cp_AcStatsList.ToArray());

                        st_csv_Content.AppendLine(cp_concatAcStats);        // change csv content name
                        //throw new ApplicationException("st ac");

                    }
                    foreach (var bc_st in bcList)
                    {
                        List<object> cp_BcStatsList = new List<object>();

                        var tempStruct = bc_st.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double bc_volume = Math.Round(bc_st.Volume, 3);

                        cp_BcStatsList.Add(curredFirstName);
                        cp_BcStatsList.Add(curredLastName);
                        cp_BcStatsList.Add(id);
                        cp_BcStatsList.Add(randomId);
                        cp_BcStatsList.Add(courseName);
                        cp_BcStatsList.Add(planName);
                        cp_BcStatsList.Add(structureSet.Id);
                        cp_BcStatsList.Add(tempStruct);
                        cp_BcStatsList.Add(bc_volume);

                        foreach (var ac_struct in st_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - bc_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, bc_st), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_st, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, bc_st, ac_volumeOverlap);

                                cp_BcStatsList.Add(cpAc_volume);
                                cp_BcStatsList.Add(ac_volumeDifference);
                                cp_BcStatsList.Add(ac_volumeOverlap);
                                cp_BcStatsList.Add(ac_percentOverlap);
                                cp_BcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in st_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - bc_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, bc_st), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_st, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, bc_st, bc_volumeOverlap);

                                cp_BcStatsList.Add(cpBc_volume);
                                cp_BcStatsList.Add(bc_volumeDifference);
                                cp_BcStatsList.Add(bc_volumeOverlap);
                                cp_BcStatsList.Add(bc_percentOverlap);
                                cp_BcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatBcStats = string.Join(",", cp_BcStatsList.ToArray());

                        st_csv_Content.AppendLine(cp_concatBcStats);        // change csv content name
                        //throw new ApplicationException("st bc");

                    }

                    #endregion ST csv

                    #region TM csv

                    tm_csv_Content.AppendLine(tm_concatHeaders);        // change headers content

                    foreach (var ac_tm in acList)
                    {
                        List<object> cp_AcStatsList = new List<object>();

                        var tempStruct = ac_tm.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double ac_volume = Math.Round(ac_tm.Volume, 3);

                        cp_AcStatsList.Add(curredFirstName);
                        cp_AcStatsList.Add(curredLastName);
                        cp_AcStatsList.Add(id);
                        cp_AcStatsList.Add(randomId);
                        cp_AcStatsList.Add(courseName);
                        cp_AcStatsList.Add(planName);
                        cp_AcStatsList.Add(structureSet.Id);
                        cp_AcStatsList.Add(tempStruct);
                        cp_AcStatsList.Add(ac_volume);

                        foreach (var ac_struct in tm_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - ac_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, ac_tm), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_tm, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, ac_tm, ac_volumeOverlap);

                                cp_AcStatsList.Add(cpAc_volume);
                                cp_AcStatsList.Add(ac_volumeDifference);
                                cp_AcStatsList.Add(ac_volumeOverlap);
                                cp_AcStatsList.Add(ac_percentOverlap);
                                cp_AcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in tm_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - ac_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, ac_tm), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_tm, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, ac_tm, bc_volumeOverlap);

                                cp_AcStatsList.Add(cpBc_volume);
                                cp_AcStatsList.Add(bc_volumeDifference);
                                cp_AcStatsList.Add(bc_volumeOverlap);
                                cp_AcStatsList.Add(bc_percentOverlap);
                                cp_AcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatAcStats = string.Join(",", cp_AcStatsList.ToArray());

                        tm_csv_Content.AppendLine(cp_concatAcStats);        // change csv content name
                        //throw new ApplicationException("tm ac");

                    }
                    foreach (var bc_tm in bcList)
                    {
                        List<object> cp_BcStatsList = new List<object>();

                        var tempStruct = bc_tm.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double bc_volume = Math.Round(bc_tm.Volume, 3);

                        cp_BcStatsList.Add(curredFirstName);
                        cp_BcStatsList.Add(curredLastName);
                        cp_BcStatsList.Add(id);
                        cp_BcStatsList.Add(randomId);
                        cp_BcStatsList.Add(courseName);
                        cp_BcStatsList.Add(planName);
                        cp_BcStatsList.Add(structureSet.Id);
                        cp_BcStatsList.Add(tempStruct);
                        cp_BcStatsList.Add(bc_volume);

                        foreach (var ac_struct in tm_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - bc_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, bc_tm), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_tm, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, bc_tm, ac_volumeOverlap);

                                cp_BcStatsList.Add(cpAc_volume);
                                cp_BcStatsList.Add(ac_volumeDifference);
                                cp_BcStatsList.Add(ac_volumeOverlap);
                                cp_BcStatsList.Add(ac_percentOverlap);
                                cp_BcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in tm_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - bc_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, bc_tm), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_tm, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, bc_tm, bc_volumeOverlap);

                                cp_BcStatsList.Add(cpBc_volume);
                                cp_BcStatsList.Add(bc_volumeDifference);
                                cp_BcStatsList.Add(bc_volumeOverlap);
                                cp_BcStatsList.Add(bc_percentOverlap);
                                cp_BcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatBcStats = string.Join(",", cp_BcStatsList.ToArray());

                        tm_csv_Content.AppendLine(cp_concatBcStats);        // change csv content name
                        //throw new ApplicationException("tm bc");

                    }

                    #endregion TM csv

                    #region ZB csv

                    zb_csv_Content.AppendLine(zb_concatHeaders);        // change headers content

                    foreach (var ac_zb in acList)
                    {
                        List<object> cp_AcStatsList = new List<object>();

                        var tempStruct = ac_zb.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double ac_volume = Math.Round(ac_zb.Volume, 3);

                        cp_AcStatsList.Add(curredFirstName);
                        cp_AcStatsList.Add(curredLastName);
                        cp_AcStatsList.Add(id);
                        cp_AcStatsList.Add(randomId);
                        cp_AcStatsList.Add(courseName);
                        cp_AcStatsList.Add(planName);
                        cp_AcStatsList.Add(structureSet.Id);
                        cp_AcStatsList.Add(tempStruct);
                        cp_AcStatsList.Add(ac_volume);

                        foreach (var ac_struct in zb_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - ac_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, ac_zb), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_zb, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, ac_zb, ac_volumeOverlap);

                                cp_AcStatsList.Add(cpAc_volume);
                                cp_AcStatsList.Add(ac_volumeDifference);
                                cp_AcStatsList.Add(ac_volumeOverlap);
                                cp_AcStatsList.Add(ac_percentOverlap);
                                cp_AcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in zb_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - ac_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, ac_zb), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_zb, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, ac_zb, bc_volumeOverlap);

                                cp_AcStatsList.Add(cpBc_volume);
                                cp_AcStatsList.Add(bc_volumeDifference);
                                cp_AcStatsList.Add(bc_volumeOverlap);
                                cp_AcStatsList.Add(bc_percentOverlap);
                                cp_AcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatAcStats = string.Join(",", cp_AcStatsList.ToArray());

                        zb_csv_Content.AppendLine(cp_concatAcStats);        // change csv content name
                        //throw new ApplicationException("zb ac");

                    }
                    foreach (var bc_zb in bcList)
                    {
                        List<object> cp_BcStatsList = new List<object>();

                        var tempStruct = bc_zb.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double bc_volume = Math.Round(bc_zb.Volume, 3);

                        cp_BcStatsList.Add(curredFirstName);
                        cp_BcStatsList.Add(curredLastName);
                        cp_BcStatsList.Add(id);
                        cp_BcStatsList.Add(randomId);
                        cp_BcStatsList.Add(courseName);
                        cp_BcStatsList.Add(planName);
                        cp_BcStatsList.Add(structureSet.Id);
                        cp_BcStatsList.Add(tempStruct);
                        cp_BcStatsList.Add(bc_volume);

                        foreach (var ac_struct in zb_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - bc_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, bc_zb), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_zb, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, bc_zb, ac_volumeOverlap);

                                cp_BcStatsList.Add(cpAc_volume);
                                cp_BcStatsList.Add(ac_volumeDifference);
                                cp_BcStatsList.Add(ac_volumeOverlap);
                                cp_BcStatsList.Add(ac_percentOverlap);
                                cp_BcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in zb_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - bc_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, bc_zb), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_zb, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, bc_zb, bc_volumeOverlap);

                                cp_BcStatsList.Add(cpBc_volume);
                                cp_BcStatsList.Add(bc_volumeDifference);
                                cp_BcStatsList.Add(bc_volumeOverlap);
                                cp_BcStatsList.Add(bc_percentOverlap);
                                cp_BcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatBcStats = string.Join(",", cp_BcStatsList.ToArray());

                        zb_csv_Content.AppendLine(cp_concatBcStats);        // change csv content name
                        //throw new ApplicationException("zb bc");

                    }

                    #endregion ZB csv

                    #region ORG csv

                    org_csv_Content.AppendLine(org_concatHeaders);        // change headers content

                    foreach (var ac_org in acList)
                    {
                        List<object> cp_AcStatsList = new List<object>();

                        var tempStruct = ac_org.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double ac_volume = Math.Round(ac_org.Volume, 3);

                        cp_AcStatsList.Add(curredFirstName);
                        cp_AcStatsList.Add(curredLastName);
                        cp_AcStatsList.Add(id);
                        cp_AcStatsList.Add(randomId);
                        cp_AcStatsList.Add(courseName);
                        cp_AcStatsList.Add(planName);
                        cp_AcStatsList.Add(structureSet.Id);
                        cp_AcStatsList.Add(tempStruct);
                        cp_AcStatsList.Add(ac_volume);

                        foreach (var ac_struct in org_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - ac_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, ac_org), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_org, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, ac_org, ac_volumeOverlap);

                                cp_AcStatsList.Add(cpAc_volume);
                                cp_AcStatsList.Add(ac_volumeDifference);
                                cp_AcStatsList.Add(ac_volumeOverlap);
                                cp_AcStatsList.Add(ac_percentOverlap);
                                cp_AcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in org_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - ac_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, ac_org), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(ac_org, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, ac_org, bc_volumeOverlap);

                                cp_AcStatsList.Add(cpBc_volume);
                                cp_AcStatsList.Add(bc_volumeDifference);
                                cp_AcStatsList.Add(bc_volumeOverlap);
                                cp_AcStatsList.Add(bc_percentOverlap);
                                cp_AcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatAcStats = string.Join(",", cp_AcStatsList.ToArray());

                        org_csv_Content.AppendLine(cp_concatAcStats);        // change csv content name
                        //throw new ApplicationException("org ac");
                    }
                    foreach (var bc_org in bcList)
                    {
                        List<object> cp_BcStatsList = new List<object>();

                        var tempStruct = bc_org.ToString();
                        if (tempStruct.Contains(":"))
                        {
                            int index = tempStruct.IndexOf(":");
                            tempStruct = tempStruct.Substring(0, index);
                        }
                        double bc_volume = Math.Round(bc_org.Volume, 3);

                        cp_BcStatsList.Add(curredFirstName);
                        cp_BcStatsList.Add(curredLastName);
                        cp_BcStatsList.Add(id);
                        cp_BcStatsList.Add(randomId);
                        cp_BcStatsList.Add(courseName);
                        cp_BcStatsList.Add(planName);
                        cp_BcStatsList.Add(structureSet.Id);
                        cp_BcStatsList.Add(tempStruct);
                        cp_BcStatsList.Add(bc_volume);

                        foreach (var ac_struct in org_List)                  // change list name
                        {
                            if (ac_struct.Id.Contains("_AC"))
                            {
                                // cp is contouring physician
                                Structure cp_acStruct = null;
                                cp_acStruct = ac_struct;

                                double cpAc_volume = Math.Round(cp_acStruct.Volume, 3);
                                double ac_volumeDifference = Math.Round(cpAc_volume - bc_volume, 3);
                                double ac_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_acStruct, bc_org), 3);
                                double ac_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_org, ac_volumeOverlap), 1);
                                double ac_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_acStruct, bc_org, ac_volumeOverlap);

                                cp_BcStatsList.Add(cpAc_volume);
                                cp_BcStatsList.Add(ac_volumeDifference);
                                cp_BcStatsList.Add(ac_volumeOverlap);
                                cp_BcStatsList.Add(ac_percentOverlap);
                                cp_BcStatsList.Add(ac_diceCoefficient);
                            }
                        }
                        foreach (var bc_struct in org_List)                  // change list name
                        {
                            if (bc_struct.Id.Contains("_BC"))
                            {
                                // cp is contouring physician
                                Structure cp_bcStruct = null;
                                cp_bcStruct = bc_struct;

                                double cpBc_volume = Math.Round(cp_bcStruct.Volume, 3);
                                double bc_volumeDifference = Math.Round(cpBc_volume - bc_volume, 3);
                                double bc_volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(cp_bcStruct, bc_org), 3);
                                double bc_percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(bc_org, bc_volumeOverlap), 1);
                                double bc_diceCoefficient = CalculateOverlap.DiceCoefficient(cp_bcStruct, bc_org, bc_volumeOverlap);

                                cp_BcStatsList.Add(cpBc_volume);
                                cp_BcStatsList.Add(bc_volumeDifference);
                                cp_BcStatsList.Add(bc_volumeOverlap);
                                cp_BcStatsList.Add(bc_percentOverlap);
                                cp_BcStatsList.Add(bc_diceCoefficient);
                            }
                        }
                        string cp_concatBcStats = string.Join(",", cp_BcStatsList.ToArray());

                        org_csv_Content.AppendLine(cp_concatBcStats);        // change csv content name
                        //throw new ApplicationException("org bc");
                    }

                    #endregion ORG csv

                    #region TOTAL csv

                    total_csv_Content.AppendLine(total_csv_concatHeaders);
                    
                    foreach (var s_ac in acList)
                    {
                        foreach (var s_bc in bcList)
                        {
                            Structure acStruct = null;
                            Structure bcStruct = null;
                            if (s_ac.Id.Equals("BP_AC"))
                            {
                                if (s_bc.Id.Equals("BP_BC"))
                                {
                                    List<object> cp_statsList = new List<object>();

                                    acStruct = s_ac;
                                    bcStruct = s_bc;

                                    double acVolume = Math.Round(acStruct.Volume, 3);
                                    double bcVolume = Math.Round(bcStruct.Volume, 3);
                                    double volumeDifference = Math.Round(acVolume - bcVolume, 3);
                                    double volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(acStruct, bcStruct), 3);
                                    double percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(acStruct, volumeOverlap), 1);
                                    double diceCoefficient = CalculateOverlap.DiceCoefficient(acStruct, bcStruct, volumeOverlap);

                                    cp_statsList.Add(curredFirstName);
                                    cp_statsList.Add(curredLastName);
                                    cp_statsList.Add(id);
                                    cp_statsList.Add(randomId);
                                    cp_statsList.Add(courseName);
                                    cp_statsList.Add(planName);
                                    cp_statsList.Add(structureSet.Id);
                                    cp_statsList.Add("BP");
                                    cp_statsList.Add(acVolume);
                                    cp_statsList.Add(bcVolume);
                                    cp_statsList.Add(volumeDifference);
                                    cp_statsList.Add(volumeOverlap);
                                    cp_statsList.Add(percentOverlap);
                                    cp_statsList.Add(diceCoefficient);

                                    string cp_concatStats = string.Join(",", cp_statsList.ToArray());

                                    total_csv_Content.AppendLine(cp_concatStats);
                                    //throw new ApplicationException("bp");
                                }
                            }
                            if (s_ac.Id.Equals("DZ_AC"))
                            {
                                if (s_bc.Id.Equals("DZ_BC"))
                                {
                                    List<object> cp_statsList = new List<object>();

                                    acStruct = s_ac;
                                    bcStruct = s_bc;

                                    double acVolume = Math.Round(acStruct.Volume, 3);
                                    double bcVolume = Math.Round(bcStruct.Volume, 3);
                                    double volumeDifference = Math.Round(acVolume - bcVolume, 3);
                                    double volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(acStruct, bcStruct), 3);
                                    double percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(acStruct, volumeOverlap), 1);
                                    double diceCoefficient = CalculateOverlap.DiceCoefficient(acStruct, bcStruct, volumeOverlap);

                                    cp_statsList.Add(curredFirstName);
                                    cp_statsList.Add(curredLastName);
                                    cp_statsList.Add(id);
                                    cp_statsList.Add(randomId);
                                    cp_statsList.Add(courseName);
                                    cp_statsList.Add(planName);
                                    cp_statsList.Add(structureSet.Id);
                                    cp_statsList.Add("DZ");
                                    cp_statsList.Add(acVolume);
                                    cp_statsList.Add(bcVolume);
                                    cp_statsList.Add(volumeDifference);
                                    cp_statsList.Add(volumeOverlap);
                                    cp_statsList.Add(percentOverlap);
                                    cp_statsList.Add(diceCoefficient);

                                    string cp_concatStats = string.Join(",", cp_statsList.ToArray());

                                    total_csv_Content.AppendLine(cp_concatStats);
                                    //throw new ApplicationException("dz");
                                }
                            }
                            if (s_ac.Id.Equals("JJ_AC"))
                            {
                                if (s_bc.Id.Equals("JJ_BC"))
                                {
                                    List<object> cp_statsList = new List<object>();

                                    acStruct = s_ac;
                                    bcStruct = s_bc;

                                    double acVolume = Math.Round(acStruct.Volume, 3);
                                    double bcVolume = Math.Round(bcStruct.Volume, 3);
                                    double volumeDifference = Math.Round(acVolume - bcVolume, 3);
                                    double volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(acStruct, bcStruct), 3);
                                    double percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(acStruct, volumeOverlap), 1);
                                    double diceCoefficient = CalculateOverlap.DiceCoefficient(acStruct, bcStruct, volumeOverlap);

                                    cp_statsList.Add(curredFirstName);
                                    cp_statsList.Add(curredLastName);
                                    cp_statsList.Add(id);
                                    cp_statsList.Add(randomId);
                                    cp_statsList.Add(courseName);
                                    cp_statsList.Add(planName);
                                    cp_statsList.Add(structureSet.Id);
                                    cp_statsList.Add("JJ");
                                    cp_statsList.Add(acVolume);
                                    cp_statsList.Add(bcVolume);
                                    cp_statsList.Add(volumeDifference);
                                    cp_statsList.Add(volumeOverlap);
                                    cp_statsList.Add(percentOverlap);
                                    cp_statsList.Add(diceCoefficient);

                                    string cp_concatStats = string.Join(",", cp_statsList.ToArray());

                                    total_csv_Content.AppendLine(cp_concatStats);
                                    //throw new ApplicationException("jj");
                                }
                            }
                            if (s_ac.Id.Equals("JP_AC"))
                            {
                                if (s_bc.Id.Equals("JP_BC"))
                                {
                                    List<object> cp_statsList = new List<object>();

                                    acStruct = s_ac;
                                    bcStruct = s_bc;

                                    double acVolume = Math.Round(acStruct.Volume, 3);
                                    double bcVolume = Math.Round(bcStruct.Volume, 3);
                                    double volumeDifference = Math.Round(acVolume - bcVolume, 3);
                                    double volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(acStruct, bcStruct), 3);
                                    double percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(acStruct, volumeOverlap), 1);
                                    double diceCoefficient = CalculateOverlap.DiceCoefficient(acStruct, bcStruct, volumeOverlap);

                                    cp_statsList.Add(curredFirstName);
                                    cp_statsList.Add(curredLastName);
                                    cp_statsList.Add(id);
                                    cp_statsList.Add(randomId);
                                    cp_statsList.Add(courseName);
                                    cp_statsList.Add(planName);
                                    cp_statsList.Add(structureSet.Id);
                                    cp_statsList.Add("JP");
                                    cp_statsList.Add(acVolume);
                                    cp_statsList.Add(bcVolume);
                                    cp_statsList.Add(volumeDifference);
                                    cp_statsList.Add(volumeOverlap);
                                    cp_statsList.Add(percentOverlap);
                                    cp_statsList.Add(diceCoefficient);

                                    string cp_concatStats = string.Join(",", cp_statsList.ToArray());

                                    total_csv_Content.AppendLine(cp_concatStats);
                                    //throw new ApplicationException("jp");
                                }
                            }
                            if (s_ac.Id.Equals("MA_AC"))
                            {
                                if (s_bc.Id.Equals("MA_BC"))
                                {
                                    List<object> cp_statsList = new List<object>();

                                    acStruct = s_ac;
                                    bcStruct = s_bc;

                                    double acVolume = Math.Round(acStruct.Volume, 3);
                                    double bcVolume = Math.Round(bcStruct.Volume, 3);
                                    double volumeDifference = Math.Round(acVolume - bcVolume, 3);
                                    double volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(acStruct, bcStruct), 3);
                                    double percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(acStruct, volumeOverlap), 1);
                                    double diceCoefficient = CalculateOverlap.DiceCoefficient(acStruct, bcStruct, volumeOverlap);

                                    cp_statsList.Add(curredFirstName);
                                    cp_statsList.Add(curredLastName);
                                    cp_statsList.Add(id);
                                    cp_statsList.Add(randomId);
                                    cp_statsList.Add(courseName);
                                    cp_statsList.Add(planName);
                                    cp_statsList.Add(structureSet.Id);
                                    cp_statsList.Add("MA");
                                    cp_statsList.Add(acVolume);
                                    cp_statsList.Add(bcVolume);
                                    cp_statsList.Add(volumeDifference);
                                    cp_statsList.Add(volumeOverlap);
                                    cp_statsList.Add(percentOverlap);
                                    cp_statsList.Add(diceCoefficient);

                                    string cp_concatStats = string.Join(",", cp_statsList.ToArray());

                                    total_csv_Content.AppendLine(cp_concatStats);
                                    //throw new ApplicationException("ma");
                                }
                            }
                            if (s_ac.Id.Equals("NM_AC"))
                            {
                                if (s_bc.Id.Equals("NM_BC"))
                                {
                                    List<object> cp_statsList = new List<object>();

                                    acStruct = s_ac;
                                    bcStruct = s_bc;

                                    double acVolume = Math.Round(acStruct.Volume, 3);
                                    double bcVolume = Math.Round(bcStruct.Volume, 3);
                                    double volumeDifference = Math.Round(acVolume - bcVolume, 3);
                                    double volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(acStruct, bcStruct), 3);
                                    double percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(acStruct, volumeOverlap), 1);
                                    double diceCoefficient = CalculateOverlap.DiceCoefficient(acStruct, bcStruct, volumeOverlap);

                                    cp_statsList.Add(curredFirstName);
                                    cp_statsList.Add(curredLastName);
                                    cp_statsList.Add(id);
                                    cp_statsList.Add(randomId);
                                    cp_statsList.Add(courseName);
                                    cp_statsList.Add(planName);
                                    cp_statsList.Add(structureSet.Id);
                                    cp_statsList.Add("NM");
                                    cp_statsList.Add(acVolume);
                                    cp_statsList.Add(bcVolume);
                                    cp_statsList.Add(volumeDifference);
                                    cp_statsList.Add(volumeOverlap);
                                    cp_statsList.Add(percentOverlap);
                                    cp_statsList.Add(diceCoefficient);

                                    string cp_concatStats = string.Join(",", cp_statsList.ToArray());

                                    total_csv_Content.AppendLine(cp_concatStats);
                                    //throw new ApplicationException("nm");
                                }
                            }
                            if (s_ac.Id.Equals("RC_AC"))
                            {
                                if (s_bc.Id.Equals("RC_BC"))
                                {
                                    List<object> cp_statsList = new List<object>();

                                    acStruct = s_ac;
                                    bcStruct = s_bc;

                                    double acVolume = Math.Round(acStruct.Volume, 3);
                                    double bcVolume = Math.Round(bcStruct.Volume, 3);
                                    double volumeDifference = Math.Round(acVolume - bcVolume, 3);
                                    double volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(acStruct, bcStruct), 3);
                                    double percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(acStruct, volumeOverlap), 1);
                                    double diceCoefficient = CalculateOverlap.DiceCoefficient(acStruct, bcStruct, volumeOverlap);

                                    cp_statsList.Add(curredFirstName);
                                    cp_statsList.Add(curredLastName);
                                    cp_statsList.Add(id);
                                    cp_statsList.Add(randomId);
                                    cp_statsList.Add(courseName);
                                    cp_statsList.Add(planName);
                                    cp_statsList.Add(structureSet.Id);
                                    cp_statsList.Add("RC");
                                    cp_statsList.Add(acVolume);
                                    cp_statsList.Add(bcVolume);
                                    cp_statsList.Add(volumeDifference);
                                    cp_statsList.Add(volumeOverlap);
                                    cp_statsList.Add(percentOverlap);
                                    cp_statsList.Add(diceCoefficient);

                                    string cp_concatStats = string.Join(",", cp_statsList.ToArray());

                                    total_csv_Content.AppendLine(cp_concatStats);
                                    //throw new ApplicationException("rc");
                                }
                            }
                            if (s_ac.Id.Equals("ST_AC"))
                            {
                                if (s_bc.Id.Equals("ST_BC"))
                                {
                                    List<object> cp_statsList = new List<object>();

                                    acStruct = s_ac;
                                    bcStruct = s_bc;

                                    double acVolume = Math.Round(acStruct.Volume, 3);
                                    double bcVolume = Math.Round(bcStruct.Volume, 3);
                                    double volumeDifference = Math.Round(acVolume - bcVolume, 3);
                                    double volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(acStruct, bcStruct), 3);
                                    double percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(acStruct, volumeOverlap), 1);
                                    double diceCoefficient = CalculateOverlap.DiceCoefficient(acStruct, bcStruct, volumeOverlap);

                                    cp_statsList.Add(curredFirstName);
                                    cp_statsList.Add(curredLastName);
                                    cp_statsList.Add(id);
                                    cp_statsList.Add(randomId);
                                    cp_statsList.Add(courseName);
                                    cp_statsList.Add(planName);
                                    cp_statsList.Add(structureSet.Id);
                                    cp_statsList.Add("ST");
                                    cp_statsList.Add(acVolume);
                                    cp_statsList.Add(bcVolume);
                                    cp_statsList.Add(volumeDifference);
                                    cp_statsList.Add(volumeOverlap);
                                    cp_statsList.Add(percentOverlap);
                                    cp_statsList.Add(diceCoefficient);

                                    string cp_concatStats = string.Join(",", cp_statsList.ToArray());

                                    total_csv_Content.AppendLine(cp_concatStats);
                                    //throw new ApplicationException("st");
                                }
                            }
                            if (s_ac.Id.Equals("TM_AC"))
                            {
                                if (s_bc.Id.Equals("TM_BC"))
                                {
                                    List<object> cp_statsList = new List<object>();

                                    acStruct = s_ac;
                                    bcStruct = s_bc;

                                    double acVolume = Math.Round(acStruct.Volume, 3);
                                    double bcVolume = Math.Round(bcStruct.Volume, 3);
                                    double volumeDifference = Math.Round(acVolume - bcVolume, 3);
                                    double volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(acStruct, bcStruct), 3);
                                    double percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(acStruct, volumeOverlap), 1);
                                    double diceCoefficient = CalculateOverlap.DiceCoefficient(acStruct, bcStruct, volumeOverlap);

                                    cp_statsList.Add(curredFirstName);
                                    cp_statsList.Add(curredLastName);
                                    cp_statsList.Add(id);
                                    cp_statsList.Add(randomId);
                                    cp_statsList.Add(courseName);
                                    cp_statsList.Add(planName);
                                    cp_statsList.Add(structureSet.Id);
                                    cp_statsList.Add("TM");
                                    cp_statsList.Add(acVolume);
                                    cp_statsList.Add(bcVolume);
                                    cp_statsList.Add(volumeDifference);
                                    cp_statsList.Add(volumeOverlap);
                                    cp_statsList.Add(percentOverlap);
                                    cp_statsList.Add(diceCoefficient);

                                    string cp_concatStats = string.Join(",", cp_statsList.ToArray());

                                    total_csv_Content.AppendLine(cp_concatStats);
                                    //throw new ApplicationException("tm");
                                }
                            }
                            if (s_ac.Id.Equals("ZB_AC"))
                            {
                                if (s_bc.Id.Equals("ZB_BC"))
                                {
                                    List<object> cp_statsList = new List<object>();

                                    acStruct = s_ac;
                                    bcStruct = s_bc;

                                    double acVolume = Math.Round(acStruct.Volume, 3);
                                    double bcVolume = Math.Round(bcStruct.Volume, 3);
                                    double volumeDifference = Math.Round(acVolume - bcVolume, 3);
                                    double volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(acStruct, bcStruct), 3);
                                    double percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(acStruct, volumeOverlap), 1);
                                    double diceCoefficient = CalculateOverlap.DiceCoefficient(acStruct, bcStruct, volumeOverlap);

                                    cp_statsList.Add(curredFirstName);
                                    cp_statsList.Add(curredLastName);
                                    cp_statsList.Add(id);
                                    cp_statsList.Add(randomId);
                                    cp_statsList.Add(courseName);
                                    cp_statsList.Add(planName);
                                    cp_statsList.Add(structureSet.Id);
                                    cp_statsList.Add("ZB");
                                    cp_statsList.Add(acVolume);
                                    cp_statsList.Add(bcVolume);
                                    cp_statsList.Add(volumeDifference);
                                    cp_statsList.Add(volumeOverlap);
                                    cp_statsList.Add(percentOverlap);
                                    cp_statsList.Add(diceCoefficient);

                                    string cp_concatStats = string.Join(",", cp_statsList.ToArray());

                                    total_csv_Content.AppendLine(cp_concatStats);
                                    //throw new ApplicationException("zb");
                                }
                            }
                            if (s_ac.Id.Equals("ORG_AC"))
                            {
                                if (s_bc.Id.Equals("ORG_BC"))
                                {
                                    List<object> cp_statsList = new List<object>();

                                    acStruct = s_ac;
                                    bcStruct = s_bc;

                                    double acVolume = Math.Round(acStruct.Volume, 3);
                                    double bcVolume = Math.Round(bcStruct.Volume, 3);
                                    double volumeDifference = Math.Round(acVolume - bcVolume, 3);
                                    double volumeOverlap = Math.Round(CalculateOverlap.VolumeOverlap(acStruct, bcStruct), 3);
                                    double percentOverlap = Math.Round(CalculateOverlap.PercentOverlap(acStruct, volumeOverlap), 1);
                                    double diceCoefficient = CalculateOverlap.DiceCoefficient(acStruct, bcStruct, volumeOverlap);

                                    cp_statsList.Add(curredFirstName);
                                    cp_statsList.Add(curredLastName);
                                    cp_statsList.Add(id);
                                    cp_statsList.Add(randomId);
                                    cp_statsList.Add(courseName);
                                    cp_statsList.Add(planName);
                                    cp_statsList.Add(structureSet.Id);
                                    cp_statsList.Add("ORG");
                                    cp_statsList.Add(acVolume);
                                    cp_statsList.Add(bcVolume);
                                    cp_statsList.Add(volumeDifference);
                                    cp_statsList.Add(volumeOverlap);
                                    cp_statsList.Add(percentOverlap);
                                    cp_statsList.Add(diceCoefficient);

                                    string cp_concatStats = string.Join(",", cp_statsList.ToArray());

                                    total_csv_Content.AppendLine(cp_concatStats);
                                    //throw new ApplicationException("org");
                                }
                            }
                        }
                    }
                    #endregion TOTAL csv

                    #endregion csv files

                    #region create csv files

                    File.AppendAllText(bp_Path, bp_csv_Content.ToString());
                    File.AppendAllText(dz_Path, dz_csv_Content.ToString());
                    File.AppendAllText(jj_Path, jj_csv_Content.ToString());
                    File.AppendAllText(jp_Path, jp_csv_Content.ToString());
                    File.AppendAllText(ma_Path, ma_csv_Content.ToString());
                    File.AppendAllText(nm_Path, nm_csv_Content.ToString());
                    File.AppendAllText(rc_Path, rc_csv_Content.ToString());
                    File.AppendAllText(st_Path, st_csv_Content.ToString());
                    File.AppendAllText(tm_Path, tm_csv_Content.ToString());
                    File.AppendAllText(zb_Path, zb_csv_Content.ToString());
                    File.AppendAllText(org_Path, org_csv_Content.ToString());
                    File.AppendAllText(total_Path, total_csv_Content.ToString());

                    #endregion create csv files
                }

                else // if not an external plan
                {
                    string message = string.Format("Plan {0} is not an external beam plan. ", plan.Name);
                    MessageBox.Show(message);
                }

                #endregion script execution
            }
        }
    }
}
