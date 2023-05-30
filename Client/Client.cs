using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Client : IDisposable
    {
        private ClientServiceProxy serviceProxy;
        public Client()
        {
            serviceProxy = new ClientServiceProxy();
        }

        public void Start()
        {
            Console.WriteLine("Client started.");
            Console.WriteLine("Enter command (Send / Get [min/max/stand] parameter):");

            string command;
            do
            {
                command = Console.ReadLine();
                ProcessCommand(command);
            }
            while (!string.Equals(command, "exit", StringComparison.OrdinalIgnoreCase));
        }

        private void ProcessCommand(string command)
        {
            string[] parts = command.Split(' ');
            string operation = parts[0].ToLower();

            switch (operation)
            {
                case "send":
                    serviceProxy.SendCsvFiles();
                    break;
                case "get":
                    if (parts.Length < 2)
                    {
                        Console.WriteLine("Invalid command. Usage: Get [operation]");
                        return;
                    }

                    string op = parts[1].ToLower();

                    serviceProxy.GetMinMaxStand(op);
                    break;
                case "exit":
                    Console.WriteLine("Exiting client application...");
                    break;
                default:
                    Console.WriteLine("Invalid command.");
                    break;
            }
        }

        public void Dispose()
        {
            if (serviceProxy != null)
            {
                serviceProxy.Close();
                serviceProxy = null;
            }
        }

    }
}
