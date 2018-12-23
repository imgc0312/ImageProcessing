using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW2_video
{
    public class MyFilterData
    {
        public enum CRITERIA_METHOD { SQUARE, ABSOLUTE }// compress match criteria

        private int size
        {
            get
            {
                return _data.GetLength(0);
            }
        }
        private double[,][] _data;

        public MyFilterData()
        {
            _data = new double[0, 0][];
        }

        public MyFilterData(int size)
        {
            _data = new double[size, size][];
        }

        public double[] this[int x, int y]
        {
            get
            {
                return _data[x, y];
            }
            set
            {
                _data[x, y] = value;
            }
        }

        public int fill(BitmapData srx, int x, int y, MyFilter.BorderMethod borderMethod, MyFilter filter)
        {
            int size = filter.size;
            _data = new double[size, size][];
            int startX = x - size / 2;
            int startY = y - size / 2;
            int pixels = 0;
            for (int j = 0; j < size; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    _data[i, j] = MyFilter.getPixel(srx, startX + i, startY + j, borderMethod, filter[i, j]);
                    if (_data[i, j] != null)
                        pixels++;
                }
            }
            return pixels;
        }

        public void set(BitmapData src, int x, int y, MyFilter filter)
        {
            int size = filter.size;
            _data = new double[size, size][];
            int startX = x - size / 2;
            int startY = y - size / 2;
            for (int j = 0; j < size; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    MyFilter.setPixel(src, startX +i, startY + j, this[i, j], filter[i,j]);
                }
            }
            return ;
        }

        public byte[] count(double rate)
        {// sigma each pixel in _data (rate*pixel)
            byte[] output = new byte[3];
            double[] temp = new double[3];
            foreach (double[] pixel in _data)
            {
                if (pixel != null)
                {
                    temp[0] += pixel[0];
                    temp[1] += pixel[1];
                    temp[2] += pixel[2];
                }
            }
            temp[0] *= rate;
            temp[1] *= rate;
            temp[2] *= rate;

            output = MyFilterData.boundPixel(temp);
            return output;
        }

        public byte[] countAbs(double rate)
        {// sigma each pixel in _data (rate*pixel) && abs output
            byte[] output = new byte[3];
            double[] temp = new double[3];
            foreach (double[] pixel in _data)
            {
                if (pixel != null)
                {
                    temp[0] += pixel[0];
                    temp[1] += pixel[1];
                    temp[2] += pixel[2];
                }
            }

            temp[0] = Math.Abs(temp[0] * rate);
            temp[1] = Math.Abs(temp[1] * rate);
            temp[2] = Math.Abs(temp[2] * rate);
            output = MyFilterData.boundPixel(temp);
            return output;
        }

        public List<double>[] sort()
        {// return sort list
            List<double>[] sortData = new List<double>[3];

            int pixels = 0;
            foreach (double[] pixel in _data)
            {
                if (pixel != null)
                    pixels++;
            }

            sortData[0] = new List<double>(pixels);
            sortData[1] = new List<double>(pixels);
            sortData[2] = new List<double>(pixels);

            foreach (double[] pixel in _data)
            {
                if (pixel != null)
                {
                    sortData[0].Add(pixel[0]);
                    sortData[1].Add(pixel[1]);
                    sortData[2].Add(pixel[2]);
                }
            }

            sortData[0].Sort();
            sortData[1].Sort();
            sortData[2].Sort();
            return sortData;
        }

        public Bitmap transToBitmap()
        {
            Bitmap newBitmap = new Bitmap(size, size);
            BitmapData newData = newBitmap.LockBits(MyDeal.boundB(newBitmap), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* newPtr = (byte*)newData.Scan0;
                int skipByte = newData.Stride - 3 * newData.Width;
                for(int j = 0; j < newData.Height; j++)
                {
                    for(int i = 0; i < newData.Width; i++)
                    {
                        byte[] pixel = boundPixel(_data[i, j]);
                        newPtr[0] = pixel[0];
                        newPtr[1] = pixel[1];
                        newPtr[2] = pixel[2];
                        newPtr += 3;
                    }
                    newPtr += skipByte;
                }
            }
            newBitmap.UnlockBits(newData);
            return newBitmap;
        }

        public static byte[] boundPixel(double[] src)
        {// convert double to byte
            byte[] output = new byte[3];
            try
            {
                if (src[0] < 0)
                    output[0] = 0;
                else if (src[0] > 255)
                    output[0] = 255;
                else
                    output[0] = Convert.ToByte(src[0]);
            }
            catch (OverflowException)
            {
                output[0] = 255;
            }
            try
            {
                if (src[1] < 0)
                    output[1] = 0;
                else if (src[1] > 255)
                    output[1] = 255;
                else
                    output[1] = Convert.ToByte(src[1]);
            }
            catch (OverflowException)
            {
                output[1] = 255;
            }
            try
            {
                if (src[2] < 0)
                    output[2] = 0;
                else if (src[2] > 255)
                    output[2] = 255;
                else
                    output[2] = Convert.ToByte(src[2]);
            }
            catch (OverflowException)
            {
                output[2] = 255;
            }
            return output;
        }

        public static MyFilterData add(MyFilterData left, MyFilterData right)
        {
            MyFilterData newFilterData = new MyFilterData(left.size);
            for (int j = 0; j < newFilterData.size; j++)
            {
                for (int i = 0; i < newFilterData.size; i++)
                {
                    newFilterData[i, j] = new double[3];
                    newFilterData[i, j][0] = left[i, j][0] + right[i, j][0];
                    newFilterData[i, j][1] = left[i, j][1] + right[i, j][1];
                    newFilterData[i, j][2] = left[i, j][2] + right[i, j][2];
                }
            }
            return newFilterData;
        }

        public static double compare(MyFilterData left, MyFilterData right)
        {// return value high means the right is not like left
            double rate = 0.0;
            for (int j = 0; j < left.size; j++)
            {
                for (int i = 0; i < left.size; i++)
                {
                    rate += 
                        (Math.Pow(left[i, j][0] - right[i, j][0], 2) +
                        Math.Pow(left[i, j][1] - right[i, j][1], 2) +
                        Math.Pow(left[i, j][2] - right[i, j][2], 2)) / 3;
                }
            }
            return rate;
        }

        public static double compare(MyFilterData left, MyFilterData right, CRITERIA_METHOD criteria)
        {// return value high means the right is not like left
            double rate = 0.0;
            switch (criteria)
            {
                case CRITERIA_METHOD.ABSOLUTE:
                    for (int j = 0; j < left.size; j++)
                    {
                        for (int i = 0; i < left.size; i++)
                        {
                            rate +=
                                (Math.Abs(left[i, j][0] - right[i, j][0]) +
                                Math.Abs(left[i, j][1] - right[i, j][1]) +
                                Math.Abs(left[i, j][2] - right[i, j][2])) / 3;
                        }
                    }
                    break;
                case CRITERIA_METHOD.SQUARE:
                    for (int j = 0; j < left.size; j++)
                    {
                        for (int i = 0; i < left.size; i++)
                        {
                            rate +=
                                (Math.Pow(left[i, j][0] - right[i, j][0], 2) +
                                Math.Pow(left[i, j][1] - right[i, j][1], 2) +
                                Math.Pow(left[i, j][2] - right[i, j][2], 2)) / 3;
                        }
                    }
                    break;
            }
            
            return rate;
        }

    }

}
