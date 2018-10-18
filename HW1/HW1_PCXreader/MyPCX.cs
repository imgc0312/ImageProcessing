using System;
using System.Collections.Generic;
using System.Linq;
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
        byte manufacturer;
        byte version;
        byte encoding;
        byte bitsPerPixel;
        ushort Xmin, Ymin, Xmax, Ymax;
        ushort Hdpi, Vdpi;
        fixed byte colorMap[48];
        byte reserved;
        byte nPlanes;
        ushort bytesPerLine;
        ushort paletteInfo;
        ushort hScreenSize;
        ushort vScreenSize;
        fixed byte filler[54];
    }

    public class MyPCX
    {
        PCXHeader header ;
        public MyPCX()
        {

        }
    }
}
