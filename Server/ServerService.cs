using Common;
using Common.Enum;
using Common.Params;
using Common.Util;
using Database;
using Files.Commands;
using Files.FileHandlers.Impl;
using Files.Queries;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Server
{
    public class ServerService : IServerService, IDisposable
    {
        private string csvFolderPath;
        private string txtFolderPath;
        private XmlDatabase<Load> xmlDatabase;
        private XmlDatabase<Audit> xmlDatabaseAudit;
        private int brojac;
        private List<string> greske;
        private bool disposedValue = false;

        public delegate string CalculationDelegate(double v);
        public event CalculationDelegate MinValueCalculated;
        public event CalculationDelegate MaxValueCalculated;
        public event CalculationDelegate StandValueCalculated;

        public ServerService()
        {
            this.csvFolderPath = GetCsvFolderPathFromConfig();
            this.txtFolderPath = GetTxtFolderPathFromConfig();
            string userDefined = ConfigurationManager.AppSettings["UseUserDefinedPath"];
            string location = ConfigurationManager.AppSettings["XMLPath"];
            if (bool.TryParse(userDefined, out bool userDefinedBool)){
                if (userDefinedBool)
                {
                    FileDirUtil.CheckCreatePath(location);
                    xmlDatabase = new XmlDatabase<Load>(location + "/TBL_LOAD.xml");
                    xmlDatabaseAudit = new XmlDatabase<Audit>(location + "/TBL_AUDIT.xml");
                    return;
                }
                
            }
            xmlDatabase = new XmlDatabase<Load>("TBL_LOAD.xml");
            xmlDatabaseAudit = new XmlDatabase<Audit>("TBL_AUDIT.xml");

        }

        private string GetCsvFolderPathFromConfig()
        {
            //return ConfigurationManager.AppSettings["CsvFolderPath"];
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string outputFilePath = Path.Combine(dir, "Temp");  // uploadPath
            FileDirUtil.CheckCreatePath(outputFilePath);
            return outputFilePath;
        }

        private string GetTxtFolderPathFromConfig()
        {
            return ConfigurationManager.AppSettings["TxtFolderPath"];
        }

        public void ProccesCsvFiles()
        {
            try
            {
                
                string[] csvFiles = Directory.GetFiles(csvFolderPath, "*.csv");
                brojac = 0;
                greske = new List<string>();
                foreach (string csvFile in csvFiles)
                {
                    
                    List<Audit> audits;
                    List<Load> loads = ParseCsvFile(csvFile, out audits);
                    foreach (Load load in loads)
                    {
                        //Console.WriteLine(load.ToString());
                        xmlDatabase.Add(load);
                    }
                    foreach (Audit audit in audits)
                    {
                        //Console.WriteLine(audit.ToString());
                        xmlDatabaseAudit.Add(audit);
                    }

                    File.Delete(csvFile);

                    Console.WriteLine($"Processed CSV file");
                }

                Console.WriteLine("CSV files sent and processed successfully.");

                MinValueCalculated += HandleMinValueCalculated;
                MaxValueCalculated += HandleMaxValueCalculated;
                StandValueCalculated += HandleStandValueCalculated;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending and processing CSV files: {ex.Message}");
            }
        }

        private List<Load> ParseCsvFile(string filePath, out List<Audit> audits)
        {
            List<Load> loads = new List<Load>();
            audits = new List<Audit>();
            

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        bool flag = false;
                        brojac++;
                        string[] values = line.Split(',');
                        int duzina = values.Length;

                        if (duzina != 2)
                        {
                            Audit audit = new Audit
                            {
                                Id = brojac,
                                Timestamp = DateTime.Now,
                                Message = "Neodgovarajuci broj parametara",
                                MessageType = MessageType.Warning
                            };

                            greske.Add(audit.ToString());
                            audits.Add(audit);
                            flag = true;
                            
                        }

                        if (duzina < 1 || !DateTime.TryParse(values[0], out DateTime timeStampAudit))
                        {
                            Audit audit = new Audit
                            {
                                Id = brojac,
                                Timestamp = DateTime.Now,
                                Message = "Nevalidan podatak: TIME_STAMP",
                                MessageType = MessageType.Error
                            };
                            greske.Add(audit.ToString());

                            audits.Add(audit);
                            flag = true;
                            //continue;

                        }
                        if (duzina < 2 || !double.TryParse(values[1], out double measuredValueAudit))
                        {
                            Audit audit = new Audit
                            {
                                Id = brojac,
                                Timestamp = DateTime.Now,
                                Message = "Nevalidan podatak: MEASURED_VALUE",
                                MessageType = MessageType.Error
                            };
                            greske.Add(audit.ToString());
                            audits.Add(audit);
                            flag = true;
                            //continue;
                        }

                        if (flag)
                        {
                            
                            continue;
                        }

                        CultureInfo culture = CultureInfo.InvariantCulture;

                        DateTime.TryParse(values[0], out DateTime timeStamp);
                        double.TryParse(values[1], NumberStyles.Any, culture, out double measuredValue);

                        Audit auditT = new Audit
                        {
                            Id = brojac,
                            Timestamp = DateTime.Now,
                            Message = "Prosledjeni podaci su validni",
                            MessageType = MessageType.Info
                        };

                        audits.Add(auditT);

                        //Console.WriteLine(id);
                        //Console.WriteLine(timeStamp);
                        //Console.WriteLine(measuredValue);
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

                audits.Add(audit);
            }

            return loads;
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
                           // resultMessage = $"Min Load: {result}";
                            resultMessage = HandleMinValueCalculated(result);
                            break;
                        case "max":
                            result = GetMaxValue(loads);
                            // resultMessage = $"Max Load: {result}";
                            resultMessage = HandleMaxValueCalculated(result);
                            break;
                        case "stand":
                            result = GetStandardDeviation(loads);
                            // resultMessage = $"Standard deviation: {result}";
                            resultMessage = HandleStandValueCalculated(result);
                            break;
                        default:
                            Console.WriteLine("Invalid operation.");
                            return "";
                    }
                    resultMessageFull += resultMessage + "\n";
                }
                
                //resultMessageFull += resultMessage + "\n";

                Console.WriteLine(resultMessageFull);
                
                string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string outputFilePath = Path.Combine(dir, txtFolderPath);  // uploadPath
                FileDirUtil.CheckCreatePath(outputFilePath);
                string ret = "calculations" + DateTime.Now.ToString("_yyyy_MM_dd_HHmm") + ".txt";
                string fileName = "\\" + ret;
                outputFilePath += fileName;

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
            MinValueCalculated?.Invoke(minValue);
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
            MaxValueCalculated?.Invoke(maxValue);
            return maxValue;
        }

        private double GetStandardDeviation(List<Load> loads)
        {
            if (loads == null || loads.Count == 0)
            {
                throw new ArgumentException("List cannot be null or empty.");
            }

            //racunanje aritmeticke sredine
            double sum = 0;
            foreach (var load in loads)
            {
                sum += load.MeasuredValue;
            }
            double mean = sum / loads.Count;

            //racunanje sume kvadratnih razlika uzoraka i aritmeticke sredine
            double difference = 0;
            double sumOfSquaredDifferences = 0;
            foreach (var load in loads)
            {
                difference = load.MeasuredValue - mean;
                sumOfSquaredDifferences += difference * difference;
            }

            double variance = sumOfSquaredDifferences / loads.Count;
            double standValue = Math.Sqrt(variance);

            StandValueCalculated?.Invoke(standValue);

            return standValue;
        }

        private string HandleMinValueCalculated(double minValue)
        {
            return $"Min Load: {minValue}";
        }

        private string HandleMaxValueCalculated(double maxValue)
        {
            return $"Max Load: {maxValue}";
        }

        private string HandleStandValueCalculated(double standValue)
        {
            return $"Standard deviation: {standValue}";
        }

        public List<string> AuditGreske()
        {
            if(greske.Count>0) return greske;
            return null;
        }

       /* public void Dispose()
        {
            xmlDatabase.Dispose();
        }*/

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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~ServerService()
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
