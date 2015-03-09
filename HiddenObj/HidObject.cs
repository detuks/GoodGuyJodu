using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HiddenObj
{
    class HidObject
    {
        public string Name;
        public string SkinName;
        public int Duration;
        public Color ObjColor;
        public int Range;

        public HidObject(string name,string skinName, int duration, Color objColor, int range)
        {
            Name = name;
            SkinName = skinName;

            Duration = duration;
            ObjColor = objColor;
            Range = range;
        }
    }
}
