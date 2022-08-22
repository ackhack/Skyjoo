namespace Skyjoo.GameLogic
{
    public class SkyjoPlayerField
    {
        public SkyjoPlayerField()
        {
            CurrentCard = new SkyjoCard(SkyjoCardNumber.Placeholder, true);
        }

        public SkyjoCard[] FieldCards;
        public SkyjoCard CurrentCard;
    }
}