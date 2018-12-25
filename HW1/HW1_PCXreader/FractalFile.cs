using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace HW1_PCXreader
{
    [Serializable]
    public class FractalFile
    {
        public FractalData[,] pairs;
        public FractalFile(int cols, int rows)
        {
            pairs = new FractalData[cols, rows];
        }

        public static void writeToFile(Stream fs, FractalFile myff)
        {// write myct to fs
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, myff);
        }

        public static FractalFile readFromFile(Stream fs)
        {// read myct from fs
            BinaryFormatter bf = new BinaryFormatter();
            return (FractalFile)bf.Deserialize(fs);
        }
    }

    [Serializable]
    public class FractalData{
        public int DX;
        public int DY;
        public int Dt;
        public double s;
        public double o;
        public FractalData(int x, int y, int t, double s, double o)
        {
            DX = x;
            DY = y;
            Dt = t;
            this.s = s;
            this.o = o;
        }
    }
}
