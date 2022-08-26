using Java.Lang;

namespace Skyjoo.GameLogic.Images
{
    public class ImageMapping
    {
        public IconPack IconPack { get; set; }
        public ImageMapping(IconPack iconPack)
        {
            IconPack = iconPack;
        }

        public int GetPlaceHolderImageId()
        {
            switch (IconPack)
            {
                case IconPack.Beer:
                    return Resource.Drawable.beer_IconPlaceholder;
                case IconPack.Default:
                default:
                    return Resource.Drawable.default_IconPlaceholder;
            }
        }

        public int GetCardImageId(SkyjoCardNumber number, bool isVisible)
        {
            switch (IconPack)
            {
                case IconPack.Beer:
                    return getBeerImageId(number, isVisible);
                case IconPack.Default:
                default:
                    return getDefaultImageId(number, isVisible);
            }
        }

        private int getDefaultImageId(SkyjoCardNumber number, bool isVisible)
        {
            if (!isVisible) return Resource.Drawable.default_IconBack;
            switch (number)
            {
                case SkyjoCardNumber.Minus2:
                    return Resource.Drawable.default_IconM2;
                case SkyjoCardNumber.Minus1:
                    return Resource.Drawable.default_IconM1;
                case SkyjoCardNumber.Zero:
                    return Resource.Drawable.default_Icon0;
                case SkyjoCardNumber.Plus1:
                    return Resource.Drawable.default_Icon1;
                case SkyjoCardNumber.Plus2:
                    return Resource.Drawable.default_Icon2;
                case SkyjoCardNumber.Plus3:
                    return Resource.Drawable.default_Icon3;
                case SkyjoCardNumber.Plus4:
                    return Resource.Drawable.default_Icon4;
                case SkyjoCardNumber.Plus5:
                    return Resource.Drawable.default_Icon5;
                case SkyjoCardNumber.Plus6:
                    return Resource.Drawable.default_Icon6;
                case SkyjoCardNumber.Plus7:
                    return Resource.Drawable.default_Icon7;
                case SkyjoCardNumber.Plus8:
                    return Resource.Drawable.default_Icon8;
                case SkyjoCardNumber.Plus9:
                    return Resource.Drawable.default_Icon9;
                case SkyjoCardNumber.Plus10:
                    return Resource.Drawable.default_Icon10;
                case SkyjoCardNumber.Plus11:
                    return Resource.Drawable.default_Icon11;
                case SkyjoCardNumber.Plus12:
                    return Resource.Drawable.default_Icon12;
                case SkyjoCardNumber.Placeholder:
                default:
                    return Resource.Drawable.default_IconPlaceholder;
            }
        }


        private int getBeerImageId(SkyjoCardNumber number, bool isVisible)
        {
            if (!isVisible) return Resource.Drawable.beer_IconBack;
            switch (number)
            {
                case SkyjoCardNumber.Minus2:
                    return Resource.Drawable.beer_IconM2;
                case SkyjoCardNumber.Minus1:
                    return Resource.Drawable.beer_IconM1;
                case SkyjoCardNumber.Zero:
                    return Resource.Drawable.beer_Icon0;
                case SkyjoCardNumber.Plus1:
                    return Resource.Drawable.beer_Icon1;
                case SkyjoCardNumber.Plus2:
                    return Resource.Drawable.beer_Icon2;
                case SkyjoCardNumber.Plus3:
                    return Resource.Drawable.beer_Icon3;
                case SkyjoCardNumber.Plus4:
                    return Resource.Drawable.beer_Icon4;
                case SkyjoCardNumber.Plus5:
                    return Resource.Drawable.beer_Icon5;
                case SkyjoCardNumber.Plus6:
                    return Resource.Drawable.beer_Icon6;
                case SkyjoCardNumber.Plus7:
                    return Resource.Drawable.beer_Icon7;
                case SkyjoCardNumber.Plus8:
                    return Resource.Drawable.beer_Icon8;
                case SkyjoCardNumber.Plus9:
                    return Resource.Drawable.beer_Icon9;
                case SkyjoCardNumber.Plus10:
                    return Resource.Drawable.beer_Icon10;
                case SkyjoCardNumber.Plus11:
                    return Resource.Drawable.beer_Icon11;
                case SkyjoCardNumber.Plus12:
                    return Resource.Drawable.beer_Icon12;
                case SkyjoCardNumber.Placeholder:
                default:
                    return Resource.Drawable.beer_IconPlaceholder;
            }
        }
    }
}