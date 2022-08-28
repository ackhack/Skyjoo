using Skyjoo.GameLogic;

namespace Skyjoo.Images
{
    public class ImageHandler
    {
        private ImageMapping ImageMapping { get; set; }

        public ImageHandler(IconPack iconPack)
        {
            ImageMapping = new ImageMapping(iconPack);
        }

        public int GetPlaceholderImageId()
        {
            return ImageMapping.GetPlaceHolderImageId();
        }

        public int GetCardImageId(SkyjoCardNumber cardNumber, bool isVisible)
        {
            return ImageMapping.GetCardImageId(cardNumber, isVisible);
        }
    }

    public enum IconPack
    {
        Default,
        Beer
    }
}