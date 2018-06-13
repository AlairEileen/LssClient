using Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
using VirtualWMQ.ViewModels;

namespace VirtualWMQ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {




        private MainWindowData mwData = new MainWindowData();
        private MessageManager messageManager = new MessageManager();

        private static byte[] result = new byte[1024];
        Socket server;
        bool serverIsStart = true;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = mwData;
            messageManager.SetViewData(mwData);
            messageManager.SetView(this);
        }

        private void BtnStartServer_Click(object sender, RoutedEventArgs e)
        {
            StartServer();
        }

        private void StartServer()
        {
            IPAddress iPAddress = IPAddress.Parse("0.0.0.0");
            int port = 7701;
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(iPAddress, port));
            server.SendTimeout = 10000;
            server.Listen(50);
            serverIsStart = true;
            mwData.Logs += $"创建连接成功！IP:{iPAddress.ToString()},Port:{port}\n";
            Thread listenTh = new Thread(ListenClientConnect);
            listenTh.Start();
        }

        private void ListenClientConnect()
        {
            while (true && serverIsStart)
            {
                try
                {
                    Socket client = server.Accept();
                    messageManager.SendMessage(client, new Msg { Type = MsgType.Heart });
                    Thread heartThread = new Thread(SendHeart);
                    heartThread.Start(client);
                    Thread receiveThread = new Thread(ReceiveMessage);
                    receiveThread.Start(client);
                }
                catch { }
                finally { }
            }
        }

        private void SendHeart(object obj)
        {
            Socket client = (Socket)obj;

            while (true && serverIsStart)
            {
                try
                {
                    Thread.Sleep(1000 * 2);

                    if (client != null && client.Connected)
                    {
                        messageManager.SendMessage(client, new Msg { Type = MsgType.Heart });
                    }
                }
                catch (SocketException)
                {

                    DisConnectClient(client);
                    break;
                }
                catch { }
            }

        }

        public void DisConnectClient(Socket client)
        {

            if (client != null)
            {
                lock (client)
                {
                    var device = mwData.Devices.Select(x => x).Where(x => x.Client == client).FirstOrDefault();
                    if (device != null)
                    {
                        device.Status = "离线";
                    }

                    try
                    {
                        client.Shutdown(SocketShutdown.Both);
                    }
                    catch { }
                    finally
                    {
                        client.Close();
                        client.Dispose();
                    }
                }
            }
        }


        private void ReceiveMessage(object clientSocket)
        {
            Socket client = (Socket)clientSocket;
            while (true && serverIsStart)
            {
                try
                {
                    int receiveNumber = client.Receive(result);
                    messageManager.ReceiveMessage(client, result, receiveNumber);

                }
                catch (SocketException)
                {
                    DisConnectClient(client);

                    break;
                }
                catch (Exception)
                {
                    //MessageBox.Show(e.Message);

                }
            }

        }


        private void BtnCloseServer_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                foreach (var item in mwData.Devices)
                {
                    DisConnectClient(item.Client);
                }
                server.Shutdown(SocketShutdown.Both);
            }
            catch { }
            finally
            {
                server.Close();
                server.Dispose();
                serverIsStart = false;
                mwData.Logs += "已经强制关闭服务!\n";
            }
        }
        private void BtnOutB_Click(object sender, RoutedEventArgs e)
        {

            var device = GetSelectedDevice();
            if (device != null)
            {
                messageManager.SendMessage(device.Client, new OutCoinMsg { Type = MsgType.OutCoin, CoinCount = 3 });
            }
        }

        private ClientInfo GetSelectedDevice()
        {
            try
            {
                var select = LbDevices.SelectedItem.ToString();
                var id = select.Substring(0, select.IndexOf("-"));
                var device = mwData.Devices.Select(x => x).Where(x => x.ClientID.Equals(id)).FirstOrDefault();
                return device;
            }
            catch (Exception)
            {
                MessageBox.Show("请选择设备");
                return null;
            }
        }

        private void BtnOutline_Click(object sender, RoutedEventArgs e)
        {

            var device = GetSelectedDevice();
            if (device != null)
            {
                DisConnectClient(device.Client);
            }
        }
    }
}
