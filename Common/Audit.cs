using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public enum MessageType { [EnumMember] Info, [EnumMember] Warning, [EnumMember] Error }

    [DataContract]
    public class Audit
    {
        private int id;
        private DateTime timestamp;
        private MessageType messageType;
        private string message;

        public Audit() { }

        public Audit(int id, DateTime timestamp, MessageType messageType, string message)
        {
            this.id = id;
            this.timestamp = timestamp;
            this.messageType = messageType;
            this.message = message;
        }

        [DataMember]
        public int Id { get => id; set => id = value; }
        [DataMember]
        public DateTime Timestamp { get => timestamp; set => timestamp = value; }
        [DataMember]
        public MessageType MessageType { get => messageType; set => messageType = value; }
        [DataMember]
        public string Message { get => message; set => message = value; }

        public override string ToString()
        {
            return "ID: " + Id + " Poruka greske: " + Message;
        }
    }
}
