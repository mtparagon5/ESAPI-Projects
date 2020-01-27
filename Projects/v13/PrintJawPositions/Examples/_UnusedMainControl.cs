using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProximityStatistics
{
    class UnusedMainControl
    {
            //<StackPanel Margin = "0,0,661,0" >
            //    < Canvas Margin="0,0,563,59">
            //        <Border Margin = "40" BorderThickness="1" BorderBrush="#282828">
            //            <Canvas x:Uid="MainCanvas" x:Name="MainCanvas" Width="460" Height="352"/>
            //        </Border>
            //        <Label Canvas.Left="171" Canvas.Top= "8" Content= "Cumulative Dose Volume Histogram" FontWeight= "Bold" />
            //        < Label x:Name= "VolumeLabel" Canvas.Left= "0" Canvas.Top= "28" Content= "100%" Height= "28" />
            //        < Label Canvas.Left= "474" Canvas.Top= "394" Content= "100%" Height= "28" Name= "DoseMaxLabel" />
            //        < Button Name= "ShowOAR_Btn" Content= "Show OARs" Canvas.Left= "132" Canvas.Top= "399" Width= "97" Click= "ShowOars_Clicked" Height= "23" Background= "#DCDCDC" />
            //        < Button Name= "ShowTargets_Btn" Content= "Show Targets" Canvas.Left= "229.4" Canvas.Top= "399" Width= "97" Click= "ShowTargets_Clicked" Height= "23" Background= "#DCDCDC" />
            //        < Button Name= "ClearDvh_Btn" Content= "Clear DVH" Canvas.Left= "326" Canvas.Top= "399" Width= "97" Click= "ClearDvh_Clicked" Height= "23" Background= "#DCDCDC" />
            //    </ Canvas >
            //    < GroupBox Header= "General Dose Statistics" Canvas.Left= "12" Canvas.Top= "454" Margin= "10,366,5,2" FontWeight= "Bold" Height= "auto" >
            //        < StackPanel >
            //            < DataGrid x:Name= "StructureInfo_DG" HorizontalContentAlignment= "Center" ScrollViewer.CanContentScroll= "True" Margin= "0,10,0,2" Height= "auto" >
            //                < DataGrid.Resources >
            //                    < Style TargetType= "{x:Type DataGridColumnHeader}" >
            //                        < Setter Property= "Background" Value= "LightBlue" />
            //                        < Setter Property= "FontWeight" Value= "Bold" />
            //                        < Setter Property= "BorderThickness" Value= "0,0,1,2" />
            //                        < Setter Property= "BorderBrush" Value= "Black" />
            //                        < Setter Property= "HorizontalContentAlignment" Value= "Center" />
            //                    </ Style >
            //                </ DataGrid.Resources >
            //                < !--< DataGrid.CellStyle >
            //                    < Style TargetType= "DataGridCell" >
            //                        < Setter Property= "BorderBrush" Value= "Black" />
            //                        < Setter Property= "BorderThickness" Value= "0" />
            //                        < Setter Property= "Opacity" Value= "1" />
            //                        -->
            //                < !--< Setter Property= "Margin" Value= "5,5,5,5" /> -->
            //                < !-- < Setter Property= "Background" Value= "White" /> -->
            //                < !--
            //                    </ Style >
            //                </ DataGrid.CellStyle > -->
            //                < DataGrid.Columns >
            //                    < DataGridTextColumn Header= "LC" Width= ".1*" IsReadOnly= "True" FontWeight= "Normal" >
            //                        < DataGridTextColumn.ElementStyle >
            //                            < Style TargetType= "{x:Type TextBlock}" >
            //                                < Setter Property= "Background" Value= "{Binding color}" />
            //                            </ Style >
            //                        </ DataGridTextColumn.ElementStyle >
            //                    </ DataGridTextColumn >
            //                    < DataGridTextColumn Header= "Structure Id" Binding= "{Binding id}" Width= "*" IsReadOnly= "True" FontWeight= "Normal" />
            //                    < DataGridTextColumn Header= "Volume" Binding= "{Binding volume}" Width= "*" IsReadOnly= "True" FontWeight= "Normal" >
            //                        < DataGridTextColumn.ElementStyle >
            //                            < Style TargetType= "{x:Type TextBlock}" >
            //                                < Setter Property= "HorizontalAlignment" Value= "Center" />
            //                            </ Style >
            //                        </ DataGridTextColumn.ElementStyle >
            //                    </ DataGridTextColumn >
            //                    < DataGridTextColumn Header= "Max (0.03cc)" Binding= "{Binding max03}" Width= "*" IsReadOnly= "True" FontWeight= "Normal" >
            //                        < DataGridTextColumn.ElementStyle >
            //                            < Style TargetType= "{x:Type TextBlock}" >
            //                                < Setter Property= "HorizontalAlignment" Value= "Center" />
            //                            </ Style >
            //                        </ DataGridTextColumn.ElementStyle >
            //                    </ DataGridTextColumn >
            //                    < DataGridTextColumn Header= "Max" Binding= "{Binding max}" Width= "*" IsReadOnly= "True" FontWeight= "Normal" >
            //                        < DataGridTextColumn.ElementStyle >
            //                            < Style TargetType= "{x:Type TextBlock}" >
            //                                < Setter Property= "HorizontalAlignment" Value= "Center" />
            //                            </ Style >
            //                        </ DataGridTextColumn.ElementStyle >
            //                    </ DataGridTextColumn >
            //                    < DataGridTextColumn Header= "Mean" Binding= "{Binding mean}" Width= "*" IsReadOnly= "True" FontWeight= "Normal" >
            //                        < DataGridTextColumn.ElementStyle >
            //                            < Style TargetType= "{x:Type TextBlock}" >
            //                                < Setter Property= "HorizontalAlignment" Value= "Center" />
            //                            </ Style >
            //                        </ DataGridTextColumn.ElementStyle >
            //                    </ DataGridTextColumn >
            //                    < DataGridTemplateColumn Header= "" Width= ".47*" >
            //                        < DataGridTemplateColumn.CellTemplate >
            //                            < DataTemplate >
            //                                < Button Name= "test_btn" Content= "n/a" Click= "StructureInfoRemoveButton_Clicked" Background= "#DCDCDC" FontWeight= "Regular" />
            //                            </ DataTemplate >
            //                        </ DataGridTemplateColumn.CellTemplate >
            //                    </ DataGridTemplateColumn >
            //                </ DataGrid.Columns >
            //            </ DataGrid >
            //        </ StackPanel >
            //    </ GroupBox >
            //</ StackPanel >
            //<ComboBox x:Name="Structure_ComboBox" HorizontalAlignment="Left" Margin="537,10,0,0" VerticalAlignment="Top" Width="125" SelectionChanged="Structure_ComboBox_SelectionChanged"/>
            //<CheckBox x:Name="absDose_CB" Content="Gy&#x9;" HorizontalAlignment="Left" Margin="849,16,0,0" VerticalAlignment="Top" RenderTransformOrigin="0.429,0.422" Width="38" IsChecked="True" Checked="absDose_CB_Checked" Unchecked="absDose_CB_UnChecked"/>
            //<CheckBox x:Name="absVolume_CB" Content="cc" HorizontalAlignment="Left" Margin="887,16,0,0" VerticalAlignment="Top" IsChecked="True" Checked="absVolume_CB_Checked" Width="32" Unchecked="absVolume_CB_UnChecked"/>
            //<TextBlock x:Name="StructureVolume_TB" HorizontalAlignment="Left" Margin="667,16,0,0" TextWrapping="Wrap" Text="Select a structure" VerticalAlignment="Top" Width="177" Height="16"/>
            //<GroupBox x:Name="DoseConstraints_GB" Header="General Dose Constraints" Margin=" 0,0,0,2" Height="auto" FontWeight="Bold">
            //        <StackPanel>
            //            <DataGrid Name = "Constraints_DG" HorizontalContentAlignment="Center" ScrollViewer.CanContentScroll="True" Canvas.Left="40" Canvas.Top="431" Margin="0,10,0,2" FontWeight="Normal"  Height="auto">
            //                <DataGrid.Resources>
            //                    <Style TargetType = "{x:Type DataGridColumnHeader}" >
            //                        < Setter Property="Background" Value="LightBlue"/>
            //                        <Setter Property = "FontWeight" Value="Bold"/>
            //                        <Setter Property = "BorderThickness" Value="0,0,1,2"/>
            //                        <Setter Property = "BorderBrush" Value="Black"/>
            //                        <Setter Property = "HorizontalContentAlignment" Value="Center"/>
            //                    </Style>
            //                </DataGrid.Resources>
            //                <DataGrid.Columns>
            //                    <DataGridTextColumn Header = "Structure Id" Binding="{Binding Id}" Width=".6*" IsReadOnly="True"/>
            //                    <DataGridTextColumn Header = "Type" Binding="{Binding Type}" Width=".4*" IsReadOnly="True">
            //                        <DataGridTextColumn.ElementStyle>
            //                            <Style TargetType = "TextBlock" >
            //                                < Setter Property="HorizontalAlignment" Value="Center" />
            //                            </Style>
            //                        </DataGridTextColumn.ElementStyle>
            //                    </DataGridTextColumn>
            //                    <DataGridTextColumn Header = "Limit" Binding="{Binding Limit}" Width=".4*" IsReadOnly="True">
            //                        <DataGridTextColumn.ElementStyle>
            //                            <Style TargetType = "TextBlock" >
            //                                < Setter Property="HorizontalAlignment" Value="Center" />
            //                            </Style>
            //                        </DataGridTextColumn.ElementStyle>
            //                    </DataGridTextColumn>
            //                    <DataGridTextColumn Header = "Actual" Binding="{Binding DoseValue}" Width=".5*" IsReadOnly="True">
            //                        <DataGridTextColumn.ElementStyle>
            //                            <Style TargetType = "TextBlock" >
            //                                < Setter Property="HorizontalAlignment" Value="Center" />
            //                            </Style>
            //                        </DataGridTextColumn.ElementStyle>
            //                    </DataGridTextColumn>
            //                    <DataGridTextColumn Header = "Result" Binding="{Binding Result}" Width=".4*" IsReadOnly="True">
            //                        <DataGridTextColumn.ElementStyle>
            //                            <Style TargetType = "TextBlock" >
            //                                < Style.Triggers >
            //                                    < Trigger Property="Text" Value="MET">
            //                                        <Setter Property = "Background" Value="LightGreen"/>
            //                                    </Trigger>
            //                                    <Trigger Property = "Text" Value="NOT MET">
            //                                        <Setter Property = "Background" Value="#F63C3C"/>
            //                                    </Trigger>
            //                                </Style.Triggers>
            //                                <Setter Property = "HorizontalAlignment" Value="Center" />
            //                            </Style>
            //                        </DataGridTextColumn.ElementStyle>
            //                    </DataGridTextColumn>
            //                    <DataGridTextColumn Header = "Dose at Limit (Gy)" Binding="{Binding Other}" Width=".7*" IsReadOnly="True">
            //                        <DataGridTextColumn.ElementStyle>
            //                            <Style TargetType = "TextBlock" >
            //                                < Setter Property="HorizontalAlignment" Value="Center" />
            //                            </Style>
            //                        </DataGridTextColumn.ElementStyle>
            //                    </DataGridTextColumn>
            //                    <DataGridTemplateColumn Header = "" Width=".2*">
            //                        <DataGridTemplateColumn.CellTemplate>
            //                            <DataTemplate>
            //                                <Button Name = "test_btn" Content="n/a" Click="ConstraintRemoveButton_Clicked" Background="#DCDCDC"/>
            //                            </DataTemplate>
            //                        </DataGridTemplateColumn.CellTemplate>
            //                    </DataGridTemplateColumn>
            //                </DataGrid.Columns>
            //            </DataGrid>
            //        </StackPanel>
            //    </GroupBox>
            //<DataGridTextColumn Header = "D95" Binding="{Binding d95}" Width=".8*" IsReadOnly="True">
            //                    <DataGridTextColumn.ElementStyle>
            //                        <Style TargetType = "TextBlock" >
            //                            < Setter Property="HorizontalAlignment" Value="Center" />
            //                        </Style>
            //                    </DataGridTextColumn.ElementStyle>
            //                </DataGridTextColumn>
            //                <DataGridTextColumn Header = "Min(0.03cc)" Binding="{Binding min03}" Width="*" IsReadOnly="True">
            //                    <DataGridTextColumn.ElementStyle>
            //                        <Style TargetType = "TextBlock" >
            //                            < Setter Property="HorizontalAlignment" Value="Center" />
            //                        </Style>
            //                    </DataGridTextColumn.ElementStyle>
            //                </DataGridTextColumn>
            //                <DataGridTextColumn Header = "Min" Binding="{Binding min}" Width=".7*" IsReadOnly="True">
            //                    <DataGridTextColumn.ElementStyle>
            //                        <Style TargetType = "TextBlock" >
            //                            < Setter Property="HorizontalAlignment" Value="Center" />
            //                        </Style>
            //                    </DataGridTextColumn.ElementStyle>
            //                </DataGridTextColumn>
            //                <DataGridTextColumn Header = "Max(0.03cc)" Binding="{Binding max03}" Width="*" IsReadOnly="True">
            //                    <DataGridTextColumn.ElementStyle>
            //                        <Style TargetType = "TextBlock" >
            //                            < Setter Property="HorizontalAlignment" Value="Center" />
            //                        </Style>
            //                    </DataGridTextColumn.ElementStyle>
            //                </DataGridTextColumn>
            //                <DataGridTextColumn Header = "Max" Binding="{Binding max}" Width=".7*" IsReadOnly="True">
            //                    <DataGridTextColumn.ElementStyle>
            //                        <Style TargetType = "TextBlock" >
            //                            < Setter Property="HorizontalAlignment" Value="Center" />
            //                        </Style>
            //                    </DataGridTextColumn.ElementStyle>
            //                </DataGridTextColumn>
            //                <DataGridTextColumn Header = "Mean" Binding="{Binding mean}" Width=".7*" IsReadOnly="True">
            //                    <DataGridTextColumn.ElementStyle>
            //                        <Style TargetType = "TextBlock" >
            //                            < Setter Property="HorizontalAlignment" Value="Center" />
            //                        </Style>
            //                    </DataGridTextColumn.ElementStyle>
            //                </DataGridTextColumn>
            //<GroupBox Header = "Manually Calculate DVH Stats" Height="50" VerticalAlignment="Top" FontWeight="Bold">
            //        <WrapPanel x:Name="VolumeAtDose_WP" HorizontalAlignment="Left" VerticalAlignment="Center">
            //            <TextBlock x:Name="V_TB" Text="V" Height="15" TextAlignment="Right" HorizontalAlignment="Center" VerticalAlignment="Center" Width="13" FontWeight="Normal"/>
            //            <TextBox x:Name="volumeAtDose_Input" Background="LightBlue" Margin="3,0,0,0" Height="18" HorizontalAlignment="Center" HorizontalContentAlignment="Center" Width="35" TextChanged="volumeAtDose_Input_Changed" FontWeight="Normal"/>
            //            <TextBlock x:Name="volumeAtDoseResult_TB" Text="volume at dose" Margin="2,0,65,0" Height="15" HorizontalAlignment="Center" VerticalAlignment="Center" FontWeight="Normal"/>
            //            <TextBlock x:Name="D_TB" Text="D" Height="15" HorizontalAlignment="Center" VerticalAlignment="Center" Margin="30,0,0,0" FontWeight="Normal"/>
            //            <TextBox x:Name="doseAtVolume_Input" Background="LightBlue" Margin="3,0,0,0" Height="18" HorizontalAlignment="Center" HorizontalContentAlignment="Center" Width="35" TextChanged="doseAtVolume_Input_Changed" FontWeight="Normal"/>
            //            <TextBlock x:Name="doseAtVolumeResult_TB" Text="dose at volume" Margin="2,0,0,0" Height="15" HorizontalAlignment="Center" VerticalAlignment="Center" FontWeight="Normal" />
            //        </WrapPanel>
            //</GroupBox>
    }
}
