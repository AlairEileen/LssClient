using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using Common;
using VirtualWMQ.ViewModels;

namespace VirtualWMQ
{
    public class MessageManager : MessageManagerAbs
    {
        private MainWindow view;
        private MainWindowData viewData;


        public override void ExecuteOutCoin(OutCoinMsg obj)
        {
            MessageBox.Show("执行出币");
        }

        public override void OnSendMsgException(Socket socket, Msg msg)
        {
            view.DisConnectClient(socket);

        }

        public override void OnSendMsgSocketException(Socket socket, Msg msg)
        {
            view.DisConnectClient(socket);

        }

        internal void SetViewData(MainWindowData viewData)
        {
            this.viewData = viewData;
        }

        internal void SetView(MainWindow mainWindow)
        {
            this.view = mainWindow;
        }

        public override void ExecuteHeart(Msg obj)
        {
            CheckClient(obj);
            var device = viewData.Devices.Select(x => x).Where(x => x.Client == obj.Socket).FirstOrDefault();
            device.Heart = "♥";
            new Thread(() =>
                        {
                            Thread.Sleep(500);
                            device.Heart = "";
                        }).Start();


        }
        public void CheckClient(Msg obj)
        {


            if (viewData.Devices.Count == 0)
            {
                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    viewData.Devices.Add(new ClientInfo { ClientID = obj.ClientID, Client = obj.Socket, IP = obj.IP, Status = "在线" });
                }));
                viewData.Logs += $"客户端{obj.IP}连接成功，ID：{ obj.ClientID}\n";

            }

            else
            {
                //for (int i = 0; i < viewData.Devices.Count; i++)
                //{
                //    var dd = viewData.Devices[i];
                //    if (dd.ClientID.Equals(obj.ClientID))
                //    {
                //        dd.Client = obj.Socket;
                //        dd.IP = obj.IP;
                //        dd.Status = "在线";
                //        viewData.Devices.RemoveAt(i);
                //        viewData.Devices.Insert(i, dd);
                //    }
                //}

                var d = viewData.Devices.Select(x => x).Where(x => x.ClientID == obj.ClientID).FirstOrDefault();

                if (d == null)
                {
                    App.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        viewData.Devices.Add(new ClientInfo { ClientID = obj.ClientID, Client = obj.Socket, IP = obj.IP, Status = "在线" });
                    }));
                    viewData.Logs += $"客户端{obj.IP}连接成功，ID：{ obj.ClientID}\n";

                }
                else if (d.Status.Equals("离线"))
                {
                    d.Status = "在线";
                    d.Client = obj.Socket;
                    d.IP = obj.IP;
                    viewData.Logs += $"客户端{obj.IP}连接成功，ID：{ obj.ClientID}\n";

                }
            }

        }


        public override void ExecuteOutCoinFinish(Msg obj)
        {
            var device = viewData.Devices.Select(x => x).Where(x => x.Client == obj.Socket).FirstOrDefault();
            viewData.Logs += $"设备ID {device.ClientID}:执行出币完成\n";
        }

        public override void ExecuteSetDefaultChance(DefaultChanceSetMsg obj)
        {
            throw new NotImplementedException();
        }
    }
}