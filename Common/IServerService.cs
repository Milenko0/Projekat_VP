using Common.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ServiceContract]
    public interface IServerService : IDisposable
    {
        [OperationContract]
        void ProccesCsvFiles();

        [OperationContract]
        void GetMinMaxStand(string[] operation);

        [OperationContract]
        FileManipulationResults SendFile(FileManipulationOptions options);
    }
}
