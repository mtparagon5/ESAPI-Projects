
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

// esapi
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


namespace VMS.TPS
{
	public class Script
    {
        public Script()
        {
        }
		//---------------------------------------------------------------------------------------------  
		#region execute

		public void Execute(ScriptContext context)
		{
            // Whole window or primary screen
            //var image = ScreenCapture.CaptureDesktop();
            //image.Save(@"H:\snippetsource.jpg", ImageFormat.Jpeg);

            // active window
            var image = ScreenCapture.CaptureActiveWindow();
            image.Save(@"H:\snippetsource.jpg", ImageFormat.Jpeg);

        }

        #region helper methods

        public class ScreenCapture
        {
            [DllImport("user32.dll")]
            private static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern IntPtr GetDesktopWindow();

            [StructLayout(LayoutKind.Sequential)]
            private struct Rect
            {
                public int Left;
                public int Top;
                public int Right;
                public int Bottom;
            }

            [DllImport("user32.dll")]
            private static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

            public static Image CaptureDesktop()
            {
                return CaptureWindow(GetDesktopWindow());
            }

            public static Bitmap CaptureActiveWindow()
            {
                return CaptureWindow(GetForegroundWindow());
            }

            public static Bitmap CaptureWindow(IntPtr handle)
            {
                var rect = new Rect();
                GetWindowRect(handle, ref rect);
                var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
                var result = new Bitmap(bounds.Width, bounds.Height);

                using (var graphics = Graphics.FromImage(result))
                {
                    graphics.CopyFromScreen(new System.Drawing.Point(bounds.Left, bounds.Top), System.Drawing.Point.Empty, bounds.Size);
                }

                return result;
            }
        }

        #endregion

        #endregion execute
    }
}
