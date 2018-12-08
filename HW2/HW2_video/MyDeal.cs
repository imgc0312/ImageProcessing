using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW2_video
{
    class MyDeal
    {
        public static double PSNR(Image before, Image after)
        {// PSNR = 10 log( 255^2 / sigma(pixel difference) )
            //if one of arg is null, then return null
            //if args not equal Size return negativeInfinity
            //else return PSNR 
            double x = 0.0;
            if (before == null)
                return x;
            if (after == null)
                return x;
            Bitmap beforeBitmap = new Bitmap(before);
            Bitmap afterBitmap = new Bitmap(after);
            if (!boundB(beforeBitmap).Equals(boundB(afterBitmap)))
                return Double.NegativeInfinity;
            BitmapData beforeData = beforeBitmap.LockBits(boundB(beforeBitmap), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData afterData = afterBitmap.LockBits(boundB(afterBitmap), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int height = beforeData.Height;
            int width = beforeData.Width;
            int skipByte = beforeData.Stride - 3 * width;
            unsafe
            {
                byte* beforePtr = (byte*)beforeData.Scan0;
                byte* afterPtr = (byte*)afterData.Scan0;
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        x += Math.Pow(afterPtr[0] - beforePtr[0], 2);
                        x += Math.Pow(afterPtr[1] - beforePtr[1], 2);
                        x += Math.Pow(afterPtr[2] - beforePtr[2], 2);
                        beforePtr += 3;
                        afterPtr += 3;
                    }
                    beforePtr += skipByte;
                    afterPtr += skipByte;
                }
            }
            beforeBitmap.UnlockBits(beforeData);
            afterBitmap.UnlockBits(afterData);
            if (x == 0.0)
                x = Double.PositiveInfinity;
            else
                x = 10 * Math.Log10(255.0 * 255.0 * 3.0 * width * height / x);
            return x;
        }

        private static Rectangle boundB(Bitmap src)
        {//get Bitmap rectangle
            return new Rectangle(0, 0, src.Width, src.Height);
        }
    }
}
