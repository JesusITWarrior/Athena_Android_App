using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Threading.Tasks;
using System.IO;
using Android.Bluetooth;
using AndroidX.AppCompat;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    public static class BluetoothManager
    {
        public static BluetoothAdapter adapter;
        public static BluetoothDevice connectedDevice;
        static string uniqueIdentifier;
        static BluetoothSocket socket;
        public static List<BluetoothDevice> devices = new List<BluetoothDevice>();
        public static MyBTReceiver receiver;

        public static void Connect(string deviceAddress)
        {
            connectedDevice = adapter.BondedDevices.FirstOrDefault(d => d.Address.Equals(deviceAddress));
            if(connectedDevice == null)
            {
                foreach (BluetoothDevice device in devices)
                {
                    if(device.Address == deviceAddress)
                    {
                        connectedDevice = device;
                        connectedDevice.CreateBond();
                        break;
                    }
                }
            }

            StartCommunication();
        }

        public static void Disconnect()
        {
            connectedDevice = null;
        }

        public static async void StartCommunication()
        {
            uniqueIdentifier = "a0f9aa1c-465a-45be-8fab-e2c9670ee7c9";
            socket = connectedDevice.CreateRfcommSocketToServiceRecord(Java.Util.UUID.FromString(uniqueIdentifier));
            //Make something say "Connecting..."
            socket.Connect();
            //Make it say "Paired"

            SendData("Hello there!");
            string test = await ReceiveData();
        }

        private static async Task<string> ReceiveData()
        {
            byte[] data = new byte[1024];
            //Read from Pi with:
            await socket.InputStream.ReadAsync(data, 0, data.Length);
            return Encoding.ASCII.GetString(data).Replace("\0","");

        }

        private static async void SendData(string data)
        {
            byte[] dataHolder = new byte[1024];
            dataHolder = Encoding.ASCII.GetBytes(data);
            //Write to Pi with:
            await socket.OutputStream.WriteAsync(dataHolder, 0, dataHolder.Length);
        }

        [BroadcastReceiver]
        public class MyBTReceiver : BroadcastReceiver
        {
            public List<BluetoothDevice> devices = new List<BluetoothDevice>();
            public event EventHandler<List<BluetoothDevice>> OnDiscoveryEnd;

            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action;
                if (action == BluetoothDevice.ActionFound)
                {
                    BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                    if(device.Name != null && !devices.Contains(device))
                        devices.Add(device);
                    BluetoothManager.devices = devices;
                    OnboardingActivity.btUIAdapter = new DeviceListViewAdapter(context, Resource.Layout.BTDeviceListLayout, devices);
                    OnboardingActivity.viewableDevices.Adapter = OnboardingActivity.btUIAdapter;
                }else if (action == BluetoothAdapter.ActionDiscoveryFinished)
                {
                    OnDiscoveryEnd?.Invoke(this, devices);
                }
            }
        }

        public class StreamData
        {
            public Stream stream;
            public byte[] data;

            public StreamData(Stream stream)
            {
                this.stream = stream;
                this.data = new byte[1024];
            }
        }
    }
}