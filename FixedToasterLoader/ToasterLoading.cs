using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Timers;
using LeagueSharp;

/*
    Copyright (C) 2014 Nikita Bernthaler

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

namespace ToasterLoading
{
    internal class ToasterLoading
    {
        private const int WM_KEYUP = 0x101;
        private const int Disable = 0x20; // Space

        private const int SecondsToWait = 250;

        private readonly MemoryStream _packet;
        private bool _escaped;
        private Timer _timer;

        public ToasterLoading()
        {
            _packet = new MemoryStream();
            Game.OnGameSendPacket += OnGameSendPacket;
            Game.OnWndProc += OnWndProc;
            Drawing.OnDraw += OnDraw;
        }

        private void OnWndProc(WndEventArgs args)
        {
            try
            {
                if (args.Msg == WM_KEYUP && args.WParam == Disable)
                {
                    _escaped = true;
                    Game.SendPacket(_packet.ToArray(), PacketChannel.C2S, PacketProtocolFlags.Reliable);
                    _packet.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnDraw(EventArgs args)
        {
            try
            {
                if (_escaped)
                    return;

                Drawing.DrawText(10, 10, Color.Green, Assembly.GetExecutingAssembly().GetName().Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnGameSendPacket(GamePacketEventArgs args)
        {
            try
            {
                if (args.PacketData[0] == 190 && !_escaped)
                {
                    args.Process = false;
                    _packet.Write(args.PacketData, 0, args.PacketData.Length);
                    _timer = new Timer(SecondsToWait*1000);
                    _timer.Elapsed += OnTimedEvent;
                    _timer.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                if (!_escaped)
                {
                    _escaped = true;
                    Game.SendPacket(_packet.ToArray(), PacketChannel.C2S, PacketProtocolFlags.Reliable);
                    _packet.Close();
                }
                _timer.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}