namespace Skyjoo.GameLogic
{
    public class SkyjoPlayer
    {
        public string Ip;
        public string Name;
        public int Points;
        public int InitRevealedFields;
        public bool Active;
        public SkyjoPlayerField PlayingField;

        public SkyjoPlayer(string ip, string name)
        {
            Ip = ip;
            Name = name;
            Points = 0;
            InitRevealedFields = 2;
            Active = true;
            PlayingField = new SkyjoPlayerField();

        }
    }
}