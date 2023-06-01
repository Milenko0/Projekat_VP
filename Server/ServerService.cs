

using Common;
using Common.Enum;
using Common.Params;
using Database;
using Files.Commands;
using Files.FileHandlers.Impl;
using Files.Queries;
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
    public class ServerService : IServerService
    {
        private string csvFolderPath;
        private string txtFolderPath;
        private XmlDatabase<Load> xmlDatabase;
        private XmlDatabase<Audit> xmlDatabaseAudit;

        public ServerService()
        {
            this.csvFolderPath = GetCsvFolderPathFromConfig();
            this.txtFolderPath = ConfigurationManager.AppSettings["TxtFolderPath"];
            xmlDatabase = new XmlDatabase<Load>("TBL_LOAD.xml");
            xmlDatabaseAudit = new XmlDatabase<Audit>("TBL_AUDIT.xml");
        }

        private string GetCsvFolderPathFromConfig()
        {
            return ConfigurationManager.AppSettings["CsvFolderPath"];
        }

        public void ProccesCsvFiles()
        {
            try
            {
                string[] csvFiles = Directory.GetFiles(csvFolderPath, "*.csv");

                foreach (string csvFile in csvFiles)
                {
                    List<Load> loads = ParseCsvFile(csvFile);
                    foreach (Load load in loads)
                    {
                        Console.WriteLine(load.ToString());
                        xmlDatabase.Add(load);
                    }

                    File.Delete(csvFile);

                    Console.WriteLine($"Processed CSV file: {csvFile}");
                }

                Console.WriteLine("CSV files sent and processed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending and processing CSV files: {ex.Message}");
            }
        }

        public string GetMinMaxStand(string[] operation)
        {
            try
            {
                List<Load> loads = xmlDatabase.GetAll();
                double result = 0;

                string resultMessage;
                string resultMessageFull = "";
                
                for (int i = 1; i < operation.Length; i++)
                {
                    switch (operation[i].ToLower())
                    {
                        case "min":
                            result = GetMinValue(loads);
                            resultMessage = $"Min Load: {result}";
                            break;
                        case "max":
                            result = GetMaxValue(loads);
                            resultMessage = $"Max Load: {result}";
                            break;
                        case "stand":
                            result = GetStandardDeviation(loads);
                            resultMessage = $"Standard deviation: {result}";
                            break;
                        default:
                            Console.WriteLine("Invalid operation.");
                            return "";
                    }
                    resultMessageFull += resultMessage + "\n";
                }
                //resultMessageFull += resultMessage + "\n";

                Console.WriteLine(resultMessageFull);
                
                string ret =  "calculations" + DateTime.Now.ToString("_yyyy_MM_dd_HHmm") + ".txt";
                string fileName = "\\" + ret;

                string projectPath = @"..\Client\bin\Debug\Client.exe";
               // Configuration config = ConfigurationManager.OpenExeConfiguration(projectPath);

                // uploadPathTest - samo za test... inace putanja treba da se cita iz Client -> app.config, to je uploadPath u outputFilePath...

                string uploadPathTest = ConfigurationManager.AppSettings["TxtFolderPath"];

                //string uploadPath = config.AppSettings.Settings["TxtFolderPath"].Value;

                uploadPathTest += fileName;

                string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string outputFilePath = Path.Combine(dir, uploadPathTest);  // uploadPath

                File.WriteAllText(outputFilePath, resultMessageFull);

                
                Console.WriteLine($"Result written to file: {outputFilePath}");
                return ret;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving result: {ex.Message}");
                return "";
            }
        }

        private List<Load> ParseCsvFile(string filePath)
        {
            List<Load> loads = new List<Load>();

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    int brojac = 0;
                    int brojacAudit = 0;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        brojac++;
                        string[] values = line.Split(',');
                        
                        if (values.Length != 2)
                        {
                            brojacAudit++;
                            Audit audit = new Audit
                            {
                                Id = loads.Count + 1,
                                Timestamp = DateTime.Now,
                                Message = $"Invalid number of values in line: {line}",
                                MessageType = MessageType.Error
                            };

                            AddAudit(audit);
                            continue;
                        }
                        /*
                        if (!int.TryParse(values[0], out int id) || !DateTime.TryParse(values[1], out DateTime timeStamp) ||
                            !double.TryParse(values[2], out double measuredValue))
                        {
                            Audit audit = new Audit
                            {
                                Id = loads.Count + 1,
                                Timestamp = DateTime.Now,
                                Message = $"Invalid format in line: {line}",
                                MessageType = MessageType.Error
                            };

                            AddAudit(audit);
                            continue;
                        }
*/
                        //int.TryParse(values[0], out int id);
                        DateTime.TryParse(values[0], out DateTime timeStamp);
                        double.TryParse(values[1], out double measuredValue);

                        //Console.WriteLine(id);
                        Console.WriteLine(timeStamp);
                        Console.WriteLine(measuredValue);
                        Load load = new Load
                        {
                            Id = brojac,
                            Timestamp = timeStamp,
                            MeasuredValue = measuredValue
                        };

                        loads.Add(load);
                    }
                }
            }
            catch (Exception ex)
            {
                Audit audit = new Audit
                {
                    Id = loads.Count + 1,
                    Timestamp = DateTime.Now,
                    Message = $"Error parsing CSV file: {ex.Message}",
                    MessageType = MessageType.Error
                };

                AddAudit(audit);
            }

            return loads;
        }

        private void AddAudit(Audit audit)
        {
            // Add audit object to XML database
        }

        private double GetMinValue(List<Load> loads)
        {



            double minValue = loads[0].MeasuredValue;

            if (loads.Count() > 0)
            {
                foreach (var load in loads)
                {
                    if (minValue > load.MeasuredValue)
                        minValue = load.MeasuredValue;
                }
            }

            return minValue;
        }

        private double GetMaxValue(List<Load> loads)
        {
            double maxValue = loads[0].MeasuredValue;

            if (loads.Count() > 0)
            {
                foreach (var load in loads)
                {
                    if (maxValue < load.MeasuredValue)
                        maxValue = load.MeasuredValue;
                }
            }

            return maxValue;
        }

        private double GetStandardDeviation(List<Load> loads)
        {
            /*
            double standardDeviation = 0;
            double x = 0;

            foreach (var load in loads)
            {
                x += load.MeasuredValue;
            }

            x = x / loads.Count;

            double sum = 0;
            foreach (var load in loads)
            {
                sum += Math.Pow((load.MeasuredValue - x), 2);
            }

            standardDeviation = Math.Sqrt( (sum) / (loads.Count() - 1) );

            return standardDeviation;
            */
            List<double> doubleList = new List<double>();
            foreach(var load in loads)
            {
                doubleList.Add(load.MeasuredValue);
            }
            
            double average = doubleList.Average();
            double sumOfDerivation = 0;
            foreach (double value in doubleList)
            {
                sumOfDerivation += (value) * (value);
            }
            double sumOfDerivationAverage = sumOfDerivation / (doubleList.Count - 1);
            return Math.Sqrt(sumOfDerivationAverage - (average * average));
            

            
        }

        public void Dispose()
        {
            xmlDatabase.Dispose();
        }

        [OperationBehavior(AutoDisposeParameters = true)]
        public FileManipulationResults SendFile(FileManipulationOptions options)
        {
            Console.WriteLine($"Receiving file with name: \"{options.FileName}\"");
            return new InsertFileHandler(GetInsertFileCommand(options)).InsertFile();

        }

        private ICommand GetInsertFileCommand(FileManipulationOptions options)
        {
            //if (options.StorageType == StorageTypes.FileSystem)
            //{
                return new FileSystemInsertFileCommand(options, csvFolderPath);
            //}
            //return new DBInsertFileCommand(InMemoryDataBase.Instance, options);
        }

        [OperationBehavior(AutoDisposeParameters = true)]
        public FileManipulationResults GetFiles(FileManipulationOptions options)
        {
            Console.WriteLine($"Geting files starting with: \"{options.FileName}\"");
            return new GetFilesHandler(GetFilesQuery(options)).GetFiles();
        }

        private IQuery GetFilesQuery(FileManipulationOptions options)
        {
            //if (options.StorageType == StorageTypes.FileSystem)
            //{
                return new FileSystemGetFilesQuery(options, txtFolderPath);
           // }
            //return new DBGetFilesQuery(InMemoryDataBase.Instance, options);
        }
    }
}
