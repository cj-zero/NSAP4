using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace OpenAuth.App.Order.Request
{
    [Serializable]
    [DataContract]
    public class DeliveryItemAid
    {
        [DataMember]
        public string ItemCode { get; set; }

        [DataMember]
        public string Times1 { get; set; }

        [DataMember]
        public string Times2 { get; set; }

        [DataMember]
        public string Times3 { get; set; }
        [DataMember]
        public string Parent { get; set; }
    }
}
