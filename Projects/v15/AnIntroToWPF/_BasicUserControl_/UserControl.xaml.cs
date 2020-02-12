using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace VMS.TPS
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainControl : UserControl
  {
    public MainControl()
    {
        InitializeComponent();
    }

    public double DEFAULT_BIN_WIDTH = 0.001;

    // you can declare public/private variables here
    public Patient patient;
    public StructureSet ss;
    public PlanningItem pitem;

    // for the filtered/sorted structure lists
    public IEnumerable<Structure> sorted_gtvList;
    public IEnumerable<Structure> sorted_ctvList;
    public IEnumerable<Structure> sorted_itvList;
    public IEnumerable<Structure> sorted_ptvList;
    public IEnumerable<Structure> sorted_targetList;
    public IEnumerable<Structure> sorted_oarList;
    public IEnumerable<Structure> sorted_structureList;
    public IEnumerable<Structure> sorted_emptyStructuresList;


    // when you add a click event for a button the functions generally go here in this file
    private void GetTargetVolume_Button_Click(object sender, RoutedEventArgs e)
    {
      var s = sender as Structure;
      var target = ss.Structures.Single(st => st.Id == s.Id);

      MessageBox.Show(string.Format("{0}: {1} cc", target.Id, Math.Round(target.Volume, 3)));
    }

    private void GetOars_Button_Click(object sender, RoutedEventArgs e)
    {
      foreach (var s in sorted_oarList)
      {
        Oars_ListView.Items.Add(s.Id);
      }
    }
  }
}
