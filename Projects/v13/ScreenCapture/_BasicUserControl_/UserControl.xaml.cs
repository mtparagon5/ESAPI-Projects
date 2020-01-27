using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VMS.TPS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainControl : System.Windows.Controls.UserControl
    {
        public MainControl()
        {
            InitializeComponent();
        }

        #region variables

        public string patientId;
        public string fileSuffix;
        public string courseId;

        #endregion variables

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr onj);

        private void Primary_OnClick(object sender, RoutedEventArgs e)
        {
            patientId = PatientId.Text;
            //if (FileSuffix.Text != "")
            //{
            //    fileSuffix = "_" + FileSuffix.Text;
            //}
            //else
            //{
            //    fileSuffix = FileSuffix.Text;
            //}

            if (patientId == "")
            {
                System.Windows.MessageBox.Show("Please enter the patient id for the corresponding screen shot.");
            }
            else
            {
                var screen = Screen.AllScreens[0];
                var screenshot = new Bitmap(screen.Bounds.Width,
                                            screen.Bounds.Height,
                                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                // Create a graphics object from the bitmap
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    //g.CopyFromScreen(0, 0, 0, 0, screenshot.Size);
                    g.CopyFromScreen(screen.Bounds.X,
                                        screen.Bounds.Y,
                                        0,
                                        0,
                                        screen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);
                }
                IntPtr handle = IntPtr.Zero;
                try
                {
                    handle = screenshot.GetHbitmap();
                    ImageControl.Source = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                    screenshot.Save(string.Format("\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\Images\\{0}_{1}{2}.jpg", patientId, courseId, fileSuffix)); //saving
                }
                catch (Exception)
                {

                }

                finally
                {
                    DeleteObject(handle);
                }
            }
        }
        private void Secondary_OnClick(object sender, RoutedEventArgs e)
        {
            patientId = PatientId.Text;
            if (FileSuffix.Text != "")
            {
                fileSuffix = "_" + FileSuffix.Text;
            }
            else
            {
                fileSuffix = FileSuffix.Text;
            }

            if (patientId == "")
            {
                System.Windows.MessageBox.Show("Please enter the patient id for the corresponding screen shot.");
            }
            else
            {
                var screen = Screen.AllScreens[1];
                var screenshot = new Bitmap(screen.Bounds.Width,
                                            screen.Bounds.Height,
                                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                // Create a graphics object from the bitmap
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    //g.CopyFromScreen(0, 0, 0, 0, screenshot.Size);
                    g.CopyFromScreen(screen.Bounds.X,
                                        screen.Bounds.Y,
                                        0,
                                        0,
                                        screen.Bounds.Size,
                                        CopyPixelOperation.SourceCopy);
                }
                IntPtr handle = IntPtr.Zero;
                try
                {
                    handle = screenshot.GetHbitmap();
                    ImageControl.Source = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                    screenshot.Save(string.Format("\\\\Client\\S$\\shares\\RadOnc\\ePHI\\RO PHI PHYSICS\\matt\\Images\\{0}{1}.jpg", patientId, fileSuffix)); //saving
                }
                catch (Exception)
                {

                }

                finally
                {
                    DeleteObject(handle);
                }
            }
        }

        private void FileSuffix_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as System.Windows.Controls.ComboBox;

            if (cb.SelectedValue.ToString() != "")
            {
                fileSuffix = "_" + cb.SelectedValue.ToString();
            }
            else
            {
                fileSuffix = string.Empty;
            }
        }
    }
}
