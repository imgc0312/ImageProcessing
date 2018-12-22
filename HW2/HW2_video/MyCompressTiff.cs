using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace HW2_video
{
    public class MyCompressTiffDefine
    {
        public enum TYPE { SUBSAMPLE, MOTION }
    }
    [Serializable]
    public class MyCompressTiff
    {
        public MyCompressTiffDefine.TYPE type;
        public List<Image> baseImg;//if no compress ref
        public List<MyMotionTiff> motionTiff;// compress
        public MyCompressTiff()
        {
            baseImg = new List<Image>(0);
            motionTiff = new List<MyMotionTiff>(0);
            type = MyCompressTiffDefine.TYPE.MOTION;
        }

        public static void writeToFile(Stream fs, MyCompressTiff myct)
        {// write myct to fs
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, myct);
        }

        public static MyCompressTiff readFromFile(Stream fs)
        {// read myct from fs
            BinaryFormatter bf = new BinaryFormatter();
            return (MyCompressTiff)bf.Deserialize(fs);
        }

        public static MyTiff decode(MyCompressTiff myct)
        {
            return decode(myct, null);
        }

        public static MyTiff decode(MyCompressTiff myct, MyDeal.ProgressMonitor monitor)
        {
            switch (myct.type)
            {
                case MyCompressTiffDefine.TYPE.MOTION:
                    return decodeMotion(myct, monitor);
            }
            return null;
        }

        private static MyTiff decodeMotion(MyCompressTiff myct, MyDeal.ProgressMonitor monitor)
        {
            MyTiff decodeTiff = new MyTiff();
            Bitmap baseImg = null;
            MyMotionTiff targetMotion;
            for (int i = 0; i < myct.baseImg.Count; i++)
            {
                if (myct.baseImg.ElementAt(i) != null)
                {
                    decodeTiff.views.Add(baseImg = new Bitmap(myct.baseImg.ElementAt(i)));
                }
                else
                {
                    targetMotion = myct.motionTiff.ElementAt(i);
                    baseImg = MyMotionTiff.decode(baseImg, targetMotion);
                    decodeTiff.views.Add(baseImg);
                }
                if (monitor != null)
                    monitor.OnValueChanged(new MyDeal.ValueEventArgs() { value = (double)i / myct.baseImg.Count });
            }
            return decodeTiff;
        }
    }
}
