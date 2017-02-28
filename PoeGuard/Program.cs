using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopDuplication;
using Tesseract;
using System.Drawing;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.Structure;

namespace PoeGuard
{
    class Program
    {
        static readonly int BINARY_TRESHOLD = 180;
        static void Main(string[] args)
        {
            DesktopDuplicator duplicator = null;
            var rect = new Rectangle(55, 785, 130, 50);

            try
            {
                duplicator = new DesktopDuplicator(0);

                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                {
                    engine.SetVariable("tessedit_char_backlist", "!?@#$%&*()<>_-+=:;'\"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
                    engine.SetVariable("tessedit_char_whitelist", "/.,0123456789");
                    engine.SetVariable("classify_bln_numeric_mode", "1");

                    while (true)
                    {
                        var elapsed = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                        DesktopFrame frame = null;
                        try
                        {
                            frame = duplicator.GetLatestFrame();
                        }
                        catch
                        {
                            duplicator = new DesktopDuplicator(0);
                            continue;
                        }

                        if (frame != null)
                        {
                            Bitmap area = CropImage(frame.DesktopImage, rect);

                            using (Image<Gray, Byte> image = new Image<Gray, Byte>(area))
                            using (var tresh = image.ThresholdBinary(new Gray(BINARY_TRESHOLD), new Gray(255)))
                            using (var page = engine.Process(tresh.ToBitmap()))
                            {
                                Console.WriteLine("{0} in {1}ms", page.GetText(), (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - elapsed);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Read();
            }
        }

        private static Bitmap CropImage(Bitmap source, Rectangle section)
        {
            // An empty bitmap which will hold the cropped image
            Bitmap bmp = new Bitmap(section.Width, section.Height);

            Graphics g = Graphics.FromImage(bmp);

            // Draw the given area (section) of the source image
            // at location 0,0 on the empty bitmap (bmp)
            g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);

            return bmp;
        }
    }
}
