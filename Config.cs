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

        public Config()
        {
            Token = File.ReadAllText("Config/token.txt");
            ChannelId = ulong.Parse(File.ReadAllText("Config/channelid.txt"));
        }
    }
}
