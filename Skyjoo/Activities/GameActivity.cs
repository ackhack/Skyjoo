using Android.App;
using Android.Content.PM;
using Android.OS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skyjoo.Dependency;
using Skyjoo.GameLogic;
using System;
using System.Text;

namespace Skyjoo
{
    [Activity(Label = "Game", MainLauncher = false, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class GameActivity : Activity
    {

        private SkyjoBoard board;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.waiting_layout);
            DependencyClass.Client.HostMessageRecieved += ClientRecievedHostMessage;
            if (DependencyClass.Server != null)
            {
                DependencyClass.Server.ReceivedMessage += ServerReceivedMessage;
                initGameAsHost();
            }
        }

        private void ServerReceivedMessage(object sender, NetworkCommunication.Core.MessageReceivedEventArgs e)
        {
            var senderIp = e.Host.Address;
            var clientMessages = JsonConvert.DeserializeObject<SkyjoClientMessage[]>("[" + Encoding.UTF8.GetString(e.Message).Replace("}{", "},{") + "]");
            foreach (var clientMessage in clientMessages)
            {
                var content = (JContainer)clientMessage.Content;
                System.Diagnostics.Debug.WriteLine("Server: " + clientMessage.Type);

                switch (clientMessage.Type)
                {
                    case SkyjoClientMessageType.Login:
                        //ignore as game has already started
                        break;
                    case SkyjoClientMessageType.FieldUpdate:
                        var clientFieldUpdateMessage = content.ToObject<SkyjoClientFieldUpdateMessage>();
                        if (clientFieldUpdateMessage.Type == FieldUpdateType.ShuffleStack)
                        {
                            var shuffleMessage = JsonConvert.SerializeObject(new SkyjoHostMessage(SkyjoHostMessageType.PlayerLogout, new SkyjoHostShuffleNumberMessage(new Random().Next())));
                            DependencyClass.Server.SendMessageToClients(shuffleMessage);
                            return;
                        }
                        System.Diagnostics.Debug.WriteLine("Server Player " + clientFieldUpdateMessage.PlayerIndex + " Field " + clientFieldUpdateMessage.FieldIndex);
                        var hostFieldUpdateMessage = JsonConvert.SerializeObject(new SkyjoHostMessage(SkyjoHostMessageType.FieldUpdate, new SkyjoHostFieldUpdateMessage(clientFieldUpdateMessage.PlayerIndex, clientFieldUpdateMessage.FieldIndex, clientFieldUpdateMessage.Type)));
                        DependencyClass.Server.SendMessageToClients(hostFieldUpdateMessage);
                        break;
                    case SkyjoClientMessageType.Logout:
                        var logoutMessage = JsonConvert.SerializeObject(new SkyjoHostMessage(SkyjoHostMessageType.PlayerLogout, new SkyjoHostPlayerLogoutMessage(senderIp)));
                        DependencyClass.Server.SendMessageToClients(logoutMessage);
                        break;
                }
            }
        }

        private void ClientRecievedHostMessage(object sender, NetworkCommunication.Core.MessageReceivedEventArgs e)
        {
            var hostMessages = JsonConvert.DeserializeObject<SkyjoHostMessage[]>("[" + Encoding.UTF8.GetString(e.Message).Replace("}{", "},{") + "]");
            foreach (var hostMessage in hostMessages)
            {
                var content = (JContainer)hostMessage.Content;

                switch (hostMessage.Type)
                {
                    case SkyjoHostMessageType.GameStart:
                        RunOnUiThread(() =>
                        {
                            SetContentView(Resource.Layout.game_layout);
                        });
                        var gameStartMessage = content.ToObject<SkyjoHostGameStartMessage>();
                        startGameAsClient(gameStartMessage.Info);
                        break;
                    case SkyjoHostMessageType.FieldUpdate:
                        var fieldUpdateMessage = content.ToObject<SkyjoHostFieldUpdateMessage>();
                        System.Diagnostics.Debug.WriteLine("Client Player " + fieldUpdateMessage.PlayerIndex + " Field " + fieldUpdateMessage.FieldIndex);
                        board.HandleFieldUpdate(fieldUpdateMessage.PlayerIndex, fieldUpdateMessage.FieldIndex, fieldUpdateMessage.Type);
                        break;
                    case SkyjoHostMessageType.ShuffleNumber:
                        var shuffleNumberMessage = content.ToObject<SkyjoHostShuffleNumberMessage>();
                        board.HandleStackShuffle(shuffleNumberMessage.ShuffleNumber);
                        break;
                    case SkyjoHostMessageType.PlayerLogout:
                        var playerLogoutMessage = content.ToObject<SkyjoHostPlayerLogoutMessage>();
                        board.HandlePlayerLogout(playerLogoutMessage.Ip);
                        break;
                    case SkyjoHostMessageType.GameRestart:
                        var restartMessage = content.ToObject<SkyjoHostGameRestartMessage>();
                        board.HandleGameRestart(restartMessage.StackSeed);
                        break;
                }
            }
        }

        private void initGameAsHost()
        {
            var gameInfo = SkyjoGameInfoCreater.CreateGameInfo(DependencyClass.PlayerLogins);

            var message = new SkyjoHostMessage(SkyjoHostMessageType.GameStart, new SkyjoHostGameStartMessage(gameInfo));

            System.Diagnostics.Debug.WriteLine("Sending Start to Clients");
            DependencyClass.Server.SendMessageToClients(JsonConvert.SerializeObject(message));
        }

        private void startGameAsClient(SkyjoGameInfo info)
        {
            startGame(info);
        }

        private void startGame(SkyjoGameInfo info)
        {
            DependencyClass.ImageHandler = new Images.ImageHandler(DependencyClass.IconPack);
            int i = 0;
            foreach (var value in info.Players)
            {
                if (value.Key.StartsWith(DependencyClass.LocalIp))
                {
                    board = new SkyjoBoard(this, i);
                    board.FieldUpdated += Board_FieldUpdated;
                    board.InitGame(info);
                    break;
                }
                i++;
            }
        }

        public void RestartGame()
        {
            var message = new SkyjoHostMessage(SkyjoHostMessageType.GameRestart, new SkyjoHostGameRestartMessage(new Random().Next()));

            System.Diagnostics.Debug.WriteLine("Sending Restart to Clients");
            DependencyClass.Server.SendMessageToClients(JsonConvert.SerializeObject(message));
        }

        private void Board_FieldUpdated(object sender, FieldUpdateEventArgs e)
        {
            var message = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new SkyjoClientMessage(SkyjoClientMessageType.FieldUpdate, new SkyjoClientFieldUpdateMessage(e.PlayerIndex, e.FieldIndex, e.Type))));
            DependencyClass.Client.SendMessage(message);
        }
    }
}