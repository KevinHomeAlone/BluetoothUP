using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.ComponentModel;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;
using System.IO;
using Microsoft.Win32;
using InTheHand.Net;
//using Windows.Devices.Bluetooth;

namespace BluetoothUP
{
    public partial class MainWindow : Window
    {
        BackgroundWorker backgroundWorker;
        List<BluetoothDeviceInfo> devices;

        public MainWindow()
        {
            InitializeComponent();
            devices = new List<BluetoothDeviceInfo>();
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += new DoWorkEventHandler(backgroundWorker_DoWork);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker_RunWorkerCompleted);
            
        }

        void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            listBoxBTDevices.ItemsSource = (List<String>)e.Result;
            Cursor = Cursors.Arrow;
        }

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            
            List<String> devicesNames = new List<String>();
            BluetoothClient bc = new InTheHand.Net.Sockets.BluetoothClient();
            BluetoothDeviceInfo[] array = bc.DiscoverDevices(10);
            int count = array.Length;
            for (int i = 0; i < count; i++)
            {
                //Device device = new Device(array[i]);
                devices.Add(array[i]);
                devicesNames.Add(array[i].DeviceName);
                
            }
            e.Result = devicesNames;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StringBuilder deviceInfo = new StringBuilder();
            deviceInfo.Append(devices[listBoxBTDevices.SelectedIndex].DeviceName);
            deviceInfo.Append("\n");
            deviceInfo.Append("Class Of Device: ");
            deviceInfo.Append(devices[listBoxBTDevices.SelectedIndex].ClassOfDevice);
            deviceInfo.Append("\n");
            deviceInfo.Append("Device Address: ");
            deviceInfo.Append(devices[listBoxBTDevices.SelectedIndex].DeviceAddress);
            deviceInfo.Append("\n");
            deviceInfo.Append("Last seen: ");
            deviceInfo.Append(devices[listBoxBTDevices.SelectedIndex].LastSeen);
            deviceInfo.Append("\n");
            deviceInfo.Append("Authenticated: ");
            deviceInfo.Append(devices[listBoxBTDevices.SelectedIndex].Authenticated.ToString());
            deviceInfo.Append("\n");
            deviceInfo.Append("Connected: ");
            deviceInfo.Append(devices[listBoxBTDevices.SelectedIndex].Connected.ToString());
            textBlockDeviceInfo.Text = deviceInfo.ToString();
            if (devices[listBoxBTDevices.SelectedIndex].Authenticated)
                buttonPairDevice.IsEnabled = false;
            else
                buttonPairDevice.IsEnabled = true;
        }

        private void buttonScan_Click(object sender, RoutedEventArgs e)
        {
            if (!backgroundWorker.IsBusy)
            {
                Cursor = Cursors.Wait;
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void buttonPairDevice_Click(object sender, RoutedEventArgs e){
            BluetoothSecurity.PairRequest(devices[listBoxBTDevices.SelectedIndex].DeviceAddress, null);
        }

        private void buttonChooseFile_Click(object sender, RoutedEventArgs e){
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                textBoxFilePath.Text = openFileDialog.FileName.ToString();
        }

        private void buttonSend_Click(object sender, RoutedEventArgs e){
            if (textBoxFilePath.Text == "")
                MessageBox.Show("No path choosen");
            else if(listBoxBTDevices.SelectedIndex == -1)
                MessageBox.Show("No device choosen");
            else
            {
                Cursor = Cursors.Wait;
                MessageBox.Show(sendFile(devices[listBoxBTDevices.SelectedIndex], textBoxFilePath.Text).ToString());
                Cursor = Cursors.Arrow;
            }
            
        }

        public static ObexStatusCode sendFile(BluetoothDeviceInfo device, string fileToSend){
            String fileName = fileToSend.Substring(fileToSend.LastIndexOf("\\"));
            System.Console.WriteLine($"File name: {fileName}");
            // Build te OBEX uri and create an OBEX web request
            var obexUri = new Uri("obex://" + device.DeviceAddress + "/" + fileToSend);
            var request = new ObexWebRequest(obexUri);

            // Fill the request with the file data
            request.ReadFile(fileToSend);

            // Send the request to the targeted bluetooth device
            ObexWebResponse response = request.GetResponse() as ObexWebResponse;
            response.Close();

            return response.StatusCode;
        }

        /*public void DownloadFile(BluetoothDeviceInfo device, string remoteFilePath, string remoteFileName, string localFileName)
        {
            // Create client object
            BluetoothClient cli = new BluetoothClient();

            // Connect to the given device in FileTranfer mode
            cli.Connect(new BluetoothEndPoint(device.DeviceAddress, InTheHand.Net.Bluetooth.BluetoothService.ObexFileTransfer));

            // Create a remote session and navigate to the bluetooth directory
            ObexClientSession sess = new ObexClientSession(cli.GetStream(), UInt16.MaxValue);
            sess.Connect(ObexConstant.Target.FolderBrowsing);
            // Navigate to the file location on the remote device
            sess.SetPath(remoteFilePath);

            // Create a filestream and write the content of the file into it, then filename is returned
            FileStream fs = new FileStream(localFileName, FileMode.OpenOrCreate, FileAccess.Write);
            sess.GetTo(fs, remoteFileName, null);
        }*/
    }
}
