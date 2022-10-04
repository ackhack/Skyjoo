using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Skyjoo.GameLogic.Bots
{
    public abstract partial class BaseBot
    {
        public abstract void PlayMove(SkyjoBoard board);

        public void RevealTwoCards(SkyjoBoard board)
        {
            int fieldSize = board.FieldWidth * board.FieldHeight;
            int field1 = random.Next() % fieldSize;
            executeMove(board, FieldUpdateType.RevealOnField, field1);
            executeMove(board, FieldUpdateType.RevealOnField, (field1 + 1 + (random.Next() % (fieldSize - 2))) % fieldSize);
        }

        [DebuggerStepThrough]
        protected List<int> GetAllUnreaveledFields(SkyjoBoard board)
        {
            List<int> fields = new List<int>();

            for (int i = 0; i < board.Players[PlayerIndex].PlayingField.FieldCards.Length; i++)
            {
                if (!board.Players[PlayerIndex].PlayingField.FieldCards[i].IsVisible)
                    fields.Add(i);
            }
            return fields;
        }

        [DebuggerStepThrough]
        protected List<List<SkyjoCard>> GetRows(SkyjoBoard board)
        {

            List<List<SkyjoCard>> rows = new List<List<SkyjoCard>>();

            for (int i = 0; i < board.FieldWidth; i++)
            {
                rows.Add(new List<SkyjoCard>());
            }
            for (int i = 0; i < board.Players[PlayerIndex].PlayingField.FieldCards.Length; i++)
            {
                rows[i % rows.Count].Add(board.Players[PlayerIndex].PlayingField.FieldCards[i]);
            }
            return rows;
        }

        [DebuggerStepThrough]
        protected int GetNumberOfInvisiblesInRows(List<List<SkyjoCard>> rows)
        {
            int numberInvisible = 0;
            foreach (var row in rows)
            {
                numberInvisible += GetNumberOfInvisiblesInRow(row);
            }
            return numberInvisible;
        }

        [DebuggerStepThrough]
        protected int GetNumberOfInvisiblesInRow(List<SkyjoCard> row)
        {
            int numberInvisible = 0;
            foreach (var card in row)
            {
                if (!card.IsVisible)
                    numberInvisible++;
            }
            return numberInvisible;
        }

        protected bool HasLowestFieldValue(SkyjoBoard board, SkyjoCard lastCard)
        {
            logInfo("Checking for lowest field value");
            int ownValue = GetFieldValueLastCard(GetRows(board), lastCard);
            if (ownValue == -1)
                return false;

            for (int i = 0; i < board.Players.Length; i++)
            {
                if (i == PlayerIndex)
                    continue;
                if (ownValue >= GetFieldValue(board.Players[i].PlayingField.FieldCards))
                    return false;
            }
            return true;
        }

        protected int GetFieldValueLastCard(List<List<SkyjoCard>> rows, SkyjoCard lastCard)
        {
            bool firstInvisible = false;
            var value = -1;

            for (int r = 0; r < rows.Count; r++)
            {
                for (int c = 0; c < rows[r].Count; c++)
                {
                    if (!rows[r][c].IsVisible)
                    {
                        // if multiple invisible, abort
                        if (firstInvisible)
                            return -1;

                        // insert card
                        firstInvisible = true;
                        rows[r][c] = lastCard;

                        // check if row can be cleared
                        if (rows[r].Distinct().Count() == 1)
                        {
                            for (int i = 0; i < rows[r].Count; i++)
                            {
                                rows[r][i] = new SkyjoCard(SkyjoCardNumber.Placeholder, true);
                            }
                        }
                    }
                }
            }

            // calculate value
            for (int r = 0; r < rows.Count; r++)
            {
                for (int c = 0; c < rows[r].Count; c++)
                {
                    value += rows[r][c].GetValue();
                }
            }
            return value;
        }

        [DebuggerStepThrough]
        protected int GetFieldValue(SkyjoCard[] cards)
        {
            int value = 0;

            foreach (var card in cards)
            {
                if (!card.IsVisible)
                {
                    value += 6;
                    continue;
                }
                value += card.GetValue();
            }

            return value;
        }

        [DebuggerStepThrough]
        protected int GetIndexOfHighestCardInRows(List<List<SkyjoCard>> rows)
        {
            var value = SkyjoCardNumber.Placeholder;
            var index = -1;

            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = 0; i < rows[i].Count; j++)
                {
                    if (rows[i][j].Number > value && rows[i][j].IsVisible)
                    {
                        value = rows[i][j].Number;
                        index = GetPlayingFieldIndexFromRow(i, rows.Count, j);
                    }
                }
            }

            return index;
        }

        [DebuggerStepThrough]
        protected int GetIndexOfHighestCardInRow(List<SkyjoCard> row, SkyjoCardNumber ignore = SkyjoCardNumber.Placeholder)
        {
            var value = SkyjoCardNumber.Placeholder;
            var index = -1;

            for (int i = 0; i < row.Count; i++)
            {
                if (ignore != SkyjoCardNumber.Placeholder && row[i].Number == ignore)
                    continue;
                if (row[i].Number > value && row[i].IsVisible)
                {
                    index = i;
                }
            }

            return index;
        }

        protected int GetPlayingFieldIndexFromRow(int rowIndex, int rowCount, int fieldIndex)
        {
            return fieldIndex * rowCount + rowIndex;
        }

        [DebuggerStepThrough]
        protected int GetRandomRevealField(SkyjoBoard board)
        {
            var fields = GetAllUnreaveledFields(board);
            return fields[random.Next() % fields.Count];
        }

        [DebuggerStepThrough]
        protected bool IsRowCollecting(List<SkyjoCard> row)
        {
            return row.Count != row.Distinct().Count();
        }

        [DebuggerStepThrough]
        protected bool IsRowLowRow(List<SkyjoCard> row)
        {
            return row.Any(x => x.IsVisible && x.Number < SkyjoCardNumber.Plus5 && !x.IsPlaceholder);
        }
    }
}