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
    public unsafe struct PCXHeader
    {
        public byte manufacturer;
        public byte version;
        public byte encoding;
        public byte bitsPerPixel;
        public ushort Xmin, Ymin, Xmax, Ymax;
        public ushort Hdpi, Vdpi;
        public fixed byte colorMap[48];
        public byte reserved;
        public byte nPlanes;
        public ushort bytesPerLine;
        public ushort paletteInfo;
        public ushort hScreenSize;
        public ushort vScreenSize;
        public fixed byte filler[54];

    }

    public class MyPCX
    {
        static int palette256Space = 768;
        PCXHeader header = new PCXHeader();
        byte[] data ;
        byte[] colorPalette ;

        public MyPCX()
        {
            header.Xmax = 0;
            header.Xmin = 0;
            header.Ymax = 0;
            header.Ymin = 0;
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
            int headerSize = Marshal.SizeOf(header);
            IntPtr buffer = Marshal.AllocHGlobal(headerSize);
            try
            {
                Marshal.Copy(bytes, byteSize, buffer, headerSize);
                Marshal.PtrToStructure(buffer, header); //set header
                if (header.version == 5)
                {
                    setBytesByBytes(data, bytes, headerSize, byteSize - palette256Space - headerSize);  //set palette 
                    setBytesByBytes(colorPalette, bytes, byteSize - palette256Space - 1, palette256Space);  //set data
                }
                else
                {
                    setBytesByBytes(data, bytes, headerSize, byteSize - headerSize);    //set data
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        void setBytesByBytes(byte[] target, byte[] srcBytes, int StartIndex, int Size)
        {
            target = new byte[Size];
            IntPtr buffer = Marshal.AllocHGlobal(Size);
            try
            {
                Marshal.Copy(srcBytes, StartIndex, buffer, Size);
                Marshal.PtrToStructure(buffer, target);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public Bitmap getView()
        {
            Bitmap image = new Bitmap(200, 100);
            Graphics imageGraphics = Graphics.FromImage(image);
            //write pixel by imageGraphics
            return image;
        }

    }
}
