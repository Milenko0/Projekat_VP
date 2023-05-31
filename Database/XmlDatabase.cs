using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Database
{
    public class XmlDatabase<T> : IDisposable where T : class
    {
        private List<T> data;
        private readonly string filePath;

        public XmlDatabase(string filePath)
        {
            this.filePath = filePath;
            LoadData();
        }
        public void Add(T item)
        {
            data.Add(item);
            SaveData();
        }

        public List<T> GetAll()
        {
            return data;
        }

        private void LoadData()
        {
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
                    data = (List<T>)serializer.Deserialize(reader);
                }
            }
            else
            {
                data = new List<T>();
            }
        }

        private void SaveData()
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
                serializer.Serialize(writer, data);
            }
        }

        public void Dispose()
        {
            SaveData();
        }
    }
}
