using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ClientServiceProxy : ClientBase<IServerService>, IServerService
    {
        public void SendCsvFiles()
        {
            Channel.SendCsvFiles();
        }

        public void GetMinMaxStand(string[] operation)
        {
            Channel.GetMinMaxStand(operation);
        }
    }
}
