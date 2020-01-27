namespace VMS.TPS
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Windows;
  using System.Windows.Media.Media3D;
  using System.Xml;
  using VMS.TPS.Common.Model.API;
  using VMS.TPS.Common.Model.Types;

  /// <summary>
  /// ESJO is short for Eclipse Scripting JSON Object. The ESJO Class can be used to create nested JSON objects from plan data.
  /// These JSON objects can then be written to JSON files and saved for later analysis. 
  /// JSON files are object-oriented and can be parsed clearly and easily. For more information on JSON and other object-oriented languages: 
  /// <see cref="!:https://en.wikipedia.org/wiki/JSON">WIKI</see> and 
  ///	AHref <a href="https://www.json.org/">JSON.ORG</a> <see href="https://www.json.org/">HERE</see>
  ///	NOTE: some comments may not accurately represent the methods they are associated with. Be sure to compare method name with associated comments.
  /// </summary>
  public class ESJO : IEnumerator, IEnumerable
  {
    #region object properties

    // JSON object key
    private string key;
    public string Key => key ?? string.Empty;
    // string value
    private string strValue;
    public string StrValue => strValue ?? string.Empty;
    // double value
    private double dblValue;
    public double DblValue => dblValue;
    // boolean value
    private bool boolValue;
    public bool BoolValue => boolValue;
    // object array of doubles
    // e.g., for storing DVH object
    private List<string> strListValue;
    public List<string> StrListValue { get { return strListValue != null ? strListValue : new List<string> { string.Empty }; } }

    // object array of doubles
    // e.g., for storing DVH object
    private List<Course> pCourseListValue;
    public List<Course> PCourseListValue { get { return pCourseListValue; } }

    // object array of beams
    private List<Beam> beamListValue;
    public List<Beam> BeamListValue { get { return beamListValue; } }

    // object array of Diagnosis
    private List<Diagnosis> dxListValue;
    public List<Diagnosis> DxListValue { get { return dxListValue; } }

    // object array of plansetups
    // e.g., for storing plans
    private List<PlanSetup> pSetupListValue;
    public List<PlanSetup> PSetupListValue { get { return pSetupListValue; } }

    // object array of plansetups
    // e.g., for storing plans
    private List<PPlan> pPlanListValue;
    public List<PPlan> PPlanListValue { get { return pPlanListValue; } }

    // object array of doubles
    // e.g., for storing DVH object
    private List<PlanSum> pSumListValue;
    public List<PlanSum> PSumListValue { get { return pSumListValue; } }

    // object array of doubles
    // e.g., for storing DVH object
    private List<int> intLstValue;
    public List<int> IntLstValue { get { return intLstValue != null ? intLstValue : new List<int> { }; } }

    // object array of doubles
    // e.g., for storing DVH object
    private List<Tuple<double, double>> tupLstValue;
    public List<Tuple<double, double>> TupLstValue { get { return tupLstValue != null ? tupLstValue : new List<Tuple<double, double>> { Tuple.Create(Double.NaN, Double.NaN) }; } }

    // object array of doubles
    // e.g., for storing DVH object
    private List<Tuple<string, Tuple<int, double>>> sumFractionationSchedule;
    public List<Tuple<string, Tuple<int, double>>> SumFractionationSchedule { get { return sumFractionationSchedule; } }

    // object array of objects
    // e.g., for storing nested object layers
    private List<ESJO> jsonObjectsList;
    public List<ESJO> JsonObjectsList { get { return jsonObjectsList != null ? jsonObjectsList : new List<ESJO> { CreateESJO(string.Empty, string.Empty) }; } }

    // output string in json format
    private string jsonString;
    public string JsonString { get { return jsonString != null ? jsonString : string.Empty; } }

    #endregion object properties

    #region object methods

    // Object Methods

    /// <summary>
    /// Null / Empty ESJO
    /// </summary>
    //private ESJO()
    //{
    //	key = null;
    //	strValue = null;
    //	dblValue = Double.NaN;
    //	boolValue = false;
    //	tupLstValue = null;
    //	jsonObjectsList = null;
    //	jsonString = null;
    //}
    public ESJO()
    {
      key = null;
      strValue = null;
      dblValue = Double.NaN;
      boolValue = false;
      strListValue = null;
      pSetupListValue = null;
      pSumListValue = null;
      pCourseListValue = null;
      tupLstValue = null;
      intLstValue = null;
      jsonObjectsList = null;
      jsonString = null;
    }

    /// <summary>
    /// Creates an object with a value of Type String
    /// </summary>
    /// <param name="inputKey">ESJOs Key</param>
    /// <param name="value">ESJOs Value</param>
    /// <returns></returns>
    public static ESJO CreateESJO(string inputKey, string value)
    {
      ESJO esjo = new ESJO();
      esjo.key = inputKey;
      esjo.strValue = value;
      esjo.jsonString = string.Format("\"{0}\":\"{1}\"", esjo.key, esjo.strValue);

      return esjo;
    }

    /// <summary>
    /// Creates an object with a value of Type Double
    /// </summary>
    /// <param name="inputKey">ESJOs Key</param>
    /// <param name="value">ESJOs Value</param>
    /// <returns></returns>
    public static ESJO CreateESJO(string inputKey, double value)
    {
      ESJO esjo = new ESJO();
      esjo.key = inputKey;
      esjo.dblValue = value;
      esjo.jsonString = string.Format("\"{0}\":{1}", esjo.key, esjo.dblValue);

      return esjo;
    }

    /// <summary>
    /// Creates an object with a value of Type Bool
    /// </summary>
    /// <param name="inputKey">ESJOs Key</param>
    /// <param name="value">ESJOs Value</param>
    /// <returns></returns>
    public static ESJO CreateESJO(string inputKey, bool value)
    {
      ESJO esjo = new ESJO();
      esjo.key = inputKey;
      esjo.boolValue = value;
      esjo.jsonString = string.Format("\"{0}\":{1}", esjo.key, esjo.boolValue);

      return esjo;
    }

    /// <summary>
    /// Creates an object with its value being an array of strings
    /// e.g., for storing a list of structures
    /// </summary>
    /// <param name="inputKey">ESJOs Key</param>
    /// <param name="value">ESJOs Value</param>
    /// <returns></returns>
    public static ESJO CreateESJO(string inputKey, List<string> value)
    {
      ESJO esjo = new ESJO();
      esjo.key = inputKey;
      esjo.strListValue = value;
      esjo.jsonString = "\"" + esjo.key + "\":[";
      foreach (var str in esjo.strListValue)
      {
        esjo.jsonString += string.Format("\"{0}\",", str);
      }
      esjo.jsonString = esjo.jsonString.TrimEnd(',');
      esjo.jsonString += "]";

      return esjo;
    }

    /// <summary>
    /// Creates an object with its value being an array of ints
    /// e.g., for storing a triangle indices
    /// </summary>
    /// <param name="inputKey">ESJOs Key</param>
    /// <param name="value">ESJOs Value</param>
    /// <returns></returns>
    public static ESJO CreateESJO(string inputKey, List<int> value)
    {
      ESJO esjo = new ESJO();
      esjo.key = inputKey;
      esjo.intLstValue = value;
      esjo.jsonString = "\"" + esjo.key + "\":[";
      foreach (var str in esjo.intLstValue)
      {
        esjo.jsonString += string.Format("{0},", str);
      }
      esjo.jsonString = esjo.jsonString.TrimEnd(',');
      esjo.jsonString += "]";

      return esjo;
    }

    /// <summary>
    /// Creates an object with its value being an array of Courses
    /// e.g., for storing a list of the patients courses
    /// </summary>
    /// <param name="inputKey">Key</param>
    /// <param name="value">List of Courses</param>
    /// <returns></returns>
    public static ESJO CreateESJO(string inputKey, List<Course> value)
    {
      ESJO esjo = new ESJO();
      esjo.key = inputKey;
      esjo.pCourseListValue = value;
      esjo.jsonString = "\"" + esjo.key + "\":[";
      foreach (var course in esjo.pCourseListValue)
      {
        esjo.jsonString += string.Format("{0},", course.Id.ToString().Replace(" ", "_"));
      }
      esjo.jsonString = esjo.jsonString.TrimEnd(',');
      esjo.jsonString += "]";

      return esjo;
    }

    /// <summary>
    /// Creates an object with its value being an array of Beams
    /// e.g., for storing a list of the tx fields
    /// </summary>
    /// <param name="inputKey">Key</param>
    /// <param name="value">List of Courses</param>
    /// <returns></returns>
    public static ESJO CreateESJO(string inputKey, List<Beam> value)
    {
      ESJO esjo = new ESJO();
      esjo.key = inputKey;
      esjo.beamListValue = value;
      esjo.jsonString = "\"" + esjo.key + "\":[";
      foreach (var beam in esjo.beamListValue)
      {
        esjo.jsonString += string.Format("{{\"Id\":\"{0}\",\"MLCPlanType\":\"{1}\",\"MU\":{2},\"DoseRate\":{3},\"Machine\":\"{4}\",\"Energy\":\"{5}\",\"GantryDirection\":\"{6}\",\"Isocenter\":{{\"X\":{7},\"Y\":{8},\"Z\":{9} }}}},",
                                                beam.Id.ToString().Replace(" ", "_"), beam.MLCPlanType, beam.Meterset.Value, beam.DoseRate,
                                                beam.TreatmentUnit.Id, beam.EnergyModeDisplayName, beam.GantryDirection,
                                                beam.IsocenterPosition.x, beam.IsocenterPosition.y, beam.IsocenterPosition.z);
      }
      esjo.jsonString = esjo.jsonString.TrimEnd(',');
      esjo.jsonString += "]";

      return esjo;
    }

    /// <summary>
    /// Creates an object with its value being an array of Diagnoses
    /// </summary>
    /// <param name="inputKey">Key</param>
    /// <param name="value">List of Courses</param>
    /// <returns></returns>
    public static ESJO CreateESJO(string inputKey, List<Diagnosis> value)
    {
      ESJO esjo = new ESJO();
      esjo.key = inputKey;
      esjo.dxListValue = value;
      esjo.jsonString = "\"" + esjo.key + "\":[";
      foreach (var dx in esjo.dxListValue)
      {
        esjo.jsonString += string.Format("{{\"ClinicalDescription\":\"{0}\",\"Code\":\"{1}\",\"CodeTable\":\"{2}\",\"Comment\":\"{3}\",\"Id\":\"{4}\",\"Name\":\"{5}\"}},",
                                                dx.ClinicalDescription.Trim(), dx.Code.Trim(), dx.CodeTable.Trim(),
                                                dx.Comment.Trim(), dx.Id.Trim(), dx.Name.Trim());
      }
      esjo.jsonString = esjo.jsonString.TrimEnd(',');
      esjo.jsonString += "]";

      return esjo;
    }

    /// <summary>
    /// Creates an object with its value being an array of Plans
    /// e.g., for storing a list of the plans in a course
    /// </summary>
    /// <param name="inputKey">Key</param>
    /// <param name="value">List of Plans</param>
    /// <returns></returns>
    public static ESJO CreateESJO(string inputKey, List<PPlan> value)
    {
      ESJO mainEsjo = new ESJO();
      mainEsjo.key = inputKey;
      mainEsjo.pPlanListValue = value;
      mainEsjo.jsonString = "\"" + mainEsjo.key + "\":[";
      try
      {
        var planList = new List<ESJO>();
        var isCalculatedList = new List<Tuple<Structure, Structure>>();
        foreach (var plan in mainEsjo.pPlanListValue)
        {

          //MessageBox.Show(string.Format("{0} : {1}", plan.Id, plan.Type));
          if (plan.Type == "Plan")
          {
            #region if plan
            // sort structures for manipulation
            GenerateStructureList.cleanAndOrderStructures(plan.StructureSet, out IEnumerable<Structure> sorted_gtvList,
                                                            out IEnumerable<Structure> sorted_ctvList,
                                                            out IEnumerable<Structure> sorted_itvList,
                                                            out IEnumerable<Structure> sorted_ptvList,
                                                            out IEnumerable<Structure> sorted_targetList,
                                                            out IEnumerable<Structure> sorted_oarList,
                                                            out IEnumerable<Structure> sorted_structureList,
                                                            out IEnumerable<Structure> sorted_emptyStructuresList);

            //var m = string.Empty;
            //foreach(var oar in sorted_oarList)
            //{
            //    m += oar + "\n";
            //}

            //MessageBox.Show(m);

            var isOptimized = true;
            //var planType = string.Empty;
            var mlcTypes = new List<string>();
            foreach (var beam in plan.Beams)
            {
              if (beam.MLCPlanType == MLCPlanType.VMAT || beam.MLCPlanType == MLCPlanType.DoseDynamic) { isOptimized = true; }

              if (!beam.IsSetupField)
              {
                var mlcType = string.Empty;
                if (beam.GantryDirection != VMS.TPS.Common.Model.Types.GantryDirection.None)
                {
                  if (beam.MLCPlanType == VMS.TPS.Common.Model.Types.MLCPlanType.VMAT) mlcType = "VMAT";
                  else mlcType = "ARC";
                  mlcTypes.Add(mlcType);
                }
                else
                {
                  if (beam.MLCPlanType == VMS.TPS.Common.Model.Types.MLCPlanType.DoseDynamic) mlcType = "IMRT";
                  else mlcType = "STATIC";
                  mlcTypes.Add(mlcType);
                }
              }
            }
            foreach (var mtype in mlcTypes)
            {
              if (mtype != "VMAT" && mtype != "IMRT") { isOptimized = false; }
            } // a FiF plan will have some IMRT and some STATIC - need all to be IMRT or VMAT

            //MessageBox.Show("isOptimized Defined");


            var planJOList = new List<ESJO>();

            ESJO planId = ESJO.CreateESJO("PlanId", plan.PSetup.Id.ToString().Replace(" ", "_")); planJOList.Add(planId);
            ESJO planApprovalStatus = ESJO.CreateESJO("ApprovalStatus", plan.ApprovalStatus.ToString()); planJOList.Add(planApprovalStatus);
            ESJO planComment = ESJO.CreateESJO("Comment", plan.PSetup.Comment.ToString()); planJOList.Add(planComment);
            ESJO planCreationDate = ESJO.CreateESJO("CreationDate", plan.PSetup.CreationDateTime.ToString()); planJOList.Add(planCreationDate);
            ESJO planCreationUserName = ESJO.CreateESJO("CreationUserName", plan.PSetup.CreationUserName.ToString()); planJOList.Add(planCreationUserName);
            ESJO planTreated = ESJO.CreateESJO("IsTreated", plan.PSetup.IsTreated.ToString().ToLower()); planJOList.Add(planTreated);
            ESJO planIntent = ESJO.CreateESJO("Intent", plan.PSetup.PlanIntent); planJOList.Add(planIntent);
            ESJO planTxOrientation = ESJO.CreateESJO("TreatmentOrientation", plan.PSetup.TreatmentOrientation.ToString()); planJOList.Add(planTxOrientation);
            //MessageBox.Show("Plan Beams Starting");

            var beamInfoList = new List<ESJO>();
            ESJO beamInfo = ESJO.CreateESJO("TreatmentFields", plan.Beams.ToList()); planJOList.Add(beamInfo);

            //ESJO beamInfo = ESJO.CreateESJO("TreatmentFields", plan.Beams.ToList());

            //MessageBox.Show("Plan Desctiption JOs Created");


            // image info
            var imageList = new List<ESJO>();
            ESJO imageAssociatedImage = ESJO.CreateESJO("Id", plan.StructureSet.Image.Id.ToString().Replace(" ", "_")); imageList.Add(imageAssociatedImage);
            ESJO imageAssignedStructureSet = ESJO.CreateESJO("StructureSet", plan.StructureSet.Id.ToString().Replace(" ", "_")); imageList.Add(imageAssignedStructureSet);
            ESJO imageSeries = ESJO.CreateESJO("Series", plan.StructureSet.Image.Series.ToString().Replace(" ", "_")); imageList.Add(imageSeries);
            ESJO imageLength = ESJO.CreateESJO("Length", Math.Round(plan.StructureSet.Image.Origin.Length, 3)); imageList.Add(imageLength);

            //MessageBox.Show("Plan Image JOs Created");


            // image origin info
            var originList = new List<ESJO>();
            ESJO imageOrigin_X = ESJO.CreateESJO("X", Math.Round(plan.StructureSet.Image.Origin.x, 3)); originList.Add(imageOrigin_X);
            ESJO imageOrigin_Y = ESJO.CreateESJO("Y", Math.Round(plan.StructureSet.Image.Origin.y, 3)); originList.Add(imageOrigin_Y);
            ESJO imageOrigin_Z = ESJO.CreateESJO("Z", Math.Round(plan.StructureSet.Image.Origin.z, 3)); originList.Add(imageOrigin_Z);
            ESJO imageOriginInfo = ESJO.CreateESJO("Origin", originList); imageList.Add(imageOriginInfo); // add origin info to image info list

            //MessageBox.Show("Image Origin JOs Created");


            // user origin info
            var userOriginList = new List<ESJO>();
            ESJO imageHasUserOrigin = ESJO.CreateESJO("HasUserOrigin", plan.StructureSet.Image.HasUserOrigin.ToString().ToLower()); userOriginList.Add(imageHasUserOrigin);
            if (plan.StructureSet.Image.HasUserOrigin)
            {
              ESJO imageUserOrigin_X = ESJO.CreateESJO("X", Math.Round(plan.StructureSet.Image.UserOrigin.x, 3)); userOriginList.Add(imageUserOrigin_X);
              ESJO imageUserOrigin_Y = ESJO.CreateESJO("Y", Math.Round(plan.StructureSet.Image.UserOrigin.y, 3)); userOriginList.Add(imageUserOrigin_Y);
              ESJO imageUserOrigin_Z = ESJO.CreateESJO("Z", Math.Round(plan.StructureSet.Image.UserOrigin.z, 3)); userOriginList.Add(imageUserOrigin_Z);
            }
            ESJO imageUserOriginInfo = ESJO.CreateESJO("UserOrigin", userOriginList, "{", "}"); imageList.Add(imageUserOriginInfo); // add user origin info to image info list
            ESJO planImageInfo = ESJO.CreateESJO("Image", imageList, "{", "}"); planJOList.Add(planImageInfo); // add image info to plan info

            //MessageBox.Show("Image JOs Added");


            // target volume id
            try
            {
              ESJO planTargetVolume = ESJO.CreateESJO("TargetVolume", plan.PSetup.TargetVolumeID.Replace(" ", "_")); planJOList.Add(planTargetVolume);
            }
            catch
            {
              //MessageBox.Show("Target Volume Not Selected");
            }

            // rx info
            plan.PSetup.DoseValuePresentation = DoseValuePresentation.Absolute;
            ESJO planTotalPrescribedDose = ESJO.CreateESJO("TotalPrescribedDose", plan.RxDose); planJOList.Add(planTotalPrescribedDose);
            if (plan.PSetup.NumberOfFractions.HasValue)
            {
              ESJO planNumberOfFractions = ESJO.CreateESJO("Fractions", plan.Fractions); planJOList.Add(planNumberOfFractions);
            }
            else
            {
              ESJO planNumberOfFractions = ESJO.CreateESJO("Fractions", "NoValue"); planJOList.Add(planNumberOfFractions);
            }
            ESJO planDosePerFraction = ESJO.CreateESJO("DosePerFraction", plan.DosePerFraction); planJOList.Add(planDosePerFraction);

            //MessageBox.Show("Rx JOs Added");


            // optimization info
            var optimizationList = new List<ESJO>();
            if (isOptimized)
            {

              var structuresWithObjectives = new List<string>();
              ESJO planUsesJawTracking = ESJO.CreateESJO("UsesJawTracking", plan.PSetup.OptimizationSetup.UseJawTracking.ToString().ToLower()); planJOList.Add(planUsesJawTracking);

              var optiObjectivesList = new List<ESJO>();
              var optiParametersList = new List<ESJO>();

              var directoryPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanData__\\_Exports_\\" + plan.Course.Patient.Id;
              if (!Directory.Exists(directoryPath))
              {
                Directory.CreateDirectory(directoryPath);
              }

              var optiXmlPath = directoryPath + "\\" + plan.Id + "_OptimizationInfo.xml";

              XmlTextWriter writer = new XmlTextWriter(optiXmlPath, System.Text.Encoding.UTF8);
              writer.Formatting = Formatting.Indented;

              writer.WriteStartDocument();

              writer.WriteComment("XML File for Optimization Parameters");

              writer.WriteStartElement("Optimization");

              foreach (var obj in plan.PSetup.OptimizationSetup.Objectives)
              {
                obj.WriteXml(writer);
                structuresWithObjectives.Add(obj.StructureId);
              }

              foreach (var param in plan.PSetup.OptimizationSetup.Parameters)
              {
                param.WriteXml(writer);
              }
              writer.WriteEndElement();
              writer.WriteEndDocument();
              writer.Flush();
              writer.Close();

              ESJO planStructuresWithObjectives = CreateESJO("StructuresWithOptimizationObjectives", structuresWithObjectives); planJOList.Add(planStructuresWithObjectives);

            }
            //MessageBox.Show("Opti JOs Added");


            // dose statistics
            var doseList = new List<ESJO>();
            //if (plan.ApprovalStatus == PlanSetupApprovalStatus.Reviewed ||
            //    plan.ApprovalStatus == PlanSetupApprovalStatus.PlanningApproved ||
            //    plan.ApprovalStatus == PlanSetupApprovalStatus.TreatmentApproved)
            //{
            //ESJO doseSeries = ESJO.CreateESJO("Series", plan.PSetup.Dose.Series.Id.ToString().Replace(" ", "_")); doseList.Add(doseSeries);

            plan.PSetup.DoseValuePresentation = DoseValuePresentation.Absolute;
            //MessageBox.Show("1");
            ESJO absDoseMax = ESJO.CreateESJO("AbsDoseMax3D", Math.Round(plan.PSetup.Dose.DoseMax3D.Dose, 5)); doseList.Add(absDoseMax);

            plan.PSetup.DoseValuePresentation = DoseValuePresentation.Relative;
            ESJO relDoseMax = ESJO.CreateESJO("RelDoseMax3D", Math.Round(plan.PSetup.Dose.DoseMax3D.Dose, 5)); doseList.Add(relDoseMax);

            ESJO doseMaxLocation_X = ESJO.CreateESJO("DoseMax3DLocation_X", Math.Round(plan.PSetup.Dose.DoseMax3DLocation.x, 3)); doseList.Add(doseMaxLocation_X);
            ESJO doseMaxLocation_Y = ESJO.CreateESJO("DoseMax3DLocation_Y", Math.Round(plan.PSetup.Dose.DoseMax3DLocation.y, 3)); doseList.Add(doseMaxLocation_Y);
            ESJO doseMaxLocation_Z = ESJO.CreateESJO("DoseMax3DLocation_Z", Math.Round(plan.PSetup.Dose.DoseMax3DLocation.z, 3)); doseList.Add(doseMaxLocation_Z);

            //var isodosesList = new List<ESJO>();
            //plan.PSetup.DoseValuePresentation = DoseValuePresentation.Absolute; 
            //foreach (var dose in plan.PSetup.Dose.Isodoses)
            //{
            //    if (dose.Level.Dose <= plan.MaxDose)
            //    {
            //       //MessageBox.Show(dose.Level.ValueAsString + " started");

            //        var isodoseList = new List<ESJO>();
            //        var meshGeometryList = new List<ESJO>();
            //        var boundsList = new List<ESJO>();
            //        var sizeList = new List<ESJO>();
            //        //var locationList = new List<ESJO>();

            //        ESJO color = ESJO.CreateESJO("Color", string.Format("({0},{1},{2})", dose.Color.R, dose.Color.G, dose.Color.B)); isodoseList.Add(color);
            //        ESJO level = ESJO.CreateESJO("Level", dose.Level.ValueAsString); isodoseList.Add(level);


            //       //MessageBox.Show("color and level added");


            //        //ESJO boundsLocation_X = ESJO.CreateESJO("X", dose.MeshGeometry.Bounds.Location.X); locationList.Add(boundsLocation_X);
            //        //ESJO boundsLocation_Y = ESJO.CreateESJO("Y", dose.MeshGeometry.Bounds.Location.Y); locationList.Add(boundsLocation_Y);
            //        //ESJO boundsLocation_Z = ESJO.CreateESJO("Z", dose.MeshGeometry.Bounds.Location.Z); locationList.Add(boundsLocation_Z);
            //        //ESJO location = ESJO.CreateESJO("Location", locationList, "{", "}"); boundsList.Add(location); // add location obj to bounds obj

            //       //MessageBox.Show("Location added");

            //        //ESJO size_X = ESJO.CreateESJO("X", dose.MeshGeometry.Bounds.Size.X); sizeList.Add(size_X);
            //        //ESJO size_Y = ESJO.CreateESJO("Y", dose.MeshGeometry.Bounds.Size.Y); sizeList.Add(size_Y);
            //        //ESJO size_Z = ESJO.CreateESJO("Z", dose.MeshGeometry.Bounds.Size.Z); sizeList.Add(size_Z);

            //        //ESJO size = ESJO.CreateESJO("Size", sizeList, "{", "}"); boundsList.Add(size); // add size obj to bounds obj

            //       //MessageBox.Show("Size added");


            //        //ESJO bounds_X = ESJO.CreateESJO("X", dose.MeshGeometry.Bounds.X); boundsList.Add(bounds_X);
            //        //ESJO bounds_Y = ESJO.CreateESJO("Y", dose.MeshGeometry.Bounds.Y); boundsList.Add(bounds_Y);
            //        //ESJO bounds_Z = ESJO.CreateESJO("Z", dose.MeshGeometry.Bounds.Z); boundsList.Add(bounds_Z);
            //        //ESJO boundsSize_X = ESJO.CreateESJO("SizeX", dose.MeshGeometry.Bounds.SizeX); boundsList.Add(boundsSize_X);
            //        //ESJO boundsSize_Y = ESJO.CreateESJO("SizeY", dose.MeshGeometry.Bounds.SizeY); boundsList.Add(boundsSize_Y);
            //        //ESJO boundsSize_Z = ESJO.CreateESJO("SizeZ", dose.MeshGeometry.Bounds.SizeZ); boundsList.Add(boundsSize_Z);

            //        //ESJO bounds = ESJO.CreateESJO("Bounds", boundsList, "{", "}"); meshGeometryList.Add(bounds); // add bounds object to mesh geometry obj

            //       //MessageBox.Show("Bounds added");


            //        //var positionsList = new List<ESJO>();
            //        //for (var i = 0; i < dose.MeshGeometry.Positions.Count; i++)
            //        //{
            //        //    var coordinates = new List<ESJO>();
            //        //    var p = dose.MeshGeometry.Positions[i];
            //        //    ESJO x = ESJO.CreateESJO("X", p.X); coordinates.Add(x);
            //        //    ESJO y = ESJO.CreateESJO("Y", p.Y); coordinates.Add(y);
            //        //    ESJO z = ESJO.CreateESJO("Z", p.Z); coordinates.Add(z);

            //        //    ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); positionsList.Add(point);
            //        //}
            //        //ESJO postions = ESJO.CreateESJO("Positions", positionsList, "[", "]"); meshGeometryList.Add(postions); // add positions object to mesh geometry obj

            //       //MessageBox.Show("Positions added");


            //        var textureCoordinatesList = new List<ESJO>();
            //        for (var i = 0; i < dose.MeshGeometry.TextureCoordinates.Count; i++)
            //        {
            //            var coordinates = new List<ESJO>();
            //            var tc = dose.MeshGeometry.TextureCoordinates[i];
            //            ESJO x = ESJO.CreateESJO("X", tc.X); coordinates.Add(x);
            //            ESJO y = ESJO.CreateESJO("Y", tc.Y); coordinates.Add(y);

            //            ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); textureCoordinatesList.Add(point);
            //        }
            //        ESJO textureCoordinates = ESJO.CreateESJO("TextureCoordinates", textureCoordinatesList, "[", "]"); meshGeometryList.Add(textureCoordinates); // add texture coordinates object to mesh geometry obj

            //       //MessageBox.Show("texture coordinates added");


            //        var normalsList = new List<ESJO>();
            //        for (var i = 0; i < dose.MeshGeometry.Normals.Count; i++)
            //        {
            //            var coordinates = new List<ESJO>();
            //            var n = dose.MeshGeometry.Normals[i];
            //            ESJO length = ESJO.CreateESJO("Length", n.Length); coordinates.Add(length);
            //            ESJO x = ESJO.CreateESJO("X", n.X); coordinates.Add(x);
            //            ESJO y = ESJO.CreateESJO("Y", n.Y); coordinates.Add(y);
            //            ESJO z = ESJO.CreateESJO("Z", n.Z); coordinates.Add(z);

            //            ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); normalsList.Add(point);
            //        }
            //        ESJO normals = ESJO.CreateESJO("Normals", normalsList, "[", "]"); meshGeometryList.Add(textureCoordinates); // add normals object to mesh geometry obj

            //       //MessageBox.Show("Normals added");


            //        var triangleIndicesList = new List<int>();
            //        for (var i = 0; i < dose.MeshGeometry.TriangleIndices.Count; i++)
            //        {
            //            triangleIndicesList.Add(dose.MeshGeometry.TriangleIndices[i]);
            //        }
            //        ESJO triangleIndices = ESJO.CreateESJO("TriangleIndices", triangleIndicesList); meshGeometryList.Add(triangleIndices); // add normals object to mesh geometry obj

            //       //MessageBox.Show("Triangle Indices added");


            //        ESJO meshGeometry = ESJO.CreateESJO("MeshGeometry", meshGeometryList, "{", "}"); isodoseList.Add(meshGeometry); // add mesh geometry object to main isodose obj

            //       //MessageBox.Show("mesh added");


            //        ESJO isodose = ESJO.CreateESJO(isodoseList, "{", "}"); isodosesList.Add(isodose);

            //       //MessageBox.Show(dose.Level.ValueAsString + " added");
            //    }


            //}
            //ESJO isodoses = ESJO.CreateESJO("Isodoses", isodosesList, "[", "]"); doseList.Add(isodoses);
            //}
            ESJO planIsodoseData = ESJO.CreateESJO("DoseMaxStatistics", doseList); planJOList.Add(planIsodoseData);

            ////MessageBox.Show("Isodose JOs Added");

            //MessageBox.Show("2");


            var oarDataObjectList = new List<ESJO>();
            foreach (var structure in sorted_oarList)
            {
              // dvhdatas
              DVHData dvhAA = plan.PItem.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);
              DVHData dvhAR = plan.PItem.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
              DVHData dvhRR = plan.PItem.GetDVHCumulativeData(structure, DoseValuePresentation.Relative, VolumePresentation.Relative, 0.001);


              // lists
              List<Tuple<double, double>> relVolDvhTuple = new List<Tuple<double, double>>();
              List<Tuple<double, double>> absVolDvhTuple = new List<Tuple<double, double>>();
              List<Tuple<double, double>> relDoseAndVolDvhTuple = new List<Tuple<double, double>>();
              var joList = new List<ESJO>();

              // esjos
              ESJO id = ESJO.CreateESJO("StructureId", structure.Id.ToString().Replace(" ", "_")); joList.Add(id);
              ESJO dicomType = ESJO.CreateESJO("DicomType", structure.DicomType); joList.Add(dicomType);
            //MessageBox.Show("3");

              // color
              ESJO color = ESJO.CreateESJO("RGB", string.Format("rgb({0},{1},{2})", structure.Color.R, structure.Color.G, structure.Color.B)); joList.Add(color);
              ESJO colorA = ESJO.CreateESJO("RGBA", string.Format("rgba({0},{1},{2},{3})", structure.Color.R, structure.Color.G, structure.Color.B, structure.Color.A)); joList.Add(colorA);

              // structure centerpoint
              var centerpointList = new List<ESJO>();
              ESJO centerPoint_X = ESJO.CreateESJO("X", Math.Round(structure.CenterPoint.x, 3)); centerpointList.Add(centerPoint_X);
              ESJO centerPoint_Y = ESJO.CreateESJO("Y", Math.Round(structure.CenterPoint.y, 3)); centerpointList.Add(centerPoint_Y);
              ESJO centerPoint_Z = ESJO.CreateESJO("Z", Math.Round(structure.CenterPoint.z, 3)); centerpointList.Add(centerPoint_Z);
              ESJO centerPoint = ESJO.CreateESJO("CenterPoint", centerpointList); joList.Add(centerPoint); // add centerpoint
            //MessageBox.Show("4");

              var meshGeometryList = new List<ESJO>();
              var boundsList = new List<ESJO>();
              //var sizeList = new List<ESJO>();
              //var locationList = new List<ESJO>();

              //ESJO boundsLocation_X = ESJO.CreateESJO("X", structure.MeshGeometry.Bounds.Location.X); locationList.Add(boundsLocation_X);
              //ESJO boundsLocation_Y = ESJO.CreateESJO("Y", structure.MeshGeometry.Bounds.Location.Y); locationList.Add(boundsLocation_Y);
              //ESJO boundsLocation_Z = ESJO.CreateESJO("Z", structure.MeshGeometry.Bounds.Location.Z); locationList.Add(boundsLocation_Z);
              //ESJO location = ESJO.CreateESJO("Location", locationList, "{", "}"); boundsList.Add(location); // add location obj to bounds obj

              //ESJO size_X = ESJO.CreateESJO("X", structure.MeshGeometry.Bounds.Size.X); sizeList.Add(size_X);
              //ESJO size_Y = ESJO.CreateESJO("Y", structure.MeshGeometry.Bounds.Size.Y); sizeList.Add(size_Y);
              //ESJO size_Z = ESJO.CreateESJO("Z", structure.MeshGeometry.Bounds.Size.Z); sizeList.Add(size_Z);

              //ESJO size = ESJO.CreateESJO("Size", sizeList, "{", "}"); boundsList.Add(size); // add size obj to bounds obj

              ESJO bounds_X = ESJO.CreateESJO("X", structure.MeshGeometry.Bounds.X); boundsList.Add(bounds_X);
              ESJO bounds_Y = ESJO.CreateESJO("Y", structure.MeshGeometry.Bounds.Y); boundsList.Add(bounds_Y);
              ESJO bounds_Z = ESJO.CreateESJO("Z", structure.MeshGeometry.Bounds.Z); boundsList.Add(bounds_Z);
              ESJO boundsSize_X = ESJO.CreateESJO("SizeX", structure.MeshGeometry.Bounds.SizeX); boundsList.Add(boundsSize_X);
              ESJO boundsSize_Y = ESJO.CreateESJO("SizeY", structure.MeshGeometry.Bounds.SizeY); boundsList.Add(boundsSize_Y);
              ESJO boundsSize_Z = ESJO.CreateESJO("SizeZ", structure.MeshGeometry.Bounds.SizeZ); boundsList.Add(boundsSize_Z);

              ESJO bounds = ESJO.CreateESJO("Bounds", boundsList, "{", "}"); meshGeometryList.Add(bounds); // add bounds object to mesh geometry obj
            //MessageBox.Show("5");

              //var positionsList = new List<ESJO>();
              //for (var i = 0; i < structure.MeshGeometry.Positions.Count; i++)
              //{
              //    var coordinates = new List<ESJO>();
              //    var p = structure.MeshGeometry.Positions[i];
              //    ESJO x = ESJO.CreateESJO("X", p.X); coordinates.Add(x);
              //    ESJO y = ESJO.CreateESJO("Y", p.Y); coordinates.Add(y);
              //    ESJO z = ESJO.CreateESJO("Z", p.Z); coordinates.Add(z);

              //    ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); positionsList.Add(point);
              //}
              //ESJO postions = ESJO.CreateESJO("Positions", positionsList, "[{", "}]"); meshGeometryList.Add(postions); // add positions object to mesh geometry obj

              //var textureCoordinatesList = new List<ESJO>();
              //for (var i = 0; i < structure.MeshGeometry.TextureCoordinates.Count; i++)
              //{
              //    var coordinates = new List<ESJO>();
              //    var tc = structure.MeshGeometry.TextureCoordinates[i];
              //    ESJO x = ESJO.CreateESJO("X", tc.X); coordinates.Add(x);
              //    ESJO y = ESJO.CreateESJO("Y", tc.Y); coordinates.Add(y);

              //    ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); textureCoordinatesList.Add(point);
              //}
              //ESJO textureCoordinates = ESJO.CreateESJO("TextureCoordinates", textureCoordinatesList, "[", "]"); meshGeometryList.Add(textureCoordinates); // add texture coordinates object to mesh geometry obj

              //var normalsList = new List<ESJO>();
              //for (var i = 0; i < structure.MeshGeometry.Normals.Count; i++)
              //{
              //    var coordinates = new List<ESJO>();
              //    var n = structure.MeshGeometry.Normals[i];
              //    ESJO length = ESJO.CreateESJO("Length", n.Length); coordinates.Add(length);
              //    ESJO x = ESJO.CreateESJO("X", n.X); coordinates.Add(x);
              //    ESJO y = ESJO.CreateESJO("Y", n.Y); coordinates.Add(y);
              //    ESJO z = ESJO.CreateESJO("Z", n.Z); coordinates.Add(z);

              //    ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); normalsList.Add(point);
              //}
              //ESJO normals = ESJO.CreateESJO("Normals", normalsList, "[", "]"); meshGeometryList.Add(textureCoordinates); // add normals object to mesh geometry obj

              //var triangleIndicesList = new List<int>();
              //for (var i = 0; i < structure.MeshGeometry.TriangleIndices.Count; i++)
              //{
              //    triangleIndicesList.Add(structure.MeshGeometry.TriangleIndices[i]);
              //}
              //ESJO triangleIndices = ESJO.CreateESJO("TriangleIndices", triangleIndicesList); meshGeometryList.Add(triangleIndices); // add normals object to mesh geometry obj

              ESJO meshGeometry = ESJO.CreateESJO("MeshGeometry", meshGeometryList, "{", "}"); joList.Add(meshGeometry); // add mesh geometry object to main isodose obj

              ESJO segments = ESJO.CreateESJO("NumberOfSegments", (structure.GetNumberOfSeparateParts().ToString())); joList.Add(segments);
              ESJO isHighResolution = ESJO.CreateESJO("IsHighResolution", structure.IsHighResolution.ToString().ToLower()); joList.Add(isHighResolution);
              ESJO volume = ESJO.CreateESJO("Volume_cc", Math.Round(structure.Volume, 5)); joList.Add(volume);
            //MessageBox.Show("6");

              // dose stats

              var sDoseStatsList = new List<ESJO>();
              //if (plan.ApprovalStatus == PlanSetupApprovalStatus.Reviewed ||
              //plan.ApprovalStatus == PlanSetupApprovalStatus.PlanningApproved ||
              //plan.ApprovalStatus == PlanSetupApprovalStatus.TreatmentApproved)
              //{
              var sVolume = Math.Round(structure.Volume, 5);

              if (dvhAA.MaxDose.Unit == DoseValue.DoseUnit.Gy)
              {
                ESJO minDose_03 = ESJO.CreateESJO("MinDose_0_03cc_Gy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (sVolume - 0.03)), 5)); sDoseStatsList.Add(minDose_03);
                ESJO minDose = ESJO.CreateESJO("MinDose_Gy", Math.Round(dvhAA.MinDose.Dose, 5)); sDoseStatsList.Add(minDose);
                ESJO maxDose_03 = ESJO.CreateESJO("MaxDose_0_03cc_Gy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, 0.03), 5)); sDoseStatsList.Add(maxDose_03);
                ESJO maxDose = ESJO.CreateESJO("MaxDose_Gy", Math.Round(dvhAA.MaxDose.Dose, 5)); sDoseStatsList.Add(maxDose);
                ESJO meanDose = ESJO.CreateESJO("MeanDose_Gy", Math.Round(dvhAA.MeanDose.Dose, 5)); sDoseStatsList.Add(meanDose);
                ESJO medianDose = ESJO.CreateESJO("MedianDose_Gy", Math.Round(dvhAA.MedianDose.Dose, 5)); sDoseStatsList.Add(medianDose);
                ESJO std = ESJO.CreateESJO("STD", Math.Round(dvhAA.StdDev, 5)); sDoseStatsList.Add(std);
            //MessageBox.Show("7");

              }
              else if (dvhAA.MaxDose.Unit == DoseValue.DoseUnit.cGy)
              {
                ESJO minDose_03 = ESJO.CreateESJO("MinDose_0_03cc_cGy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (sVolume - 0.03)), 5)); sDoseStatsList.Add(minDose_03);
                ESJO minDose = ESJO.CreateESJO("MinDose_cGy", Math.Round(dvhAA.MinDose.Dose, 5)); sDoseStatsList.Add(minDose);
                ESJO maxDose_03 = ESJO.CreateESJO("MaxDose_0_03cc_cGy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, 0.03), 5)); sDoseStatsList.Add(maxDose_03);
                ESJO maxDose = ESJO.CreateESJO("MaxDose_cGy", Math.Round(dvhAA.MaxDose.Dose, 5)); sDoseStatsList.Add(maxDose);
                ESJO meanDose = ESJO.CreateESJO("MeanDose_cGy", Math.Round(dvhAA.MeanDose.Dose, 5)); sDoseStatsList.Add(meanDose);
                ESJO medianDose = ESJO.CreateESJO("MedianDose_cGy", Math.Round(dvhAA.MedianDose.Dose, 5)); sDoseStatsList.Add(medianDose);
                ESJO std = ESJO.CreateESJO("STD", Math.Round(dvhAA.StdDev, 5)); sDoseStatsList.Add(std);
            //MessageBox.Show("8");

              }

              for (double i = 0; i <= dvhAR.MaxDose.Dose + .1; i += .1)
              {
                relVolDvhTuple.Add(Tuple.Create(Math.Round(i, 2), Math.Round(DvhExtensions.getVolumeAtDose(dvhAR, i), 2)));
              }
              for (double i = 0; i <= dvhAA.MaxDose.Dose + .1; i += .1)
              {
                absVolDvhTuple.Add(Tuple.Create(Math.Round(i, 2), Math.Round(DvhExtensions.getVolumeAtDose(dvhAA, i), 3)));
              }
              for (double i = 0; i <= dvhRR.MaxDose.Dose + .5; i += .5)
              {
                relDoseAndVolDvhTuple.Add(Tuple.Create(Math.Round(i, 2), Math.Round(DvhExtensions.getVolumeAtDose(dvhRR, i), 3)));
              }
            //MessageBox.Show("9");

              ESJO dvh_relVol = ESJO.CreateESJO("DVH_AbsDose_RelVol", relVolDvhTuple); sDoseStatsList.Add(dvh_relVol);
              ESJO dvh_absVol = ESJO.CreateESJO("DVH_AbsDose_AbsVol", absVolDvhTuple); sDoseStatsList.Add(dvh_absVol);
              ESJO dvh_relDoseAndVol = ESJO.CreateESJO("DVH_RelDose_RelVol", relDoseAndVolDvhTuple); sDoseStatsList.Add(dvh_relDoseAndVol);
              //}
              ESJO doseStats = ESJO.CreateESJO("Dose", sDoseStatsList, "[{", "}]"); joList.Add(doseStats);

              ESJO structureObjects = ESJO.CreateESJO(joList, "{", "}");
              oarDataObjectList.Add(structureObjects);
            //MessageBox.Show("10");

            }
            // create json object to include all oar data and add it to plan data object list
            var structureData_jo = ESJO.CreateESJO("OarData", oarDataObjectList, "[", "]"); planJOList.Add(structureData_jo);

            //MessageBox.Show("OAR JOs Added");


            var targetDataObjectList = new List<ESJO>();
            foreach (var structure in sorted_targetList)
            {
              // dvhdatas
              DVHData dvhAA = plan.PItem.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);
              DVHData dvhAR = plan.PItem.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
              DVHData dvhRR = plan.PItem.GetDVHCumulativeData(structure, DoseValuePresentation.Relative, VolumePresentation.Relative, 0.001);


              // lists
              List<Tuple<double, double>> relVolDvhTuple = new List<Tuple<double, double>>();
              List<Tuple<double, double>> absVolDvhTuple = new List<Tuple<double, double>>();
              List<Tuple<double, double>> relDoseAndVolDvhTuple = new List<Tuple<double, double>>();
              var joList = new List<ESJO>();

              // esjos
              ESJO id = ESJO.CreateESJO("TargetId", structure.Id.ToString().Replace(" ", "_")); joList.Add(id);
              ESJO dicomType = ESJO.CreateESJO("DicomType", structure.DicomType); joList.Add(dicomType);

              // color
              ESJO color = ESJO.CreateESJO("RGB", string.Format("rgb({0},{1},{2})", structure.Color.R, structure.Color.G, structure.Color.B)); joList.Add(color);
              ESJO colorA = ESJO.CreateESJO("RGBA", string.Format("rgba({0},{1},{2},{3})", structure.Color.R, structure.Color.G, structure.Color.B, structure.Color.A)); joList.Add(colorA);

              // structure centerpoint
              var centerpointList = new List<ESJO>();
              ESJO centerPoint_X = ESJO.CreateESJO("X", Math.Round(structure.CenterPoint.x, 3)); centerpointList.Add(centerPoint_X);
              ESJO centerPoint_Y = ESJO.CreateESJO("Y", Math.Round(structure.CenterPoint.y, 3)); centerpointList.Add(centerPoint_Y);
              ESJO centerPoint_Z = ESJO.CreateESJO("Z", Math.Round(structure.CenterPoint.z, 3)); centerpointList.Add(centerPoint_Z);
              ESJO centerPoint = ESJO.CreateESJO("CenterPoint", centerpointList); joList.Add(centerPoint); // add centerpoint

              var meshGeometryList = new List<ESJO>();
              var boundsList = new List<ESJO>();
              //var sizeList = new List<ESJO>();
              //var locationList = new List<ESJO>();

              //ESJO boundsLocation_X = ESJO.CreateESJO("X", structure.MeshGeometry.Bounds.Location.X); locationList.Add(boundsLocation_X);
              //ESJO boundsLocation_Y = ESJO.CreateESJO("Y", structure.MeshGeometry.Bounds.Location.Y); locationList.Add(boundsLocation_Y);
              //ESJO boundsLocation_Z = ESJO.CreateESJO("Z", structure.MeshGeometry.Bounds.Location.Z); locationList.Add(boundsLocation_Z);
              //ESJO location = ESJO.CreateESJO("Location", locationList, "{", "}"); boundsList.Add(location); // add location obj to bounds obj

              //ESJO size_X = ESJO.CreateESJO("X", structure.MeshGeometry.Bounds.Size.X); sizeList.Add(size_X);
              //ESJO size_Y = ESJO.CreateESJO("Y", structure.MeshGeometry.Bounds.Size.Y); sizeList.Add(size_Y);
              //ESJO size_Z = ESJO.CreateESJO("Z", structure.MeshGeometry.Bounds.Size.Z); sizeList.Add(size_Z);

              //ESJO size = ESJO.CreateESJO("Size", sizeList, "{", "}"); boundsList.Add(size); // add size obj to bounds obj

              ESJO bounds_X = ESJO.CreateESJO("X", structure.MeshGeometry.Bounds.X); boundsList.Add(bounds_X);
              ESJO bounds_Y = ESJO.CreateESJO("Y", structure.MeshGeometry.Bounds.Y); boundsList.Add(bounds_Y);
              ESJO bounds_Z = ESJO.CreateESJO("Z", structure.MeshGeometry.Bounds.Z); boundsList.Add(bounds_Z);
              ESJO boundsSize_X = ESJO.CreateESJO("SizeX", structure.MeshGeometry.Bounds.SizeX); boundsList.Add(boundsSize_X);
              ESJO boundsSize_Y = ESJO.CreateESJO("SizeY", structure.MeshGeometry.Bounds.SizeY); boundsList.Add(boundsSize_Y);
              ESJO boundsSize_Z = ESJO.CreateESJO("SizeZ", structure.MeshGeometry.Bounds.SizeZ); boundsList.Add(boundsSize_Z);

              ESJO bounds = ESJO.CreateESJO("Bounds", boundsList, "{", "}"); meshGeometryList.Add(bounds); // add bounds object to mesh geometry obj

              //var positionsList = new List<ESJO>();
              //for (var i = 0; i < structure.MeshGeometry.Positions.Count; i++)
              //{
              //    var coordinates = new List<ESJO>();
              //    var p = structure.MeshGeometry.Positions[i];
              //    ESJO x = ESJO.CreateESJO("X", p.X); coordinates.Add(x);
              //    ESJO y = ESJO.CreateESJO("Y", p.Y); coordinates.Add(y);
              //    ESJO z = ESJO.CreateESJO("Z", p.Z); coordinates.Add(z);

              //    ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); positionsList.Add(point);
              //}
              //ESJO postions = ESJO.CreateESJO("Positions", positionsList, "[{", "}]"); meshGeometryList.Add(postions); // add positions object to mesh geometry obj

              //var textureCoordinatesList = new List<ESJO>();
              //for (var i = 0; i < structure.MeshGeometry.TextureCoordinates.Count; i++)
              //{
              //    var coordinates = new List<ESJO>();
              //    var tc = structure.MeshGeometry.TextureCoordinates[i];
              //    ESJO x = ESJO.CreateESJO("X", tc.X); coordinates.Add(x);
              //    ESJO y = ESJO.CreateESJO("Y", tc.Y); coordinates.Add(y);

              //    ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); textureCoordinatesList.Add(point);
              //}
              //ESJO textureCoordinates = ESJO.CreateESJO("TextureCoordinates", textureCoordinatesList, "[", "]"); meshGeometryList.Add(textureCoordinates); // add texture coordinates object to mesh geometry obj

              //var normalsList = new List<ESJO>();
              //for (var i = 0; i < structure.MeshGeometry.Normals.Count; i++)
              //{
              //    var coordinates = new List<ESJO>();
              //    var n = structure.MeshGeometry.Normals[i];
              //    ESJO length = ESJO.CreateESJO("Length", n.Length); coordinates.Add(length);
              //    ESJO x = ESJO.CreateESJO("X", n.X); coordinates.Add(x);
              //    ESJO y = ESJO.CreateESJO("Y", n.Y); coordinates.Add(y);
              //    ESJO z = ESJO.CreateESJO("Z", n.Z); coordinates.Add(z);

              //    ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); normalsList.Add(point);
              //}
              //ESJO normals = ESJO.CreateESJO("Normals", normalsList, "[", "]"); meshGeometryList.Add(textureCoordinates); // add normals object to mesh geometry obj

              //var triangleIndicesList = new List<int>();
              //for (var i = 0; i < structure.MeshGeometry.TriangleIndices.Count; i++)
              //{
              //    triangleIndicesList.Add(structure.MeshGeometry.TriangleIndices[i]);
              //}
              //ESJO triangleIndices = ESJO.CreateESJO("TriangleIndices", triangleIndicesList); meshGeometryList.Add(triangleIndices); // add normals object to mesh geometry obj

              ESJO meshGeometry = ESJO.CreateESJO("MeshGeometry", meshGeometryList, "{", "}"); joList.Add(meshGeometry); // add mesh geometry object to main isodose obj

              ESJO segments = ESJO.CreateESJO("NumberOfSegments", (structure.GetNumberOfSeparateParts().ToString())); joList.Add(segments);
              ESJO isHighResolution = ESJO.CreateESJO("IsHighResolution", structure.IsHighResolution.ToString().ToLower()); joList.Add(isHighResolution);
              ESJO volume = ESJO.CreateESJO("Volume_cc", Math.Round(structure.Volume, 5)); joList.Add(volume);

              // dose stats
              var tDoseStatsList = new List<ESJO>();
              //if (plan.ApprovalStatus == PlanSetupApprovalStatus.Reviewed ||
              //plan.ApprovalStatus == PlanSetupApprovalStatus.PlanningApproved ||
              //plan.ApprovalStatus == PlanSetupApprovalStatus.TreatmentApproved)
              //{
              var tVolume = Math.Round(structure.Volume, 5);

              if (dvhAA.MaxDose.Unit == DoseValue.DoseUnit.Gy)
              {
                ESJO d95 = ESJO.CreateESJO("D95pct_Gy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (tVolume * .95)), 5)); tDoseStatsList.Add(d95);
                ESJO minDose_03 = ESJO.CreateESJO("MinDose_0_03cc_Gy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (tVolume - 0.03)), 5)); tDoseStatsList.Add(minDose_03);
                ESJO minDose = ESJO.CreateESJO("MinDose_Gy", Math.Round(dvhAA.MinDose.Dose, 5)); tDoseStatsList.Add(minDose);
                ESJO maxDose_03 = ESJO.CreateESJO("MaxDose_0_03cc_Gy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, 0.03), 5)); tDoseStatsList.Add(maxDose_03);
                ESJO maxDose = ESJO.CreateESJO("MaxDose_Gy", Math.Round(dvhAA.MaxDose.Dose, 5)); tDoseStatsList.Add(maxDose);
                ESJO meanDose = ESJO.CreateESJO("MeanDose_Gy", Math.Round(dvhAA.MeanDose.Dose, 5)); tDoseStatsList.Add(meanDose);
                ESJO medianDose = ESJO.CreateESJO("MedianDose_Gy", Math.Round(dvhAA.MedianDose.Dose, 5)); tDoseStatsList.Add(medianDose);
                ESJO std = ESJO.CreateESJO("STD", Math.Round(dvhAA.StdDev, 5)); tDoseStatsList.Add(std);
              }
              else if (dvhAA.MaxDose.Unit == DoseValue.DoseUnit.cGy)
              {
                ESJO d95 = ESJO.CreateESJO("D95pct_cGy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (tVolume * .95)), 5)); tDoseStatsList.Add(d95);
                ESJO minDose_03 = ESJO.CreateESJO("MinDose_0_03cc_cGy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (tVolume - 0.03)), 5)); tDoseStatsList.Add(minDose_03);
                ESJO minDose = ESJO.CreateESJO("MinDose_cGy", Math.Round(dvhAA.MinDose.Dose, 5)); tDoseStatsList.Add(minDose);
                ESJO maxDose_03 = ESJO.CreateESJO("MaxDose_0_03cc_cGy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, 0.03), 5)); tDoseStatsList.Add(maxDose_03);
                ESJO maxDose = ESJO.CreateESJO("MaxDose_cGy", Math.Round(dvhAA.MaxDose.Dose, 5)); tDoseStatsList.Add(maxDose);
                ESJO meanDose = ESJO.CreateESJO("MeanDose_cGy", Math.Round(dvhAA.MeanDose.Dose, 5)); tDoseStatsList.Add(meanDose);
                ESJO medianDose = ESJO.CreateESJO("MedianDose_cGy", Math.Round(dvhAA.MedianDose.Dose, 5)); tDoseStatsList.Add(medianDose);
                ESJO std = ESJO.CreateESJO("STD", Math.Round(dvhAA.StdDev, 5)); tDoseStatsList.Add(std);
              }

              for (double i = 0; i <= dvhAR.MaxDose.Dose + .1; i += .1)
              {
                relVolDvhTuple.Add(Tuple.Create(Math.Round(i, 2), Math.Round(DvhExtensions.getVolumeAtDose(dvhAR, i), 2)));
              }
              for (double i = 0; i <= dvhAA.MaxDose.Dose + .1; i += .1)
              {
                absVolDvhTuple.Add(Tuple.Create(Math.Round(i, 2), Math.Round(DvhExtensions.getVolumeAtDose(dvhAA, i), 3)));
              }
              for (double i = 0; i <= dvhRR.MaxDose.Dose + .5; i += .5)
              {
                relDoseAndVolDvhTuple.Add(Tuple.Create(Math.Round(i, 2), Math.Round(DvhExtensions.getVolumeAtDose(dvhRR, i), 3)));
              }
              ESJO dvh_relVol = ESJO.CreateESJO("DVH_AbsDose_RelVol", relVolDvhTuple); tDoseStatsList.Add(dvh_relVol);
              ESJO dvh_absVol = ESJO.CreateESJO("DVH_AbsDose_AbsVol", absVolDvhTuple); tDoseStatsList.Add(dvh_absVol);
              ESJO dvh_relDoseAndVol = ESJO.CreateESJO("DVH_RelDose_RelVol", relDoseAndVolDvhTuple); tDoseStatsList.Add(dvh_relDoseAndVol);
              //}
              ESJO doseStats = ESJO.CreateESJO("Dose", tDoseStatsList, "[{", "}]"); joList.Add(doseStats);

              ESJO structureObjects = ESJO.CreateESJO(joList, "{", "}");
              targetDataObjectList.Add(structureObjects);
            }
            // create json object to include all target data and add it to plan data object list
            var targetData_jo = ESJO.CreateESJO("TargetData", targetDataObjectList, "[", "]"); planJOList.Add(targetData_jo);

            //MessageBox.Show("target JOs Added");


            var proxStatsObjectList = new List<ESJO>();

            foreach (var t in sorted_ptvList)
            {
              var targetList = new List<ESJO>();
              var tId = t.Id.ToString().Replace(" ", "_").Split(':').First();
              var tVolume = Math.Round(t.Volume, 3);
              var tColor = "#" + t.Color.ToString().Substring(3, 6);
              ESJO targetId = ESJO.CreateESJO("TargetId", tId); targetList.Add(targetId);
              ESJO targetColor = ESJO.CreateESJO("TargetHexColor", tColor); targetList.Add(targetColor);
              ESJO targetVolume = ESJO.CreateESJO("TargetVolume", tVolume); targetList.Add(targetVolume);

              var proxStatsList = new List<ESJO>();
              foreach (var s in sorted_oarList)
              {
                if (!isCalculatedList.Contains(Tuple.Create(t, s)))
                {
                  if (!s.Id.ToLower().Contains("ci-") ||
                      !s.Id.ToLower().Contains("ci_"))
                  {
                    var structureList = new List<ESJO>();
                    var sId = s.Id.ToString().Replace(" ", "_").Split(':').First();
                    var sVolume = Math.Round(s.Volume, 3);
                    var sColor = "#" + s.Color.ToString().Substring(3, 6);
                    ESJO structureId = ESJO.CreateESJO("StructureId", sId); structureList.Add(structureId);
                    ESJO overlappingTarget = ESJO.CreateESJO("OverlappingTarget", tId); structureList.Add(overlappingTarget);
                    ESJO structureColor = ESJO.CreateESJO("StructureHexColor", sColor); structureList.Add(structureColor);
                    ESJO structureVolume = ESJO.CreateESJO("StructureVolume", sVolume); structureList.Add(structureVolume);

                    var structureOverlapAbs = Math.Round(CalculateOverlap.VolumeOverlap(t, s), 3);
                    var structureOverlapPct = structureOverlapAbs == 0 ? 0 : Math.Round(CalculateOverlap.PercentOverlap(s, structureOverlapAbs), 1);
                    var targetOverlapPct = structureOverlapAbs == 0 ? 0 : Math.Round(CalculateOverlap.PercentOverlap(t, structureOverlapAbs), 1);
                    var distance = structureOverlapAbs > 0 ? 0 : Math.Round(CalculateOverlap.ShortestDistance(t, s), 1);
                    ESJO absoluteOverlap = ESJO.CreateESJO("AbsVolStructureOverlap", structureOverlapAbs); structureList.Add(absoluteOverlap);
                    ESJO sPercentOverlap = ESJO.CreateESJO("PctStructureOverlap", structureOverlapPct); structureList.Add(sPercentOverlap);
                    ESJO tPercentOverlap = ESJO.CreateESJO("PctTargetOverlap", targetOverlapPct); structureList.Add(tPercentOverlap);
                    ESJO distanceFromTarget = ESJO.CreateESJO("DistanceFromTarget", distance); structureList.Add(distanceFromTarget);

                    ESJO sJO = ESJO.CreateESJO(structureList, "{", "}"); proxStatsList.Add(sJO);
                    isCalculatedList.Add(Tuple.Create(t, s));
                  }
                }
              }
              ESJO sProxStats = ESJO.CreateESJO("StructureProximity", proxStatsList, "[", "]"); targetList.Add(sProxStats);
              ESJO tJO = ESJO.CreateESJO(targetList, "{", "}"); proxStatsObjectList.Add(tJO);

            }

            ESJO proxStats = ESJO.CreateESJO("ProximityStatistics", proxStatsObjectList, "[", "]"); planJOList.Add(proxStats);

            ////MessageBox.Show("Proximity JOs Added");


            ESJO planObjects = ESJO.CreateESJO(planJOList, "{", "}");
            planList.Add(planObjects);

            //MessageBox.Show(plan.Id + " completed");

            #endregion if plan
          }
          if (plan.Type == "PlanSum")
          {
            #region if plan sum
            // sort structures for manipulation
            GenerateStructureList.cleanAndOrderStructures(plan.PSum.StructureSet, out IEnumerable<Structure> sorted_gtvList,
                                                                                out IEnumerable<Structure> sorted_ctvList,
                                                                                out IEnumerable<Structure> sorted_itvList,
                                                                                out IEnumerable<Structure> sorted_ptvList,
                                                                                out IEnumerable<Structure> sorted_targetList,
                                                                                out IEnumerable<Structure> sorted_oarList,
                                                                                out IEnumerable<Structure> sorted_structureList,
                                                                                out IEnumerable<Structure> sorted_emptyStructuresList);

            ////MessageBox.Show("sum - Structure Lists Created");

            //var isOptimized = true;
            //var planType = string.Empty;
            //var mlcTypes = new List<string>();
            //foreach (var beam in plan.Beams)
            //{
            //    if (beam.MLCPlanType == MLCPlanType.VMAT || beam.MLCPlanType == MLCPlanType.DoseDynamic) { isOptimized = true; }

            //    if (!beam.IsSetupField)
            //    {
            //        var mlcType = string.Empty;
            //        if (beam.GantryDirection != VMS.TPS.Common.Model.Types.GantryDirection.None)
            //        {
            //            if (beam.MLCPlanType == VMS.TPS.Common.Model.Types.MLCPlanType.VMAT) mlcType = "VMAT";
            //            else mlcType = "ARC";
            //            mlcTypes.Add(mlcType);
            //        }
            //        else
            //        {
            //            if (beam.MLCPlanType == VMS.TPS.Common.Model.Types.MLCPlanType.DoseDynamic) mlcType = "IMRT";
            //            else mlcType = "STATIC";
            //            mlcTypes.Add(mlcType);
            //        }
            //        foreach (var mtype in mlcTypes) { if (mtype != "VMAT" || mtype != "IMRT") { isOptimized = false; } } // a FiF plan will have some IMRT and some STATIC - need all to be IMRT or VMAT
            //    }
            //}

            //MessageBox.Show("sum - isOptimized Defined");


            var planJOList = new List<ESJO>();

            ESJO planId = ESJO.CreateESJO("PlanId", plan.PSum.Id.ToString().Replace(" ", "_")); planJOList.Add(planId);
            ESJO planApprovalStatus = ESJO.CreateESJO("ApprovalStatus", "PlanSum"); planJOList.Add(planApprovalStatus);
            ESJO planComment = ESJO.CreateESJO("Comment", plan.PSum.Comment.ToString()); planJOList.Add(planComment);
            //ESJO planCreationDate = ESJO.CreateESJO("CreationDate", plan.PSum.CreationDateTime.ToString()); planJOList.Add(planCreationDate);
            //ESJO planCreationUserName = ESJO.CreateESJO("CreationUserName", plan.PSetup.CreationUserName.ToString()); planJOList.Add(planCreationUserName);
            //ESJO planTreated = ESJO.CreateESJO("IsTreated", plan.PSetup.IsTreated.ToString().ToLower()); planJOList.Add(planTreated);
            //ESJO planIntent = ESJO.CreateESJO("Intent", plan.PSetup.PlanIntent); planJOList.Add(planIntent);
            ESJO planTxOrientation = ESJO.CreateESJO("TreatmentOrientation", plan.PSetup.TreatmentOrientation.ToString()); planJOList.Add(planTxOrientation);

            var beamInfoList = new List<ESJO>();
            foreach (var tuple in plan.BeamsList)
            {
              ESJO bi = ESJO.CreateESJO(tuple.Item1, tuple.Item2);
              beamInfoList.Add(bi);
            }
            ESJO beamInfo = ESJO.CreateESJO("PlanSumTreatmentFields", beamInfoList, "[{", "}]"); planJOList.Add(beamInfo);

            //MessageBox.Show("sum - Plan Desctiption JOs Created");

            if (plan.StructureSets.Count() == 1)
            {
              // image info
              var imageList = new List<ESJO>();
              ESJO imageAssociatedImage = ESJO.CreateESJO("Id", plan.PSum.StructureSet.Image.Id.ToString().Replace(" ", "_")); imageList.Add(imageAssociatedImage);
              ESJO imageAssignedStructureSet = ESJO.CreateESJO("StructureSet", plan.PSum.StructureSet.Id.ToString().Replace(" ", "_")); imageList.Add(imageAssignedStructureSet);
              ESJO imageSeries = ESJO.CreateESJO("Series", plan.StructureSet.Image.Series.ToString().Replace(" ", "_")); imageList.Add(imageSeries);
              ESJO imageLength = ESJO.CreateESJO("StructureSet", Math.Round(plan.PSum.StructureSet.Image.Origin.Length, 3)); imageList.Add(imageLength);

              // image origin info
              var originList = new List<ESJO>();
              ESJO imageOrigin_X = ESJO.CreateESJO("X", Math.Round(plan.StructureSet.Image.Origin.x, 3)); originList.Add(imageOrigin_X);
              ESJO imageOrigin_Y = ESJO.CreateESJO("Y", Math.Round(plan.StructureSet.Image.Origin.y, 3)); originList.Add(imageOrigin_Y);
              ESJO imageOrigin_Z = ESJO.CreateESJO("Z", Math.Round(plan.StructureSet.Image.Origin.z, 3)); originList.Add(imageOrigin_Z);
              ESJO imageOriginInfo = ESJO.CreateESJO("Origin", imageList); imageList.Add(imageOriginInfo); // add origin info to image info list

              // user origin info
              var userOriginList = new List<ESJO>();
              ESJO imageHasUserOrigin = ESJO.CreateESJO("HasUserOrigin", plan.StructureSet.Image.HasUserOrigin.ToString().ToLower()); userOriginList.Add(imageHasUserOrigin);
              if (plan.StructureSet.Image.HasUserOrigin)
              {
                ESJO imageUserOrigin_X = ESJO.CreateESJO("X", Math.Round(plan.StructureSet.Image.UserOrigin.x, 3)); userOriginList.Add(imageUserOrigin_X);
                ESJO imageUserOrigin_Y = ESJO.CreateESJO("Y", Math.Round(plan.StructureSet.Image.UserOrigin.y, 3)); userOriginList.Add(imageUserOrigin_Y);
                ESJO imageUserOrigin_Z = ESJO.CreateESJO("Z", Math.Round(plan.StructureSet.Image.UserOrigin.z, 3)); userOriginList.Add(imageUserOrigin_Z);
              }
              ESJO imageUserOriginInfo = ESJO.CreateESJO("UserOrigin", userOriginList, "{", "}"); imageList.Add(imageUserOriginInfo); // add user origin info to image info list
              ESJO planImageInfo = ESJO.CreateESJO("Image", imageList, "{", "}"); planJOList.Add(planImageInfo); // add image info to plan info

            }
            else
            {
              var ssImageInfoList = new List<ESJO>();
              foreach (var ss in plan.StructureSets)
              {
                // image info
                var imageList = new List<ESJO>();
                ESJO imageAssociatedImage = ESJO.CreateESJO("Id", plan.StructureSet.Image.Id.ToString().Replace(" ", "_")); imageList.Add(imageAssociatedImage);
                ESJO imageAssignedStructureSet = ESJO.CreateESJO("StructureSet", plan.StructureSet.Id.ToString().Replace(" ", "_")); imageList.Add(imageAssignedStructureSet);
                ESJO imageSeries = ESJO.CreateESJO("Series", plan.StructureSet.Image.Series.ToString().Replace(" ", "_")); imageList.Add(imageSeries);
                ESJO imageLength = ESJO.CreateESJO("StructureSet", Math.Round(plan.StructureSet.Image.Origin.Length, 3)); imageList.Add(imageLength);

                // image origin info
                var originList = new List<ESJO>();
                ESJO imageOrigin_X = ESJO.CreateESJO("X", Math.Round(plan.StructureSet.Image.Origin.x, 3)); originList.Add(imageOrigin_X);
                ESJO imageOrigin_Y = ESJO.CreateESJO("Y", Math.Round(plan.StructureSet.Image.Origin.y, 3)); originList.Add(imageOrigin_Y);
                ESJO imageOrigin_Z = ESJO.CreateESJO("Z", Math.Round(plan.StructureSet.Image.Origin.z, 3)); originList.Add(imageOrigin_Z);
                ESJO imageOriginInfo = ESJO.CreateESJO("Origin", imageList); imageList.Add(imageOriginInfo); // add origin info to image info list

                // user origin info
                var userOriginList = new List<ESJO>();
                ESJO imageHasUserOrigin = ESJO.CreateESJO("HasUserOrigin", plan.StructureSet.Image.HasUserOrigin.ToString().ToLower()); userOriginList.Add(imageHasUserOrigin);
                if (plan.StructureSet.Image.HasUserOrigin)
                {
                  ESJO imageUserOrigin_X = ESJO.CreateESJO("X", Math.Round(plan.StructureSet.Image.UserOrigin.x, 3)); userOriginList.Add(imageUserOrigin_X);
                  ESJO imageUserOrigin_Y = ESJO.CreateESJO("Y", Math.Round(plan.StructureSet.Image.UserOrigin.y, 3)); userOriginList.Add(imageUserOrigin_Y);
                  ESJO imageUserOrigin_Z = ESJO.CreateESJO("Z", Math.Round(plan.StructureSet.Image.UserOrigin.z, 3)); userOriginList.Add(imageUserOrigin_Z);
                }
                ESJO imageUserOriginInfo = ESJO.CreateESJO("UserOrigin", userOriginList, "{", "}"); imageList.Add(imageUserOriginInfo); // add user origin info to image info list
                ESJO planImageInfo = ESJO.CreateESJO(imageList, "{", "}"); ssImageInfoList.Add(planImageInfo); // add image info to plan info
              }
              ESJO sumImageInfo = ESJO.CreateESJO("Images", ssImageInfoList, "[", "]");
            }

            // target volume id
            try
            {
              var listOfTargetVolumes = new List<string>();
              foreach (var ps in plan.PSetups) { listOfTargetVolumes.Add(ps.TargetVolumeID.Replace(" ", "_")); }
              ESJO plansumTargetVolumes = ESJO.CreateESJO("TargetVolumes", listOfTargetVolumes); planJOList.Add(plansumTargetVolumes);
            }
            catch
            {
              //MessageBox.Show("Sum - No Target Volumes Collected");
            }

            // rx info
            plan.PSetup.DoseValuePresentation = DoseValuePresentation.Absolute;
            ESJO planSumFractionationSchedule = ESJO.CreateESJO("PlanSumFractionationSchedule", plan.PlanFractionList); planJOList.Add(planSumFractionationSchedule);
            //if (plan.PSetupNumberOfFractions.HasValue)
            //{
            //    ESJO planNumberOfFractions = ESJO.CreateESJO("Fractions", plan.Fractions); planJOList.Add(planNumberOfFractions);
            //}
            //else
            //{
            //    ESJO planNumberOfFractions = ESJO.CreateESJO("Fractions", "NoValue"); planJOList.Add(planNumberOfFractions);
            //}
            //ESJO planDosePerFraction = ESJO.CreateESJO("DosePerFraction", plan.DosePerFraction); planJOList.Add(planDosePerFraction);

            //MessageBox.Show("sum - Rx JOs Added");


            //// optimization info
            //var optimizationList = new List<ESJO>();
            //if (isOptimized)
            //{
            //    var structuresWithObjectives = new List<string>();
            //    ESJO planUsesJawTracking = ESJO.CreateESJO("UsesJawTracking", plan.PSetup.OptimizationSetup.UseJawTracking.ToString().ToLower()); planJOList.Add(planUsesJawTracking);

            //    var optiObjectivesList = new List<ESJO>();
            //    var optiParametersList = new List<ESJO>();

            //    var optiXmlPath = "\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\__PlanData__\\_Exports_\\" + plan.Course.Patient.Id.ToString().Replace(" ", "_") + "\\" + plan.Id + "_OptimizationInfo.xml";

            //    XmlTextWriter writer = new XmlTextWriter(optiXmlPath, System.Text.Encoding.UTF8);
            //    writer.Formatting = Formatting.Indented;

            //    writer.WriteStartDocument();

            //    writer.WriteComment("XML File for Optimization Parameters");

            //    writer.WriteStartElement("Optimization");

            //    foreach (var obj in plan.PSetup.OptimizationSetup.Objectives)
            //    {
            //        obj.WriteXml(writer);
            //        structuresWithObjectives.Add(obj.StructureId);
            //    }

            //    foreach (var param in plan.PSetup.OptimizationSetup.Parameters)
            //    {
            //        param.WriteXml(writer);
            //    }
            //    writer.WriteEndElement();
            //    writer.WriteEndDocument();
            //    writer.Flush();
            //    writer.Close();

            //    ESJO planStructuresWithObjectives = CreateESJO("StructuresWithOptimizationObjectives", structuresWithObjectives); planJOList.Add(planStructuresWithObjectives);
            //}
            ////MessageBox.Show("sum - Opti JOs Added");


            // dose statistics
            var doseList = new List<ESJO>();


            //ESJO doseSeries = ESJO.CreateESJO("Series", plan.SeriesList.ToList()); doseList.Add(doseSeries);

            plan.PSetup.DoseValuePresentation = DoseValuePresentation.Absolute;
            ESJO absDoseMax = ESJO.CreateESJO("AbsDoseMax3D", Math.Round(plan.MaxDose, 5)); doseList.Add(absDoseMax);

            ESJO doseMaxLocation_X = ESJO.CreateESJO("DoseMax3DLocation_X", Math.Round(plan.PSum.Dose.DoseMax3DLocation.x, 3)); doseList.Add(doseMaxLocation_X);
            ESJO doseMaxLocation_Y = ESJO.CreateESJO("DoseMax3DLocation_Y", Math.Round(plan.PSum.Dose.DoseMax3DLocation.y, 3)); doseList.Add(doseMaxLocation_Y);
            ESJO doseMaxLocation_Z = ESJO.CreateESJO("DoseMax3DLocation_Z", Math.Round(plan.PSum.Dose.DoseMax3DLocation.z, 3)); doseList.Add(doseMaxLocation_Z);

            //var isodosesList = new List<ESJO>();
            //foreach (var dose in plan.PSetup.Dose.Isodoses)
            //{
            //    var isodoseList = new List<ESJO>();
            //    var meshGeometryList = new List<ESJO>();
            //    var boundsList = new List<ESJO>();
            //    var sizeList = new List<ESJO>();
            //    var locationList = new List<ESJO>();

            //    ESJO color = ESJO.CreateESJO("Color", string.Format("({0},{1},{2})", dose.Color.R, dose.Color.G, dose.Color.B)); isodoseList.Add(color);
            //    ESJO level = ESJO.CreateESJO("Level", dose.Level.ValueAsString); isodoseList.Add(level);

            //    ESJO boundsLocation_X = ESJO.CreateESJO("X", dose.MeshGeometry.Bounds.Location.X); locationList.Add(boundsLocation_X);
            //    ESJO boundsLocation_Y = ESJO.CreateESJO("Y", dose.MeshGeometry.Bounds.Location.Y); locationList.Add(boundsLocation_Y);
            //    ESJO boundsLocation_Z = ESJO.CreateESJO("Z", dose.MeshGeometry.Bounds.Location.Z); locationList.Add(boundsLocation_Z);
            //    ESJO location = ESJO.CreateESJO("Location", locationList, "{", "}"); boundsList.Add(location); // add location obj to bounds obj

            //    ESJO size_X = ESJO.CreateESJO("X", dose.MeshGeometry.Bounds.Size.X); sizeList.Add(size_X);
            //    ESJO size_Y = ESJO.CreateESJO("Y", dose.MeshGeometry.Bounds.Size.Y); sizeList.Add(size_Y);
            //    ESJO size_Z = ESJO.CreateESJO("Z", dose.MeshGeometry.Bounds.Size.Z); sizeList.Add(size_Z);

            //    ESJO size = ESJO.CreateESJO("Size", sizeList, "{", "}"); boundsList.Add(size); // add size obj to bounds obj

            //    ESJO bounds_X = ESJO.CreateESJO("X", dose.MeshGeometry.Bounds.X); boundsList.Add(bounds_X);
            //    ESJO bounds_Y = ESJO.CreateESJO("Y", dose.MeshGeometry.Bounds.Y); boundsList.Add(bounds_Y);
            //    ESJO bounds_Z = ESJO.CreateESJO("Z", dose.MeshGeometry.Bounds.Z); boundsList.Add(bounds_Z);
            //    ESJO boundsSize_X = ESJO.CreateESJO("SizeX", dose.MeshGeometry.Bounds.SizeX); boundsList.Add(boundsSize_X);
            //    ESJO boundsSize_Y = ESJO.CreateESJO("SizeY", dose.MeshGeometry.Bounds.SizeY); boundsList.Add(boundsSize_Y);
            //    ESJO boundsSize_Z = ESJO.CreateESJO("SizeZ", dose.MeshGeometry.Bounds.SizeZ); boundsList.Add(boundsSize_Z);

            //    ESJO bounds = ESJO.CreateESJO("Bounds", boundsList, "{", "}"); meshGeometryList.Add(bounds); // add bounds object to mesh geometry obj

            //    //var positionsList = new List<ESJO>();
            //    //for (var i = 0; i < dose.MeshGeometry.Positions.Count; i++)
            //    //{
            //    //    var coordinates = new List<ESJO>();
            //    //    var p = dose.MeshGeometry.Positions[i];
            //    //    ESJO x = ESJO.CreateESJO("X", p.X); coordinates.Add(x);
            //    //    ESJO y = ESJO.CreateESJO("Y", p.Y); coordinates.Add(y);
            //    //    ESJO z = ESJO.CreateESJO("Z", p.Z); coordinates.Add(z);

            //    //    ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); positionsList.Add(point);
            //    //}
            //    //ESJO postions = ESJO.CreateESJO("Positions", positionsList, "[", "]"); meshGeometryList.Add(postions); // add positions object to mesh geometry obj

            //    //var textureCoordinatesList = new List<ESJO>();
            //    //for (var i = 0; i < dose.MeshGeometry.TextureCoordinates.Count; i++)
            //    //{
            //    //    var coordinates = new List<ESJO>();
            //    //    var tc = dose.MeshGeometry.TextureCoordinates[i];
            //    //    ESJO x = ESJO.CreateESJO("X", tc.X); coordinates.Add(x);
            //    //    ESJO y = ESJO.CreateESJO("Y", tc.Y); coordinates.Add(y);

            //    //    ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); textureCoordinatesList.Add(point);
            //    //}
            //    //ESJO textureCoordinates = ESJO.CreateESJO("TextureCoordinates", textureCoordinatesList, "[", "]"); meshGeometryList.Add(textureCoordinates); // add texture coordinates object to mesh geometry obj

            //    //var normalsList = new List<ESJO>();
            //    //for (var i = 0; i < dose.MeshGeometry.Normals.Count; i++)
            //    //{
            //    //    var coordinates = new List<ESJO>();
            //    //    var n = dose.MeshGeometry.Normals[i];
            //    //    ESJO length = ESJO.CreateESJO("Length", n.Length); coordinates.Add(length);
            //    //    ESJO x = ESJO.CreateESJO("X", n.X); coordinates.Add(x);
            //    //    ESJO y = ESJO.CreateESJO("Y", n.Y); coordinates.Add(y);
            //    //    ESJO z = ESJO.CreateESJO("Z", n.Z); coordinates.Add(z);

            //    //    ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); normalsList.Add(point);
            //    //}
            //    //ESJO normals = ESJO.CreateESJO("Normals", normalsList, "[", "]"); meshGeometryList.Add(textureCoordinates); // add normals object to mesh geometry obj

            //    //var triangleIndicesList = new List<int>();
            //    //for (var i = 0; i < dose.MeshGeometry.TriangleIndices.Count; i++)
            //    //{
            //    //    triangleIndicesList.Add(dose.MeshGeometry.TriangleIndices[i]);
            //    //}
            //    //ESJO triangleIndices = ESJO.CreateESJO("TriangleIndices", triangleIndicesList); meshGeometryList.Add(triangleIndices); // add normals object to mesh geometry obj

            //    ESJO meshGeometry = ESJO.CreateESJO("MeshGeometry", meshGeometryList, "{", "}"); isodoseList.Add(meshGeometry); // add mesh geometry object to main isodose obj

            //    ESJO isodose = ESJO.CreateESJO(isodoseList, "{", "}"); isodosesList.Add(isodose);
            //}
            //ESJO isodoses = ESJO.CreateESJO("Isodoses", isodosesList, "[", "]"); doseList.Add(isodoses);

            ESJO planIsodoseData = ESJO.CreateESJO("DoseMaxStatistics", doseList); planJOList.Add(planIsodoseData);

            //MessageBox.Show("sum - Isodose JOs Added");



            var oarDataObjectList = new List<ESJO>();
            foreach (var structure in sorted_oarList)
            {
              // dvhdatas
              DVHData dvhAA = plan.PSum.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);
              DVHData dvhAR = plan.PSum.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
              //DVHData dvhRR = plan.PSum.GetDVHCumulativeData(structure, DoseValuePresentation.Relative, VolumePresentation.Relative, 0.001);


              // lists
              List<Tuple<double, double>> relVolDvhTuple = new List<Tuple<double, double>>();
              List<Tuple<double, double>> absVolDvhTuple = new List<Tuple<double, double>>();
              //List<Tuple<double, double>> relDoseAndVolDvhTuple = new List<Tuple<double, double>>();
              var joList = new List<ESJO>();

              // esjos
              ESJO id = ESJO.CreateESJO("StructureId", structure.Id.ToString().Replace(" ", "_")); joList.Add(id);
              ESJO dicomType = ESJO.CreateESJO("DicomType", structure.DicomType); joList.Add(dicomType);

              // color
              ESJO color = ESJO.CreateESJO("RGB", string.Format("rgba({0},{1},{2})", structure.Color.R, structure.Color.G, structure.Color.B)); joList.Add(color);
              ESJO colorA = ESJO.CreateESJO("RGBA", string.Format("rgba({0},{1},{2},{3})", structure.Color.R, structure.Color.G, structure.Color.B, structure.Color.A)); joList.Add(colorA);

              // structure centerpoint
              var centerpointList = new List<ESJO>();
              ESJO centerPoint_X = ESJO.CreateESJO("X", Math.Round(structure.CenterPoint.x, 3)); centerpointList.Add(centerPoint_X);
              ESJO centerPoint_Y = ESJO.CreateESJO("Y", Math.Round(structure.CenterPoint.y, 3)); centerpointList.Add(centerPoint_Y);
              ESJO centerPoint_Z = ESJO.CreateESJO("Z", Math.Round(structure.CenterPoint.z, 3)); centerpointList.Add(centerPoint_Z);
              ESJO centerPoint = ESJO.CreateESJO("CenterPoint", centerpointList); joList.Add(centerPoint); // add centerpoint

              var meshGeometryList = new List<ESJO>();
              var boundsList = new List<ESJO>();
              //var sizeList = new List<ESJO>();
              //var locationList = new List<ESJO>();

              //ESJO boundsLocation_X = ESJO.CreateESJO("X", structure.MeshGeometry.Bounds.Location.X); locationList.Add(boundsLocation_X);
              //ESJO boundsLocation_Y = ESJO.CreateESJO("Y", structure.MeshGeometry.Bounds.Location.Y); locationList.Add(boundsLocation_Y);
              //ESJO boundsLocation_Z = ESJO.CreateESJO("Z", structure.MeshGeometry.Bounds.Location.Z); locationList.Add(boundsLocation_Z);
              //ESJO location = ESJO.CreateESJO("Location", locationList, "{", "}"); boundsList.Add(location); // add location obj to bounds obj

              //ESJO size_X = ESJO.CreateESJO("X", structure.MeshGeometry.Bounds.Size.X); sizeList.Add(size_X);
              //ESJO size_Y = ESJO.CreateESJO("Y", structure.MeshGeometry.Bounds.Size.Y); sizeList.Add(size_Y);
              //ESJO size_Z = ESJO.CreateESJO("Z", structure.MeshGeometry.Bounds.Size.Z); sizeList.Add(size_Z);

              //ESJO size = ESJO.CreateESJO("Size", sizeList, "{", "}"); boundsList.Add(size); // add size obj to bounds obj

              ESJO bounds_X = ESJO.CreateESJO("X", structure.MeshGeometry.Bounds.X); boundsList.Add(bounds_X);
              ESJO bounds_Y = ESJO.CreateESJO("Y", structure.MeshGeometry.Bounds.Y); boundsList.Add(bounds_Y);
              ESJO bounds_Z = ESJO.CreateESJO("Z", structure.MeshGeometry.Bounds.Z); boundsList.Add(bounds_Z);
              ESJO boundsSize_X = ESJO.CreateESJO("SizeX", structure.MeshGeometry.Bounds.SizeX); boundsList.Add(boundsSize_X);
              ESJO boundsSize_Y = ESJO.CreateESJO("SizeY", structure.MeshGeometry.Bounds.SizeY); boundsList.Add(boundsSize_Y);
              ESJO boundsSize_Z = ESJO.CreateESJO("SizeZ", structure.MeshGeometry.Bounds.SizeZ); boundsList.Add(boundsSize_Z);

              ESJO bounds = ESJO.CreateESJO("Bounds", boundsList, "{", "}"); meshGeometryList.Add(bounds); // add bounds object to mesh geometry obj

              //var positionsList = new List<ESJO>();
              //for (var i = 0; i < structure.MeshGeometry.Positions.Count; i++)
              //{
              //    var coordinates = new List<ESJO>();
              //    var p = structure.MeshGeometry.Positions[i];
              //    ESJO x = ESJO.CreateESJO("X", p.X); coordinates.Add(x);
              //    ESJO y = ESJO.CreateESJO("Y", p.Y); coordinates.Add(y);
              //    ESJO z = ESJO.CreateESJO("Z", p.Z); coordinates.Add(z);

              //    ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); positionsList.Add(point);
              //}
              //ESJO postions = ESJO.CreateESJO("Positions", positionsList, "[", "]"); meshGeometryList.Add(postions); // add positions object to mesh geometry obj

              //var textureCoordinatesList = new List<ESJO>();
              //for (var i = 0; i < structure.MeshGeometry.TextureCoordinates.Count; i++)
              //{
              //    var coordinates = new List<ESJO>();
              //    var tc = structure.MeshGeometry.TextureCoordinates[i];
              //    ESJO x = ESJO.CreateESJO("X", tc.X); coordinates.Add(x);
              //    ESJO y = ESJO.CreateESJO("Y", tc.Y); coordinates.Add(y);

              //    ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); textureCoordinatesList.Add(point);
              //}
              //ESJO textureCoordinates = ESJO.CreateESJO("TextureCoordinates", textureCoordinatesList, "[", "]"); meshGeometryList.Add(textureCoordinates); // add texture coordinates object to mesh geometry obj

              //var normalsList = new List<ESJO>();
              //for (var i = 0; i < structure.MeshGeometry.Normals.Count; i++)
              //{
              //    var coordinates = new List<ESJO>();
              //    var n = structure.MeshGeometry.Normals[i];
              //    ESJO length = ESJO.CreateESJO("Length", n.Length); coordinates.Add(length);
              //    ESJO x = ESJO.CreateESJO("X", n.X); coordinates.Add(x);
              //    ESJO y = ESJO.CreateESJO("Y", n.Y); coordinates.Add(y);
              //    ESJO z = ESJO.CreateESJO("Z", n.Z); coordinates.Add(z);

              //    ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); normalsList.Add(point);
              //}
              //ESJO normals = ESJO.CreateESJO("Normals", normalsList, "[", "]"); meshGeometryList.Add(textureCoordinates); // add normals object to mesh geometry obj

              //var triangleIndicesList = new List<int>();
              //for (var i = 0; i < structure.MeshGeometry.TriangleIndices.Count; i++)
              //{
              //    triangleIndicesList.Add(structure.MeshGeometry.TriangleIndices[i]);
              //}
              //ESJO triangleIndices = ESJO.CreateESJO("TriangleIndices", triangleIndicesList); meshGeometryList.Add(triangleIndices); // add normals object to mesh geometry obj

              ESJO meshGeometry = ESJO.CreateESJO("MeshGeometry", meshGeometryList, "{", "}"); joList.Add(meshGeometry); // add mesh geometry object to main isodose obj

              ESJO segments = ESJO.CreateESJO("NumberOfSegments", (structure.GetNumberOfSeparateParts().ToString())); joList.Add(segments);
              ESJO isHighResolution = ESJO.CreateESJO("IsHighResolution", structure.IsHighResolution.ToString().ToLower()); joList.Add(isHighResolution);
              ESJO volume = ESJO.CreateESJO("Volume_cc", Math.Round(structure.Volume, 5)); joList.Add(volume);

              // dose stats

              var doseStatsList = new List<ESJO>();

              var sVolume = Math.Round(structure.Volume, 5);

              if (dvhAA.MaxDose.Unit == DoseValue.DoseUnit.Gy)
              {
                ESJO minDose_03 = ESJO.CreateESJO("MinDose_0_03cc_Gy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (sVolume - 0.03)), 5)); doseStatsList.Add(minDose_03);
                ESJO minDose = ESJO.CreateESJO("MinDose_Gy", Math.Round(dvhAA.MinDose.Dose, 5)); doseStatsList.Add(minDose);
                ESJO maxDose_03 = ESJO.CreateESJO("MaxDose_0_03cc_Gy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, 0.03), 5)); doseStatsList.Add(maxDose_03);
                ESJO maxDose = ESJO.CreateESJO("MaxDose_Gy", Math.Round(dvhAA.MaxDose.Dose, 5)); doseStatsList.Add(maxDose);
                ESJO meanDose = ESJO.CreateESJO("MeanDose_Gy", Math.Round(dvhAA.MeanDose.Dose, 5)); doseStatsList.Add(meanDose);
                ESJO medianDose = ESJO.CreateESJO("MedianDose_Gy", Math.Round(dvhAA.MedianDose.Dose, 5)); doseStatsList.Add(medianDose);
                ESJO std = ESJO.CreateESJO("STD", Math.Round(dvhAA.StdDev, 5)); doseStatsList.Add(std);
              }
              else if (dvhAA.MaxDose.Unit == DoseValue.DoseUnit.cGy)
              {
                ESJO minDose_03 = ESJO.CreateESJO("MinDose_0_03cc_cGy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (sVolume - 0.03)), 5)); doseStatsList.Add(minDose_03);
                ESJO minDose = ESJO.CreateESJO("MinDose_cGy", Math.Round(dvhAA.MinDose.Dose, 5)); doseStatsList.Add(minDose);
                ESJO maxDose_03 = ESJO.CreateESJO("MaxDose_0_03cc_cGy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, 0.03), 5)); doseStatsList.Add(maxDose_03);
                ESJO maxDose = ESJO.CreateESJO("MaxDose_cGy", Math.Round(dvhAA.MaxDose.Dose, 5)); doseStatsList.Add(maxDose);
                ESJO meanDose = ESJO.CreateESJO("MeanDose_cGy", Math.Round(dvhAA.MeanDose.Dose, 5)); doseStatsList.Add(meanDose);
                ESJO medianDose = ESJO.CreateESJO("MedianDose_cGy", Math.Round(dvhAA.MedianDose.Dose, 5)); doseStatsList.Add(medianDose);
                ESJO std = ESJO.CreateESJO("STD", Math.Round(dvhAA.StdDev, 5)); doseStatsList.Add(std);
              }

              for (double i = 0; i <= dvhAR.MaxDose.Dose + .1; i += .1)
              {
                relVolDvhTuple.Add(Tuple.Create(Math.Round(i, 2), Math.Round(DvhExtensions.getVolumeAtDose(dvhAR, i), 2)));
              }
              for (double i = 0; i <= dvhAA.MaxDose.Dose + .1; i += .1)
              {
                absVolDvhTuple.Add(Tuple.Create(Math.Round(i, 2), Math.Round(DvhExtensions.getVolumeAtDose(dvhAA, i), 3)));
              }
              //for (double i = 0; i <= dvhRR.MaxDose.Dose + .5; i += .5)
              //{
              //    relDoseAndVolDvhTuple.Add(Tuple.Create(Math.Round(i, 2), Math.Round(DvhExtensions.getVolumeAtDose(dvhRR, i), 3)));
              //}
              ESJO dvh_relVol = ESJO.CreateESJO("DVH_AbsDose_RelVol", relVolDvhTuple); doseStatsList.Add(dvh_relVol);
              ESJO dvh_absVol = ESJO.CreateESJO("DVH_AbsDose_AbsVol", absVolDvhTuple); doseStatsList.Add(dvh_absVol);
              //ESJO dvh_relDoseAndVol = ESJO.CreateESJO("DVH_RelDose_RelVol", relDoseAndVolDvhTuple); doseStatsList.Add(dvh_relDoseAndVol);

              ESJO doseStats = ESJO.CreateESJO("Dose", doseStatsList, "[{", "}]"); joList.Add(doseStats);

              ESJO structureObjects = ESJO.CreateESJO(joList, "{", "}");
              oarDataObjectList.Add(structureObjects);
            }
            // create json object to include all oar data and add it to plan data object list
            var structureData_jo = ESJO.CreateESJO("OarData", oarDataObjectList, "[", "]"); planJOList.Add(structureData_jo);

            //MessageBox.Show("sum - OAR JOs Added");


            var targetDataObjectList = new List<ESJO>();
            foreach (var structure in sorted_targetList)
            {
              // dvhdatas
              DVHData dvhAA = plan.PSum.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, VolumePresentation.AbsoluteCm3, 0.001);
              DVHData dvhAR = plan.PSum.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.001);
              //DVHData dvhRR = plan.PItem.GetDVHCumulativeData(structure, DoseValuePresentation.Relative, VolumePresentation.Relative, 0.001);


              // lists
              List<Tuple<double, double>> relVolDvhTuple = new List<Tuple<double, double>>();
              List<Tuple<double, double>> absVolDvhTuple = new List<Tuple<double, double>>();
              //List<Tuple<double, double>> relDoseAndVolDvhTuple = new List<Tuple<double, double>>();
              var joList = new List<ESJO>();

              // esjos
              ESJO id = ESJO.CreateESJO("TargetId", structure.Id.ToString().Replace(" ", "_")); joList.Add(id);
              ESJO dicomType = ESJO.CreateESJO("DicomType", structure.DicomType); joList.Add(dicomType);

              // color
              ESJO color = ESJO.CreateESJO("RGB", string.Format("rgba({0},{1},{2})", structure.Color.R, structure.Color.G, structure.Color.B)); joList.Add(color);
              ESJO colorA = ESJO.CreateESJO("RGBA", string.Format("rgba({0},{1},{2},{3})", structure.Color.R, structure.Color.G, structure.Color.B, structure.Color.A)); joList.Add(colorA);

              // structure centerpoint
              var centerpointList = new List<ESJO>();
              ESJO centerPoint_X = ESJO.CreateESJO("X", Math.Round(structure.CenterPoint.x, 3)); centerpointList.Add(centerPoint_X);
              ESJO centerPoint_Y = ESJO.CreateESJO("Y", Math.Round(structure.CenterPoint.y, 3)); centerpointList.Add(centerPoint_Y);
              ESJO centerPoint_Z = ESJO.CreateESJO("Z", Math.Round(structure.CenterPoint.z, 3)); centerpointList.Add(centerPoint_Z);
              ESJO centerPoint = ESJO.CreateESJO("CenterPoint", centerpointList); joList.Add(centerPoint); // add centerpoint

              var meshGeometryList = new List<ESJO>();
              var boundsList = new List<ESJO>();
              //var sizeList = new List<ESJO>();
              //var locationList = new List<ESJO>();

              //ESJO boundsLocation_X = ESJO.CreateESJO("X", structure.MeshGeometry.Bounds.Location.X); locationList.Add(boundsLocation_X);
              //ESJO boundsLocation_Y = ESJO.CreateESJO("Y", structure.MeshGeometry.Bounds.Location.Y); locationList.Add(boundsLocation_Y);
              //ESJO boundsLocation_Z = ESJO.CreateESJO("Z", structure.MeshGeometry.Bounds.Location.Z); locationList.Add(boundsLocation_Z);
              //ESJO location = ESJO.CreateESJO("Location", locationList, "{", "}"); boundsList.Add(location); // add location obj to bounds obj

              //ESJO size_X = ESJO.CreateESJO("X", structure.MeshGeometry.Bounds.Size.X); sizeList.Add(size_X);
              //ESJO size_Y = ESJO.CreateESJO("Y", structure.MeshGeometry.Bounds.Size.Y); sizeList.Add(size_Y);
              //ESJO size_Z = ESJO.CreateESJO("Z", structure.MeshGeometry.Bounds.Size.Z); sizeList.Add(size_Z);

              //ESJO size = ESJO.CreateESJO("Size", sizeList, "{", "}"); boundsList.Add(size); // add size obj to bounds obj

              ESJO bounds_X = ESJO.CreateESJO("X", structure.MeshGeometry.Bounds.X); boundsList.Add(bounds_X);
              ESJO bounds_Y = ESJO.CreateESJO("Y", structure.MeshGeometry.Bounds.Y); boundsList.Add(bounds_Y);
              ESJO bounds_Z = ESJO.CreateESJO("Z", structure.MeshGeometry.Bounds.Z); boundsList.Add(bounds_Z);
              ESJO boundsSize_X = ESJO.CreateESJO("SizeX", structure.MeshGeometry.Bounds.SizeX); boundsList.Add(boundsSize_X);
              ESJO boundsSize_Y = ESJO.CreateESJO("SizeY", structure.MeshGeometry.Bounds.SizeY); boundsList.Add(boundsSize_Y);
              ESJO boundsSize_Z = ESJO.CreateESJO("SizeZ", structure.MeshGeometry.Bounds.SizeZ); boundsList.Add(boundsSize_Z);

              ESJO bounds = ESJO.CreateESJO("Bounds", boundsList, "{", "}"); meshGeometryList.Add(bounds); // add bounds object to mesh geometry obj

              //var positionsList = new List<ESJO>();
              //for (var i = 0; i < structure.MeshGeometry.Positions.Count; i++)
              //{
              //    var coordinates = new List<ESJO>();
              //    var p = structure.MeshGeometry.Positions[i];
              //    ESJO x = ESJO.CreateESJO("X", p.X); coordinates.Add(x);
              //    ESJO y = ESJO.CreateESJO("Y", p.Y); coordinates.Add(y);
              //    ESJO z = ESJO.CreateESJO("Z", p.Z); coordinates.Add(z);

              //    ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); positionsList.Add(point);
              //}
              //ESJO postions = ESJO.CreateESJO("Positions", positionsList, "[", "]"); meshGeometryList.Add(postions); // add positions object to mesh geometry obj

              //var textureCoordinatesList = new List<ESJO>();
              //for (var i = 0; i < structure.MeshGeometry.TextureCoordinates.Count; i++)
              //{
              //    var coordinates = new List<ESJO>();
              //    var tc = structure.MeshGeometry.TextureCoordinates[i];
              //    ESJO x = ESJO.CreateESJO("X", tc.X); coordinates.Add(x);
              //    ESJO y = ESJO.CreateESJO("Y", tc.Y); coordinates.Add(y);

              //    ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); textureCoordinatesList.Add(point);
              //}
              //ESJO textureCoordinates = ESJO.CreateESJO("TextureCoordinates", textureCoordinatesList, "[", "]"); meshGeometryList.Add(textureCoordinates); // add texture coordinates object to mesh geometry obj

              //var normalsList = new List<ESJO>();
              //for (var i = 0; i < structure.MeshGeometry.Normals.Count; i++)
              //{
              //    var coordinates = new List<ESJO>();
              //    var n = structure.MeshGeometry.Normals[i];
              //    ESJO length = ESJO.CreateESJO("Length", n.Length); coordinates.Add(length);
              //    ESJO x = ESJO.CreateESJO("X", n.X); coordinates.Add(x);
              //    ESJO y = ESJO.CreateESJO("Y", n.Y); coordinates.Add(y);
              //    ESJO z = ESJO.CreateESJO("Z", n.Z); coordinates.Add(z);

              //    ESJO point = ESJO.CreateESJO(i.ToString(), coordinates, "{", "}"); normalsList.Add(point);
              //}
              //ESJO normals = ESJO.CreateESJO("Normals", normalsList, "[", "]"); meshGeometryList.Add(textureCoordinates); // add normals object to mesh geometry obj

              //var triangleIndicesList = new List<int>();
              //for (var i = 0; i < structure.MeshGeometry.TriangleIndices.Count; i++)
              //{
              //    triangleIndicesList.Add(structure.MeshGeometry.TriangleIndices[i]);
              //}
              //ESJO triangleIndices = ESJO.CreateESJO("TriangleIndices", triangleIndicesList); meshGeometryList.Add(triangleIndices); // add normals object to mesh geometry obj

              ESJO meshGeometry = ESJO.CreateESJO("MeshGeometry", meshGeometryList, "{", "}"); joList.Add(meshGeometry); // add mesh geometry object to main isodose obj

              ESJO segments = ESJO.CreateESJO("NumberOfSegments", (structure.GetNumberOfSeparateParts().ToString())); joList.Add(segments);
              ESJO isHighResolution = ESJO.CreateESJO("IsHighResolution", structure.IsHighResolution.ToString().ToLower()); joList.Add(isHighResolution);
              ESJO volume = ESJO.CreateESJO("Volume_cc", Math.Round(structure.Volume, 5)); joList.Add(volume);

              // dose stats
              var doseStatsList = new List<ESJO>();

              var tVolume = Math.Round(structure.Volume, 5);

              if (dvhAA.MaxDose.Unit == DoseValue.DoseUnit.Gy)
              {
                ESJO d95 = ESJO.CreateESJO("D95pct_Gy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (tVolume * .95)), 5)); doseStatsList.Add(d95);
                ESJO minDose_03 = ESJO.CreateESJO("MinDose_0_03cc_Gy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (tVolume - 0.03)), 5)); doseStatsList.Add(minDose_03);
                ESJO minDose = ESJO.CreateESJO("MinDose_Gy", Math.Round(dvhAA.MinDose.Dose, 5)); doseStatsList.Add(minDose);
                ESJO maxDose_03 = ESJO.CreateESJO("MaxDose_0_03cc_Gy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, 0.03), 5)); doseStatsList.Add(maxDose_03);
                ESJO maxDose = ESJO.CreateESJO("MaxDose_Gy", Math.Round(dvhAA.MaxDose.Dose, 5)); doseStatsList.Add(maxDose);
                ESJO meanDose = ESJO.CreateESJO("MeanDose_Gy", Math.Round(dvhAA.MeanDose.Dose, 5)); doseStatsList.Add(meanDose);
                ESJO medianDose = ESJO.CreateESJO("MedianDose_Gy", Math.Round(dvhAA.MedianDose.Dose, 5)); doseStatsList.Add(medianDose);
                ESJO std = ESJO.CreateESJO("STD", Math.Round(dvhAA.StdDev, 5)); doseStatsList.Add(std);
              }
              else if (dvhAA.MaxDose.Unit == DoseValue.DoseUnit.cGy)
              {
                ESJO d95 = ESJO.CreateESJO("D95pct_cGy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (tVolume * .95)), 5)); doseStatsList.Add(d95);
                ESJO minDose_03 = ESJO.CreateESJO("MinDose_0_03cc_cGy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, (tVolume - 0.03)), 5)); doseStatsList.Add(minDose_03);
                ESJO minDose = ESJO.CreateESJO("MinDose_cGy", Math.Round(dvhAA.MinDose.Dose, 5)); doseStatsList.Add(minDose);
                ESJO maxDose_03 = ESJO.CreateESJO("MaxDose_0_03cc_cGy", Math.Round(DoseChecks.getDoseAtVolume(dvhAA, 0.03), 5)); doseStatsList.Add(maxDose_03);
                ESJO maxDose = ESJO.CreateESJO("MaxDose_cGy", Math.Round(dvhAA.MaxDose.Dose, 5)); doseStatsList.Add(maxDose);
                ESJO meanDose = ESJO.CreateESJO("MeanDose_cGy", Math.Round(dvhAA.MeanDose.Dose, 5)); doseStatsList.Add(meanDose);
                ESJO medianDose = ESJO.CreateESJO("MedianDose_cGy", Math.Round(dvhAA.MedianDose.Dose, 5)); doseStatsList.Add(medianDose);
                ESJO std = ESJO.CreateESJO("STD", Math.Round(dvhAA.StdDev, 5)); doseStatsList.Add(std);
              }

              for (double i = 0; i <= dvhAR.MaxDose.Dose + .1; i += .1)
              {
                relVolDvhTuple.Add(Tuple.Create(Math.Round(i, 2), Math.Round(DvhExtensions.getVolumeAtDose(dvhAR, i), 2)));
              }
              for (double i = 0; i <= dvhAA.MaxDose.Dose + .1; i += .1)
              {
                absVolDvhTuple.Add(Tuple.Create(Math.Round(i, 2), Math.Round(DvhExtensions.getVolumeAtDose(dvhAA, i), 3)));
              }
              //for (double i = 0; i <= dvhRR.MaxDose.Dose + .5; i += .5)
              //{
              //    relDoseAndVolDvhTuple.Add(Tuple.Create(Math.Round(i, 2), Math.Round(DvhExtensions.getVolumeAtDose(dvhRR, i), 3)));
              //}
              ESJO dvh_relVol = ESJO.CreateESJO("DVH_AbsDose_RelVol", relVolDvhTuple); doseStatsList.Add(dvh_relVol);
              ESJO dvh_absVol = ESJO.CreateESJO("DVH_AbsDose_AbsVol", absVolDvhTuple); doseStatsList.Add(dvh_absVol);
              //ESJO dvh_relDoseAndVol = ESJO.CreateESJO("DVH_RelDose_RelVol", relDoseAndVolDvhTuple); doseStatsList.Add(dvh_relDoseAndVol);

              ESJO doseStats = ESJO.CreateESJO("Dose", doseStatsList); joList.Add(doseStats);

              ESJO structureObjects = ESJO.CreateESJO(joList, "{", "}");
              targetDataObjectList.Add(structureObjects);
            }
            // create json object to include all target data and add it to plan data object list
            var targetData_jo = ESJO.CreateESJO("TargetData", targetDataObjectList, "[", "]"); planJOList.Add(targetData_jo);

            //MessageBox.Show("sum - Target JOs Added");


            //var proxStatsObjectList = new List<ESJO>();
            //foreach (var t in sorted_ptvList)
            //{
            //    var targetList = new List<ESJO>();
            //    var tId = t.Id.ToString().Replace(" ", "_").Split(':').First();
            //    var tVolume = Math.Round(t.Volume, 3);
            //    var tColor = "#" + t.Color.ToString().Substring(3, 6);
            //    ESJO targetId = ESJO.CreateESJO("TargetId", tId); targetList.Add(targetId);
            //    ESJO targetColor = ESJO.CreateESJO("TargetHexColor", tColor); targetList.Add(targetColor);
            //    ESJO targetVolume = ESJO.CreateESJO("TargetVolume", tVolume); targetList.Add(targetVolume);

            //    var proxStatsList = new List<ESJO>();
            //    foreach (var s in sorted_oarList)
            //    {
            //        var structureList = new List<ESJO>();
            //        var sId = s.Id.ToString().Replace(" ", "_").Split(':').First();
            //        var sVolume = Math.Round(s.Volume, 3);
            //        var sColor = "#" + s.Color.ToString().Substring(3, 6);
            //        ESJO structureId = ESJO.CreateESJO("StructureId", sId); structureList.Add(structureId);
            //        ESJO overlappingTarget = ESJO.CreateESJO("OverlappingTarget", tId); structureList.Add(overlappingTarget);
            //        ESJO structureColor = ESJO.CreateESJO("StructureHexColor", sColor); structureList.Add(structureColor);
            //        ESJO structureVolume = ESJO.CreateESJO("StructureVolume", sVolume); structureList.Add(structureVolume);

            //        var structureOverlapAbs = Math.Round(CalculateOverlap.VolumeOverlap(t, s), 3);
            //        var structureOverlapPct = structureOverlapAbs == 0 ? 0 : Math.Round(CalculateOverlap.PercentOverlap(s, structureOverlapAbs), 1);
            //        var targetOverlapPct = structureOverlapAbs == 0 ? 0 : Math.Round(CalculateOverlap.PercentOverlap(t, structureOverlapAbs), 1);
            //        var distance = structureOverlapAbs > 0 ? 0 : Math.Round(CalculateOverlap.ShortestDistance(t, s), 1);
            //        ESJO absoluteOverlap = ESJO.CreateESJO("StructureId", structureOverlapAbs); structureList.Add(absoluteOverlap);
            //        ESJO sPercentOverlap = ESJO.CreateESJO("PctStructureOverlap", structureOverlapPct); structureList.Add(sPercentOverlap);
            //        ESJO tPercentOverlap = ESJO.CreateESJO("PctTargetOverlap", targetOverlapPct); structureList.Add(tPercentOverlap);
            //        ESJO distanceFromTarget = ESJO.CreateESJO("DistanceFromTarget", distance); structureList.Add(distanceFromTarget);

            //        ESJO sJO = ESJO.CreateESJO(structureList, "{", "}"); proxStatsList.Add(sJO);
            //    }
            //    ESJO sProxStats = ESJO.CreateESJO("StructureProximity", proxStatsList); targetList.Add(sProxStats);
            //    ESJO tJO = ESJO.CreateESJO(targetList, "{", "}"); proxStatsObjectList.Add(tJO);
            //}
            //ESJO proxStats = ESJO.CreateESJO("ProximityStatistics", proxStatsObjectList); planJOList.Add(proxStats);

            ////MessageBox.Show("sum - Proximity JOs Added");


            ESJO planObjects = ESJO.CreateESJO(planJOList, "{", "}");
            planList.Add(planObjects);
            #endregion if plan sum
          }


        }

        mainEsjo.jsonString += "";
        foreach (var jo in planList)
        {
          mainEsjo.jsonString += jo.JsonString + ",";
        }
        mainEsjo.jsonString = mainEsjo.jsonString.TrimEnd(',');
        mainEsjo.jsonString += "";


        mainEsjo.jsonString = mainEsjo.jsonString.TrimEnd(',');
        mainEsjo.jsonString += "]";

        return mainEsjo;
      }
      catch
      {
        mainEsjo.jsonString = "incomplete";
        return mainEsjo;
      }

    }

    /// <summary>
    /// Creates an object with its value being an array of PlanSums
    /// e.g., for storing a list of the plan sums in a course
    /// </summary>
    /// <param name="inputKey">Key</param>
    /// <param name="value">List of PlanSums</param>
    /// <returns></returns>
    public static ESJO CreateESJO(string inputKey, List<PlanSum> value)
    {
      ESJO esjo = new ESJO();
      esjo.key = inputKey;
      esjo.pSumListValue = value;
      esjo.jsonString = "\"" + esjo.key + "\":[";
      foreach (var psum in esjo.pSumListValue)
      {
        var joList = new List<ESJO>();
        ESJO sumId = ESJO.CreateESJO("Id", psum.Id.ToString().Replace(" ", "_"));
        //esjo.jsonString += string.Format("{0},", psum.Id.ToString());
      }
      esjo.jsonString = esjo.jsonString.TrimEnd(',');
      esjo.jsonString += "]";

      return esjo;
    }

    /// <summary>
    /// Creates an object with its value being an array of doubles
    /// e.g., for storing a structure's DVH as an array
    /// </summary>
    /// <param name="inputKey">ESJOs Key</param>
    /// <param name="value">ESJOs Value</param>
    /// <returns></returns>
    public static ESJO CreateESJO(string inputKey, List<Tuple<double, double>> value)
    {
      ESJO esjo = new ESJO();
      esjo.key = inputKey;
      esjo.tupLstValue = value;
      esjo.jsonString = "\"" + esjo.key + "\":[";
      foreach (var tuple in esjo.tupLstValue)
      {
        esjo.jsonString += string.Format("[{0}, {1}],", tuple.Item1, tuple.Item2);
      }
      esjo.jsonString = esjo.jsonString.TrimEnd(',');
      esjo.jsonString += "]";

      return esjo;
    }

    /// <summary>
    /// Creates an object with its value being an array of doubles
    /// e.g., for storing a structure's DVH as an array
    /// </summary>
    /// <param name="inputKey">ESJOs Key</param>
    /// <param name="value">ESJOs Value</param>
    /// <returns></returns>
    public static ESJO CreateESJO(string inputKey, List<Tuple<string, Tuple<int, double>>> value)
    {
      ESJO esjo = new ESJO();
      esjo.key = inputKey;
      esjo.sumFractionationSchedule = value;
      esjo.jsonString = "\"" + esjo.key + "\":[";
      foreach (var fxSchedule in esjo.sumFractionationSchedule)
      {
        esjo.jsonString += string.Format("{{ \"{0}\": {{ \"NumberOfFractions\" : {1}, \"DosePerFraction\": {2} }} }},", fxSchedule.Item1, fxSchedule.Item2.Item1, fxSchedule.Item2.Item2);
      }
      esjo.jsonString = esjo.jsonString.TrimEnd(',');
      esjo.jsonString += "]";

      return esjo;
    }

    /// <summary>
    /// Creates an object with its value being an array of other ESJOs - with pre-determined brackets [{}]
    /// e.g., for storing nested object layers
    /// </summary>
    /// <param name="inputKey">ESJOs Key</param>
    /// <param name="value">ESJOs Value</param>
    /// <returns></returns>
    public static ESJO CreateESJO(string inputKey, List<ESJO> value)
    {
      ESJO esjo = new ESJO();
      esjo.key = inputKey;
      esjo.jsonObjectsList = value;
      esjo.jsonString = "\"" + esjo.key + "\":[{";
      foreach (var jo in esjo.jsonObjectsList)
      {
        esjo.jsonString += jo.jsonString + ",";
      }
      esjo.jsonString = esjo.jsonString.TrimEnd(',');
      esjo.jsonString += "}]";

      return esjo;
    }

    /// <summary>
    /// Creates an object with its value being an array of other ESJOs - customizable brackets
    /// e.g., for storing nested object layers
    /// </summary>
    /// <param name="inputKey">ESJOs Key</param>
    /// <param name="value">ESJOs Value</param>
    /// <param name="openingBracket">Opening Bracket</param>
    /// <param name="closingBracket">Closing Bracket</param>
    /// <returns></returns>
    public static ESJO CreateESJO(string inputKey, List<ESJO> value, string openingBracket, string closingBracket)
    {
      ESJO esjo = new ESJO();
      esjo.key = inputKey;
      esjo.jsonObjectsList = value;
      esjo.jsonString = "\"" + esjo.key + "\":" + openingBracket;
      foreach (var jo in esjo.jsonObjectsList)
      {
        esjo.jsonString += jo.jsonString + ",";
      }
      esjo.jsonString = esjo.jsonString.TrimEnd(',');
      esjo.jsonString += closingBracket;

      return esjo;
    }

    /// <summary>
    /// Creates an ESJO made of other ESJOs - customizable brackets
    /// for nesting either a dictionary object or array object
    /// does not require a key
    /// </summary>
    /// <param name="listOfJsonObjects">List containing ESJOs</param>
    /// <param name="openingBracket">Opening Bracket</param>
    /// <param name="closingBracket">Closing Bracket</param>
    /// <returns></returns>
    public static ESJO CreateESJO(List<ESJO> listOfJsonObjects, string openingBracket, string closingBracket)
    {
      ESJO esjo = new ESJO();
      esjo.jsonString = openingBracket;
      esjo.jsonObjectsList = listOfJsonObjects;
      foreach (var jo in esjo.jsonObjectsList)
      {
        esjo.jsonString += jo.jsonString + ",";
      }
      esjo.jsonString = esjo.jsonString.TrimEnd(',');
      esjo.jsonString += closingBracket;

      return esjo;
    }

    /// <summary>
    /// Used to create an JSON object string that contains a check's title, result message, and pass message
    /// </summary>
    /// <param name="planCkeckCompleted">Title of the plan being completed</param>
    /// <param name="resultMessage">plan check result message</param>
    /// <param name="passMessage">plan check pass message</param>
    /// <param name="openingBracket">opening bracket for the JSON object string</param>
    /// <param name="closingBracket">closing bracket for the JSON object string</param>
    /// <returns></returns>
    public static ESJO CreateESJO(string planCkeckCompleted, string resultMessage, string passMessage, string openingBracket, string closingBracket)
    {
      ESJO esjo = new ESJO();
      esjo.jsonString = openingBracket;
      esjo.jsonString += "\"PlanCkeckCompleted\": \"" + CleanString.clean(planCkeckCompleted) + "\",";
      esjo.jsonString += "\"Result\": \"" + CleanString.clean(resultMessage) + "\",";
      esjo.jsonString += "\"Pass\": \"" + CleanString.clean(passMessage) + "\"";
      esjo.jsonString += closingBracket;

      return esjo;
    }

    #endregion object methods

    #region IEnumerator and IEnumerable Requirements
    /// <summary>
    /// position variable
    /// IEnumerator and IEnumerable require these methods.
    /// </summary>
    int position = -1;
    /// <summary>
    /// IEnumerator and IEnumerable require these methods.
    /// </summary>
    /// <returns></returns>
    public IEnumerator GetEnumerator()
    {
      return this;
    }

    /// <summary>
    /// IEnumerator and IEnumerable require these methods.
    /// IEnumerator
    /// </summary>
    /// <returns></returns>
    public bool MoveNext()
    {
      position++;
      return (position < JsonObjectsList.Count);
    }

    /// <summary>
    /// IEnumerator and IEnumerable require these methods.
    /// IEnumerable
    /// </summary>
    public void Reset()
    { position = 0; }

    /// <summary>
    /// IEnumerator and IEnumerable require these methods.
    /// IEnumerable
    /// </summary>
    public object Current
    {
      get { return JsonObjectsList[position]; }
    }
    #endregion IEnumerator and IEnumerable Requirements
  }
}
