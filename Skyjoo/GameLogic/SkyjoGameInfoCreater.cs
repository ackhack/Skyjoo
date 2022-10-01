using System;
using System.Collections.Generic;

namespace Skyjoo.GameLogic
{
    public class SkyjoGameInfoCreater
    {
        public static SkyjoGameInfo CreateGameInfo(Dictionary<string, string> playerLogins)
        {
            SkyjoGameInfo info = new SkyjoGameInfo();

            info.FieldWidth = 4;
            info.FieldHeight = 3;

            info.Players = playerLogins;

            info.StackSeed = new Random().Next();
            info.BotSeed = new Random().Next();

            return info;
        }
    }
}