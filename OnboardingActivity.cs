using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Google.Android.Material.TextField;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using AndroidX.AppCompat.App;
using System.Linq;
using System.Text;
using System.IO;
using Android.Bluetooth;
using Android.Content.PM;

//Use UserData.DataStruct for passing username and password to pi

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    [Activity(Label = "OnboardingActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class OnboardingActivity : AppCompatActivity
    {
        //Do something with this        
        public static DeviceListViewAdapter btUIAdapter;
        public static WifiListViewAdapter wifiListViewAdapter;
        public static ListView viewableDevices;
        public static ListView viewableNetworks;
        Button refreshButton;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.bluetoothsearch);
            viewableDevices = FindViewById<ListView>(Resource.Id.devices);
            refreshButton = FindViewById<Button>(Resource.Id.RefreshBluetooth);

            refreshButton.Click += RestartDiscovery;

            // Create your application here
            BluetoothManager.adapter = BluetoothAdapter.DefaultAdapter;
            BluetoothManager.SwitchToWifi += DiscoveryFinished;
            if (!BluetoothManager.adapter.IsEnabled)
                TurnOnYourBluetooth();
            else
            {
                StartDiscovery();
            }
        }
        private void TurnOnYourBluetooth()
        {
            //Inform user that they need to turn on their Bluetooth.
        }

        private void StartDiscovery()
        {
            if (!BluetoothManager.adapter.IsDiscovering)
            {
                Intent intent = new Intent(BluetoothAdapter.ActionRequestEnable);
                bool hasPerms = false;
                StartActivityForResult(intent, 1);
                while (!hasPerms)
                {
                    hasPerms = CheckPerms();
                }
                DiscoverBluetooth();
            }
        }

        private void DiscoverBluetooth()
        {
            IntentFilter filter = new IntentFilter();
            filter.AddAction(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            BluetoothManager.receiver = new BluetoothManager.MyBTReceiver();
            RegisterReceiver(BluetoothManager.receiver, filter);
            BluetoothManager.adapter.StartDiscovery();
            Toast.MakeText(this, "Bluetooth started", ToastLength.Short).Show();
        }

        private void RestartDiscovery(object sender, EventArgs e)
        {
            BluetoothManager.adapter.CancelDiscovery();
            UnregisterReceiver(BluetoothManager.receiver);
            BluetoothManager.receiver = null;
            BluetoothManager.devices.Clear();
            btUIAdapter = new DeviceListViewAdapter(this, Resource.Layout.BTDeviceListLayout, BluetoothManager.devices);
            viewableDevices.Adapter = btUIAdapter;


            IntentFilter filter = new IntentFilter();
            filter.AddAction(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            BluetoothManager.receiver = new BluetoothManager.MyBTReceiver();
            RegisterReceiver(BluetoothManager.receiver, filter);
            BluetoothManager.adapter.StartDiscovery();
        }

        private void DiscoveryFinished(List<string> networks)
        {
            UnregisterReceiver(BluetoothManager.receiver);
            BluetoothManager.receiver = null;
            //BluetoothManager.devices = e;

            SetContentView(Resource.Layout.wifisearch);
            viewableNetworks = FindViewById<ListView>(Resource.Id.wifiNetworks);
            wifiListViewAdapter = new WifiListViewAdapter(this, Resource.Layout.WifiNetworkListLayout, networks);
            viewableNetworks.Adapter = wifiListViewAdapter;

            WifiListViewAdapter.WifiActivityTime += Wifi;
        }

        private bool CheckPerms()
        {
            if(Build.VERSION.SdkInt > Android.OS.BuildVersionCodes.Lollipop)
            {
                int permissionCheck = (int)CheckSelfPermission("android.permission.ACCESS_FINE_LOCATION");
                permissionCheck += (int)CheckSelfPermission("android.permission.ACCESS_COARSE_LOCATION");
                permissionCheck += (int)CheckSelfPermission("android.permission.BLUETOOTH");
                permissionCheck += (int)CheckSelfPermission("android.permission.BLUETOOTH_ADMIN");
                if(permissionCheck != 0)
                {
                    RequestPermissions(new string[] { "android.permission.ACCESS_FINE_LOCATION", "android.permission.ACCESS_COARSE_LOCATION", "android.permission.BLUETOOTH", "android.permission.BLUETOOTH_ADMIN" }, 0);
                    permissionCheck = (int)CheckSelfPermission("android.permission.ACCESS_FINE_LOCATION");
                    permissionCheck += (int)CheckSelfPermission("android.permission.ACCESS_COARSE_LOCATION");
                    permissionCheck += (int)CheckSelfPermission("android.permission.BLUETOOTH");
                    permissionCheck += (int)CheckSelfPermission("android.permission.BLUETOOTH_ADMIN");
                    if(permissionCheck != 0)
                        return false;
                    else
                        return true;
                }
            }
            return true;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if(requestCode == 0)
            {
                Toast.MakeText(this, "Request granted", ToastLength.Short).Show();
            }
            else
            {
                Toast.MakeText(this, "Request denied", ToastLength.Short).Show();
            }
        }

        private void Wifi(string name)
        {
            Intent i = new Intent(this, typeof(WifiConnection));
            Bundle bundle = new Bundle();
            bundle.PutString("WifiTitle", name);
            i.PutExtras(bundle);
            StartActivityForResult(i, 0);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (data != null)
            {
                if (data.HasExtra("status"))
                {
                    bool creds = data.Extras.GetBoolean("status");
                    if (creds)
                        Toast.MakeText(this, "Board Successfully Connected to Wifi", ToastLength.Short).Show();
                        Finish();
                }
            }
        }
    }
    public class DeviceListViewAdapter : BaseAdapter<BluetoothDevice>
    {
        private LayoutInflater layinf;
        private List<BluetoothDevice> devices;
        private int resourceId;
        Context ctx;

        public DeviceListViewAdapter(Context context, int tvResourceId, List<BluetoothDevice> devices)
        {
            this.devices = devices;
            layinf = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            resourceId = tvResourceId;
            ctx = context;
        }

        public override int Count
        {
            get { return devices.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override BluetoothDevice this[int position]
        {
            get { return devices[position]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            convertView = layinf.Inflate(resourceId, null);
            BluetoothDevice device = devices[position];

            if(device != null)
            {
                TextView deviceName = convertView.FindViewById<TextView>(Resource.Id.deviceName);
                TextView deviceAddress = convertView.FindViewById<TextView>(Resource.Id.deviceAddress);
                Button deviceButton = convertView.FindViewById<Button>(Resource.Id.deviceSelectButton);
                deviceButton.Click += (o, e) =>
                {
                    //Block any other buttons from being clicked
                    Toast.MakeText(ctx, "Test Notification " + position.ToString(), ToastLength.Short).Show();
                    BluetoothManager.Connect(deviceAddress.Text);
                    Toast.MakeText(ctx, "Should be connected", ToastLength.Short).Show();
                };

                deviceName.Text = device.Name;
                deviceAddress.Text = device.Address;
            }
            return convertView;
        }

    }

    public class WifiListViewAdapter : BaseAdapter<string>
    {
        private LayoutInflater layinf;
        private List<string> wifiList;
        private int resourceId;
        Context ctx;
        public static event WifiInfo WifiActivityTime;
        public delegate void WifiInfo(string name);

        public WifiListViewAdapter(Context context, int tvResourceId, List<string> wifiList)
        {
            this.wifiList = wifiList;
            layinf = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            resourceId = tvResourceId;
            ctx = context;
        }

        public override int Count
        {
            get { return wifiList.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override string this[int position]
        {
            get { return wifiList[position]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            convertView = layinf.Inflate(resourceId, null);
            string wifiNetwork = wifiList[position];

            if (wifiNetwork != null)
            {
                TextView wifiName = convertView.FindViewById<TextView>(Resource.Id.wifiName);
                Button wifiButton = convertView.FindViewById<Button>(Resource.Id.wifiSelectButton);
                TextInputLayout uLayout = convertView.FindViewById<TextInputLayout>(Resource.Id.usernameLayout);
                //TextView username = convertView.FindViewById<TextView>(Resource.Id.usernameInput);
                TextInputLayout pLayout = convertView.FindViewById<TextInputLayout>(Resource.Id.passwordLayout);
                //TextView password = convertView.FindViewById<TextView>(Resource.Id.passwordInput);
                //Button connectButton = convertView.FindViewById<Button>(Resource.Id.connectButton);

                //uLayout.Visibility = ViewStates.Gone;
                //pLayout.Visibility = ViewStates.Gone;
                //connectButton.Visibility = ViewStates.Gone;

                wifiButton.Click += (o, e) =>
                {
                    WifiActivityTime.Invoke(wifiNetwork);
                    Toast.MakeText(ctx, "Test Notification " + position.ToString(), ToastLength.Short).Show();
                };

                /*connectButton.Click += (o, e) =>
                {
                    BluetoothManager.WifiStruct wifiRequest = new BluetoothManager.WifiStruct();
                    wifiRequest.SSID = wifiNetwork;
                    //If there's a username, this will inclue that.
                    wifiRequest.key = password.Text;
                    string request = Newtonsoft.Json.JsonConvert.SerializeObject(wifiRequest);
                    BluetoothManager.SendData(request);
                };*/

                wifiName.Text = wifiNetwork;
            }
            return convertView;
        }
    }
}