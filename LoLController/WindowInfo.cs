using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Xaml.Controls;

namespace LoLController
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;        // x position of upper-left corner  
        public int Top;         // y position of upper-left corner  
        public int Right;       // x position of lower-right corner  
        public int Bottom;      // y position of lower-right corner  
    }

    internal class WindowInfo
    {
        //[DllImport("user32.dll", ExactSpelling = false)]
        //static extern IntPtr GetForegroundWindow();
        //[DllImport("user32.dll", ExactSpelling = false)]
        //static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        //[DllImport("user32.dll", ExactSpelling = false)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);


        //public static WindowInfo GetActiveWindow()
        //{
        //    const int nChars = 256;
        //    StringBuilder titleBuffer = new StringBuilder(nChars);

        //    IntPtr handle = GetForegroundWindow();

        //    if (GetWindowText(handle, titleBuffer, nChars) > 0 && GetWindowRect(handle, out RECT rect))
        //    {
        //        return new WindowInfo(handle, titleBuffer.ToString(), rect);
        //    }

        //    return null;
        //}


        //[DllImport("User32.dll", ExactSpelling = true)]
        //public static extern Int32 SendMessage(IntPtr hWnd, int Msg, int wParam, [MarshalAs(UnmanagedType.LPStr)] string lParam);

        //[DllImport("user32.dll", SetLastError = true, ExactSpelling = true)]
        //public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        //[DllImport("user32.dll", SetLastError = true, ExactSpelling = true)]
        //public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        public static Size GetCurrentDisplaySize()
        {
            var displayInformation = DisplayInformation.GetForCurrentView();
            TypeInfo t = typeof(DisplayInformation).GetTypeInfo();
            var props = t.DeclaredProperties.Where(x => x.Name.StartsWith("Screen") && x.Name.EndsWith("InRawPixels")).ToArray();
            var w = props.Where(x => x.Name.Contains("Width")).First().GetValue(displayInformation);
            var h = props.Where(x => x.Name.Contains("Height")).First().GetValue(displayInformation);
            var size = new Size(System.Convert.ToDouble(w), System.Convert.ToDouble(h));
            switch (displayInformation.CurrentOrientation)
            {
                case DisplayOrientations.Landscape:
                case DisplayOrientations.LandscapeFlipped:
                    size = new Size(Math.Max(size.Width, size.Height), Math.Min(size.Width, size.Height));
                    break;
                case DisplayOrientations.Portrait:
                case DisplayOrientations.PortraitFlipped:
                    size = new Size(Math.Min(size.Width, size.Height), Math.Max(size.Width, size.Height));
                    break;
            }
            return size;
        }

        public IntPtr Handle { get; private set; } // legacy from user32.dll, but im too lazy to remove it
        public string Title { get; private set; }
        public RECT Rectangle { get; private set; }

        public WindowInfo(IntPtr handle, string title, RECT rectangle)
        {
            Handle = handle;
            Title = title;
            Rectangle = rectangle;
        }
    }
}
