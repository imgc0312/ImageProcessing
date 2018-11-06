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
