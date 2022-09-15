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
using Android.Bluetooth;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    [Activity(Label = "OnboardingActivity")]
    public class OnboardingActivity : AppCompatActivity
    {
        static BluetoothAdapter adapter;
        List<BluetoothDevice> devices = new List<BluetoothDevice>();
        MyBTReceiver receiver;

        static DeviceListViewAdapter btUIAdapter;
        static ListView viewableDevices;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.onboardingprocess);
            viewableDevices = FindViewById<ListView>(Resource.Id.devices);
            // Create your application here
            adapter = BluetoothAdapter.DefaultAdapter;
            if (!adapter.IsEnabled)
                TurnOnYourBluetooth();
            else
            {
                StartDiscovery();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            DiscoverBluetooth();
        }
        private void TurnOnYourBluetooth()
        {
            //Inform user that they need to turn on their Bluetooth.
        }

        private void StartDiscovery()
        {
            if (!adapter.IsDiscovering)
            {
                Intent intent = new Intent(BluetoothAdapter.ActionRequestEnable);
                StartActivityForResult(intent, 1);
                //checkPerms();
            }
        }

        private void DiscoverBluetooth()
        {
            checkPerms();
            IntentFilter filter = new IntentFilter();
            filter.AddAction(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            receiver = new MyBTReceiver();
            receiver.OnDiscoveryEnd += DiscoveryFinished;
            RegisterReceiver(receiver, filter);
            adapter.StartDiscovery();
            Toast.MakeText(this, "Bluetooth started", ToastLength.Short).Show();
        }

        private void DiscoveryFinished(object sender, List<BluetoothDevice> e)
        {
            UnregisterReceiver(receiver);
            receiver = null;

            devices = e;
        }

        private void checkPerms()
        {
            if(Build.VERSION.SdkInt > Android.OS.BuildVersionCodes.Lollipop)
            {
                Android.Content.PM.Permission permissionCheck = CheckSelfPermission("android.permission.ACCESS_FINE_LOCATION");
                /*permissionCheck += (int)CheckSelfPermission("android.permission.ACCESS_COARSE_LOCATION");
                if(permissionCheck != 0)
                {
                    RequestPermissions(new string[] { "Manifest.permission.ACCESS_FINE_LOCATION", "Manifest.permission.ACCESS_COARSE_LOCATION" }, 0);
                }*/
            }
        }
        [BroadcastReceiver]
        class MyBTReceiver : BroadcastReceiver
        {
            public List<BluetoothDevice> devices = new List<BluetoothDevice>();
            public event EventHandler<List<BluetoothDevice>> OnDiscoveryEnd;

            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action;
                if (action == BluetoothDevice.ActionFound)
                {
                    BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                    devices.Add(device);
                    btUIAdapter = new DeviceListViewAdapter(context, Resource.Layout.BTDeviceListLayout, devices);
                    viewableDevices.Adapter = btUIAdapter;
                }else if (action == BluetoothAdapter.ActionDiscoveryFinished)
                {
                    OnDiscoveryEnd?.Invoke(this, devices);
                }
            }
        }
    }
    class DeviceListViewAdapter : BaseAdapter<BluetoothDevice>
    {
        private LayoutInflater layinf;
        private List<BluetoothDevice> devices;
        private int resourceId;

        public DeviceListViewAdapter(Context context, int tvResourceId, List<BluetoothDevice> devices)
        {
            this.devices = devices;
            layinf = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            resourceId = tvResourceId;
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

                deviceName.Text = device.Name;
                deviceAddress.Text = device.Address;
            }
            return convertView;
        }

    }
}