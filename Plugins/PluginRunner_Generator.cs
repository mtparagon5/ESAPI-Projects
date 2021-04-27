using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VMS.TPS.Common.Model.API;

namespace VMS.TPS
{
  class Script
  {
    public void Execute(ScriptContext context, Window window)
    {
      createGui(window);
    }

    // path to the published scripts directory
    private const string PUBLISHED_SCRIPTS_DIRECTORY = @"YOUR_PUBLISHED_SCRIPTS_DIR_HERE";

    // examples below are for an example app called PlanChecker that lives in the example directory: ..\PublishedScripts\PlanChecker\PlanChecker.exe
    private const string APP_DIRECTORY_NAME= @"YOUR_APP_DIR_HERE"; // YOUR APP DIR HERE - e.g., PlanChecker
    private const string APP_FILE_NAME = @"YOUR_APP_FILENAME_HERE"; // YOUR APP FILENAME HERE - e.g., PlanChecker.esapi, PlanChecker, etc.

    ScrollViewer sv = new ScrollViewer();
    StackPanel root_sp = new StackPanel();
    StackPanel first_sp = new StackPanel();
    StackPanel second_sp = new StackPanel();
    Label user_app_dir_label = new Label();
    Label user_app_file_name_label = new Label();
    Label full_app_path_label = new Label();
    Label plugin_runner_label = new Label();
    TextBox dir_name_tb = new TextBox();
    TextBox file_name_tb = new TextBox();
    Button button = new Button();

    public void createGui(Window window)
    {
      sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;

      window.Background = Brushes.White;
      window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      window.Height = 400;
      window.Width= 600;
      window.Title = "Plugin Generator - Create Plugin Runners for Application .exe Files";
      
      root_sp.Margin = new Thickness(5);
      root_sp.HorizontalAlignment = HorizontalAlignment.Center;
      root_sp.VerticalAlignment = VerticalAlignment.Center;

      first_sp.Orientation = Orientation.Horizontal;
      first_sp.Margin = new Thickness(5);

      second_sp.Orientation = Orientation.Horizontal;
      second_sp.Margin = new Thickness(5);



      user_app_dir_label.Content = "Your App Directory Name:";
      user_app_dir_label.FontWeight = FontWeights.Bold;
      user_app_dir_label.Width = 200;

      dir_name_tb.Width = 150;
      dir_name_tb.Margin = new Thickness(5);
      dir_name_tb.TextChanged += Dir_name_tb_TextChanged;



      user_app_file_name_label.Content = "Your App File Name:";
      user_app_file_name_label.FontWeight = FontWeights.Bold;
      user_app_file_name_label.Width = 200;

      file_name_tb.Width = 150;
      file_name_tb.Margin = new Thickness(5);
      file_name_tb.TextChanged += File_name_tb_TextChanged;
      


      full_app_path_label.Content = "";



      button.Content = "Create Plugin Runner";
      button.Padding = new Thickness(5);
      button.HorizontalAlignment = HorizontalAlignment.Center;
      button.Width = 150;
      button.Click += Button_Click;



      first_sp.Children.Add(user_app_dir_label);
      first_sp.Children.Add(dir_name_tb);

      second_sp.Children.Add(user_app_file_name_label);
      second_sp.Children.Add(file_name_tb);

      root_sp.Children.Add(first_sp);
      root_sp.Children.Add(second_sp);
      root_sp.Children.Add(full_app_path_label);
      root_sp.Children.Add(plugin_runner_label);
      root_sp.Children.Add(button);

      sv.Content = root_sp;

      window.Content = sv;
    }

    

    private void UpdateAppPath()
    {
      full_app_path_label.Content = string.Format("App Path:\n\t{0}\\{1}\\{2}.exe", PUBLISHED_SCRIPTS_DIRECTORY, dir_name_tb.Text, file_name_tb.Text);
      plugin_runner_label.Content = string.Format("Plugin Runner Path:\n\t{0}\\{1}.cs", PUBLISHED_SCRIPTS_DIRECTORY, file_name_tb.Text);
    }

    private void Dir_name_tb_TextChanged(object sender, TextChangedEventArgs e)
    {
      UpdateAppPath();
    }

    private void File_name_tb_TextChanged(object sender, TextChangedEventArgs e)
    {
      UpdateAppPath();
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      var user_defined_app_directory_name = dir_name_tb.Text;
      var user_defined_app_file_name = file_name_tb.Text;
      createPluginRunner(PUBLISHED_SCRIPTS_DIRECTORY, user_defined_app_directory_name, user_defined_app_file_name);
    }

    private void WriteFile(string filePath, string fileContent)
    {
      File.WriteAllText(filePath, fileContent);
    }

    private void createPluginRunner(string publishedScriptsDirectory, string appDirectoryName, string appFileName)
    {
      var appToRun = string.Format(@"{0}\{1}\{2}.exe", publishedScriptsDirectory, appDirectoryName, appFileName);
      var pluginRunnerPath = string.Format(@"{0}\{1}.cs", publishedScriptsDirectory, appFileName);

      var fileContent = 
@"using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using VMS.TPS.Common.Model.API;
using System.Windows.Controls;

namespace VMS.TPS
{
  public class Script
  {
    public void Execute(ScriptContext context)
    {
      try
      {
        Process.Start(" + "@\"" + appToRun + "\");" +
      @"}
      catch (Exception exc)
      {
        MessageBox.Show(string.Format(" + "\"Failed to start application:\\n\\n{0}\"" + ", exc.ToString()));" +
      @"}
    }
  }
}";
      MessageBox.Show(fileContent);
      WriteFile(pluginRunnerPath, fileContent);
    }
  }
}







