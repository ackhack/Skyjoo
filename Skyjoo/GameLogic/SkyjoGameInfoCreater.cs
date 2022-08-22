using System;
using System.Collections.Generic;

namespace Skyjoo.GameLogic
{
    public class SkyjoGameInfoCreater
    {
        public static SkyjoGameInfo CreateGameInfo(Dictionary<string, string> playerLogins)
        {
            SkyjoGameInfo info = new SkyjoGameInfo();

            info.Players = playerLogins;

            info.StackSeed = new Random().Next();

            return info;
        }
    }
}