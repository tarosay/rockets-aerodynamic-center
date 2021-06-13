using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 空力中心を求める
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            DragDropMethod(e);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) { e.Effect = DragDropEffects.Copy; }
        }
        private void DragDropMethod(DragEventArgs e)
        {
            string[] filenames = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (filenames.Length == 0 || !File.Exists(filenames[0]))
            {
                return;
            }

            Image image = null;
            string filename = "";
            int width = 0;
            int height = 0;
            byte[] bitmap_Arrays = null;
            Bitmap bitmap = null;

            try
            {
                filename = filenames[0];

                bitmap = new Bitmap(filename);
                width = bitmap.Width;
                height = bitmap.Height;

                // ビットマップを bitmap_Arrays[] に読み込み
                BitmapData data = null;
                data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                bitmap_Arrays = new byte[width * height * 4];
                Marshal.Copy(data.Scan0, bitmap_Arrays, 0, bitmap_Arrays.Length);
                bitmap.UnlockBits(data);
                bitmap.Dispose();
                bitmap = null;

                //// ビットマップを設定
                //bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                //data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                //Marshal.Copy(bitmap_Arrays, 0, data.Scan0, bitmap_Arrays.Length);
                //bitmap.UnlockBits(data);

                //グリーン以外を黒く塗る
                byte b_blue = bitmap_Arrays[0];
                byte b_green = bitmap_Arrays[1];
                byte b_red = bitmap_Arrays[2];
                byte red = 0;
                byte green = 0;
                byte blue = 0;
                int pixTotal = 0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        blue = bitmap_Arrays[4 * (x + width * y)];
                        green = bitmap_Arrays[4 * (x + width * y) + 1];
                        red = bitmap_Arrays[4 * (x + width * y) + 2];

                        if (b_red != red || b_green != green || b_blue != blue)
                        {
                            bitmap_Arrays[4 * (x + width * y)] = 0;
                            bitmap_Arrays[4 * (x + width * y) + 1] = 0;
                            bitmap_Arrays[4 * (x + width * y) + 2] = 0;
                            pixTotal++;
                        }
                        bitmap_Arrays[4 * (x + width * y) + 3] = 0xff;
                    }
                }

                //// ビットマップを設定
                //bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                //data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                //Marshal.Copy(bitmap_Arrays, 0, data.Scan0, bitmap_Arrays.Length);
                //bitmap.UnlockBits(data);

                int pixCnt = 0;
                int pixCenter = pixTotal / 2;
                int xCentor = 0;
                int yCentor = 0;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        blue = bitmap_Arrays[4 * (x + width * y)];
                        green = bitmap_Arrays[4 * (x + width * y) + 1];
                        red = bitmap_Arrays[4 * (x + width * y) + 2];

                        if (red == 0 && green == 0 && blue == 0)
                        {
                            pixCnt++;
                            if (pixCnt >= pixCenter) {
                                xCentor = x;
                                yCentor = y;
                                break;
                            }
                        }
                    }
                    if (pixCnt >= pixCenter) { break; }
                }

                for (int y = 0; y < height; y++)
                {
                    blue = bitmap_Arrays[4 * (xCentor + width * y)];
                    green = bitmap_Arrays[4 * (xCentor + width * y) + 1];
                    red = bitmap_Arrays[4 * (xCentor + width * y) + 2];

                    if (red == 0 && green == 0 && blue == 0)
                    {
                        bitmap_Arrays[4 * (xCentor + width * y)] = 0xff;
                        bitmap_Arrays[4 * (xCentor + width * y) + 1] = 0xff;
                        bitmap_Arrays[4 * (xCentor + width * y) + 2] = 0xff;
                    }
                }

                // ビットマップを設定
                bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(bitmap_Arrays, 0, data.Scan0, bitmap_Arrays.Length);
                bitmap.UnlockBits(data);

                bitmap.Save(Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + ".jpg", ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
                return;
            }

            Image imgOld = pbxData.Image;
            pbxData.Image = (Image)bitmap.Clone();
            pbxData.Refresh();

            if (imgOld != null)
            {
                imgOld.Dispose();
                imgOld = null;
            }

            if (image != null)
            {
                image.Dispose();
                image = null;
            }

            if (bitmap != null)
            {
                bitmap.Dispose();
                bitmap = null;
            }
        }
    }
}
