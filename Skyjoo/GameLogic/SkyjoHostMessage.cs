namespace Skyjoo.GameLogic
{
    public class SkyjoHostMessage
    {
        public SkyjoHostMessageType Type;

        public object Content;

        public SkyjoHostMessage(SkyjoHostMessageType type, object content)
        {
            Type = type;
            Content = content;
        }
    }

    public class SkyjoHostGameStartMessage
    {
        public SkyjoGameInfo Info;

        public SkyjoHostGameStartMessage(SkyjoGameInfo info)
        {
            Info = info;
        }
    }

    public class SkyjoHostFieldUpdateMessage
    {
        public int PlayerIndex;
        public int FieldIndex;
        public FieldUpdateType Type;
        public SkyjoHostFieldUpdateMessage(int playerIndex, int fieldIndex, FieldUpdateType type)
        {
            PlayerIndex = playerIndex;
            FieldIndex = fieldIndex;
            Type = type;
        }
    }

    public class SkyjoHostGameStopMessage
    {
        public int WinnerIndex;

        public SkyjoHostGameStopMessage(int winnerIndex)
        {
            WinnerIndex = winnerIndex;
        }
    }

    public class SkyjoHostShuffleNumberMessage
    {
        public int ShuffleNumber;

        public SkyjoHostShuffleNumberMessage(int shuffleNumber)
        {
            ShuffleNumber = shuffleNumber;
        }
    }

    public class SkyjoHostPlayerLogoutMessage
    {
        public string Ip;

        public SkyjoHostPlayerLogoutMessage(string ip)
        {
            Ip = ip;
        }
    }
    public class SkyjoHostGameRestartMessage
    {
        public int StackSeed;

        public SkyjoHostGameRestartMessage(int stackSeed)
        {
            StackSeed = stackSeed;
        }
    }

    public enum SkyjoHostMessageType
    {
        GameStart,
        FieldUpdate,
        ShuffleNumber,
        PlayerLogout,
        GameRestart
    }
}