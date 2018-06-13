using System;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using Client.ViewModels;
using Common;

namespace Client
{
    public class MessageManager : MessageManagerAbs
    {
        private MainWindow view;
        private MainWindowData viewData;

        internal void SetViewData(MainWindowData viewData)
        {
            this.viewData = viewData;
        }

        internal void SetView(MainWindow mainWindow)
        {
            this.view = mainWindow;
        }

        public override void OnSendMsgSocketException(Socket socket, Msg msg)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            viewData.Logs += "本机已断开！\n";
        }

        public override void OnSendMsgException(Socket socket, Msg msg)
        {
            view.DisConnectClient();

        }
        public override void ExecuteOutCoin(OutCoinMsg obj)
        {
           
            viewData.Logs += $"正在出币\n数量：{obj.CoinCount}\n概率：{obj.Chance}\n电压：{obj.Voltage}";
            obj.ClientID = viewData.ClientID;
            new Thread(() =>
            {
                int i = 1;
                while (i <= obj.CoinCount)
                {
                    Thread.Sleep(10);
                    viewData.RunStatus += $"出币{i}/{obj.CoinCount}个";
                    i++;
                }
                viewData.Logs += $"完成：出币{obj.CoinCount}个\n";

                SendMessage(obj.Socket, new OutCoinMsgFinish { Type = MsgType.OutCoinFinish, ClientID = obj.ClientID, MessageID = obj.MessageID });
            }).Start();
        }

        public override void ExecuteHeart(Msg obj)
        {
            viewData.Heart = "·";
            new Thread(() =>
            {
                Thread.Sleep(500);
                viewData.Heart = "";
               
            }).Start();
        }

        public override void ExecuteOutCoinFinish(Msg obj)
        {
            //throw new NotImplementedException();
        }

        public override void ExecuteSetDefaultChance(DefaultChanceSetMsg obj)
        {
            viewData.Logs += "正在设置默认概率\n";
            obj.ClientID = viewData.ClientID;
            new Thread(() =>
            {
                    Thread.Sleep(1000);
                    viewData.RunStatus += $"设置默认概率为：{obj.Chance}\n";
                viewData.Logs += $"设置概率{obj.Chance}成功！\n";

                SendMessage(obj.Socket, new DefaultChanceSetMsgFinish { Type = MsgType.SetDefaultChanceFinish, ClientID = obj.ClientID, MessageID = obj.MessageID });
            }).Start();
        }

        public override void ExecuteSetVoltageDefault(DefaultVoltageSetMsg obj)
        {
            viewData.Logs += "正在设置默认电压\n";
            obj.ClientID = viewData.ClientID;
            new Thread(() =>
            {
                Thread.Sleep(1000);
                viewData.RunStatus += $"设置默认电压为：{obj.Voltage}\n";
                viewData.Logs += $"设置电压{obj.Voltage}成功！\n";

                SendMessage(obj.Socket, new DefaultVoltageSetMsgFinish { Type = MsgType.SetDefaultVoltagSuccess, ClientID = obj.ClientID, MessageID = obj.MessageID });
            }).Start();
        }
    }
}