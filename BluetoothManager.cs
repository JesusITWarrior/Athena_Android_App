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
using Newtonsoft.Json;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    public static class BluetoothManager
    {
        public delegate void WifiHandler(List<string> list);
        public static event WifiHandler SwitchToWifi;
        public static BluetoothAdapter adapter;
        public static BluetoothDevice connectedDevice = null;
        static string uniqueIdentifier;
        public static BluetoothSocket socket;
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
            //Make it say "Paired" or connected

            UserData.DataStruct creds = new UserData.DataStruct();
            creds.username = UserData.username;
            creds.password = UserData.password;
            //creds.username = "My";
            //creds.password = "A";
            string credString = JsonConvert.SerializeObject(creds);
            SendData(credString);
            
            string wifiRaw = await ReceiveData();
            List<string> networks = WifiConvert(wifiRaw);
            SwitchToWifi.Invoke(networks);
        }

        public static async Task<string> ReceiveData()
        {
            byte[] data = new byte[2048];
            //Read from Pi with:
            await socket.InputStream.ReadAsync(data, 0, data.Length);
            return Encoding.ASCII.GetString(data).Replace("\0","");

        }

        public static async void SendData(string data)
        {
            byte[] dataHolder = new byte[1024];
            dataHolder = Encoding.ASCII.GetBytes(data);
            //Write to Pi with:
            await socket.OutputStream.WriteAsync(dataHolder, 0, dataHolder.Length);
        }

        private static List<string> WifiConvert(string raw)
        {
            List<string> wifiList = new List<string>();
            while(raw != "")
            {
                raw = raw.Trim();
                string item;
                if (raw.Contains("\n"))
                    item = raw.Substring(0, raw.IndexOf("\n") + 2);
                else
                    item = raw;
                raw = raw.Replace(item, "");
                item = item.Replace("ESSID:\"", "");
                item = item.Replace("\"\n","");
                item = item.Replace("\\x00", "");
                if(item.TrimEnd() !="")
                    wifiList.Add(item);
            }
            return wifiList;
        }

        public struct WifiStruct
        {
            public string SSID { get; set; }
            public string identity { get; set; }
            public string key { get; set; }
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
                    if (device.Name != null && OriginalDevice(devices, device))
                    {
                        devices.Add(device);
                        BluetoothManager.devices = devices;
                        OnboardingActivity.btUIAdapter = new DeviceListViewAdapter(context, Resource.Layout.BTDeviceListLayout, devices);
                        OnboardingActivity.viewableDevices.Adapter = OnboardingActivity.btUIAdapter;
                    }
                }else if (action == BluetoothAdapter.ActionDiscoveryFinished)
                {
                    OnDiscoveryEnd?.Invoke(this, devices);
                }
            }
            private bool OriginalDevice(List<BluetoothDevice> btl, BluetoothDevice btd)
            {
                foreach (BluetoothDevice dev in btl)
                {
                    if(btd.Address == dev.Address)
                    {
                        return false;
                    }
                }
                return true;
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