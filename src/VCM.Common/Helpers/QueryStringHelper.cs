using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace VCM.Common.Helpers
{
    public class StrCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
        }
    }
    public class QueryStringHelper
    {
        private SortedList<String, DateTime> _requestDataDate = new SortedList<String, DateTime>(new StrCompare());
        private SortedList<String, String> _requestData = new SortedList<String, String>(new StrCompare());
        private SortedList<String, String> _responseData = new SortedList<String, String>(new StrCompare());
        public void AddRequestData(string key, string value)
        {
            _requestData.Add(key, value);
            //if (!String.IsNullOrEmpty(value))
            //{
            //    _requestData.Add(key, value);
            //}
        }
        public void AddResponseData(string key, string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }
        public string GetResponseData(string key)
        {
            string retValue;
            if (_responseData.TryGetValue(key, out retValue))
            {
                return retValue;
            }
            else
            {
                return string.Empty;
            }
        }
        public string GetRequestRaw()
        {
            StringBuilder data = new StringBuilder();
            foreach (KeyValuePair<string, string> kv in _requestData)
            {
                if (!String.IsNullOrEmpty(kv.Value))
                {
                    data.Append(kv.Key + "=" + kv.Value + "&");
                }
            }
            //remove last '&'
            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }
            return data.ToString();
        }

    }
}
