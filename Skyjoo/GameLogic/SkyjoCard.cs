using Skyjoo.Dependency;
using System;

namespace Skyjoo.GameLogic
{
    public class SkyjoCard
    {
        public SkyjoCard(SkyjoCardNumber number, bool isVisible)
        {
            Number = number;
            IsVisible = isVisible;
        }

        public SkyjoCardNumber Number;
        public bool IsVisible;
        public bool IsPlaceholder { get { return Number == SkyjoCardNumber.Placeholder; } }

        public static int GetPlaceholderImageId()
        {
            return DependencyClass.ImageHandler.GetPlaceholderImageId();
        }

        public int GetImageId()
        {
            return DependencyClass.ImageHandler.GetCardImageId(Number,IsVisible);
        }

        public int GetValue()
        {
            switch (Number)
            {
                case SkyjoCardNumber.Minus2:
                    return -2;
                case SkyjoCardNumber.Minus1:
                    return -1;
                case SkyjoCardNumber.Zero:
                    return 0;
                case SkyjoCardNumber.Plus1:
                    return 1;
                case SkyjoCardNumber.Plus2:
                    return 2;
                case SkyjoCardNumber.Plus3:
                    return 3;
                case SkyjoCardNumber.Plus4:
                    return 4;
                case SkyjoCardNumber.Plus5:
                    return 5;
                case SkyjoCardNumber.Plus6:
                    return 6;
                case SkyjoCardNumber.Plus7:
                    return 7;
                case SkyjoCardNumber.Plus8:
                    return 8;
                case SkyjoCardNumber.Plus9:
                    return 9;
                case SkyjoCardNumber.Plus10:
                    return 10;
                case SkyjoCardNumber.Plus11:
                    return 11;
                case SkyjoCardNumber.Plus12:
                    return 12;
                case SkyjoCardNumber.Placeholder:
                default:
                    return 0;
            }
        }

        public static SkyjoCard[] GetAllCards()
        {

            SkyjoCard[] cards = new SkyjoCard[150];

            int fill = 0;
            foreach (SkyjoCardNumber number in Enum.GetValues(typeof(SkyjoCardNumber)))
            {
                if (number == SkyjoCardNumber.Placeholder) continue;

                for (int j = 0; j < SkyjoCard.GetInitAmountOfCard(number); j++)
                {
                    cards[fill] = new SkyjoCard(number, false);
                    fill++;
                }
            }

            return cards;
        }

        static private int GetInitAmountOfCard(SkyjoCardNumber number)
        {
            switch (number)
            {
                case SkyjoCardNumber.Minus2:
                    return 5;
                case SkyjoCardNumber.Zero:
                    return 15;
                default:
                    return 10;
            }
        }

        public override int GetHashCode()
        {
            return ((int)Number << 1) + (IsVisible ? 1 : 0);
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType()) return false;

            SkyjoCard othercard = (SkyjoCard)obj;

            if (!IsVisible || !othercard.IsVisible) return false;
            return Number == othercard.Number;
        }
    }

    public enum SkyjoCardNumber
    {
        Placeholder,
        Minus2,
        Minus1,
        Zero,
        Plus1,
        Plus2,
        Plus3,
        Plus4,
        Plus5,
        Plus6,
        Plus7,
        Plus8,
        Plus9,
        Plus10,
        Plus11,
        Plus12
    }
}