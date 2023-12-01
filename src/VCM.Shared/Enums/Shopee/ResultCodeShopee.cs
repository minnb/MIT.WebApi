using System;
using System.Collections.Generic;
using System.Text;

namespace VCM.Shared.Enums.Shopee
{
    public enum ResultCodeShopee
    {
        success = 200,
        error_params = 291,
        error_header = 292,
        error_server = 293,
        error_forbidden = 294,
        error_access_token = 295,
        error_version_too_low = 296
    }
}
