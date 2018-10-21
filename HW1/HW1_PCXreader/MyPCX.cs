using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HW1_PCXreader
{

    //public struct Pixel
    //{
    //    byte R;
    //    byte G;
    //    byte B;
    //    Pixel()
    //    {
    //        R = 0;
    //        G = 0;
    //        B = 0;
    //    }
    //}
    public struct PCXHeader
    {
        public byte manufacturer ;
        public byte version;
        public byte encoding;
        public byte bitsPerPixel;
        public ushort Xmin, Ymin, Xmax, Ymax;
        public ushort Hdpi, Vdpi;
        public byte[] colorMap ;
        public byte reserved;
        public byte nPlanes;
        public ushort bytesPerLine;
        public ushort paletteInfo;
        public ushort hScreenSize;
        public ushort vScreenSize;
        public byte[] filler;

        public bool set(byte[] src)
        {
            try
            {
                MyDeal.setTByBytes(ref manufacturer, src, 0);
                MyDeal.setTByBytes(ref version, src, 1);
                MyDeal.setTByBytes(ref encoding, src, 2);
                MyDeal.setTByBytes(ref bitsPerPixel, src, 3);
                MyDeal.setTByBytes(ref Xmin, src, 4);
                MyDeal.setTByBytes(ref Ymin, src, 6);
                MyDeal.setTByBytes(ref Xmax, src, 8);
                MyDeal.setTByBytes(ref Ymax, src, 10);
                MyDeal.setTByBytes(ref Hdpi, src, 12);
                MyDeal.setTByBytes(ref Vdpi, src, 14);
                MyDeal.setBytesByBytes(ref colorMap, src, 16, 48);
                MyDeal.setTByBytes(ref reserved, src, 64);
                MyDeal.setTByBytes(ref nPlanes, src, 65);
                MyDeal.setTByBytes(ref bytesPerLine, src, 66);
                MyDeal.setTByBytes(ref paletteInfo, src, 68);
                MyDeal.setTByBytes(ref hScreenSize, src, 70);
                MyDeal.setTByBytes(ref vScreenSize, src, 72);
                MyDeal.setBytesByBytes(ref filler, src, 74, 54);

            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                return false;
            }
            return true;
        }
        
    }

    public class MyPCX
    {
        static int palette256Space = 768;
        static int headerSize = 128;
        public PCXHeader header ;
        public int width { get { return header.Xmax - header.Xmin + 1;/*return header.bytesPerLine;*/ } }
        public int height { get { return header.Ymax - header.Ymin + 1; } }
        public byte[] data ;
        public byte[] colorPalette ;
        public Pen[] palette;
        public static int pleCols = 32;
        public static int pleRows = 256 / pleCols;
        public static int blockSize = 10;

        public MyPCX()
        {
            
        }

        public MyPCX(String filePath)
        {
            from(filePath);
        }

        public bool from(String filePath)
        {
            byte[] buffer = null;
            FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                BinaryReader br = new BinaryReader(file);
                int fileLen = System.Convert.ToInt32(file.Length);
                buffer = br.ReadBytes(fileLen);
                setAllByBytes(buffer, fileLen);
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                file.Close();
                return false;
            }
            finally
            {
                file.Close();
            }
            return true;

        }

        void setAllByBytes(byte[] bytes, int byteSize)
        {
            header.set(bytes);
            switch (header.nPlanes)
            {
                case 1:
                    MyDeal.setBytesByBytes(ref data, bytes, headerSize, byteSize - palette256Space - headerSize);  //set data
                    MyDeal.setBytesByBytes(ref colorPalette, bytes, byteSize - palette256Space, palette256Space);  //set palette 
                    setPalette();
                    break;
                case 3:
                    MyDeal.setBytesByBytes(ref data, bytes, headerSize, byteSize - headerSize);    //set data
                    colorPalette = null;
                    setPalette();
                    break;
                default:
                    MyDeal.setBytesByBytes(ref data, bytes, headerSize, byteSize - palette256Space - headerSize);  //set data
                    MyDeal.setBytesByBytes(ref colorPalette, bytes, byteSize - palette256Space, palette256Space);  //set palette 
                    setPalette();
                    break;
            }
        }

        private void setPalette()
        {
            if (colorPalette == null)
            {
                palette = null;
                return;
            }
            palette = new Pen[256];
            for(int index = 0; index < 256; index ++)
            {
                palette[index] = new Pen(Color.FromArgb(colorPalette[3 * index + 0], colorPalette[3 * index + 1], colorPalette[3 * index + 2]));
                //palette[index].Color = Color.FromArgb(colorPalette[3 * index + 0], colorPalette[3 * index + 1], colorPalette[3 * index + 2]);
            }
        }
        public Bitmap getView()
        {
            switch (header.nPlanes)
            {
                case 1:
                    return getView1();
                case 3:
                    return getView3();
                default:
                    return getView1();
            }
        }

        public Bitmap getView1()
        {
            Bitmap image = new Bitmap(width, height);
            Graphics imageGraphics = Graphics.FromImage(image);
            //write pixel by imageGraphics
            int colored = 0;
            int coloredLine = 0;
            if (data == null)
            {
                Debug.Print("data empty:getView()");
                return null;
            }
            if (palette == null)
            {
                Debug.Print("palette empty:getView()");
                return null;
            }
            for (int index = 0; index < data.Length; index++)
            {
                try
                {
                    if(colored >= width)
                    {
                        colored = 0;
                        coloredLine++;
                        index += header.bytesPerLine - width;//there has some null in per line
                    }

                    byte one = data[index];
                    if (one >= 0xc0) // 2 high bits set
                    {
                        byte two = data[++index];
                        int RL = (one & 0x3f);
                        imageGraphics.DrawRectangle(palette[two], colored % width, coloredLine, RL, 1);
                        colored += RL;
                    }
                    else
                    {
                        imageGraphics.DrawRectangle(palette[one], colored % width, coloredLine, 1, 1);
                        colored++;
                    }
                }
                catch (Exception e)
                {
                    Debug.Print("one or two empty:getView() " + e.ToString());
                    return image;
                }
                if (coloredLine >= height)
                    break;
            }
            return image;
        }

        public Bitmap getView3()
        {
            Bitmap image = new Bitmap(width, height);
            Graphics imageGraphics = Graphics.FromImage(image);
            //write pixel by imageGraphics
            int colored = 0;
            int coloredLine = 0;
            byte[] rowData = new byte[header.bytesPerLine * header.bytesPerLine];//there may has some null in per line end
            if (data == null)
            {
                Debug.Print("data empty:getView()");
                return null;
            }
            for (int index = 0; index < data.Length; index ++)
            {
                try
                {
                    byte one = data[index];
                    if (one >= 0xc0) // 2 high bits set
                    {
                        byte two = data[++index];
                        for(int i = (one & 0x3f); i > 0; i--)
                        {
                            rowData[colored] = two;
                            colored++;
                        }
                    }
                    else
                    {
                        rowData[colored] = one;
                        colored++;
                    }

                    if(colored >=  3 * header.bytesPerLine)
                    {
                        for(int i = 0; i < width; i++)
                        {
                            imageGraphics.DrawRectangle(new Pen(Color.FromArgb(rowData[i], rowData[i + header.bytesPerLine] , rowData[i + 2 * header.bytesPerLine])), i, coloredLine, 1, 1);
                        }
                        colored = 0;
                        coloredLine++;
                    }
                }
                catch (Exception e)
                {
                    Debug.Print("colored " + colored +" :getView() " + e.ToString());
                    return image;
                }
                if (coloredLine >= height)
                    break;
            }
            return image;
        }

        public Bitmap getPalette()
        {
            Bitmap image = new Bitmap(pleCols * blockSize, pleRows*blockSize);
            Graphics imageGraphics = Graphics.FromImage(image);
            //write pixel by imageGraphics
            if (palette == null)
            {
                if(header.nPlanes == 1) //need to use palette
                    Debug.Print("palette empty:getView()");
                return null;
            }
            for (int index = 0; index < 256; index++)
            {
                try
                {
                    //imageGraphics.DrawRectangle(palette[index], (index % pleCols) * blockSize, (index / pleCols) * blockSize, blockSize, blockSize);
                    imageGraphics.FillRectangle(palette[index].Brush, (index % pleCols) * blockSize, (index / pleCols) * blockSize, blockSize, blockSize);
                }
                catch (Exception e)
                {
                    Debug.Print("palette empty:getView() " + e.ToString());
                    return image;
                }
            }
            return image;
        }

    }

}
