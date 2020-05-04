using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private Task relayTask;
        private string remoteAddressText;
        private string portText;
        private IPAddress remoteAddress;
        private int localPort;
        private int packetsSent;
        private int packetsReceived;

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

        private void packetSentCount()
        {
            packetsSent++;
            sentTextBox.Dispatcher.Invoke(() => { sentTextBox.Text = packetsSent.ToString(); });
        }

        private void packetRcvdCount()
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
            }
            Thread.Sleep(1000);
        }
    }
}
