using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using NetworkCommunication.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skyjoo.GameLogic;
using System;
using System.Text;

namespace Skyjoo
{
    [Activity(Label = "Server", MainLauncher = false, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ServerActivity : Activity, TextView.IOnEditorActionListener
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.server_layout);


            lblServerStatus = FindViewById<TextView>(Resource.Id.lblServerStatus);

            txtPort = FindViewById<EditText>(Resource.Id.txtPort);

            btnStartGame = FindViewById<Button>(Resource.Id.btnStartGame);


            txtMessages = FindViewById<TextView>(Resource.Id.txtMessages);

            btnStartGame.Click += BtnStartGame_Click;

            txtPort.SetOnEditorActionListener(this);

            var ipTV = FindViewById<TextView>(Resource.Id.textHostIp);

            ipTV.Text = DependencyClass.LocalIp;

            socketServer = new SocketServer();

            socketServer.StateChanged += SocketServer_StateChanged;

            DependencyClass.Server = socketServer;

            port = int.Parse(Resources.GetString(Resource.String.default_port));

            int.TryParse(txtPort.Text, out port);

            socketServer.Run(port);
            socketServer.ReceivedMessage += SocketServer_ReceivedMessage;
        }

        private void SocketServer_ReceivedMessage(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                var senderIp = e.Host.Address;
                var clientMessage = JsonConvert.DeserializeObject<SkyjoClientMessage>(Encoding.UTF8.GetString(e.Message));
                var content = (JContainer)clientMessage.Content;
                System.Diagnostics.Debug.WriteLine(clientMessage.Type);

                switch (clientMessage.Type)
                {
                    case SkyjoClientMessageType.Login:
                        var loginMessage = content.ToObject<SkyjoClientLoginMessage>();
                        DependencyClass.PlayerLogins.Add(senderIp, loginMessage.UserName);
                        AddMessage(string.Format(Resources.GetString(Resource.String.login_message), loginMessage.UserName));
                        break;
                    case SkyjoClientMessageType.FieldUpdate:
                        //game hasnt started yet
                        break;
                    case SkyjoClientMessageType.Logout:
                        if (DependencyClass.PlayerLogins.TryGetValue(senderIp, out var user))
                        {
                            AddMessage(string.Format(Resources.GetString(Resource.String.logout_message), user));
                            DependencyClass.PlayerLogins.Remove(senderIp);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

        }

        protected void BtnStartGame_Click(object sender, EventArgs e)
        {
            if (socketServer.State == SocketServerState.Running && socketClient.State == SocketClientState.Connected)
            {
                StartActivity(typeof(GameActivity));
                socketServer.ReceivedMessage -= SocketServer_ReceivedMessage;
            }
        }

        protected void AddMessage(string message)
        {
            RunOnUiThread(() =>
                {
                    txtMessages.Text = txtMessages.Text.Insert(0, string.Format("{0}\n", message));
                });
        }

        protected void SocketServer_StateChanged(object sender, SocketServerState state)
        {
            RunOnUiThread(() =>
                {
                    switch (state)
                    {
                        case SocketServerState.Starting:

                            lblServerStatus.Text = "Starting";

                            lblServerStatus.SetTextColor(Color.Green);

                            break;

                        case SocketServerState.Running:

                            socketClient = new SocketClient();
                            socketClient.Connect(DependencyClass.LocalIp, port);
                            socketClient.StateChanged += SocketClient_StateChanged;
                            DependencyClass.Client = socketClient;

                            lblServerStatus.Text = "Running";

                            lblServerStatus.SetTextColor(Color.Green);

                            break;

                        case SocketServerState.Stopped:

                            lblServerStatus.Text = "Stopped";

                            lblServerStatus.SetTextColor(Color.Red);

                            break;
                    }
                });
        }

        protected void SocketClient_StateChanged(object sender, SocketClientState state)
        {
            if (state == SocketClientState.Connected)
            {
                var loginMessage = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new SkyjoClientMessage(SkyjoClientMessageType.Login, new SkyjoClientLoginMessage(DependencyClass.Playername))));

                socketClient.SendMessage(loginMessage);
                socketClient.StateChanged -= SocketClient_StateChanged;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (socketServer != null)
            {
                if (socketServer.State != SocketServerState.Stopped)
                {
                    socketServer.Stop();
                }

                socketServer.StateChanged -= SocketServer_StateChanged;
            }
        }

        public bool OnEditorAction(TextView v, ImeAction actionId, KeyEvent e)
        {
            if (actionId == ImeAction.Done)
            {
                var inputMethodManager = (InputMethodManager)Application.Context.GetSystemService(Context.InputMethodService);

                inputMethodManager.HideSoftInputFromWindow(v.WindowToken, 0);

                return true;
            }

            return false;
        }

        private TextView txtMessages;

        private TextView lblServerStatus;

        private EditText txtPort;

        private Button btnStartGame;

        private SocketServer socketServer;

        private SocketClient socketClient;

        private int port;
    }
}


