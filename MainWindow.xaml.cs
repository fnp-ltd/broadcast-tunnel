using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace BroadcastTunnel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int REMOTE_CLIENT_PORT = 46353;
        private Task relayTask;
        private string remoteAddressText;
        private string portText;
        private IPAddress remoteAddress;
        private int localPort;
        private int packetsSent;
        private int packetsReceived;
        private IPEndPoint remoteEndpoint;
        private UdpClient remoteClient;
        private IPEndPoint broadcastEndpoint;
        private UdpClient broadcastClient;

        public MainWindow()
        {
            InitializeComponent();
            Log("Ready");
        }

        private void Log(String text)
        {
            logTextBlock.Dispatcher.Invoke(() => { 
                logTextBlock.Inlines.Add(text + "\n");
                logScrollViewer.ScrollToBottom();
            });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            button.IsEnabled = false;
            remoteAddressText = addressTextBox.Text.Trim();
            portText = portTextBox.Text.Trim();
            relayTask = new Task(TaskRunner);
            relayTask.GetAwaiter().OnCompleted(() => { button.IsEnabled = true;  });
            relayTask.Start();
            packetsSent = 0;
            sentTextBox.Text = "0";
            packetsReceived = 0;
            rcvdTextBox.Text = "0";
        }

        private void packetSentCountInc()
        {
            packetsSent++;
            sentTextBox.Dispatcher.Invoke(() => { sentTextBox.Text = packetsSent.ToString(); });
        }

        private void packetRcvdCountInc()
        {
            packetsReceived++;
            rcvdTextBox.Dispatcher.Invoke(() => { rcvdTextBox.Text = packetsReceived.ToString(); });
        }

        private void TaskRunner()
        {
            try
            {
                Log("Starting...");
                remoteAddress = null;
                Regex ipPattern = new Regex(@"^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$");
                Match ipMatch = ipPattern.Match(remoteAddressText);
                if (ipMatch.Success)
                {
                    Log("IP Address found");
                    byte[] addressBytes = new byte[4];
                    for (int i = 0; i < 4; i++) addressBytes[i] = byte.Parse(ipMatch.Groups[i+1].Value);
                    remoteAddress = new IPAddress(addressBytes);
                }
                else
                {
                    Log($"Resolving {remoteAddressText}");
                    IPAddress[] addresses = Dns.GetHostAddresses(remoteAddressText);
                    if (addresses.Length > 1) Log($"Multiple addresses returned for {remoteAddressText}");
                    remoteAddress = addresses[0];
                }
                Log($"Using remote address {remoteAddress}");
                localPort = int.Parse(portText);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                return;
            }

            remoteEndpoint = new IPEndPoint(IPAddress.Any, REMOTE_CLIENT_PORT);
            remoteClient = new UdpClient(remoteEndpoint);
            remoteClient.BeginReceive(RemoteReceived, null);

            broadcastEndpoint = new IPEndPoint(IPAddress.Any, localPort);
            broadcastClient = new UdpClient();
            broadcastClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            broadcastClient.ExclusiveAddressUse = false; // only if you want to send/receive on same machine.
            broadcastClient.Client.Bind(broadcastEndpoint);
            broadcastClient.BeginReceive(new AsyncCallback(this.BroadcastReceived), null);
        }

        private void RemoteReceived(IAsyncResult ar)
        {
            byte[] receiveBytes = remoteClient.EndReceive(ar, ref remoteEndpoint);
            broadcastClient.Send(receiveBytes, receiveBytes.Length, broadcastEndpoint);
            packetRcvdCountInc();
        }

        private void BroadcastReceived(IAsyncResult ar)
        {
            byte[] receiveBytes = broadcastClient.EndReceive(ar, ref broadcastEndpoint);
            remoteClient.Send(receiveBytes, receiveBytes.Length, remoteEndpoint);
            packetSentCountInc();
        }
    }
}
