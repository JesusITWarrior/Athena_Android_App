using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using AndroidX.AppCompat.App;
using System.Linq;
using System.Text;
using System.IO;
using Android.Bluetooth;
using Android.Content.PM;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    [Activity(Label = "OnboardingActivity")]
    public class OnboardingActivity : AppCompatActivity
    {
        //Do something with this

        public static DeviceListViewAdapter btUIAdapter;
        public static ListView viewableDevices;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.onboardingprocess);
            viewableDevices = FindViewById<ListView>(Resource.Id.devices);
            // Create your application here
            BluetoothManager.adapter = BluetoothAdapter.DefaultAdapter;
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
            BluetoothManager.receiver.OnDiscoveryEnd += DiscoveryFinished;
            RegisterReceiver(BluetoothManager.receiver, filter);
            BluetoothManager.adapter.StartDiscovery();
            Toast.MakeText(this, "Bluetooth started", ToastLength.Short).Show();
        }

        private void DiscoveryFinished(object sender, List<BluetoothDevice> e)
        {
            UnregisterReceiver(BluetoothManager.receiver);
            BluetoothManager.receiver = null;

            BluetoothManager.devices = e;
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
                    BluetoothManager.Connect(deviceAddress.Text);
                    Toast.MakeText(ctx, "Test Notification " + position.ToString(), ToastLength.Short).Show();
                };

                deviceName.Text = device.Name;
                deviceAddress.Text = device.Address;
            }
            return convertView;
        }

    }
}