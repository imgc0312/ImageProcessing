using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HW2_video
{
    public class MyDeal
    {
        private static double maxPSNR = 100.0;
        public static double PSNR(Image before, Image after)
        {// PSNR = 10 log( 255^2 / sigma(pixel difference) )
            //if one of arg is null, then return null
            //if args not equal Size return negativeInfinity
            //else return PSNR 
            double x = 0.0;
            if (before == null)
                return maxPSNR;
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

            if (x > maxPSNR)
                x = maxPSNR;
            return x;
        }

        public static Rectangle boundB(Bitmap src)
        {//get Bitmap rectangle
            return new Rectangle(0, 0, src.Width, src.Height);
        }


        /// <summary>
        /// other class-->
        /// </summary>

        // for progress
        public class ValueEventArgs : EventArgs //for progressbar
        {
            public double value { set; get; }
            public int percent { get { return Convert.ToInt32(value * 100); } }
        }

        public delegate void ValueChangedEventHandler(object sender, ValueEventArgs e);

        public class ProgressMonitor
        {
            private event ValueChangedEventHandler ValueChanged; //change event
            public double current = 0;
            public ProgressBar view { get; set; }
            //int tryTime = 0;
            public ProgressMonitor()
            {
                ValueChanged = new ValueChangedEventHandler(ValueChangeMethod);
                view = null;
            }

            public ProgressMonitor(ProgressBar view)
            {
                ValueChanged = new ValueChangedEventHandler(ValueChangeMethod);
                this.view = view;
            }

            public void start()
            {
                current = 0;
                if (view != null)
                {
                    view.Value = 0;
                    view.Visible = true;
                    //Debug.Print("progress : start" + tryTime);
                }
            }

            public void fine()
            {
                current = 1.0;
                if (view != null)
                {
                    view.Value = 100;
                    view.Visible = false;
                    //Debug.Print("progress : fine" + tryTime);
                    //tryTime++;
                }
            }

            public void OnValueChanged(ValueEventArgs e)
            {
                if (this.ValueChanged != null)
                {
                    this.ValueChanged(this, e);
                }
            }

            public void ValueChangeMethod(object sender, ValueEventArgs e)
            {
                current = e.value;
                if (view != null)
                {
                    view.Value = e.percent;
                    //Debug.Print("progress : " + view.Value);
                }
                //Debug.Print("progress : change");
            }
        }

    }
}
