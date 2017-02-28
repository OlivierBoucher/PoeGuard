using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DesktopDuplication;
using Tesseract;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.Structure;

namespace TesterForm
{
    public partial class Form1 : Form
    {

        private DesktopDuplicator desktopDuplicator;

        public Form1()
        {
            InitializeComponent();

            try
            {
                desktopDuplicator = new DesktopDuplicator(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {
                engine.SetVariable("tessedit_char_backlist", "!?@#$%&*()<>_-+=:;'\"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
                engine.SetVariable("tessedit_char_whitelist", "/.,0123456789");
                engine.SetVariable("classify_bln_numeric_mode", "1");
                var rect1 = new Rectangle(55, 785, 130, 50);
                while (true)
                {
                    Application.DoEvents();

                    var elapsed = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    DesktopFrame frame = null;
                    try
                    {
                        frame = desktopDuplicator.GetLatestFrame();
                    }
                    catch
                    {
                        desktopDuplicator = new DesktopDuplicator(0);
                        continue;
                    }

                    if (frame != null)
                    {   
                        
                        Bitmap gauges = this.CropImage(frame.DesktopImage, rect1);

                        using (Image<Gray, Byte> image = new Image<Gray, Byte>(gauges))
                        using (var tresh = image.ThresholdBinary(new Gray(180), new Gray(255)))
                        {
                            this.pictureBox1.Image = tresh.ToBitmap();
                            using (var page = engine.Process(tresh.ToBitmap(), new Rect(0, 0, 130, 50)))
                            {
                                this.label1.Text = page.GetText();
                                this.label2.Text = String.Format("{0}ms", (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - elapsed);
                            }

                        }
                    }
                }
            }
        }
        private Bitmap CropImage(Bitmap source, Rectangle section)
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
