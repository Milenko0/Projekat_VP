using Common.Enum;
using Common.Util;
using System;
using System.Configuration;
using System.ServiceModel;
using Client.Enums;
using Client.FileInUseCheck;
using Client.FileSending;
using Common;
using System.IO;
using Client.Downloading;
using DownloaderClient.Downloading;

namespace Client
{
    public class Program
    {
        static void Main()
        {
            var upload = ConfigurationManager.AppSettings["uploadPath"];
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string uploadPath = Path.Combine(dir, upload);
            FileDirUtil.CheckCreatePath(uploadPath);
            
            var download = ConfigurationManager.AppSettings["downloadPath"];
            string downloadPath = Path.Combine(dir, download);
            FileDirUtil.CheckCreatePath(downloadPath);

            ChannelFactory<IServerService> factory = new ChannelFactory<IServerService>("ServiceServer");
            IServerService proxy = factory.CreateChannel();

            Console.WriteLine("Client started.");
            Console.WriteLine("Enter command (Send / Get [min/max/stand] parameter):");

            string command;
            do
            {
                command = Console.ReadLine();
                ProcessCommand(proxy, command, uploadPath, downloadPath);
            }
            while (!string.Equals(command, "exit", StringComparison.OrdinalIgnoreCase));

            
        
            //IUploader uploader = GetUploader(uploaderType, GetFileSender(proxy, GetFileInUseChecker(), storageType, uploadPath), uploadPath);
            //uploader.Start();
            //Console.WriteLine("Uploader Client is running. Press any key to exit.");
            //Console.ReadLine();
            Environment.Exit(0);
            /*
            using (Client client = new Client())
            {
                client.Start();
            }
            */
        }
        private static IFileInUseChecker GetFileInUseChecker()
        {
            return new FileInUseCommonChecker();
        }

        private static IFileSender GetFileSender(IServerService proxy, IFileInUseChecker fileInUseChecker, StorageTypes storageType, string uploadPath)
        {
            return new FileSender(proxy, fileInUseChecker, storageType, uploadPath);
        }

        private static IDownloader GetDownloader(IServerService proxy, string path, StorageTypes storageType)
        {
            return new StartNameDownloader(proxy, path, storageType);
        }

        private static void ProcessCommand(IServerService serviceProxy, string command, string uploadPath, string downloadPath)
        {
            string[] parts = command.Split(' ');
            string operation = parts[0].ToLower();

            string sStorageType = ConfigurationManager.AppSettings["storageType"];
            if (!Enum.TryParse(sStorageType, out StorageTypes storageType))
                storageType = StorageTypes.FileSystem;
            //Console.WriteLine($"{storageType} is being used.");
            string sUploaderType = ConfigurationManager.AppSettings["uploaderType"];
            if (!Enum.TryParse(sUploaderType, out UploaderTypes uploaderType))
                uploaderType = UploaderTypes.Event;
            //Console.WriteLine($"Upload path: {uploadPath}");
            switch (operation)
            {
                case "send":
                    
                    IFileSender fileSender = GetFileSender(serviceProxy, GetFileInUseChecker(), storageType, uploadPath);
                    fileSender.SendFiles();
                    serviceProxy.ProccesCsvFiles();
                    break;
                case "get":
                    if (parts.Length < 2)
                    {
                        Console.WriteLine("Invalid command. Usage: Get [operation]...");
                        return;
                    }
                    
                    string s = serviceProxy.GetMinMaxStand(parts); ;
                    IDownloader downloader = GetDownloader(serviceProxy, downloadPath, storageType);
            
                        if (s != null && !string.IsNullOrEmpty(s))
                        {
                            downloader.Download(s);
                        }
                   
                    break;
                case "exit":
                    Console.WriteLine("Exiting client application...");
                    break;
                default:
                    Console.WriteLine("Invalid command.");
                    break;
            }
        }
    }
}
