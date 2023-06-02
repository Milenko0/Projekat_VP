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
    public interface IServerService
    {
        [OperationContract]
        void ProccesCsvFiles();

        [OperationContract]
        string GetMinMaxStand(string[] operation);

        [OperationContract]
        List<string> AuditGreske();

        [OperationContract]
        FileManipulationResults SendFile(FileManipulationOptions options);

        [OperationContract]
        FileManipulationResults GetFiles(FileManipulationOptions options);
    }
}
