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
    /// <summary>
    /// Class handles all Bluetooth Functions
    /// </summary>
    public static class BluetoothManager
    {
        public delegate void WifiHandler(List<string> list);
        public static event WifiHandler SwitchToWifi;
        public static event EventHandler ResetBluetooth;
        public static BluetoothAdapter adapter;
        public static BluetoothDevice connectedDevice = null;
        static string uniqueIdentifier;
        public static BluetoothSocket socket;
        public static List<BluetoothDevice> devices = new List<BluetoothDevice>();
        public static MyBTReceiver receiver;

        /// <summary>
        /// Connects phone to Athena device
        /// </summary>
        /// <param name="deviceAddress">Athena's (hopefully) MAC address</param>
        public static void Connect(string deviceAddress)
        {
            //Checks if Athena has been paired previously and pairs based on that
            //connectedDevice = adapter.BondedDevices.FirstOrDefault(d => d.Address.Equals(deviceAddress));
            //If above line fails, it creates a bond to pair and then connect
            if(connectedDevice == null)
            {
                foreach (BluetoothDevice device in devices)
                {
                    if(device.Address == deviceAddress)
                    {
                        connectedDevice = device;
                        //connectedDevice.CreateBond();
                        break;
                    }
                }
            }

            StartCommunication();
        }

        /// <summary>
        /// Disconnects the device
        /// </summary>
        public static void Disconnect()
        {
            connectedDevice = null;
        }

        /// <summary>
        /// Creates socket and attempts connection to Athena device
        /// </summary>
        public static async void StartCommunication()
        {
            //UUID for connecting with the service
            uniqueIdentifier = "a0f9aa1c-465a-45be-8fab-e2c9670ee7c9";
            //Creates Socket with the UUID
            socket = connectedDevice.CreateRfcommSocketToServiceRecord(Java.Util.UUID.FromString(uniqueIdentifier));
            //Make something say "Connecting..."
            bool worked = false;
            int connectionRetries = 0;
            //Attempts to connect 6 times before giving up, if it works, it breaks out of loop
            while (!worked)
            {
                try
                {
                    socket.Connect();
                    worked = true;
                } catch {
                    connectionRetries++;
                    if (connectionRetries == 5)
                        return;
                }
                
            }
            //Make it say "Paired" or connected

            //Creates new credentials object to pass to the Athena device as JSON
            UserData.DataStruct creds = new UserData.DataStruct();
            creds.username = UserData.username;
            creds.password = UserData.password;
            string credString = JsonConvert.SerializeObject(creds);

            //Sends the JSON credentials to the device.
            SendData(credString);
            
            //Waits until it receives a list of Wifi Networks from Athena device
            string wifiRaw = await ReceiveData();
            //Converts wifi string into a readable List of wifi networks
            List<string> networks = WifiConvert(wifiRaw);
            //Invoke Wifi-switching event so Activity can switch to wifi-credential input
            SwitchToWifi.Invoke(networks);
        }

        public static void InvokeBluetoothReset()
        {
            ResetBluetooth.Invoke(1, EventArgs.Empty);
        }

        /// <summary>
        /// Converts Received Data from Bluetooth bytes into a readable string format
        /// </summary>
        /// <returns>
        /// string = data that was received as a string
        /// </returns>
        public static async Task<string> ReceiveData()
        {
            byte[] data = new byte[5120];
            //Read from Pi with:
            await socket.InputStream.ReadAsync(data, 0, data.Length);
            return Encoding.ASCII.GetString(data).Replace("\0","");

        }

        /// <summary>
        /// Converts string passed in to bytes and sends it over Bluetooth
        /// </summary>
        /// <param name="data"></param>
        public static async void SendData(string data)
        {
            byte[] dataHolder = new byte[1024];
            dataHolder = Encoding.ASCII.GetBytes(data);
            //Write to Pi with:
            await socket.OutputStream.WriteAsync(dataHolder, 0, dataHolder.Length);
        }

        /// <summary>
        /// Converts raw string of networks into List of strings
        /// </summary>
        /// <param name="raw">Raw Wifi string</param>
        /// <returns>
        /// string list = more organized list of strings
        /// </returns>
        private static List<string> WifiConvert(string raw)
        {
            List<string> wifiList = JsonConvert.DeserializeObject<List<string>>(raw);
            /*List<string> wifiList = new List<string>();
            //Until the raw variable is empty, it will add more networks to the list
            while(raw != "")
            {
                raw = raw.Trim();
                string item;
                //Gets the name of the Wifi network (if it's not the last)
                if (raw.Contains("\n"))
                    item = raw.Substring(0, raw.IndexOf("\n") + 2);
                //All that should be left is the last Wifi network detected.
                else
                    item = raw;
                //Removes item from raw variable
                raw = raw.Replace(item, "");
                //Prunes anything not necessary from the item's name
                item = item.Replace("ESSID:\"", "");
                item = item.Replace("\"\n","");
                item = item.Replace("\\x00", "");
                item = item.Replace("\"","");
                if(item.TrimEnd() !="" && !wifiList.Contains(item))
                    wifiList.Add(item);
            }*/
            return wifiList;
        }

        /// <summary>
        /// Struct used to pass WiFi credentials to the device.
        /// </summary>
        public struct WifiStruct
        {
            public string SSID { get; set; }
            public string identity { get; set; }
            public string key { get; set; }
        }

        /// <summary>
        /// Updates list of bluetooth devices that are able to be connected to when discovering devices
        /// </summary>
        [BroadcastReceiver]
        public class MyBTReceiver : BroadcastReceiver
        {
            public List<BluetoothDevice> devices = new List<BluetoothDevice>();
            public event EventHandler<List<BluetoothDevice>> OnDiscoveryEnd;
            /// <summary>
            /// Adds device to Bluetooth device list if it doesn't already exist
            /// </summary>
            /// <param name="context">The activity's context</param>
            /// <param name="intent">The activity's intent</param>
            public override void OnReceive(Context context, Intent intent)
            {
                string action = intent.Action;
                if (action == BluetoothDevice.ActionFound)
                {
                    BluetoothDevice device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                    //If the device is original and has a name, it adds it to the list
                    if (device.Name != null && OriginalDevice(devices, device))
                    {
                        devices.Add(device);
                        //Updates the devices list
                        BluetoothManager.devices = devices;
                        //Updates the bluetooth device list in the activity
                        OnboardingActivity.btUIAdapter = new DeviceListViewAdapter(context, Resource.Layout.BTDeviceListLayout, devices);
                        OnboardingActivity.viewableDevices.Adapter = OnboardingActivity.btUIAdapter;
                    }
                }else if (action == BluetoothAdapter.ActionDiscoveryFinished)
                {
                    OnDiscoveryEnd?.Invoke(this, devices);
                }
            }

            /// <summary>
            /// Checks to see if the bluetooth device is not already in the list
            /// </summary>
            /// <param name="btl">Bluetooth device list</param>
            /// <param name="btd">Bluetooth device being analyzed</param>
            /// <returns>
            /// true = original device
            /// false = duplicated device
            /// </returns>
            private bool OriginalDevice(List<BluetoothDevice> btl, BluetoothDevice btd)
            {
                foreach (BluetoothDevice dev in btl)
                {
                    if(btd.Address == dev.Address)
                    {
                        //Duplicated device (is in list)
                        return false;
                    }
                }
                //Original device (not in list)
                return true;
            }
        }
        /*public class StreamData
        {
            public Stream stream;
            public byte[] data;

            public StreamData(Stream stream)
            {
                this.stream = stream;
                this.data = new byte[1024];
            }
        }*/
    }
}