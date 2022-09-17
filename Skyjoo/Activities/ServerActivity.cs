using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using NetworkCommunication.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skyjoo.Dependency;
using Skyjoo.GameLogic;
using Skyjoo.ReOrderView;
using System;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace Skyjoo
{
    [Activity(Label = "Server", MainLauncher = false, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ServerActivity : Activity, TextView.IOnEditorActionListener, IOnStartDragListener
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.server_layout);

            PlayerList = new ObservableCollection<ReOrderListItem>();
            playerRecycler = FindViewById<RecyclerView>(Resource.Id.recyclerPlayers);
            var resourceAdapter = new ReOrderAdapters(PlayerList, this);
            playerRecycler = FindViewById<RecyclerView>(Resource.Id.recyclerPlayers);
            playerRecycler.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Vertical, false));
            playerRecycler.SetAdapter(resourceAdapter);
            playerRecycler.HasFixedSize = true;

            ItemTouchHelper.Callback callback = new SimpleItemTouchHelperCallback(resourceAdapter);
            playerItemTouchHelper = new ItemTouchHelper(callback);
            playerItemTouchHelper.AttachToRecyclerView(playerRecycler);

            lblServerStatus = FindViewById<TextView>(Resource.Id.lblServerStatus);
            txtPort = FindViewById<EditText>(Resource.Id.txtPort);
            btnStartGame = FindViewById<Button>(Resource.Id.btnStartGame);
            btnStartGame.Click += BtnStartGame_Click;
            txtPort.SetOnEditorActionListener(this);

            var ipTV = FindViewById<TextView>(Resource.Id.textHostIp);
            ipTV.Text = DependencyClass.LocalIp;

            socketServer = new SocketServer(8);
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
                        AddPlayer(senderIp, loginMessage.UserName);
                        break;
                    case SkyjoClientMessageType.FieldUpdate:
                        //game hasnt started yet
                        break;
                    case SkyjoClientMessageType.Logout:
                        if (DependencyClass.PlayerLogins.TryGetValue(senderIp, out var name))
                        {
                            RemovePlayer(senderIp, name);
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
                foreach (var item in PlayerList.Reverse())
                {
                    DependencyClass.PlayerLogins.Add(item.Ip, item.Name);
                }
                StartActivity(typeof(GameActivity));
                socketServer.ReceivedMessage -= SocketServer_ReceivedMessage;
            }
        }

        protected void AddPlayer(string ip,string name)
        {
            PlayerList.Add(new ReOrderListItem(ip,name));
            RunOnUiThread(() =>
            {
                playerRecycler.GetAdapter().NotifyDataSetChanged();
            });
        }

        protected void RemovePlayer(string ip, string name)
        {
            foreach (var item in PlayerList)
            {
                if (item.Ip == ip)
                {
                    PlayerList.Remove(item);
                    break;
                }
            }
            RunOnUiThread(() =>
            {
                playerRecycler.GetAdapter().NotifyDataSetChanged();
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

                PlayerList.Clear();
                DependencyClass.Client = null;
                DependencyClass.Server = null;
                DependencyClass.PlayerLogins = new Dictionary<string, string>();
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

        public void OnStartDrag(RecyclerView.ViewHolder viewHolder)
        {
            playerItemTouchHelper.StartDrag(viewHolder);
        }

        private RecyclerView playerRecycler;

        public ObservableCollection<ReOrderListItem> PlayerList;

        private ItemTouchHelper playerItemTouchHelper;

        private TextView lblServerStatus;

        private EditText txtPort;

        private Button btnStartGame;

        private SocketServer socketServer;

        private SocketClient socketClient;

        private int port;
    }
}


