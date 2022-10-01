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
            Thread.Sleep(500);
            if (checkReverseStackGood(board, out int index))
            {
                board.ValidateMove(PlayerIndex, FieldUpdateType.ReversedStackToCurrent);
                Thread.Sleep(100);
                board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToField, index);
            }
            else
            {
                var card = board.SkyjoCardStack.PeekTopCard();
                board.ValidateMove(PlayerIndex, FieldUpdateType.StackToCurrent);
                Thread.Sleep(100);
                moveCurrentFromStack(board,card);
            }
        }

        public new void RevealTwoCards(SkyjoBoard board)
        {
            int fieldSize = board.FieldWidth * board.FieldHeight;
            int field1 = random.Next() % fieldSize;
            int field2;
            //not the same row
            do
            {
                field2 = random.Next() % fieldSize;
            } while ((field2 + field1) % board.FieldWidth == 0);
            board.ValidateMove(PlayerIndex, FieldUpdateType.RevealOnField, field1);
            board.ValidateMove(PlayerIndex, FieldUpdateType.RevealOnField, field2);
        }

        private void moveCurrentFromStack(SkyjoBoard board, SkyjoCardNumber topStackCard)
        {
            SkyjoCardNumber currentNumber = topStackCard;

            var rows = GetRows(board);
            var nInvisible = GetNumberOfInvisiblesInRows(rows);

            //Endgame
            if (nInvisible == 1)
            {
                if (HasLowestFieldValue(board, board.Players[PlayerIndex].PlayingField.CurrentCard))
                {
                    //Finish Game
                    for (int i = 0; i < board.Players[PlayerIndex].PlayingField.FieldCards.Length; i++)
                    {
                        if (board.Players[PlayerIndex].PlayingField.FieldCards[i].IsPlaceholder)
                        {
                            board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToField, i);
                            break;
                        }
                    }
                    return;
                }
                else
                {
                    //Postpone Endgame
                    board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToField, GetIndexOfHighestCardInRows(rows));
                    return;
                }
            }

            //Gamestart
            var numberMin = board.FieldWidth;
            if (nInvisible > (board.Players[PlayerIndex].PlayingField.FieldCards.Length - numberMin))
            {
                int rowIndex = -1;
                for (int i = 0; i < rows.Count; i++)
                {
                    for (int j = 0; j < rows[i].Count; j++)
                    {
                        if (rows[i][j].Number == currentNumber)
                        {
                            rowIndex = i;
                            break;
                        }
                    }
                    if (rowIndex > -1)
                        break;
                }

                //currNum on field
                if (rowIndex > -1)
                {
                    //if any card in row is Invisible, replace
                    for (int j = 0; j < rows[rowIndex].Count; j++)
                    {
                        if (!rows[rowIndex][j].IsVisible)
                        {
                            board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToField, GetPlayingFieldIndexFromRow(rowIndex, rows[rowIndex].Count, j));
                            return;
                        }
                    }

                    //get highest number that is not currNumber, replace
                    board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToField, GetIndexOfHighestCardInRow(rows[rowIndex], currentNumber));
                    return;
                }
                //currNumber not on field
                else
                {
                    if (currentNumber < SkyjoCardNumber.Plus5)
                    {
                        //find low row, put there
                        for (int i = 0; i < rows.Count; i++)
                        {
                            if (IsRowLowRow(rows[i]))
                            {
                                int possibleRowSpot = -1;
                                for (int j = 0; j < rows[i].Count; j++)
                                {
                                    if (!rows[i][j].IsVisible)
                                    {
                                        board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToField, GetPlayingFieldIndexFromRow(i, rows.Count, j));
                                        return;
                                    }

                                    if (rows[i][j].Number > currentNumber && possibleRowSpot > -1 && rows[i][j].Number > rows[i][possibleRowSpot].Number)
                                    {
                                        possibleRowSpot = j;
                                    }
                                }
                                if (possibleRowSpot > -1)
                                {
                                    board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToField, GetPlayingFieldIndexFromRow(i, rows.Count, possibleRowSpot));
                                    return;
                                }
                            }
                        }
                        // if no low row, find row that doesnt collect, put it in there
                        for (int i = 0; i < rows.Count; i++)
                        {
                            if (!IsRowCollecting(rows[i]))
                            {
                                int possibleRowSpot = -1;
                                for (int j = 0; j < rows[i].Count; j++)
                                {
                                    if (!rows[i][j].IsVisible)
                                    {
                                        board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToField, GetPlayingFieldIndexFromRow(i, rows.Count, j));
                                        return;
                                    }

                                    if (rows[i][j].Number > currentNumber && possibleRowSpot > -1 && rows[i][j].Number > rows[i][possibleRowSpot].Number)
                                    {
                                        possibleRowSpot = j;
                                    }
                                }
                                if (possibleRowSpot > -1)
                                {
                                    board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToField, GetPlayingFieldIndexFromRow(i, rows.Count, possibleRowSpot));
                                    return;
                                }
                            }
                        }
                        //else replace highest card on board
                        board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToField, GetIndexOfHighestCardInRows(rows));
                        return;
                        
                    }
                    else
                    {
                        //move to reverse
                        board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToReverseStack);
                        int anyInvisible = -1;
                        //find row with only placeholders
                        for (int i = 0; i < rows.Count; i++)
                        {
                            bool hasOnlyInvisible = true;
                            for (int j = 0; j < rows[i].Count; j++)
                            {
                                if ((anyInvisible == -1 || random.Next() % 2 == 0) && !rows[i][j].IsVisible)
                                    anyInvisible = GetPlayingFieldIndexFromRow(i, rows.Count, j);

                                if (rows[i][j].IsVisible)
                                    hasOnlyInvisible = false;
                            }
                            if (hasOnlyInvisible)
                            {
                                //if only invisible in row, replace one
                                board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToField, GetPlayingFieldIndexFromRow(i, rows.Count, random.Next() % rows[i].Count));
                                return;
                            }
                        }
                        //if no row with only invisible, replace random invisible
                        board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToField, anyInvisible);
                        return;
                    }
                }
            }

            //default
            if (checkForFieldIndex(board, currentNumber, out int index))
            {
                board.ValidateMove(PlayerIndex, FieldUpdateType.CurrentToField, index);
                return;
            }
            else
            {
                board.ValidateMove(PlayerIndex, FieldUpdateType.RevealOnField, GetRandomRevealField(board));
                return;
            }
        }

        private bool checkReverseStackGood(SkyjoBoard board, out int index)
        {
            return checkForFieldIndex(board, board.ReverseSkyjoCardStack.PeekTopCard(), out index);
        }

        private bool checkForFieldIndex(SkyjoBoard board, SkyjoCardNumber number, out int index)
        {
            index = -1;
            if (board.ReverseSkyjoCardStack.Count == 0 || number == SkyjoCardNumber.Placeholder)
                return false;

            var rows = GetRows(board);
            var nInvisible = GetNumberOfInvisiblesInRows(rows);

            //Endgame
            if (nInvisible == 1 && HasLowestFieldValue(board, new SkyjoCard(number, true)))
            {
                //Finish Game
                for (int i = 0; i < board.Players[PlayerIndex].PlayingField.FieldCards.Length; i++)
                {
                    if (board.Players[PlayerIndex].PlayingField.FieldCards[i].IsPlaceholder)
                    {
                        index = i;
                        return true;
                    }
                }
                return false;
            }

            int possibleFieldSpotWithNumber = -1;
            int possibleFieldSpot = -1;
            for (int i = 0; i < rows.Count; i++)
            {
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

                    if (rows[i][j].IsVisible && rows[i][j].Number > number && possibleRowSpot > -1 && rows[i][j].Number > rows[i][possibleRowSpot].Number)
                    {
                        possibleRowSpot = j;
                    }
                }
                //finish a row
                if (numberCurrInRow == rows[0].Count - 1)
                {
                    index = invisibleIndex;
                    return true;
                }
                //if there is a row with currNumber and an invisible card, remember it
                if (numberCurrInRow > 0 && invisibleIndex > -1)
                {
                    possibleFieldSpotWithNumber = invisibleIndex;
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
                index = possibleFieldSpotWithNumber;
                return true;
            }
            //if there is a row with currNumber and no invisible card and a card with higher value, replace
            if (possibleFieldSpot > -1)
            {
                index = possibleFieldSpot;
                return true;
            }

            //if there is no possible spot, don't take card
            if (number > SkyjoCardNumber.Plus4)
            {
                return false;
            }
            //card is 4 or lower
            else
            {
                //find low row, put there
                for (int i = 0; i < rows.Count; i++)
                {
                    if (IsRowLowRow(rows[i]))
                    {
                        int possibleRowSpot = -1;
                        for (int j = 0; j < rows[i].Count; j++)
                        {
                            if (!rows[i][j].IsVisible)
                            {
                                index = GetPlayingFieldIndexFromRow(i, rows.Count, j);
                                return true;
                            }

                            if (rows[i][j].Number > number && possibleRowSpot > -1 && rows[i][j].Number > rows[i][possibleRowSpot].Number)
                            {
                                possibleRowSpot = j;
                            }
                        }
                        if (possibleRowSpot > -1)
                        {
                            index = GetPlayingFieldIndexFromRow(i, rows.Count, possibleRowSpot);
                            return true;
                        }
                    }
                }
                // if no low row, find row that doesnt collect, put it in there
                for (int i = 0; i < rows.Count; i++)
                {
                    if (!IsRowCollecting(rows[i]))
                    {
                        int possibleRowSpot = -1;
                        for (int j = 0; j < rows[i].Count; j++)
                        {
                            if (!rows[i][j].IsVisible)
                            {
                                index = GetPlayingFieldIndexFromRow(i, rows.Count, j);
                                return true;
                            }

                            if (rows[i][j].Number > number && possibleRowSpot > -1 && rows[i][j].Number > rows[i][possibleRowSpot].Number)
                            {
                                possibleRowSpot = j;
                            }
                        }
                        if (possibleRowSpot > -1)
                        {
                            index = GetPlayingFieldIndexFromRow(i, rows.Count, possibleRowSpot);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

    }
}