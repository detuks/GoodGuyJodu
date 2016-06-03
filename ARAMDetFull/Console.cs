using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;

namespace ARAMDetFull
{
    class Console
    {
        public static void WriteLine(Object ex)
        {
            System.Console.WriteLine(ex);
            Game.PrintChat(ex.ToString());

        }
    }
}
