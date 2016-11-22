﻿using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers
{
    internal static class XmlSerializerHelpers
    {
        internal static string Serialize<T>(T item)
        {
            var xs = new XmlSerializer(item.GetType());
            using (var sw = new StringWriter())
            {
                xs.Serialize(sw, item);
                return sw.ToString();
            }
        }

        internal static string Serialize<T>(T item, string rootName)
        {
            var xs = new XmlSerializer(item.GetType(), new XmlRootAttribute(rootName));
            using (var sw = new StringWriter())
            {
                xs.Serialize(sw, item);
                return sw.ToString();
            }
        }

        internal static T Deserialize<T>(string xmlString)
        {
            if (string.IsNullOrWhiteSpace(xmlString))
            {
                return default(T);
            }

            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(xmlString)))
            {
                return (T)new XmlSerializer(typeof(T)).Deserialize(ms);
            }
        }
    }
}
