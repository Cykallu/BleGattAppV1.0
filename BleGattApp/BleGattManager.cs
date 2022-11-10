using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
public enum BleState
{
    ENone,
    EDiscovery,
    EConnecting,
    EConnected
}

namespace BleGattApp
{
    public class BleGattManager
    {
        /// <summary>
        /// deviceWatcher
        /// </summary>
        private DeviceWatcher deviceWatcher;

        /// <summary>
        /// bleDevice
        /// </summary>
        private BluetoothLEDevice bleDevice;

        /// <summary>
        /// Gattcharacteristic
        /// </summary>
        private GattCharacteristic selectedCharasteristic;

        /// <summary>
        /// tcpGameClient
        /// </summary>
        private TcpGameClient tcpGameClient;

        /// <summary>
        /// BleState
        /// </summary>
        private BleState bleState = BleState.ENone;


        /// <summary>
        /// BleGattManager
        /// </summary>
        public BleGattManager(TcpGameClient tcpGameClient)
        {
            this.tcpGameClient = tcpGameClient;
        }

        /// <summary>
        /// StartBLEScanner
        /// </summary>
        public void StartBLEScanner()
        {
            Debug.WriteLine("BleGattManager - StartBLEScanner()");

            bleState = BleState.EDiscovery;
            // TODO : Use Paired or unpaired CreateWatcher
            //deviceWatcher = DeviceInformation.CreateWatcher(BluetoothLEDevice.GetDeviceSelectorFromPairingState(false));
            deviceWatcher = DeviceInformation.CreateWatcher(BluetoothLEDevice.GetDeviceSelectorFromPairingState(true));

            // TODO : Register Handlers, and Start watcher Check Documention!!!!
            // Register event handlers before starting the watcher.
            // Added, Updated and Removed are required to get all nearby devices
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;

            // EnumerationCompleted and Stopped are optional to implement.
            deviceWatcher.Stopped += DeviceWatcher_Stopped;
            // Start the watcher.
            deviceWatcher.Start();

            Debug.WriteLine("BleGattManager - StartBLEScanner() -> Done");
        }

        /// <summary>
        /// DeviceWatcher_Stopped
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void DeviceWatcher_Stopped(DeviceWatcher a, object b)
        {
            if (bleState == BleState.EDiscovery || bleState == BleState.EConnecting)
            {
                if (bleDevice != null)
                {
                    bleDevice.ConnectionStatusChanged -= ConnectionStatusChangedHandler;
                    bleDevice.Dispose();
                }
                if (selectedCharasteristic != null)
                {
                    selectedCharasteristic.ValueChanged -= Characteristic_ValueChanged;
                }

                bleState = BleState.EDiscovery;
                deviceWatcher.Start();
            }
        }

        public void ConnectionStatusChangedHandler(BluetoothLEDevice bluetoothLEDevice, object o)
        {
            if (bluetoothLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                bleState = BleState.EConnected;
            }
            else if (bluetoothLEDevice.ConnectionStatus == BluetoothConnectionStatus.Disconnected && deviceWatcher.Status == DeviceWatcherStatus.Stopped)
            {
                if (bluetoothLEDevice != null)
                {
                    bluetoothLEDevice.ConnectionStatusChanged -= ConnectionStatusChangedHandler;
                    bluetoothLEDevice.Dispose();
                }
                if (selectedCharasteristic != null)
                {
                    selectedCharasteristic.ValueChanged -= Characteristic_ValueChanged;
                }
                bleState = BleState.EDiscovery;
                deviceWatcher.Start();
            }
        }

        /// <summary>
        /// DeviceWatcher_Removed
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void DeviceWatcher_Removed(DeviceWatcher a, DeviceInformationUpdate b)
        {
        }

        /// <summary>
        /// DeviceWatcher_Updated
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public void DeviceWatcher_Updated(DeviceWatcher a, DeviceInformationUpdate b)
        {
        }

        /// <summary>
        /// DeviceWatcher_AddedAsync
        /// </summary>
        /// <param name="deviceWatcher"></param>
        /// <param name="deviceInformation"></param>
        public void DeviceWatcher_Added(DeviceWatcher deviceWatcher, DeviceInformation deviceInformation)
        {
            Debug.WriteLine("BleGattManager - DeviceWatcher_Added()");

            // TODO : Check that bluetooth name is same what you setup on esp32 side
            if (deviceInformation.Name == "BallRoller2000" && bleState == BleState.EDiscovery)
            {
                Debug.WriteLine("BleGattManager - DeviceWatcher_Added() - > Device Found");
                bleState = BleState.EConnecting;

                deviceWatcher.Stop();



                // HOX!! Use Task.Run for all async methods
                // // Note: BluetoothLEDevice.FromIdAsync must be called from a UI thread because it may prompt for consent.
                bleDevice = Task.Run(async () => await BluetoothLEDevice.FromIdAsync(deviceInformation.Id)).Result;

                // Get Gatt Services
                // Now that you have a BluetoothLEDevice object, the next step is to discover what data the device exposes. The first step to do this is to query for services:
                GattDeviceServicesResult serviceresult = Task.Run(async () => await bleDevice.GetGattServicesAsync()).Result;
                if (serviceresult.Status == GattCommunicationStatus.Success)
                {

                    Debug.WriteLine("BleGattManager - DeviceWatcher_Added() - > GattCommunicationStatus.Success");
                    var services = serviceresult.Services;
                    Debug.WriteLine("BleGattManager - DeviceWatcher_Added() - > Count of Services " + services.Count.ToString());

                    // TODO: Get Chharastics
                    // Once the service of interest has been identified, the next step is to query for characteristics.

                    GattCharacteristicsResult charasteristicresult = Task.Run(async () => await services[2].GetCharacteristicsAsync()).Result;

                    if (charasteristicresult.Status == GattCommunicationStatus.Success)
                    {
                        Debug.WriteLine("BleGattManager - DeviceWatcher_Added() - > GattCommunicationStatus.Success");
                        var characteristics = charasteristicresult.Characteristics;
                        selectedCharasteristic = characteristics[0];
                        GattCharacteristicProperties properties = characteristics[0].CharacteristicProperties;

                        Debug.WriteLine("BleGattManager - DeviceWatcher_Added() - > Count of characteristics " + characteristics.Count.ToString());

                        if (properties.HasFlag(GattCharacteristicProperties.Read))
                        {
                            Debug.WriteLine("BleGattManager - DeviceWatcher_Added() - > Read OK");
                        }
                        else
                        {
                            Debug.WriteLine("BleGattManager - DeviceWatcher_Added() - >  Read FAIL");
                        }
                        if (properties.HasFlag(GattCharacteristicProperties.Write))
                        {
                            Debug.WriteLine("BleGattManager - DeviceWatcher_Added() - >  Write OK");
                        }
                        else
                        {
                            Debug.WriteLine("BleGattManager - DeviceWatcher_Added() - >  Write FAIL");
                        }
                        if (properties.HasFlag(GattCharacteristicProperties.Notify))
                        {
                            Debug.WriteLine("BleGattManager - DeviceWatcher_Added() - > Notify OK");
                        }
                        else
                        {
                            Debug.WriteLine("BleGattManager - DeviceWatcher_Added() - > Notify FAIL");
                        }

                        // TODO : Subscribing for notifications

                        GattCommunicationStatus status = Task.Run(async () => await characteristics[0].WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.Notify)).Result;

                        if (status == GattCommunicationStatus.Success)
                        {
                            Debug.WriteLine("BleGattManager - DeviceWatcher_Added() - > Notify Register OK ");
                            characteristics[0].ValueChanged += Characteristic_ValueChanged;
                        }
                        else
                        {
                            Debug.WriteLine("BleGattManager - DeviceWatcher_Added() - > Notify Register Failt ");
                        }
                        if (bleDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
                        {
                            bleState = BleState.EConnected;
                            Debug.WriteLine("BleGattManager - DeviceWatcher_Added() -> Connected");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("BleGattManager - DeviceWatcher_Added() - > Fail ");
                        // You can add error management here, if fail, propably you need retry, for ex. startScan again
                    }
                }
            }
        }

        /// <summary>
        /// Characteristic_ValueChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Characteristic_ValueChanged(GattCharacteristic sender,
                            GattValueChangedEventArgs args)
        {


            // TODO : Perform Read/Write operations on a characteristic
            // Read Values and call tcmpGameClient.SendMessage and pass readed byte array


            var reader = DataReader.FromBuffer(args.CharacteristicValue);


            byte[] input = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(input);
            string str = Encoding.Default.GetString(input);

            tcpGameClient.SendMessage(str);
            Debug.WriteLine(str);




        }
    }
}
