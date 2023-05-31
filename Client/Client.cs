using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Client
{
    public class Client : IDisposable
    {
        private ClientServiceProxy serviceProxy;
        private bool disposedValue = false;
        private static MemoryStream stream = new MemoryStream();
        
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

        /*public void Dispose()
        {
            if (serviceProxy != null)
            {
                serviceProxy.Close();
                serviceProxy = null;
            }
        }*/
        
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    stream.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~Client()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }
}
