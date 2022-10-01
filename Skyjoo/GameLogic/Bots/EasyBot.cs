using System;

namespace Skyjoo.GameLogic.Bots
{
    public class EasyBot : BaseBot
    {
        public EasyBot(string name, int playerIndex, int randomSeed)
        {
            Name = name;
            PlayerIndex = playerIndex;
            Difficulty = BotDifficulty.EASY;
            random = new Random(randomSeed);
        }
        public override void PlayMove(SkyjoBoard board)
        {
            if (board.ReverseSkyjoCardStack.Count == 0)
                StackMove(board);
            else
                switch (random.Next() % 2)
                {
                    case 0:
                        ReverseStackMove(board);
                        break;
                    case 1:
                        StackMove(board);
                        break;
                }
        }

        protected void ReverseStackMove(SkyjoBoard board)
        {
            board.ValidateMove(PlayerIndex, FieldUpdateType.ReversedStackToCurrent);
            board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToField, random.Next() % (board.FieldWidth * board.FieldHeight));
        }

        protected void StackMove(SkyjoBoard board)
        {
            board.ValidateMove(PlayerIndex, FieldUpdateType.StackToCurrent);
            switch (random.Next() % 2)
            {
                case 0:
                    board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToField, random.Next() % (board.FieldWidth * board.FieldHeight));
                    break;
                case 1:
                    board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToReverseStack);
                    board.ValidateMove(PlayerIndex, FieldUpdateType.RevealOnField, GetRandomRevealField(board));
                    break;
            }
        }
    }
}