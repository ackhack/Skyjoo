using System.Collections.Generic;

namespace Skyjoo.GameLogic
{
    public class ReverseSkyjoCardStack : SkyjoCardStack
    {
        /// <summary>
        /// Removes all cards from the stack
        /// </summary>
        /// <returns>A List of all Cards removed</returns>
        public List<SkyjoCard> GetAllCards()
        {
            var list = cards;
            cards = new List<SkyjoCard>();
            return list;
        }

        public override SkyjoCardNumber PeekTopCard()
        {
            if (cards.Count == 0) return SkyjoCardNumber.Placeholder;
            return cards[cards.Count - 1].Number;
        }

        public override SkyjoCard GetTopCard()
        {
            if (cards.Count == 0) return null;
            var card = cards[cards.Count - 1];
            cards.RemoveAt(cards.Count - 1);
            return card;
        }

        public override int GetTopImageId()
        {
            if (cards.Count == 0) return SkyjoCard.GetPlaceholderImageId();
            return cards[cards.Count - 1].GetImageId();
        }
    }
}