using Common;
using Database;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ServerService : IServerService
    {
        private string csvFolderPath;
        private XmlDatabase<Load> xmlDatabase;

        public ServerService()
        {
            csvFolderPath = GetCsvFolderPathFromConfig();
            xmlDatabase = new XmlDatabase<Load>("D:\\fakultet\\6 - semestar\\Virtuelizacija procesa\\zadatak4\\Fajlovi\\xml_baza\\database.xml");
        }

        private string GetCsvFolderPathFromConfig()
        {
            return ConfigurationManager.AppSettings["CsvFolderPath"];
        }

        public void SendCsvFiles()
        {
            try
            {
                string[] csvFiles = Directory.GetFiles(csvFolderPath, "*.csv");

                foreach (string csvFile in csvFiles)
                {
                    List<Load> loads = ParseCsvFile(csvFile);

                    foreach (Load load in loads)
                    {
                        xmlDatabase.Add(load);

                        Console.WriteLine("load MeasuredValue: " + load.MeasuredValue);
                        Console.WriteLine("load Id: " + load.Id);
                        Console.WriteLine("load Timestamp: " + load.Timestamp.ToString());

                    }

                    File.Delete(csvFile);

                    Console.WriteLine($"Processed CSV file: {csvFile}");
                }

                Console.WriteLine("CSV files sent and processed successfully.");


                List<Load> listaTest = xmlDatabase.GetAll();
                foreach (var lista in listaTest)
                {
                    Console.WriteLine("TEST VALUE: " + lista.MeasuredValue);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending and processing CSV files: {ex.Message}");
            }
        }

        public void GetMinMaxStand(string operation)
        {
            try
            {
                List<Load> loads = xmlDatabase.GetAll();
                double result = 0;

                string resultMessage;

                switch (operation.ToLower())
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
                        return;
                }

                Console.WriteLine(resultMessage);

                string fileName = "calculations" + DateTime.Now.ToString("_yyyy_MM_dd_HHmm") + ".txt";

                string projectPath = @"..\Client\bin\Debug\Client.exe";
                Configuration config = ConfigurationManager.OpenExeConfiguration(projectPath);

                // uploadPathTest - samo za test... inace putanja treba da se cita iz Client -> app.config, to je uploadPath u outputFilePath...

                string uploadPathTest = ConfigurationManager.AppSettings["TxtFolderPath"];

                string uploadPath = config.AppSettings.Settings["TxtFolderPath"].Value;

                uploadPath += fileName;

                string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string outputFilePath = Path.Combine(dir, uploadPathTest);  // uploadPath

                File.WriteAllText(outputFilePath, resultMessage);

                Console.WriteLine($"Result written to file: {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving result: {ex.Message}");
            }
        }

        private List<Load> ParseCsvFile(string filePath)
        {
            List<Load> loads = new List<Load>();

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] values = line.Split(',');

                        if (values.Length != 2)
                        {
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

                        Load load = new Load
                        {
                            Id = id,
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

            standardDeviation = Math.Sqrt((sum) / (loads.Count() - 1));

            return standardDeviation;
        }

        public void Dispose()
        {
            xmlDatabase.Dispose();
        }
    }
}
