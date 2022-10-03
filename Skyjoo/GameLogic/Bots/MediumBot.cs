using System;
using System.Threading;

namespace Skyjoo.GameLogic.Bots
{
    public class MediumBot : BaseBot
    {
        public MediumBot(string name, int playerIndex, int randomSeed)
        {
            Name = name;
            PlayerIndex = playerIndex;
            Difficulty = BotDifficulty.MEDIUM;
            random = new Random(randomSeed);
        }

        public override void PlayMove(SkyjoBoard board)
        {
            //Thread.Sleep(500);
            if (checkReverseStackGood(board, out int index))
            {
                //Thread.Sleep(500);
                executeMove(board, FieldUpdateType.ReversedStackToCurrent);
                //Thread.Sleep(500);
                executeMove(board, FieldUpdateType.CurrentToField, index);
            }
            else
            {
                //Thread.Sleep(500);
                var card = board.SkyjoCardStack.PeekTopCard();
                executeMove(board, FieldUpdateType.StackToCurrent);
                //Thread.Sleep(500);
                moveCurrentFromStack(board, card);
            }
        }

        public new void RevealTwoCards(SkyjoBoard board)
        {
            int fieldSize = board.FieldWidth * board.FieldHeight;
            int field1 = random.Next() % fieldSize;
            int field2;
            //not the same row
            int row1 = field1 % board.FieldWidth;
            do
            {
                field2 = random.Next() % fieldSize;
            } while (field2 % board.FieldWidth == row1);
            executeMove(board, FieldUpdateType.RevealOnField, field1);
            executeMove(board, FieldUpdateType.RevealOnField, field2);
        }

        private void moveCurrentFromStack(SkyjoBoard board, SkyjoCardNumber topStackCard)
        {
            SkyjoCardNumber currentNumber = topStackCard;

            var rows = GetRows(board);
            var nInvisible = GetNumberOfInvisiblesInRows(rows);

            //Endgame
            if (nInvisible == 1)
            {
                logInfo("Endgame");
                if (HasLowestFieldValue(board, board.Players[PlayerIndex].PlayingField.CurrentCard))
                {
                    logInfo("Finishing Game");
                    //Finish Game
                    for (int i = 0; i < board.Players[PlayerIndex].PlayingField.FieldCards.Length; i++)
                    {
                        if (!board.Players[PlayerIndex].PlayingField.FieldCards[i].IsVisible)
                        {
                            executeMove(board, FieldUpdateType.CurrentToField, i);
                            break;
                        }
                    }
                    return;
                }
                else
                {
                    //Postpone Endgame
                    logInfo("Moving to highest card");
                    executeMove(board, FieldUpdateType.CurrentToField, GetIndexOfHighestCardInRows(rows));
                    return;
                }
            }

            //default
            if (checkForFieldIndex(board, currentNumber, out int index))
            {
                executeMove(board, FieldUpdateType.CurrentToField, index);
                return;
            }
            else
            {
                executeMove(board, FieldUpdateType.CurrentToReverseStack);
                executeMove(board, FieldUpdateType.RevealOnField, GetRandomRevealField(board));
                return;
            }
        }

        private bool checkReverseStackGood(SkyjoBoard board, out int index)
        {
            logInfo("Checking if reverse is good");
            return checkForFieldIndex(board, board.ReverseSkyjoCardStack.PeekTopCard(), out index);
        }

        private bool checkForFieldIndex(SkyjoBoard board, SkyjoCardNumber number, out int index)
        {
            index = -1;
            if (board.ReverseSkyjoCardStack.Count == 0 || number == SkyjoCardNumber.Placeholder)
                return false;

            var rows = GetRows(board);
            var nInvisible = GetNumberOfInvisiblesInRows(rows);

            bool canReplaceInvisible = nInvisible > 1;
            //Try to finish a row
            if (number > SkyjoCardNumber.Zero)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    if (rows[i][0].IsPlaceholder)
                        continue;

                    bool skipCheck = false;
                    bool isInvisible = false;
                    int replaceIndex = -1;
                    for (int j = 0; j < rows[i].Count; j++)
                    {
                        if (!rows[i][j].IsVisible)
                        {
                            if (replaceIndex > -1)
                            {
                                skipCheck = true;
                                break;
                            }
                            replaceIndex = j;
                            isInvisible = true;
                        }
                        if (rows[i][j].IsVisible && rows[i][j].Number != number)
                        {
                            if (replaceIndex > -1)
                            {
                                skipCheck = true;
                                break;
                            }
                            replaceIndex = j;
                        }
                    }
                    if (!skipCheck && replaceIndex > -1)
                    {
                        if (!(!canReplaceInvisible && isInvisible))
                        {
                            index = GetPlayingFieldIndexFromRow(i, rows.Count, replaceIndex);
                            return true;
                        }
                    }
                }
            }


            //Endgame
            if (nInvisible == 1)
            {
                if (HasLowestFieldValue(board, new SkyjoCard(number, true)))
                {
                    logInfo("Finishing game");
                    //Finish Game
                    for (int i = 0; i < board.Players[PlayerIndex].PlayingField.FieldCards.Length; i++)
                    {
                        if (!board.Players[PlayerIndex].PlayingField.FieldCards[i].IsVisible)
                        {
                            index = i;
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }

            int possibleFieldSpotWithNumber = -1;
            int possibleFieldSpot = -1;
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i][0].IsPlaceholder)
                    continue;
                //check rows if currNumber is in there
                int invisibleIndex = -1;
                int numberCurrInRow = 0;
                int possibleRowSpot = -1;
                for (int j = 0; j < rows[i].Count; j++)
                {
                    if (!rows[i][j].IsVisible)
                        invisibleIndex = j;

                    if (rows[i][j].IsVisible && rows[i][j].Number == number)
                        numberCurrInRow++;

                    if (rows[i][j].IsVisible && rows[i][j].Number > number && (possibleRowSpot == -1 || rows[i][j].Number > rows[i][possibleRowSpot].Number))
                    {
                        possibleRowSpot = j;
                    }
                }
                //if there is a row with currNumber and an invisible card, remember it
                if (numberCurrInRow > 0 && invisibleIndex > -1)
                {
                    possibleFieldSpotWithNumber = GetPlayingFieldIndexFromRow(i, rows.Count, invisibleIndex);
                }
                //if there is a row with currNumber and no invisible card and a card with higher value, remember it
                if (numberCurrInRow > 0 && possibleRowSpot > -1)
                {
                    possibleFieldSpot = GetPlayingFieldIndexFromRow(i, rows.Count, possibleRowSpot);
                }
            }
            //if there is a row with currNumber and invisible card, replace
            if (possibleFieldSpotWithNumber > -1)
            {
                logInfo("Moving to row with number in it and replace invisible");
                index = possibleFieldSpotWithNumber;
                return true;
            }
            //if there is a row with currNumber and no invisible card and a card with higher value, replace
            if (possibleFieldSpot > -1)
            {
                logInfo("Moving to row with number in it and replace higher number");
                index = possibleFieldSpot;
                return true;
            }

            //if there is no possible spot, don't take card
            if (number > SkyjoCardNumber.Plus4)
            {
                logInfo("Card is useless");
                return false;
            }
            //card is 4 or lower
            else
            {
                if (number > SkyjoCardNumber.Plus1)
                {
                    //find row with only invis, put there
                    logInfo("Searching for only invisibles row");
                    int possibleIndex = -1;
                    for (int i = 0; i < rows.Count; i++)
                    {
                        if (rows[i][0].IsPlaceholder)
                            continue;
                        if (GetNumberOfInvisiblesInRow(rows[i]) == board.FieldHeight)
                        {
                            possibleIndex = GetPlayingFieldIndexFromRow(i, rows.Count, random.Next() % board.FieldHeight);
                        }
                        //check for ultra low row (max 1)
                        bool isUltraLow = true;
                        int invisibleIndex = -1;
                        int possibleRowSpot = -1;
                        for (int j = 0; j < rows[i].Count; j++)
                        {
                            if (!rows[i][j].IsVisible)
                            {
                                invisibleIndex = j;
                            }
                            if (rows[i][j].Number > SkyjoCardNumber.Plus1)
                            {
                                isUltraLow = false;
                                break;
                            }
                            if (rows[i][j].IsVisible && rows[i][j].Number > number && (possibleRowSpot == -1 || rows[i][j].Number > rows[i][possibleRowSpot].Number))
                            {
                                possibleRowSpot = j;
                            }
                        }
                        if (isUltraLow)
                        {
                            if (invisibleIndex > -1)
                            {
                                index = GetPlayingFieldIndexFromRow(i, rows.Count, invisibleIndex);
                                return true;
                            }
                            else if (possibleRowSpot > -1)
                            {
                                index = GetPlayingFieldIndexFromRow(i, rows.Count, possibleRowSpot);
                                return true;
                            }

                        }
                    }
                    if (possibleIndex > -1)
                    {
                        index = possibleIndex;
                        return true;
                    }
                }
                logInfo("Searching for low row");
                //find low row, put there
                for (int i = 0; i < rows.Count; i++)
                {
                    if (rows[i][0].IsPlaceholder)
                        continue;
                    if (IsRowLowRow(rows[i]))
                    {
                        int possibleRowSpot = -1;
                        for (int j = 0; j < rows[i].Count; j++)
                        {
                            if (!rows[i][j].IsVisible)
                            {
                                logInfo("Moving to Invisible");
                                index = GetPlayingFieldIndexFromRow(i, rows.Count, j);
                                return true;
                            }

                            if (rows[i][j].Number > number && (possibleRowSpot == -1 || rows[i][j].Number > rows[i][possibleRowSpot].Number))
                            {
                                possibleRowSpot = j;
                            }
                        }
                        if (possibleRowSpot > -1)
                        {
                            logInfo("Moving to higher number");
                            index = GetPlayingFieldIndexFromRow(i, rows.Count, possibleRowSpot);
                            return true;
                        }
                    }
                }
                // if no low row, find row that doesnt collect, put it in there
                logInfo("Searching for non collecting row");
                for (int i = 0; i < rows.Count; i++)
                {
                    if (rows[i][0].IsPlaceholder)
                        continue;
                    if (!IsRowCollecting(rows[i]))
                    {
                        int possibleRowSpot = -1;
                        for (int j = 0; j < rows[i].Count; j++)
                        {
                            if (!rows[i][j].IsVisible)
                            {
                                logInfo("Moving to Invisible");
                                index = GetPlayingFieldIndexFromRow(i, rows.Count, j);
                                return true;
                            }

                            if (rows[i][j].Number > number && (possibleRowSpot == -1 || rows[i][j].Number > rows[i][possibleRowSpot].Number))
                            {
                                possibleRowSpot = j;
                            }
                        }
                        if (possibleRowSpot > -1)
                        {
                            logInfo("Moving to higher number");
                            index = GetPlayingFieldIndexFromRow(i, rows.Count, possibleRowSpot);
                            return true;
                        }
                    }
                }
                //find row with only invis, put there
                logInfo("Searching for only invisibles row");
                for (int i = 0; i < rows.Count; i++)
                {
                    if (rows[i][0].IsPlaceholder)
                        continue;
                    if (GetNumberOfInvisiblesInRow(rows[i]) == board.FieldHeight)
                    {
                        index = GetPlayingFieldIndexFromRow(i, rows.Count, random.Next() % board.FieldHeight);
                        return true;
                    }
                }
            }
            logInfo("Card is useless 2");
            return false;
        }

    }
}