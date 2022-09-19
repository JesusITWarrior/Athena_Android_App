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
        static BluetoothAdapter adapter;
        static BluetoothDevice connectedDevice;
        Java.Util.UUID uniqueIdentifier;
        BluetoothSocket socket;
        List<BluetoothDevice> devices = new List<BluetoothDevice>();
        MyBTReceiver receiver;
        //Do something with this

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
                CheckPerms();
                DiscoverBluetooth();
            }
        }

        private void DiscoverBluetooth()
        {
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

        private void CheckPerms()
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
                }
            }
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
                    if(device.Name != null)
                        devices.Add(device);
                    btUIAdapter = new DeviceListViewAdapter(context, Resource.Layout.BTDeviceListLayout, devices);
                    viewableDevices.Adapter = btUIAdapter;
                }else if (action == BluetoothAdapter.ActionDiscoveryFinished)
                {
                    OnDiscoveryEnd?.Invoke(this, devices);
                }
            }
        }
        public async void StartCommunication(object sender, BluetoothDevice device)
        {
            connectedDevice = device;
            uniqueIdentifier = Java.Util.UUID.RandomUUID();
            socket = device.CreateRfcommSocketToServiceRecord(uniqueIdentifier);
            //Make something say "Connecting..."
            await socket.ConnectAsync();
            //Make it say "Paired"
            int number = await ReceiveData();
        }

        private async Task<int> ReceiveData()
        {
            byte[] buffer = new byte[1024];
            //Read from Pi with:
            return await socket.InputStream.ReadAsync(buffer, 0, buffer.Length);
        }

        private async void SendData(string data)
        {
            byte[] buffer = new byte[1024];
            buffer = Convert.FromBase64String(data);
            //Write to Pi with:
            await socket.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
    class DeviceListViewAdapter : BaseAdapter<BluetoothDevice>
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
                    //devices[position].
                    Toast.MakeText(ctx, "Test Notification " + position.ToString(), ToastLength.Short).Show();
                };

                deviceName.Text = device.Name;
                deviceAddress.Text = device.Address;
            }
            return convertView;
        }

    }
}