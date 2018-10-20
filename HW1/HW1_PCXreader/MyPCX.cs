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
        public int width { get { return header.Xmax - header.Xmin + 1; } }
        public int height { get { return header.Ymax - header.Ymin + 1; } }
        public byte[] data ;
        public byte[] colorPalette ;
        public Pen[] palette;

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
            //if (header.version == 5)
            //{
            //    MyDeal.setBytesByBytes(data, bytes, headerSize, byteSize - palette256Space - headerSize); //set data  //set palette 
            //    MyDeal.setBytesByBytes(colorPalette, bytes, byteSize - palette256Space - 1, palette256Space);  //set palette 
            //}
            //else
            //{
            //    MyDeal.setBytesByBytes(data, bytes, headerSize, byteSize - headerSize);    //set data
            //}
            MyDeal.setBytesByBytes(ref data, bytes, headerSize, byteSize - palette256Space - headerSize);  //set data
            MyDeal.setBytesByBytes(ref colorPalette, bytes, byteSize - palette256Space , palette256Space);  //set palette 
            setPalette();
        }

        private void setPalette()
        {
            if (colorPalette == null)
                return;
            palette = new Pen[256];
            for(int index = 0; index < 256; index ++)
            {

                palette[index] = new Pen(Color.FromArgb(colorPalette[3 * index + 0], colorPalette[3 * index + 1], colorPalette[3 * index + 2]));
                //palette[index].Color = Color.FromArgb(colorPalette[3 * index + 0], colorPalette[3 * index + 1], colorPalette[3 * index + 2]);
            }
        }
        
        public Bitmap getView()
        {
            Bitmap image = new Bitmap(width, height);
            Graphics imageGraphics = Graphics.FromImage(image);
            //write pixel by imageGraphics
            int colored = 0;
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
                    byte one = data[index];
                    if (one >= 0xc0) // 2 high bits set
                    {
                        byte two = data[++index];
                        
                        for (int RL = (one & 0x3f); RL > 0;)
                        {
                            int L = width - (colored % width);
                            if (L < RL)
                            {
                                imageGraphics.DrawRectangle(palette[two], colored % width, colored / width, L, 1);
                                RL -= L;
                                colored += L;
                            }
                            else
                            {
                                imageGraphics.DrawRectangle(palette[two ], colored % width, colored / width, RL, 1);
                                colored += RL;
                                RL = 0;
                            }
                        }
                    }
                    else
                    {
                        imageGraphics.DrawRectangle(palette[one ], colored % width, colored / width, 1, 1);
                        colored++;
                    }
                }
                catch (Exception e)
                {
                    Debug.Print("one or two empty:getView() " + e.ToString());
                    return image;
                }
                if (colored >= width * height)
                    break;
            }
            return image;
        }

        public Bitmap getPalette()
        {
            int pleCols = 32;
            int pleRows = 256 / pleCols;
            int blockSize = 10;
            Bitmap image = new Bitmap(pleCols * blockSize, pleRows*blockSize);
            Graphics imageGraphics = Graphics.FromImage(image);
            //write pixel by imageGraphics
            if (palette == null)
            {
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
