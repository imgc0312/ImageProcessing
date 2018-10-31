using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HW1_PCXreader
{
    class MyF
    {
        public static Rectangle bound(Bitmap src)
        {
            return new Rectangle(0, 0, src.Width, src.Height);
        }
    }
    /** 以下 not use now**/
    delegate void DealBGR<T>(ref byte B, ref byte G, ref byte R, ref T objects);

    class MyMat
    {
        public int lineByte { get { return width * 3; } }
        public int stride {
            get {
                if ((lineByte % 4) > 0)
                    return lineByte + 4 - (lineByte % 4);
                else
                    return lineByte;
            }
        }
        public int width { get; set; }
        public int height { get; set; }
        private Bitmap src;

        public MyMat()
        {
            width = 0;
            height = 0;
            src = null;
        }

        public MyMat(Bitmap img)
        {
            this.width = img.Width;
            this.height = img.Height;
            src = img;
        }

        public byte[] getPixel(int x, int y)
        {
            if(src == null)
            {
                throw new Exception("MyMat getPixel(): data null");
            }
            BitmapData data = src.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            byte[] output = new byte[3];
            if ((x < 0) || (x >= width) || (y < 0) || (y >= height))
            {
                throw new Exception("MyMat getPixel(): (x,y) out of bound");
            }
            unsafe
            {
                byte* ptr = (byte*)(data.Scan0);
                ptr += y * stride;
                ptr += x * 3;
                output[0] = ptr[0];
                output[1] = ptr[1];
                output[2] = ptr[2];
            }
            src.UnlockBits(data);
            return output;
        }

        public void setPixel(int x, int y, byte[] bgr)
        {
            if (src == null)
            {
                throw new Exception("MyMat setPixel(): data null");
            }
            if (bgr.Length != 3)
                throw new Exception("MyMat setPixel(): bgr.Length != 3");
            if((x < 0) || (x >= width) || (y < 0) || (y >= height))
            {
                throw new Exception("MyMat setPixel(): (x,y) out of bound");
            }
            BitmapData data = src.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* ptr = (byte*)(data.Scan0);
                ptr += y * stride;
                ptr += x * 3;
                ptr[0] = bgr[0];
                ptr[1] = bgr[1];
                ptr[2] = bgr[2];
            }
            src.UnlockBits(data);
        }

        public void runAll<T>(DealBGR<T> method, ref T objects)
        {
            if (src == null)
            {
                throw new Exception("MyMat runAll(): data null");
            }
            BitmapData data = src.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            unsafe
            {
                byte* ptr = (byte*)(data.Scan0);
                for (int i = 0; i < data.Height; i++)
                {
                    for (int j = 0; j < data.Width; j++)
                    {
                        // write the logic implementation here
                        method(ref ptr[0],ref ptr[1],ref ptr[2], ref objects);   
                        ptr += 3;
                    }
                    ptr += data.Stride - data.Width * 3;
                }
            }
            src.UnlockBits(data);
        }

        public byte[] getData()
        {
            if (src == null)
            {
                throw new Exception("MyMat runAll(): data null");
            }
            BitmapData data = src.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte[] output = new byte[width * height * 3];
            unsafe
            {
                byte* ptr = (byte*)(data.Scan0);
                int skipByte = data.Stride - data.Width * 3; ;
                for (int i = 0, cur = 0; i < data.Height; i++)
                {
                    for(int j = 0; j < (data.Width * 3); j++)
                    {
                        output[cur] = ptr[0];
                        cur++;
                        ptr++;
                    }

                    ptr += skipByte;
                }
            }
            src.UnlockBits(data);
            return output;
        }

    }

}
