using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace VCM.Shared.Enums
{
    public enum ApiStatusEnum
    {
        OK = 200,
        Failed = 401,
        Timeout = 408,
        UpgradeRequired = 426,

    }

    public enum ApiMessageEnum
    {
        [EnumMember(Value = "Lỗi convert json to object")]
        JsonStrError = 810,

    }
}
