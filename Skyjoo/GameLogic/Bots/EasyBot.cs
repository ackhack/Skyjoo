using System;
using System.Threading;

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
            logInfo("Reverse Stack Move");
            executeMove(board, FieldUpdateType.ReversedStackToCurrent);
            //Thread.Sleep(500);
            executeMove(board, FieldUpdateType.CurrentToField, random.Next() % (board.FieldWidth * board.FieldHeight));
        }

        protected void StackMove(SkyjoBoard board)
        {
            logInfo("Stack Move");
            executeMove(board, FieldUpdateType.StackToCurrent);
            //Thread.Sleep(500);
            switch (random.Next() % 2)
            {
                case 0:
                    executeMove(board, FieldUpdateType.CurrentToField, random.Next() % (board.FieldWidth * board.FieldHeight));
                    break;
                case 1:
                    executeMove(board, FieldUpdateType.CurrentToReverseStack);
                    //Thread.Sleep(500);
                    executeMove(board, FieldUpdateType.RevealOnField, GetRandomRevealField(board));
                    break;
            }
        }
    }
}