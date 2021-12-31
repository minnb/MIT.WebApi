using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace VCM.Common.Helpers
{
    public static class ConvertHelper
    {
        public static string SerializeXmlNoHeader<T>(this T value)
        {
            if (value == null) return string.Empty;
            var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var xmlSerializer = new XmlSerializer(typeof(T));

            using var stringWriter = new Utf8StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true, Encoding = Encoding.Unicode, OmitXmlDeclaration = true });
            xmlSerializer.Serialize(xmlWriter, value, ns);
            return stringWriter.ToString();
        }
        public static string SerializeXml<T>(this T value)
        {
            if (value == null) return string.Empty;
            var xmlSerializer = new XmlSerializer(typeof(T));
            using var stringWriter = new Utf8StringWriter();
            using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true, Encoding =  Encoding.Unicode }))
            {
                xmlSerializer.Serialize(xmlWriter, value);
                return stringWriter.ToString();
            }
        }
        public static List<int> ListStringToInt(string str, string c)
        {
            List<int> result = new List<int>();
            if (!string.IsNullOrEmpty(str)) 
            {
                var lst = str.Split(c).ToList();
                foreach(var item in lst)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        result.Add(int.TryParse(item, out int i) ? i : 0);
                    }
                }
                //result = str.Split(c).ToList().Select(int.Parse).ToList();  
            }
            return result;
        }
    }
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return new UTF8Encoding(true); } 
        }
    }
}
