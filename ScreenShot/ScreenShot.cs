using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ScreenshotCapturer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get the current screen size
            Rectangle bounds = Screen.GetBounds(Point.Empty);

            // Create a bitmap object to capture the screenshot
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                // Create a graphics object from the bitmap
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    // Capture the screen and draw it on the bitmap
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);

                    // Save the bitmap as a PNG file in the ProgramData directory
                    bitmap.Save(@"C:\ProgramData\Screenshot.png", ImageFormat.Png);
                }
            }
        }
    }
}


