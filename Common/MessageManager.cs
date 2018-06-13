using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public abstract class MessageManagerAbs
    {
        public void SendMessage(Socket socket, Msg msg)
        {
            try
            {
                socket.Send(msg.ToMsgByteArray());
            }
            catch (SocketException)
            {
                OnSendMsgSocketException(socket, msg);
            }
            catch
            {
                OnSendMsgException(socket, msg);
            }
        }

        public abstract void OnSendMsgSocketException(Socket socket, Msg msg);
        public abstract void OnSendMsgException(Socket socket, Msg msg);

        public void ReceiveMessage(Socket socket, byte[] data, int dataSize)
        {
            new Msg(socket, data, dataSize, ExecuteHeart, ExecuteOutCoin, ExecuteOutCoinFinish, ExecuteSetDefaultChance, ExecuteSetVoltageDefault);
        }

        public abstract void ExecuteOutCoinFinish(Msg obj);
        public abstract void ExecuteOutCoin(OutCoinMsg obj);
        public abstract void ExecuteHeart(Msg obj);
        public abstract void ExecuteSetDefaultChance(DefaultChanceSetMsg obj);
        public abstract void ExecuteSetVoltageDefault(DefaultVoltageSetMsg obj);

    }

    public class Msg
    {

        [JsonIgnore]
        public MsgType Type { get; set; }
        public string MessageID { get; set; }
        public string ClientID { get; set; }
        [JsonIgnore]
        public string IP
        {
            get
            {
                return Socket.RemoteEndPoint.ToString();
            }
        }
        [JsonIgnore]
        public Socket Socket { get; set; }
        public Msg() { }
        public Msg(Socket socket, byte[] dataB, int dataSize, Action<Msg> ExecuteHeart, Action<OutCoinMsg> ExecuteOutCoin, Action<Msg> ExecuteOutCoinFinish, Action<DefaultChanceSetMsg> ExecuteSetDefaultChance, Action<DefaultVoltageSetMsg> ExecuteSetDefaultVoltage)
        {


            var dataArray = dataB.GetMessageByte(dataSize);
            foreach (var data in dataArray)
            {

                var bMsg = GetTypeAndMessageContent(data);
                switch (bMsg.Item1)
                {
                    case MsgType.Heart:

                        ReceivedMsg(data, socket, bMsg.Item1, bMsg.Item2, ExecuteHeart);
                        break;
                    case MsgType.OutCoinFinish:
                        Socket = socket;
                        ExecuteOutCoinFinish(this);
                        break;
                    case MsgType.OutCoin:
                        ReceivedMsg(data, socket, bMsg.Item1, bMsg.Item2, ExecuteOutCoin);
                        break;
                    case MsgType.SetDefaultVoltage:
                        ReceivedMsg(data, socket, bMsg.Item1, bMsg.Item2, ExecuteSetDefaultVoltage);
                        break;
                    case MsgType.SetDefaultChance:
                        ReceivedMsg(data, socket, bMsg.Item1, bMsg.Item2, ExecuteSetDefaultChance);
                        break;
                    default:
                        break;
                }
            }
        }



        private static byte[][] GetAllTypeByteArray()
        {
            var fields = typeof(MsgType).GetFields(BindingFlags.Static | BindingFlags.Public);
            var array = new List<byte[]>();

            foreach (var fi in fields)
                array.Add(BitConverter.GetBytes((int)fi.GetValue(null)));
            return array.ToArray();
        }

        private static (MsgType, string) GetTypeAndMessageContent(byte[] data)
        {
            var ht = GetHeaderValue(GetAllTypeByteArray(), data);
            var v = "";
            if (ht.Length < data.Length)
            {
                v = Encoding.ASCII.GetString(data, ht.Length, data.Length - ht.Length);
            }
            return ((MsgType)BitConverter.ToInt32(ht, 0), v);
        }
        /// <summary>
        /// 从数据包中获取指令
        /// </summary>
        /// <param name="v">所有指令数组</param>
        /// <param name="data">当前数据包</param>
        /// <returns></returns>
        private static byte[] GetHeaderValue(byte[][] v, byte[] data)
        {

            byte[] b = null;
            foreach (var item in v)
            {
                var flag = true;
                for (int i = 0; i < item.Length; i++)
                {
                    if (item[0] != data[0])
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    b = item;
                    break;
                }
            }
            return b;
        }

        internal byte[] ToMessageByte()
        {
            JsonSerializerSettings settings;
            settings = new JsonSerializerSettings
            { ContractResolver = new LimitPropsContractResolver(GetJsonLimitParams()) };

            var jsonData = JsonConvert.SerializeObject(this, settings);
            var coreData = Encoding.ASCII.GetBytes(jsonData);
            var data = new byte[coreData.Length + GetTypeByteArray().Length];
            GetTypeByteArray().CopyTo(data, 0);
            coreData.CopyTo(data, GetTypeByteArray().Length);
            return data;
        }
        public byte[] ToMsgByteArray()
        {
            //JsonSerializerSettings settings;
            //settings = new JsonSerializerSettings
            //{ ContractResolver = new LimitPropsContractResolver(GetJsonLimitParams()) };

            //var jsonData = JsonConvert.SerializeObject(this, settings);

            //var coreData = Encoding.UTF8.GetBytes(jsonData);
            //var data = new byte[coreData.Length + 4];
            //GetTypeByteArray().CopyTo(data, 0);
            //coreData.CopyTo(data, 4);
            return this.ToPackage();
        }

        public virtual string[] GetJsonLimitParams()
        {
            return new string[] { "ClientID" };
        }

        private byte[] GetTypeByteArray()
        {
            var type = (int)Type;

            return BitConverter.GetBytes(type);
        }

        private T GetMsg<T>(byte[] data, int dataSize)
        {
            var json = Encoding.UTF8.GetString(data, 4, dataSize - 4);
            return JsonConvert.DeserializeObject<T>(json);
        }
        private void ReceivedMsg<T>(byte[] data, Socket socket, MsgType msgType, string json, Action<T> Executer) where T : Msg
        {
            //T msg = GetMsg<T>(data, dataSize);
            //msg.Type = this.Type;
            //msg.Socket = this.Socket;
            //Executer(msg);
            //var msg = JsonConvert.DeserializeObject<T>(json);
            //msg.MType = msgType;
            //clientModel.ClientID = msg.ClientID;
            //execute(msg);

            var msg = JsonConvert.DeserializeObject<T>(json);
            msg.Type = msgType;
            msg.Socket = socket;
            //clientModel.ClientID = msg.ClientID;
            Executer(msg);

        }
    }
    public class DefaultChanceSetMsg : Msg
    {
        public int Chance { get; set; }
    }

    public class DefaultVoltageSetMsg : Msg
    {
        public int Voltage { get; set; }
    }

    public class OutCoinMsg : DefaultChanceSetMsg
    {
        public int CoinCount { get; set; }
        public int Voltage { get; set; }

    }
    public class OutCoinMsgFinish : Msg
    {
        public override string[] GetJsonLimitParams()
        {
            return new string[] { "ClientID", "MessageID" };
        }
    }

    /// <summary>
    /// 日志消息
    /// </summary>
    public class LogMessage : Msg
    {
        public LogModel LogInfo { get; set; }

        /// <summary>
        /// 获取当前实例的Json
        /// </summary>
        /// <returns></returns>
        public override string[] GetJsonLimitParams()
        {
            return new string[] {
                "LogInfo",
                "ClientID",
                "Content"
            };
        }
    }

    /// <summary>
    /// 错误消息
    /// </summary>
    public class BugMessage : Msg
    {
        public BugModel BugInfo { get; set; }
        /// <summary>
        /// 获取当前实例的Json
        /// </summary>
        /// <returns></returns>
        public override string[] GetJsonLimitParams()
        {
            return new string[] {
                "ClientID",
                "BugInfo",
                "Content"
            };
        }
    }
    public class DefaultChanceSetMsgFinish : Msg
    {
        public override string[] GetJsonLimitParams()
        {
            return new string[] { "ClientID", "MessageID" };
        }

    }
    public class DefaultVoltageSetMsgFinish : Msg
    {
        public override string[] GetJsonLimitParams()
        {
            return new string[] { "ClientID", "MessageID" };
        }

    }

    public enum MsgType
    {
        /// <summary>
        /// 心跳
        /// </summary>
        Heart = 1000,
        /// <summary>
        /// 出币
        /// </summary>
        OutCoin = 2000,
        /// <summary>
        /// 出币完成
        /// </summary>
        OutCoinFinish = 2001,
        /// <summary>
        /// 执行失败
        /// </summary>
        ExecuteFailed = 4000,
        /// <summary>
        /// 上报日志
        /// </summary>
        ReportLog = 7000,
        /// <summary>
        /// 上报故障
        /// </summary>
        ReportBug = 7001,
        /// <summary>
        /// 设置默认概率
        /// </summary>
        SetDefaultChance = 9000,
        /// <summary>
        /// 设置默认概率成功
        /// </summary>
        SetDefaultChanceFinish = 9001,
        /// <summary>
        /// 设置默认电压
        /// </summary>
        SetDefaultVoltage = 9002,
        /// <summary>
        /// 设置默认电压成功
        /// </summary>
        SetDefaultVoltagSuccess = 9003
    }
}
