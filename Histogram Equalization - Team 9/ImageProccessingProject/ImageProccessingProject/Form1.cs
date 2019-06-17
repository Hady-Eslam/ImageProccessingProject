using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace ImageProccessingProject
{
    public partial class Form1 : Form
    {
        private Bitmap OriginalImage;
        int w, h, r, g, b, x = 0, y = 0, count = 0;

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        public Form1()
        {
            InitializeComponent();
        }

        private OpenFileDialog openFileDialog = new OpenFileDialog();

        private void button1_Click(object sender, EventArgs e)
        {

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    OriginalImage = new Bitmap(openFileDialog.FileName);
                    w = OriginalImage.Width;
                    h = OriginalImage.Height;
                }
                catch
                {
                    MessageBox.Show("Can't open file.");
                }
                pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox1.Image = OriginalImage;

                // Get Histogram
                
                Bitmap bmp = new Bitmap(OriginalImage);
                int[] histogram_r = new int[256];
                float max = 0;

                for (int i = 0; i < bmp.Width; i++)
                {
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        int redValue = bmp.GetPixel(i, j).R;
                        histogram_r[redValue]++;
                        if (max < histogram_r[redValue])
                            max = histogram_r[redValue];
                    }
                }

                int histHeight = 232;
                Bitmap img = new Bitmap(256, histHeight + 10);
                using (Graphics g = Graphics.FromImage(img))
                {
                    for (int i = 0; i < histogram_r.Length; i++)
                    {
                        float pct = histogram_r[i] / max;   // What percentage of the max is this value?
                        g.DrawLine(Pens.Black,
                            new Point(i, img.Height - 5),
                            new Point(i, img.Height - 5 - (int)(pct * histHeight))  // Use that percentage of the height
                            );
                    }
                }
                pictureBox2.Image = img;
            }
        }

        static double Clamp(double val, double min, double max)
        {
            return Math.Min(Math.Max(val, min), max);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double blackPointPercent = 0.01;
            double whitePointPercent = 0.03;
            BitmapData srcData = OriginalImage.LockBits(new Rectangle(0, 0, OriginalImage.Width, OriginalImage.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            Bitmap destImage = new Bitmap(OriginalImage.Width, OriginalImage.Height);

            BitmapData destData = destImage.LockBits(new Rectangle(0, 0, destImage.Width, destImage.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int stride = srcData.Stride;
            IntPtr srcScan0 = srcData.Scan0;
            IntPtr destScan0 = destData.Scan0;
            var freq = new int[256];
            unsafe
            {
                byte* src = (byte*)srcScan0;
                for (int y = 0; y < OriginalImage.Height; ++y)
                {
                    for (int x = 0; x < OriginalImage.Width; ++x)
                    {
                        ++freq[src[y * stride + x * 4]];
                    }
                }

                int numPixels = OriginalImage.Width * OriginalImage.Height;
                int minI = 0;
                var blackPixels = numPixels * blackPointPercent;
                int accum = 0;

                while (minI < 255)
                {
                    accum += freq[minI];
                    if (accum > blackPixels) break;
                    ++minI;
                }

                int maxI = 255;
                var whitePixels = numPixels * whitePointPercent;
                accum = 0;

                while (maxI > 0)
                {
                    accum += freq[maxI];
                    if (accum > whitePixels) break;
                    --maxI;
                }
                double spread = 255d / (maxI - minI);
                byte* dst = (byte*)destScan0;
                for (int y = 0; y < OriginalImage.Height; ++y)
                {
                    for (int x = 0; x < OriginalImage.Width; ++x)
                    {
                        int i = y * stride + x * 4;

                        byte val = (byte)Clamp(Math.Round((src[i] - minI) * spread), 0, 255);
                        dst[i] = val;
                        dst[i + 1] = val;
                        dst[i + 2] = val;
                        dst[i + 3] = 255;
                    }
                }
                OriginalImage.UnlockBits(srcData);
                destImage.UnlockBits(destData);
                pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox3.Image = destImage;
                Bitmap bmpb = new Bitmap(destImage);
                int[] histogram_r = new int[256];
                float max = 0;

                for (int i = 0; i < bmpb.Width; i++)
                {
                    for (int j = 0; j < bmpb.Height; j++)
                    {
                        int redValue = bmpb.GetPixel(i, j).R;
                        histogram_r[redValue]++;
                        if (max < histogram_r[redValue])
                            max = histogram_r[redValue];
                    }
                }

                int histHeight = 232;
                Bitmap imga = new Bitmap(256, histHeight + 10);
                using (Graphics g = Graphics.FromImage(imga))
                {
                    for (int i = 0; i < histogram_r.Length; i++)
                    {
                        float pct = histogram_r[i] / max;   // What percentage of the max is this value?
                        g.DrawLine(Pens.Black,
                            new Point(i, imga.Height - 5),
                            new Point(i, imga.Height - 5 - (int)(pct * histHeight))  // Use that percentage of the height
                            );
                    }
                }
                pictureBox4.Image = imga;
            }
        }
    }
}
