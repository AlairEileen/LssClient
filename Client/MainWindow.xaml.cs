using Client.ViewModels;
using Common;
using System;
using System.Collections.Generic;
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

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static byte[] result = new byte[1024];
        Socket clientSocket;
        bool clientIsStart = true;

        private MainWindowData mwData = new MainWindowData();
        private MessageManager messageManager = new MessageManager();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = mwData;
            messageManager.SetViewData(mwData);
            messageManager.SetView(this);
        }
        private void SendHeart(object obj)
        {
            var abc = 0;
            while (true && clientIsStart)
            {
                try
                {
                    messageManager.SendMessage(clientSocket, new Msg { Type = MsgType.Heart, ClientID = mwData.ClientID });
                    Thread.Sleep(1000 * 10);
                    abc++;
                    if (abc % 5 == 0)
                    {
                        messageManager. SendMessage(clientSocket, new BugMessage { Type = MsgType.ReportBug, ClientID = mwData.ClientID,BugInfo = new BugModel { Content = $"Bug_{abc}" } });
                    }
                }
                catch (SocketException)
                {

                    DisConnectClient();
                    break;
                }
                catch { }
            }

        }


        public void DisConnectClient()
        {
            if (clientSocket != null)
            {
                mwData.Logs += "连接已断开";
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                }
                catch { }
                finally
                {
                    clientSocket.Close();
                    clientSocket.Dispose();
                    clientIsStart = false;
                }
            }

        }
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            mwData.Logs += $"设备ID：{mwData.ClientID}\n";


            //设定服务器IP地址  
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            //IPAddress ip = IPAddress.Parse("47.94.208.29");
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(new IPEndPoint(ip, 7701)); //配置服务器IP与端口  
                clientSocket.SendTimeout = 10000;
                clientIsStart = true;

                mwData.Logs += "连接服务器成功\n";
            }
            catch(Exception ex)
            {
                
                mwData.Logs += $"连接服务器失败:{ex.Message}\n";
                return;
            }
            //messageManager.SendMessage(clientSocket, new Msg {Type=MsgType.Heart, ClientID = mwData.ClientID });
            Thread receiveThread = new Thread(ReceiveMessage);
            receiveThread.Start();
            Thread heartThread = new Thread(SendHeart);
            heartThread.Start();

        }

        private void ReceiveMessage()
        {

            while (true && clientIsStart)
            {
                try
                {
                    //通过clientSocket接收数据  
                    int receiveLength = clientSocket.Receive(result);
                    messageManager.ReceiveMessage(clientSocket, result, receiveLength);
                }
                catch (SocketException)
                {
                    DisConnectClient();
                    break;
                }
                catch (Exception)
                {

                }

            }
        }


        internal void ToBottom(TextBox textBox)
        {
            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                textBox.ScrollToEnd();

            }));
        }


        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                clientSocket.Shutdown(SocketShutdown.Both);

            }
            finally
            {
                clientSocket.Close();
                clientSocket.Dispose();
                clientIsStart = false;
                mwData.Logs += "已经强制断开！";
            }

        }
    }
}
