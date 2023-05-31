using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Server
    {
        private ServiceHost host;
        private string csvFolderPath;

        public Server()
        {
            csvFolderPath = GetCsvFolderPathFromConfig();
        }

        private string GetCsvFolderPathFromConfig()
        {
            string uploadPath = ConfigurationManager.AppSettings["CsvFolderPath"];
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string absolutePath = Path.Combine(dir, uploadPath);
            return absolutePath;
        }

        public void Start()
        {
            host = new ServiceHost(typeof(ServerService));
            host.Open();

            Console.WriteLine("Server started.");
            Console.WriteLine("Press Enter to stop the server.");
            Console.ReadLine();

            host.Close();
        }

    }
}
