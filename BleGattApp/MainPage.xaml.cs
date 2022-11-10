using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BleGattApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Tcp Client
        /// </summary>
        private TcpGameClient client;

        /// <summary>
        /// Ble Gatt Manager
        /// </summary>
        private BleGattManager bleGattManager;


        /// <summary>
        /// MainPage Constructor
        /// </summary>
        public MainPage()
        {
            Debug.WriteLine("MainPage - MainPage()");

            this.InitializeComponent();

            client = new TcpGameClient();

            // TODO : Call Connect method from created instance
            client.Connect();

            // TODO : Create instance of BleGattManager Class, pass TcpGameClient instance to constructor
            bleGattManager = new BleGattManager(client);



        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Mainpage - Button_Click()");
            // TODO : Call StartBLEScanner method from created instance (BleGattManager)
            bleGattManager.StartBLEScanner();


        }
    }
}
