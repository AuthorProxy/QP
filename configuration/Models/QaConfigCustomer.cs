using System.Xml.Serialization;

namespace Quantumart.QP8.Configuration.Models
{
    public class QaConfigCustomer
    {
        [XmlAttribute("customer_name")]
        public string CustomerName { get; set; }

        [XmlAttribute("exclude_from_schedulers")]
        public bool ExcludeFromSchedulers { get; set; }

        [XmlElement("db")]
        public string ConnectionString { get; set; }
    }
}