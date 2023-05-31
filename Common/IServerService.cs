using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IServerService
    {
        [OperationContract]
        void SendCsvFiles();

        [OperationContract]
        void GetMinMaxStand(string operation);
    }
}
