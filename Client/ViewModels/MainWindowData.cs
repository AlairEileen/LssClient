using Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    public class MainWindowData : BindableObject
    {
        private string runStatus = "";

        public string Logs { get => logs; set { SetProperty(ref logs, value); } }

        public string RunStatus { get => runStatus; set => SetProperty(ref runStatus, value); }
        public string ClientID { get => clientID; set => SetProperty(ref clientID, value); }
        public string Heart { get => heart; set => SetProperty(ref heart,value); }

        private string logs = "";
        private string clientID = "";

        private string heart = "";


    }


}
