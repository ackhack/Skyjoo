using Skyjoo.GameLogic.Bots;

namespace Skyjoo.GameLogic
{
    public class SkyjoPlayer
    {
        public string Ip;
        public string Name;
        public int Points;
        public int InitRevealedFields;
        public bool IsActive;
        public bool IsBot;
        public BaseBot Bot;
        public SkyjoPlayerField PlayingField;

        public SkyjoPlayer(string ip, string name, int fieldWidth, int fieldHeight)
        {
            Ip = ip;
            Name = name;
            Points = 0;
            InitRevealedFields = 2;
            IsActive = true;
            IsBot = false;
            PlayingField = new SkyjoPlayerField(fieldWidth, fieldHeight);

        }
    }
}