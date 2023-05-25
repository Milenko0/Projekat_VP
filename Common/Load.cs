using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class Load
    {
        private int id;
        private DateTime timestamp;
        private double measuredValue;

        public Load()
        {

        }

        public Load(int id, DateTime timestamp, double measuredValue)
        {
            this.id = id;
            this.timestamp = timestamp;
            this.measuredValue = measuredValue;
        }

        [DataMember]
        public int Id { get => id; set => id = value; }
        [DataMember]
        public DateTime Timestamp { get => timestamp; set => timestamp = value; }
        [DataMember]
        public double MeasuredValue { get => measuredValue; set => measuredValue = value; }

        public override string ToString()
        {
            return Id + " " + Timestamp + " " + MeasuredValue;
        }
    }
}
