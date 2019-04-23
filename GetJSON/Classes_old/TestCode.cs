using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataExtraction.Classes
{
    class TestCode
    {
        //foreach (StructureStats item in StructureInfo_DG.Items)
        //{
        //    if (SelectedStructure.Id.ToString().Split(':').First() == item.id)
        //    {
        //        item.color = SelectedStructure.Color.ToString();
        //        StructureInfo_DG.Items.Add(item.color);
        //    }
        //}


        //foreach  (VolumeDoseConstraint cont in mainControl.Constraints_DG.Items)
        //{
        //    double vOverlap = 0;
        //    Structure targetWithMostOverlap = null;
        //    int counter = 0;
        //    if (cont.Result == "NOT MET")
        //        ++counter;
        //    if (counter > 0)
        //    {
        //        double tempOverlap = 0;
        //        foreach (Structure ptv in ptvList)
        //        {
        //            tempOverlap = CalculateOverlap.VolumeOverlap(ptv, cont.Structure);
        //            if (tempOverlap > vOverlap) { vOverlap = tempOverlap; targetWithMostOverlap = ptv; }
        //        }
        //        double percentOverlap = CalculateOverlap.PercentOverlap(cont.Structure, vOverlap);
        //        cont.Overlap = string.Format("{0}: {1:F3}cc ({2:F1}%)", targetWithMostOverlap.ToString().Split(':').First(), vOverlap, percentOverlap);
        //    }
        //}
        //public VolumeDoseConstraint(Structure structure, string id, IEnumerable<Structure> ptvList, DVHData dvh, double volumeLimit, double doseLimit, string volumeLabel)
        //{
        //    Structure = structure;
        //    Id = id;
        //    double volumeAtDose = Math.Round(DoseChecks.getVolumeAtDose(dvh, doseLimit), 3);
        //    DoseValue = string.Format("V{0:F1} Gy = {1:F3}", doseLimit, volumeAtDose);
        //    Limit = string.Format("{0:F1} {1}", volumeLimit, volumeLabel);
        //    Result = volumeAtDose <= volumeLimit ? "MET" : "NOT MET";
        //    Overlap = "";
        //    //double vOverlap = 0;
        //    //Structure targetWithMostOverlap = null;
        //    //if (volumeAtDose > volumeLimit)
        //    //{
        //    //    double tempOverlap = 0;
        //    //    foreach (Structure ptv in ptvList)
        //    //    {
        //    //        tempOverlap = CalculateOverlap.VolumeOverlap(ptv, structure);
        //    //        if (tempOverlap > vOverlap) { vOverlap = tempOverlap; targetWithMostOverlap = ptv; }
        //    //    }
        //    //    double percentOverlap = CalculateOverlap.PercentOverlap(structure, vOverlap);
        //    //    Overlap = string.Format("{0}: {1:F3}cc ({2:F1}%)", targetWithMostOverlap, vOverlap, percentOverlap);
        //    //}

        //}

        //<DataGridTextColumn Header = "Overlap" Binding="{Binding Overlap}" Width=".8*" IsReadOnly="True">
        //    <DataGridTextColumn.ElementStyle>
        //        <Style TargetType = "TextBlock" >
        //            < Setter Property="HorizontalAlignment" Value="Center" />
        //        </Style>
        //    </DataGridTextColumn.ElementStyle>
        //</DataGridTextColumn>



        //List<string> structuresNotMeetingVolumeConstraints = new List<string>();

        //int counter = 0;
        //foreach (VolumeDoseConstraint cont in mainControl.Constraints_DG.Items)
        //{
        //    if (cont.DoseValue.Contains("V") && 
        //        (cont.Result == "NOT MET"))
        //    {
        //        ++counter;
        //    }
        //    if (counter > 0)
        //    {
        //        foreach (Structure oar in mainControl.sorted_oarList)
        //        {
        //            if (oar.Id == cont.Id)
        //            {
        //                Structure firstPtvInList = mainControl.sorted_ptvList.First();
        //                OverlapStats overlapInfo = new OverlapStats();
        //                overlapInfo.structureId = oar?.Id.ToString().Split(':').First();
        //                overlapInfo.ptvId = firstPtvInList?.Id.ToString().Split(':').First();
        //                overlapInfo.overlapAbs = Math.Round(CalculateOverlap.VolumeOverlap(firstPtvInList, oar), 3);
        //                overlapInfo.overlapPct = overlapInfo.overlapAbs == 0 ? 0 : Math.Round(CalculateOverlap.PercentOverlap(oar, overlapInfo.overlapAbs), 1);
        //                overlapInfo.distance = overlapInfo.overlapAbs > 0 ? 0 : Math.Round(CalculateOverlap.ShortestDistance(firstPtvInList, oar), 1);

        //                mainControl.OverlapInfo_DG.Items.Add(overlapInfo);
        //            }
        //        }
        //    }
        //}
        ////foreach (Structure oar in mainControl.sorted_oarList)
        ////{
        ////    if (structuresNotMeetingVolumeConstraints.Contains(oar.Id.ToString()))
        ////    {

        ////        if (counter > 0)
        ////        {
        ////            Structure firstPtvInList = mainControl.sorted_ptvList.First();
        ////            OverlapStats overlapInfo = new OverlapStats();
        ////            overlapInfo.structureId = oar?.Id.ToString().Split(':').First();
        ////            overlapInfo.ptvId = firstPtvInList?.Id.ToString().Split(':').First();
        ////            overlapInfo.overlapAbs = Math.Round(CalculateOverlap.VolumeOverlap(firstPtvInList, oar), 3);
        ////            overlapInfo.overlapPct = overlapInfo.overlapAbs == 0 ? 0 : Math.Round(CalculateOverlap.PercentOverlap(oar, overlapInfo.overlapAbs), 1);
        ////            overlapInfo.distance = overlapInfo.overlapAbs > 0 ? 0 : Math.Round(CalculateOverlap.ShortestDistance(firstPtvInList, oar), 1);

        ////            mainControl.OverlapInfo_DG.Items.Add(overlapInfo);

        ////            // logic from dvh lookups -- to help optimize calc speed
        ////            //volumeIntersection = CalculateOverlap.VolumeOverlap(SelectedStructure, SelectedStructure2);
        ////            //percentOverlap = volumeIntersection == 0 ? 0 : CalculateOverlap.PercentOverlap(SelectedStructure2, volumeIntersection);
        ////            //shortestDistance = volumeIntersection > 0 ? 0 : CalculateOverlap.ShortestDistance(SelectedStructure, SelectedStructure2);
        ////        }
        ////    }
        ////}
    }
    class TestXml
    {
        //        <UserControl x:Class="DoseReview.MainControl"
        //             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        //             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        //             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
        //             xmlns:d="http://schemas.microsoft.com/expression/blend/2008" 
        //             mc:Ignorable="d" 
        //             Height="auto" Width="1100" Background="#FEFEFE" >

        //    <Grid>
        //        <Grid.RowDefinitions>
        //            <!-- Title Bar -->
        //            <RowDefinition Height = "auto" />
        //            < !--Window Content -->
        //            <RowDefinition Height = "*" />
        //        </ Grid.RowDefinitions >


        //        < Grid Grid.Row="0" Grid.Column="0" Panel.ZIndex="1" Height="22"  Background="#FEFEFE">
            
        //            <Grid.ColumnDefinitions>
                
        //                <!-- Icon -->
        //                <ColumnDefinition Width = "23" />
        //                < !--Title-- >
        //                < ColumnDefinition Width="*"/>
        //                <!-- Windows Buttons -->
        //                <ColumnDefinition Width = "auto" />


        //            </ Grid.ColumnDefinitions >

        //            < Image Grid.Column="0" 
        //                   Height="17.5"
        //                   Margin="2,0,2,0"
        //                   Source="/Images/varian.png"/>

        //            <Viewbox Grid.Column="1" Margin= "0" >
        //                < TextBlock Text= "{Binding Title, FallbackValue=DVH Review}" />
        //            </ Viewbox >

        //            < StackPanel Grid.Column= "2" Orientation= "Horizontal"
        //                        HorizontalAlignment= "Right"
        //                        VerticalAlignment= "Center"
        //                        Margin= "0,0,0,0" >

        //                < Button x:Name= "MinimizeButton"
        //                        KeyboardNavigation.IsTabStop= "False"
        //                        Click= "MinimizeButton_Clicked"
        //                        Content= "__"
        //                        Width= "18"
        //                        Background= "#FEFEFE"
        //                        />

        //                < Button x:Name= "MaximizeButton"
        //                        KeyboardNavigation.IsTabStop= "False"
        //                        Click= "MaximizeButton_Clicked"
        //                        Content= "[ ]"
        //                        Width= "18"
        //                        Background= "#FEFEFE"
        //                        />

        //                < Button x:Name= "CloseButton"
        //                        KeyboardNavigation.IsTabStop= "False"
        //                        Click= "CloseButton_Clicked"
        //                        Content= "X"
        //                        Width= "18"
        //                        Background= "#FEFEFE" >
        //                    < Button.Style >
        //                        < Style TargetType= "Button" >
        //                            < Style.Triggers >
        //                                < Trigger Property= "IsMouseOver" Value= "True" >
        //                                    < Setter Property= "Background" Value= "Red" />
        //                                </ Trigger >
        //                            </ Style.Triggers >
        //                        </ Style >
        //                    </ Button.Style >
        //                </ Button >
        //            </ StackPanel >
        //        </ Grid >

        //        < !--< Border CornerRadius= "2" BorderBrush= "LightGray" BorderThickness= "1" > -->
        //        < Grid Grid.Row= "1" >
        //            < StackPanel Margin= "0,0,563,0" >
        //                < Canvas Margin= "0,0,563,59" >
        //                    < Border Margin= "40" BorderThickness= "1" BorderBrush= "#282828" >
        //                        < Canvas x:Uid= "MainCanvas" x:Name= "MainCanvas" Width= "460" Height= "352" />
        //                    </ Border >
        //                    < Label Canvas.Left= "181" Canvas.Top= "12" Content= "Cumulative Dose Volume Histogram" FontWeight= "Bold" />
        //                    < Label x:Name= "VolumeLabel" Canvas.Left= "0" Canvas.Top= "28" Content= "100%" Height= "28" />
        //                    < Label Canvas.Left= "474" Canvas.Top= "394" Content= "100%" Height= "28" Name= "DoseMaxLabel" />
        //                    < Button Content= "Show OARs" Canvas.Left= "136" Canvas.Top= "399" Width= "97" Click= "ShowOars_Clicked" Height= "23" />
        //                    < Button Content= "Show Targets" Canvas.Left= "233" Canvas.Top= "399" Width= "97" Click= "ShowTargets_Clicked" Height= "23" />
        //                    < Button Content= "Clear DVH" Canvas.Left= "330" Canvas.Top= "399" Width= "97" Click= "ClearDvh_Clicked" Height= "23" />
        //                </ Canvas >
        //                < GroupBox Header= "General Dose Statistics" Canvas.Left= "12" Canvas.Top= "454" Margin= "10,366,5,0" FontWeight= "Bold" Height= "auto" >
        //                    < StackPanel >
        //                        < DataGrid x:Name= "StructureInfo_DG" HorizontalContentAlignment= "Center" Margin= "0,10,0,0" Height= "auto" >
        //                            < DataGrid.Resources >
        //                                < Style TargetType= "{x:Type DataGridColumnHeader}" >
        //                                    < Setter Property= "Background" Value= "LightBlue" />
        //                                    < Setter Property= "FontWeight" Value= "SemiBold" />
        //                                    < Setter Property= "BorderThickness" Value= "0,0,1,2" />
        //                                    < Setter Property= "BorderBrush" Value= "Black" />
        //                                    < Setter Property= "HorizontalContentAlignment" Value= "Center" />
        //                                </ Style >
        //                            </ DataGrid.Resources >
        //                            < DataGrid.Columns >
        //                                < DataGridTextColumn Header= "LC" Width= ".1*" IsReadOnly= "True" FontWeight= "Normal" >
        //                                    < DataGridTextColumn.ElementStyle >
        //                                        < Style TargetType= "{x:Type TextBlock}" >
        //                                            < Setter Property= "Background" Value= "{Binding color}" />
        //                                        </ Style >
        //                                    </ DataGridTextColumn.ElementStyle >
        //                                </ DataGridTextColumn >
        //                                < DataGridTextColumn Header= "Structure Id" Binding= "{Binding id}" Width= "*" IsReadOnly= "True" FontWeight= "Normal" />
        //                                < DataGridTextColumn Header= "Max (0.03cc)" Binding= "{Binding max03}" Width= "*" IsReadOnly= "True" FontWeight= "Normal" >
        //                                    < DataGridTextColumn.ElementStyle >
        //                                        < Style TargetType= "{x:Type TextBlock}" >
        //                                            < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                        </ Style >
        //                                    </ DataGridTextColumn.ElementStyle >
        //                                </ DataGridTextColumn >
        //                                < DataGridTextColumn Header= "Max" Binding= "{Binding max}" Width= "*" IsReadOnly= "True" FontWeight= "Normal" >
        //                                    < DataGridTextColumn.ElementStyle >
        //                                        < Style TargetType= "{x:Type TextBlock}" >
        //                                            < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                        </ Style >
        //                                    </ DataGridTextColumn.ElementStyle >
        //                                </ DataGridTextColumn >
        //                                < DataGridTextColumn Header= "Mean" Binding= "{Binding mean}" Width= "*" IsReadOnly= "True" FontWeight= "Normal" >
        //                                    < DataGridTextColumn.ElementStyle >
        //                                        < Style TargetType= "{x:Type TextBlock}" >
        //                                            < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                        </ Style >
        //                                    </ DataGridTextColumn.ElementStyle >
        //                                </ DataGridTextColumn >
        //                            </ DataGrid.Columns >
        //                        </ DataGrid >
        //                    </ StackPanel >
        //                </ GroupBox >
        //            </ StackPanel >
        //            < StackPanel x:Name= "StructureInfo_SP" Margin= "537,37,10,0" >
        //                < GroupBox Header= "Manually Calculate DVH Stats" Height= "50" VerticalAlignment= "Top" FontWeight= "Bold" >
        //                    < WrapPanel x:Name= "VolumeAtDose_WP" HorizontalAlignment= "Left" VerticalAlignment= "Center" >
        //                        < TextBlock x:Name= "V_TB" Text= "V" Height= "15" TextAlignment= "Right" HorizontalAlignment= "Center" VerticalAlignment= "Center" Width= "13" FontWeight= "Normal" />
        //                        < TextBox x:Name= "volumeAtDose_Input" Background= "LightBlue" Margin= "3,0,0,0" Height= "18" HorizontalAlignment= "Center" HorizontalContentAlignment= "Center" Width= "35" TextChanged= "volumeAtDose_Input_Changed" FontWeight= "Normal" />
        //                        < TextBlock x:Name= "volumeAtDoseResult_TB" Text= "volume at dose" Margin= "2,0,65,0" Height= "15" HorizontalAlignment= "Center" VerticalAlignment= "Center" FontWeight= "Normal" />
        //                        < TextBlock x:Name= "D_TB" Text= "D" Height= "15" HorizontalAlignment= "Center" VerticalAlignment= "Center" Margin= "30,0,0,0" FontWeight= "Normal" />
        //                        < TextBox x:Name= "doseAtVolume_Input" Background= "LightBlue" Margin= "3,0,0,0" Height= "18" HorizontalAlignment= "Center" HorizontalContentAlignment= "Center" Width= "35" TextChanged= "doseAtVolume_Input_Changed" FontWeight= "Normal" />
        //                        < TextBlock x:Name= "doseAtVolumeResult_TB" Text= "dose at volume" Margin= "2,0,0,0" Height= "15" HorizontalAlignment= "Center" VerticalAlignment= "Center" FontWeight= "Normal" />
        //                    </ WrapPanel >
        //                </ GroupBox >
        //                < GroupBox x:Name= "TargetStats_GB" Header= "Target Dose Statistics" Canvas.Left= "574" Canvas.Top= "40" Height= "172" FontWeight= "Bold" >
        //                    < DataGrid Name= "TargetCoverage_DG" HorizontalContentAlignment= "Center" Canvas.Left= "40" Canvas.Top= "431" Margin= "0,10,-2,-2" FontWeight= "Normal" >
        //                        < DataGrid.Resources >
        //                            < Style TargetType= "{x:Type DataGridColumnHeader}" >
        //                                < Setter Property= "Background" Value= "LightBlue" />
        //                                < Setter Property= "FontWeight" Value= "SemiBold" />
        //                                < Setter Property= "BorderThickness" Value= "0,0,1,2" />
        //                                < Setter Property= "BorderBrush" Value= "Black" />
        //                                < Setter Property= "HorizontalContentAlignment" Value= "Center" />
        //                            </ Style >
        //                        </ DataGrid.Resources >
        //                        < DataGrid.Columns >
        //                            < DataGridTextColumn Header= "LC" Width= ".18*" IsReadOnly= "True" FontWeight= "Normal" >
        //                                < DataGridTextColumn.ElementStyle >
        //                                    < Style TargetType= "TextBlock" >
        //                                        < Setter Property= "Background" Value= "{Binding color}" />
        //                                    </ Style >
        //                                </ DataGridTextColumn.ElementStyle >
        //                            </ DataGridTextColumn >
        //                            < DataGridTextColumn Header= "Target Id" Binding= "{Binding targetId}" Width= "1.1*" IsReadOnly= "True" />
        //                            < DataGridTextColumn Header= "D95" Binding= "{Binding d95}" Width= ".8*" IsReadOnly= "True" >
        //                                < DataGridTextColumn.ElementStyle >
        //                                    < Style TargetType= "TextBlock" >
        //                                        < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                    </ Style >
        //                                </ DataGridTextColumn.ElementStyle >
        //                            </ DataGridTextColumn >
        //                            < DataGridTextColumn Header= "Min(0.03cc)" Binding= "{Binding min03}" Width= "*" IsReadOnly= "True" >
        //                                < DataGridTextColumn.ElementStyle >
        //                                    < Style TargetType= "TextBlock" >
        //                                        < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                    </ Style >
        //                                </ DataGridTextColumn.ElementStyle >
        //                            </ DataGridTextColumn >
        //                            < DataGridTextColumn Header= "Min" Binding= "{Binding min}" Width= ".7*" IsReadOnly= "True" >
        //                                < DataGridTextColumn.ElementStyle >
        //                                    < Style TargetType= "TextBlock" >
        //                                        < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                    </ Style >
        //                                </ DataGridTextColumn.ElementStyle >
        //                            </ DataGridTextColumn >
        //                            < DataGridTextColumn Header= "Max(0.03cc)" Binding= "{Binding max03}" Width= "*" IsReadOnly= "True" >
        //                                < DataGridTextColumn.ElementStyle >
        //                                    < Style TargetType= "TextBlock" >
        //                                        < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                    </ Style >
        //                                </ DataGridTextColumn.ElementStyle >
        //                            </ DataGridTextColumn >
        //                            < DataGridTextColumn Header= "Max" Binding= "{Binding max}" Width= ".7*" IsReadOnly= "True" >
        //                                < DataGridTextColumn.ElementStyle >
        //                                    < Style TargetType= "TextBlock" >
        //                                        < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                    </ Style >
        //                                </ DataGridTextColumn.ElementStyle >
        //                            </ DataGridTextColumn >
        //                            < DataGridTextColumn Header= "Mean" Binding= "{Binding mean}" Width= ".7*" IsReadOnly= "True" >
        //                                < DataGridTextColumn.ElementStyle >
        //                                    < Style TargetType= "TextBlock" >
        //                                        < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                    </ Style >
        //                                </ DataGridTextColumn.ElementStyle >
        //                            </ DataGridTextColumn >
        //                            < DataGridTextColumn Header= "# Segments" Binding= "{Binding segments}" Width= "*" IsReadOnly= "True" >
        //                                < DataGridTextColumn.ElementStyle >
        //                                    < Style TargetType= "TextBlock" >
        //                                        < Style.Triggers >
        //                                            < Trigger Property= "Text" Value= "1" >
        //                                                < Setter Property= "Background" Value= "LightGreen" />
        //                                            </ Trigger >
        //                                            < Trigger Property= "Text" Value= ">1" >
        //                                                < Setter Property= "Background" Value= "#f4d641" />
        //                                            </ Trigger >
        //                                        </ Style.Triggers >
        //                                        < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                    </ Style >
        //                                </ DataGridTextColumn.ElementStyle >
        //                            </ DataGridTextColumn >
        //                        </ DataGrid.Columns >
        //                    </ DataGrid >
        //                    < !--< TextBlock x:Name= "TargetStats_TB" TextWrapping= "Wrap" Text= "stats go here" Margin= "0,3,212,31" HorizontalAlignment= "Left" VerticalAlignment= "Top" /> -->
        //                </ GroupBox >
        //                < GroupBox x:Name= "TargetProx_GB" Header= "Target Proximity Statistics" Height= "166" FontWeight= "Bold" >
        //                    < DataGrid Name= "OverlapInfo_DG" HorizontalContentAlignment= "Center" Canvas.Left= "40" Canvas.Top= "431" Margin= "0,10,-2,-2" FontWeight= "Normal" >
        //                        < DataGrid.Resources >
        //                            < Style TargetType= "{x:Type DataGridColumnHeader}" >
        //                                < Setter Property= "Background" Value= "LightBlue" />
        //                                < Setter Property= "FontWeight" Value= "SemiBold" />
        //                                < Setter Property= "BorderThickness" Value= "0,0,1,2" />
        //                                < Setter Property= "BorderBrush" Value= "Black" />
        //                                < Setter Property= "HorizontalContentAlignment" Value= "Center" />
        //                            </ Style >
        //                        </ DataGrid.Resources >


        //                        < DataGrid.Columns >
        //                            < DataGridTextColumn Header= "Structure Id" Binding= "{Binding structureId}" Width= "*" IsReadOnly= "True" />
        //                            < DataGridTextColumn Header= "Overlapping PTV" Binding= "{Binding ptvId}" Width= "*" IsReadOnly= "True" >
        //                                < DataGridTextColumn.ElementStyle >
        //                                    < Style TargetType= "TextBlock" >
        //                                        < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                    </ Style >
        //                                </ DataGridTextColumn.ElementStyle >
        //                            </ DataGridTextColumn >
        //                            < DataGridTextColumn Header= "Dist (cm)" Binding= "{Binding distance}" Width= ".6*" IsReadOnly= "True" >
        //                                < DataGridTextColumn.ElementStyle >
        //                                    < Style TargetType= "TextBlock" >
        //                                        < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                    </ Style >
        //                                </ DataGridTextColumn.ElementStyle >
        //                            </ DataGridTextColumn >
        //                            < DataGridTextColumn Header= "Overlap (cc)" Binding= "{Binding overlapAbs}" Width= ".9*" IsReadOnly= "True" >
        //                                < DataGridTextColumn.ElementStyle >
        //                                    < Style TargetType= "TextBlock" >
        //                                        < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                    </ Style >
        //                                </ DataGridTextColumn.ElementStyle >
        //                            </ DataGridTextColumn >
        //                            < DataGridTextColumn Header= "Overlap (%)" Binding= "{Binding overlapPct}" Width= ".9*" IsReadOnly= "True" >
        //                                < DataGridTextColumn.ElementStyle >
        //                                    < Style TargetType= "TextBlock" >
        //                                        < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                    </ Style >
        //                                </ DataGridTextColumn.ElementStyle >
        //                            </ DataGridTextColumn >
        //                            < DataGridTemplateColumn Header= "" Width= ".5*" >
        //                                < DataGridTemplateColumn.CellTemplate >
        //                                    < DataTemplate >
        //                                        < Button Content= "Remove" Click= "RemoveButton_Clicked" />
        //                                    </ DataTemplate >
        //                                </ DataGridTemplateColumn.CellTemplate >
        //                            </ DataGridTemplateColumn >
        //                        </ DataGrid.Columns >
        //                    </ DataGrid >
        //                </ GroupBox >
        //                < GroupBox x:Name= "DoseConstraints_GB" Header= "General Dose Constraints" Height= "auto" FontWeight= "Bold" >
        //                    < StackPanel >
        //                        < DataGrid Name= "Constraints_DG" HorizontalContentAlignment= "Center" Canvas.Left= "40" Canvas.Top= "431" Margin= "0,10,0,0" FontWeight= "Normal"  Height= "auto" >
        //                            < DataGrid.Resources >
        //                                < Style TargetType= "{x:Type DataGridColumnHeader}" >
        //                                    < Setter Property= "Background" Value= "LightBlue" />
        //                                    < Setter Property= "FontWeight" Value= "SemiBold" />
        //                                    < Setter Property= "BorderThickness" Value= "0,0,1,2" />
        //                                    < Setter Property= "BorderBrush" Value= "Black" />
        //                                    < Setter Property= "HorizontalContentAlignment" Value= "Center" />
        //                                </ Style >
        //                            </ DataGrid.Resources >
        //                            < DataGrid.Columns >
        //                                < DataGridTextColumn Header= "Structure Id" Binding= "{Binding Id}" Width= ".6*" IsReadOnly= "True" />
        //                                < DataGridTextColumn Header= "Dose Value" Binding= "{Binding DoseValue}" Width= ".8*" IsReadOnly= "True" >
        //                                    < DataGridTextColumn.ElementStyle >
        //                                        < Style TargetType= "TextBlock" >
        //                                            < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                        </ Style >
        //                                    </ DataGridTextColumn.ElementStyle >
        //                                </ DataGridTextColumn >
        //                                < DataGridTextColumn Header= "Limit" Binding= "{Binding Limit}" Width= ".4*" IsReadOnly= "True" >
        //                                    < DataGridTextColumn.ElementStyle >
        //                                        < Style TargetType= "TextBlock" >
        //                                            < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                        </ Style >
        //                                    </ DataGridTextColumn.ElementStyle >
        //                                </ DataGridTextColumn >
        //                                < DataGridTextColumn Header= "Result" Binding= "{Binding Result}" Width= ".5*" IsReadOnly= "True" >
        //                                    < DataGridTextColumn.ElementStyle >
        //                                        < Style TargetType= "TextBlock" >
        //                                            < Style.Triggers >
        //                                                < Trigger Property= "Text" Value= "MET" >
        //                                                    < Setter Property= "Background" Value= "LightGreen" />
        //                                                </ Trigger >
        //                                                < Trigger Property= "Text" Value= "NOT MET" >
        //                                                    < Setter Property= "Background" Value= "#F63C3C" />
        //                                                </ Trigger >
        //                                            </ Style.Triggers >
        //                                            < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                        </ Style >
        //                                    </ DataGridTextColumn.ElementStyle >
        //                                </ DataGridTextColumn >
        //                                < DataGridTextColumn Header= "Other (Gy)" Binding= "{Binding Other}" Width= ".7*" IsReadOnly= "True" >
        //                                    < DataGridTextColumn.ElementStyle >
        //                                        < Style TargetType= "TextBlock" >
        //                                            < Setter Property= "HorizontalAlignment" Value= "Center" />
        //                                        </ Style >
        //                                    </ DataGridTextColumn.ElementStyle >
        //                                </ DataGridTextColumn >
        //                            </ DataGrid.Columns >
        //                        </ DataGrid >
        //                    </ StackPanel >
        //                </ GroupBox >
        //            </ StackPanel >
        //            < ComboBox x:Name= "Structure_ComboBox" HorizontalAlignment= "Left" Margin= "537,10,0,0" VerticalAlignment= "Top" Width= "125" SelectionChanged= "Structure_ComboBox_SelectionChanged" />
        //            < CheckBox x:Name= "absDose_CB" Content= "Gy&#x9;" HorizontalAlignment= "Left" Margin= "849,16,0,0" VerticalAlignment= "Top" RenderTransformOrigin= "0.429,0.422" Width= "38" IsChecked= "True" Checked= "absDose_CB_Checked" Unchecked= "absDose_CB_UnChecked" />
        //            < CheckBox x:Name= "absVolume_CB" Content= "cc" HorizontalAlignment= "Left" Margin= "887,16,0,0" VerticalAlignment= "Top" IsChecked= "True" Checked= "absVolume_CB_Checked" Width= "32" Unchecked= "absVolume_CB_UnChecked" />
        //            < TextBlock x:Name= "StructureVolume_TB" HorizontalAlignment= "Left" Margin= "667,16,0,0" TextWrapping= "Wrap" Text= "Select a structure" VerticalAlignment= "Top" Width= "177" Height= "16" />
        //            < TextBlock x:Name= "PrimaryOnc_TB" HorizontalAlignment= "Left" Margin= "946,16,0,0" TextWrapping= "Wrap" Text= "Primary Onc:" VerticalAlignment= "Top" Width= "75" FontWeight= "Bold" />
        //            < TextBlock x:Name= "PrimaryOnc" HorizontalAlignment= "Left" Margin= "1026,16,0,0" TextWrapping= "Wrap" Text= "" VerticalAlignment= "Top" Width= "74" />
        //        </ Grid >
        //    < !--</ Border > -->
        //    </ Grid >
        //</ UserControl >
    }
}
