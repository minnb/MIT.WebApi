using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace VCM.Common.Helpers
{
    public static class ConvertHelper
    {
        public static DateTime ConvertStringDateTimeToDate(string strDateTime, string conversion)
        {
            DateTime dtFinaldate; 
            string sDateTime;
            try { dtFinaldate = Convert.ToDateTime(strDateTime); }
            catch 
            {
                string[] sDate = strDateTime.Split("-");
                sDateTime = sDate[1] + conversion + sDate[0] + conversion + sDate[2];
                dtFinaldate = Convert.ToDateTime(sDateTime);
            }
            return dtFinaldate;
        }
        public static DateTime ConvertStringToDate(string strDate, string formatDate)
        {
            DateTime dt = DateTime.ParseExact(strDate, formatDate, CultureInfo.InvariantCulture);
            return dt;
        }
        public static string GetUserNameAuthorization(string authorization)
        {
            try
            {
                var authHeaderValue = AuthenticationHeaderValue.Parse(authorization);
                var bytes = Convert.FromBase64String(authHeaderValue.Parameter);
                string[] credentials = Encoding.UTF8.GetString(bytes).Split(":");
                string client_code = credentials[0];
                //string hash_key = credentials[1];
                return client_code;
            }
            catch
            {
                return null;
            }
        }
        public static Type BaseType(Type oType)
        {
            //#### If the passed oType is valid, .IsValueType and is logicially nullable, .Get(its)UnderlyingType
            if (oType != null && oType.IsValueType &&
                oType.IsGenericType && oType.GetGenericTypeDefinition() == typeof(Nullable<>)
                )
            {
                return Nullable.GetUnderlyingType(oType);
            }
            //#### Else the passed oType was null or was not logicially nullable, so simply return the passed oType
            else
            {
                return oType;
            }
        }

        public static DataTable EnumToDataTable<T>(IEnumerable<T> l_oItems)
        {
            var firstItem = l_oItems.FirstOrDefault();
            if (firstItem == null)
                return new DataTable();

            DataTable oReturn = new DataTable(TypeDescriptor.GetClassName(firstItem));
            object[] a_oValues;
            int i;

            var properties = TypeDescriptor.GetProperties(firstItem);

            int count = 0;
            foreach (PropertyDescriptor property in properties)
            {
                count++;
                if(count <= properties.Count / 2)
                {
                    oReturn.Columns.Add(property.Name, BaseType(property.PropertyType));
                }
            }

            //#### Traverse the l_oItems
            foreach (T oItem in l_oItems)
            {
                //#### Collect the a_oValues for this loop
                a_oValues = new object[properties.Count];

                //#### Traverse the a_oProperties, populating each a_oValues as we go
                for (i = 0; i < properties.Count; i++)
                    a_oValues[i] = properties[i].GetValue(oItem);

                //#### .Add the .Row that represents the current a_oValues into our oReturn value
                oReturn.Rows.Add(a_oValues);
            }

            //#### Return the above determined oReturn value to the caller
            return oReturn;
        }
     

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
