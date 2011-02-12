using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Scale_TIFF
{
    public partial class Form1 : Form
    {
        private int pw = 1650, ph = 2400, ow = 800;

        public Form1()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private string dir;

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog(this) != DialogResult.OK) return;

            listBox1.Items.Clear();
            dir = folderBrowserDialog1.SelectedPath;
            var files = new List<string>(Directory.GetFiles(dir, "*.tif*"));
            files.Sort(new NumberStringComparer());
            foreach (var file in files)
                listBox1.Items.Add(Path.GetFileName(file));
        }

        private Bitmap GetBitmap(string file)
        {
            Bitmap img = null;
            try
            {
#if false
                using (var bmp = new Bitmap(file))
                {
                    img = new Bitmap(bmp.Width, bmp.Height);
                    using (var g = Graphics.FromImage(img))
                    {
                        g.DrawImage(bmp, 0, 0, img.Width, img.Height);
                        int x = (img.Width - pw) / 2, y = (img.Height - ph) / 2;
                        using (var pen = new Pen(Color.Red, 4))
                            g.DrawRectangle(pen, x, y, pw, ph);
                    }
                }
#else
                using (var bmp = new Bitmap(file))
                {
                    img = new Bitmap(pw, ph);
                    int px = (pw - bmp.Width) / 2, py = (ph - bmp.Height) / 2;
                    using (var g = Graphics.FromImage(img))
                    {
                        g.DrawImage(bmp, px, py, bmp.Width, bmp.Height);
                    }
                }
#endif
            }
            catch { }
            return img;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var file = listBox1.SelectedItem.ToString();
            var img = GetBitmap(Path.Combine(dir, file));
            if (img != null)
            {
                viewerBox1.Image = img;
                viewerBox1.Visible = true;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Value = 0;
            menuStrip1.Enabled = listBox1.Enabled = false;

            var args = new string[listBox1.Items.Count];
            for (int i = 0; i < listBox1.Items.Count; i++)
                args[i] = listBox1.Items[i].ToString();
            backgroundWorker1.RunWorkerAsync(args);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var path = Path.Combine(dir, "resized");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var args = e.Argument as string[];
            int p = 0;
            for (int i = 0; i < args.Length; i++)
            {
                if (backgroundWorker1.CancellationPending) break;

                int pp = i * 100 / args.Length;
                if (p != pp) backgroundWorker1.ReportProgress(p = pp);

                var file = args[i];
                var img1 = GetBitmap(Path.Combine(dir, file));
                if (img1 == null) continue;

                int oh = img1.Height * ow / img1.Width;
                using (var img2 = new Bitmap(ow, oh))
                {
                    using (var g = Graphics.FromImage(img2))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(img1, 0, 0, img2.Width, img2.Height);
                    }
                    img2.Save(Path.Combine(path, Path.ChangeExtension(file, ".jpg")), ImageFormat.Jpeg);
                }
                img1.Dispose();
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripProgressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripProgressBar1.Value = 0;
            menuStrip1.Enabled = listBox1.Enabled = true;
            if (e.Error != null)
            {
                MessageBox.Show(this, e.ToString(), Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            if (backgroundWorker1.IsBusy)
            {
                var r = MessageBox.Show(this,
                    "処理をキャンセルしますか？", Text,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r == DialogResult.Yes)
                    backgroundWorker1.CancelAsync();
                else
                    e.Cancel = true;
            }
        }
    }
}
