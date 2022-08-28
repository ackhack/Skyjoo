using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Skyjoo.Images;
using System;
using System.Net;
using System.Net.Sockets;
using System.Resources;
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

            DependencyClass.StorageHandler = new Storage.StorageHandler();

            var btnServer = FindViewById<Button>(Resource.Id.btnServer);

            var btnClient = FindViewById<Button>(Resource.Id.btnClient);

            var nameBox = FindViewById<EditText>(Resource.Id.textName);
            nameBox.Text = DependencyClass.StorageHandler.GetName();

            var iconPackSpinner = FindViewById<Spinner>(Resource.Id.spinner_iconpack);
            iconPackSpinner.Adapter = new IconPackAdapter(this);
            iconPackSpinner.ItemSelected += onIconPackSpinnerItemSelected;
            iconPackSpinner.SetSelection((int)DependencyClass.StorageHandler.GetIconPack());

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
                    DependencyClass.StorageHandler.SetIconPack((IconPack)iconPackSpinner.SelectedItemPosition);
                    DependencyClass.StorageHandler.SetName(nameBox.Text);
                    DependencyClass.Playername = nameBox.Text;
                    StartActivity(typeof(ServerActivity));
                }
            };

            btnClient.Click += (sender, e) =>
            {
                if (nameBox.Text.Length > 0)
                {
                    DependencyClass.StorageHandler.SetIconPack((IconPack)iconPackSpinner.SelectedItemPosition);
                    DependencyClass.StorageHandler.SetName(nameBox.Text);
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
