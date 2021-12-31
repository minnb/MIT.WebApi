using System;
using System.Collections.Generic;
using System.Text;

namespace PhucLong.Interface.Central.Models.ApiModel.CRX
{
    public class RspCX
    {
        public int Status { get; set; }
        public string Message { get; set; }
    }
    public class RspVoucherCRX
    {
        public DataRspCRX Data { get; set; }
    }
    public class DataRspCRX
    {
        public int ErrorCode { get; set; }
        public string Message { get; set; }
    }

    public class DataRspVoucherSAP
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
