using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace HiddenObj
{
    class ListedHO
    {
        public int NetworkId;
        public string Name;
        public int Duration;
        public System.Drawing.Color ObjColor;
        public int Range;
        public Vector3 Position;
        public float CreatedAt;

        public ListedHO(int NetID, string name, int duration, System.Drawing.Color objColor, int range, Vector3 position, float createdAt)
        {
            NetworkId = NetID;
            Name = name;
            Duration = duration;
            ObjColor = objColor;
            Range = range;
            Position = position;
            CreatedAt = createdAt;
        }
    }
}
