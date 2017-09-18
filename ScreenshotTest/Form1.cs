using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace ScreenshotTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private readonly Stopwatch sw = new Stopwatch();
        private Bitmap CaptureImage(int x, int y)
        {
            Bitmap b = new Bitmap(100, 100);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.CopyFromScreen(x, y, 0, 0, new Size(100, 100), CopyPixelOperation.SourceCopy);
            }
            return b;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap bmp = null;
            int x = 60;
            sw.Start();
            for (int i = 0; i < x; i++)
            {
                bmp = CaptureImage(100, 100);
            }
            sw.Stop();
            label1.Text = (sw.ElapsedMilliseconds/ x).ToString();
        }
    }
}