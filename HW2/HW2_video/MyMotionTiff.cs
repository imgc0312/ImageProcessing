using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW2_video
{
    [Serializable]
    public class MyMotionTiff
    {
        public int Width { get { return motionData.GetLength(0); } }
        public int Height { get { return motionData.GetLength(1); } }
        private int[,][] motionData;
        public MyMotionTiff(int width, int height)
        {
            motionData = new int[width, height][];
        }
        public int[] this[int x, int y] {
            get
            {
                return motionData[x, y];
            }
            set
            {
                motionData[x, y] = new int[2];
                motionData[x, y][0] = value[0];
                motionData[x, y][1] = value[1];
            }
        }

        public static Bitmap decode(Bitmap baseImg, MyMotionTiff motion)
        {
            MyFilter filter = new MyFilter(MyCompresser.compressKernelSize);
            filter.setData(1);
            Bitmap newImg = new Bitmap(baseImg.Width, baseImg.Height);
            BitmapData baseData = baseImg.LockBits(MyDeal.boundB(baseImg), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData newData = newImg.LockBits(MyDeal.boundB(newImg), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            MyFilterData srcFilterData = new MyFilterData();
            for (int y = 0 + MyCompresser.compressKernelSize / 2; y < motion.Height; y += MyCompresser.compressKernelSize)
            {
                for (int x = 0 + MyCompresser.compressKernelSize / 2; x < motion.Width; x += MyCompresser.compressKernelSize)
                {
                    srcFilterData.fill(baseData, motion[x, y][0], motion[x, y][1], MyFilter.BorderMethod.ZERO, filter);
                    Debug.Print("the " + x + " , " + y + " from " + motion[x, y][0] + " , " + motion[x, y][1]);
                    unsafe
                    {
                        byte* dstPtr = (byte*)newData.Scan0;
                        int skipByte = newData.Stride - 3 * MyCompresser.compressKernelSize;
                        dstPtr += (y - MyCompresser.compressKernelSize / 2) * newData.Stride + (x - MyCompresser.compressKernelSize / 2) * 3;
                        for (int j = 0; j < MyCompresser.compressKernelSize; j++)
                        {
                            for(int i = 0; i < MyCompresser.compressKernelSize; i++)
                            {
                                byte[] result = MyFilterData.boundPixel(srcFilterData[i, j]);
                                //Debug.Print("Data get " + result[0] + " , " + result[1] + " , " + result[2]);
                                dstPtr[0] = result[0];
                                dstPtr[1] = result[1];
                                dstPtr[2] = result[2];
                                dstPtr += 3;
                            }
                            dstPtr += skipByte;
                        }
                    }
                }
            }
            baseImg.UnlockBits(baseData);
            newImg.UnlockBits(newData);
            return newImg;
        }

        public static Bitmap drawVector(MyMotionTiff motion)
        {
            if (motion == null)
                return null;
            MyFilter filter = new MyFilter(MyCompresser.compressKernelSize);
            filter.setData(1);
            Bitmap newImg = new Bitmap(motion.Width, motion.Height);
            Graphics grapic = Graphics.FromImage(newImg);
            int colorLowBound = 64;// for random color value lower bound
            int colorHighBound = 256 - colorLowBound;
            Color[] colors = new Color[motion.Width * motion.Height / MyCompresser.compressKernelSize / MyCompresser.compressKernelSize];
            Random random = new Random();
            int i = 0;//pen sequence
            //draw box
            for (int y = 0; y < motion.Height; y += MyCompresser.compressKernelSize)
            {
                for (int x = 0; x < motion.Width; x += MyCompresser.compressKernelSize)
                {
                    colors[i] = Color.FromArgb(colorLowBound + random.Next()% colorHighBound, colorLowBound + random.Next() % colorHighBound, colorLowBound + random.Next() % colorHighBound);
                    grapic.FillRectangle(new SolidBrush(colors[i]), x, y, MyCompresser.compressKernelSize, MyCompresser.compressKernelSize);
                    //grapic.DrawRectangle(new Pen(colors[i]), x , y, MyCompresser.compressKernelSize, MyCompresser.compressKernelSize);
                    i++;
                }
            }
            i = 0;//pen sequence
            //draw vector line
            for (int y = 0 + MyCompresser.compressKernelSize / 2; y < motion.Height; y += MyCompresser.compressKernelSize)
            {
                for (int x = 0 + MyCompresser.compressKernelSize / 2; x < motion.Width; x += MyCompresser.compressKernelSize)
                {
                    grapic.DrawLine(new Pen(Color.FromArgb(128, colors[i])), x, y, motion[x, y][0], motion[x, y][1]);
                    i++;
                }
            }

            return newImg;
        }
    }
}
