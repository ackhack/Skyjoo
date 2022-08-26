using Android.App;
using Android.Bluetooth;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Skyjoo.GameLogic.Images;
using System;
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Threading.Tasks;
using Xamarin.Essentials;
using static Android.Icu.Text.Transliterator;
using static Android.Webkit.WebSettings;
using static Java.Util.Jar.Attributes;

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

            var iconPackSpinner = FindViewById<Spinner>(Resource.Id.spinner_iconpack);
            iconPackSpinner.Adapter = new IconPackAdapter(this);
            iconPackSpinner.ItemSelected += onIconPackSpinnerItemSelected;
            iconPackSpinner.SetSelection((int)getIconPack());

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
                    setIconPack((IconPack)iconPackSpinner.SelectedItemPosition);
                    setName(nameBox.Text);
                    DependencyClass.Playername = nameBox.Text;
                    StartActivity(typeof(ServerActivity));
                }
            };

            btnClient.Click += (sender, e) =>
            {
                if (nameBox.Text.Length > 0)
                {
                    setIconPack((IconPack)iconPackSpinner.SelectedItemPosition);
                    setName(nameBox.Text);
                    DependencyClass.Playername = nameBox.Text;
                    StartActivity(typeof(ClientActivity));
                }
            };
        }

        private void onIconPackSpinnerItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            DependencyClass.IconPack = (IconPack)e.Position;
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
        private void setIconPack(IconPack pack)
        {
            SecureStorage.SetAsync("iconPack", pack.ToString());
        }

        private IconPack getIconPack()
        {
            try
            {
                return (IconPack)Enum.Parse(typeof(IconPack), SecureStorage.GetAsync("iconPack").Result);
            }
            catch (Exception)
            {
                return IconPack.Default;
            }
        }
    }

    class IconPackAdapter : BaseAdapter
    {
        private Activity activity;
        public override int Count => Enum.GetNames(typeof(IconPack)).Length;

        public IconPackAdapter(Activity activity)
        {
            this.activity = activity;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return Enum.GetNames(typeof(IconPack))[position];
        }

        public override long GetItemId(int position)
        {
            return 0;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            TextView imageView = new TextView(activity);
            imageView.Text = Enum.GetNames(typeof(IconPack))[position];
            imageView.TextSize = 16;
            return imageView;
        }
    }
}
