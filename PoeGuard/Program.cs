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
using System.Net.NetworkInformation;
using System.Net;
using System.IO;
using System.Threading;

namespace PoeGuard
{
    class Program
    {
        static readonly int BINARY_TRESHOLD = 180;
        static readonly string PATH_OF_EXILE_EXECUTABLE = "PathOfExileSteam.exe";
        static readonly Rectangle PATH_OF_EXILE_HEALTH_REGION = new Rectangle(55, 785, 130, 50);

        static bool pathOfExileHasFocus = false;
        static int pathOfExileProcessID = -1;
        static TcpRow pathOfExileEndpoint = null;

        static void Main(string[] args)
        {
            ActiveProcessObserver observer = new ActiveProcessObserver();
            DesktopDuplicator duplicator = null;

            observer.ProcessChanged += Observer_ProcessChanged;
            observer.Observe();

            try
            {
                duplicator = new DesktopDuplicator(0);

                using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                {
                    engine.SetVariable("tessedit_char_backlist", "!?@#$%&*()<>_-+=:;'\"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
                    engine.SetVariable("tessedit_char_whitelist", "/.,0123456789");
                    engine.SetVariable("classify_bln_numeric_mode", "1");

                    do
                    {
                        if (pathOfExileHasFocus)
                        {
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
                                // TODO(Olivier): Probably a worthless optimization since the screen will be a game
                                // but we could verify that our target rectangle is whitin bounds of the frame's
                                // changed areas
                                Bitmap area = CropImage(frame.DesktopImage, PATH_OF_EXILE_HEALTH_REGION);

                                using (Image<Gray, Byte> image = new Image<Gray, Byte>(area))
                                using (var tresh = image.ThresholdBinary(new Gray(BINARY_TRESHOLD), new Gray(255)))
                                using (var page = engine.Process(tresh.ToBitmap()))
                                {
                                    //TODO(Olivier): Use a regex to validate that the page's text is actually what we want
                                }
                            }
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }
                    while (true);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                Console.Read();
            }
        }

        private static void Observer_ProcessChanged(object sender, int exitingProcess, int activeProcess)
        {
            pathOfExileHasFocus = ProcessManager.GetProcessName(activeProcess) == PATH_OF_EXILE_EXECUTABLE;
            if (pathOfExileHasFocus)
            {
                Console.WriteLine("Path of Exile got focus");
                pathOfExileProcessID = activeProcess;
                pathOfExileEndpoint = ConnectionManager.GetExtendedTcpTable(false).First(x => x.ProcessId == activeProcess);
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
