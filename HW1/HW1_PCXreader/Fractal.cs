using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HW1_PCXreader
{
    public class Fractal
    {
        Form_FractalFile activeFrom = null;
        SaveFileDialog saver = null;
        static int blockSize = 8;//range block width
        static int blockDSize = blockSize * 2;
        static int Dstep = 2;//domain jump length
        BlockImage[,] R;//range
        BlockImage[,][] D;//domain
        FractalFile file = null;

        bool EncodeDraw = false;
        PictureBox viewer = null;
        Graphics VG = null;
        int matchSpeed = 3;
        Pen red = new Pen(Color.Red);
        Pen blue = new Pen(Color.Blue);
        ProgressMonitor monitor = null;
        delegate void formChange(bool b);

        public void connect(Form_FractalFile activeFrom, SaveFileDialog saver)
        {
            this.activeFrom = activeFrom;
            this.saver = saver;
        }

        public void connect(ProgressMonitor monitor)
        {
            this.monitor = monitor;
        }

        public void connect(PictureBox searchView)
        {
            if (searchView == null)
            {
                this.viewer = null;
                this.VG = null;
            }
            else
            {
                this.viewer = searchView;
                this.VG = Graphics.FromImage(searchView.Image);
            }
                
        }

        public void import(FractalFile file)
        {
            this.file = file;
        }

        void setEnable(bool b)
        {
            activeFrom.tableLayoutPanel1.Enabled = b;
        }

        public void encode(Bitmap src)
        {
            if (src == null)
                return;
            if (activeFrom != null)
                activeFrom.BeginInvoke(new formChange(setEnable), false);
            if (monitor != null)
                monitor.start();
            BitmapData srcData = src.LockBits(MyF.bound(src), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            Thread threadR = new Thread(new ThreadStart(new Action(() => {
                buildR(srcData);
            })));
            Thread threadD = new Thread(new ThreadStart(new Action(() => {
                buildD(srcData);
            })));
            threadD.Start();
            threadR.Start();
            threadR.Join();
            threadD.Join();
            src.UnlockBits(srcData);

            if (monitor != null)
                monitor.OnValueChanged(new ValueEventArgs() { value = 0.1 });

            findMatch(out file);
            if (monitor != null)
                monitor.fine();
            if (activeFrom != null)
                activeFrom.BeginInvoke(new formChange(saveFile), true);
        }

        public Bitmap decode(Bitmap src)
        {
            if (file == null)
                return null;
            if (src == null)
                return null;
            if (monitor != null)
                monitor.start();
            BitmapData srcData = src.LockBits(MyF.bound(src), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            buildD(srcData);
            src.UnlockBits(srcData);
            Bitmap output = map();
            if (monitor != null)
                monitor.fine();
            return output;
        }

        private void saveFile(bool noUse)
        {
            if (saver != null)
            {//save compress file
                if (saver.ShowDialog() == DialogResult.OK)
                {
                    // If the file name is not an empty string open it for saving.  
                    if (saver.FileName != "")
                    {
                        FileStream fs = (FileStream)saver.OpenFile();
                        FractalFile.writeToFile(fs, file);
                        fs.Close();
                    }
                }
            }
            activeFrom.tableLayoutPanel1.Enabled = true;
        }

        private void buildR(BitmapData srcData)
        {
            int Rcols = srcData.Width / blockSize;
            int Rrows = srcData.Height / blockSize;
            int centerOffset = blockSize / 2 ;
            R = new BlockImage[Rcols, Rrows];
            for (int j = 0; j < Rrows; j++)
            {
                for(int i = 0; i < Rcols; i++)
                {
                    R[i, j] = new BlockImage(blockSize);
                    R[i, j].fill(srcData, i * blockSize + centerOffset, j * blockSize + centerOffset, MyFilter.BorderMethod.NEAR);
                }
            }
        }

        private void buildD(BitmapData srcData)
        {
            int Dcols = (srcData.Width - blockDSize) / Dstep + 1;
            int Drows = (srcData.Height - blockDSize) / Dstep + 1;
            double Dcount = 0;
            double Dsize = Dcols * Drows;
            int centerOffset = blockSize;
            D = new BlockImage[Dcols, Drows][];
            for (int j = 0; j < Drows; j++)
            {
                for (int i = 0; i < Dcols; i++)
                {
                    Dcount++;
                    if (monitor != null)
                        monitor.OnValueChanged(new ValueEventArgs() { value = Dcount / Dsize });
                    BlockImage tD = new BlockImage(blockDSize);
                    tD.fill(srcData, i * blockDSize + centerOffset, j * blockDSize + centerOffset, MyFilter.BorderMethod.NEAR);
                    D[i, j] = new BlockImage[8];
                    D[i, j][0] = tD.subSample();
                    buildDT(D[i, j]);
                }
            }
        }

        private void buildDT(BlockImage[] d)
        {
            if (d[0] != null)
            {
                for (int t = 1; t < 8; t++)
                {
                    d[t] = BlockImage.T(d[0], t);
                }
            }
        }

        private void findMatch(out FractalFile file)
        {
            file = new FractalFile(R.GetLength(0), R.GetLength(1));
            double Rcount = 0;
            double Rsize = R.GetLength(1) * R.GetLength(0);
            for (int j = 0; j < R.GetLength(1); j++)
            {
                for(int i = 0; i < R.GetLength(0); i++)
                {
                    Rcount++;
                    if (monitor != null)
                        monitor.OnValueChanged(new ValueEventArgs() { value = Rcount / Rsize });
                    // refer R 
                    double md = Double.PositiveInfinity;
                    int x = 0, y = 0, t = 0;
                    double s = 1.0, o = 0;
                    for (int Dr = 0; Dr < D.GetLength(1); Dr++)
                    {
                        for (int Dc = 0; Dc < D.GetLength(0); Dc++)
                        {
                            if (EncodeDraw && (viewer != null) && (VG != null))
                            {
                                matchDraw(i, j, Dc, Dr);
                            }
                            //compare D
                            for (int tt = 0; tt < 8; tt++)
                            {
                                double ts, to;
                                double distance = BlockImage.mse(R[i, j], D[Dc, Dr][tt], out ts, out to);
                                if (md > distance)
                                {
                                    md = distance;
                                    x = Dc;
                                    y = Dr;
                                    t = tt;
                                    s = ts;
                                    o = to;
                                }
                            }
                        }
                    }
                    file.pairs[i, j] = new FractalData(x, y, t, s, o);
                }
            }
        }

        private Bitmap map()
        {
            if (file == null)
                return null;
            FractalData[,] data = file.pairs;
            int cols = data.GetLength(0);
            int rows = data.GetLength(1);
            R = new BlockImage[cols, rows];
            double Rcount = 0;
            double progressSize = cols * rows * 2;// to let grogress% < 1/2
            for(int j = 0; j < rows; j++)
            {
                for(int i = 0; i < cols; i++)
                {
                    FractalData target = data[i, j];
                    R[i, j] = D[target.DX, target.DY][target.Dt].SO(target.s, target.o) ;

                    Rcount++;
                    if(monitor != null)
                    {
                        monitor.OnValueChanged(new ValueEventArgs() { value = Rcount / progressSize });
                    }
                }
            }

            return generate();
        }

        private Bitmap generate()
        {
            if (R == null)
                return null;
            int cols = R.GetLength(0);
            int rows = R.GetLength(1);
            Bitmap dst = new Bitmap(cols * blockSize, rows * blockSize);
            BitmapData dstData = dst.LockBits(MyF.bound(dst), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            double Rcount = cols * rows - 1;// to let grogress% > 1/2
            double progressSize = cols * rows * 2;

            for (int j = 0; j < rows; j++)
            {
                for (int i = 0; i < cols; i++)
                {
                    Rcount++;
                    if (monitor != null)
                        monitor.OnValueChanged(new ValueEventArgs() { value = Rcount / progressSize });
                    R[i, j].draw(dstData, i * blockSize, j * blockSize);
                }
            }
            dst.UnlockBits(dstData);
            return dst; 
        }

        private void matchDraw(int Rx, int Ry, int Dx, int Dy)
        {
            VG.Clear(Color.Transparent);
            VG.DrawRectangle(red, Rx * blockSize, Ry * blockSize, blockSize, blockSize);
            VG.DrawRectangle(blue, Dx * Dstep, Dy * Dstep, blockDSize, blockDSize);
            viewer.Invalidate();
            Thread.Sleep(matchSpeed);
        }
    }

    public class BlockImage
    {//僅支援gray
        double[,] _pixels;
        public int Size { get { return _pixels.GetLength(0); } }
        public double this[int x, int y] { get { return _pixels[x, y]; } set { _pixels[x, y] = value; } }
        private static double[] MSE_S = { 0.2, 0.5, 0.8, 1.0, 1.2 };

        public BlockImage(int size)
        {
            _pixels = new double[size, size];
        }

        private double getP( int x, int y, MyFilter.BorderMethod borderMethod)
        {
            switch (borderMethod)
            {
                case MyFilter.BorderMethod.NULL:
                case MyFilter.BorderMethod.ZERO:
                    if (x < 0 || x >= Size || y < 0 || y >= Size)
                    {
                        return 0;
                    }
                    else
                    {
                        return _pixels[x, y];
                    }
                case MyFilter.BorderMethod.NEAR:
                    if (x < 0)
                        x = 0;
                    else if (x >= Size)
                        x = Size - 1;
                    if (y < 0)
                        y = 0;
                    else if (y >= Size)
                        y = Size - 1;
                    return _pixels[x, y];
            }
            return 0;
        }

        public void fill(BitmapData src, int x, int y, MyFilter.BorderMethod borderMethod)
        {
            int startX = x - Size / 2;
            int startY = y - Size / 2;
            for (int j = 0; j < Size; j++)
            {
                for (int i = 0; i < Size; i++)
                {
                    _pixels[i, j] = MyFilter.getPixel(src, startX + i, startY + j, borderMethod, 1.0)[0];
                }
            }
        }

        public void draw(BitmapData src, int startX, int startY)
        {
            double[] pixel = new double[3];
            for (int j = 0; j < Size; j++)
            {
                for (int i = 0; i < Size; i++)
                {
                    for(int c = 0; c < 3; c++)
                    {
                        pixel[c] = _pixels[i, j];
                    }
                    MyFilter.setPixel(src, startX + i, startY + j, pixel);
                }
            }
        }

        public double avg()
        {
            double output = 0;
            for (int j = 0; j < Size; j++)
            {
                for (int i = 0; i < Size; i++)
                {
                    output += _pixels[i, j];
                }
            }
            output /= (Size * Size);
            return output;
        }

        public BlockImage subSample()
        {//縮減至 1/4 大小
            int subSize = Size / 2;
            BlockImage blockS = new BlockImage(subSize);
            
            for (int j = 0; j < subSize; j++)
            {
                for (int i = 0; i < subSize; i++)
                {
                    int Tx = 2 * i;
                    int Ty = 2 * j;
                    blockS[i, j] = this.getP(Tx, Ty, MyFilter.BorderMethod.NEAR);
                    blockS[i, j] += this.getP(Tx + 1, Ty, MyFilter.BorderMethod.NEAR);
                    blockS[i, j] += this.getP(Tx, Ty + 1, MyFilter.BorderMethod.NEAR);
                    blockS[i, j] += this.getP(Tx + 1, Ty + 1, MyFilter.BorderMethod.NEAR);
                    blockS[i, j] /= 4;
                }
            }
            return blockS;
        }

        public BlockImage SO(double s, double o)
        {//生成新亮度block
            BlockImage nBlock = new BlockImage(Size);

            for (int j = 0; j < Size; j++)
            {
                for (int i = 0; i < Size; i++)
                {
                    nBlock[i, j] = _pixels[i, j] * s + o;
                }
            }
            return nBlock;
        }

        public static double mse(BlockImage R, BlockImage D, out double s, out double o)
        {//return the min compare rate & s & o
            double distance = Double.PositiveInfinity;
            double td, ts, to;
            s = 1.0;
            o = 0;
            for (int cs = 0; cs < MSE_S.Length; cs++)
            {
                ts = MSE_S[cs];
                to = R.avg() - ts * D.avg();
                td = compare(R, D, ts, to);
                if(td < distance)
                {
                    distance = td;
                    s = ts;
                    o = to;
                }
            }
            return distance;
        }

        private static double compare(BlockImage R, BlockImage D, double s, double o)
        {
            double distance = 0;
            for(int j = 0; j < R.Size; j++)
            {
                for(int i = 0; i < R.Size; i++)
                {
                    distance += Math.Pow(R[i, j] - s * D[i, j] - o, 2);
                }
            }
            return distance;
        }

        public static BlockImage T(BlockImage src,int type)
        {
            ///type:
            /// 0 : no change
            /// 1 : rotate 90
            /// 2 : rotate 180
            /// 3 : rotate 270
            /// 4 : mirror 0
            /// 5 : mirror 90
            /// 6 : mirror 45
            /// 7 : mirror 135
            switch (type)
            {
                case 0:
                    return src;
                case 1://rotate 90
                    return T1(src);
                case 2://rotate 180
                    return T2(src);
                case 3://rotate 270
                    return T3(src);
                case 4://mirror 0
                    return T4(src);
                case 5://mirror 90
                    return T5(src);
                case 6://mirror 45
                    return T6(src);
                case 7://mirror 135
                    return T7(src);
            }
            return null;
        }

        private static BlockImage T1(BlockImage src)
        {//rotate 90
            int size = src.Size;
            BlockImage nBlock = new BlockImage(size);
            for(int j = 0; j < size; j++)
            {
                for(int i = 0; i < size; i++)
                {
                    nBlock[i, j] = src[ j, size - 1 - i];
                }
            }
            return nBlock;
        }

        private static BlockImage T2(BlockImage src)
        {//rotate 180
            int size = src.Size;
            BlockImage nBlock = new BlockImage(size);
            for (int j = 0; j < size; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    nBlock[i, j] = src[size - 1 - i, size - 1 - j];
                }
            }
            return nBlock;
        }

        private static BlockImage T3(BlockImage src)
        {//rotate 270
            int size = src.Size;
            BlockImage nBlock = new BlockImage(size);
            for (int j = 0; j < size; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    nBlock[i, j] = src[size - 1 - j, i];
                }
            }
            return nBlock;
        }

        private static BlockImage T4(BlockImage src)
        {//mirror 0
            int size = src.Size;
            BlockImage nBlock = new BlockImage(size);
            for (int j = 0; j < size; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    nBlock[i, j] = src[ i, size - 1 - j];
                }
            }
            return nBlock;
        }

        private static BlockImage T5(BlockImage src)
        {//mirror 90
            int size = src.Size;
            BlockImage nBlock = new BlockImage(size);
            for (int j = 0; j < size; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    nBlock[i, j] = src[size - 1 - i, j];
                }
            }
            return nBlock;
        }

        private static BlockImage T6(BlockImage src)
        {//mirror 45
            int size = src.Size;
            BlockImage nBlock = new BlockImage(size);
            for (int j = 0; j < size; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    nBlock[i, j] = src[size - 1 - j, size - 1 - i];
                }
            }
            return nBlock;
        }

        private static BlockImage T7(BlockImage src)
        {//mirror 135
            int size = src.Size;
            BlockImage nBlock = new BlockImage(size);
            for (int j = 0; j < size; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    nBlock[i, j] = src[ j, i];
                }
            }
            return nBlock;
        }
    }
}
