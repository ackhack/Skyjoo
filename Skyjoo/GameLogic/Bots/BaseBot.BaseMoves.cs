using Android.Bluetooth;
using System.Collections.Generic;
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
            board.ValidateMove(PlayerIndex, FieldUpdateType.RevealOnField, field1);
            board.ValidateMove(PlayerIndex, FieldUpdateType.RevealOnField, (field1 + 1 + (random.Next() % (fieldSize - 2))) % fieldSize);
        }

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

        protected int GetNumberOfInvisiblesInRows(List<List<SkyjoCard>> rows)
        {
            int numberInvisible = 0;
            foreach (var row in rows)
            {
                numberInvisible += GetNumberOfInvisiblesInRow(row);
            }
            return numberInvisible;
        }

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
            int ownValue = GetFieldValueLastCard(board, lastCard);
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

        protected int GetFieldValueLastCard(SkyjoBoard board, SkyjoCard lastCard)
        {
            var cards = board.Players[PlayerIndex].PlayingField.FieldCards;
            var invisibleIndex = -1;
            var value = -1;
            for (int n = 0; n < cards.Length; n++)
            {
                if (!cards[n].IsVisible)
                {
                    if (invisibleIndex > -1)
                        return -1;
                    invisibleIndex = n;
                    cards[n] = lastCard;
                    //clear rows
                    for (int i = 0; i < board.FieldWidth; i++)
                    {
                        SkyjoCardNumber firstCard = cards[i].Number;
                        if (firstCard == SkyjoCardNumber.Placeholder)
                            continue;
                        bool canClear = true;

                        for (int j = 0; j < board.FieldHeight; j++)
                        {
                            var currIndex = i + j * board.FieldWidth;
                            if (!cards[currIndex].IsVisible)
                            {
                                canClear = false;
                                break;
                            }
                            if (j > 0)
                            {
                                if (firstCard != cards[currIndex].Number)
                                {
                                    canClear = false;
                                    break;
                                }
                            }
                        }

                        if (canClear)
                        {
                            for (int j = 0; j < board.FieldHeight; j++)
                            {
                                var currIndex = i + j * board.FieldWidth;
                                cards[currIndex] = new SkyjoCard(SkyjoCardNumber.Placeholder, true);
                            }
                        }
                    }
                    value = GetFieldValue(cards);
                }
            }
            return value;
        }

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
                        index = GetPlayingFieldIndexFromRow(j, rows[i].Count, i);
                    }
                }
            }

            return index;
        }

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

        protected int GetRandomRevealField(SkyjoBoard board)
        {
            var fields = GetAllUnreaveledFields(board);
            return fields[random.Next() % fields.Count];
        }

        protected bool IsRowCollecting(List<SkyjoCard> row)
        {
            return row.Count != row.Distinct().Count();
        }

        protected bool IsRowLowRow(List<SkyjoCard> row)
        {
            return row.Any(x => x.IsVisible && x.Number < SkyjoCardNumber.Plus5 && !x.IsPlaceholder);
        }
    }
}