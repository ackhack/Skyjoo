namespace Skyjoo.GameLogic
{
    public class SkyjoPlayerField
    {
        public int width;
        public int height;
        public SkyjoPlayerField(int width, int height)
        {
            CurrentCard = new SkyjoCard(SkyjoCardNumber.Placeholder, true);
            this.width = width;
            this.height = height;
        }

        public SkyjoCard[] FieldCards;
        public SkyjoCard CurrentCard;

        public void ClearRows()
        {
            for (int i = 0; i < width; i++)
            {
                SkyjoCardNumber firstCard = FieldCards[i].Number;
                if (firstCard == SkyjoCardNumber.Placeholder)
                    continue;

                bool canClear = true;

                for (int j = 0; j < height; j++)
                {
                    var currIndex = i + j * width;
                    if (!FieldCards[currIndex].IsVisible)
                    {
                        canClear = false;
                        break;
                    }
                    if (j > 0)
                    {
                        if (firstCard != FieldCards[currIndex].Number)
                        {
                            canClear = false;
                            break;
                        }
                    }
                }

                if (canClear)
                {
                    for (int j = 0; j < height; j++)
                    {
                        var currIndex = i + j * width;
                        FieldCards[currIndex] = new SkyjoCard(SkyjoCardNumber.Placeholder, true);
                    }
                }
            }
        }
    }
}