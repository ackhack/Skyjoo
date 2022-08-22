using System;
using System.Collections.Generic;

namespace Skyjoo.GameLogic
{
    public class SkyjoCardStack
    {
        protected List<SkyjoCard> cards = new List<SkyjoCard>();

        public int Count { get { return cards.Count; } }

        /// <summary>
        /// Removes the top card of the stack
        /// </summary>
        /// <returns>The top card or null if stack is empty</returns>
        virtual public SkyjoCard GetTopCard()
        {
            if (cards.Count == 0) return null;
            var card = cards[0];
            cards.RemoveAt(0);
            return card;
        }

        /// <summary>
        /// Returns the image id of the top card or the placeholderId if empty
        /// </summary>
        /// <returns>A Resource.Drawable id</returns>
        virtual public int GetTopImageId()
        {
            if (cards.Count == 0) return SkyjoCard.GetPlaceholderImageId();
            return cards[0].GetImageId();
        }

        /// <summary>
        /// Adds card(s) to the stack, except for placeholders
        /// </summary>
        /// <param name="cards">The card(s) to add</param>
        public void AddCards(params SkyjoCard[] cards)
        {
            foreach (SkyjoCard card in cards)
            {
                if (card.Number == SkyjoCardNumber.Placeholder) continue;
                this.cards.Add(card);
            }
        }

        /// <summary>
        /// Clears all cards
        /// </summary>
        public void Clear()
        {
            cards = new List<SkyjoCard>();
        }

        /// <summary>
        /// Shuffles the stack
        /// </summary>
        /// <param name="seed">A randomness seed</param>
        public void Shuffle(int seed)
        {
            Random rng = new Random(seed);
            int n = cards.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                SkyjoCard value = cards[k];
                cards[k] = cards[n];
                cards[n] = value;
            }
        }

        public static SkyjoCardStack GetCardStack(int seed)
        {
            var stack = new SkyjoCardStack();

            stack.AddCards(SkyjoCard.GetAllCards());

            stack.Shuffle(seed);

            return stack;
        }
    }
}