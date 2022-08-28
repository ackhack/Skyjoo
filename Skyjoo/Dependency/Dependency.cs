using NetworkCommunication.Core;
using System.Collections.Generic;
using Skyjoo.Images;
using Skyjoo.Storage;

namespace Skyjoo.Dependency
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
        public static StorageHandler StorageHandler;
    }
}