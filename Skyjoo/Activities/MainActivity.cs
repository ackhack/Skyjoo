using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using System;
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Threading.Tasks;
using Xamarin.Essentials;

[assembly: NeutralResourcesLanguage("en-US")]
namespace Skyjoo
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            var btnServer = FindViewById<Button>(Resource.Id.btnServer);

            var btnClient = FindViewById<Button>(Resource.Id.btnClient);

            var nameBox = FindViewById<EditText>(Resource.Id.textName);
            nameBox.Text = getName().Result;

            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    DependencyClass.LocalIp = endPoint.Address.ToString();
                }
            }
            catch (Exception)
            {
                DependencyClass.LocalIp = "127.0.0.1";
            }

            btnServer.Click += (sender, e) =>
            {
                if (nameBox.Text.Length > 0)
                {
                    setName(nameBox.Text);
                    DependencyClass.Playername = nameBox.Text;
                    StartActivity(typeof(ServerActivity));
                }
            };

            btnClient.Click += (sender, e) =>
            {
                if (nameBox.Text.Length > 0)
                {
                    setName(nameBox.Text);
                    DependencyClass.Playername = nameBox.Text;
                    StartActivity(typeof(ClientActivity));
                }
            };
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void setName(string name)
        {
            if (name != DeviceInfo.Name)
                SecureStorage.SetAsync("username", name);
        }

        async private Task<string> getName()
        {
            try
            {
                return await SecureStorage.GetAsync("username") ?? DeviceInfo.Name;
            }
            catch (Exception)
            {
                return DeviceInfo.Name;
            }
        }
    }
}
