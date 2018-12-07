using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HW2_video
{
    public class MyTiff
    {
        public List<Image> views = new List<Image>(0);
        public int Size
        {
            get
            {
                return views.Count;
            }
        }
        int _curr = 0;
        public int Next
        {
            get {
                _curr++;
                if (_curr >= views.Count)
                    return (_curr = (views.Count - 1));
                else
                    return _curr;
            }
        }
        public int Back
        {
            get
            {
                _curr--;
                if (_curr < 0)
                    return (_curr = 0);
                else
                    return _curr;
            }
        }
        public int Current
        {
            get { return _curr; }
            set {
                _curr = value;
                if (Size != 0)
                    _curr %= Size;
            }
        }
        public int CurrState
        {// 0 : start , 1 : current , 2 : end
            get
            {
                if (_curr == 0)
                    return 0;
                else if (_curr == (views.Count - 1))
                    return 2;
                else
                    return 1;
            }
        }

        public Image View
        {
            get
            {
                return views.ElementAt(_curr);
            }
        }

        public Image NextView
        {
            get
            {
                if ((_curr + 1) >= views.Count)
                    return views.ElementAt(views.Count - 1);
                else
                    return views.ElementAt(_curr + 1);
            }
        }

        public Image BackView
        {
            get
            {
                if ((_curr - 1) < 0)
                    return views.ElementAt(0);
                else
                    return views.ElementAt(_curr - 1);
            }
        }

        public Image this[int cur]
        {
            get
            {
                _curr = cur;
                if (Size != 0)
                {
                    _curr %= Size;
                    return views.ElementAt(_curr);
                }
                else
                    return null;
            }
        }
        string _filePath = "";
        string _fileName = "";
        string fileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                if (value == null)
                {
                    _fileName = "";
                    _filePath = "";
                }
                string[] fileAr = value.Split('\\');
                _fileName = fileAr[fileAr.Length - 1].Replace(".01.tiff", null);
                _fileName = _fileName.Replace("01.512.tiff", null);
                _filePath = value.Replace(fileAr[fileAr.Length - 1], null);
            }
        }




        public MyTiff()
        {
            ;
        }

        public MyTiff(Image view)
        {
            fileName = "<no name>";
            views.Add(view);
        }

        public void from(string filePath)
        {
            fileName = filePath;
            openALL(_filePath, _fileName);
        }

        private void openALL(string root, string name)
        {
            views.Clear();
            foreach (string fname in System.IO.Directory.GetFiles(root))
            {
                if (fname.Contains(name))
                {
                    views.Add(open(fname));
                }
            }
        }

        private Image open(string name)
        {
            // Draw the Image
            Image image = Image.FromFile(name);
            return image;
        }
    }
    
}
