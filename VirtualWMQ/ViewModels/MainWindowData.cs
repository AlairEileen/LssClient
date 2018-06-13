using Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace VirtualWMQ.ViewModels
{
    public class MainWindowData : BindableObject
    {
        private ObservableCollection<ClientInfo> devices = new ObservableCollection<ClientInfo>();

        public ObservableCollection<ClientInfo> Devices { get => devices; set => SetProperty(ref devices, value); }
        public string Logs { get => logs; set { SetProperty(ref logs, value); } }


        private string logs = "";



    }

    public class ClientInfo:BindableObject
    {
        private string clientID;
        private string status;
        private string iP;
        private string heart = "";

        public Socket Client { get; set; }
        public string ClientID { get => clientID; set => SetProperty(ref clientID, value); }
        public string Status { get => status; set => SetProperty(ref status, value); }
        public string IP { get => iP; set => SetProperty(ref iP, value); }
        public string Heart { get => heart; set => SetProperty(ref heart, value); }

        public override string ToString()
        {
            return $@"{ClientID}-{Status}:{IP}{Heart}";
        }
    }
}
