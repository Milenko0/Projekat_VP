using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (ServiceHost host = new ServiceHost(typeof(ServerService)))
            {
                host.Open();

                Console.WriteLine("Server started.");
                Console.WriteLine("Press Enter to stop the server.");
                Console.ReadLine();

                host.Close();
            }
        }
    }
}
