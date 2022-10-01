using System;

namespace Skyjoo.GameLogic.Bots
{
    public class HardBot: BaseBot
    {
        public HardBot(string name, int playerIndex, int randomSeed)
        {
            Name = name;
            PlayerIndex = playerIndex;
            Difficulty = BotDifficulty.HARD;
            random = new Random(randomSeed);
        }
        public override void PlayMove(SkyjoBoard board)
        {
            throw new NotImplementedException();
        }

        public new void RevealTwoCards(SkyjoBoard board)
        {
            throw new NotImplementedException();
        }
    }
}