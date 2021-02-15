using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MudaePokeFarm
{
    internal class Config
    {
        public readonly string Token;

        public readonly ulong ChannelId;

        public readonly TimeSpan Delay;

        public Config()
        {
            Token = File.ReadAllText("Config/token.txt");
            ChannelId = ulong.Parse(File.ReadAllText("Config/channelid.txt"));

            string[] delayLines = File.ReadAllLines("Config/delay.txt");
            Delay = new TimeSpan(int.Parse(delayLines[0].Split(':')[1]), int.Parse(delayLines[1].Split(':')[1]), int.Parse(delayLines[2].Split(':')[1]));
        }
    }
}
