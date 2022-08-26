using NetworkCommunication.Core;
using System.Collections.Generic;
using Skyjoo.GameLogic.Images;

namespace Skyjoo
{
    internal class DependencyClass
    {
        public static SocketClient Client;
        public static SocketServer Server;
        public static string Playername;
        public static string LocalIp;
        public static Dictionary<string, string> PlayerLogins = new Dictionary<string, string>();
        public static ImageHandler ImageHandler;
        public static IconPack IconPack = IconPack.Default;
    }
}