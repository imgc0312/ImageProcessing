using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW1_PCXreader
{
    public class MyFilter
    {
        public enum BorderMethod { NULL, ZERO, NEAR };
        public enum GradientOperator { SOBEL, PREWITT };
        public enum GradientDirect { X, Y, BOTH };
        public delegate byte[] FilterCount(BitmapData data, int x, int y, BorderMethod borderMethod, MyFilter filter);
        public class MyFilterData
        {
            private int size{
                get {
                    return _data.getLength(0);
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

            public int fill(BitmapData srx, int x, int y, BorderMethod borderMethod, MyFilter filter)
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
                        _data[i,j] = getPixel(srx, startX + i, startY + j, borderMethod, filter[i,j]);
                        if (_data[i, j] != null)
                            pixels++;
                    }
                }
                return pixels;
            }

            public byte[] count(double rate)
            {// sigma each pixel in _data (rate*pixel)
                byte[] output = new byte[3];
                double[] temp = new double[3];
                foreach(double[] pixel in _data)
                {
                    if(pixel != null)
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
                catch (OverflowException e)
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
                catch (OverflowException e)
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
                catch (OverflowException e)
                {
                    output[2] = 255;
                }
                return output;
            }
            
            public static MyFilterData add(MyFilterData left, MyFilterData right){
                MyFilterData newFilterData = new MyFilterData(left.size)
                    //deal.....
            }

        }

        /// <summary>
        /// member->
        /// </summary>

        int _size;
        public int size
        {
            get
            {
                return _size;
            }
        }
        double[,] data;
        
        //gradient operator MyFilter
        private static MyFilter _SOBEL_X = null;
        private static MyFilter _SOBEL_Y = null;
        private static MyFilter _PREWITT_X = null;
        private static MyFilter _PREWITT_Y = null;
        
        /// <summary>
        /// function-->
        /// </summary>

        public MyFilter()
        {
            _size = 0;
            data = new double[size , size];
        }

        public MyFilter(int size)
        {
            _size = size;
            data = new double[size , size];
        }

        public MyFilter(int size, double value)
        {
            _size = size;
            data = new double[size, size];
            setData(value);
        }

        public double this[int x, int y]
        {
            get
            {
                return data[x, y];
            }
            set
            {
                data[x, y] = value;
            }
        }

        public void setData(double value)
        {// set all
            for (int j = 0; j < size; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    try
                    {
                        data[i, j] = value;
                    }
                    catch
                    {
                        ;
                    }
                }
            }
        }

        public void setData(int startX, int startY, int endX, int endY, double value)
        {//set range
            for(int j = startY; j <= endY; j++)
            {
                for(int i = startX; i <= endX; i++)
                {
                    try
                    {
                        data[i, j] = value;
                    }
                    catch
                    {
                        ;
                    }
                }
            }
        }

        private static byte[] getPixel(BitmapData data, int x, int y, BorderMethod method)
        {
            byte[] output = new byte[3];
            switch (method)
            {
                case BorderMethod.NULL:
                    if (x < 0 || x >= data.Width || y < 0 || y >= data.Height)
                    {
                        return null;
                    }
                    else
                    {
                        unsafe
                        {
                            byte* target = (byte*)data.Scan0;
                            target += y * data.Stride + x * 3;
                            output[0] = target[0];
                            output[1] = target[1];
                            output[2] = target[2];
                        }
                        return output;
                    }
                case BorderMethod.ZERO:
                    if (x < 0 || x >= data.Width || y < 0 || y >= data.Height)
                    {
                        return new byte[3]{ 0, 0, 0};
                    }
                    else
                    {
                        unsafe
                        {
                            byte* target = (byte*)data.Scan0;
                            target += y * data.Stride + x * 3;
                            output[0] = target[0];
                            output[1] = target[1];
                            output[2] = target[2];
                        }
                        return output;
                    }
                case BorderMethod.NEAR:
                    if (x < 0)
                        x = 0;
                    else if (x >= data.Width)
                        x = data.Width - 1;
                    if (y < 0)
                        y = 0;
                    else if (y >= data.Height)
                        y = data.Height - 1;
                    unsafe
                    {
                        byte* target = (byte*)data.Scan0;
                        target += y * data.Stride + x * 3;
                        output[0] = target[0];
                        output[1] = target[1];
                        output[2] = target[2];
                    }
                    return output;
            }
            return output;
        }

        private static double[] getPixel(BitmapData data, int x, int y, BorderMethod method, double rate)
        {
            double[] output = new double[3];
            switch (method)
            {
                case BorderMethod.NULL:
                    if (x < 0 || x >= data.Width || y < 0 || y >= data.Height)
                    {
                        return null;
                    }
                    else
                    {
                        unsafe
                        {
                            byte* target = (byte*)data.Scan0;
                            target += y * data.Stride + x * 3;
                            output[0] = rate * target[0];
                            output[1] = rate * target[1];
                            output[2] = rate * target[2];
                        }
                        return output;
                    }
                case BorderMethod.ZERO:
                    if (x < 0 || x >= data.Width || y < 0 || y >= data.Height)
                    {
                        return new double[3] { 0, 0, 0 };
                    }
                    else
                    {
                        unsafe
                        {
                            byte* target = (byte*)data.Scan0;
                            target += y * data.Stride + x * 3;
                            output[0] = rate * target[0];
                            output[1] = rate * target[1];
                            output[2] = rate * target[2];
                        }
                        return output;
                    }
                case BorderMethod.NEAR:
                    if (x < 0)
                        x = 0;
                    else if (x >= data.Width)
                        x = data.Width - 1;
                    if (y < 0)
                        y = 0;
                    else if (y >= data.Height)
                        y = data.Height - 1;
                    unsafe
                    {
                        byte* target = (byte*)data.Scan0;
                        target += y * data.Stride + x * 3;
                        output[0] = rate * target[0];
                        output[1] = rate * target[1];
                        output[2] = rate * target[2];
                    }
                    return output;
            }
            return output;
        }

        public static byte[] meanBlur(BitmapData data, int x, int y, BorderMethod borderMethod, MyFilter filter)
        {//this is a FilterCount, Mean Blur
            byte[] output = new byte[3];
            int size = filter.size;
            if(size <= 1)
            {
                return getPixel(data, x, y, borderMethod);
            }
            else
            {
                MyFilterData kernel = new MyFilterData();
                int pixels = 0;
                filter.setData(1.0);
                pixels = kernel.fill(data, x, y, borderMethod, filter);
                if (pixels <= 0)
                    throw new DivideByZeroException("blur pixel size 0");
                return kernel.count(1.0 / pixels);
            }
        }

        public static byte[] medianBlur(BitmapData data, int x, int y, BorderMethod borderMethod, MyFilter filter)
        {//this is a FilterCount, Mean Blur
            byte[] output = new byte[3];
            int size = filter.size;
            if (size <= 1)
            {
                return getPixel(data, x, y, borderMethod);
            }
            else
            {
                MyFilterData kernel = new MyFilterData();
                int pixels = 0;
                filter.setData(1.0);
                pixels = kernel.fill(data, x, y, borderMethod, filter);
                List<double>[] sortList = kernel.sort();
                double[] pixel = new double[3];
                pixel[0] = sortList[0].ElementAt<double>(sortList[0].Count/2);
                pixel[1] = sortList[1].ElementAt<double>(sortList[1].Count / 2);
                pixel[2] = sortList[2].ElementAt<double>(sortList[2].Count / 2);
                return MyFilterData.boundPixel(pixel);
            }
        }

        public static byte[] highBoost(BitmapData data, int x, int y, BorderMethod borderMethod, MyFilter filter)
        {//this is a FilterCount, Mean Blur
            byte[] output = new byte[3];
            int size = filter.size;
            if (size <= 1)
            {
                return getPixel(data, x, y, borderMethod);
            }
            else
            {
                MyFilterData kernel = new MyFilterData();
                
                kernel.fill(data, x, y, borderMethod, filter);

                double weightSize = 0;
                int targetPoint = filter.size / 2;
                for (int j = 0; j < filter.size; j++)
                {
                    for (int i = 0; i < filter.size; i++)
                    {
                        if((i != targetPoint) || (j != targetPoint))//ignore kernel target
                            weightSize += filter[i, j];
                    }
                }
                weightSize = 1 - weightSize;
                if (weightSize <= 0)
                    throw new DivideByZeroException("high boost weightSize 0");
                return kernel.countAbs(1.0 / weightSize);
            }
        }
        
        public static MyFilter.FilterCount gradient(MyFilter.GradientOperator operator, MyFiltr.GradientDirect direct){
            switch (operator)
            {
                case MyFilter.GradientOperator.SOBEL:
                    switch (direct)
                    {
                        case MyFilter.GradientDirect.X:
                            return kernel;
                        case MyFilter.GradientDirect.Y:
                            return kernel;
                        case MyFilter.GradientDirect.BOTH:
                            return ;
                    }
                case MyFilter.GradientOperator.PREWITT:
                default:
                    switch (direct)
                    {
                        case MyFilter.GradientDirect.X:
                            return kernel;
                        case MyFilter.GradientDirect.Y:
                            return kernel;
                        case MyFilter.GradientDirect.BOTH:
                            return ;
                    }
            }
        }
        
        private static byte[] SOBEL_X(BitmapData data, int x, int y, BorderMethod borderMethod, MyFilter filter)
        {//SOBEL X , not use filter
            byte[] output = new byte[3];
            MyFilterData kernel = new MyFilterData();
            kernel.fill(data, x, y, borderMethod, MyFilter.GradientKernel(MyFilter.GradientOperator.SOBEL, MyFilter.GradientDirect.X));
            return kernel.count(1.0);
        }
        
        private static byte[] SOBEL_Y(BitmapData data, int x, int y, BorderMethod borderMethod, MyFilter filter)
        {//SOBEL Y , not use filter
            byte[] output = new byte[3];
            MyFilterData kernel = new MyFilterData();
            kernel.fill(data, x, y, borderMethod, MyFilter.GradientKernel(MyFilter.GradientOperator.SOBEL, MyFilter.GradientDirect.Y));
            return kernel.count(1.0);
        }
        
        private static byte[] SOBEL_BOTH(BitmapData data, int x, int y, BorderMethod borderMethod, MyFilter filter)
        {//SOBEL Y , not use filter
            byte[] output = new byte[3];
            MyFilterData kernel1 = new MyFilterData();
            MyFilterData kernel2 = new MyFilterData();
            kernel1.fill(data, x, y, borderMethod, MyFilter.GradientKernel(MyFilter.GradientOperator.SOBEL, MyFilter.GradientDirect.X));
            kernel2.fill(data, x, y, borderMethod, MyFilter.GradientKernel(MyFilter.GradientOperator.SOBEL, MyFilter.GradientDirect.Y));
            kernel1.
            return kernel.count(1.0);
        }

        /// <summary>
        /// val-->
        /// </summary>
        public static MyFilter HighPassKernel(int option)
        {
            MyFilter highPass = new MyFilter(3);
            switch (option)
            {
                case 2:
                    highPass.setData(1);
                    highPass[1, 0] = -2;
                    highPass[0, 1] = -2;
                    highPass[1, 1] = 5;
                    highPass[2, 1] = -2;
                    highPass[1, 2] = -2;
                    return highPass;
                case 1:
                    highPass.setData(-1);
                    highPass[1, 1] = 9;
                    return highPass;
                case 0:
                default:
                    highPass[1, 0] = -1;
                    highPass[0, 1] = -1;
                    highPass[1, 1] = 5;
                    highPass[2, 1] = -1;
                    highPass[1, 2] = -1;
                    return highPass;
            }
        }
        
        public static MyFilter GradientKernel(GradientOperator operator, GradientDirect direct)
        {// please not use MyFilter.GradientDirect.BOTH
            MyFilter kernel = new MyFilter(3);
            switch (operator)
            {
                case MyFilter.GradientOperator.SOBEL:
                    switch (direct)
                    {
                        case MyFilter.GradientDirect.X:
                            if (_SOBEL_X == null){
                                kernel[0, 0] = 1;
                                kernel[2, 0] = -1;
                                kernel[0, 1] = 2;
                                kernel[2, 1] = -2;
                                kernel[0, 2] = 1;
                                kernel[2, 2] = -1;
                                _SOBEL_X = kernel;
                                return kernel;
                            }
                            else
                                return _SOBEL_X;
                        case MyFilter.GradientDirect.Y:
                            if (_SOBEL_Y == null){
                                kernel[0, 0] = 1;
                                kernel[1, 0] = 2;
                                kernel[2, 0] = 1;
                                kernel[0, 2] = -1;
                                kernel[1, 2] = -2;
                                kernel[2, 2] = -1;
                                _SOBEL_Y = kernel;
                                return kernel;
                            }
                            else
                                return _SOBEL_Y;
                    }
                case MyFilter.GradientOperator.PREWITT:
                    switch (direct)
                    {
                        case MyFilter.GradientDirect.X:
                            if (_PREWITT_X == null){
                                kernel[0, 0] = 1;
                                kernel[2, 0] = -1;
                                kernel[0, 1] = 1;
                                kernel[2, 1] = -1;
                                kernel[0, 2] = 1;
                                kernel[2, 2] = -1;
                                _PREWITT_X = kernel;
                                return kernel;
                            }
                            else
                                return _PREWITT_X;
                        case MyFilter.GradientDirect.Y:
                            if (_PREWITT_Y == null){
                                kernel[0, 0] = 1;
                                kernel[1, 0] = 1;
                                kernel[2, 0] = 1;
                                kernel[0, 2] = -1;
                                kernel[1, 2] = -1;
                                kernel[2, 2] = -1;
                                _PREWITT_Y = kernel;
                                return kernel;
                            }
                            else
                                return _PREWITT_Y;
                    }
                default:
                    return kernel;
            }
        }

        /// <summary>
        /// operate-->
        /// </summary>
        public static MyFilter add(MyFilter left, MyFilter right)
        {
            MyFilter newFilter = new MyFilter(left.size);
            for(int j = 0; j < newFilter.size; j++)
            {
                for(int i = 0; i < newFilter.size; i++)
                {
                    newFilter[i, j] = left[i, j] + right[i, j];
                }
            }
            return newFilter;
        }
    }

}
