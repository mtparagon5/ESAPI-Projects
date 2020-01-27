
namespace VMS.TPS
{
  using System;
  using System.Linq;
  using System.Text;
  using System.Windows;
  using System.Collections.Generic;
  using VMS.TPS.Common.Model.API;
  using VMS.TPS.Common.Model.Types;
  using System.IO;
  using System.Reflection;
  using System.Xml;
  using System.Windows.Controls;
  using System.Text.RegularExpressions;
  using Microsoft.Office.Interop.Word;
  using Word = Microsoft.Office.Interop.Word;

  public class WordExtensions
  {
    static Microsoft.Office.Interop.Word.WdColor colorPass = (Microsoft.Office.Interop.Word.WdColor)(243 + 0x100 * 248 + 0x10000 * 233);
    static Microsoft.Office.Interop.Word.WdColor colorWarning = (Microsoft.Office.Interop.Word.WdColor)(170 + 0x100 * 170 + 0x10000 * 120);
    static Microsoft.Office.Interop.Word.WdColor colorError = (Microsoft.Office.Interop.Word.WdColor)(180 + 0x100 * 215 + 0x10000 * 230);
    static Microsoft.Office.Interop.Word.WdColor colorCantUnderstand = (Microsoft.Office.Interop.Word.WdColor)(240 + 0x100 * 244 + 0x10000 * 255);
    //    static Microsoft.Office.Interop.Word.WdColor colorNotApplicable = (Microsoft.Office.Interop.Word.WdColor)(200 + 0x100 * 199 + 0x10000 * 199);

    public static bool IsFormFieldChecked(Cell tableCell)
    {
      // There should only be one form field in a cell and it should be a check box.
      if (tableCell.Range.FormFields.Count != 1)
      {
        return false;
      }

      object fieldIndex = 1;
      FormField formField = tableCell.Range.FormFields.get_Item(ref fieldIndex);

      // Ensure the form field is a check box.
      if (formField.Type != WdFieldType.wdFieldFormCheckBox)
      {
        return false;
      }

      // Return the value of the check box.
      return formField.CheckBox.Value;
    }

    public static int GetFormFieldIndex(Cell tableCell)
    {
      // There should only be one form field in a cell and it should be a check box.
      if (tableCell.Range.FormFields.Count != 1)
      {
        return -1;
      }

      object fieldIndex = 1;
      FormField formField = tableCell.Range.FormFields.get_Item(ref fieldIndex);

      // Ensure the form field is a check box.
      if (formField.Type != WdFieldType.wdFieldFormDropDown)
      {
        return -1;
      }

      // Return the value of the check box.
      return formField.DropDown.Value;
    }

    public static void MarkCellNoPrint(double prescriptionDose, double planDose, Cell tableCell, char comparisonSign)
    {

      if (comparisonSign == '<')
      {
        if (planDose > 1.1 * prescriptionDose) tableCell.Shading.BackgroundPatternColor = colorError;
        else if (planDose > prescriptionDose) tableCell.Shading.BackgroundPatternColor = colorWarning;
        else tableCell.Shading.BackgroundPatternColor = colorPass;
      }

      else if (comparisonSign == '>')
      {
        if (planDose < 0.9 * prescriptionDose) tableCell.Shading.BackgroundPatternColor = colorError;
        else if (planDose < prescriptionDose) tableCell.Shading.BackgroundPatternColor = colorWarning;
        else tableCell.Shading.BackgroundPatternColor = colorPass;
      }

      else if (comparisonSign == '~')
      {
        if (planDose < 0.9 * prescriptionDose) tableCell.Shading.BackgroundPatternColor = colorWarning;
        else if (planDose > 1.1 * prescriptionDose) tableCell.Shading.BackgroundPatternColor = colorWarning;
        else tableCell.Shading.BackgroundPatternColor = colorPass;
      }

      else if (comparisonSign == '=')
      {
        if (planDose == prescriptionDose) tableCell.Shading.BackgroundPatternColor = colorPass;
        else tableCell.Shading.BackgroundPatternColor = colorError;
      }

      else
        tableCell.Shading.BackgroundPatternColor = colorPass;

    }

    public static void MarkCell(double prescriptionDose, double planDose, Cell tableCell, char comparisonSign)
    {
      string temp = tableCell.Range.Text;
      tableCell.Range.Text = temp + "  (" + string.Format("{0:0.0}", planDose) + ")";

      MarkCellNoPrint(prescriptionDose, planDose, tableCell, comparisonSign);
    }

    public static void MarkCellForPTVEvaluation(double prescriptionDosePercent, double prescriptionDose, double planDose, Cell tableCell, char comparisonSign)
    {

      tableCell.Range.Text = string.Format("{0:0.0}", planDose);

      MarkCellNoPrint(prescriptionDosePercent * prescriptionDose, planDose, tableCell, comparisonSign);

      //if wrong, print why
      if (tableCell.Shading.BackgroundPatternColor != colorPass)
      {
        tableCell.Range.Text = string.Format("{0:0.0}", planDose) + "  (" + string.Format("{0:0.0}", (100.0 * planDose) / prescriptionDose) + "%)";
      }

    }

    public static void MarkCell(double prescriptionDoseMin, double prescriptionDoseMax, double planDose, Cell tableCell)
    {

      string temp = string.Format("{0:0.00}", prescriptionDoseMin);
      temp = temp + "  (" + string.Format("{0:0.00}", planDose) + ") ";
      temp = temp + string.Format("{0:0.00}", prescriptionDoseMax);

      tableCell.Range.Text = temp;

      if (planDose > 1.1 * prescriptionDoseMax) tableCell.Shading.BackgroundPatternColor = colorError;
      if (planDose < 1.1 * prescriptionDoseMin) tableCell.Shading.BackgroundPatternColor = colorError;
      if ((planDose < 1.1 * prescriptionDoseMax) && (planDose > prescriptionDoseMax)) tableCell.Shading.BackgroundPatternColor = colorWarning;
      if ((planDose > 0.9 * prescriptionDoseMin) && (planDose < prescriptionDoseMax)) tableCell.Shading.BackgroundPatternColor = colorWarning;
      if ((planDose > prescriptionDoseMin) && (planDose < prescriptionDoseMax)) tableCell.Shading.BackgroundPatternColor = colorPass;

    }

    public static string GetCellTextValue(string input)
    {
      return input.Substring(0, input.Length - 1);
    }

    public static double GetCellDoubleValue(string input)
    {
      string resultString = Regex.Match(input, @"[-+]?([0-9]*\.[0-9]+|[0-9]+)").Value;
      double asValue = Convert.ToDouble(resultString);
      return asValue;
    }

    public static void AddColumn(Table table, string structureText, string constraintType, string constraintValue)
    {

      //Row lastRow = table.Rows.Add(System.Reflection.Missing.Value); -- this causes error for some reason
      Row lastRow = table.Rows.Add(Missing.Value);    // but this works some how

      table.Cell(lastRow.Index, 1).Range.Text = structureText;
      table.Cell(lastRow.Index, 2).Range.Text = constraintType;
      table.Cell(lastRow.Index, 3).Range.Text = constraintValue;
    }

    public static void AddSectionHeader(Table table, string headerText)
    {
      table.Cell(1, 1).Range.Text = headerText;
    }

    public static void AddStandardConstraints(Table constraintsTable, PlanSetup selectedPlan)
    {

      AddSectionHeader(constraintsTable, "STANDARD CONSTRAINTS");


      Structure testStructure = PlanExtensions.GetStructure("Bladder", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "V65Gy", "50%");
        AddColumn(constraintsTable, testStructure.Id, "V70Gy", "35%");
        AddColumn(constraintsTable, testStructure.Id, "V75Gy", "25%");
        AddColumn(constraintsTable, testStructure.Id, "V80Gy", "15%");
      }


      //Urmatoarele 3 nu sint oficial in QUANTEC : 
      testStructure = PlanExtensions.GetStructure("Brachial Plexus LT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 66 Gy");
        AddColumn(constraintsTable, testStructure.Id, "60", "5%");
      }

      testStructure = PlanExtensions.GetStructure("Brachial Plexus RT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 66 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V60cc", "5%");
      }

      testStructure = PlanExtensions.GetStructure("Brainstem", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 54 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 59 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V60cc", "<1%");
      }

      testStructure = PlanExtensions.GetStructure("Optic Chiasm", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 54 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V60cc", "<1%");
      }

      testStructure = PlanExtensions.GetStructure("Cochlea LT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "mean", "< 45 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V55cc", "<5%");
      }

      testStructure = PlanExtensions.GetStructure("Cochlea RT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "mean", "< 45 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V55cc", "<5%");
      }

      testStructure = PlanExtensions.GetStructure("Brainstem", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 54 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 59 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V60cc", "<1%");
      }

      testStructure = PlanExtensions.GetStructure("Constrictor", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "mean", "< 54 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V52cc", "<60%");
        AddColumn(constraintsTable, testStructure.Id, "V50cc", "<51%");
      }

      testStructure = PlanExtensions.GetStructure("Duodenum", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 60 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V45cc", "< 33%");
      }

      testStructure = PlanExtensions.GetStructure("Ear", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "mean", "< 50 Gy");
      }

      testStructure = PlanExtensions.GetStructure("Esophagus", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "mean", "< 35 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V35%", "< 50 %");
        AddColumn(constraintsTable, testStructure.Id, "V50%", "< 40 %");
        AddColumn(constraintsTable, testStructure.Id, "V70%", "< 20 %");
      }

      testStructure = PlanExtensions.GetStructure("Globe LT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 50 Gy");
        AddColumn(constraintsTable, testStructure.Id, "mean", "< 35 Gy");
      }

      testStructure = PlanExtensions.GetStructure("Globe RT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 50 Gy");
        AddColumn(constraintsTable, testStructure.Id, "mean", "< 35 Gy");
      }

      testStructure = PlanExtensions.GetStructure("Femoral Head LT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 50 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V45cc", "< 25 %");
        AddColumn(constraintsTable, testStructure.Id, "V40cc", "< 40 %");
      }

      testStructure = PlanExtensions.GetStructure("Femoral Head RT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 50 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V45cc", "< 25 %");
        AddColumn(constraintsTable, testStructure.Id, "V40cc", "< 40 %");
      }

      testStructure = PlanExtensions.GetStructure("Larynx", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 66 Gy");
        AddColumn(constraintsTable, testStructure.Id, "mean", "< 44 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V47cc", "< 60 %");
        AddColumn(constraintsTable, testStructure.Id, "V50cc", "< 21 %");
      }

      testStructure = PlanExtensions.GetStructure("Heart", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "mean", "< 26 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V25cc", "< 10 %");
        AddColumn(constraintsTable, testStructure.Id, "V45cc", "< 67 %");
        AddColumn(constraintsTable, testStructure.Id, "V60cc", "< 33 %");
      }

      testStructure = PlanExtensions.GetStructure("Kidney LT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 23 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V33%", "< 50 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V67%", "< 20 Gy");
      }

      testStructure = PlanExtensions.GetStructure("Kidney RT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 23 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V33%", "< 50 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V67%", "< 20 Gy");
      }

      testStructure = PlanExtensions.GetStructure("Lens LT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 25 Gy");
        AddColumn(constraintsTable, testStructure.Id, "mean", "< 10 Gy");
      }

      testStructure = PlanExtensions.GetStructure("Lens RT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 25 Gy");
        AddColumn(constraintsTable, testStructure.Id, "mean", "< 10 Gy");
      }


      testStructure = PlanExtensions.GetStructure("Liver", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 30 Gy"); //asta este ciudata rau aci !!!
        AddColumn(constraintsTable, testStructure.Id, "V50%", "< 35 Gy");
      }


      testStructure = PlanExtensions.GetStructure("Lung", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "mean", "< 20 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V20Gy", "37%");
      }

      testStructure = PlanExtensions.GetStructure("Mandible", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 70 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V1cc", "< 75 Gy");
      }

      testStructure = PlanExtensions.GetStructure("Optic Nerve LT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 54 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V1%", "< 60 Gy");
      }

      testStructure = PlanExtensions.GetStructure("Optic Nerve RT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 54 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V1%", "< 60 Gy");
      }

      testStructure = PlanExtensions.GetStructure("Oral Cavity", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "mean", "40Gy");
      }

      testStructure = PlanExtensions.GetStructure("Parotid LT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "V20Gy", "< 20 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V50%", "< 30 Gy");
        AddColumn(constraintsTable, testStructure.Id, "mean", "< 26 Gy");
      }

      testStructure = PlanExtensions.GetStructure("Parotid RT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "V20Gy", "< 20 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V50%", "< 30 Gy");
        AddColumn(constraintsTable, testStructure.Id, "mean", "< 26 Gy");
      }

      testStructure = PlanExtensions.GetStructure("Pharynx", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "mean", "50Gy");
      }

      testStructure = PlanExtensions.GetStructure("Penile Bulb", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "mean", "< 52.5 Gy");
      }

      //HELP: verifica astea
      testStructure = PlanExtensions.GetStructure("Rectum", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "V60Gy", "50%");
        AddColumn(constraintsTable, testStructure.Id, "V65Gy", "35%");
        AddColumn(constraintsTable, testStructure.Id, "V70Gy", "25%");
        AddColumn(constraintsTable, testStructure.Id, "V75Gy", "15%");
      }

      testStructure = PlanExtensions.GetStructure("Small Bowel", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 50 Gy");
        //HELPAddColumn(constraintsTable, testStructure.Id, "V15", "120cc");
        //HELPAddColumn(constraintsTable, testStructure.Id, "V45", "195cc");
      }

      testStructure = PlanExtensions.GetStructure("Spinal Cord", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 45 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V0.03cc", " < 48 Gy");
      }

      testStructure = PlanExtensions.GetStructure("Stomach", selectedPlan);
      if (testStructure != null)
      {
        //help AddColumn(constraintsTable, testStructure.Id, "D100", "45Gy");
        AddColumn(constraintsTable, testStructure.Id, "max", "< 54 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V2%", "< 50 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V25%", "< 45 Gy");
      }

      testStructure = PlanExtensions.GetStructure("Submandibular LT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 39 Gy");
      }

      testStructure = PlanExtensions.GetStructure("Submandibular RT", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 39 Gy");
      }

      testStructure = PlanExtensions.GetStructure("Temporal", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 60 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V1%", "< 65 Gy");
      }

      testStructure = PlanExtensions.GetStructure("Tongue", selectedPlan);
      if (testStructure != null)
      {
        AddColumn(constraintsTable, testStructure.Id, "max", "< 55 Gy");
        AddColumn(constraintsTable, testStructure.Id, "V1%", "< 65 Gy");
      }
    }

    public static void AddSBRTOneFraction(Table constraintsTable, PlanSetup selectedPlan)
    {

      AddSectionHeader(constraintsTable, "SBRT 1 FRACTION");


      if (selectedPlan.NumberOfFractions <= 5) //SBRT fractionation
      {


        Structure testStructure = PlanExtensions.GetStructure("Bladder", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 18.4 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V15cc", "< 11.4 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Brachial plexus", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", " < 17.5 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V3cc", " < 14.0 Gy");
        }


        testStructure = PlanExtensions.GetStructure("Brain ", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 10 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Brainstem", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 15 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V0.5cc", "< 10 Gy");
        }


        testStructure = PlanExtensions.GetStructure("Bronchus", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 13.3 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V0.5cc", "< 12.4 Gy");

        }

        testStructure = PlanExtensions.GetStructure("Cauda", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 16 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V5cc", "< 14 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Cochlea", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 9 Gy");
        }


        testStructure = PlanExtensions.GetStructure("Colon", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 18.4 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V20cc", "< 14.3 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Duodenum", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 12.4 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V5cc", "< 11.2 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 9.0 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Esophagus", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 15.4 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V5cc", "< 11.9 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Femoral", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 14.0 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Great vessels", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 37 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 31 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Heart", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 22 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V15cc", "< 16 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Jejunum", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 15.4 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V5cc", "< 11.9 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Liver", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "V700cc", "< 9.1 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Lungs", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "V1000cc", "< 7.4 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V1500cc", "< 7.0 Gy");
        }


        testStructure = PlanExtensions.GetStructure("Optic Nerve LT", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 8 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V0.2cc", "< 8 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Optic Nerve RT", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 8 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V0.2cc", "< 8 Gy");
        }


        testStructure = PlanExtensions.GetStructure("Penile Bulb", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 34 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V3cc", "< 14 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Rectum", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 18.4 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V20cc", "< 14.3 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Kidney LT", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "V200cc", "< 8.4 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Kidney RT", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "V200cc", "< 8.4 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Rib", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 30 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V1cc", "< 22 Gy");
        }


        testStructure = PlanExtensions.GetStructure("Plexus", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 16 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V5cc", "< 14.4 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Skin", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 26 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 23 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Spinal Cord", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 14 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V10%", "< 10 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V0.35cc", "< 10 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V1.2cc", "< 7 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Stomach", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 12.4 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 11.2 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Trachea", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 20.2 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V4cc", "< 10.5 Gy");
        }
      }

    }

    public static void AddSBRTThreeFraction(Table constraintsTable, PlanSetup selectedPlan)
    {

      AddSectionHeader(constraintsTable, "SBRT 3 FRACTIONS");

      if (selectedPlan.NumberOfFractions <= 5) //SBRT fractionation
      {


        Structure testStructure = PlanExtensions.GetStructure("Bladder", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 28.2 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V15cc", "< 16.8 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Brachial plexus", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", " < 24 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V3cc", " < 20.4 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Brainstem", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 23.1 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V0.5cc", "< 18 Gy");
        }


        testStructure = PlanExtensions.GetStructure("Bronchus", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 23.1 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V0.5cc", "< 18.9 Gy");

        }

        testStructure = PlanExtensions.GetStructure("Cauda", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 24 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V5cc", "< 21.9 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Cochlea", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 17.1 Gy");
        }


        testStructure = PlanExtensions.GetStructure("Colon", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 28.2 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V20cc", "< 24 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Duodenum", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 22.2 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V5cc", "< 16.5 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 11.4 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Esophagus", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 25.2 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V5cc", "< 17.7 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Femoral", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 21.9 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Great vessels", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 45 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 39 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Heart", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 30 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V15cc", "< 24 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Jejunum", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 25.2 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V5cc", "< 17.7 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Liver", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "V700cc", "< 19.2 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Lungs", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "V1000cc", "< 12.4 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V1500cc", "< 11.6 Gy");
        }


        testStructure = PlanExtensions.GetStructure("Optic Nerve LT", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 17.4 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V0.2cc", "< 15.3 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Optic Nerve RT", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 17.4 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V0.2cc", "< 15.3 Gy");
        }


        testStructure = PlanExtensions.GetStructure("Penile Bulb", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, "Penile Bulb", "max", "< 42 Gy");
          AddColumn(constraintsTable, "Penile Bulb", "V3cc", "< 21.9 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Rectum", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 28.2 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V20cc", "< 24 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Kidney", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, "Kidneys", "V200cc", "< 16 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Rib", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 30 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V1cc", "< 28.8 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V30cc", "< 30 Gy");
        }


        testStructure = PlanExtensions.GetStructure("Plexus", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 24 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V5cc", "< 22.5 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Skin", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 33 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 30 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Spinal Cord", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 21.9 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V10%", "< 18 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V0.35cc", "< 18 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V1.2cc", "< 12.3 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Stomach", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 22.2 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 16.5 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Trachea", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 30 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V4cc", "< 15 Gy");
        }
      }

    }

    public static void AddSBRTFiveFraction(Table constraintsTable, PlanSetup selectedPlan)
    {

      AddSectionHeader(constraintsTable, "SBRT 5 FRACTIONS");

      if (selectedPlan.NumberOfFractions <= 5) //SBRT fractionation
      {


        Structure testStructure = PlanExtensions.GetStructure("Bladder", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 38 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V15cc", "< 18.3 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Brachial plexus", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", " < 30.5 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V3cc", " < 27 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Brainstem", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 31 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V0.5cc", "< 23 Gy");
        }


        testStructure = PlanExtensions.GetStructure("Bronchus", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 33 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V0.5cc", "< 21 Gy");

        }

        testStructure = PlanExtensions.GetStructure("Cauda", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 32 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V5cc", "< 30 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Cochlea", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 25 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Colon", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 38 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V20cc", "< 25 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Duodenum", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 32 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V5cc", "< 18 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 12.5 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Esophagus", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 35 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V5cc", "< 19.5 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Femor LT", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 30 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Femor RT", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 30 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Great vessels", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 53 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 47 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Heart", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 38 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V15cc", "< 32 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Jejunum", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 35 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V5cc", "< 19.5 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Liver", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "V700cc", "< 21 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Lungs", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "V1000cc", "< 13.5 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V1500cc", "< 12.5 Gy");
        }


        testStructure = PlanExtensions.GetStructure("Optic Nerve LT", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 25 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V0.2cc", "< 23 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Optic Nerve RT", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 25 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V0.2cc", "< 23 Gy");
        }


        testStructure = PlanExtensions.GetStructure("Penile Bulb", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 50 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V3cc", "< 30 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Rectum", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 38 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V20cc", "< 25 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Kidney LT", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "V200cc", "< 17.5 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Kidney RT", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "V200cc", "< 17.5 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Rib", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 43 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V1cc", "< 35 Gy");
        }


        testStructure = PlanExtensions.GetStructure("Plexus", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 32 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V5cc", "< 30 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Skin", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 36.5 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 39.5 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Spinal Cord", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 30 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V10%", "< 23 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V0.35cc", "< 23 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V1.2cc", "< 14.5 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Stomach", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 32 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V10cc", "< 18 Gy");
        }

        testStructure = PlanExtensions.GetStructure("Trachea", selectedPlan);
        if (testStructure != null)
        {
          AddColumn(constraintsTable, testStructure.Id, "max", "< 40 Gy");
          AddColumn(constraintsTable, testStructure.Id, "V4cc", "< 16.5 Gy");
        }
      }

    }

    public static void AddLungSBRTConstraints(Table constraintsTable, PlanSetup selectedPlan)
    {

      AddSectionHeader(constraintsTable, "LUNG SBRT CONSTRAINTS");

      AddColumn(constraintsTable, "CI", "", "<1.2"); //? HELP ??? 
      AddColumn(constraintsTable, "R50%", "", "<5.11");
      AddColumn(constraintsTable, "Lung", "mean", "<50%");
      AddColumn(constraintsTable, "Lung", "V10Gy", "<5%");
      AddColumn(constraintsTable, "Lung", "V20Gy", "<10%"); //HELP ?
      AddColumn(constraintsTable, "Spinal Cord", "max", "<30Gy");
      AddColumn(constraintsTable, "Spinal Cord PRV_5mm", "max", "<30Gy");
      AddColumn(constraintsTable, "Skin", "max", "<40Gy");
      AddColumn(constraintsTable, "Esophagus", "max", "<32.5Gy");
      AddColumn(constraintsTable, "Heart", "max", "<35Gy");
    }
  }

}
