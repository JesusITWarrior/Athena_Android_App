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
    /// <summary>
    /// Handles Onboarding Workflow
    /// </summary>
    [Activity(Label = "OnboardingActivity", ScreenOrientation = ScreenOrientation.Portrait)]
    public class OnboardingActivity : AppCompatActivity
    {
        //Bluetooth Device List UI Adapter     
        public static DeviceListViewAdapter btUIAdapter;
        //Wifi Network List UI Adapter
        public static WifiListViewAdapter wifiListViewAdapter;
        //Bluetooth Device ListView
        public static ListView viewableDevices;
        //Wifi Network ListView
        public static ListView viewableNetworks;
        //Refresh Bluetooth/Wifi button (depending on what is shown)
        Button refreshButton;
        Dialog loginDialog;
        public static Dialog loading;

        /// <summary>
        /// Run as "Main" function. Ran when activity is started.
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetUpBluetoothView(this, EventArgs.Empty);
        }

        private void SetUpBluetoothView(object o, EventArgs e)
        {
            SetContentView(Resource.Layout.bluetoothsearch);

            loading = new Dialog(this);
            //Sets Views and components to global variables for class use
            viewableDevices = FindViewById<ListView>(Resource.Id.devices);
            refreshButton = FindViewById<Button>(Resource.Id.RefreshBluetooth);

            //Subscribes refreshButton's click event to RestartDiscovery function
            refreshButton.Click += RestartDiscovery;

            //Sets the bluetooth adapter to the default Android bluetooth adapter
            BluetoothManager.adapter = BluetoothAdapter.DefaultAdapter;

            //Subscribes SwitchToWifi event to DiscoveryFinished Function because when devices is connected, phone should stop looking for other devices.
            BluetoothManager.SwitchToWifi += DiscoveryFinished;
            BluetoothManager.ResetBluetooth += SetUpBluetoothView;

            //Checks if the bluetooth adapater is on or off
            if (!BluetoothManager.adapter.IsEnabled)
                //Bluetooth is off, should ask user to turn it on
                //TODO: work on this a bit
                TurnOnYourBluetooth();
            else
            {
                //Bluetooth is on, should begin discovering bluetooth devices
                StartDiscovery();
            }
        }

        /// <summary>
        /// Inform user that they need to turn on their Bluetooth.
        /// </summary>
        private async void TurnOnYourBluetooth()
        {
            //Popup for bluetooth
            Dialog bluetoothWarning = new Dialog(this);
            bluetoothWarning.SetContentView(Resource.Layout.TurnOnBluetoothPopup);
            bluetoothWarning.Show();
            Button cancel = bluetoothWarning.FindViewById<Button>(Resource.Id.cancelButton);
            Button turnOn = bluetoothWarning.FindViewById<Button>(Resource.Id.connectButton);
            bool updated = false;
            cancel.Click += (o,e) =>
            {
                bluetoothWarning.Dismiss();
                bluetoothWarning.Hide();
                updated = true;
            };
            turnOn.Click += (o, e) =>
            {
                BluetoothManager.adapter.Enable();
                bluetoothWarning.Dismiss();
                bluetoothWarning.Hide();
                Toast.MakeText(this, "Bluetooth Should Be On Now", ToastLength.Short).Show();
                updated = true;
                //StartDiscovery();
            };
            while (!updated)
            {
                if (BluetoothManager.adapter.IsEnabled)
                {
                    updated = true;
                }
                else
                {
                    await Task.Delay(1500);
                }
            }
        }

        /// <summary>
        /// Checks all permissions before starting bluetooth discovery
        /// </summary>
        private async void StartDiscovery()
        {
            if (!BluetoothManager.adapter.IsDiscovering)
            {
                //Checks for bluetooth managing permissions:
                Intent intent = new Intent(BluetoothAdapter.ActionRequestEnable);
                bool hasPerms = false;
                StartActivityForResult(intent, 1);
                //Checks for user permissions and keeps trying to get perms if it doesn't have them.
                //TODO: Check if this has a forever loop bug that will not keep prompting user
                while (!hasPerms)
                {
                    hasPerms = await CheckPerms();
                }
                
                DiscoverBluetooth();
            }
        }

        /// <summary>
        /// Makes phone discover nearby bluetooth devices to pair to
        /// </summary>
        private void DiscoverBluetooth()
        {
            //Sets up Bluetooth receiver for reading any nearby pairable devices
            IntentFilter filter = new IntentFilter();
            filter.AddAction(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            BluetoothManager.receiver = new BluetoothManager.MyBTReceiver();
            RegisterReceiver(BluetoothManager.receiver, filter);
            BluetoothManager.devices.Clear();

            //Begins the discovery with the Registered receiver
            BluetoothManager.adapter.StartDiscovery();
            Toast.MakeText(this, "Bluetooth started", ToastLength.Short).Show();
        }

        /// <summary>
        /// Stops discovery, resetting receiver and list and then restarts it
        /// </summary>
        /// <param name="sender">Button that ran this function</param>
        /// <param name="e">Should be empty</param>
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

        /// <summary>
        /// Called when paired and connected to Athena via Bluetooth. Should unregister receiver and begin WiFi searching
        /// </summary>
        /// <param name="networks"></param>
        private void DiscoveryFinished(List<string> networks)
        {
            try
            {
                UnregisterReceiver(BluetoothManager.receiver);
                BluetoothManager.receiver = null;

                //Sets XML to WiFi searching layout
                SetContentView(Resource.Layout.wifisearch);
                loading.Dismiss();
                loading.Hide();
                //Assigns all views and necessary view adapters
                viewableNetworks = FindViewById<ListView>(Resource.Id.wifiNetworks);
                wifiListViewAdapter = new WifiListViewAdapter(this, Resource.Layout.WifiNetworkListLayout, networks);
                viewableNetworks.Adapter = wifiListViewAdapter;

                //Subscribes WifiActivityTime to the Wifi function
                WifiListViewAdapter.WifiActivityTime += Wifi;
            }
            catch
            {
                Toast.MakeText(this, "Bluetooth Issue, please try again...", ToastLength.Short).Show();
                BluetoothManager.InvokeBluetoothReset();
            }
        }

        /// <summary>
        /// Checks if all perms are granted, requests if any are denied. Automatically accepts if Android version is less than Lollipop
        /// </summary>
        /// <returns>
        /// true = all perms granted
        /// false = some/all perms denied
        /// </returns>
        private async Task<bool> CheckPerms()
        {
            if(Build.VERSION.SdkInt > Android.OS.BuildVersionCodes.Lollipop)
            {
                int permissionCheck = 0;
                permissionCheck += (int)CheckSelfPermission("android.permission.ACCESS_FINE_LOCATION");
                if (permissionCheck != 0)
                {
                    RequestPermissions(new string[] { "android.permission.ACCESS_FINE_LOCATION" }, 0);
                    permissionCheck = (int)CheckSelfPermission("android.permission.ACCESS_FINE_LOCATION");
                    if (permissionCheck != 0)
                        return false;
                }

                permissionCheck = 0;
                permissionCheck += (int)CheckSelfPermission("android.permission.ACCESS_COARSE_LOCATION");
                if (permissionCheck != 0)
                {
                    RequestPermissions(new string[] { "android.permission.ACCESS_COARSE_LOCATION" }, 0);
                    permissionCheck = (int)CheckSelfPermission("android.permission.ACCESS_COARSE_LOCATION");
                    if (permissionCheck != 0)
                        return false;
                }

                permissionCheck = 0;
                permissionCheck += (int)CheckSelfPermission("android.permission.BLUETOOTH");
                if (permissionCheck != 0)
                {
                    RequestPermissions(new string[] { "android.permission.BLUETOOTH" }, 0);
                    permissionCheck = (int)CheckSelfPermission("android.permission.BLUETOOTH");
                    if (permissionCheck != 0)
                        return false;
                }

                permissionCheck = 0;
                permissionCheck += (int)CheckSelfPermission("android.permission.BLUETOOTH_ADMIN");
                if (permissionCheck != 0)
                {
                    RequestPermissions(new string[] { "android.permission.BLUETOOTH_ADMIN" }, 0);
                    permissionCheck = (int)CheckSelfPermission("android.permission.BLUETOOTH_ADMIN");
                    if (permissionCheck != 0)
                        return false;
                }
                return true;
            }
            return true;
        }

        /// <summary>
        /// Function is run when permission is either granted or denied. Does nothing at the moment
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="permissions"></param>
        /// <param name="grantResults"></param>
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

        /// <summary>
        /// Run when WiFi Network is selected. Bundles name for the WiFi connection Activity and begins it.
        /// </summary>
        /// <param name="name">Name of the WiFi network</param>
        private void Wifi(string name)
        {
            try
            {
                loginDialog = new Dialog(this);
                loginDialog.SetContentView(Resource.Layout.WifiCredentialPopup);
                TextView wifiName = loginDialog.FindViewById<TextView>(Resource.Id.wifiName);
                TextView user = loginDialog.FindViewById<TextView>(Resource.Id.usernameInput);
                TextView password = loginDialog.FindViewById<TextView>(Resource.Id.passwordInput);
                wifiName.Text = name;
                Button connect = loginDialog.FindViewById<Button>(Resource.Id.connectButton);
                Button cancel = loginDialog.FindViewById<Button>(Resource.Id.cancelButton);

                connect.Click += (o, e) =>
                {
                    loading.SetContentView(Resource.Layout.whole_screen_loading_symbol);
                    loading.Show();
                    BluetoothManager.WifiStruct cred = new BluetoothManager.WifiStruct();
                    cred.SSID = wifiName.Text;
                    //If identity is empty, it passes null to device, otherwise it trims and sends the identity... will possibly remove
                    cred.identity = (user.Text.Trim() == "") ? null : user.Text.Trim();
                    cred.key = password.Text.Trim();
                    //JSON payload created from WifiStruct "cred" object
                    string auth = Newtonsoft.Json.JsonConvert.SerializeObject(cred);

                    BluetoothManager.SendData(auth);
                    bool gotGoodResponse = false;
                    //Waits until it receives a confirmation instead of a Raw WifiList from the board
                    while (!gotGoodResponse)
                    {
                        //Attempts to convert response to a boolean
                        try
                        {
                            //Receives data from Athena device
                            string confirmation = System.Threading.Tasks.Task.Run(async () => await BluetoothManager.ReceiveData()).Result;
                            //Attempt to convert data to Boolean
                            bool isWorking = Convert.ToBoolean(confirmation);
                            //If we got here, that means it's a boolean
                            gotGoodResponse = true;
                            loading.Dismiss();
                            loading.Hide();
                            //If we received a successful connection: close up shop
                            if (isWorking)
                            {
                                BluetoothManager.socket.Close();
                                loginDialog.Dismiss();
                                loginDialog.Hide();
                                Toast.MakeText(this, "Board Successfully Connected to Wifi", ToastLength.Short).Show();
                                Finish();
                            }
                            //We got a failure, which means device couldn't connect
                            else
                            {
                                //Throw exception up here
                            }
                        }
                        //Response is not a boolean, so loop needs to be repeated
                        catch
                        {
                            gotGoodResponse = false;
                        }
                    }
                };

                cancel.Click += (o, e) =>
                {
                    loginDialog.Dismiss();
                    loginDialog.Hide();
                };
                loginDialog.Show();
            }
            catch
            {
                Toast.MakeText(this, "Bluetooth Issue, please try again...", ToastLength.Short).Show();
                BluetoothManager.InvokeBluetoothReset();
            }
        }

        /// <summary>
        /// Run when a child activity is finished with a result. In this case, when WifiConnection is finished, it should return credentials
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="resultCode"></param>
        /// <param name="data">Intent with Bundle data to be read</param>
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

    /// <summary>
    /// Bluetooth List View Adapter for updating bluetooth device list
    /// </summary>
    public class DeviceListViewAdapter : BaseAdapter<BluetoothDevice>
    {
        private LayoutInflater layinf;
        private List<BluetoothDevice> devices;
        private static event EventHandler BluetoothReset;
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

        /// <summary>
        /// Populates ListView with the List
        /// </summary>
        /// <param name="position">List index</param>
        /// <param name="convertView">The ListView used</param>
        /// <param name="parent"></param>
        /// <returns>
        /// View = Updated ListView to be updated by Activity
        /// </returns>
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            //Creates a position for the convertView
            convertView = layinf.Inflate(resourceId, null);
            //Gets the device at the devices position
            BluetoothDevice device = devices[position];

            if(device != null)
            {
                //Populates all data for the list
                TextView deviceName = convertView.FindViewById<TextView>(Resource.Id.deviceName);
                TextView deviceAddress = convertView.FindViewById<TextView>(Resource.Id.deviceAddress);
                Button deviceButton = convertView.FindViewById<Button>(Resource.Id.deviceSelectButton);
                deviceButton.Click += (o, e) =>
                {
                    //TODO: Block any other buttons from being clicked
                    OnboardingActivity.loading.SetContentView(Resource.Layout.whole_screen_loading_symbol);
                    OnboardingActivity.loading.Show();
                    Task.Delay(2000);
                    //When clicked, it should attempt a bluetooth socket connection
                    Toast.MakeText(ctx, "Conntecting to "+deviceName.Text, ToastLength.Short).Show();
                    try
                    {
                        BluetoothManager.Connect(deviceAddress.Text);
                        Toast.MakeText(ctx, "Connection successful. Getting WiFi networks.", ToastLength.Short).Show();
                    }
                    catch
                    {
                        Toast.MakeText(ctx, "Bluetooth Issue, please try again...", ToastLength.Short).Show();
                        BluetoothManager.InvokeBluetoothReset();
                    }
                };

                deviceName.Text = device.Name;
                deviceAddress.Text = device.Address;
            }
            //Returns the view for the activity to update
            return convertView;
        }

    }

    /// <summary>
    /// Wifi Network List ListView adapter
    /// </summary>
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

        /// <summary>
        /// Populates ListView with List
        /// </summary>
        /// <param name="position">List index</param>
        /// <param name="convertView">The ListView used</param>
        /// <param name="parent"></param>
        /// <returns>
        /// View = Updated ListView to be updated by Activity
        /// </returns>
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            //Creates a position for the convertView
            convertView = layinf.Inflate(resourceId, null);
            string wifiNetwork = wifiList[position];

            if (wifiNetwork != null)
            {
                //Populates all data into the Views
                TextView wifiName = convertView.FindViewById<TextView>(Resource.Id.wifiName);
                Button wifiButton = convertView.FindViewById<Button>(Resource.Id.wifiSelectButton);
                TextInputLayout uLayout = convertView.FindViewById<TextInputLayout>(Resource.Id.usernameLayout);
                //TextView username = convertView.FindViewById<TextView>(Resource.Id.usernameInput);
                TextInputLayout pLayout = convertView.FindViewById<TextInputLayout>(Resource.Id.passwordLayout);

                wifiButton.Click += (o, e) =>
                {
                    //Invokes the WifiActivityTime event, so Activity can begin WifiConnection Activity
                    WifiActivityTime.Invoke(wifiNetwork);
                    Toast.MakeText(ctx, "Test Notification " + position.ToString(), ToastLength.Short).Show();
                };

                wifiName.Text = wifiNetwork;
            }
            //Returns convertView for Activity to update
            return convertView;
        }
    }
}