using System;
using System.Collections;
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

        public static byte[] boundP(double[] p)
        {
            if (p == null)
                return null;
            byte[] np = new byte[3];
            for(int i = 0; i < 3; i++)
            {
                double v = p[i];
                if (v < 0)
                    v = 0;
                else if (v > 255)
                    v = 255;
                np[i] = Convert.ToByte(v);
            }
            return np;
        }
    }

    class BitPlanes
    {
        public enum CodingMethod { Binary, GrayCode};
        public static byte[] BitMask = { (byte)0x01, (byte)0x02, (byte)0x04, (byte)0x08, (byte)0x10, (byte)0x20, (byte)0x40, (byte)0x80 };

        Bitmap[] bitPlanes;
        public BitPlanes()
        {
            bitPlanes = new Bitmap[8];
        }

        public BitPlanes(Bitmap src)
        {
            bitPlanes = getBitPlane(src, CodingMethod.Binary);
        }

        public void froms(Bitmap src, CodingMethod method)
        {
            bitPlanes = getBitPlane(src, method);
        }

        public Bitmap this[int i]
        {
            get
            {
                return bitPlanes[i];
            }
            set
            {
                bitPlanes[i] = value;
            }
        }

        private BitmapData lockBimap(Bitmap src, ImageLockMode mode)
        {
            if (src == null)
                return null;
            else
            {
                return src.LockBits(MyF.bound(src), mode, PixelFormat.Format24bppRgb);
            }
        }


        public Bitmap merge(CodingMethod method)
        {
            int width = 0;
            int height = 0;
            Bitmap dst = null;
            for(int k = 7; k >= 0; k--)
            {// try to find output size
                if (this[k] != null)
                {
                    width = Math.Max(width, this[k].Width);
                    height = Math.Max(height, this[k].Height);
                }
            }
            if ((width == 0) || (height == 0))
                return dst;
            dst = new Bitmap(width, height);
            BitmapData[] srcDatas = bitPlanes.ToArray().Select(x => lockBimap(x, ImageLockMode.ReadOnly)).ToArray();
            BitmapData dstData = dst.LockBits(MyF.bound(dst), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* dstPtr = (byte*)dstData.Scan0;
                int skipByte = dstData.Stride - dstData.Width*3;
                for(int j = 0;j < dstData.Height; j++)
                {
                    for(int i = 0; i < dstData.Width; i++)
                    {
                        byte srcByte = getByte(srcDatas, i, j);
                        switch (method)
                        {
                            case CodingMethod.GrayCode:
                                srcByte = gray2Bin(srcByte);
                                break;
                            case CodingMethod.Binary:
                            default:
                                break;
                        }
                        dstPtr[0] = srcByte;
                        dstPtr[1] = dstPtr[0];
                        dstPtr[2] = dstPtr[0];
                        dstPtr +=3;
                    }
                    dstPtr += skipByte;
                }
            }
            dst.UnlockBits(dstData);
            for(int i = 0; i < 8; i++)
            {
                if (bitPlanes[i] == null)
                    continue;
                bitPlanes[i].UnlockBits(srcDatas[i]);
            }
            return dst;
        }

        byte getByte(BitmapData[] bitPlanes, int x, int y)
        {
            byte output = 0;
            if (bitPlanes == null)
                return output;
            for(int k = 7; k >= 0; k--)
            {
                output = unchecked((byte)(output << 1));
                output += getBit(bitPlanes[k], x, y);
            }
            return output;
        }

        byte getBit(BitmapData src, int x, int y)
        {
            if (src == null)
                return 0;
            if ((x < 0) || (x >= src.Width) || (y < 0) || (y >= src.Height))
                return 0;
            unsafe
            {
                byte* srcPtr = (byte*)src.Scan0;
                if (srcPtr[y * src.Stride + x*3] > 0)
                    return 1;
                else
                    return 0;
            }
        }

        public static Bitmap[] getBitPlane(Bitmap src, CodingMethod method)
        {
            Bitmap[] planes = new Bitmap[8];
            if (src == null)
                return planes;
            for (int i = 0; i < 8; i++) {
                planes[i] = new Bitmap(src.Width, src.Height);
                //Debug.Print(BitMask[i].ToString() + " ");
            }
            BitmapData srcData = src.LockBits(MyF.bound(src), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData[] dstDatas = planes.ToArray().Select(x => x.LockBits(MyF.bound(x), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb)).ToArray();

            if(srcData == null)
            {
                Debug.Print("no srcData");
                return planes;
            }
            unsafe
            {
                int skipByte = srcData.Stride - srcData.Width*3 ;
                //Debug.Print(" " + skipByte + " " + srcData.Stride + " " + srcData.Width);
                //int skipBit = dstDatas[0].Stride - dstDatas[0].Width;
                byte* srcPtr = (byte*)srcData.Scan0;
                byte*[] dstPtrs = new byte*[8];
                for(int i = 0; i < 8; i++)
                {
                    dstPtrs[i] = (byte*)dstDatas[i].Scan0;
                }

                for (int j = 0; j < srcData.Height ; j++)
                {
                    for(int i = 0; i < srcData.Width; i++)
                    {
                        for (int k = 7; k >= 0; k--)
                        {
                            //Debug.Print(" " + i + " " + j + " " + k);
                            byte srcByte = srcPtr[0];
                            switch (method)
                            {
                                case CodingMethod.GrayCode:
                                    srcByte = bin2Gray(srcByte);
                                    break;
                                case CodingMethod.Binary:
                                default:
                                    break;
                            }
                            if ((BitMask[k] & srcByte) != 0)
                            {
                                byte* target = dstPtrs[k];
                                target[0] = 255;
                                target[1] = 255;
                                target[2] = 255;
                            }
                            
                            dstPtrs[k]+=3;
                        }
                        srcPtr+=3;
                    }
                    srcPtr += skipByte;
                    for (int k = 7; k >= 0; k--)
                        dstPtrs[k] += skipByte;
                }
            }
            src.UnlockBits(srcData);
            for (int i = 0; i < 8; i++)
            {
                planes[i].UnlockBits(dstDatas[i]);
            }
            return planes;
        }

        public static byte bin2Gray(byte src)
        {
            return (byte)(src ^ (src >> 1));
        }

        public static byte gray2Bin(byte src)
        {
            byte t = src;
            for(int i = 1; i < 8; i++)
            {
                t = (byte)(t ^ (t >> 1));
            }
            return t;
        }
    };

    class MyMat
    {
        public double[,] data;
        public double this[int row,int col]
        {
            get
            {
                return data[row, col];
            }
            set
            {
                data[row, col] = value;
            }
        }
        public int rows { get { return data.GetLength(0); } }
        public int cols { get { return data.GetLength(1); } }

        /// <summary>
        /// function-->
        /// </summary>

        public MyMat()
        {
            data = new double[0, 0];
        }
        public MyMat(int rows, int cols)
        {
            data = new double[rows, cols];
        }

        /// <summary>
        /// val-->
        /// </summary>
        public static MyMat NULL() { return new MyMat(); }
        public static MyMat ROTATE(int degrees)
        {
            double rad = Math.PI * degrees / 180.0;
            return ROTATE(rad);
        }
        public static MyMat ROTATE (double rad)
        {
            MyMat output = new MyMat(2, 2);
            output[0, 0] = Math.Cos(rad);
            output[1, 0] = Math.Sin(rad);
            output[0, 1] = -1.0 * output[1, 0];
            output[1, 1] = output[0, 0];
            return output;
        }
        public static MyMat MIRROR(int degrees)
        {
            double rad = Math.PI * degrees / 180.0;
            return MIRROR(rad);
        }
        public static MyMat MIRROR(double rad)
        {
            MyMat output = new MyMat(2, 2);
            output[0, 0] = Math.Cos(2 * rad);
            output[1, 0] = Math.Sin(2 * rad);
            output[0, 1] = output[1, 0];
            output[1, 1] = -1.0 * output[0, 0];
            return output;
        }

        /// <summary>
        /// operate-->
        /// </summary>
        public static MyMat mul(MyMat left, MyMat right)
        {
            if(left.cols == 0 || left.rows == 0 || right.cols == 0 || right.rows == 0)
            {
                Debug.Print(" MyMat mul : " + left.cols + " " + left.rows + " " + right.cols + " " + right.rows);
                return MyMat.NULL();
            }
            if(left.cols != right.rows)
            {
                Debug.Print(" MyMat mul : left.cols != right.rows");
                return MyMat.NULL();
            }
            MyMat output = new MyMat(left.rows, right.cols);
            for(int i = 0; i < output.rows; i++)
            {
                for(int j = 0; j < output.cols; j++)
                {
                    output[i, j] = 0;
                    for (int k = 0; k < left.cols; k++)
                    {
                        output[i, j] += left[i, k] * right[k, j];
                    }
                }
            }
            return output;
        }
    }

}
