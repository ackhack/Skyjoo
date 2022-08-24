using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using NetworkCommunication.Core;
using Newtonsoft.Json;
using Skyjoo.GameLogic;
using System;
using System.Text;

namespace Skyjoo
{
    [Activity(Label = "Client", MainLauncher = false, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ClientActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.client_layout);

            lblConnectStatus = FindViewById<TextView>(Resource.Id.lblConnectStatus);
            txtIPAddress = FindViewById<EditText>(Resource.Id.txtIPAddress);
            txtIPAddress.Text = DependencyClass.LocalIp;
            txtPort = FindViewById<EditText>(Resource.Id.txtPort);
            btnConnect = FindViewById<Button>(Resource.Id.btnConnect);
            btnConnect.Click += BtnConnect_Click;

            socketClient = new SocketClient();
            socketClient.StateChanged += SocketClient_StateChanged;
            DependencyClass.Client = socketClient;
        }

        protected void BtnConnect_Click(object sender, EventArgs e)
        {
            if (socketClient.State == SocketClientState.Disconnected)
            {
                if (int.TryParse(txtPort.Text, out int port))
                {
                    socketClient.Connect(txtIPAddress.Text, port);
                }
            }
            else
            {
                socketClient.Disconnect();
            }
        }

        protected void SocketClient_StateChanged(object sender, SocketClientState state)
        {
            RunOnUiThread(() =>
                {
                    switch (state)
                    {
                        case SocketClientState.Connected:

                            var loginMessage = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new SkyjoClientMessage(SkyjoClientMessageType.Login, new SkyjoClientLoginMessage(DependencyClass.Playername))));

                            socketClient.SendMessage(loginMessage);
                            StartActivity(typeof(GameActivity));
                            lblConnectStatus.Text = "Connected";

                            btnConnect.Text = "Disconnect";

                            lblConnectStatus.SetTextColor(Color.Green);

                            break;

                        case SocketClientState.Disconnected:

                            lblConnectStatus.Text = "Disconnected";

                            btnConnect.Text = "Connect";

                            lblConnectStatus.SetTextColor(Color.Red);

                            break;
                    }
                });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (socketClient != null)
            {
                if (socketClient.State != SocketClientState.Disconnected)
                {
                    var logout = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new SkyjoClientMessage(SkyjoClientMessageType.Login, new SkyjoClientLogoutMessage(DependencyClass.LocalIp))));

                    socketClient.SendMessage(logout);
                    socketClient.Disconnect();
                }

                socketClient.StateChanged -= SocketClient_StateChanged;
            }
        }

        private TextView lblConnectStatus;

        private EditText txtPort;

        private EditText txtIPAddress;

        private Button btnConnect;

        private SocketClient socketClient;
    }
}


