namespace Skyjoo.GameLogic
{
    public class SkyjoClientMessage
    {
        public SkyjoClientMessageType Type;

        public object Content;

        public SkyjoClientMessage(SkyjoClientMessageType type, object content)
        {
            Type = type;
            Content = content;
        }
    }

    public class SkyjoClientLoginMessage
    {
        public string UserName;

        public SkyjoClientLoginMessage(string userName)
        {
            UserName = userName;
        }
    }

    public class SkyjoClientFieldUpdateMessage
    {
        public int PlayerIndex;
        public int FieldIndex;
        public FieldUpdateType Type;
        public SkyjoClientFieldUpdateMessage(int playerIndex, int fieldIndex, FieldUpdateType type)
        {
            PlayerIndex = playerIndex;
            FieldIndex = fieldIndex;
            Type = type;
        }
    }

    public class SkyjoClientLogoutMessage
    {
        public string Ip;

        public SkyjoClientLogoutMessage(string ip)
        {
            Ip = ip;
        }
    }

    public enum SkyjoClientMessageType
    {
        Login,
        FieldUpdate,
        Logout
    }
}